namespace Kenc.ACMELib.ACMEEntities
{
    using System;
    using Kenc.ACMELib.ACMEResponses;
    using Newtonsoft.Json;

    /// <summary>
    /// Describes an order in the ACME protocol.
    /// </summary>
    public class Order : ILocationResponse
    {
        [JsonIgnore]
        public static string Processing = "processing";

        [JsonIgnore]
        public static string Valid = "valid";

        [JsonIgnore]
        public Uri Location { get; set; }

        [JsonProperty("status")]
        public string Status { get; set; }

        [JsonProperty("expires")]
        public DateTime? Expires { get; set; }

        [JsonProperty("identifiers")]
        public OrderIdentifier[] Identifiers { get; set; }

        [JsonProperty("notBefore")]
        public DateTime? NotBefore { get; set; }

        [JsonProperty("notAfter")]
        public DateTime? NotAfter { get; set; }

        [JsonProperty("error")]
        public Problem Error { get; set; }

        [JsonProperty("authorizations")]
        public Uri[] Authorizations { get; set; }

        [JsonProperty("finalize")]
        public Uri Finalize { get; set; }

        [JsonProperty("certificate")]
        public Uri Certificate { get; set; }
    }
}
