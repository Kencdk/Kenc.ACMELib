namespace Kenc.ACMELib.JsonWebSignature
{
    using System.Text.Json.Serialization;

    /// <summary>
    /// Class implementing a Json Web Key https://tools.ietf.org/html/rfc7517
    /// </summary>
    public class Jwk
    {
        [JsonPropertyName("kty")]
        public string KeyType { get; set; }

        [JsonPropertyName("kid")]
        public string KeyId { get; set; }

        [JsonPropertyName("use")]
        public string Use { get; set; }

        [JsonPropertyName("n")]
        public string Modulus { get; set; }

        [JsonPropertyName("e")]
        public string Exponent { get; set; }

        [JsonPropertyName("d")]
        public string D { get; set; }

        [JsonPropertyName("p")]
        public string P { get; set; }

        [JsonPropertyName("q")]
        public string Q { get; set; }

        [JsonPropertyName("dp")]
        public string DP { get; set; }

        [JsonPropertyName("dq")]
        public string DQ { get; set; }

        [JsonPropertyName("qi")]
        public string InverseQ { get; set; }

        [JsonPropertyName("alg")]
        public string Algorithm { get; set; }
    }
}