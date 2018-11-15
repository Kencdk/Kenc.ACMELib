namespace Kenc.ACMELib
{
    using System;
    using System.Collections.Generic;
    using System.Security.Cryptography;
    using System.Security.Cryptography.X509Certificates;
    using System.Threading;
    using System.Threading.Tasks;
    using Kenc.ACMELib.ACMEEntities;
    using Kenc.ACMELib.ACMEResponses;

    /// <summary>
    /// Interface describing an ACME client.
    /// </summary>
    public interface IACMEClient
    {
        /// <summary>
        /// Notify ACME servers that challenges are completed.
        /// </summary>
        /// <param name="uri">Uri of completed challenge.</param>
        /// <param name="token">Challenge token.</param>
        /// <param name="authorization">Authorization token.</param>
        /// <param name="cancellationToken">Cancellation token for the async request.</param>
        /// <returns><see cref="AuthorizationChallengeResponse"/></returns>
        Task<AuthorizationChallengeResponse> CompleteChallengeAsync(Uri uri, string token, string authorization, CancellationToken cancellationToken = default);

        /// <summary>
        /// Get authorization challenge
        /// </summary>
        /// <param name="uri">Uri of the challenge.</param>
        /// <param name="cancellationToken">Cancellation token for the async request.</param>
        /// <returns><see cref="AuthorizationChallengeResponse"/></returns>
        Task<AuthorizationChallengeResponse> GetAuthorizationChallengeAsync(Uri uri, CancellationToken cancellationToken = default);

        /// <summary>
        /// Get the certificate matching your order.
        /// </summary>
        /// <param name="order">A valid order.</param>
        /// <param name="cancellationToken">Cancellation token for the async request.</param>
        /// <returns>A <see cref="X509Certificate2"/> for the specified domain(s).</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="order"/>.Status isn't <see cref="Order.Valid"/></exception>
        Task<X509Certificate2> GetCertificateAsync(Order order, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets the directory listing from the specified ACME server.
        /// </summary>
        /// <param name="token">Cancellation token for the async requests.</param>
        /// <returns><see cref="ACMEDirectory"/></returns>
        Task<ACMEDirectory> GetDirectoryAsync(CancellationToken token = default);

        /// <summary>
        /// Sends an order for the specified domains and authorization types.
        /// </summary>
        /// <param name="identifiers">An <see cref="IEnumerable{OrderIdentifier}"/> specifying domain and <see cref="ChallengeType"/>.</param>
        /// <param name="cancellationToken">Cancellation token for the async request.</param>
        /// <returns>An <see cref="Order"/> object with details for the requested identifiers.</returns>
        Task<Order> OrderAsync(IEnumerable<OrderIdentifier> identifiers, CancellationToken cancellationToken = default);

        /// <summary>
        /// Register a new account using the previously supplied <see cref="RSA"/> key.
        /// </summary>
        /// <param name="contacts">Means of contact as a string array</param>
        /// <param name="cancellationToken">Cancellation token for the async call.</param>
        /// <returns><see cref="Account"/></returns>
        /// <exception cref="ACMEException">Thrown for all errors from ACME servers.</exception>
        /// <exception cref="InvalidServerResponse">Thrown when the response from the server wasn't expected.</exception>
        Task<Account> RegisterAsync(string[] contacts, CancellationToken cancellationToken = default);

        /// <summary>
        /// Send a revoke certificate request for the selected certificate.
        /// </summary>
        /// <param name="certificate">Certificate to revoke.</param>
        /// <param name="revocationReason">Reason for revocation.</param>
        /// <param name="cancellationToken">Cancellation token for the async call.</param>
        /// <returns>A task representing the revoke action.</returns>
        /// <exception cref="Exceptions.API.UnauthorizedException">Thrown if the user isn't authorized to revoke the certificate.</exception>
        /// <exception cref="Exceptions.API.BadRevocationReasonException">Thrown if the <paramref name="revocationReason"/> isn't allowed.</exception>
        Task RevokeCertificateAsync(X509Certificate certificate, RevocationReason revocationReason, CancellationToken cancellationToken = default);

        /// <summary>
        /// Request a certificate.
        /// </summary>
        /// <param name="order">A previously completed order.</param>
        /// <param name="key">The private key to sign the certificate request with.</param>
        /// <param name="cancellationToken">Cancellation token for the async request.</param>
        /// <returns>An updated <see cref="Order"/> object.</returns>
        /// <remarks>The subject name for the request is the first identifier in <paramref name="order"/>. Subsequent identifiers are added as alternative names.</remarks>
        Task<Order> RequestCertificateAsync(Order order, RSACryptoServiceProvider key, CancellationToken cancellationToken = default);

        Task<AuthorizationChallengeResponse> UpdateChallengeAsync(Uri uri, string token, CancellationToken cancellationToken = default);

        /// <summary>
        /// Requests a status update from ACME regarding <paramref name="order"/>.
        /// </summary>
        /// <param name="order">A previously created order.</param>
        /// <param name="cancellationToken">Cancellation token for the async request.</param>
        /// <returns>An updated <see cref="Order"/> object.</returns>
        Task<Order> UpdateOrderAsync(Order order, CancellationToken cancellationToken = default);

        /// <summary>
        /// Get an existing account record using the encryption key specified in constructor.
        /// </summary>
        /// <param name="cancellationToken">Cancellation token for the async request.</param>
        /// <returns>A <see cref="Account"/> record if one exists.</returns>
        Task<Account> GetAccountAsync(CancellationToken cancellationToken = default);
    }
}