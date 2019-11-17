namespace Kenc.ACMELib
{
    /// <summary>
    /// Defines the list of environments for ACME
    /// </summary>
    public static class ACMEEnvironment
    {
        public const string StagingV1 = "https://acme-staging.api.letsencrypt.org/directory";
        public const string ProductionV1 = "https://acme-v01.api.letsencrypt.org/directory";

        public const string StagingV2 = "https://acme-staging-v02.api.letsencrypt.org/directory";
        public const string ProductionV2 = "https://acme-v02.api.letsencrypt.org/directory";
    }
}