using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KNTCommon.BusinessIO.DTOs
{
    public class IoTaskDetailsDTO
    {
        public int IoTaskDetailId { get; set; }

        public int IoTaskId { get; set; }

        public string? Par1 { get; set; }

        public string? Par2 { get; set; }

        public string? Par3 { get; set; }

        public string? Par4 { get; set; }
        
        public string? Par5 { get; set; }

        public int TaskDetailOrder { get; set; }

        public string? Info { get; set; }

    }
}
