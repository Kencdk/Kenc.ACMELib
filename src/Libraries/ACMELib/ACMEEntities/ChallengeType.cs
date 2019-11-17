namespace Kenc.ACMELib.ACMEEntities
{
    /// <summary>
    /// Available challenge types in the ACME protocol.
    /// </summary>
    public static class ChallengeType
    {
        public static readonly string DNSChallenge = "dns";
        public static readonly string HttpChallenge = "http";
        public static readonly string TLSSNIChallenge = "tls-sni";
    }
}