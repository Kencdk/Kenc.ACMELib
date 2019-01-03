namespace Kenc.ACMELib.ACMEResponses
{
    using System;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;

    /// <summary>
    /// Describes an authorization challenge in the ACME protocol.
    /// </summary>
    public class AuthorizationChallenge
    {
        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("status")]
        [JsonConverter(typeof(StringEnumConverter))]
        public ACMEStatus Status { get; set; }

        [JsonProperty("url")]
        public Uri Url { get; set; }

        [JsonProperty("token")]
        public string Token { get; set; }

        [JsonIgnore]
        public string AuthorizationToken { get; set; }
    }
}