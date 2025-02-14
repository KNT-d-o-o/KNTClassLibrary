using System.Diagnostics;
using System.Runtime.InteropServices;

namespace KNTToolsAndAccessories
{
    public class Tools
    {
        public void LogEvent(String msg)
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                using EventLog eventLog = new EventLog("Application");

                eventLog.Source = "Application";
                eventLog.WriteEntry(msg, EventLogEntryType.Error);

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
