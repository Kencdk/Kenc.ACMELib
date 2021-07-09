namespace Kenc.ACMELib.JsonWebSignature
{
    using System.Text.Json.Serialization;

    /// <summary>
    /// Class implementing Jws messages.
    /// </summary>
    public class JwsMessage
    {
        [JsonPropertyName("header")]
        public JwsHeader Header { get; set; }

        [JsonPropertyName("protected")]
        public string Protected { get; set; }

        [JsonPropertyName("payload")]
        public string Payload { get; set; }

        [JsonPropertyName("signature")]
        public string Signature { get; set; }
    }
}
