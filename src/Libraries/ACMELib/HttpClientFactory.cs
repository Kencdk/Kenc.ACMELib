namespace Kenc.ACMELib
{
    using System.Net.Http;

    /// <summary>
    /// Implementation of <see cref="IHttpClientFactory"/>.
    /// </summary>
    public class HttpClientFactory : IHttpClientFactory
    {
        /// <summary>
        /// Creates a new <see cref="HttpClient"/>.
        /// </summary>
        /// <returns>An instance of <see cref="HttpClient"/></returns>
        public HttpClient CreateHttpClient()
        {
            return new HttpClient();
        }
    }
}