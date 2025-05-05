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

namespace KNTToolsAndAccessories
{
    public class Accessories
    {
        #region regedit

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

        /// <summary>
        /// bar to unit conversion
        /// </summary>
        /// <param name="barPressure"></param>
        /// <param name="unit"></param>
        /// <returns></returns>
        public static double MbarToUnit(double barPressure, string unit)
        {
            double convertedPressure = barPressure; // mbar

            switch (unit)
            {
                case "bar":
                    convertedPressure /= 1000;
                    break;
                case "atm":
                    convertedPressure *= 1.01325 / 1000;
                    break;
                case "Pa":
                    convertedPressure *= 100;
                    break;
                case "kPa":
                    convertedPressure *= 0.1;
                    break;
                case "Torr": // mmHg
                    convertedPressure *= 750.06 / 1000;
                    break;
                case "psi": // pounds per square inch
                    convertedPressure *= 14.5038 / 1000;
                    break;
                case "mmH2O":
                    convertedPressure *= 10.1973;
                    break;
            }
            return convertedPressure;
        }

        /// <summary>
        /// unit to bar conversion
        /// </summary>
        /// <param name="unitPressure"></param>
        /// <param name="unit"></param>
        /// <returns></returns>
        public static double UnitToMbar(double unitPressure, string unit)
        {
            double mbarPressure = unitPressure; // unit

            switch (unit)
            {
                case "bar":
                    mbarPressure *= 1000;
                    break;
                case "atm":
                    mbarPressure /= 1.01325 * 1000;
                    break;
                case "Pa":
                    mbarPressure /= 100;
                    break;
                case "kPa":
                    mbarPressure /= 0.1;
                    break;
                case "Torr": // mmHg
                    mbarPressure /= 750.06 * 1000;
                    break;
                case "psi": // pounds per square inch
                    mbarPressure /= 14.5038 * 1000;
                    break;
                case "mmH2O":
                    mbarPressure /= 10.1973;
                    break;
            }
            return mbarPressure;
        }

        /// <summary>
        /// ml/min to unit conversion
        /// </summary>
        /// <param name="leak"></param>
        /// <param name="unit"></param>
        /// <returns></returns>
        public static double LeakToUnit(double leak, string unit)
        {
            double convertedLeak = leak; // ml/min

            switch (unit)
            {
                case "mbar*l/s":
                    convertedLeak *= 0.0168875;
                    break;
                case "Torr*l/s":
                    convertedLeak *= 0.0126666667;
                    break;
                case "sccm":
                case "atm*ml/min":
                case "cm3/min":
                    convertedLeak *= 1;
                    break;
                case "atm*cc/s":
                case "cm3/s":
                case "ml/s":
                    convertedLeak /= 60;
                    break;
                case "Pa*m3/s":
                    convertedLeak *= 0.00168875;
                    break;
                case "slm":
                    convertedLeak /= 1000;
                    break;
                case "kg Mole/s":
                    convertedLeak *= (28.97 / (1000 * 22.414 * 60)); // 28.97 - molar mass for air !
                    break;
                case "sft3/zr":
                    convertedLeak *= (0.035315 / 1000 * 525600);
                    break;
                case "mm3/s":
                    convertedLeak *= (1000 / 60);
                    break;
                case "cm3/h":
                case "ml/h":
                    convertedLeak *= 60;
                    break;
            }
            return convertedLeak;
        }

        /// <summary>
        /// unit to ml/min conversion
        /// </summary>
        /// <param name="unitVal"></param>
        /// <param name="unit"></param>
        /// <returns></returns>
        public static double UnitToLeak(double unitVal, string unit)
        {
            double leak = unitVal; // unit

            switch (unit)
            {
                case "mbar*l/s":
                    leak /= 0.0168875;
                    break;
                case "Torr*l/s":
                    leak /= 0.0126666667;
                    break;
                case "sccm":
                case "atm*ml/min":
                case "cm3/min":
                    leak /= 1;
                    break;
                case "atm*cc/s":
                case "cm3/s":
                case "ml/s":
                    leak *= 60;
                    break;
                case "Pa*m3/s":
                    leak /= 0.00168875;
                    break;
                case "slm":
                    leak *= 1000;
                    break;
                case "kg Mole/s":
                    leak /= (28.97 / (1000 * 22.414 * 60)); // 28.97 - molar mass for air !
                    break;
                case "sft3/zr":
                    leak /= (0.035315 / 1000 * 525600);
                    break;
                case "mm3/s":
                    leak /= (1000 / 60);
                    break;
                case "cm3/h":
                case "ml/h":
                    leak /= 60;
                    break;
            }
            return leak;
        }

