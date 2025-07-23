using KNTCommon.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KNTCommon.Business.DTOs
{
    public class ServiceControlDTO
    {
        public int ServiceId { get; set; }

        public required string ServiceName { get; set; }

        public string? ServiceTitle { get; set; }

        public int? Status { get; set; }

        public string? path { get; set; }

    }
}