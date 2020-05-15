namespace Kenc.ACMELib.JsonWebSignature
{
    using System;
    using System.Security.Cryptography;
    using System.Text;
    using Kenc.ACMELib.ACMEEntities;
    using Newtonsoft.Json;

    /// <summary>
    /// Class implementing Json Web Signature, https://tools.ietf.org/html/rfc7515
    /// </summary>
    public class Jws
    {
        private readonly Jwk jwk;
        private readonly RSA rsa;

        public Jws(RSA rsa, string keyId)
        {
            this.rsa = rsa ?? throw new ArgumentNullException(nameof(rsa));

            RSAParameters publicParameters = rsa.ExportParameters(false);
            jwk = new Jwk
            {
                KeyType = "RSA",
                Exponent = Utilities.Base64UrlEncoded(publicParameters.Exponent),
                Modulus = Utilities.Base64UrlEncoded(publicParameters.Modulus),
                KeyId = keyId
            };
        }

        public JwsMessage Encode<TPayload>(TPayload payload, JwsHeader protectedHeader)
        {
            if (protectedHeader == null)
            {
                throw new ArgumentNullException(nameof(protectedHeader));
            }

            protectedHeader.Algorithm = "RS256";
            if (!string.IsNullOrWhiteSpace(jwk.KeyId))
            {
                protectedHeader.KeyId = jwk.KeyId;
            }
            else
            {
                protectedHeader.Key = jwk;
            }

            var message = new JwsMessage
            {
                Payload = Utilities.Base64UrlEncoded(JsonConvert.SerializeObject(payload)),
                Protected = Utilities.Base64UrlEncoded(JsonConvert.SerializeObject(protectedHeader))
            };

            message.Signature = Utilities.Base64UrlEncoded(
                rsa.SignData(Encoding.ASCII.GetBytes(message.Protected + "." + message.Payload),
                    HashAlgorithmName.SHA256,
                    RSASignaturePadding.Pkcs1));

            return message;
        }

        public string GetKeyAuthorization(string token)
        {
            return token + "." + jwk.GetSha256Thumbprint();
        }

        public string GetDNSKeyAuthorization(string token)
        {
            var json = $"{token}.{jwk.GetSha256Thumbprint()}";
            using var sha256 = SHA256.Create();
            return Utilities.Base64UrlEncoded(sha256.ComputeHash(Encoding.UTF8.GetBytes(json)));
        }

        internal void SetKeyId(Account account)
        {
            jwk.KeyId = account.Location.ToString();
        }
    }
}