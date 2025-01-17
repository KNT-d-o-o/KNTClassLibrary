using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using OpenQA.Selenium;
using OpenQA.Selenium.BiDi.Modules.Network;
using OpenQA.Selenium.Edge;

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

            string logFolderPath = string.Empty;

            if (port == "5005")
                logFolderPath = "C:/KntLeakTester/Log";
            else if(port == "5015")
                logFolderPath = "C:/Programs/PUBLISH-SMM/Log";

            if (!Directory.Exists(logFolderPath))
            {
                Directory.CreateDirectory(logFolderPath);
            }
            string shutdownFilePath = logFolderPath + "/shutdown.txt";
            string logFilePath = logFolderPath + "/log.txt";
            if (!File.Exists(logFilePath))
            {
                using (FileStream fs = File.Create(logFilePath)) { }
            }

            if (File.Exists(shutdownFilePath))
            {
                File.Delete(shutdownFilePath);
#if DEBUG
                Console.WriteLine("Previous shutdown file found and deleted.");
#endif
            }

            using (StreamWriter writer = new StreamWriter(logFilePath, append: true))
            {
                writer.WriteLine(DateTime.Now + " KNT Leak Tester GUI started.");
            }

            int checkInterval = 10000;
            if (args.Length > 1)
                checkInterval = int.Parse(args[1]);

            var edgeOptions = new EdgeOptions();
            edgeOptions.AddArgument("disable-gpu");
            edgeOptions.AddArgument("disable-infobars");
            edgeOptions.AddExcludedArgument("enable-automation");
            edgeOptions.AddArgument("-start-maximized");
            edgeOptions.AddArgument("-start-fullscreen");
            edgeOptions.AddArgument("--disable-popup-blocking");
            edgeOptions.AddArguments("--disable-extensions");
            edgeOptions.AddArgument("--disable-notifications");
            edgeOptions.AddArguments("--disable-application-cache");
            edgeOptions.AddArguments("--guest");
            edgeOptions.AddArgument("--no-sandbox");
            edgeOptions.AddArgument("--disable-dev-shm-usage");

            edgeOptions.AddArgument("--disable-accelerated-2d-canvas");
            edgeOptions.AddArgument("--disable-accelerated-video-decode");
            edgeOptions.AddArgument("--disable-renderer-backgrounding");
            edgeOptions.AddArgument("--blink-settings=animationPolicy=none");

            edgeOptions.AddArgument("--force-device-scale-factor=1");
            edgeOptions.AddArgument("--disable-pinch");
            edgeOptions.AddArgument("--disable-gesture-requirement-for-media-controls");


            var edgeDriverPath = @"Driver";

            using (IWebDriver driver = new EdgeDriver(edgeDriverPath, edgeOptions))
            {
                string url = $"http://localhost:{port}";
                driver.Navigate().GoToUrl(url);

                while (true)
                {
                    // remove shutdown file
                    if (File.Exists(shutdownFilePath))
                    {
                        using (StreamWriter writer = new StreamWriter(logFilePath, append: true))
                        {
                            writer.WriteLine(DateTime.Now + " KNT Leak Tester GUI stopped.");
                        }
                        Console.WriteLine(DateTime.Now + " #1 Shutdown signal received. Exiting...");
                        break;
                    }

                    if (checkInterval > 0)
                    {
                        try
                        {
                            Thread.Sleep(checkInterval);

                            if (IsRefreshNeeded(driver))
                            {
                                driver.Navigate().Refresh();
#if DEBUG
                                Console.WriteLine("The page has been refreshed.");
#endif
                            }
                            else
                            {
#if DEBUG
                                Console.WriteLine("The page does not need to be refreshed.");
#endif
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(DateTime.Now + $" #2 An error has occurred: {ex.Message}");
                            return;
                        }
                    }
                }
            }
        }

        static bool IsRefreshNeeded(IWebDriver driver)
        {
            try
            {
                // Attempt to locate specific elements that indicate a timeout or connection issue
                var errorMessages = new[]
                {
                    "Timeout",          // General timeout message
                    "Connection lost",  // Connection lost message
                    "Reload",           // Reload prompt message
                    "Page unresponsive" // Page unresponsive message
                };

                foreach (string message in errorMessages)
                {
                    if (driver.FindElements(By.XPath($"//*[contains(text(), '{message}')]")).Count > 0)
                    {
                        return true;
                    }
                }

                return false;
            }
            catch (NoSuchElementException)
            {
                // If no error elements are found, assume the page is still loaded properly
                return false;
            }
        }

        static void RestartIIS()
        {
            try
            {
                ProcessStartInfo iisResetProcessInfo = new ProcessStartInfo
                {
                    FileName = "iisreset",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    Verb = "runas" // Request elevated privileges
                };

                using (Process iisResetProcess = new Process())
                {
                    iisResetProcess.StartInfo = iisResetProcessInfo;
                    iisResetProcess.Start();

                    // Read the output (optional)
                    string output = iisResetProcess.StandardOutput.ReadToEnd();
                    string error = iisResetProcess.StandardError.ReadToEnd();

                    iisResetProcess.WaitForExit();

                    if (iisResetProcess.ExitCode == 0)
                    {
                        Console.WriteLine(DateTime.Now + " #3 IIS has been reset successfully.");
                    }
                    else
                    {
                        Console.WriteLine(DateTime.Now + $" #4 Error resetting IIS: {error}");
                    }

                    Console.WriteLine(DateTime.Now + " #5 " + output);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(DateTime.Now + $" #6 An error occurred: {ex.Message}");
            }
        }

    }
}
