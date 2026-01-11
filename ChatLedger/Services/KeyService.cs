using System.Security.Cryptography;

namespace ChatLedger.Services
{
    public class KeyService
    {
        private readonly RSA _rsa;
        private const string KeyFileName = "authority_keys.xml";

        public KeyService()
        {
            _rsa = RSA.Create();

            if (File.Exists(KeyFileName))
            {
                string xmlKeys = File.ReadAllText(KeyFileName);
                _rsa.FromXmlString(xmlKeys);
            }
            else
            {
                string xmlKeys = _rsa.ToXmlString(true);
                File.WriteAllText(KeyFileName, xmlKeys);
            }
        }
        public string SignData(string dataHash)
        {
            byte[] dataBytes = Convert.FromHexString(dataHash);
            byte[] signatureBytes = _rsa.SignData(dataBytes, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
            return Convert.ToBase64String(signatureBytes);
        }

        public bool VerifySignature(string dataHash, string signatureBase64)
        {
            try
            {
                byte[] dataBytes = Convert.FromHexString(dataHash);
                byte[] signatureBytes = Convert.FromBase64String(signatureBase64);
                return _rsa.VerifyData(dataBytes, signatureBytes, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
            }
            catch
            {
                return false;
            }
        }
    }
}
