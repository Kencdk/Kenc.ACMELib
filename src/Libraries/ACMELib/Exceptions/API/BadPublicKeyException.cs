namespace Kenc.ACMELib.Exceptions.API
{
    using System;

    [Serializable]
    [ACMEException("urn:ietf:params:acme:error:badpublickey")]
    public class BadPublicKeyException : ACMEException
    {
        public BadPublicKeyException(int status, string detail) : base(status, detail)
        {
        }
    }
}
