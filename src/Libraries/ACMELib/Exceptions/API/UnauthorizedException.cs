namespace Kenc.ACMELib.Exceptions.API
{
    using System;

    [Serializable]
    [ACMEException("urn:ietf:params:acme:error:unauthorized")]
    public class UnauthorizedException : ACMEException
    {
        public UnauthorizedException(int status, string detail) : base(status, detail)
        {
        }
    }
}
