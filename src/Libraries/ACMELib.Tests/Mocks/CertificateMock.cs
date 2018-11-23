namespace ACMELibCore.Test.Mocks
{
    using System.Security.Cryptography.X509Certificates;

    class CertificateMock : X509Certificate2
    {
        private readonly string publicKeyString;

        public CertificateMock(string publicKeyString)
        {
            this.publicKeyString = publicKeyString;
        }

        public override string GetPublicKeyString()
        {
            return publicKeyString;
        }

        public override string GetRawCertDataString()
        {
            return base.GetRawCertDataString();
        }

        public override byte[] GetRawCertData()
        {
            return new byte[]
            {
                1,2,3,4,5,6,7,8,9,10
            };
        }
    }
}
