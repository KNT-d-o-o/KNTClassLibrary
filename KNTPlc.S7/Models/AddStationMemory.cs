using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KNTPlc.S7.Models
{
    public class AddStationMemory
    {
        // polja (meta) pridejo iz baze
        public List<AddStationFieldInfo> Fields { get; private set; } = new();

        // dejanski podatki: Key = FieldName, Value = tipiziran array (short[], int[], float[], string[])
        public Dictionary<string, Array> Data { get; } = new();

        public void Initialize(IEnumerable<AddStationFieldInfo> fields)
        {
            Fields = fields.OrderBy(f => f.OrderIndex).ToList();
            Data.Clear();

            foreach (var f in Fields)
            {
                Array arr = f.DataType switch
                {
                    "Int16" => new short[f.Length],
                    "Int32" => new int[f.Length],
                    "Real" => new float[f.Length],
                    string s when s.StartsWith("String") => new string[f.Length],
                    _ => throw new NotSupportedException($"Unsupported DataType {f.DataType}")
                };

                Data[f.FieldName] = arr;
            }
        }

        public int CalculateSize()
        {
            if (Fields == null || Fields.Count == 0) return 0;
            return Fields.Sum(f => f.ByteSize * f.Length);
        }

        public void Parse(byte[] buffer)
        {
            if (Fields == null || Fields.Count == 0)
                throw new InvalidOperationException("Fields not initialized for AddStationMemory.");

            int offset = 0;
            foreach (var f in Fields)
            {
                if (!Data.TryGetValue(f.FieldName, out var arr))
                    throw new InvalidOperationException($"Field '{f.FieldName}' not initialized in AddStationMemory.Data");

                for (int i = 0; i < f.Length; i++)
                {
                    switch (f.DataType)
                    {
                        case "Int16":
                            {
                                if (offset + 2 > buffer.Length) throw new ArgumentException("Buffer too small");
                                byte[] tmp = new byte[2];
                                Array.Copy(buffer, offset, tmp, 0, 2);
                                if (BitConverter.IsLittleEndian) Array.Reverse(tmp);
                                ((short[])arr)[i] = BitConverter.ToInt16(tmp, 0);
                                offset += 2;
                            }
                            break;

                        case "Int32":
                            {
                                if (offset + 4 > buffer.Length) throw new ArgumentException("Buffer too small");
                                byte[] tmp = new byte[4];
                                Array.Copy(buffer, offset, tmp, 0, 4);
                                if (BitConverter.IsLittleEndian) Array.Reverse(tmp);
                                ((int[])arr)[i] = BitConverter.ToInt32(tmp, 0);
                                offset += 4;
                            }
                            break;

                        case "Real":
                            {
                                if (offset + 4 > buffer.Length) throw new ArgumentException("Buffer too small");
                                byte[] tmp = new byte[4];
                                Array.Copy(buffer, offset, tmp, 0, 4);
                                if (BitConverter.IsLittleEndian) Array.Reverse(tmp);
                                ((float[])arr)[i] = BitConverter.ToSingle(tmp, 0);
                                offset += 4;
                            }
                            break;

                        default:
                            if (f.DataType.StartsWith("String"))
                            {
                                int strMax = f.StringSize ?? 0;
                                // pričakujemo: [1 byte maxLen][1 byte currLen][strMax bytes content]
                                if (offset + 2 + strMax > buffer.Length) throw new ArgumentException("Buffer too small for string");
                                byte declaredMax = buffer[offset];     // običajno max len
                                byte currLen = buffer[offset + 1];    // dejanska dolžina
                                int readLen = Math.Min(currLen, strMax);
                                string s = readLen > 0 ? Encoding.ASCII.GetString(buffer, offset + 2, readLen) : string.Empty;
                                ((string[])arr)[i] = s;
                                offset += 2 + strMax;
                            }
                            else
                            {
                                throw new NotSupportedException($"Unsupported DataType {f.DataType}");
                            }
                            break;
                    }
                }
            }

            // opcijsko: preveri offset==buffer.Length ali offset <= buffer.Length
        }
    }
}
