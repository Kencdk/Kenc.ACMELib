namespace Kenc.ACMELib.Exceptions
{
    using System;

    /// <summary>
    /// An invalid response was received from the server.
    /// </summary>
    public class InvalidServerResponse : Exception
    {
        /// <summary>
        /// The string response from the server.
        /// </summary>
        public string Response
        {
            get;
            private set;
        }

        /// <summary>
        /// The URI requested when the response was received.
        /// </summary>
        public string RequestUri
        {
            get;
            private set;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="InvalidServerResponse"/> class.
        /// </summary>
        /// <param name="message">Message describing exception.</param>
        /// <param name="response">Response message.</param>
        /// <param name="requestUri">Uri of request.</param>
        public InvalidServerResponse(string message, string response, string requestUri) : base(message)
        {
            this.Response = response;
            this.RequestUri = requestUri;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="InvalidServerResponse"/> class.
        /// </summary>
        /// <param name="message">Message describing exception.</param>
        /// <param name="response">Response message.</param>
        /// <param name="requestUri">Uri of request.</param>
        public InvalidServerResponse(string message, string response, Uri requestUri) : this(message, response, requestUri.ToString())
        {
        }
    }
}