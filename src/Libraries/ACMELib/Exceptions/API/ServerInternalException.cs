namespace Kenc.ACMELib.Exceptions.API
{
    using System;

    [Serializable]
    [ACMEException("urn:ietf:params:acme:error:serverinternal")]
    public class ServerInternalException : ACMEException
    {
        public ServerInternalException(int status, string detail) : base(status, detail)
        {
        }
    }
}
