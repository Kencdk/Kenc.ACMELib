namespace Kenc.ACMELib.ACMEObjects
{
    using System.Text.Json.Serialization;

    /// <summary>
    /// Decribes an order identifier in the ACME protocol.
    /// </summary>
    public class OrderIdentifier
    {
        [JsonPropertyName("type")]
        public string Type { get; set; }

        [JsonPropertyName("value")]
        public string Value { get; set; }
    }
}
