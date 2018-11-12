namespace Kenc.ACMELib.JWS
{
    using Newtonsoft.Json;

    /// <summary>
    /// Class implementing Jws messages.
    /// </summary>
    public class JwsMessage
    {
        [JsonProperty("header")]
        public JwsHeader Header { get; set; }

        [JsonProperty("protected")]
        public string Protected { get; set; }

        [JsonProperty("payload")]
        public string Payload { get; set; }

        [JsonProperty("signature")]
        public string Signature { get; set; }
    }
}