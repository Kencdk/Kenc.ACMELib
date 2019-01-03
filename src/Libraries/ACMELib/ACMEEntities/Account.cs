namespace Kenc.ACMELib.ACMEEntities
{
    using System;
    using Kenc.ACMELib.ACMEResponses;
    using Kenc.ACMELib.JWS;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;

    /// <summary>
    /// Describes an account in the ACME protocol.
    /// </summary>
    public class Account : ILocationResponse
    {
        [JsonProperty("termsOfServiceAgreed")]
        public bool TermsOfServiceAgreed { get; set; }

        [JsonProperty("contact")]
        public string[] Contacts { get; set; }

        [JsonProperty("status")]
        [JsonConverter(typeof(StringEnumConverter))]
        public ACMEStatus Status { get; set; }

        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("createdAt")]
        public DateTime CreatedAt { get; set; }

        [JsonProperty("key")]
        public Jwk Key { get; set; }

        [JsonProperty("initialIp")]
        public string InitialIp { get; set; }

        [JsonProperty("orders")]
        public Uri Orders { get; set; }

        [JsonIgnore]
        public Uri Location { get; set; }

        [JsonProperty("onlyReturnExisting")]
        public bool OnlyReturnExisting { get; set; }
    }
}