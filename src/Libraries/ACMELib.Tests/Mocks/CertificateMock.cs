namespace ACMELibCore.Test.Mocks
{
    using System.Security.Cryptography.X509Certificates;

    internal class CertificateMock : X509Certificate2
    {
        private readonly string publicKeyString;

#pragma warning disable SYSLIB0026 // Type or member is obsolete
        public CertificateMock(string publicKeyString)
        {
            this.publicKeyString = publicKeyString;
        }
#pragma warning restore SYSLIB0026 // Type or member is obsolete

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
