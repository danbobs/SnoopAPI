using Snoop.Common.Models;

namespace Snoop.API.EncryptionService.Services.Interfaces
{
    public interface IEncrypter
    {
        bool TryEncrypt(string textToEncrypt, out string encrypted);
        bool TryDecrypt(string textToDecrypt, out string decrypted);
        void RotateKeys();
        HealthStatus GetStatus();   
    }
}
