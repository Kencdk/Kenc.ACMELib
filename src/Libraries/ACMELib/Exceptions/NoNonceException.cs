namespace Kenc.ACMELib.Exceptions
{
    using System;

    /// <summary>
    /// Exception thrown when a nonce is required, but is never set.
    /// </summary>
    public class NoNonceException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="NoNonceException"/> class.
        /// </summary>
        public NoNonceException() : base("No Nonce specified.")
        {
        }
    }
}