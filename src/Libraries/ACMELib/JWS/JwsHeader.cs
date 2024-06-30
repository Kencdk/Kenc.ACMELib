namespace Kenc.ACMELib.JsonWebSignature
{
    using System;
    using System.Text.Json.Serialization;

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
            Key = key;
            Algorithm = algorithm;
        }

        public JwsHeader(string nonce, Uri url)
        {
            if (string.IsNullOrEmpty(nonce))
            {
                throw new ArgumentNullException(nameof(nonce));
            }

            Url = url;
            Nonce = nonce;
        }

        [JsonPropertyName("alg")]
        public string Algorithm { get; set; }

        [JsonPropertyName("jwk")]
        public Jwk Key { get; set; }

        [JsonPropertyName("kid")]
        public string KeyId { get; set; }

        [JsonPropertyName("nonce")]
        public string Nonce { get; set; }

        [JsonPropertyName("url")]
        public Uri Url { get; set; }
    }
}
