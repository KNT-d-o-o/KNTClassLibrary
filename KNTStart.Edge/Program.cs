using System;
using System.Diagnostics;
using System.IO;
using System.Threading;

namespace KNTLeakTester.Edge
{
    class Program
    {
        static void Main(string[] args)
        {
            string port = "80";
            if (args.Length < 1)
            {
                Console.WriteLine(DateTime.Now + " argument Port must be defined.");
                return;
            }
            else
            {
                port = args[0];
                Console.WriteLine(DateTime.Now + $" start web server, Port {port}.");
            }

            string logFolderPath = port == "5005" ? "C:/KntLeakTester/Log" : "C:/Programs/PUBLISH-SMM/Log";

            if (!Directory.Exists(logFolderPath))
                Directory.CreateDirectory(logFolderPath);

            string shutdownFilePath = Path.Combine(logFolderPath, "shutdown.txt");
            string logFilePath = Path.Combine(logFolderPath, "log.txt");
            if (!File.Exists(logFilePath))
                File.Create(logFilePath).Dispose();

            if (File.Exists(shutdownFilePath))
            {
                File.Delete(shutdownFilePath);
                Console.WriteLine("Previous shutdown file found and deleted.");
            }

            using (StreamWriter writer = new StreamWriter(logFilePath, append: true))
                writer.WriteLine(DateTime.Now + " KNT Leak Tester GUI started.");

            int checkInterval = args.Length > 1 ? int.Parse(args[1]) : 10000;
            string edgePath = @"C:\Program Files (x86)\Microsoft\Edge\Application\msedge.exe";
            string url = $"http://localhost:{port}";

            Process edgeProcess = null;
            while (true)
            {
                
                Process[] edgeProcesses = Process.GetProcessesByName("msedge");

                // Preveri, ali je prejet ukaz za zaustavitev
                if (File.Exists(shutdownFilePath))
                {
                    using (StreamWriter writer = new StreamWriter(logFilePath, append: true))
                        writer.WriteLine(DateTime.Now + " KNT Leak Tester GUI stopped.");

                    Console.WriteLine(DateTime.Now + " Shutdown signal received. Exiting...");

                    try
                    {
                        edgeProcess.Kill();
                        edgeProcess.WaitForExit(); // Počakaj, da se proces v celoti zapre
                    }
                    catch { }

                    break;
                }

                // Če Edge ne teče, ga zaženi
                if (edgeProcesses.Length == 0 && !File.Exists(shutdownFilePath))
                {
                    Console.WriteLine(DateTime.Now + " Edge is not running. Restarting...");
                    edgeProcess = Process.Start(new ProcessStartInfo
                    {
                        FileName = edgePath,
                        Arguments = "--kiosk --edge-kiosk-type=fullscreen http://localhost:" + port,
                        UseShellExecute = true,
                        CreateNoWindow = false
                    });

                    Thread.Sleep(5000); // Počakaj, da se Edge zažene
                }

                // Preveri, ali je potrebna osvežitev strani
                if (checkInterval > 0 && !File.Exists(shutdownFilePath))
                {
                    Thread.Sleep(checkInterval);
                    if (IsRefreshNeeded(url))
                    {
                        RestartEdge(edgeProcess, new ProcessStartInfo
                        {
                            FileName = edgePath,
                            Arguments = "--kiosk --edge-kiosk-type=fullscreen http://localhost:" + port,
                            UseShellExecute = true,
                            CreateNoWindow = false
                        });
                        Console.WriteLine(DateTime.Now + " The page has been refreshed.");
                    }
                }
            }
        }

        static bool IsRefreshNeeded(string url)
        {
            try
            {
                // PowerShell ukaz za preverjanje statusa spletne strani
                ProcessStartInfo powerShellInfo = new ProcessStartInfo
                {
                    FileName = "powershell",
                    Arguments = $"-Command \"try {{ Invoke-WebRequest -Uri '{url}' -UseBasicParsing -TimeoutSec 5 }} catch {{ Write-Host 'Error' ; exit 1 }}\"",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                using (Process process = Process.Start(powerShellInfo))
                {
                    process.WaitForExit();

                    // Če se zgodi napaka, bo vrnjen exit code 1
                    if (process.ExitCode != 0)
                    {
                        return true;  // Napaka, potrebna osvežitev
                    }

                    // Preveri izhodne podatke, če je napaka izpisana (če potrebujemo dodatno preverjanje)
                    string output = process.StandardOutput.ReadToEnd();
                    if (output.Contains("Error"))
                    {
                        return true;  // Če je izpisana napaka, potrebna osvežitev
                    }

                    return false; // Če ni napake, ni potrebna osvežitev
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(DateTime.Now + $" Error checking page status: {ex.Message}");
                return true;  // Če pride do napake pri izvajanju funkcije, domnevamo, da je potrebna osvežitev
            }
        }

        static void RestartEdge(Process edgeProcess, ProcessStartInfo psi)
        {
            edgeProcess.Kill();
            Thread.Sleep(2000);
            Process.Start(psi);
        }
    }
}
