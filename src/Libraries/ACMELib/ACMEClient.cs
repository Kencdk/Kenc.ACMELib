namespace Kenc.ACMELib
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
#if DEBUG
    using System.Diagnostics;
#else
    using System.IO;
#endif
    using System.Linq;
    using System.Net.Http;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using System.Security.Cryptography;
    using System.Security.Cryptography.X509Certificates;
    using System.Text;
    using System.Text.Json;
    using System.Threading;
    using System.Threading.Tasks;
    using Kenc.ACMELib.ACMEObjects;
    using Kenc.ACMELib.ACMEResponses;
    using Kenc.ACMELib.Exceptions;
    using Kenc.ACMELib.JsonWebSignature;

    /// <summary>
    /// Implementation of an ACME client.
    /// Following https://tools.ietf.org/html/draft-ietf-acme-acme-16
    /// </summary>
    /// <inheritdoc/>
    public class ACMEClient : IACMEClient
    {
        private const string ApplicationJoseAndJson = "application/jose+json";
        private const string ApplicationPemCertChainMime = "application/pem-certificate-chain";
        private const string ApplicationProblemJsonMime = "application/problem+json";
        private static readonly JsonSerializerOptions JsonSettings = new()
        {
            WriteIndented = true,
        };

        private readonly HttpClient client;
        private readonly Uri endpoint;
        private readonly Jws jws;
        private readonly ConcurrentQueue<string> nonces = new();

        public ACMEDirectory Directory { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ACMEClient"/> class.
        /// </summary>
        /// <param name="endpoint">The selected endpoint of the ACME server.</param>
        /// <param name="rsaKey">Encryption key for account.</param>
        /// <param name="httpClient">An instance of a <see cref="HttpClient"/> for API requests.</param>
        public ACMEClient(Uri endpoint, RSA rsaKey, HttpClient httpClient)
        {
            client = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            this.endpoint = endpoint ?? throw new ArgumentNullException(nameof(endpoint));
            _ = rsaKey ?? throw new ArgumentNullException(nameof(rsaKey));
            jws = new Jws(rsaKey, string.Empty);

            if (client.DefaultRequestHeaders.UserAgent.Count == 0)
            {
                // add the user agent header, if one isn't added already.
                Type acmeClientType = typeof(ACMEClient);
                AssemblyFileVersionAttribute runtimeVersion = acmeClientType.Assembly.GetCustomAttribute<AssemblyFileVersionAttribute>();

                var userAgent = $"{acmeClientType.FullName}/{runtimeVersion.Version} ({RuntimeInformation.OSDescription} {RuntimeInformation.ProcessArchitecture})";
                client.DefaultRequestHeaders.UserAgent.TryParseAdd(userAgent);
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ACMEClient"/> class.
        /// </summary>
        /// <param name="endpoint">The selected endpoint of the ACME server.</param>
        /// <param name="rsaKey">Encryption key for account.</param>
        /// <param name="httpClientFactory">Intance of <see cref="IHttpClientFactory"/> to create an instance of <see cref="HttpClient"/></param>
        public ACMEClient(Uri endpoint, RSA rsaKey, IHttpClientFactory httpClientFactory) : this(endpoint, rsaKey, httpClientFactory.CreateClient("ACME"))
        {
        }

        public async Task<AuthorizationChallengeResponse> CompleteChallengeAsync(Uri uri, string token, string authorization, CancellationToken cancellationToken = default)
        {
            var message = new AuthorizeChallenge
            {
                KeyAuthorization = authorization
            };

            return await PostAsync<AuthorizationChallengeResponse>(uri, message, cancellationToken);
        }

        public async Task<Account> GetAccountAsync(CancellationToken cancellationToken = default)
        {
            if (Directory == null)
            {
                await GetDirectoryAsync(cancellationToken);
            }

            var message = new Account
            {
                OnlyReturnExisting = true
            };

            return await PostAsync<Account>(Directory.NewAccount, message, cancellationToken);
        }

        public async Task<AuthorizationChallengeResponse> GetAuthorizationChallengeAsync(Uri uri, CancellationToken cancellationToken = default)
        {
            AuthorizationChallengeResponse result = await GetAsync<AuthorizationChallengeResponse>(uri, cancellationToken);
            if (result is AuthorizationChallengeResponse acmeOrder)
            {
                if (result.Challenges != null)
                {
                    foreach (AuthorizationChallenge challenge in result.Challenges)
                    {
                        if (challenge.Type == "dns-01")
                        {
                            challenge.AuthorizationToken = jws.GetDNSKeyAuthorization(challenge.Token);
                        }
                        else
                        {
                            challenge.AuthorizationToken = jws.GetKeyAuthorization(challenge.Token);
                        }
                    }
                }

                return acmeOrder;
            }

            throw new InvalidServerResponseException("GetAuthorizationChallenge", Directory.NewAccount);
        }

        public async Task<X509Certificate2> GetCertificateAsync(Order order, CancellationToken cancellationToken = default)
        {
            if (order == null)
            {
                throw new ArgumentNullException(nameof(order));
            }

            if (order.Status != ACMEStatus.Valid)
            {
                throw new ArgumentOutOfRangeException(nameof(order.Status), "Order status is not in valid range.");
            }

            var result = await GetAsync<string>(order.Certificate, cancellationToken);
            return new X509Certificate2(Encoding.UTF8.GetBytes(result));
        }

        public async Task<ACMEDirectory> GetDirectoryAsync(CancellationToken token = default)
        {
            Directory = await RequestDirectoryAsync(token);
            return Directory;
        }

        public async Task<ACMEDirectory> InitializeAsync()
        {
            await GetDirectoryAsync();
            await NewNonceAsync();
            return Directory;
        }

        public async Task<AuthorizationChallengeResponse> NewAuthorizationAsync(string domain, CancellationToken cancellationToken = default)
        {
            if (Directory.NewAuthz == null)
            {
                throw new InvalidOperationException("Target CA doesn't support PreAuthorization requests.");
            }

            var request = new PreAuthorizationRequest
            {
                Type = "dns",
                Value = domain
            };

            return await PostAsync<AuthorizationChallengeResponse>(Directory.NewAuthz, request, cancellationToken);
        }

        public async Task<Order> OrderAsync(IEnumerable<OrderIdentifier> identifiers, CancellationToken cancellationToken = default)
        {
            var message = new Order
            {
                Expires = DateTime.UtcNow.AddDays(2),
                Identifiers = identifiers.ToArray()
            };

            return await PostAsync<Order>(Directory.NewOrder, message, cancellationToken);
        }

        public async Task<Account> RegisterAsync(string[] contacts, CancellationToken cancellationToken = default)
        {
            if (Directory == null)
            {
                await GetDirectoryAsync(cancellationToken);
            }

            var message = new Account
            {
                TermsOfServiceAgreed = true,
                Contacts = contacts,
            };

            return await PostAsync<Account>(Directory.NewAccount, message, cancellationToken);
        }

        public async Task<Order> RequestCertificateAsync(Order order, RSACryptoServiceProvider key, CancellationToken cancellationToken = default)
        {
            if (order == null)
            {
                throw new ArgumentNullException(nameof(order));
            }

            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            var identifiers = order.Identifiers.Select(item => item.Value).ToList();
            if (identifiers.Any(x => x[0] == '*'))
            {
                // ensure wildcards are at the end.
                identifiers.Reverse();
            }

            var csr = new CertificateRequest("CN=" + identifiers.First(), key, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);

            var san = new SubjectAlternativeNameBuilder();
            foreach (var identifier in identifiers.Skip(1))
            {
                san.AddDnsName(identifier);
            }
            csr.CertificateExtensions.Add(san.Build());

            var message = new FinalizeRequest
            {
                CSR = Utilities.Base64UrlEncoded(csr.CreateSigningRequest())
            };

            return await PostAsync<Order>(order.Finalize, message, cancellationToken);
        }

        public async Task RevokeCertificateAsync(X509Certificate certificate, RevocationReason revocationReason, CancellationToken cancellationToken = default)
        {
            if (certificate == null)
            {
                throw new ArgumentNullException(nameof(certificate));
            }

            var revocationRequest = new CertificateRevocationRequest
            {
                Reason = revocationReason,
                Certificate = Utilities.Base64UrlEncoded(certificate.GetRawCertData())
            };

            await PostAsync<string>(Directory.RevokeCertificate, revocationRequest, cancellationToken);
        }

        public async Task<AuthorizationChallengeResponse> UpdateChallengeAsync(Uri uri, string token, CancellationToken cancellationToken = default)
        {
            var message = new AuthorizeChallenge
            {
                KeyAuthorization = jws.GetKeyAuthorization(token)
            };

            return await PostAsync<AuthorizationChallengeResponse>(uri, message, cancellationToken);
        }

        public async Task<Order> UpdateOrderAsync(Order order, CancellationToken cancellationToken = default)
        {
            if (order == null)
            {
                throw new ArgumentNullException(nameof(order));
            }

            return await GetAsync<Order>(order.Location, cancellationToken);
        }

        private async Task<TResult> GetAsync<TResult>(Uri uri, CancellationToken cancellationToken) where TResult : class
        {
            var httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, uri);
            return await SendRequest<TResult>(httpRequestMessage, cancellationToken);
        }

        private async Task<string> HeadAsync(Uri uri, CancellationToken cancellationToken)
        {
            var httpRequestMessage = new HttpRequestMessage(HttpMethod.Head, uri);
            return await SendRequest<string>(httpRequestMessage, cancellationToken);
        }

        private async Task<string> NewNonceAsync(CancellationToken cancellationToken = default)
        {
            return await HeadAsync(Directory.NewNonce, cancellationToken);
        }

        private async Task<TResult> PostAsync<TResult>(Uri uri, object message, CancellationToken cancellationToken) where TResult : class
        {
            if (!nonces.TryDequeue(out var nonce))
            {
                await NewNonceAsync(cancellationToken);
            }

            JwsMessage encodedMessage = jws.Encode(message, new JwsHeader(nonce, uri));
            var jsonString = JsonSerializer.Serialize(encodedMessage);
            var content = new StringContent(jsonString, Encoding.UTF8, ApplicationJoseAndJson);

            var httpRequestMessage = new HttpRequestMessage(HttpMethod.Post, uri)
            {
                Content = content
            };

            // workaround the fact that ACME can't handle having a charset in the content-type.
            httpRequestMessage.Content.Headers.ContentType.CharSet = null;
            return await SendRequest<TResult>(httpRequestMessage, cancellationToken);
        }

        private async Task<ACMEDirectory> RequestDirectoryAsync(CancellationToken cancellationToken = default)
        {
            var uri = new Uri(endpoint, "directory");
            return await GetAsync<ACMEDirectory>(uri, cancellationToken);
        }

        private async Task<TResult> SendRequest<TResult>(HttpRequestMessage httpRequestMessage, CancellationToken cancellationToken) where TResult : class
        {
            HttpResponseMessage httpResponseMessage = await client.SendAsync(httpRequestMessage, cancellationToken);
            if (httpResponseMessage.Headers.TryGetValues("Replay-Nonce", out IEnumerable<string> replayValues))
            {
                foreach (var nonce in replayValues)
                {
                    if (!string.IsNullOrWhiteSpace(nonce))
                    {
                        nonces.Enqueue(nonce);
                    }
                }
            }

            if (!httpResponseMessage.IsSuccessStatusCode)
            {
                if (httpResponseMessage.Content.Headers.ContentType.MediaType.Equals(ApplicationProblemJsonMime, StringComparison.OrdinalIgnoreCase))
                {
                    var rawStr = await httpResponseMessage.Content.ReadAsStringAsync();
                    Problem problem = JsonSerializer.Deserialize<Problem>(rawStr);
                    problem.RawJson = rawStr;
                    ExceptionHelper.ThrowException(problem);
                }

                // throw the HttpRequestException
                httpResponseMessage.EnsureSuccessStatusCode();
            }

            TResult responseContent = null;
            if (httpResponseMessage.Content != null && httpResponseMessage.Content.Headers.ContentLength > 0)
            {
                if (httpResponseMessage.Content.Headers.ContentType.MediaType.Equals(ApplicationPemCertChainMime, StringComparison.OrdinalIgnoreCase))
                {
                    var responseStr = await httpResponseMessage.Content.ReadAsStringAsync();
                    responseContent = (TResult)(object)(responseStr);
                }
                else
                {
#if DEBUG
                    // while debugging; have easy access to the response message.
                    var responseStr = await httpResponseMessage.Content.ReadAsStringAsync();
                    responseContent = JsonSerializer.Deserialize<TResult>(responseStr, JsonSettings);
#else
                    using Stream stream = await httpResponseMessage.Content.ReadAsStreamAsync();
                    responseContent = await JsonSerializer.DeserializeAsync<TResult>(stream, JsonSettings, cancellationToken);
#endif
                }

                if (httpResponseMessage.Headers.TryGetValues("Location", out IEnumerable<string> locationValues))
                {
                    if (responseContent is ILocationResponse locationResponse)
                    {
                        // there should only be a single location header.
                        locationResponse.Location = new Uri(locationValues.First());
                    }
#if DEBUG
                    else
                    {
                        Debug.WriteLine($"{nameof(ACMEClient)}: Response had location: {string.Join(',', locationValues)} but interface not applied to {responseContent?.GetType()}");
                    }
#endif
                }
            }

            if (responseContent != null && responseContent is Account account)
            {
                jws.SetKeyId(account);
            }

            return responseContent;
        }
    }
}