namespace Kenc.ACMELib.Exceptions.API
{
    using System;

    [Serializable]
    [ACMEException("urn:ietf:params:acme:error:caa")]
    public class CAAException : ACMEException
    {
        public CAAException(int status, string detail) : base(status, detail)
        {
        }
    }
}
