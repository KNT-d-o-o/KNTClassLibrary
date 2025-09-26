using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KNTPlc.S7.Models
{
    public class StationInfo
    {
        public int StationID { get; set; }
        public int StationDB { get; set; }
        public int AddStationDB { get; set; }
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

        public List<AddStationFieldInfo> Fields { get; set; } = new();
        public AddStationMemory AddStationMemory { get; set; } = new AddStationMemory();

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
