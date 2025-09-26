using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KNTPlc.S7.Models
{
    public class AddStationFieldInfo
    {
        public int StationId { get; set; }
        public string FieldName { get; set; } = string.Empty;
        public string DataType { get; set; } = string.Empty; // npr. "Int16", "Real", "String[80]"
        public int OrderIndex { get; set; }
        public int Length { get; set; } // koliko elementov (array length)
        public int? StringSize { get; set; }

        public int ByteSize =>
        DataType switch
        {
            "Int16" => 2,
            "Int32" => 4,
            "Real" => 4,
            string s when s.StartsWith("String") => (StringSize ?? 0) + 2,
            _ => throw new NotSupportedException($"Unsupported DataType {DataType}")
        };
    }
}
