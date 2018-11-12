namespace Kenc.ACMELib
{
    using Kenc.ACMELib.JWS;

    /// <summary>
    /// Interface for a factory to create a <see cref="IRestClient"/>.
    /// </summary>
    public interface IRestClientFactory
    {
        /// <summary>
        /// Creates a <see cref="IRestClient"/>.
        /// </summary>
        /// <param name="jws">Jws key to sign requests.</param>
        /// <returns>A class that fulfills the <see cref="IRestClient"/> interface.</returns>
        IRestClient CreateRestClient(Jws jws);
    }
}