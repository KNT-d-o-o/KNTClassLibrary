using KNTPlc.S7.Models;
using Sharp7;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KNTPlc.S7
{
    public class S7Plc
    {
        private S7Controller? _client;
        public bool IsConnected => _client?.IsConnected ?? false;

      //  public StationMemory Memory { get; private set; } = new StationMemory();

        public const int STRLENGTH = 80;

        public bool Connect(string ip, int rack = 0, int slot = 1)
        {
            _client = new S7Controller();
            return _client.Connect(ip, rack, slot);
        }

        public void Disconnect() => _client?.Disconnect();

        // read all data in station
        public (bool, int) ReadEntireDb(StationInfo station)
        {
            if (!IsConnected || _client == null) 
                return (false, S7Consts.errTCPConnectionFailed);

            int dbSize = CalculateDbSize(station); // count bajtes for all types
            byte[] allBuffer = new byte[dbSize];

            var (ok, err) = _client.ReadDb(station.StationDB, station.offset, allBuffer);
            if (!ok)
                return (false, err);

            // split buffer to arrays
            ParseAllBuffer(allBuffer, station);
            return (true, err);
        }

        // read additional station data
        public (bool, int) ReadAddStationDb(StationInfo station)
        {
            if (!IsConnected || _client == null)
                return (false, S7Consts.errTCPConnectionFailed);

            if (station.Fields == null || station.Fields.Count == 0)
                return (false, /* custom err code */ S7Consts.errCliItemNotAvailable);

            // Inicializiraj AddStationMemory (ustvari arr[] za Vsako polje)
            station.AddStationMemory.Initialize(station.Fields);

            int dbSize = station.AddStationMemory.CalculateSize();
            if (dbSize <= 0) return (true, 0);

            byte[] allBuffer = new byte[dbSize];

            // klic k klientu (predpostavka: _client.ReadDb(...) vrne (bool ok, int err) ali podobno)
            var (ok, err) = _client.ReadDb(station.AddStationDB, 0, allBuffer);
            if (!ok)
                return (false, err);

            // parsaj v tipizirane array-e
            station.AddStationMemory.Parse(allBuffer);

            return (true, err);
        }

        // write all data in station
        public (bool, int) WriteEntireDb(StationInfo station)
        {
            if (!IsConnected || _client == null) return (false, S7Consts.errTCPConnectionFailed);

            int dbSize = CalculateDbSize(station);
            byte[] allBuffer = new byte[dbSize];

            // fill buffer from arrays
            BuildAllBuffer(allBuffer, station);

            var (ok, err) = _client.WriteDb(station.StationDB, station.offset, allBuffer);
            return (ok, err);
        }

        // calculate max size
        private int CalculateDbSize(StationInfo station)
        {
            // DB size = most large offset + size of type
            int maxOffset = 0;

            maxOffset = Math.Max(maxOffset, station.BoolEndPos + 1);
            maxOffset = Math.Max(maxOffset, station.IntEndPos + 2);   // int16 = 2 bytes
            maxOffset = Math.Max(maxOffset, station.DIntEndPos + 4);  // int32 = 4 bytes
            maxOffset = Math.Max(maxOffset, station.RealEndPos + 4);  // float = 4 bytes
            maxOffset = Math.Max(maxOffset, station.StringEndPos);    // depend of length of string[]
            maxOffset = Math.Max(maxOffset, station.TimeEndPos + 4);  // TimeSpan in ms = 4 bytes
            maxOffset = Math.Max(maxOffset, station.DtlEndPos + 12);   // DateTime = 12 bytes (ticks)
            return maxOffset;
        }

        // memory structure definition
        public void InitMemoryFromStation(StationInfo station)
        {
            if (station.BoolEndPos != -1 && station.BoolStartPos != -1 && station.BoolEndBit != -1 && station.BoolStartBit != -1)
            {
                int totalBits = ((station.BoolEndPos - station.BoolStartPos + 1) * 8) -
                station.BoolStartBit - (8 - station.BoolEndBit - 1);

                station.Memory.BoolArr = new bool[totalBits];
            }

            if (station.IntEndPos != -1 && station.IntStartPos != -1)
            {
                int intCount = (station.IntEndPos - station.IntStartPos) / 2 + 1;
                station.Memory.IntArr = new short[intCount];
            }

            if (station.DIntEndPos != -1 && station.DIntStartPos != -1)
            {
                int dIntCount = (station.DIntEndPos - station.DIntStartPos) / 4 + 1;
                station.Memory.DIntArr = new int[dIntCount];
            }
            if (station.RealEndPos != -1 && station.RealStartPos != -1)
            {
                int realCount = (station.RealEndPos - station.RealStartPos) / 4 + 1;
                station.Memory.RealArr = new float[realCount];
            }

            if (station.StringEndPos != -1 && station.StringStartPos != -1)
            {
                int blockSize = STRLENGTH + 2; // 2 bytes header + 80 STRLENGTH content
                int strCount = (station.StringEndPos - station.StringStartPos) / blockSize + 1;
                station.Memory.StringArr = new string[strCount];
            }

            if (station.TimeEndPos != -1 && station.TimeStartPos != -1)
            {
                int timeCount = (station.TimeEndPos - station.TimeStartPos) / 4 + 1;
                station.Memory.TimeArr = new TimeSpan[timeCount];
            }

            if (station.DtlEndPos != -1 && station.DtlStartPos != -1)
            {
                int dtlCount = (station.DtlEndPos - station.DtlStartPos) / 12 + 1;
                station.Memory.DtlArr = new DateTime[dtlCount];
            }
        }

        // dynamic memory for additional station
        public void InitAddStationMemory(StationInfo station, List<AddStationFieldInfo> fields)
        {
            station.Fields = fields;
            station.AddStationMemory.Data.Clear(); // počisti obstoječe podatke

            foreach (var field in fields.OrderBy(f => f.OrderIndex))
            {
                Array arr = field.DataType switch
                {
                    "Int16" => new short[field.Length],
                    "Int32" => new int[field.Length],
                    "Real" => new float[field.Length],
                    string s when s.StartsWith("String") => new char[field.Length, (field.StringSize ?? 0) + 2],
                    _ => throw new NotSupportedException($"Unsupported DataType {field.DataType}")
                };

                station.AddStationMemory.Data[field.FieldName] = arr;
            }

        }

        // parse all data from station
        private void ParseAllBuffer(byte[] buffer, StationInfo station)
        {
            InitMemoryFromStation(station);
            // -------------------
            // BOOL
            // -------------------
            if (station.Memory.BoolArr.Length > 0)
            {
                int bitIndex = 0;

                for (int pos = station.BoolStartPos; pos <= station.BoolEndPos; pos++)
                {
                    byte byt = buffer[pos];

                    // margin of bits for this byte
                    int startBit = (pos == station.BoolStartPos) ? station.BoolStartBit : 0;
                    int endBit = (pos == station.BoolEndPos) ? station.BoolEndBit : 7;

                    for (int bit = startBit; bit <= endBit; bit++)
                    {
                        station.Memory.BoolArr[bitIndex++] = (byt & (1 << bit)) != 0;
                    }
                }
            }

            // -------------------
            // INT16
            // -------------------
            if (station.Memory.IntArr.Length > 0)
            {
                for (int i = 0; i < (station.IntEndPos - station.IntStartPos) / 2 + 1; i++)
                {
                    byte[] tmp = new byte[2];
                    Array.Copy(buffer, station.IntStartPos + i * 2, tmp, 0, 2);
                    if (BitConverter.IsLittleEndian)
                        Array.Reverse(tmp);
                    station.Memory.IntArr[i] = BitConverter.ToInt16(tmp, 0);
                }
            }

            // -------------------
            // INT32
            // -------------------
            if (station.Memory.DIntArr.Length > 0)
            {
                for (int i = 0; i < (station.DIntEndPos - station.DIntStartPos) / 4 + 1; i++)
                {
                    byte[] tmp = new byte[4];
                    Array.Copy(buffer, station.DIntStartPos + i * 4, tmp, 0, 4);
                    if (BitConverter.IsLittleEndian)
                        Array.Reverse(tmp);
                    station.Memory.DIntArr[i] = BitConverter.ToInt32(tmp, 0);
                }
            }

            // -------------------
            // REAL
            // -------------------
            if (station.Memory.RealArr.Length > 0)
            {
                for (int i = 0; i < (station.RealEndPos - station.RealStartPos) / 4 + 1; i++)
                {
                    byte[] tmp = new byte[4];
                    Array.Copy(buffer, station.RealStartPos + i * 4, tmp, 0, 4);
                    if (BitConverter.IsLittleEndian)
                        Array.Reverse(tmp);
                    station.Memory.RealArr[i] = BitConverter.ToSingle(tmp, 0);
                }
            }

            // -------------------
            // STRING[]
            // -------------------
            // fixed length of string in DB STRLENGTH bytes
            if (station.Memory.StringArr.Length > 0)
            {
                int blockSize = STRLENGTH + 2; // 2 bytes header + STRLENGTH znakov vsebine
                {
                    for (int i = 0; i < (station.StringEndPos - station.StringStartPos) / blockSize + 1; i++)
                    {
                        int pos = station.StringStartPos + i * blockSize;
                        if (pos + 2 > buffer.Length) { station.Memory.StringArr[i] = string.Empty; continue; }

                        byte maxLen = buffer[pos];
                        byte curLen = buffer[pos + 1];
                        int len = Math.Min(curLen, blockSize - 2);

                        if (pos + 2 + len > buffer.Length)
                        {
                            // safety: if buffer is shorter
                            len = Math.Max(0, buffer.Length - (pos + 2));
                        }

                        string s = len > 0 ? Encoding.ASCII.GetString(buffer, pos + 2, len) : string.Empty;
                        s = s.TrimEnd('\0');        // remove null terminator
                        s = s.Trim();               // remove whitespace
                        s = s.Trim('"');            // remove "

                        station.Memory.StringArr[i] = s;
                    }
                }
            }

            // -------------------
            // TIME (ms) - 4 bytes
            // -------------------
            if (station.Memory.TimeArr.Length > 0)
            {
                for (int i = 0; i < (station.TimeEndPos - station.TimeStartPos) / 4 + 1; i++)
                {
                    byte[] tmp = new byte[4];
                    Array.Copy(buffer, station.TimeStartPos + i * 4, tmp, 0, 4);
                    if (BitConverter.IsLittleEndian)
                        Array.Reverse(tmp);
                    int ms = BitConverter.ToInt32(tmp, 0);
                    station.Memory.TimeArr[i] = TimeSpan.FromMilliseconds(ms);
                }
            }

            // -------------------
            // DTL (DateTime) - 8 bytes ticks
            // -------------------
            if (station.Memory.DtlArr.Length > 0)
            {
                for (int i = 0; i < (station.DtlEndPos - station.DtlStartPos) / 12 + 1; i++)
                {
                    station.Memory.DtlArr[i] = ParseS7DateTime(buffer, station.DtlStartPos + i * 12);
                }
            }
        }

        // parse DateTime from bytes
        private static DateTime ParseS7DateTime(byte[] buffer, int pos)
        {
            int year = (buffer[pos] << 8) | buffer[pos + 1];
            int month = buffer[pos + 2];
            int day = buffer[pos + 3];
            int hour = buffer[pos + 5];
            int minute = buffer[pos + 6];
            int second = buffer[pos + 7];
            int millis = (buffer[pos + 8] << 8) | buffer[pos + 9];

            return new DateTime(year, month, day, hour, minute, second).AddMilliseconds(millis);
        }

        // write all buffer to station
        private void BuildAllBuffer(byte[] buffer, StationInfo station)
        {
            // BOOL
            for (int i = 0; i < station.Memory.BoolArr.Length; i++)
            {
                int bitIndexInMemory = i;
                bool value = station.Memory.BoolArr[bitIndexInMemory];

                int bytePos = station.BoolStartPos + (i + station.BoolStartBit) / 8;
                int bitPos = (i + station.BoolStartBit) % 8;

                if (value)
                    buffer[bytePos] |= (byte)(1 << bitPos);
                else
                    buffer[bytePos] &= (byte)~(1 << bitPos);
            }

            // INT16
            for (int i = 0; i < station.Memory.IntArr.Length; i++)
            {
                byte[] tmp = BitConverter.GetBytes(station.Memory.IntArr[i]);
                if (BitConverter.IsLittleEndian)
                    Array.Reverse(tmp);
                Array.Copy(tmp, 0, buffer, station.IntStartPos + i * 2, 2);
            }

            // INT32
            for (int i = 0; i < station.Memory.DIntArr.Length; i++)
            {
                byte[] tmp = BitConverter.GetBytes(station.Memory.DIntArr[i]);
                if (BitConverter.IsLittleEndian)
                    Array.Reverse(tmp);
                Array.Copy(tmp, 0, buffer, station.DIntStartPos + i * 4, 4);
            }

            // REAL
            for (int i = 0; i < station.Memory.RealArr.Length; i++)
            {
                byte[] tmp = BitConverter.GetBytes(station.Memory.RealArr[i]);
                if (BitConverter.IsLittleEndian)
                    Array.Reverse(tmp);
                Array.Copy(tmp, 0, buffer, station.RealStartPos + i * 4, 4);
            }

            // STRING[]
            int blockSize = STRLENGTH + 2;

            for (int i = 0; i < station.Memory.StringArr.Length; i++)
            {
                int pos = station.StringStartPos + i * blockSize;
                if (pos + blockSize > buffer.Length)
                    break; // ne presežemo bufferja

                string s = station.Memory.StringArr[i] ?? string.Empty;
                // remove "
                s = s.Trim('"');

                byte[] strBytes = Encoding.ASCII.GetBytes(s);
                int curLen = Math.Min(strBytes.Length, STRLENGTH);

                // header
                buffer[pos] = (byte)STRLENGTH;  // max length
                buffer[pos + 1] = (byte)curLen; // current length

                // clear data area (null fill)
                Array.Clear(buffer, pos + 2, STRLENGTH);

                // copy actual bytes
                if (curLen > 0)
                    Array.Copy(strBytes, 0, buffer, pos + 2, curLen);
            }

            // TIME
            for (int i = 0; i < station.Memory.TimeArr.Length; i++)
            {
                int ms = (int)station.Memory.TimeArr[i].TotalMilliseconds;
                byte[] tmp = BitConverter.GetBytes(ms);
                if (BitConverter.IsLittleEndian)
                    Array.Reverse(tmp);
                Array.Copy(tmp, 0, buffer, station.TimeStartPos + i * 4, 4);
            }

            // DTL
            for (int i = 0; i < station.Memory.DtlArr.Length; i++)
            {
                DateTime dt = station.Memory.DtlArr[i];
                byte[] tmp = new byte[12];

                // year (2 bytes, big endian)
                ushort year = (ushort)dt.Year;
                tmp[0] = (byte)(year >> 8);
                tmp[1] = (byte)(year & 0xFF);

                tmp[2] = (byte)dt.Month;
                tmp[3] = (byte)dt.Day;
                tmp[4] = (byte)dt.DayOfWeek; // 0=Sunday .. 6=Saturday
                tmp[5] = (byte)dt.Hour;
                tmp[6] = (byte)dt.Minute;
                tmp[7] = (byte)dt.Second;

                // nanoseconds
                int ns = dt.Millisecond * 1_000_000; // pretvori ms -> ns
                tmp[8] = (byte)((ns >> 24) & 0xFF);
                tmp[9] = (byte)((ns >> 16) & 0xFF);
                tmp[10] = (byte)((ns >> 8) & 0xFF);
                tmp[11] = (byte)(ns & 0xFF);

                Array.Copy(tmp, 0, buffer, station.DtlStartPos + i * 12, 12);
            }
        }

        // Bool

        public (StationData?, int) ReadStationDataBool(int dbNumber, int byteIndex, int bitIndex)
        {
            if (!IsConnected || _client == null) return (null, S7Consts.errTCPConnectionFailed);

            if (bitIndex < 0 || bitIndex > 7)
                throw new ArgumentOutOfRangeException(nameof(bitIndex), "Bit index must be 0–7.");

            byte[] buffer = new byte[1]; // read 1 byte
            var (ok, err) = _client.ReadDb(dbNumber, byteIndex, buffer);
            if (!ok)
                return (null, err);

            bool value = (buffer[0] & (1 << bitIndex)) != 0;

            return (new StationData { DbNumber = dbNumber, Offset = byteIndex, OffsetBit = bitIndex, DataType = "BOOL", Value = value ? 1 : 0 }, err);
        }

        public (bool, int) WriteStationDataBool(int dbNumber, int byteIndex, int bitIndex, bool value)
        {
            if (!IsConnected || _client == null) return (false, S7Consts.errTCPConnectionFailed);

            if (bitIndex < 0 || bitIndex > 7)
                throw new ArgumentOutOfRangeException(nameof(bitIndex), "Bit index must be 0–7.");

            // read actual byte (not to replace other bits)
            byte[] buffer = new byte[1];
            var (ok, err) = _client.ReadDb(dbNumber, byteIndex, buffer);
            if (!ok)
                return (false, err);

            if (value)
                buffer[0] |= (byte)(1 << bitIndex);  // set bit
            else
                buffer[0] &= (byte)~(1 << bitIndex); // clear bit

            (ok, err) = _client.WriteDb(dbNumber, byteIndex, buffer);
            return (ok, err);
        }

        // Int

        public (StationData?, int) ReadStationDataInt16(int dbNumber, int startByte)
        {
            if (!IsConnected || _client == null) return (null, S7Consts.errTCPConnectionFailed);

            byte[] buffer = new byte[2]; // 16-bit int = 2 bytes
            var (ok, err) = _client.ReadDb(dbNumber, startByte, buffer);
            if (!ok)
                return (null, err);

            // PLC is big-endian, C# little-endian
            if (BitConverter.IsLittleEndian)
                Array.Reverse(buffer);

            short value = BitConverter.ToInt16(buffer, 0);

            return (new StationData { DbNumber = dbNumber, Offset = startByte, DataType = "INT16", Value = value }, err);
        }

        public (bool, int) WriteStationDataInt16(int dbNumber, int startByte, short value)
        {
            if (!IsConnected || _client == null) return (false, S7Consts.errTCPConnectionFailed);

            byte[] buffer = BitConverter.GetBytes(value);

            // shift bytes, because PLC is big-endian
            if (BitConverter.IsLittleEndian)
                Array.Reverse(buffer);

            var (ok, err) = _client.WriteDb(dbNumber, startByte, buffer);
            return (ok, err);
        }

        public (bool, int) WriteStationDataInt16Array(int dbNumber, int startByte, short[] values)
        {
            if (!IsConnected || _client == null)
                return (false, S7Consts.errTCPConnectionFailed);

            // rezerviramo buffer za vse short vrednosti
            byte[] buffer = new byte[values.Length * 2];

            for (int i = 0; i < values.Length; i++)
            {
                byte[] tmp = BitConverter.GetBytes(values[i]);

                // Siemens PLC je big-endian → po potrebi obrnemo
                if (BitConverter.IsLittleEndian)
                    Array.Reverse(tmp);

                Array.Copy(tmp, 0, buffer, i * 2, 2);
            }

            // en sam zapis naenkrat
            var (ok, err) = _client.WriteDb(dbNumber, startByte, buffer);
            return (ok, err);
        }

        // Dint

        public (StationData?, int) ReadStationDataInt32(int dbNumber, int startByte)
        {
            if (!IsConnected || _client == null) return (null, S7Consts.errTCPConnectionFailed);

            byte[] buffer = new byte[4];
            var (ok, err) = _client.ReadDb(dbNumber, startByte, buffer);
            if (!ok)
                return (null, err);

            // PLC is big-endian, C# little-endian
            if (BitConverter.IsLittleEndian)
                Array.Reverse(buffer);

            int value = BitConverter.ToInt32(buffer, 0);
            return (new StationData { DbNumber = dbNumber, Offset = startByte, DataType = "INT32", Value = value }, err);
        }

        public (bool, int) WriteStationDataInt32(int dbNumber, int startByte, int value)
        {
            if (!IsConnected || _client == null) return (false, S7Consts.errTCPConnectionFailed);

            byte[] buffer = BitConverter.GetBytes(value);

            // shift bytes, because PLC is big-endian
            if (BitConverter.IsLittleEndian)
                Array.Reverse(buffer);

            var (ok, err) = _client.WriteDb(dbNumber, startByte, buffer);
            return (ok, err);
        }

        // Real

        public (StationData?, int) ReadStationDataReal(int dbNumber, int startByte)
        {
            if (!IsConnected || _client == null) return (null, S7Consts.errTCPConnectionFailed);

            byte[] buffer = new byte[4];
            var (ok, err) = _client.ReadDb(dbNumber, startByte, buffer);
            if (!ok)
                return (null, err);

            if (BitConverter.IsLittleEndian)
                Array.Reverse(buffer);

            float value = BitConverter.ToSingle(buffer, 0);
            return (new StationData { DbNumber = dbNumber, Offset = startByte, DataType = "REAL", Value = value }, err);
        }

        public (bool, int) WriteStationDataReal(int dbNumber, int startByte, float value)
        {
            if (!IsConnected || _client == null) return (false, S7Consts.errTCPConnectionFailed);

            byte[] buffer = BitConverter.GetBytes(value);
            if (BitConverter.IsLittleEndian)
                Array.Reverse(buffer);

            var (ok, err) = _client.WriteDb(dbNumber, startByte, buffer);
            return (ok, err);
        }

        // String[]

        public (StationData?, int) ReadStationDataString(int dbNumber, int startByte, int maxLength)
        {
            if (!IsConnected || _client == null) return (null, S7Consts.errTCPConnectionFailed);

            byte[] buffer = new byte[maxLength + 2]; // S7 string: [0]=maxlen, [1]=curlen, then chars
            var (ok, err) = _client.ReadDb(dbNumber, startByte, buffer);
            if (!ok)
                return (null, err);

            int currentLen = buffer[1];
            string value = Encoding.ASCII.GetString(buffer, 2, currentLen);

            return (new StationData { DbNumber = dbNumber, Offset = startByte, DataType = "STRING", Value = value }, err);
        }

        public (bool, int) WriteStationDataString(int dbNumber, int startByte, string value, int maxLength)
        {
            if (!IsConnected || _client == null) return (false, S7Consts.errTCPConnectionFailed);
            if (value.Length > maxLength) value = value.Substring(0, maxLength);

            byte[] buffer = new byte[maxLength + 2];
            buffer[0] = (byte)maxLength;     // max len
            buffer[1] = (byte)value.Length;  // current len
            Encoding.ASCII.GetBytes(value, 0, value.Length, buffer, 2);

            var (ok, err) = _client.WriteDb(dbNumber, startByte, buffer);
            return (ok, err);
        }

        // Time

        public (StationData?, int) ReadStationDataTime(int dbNumber, int startByte)
        {
            if (!IsConnected || _client == null) return (null, S7Consts.errTCPConnectionFailed);

            byte[] buffer = new byte[4];
            var (ok, err) = _client.ReadDb(dbNumber, startByte, buffer);
            if (!ok)
                return (null, err);

            if (BitConverter.IsLittleEndian)
                Array.Reverse(buffer);

            int ms = BitConverter.ToInt32(buffer, 0);
            TimeSpan value = TimeSpan.FromMilliseconds(ms);

            return (new StationData { DbNumber = dbNumber, Offset = startByte, DataType = "TIME", Value = value }, err);
        }

        public (bool, int) WriteStationDataTime(int dbNumber, int startByte, TimeSpan value)
        {
            if (!IsConnected || _client == null) return (false, S7Consts.errTCPConnectionFailed);

            int ms = (int)value.TotalMilliseconds;
            byte[] buffer = BitConverter.GetBytes(ms);
            if (BitConverter.IsLittleEndian)
                Array.Reverse(buffer);

            var (ok, err) = _client.WriteDb(dbNumber, startByte, buffer);
            return (ok, err);
        }

        // DTL

        public (StationData?, int) ReadStationDataDTL(int dbNumber, int startByte)
        {
            if (!IsConnected || _client == null) return (null, S7Consts.errTCPConnectionFailed);

            byte[] buffer = new byte[12];
            var (ok, err) = _client.ReadDb(dbNumber, startByte, buffer);
            if (!ok)
                return (null, err);

            DateTime dtl = ParseS7DateTime(buffer, 0);
            return (new StationData { DbNumber = dbNumber, Offset = startByte, DataType = "DTL", Value = dtl }, err);
        }

        public (bool, int) WriteStationDataDTL(int dbNumber, int startByte, DateTime value)
        {
            if (!IsConnected || _client == null) return (false, S7Consts.errTCPConnectionFailed);

            byte[] buffer = new byte[12];
            buffer[0] = (byte)(value.Year >> 8);
            buffer[1] = (byte)(value.Year & 0xFF);
            buffer[2] = (byte)value.Month;
            buffer[3] = (byte)value.Day;
            buffer[5] = (byte)value.Hour;
            buffer[6] = (byte)value.Minute;
            buffer[7] = (byte)value.Second;

            int millis = value.Millisecond;
            buffer[8] = (byte)(millis >> 8);
            buffer[9] = (byte)(millis & 0xFF);

            var (ok, err) = _client.WriteDb(dbNumber, startByte, buffer);
            return (ok, err);
        }

    }
}
