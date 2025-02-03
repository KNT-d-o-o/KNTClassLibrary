using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KNTCommon.BusinessIO.DTOs
{
    public class IoTasksDTO
    {
        public int IoTaskId { get; set; }

        public string? IoTaskName { get; set; }

        public int IoTaskType { get; set; }

        public int IoTaskMode { get; set; }

        public int Priority { get; set; }

        public DateTime? ExecuteDateAndTime { get; set; }

        public int Status { get; set; }

        public string? Info { get; set; }

    }
}
