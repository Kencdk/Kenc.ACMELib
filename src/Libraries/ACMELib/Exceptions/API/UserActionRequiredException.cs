namespace Kenc.ACMELib.Exceptions.API
{
    [ACMEException("urn:ietf:params:acme:error:useractionrequired")]
    public class UserActionRequiredException : ACMEException
    {
        public UserActionRequiredException(int status, string detail) : base(status, detail)
        {
        }
    }
}
