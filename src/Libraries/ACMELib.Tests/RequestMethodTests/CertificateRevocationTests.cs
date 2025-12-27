namespace Kenc.ACMELibCore.Tests.RequestMethodTests
{
    using System;
    using System.Net.Http;
    using System.Threading.Tasks;
    using FluentAssertions;
    using Kenc.ACMELib;
    using Kenc.ACMELibCore.Tests.Mocks;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class CertificateRevocationTests
    {
        [TestMethod]
        public async Task ValidateSuccesfullValidationFlow()
        {
            TestSystem testSystem = new TestSystem()
                .WithDirectoryResponse()
                .WithResponse(TestHelpers.AcmeDirectory.RevokeCertificate, string.Empty);
            (ACMEClient acmeClient, _) = testSystem.Build();

            var testCertificate = new CertificateMock(Guid.NewGuid().ToString());

            await acmeClient.GetDirectoryAsync();
            await acmeClient.RevokeCertificateAsync(testCertificate, RevocationReason.PriviledgeWithdrawn);

            /*
             restClient.Verify(rc => rc.PostAsyn<string>(TestHelpers.acmeDirectory.RevokeCertificate,
             It.Is<CertificateRevocationRequest>(req => req.Reason == RevocationReason.PriviledgeWithdrawn),
             It.IsAny<CancellationToken>()), Times.Once, "Rest Client wasn't called with expected parameters."); */
        }

        [TestMethod]
        public async Task ValidateAcmeClientThrowsArgumentNullExceptionWhenCertificateIsNull()
        {
            TestSystem testSystem = new TestSystem().WithDirectoryResponse();
            (ACMEClient acmeClient, Moq.Mock<HttpClient> _) = testSystem.Build();

            var action = async () => await acmeClient.RevokeCertificateAsync(null, RevocationReason.PriviledgeWithdrawn);
            await action.Should().ThrowAsync<ArgumentNullException>();
        }
    }
}