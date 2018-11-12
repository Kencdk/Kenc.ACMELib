namespace Kenc.ACMELib
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Describes an interface for a restful client.
    /// </summary>
    public interface IRestClient
    {
        /// <summary>
        /// Sends a GET request to the targeted endpoint.
        /// </summary>
        /// <typeparam name="TResult">The class to serialize the result into.</typeparam>
        /// <param name="uri">Uri of the endpoint.</param>
        /// <param name="token">Cancellation token to cancel the request.</param>
        /// <returns>a <typeparamref name="TResult"/> class with the result and a string containing the result.</returns>
        Task<(TResult Result, string Response)> GetAsync<TResult>(Uri uri, CancellationToken token) where TResult : class;

        /// <summary>
        /// Sends a HEAD request to the targeted endpoint.
        /// </summary>
        /// <typeparam name="TResult">The class to serialize the result into.</typeparam>
        /// <param name="uri">Uri of the endpoint.</param>
        /// <param name="token">Cancellation token to cancel the request.</param>
        /// <returns>a <typeparamref name="TResult"/> class with the result and a string containing the result.</returns>
        Task<(TResult result, string response)> HeadAsync<TResult>(Uri uri, CancellationToken token) where TResult : class;

        /// <summary>
        /// Sends a POST request to the targeted endpoint.
        /// </summary>
        /// <typeparam name="TResult">The class to serialize the result into.</typeparam>
        /// <param name="uri">Uri of the endpoint.</param>
        /// <param name="message">Message body to send as part of the request.</param>
        /// <param name="token">Cancellation token to cancel the request.</param>
        /// <returns>a <typeparamref name="TResult"/> class with the result and a string containing the result.</returns>
        Task<(TResult Result, string Response)> PostAsync<TResult>(Uri uri, object message, CancellationToken token) where TResult : class;
    }
}