        const int PASCAL_CONV_FACTOR = 10;

        /// <summary>
        /// int from DB to float pascal conversion
        /// </summary>
        /// <param name="val"></param>
        /// <returns></returns>
        public static float IntToPascal(int val)
        {
            float pascalPressure = (float)val / PASCAL_CONV_FACTOR;
            return pascalPressure;
        }

        /// <summary>
        /// float pascal to int DB conversion
        /// </summary>
        /// <param name="val"></param>
        /// <returns></returns>
        public static int PascalToInt(float val)
        {
            int intPressure = (int)(val * PASCAL_CONV_FACTOR);
            return intPressure;
        }

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
        /// format to any decimal places
        /// </summary>
        /// <param name="format"></param>
        /// <param name="val"></param>
        /// <returns></returns>
        public static string FormatToDecimalValues(int format, double val, bool shortExp)
        {
            if (format == 0) // 15 decimal
                return string.Format("{0,1:0.###############}", val);
            string strVal = "0";

            if ((format == 3 && Math.Abs(val) < 0.01) && val != 0)
            {
                if (!shortExp)
                    strVal = string.Format("{0:0.###E+00}", val); // Exp E+00, 3 decimal
                else
                    strVal = string.Format("{0:0.###E+0}", val); // Exp E+0, 3 decimal
            }
            else if ((format == 2 && Math.Abs(val) < 0.001) && val != 0)
            {
                if (!shortExp)
                    strVal = string.Format("{0:0.##E+00}", val); // Exp E+00, Exp, 2 decimal
                else
                    strVal = string.Format("{0:0.##E+0}", val); // Exp E+0, Exp, 2 decimal
            }
            else if ((format == 1 && Math.Abs(val) < 0.0001) && val != 0)
            {
                if (!shortExp)
                    strVal = string.Format("{0:0.#E+00}", val); // Exp E+00, 1 decimal
                else
                    strVal = string.Format("{0:0.#E+0}", val); // Exp E+0, 1 decimal
            }
            else if (((format == 3 && Math.Abs(val) < 0.1) || (format == 2 && Math.Abs(val) < 0.01) || (format == 1 && Math.Abs(val) < 0.001)) && val != 0)
                strVal = string.Format("{0:0.#####}", val); // 5 decimal
            else if (((format == 3 && Math.Abs(val) < 1) || (format == 2 && Math.Abs(val) < 0.1) || (format == 1 && Math.Abs(val) < 0.01)) && val != 0)
                strVal = string.Format("{0:0.####}", val); // 4 decimal
            else if ((format == 3 || (format == 2 && Math.Abs(val) < 1) || (format == 1 && Math.Abs(val) < 0.1)) && val != 0)
                strVal = string.Format("{0:0.###}", val); // 3 decimal
            else if ((format == 2 || (format == 1 && Math.Abs(val) < 1)) && val != 0)
                strVal = string.Format("{0:0.##}", val); // 2 decimal
            else
                strVal = string.Format("{0:0.#}", val); // 1 decimal
            return strVal;
        }

        /// <summary>
        /// get nice number step for graph
        /// </summary>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <param name="desiredTicks"></param>
        /// <returns></returns>
        public static double GetNiceStep(double min, double max, int desiredTicks)
        {
            double range = max - min;
            double rawStep = range / desiredTicks;
            double magnitude = Math.Pow(10, Math.Floor(Math.Log10(rawStep)));
            double refinedStep = Math.Ceiling(rawStep / magnitude) * magnitude;
            return refinedStep;
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
