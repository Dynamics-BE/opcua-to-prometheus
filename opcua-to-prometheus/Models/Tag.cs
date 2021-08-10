using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace opcua_to_prometheus.Models
{
    public class Tag
    {
        public int SubscriptionInterval { get; set; }
        public string MetricsName { get; set; }
        public string NodeID { get; set; }
        public object Value { get; set; }
    }
}
