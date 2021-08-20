using Snoop.API.EncryptionService.Services.Interfaces;
using Snoop.Common.Models;

namespace Snoop.API.EncryptionService.Services.Encrypters
{

    // TODO. Should be similar to SymmetricEncrypter. Will allow DSA, ECDiffieHellman, ECDsa, RSA algorithms

    public class AsymmetricEncrypter : IEncrypter
    {
        public HealthStatus GetStatus()
        {
            throw new System.NotImplementedException();
        }

        public void RotateKeys()
        {
            throw new System.NotImplementedException();
        }

        public bool TryDecrypt(string textToDecrypt, out string decrypted)
        {
            throw new System.NotImplementedException();
        }

        public bool TryEncrypt(string textToEncrypt, out string encrypted)
        {
            throw new System.NotImplementedException();
        }
    }
}
