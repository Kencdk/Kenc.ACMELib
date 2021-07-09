namespace Kenc.ACMELib.ACMEResponses
{
    using System;
    using System.Text.Json.Serialization;
    using Kenc.ACMELib.ACMEObjects;

    /// <summary>
    /// Describes an authorization challenge response in the ACME protocol.
    /// </summary>
    public class AuthorizationChallengeResponse : ILocationResponse
    {
        [JsonPropertyName("identifier")]
        public OrderIdentifier Identifier { get; set; }

        [JsonPropertyName("status")]
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public ACMEStatus Status { get; set; }

        [JsonPropertyName("expires")]
        public DateTime? Expires { get; set; }

        [JsonPropertyName("wildcard")]
        public bool Wildcard { get; set; }

        [JsonPropertyName("challenges")]
        public AuthorizationChallenge[] Challenges { get; set; }

        [JsonIgnore]
        public Uri Location { get; set; }
    }
}