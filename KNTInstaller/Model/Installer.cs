using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KNTInstaller.Model
{
    public class Installer
    {
        // Common
        public bool Common_IIS { get; set; }
        public bool Common_DotNetHosting { get; set; }
        public bool Common_Edge { get; set; }

        // SMM
        public bool SMM_IISCreateApp { get; set; }
        public bool SMM_OpenFirewallPort { get; set; }
        public bool SMM { get; set; }

        // services       
        public bool Services_IO { get; set; }
    }
}
