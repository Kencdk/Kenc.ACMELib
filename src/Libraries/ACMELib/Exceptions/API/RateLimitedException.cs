namespace Kenc.ACMELib.Exceptions.API
{
    using System;

    [Serializable]
    [ACMEException("urn:ietf:params:acme:error:ratelimited")]
    public class RateLimitedException : ACMEException
    {
        public RateLimitedException(int status, string detail) : base(status, detail)
        {
        }
    }
}
