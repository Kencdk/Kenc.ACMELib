namespace ACMELibCore.Test.Mocks
{
    using System.Net.Http;
    using Kenc.ACMELib;

    public class TestHttpClientFactory : IHttpClientFactory
    {
        private HttpMessageHandler messageHandler;

        public TestHttpClientFactory(HttpMessageHandler messageHandler)
        {
            this.messageHandler = messageHandler;
        }

        public HttpClient CreateHttpClient()
        {
            var httpClient = new HttpClient(this.messageHandler);
            return httpClient;
        }
    }
}