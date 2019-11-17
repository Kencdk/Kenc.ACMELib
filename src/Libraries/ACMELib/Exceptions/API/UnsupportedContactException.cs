namespace Kenc.ACMELib.Exceptions.API
{
    using System;

    [Serializable]
    [ACMEException("urn:ietf:params:acme:error:unsupportedcontact")]
    public class UnsupportedContactException : ACMEException
    {
        public UnsupportedContactException(int status, string detail) : base(status, detail)
        {
        }
    }
}
