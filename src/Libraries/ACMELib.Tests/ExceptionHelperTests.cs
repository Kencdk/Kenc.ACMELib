namespace Kenc.ACMELibCore.Tests
{
    using System;
    using FluentAssertions;
    using FluentAssertions.Specialized;
    using Kenc.ACMELib.ACMEResponses;
    using Kenc.ACMELib.Exceptions;
    using Kenc.ACMELib.Exceptions.API;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class ExceptionHelperTests
    {
        [TestMethod]
        [DataRow("urn:ietf:params:acme:error:accountdoesnotexist", typeof(AccountDoesNotExistException))]
        [DataRow("urn:ietf:params:acme:error:badcsr", typeof(BadCSRException))]
        [DataRow("urn:ietf:params:acme:error:badrevocationreason", typeof(BadRevocationReasonException))]
        [DataRow("urn:ietf:params:acme:error:badsignaturealgorithm", typeof(BadSignatureAlgorithmException))]
        [DataRow("urn:ietf:params:acme:error:rejectedidentifier", typeof(RejectedIdentifierException))]
        [DataRow("urn:ietf:params:acme:error:caa", typeof(CAAException))]
        [DataRow("urn:ietf:params:acme:error:connection", typeof(ConnectionException))]
        [DataRow("urn:ietf:params:acme:error:dns", typeof(DNSException))]
        [DataRow("urn:ietf:params:acme:error:externalaccountrequired", typeof(ExternalAccountRequiredException))]
        [DataRow("urn:ietf:params:acme:error:incorrectresponse", typeof(IncorrectResponseException))]
        [DataRow("urn:ietf:params:acme:error:invalidcontact", typeof(InvalidContactException))]
        [DataRow("urn:ietf:params:acme:error:malformed", typeof(MalformedException))]
        [DataRow("urn:ietf:params:acme:error:ratelimited", typeof(RateLimitedException))]
        [DataRow("urn:ietf:params:acme:error:serverinternal", typeof(ServerInternalException))]
        [DataRow("urn:ietf:params:acme:error:tls", typeof(TLSException))]
        [DataRow("urn:ietf:params:acme:error:unauthorized", typeof(UnauthorizedException))]
        [DataRow("urn:ietf:params:acme:error:unsupportedcontact", typeof(UnsupportedContactException))]
        [DataRow("urn:ietf:params:acme:error:unsupportedidentifier", typeof(UnsupportedIdentifierException))]
        [DataRow("urn:ietf:params:acme:error:useractionrequired", typeof(UserActionRequiredException))]
        public void ValidateVariousExceptionsAreThown(string problemType, Type expectedExceptionType)
        {
            var problem = new Problem
            {
                Type = problemType
            };
            Action action = () => ExceptionHelper.ThrowException(problem);
            ExceptionAssertions<Exception> exception = action.Should().Throw();
            exception.Which.Should().BeOfType(expectedExceptionType);
        }
    }
}