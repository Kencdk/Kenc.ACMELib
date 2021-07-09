namespace Kenc.ACMELib.ACMEObjects
{
    using System.Text.Json.Serialization;

    /// <summary>
    /// Describes an authorize challenge in the ACME protocol.
    /// </summary>
    public class AuthorizeChallenge
    {
        [JsonPropertyName("keyAuthorization")]
        public string KeyAuthorization { get; set; }
    }
}
