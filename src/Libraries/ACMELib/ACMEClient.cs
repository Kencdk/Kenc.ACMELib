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
    using Kenc.ACMELib.JWS;

    /// <summary>
    /// Implementation of an ACME client.
    /// Following https://tools.ietf.org/html/draft-ietf-acme-acme-16
    /// </summary>
    public class ACMEClient : IACMEClient
    {
        private Jws jws;
        private readonly RSA rsaKey;
        private readonly Uri endpoint;
        public ACMEDirectory Directory;

        private string nonce;
        private readonly IRestClient client;

        /// <summary>
        /// Initializes a new instance of the <see cref="ACMEClient"/> class.
        /// </summary>
        /// <param name="endpoint">The selected endpoint of the ACME server.</param>
        /// <param name="rsaKey">Encryption key for account.</param>
        /// <param name="restClientFactory">An instance of a <see cref="IRESTClient"/> for API requests.</param>
        public ACMEClient(string endpoint, RSA rsaKey, IRestClientFactory restClientFactory)
        {
            if (endpoint == null)
            {
                throw new ArgumentNullException(nameof(endpoint));
            }

            this.endpoint = new Uri(endpoint);
            this.rsaKey = rsaKey ?? throw new ArgumentNullException(nameof(rsaKey));

            jws = new Jws(rsaKey, string.Empty);
            client = restClientFactory.CreateRestClient(jws);
        }

        public async Task<ACMEDirectory> InitializeAsync()
        {
            await GetDirectoryAsync();
            await NewNonceAsync();
            return Directory;
        }

        /// <summary>
        /// Register a new account using the previously supplied <see cref="RSA"/> key.
        /// </summary>
        /// <param name="contacts">Means of contact as a string array</param>
        /// <param name="cancellationToken">Cancellation token for the async call.</param>
        /// <returns><see cref="Account"/></returns>
        /// <exception cref="ACMEException">Thrown for all errors from ACME servers.</exception>
        /// <exception cref="InvalidServerResponse">Thrown when the response from the server wasn't expected.</exception>
        public async Task<Account> RegisterAsync(string[] contacts, CancellationToken cancellationToken = default)
        {
            if (Directory == null)
            {
                await GetDirectoryAsync();
            }

            var message = new Account
            {
                TermsOfServiceAgreed = true,
                Contacts = contacts,
            };

            var (result, response) = await client.PostAsync<Account>(Directory.NewAccount, message, cancellationToken);
            if (result is Account acmeAccount)
            {
                return acmeAccount;
            }

            throw new InvalidServerResponse("Invalid response from server during registration.", response, Directory.NewAccount);
        }

        /// <summary>
        /// Get an existing account record using the encryption key specified in constructor.
        /// </summary>
        /// <param name="cancellationToken">Cancellation token for the async request.</param>
        /// <returns>A <see cref="Account"/> record if one exists.</returns>
        public async Task<Account> GetAccountAsync(CancellationToken cancellationToken = default)
        {
            if (Directory == null)
            {
                await GetDirectoryAsync();
            }

            var message = new Account
            {
                OnlyReturnExisting = true
            };

            var (result, response) = await client.PostAsync<Account>(Directory.NewAccount, message, cancellationToken);
            if (result is Account acmeAccount)
            {
                return acmeAccount;
            }

            throw new InvalidServerResponse("Invalid response from server during account retrieval.", response, Directory.NewAccount);
        }

        /// <summary>
        /// Sends an order for the specified domains and authorization types.
        /// </summary>
        /// <param name="identifiers">An <see cref="IEnumerable{OrderIdentifier}"/> specifying domain and <see cref="ChallengeType"/>.</param>
        /// <param name="cancellationToken">Cancellation token for the async request.</param>
        /// <returns>An <see cref="Order"/> object with details for the requested identifiers.</returns>
        public async Task<Order> OrderAsync(IEnumerable<OrderIdentifier> identifiers, CancellationToken cancellationToken = default)
        {

            var message = new Order
            {
                Expires = DateTime.UtcNow.AddDays(2),
                Identifiers = identifiers.ToArray()
            };

            var (result, response) = await client.PostAsync<Order>(Directory.NewOrder, message, cancellationToken);
            if (result is Order acmeOrder)
            {
                return acmeOrder;
            }

            throw new InvalidServerResponse("Invalid response from server during order.", response, Directory.NewOrder);
        }

        /// <summary>
        /// Get authorization challenge
        /// </summary>
        /// <param name="uri">Uri of the challenge.</param>
        /// <param name="cancellationToken">Cancellation token for the async request.</param>
        /// <returns><see cref="AuthorizationChallengeResponse"/></returns>
        public async Task<AuthorizationChallengeResponse> GetAuthorizationChallengeAsync(Uri uri, CancellationToken cancellationToken = default)
        {
            var (result, response) = await client.GetAsync<AuthorizationChallengeResponse>(uri, cancellationToken);
            if (result is AuthorizationChallengeResponse acmeOrder)
            {
                if (result.Challenges != null)
                {
                    foreach (var challenge in result.Challenges)
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

            throw new InvalidServerResponse("Invalid response from server during GetAuthorizationChallenge.", response, Directory.NewAccount);
        }

        /// <summary>
        /// Notify ACME servers that challenges are completed.
        /// </summary>
        /// <param name="uri">Uri of completed challenge.</param>
        /// <param name="token">Challenge token.</param>
        /// <param name="authorization">Authorization token.</param>
        /// <param name="cancellationToken">Cancellation token for the async request.</param>
        /// <returns><see cref="AuthorizationChallengeResponse"/></returns>
        public async Task<AuthorizationChallengeResponse> CompleteChallengeAsync(Uri uri, string token, string authorization, CancellationToken cancellationToken = default)
        {
            var message = new AuthorizeChallenge
            {
                KeyAuthorization = authorization
            };

            var (result, response) = await client.PostAsync<AuthorizationChallengeResponse>(uri, message, cancellationToken);
            if (result is AuthorizationChallengeResponse acmeOrder)
            {
                return acmeOrder;
            }

            throw new InvalidServerResponse("Invalid response from server during CompleteChallenge.", response, Directory.NewAccount);
        }

        /// <summary>
        /// Updates a challenge record.
        /// </summary>
        /// <param name="uri">Uri of the challenge.</param>
        /// <param name="token">Token of the challenge.</param>
        /// <param name="cancellationToken">Cancellation token for the async request.</param>
        /// <returns></returns>
        public async Task<AuthorizationChallengeResponse> UpdateChallengeAsync(Uri uri, string token, CancellationToken cancellationToken = default)
        {
            var message = new AuthorizeChallenge
            {
                KeyAuthorization = jws.GetKeyAuthorization(token)
            };

            var (result, response) = await client.PostAsync<AuthorizationChallengeResponse>(uri, message, cancellationToken);
            if (result is AuthorizationChallengeResponse acmeOrder)
            {
                return acmeOrder;
            }

            throw new InvalidServerResponse("Invalid response from server during UpdateChallenge.", response, Directory.NewAccount);
        }

        /// <summary>
        /// Request a certificate.
        /// </summary>
        /// <param name="order">A previously completed order.</param>
        /// <param name="key">The private key to sign the certificate request with.</param>
        /// <param name="cancellationToken">Cancellation token for the async request.</param>
        /// <returns>An updated <see cref="Order"/> object.</returns>
        /// <remarks>The subjectname for the request is the first identifier in <paramref name="order"/>. Subsequent identifiers are added as alternative names.</remarks>
        public async Task<Order> RequestCertificateAsync(Order order, RSACryptoServiceProvider key, CancellationToken cancellationToken = default)
        {
            List<string> identifiers = null;
            if (order.Identifiers.Length > 1 && order.Identifiers[0].Value[0] == '*')
            {
                // wildcards always goes first in the response from lets encrypt; reverse the order as requesting a wildcard as the subject name fails.
                identifiers = new List<string>
                {
                    order.Identifiers[1].Value,
                    order.Identifiers[0].Value
                };
                identifiers.AddRange(order.Identifiers.Skip(2).Select(item => item.Value));
            }
            else
            {
                identifiers = order.Identifiers.Select(item => item.Value).ToList();
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

            var (result, responseText) = await client.PostAsync<Order>(order.Finalize, message, cancellationToken);
            if (result is Order acmeOrder)
            {
                return acmeOrder;
            }

            throw new InvalidServerResponse("Invalid response from server during RequestCertificate.", responseText, order.Finalize.ToString());
        }

        /// <summary>
        /// Requests a status update from ACME regarding <paramref name="order"/>.
        /// </summary>
        /// <param name="order">A previously created order.</param>
        /// <param name="cancellationToken">Cancellation token for the async request.</param>
        /// <returns>An updated <see cref="Order"/> object.</returns>
        public async Task<Order> UpdateOrderAsync(Order order, CancellationToken cancellationToken = default)
        {
            var (result, responseText) = await client.GetAsync<Order>(order.Location, cancellationToken);
            if (result is Order acmeOrder)
            {
                return acmeOrder;
            }

            throw new InvalidServerResponse("Invalid response from server during UpdateOrder.", responseText, order.Location.ToString());
        }

        /// <summary>
        /// Get the certificate matching your order.
        /// </summary>
        /// <param name="order">A valid order.</param>
        /// <param name="cancellationToken">Cancellation token for the async request.</param>
        /// <returns>A <see cref="X509Certificate2"/> for the specified domain(s).</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="order"/>.Status isn't <see cref="Order.Valid"/></exception>
        public async Task<X509Certificate2> GetCertificateAsync(Order order, CancellationToken cancellationToken = default)
        {
            if (order.Status != ACMEStatus.Valid)
            {
                var exception = new ArgumentOutOfRangeException(nameof(order.Status), "Order status is not in valid range.");
                throw exception;
            }

            var (result, responseText) = await client.GetAsync<string>(order.Certificate, cancellationToken);
            var certificate = new X509Certificate2(Encoding.UTF8.GetBytes(result));
            return certificate;
        }

        /// <summary>
        /// Gets the directory listing from the specified ACME server.
        /// </summary>
        /// <param name="token">Cancellation token for the async requsts.</param>
        /// <returns><see cref="ACMEDirectory"/></returns>
        public async Task<ACMEDirectory> GetDirectoryAsync(CancellationToken token = default)
        {
            Directory = await RequestDirectoryAsync(token).ConfigureAwait(false);
            return Directory;
        }

        /// <summary>
        /// Send a revoke certificate request for the selected certificate.
        /// </summary>
        /// <param name="certificate">Certificate to revoke.</param>
        /// <param name="revocationReason">Reason for revocation.</param>
        /// <param name="cancellationToken">Cancellation token for the async call.</param>
        /// <returns></returns>
        /// <exception cref="Exceptions.API.UnauthorizedException">Thrown if the user isn't authorized to revoke the certificate.</exception>
        /// <exception cref="Exceptions.API.BadRevocationReasonException">Thrown if the <paramref name="revocationReason"/> isn't allowed.</exception>
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

            await client.PostAsync<string>(Directory.RevokeCertificate, revocationRequest, cancellationToken);
        }

        private async Task<string> NewNonceAsync(CancellationToken token = default)
        {
            var response = await client.HeadAsync<string>(Directory.NewNonce, token);
            nonce = response.response;
            return response.response;
        }

        private async Task<ACMEDirectory> RequestDirectoryAsync(CancellationToken cancellationToken = default)
        {
            var uri = new Uri(endpoint, "directory");

            var (result, text) = await client.GetAsync<ACMEDirectory>(uri, cancellationToken);
            if (result is ACMEDirectory)
            {
                return result;
            }

            throw new InvalidServerResponse("Invalid response from server when requesting directory.", text, uri);
        }
    }
}