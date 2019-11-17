namespace Kenc.ACMELib.Exceptions.API
{
    using System;

    [Serializable]
    [ACMEException("urn:ietf:params:acme:error:malformed")]
    public class MalformedException : ACMEException
    {
        public MalformedException(int status, string detail) : base(status, detail)
        {
        }
    }
}
