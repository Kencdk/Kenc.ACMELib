namespace Kenc.ACMELib.ACMEObjects
{
    using System.Text.Json.Serialization;

    /// <summary>
    /// Describes a certificate revocation request in the ACME protocol.
    /// </summary>
    public class CertificateRevocationRequest
    {
        [JsonPropertyName("certificate")]
        public string Certificate { get; set; }

        [JsonPropertyName("reason")]
        public RevocationReason Reason { get; set; }
    }
}