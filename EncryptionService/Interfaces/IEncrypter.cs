using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
