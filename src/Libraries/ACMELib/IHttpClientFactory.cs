namespace Kenc.ACMELib
{
    using System.Net.Http;

    /// <summary>
    /// Interface for a factory to create instances of <see cref="HttpClient"/>.
    /// </summary>
    public interface IHttpClientFactory
    {
        /// <summary>
        /// Creates a new <see cref="HttpClient"/>.
        /// </summary>
        /// <returns>An instance of <see cref="HttpClient"/></returns>
        HttpClient CreateHttpClient();
    }
}
