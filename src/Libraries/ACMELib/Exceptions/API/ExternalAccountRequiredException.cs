namespace Kenc.ACMELib.Exceptions.API
{
    [ACMEException("urn:ietf:params:acme:error:externalaccountrequired")]
    public class ExternalAccountRequiredException : ACMEException
    {
        public ExternalAccountRequiredException(int status, string detail) : base(status, detail)
        {
        }
    }
}
