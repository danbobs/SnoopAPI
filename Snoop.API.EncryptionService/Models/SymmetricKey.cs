using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Snoop.API.EncryptionService.Models
{
    public class SymmetricKey : Key
    {
        public string InitializationVector { get; set; }
        public string Key { get; set; }
    }
}
