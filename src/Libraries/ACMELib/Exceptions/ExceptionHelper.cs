namespace Kenc.ACMELib.Exceptions
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Kenc.ACMELib.ACMEResponses;
    using Kenc.ACMELib.Exceptions.API;

    /// <summary>
    /// Helps find the right exception for acme problems.
    /// </summary>
    public static class ExceptionHelper
    {
        private static List<Type> KnownExceptions = new List<Type>
        {
            typeof(AccountDoesNotExistException),
            typeof(BadCSRException),
            typeof(BadNonceException),
            typeof(BadRevocationReasonException),
            typeof(BadSignatureAlgorithmException),
            typeof(CAAException),
            typeof(ConnectionException),
            typeof(DNSException),
            typeof(ExternalAccountRequiredException),
            typeof(IncorrectResponseException),
            typeof(InvalidContactException),
            typeof(MalformedException),
            typeof(RateLimitedException),
            typeof(RejectedIdentifierException),
            typeof(ServerInternalException),
            typeof(TLSException),
            typeof(UnauthorizedException),
            typeof(UnsupportedContactException),
            typeof(UnsupportedIdentifierException),
            typeof(UserActionRequiredException)
        };

        /// <summary>
        /// Throw an exception based on the <paramref name="problem"/>
        /// </summary>
        /// <param name="problem">Problem from the ACME protocol.</param>
        public static void ThrowException(Problem problem)
        {
            var typeWhereAttributeMatches = KnownExceptions.Where(ke =>
            {
                var attribute = ke.CustomAttributes.FirstOrDefault(ca => ca.AttributeType.IsEquivalentTo(typeof(ACMEExceptionAttribute)));
                return (string)attribute.ConstructorArguments[0].Value == problem.Type;
            }).FirstOrDefault();

            if (typeWhereAttributeMatches != null)
            {
                throw (ACMEException)Activator.CreateInstance(typeWhereAttributeMatches, new object[] { problem.Status, problem.Detail });
            }

            throw new ACMEException(problem.Status, problem.Detail);
        }
    }
}