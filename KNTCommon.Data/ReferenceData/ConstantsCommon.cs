using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KNTCommon.Data.ReferenceData
{
    public class ConstantsCommon
    {
        public enum DialogType
        {
            None,
            Add,
            Edit,
            Copy,
            Delete,
        }
        public static string Decimal2Format { get; } = "{0:N2}";

        public static string DateAndTimeFormat { get; } = "{0:dd.MM.yy HH:mm}";

        public static string DatePickerFormat { get; } = "dd.MM.yyyy";

        public static string[] NumericDropdownConditions { get; } = { "=", "<", ">", "<=", ">=" };

        public static string[] NumericDecimalDropdownConditions { get; } = { "=.", "=", "<", ">", "<=", ">=" };



    }
}
