using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Snoop.API.EncryptionService.Models;

namespace Snoop.API.EncryptionService.Services.Interfaces
{
    public interface IKeyStore<T> where T : Key
    {
        void StoreNewKey(T newKey);
        IEnumerable<T> GetKeys();

        T GetActiveKey();
    }
}
