using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Management;
using System.Runtime.InteropServices;
using System.Drawing;
using UnitsNet;
using System.Linq;
using System.Net;

namespace KNT_ToolsAndAccessories
{
    public class Accessories
    {
        #region regedit

       /* public void ChangeRegeditDefaultScreen(string command)
        {
            RegistryKey localMachine = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64);
            RegistryKey regKey = localMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion\Winlogon", true);
            regKey.SetValue("Shell", command, RegistryValueKind.String);
            regKey.Close();
        } */

        public void StartRemoteDesktop(string computerName)
        {
            Process rdcProcess = new Process();

            string executable = Environment.ExpandEnvironmentVariables(@"%SystemRoot%\system32\mstsc.exe");
            if (executable != null)
            {
                rdcProcess.StartInfo.FileName = executable;
                rdcProcess.StartInfo.Arguments = "/v " + computerName;  // ip or name of computer to connect
                rdcProcess.Start();
            }
        }

        #endregion

        #region Date and Time

        public struct SystemTime
        {
            public ushort Year;
            public ushort Month;
            public ushort DayOfWeek;
            public ushort Day;
            public ushort Hour;
            public ushort Minute;
            public ushort Second;
            public ushort Millisecond;
        };

        [DllImport("kernel32.dll", EntryPoint = "GetSystemTime", SetLastError = true)]
        public extern static void Win32GetSystemTime(ref SystemTime sysTime);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        internal static extern bool SetLocalTime(ref SystemTime lpSystemTime);

        public void SetDateAndTime(ushort year, ushort month, ushort day, ushort hour, ushort min, ushort sec)
        {
            // Set system date and time
            SystemTime updatedTime = new SystemTime();
            updatedTime.Year = year;
            updatedTime.Month = month;
            updatedTime.Day = day;
            updatedTime.Hour = hour;
            updatedTime.Minute = min;
            updatedTime.Second = sec;
            // Call the unmanaged function that sets the new date and time instantly
            SetLocalTime(ref updatedTime);
        }

        public (int year, int week) GetWeekOfTheYear(DateTime dateAndTime, bool firstFourDayWeek)
        {
            // https://www.c-sharpcorner.com/code/1226/get-week-number-of-a-year-in-c-sharp.aspx
            DateTime inputDate = dateAndTime;
            var d = inputDate;
            CultureInfo cul = CultureInfo.CurrentCulture;
            //string culString = CultureInfo.CurrentCulture.ToString();

            int firstDayWeek;

            if (firstFourDayWeek)
                firstDayWeek = cul.Calendar.GetWeekOfYear(d, CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday);
            else
                firstDayWeek = cul.Calendar.GetWeekOfYear(d, CalendarWeekRule.FirstDay, DayOfWeek.Monday);

            int year;

            if (firstFourDayWeek && firstDayWeek == 52 && d.Month == 1)  // int year = firstDayWeek == 52 && d.Month == 1 ? d.Year - 1 : d.Year;
                year = d.Year - 1;
            else if (firstFourDayWeek && firstDayWeek == 53 && d.Month == 1)
                year = d.Year - 1;
            else
                year = d.Year;

            return (year, firstDayWeek);
        }

        #endregion

        #region DTL (DTL#1970-01-01-00:00:00)

        public struct DTL
        {
            public ushort Year;
            public byte Month;
            public byte Day;
            public byte Weekday;         
            public byte Hour;
            public byte Minute;
            public byte Second;
            public uint Nanosecond;

            public string ToStringDtl()
            {
                return Year.ToString("D4") + "-" + Month.ToString("D2") + "-" + Day.ToString("D2") + "-" + Hour.ToString("D2") + ":" + Minute.ToString("D2") + ":" + Second.ToString("D2");
            }

            public void ToDtl(string strVal) 
            {
                try
                {
                    Year = Convert.ToUInt16(strVal.Substring(0, 4));
                    Month = Convert.ToByte(strVal.Substring(5, 2));
                    Day = Convert.ToByte(strVal.Substring(8, 2));
                    Hour = Convert.ToByte(strVal.Substring(11, 2));
                    Minute = Convert.ToByte(strVal.Substring(14, 2));
                    Second = Convert.ToByte(strVal.Substring(17, 2));
                }
                catch { }
            }

        };    

