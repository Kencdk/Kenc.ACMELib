namespace Kenc.ACMELib.Exceptions.API
{
    [ACMEException("urn:ietf:params:acme:error:unsupportedcontact")]
    public class UnsupportedContactException : ACMEException
    {
        public UnsupportedContactException(int status, string detail) : base(status, detail)
        {
        }
    }
}
