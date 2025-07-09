using KNTCommon.Business.Models;
using KNTToolsAndAccessories;
using System;
using System.Diagnostics;
using System.Management.Automation;
using System.Net.NetworkInformation;
using System.ServiceProcess;

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

        /// <summary>
        /// Set system date and time
        /// </summary>
        /// <param name="newDateTime"></param>
        /// <exception cref="FileNotFoundException"></exception>
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

        public List<NetworkInterfaceInfo> GetNetworkInterfaces()
        {
            var list = new List<NetworkInterfaceInfo>();

            foreach (var ni in NetworkInterface.GetAllNetworkInterfaces())
            {
                if (ni.OperationalStatus != OperationalStatus.Up)
                    continue;

                var ipProps = ni.GetIPProperties();
                foreach (var addr in ipProps.UnicastAddresses)
                {
                    if (addr.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                    {
                        string ip = addr.Address.ToString();

                        // not include Loopback
                        if (ni.Name.Contains("Loopback") || ip == "127.0.0.1")
                            continue;

                        list.Add(new NetworkInterfaceInfo
                        {
                            Name = ni.Name,
                            IpAddress = ip,
                            SubnetMask = addr.IPv4Mask?.ToString() ?? "255.255.255.0"
                        });
                    }
                }
            }

            return list;
        }

        public bool SetIpAddress(string interfaceAlias, string ipAddress, string netmask)
        {
            try
            {
                string exePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "KNTConsoleAdminTools.exe");
                string args = $"SetIPForAlias {ipAddress} {netmask} \"{interfaceAlias}\"";
                var startInfo = new ProcessStartInfo
                {
                    FileName = exePath,
                    Arguments = args,
                    UseShellExecute = true,
                    Verb = "runas",
                    WorkingDirectory = Path.GetDirectoryName(exePath)
                };
                var process = Process.Start(startInfo);

                if (process == null)
                    throw new Exception("Process could not be created.");

                process.WaitForExit();
                Thread.Sleep(1000);

                var actualIp = GetIpForInterface(interfaceAlias);
                if(actualIp != ipAddress)
                    throw new Exception($"IP address not changed. Expected: {ipAddress}, Found: {actualIp ?? "null"}");
            }
            catch (Exception ex)
            {
                t.LogEvent("KNTCommon.Business.Scripts #4 " + ex.Message);
                return false;
            }
            return true;
        }

        public string? GetIpForInterface(string interfaceAlias)
        {
            foreach (var nic in NetworkInterface.GetAllNetworkInterfaces())
            {
                if (nic.Name == interfaceAlias || nic.Description.Contains(interfaceAlias))
                {
                    var ipProps = nic.GetIPProperties();
                    foreach (var addr in ipProps.UnicastAddresses)
                    {
                        if (addr.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                        {
                            return addr.Address.ToString();
                        }
                    }
                }
            }
            return null;
        }

    }
}
