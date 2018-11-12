namespace Kenc.ACMELib.Exceptions.API
{
    [ACMEException("urn:ietf:params:acme:error:caa")]
    public class CAAException : ACMEException
    {
        public CAAException(int status, string detail) : base(status, detail)
        {
        }
    }
}
