using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KNTPlc.S7
{
    public class StationData
    {
        public int DbNumber { get; set; }
        public int Offset { get; set; }
        public int OffsetBit { get; set; }
        public string DataType { get; set; } = string.Empty; // "BOOL", "INT16", "INT32", "REAL", "STRING", "TIME", "DTL"
        public object? Value { get; set; }
    }
}
