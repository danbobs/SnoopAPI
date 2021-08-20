using System.Threading.Tasks;
using Snoop.Background.KeyRotation.Models;


namespace Snoop.Background.KeyRotation.Interfaces
{
    public interface IEncryptionServiceWrapper
    {
        Task<KeyRotationResult> InvokeRotateKeys();
    }
}
