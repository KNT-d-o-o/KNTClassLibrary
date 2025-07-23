using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KNTInstaller.Repository
{
    public class PowerShellRepository
    {
        public string RunPowerShell(string fileName)
        {
            fileName = "Bat/" + fileName;

            ProcessStartInfo psi = new ProcessStartInfo();
            psi.FileName = "powershell.exe";
            psi.Arguments = $" -ExecutionPolicy Bypass -File \"{fileName}\" -Verb RunAs";
            //RunBatCommand("PowerShell -Command \"Start-Process PowerShell -ArgumentList '-ExecutionPolicy Bypass -File \\\"%~dp0IIS_create_SMM.ps1\\\"' -Verb RunAs\"");
            psi.UseShellExecute = false;
            psi.RedirectStandardOutput = true;
            psi.RedirectStandardError = true;
            psi.CreateNoWindow = true;

            using (Process process = new Process())
            {
                process.StartInfo = psi;

                process.Start();

                process.WaitForExit();

                string output = process.StandardOutput.ReadToEnd().Trim(); 
                string error = process.StandardError.ReadToEnd().Trim();
                int code = process.ExitCode;

                Console.WriteLine("IZHOD:");
                Console.WriteLine(output);

                if (!string.IsNullOrEmpty(error))
                {
                    Console.WriteLine("NAPAKA:");
                    Console.WriteLine(error);
                    throw new Exception(error);
                }

                return output;
            }
        }

        public bool RunPowerShellIsOk(string fileName)
        {
            var output = RunPowerShell(fileName); // NOTE output from script Example: Check_IsFirewallOpen_SMM.ps1
            return output == "OK";
        }


    }
}
