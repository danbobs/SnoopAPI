using System.Threading.Tasks;
using Snoop.API.APIGateway.Models;
using Snoop.Common.Models;

namespace Snoop.API.APIGateway.Interfaces
{
    public interface IEncryptionServiceWrapper
    {
        Task<EncryptDecryptResult> InvokeEncrypt(string textToEncrypt);
        Task<EncryptDecryptResult> InvokeDecrypt(string textToDecrypt);
        Task<HealthStatus> InvokeHealthCheck();
    }
}
