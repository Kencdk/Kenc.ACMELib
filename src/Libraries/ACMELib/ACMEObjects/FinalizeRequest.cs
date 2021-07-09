namespace Kenc.ACMELib.ACMEObjects
{
    using System.Text.Json.Serialization;

    /// <summary>
    /// Describes a finalize request in the ACME protocol.
    /// </summary>
    public class FinalizeRequest
    {
        [JsonPropertyName("csr")]
        public string CSR { get; set; }
    }
}
