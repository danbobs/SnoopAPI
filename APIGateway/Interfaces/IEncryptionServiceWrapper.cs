using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Snoop.API.APIGateway.Models;
using Snoop.Common.Model;

namespace Snoop.API.APIGateway.Interfaces
{
    public interface IEncryptionServiceWrapper
    {
        Task<EncryptDecryptResult> InvokeEncrypt(string textToEncrypt);
        Task<EncryptDecryptResult> InvokeDecrypt(string textToDecrypt);
        Task<HealthStatus> InvokeHealthCheck();

    }
}
