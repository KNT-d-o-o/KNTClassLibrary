using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KNTCommon.Business.DTOs
{
    public class ProductionOrderDTO
    {
        public int? OrderId { get; set; }

        public string? OrderOper { get; set; }

        public string? EmployeeId { get; set; }

        public double? NumGood { get; set; }

        public double? NumImpregnation { get; set; }

        public double? NumScrap { get; set; }

        public string? MachineId { get; set; }

        public double? SpentTime { get; set; }

        public DateTime? OrderDate { get; set; }
    }
}
