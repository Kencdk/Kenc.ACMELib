namespace Kenc.ACMELib.Exceptions.API
{
    [ACMEException("urn:ietf:params:acme:error:unsupportedidentifier")]
    public class UnsupportedIdentifierException : ACMEException
    {
        public UnsupportedIdentifierException(int status, string detail) : base(status, detail)
        {
        }
    }
}