        #endregion

        #region Restart, shutdown

     /*   public void Shutdown(string cmd)
        {
            string flagToDo;

            if (cmd.ToLower() == "reboot" || cmd.ToLower() == "restart")
                flagToDo = "2";
            else if (cmd.ToLower() == "shutdown")
                flagToDo = "1";
            else
                return;

            ManagementBaseObject mboShutdown = null;
            ManagementClass mcWin32 = new ManagementClass("Win32_OperatingSystem");
            mcWin32.Get();

            // You can't shutdown without security privileges
            mcWin32.Scope.Options.EnablePrivileges = true;
            ManagementBaseObject mboShutdownParams = mcWin32.GetMethodParameters("Win32Shutdown");

            // Flag 1 means we want to shut down the system. Use "2" to reboot.
            mboShutdownParams["Flags"] = flagToDo;
            mboShutdownParams["Reserved"] = "0";
            foreach (ManagementObject manObj in mcWin32.GetInstances())
                mboShutdown = manObj.InvokeMethod("Win32Shutdown", mboShutdownParams, null);
        } */

        #endregion

        #region User Management

        public bool KntLogin(string user, string password)
        {
            if (user != "KNT")
                return false;

            // edolenc secret password to login
            if (user == "KNT" && password == "197083")
                return true;

            DateTime today = DateTime.Now;
            int pwInt = today.Day + today.Month + today.Year;
            string composedPassword = $"7{pwInt}9";

            if (composedPassword == password)
                return true;
            else
                return false;
        }

        #endregion

        #region Units of Measurements

