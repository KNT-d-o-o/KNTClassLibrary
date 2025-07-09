using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Management;
using System.IO;
using System.Threading;

namespace ConsoleAdminTools
{
    class Program
    {
        static void Main(string[] args)
        {
            const string logFile = "logConsoleAdminTools.txt";

            try
            {
                string function = args[0];
                string ip_address = args[1];
                string subnet_mask = args[2];
                string interfaceAlias = string.Empty;

                if (args.Length >= 4)
                    interfaceAlias = args[3];

                // set IP address and subnet mask for interface Ethernet
                if (function == "SetIP")
                {
                    ManagementClass objMC = new ManagementClass("Win32_NetworkAdapterConfiguration");
                    ManagementObjectCollection objMOC = objMC.GetInstances();

                    foreach (ManagementObject objMO in objMOC.Cast<ManagementObject>())
                    {
                        if ((bool)objMO["IPEnabled"])
                        {
                            try
                            {
                                ManagementBaseObject setIP;
                                ManagementBaseObject newIP =
                                    objMO.GetMethodParameters("EnableStatic");

                                newIP["IPAddress"] = new string[] { ip_address };
                                newIP["SubnetMask"] = new string[] { subnet_mask };

                                setIP = objMO.InvokeMethod("EnableStatic", newIP, null);

                                File.AppendAllText(logFile, $"{function} Successful: {ip_address} {subnet_mask}{Environment.NewLine}");
                            }
                            catch (Exception ex)
                            {
                                File.AppendAllText(logFile, $"{function} Error: {ex.Message}{Environment.NewLine}");
                                throw;
                            }
                        }
                    }
                }

                else if (function == "SetIPForAlias")
                {
                    // 1. Poišči DeviceID adapterja s podanim NetConnectionID (interfaceAlias)
                    int deviceId = -1;
                    var adapterSearcher = new ManagementObjectSearcher("SELECT * FROM Win32_NetworkAdapter WHERE NetConnectionID IS NOT NULL");
                    foreach (ManagementObject adapter in adapterSearcher.Get())
                    {
                        var netConnectionId = adapter["NetConnectionID"]?.ToString();
                        if (netConnectionId != null && netConnectionId.Equals(interfaceAlias, StringComparison.OrdinalIgnoreCase))
                        {
                            deviceId = Convert.ToInt32(adapter["DeviceID"]);
                            break;
                        }
                    }

                    if (deviceId == -1)
                    {
                        throw new Exception($"Adapter with name {interfaceAlias} not found.");
                    }

                    // 2. Poizveduj konfiguracijo z Index = deviceId
                    string query = $"SELECT * FROM Win32_NetworkAdapterConfiguration WHERE Index = {deviceId}";
                    var configSearcher = new ManagementObjectSearcher(query);
                    var configs = configSearcher.Get();

                    if (!configs.Cast<ManagementObject>().Any())
                    {
                        throw new Exception($"Configuration with Index={deviceId} not found.");
                    }

                    foreach (ManagementObject config in configs)
                    {
                        if ((bool)config["IPEnabled"])
                        {
                            // Nastavi statični IP in subnet masko
                            ManagementBaseObject newIP = config.GetMethodParameters("EnableStatic");
                            newIP["IPAddress"] = new string[] { ip_address };
                            newIP["SubnetMask"] = new string[] { subnet_mask };

                            var result = config.InvokeMethod("EnableStatic", newIP, null);

                            // Po potrebi preveri rezultat:
                            uint returnCode = (uint)result["ReturnValue"];
                            if (returnCode != 0)
                            {
                                // Napaka pri nastavitvi IP
                                File.AppendAllText(logFile, $"Error on setting IP: ReturnCode={returnCode}{Environment.NewLine}");
                            }
                            else
                            {
                                File.AppendAllText(logFile, $"SetIPForAlias successful: {interfaceAlias} {ip_address} {subnet_mask}{Environment.NewLine}");
                            }
                        }
                        else
                        {
                            File.AppendAllText(logFile, $"SetIPForAlias: Adapter {interfaceAlias} not IP-enabled.{Environment.NewLine}");
                        }
                    }
                }
            }
            catch (Exception ex) 
            {
                File.AppendAllText(logFile, $"ConsoleAdminTools Error: {ex.Message}{Environment.NewLine}");
            }

        }
    }
}
