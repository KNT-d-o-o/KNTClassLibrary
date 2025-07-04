using KNTCommon.Business.Repositories;
using KNTCommon.Business.Scripts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KNTCommon.Business.Extension
{
    public static class StringExtension
    {
        public static T GetValue<T>(this string str)
        {
            var type = typeof(T);
            if (type.IsEnum)
                return (T)Enum.Parse(type, str, ignoreCase: true);

            return (T)Convert.ChangeType(str, type);
        }
    }
}
