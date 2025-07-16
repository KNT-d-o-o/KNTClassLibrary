using System.Diagnostics;
using System.Runtime.InteropServices;

namespace KNTToolsAndAccessories
{
    public class Tools
    {
        public void LogEvent2(int errorNum, String msg)
        {
            StackTrace stackTrace = new StackTrace();
            var fullName = stackTrace!.GetFrame(1)!.GetMethod()!.DeclaringType!.FullName;
            var m = $"{fullName} #{errorNum} {msg}";

            LogEvent(m);
        }

        public void LogEvent(String msg)
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                using EventLog eventLog = new EventLog("Application")
                {
                    Source = "KNT Application"
                };
                eventLog.WriteEntry(msg, EventLogEntryType.Error);

            }
            // TO DO platforms
        }


        public static void LogEvent(String msg, EventLogEntryType type)
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                using EventLog eventLog = new EventLog("Application")
                {
                    Source = "KNT Application"
                };
                eventLog.WriteEntry(msg, type);

#if DEBUG
                Console.WriteLine(msg);
#endif
            }
            // TO DO platforms
        }


        public static string runningDots(string dots, int max)
        {
            if (dots.Length < max)
                dots += ".";
            else
                dots = string.Empty;
            return dots;
        }

    }
}
