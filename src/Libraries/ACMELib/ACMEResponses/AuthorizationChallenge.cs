namespace Kenc.ACMELib.ACMEResponses
{
    using System;
    using System.Text.Json.Serialization;

    /// <summary>
    /// Describes an authorization challenge in the ACME protocol.
    /// </summary>
    public class AuthorizationChallenge
    {
        [JsonPropertyName("type")]
        public string Type { get; set; }

        [JsonPropertyName("status")]
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public ACMEStatus Status { get; set; }

        [JsonPropertyName("url")]
        public Uri Url { get; set; }

        [JsonPropertyName("token")]
        public string Token { get; set; }

        [JsonIgnore]
        public string AuthorizationToken { get; set; }
    }
}