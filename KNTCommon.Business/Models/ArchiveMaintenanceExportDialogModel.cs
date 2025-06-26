using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KNTCommon.Business.Models
{
    public class ArchiveMaintenanceExportDialogModel
    {
        public bool SaveAsZip { get; set; } = false;
        public bool DumpAllTables { get; set; }
        public bool Advanced { get; set; }
        public FilterTypeEnum? FilterType { get; set; } = FilterTypeEnum.Date;
        public DateTime DateFrom { get; set; } = DateTime.Now;
        public DateTime DateTo { get; set; } = DateTime.Now;
        public int? TransactionIdFrom { get; set; }
        public int? TransactionIdTo { get; set; }
    }

    public enum FilterTypeEnum
    {
        Date,
        TransactionId
    }
}
