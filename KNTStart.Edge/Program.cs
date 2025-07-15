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

                        try
                        {
                            edgeProcess.Kill();
                            edgeProcess.WaitForExit(); // wait for closing process
                        }
                        catch { }
                        break;
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

                    // check if reloed is needed // IntPtr 
                    /* fstaa NOK
                    if (checkInterval > 0 && !File.Exists(shutdownFilePath))
                    {
                        if (IsRefreshNeeded("http://localhost:", debugPort))
                        {
                            edgeProcess = RestartEdge(edgeProcess, new ProcessStartInfo
                            {
                                FileName = edgePath,
                                Arguments = arguments,
                                UseShellExecute = true,
                                CreateNoWindow = false
                            });
                            DebugWriteline("The page has been refreshed.");
                        }
                    } */

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

        static bool IsRefreshNeeded(string urlServer, int port)
        {
            bool needsRefresh = false;
            ManualResetEventSlim waitHandle = new ManualResetEventSlim(false);

            try
            {
                string wsUrl = string.Empty;

                using (var client = new HttpClient())
                {
                    string jsonResponse = client.GetStringAsync($"{urlServer}{port}/json").Result;
                    var jsonArray = JsonDocument.Parse(jsonResponse).RootElement.EnumerateArray().ToList();

                    if (jsonArray.Count > 0)
                    {
                        wsUrl = jsonArray[0].GetProperty("webSocketDebuggerUrl").GetString();
                    }
                    else
                    {
                        DebugWriteline("No browser instances found.");
                        return true;
                    }
                }

                WebSocket ws = new WebSocket(wsUrl);

                int requestId = 1;
                int outerHtmlRequestId = 2;

                ws.OnMessage += (sender, e) =>
                {
                    try
                    {
                        var message = JsonDocument.Parse(e.Data).RootElement;

                        if (message.TryGetProperty("id", out var id) && id.GetInt32() == requestId)
                        {
                            string bodyNodeId = message.GetProperty("result").GetProperty("root")
                                .GetProperty("children")[1].GetProperty("nodeId").ToString();

                            string outerHtmlRequest = $"{{\"id\": {outerHtmlRequestId}, \"method\": \"DOM.getOuterHTML\", \"params\": {{\"nodeId\": {bodyNodeId}}}}}";
                            ws.Send(outerHtmlRequest);
                        }
                        else if (message.TryGetProperty("id", out var outerHtmlId) && outerHtmlId.GetInt32() == outerHtmlRequestId)
                        {
                            string htmlContent = message.GetProperty("result").GetProperty("outerHTML").GetString();

                            if (htmlContent.Contains("Reload") || htmlContent.Contains("This site can’t be reached"))
                            {
                                DebugWriteline("The page needs a refresh!");
                                needsRefresh = true;
                            }
                            else
                            {
                                DebugWriteline("The page is OK.");
                            }

                            waitHandle.Set(); // signal that we're done
                        }
                    }
                    catch (Exception ex)
                    {
                        DebugWriteline($"Error processing WebSocket message: {ex.Message}");
                        needsRefresh = true;
                        waitHandle.Set(); // error ends waiting
                    }
                };

                ws.OnOpen += (sender, e) =>
                {
                    string request = "{\"id\": 1, \"method\": \"DOM.getDocument\"}";
                    ws.Send(request);
                };

                ws.Connect();

                // Wait for WebSocket response (max 5 seconds)
                if (!waitHandle.Wait(TimeSpan.FromSeconds(5)))
                {
                    DebugWriteline("Timeout waiting for WebSocket response.");
                    needsRefresh = true;
                }

                ws.Close();
            }
            catch (Exception ex)
            {
                DebugWriteline($"Error checking page status: {ex.Message}");
                return true;
            }

            return needsRefresh;
        }

        static Process RestartEdge(Process edgeProcess, ProcessStartInfo psi)
        {
            try
            {
                DebugWriteline("Restarting Edge...");

                if (edgeProcess != null && !edgeProcess.HasExited)
                {
                    edgeProcess.Kill();
                    edgeProcess.WaitForExit();
                }

                Thread.Sleep(1000);

                edgeProcess = Process.Start(psi);

                DebugWriteline("Edge restarted.");
            }
            catch (Exception ex)
            {
                DebugWriteline($"Error restarting Edge: {ex.Message}");
            }

            return edgeProcess;
        }

        static void DebugWriteline(string str)
        {
#if DEBUG
            Console.WriteLine($"{DateTime.Now} {str}");
#endif
        }

    }
}
