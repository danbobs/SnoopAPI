using System;

namespace Snoop.Common.Model
{
    public class HealthStatus
    {
       public bool Available { get; set; }
       public string OldestKey { get; set; }
       public string NewestKey { get; set; }
    }
}
