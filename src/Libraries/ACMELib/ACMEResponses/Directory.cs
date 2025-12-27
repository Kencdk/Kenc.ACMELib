namespace Kenc.ACMELib.ACMEResponses
{
    using System;
    using System.Collections.Generic;
    using System.Text.Json;
    using System.Text.Json.Serialization;

    /// <summary>
    /// Describes a directory in the ACME protocol.
    /// </summary>
    public class ACMEDirectory
    {
        [JsonPropertyName("keyChange")]
        public Uri KeyChange { get; set; }

        [JsonPropertyName("newNonce")]
        public Uri NewNonce { get; set; }

        [JsonPropertyName("newAccount")]
        public Uri NewAccount { get; set; }

        [JsonPropertyName("newOrder")]
        public Uri NewOrder { get; set; }

        [JsonPropertyName("newAuthz")]
        public Uri NewAuthz { get; set; }

        [JsonPropertyName("revokeCert")]
        public Uri RevokeCertificate { get; set; }

        [JsonPropertyName("meta")]
        public DirectoryMeta Meta { get; set; }

        [JsonPropertyName("renewalInfo")]
        public Uri RenewalInfo { get; set; }

        [JsonExtensionData]
        public Dictionary<string, JsonElement> ExtensionData { get; set; }
    }

    public class DirectoryMeta
    {
        [JsonPropertyName("termsOfService")]
        public string TermsOfService { get; set; }

        [JsonPropertyName("website")]
        public Uri Website { get; set; }

        [JsonPropertyName("caaIdentities")]
        public IReadOnlyList<string> CaaIdentities { get; set; }

        [JsonPropertyName("profiles")]
        public IReadOnlyDictionary<string, Uri> Profiles { get; set; }

        [JsonExtensionData]
        public Dictionary<string, JsonElement> ExtensionData { get; set; }
    }
}
