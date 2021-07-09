namespace Kenc.ACMELib.ACMEObjects
{
    using System;
    using System.Text.Json.Serialization;
    using Kenc.ACMELib.ACMEResponses;
    using Kenc.ACMELib.JsonWebSignature;

    /// <summary>
    /// Describes an account in the ACME protocol.
    /// </summary>
    public class Account : ILocationResponse
    {
        [JsonPropertyName("termsOfServiceAgreed")]
        public bool TermsOfServiceAgreed { get; set; }

        [JsonPropertyName("contact")]
        public string[] Contacts { get; set; }

        [JsonPropertyName("status")]
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public ACMEStatus Status { get; set; }

        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("createdAt")]
        public DateTime CreatedAt { get; set; }

        [JsonPropertyName("key")]
        public Jwk Key { get; set; }

        [JsonPropertyName("initialIp")]
        public string InitialIp { get; set; }

        [JsonPropertyName("orders")]
        public Uri Orders { get; set; }

        [JsonIgnore]
        public Uri Location { get; set; }

        [JsonPropertyName("onlyReturnExisting")]
        public bool OnlyReturnExisting { get; set; }
    }
}
