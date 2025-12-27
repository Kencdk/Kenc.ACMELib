namespace Kenc.ACMELibCore.Tests.RequestMethodTests
{
    using System;
    using System.Net;
    using System.Threading.Tasks;
    using FluentAssertions;
    using Kenc.ACMELib;
    using Kenc.ACMELib.ACMEObjects;
    using Kenc.ACMELib.Exceptions;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class RegisterAccountTests
    {
        [TestMethod]
        public async Task ValidateRegisterAsyncPositiveFlow()
        {
            var testContacts = new[] { "mailto:test@test.invalid", "+1 012-3456-789" };
            var returnAccount = new Account { };

            TestSystem testSystem = new TestSystem()
                .WithDirectoryResponse()
                .WithResponse(TestHelpers.AcmeDirectory.NewAccount, returnAccount, locationHeader: new Uri(TestHelpers.BaseUri, "/account/1234"));
            (ACMEClient acmeClient, _) = testSystem.Build();

            await acmeClient.RegisterAsync(testContacts);

            /*
            restClient.Verify(rc => rc.PostAsync<Account>(TestHelpers.acmeDirectory.NewAccount,
                It.Is<Account>(req => req.Contacts == testContacts && req.TermsOfServiceAgreed),
                It.IsAny<CancellationToken>()), Times.Once, "Rest Client wasn't called with expected parameters.");

            restClient.Verify(rc => rc.GetAsync<ACMEDirectory>(new Uri(TestHelpers.baseUri, "directory"), It.IsAny<CancellationToken>()),
                Times.Once, "Rest Client wasn't called with expected parameters.");*/
        }

        [TestMethod]
        public async Task ValidateRegisterAsyncThrowsExceptionFromRestClient()
        {
            TestSystem testSystem = new TestSystem()
                .WithDirectoryResponse()
                .WithErrorResponse(TestHelpers.AcmeDirectory.NewAccount, "detail", string.Empty, 1234, HttpStatusCode.BadRequest);
            (ACMEClient acmeClient, _) = testSystem.Build();

            var testContacts = new[] { "mailto:test@test.invalid", "+1 012-3456-789" };
            var action = () => acmeClient.RegisterAsync(testContacts);

            await action.Should().ThrowExactlyAsync<ACMEException>().Where(ex => ex.Status == 1234 && ex.Message == "detail");
        }
    }
}