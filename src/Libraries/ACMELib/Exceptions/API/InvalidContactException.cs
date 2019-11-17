namespace Kenc.ACMELib.Exceptions.API
{
    using System;

    [Serializable]
    [ACMEException("urn:ietf:params:acme:error:invalidcontact")]
    public class InvalidContactException : ACMEException
    {
        public InvalidContactException(int status, string detail) : base(status, detail)
        {
        }
    }
}
