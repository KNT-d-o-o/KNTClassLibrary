using KNTInstaller.Model;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Management.Automation;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace KNTInstaller.Repository
{
    public class InstallRepository
    {        
        PowerShellRepository _powerShellRepository;
        CmdRepository _cmdRepository;
        ServiceRepository _serviceRepository;
        DirectoryRepository _directoryRepository;
        IISRepository _iISRepository;

        public InstallRepository()
        {
            _powerShellRepository = new PowerShellRepository();
            _cmdRepository = new CmdRepository();
            _directoryRepository = new DirectoryRepository(_powerShellRepository);
            _iISRepository = new IISRepository(_powerShellRepository);
            _serviceRepository = new ServiceRepository(_cmdRepository, _directoryRepository);            
        }

        public string IIS()
        {
            // TODO
            // test windows_features__internet_info_services.ps1
            // C:\Program Files (x86)\IIS\Microsoft Web Deploy V3\
            // Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\InetStp", "VersionString", null) != null;
            _iISRepository.Install();

            return $"IIS was NOT installed - TODO";
        }

        public string DotNetHosting()
        {
            try
            {
                _cmdRepository.RunInstall("dotnet-hosting-8.0.4-win.exe");
                return $"Dot Net Hosting was installed";
            }
            catch (Exception ex)
            {
                return $"Dot Net Hosting was NOT installed. Error ex: {ex.Message}, ex: {ex.StackTrace}";
            }            
        }               

        public string Edge()
        {
            try
            {
                _directoryRepository.ExtractZip("Edge.zip", @"C:\Programs\Edge");

                _directoryRepository.Unrestrict();                

                return $"Edge was installed";
            }
            catch(Exception ex)
            {
                return $"Edge was NOT installed. Error ex: {ex.Message}, ex: {ex.StackTrace}";
            }            
        }

        public string SMM_IIS_Stop()
        {
            try
            {                
                _iISRepository.Stop();
                return $"SMM IIS stop";
            }
            catch (Exception ex)
            {
                return $"SMM IIS was not stoped: {ex.Message}, ex: {ex.StackTrace}";
            }            
        }

        public string SMM_IIS_Start()
        {
            try
            {
                _iISRepository.Start();                
                return $"SMM IIS start";
            }
            catch (Exception ex)
            {
                return $"SMM IIS was not started: {ex.Message}, ex: {ex.StackTrace}";
            }
        }

        public string SMM()
        {
            try
            {
                _directoryRepository.ExtractZip("SMM.zip", @"C:\Programs\SMM");                
                return $"SMM was installed";
            }
            catch (Exception ex)
            {
                return $"SMM was NOT installed. Error ex: {ex.Message}, ex: {ex.StackTrace}";
            }            
        }
        public string SMM_OpenFirewallPort()
        {
            try
            {
                if(IsSMMFirewallOpen())
                    return $"SMM Open Firewall Port was already created skipping";

                _powerShellRepository.RunPowerShell("Open_Firewall_SMM.ps1");
                return $"SMM Open Firewall Port was created";
            }
            catch (Exception ex)
            {
                return $"SMM Open Firewall was NOT created. Error ex: {ex.Message}, ex: {ex.StackTrace}";
            }
        }

        public string SMM_IISCreateApp()
        {
            try
            {
                _iISRepository.CreateApp();                
                return $"IIS Create App was created";
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("Destination element already exists, please use "))
                    return $"IIS Create App was already created, skipping";

                return $"IIS Create App was NOT created. Error ex: {ex.Message}, ex: {ex.StackTrace}";
            }            
        }

        public string Services_IO()
        {
            return _serviceRepository.InstallService("KNTCommon.BusinessIO", @"C:\Programs\ServiceIO\KNTCommon.BusinessIO.Service.exe", @"C:\Programs\ServiceIO");
        }



        public bool IsIisInstalled() => Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\InetStp", "VersionString", null) != null;
        public bool IsDotnetHostingInstalled() => Directory.Exists(@"C:\Program Files (x86)\IIS\Asp.Net Core Module");
        public bool IsSMMInstalled() => Directory.Exists(@"C:\Programs\SMM");
        public bool IsEdgeInstalled() => Directory.Exists(@"C:\Programs\Edge");
        public bool IsSMMFirewallOpen() => _powerShellRepository.RunPowerShellIsOk("Check_IsFirewallOpen_SMM.ps1");
        public bool IsSMMIisApp() => _powerShellRepository.RunPowerShellIsOk("Check_IsIIS_App.ps1");
        public bool IsServiceIo() => _serviceRepository.ServiceExists("KNTCommon.BusinessIO");

    }
}
