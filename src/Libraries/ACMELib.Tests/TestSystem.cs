namespace ACMELibCore.Test
{
    using System;
    using System.Security.Cryptography;
    using System.Threading;
    using System.Threading.Tasks;
    using Kenc.ACMELib;
    using Kenc.ACMELib.ACMEResponses;
    using Kenc.ACMELib.JsonWebSignature;
    using Moq;

    class TestSystem
    {
        private RSA rsaKey;
        private Mock<IRestClient> restClient = new Mock<IRestClient>();

        public TestSystem()
        {
        }

        public TestSystem WithRSAKey(RSA key)
        {
            rsaKey = key;
            return this;
        }

        public TestSystem WithGetResponse<TResult>(Uri uri, TResult result, string resultString) where TResult : class
        {
            restClient.Setup(rc => rc.GetAsync<TResult>(It.Is<Uri>(v => v == uri), It.IsAny<CancellationToken>())).Returns(Task.FromResult<(TResult, string)>((result, resultString)));
            return this;
        }

        public TestSystem WithDirectoryResponse()
        {
            return WithGetResponse<ACMEDirectory>(TestHelpers.directoryUri, TestHelpers.acmeDirectory, "");
        }

        public (ACMEClient, Mock<IRestClient>) Build()
        {
            if (rsaKey == null)
            {
                rsaKey = RSA.Create();
            }

            var restClientFactory = new Mock<IRestClientFactory>();
            restClientFactory.Setup(rcf => rcf.CreateRestClient(It.IsAny<Jws>())).Returns(restClient.Object);

            var acmeClient = new ACMEClient(TestHelpers.baseUri.ToString(), rsaKey, restClientFactory.Object);
            return (acmeClient, restClient);
        }
    }
}