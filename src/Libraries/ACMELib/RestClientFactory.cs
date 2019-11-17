namespace Kenc.ACMELib
{
    using Kenc.ACMELib.JsonWebSignature;

    /// <summary>
    /// Implementation of the <see cref="IRestClientFactory"/>.
    /// </summary>
    public class RestClientFactory : IRestClientFactory
    {
        /// <summary>
        /// Creates an instance of <see cref="ACMERestClient"/>.
        /// </summary>
        /// <param name="jws">Jws key used to sign requests.</param>
        /// <returns>An instance of <see cref="ACMERestClient"/></returns>
        public IRestClient CreateRestClient(Jws jws)
        {
            return new ACMERestClient(jws);
        }
    }
}