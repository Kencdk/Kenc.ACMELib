namespace Kenc.ACMELib.ACMEEntities
{
    using Newtonsoft.Json;

    /// <summary>
    /// Describes a certificate revocation request in the ACME protocol.
    /// </summary>
    public class CertificateRevocationRequest
    {
        [JsonProperty("certificate")]
        public string Certificate { get; set; }

        [JsonProperty("reason")]
        public RevocationReason Reason { get; set; }
    }
}