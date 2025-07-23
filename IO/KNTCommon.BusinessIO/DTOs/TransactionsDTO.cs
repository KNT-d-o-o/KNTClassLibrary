using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KNTCommon.BusinessIO.DTOs
{
    public class TransactionsDTO
    {
        public int TransactionId { get; set; }

        public string? TransactionName { get; set; }

        public DateTime? DateAndTime { get; set; }

        public string? DmcInternal { get; set; }

        public string? DmcCustomer { get; set; }

        public int? ResultId { get; set; }

        public int? resultIdDevelopment { get; set; }

        public double? ResultValue { get; set; }

        public int? PlcId { get; set; }

        public string? OrderId { get; set; }

        public string? EmployeeId { get; set; }

        public int? ItemId { get; set; }

        public string? qualityCode { get; set; }

        public string? Etalon { get; set; }

        public string? Msa { get; set; }

        public int? Counter { get; set; }

        public int? KntMmUpdated { get; set; }

        public int? ErpMesUpdated { get; set; }

        public int? Archived { get; set; }

    }
}
