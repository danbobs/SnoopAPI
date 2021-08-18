using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Snoop.Common.Model;

namespace Snoop.API.EncryptionService.Services.Interfaces
{
    public interface IEncrypter
    {
        bool TryEncrypt(string stringToEncrypt, out string encrypted);
        bool TryDecrypt(string stringToDecrypt, out string decrypted);
        void RotateKeys();
        HealthStatus GetStatus();   
    }
}
