using System;
using System.Management;
using System.Linq;

namespace KNTConsoleAdminTools
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length < 4)
            {
                Console.WriteLine("Usage: KNTConsoleAdminTools.exe SetIP <ETHERNET_NAME> <IP_ADDRESS> <SUBNET_MASK>");
                return;
            }

            string function = args[0];
            string interfaceName = args[1];
            string ip_address = args[2];
            string subnet_mask = args[3];

            if (function == "SetIP")
            {
                try
                {
                    if (OperatingSystem.IsWindows())
                    {
                        var objMC = new ManagementClass("Win32_NetworkAdapterConfiguration");
                        var objMOC = objMC.GetInstances();

                        foreach (ManagementObject objMO in objMOC.Cast<ManagementObject>())
                        {
                            bool ipEnabled = (bool)objMO["IPEnabled"];
                            string? netConnId = objMO["NetConnectionID"] as string;

                            if (ipEnabled && !string.IsNullOrEmpty(netConnId) && netConnId.Equals(interfaceName, StringComparison.OrdinalIgnoreCase))
                            {
                                var newIP = objMO.GetMethodParameters("EnableStatic");
                                newIP["IPAddress"] = new string[] { ip_address };
                                newIP["SubnetMask"] = new string[] { subnet_mask };

                                objMO.InvokeMethod("EnableStatic", newIP, null);
                                Console.WriteLine($"IP Set: {ip_address} / {subnet_mask}");
                                return;
                            }
                        }
                    }

                    Console.WriteLine("No enabled network adapter found.");
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error: " + ex.Message);
                }
            }
            else
            {
                Console.WriteLine("Unknown function.");
            }
        }
    }
}
