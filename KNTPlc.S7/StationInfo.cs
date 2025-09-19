using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KNTPlc.S7
{
    public class StationInfo
    {
        public int StationID { get; set; }
        public int StationDB { get; set; }
        public int dbSize;
        public int BoolStartPos, BoolEndPos;
        public int BoolStartBit, BoolEndBit;
        public int IntStartPos, IntEndPos;
        public int DIntStartPos, DIntEndPos;
        public int RealStartPos, RealEndPos;
        public int StringStartPos, StringEndPos;
        public int TimeStartPos, TimeEndPos;
        public int DtlStartPos, DtlEndPos;
        public int offset;
        public StationType Type { get; set; }

        public StationMemory Memory { get; set; } = new StationMemory();
    }

    public enum StationType
    {
        None,
        AppManagement,
        StationMain,
        Recipe,
        Event
    }
}
