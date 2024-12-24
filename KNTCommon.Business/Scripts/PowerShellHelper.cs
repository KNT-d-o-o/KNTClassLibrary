using System.Diagnostics;

public class PowerShellHelper
{
    public static void ExecuteRestartScript(bool startAgain)
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
}
