namespace Kenc.ACMELib.Exceptions
{
    using System;

    /// <summary>
    /// An invalid response was received from the server.
    /// </summary>
    public class InvalidServerResponseException : Exception
    {
        /// <summary>
        /// The URI requested when the response was received.
        /// </summary>
        public Uri RequestUri
        {
            get;
            private set;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="InvalidServerResponseException"/> class.
        /// </summary>
        /// <param name="request">Type of request.</param>
        /// <param name="response">Response message.</param>
        /// <param name="requestUri">Uri of request.</param>
        public InvalidServerResponseException(string request, Uri requestUri) : base($"Invalid response from server during {request}.")
        {
            RequestUri = requestUri;
        }
    }
}