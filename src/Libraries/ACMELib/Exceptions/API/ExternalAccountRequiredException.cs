namespace Kenc.ACMELib.Exceptions.API
{
    using System;

    [Serializable]
    [ACMEException("urn:ietf:params:acme:error:externalaccountrequired")]
    public class ExternalAccountRequiredException : ACMEException
    {
        public ExternalAccountRequiredException(int status, string detail) : base(status, detail)
        {
        }
    }
}
