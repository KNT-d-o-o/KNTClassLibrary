﻿using System.Diagnostics;
using System.ServiceProcess;
using KNTToolsAndAccessories;

namespace KNTCommon.Business.Scripts
{
    public class PowerShellHelper
    {
        private readonly Tools t = new();

        public void ExecuteRestartScript(bool startAgain)
        {
            try
            {
                string baseDirectory = AppContext.BaseDirectory;
                string scriptPath = Path.Combine(baseDirectory, "Scripts", string.Empty);
                if (startAgain)
                    scriptPath = Path.Combine(baseDirectory, "Scripts", "RestartServer.ps1");
                else
                    scriptPath = Path.Combine(baseDirectory, "Scripts", "ShutdownServer.ps1");

                if (!File.Exists(scriptPath))
                {
                    throw new FileNotFoundException($"Script not found at path: {scriptPath}");
                }

                ProcessStartInfo psi = new ProcessStartInfo
                {
                    FileName = "powershell.exe",
                    Arguments = $"-NoProfile -ExecutionPolicy Bypass -File \"{scriptPath}\"",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                using (Process? process = Process.Start(psi))
                {
                    if (process != null)
                    {
                        process.WaitForExit();
                        string output = process.StandardOutput.ReadToEnd();
                        string error = process.StandardError.ReadToEnd();

                        if (!string.IsNullOrEmpty(error))
                        {
                            throw new Exception($"Error executing script: {error}");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                t.LogEvent("KNTCommon.Business.Scripts #1 " + ex.Message);
            }
        }

        public void SetSystemTime(DateTime newDateTime)
        {
            try
            {
                string baseDirectory = AppContext.BaseDirectory;
                string scriptPath = Path.Combine(baseDirectory, "Scripts", "SetDateTime.ps1");

                if (!File.Exists(scriptPath))
                {
                    throw new FileNotFoundException($"Script not found at path: {scriptPath}");
                }

                string formattedDateTime = newDateTime.ToString("yyyy-MM-dd HH:mm:ss");

                ProcessStartInfo psi = new ProcessStartInfo
                {
                    FileName = "powershell.exe",
                    Arguments = $"-NoProfile -ExecutionPolicy Bypass -File \"{scriptPath}\" -DateTime \"{formattedDateTime}\"",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    Verb = "runas"
                };

                using (Process? process = Process.Start(psi))
                {
                    if (process != null)
                    {
                        process.WaitForExit();
                        string output = process.StandardOutput.ReadToEnd();
                        string error = process.StandardError.ReadToEnd();

                        if (!string.IsNullOrEmpty(error))
                        {
                            throw new Exception($"Error executing script: {error}");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                t.LogEvent("KNTCommon.Business.Scripts #2 " + ex.Message);
            }
        }

        /// <summary>
        /// Start, stop restart service
        /// </summary>
        /// <param name="serviceName"></param>
        /// <param name="action"></param>
        /// 
        public bool StartStopService(string serviceName, string action)
        {
            // restart case
            if (action.Equals("restart", StringComparison.OrdinalIgnoreCase))
            {
                // stop
                if (!StartStopService(serviceName, "stop"))
                {
                    return false; // Če ustavitev ne uspe, ne nadaljujemo
                }

                // sleep
                Thread.Sleep(3000);

                // start
                return StartStopService(serviceName, "start");
            }

            ProcessStartInfo psi = new ProcessStartInfo
            {
                FileName = "cmd.exe",
                Arguments = $"/c sc {action} \"{serviceName}\"",
                Verb = "runas",
                UseShellExecute = true
            };

            try
            {
                Process.Start(psi);
            }
            catch (Exception ex)
            {
                t.LogEvent("KNTCommon.Business.Scripts #3 " + ex.Message);
                return false;
            }
            return true;
        }

    }
}
