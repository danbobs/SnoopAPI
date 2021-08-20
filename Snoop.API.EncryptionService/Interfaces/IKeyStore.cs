using System.Collections.Generic;
using Snoop.API.EncryptionService.Models;
using Snoop.Common.Models;

namespace Snoop.API.EncryptionService.Services.Interfaces
{
    public interface IKeyStore<T> where T : Key
    {
        void StoreNewKey(T newKey);
        IEnumerable<T> GetKeys();
        T GetActiveKey();
        HealthStatus GetStatus();
    }
}
