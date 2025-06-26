using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KNTCommon.Data.Models
{
    public class Results
    {
        [Key]
        public int ResultId { get; set; }
        public string? ResultDescription { get; set; }
        public string? ResultDescriptionLang { get; set; }
        public string? ResultCustomer { get; set; }
        public string? ResultColour { get; set; }
        public byte Used { get; set; }
    }
}
