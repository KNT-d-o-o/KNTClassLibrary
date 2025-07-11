using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using WebSocketSharp;

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
        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        const int SW_MINIMIZE = 6;
        const int SW_MAXIMIZE = 3;
        const int SW_SHOWNORMAL = 1;

        static void Main(string[] args)
        {
            string port = "80";
            if (args.Length < 1)
            {
#if DEBUG
                Console.WriteLine(DateTime.Now + " argument Port must be defined.");
#endif
                return;
            }
            else
            {
                port = args[0];
#if DEBUG
                Console.WriteLine(DateTime.Now + $" start web server, Port {port}.");
#endif
            }

            string logFolderPath = port == "5005" ? "C:\\KntLeakTester\\Log" : "C:\\Programs\\PUBLISH-SMM\\Log";

            if (!Directory.Exists(logFolderPath))
                Directory.CreateDirectory(logFolderPath);

            string logFilePath = Path.Combine(logFolderPath, "log.txt");
            if (!File.Exists(logFilePath))
                File.Create(logFilePath).Dispose();

            string shutdownFilePath = Path.Combine(logFolderPath, "shutdown.txt");
            if (File.Exists(shutdownFilePath))
            {
                File.Delete(shutdownFilePath);
#if DEBUG
                Console.WriteLine("Previous shutdown file found and deleted.");
#endif
            }

            string minimizedFilePath = Path.Combine(logFolderPath, "minimized.txt");
            if (File.Exists(minimizedFilePath))
            {
                File.Delete(minimizedFilePath);
#if DEBUG
                Console.WriteLine("Previous minimized file found and deleted.");
#endif
            }

            using (StreamWriter writer = new StreamWriter(logFilePath, append: true))
                writer.WriteLine(DateTime.Now + " KNT Leak Tester GUI started.");

            const int REFRESH_INTERVAL = 20000; // default refresh interval in milliseconds
            int checkInterval = args.Length > 2 ? int.Parse(args[2]) : REFRESH_INTERVAL;
            if(checkInterval == 0)
                checkInterval = REFRESH_INTERVAL; // if 0, set default
            string edgePath = @"C:\Program Files (x86)\Microsoft\Edge\Application\msedge.exe";
            string urlServer = "http://localhost:";
            string url = $"{urlServer}{port}";
            if (args.Length > 3)
                url += $"{args[3]}";

            string arguments = $"--remote-debugging-port=9222 {url}";
            if (args.Length > 1)
            if (args[1] == "kiosk")
                arguments = $"--kiosk --edge-kiosk-type=fullscreen {arguments}";
            else if (args[1] == "fullscreen")
                arguments = $"--start-fullscreen --disable-infobars --noerrdialogs {arguments}";

            Process edgeProcess = null;
            bool minimized = false;
            while (true)
            {
                Process[] edgeProcesses = Process.GetProcessesByName("msedge");

                // Check if a stop command has been received
                if (File.Exists(shutdownFilePath))
                    {
                        using (StreamWriter writer = new StreamWriter(logFilePath, append: true))
                            writer.WriteLine(DateTime.Now + " KNT Leak Tester GUI stopped.");
#if DEBUG
                        Console.WriteLine(DateTime.Now + " Shutdown signal received. Exiting...");
#endif

                        try
                        {
                            edgeProcess.Kill();
                            edgeProcess.WaitForExit(); // wait for closing process
                        }
                        catch { }

                        break;
                    }

                // edge not running, run it
                if (edgeProcesses.Length == 0 && !File.Exists(shutdownFilePath))
                {
#if DEBUG
                    Console.WriteLine(DateTime.Now + " Edge is not running. Restarting...");
#endif
                    edgeProcess = Process.Start(new ProcessStartInfo
                    {
                        FileName = edgePath,
                        Arguments = arguments,
                        UseShellExecute = true,
                        CreateNoWindow = false
                    });

                    minimized = false;
                    Thread.Sleep(500); // wait for runnin edge
                }

                // check if reloed is needed
                if (checkInterval > 0 && !File.Exists(shutdownFilePath))
                {
                    Thread.Sleep(checkInterval);
                    if (IsRefreshNeeded(urlServer, port))
                    {
                        edgeProcess = RestartEdge(edgeProcess, new ProcessStartInfo
                        {
                            FileName = edgePath,
                            Arguments = arguments,
                            UseShellExecute = true,
                            CreateNoWindow = false
                        });
#if DEBUG
                        Console.WriteLine(DateTime.Now + " The page has been refreshed.");
#endif
                    }
                }

                // Check minimized state
                if (edgeProcess != null)
                {
                    if (!minimized)
                    {
                        if (File.Exists(minimizedFilePath))
                        {
                            Console.WriteLine(DateTime.Now + " Edge is running. Minimized...");
                            ShowWindow(edgeProcess.MainWindowHandle, SW_MINIMIZE);
                            minimized = true;
                        }
                    }
                    else if (minimized)
                    {
                        if (!File.Exists(minimizedFilePath))
                        {
                            Console.WriteLine(DateTime.Now + " Edge is running. Maximized...");
                            if (args[1] == "fullscreen")
                                ShowWindow(edgeProcess.MainWindowHandle, SW_MAXIMIZE);
                            else
                                ShowWindow(edgeProcess.MainWindowHandle, SW_SHOWNORMAL);
                            minimized = false;
                        }
                    }
                }
            }
        }

        static bool IsRefreshNeeded(string urlServer, string port)
        {
            bool ret = false;
            try
            {       
                string wsUrl = string.Empty;

                using (var client = new HttpClient())
                {
                    // Getting a JSON response from the /json endpoint
                    string jsonResponse = client.GetStringAsync($"{urlServer}9222/json").Result;

                    // Parsing the JSON response
                    var jsonArray = JsonDocument.Parse(jsonResponse).RootElement.EnumerateArray().ToList();

                    if (jsonArray.Count > 0)
                    {
                        // Getting a WebSocket URL
                        wsUrl = jsonArray[0].GetProperty("webSocketDebuggerUrl").GetString();
                    }
                    else
                    {
#if DEBUG
                        Console.WriteLine("No browser instances found.");
#endif
                        return true;
                    }
                }

                WebSocket ws = new WebSocket(wsUrl);

                // We store the id so we can link requests to responses
                int requestId = 1; // First ID for DOM.getDocument
                int outerHtmlRequestId = 2; // ID for DOM.getOuterHTML

                ws.OnMessage += (sender, e) =>
                {
                    // Run debug log for e.Data
#if DEBUG
                    Console.WriteLine("Prejeti podatki: " + e.Data);
#endif

                    try
                    {
                        // Parse JSON response
                        var message = JsonDocument.Parse(e.Data).RootElement;

                        // Check if it is a response to DOM.getDocument
                        if (message.TryGetProperty("id", out var id) && id.GetInt32() == requestId)
                        {
                            // Extract nodeId for root element (BODY)
                            string bodyNodeId = message.GetProperty("result").GetProperty("root")
                                .GetProperty("children")[1].GetProperty("nodeId").ToString(); // index 1 is BODY

                            // Send command to get outerHTML for BODY
                            string outerHtmlRequest = $"{{\"id\": {outerHtmlRequestId}, \"method\": \"DOM.getOuterHTML\", \"params\": {{\"nodeId\": {bodyNodeId}}}}}";
                            ws.Send(outerHtmlRequest);
#if DEBUG
                            Console.WriteLine("Poslan ukaz DOM.getOuterHTML za BODY element");
#endif
                        }
                        // Check if it is a response to DOM.getOuterHTML
                        else if (message.TryGetProperty("id", out var outerHtmlId) && outerHtmlId.GetInt32() == outerHtmlRequestId)
                        {
                            // Read HTML content
                            string htmlContent = message.GetProperty("result").GetProperty("outerHTML").GetString();

                            // Check if it contains the word "Reload"
                            if (htmlContent.Contains("Reload"))
                            {
                                ret = true;
#if DEBUG
                                Console.WriteLine("Stran potrebuje osvežitev!");
#endif
                            }
                            else
                            {
#if DEBUG
                                Console.WriteLine("Stran je OK.");
#endif
                            }

#if DEBUG
                            // output HTML to files
                            Console.WriteLine("HTML vsebina: ");
                            Console.WriteLine(htmlContent);
#endif
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Data processing error: {ex.Message}");
                        ret = true;
                    }
                };

                ws.OnOpen += (sender, e) =>
                {
                    // Wait for the connection to be established and send the command to get DOM
                    string request = "{\"id\": 1, \"method\": \"DOM.getDocument\"}";
                    ws.Send(request);
#if DEBUG
                    Console.WriteLine("Poslan ukaz DOM.getDocument");
#endif
                };

                ws.Connect();

                // Wait for the communication to complete.
                Thread.Sleep(5000);

                // Close the connection when finished.
                ws.Close();

            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error checking page status: {ex.Message}");
                return true; // error, need to refresh
            }
            return ret;
        }

        static Process RestartEdge(Process edgeProcess, ProcessStartInfo psi)
        {
            try
            {
                Console.WriteLine(DateTime.Now + " Restarting Edge...");

                // close ell edge windows
                Process.Start(new ProcessStartInfo
                {
                    FileName = "taskkill",
                    Arguments = "/F /IM msedge.exe /T",
                    UseShellExecute = false,
                    CreateNoWindow = true
                })?.WaitForExit();

                // waiting for close
                Thread.Sleep(3000);

                // edge kiosk start
                edgeProcess = Process.Start(psi);
#if DEBUG
                Console.WriteLine(DateTime.Now + " Edge restarted.");
#endif
            }
            catch (Exception ex)
            {
                Console.WriteLine(DateTime.Now + $" Error restarting Edge: {ex.Message}");
            }
            return edgeProcess;
        }

    }
}
