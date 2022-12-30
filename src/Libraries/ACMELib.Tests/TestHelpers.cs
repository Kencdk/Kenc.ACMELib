namespace ACMELibCore.Test
{
    using System;
    using System.Linq.Expressions;
    using System.Net.Http;
    using System.Text;
    using System.Text.Json;
    using System.Threading;
    using System.Threading.Tasks;
    using Kenc.ACMELib.ACMEResponses;
    using Moq;
    using Moq.Protected;

    public static class TestHelpers
    {
        public static readonly Uri BaseUri = new("https://acmetest.invalid");
        public static readonly Uri DirectoryUri = new(BaseUri, "directory");

        public static readonly ACMEDirectory AcmeDirectory = new()
        {
            KeyChange = new Uri(BaseUri, "keyChange"),
            Meta = new DirectoryMeta
            {
                TermsOfService = "",
            },
            NewAccount = new Uri(BaseUri, "newAccount"),
            NewNonce = new Uri(BaseUri, "newNonce"),
            NewOrder = new Uri(BaseUri, "newOrder"),
            RevokeCertificate = new Uri(BaseUri, "revokeCertificate")
        };

        public static Mock<HttpMessageHandler> AddResponse(this Mock<HttpMessageHandler> messageHandler, Uri uri, HttpResponseMessage responseMessage)
        {
            Expression uriExpression = ItExpr.Is<HttpRequestMessage>(x => x.RequestUri == uri);
            messageHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", uriExpression, ItExpr.IsAny<CancellationToken>())
                .Returns((HttpRequestMessage request, CancellationToken token) =>
                {
                    responseMessage.RequestMessage = request;
                    return Task.FromResult(responseMessage);
                })
                .Verifiable();

            return messageHandler;
        }

        public static Mock<HttpMessageHandler> AddDirectoryResponse(this Mock<HttpMessageHandler> messageHandler)
        {
            var serializedDictionary = JsonSerializer.Serialize(AcmeDirectory);
            var responseMessage = new HttpResponseMessage
            {
                StatusCode = System.Net.HttpStatusCode.OK,
                Content = new StringContent(serializedDictionary, Encoding.UTF8, "application/json"),
            };

            return messageHandler.AddResponse(DirectoryUri, responseMessage);
        }

        public static Mock<HttpMessageHandler> AddNonce(this Mock<HttpMessageHandler> messageHandler)
        {
            var responseMessage = new HttpResponseMessage
            {
                StatusCode = System.Net.HttpStatusCode.OK,
                Content = null
            };

            responseMessage.Headers.Add("Replay-Nonce", Guid.NewGuid().ToString());
            return messageHandler.AddResponse(AcmeDirectory.NewNonce, responseMessage);
        }
    }
}
