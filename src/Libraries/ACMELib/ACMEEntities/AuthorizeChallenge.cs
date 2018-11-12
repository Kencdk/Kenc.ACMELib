namespace Kenc.ACMELib.ACMEEntities
{
    using Newtonsoft.Json;

    /// <summary>
    /// Describes an authorize challenge in the ACME protocol.
    /// </summary>
    public class AuthorizeChallenge
    {
        [JsonProperty("keyAuthorization")]
        public string KeyAuthorization { get; set; }

    }
}