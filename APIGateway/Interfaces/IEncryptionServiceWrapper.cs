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
        Task<EncryptDecryptResult> InvokeEncrypt(string stringToEncrypt);
        Task<EncryptDecryptResult> InvokeDecrypt(string stringToDecrypt);
        Task<HealthStatus> InvokeHealthCheck();

    }
}
