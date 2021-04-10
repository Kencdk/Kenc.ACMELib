namespace Kenc.ACMELib.Exceptions.API
{
    using System;

    [Serializable]
    [ACMEException("urn:ietf:params:acme:error:accountdoesnotexist")]
    public class AccountDoesNotExistException : ACMEException
    {
        public AccountDoesNotExistException(int status, string detail) : base(status, detail)
        {
        }
    }
}
