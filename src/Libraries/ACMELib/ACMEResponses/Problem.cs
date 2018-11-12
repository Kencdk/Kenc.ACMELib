namespace Kenc.ACMELib.ACMEResponses
{
    using Newtonsoft.Json;

    /// <summary>
    /// Describes a problem in the ACME protocol.
    /// </summary>
    public class Problem
    {
        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("detail")]
        public string Detail { get; set; }

        [JsonProperty("status")]
        public int Status { get; set; }

        public string RawJson { get; set; }
    }
}
