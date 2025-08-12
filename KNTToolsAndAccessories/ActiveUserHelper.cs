using System;
using System.Runtime.InteropServices;

public static class ActiveUserHelper
{
    const int WTS_CURRENT_SERVER_HANDLE = 0;
    const int WTSUserName = 5;
    const int WTSDomainName = 7;

    enum WTS_CONNECTSTATE_CLASS
    {
        WTSActive,
        WTSConnected,
        WTSConnectQuery,
        WTSShadow,
        WTSDisconnected,
        WTSIdle,
        WTSListen,
        WTSReset,
        WTSDown,
        WTSInit
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct WTS_SESSION_INFO
    {
        public int SessionID;
        [MarshalAs(UnmanagedType.LPStr)]
        public string pWinStationName;
        public WTS_CONNECTSTATE_CLASS State;
    }

    [DllImport("Wtsapi32.dll")]
    private static extern bool WTSEnumerateSessions(
        IntPtr hServer,
        int reserved,
        int version,
        out IntPtr ppSessionInfo,
        out int pCount
    );

    [DllImport("Wtsapi32.dll")]
    private static extern void WTSFreeMemory(IntPtr pMemory);

    [DllImport("Wtsapi32.dll")]
    private static extern bool WTSQuerySessionInformation(
        IntPtr hServer,
        int sessionId,
        int wtsInfoClass,
        out IntPtr ppBuffer,
        out int pBytesReturned
    );

    public static string GetActiveUserName()
    {
        IntPtr pSessionInfo = IntPtr.Zero;
        int sessionCount = 0;

        if (WTSEnumerateSessions(IntPtr.Zero, 0, 1, out pSessionInfo, out sessionCount))
        {
            int dataSize = Marshal.SizeOf(typeof(WTS_SESSION_INFO));
            IntPtr current = pSessionInfo;

            for (int i = 0; i < sessionCount; i++)
            {
                WTS_SESSION_INFO si = (WTS_SESSION_INFO)Marshal.PtrToStructure(current, typeof(WTS_SESSION_INFO));
                current += dataSize;

                if (si.State == WTS_CONNECTSTATE_CLASS.WTSActive)
                {
                    string user = GetSessionInfo(si.SessionID, WTSUserName);

                    if (!string.IsNullOrEmpty(user))
                        return user;
                }
            }

            WTSFreeMemory(pSessionInfo);
        }

        return "Public";
    }

    private static string GetSessionInfo(int sessionId, int infoClass)
    {
        IntPtr buffer;
        int bytesReturned;

        if (WTSQuerySessionInformation(IntPtr.Zero, sessionId, infoClass, out buffer, out bytesReturned) && buffer != IntPtr.Zero)
        {
            try
            {
                string result = Marshal.PtrToStringAnsi(buffer) ?? string.Empty;
                return result;
            }
            finally
            {
                WTSFreeMemory(buffer);
            }
        }
        return string.Empty;
    }
}
