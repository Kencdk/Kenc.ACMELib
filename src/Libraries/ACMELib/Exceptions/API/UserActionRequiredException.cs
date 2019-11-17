namespace Kenc.ACMELib.Exceptions.API
{
    using System;

    [Serializable]
    [ACMEException("urn:ietf:params:acme:error:useractionrequired")]
    public class UserActionRequiredException : ACMEException
    {
        public UserActionRequiredException(int status, string detail) : base(status, detail)
        {
        }
    }
}
