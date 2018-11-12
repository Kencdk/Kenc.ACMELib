namespace Kenc.ACMELib.Exceptions.API
{
    [ACMEException("urn:ietf:params:acme:error:invalidcontact")]
    public class InvalidContactException : ACMEException
    {
        public InvalidContactException(int status, string detail) : base(status, detail)
        {
        }
    }
}
