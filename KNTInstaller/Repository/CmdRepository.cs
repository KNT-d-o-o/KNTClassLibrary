using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KNTInstaller.Repository
{
    public class CmdRepository
    {
        public string RunBatCommand(string command)
        {
            ProcessStartInfo psi = new ProcessStartInfo();
            //RunBatCommand("PowerShell -Command \"Start-Process PowerShell -ArgumentList '-ExecutionPolicy Bypass -File \\\"%~dp0IIS_create_SMM.ps1\\\"' -Verb RunAs\"");
            psi.UseShellExecute = false;
            psi.RedirectStandardOutput = true;
            psi.RedirectStandardError = true;
            psi.CreateNoWindow = true;
            psi.WorkingDirectory = @"C:\Windows\System32";
            psi.FileName = @"C:\Windows\System32\cmd.exe";
            psi.Verb = "runas";
            psi.Arguments = "/c " + command;

            using (Process process = new Process())
            {
                process.StartInfo = psi;

                process.Start();

                process.WaitForExit();

                string output = process.StandardOutput.ReadToEnd().Trim(); // NOTE output from script Example: Check_IsFirewallOpen_SMM.ps1
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

        public void RunInstall(string fileName)
        {
            try
            {
                fileName = "Install/" + fileName;

                var process = new Process();
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.RedirectStandardOutput = true;
                process.StartInfo.RedirectStandardError = true;
                process.StartInfo.CreateNoWindow = true;
                process.StartInfo.FileName = fileName;
                process.StartInfo.Arguments = "/silent";  // če inštalacija podpira tihi način
                process.Start();

                process.WaitForExit();

                int exitCode = process.ExitCode;

                string output = process.StandardOutput.ReadToEnd();
                string error = process.StandardError.ReadToEnd();

                if (process.ExitCode == 0)// NOTE Exit code 0, dont aways mean install, it is also uninstall, but in silent mode it just install
                    Console.WriteLine("Inštalacija uspešna.");
                else
                    throw new Exception("Inštalacija ni uspela.");
            }
            catch (Exception)
            {
                throw;
            }
        }


    }
}
