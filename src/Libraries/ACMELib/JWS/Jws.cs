namespace Kenc.ACMELib.JWS
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
        private readonly Jwk _jwk;
        private readonly RSA _rsa;

        public Jws(RSA rsa, string keyId)
        {
            _rsa = rsa ?? throw new ArgumentNullException(nameof(rsa));

            var publicParameters = rsa.ExportParameters(false);

            _jwk = new Jwk
            {
                KeyType = "RSA",
                Exponent = Base64UrlEncoded(publicParameters.Exponent),
                Modulus = Base64UrlEncoded(publicParameters.Modulus),
                KeyId = keyId
            };
        }

        public JwsMessage Encode<TPayload>(TPayload payload, JwsHeader protectedHeader)
        {
            protectedHeader.Algorithm = "RS256";
            if (!string.IsNullOrWhiteSpace(_jwk.KeyId))
            {
                protectedHeader.KeyId = _jwk.KeyId;
            }
            else
            {
                protectedHeader.Key = _jwk;
            }

            var message = new JwsMessage
            {
                Payload = Base64UrlEncoded(JsonConvert.SerializeObject(payload)),
                Protected = Base64UrlEncoded(JsonConvert.SerializeObject(protectedHeader))
            };

            message.Signature = Base64UrlEncoded(
                _rsa.SignData(Encoding.ASCII.GetBytes(message.Protected + "." + message.Payload),
                    HashAlgorithmName.SHA256,
                    RSASignaturePadding.Pkcs1));

            return message;
        }

        private string GetSha256Thumbprint()
        {
            var json = "{\"e\":\"" + _jwk.Exponent + "\",\"kty\":\"RSA\",\"n\":\"" + _jwk.Modulus + "\"}";

            using (var sha256 = SHA256.Create())
            {
                return Base64UrlEncoded(sha256.ComputeHash(Encoding.UTF8.GetBytes(json)));
            }
        }

        public string GetKeyAuthorization(string token)
        {
            return token + "." + GetSha256Thumbprint();
        }

        public string GetDNSKeyAuthorization(string token)
        {
            var json = $"{token}.{GetSha256Thumbprint()}";
            using (var sha256 = SHA256.Create())
            {
                return Base64UrlEncoded(sha256.ComputeHash(Encoding.UTF8.GetBytes(json)));
            }
        }

        public static string Base64UrlEncoded(string s)
        {
            return Base64UrlEncoded(Encoding.UTF8.GetBytes(s));
        }

        public static string Base64UrlEncoded(byte[] arg)
        {
            return Convert.ToBase64String(arg) // encode to base64
                .Split('=')[0] // Remove any trailing ='s
                .Replace('+', '-') // convert + to -
                .Replace('/', '_'); // convert / to _
        }

        internal void SetKeyId(Account account)
        {
            _jwk.KeyId = account.Location.ToString();
        }
    }
}