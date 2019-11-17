namespace Kenc.ACMELib.Exceptions.API
{
    using System;

    [Serializable]
    [ACMEException("urn:ietf:params:acme:error:rejectedidentifier")]
    public class RejectedIdentifierException : ACMEException
    {
        public RejectedIdentifierException(int status, string detail) : base(status, detail)
        {
        }
    }
}
