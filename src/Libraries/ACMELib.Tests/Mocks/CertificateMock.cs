namespace ACMELibCore.Test.Mocks
{
    using System.Security.Cryptography.X509Certificates;

    class CertificateMock : X509Certificate2
    {
        private string publicKeyString;

        public CertificateMock(string publicKeyString)
        {
            this.publicKeyString = publicKeyString;
        }

        public override string GetPublicKeyString()
        {
            return this.publicKeyString;
        }
    }
}
