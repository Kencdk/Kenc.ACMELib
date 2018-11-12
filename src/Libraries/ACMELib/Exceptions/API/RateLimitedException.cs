namespace Kenc.ACMELib.Exceptions.API
{
    [ACMEException("urn:ietf:params:acme:error:ratelimited")]
    public class RateLimitedException : ACMEException
    {
        public RateLimitedException(int status, string detail) : base(status, detail)
        {
        }
    }
}
