namespace Kenc.ACMELib.ACMEObjects
{
    using System;
    using System.Text.Json.Serialization;
    using Kenc.ACMELib.ACMEResponses;

    /// <summary>
    /// Describes an order in the ACME protocol.
    /// </summary>
    public class Order : ILocationResponse
    {
        [JsonIgnore]
        public Uri Location { get; set; }

        [JsonPropertyName("status")]
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public ACMEStatus Status { get; set; }

        [JsonPropertyName("expires")]
        public DateTime? Expires { get; set; }

        [JsonPropertyName("identifiers")]
        public OrderIdentifier[] Identifiers { get; set; }

        [JsonPropertyName("notBefore")]
        public DateTime? NotBefore { get; set; }

        [JsonPropertyName("notAfter")]
        public DateTime? NotAfter { get; set; }

        [JsonPropertyName("error")]
        public Problem Error { get; set; }

        [JsonPropertyName("authorizations")]
        public Uri[] Authorizations { get; set; }

        [JsonPropertyName("finalize")]
        public Uri Finalize { get; set; }

        [JsonPropertyName("certificate")]
        public Uri Certificate { get; set; }
    }
}
