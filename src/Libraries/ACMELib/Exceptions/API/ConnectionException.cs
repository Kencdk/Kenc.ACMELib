namespace Kenc.ACMELib.Exceptions.API
{
    using System;

    [Serializable]
    [ACMEException("urn:ietf:params:acme:error:connection")]
    public class ConnectionException : ACMEException
    {
        public ConnectionException(int status, string detail) : base(status, detail)
        {
        }
    }
}
