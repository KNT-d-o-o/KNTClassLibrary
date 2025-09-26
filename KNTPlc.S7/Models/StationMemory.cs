using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KNTPlc.S7.Models
{
    public class StationMemory
    {
        public int StationDB { get; set; }
        public bool[] BoolArr { get; set; } = Array.Empty<bool>();
        public short[] IntArr { get; set; } = Array.Empty<short>();
        public int[] DIntArr { get; set; } = Array.Empty<int>();
        public float[] RealArr { get; set; } = Array.Empty<float>();
        public string[] StringArr { get; set; } = Array.Empty<string>();
        public TimeSpan[] TimeArr { get; set; } = Array.Empty<TimeSpan>();
        public DateTime[] DtlArr { get; set; } = Array.Empty<DateTime>();
    }
}
