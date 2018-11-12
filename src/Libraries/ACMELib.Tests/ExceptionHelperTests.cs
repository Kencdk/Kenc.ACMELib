namespace ACMELibCore.Test
{
    using Kenc.ACMELib.ACMEResponses;
    using Kenc.ACMELib.Exceptions;
    using Kenc.ACMELib.Exceptions.API;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class ExceptionHelperTests
    {
        [TestMethod]
        [ExpectedException(typeof(AccountDoesNotExistException))]
        public void ValidateAccountDoesNotExistExceptionIsThown()
        {
            var problem = new Problem
            {
                Type = "urn:ietf:params:acme:error:accountdoesnotexist"
            };

            ExceptionHelper.ThrowException(problem);
        }

        [TestMethod]
        [ExpectedException(typeof(BadCSRException))]
        public void ValidateBadCSRExceptionIsThown()
        {
            var problem = new Problem
            {
                Type = "urn:ietf:params:acme:error:badcsr"
            };

            ExceptionHelper.ThrowException(problem);
        }

        [TestMethod]
        [ExpectedException(typeof(BadRevocationReasonException))]
        public void ValidateBadRevocationReasonExceptionIsThown()
        {
            var problem = new Problem
            {
                Type = "urn:ietf:params:acme:error:badrevocationreason"
            };

            ExceptionHelper.ThrowException(problem);
        }

        [TestMethod]
        [ExpectedException(typeof(BadSignatureAlgorithmException))]
        public void ValidateBadSignatureAlgorithmExceptionIsThown()
        {
            var problem = new Problem
            {
                Type = "urn:ietf:params:acme:error:badsignaturealgorithm"
            };

            ExceptionHelper.ThrowException(problem);
        }

        [TestMethod]
        [ExpectedException(typeof(CAAException))]
        public void ValidateCAAExceptionIsThown()
        {
            var problem = new Problem
            {
                Type = "urn:ietf:params:acme:error:caa"
            };

            ExceptionHelper.ThrowException(problem);
        }

        [TestMethod]
        [ExpectedException(typeof(ConnectionException))]
        public void ValidateConnectionExceptionIsThown()
        {
            var problem = new Problem
            {
                Type = "urn:ietf:params:acme:error:connection"
            };

            ExceptionHelper.ThrowException(problem);
        }

        [TestMethod]
        [ExpectedException(typeof(DNSException))]
        public void ValidateDNSExceptionIsThown()
        {
            var problem = new Problem
            {
                Type = "urn:ietf:params:acme:error:dns"
            };

            ExceptionHelper.ThrowException(problem);
        }

        [TestMethod]
        [ExpectedException(typeof(ExternalAccountRequiredException))]
        public void ValidateExternalAccountRequiredExceptionIsThown()
        {
            var problem = new Problem
            {
                Type = "urn:ietf:params:acme:error:externalaccountrequired"
            };

            ExceptionHelper.ThrowException(problem);
        }

        [TestMethod]
        [ExpectedException(typeof(IncorrectResponseException))]
        public void ValidateIncorrectResponseExceptionIsThown()
        {
            var problem = new Problem
            {
                Type = "urn:ietf:params:acme:error:incorrectresponse"
            };

            ExceptionHelper.ThrowException(problem);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidContactException))]
        public void ValidateInvalidContactExceptionIsThown()
        {
            var problem = new Problem
            {
                Type = "urn:ietf:params:acme:error:invalidcontact"
            };

            ExceptionHelper.ThrowException(problem);
        }

        [TestMethod]
        [ExpectedException(typeof(MalformedException))]
        public void ValidateMalformedExceptionIsThown()
        {
            var problem = new Problem
            {
                Type = "urn:ietf:params:acme:error:malformed"
            };

            ExceptionHelper.ThrowException(problem);
        }

        [TestMethod]
        [ExpectedException(typeof(RateLimitedException))]
        public void ValidateRateLimitedExceptionIsThown()
        {
            var problem = new Problem
            {
                Type = "urn:ietf:params:acme:error:ratelimited"
            };

            ExceptionHelper.ThrowException(problem);
        }

        [TestMethod]
        [ExpectedException(typeof(RejectedIdentifierException))]
        public void ValidateRejectedIdentifierExceptionIsThown()
        {
            var problem = new Problem
            {
                Type = "urn:ietf:params:acme:error:rejectedidentifier"
            };

            ExceptionHelper.ThrowException(problem);
        }

        [TestMethod]
        [ExpectedException(typeof(ServerInternalException))]
        public void ValidateServerInternalExceptionIsThown()
        {
            var problem = new Problem
            {
                Type = "urn:ietf:params:acme:error:serverinternal"
            };

            ExceptionHelper.ThrowException(problem);
        }

        [TestMethod]
        [ExpectedException(typeof(TLSException))]
        public void ValidateTLSExceptionIsThown()
        {
            var problem = new Problem
            {
                Type = "urn:ietf:params:acme:error:tls"
            };

            ExceptionHelper.ThrowException(problem);
        }

        [TestMethod]
        [ExpectedException(typeof(UnauthorizedException))]
        public void ValidateUnauthorizedExceptionIsThown()
        {
            var problem = new Problem
            {
                Type = "urn:ietf:params:acme:error:unauthorized"
            };

            ExceptionHelper.ThrowException(problem);
        }

        [TestMethod]
        [ExpectedException(typeof(UnsupportedContactException))]
        public void ValidateUnsupportedContactExceptionIsThown()
        {
            var problem = new Problem
            {
                Type = "urn:ietf:params:acme:error:unsupportedcontact"
            };

            ExceptionHelper.ThrowException(problem);
        }

        [TestMethod]
        [ExpectedException(typeof(UnsupportedIdentifierException))]
        public void ValidateUnsupportedIdentifierExceptionIsThown()
        {
            var problem = new Problem
            {
                Type = "urn:ietf:params:acme:error:unsupportedidentifier"
            };

            ExceptionHelper.ThrowException(problem);
        }

        [TestMethod]
        [ExpectedException(typeof(UserActionRequiredException))]
        public void ValidateUserActionRequiredExceptionIsThown()
        {
            var problem = new Problem
            {
                Type = "urn:ietf:params:acme:error:useractionrequired"
            };

            ExceptionHelper.ThrowException(problem);
        }
    }
}