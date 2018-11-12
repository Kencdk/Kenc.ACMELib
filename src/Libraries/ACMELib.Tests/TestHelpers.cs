namespace ACMELibCore.Test
{
    using System;
    using Kenc.ACMELib.ACMEResponses;

    public static class TestHelpers
    {
        public static Uri baseUri = new Uri("https://acmetest.invalid");
        public static Uri directoryUri = new Uri(baseUri, "directory");
        public static ACMEDirectory acmeDirectory = new ACMEDirectory
        {
            KeyChange = new Uri(baseUri, "keyChange"),
            Meta = new DirectoryMeta
            {
                TermsOfService = "",
            },
            NewAccount = new Uri(baseUri, "newAccount"),
            NewNonce = new Uri(baseUri, "newNonce"),
            NewOrder = new Uri(baseUri, "newOrder"),
            RevokeCertificate = new Uri(baseUri, "revokeCertificate")
        };
    }
}
