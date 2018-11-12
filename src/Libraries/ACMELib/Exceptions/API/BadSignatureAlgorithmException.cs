namespace Kenc.ACMELib.Exceptions.API
{
    [ACMEException("urn:ietf:params:acme:error:badsignaturealgorithm")]
    public class BadSignatureAlgorithmException : ACMEException
    {
        public BadSignatureAlgorithmException(int status, string detail) : base(status, detail)
        {
        }
    }
}
