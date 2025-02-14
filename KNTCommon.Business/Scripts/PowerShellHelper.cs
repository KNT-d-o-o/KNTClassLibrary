using System.Diagnostics;

namespace KNTCommon.Business.Scripts
{
    public class PowerShellHelper
    {
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
                // Log or handle the exception
                Console.WriteLine($"Exception: {ex.Message}");
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
                // Log or handle the exception
                Console.WriteLine($"Exception: {ex.Message}");
            }
        }

        /// <summary>
        /// Start, stop restart service
        /// </summary>
        /// <param name="serviceName"></param>
        /// <param name="action"></param>
        public bool StartStopService(string serviceName, string action)
        {
            ProcessStartInfo psi = new ProcessStartInfo
            {
                FileName = "cmd.exe",
                Arguments = $"/c sc {action} \"{serviceName}\"",
                Verb = "runas",  // Povzdigne pravice (zahteva UAC)
                UseShellExecute = true
            };

            try
            {
                Process.Start(psi);
             }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
                return false;
            }
            return true;
        }

    }
}
