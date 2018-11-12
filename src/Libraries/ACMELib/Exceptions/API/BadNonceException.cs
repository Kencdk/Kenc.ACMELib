namespace Kenc.ACMELib.Exceptions.API
{
    /// <summary>
    /// Exception thrown by ACME endpoint when client sends an invalid anti-reply nonce.
    /// </summary>
    [ACMEException("urn:ietf:params:acme:error:badnonce")]
    public class BadNonceException : ACMEException
    {
        public BadNonceException(int status, string detail) : base(status, detail)
        {
        }
    }
}
