using System;
using System.ServiceProcess;
using KNTCommon.Business;

namespace KNTCommon.Blazor.Services
{
    public class WindowsServiceHelper
    {
        public string GetServiceStatus(string serviceName)
        {
            if (!OperatingSystem.IsWindows())
                return "Unsupported OS";

            try
            {
                using ServiceController sc = new ServiceController(serviceName);
                return sc.Status.ToString();
            }
            catch (InvalidOperationException)
            {
                return "Error";
            }
            catch (Exception ex)
            {
                return $"Error: {ex.Message}";
            }
        }

    }
}
