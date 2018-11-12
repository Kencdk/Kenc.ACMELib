namespace Kenc.ACMELib.Exceptions.API
{
    /// <summary>
    /// Exception thrown by ACME endpoint when client sends an invalid anti-reply nonce.
    /// </summary>
    [ACMEException("urn:ietf:params:acme:error:badcsr")]
    public class BadCSRException : ACMEException
    {
        public BadCSRException(int status, string detail) : base(status, detail)
        {
        }
    }
}
