namespace Kenc.ACMELib
{
    /// <summary>
    /// Describes status of ACME requests.
    /// </summary>
    public enum ACMEStatus
    {
        Unknown,
        Pending,
        Processing,
        Valid,
        Invalid,
        Revoked,
        Ready
    }
}