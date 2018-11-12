namespace ACMELibCore.Test.RequestMethodTests
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Kenc.ACMELib.ACMEEntities;
    using Kenc.ACMELib.ACMEResponses;
    using Kenc.ACMELib.Exceptions;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    [TestClass]
    public class RegisterAccountTests
    {
        [TestMethod]
        public async Task ValidateRegisterAsyncPositiveFlow()
        {
            var testSystem = new TestSystem().WithDirectoryResponse();
            var (acmeClient, restClient) = testSystem.Build();

            var testContacts = new[] { "mailto:test@test.invalid", "+1 012-3456-789" };
            var returnAccount = new Account { };
            restClient.Setup(rc => rc.PostAsync<Account>(TestHelpers.acmeDirectory.NewAccount, It.IsAny<Account>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult((returnAccount, string.Empty)));

            await acmeClient.RegisterAsync(testContacts);

            restClient.Verify(rc => rc.PostAsync<Account>(TestHelpers.acmeDirectory.NewAccount,
                It.Is<Account>(req => req.Contacts == testContacts && req.TermsOfServiceAgreed),
                It.IsAny<CancellationToken>()), Times.Once, "Rest Client wasn't called with expected parameters.");

            restClient.Verify(rc => rc.GetAsync<ACMEDirectory>(new Uri(TestHelpers.baseUri, "directory"), It.IsAny<CancellationToken>()),
                Times.Once, "Rest Client wasn't called with expected parameters.");
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidServerResponse))]
        public async Task ValidateRegisterAsyncThrowsInvalidServerResponseForBadResponse()
        {
            var testSystem = new TestSystem().WithDirectoryResponse();
            var (acmeClient, restClient) = testSystem.Build();

            var testContacts = new[] { "mailto:test@test.invalid", "+1 012-3456-789" };
            var returnAccount = new Account { };
            restClient.Setup(rc => rc.PostAsync<Account>(TestHelpers.acmeDirectory.NewAccount, It.IsAny<Account>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(((Account)null, string.Empty)));

            await acmeClient.RegisterAsync(testContacts);
        }

        [TestMethod]
        public async Task ValidateRegisterAsyncThrowsExceptionFromRestClient()
        {
            var testSystem = new TestSystem().WithDirectoryResponse();
            var (acmeClient, restClient) = testSystem.Build();

            var testContacts = new[] { "mailto:test@test.invalid", "+1 012-3456-789" };
            var returnAccount = new Account { };
            restClient.Setup(rc => rc.PostAsync<Account>(TestHelpers.acmeDirectory.NewAccount, It.IsAny<Account>(), It.IsAny<CancellationToken>()))
                .Throws(new ACMEException(1234, "detail"));

            try
            {
                await acmeClient.RegisterAsync(testContacts);
            }
            catch (ACMEException exception)
            {
                Assert.AreEqual(1234, exception.Status);
                Assert.AreEqual("detail", exception.Message);

                return;
            }

            Assert.Fail("Failed to stop in catch.");
        }
    }
}