namespace Kenc.ACMELib.ACMEObjects
{
    using System.Text.Json.Serialization;

    public class PreAuthorizationRequest
    {
        [JsonPropertyName("type")]
        public string Type { get; set; }

        [JsonPropertyName("value")]
        public string Value { get; set; }
    }
}
