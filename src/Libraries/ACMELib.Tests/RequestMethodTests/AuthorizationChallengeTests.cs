namespace ACMELibCore.Test.RequestMethodTests
{
    using System;
    using System.Security.Cryptography;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using Kenc.ACMELib;
    using Kenc.ACMELib.ACMEResponses;
    using Kenc.ACMELib.JWS;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    [TestClass]
    public class AuthorizationChallengeTests
    {
        private readonly Uri authorizationUrl = new Uri("https://acme.invalid/challenge/1234");

        [TestMethod]
        public async Task AuthorizationChallengeGetsProperChallenge()
        {
            var rsa = RSA.Create();

            var testSystem = new TestSystem()
                .WithRSAKey(rsa)
                .WithDirectoryResponse();
            var (acmeClient, restClient) = testSystem.Build();

            var token = $"token{Guid.NewGuid().ToString()}";
            var dnstoken = $"token{Guid.NewGuid().ToString()}";

            var challengeResponse = new AuthorizationChallengeResponse
            {
                Challenges = new[] {
                    new AuthorizationChallenge
                    {
                        Status = "pending",
                        Url = authorizationUrl,
                        Type = "dns-01",
                        Token = dnstoken
                    },
                    new AuthorizationChallenge
                    {
                        Status = "pending",
                        Token = token,
                        Type = "http-01",
                        Url = authorizationUrl
                    }
                }
            };

            restClient.Setup(rc => rc.GetAsync<AuthorizationChallengeResponse>(authorizationUrl, It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult((challengeResponse, string.Empty)));

            var challenges = await acmeClient.GetAuthorizationChallengeAsync(authorizationUrl);

            var publicParameters = rsa.ExportParameters(false);
            var jwk = new Jwk
            {
                KeyType = "RSA",
                Exponent = Utilities.Base64UrlEncoded(publicParameters.Exponent),
                Modulus = Utilities.Base64UrlEncoded(publicParameters.Modulus),
                KeyId = string.Empty
            };

            Assert.AreEqual(2, challenges.Challenges.Length);

            var expectedDnsToken = string.Empty;
            using (var sha256 = SHA256.Create())
            {
                var json = $"{dnstoken}.{Utilities.GetSha256Thumbprint(jwk)}";
                expectedDnsToken = Utilities.Base64UrlEncoded(sha256.ComputeHash(Encoding.UTF8.GetBytes(json)));
            }

            Assert.AreEqual(expectedDnsToken, challengeResponse.Challenges[0].AuthorizationToken);
            Assert.AreEqual($"{token}.{Utilities.GetSha256Thumbprint(jwk)}", challengeResponse.Challenges[1].AuthorizationToken);
        }
    }
}