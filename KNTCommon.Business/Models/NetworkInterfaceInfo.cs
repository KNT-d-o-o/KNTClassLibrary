using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KNTCommon.Business.Models
{
    public class NetworkInterfaceInfo
    {
        public required string Name { get; set; }
        public required string IpAddress { get; set; }
        public required string SubnetMask { get; set; }
    }
}
