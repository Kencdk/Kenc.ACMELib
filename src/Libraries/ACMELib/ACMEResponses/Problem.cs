namespace Kenc.ACMELib.ACMEResponses
{
    using System.Text.Json.Serialization;

    /// <summary>
    /// Describes a problem in the ACME protocol.
    /// </summary>
    public class Problem
    {
        [JsonPropertyName("type")]
        public string Type { get; set; }

        [JsonPropertyName("detail")]
        public string Detail { get; set; }

        [JsonPropertyName("status")]
        public int Status { get; set; }

        public string RawJson { get; set; }
    }
}
