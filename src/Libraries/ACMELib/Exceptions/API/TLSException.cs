namespace Kenc.ACMELib.Exceptions.API
{
    using System;

    [Serializable]
    [ACMEException("urn:ietf:params:acme:error:tls")]
    public class TLSException : ACMEException
    {
        public TLSException(int status, string detail) : base(status, detail)
        {
        }
    }
}