   /*     public List<UnitsOfPropertiesModel> unitsOfProperties;

        private string UnitsFindUnitForProperty(string property)
        {
            foreach (UnitsOfPropertiesModel p in unitsOfProperties)
            {
                if (p.PropertyName == property)
                    return p.UnitName;
            }

            return "Error";
        }

        public (double outputValue, string unit) UnitsSensor1(double inputValue)
        {
            string unit = UnitsFindUnitForProperty("Sensor1");

            Pressure value;
            switch (unit)
            {
                case "bar": return (inputValue, unit);
                case "mbar":
                    value = Pressure.FromBars(inputValue);
                    return (Math.Round(value.Millibars, 1), "mbar");
                case "psi":
                    value = Pressure.FromBars(inputValue);
                    return (Math.Round(value.PoundsForcePerSquareInch, 2), "psi");
                case "kPa":
                    value = Pressure.FromBars(inputValue);
                    return (Math.Round(value.Kilopascals, 1), "kPa");
                default:
                    return (inputValue, unit);
            }
        }

        public (double outputValue, string unit) UnitsSensor1Set(double inputValue)
        {
            string unit = UnitsFindUnitForProperty("Sensor1");

            Pressure value = Pressure.FromBars(inputValue);
            switch (unit)
            {
                case "bar": break;
                case "mbar":
                    value = Pressure.FromMillibars(inputValue);
                    break;
                case "psi":
                    value = Pressure.FromPoundsForcePerSquareInch(inputValue);
                    break;
                case "kPa":
                    value = Pressure.FromKilopascals(inputValue);
                    break;
                default:
                    break;
            }

            return (Math.Round(Convert.ToDouble(value.Bars), 2), "bar");
        }

        public (double outputValue, string unit) UnitsSensor2(double inputValue)
        {
            string unit = UnitsFindUnitForProperty("Sensor2");

            Pressure value;
            switch (unit)
            {
                case "Pa": return (inputValue, unit);
                case "mbar":
                    value = Pressure.FromBars(inputValue);
                    return (Math.Round(value.Millibars, 1), "mbar");
                case "psi":
                    value = Pressure.FromBars(inputValue);
                    return (Math.Round(value.PoundsForcePerSquareInch, 2), "psi");
                case "kPa":
                    value = Pressure.FromBars(inputValue);
                    return (Math.Round(value.Kilopascals, 2), "kPa");
                default:
                    return (inputValue, unit);
            }
        }

        public (double outputValue, string unit) UnitsSensor2Set(double inputValue)
        {
            string unit = UnitsFindUnitForProperty("Sensor2");

            Pressure value = Pressure.FromPascals(inputValue);
            switch (unit)
            {
                case "bar": break;
                case "mbar":
                    value = Pressure.FromMillibars(inputValue);
                    break;
                case "psi":
                    value = Pressure.FromPoundsForcePerSquareInch(inputValue);
                    break;
                case "kPa":
                    value = Pressure.FromKilopascals(inputValue);
                    break;
                default:
                    break;
            }

            return (Math.Round(Convert.ToDouble(value.Pascals), 2), "bar");
        }

        public (double outputValue, string unit) UnitsOutput(double inputValue)
        {
            string unit = UnitsFindUnitForProperty("Output");

            Pressure value;
            switch (unit)
            {
                case "bar": return (inputValue, unit);
                case "mbar":
                    value = Pressure.FromBars(inputValue);
                    return (Math.Round(value.Millibars, 1), "mbar");
                case "psi":
                    value = Pressure.FromBars(inputValue);
                    return (Math.Round(value.PoundsForcePerSquareInch, 2), "psi");
                case "kPa":
                    value = Pressure.FromBars(inputValue);
                    return (Math.Round(value.Kilopascals, 1), "kPa");
                default:
                    return (inputValue, unit);
            }
        }

        public (double outputValue, string unit) UnitsOutputSet(double inputValue)
        {
            string unit = UnitsFindUnitForProperty("Output");

            Pressure value = Pressure.FromBars(inputValue);
            switch (unit)
            {
                case "bar": break;
                case "mbar":
                    value = Pressure.FromMillibars(inputValue);
                    break;
                case "psi":
                    value = Pressure.FromPoundsForcePerSquareInch(inputValue);
                    break;
                case "kPa":
                    value = Pressure.FromKilopascals(inputValue);
                    break;
                default:
                    break;
            }

            return (Math.Round(Convert.ToDouble(value.Bars), 2), "bar");
        }

        public (double outputValue, string unit) UnitsLeak(double inputValue)
        {
            string unit = UnitsFindUnitForProperty("Leak");

            VolumeFlow value;
            switch (unit)
            {
                case "ml/min": return (inputValue, unit);
                case "ccm/min":
                    value = VolumeFlow.FromMillilitersPerMinute(inputValue);
                    return (Math.Round(value.CubicCentimetersPerMinute, 1), "ccm/min");
                default:
                    return (inputValue, unit);
            }
        }

        public (double outputValue, string unit) UnitsLeakSet(double inputValue)
        {
            string unit = UnitsFindUnitForProperty("Leak");

            VolumeFlow value = VolumeFlow.FromCubicCentimetersPerMinute(inputValue);
            switch (unit)
            {
                case "ml/min": break;
                case "ccm/min":
                    value = VolumeFlow.FromMillilitersPerMinute(inputValue);
                    break;
                default:
                    break;
            }

            return (Math.Round(Convert.ToDouble(value.MillilitersPerMinute), 2), "bar");
        }

        public (double outputValue, string unit) UnitsTime(double inputValue)
        {
            string unit = UnitsFindUnitForProperty("Time");

            double value;
            switch (unit)
            {
                case "sec": return (Math.Round(inputValue, 2), unit);
                case "ms":
                    value = Math.Round(inputValue * 1000);
                    return (value, "ms");
                case "min":
                    value = Math.Round(inputValue / 60, 2);
                    return (value, "ms");
                case "hour":
                    value = Math.Round(inputValue / 60 / 60, 2);
                    return (value, "hour");
                default:
                    return (Math.Round(inputValue, 2), unit);
            }
        }
        public (double outputValue, string unit) UnitsTimeSet(double inputValue)
        {
            string unit = UnitsFindUnitForProperty("Time");

            double value = inputValue;
            switch (unit)
            {
                case "sec": break;
                case "ms":
                    value = inputValue / 1000;
                    break;
                case "min":
                    value = inputValue * 60;
                    break;
                case "hour":
                    value = inputValue * 60 * 60;
                    break;
                default:
                    break;
            }

            return (Math.Round(value, 2), "sec");
        }

        public (double outputValue, string unit) UnitsVolume(double inputValue)
        {
            string unit = UnitsFindUnitForProperty("Volume");

            Volume value;
            switch (unit)
            {
                case "ml": return (inputValue, unit);
                case "l":
                    value = Volume.FromMilliliters(inputValue);
                    return (value.Liters, "l");
                case "ccm":
                    value = Volume.FromCubicCentimeters(inputValue);
                    return (value.Liters, "ccm");
                default:
                    return (inputValue, unit);
            }
        }

        public (double outputValue, string unit) UnitsVolumeSet(double inputValue)
        {
            string unit = UnitsFindUnitForProperty("Volume");

            Volume value = Volume.FromMilliliters(inputValue);
            switch (unit)
            {
                case "ml": break;
                case "l":
                    value = Volume.FromLiters(inputValue);
                    break;
                case "ccm":
                    value = Volume.FromCubicCentimeters(inputValue);
                    break;
                default:
                    break;
            }

            return (Math.Round(Convert.ToDouble(value.Milliliters), 2), "ml");
        }*/

