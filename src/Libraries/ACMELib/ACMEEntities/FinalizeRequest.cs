namespace Kenc.ACMELib.ACMEEntities
{
    using Newtonsoft.Json;

    /// <summary>
    /// Describes a finalize request in the ACME protocol.
    /// </summary>
    public class FinalizeRequest
    {
        [JsonProperty("csr")]
        public string CSR { get; set; }
    }
}
