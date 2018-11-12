namespace Kenc.ACMELib.JWS
{
    using System;
    using Newtonsoft.Json;

    /// <summary>
    /// Class implementing Jose Header https://tools.ietf.org/html/rfc7515#page-9
    /// </summary>
    public class JwsHeader
    {
        public JwsHeader()
        {
        }

        public JwsHeader(string algorithm, Jwk key)
        {
            Algorithm = algorithm;
            Key = key;
        }

        public JwsHeader(string nonce, Uri url)
        {
            this.Url = url;
            this.Nonce = nonce;
        }

        [JsonProperty("alg")]
        public string Algorithm { get; set; }

        [JsonProperty("jwk")]
        public Jwk Key { get; set; }


        [JsonProperty("kid")]
        public string KeyId { get; set; }


        [JsonProperty("nonce")]
        public string Nonce { get; set; }

        [JsonProperty("url")]
        public Uri Url { get; set; }
    }
}
