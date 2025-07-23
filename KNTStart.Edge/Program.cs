using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Management;
using WebSocketSharp;
using System.Windows.Automation;

namespace KNTLeakTester.Edge
{
    /// <summary>
    /// web server program running
    /// args:
    ///  args[0]: port of webserver
    ///  args[1]: kiosk or fullscreen or normal mode
    ///  args[2]: check interval for refresh (0 means default 20000ms)
    ///  args[3]: additional url link for sub page or properties
    /// </summary>
    class Program
    {
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool IsIconic(IntPtr hWnd);

        [DllImport("user32.dll")]
        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        const int SW_MINIMIZE = 6;
        const int SW_MAXIMIZE = 3;
        const int SW_SHOWNORMAL = 1;
        static DateTime refreshTime = DateTime.Now;

        static void Main(string[] args)
        {
            string port = "80";
            if (args.Length < 1)
            {
                DebugWriteline("argument Port must be defined.");
                return;
            }

            port = args[0];
            int webPort = int.Parse(port);
            int debugPort = 9222 + (webPort % 100);

            bool createdNew;
            string mutexName = $"KNTLeakTesterGUIMutex_Port{port}";

            using (Mutex mutex = new Mutex(true, mutexName, out createdNew))
            {
                if (!createdNew)
                {
                    DebugWriteline($"The program for port {port} is already running.");
                    return;
                }

                DebugWriteline($"start web server, Port {port}; debug port {debugPort}.");

                string logFolderPath = port == "5005" ? "C:\\KntLeakTester\\Log" : "C:\\Programs\\SMM\\Log";

                if (!Directory.Exists(logFolderPath))
                    Directory.CreateDirectory(logFolderPath);

                string logFilePath = Path.Combine(logFolderPath, "log.txt");
                if (!File.Exists(logFilePath))
                    File.Create(logFilePath).Dispose();

                string shutdownFilePath = Path.Combine(logFolderPath, "shutdown.txt");
                if (File.Exists(shutdownFilePath))
                {
                    File.Delete(shutdownFilePath);
                    DebugWriteline("Previous shutdown file found and deleted.");
                }

                string minimizedFilePath = Path.Combine(logFolderPath, "minimized.txt");
                if (File.Exists(minimizedFilePath))
                {
                    File.Delete(minimizedFilePath);
                    DebugWriteline("Previous minimized file found and deleted.");
                }

                using (StreamWriter writer = new StreamWriter(logFilePath, append: true))
                    writer.WriteLine(DateTime.Now + " KNT Leak Tester GUI started.");

                const int REFRESH_INTERVAL = 20000; // default refresh interval in milliseconds
                int checkInterval = args.Length > 2 ? int.Parse(args[2]) : REFRESH_INTERVAL;
                if (checkInterval == 0)
                    checkInterval = REFRESH_INTERVAL; // if 0, set default
                string edgePath = @"C:\Program Files (x86)\Microsoft\Edge\Application\msedge.exe";
                string urlServer = "http://localhost:";
                string url = $"{urlServer}{port}";
                if (args.Length > 3)
                    url += $"{args[3]}";

                string arguments = $"--remote-debugging-port={debugPort} {url}";
                if (args.Length > 1)
                    if (args[1] == "kiosk")
                        arguments = $"--kiosk --edge-kiosk-type=fullscreen {arguments}";
                    else if (args[1] == "fullscreen")
                        arguments = $"--start-fullscreen --disable-infobars --noerrdialogs {arguments}";

                Process edgeProcess = null;
                while (true)
                {
                    bool isEdgeRunningForThisPort = IsEdgeRunningForPort(debugPort, out edgeProcess);

                    // Check minimized state
                    try
                    {
                        if (edgeProcess != null && edgeProcess.MainWindowHandle != IntPtr.Zero)
                        {
                            bool shouldBeMinimized = File.Exists(minimizedFilePath);
                            bool isCurrentlyMinimized = IsIconic(edgeProcess.MainWindowHandle);

                            if (shouldBeMinimized && !isCurrentlyMinimized)
                            {
                                DebugWriteline("Minimizing Edge...");
                                ShowWindow(edgeProcess.MainWindowHandle, SW_MINIMIZE);
                            }
                            else if (!shouldBeMinimized && isCurrentlyMinimized)
                            {
                                DebugWriteline("Restoring Edge...");
                                if (args[1] == "fullscreen")
                                    ShowWindow(edgeProcess.MainWindowHandle, SW_MAXIMIZE);
                                else
                                    ShowWindow(edgeProcess.MainWindowHandle, SW_SHOWNORMAL);
                            }
                        }
                    }
                    catch { }

                    // Check if a stop command has been received
                    if (File.Exists(shutdownFilePath))
                    {
                        using (StreamWriter writer = new StreamWriter(logFilePath, append: true))
                            writer.WriteLine(DateTime.Now + " KNT Leak Tester GUI stopped.");
                        DebugWriteline("Shutdown signal received. Exiting...");

                        if(StopEdge(edgeProcess))
                            break;
                    }

                    // restart every day
                    if (refreshTime.Date != DateTime.Now.Date)
                    {
                        if(StopEdge(edgeProcess))
                            refreshTime = DateTime.Now;
                    }

                    // edge not running, run it
                    if (!isEdgeRunningForThisPort && !File.Exists(shutdownFilePath))
                    {
                        DebugWriteline("Edge is not running. Restarting...");
                        edgeProcess = Process.Start(new ProcessStartInfo
                        {
                            FileName = edgePath,
                            Arguments = arguments,
                            UseShellExecute = true,
                            CreateNoWindow = false
                        });

                        Thread.Sleep(500); // wait for runnin edge
                    }

                    Thread.Sleep(checkInterval);
                }
            }
        }

        static bool IsEdgeRunningForPort(int port, out Process process)
        {
            process = null;
            try
            {
                using (var searcher = new ManagementObjectSearcher("SELECT ProcessId, CommandLine FROM Win32_Process WHERE Name = 'msedge.exe'"))
                {
                    foreach (ManagementObject obj in searcher.Get())
                    {
                        string commandLine = obj["CommandLine"]?.ToString() ?? "";
                        if (commandLine.Contains($"--remote-debugging-port={port}"))
                        {
                            int pid = Convert.ToInt32(obj["ProcessId"]);
                            try
                            {
                                process = Process.GetProcessById(pid);
                                DebugWriteline($"Edge is already running with remote debugging port {port} (PID: {pid}).");
                                return true;
                            }
                            catch (Exception ex)
                            {
                                DebugWriteline($"Failed to get process by ID {pid}: {ex.Message}");
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                DebugWriteline($"Failed to check Edge processes: {ex.Message}");
            }

            return false;
        }

        static bool StopEdge(Process edgeProcess)
        {
            try
            {
                edgeProcess.Kill();
                edgeProcess.WaitForExit();
            }
            catch (Exception ex)
            {
                DebugWriteline("Failed to close old Edge process: " + ex.Message);
                return false;
            }
            return true;
        }

        static void DebugWriteline(string str)
        {
#if DEBUG
            Console.WriteLine($"{DateTime.Now} {str}");
#endif
        }

    }
}
