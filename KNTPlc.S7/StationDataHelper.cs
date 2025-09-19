using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KNTPlc.S7
{
    public static class StationDataHelper
    {
        public static T? GetValue<T>(StationData data)
        {
            if (data == null || data.Value == null)
                return default;

            try
            {
                // BOOL je shranjen kot int 0/1, pretvori v bool, če je T bool
                if (typeof(T) == typeof(bool) && data.DataType.ToUpper() == "BOOL")
                {
                    bool b = ((int)data.Value) != 0;
                    return (T)(object)b;
                }

                // ostali tipi
                return (T)Convert.ChangeType(data.Value, typeof(T));
            }
            catch
            {
                return default;
            }
        }
    }
}
