namespace Kenc.ACMELib.ACMEResponses
{
    using System;
    using Newtonsoft.Json;

    /// <summary>
    /// Describes a directory in the ACME protocol.
    /// </summary>
    public class ACMEDirectory
    {
        [JsonProperty("keyChange")]
        public Uri KeyChange { get; set; }

        [JsonProperty("newNonce")]
        public Uri NewNonce { get; set; }

        [JsonProperty("newAccount")]
        public Uri NewAccount { get; set; }

        [JsonProperty("newOrder")]
        public Uri NewOrder { get; set; }

        [JsonProperty("revokeCert")]
        public Uri RevokeCertificate { get; set; }

        [JsonProperty("meta")]
        public DirectoryMeta Meta { get; set; }
    }

    public class DirectoryMeta
    {
        [JsonProperty("termsOfService")]
        public string TermsOfService { get; set; }
    }
}
