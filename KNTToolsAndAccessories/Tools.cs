using System.Diagnostics;
using System.Runtime.InteropServices;

namespace KNT_ToolsAndAccessories
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

    }
}
