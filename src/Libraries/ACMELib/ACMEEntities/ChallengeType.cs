namespace Kenc.ACMELib.ACMEEntities
{
    /// <summary>
    /// Available challenge types in the ACME protocol.
    /// </summary>
    public static class ChallengeType
    {
        public static string DNSChallenge = "dns";
        public static string HttpChallenge = "http";
        public static string TLSSNIChallenge = "tls-sni";
    }
}