namespace Kenc.ACMELib.ACMEEntities
{
    using Newtonsoft.Json;

    /// <summary>
    /// Decribes an order identifier in the ACME protocol.
    /// </summary>
    public class OrderIdentifier
    {
        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("value")]
        public string Value { get; set; }

    }
}