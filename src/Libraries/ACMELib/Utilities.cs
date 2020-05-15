namespace Kenc.ACMELib
{
    using System;
    using System.Security.Cryptography;
    using System.Text;
    using Kenc.ACMELib.JsonWebSignature;

    public static class Utilities
    {
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

        public static string GetSha256Thumbprint(this Jwk jwk)
        {
            if (jwk == null)
            {
                throw new ArgumentNullException(nameof(jwk));
            }

            var json = "{\"e\":\"" + jwk.Exponent + "\",\"kty\":\"RSA\",\"n\":\"" + jwk.Modulus + "\"}";

            using var sha256 = SHA256.Create();
            return Utilities.Base64UrlEncoded(sha256.ComputeHash(Encoding.UTF8.GetBytes(json)));
        }
    }
}
