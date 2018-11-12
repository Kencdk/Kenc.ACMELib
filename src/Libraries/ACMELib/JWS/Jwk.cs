﻿namespace Kenc.ACMELib.JWS
{
    using Newtonsoft.Json;

    /// <summary>
    /// Class implementing a Json Web Key https://tools.ietf.org/html/rfc7517
    /// </summary>
    public class Jwk
    {
        [JsonProperty("kty")]
        public string KeyType { get; set; }

        [JsonProperty("kid")]
        public string KeyId { get; set; }

        [JsonProperty("use")]
        public string Use { get; set; }

        [JsonProperty("n")]
        public string Modulus { get; set; }

        [JsonProperty("e")]
        public string Exponent { get; set; }

        [JsonProperty("d")]
        public string D { get; set; }

        [JsonProperty("p")]
        public string P { get; set; }

        [JsonProperty("q")]
        public string Q { get; set; }

        [JsonProperty("dp")]
        public string DP { get; set; }

        [JsonProperty("dq")]
        public string DQ { get; set; }

        [JsonProperty("qi")]
        public string InverseQ { get; set; }

        [JsonProperty("alg")]
        public string Algorithm { get; set; }
    }
}