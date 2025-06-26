using AutoMapper.Configuration.Annotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KNTCommon.Business.DTOs
{
    public class ResultsDTO
    {
        public int? ResultId { get; set; }
        public string? ResultDescription { get; set; }
        public string? ResultDescriptionLang { get; set; }
        public string? ResultCustomer { get; set; }
        public string? ResultColour { get; set; }
        public byte Used { get; set; }

        [Ignore]
        public string ResultTranslatedDescription { get; set; }
    }
}