        public static string FormatTo3DecimalValues(int format, double val)
        {
            if (format == 0) // 15 decimal
                return string.Format("{0,1:0.###############}", val);
            string strVal = "0";
            if (Math.Abs(val) < 0.001 && val != 0)
                strVal = string.Format("{0,1:0.###E+00}", val); // Exp, 3 decimal
            else if (Math.Abs(val) < 0.01 && val != 0)
                strVal = string.Format("{0,1:0.#####}", val); // 5 decimal
            else if (Math.Abs(val) < 0.1 && val != 0)
                strVal = string.Format("{0,1:0.####}", val); // 4 decimal
            else
                strVal = string.Format("{0,1:0.###}", val); // 3 decimal
            return strVal;
        }

        /// <summary>
        /// dots ...
        /// </summary>
        /// <param name="dots"></param>
        /// <returns></returns>
        public static string DotContinue(string dots, int length)
        {
            string dotsRet = dots;
            if (dots.Length > 0 && !dots.Contains("."))
                dotsRet = ".";
            if (dots.Length < length)
                dotsRet = dotsRet + ".";
            else
                dotsRet = string.Empty;
            return dotsRet;
        }

        #endregion

        #region Manages DB and Error to the Log

        public static List<string> transactionString = new List<string>(), errorString = new List<string>();

        public static void AddToTransactionDbList(string transaction)
        {
            transactionString.Add(transaction);
        }

        public static void AddToErrorDbList(string error)
        {
            errorString.Add(error);
        }

        public static List<string> DisplayFromTransactionDbListToLog()
        {
            List<string> transactionTmp = new List<string>();
            if (transactionString.Count > 0)
            {
                transactionTmp = transactionString;
            }
            return transactionTmp;
        }

        public static List<string> DisplayFromErrorDbListToLog()
        {
            List<string> errorTmp = new List<string>();
            if (errorString.Count > 0)
                errorTmp = errorString;
            return errorTmp;
        }

        public static void ClearToTransactionDbList()
        {
            transactionString.Clear();
        }

        public static void ClearToErrorDbList()
        {
            errorString.Clear();
        }

        public static Color ColorByName(string name)
        {
            Color c = Color.White;
            try
            {
                switch (name)
                {
                    case "Green":
                    case "Run":
                        c = Color.FromArgb(0, 255, 0);
                        break;
                    case "Yellow":
                    case "Warn":
                    case "Wait":
                        c = Color.Yellow;
                        break;
                    case "Red":
                    case "Error":
                        c = Color.Red;
                        break;
                    default:
                        c = Color.FromName(name);
                        break;
                }
            }
            catch { }
            return c;
        }

        #endregion

        #region Network: IP	

        public bool ValidateIpV4(string ipString)
        {
            if (String.IsNullOrWhiteSpace(ipString))
                return false;
            string[] splitValues = ipString.Split('.');
            if (splitValues.Length != 4)
                return false;
            IPAddress? newIp;
            IPAddress.TryParse(ipString, address: out newIp);
            byte tempForParsing;
            return splitValues.All(r => byte.TryParse(r, out tempForParsing));
        }

        #endregion

    }
}
