using System;

namespace Snoop.Common.Models
{
    public class HealthStatus
    {
       public bool Available { get; set; }
       public string OldestKey { get; set; }
       public string NewestKey { get; set; }
    }
}
