using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace opcua_to_prometheus.Models
{
    public class Configuration
    {
        public int SubscriptionInterval { get; set; }
        public string OPCUAEndpoint { get; set; }
        public List<Tag> Tags { get; set; }
    }
}
