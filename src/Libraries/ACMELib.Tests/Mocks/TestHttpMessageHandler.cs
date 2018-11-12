namespace ACMELibCore.Test.Mocks
{
    using System;
    using System.Collections.Generic;
    using System.Net.Http;
    using System.Threading;
    using System.Threading.Tasks;

    public class TestHttpMessageHandler : HttpMessageHandler
    {
        private Dictionary<Uri, Func<HttpRequestMessage, HttpResponseMessage>> responses = new Dictionary<Uri, Func<HttpRequestMessage, HttpResponseMessage>>();

        public virtual HttpResponseMessage Send(HttpRequestMessage request)
        {
            throw new NotImplementedException("Configure mock by using setup!");
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            if (responses.ContainsKey(request.RequestUri))
            {
                var result = responses[request.RequestUri].Invoke(request);
                return Task.FromResult(result);
            }

            return Task.FromResult(Send(request));
        }

        public TestHttpMessageHandler WithResponse(Uri uri, Func<HttpRequestMessage, HttpResponseMessage> response)
        {
            responses.Add(uri, response);
            return this;
        }

        public TestHttpMessageHandler WithDirectory(Uri uri)
        {
            return this;
        }
    }
}