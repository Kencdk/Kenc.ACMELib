namespace Kenc.ACMELibCore.Tests.Mocks
{
    using System;
    using System.Collections.Generic;
    using System.Net.Http;
    using System.Threading;
    using System.Threading.Tasks;

    public class TestHttpMessageHandler : HttpMessageHandler
    {
        private readonly Dictionary<Uri, Func<HttpRequestMessage, HttpResponseMessage>> responses = [];

        public virtual HttpResponseMessage Send(HttpRequestMessage request)
        {
            throw new NotImplementedException("Configure mock by using setup!");
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            if (responses.TryGetValue(request.RequestUri, out Func<HttpRequestMessage, HttpResponseMessage> value))
            {
                HttpResponseMessage result = value.Invoke(request);
                return Task.FromResult(result);
            }

            return Task.FromResult(Send(request));
        }

        public TestHttpMessageHandler WithResponse(Uri uri, Func<HttpRequestMessage, HttpResponseMessage> response)
        {
            responses.Add(uri, response);
            return this;
        }

#pragma warning disable IDE0060 // Remove unused parameter
        public TestHttpMessageHandler WithDirectory(Uri uri)
#pragma warning restore IDE0060 // Remove unused parameter
        {
            return this;
        }
    }
}