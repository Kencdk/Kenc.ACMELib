namespace Kenc.ACMELibCore.Tests
{
    using System.Net.Http;
    using System.Security.Cryptography;
    using System.Threading;
    using System.Threading.Tasks;
    using FluentAssertions;
    using global::ACMELibCore.Test;
    using Kenc.ACMELib;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Moq.Protected;

    [TestClass]
    public class AcmeClientTests
    {
        [TestMethod]
        public async Task PostAddsProperHeader()
        {
            HttpRequestMessage httpRequestMessage = null;

            Mock<HttpMessageHandler> messageHandlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict)
                .AddDirectoryResponse()
                .AddNonce();

            messageHandlerMock.Protected()
                     .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.Is<HttpRequestMessage>(x => x.RequestUri == TestHelpers.AcmeDirectory.NewAccount), ItExpr.IsAny<CancellationToken>())
                     .Returns((HttpRequestMessage request, CancellationToken token) =>
                     {
                         httpRequestMessage = request;
                         return Task.FromResult(new HttpResponseMessage());
                     })
                     .Verifiable();

            var httpClient = new HttpClient(messageHandlerMock.Object);

            var rsaKey = RSA.Create();
            var acmeClient = new ACMEClient(TestHelpers.BaseUri, rsaKey, httpClient);
            await acmeClient.RegisterAsync(new[] { "test@test.test" });

            // assert
            httpRequestMessage.Content.Headers.ContentType.MediaType.Should().Be("application/jose+json");
            httpRequestMessage.Content.Headers.ContentType.CharSet.Should().BeNull();
        }

        [TestMethod]
        public async Task GetsANonceIfNoneIsPresent()
        {
            Mock<HttpMessageHandler> messageHandlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict)
                .AddDirectoryResponse()
                .AddNonce();

            messageHandlerMock.Protected()
                     .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.Is<HttpRequestMessage>(x => x.RequestUri == TestHelpers.AcmeDirectory.NewAccount), ItExpr.IsAny<CancellationToken>())
                     .Returns((HttpRequestMessage request, CancellationToken token) => Task.FromResult(new HttpResponseMessage()))
                     .Verifiable();

            var httpClient = new HttpClient(messageHandlerMock.Object);

            var rsaKey = RSA.Create();
            var acmeClient = new ACMEClient(TestHelpers.BaseUri, rsaKey, httpClient);
            await acmeClient.RegisterAsync(new[] { "test@test.test" });

            messageHandlerMock.VerifyAll();
        }
    }
}
