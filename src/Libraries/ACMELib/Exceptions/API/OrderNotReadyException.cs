namespace Kenc.ACMELib.Exceptions.API
{
    [ACMEException("urn:ietf:params:acme:error:ordernotready")]
    public class OrderNotReadyException : ACMEException
    {
        public OrderNotReadyException(int status, string detail) : base(status, detail)
        {
        }
    }
}
