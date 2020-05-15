namespace Kenc.ACMELib
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Security.Cryptography;
    using System.Security.Cryptography.X509Certificates;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using Kenc.ACMELib.ACMEEntities;
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
        private readonly Jws jws;
        private readonly Uri endpoint;

        public ACMEDirectory Directory { get; private set; }

        private readonly IRestClient client;

        /// <summary>
        /// Initializes a new instance of the <see cref="ACMEClient"/> class.
        /// </summary>
        /// <param name="endpoint">The selected endpoint of the ACME server.</param>
        /// <param name="rsaKey">Encryption key for account.</param>
        /// <param name="restClientFactory">An instance of a <see cref="IRESTClient"/> for API requests.</param>
        public ACMEClient(string endpoint, RSA rsaKey, IRestClientFactory restClientFactory)
        {
            if (string.IsNullOrEmpty(endpoint))
            {
                throw new ArgumentNullException(nameof(endpoint));
            }

            if (restClientFactory == null)
            {
                throw new ArgumentNullException(nameof(restClientFactory));
            }

            this.endpoint = new Uri(endpoint);
            _ = rsaKey ?? throw new ArgumentNullException(nameof(rsaKey));

            jws = new Jws(rsaKey, string.Empty);
            client = restClientFactory.CreateRestClient(jws);
        }

        public async Task<ACMEDirectory> InitializeAsync()
        {
            await GetDirectoryAsync()
                .ConfigureAwait(false);
            await NewNonceAsync()
                .ConfigureAwait(false);
            return Directory;
        }

        public async Task<Account> RegisterAsync(string[] contacts, CancellationToken cancellationToken = default)
        {
            if (Directory == null)
            {
                await GetDirectoryAsync()
                    .ConfigureAwait(false);
            }

            var message = new Account
            {
                TermsOfServiceAgreed = true,
                Contacts = contacts,
            };

            (Account result, var response) = await client.PostAsync<Account>(Directory.NewAccount, message, cancellationToken)
                .ConfigureAwait(false);
            if (result is Account acmeAccount)
            {
                return acmeAccount;
            }

            throw new InvalidServerResponseException("registration.", response, Directory.NewAccount);
        }

        public async Task<Account> GetAccountAsync(CancellationToken cancellationToken = default)
        {
            if (Directory == null)
            {
                await GetDirectoryAsync()
                    .ConfigureAwait(false);
            }

            var message = new Account
            {
                OnlyReturnExisting = true
            };

            (Account result, var response) = await client.PostAsync<Account>(Directory.NewAccount, message, cancellationToken)
                .ConfigureAwait(false);
            if (result is Account acmeAccount)
            {
                return acmeAccount;
            }

            throw new InvalidServerResponseException("account retrieval.", response, Directory.NewAccount);
        }

        public async Task<Order> OrderAsync(IEnumerable<OrderIdentifier> identifiers, CancellationToken cancellationToken = default)
        {
            var message = new Order
            {
                Expires = DateTime.UtcNow.AddDays(2),
                Identifiers = identifiers.ToArray()
            };

            (Order result, var response) = await client.PostAsync<Order>(Directory.NewOrder, message, cancellationToken)
                .ConfigureAwait(false);
            if (result is Order acmeOrder)
            {
                return acmeOrder;
            }

            throw new InvalidServerResponseException("order.", response, Directory.NewOrder);
        }

        public async Task<AuthorizationChallengeResponse> GetAuthorizationChallengeAsync(Uri uri, CancellationToken cancellationToken = default)
        {
            (AuthorizationChallengeResponse result, var response) = await client.GetAsync<AuthorizationChallengeResponse>(uri, cancellationToken)
                .ConfigureAwait(false);
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

            throw new InvalidServerResponseException("GetAuthorizationChallenge.", response, Directory.NewAccount);
        }

        public async Task<AuthorizationChallengeResponse> CompleteChallengeAsync(Uri uri, string token, string authorization, CancellationToken cancellationToken = default)
        {
            var message = new AuthorizeChallenge
            {
                KeyAuthorization = authorization
            };

            (AuthorizationChallengeResponse result, var response) = await client.PostAsync<AuthorizationChallengeResponse>(uri, message, cancellationToken)
                .ConfigureAwait(false);
            if (result is AuthorizationChallengeResponse acmeOrder)
            {
                return acmeOrder;
            }

            throw new InvalidServerResponseException("CompleteChallenge.", response, Directory.NewAccount);
        }

        public async Task<AuthorizationChallengeResponse> UpdateChallengeAsync(Uri uri, string token, CancellationToken cancellationToken = default)
        {
            var message = new AuthorizeChallenge
            {
                KeyAuthorization = jws.GetKeyAuthorization(token)
            };

            (AuthorizationChallengeResponse result, var response) = await client.PostAsync<AuthorizationChallengeResponse>(uri, message, cancellationToken)
                .ConfigureAwait(false);
            if (result is AuthorizationChallengeResponse acmeOrder)
            {
                return acmeOrder;
            }

            throw new InvalidServerResponseException("UpdateChallenge.", response, Directory.NewAccount);
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

            (Order result, var responseText) = await client.PostAsync<Order>(order.Finalize, message, cancellationToken)
                .ConfigureAwait(false);
            if (result is Order acmeOrder)
            {
                return acmeOrder;
            }

            throw new InvalidServerResponseException("RequestCertificate.", responseText, order.Finalize);
        }

        public async Task<Order> UpdateOrderAsync(Order order, CancellationToken cancellationToken = default)
        {
            if (order == null)
            {
                throw new ArgumentNullException(nameof(order));
            }

            (Order result, var responseText) = await client.GetAsync<Order>(order.Location, cancellationToken)
                .ConfigureAwait(false);
            if (result is Order acmeOrder)
            {
                return acmeOrder;
            }

            throw new InvalidServerResponseException("UpdateOrder.", responseText, order.Location);
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

            (var result, var _) = await client.GetAsync<string>(order.Certificate, cancellationToken)
                .ConfigureAwait(false);
            var certificate = new X509Certificate2(Encoding.UTF8.GetBytes(result));
            return certificate;
        }

        public async Task<ACMEDirectory> GetDirectoryAsync(CancellationToken token = default)
        {
            Directory = await RequestDirectoryAsync(token)
                .ConfigureAwait(false);
            return Directory;
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

            await client.PostAsync<string>(Directory.RevokeCertificate, revocationRequest, cancellationToken)
                .ConfigureAwait(false);
        }

        private async Task<string> NewNonceAsync(CancellationToken token = default)
        {
            (var _, var response) = await client.HeadAsync<string>(Directory.NewNonce, token)
                .ConfigureAwait(false);
            return response;
        }

        private async Task<ACMEDirectory> RequestDirectoryAsync(CancellationToken cancellationToken = default)
        {
            var uri = new Uri(endpoint, "directory");

            (ACMEDirectory result, var text) = await client.GetAsync<ACMEDirectory>(uri, cancellationToken)
                .ConfigureAwait(false);
            if (result is ACMEDirectory)
            {
                return result;
            }

            throw new InvalidServerResponseException("Invalid response from server when requesting directory.", text, uri);
        }
    }
}