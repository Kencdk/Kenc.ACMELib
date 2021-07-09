namespace ACMELibCore.Test
{
    using System;
    using System.Net;
    using System.Net.Http;
    using System.Security.Cryptography;
    using System.Text;
    using System.Text.Json;
    using System.Threading;
    using Kenc.ACMELib;
    using Kenc.ACMELib.ACMEResponses;
    using Moq;

    internal class TestSystem
    {
        private RSA rsaKey;
        private readonly Mock<HttpClient> restClient = new Mock<HttpClient>();

        public TestSystem()
        {
        }

        public TestSystem WithRSAKey(RSA key)
        {
            rsaKey = key;
            return this;
        }

        public TestSystem WithResponse(Uri uri, string resultString, HttpStatusCode statusCode = HttpStatusCode.OK, string contentType = "application/json", Uri locationHeader = null)
        {
            var responseMessage = new HttpResponseMessage
            {
                StatusCode = statusCode,
                Content = new StringContent(resultString, Encoding.UTF8, contentType),
            };

            responseMessage.Headers.Add("Replay-Nonce", Guid.NewGuid().ToString());
            if (locationHeader != null)
            {
                responseMessage.Headers.Location = locationHeader;
            }

            restClient.Setup(x => x.SendAsync(It.Is<HttpRequestMessage>(y => y.RequestUri == uri), It.IsAny<CancellationToken>()))
                .ReturnsAsync(responseMessage);
            return this;
        }

        public TestSystem WithResponse<TResult>(Uri uri, TResult result, HttpStatusCode statusCode = HttpStatusCode.OK, string contentType = "application/json", Uri locationHeader = null) where TResult : class
        {
            var resultString = JsonSerializer.Serialize(result);
            return WithResponse(uri, resultString, statusCode, contentType, locationHeader);
        }

        public TestSystem WithDirectoryResponse()
        {
            return WithResponse(TestHelpers.DirectoryUri, TestHelpers.AcmeDirectory);
        }

        public TestSystem WithErrorResponse(Uri uri, string detail, string type, int intcode, HttpStatusCode httpStatusCode)
        {
            var problem = new Problem
            {
                Detail = detail,
                Status = intcode,
                Type = type
            };

            return WithResponse(uri, problem, httpStatusCode, contentType: "application/problem+json");
        }

        public (ACMEClient, Mock<HttpClient>) Build()
        {
            if (rsaKey == null)
            {
                rsaKey = RSA.Create();
            }

            var acmeClient = new ACMEClient(TestHelpers.BaseUri, rsaKey, restClient.Object);
            return (acmeClient, restClient);
        }
    }
}