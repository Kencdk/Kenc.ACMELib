namespace ACMELibCore.Test.RequestMethodTests
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using ACMELibCore.Test.Mocks;
    using Kenc.ACMELib;
    using Kenc.ACMELib.ACMEEntities;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    [TestClass]
    public class CertificateRevocationTests
    {
        [TestMethod]
        public async Task ValidateSuccesfullValidationFlow()
        {
            var testSystem = new TestSystem().WithDirectoryResponse();
            var (acmeClient, restClient) = testSystem.Build();

            restClient.Setup(rc => rc.PostAsync<string>(TestHelpers.acmeDirectory.RevokeCertificate, It.IsAny<CertificateRevocationRequest>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult((string.Empty, string.Empty)));

            var testCertificate = new CertificateMock(Guid.NewGuid().ToString());

            await acmeClient.GetDirectoryAsync();
            await acmeClient.RevokeCertificateAsync(testCertificate, RevocationReason.PriviledgeWithdrawn);

            restClient.Verify(rc => rc.PostAsync<string>(TestHelpers.acmeDirectory.RevokeCertificate,
                It.Is<CertificateRevocationRequest>(req => req.Reason == RevocationReason.PriviledgeWithdrawn),
                It.IsAny<CancellationToken>()), Times.Once, "Rest Client wasn't called with expected parameters.");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task ValidateAcmeClientThrowsArgumentNullExceptionWhenCertificateIsNull()
        {
            var testSystem = new TestSystem().WithDirectoryResponse();
            var (acmeClient, restClient) = testSystem.Build();

            await acmeClient.RevokeCertificateAsync(null, RevocationReason.PriviledgeWithdrawn);
        }
    }
}