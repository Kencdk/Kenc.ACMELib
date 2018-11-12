namespace Kenc.ACMELib.Exceptions.API
{
    [ACMEException("urn:ietf:params:acme:error:accountdoesnotexist")]
    public class AccountDoesNotExistException : ACMEException
    {
        public AccountDoesNotExistException(int status, string detail) : base(status, detail)
        {

        }
    }
}