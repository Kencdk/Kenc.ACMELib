namespace Kenc.ACMELib.Exceptions.API
{
    [ACMEException("urn:ietf:params:acme:error:unauthorized")]
    public class UnauthorizedException : ACMEException
    {
        public UnauthorizedException(int status, string detail) : base(status, detail)
        {
        }
    }
}
