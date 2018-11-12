namespace Kenc.ACMELib.Exceptions.API
{
    [ACMEException("urn:ietf:params:acme:error:incorrectresponse")]
    public class IncorrectResponseException : ACMEException
    {
        public IncorrectResponseException(int status, string detail) : base(status, detail)
        {
        }
    }
}
