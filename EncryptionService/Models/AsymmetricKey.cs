using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Snoop.API.EncryptionService.Models
{
    public class AsymmetricKey : Key
    {
        public string Private { get; set; }
        public string Public { get; set; }
    }
}
