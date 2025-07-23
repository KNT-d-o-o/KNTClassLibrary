using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

namespace KNTInstaller.Repository
{
    public class ServiceRepository
    {
        CmdRepository _cmdRepository;
        DirectoryRepository _directoryRepository;

        public ServiceRepository(CmdRepository cmdRepository, DirectoryRepository directoryRepository)
        {
            _cmdRepository = cmdRepository;
            _directoryRepository = directoryRepository;            
        }

        public bool ServiceExists(string serviceName)
        {
            var output = _cmdRepository.RunBatCommand($"sc query {serviceName}");
            return !output.Contains("The specified service does not exist as an installed service.");
        }


        void Stop(string serviceName)
        {
            var output = _cmdRepository.RunBatCommand($"SC STOP {serviceName}");
        }

        void Start(string serviceName)
        {
            var output = _cmdRepository.RunBatCommand($"SC START {serviceName}");

            if (!output.Contains("START_PENDING"))
                throw new Exception(output);
        }

        void CreateService(string serviceName, string installPath)
        {
            var output = _cmdRepository.RunBatCommand($"SC CREATE {serviceName} binPath=\"{installPath}\" start=auto");
        }

        //  sc create KNTCommon.BusinessIO binPath= "D:\RAZVOJ\PUBLISH_IO\KNTCommon.BusinessIO.Service.exe"
        //  
        //  Install service:
        //  run cmd as Administrator:
        //  
        //  Stop service command: SC STOP KNTCommon.BusinessIO
        //  Delete service command: SC DELETE KNTCommon.BusinessIO
        //  
        //  Create service command:
        //  Debug:
        //  SC CREATE KNTCommon.BusinessIO binPath= "D:\RAZVOJ\PUBLISH_IO\KNTCommon.BusinessIO.Service.exe"
        //  Release:
        //  SC CREATE KNTCommon.BusinessIO binPath="C:\KNTLeakTester\ServiceIO\KNTCommon.BusinessIO.Service.exe" start=auto
        //  SC CREATE KNTCommon.BusinessIO binPath="C:\PROGRAMS\ServiceIO\KNTCommon.BusinessIO.Service.exe" start=auto
        //  
        //  Start service command: SC START KNTCommon.BusinessIO
        public string InstallService(string serviceName, string installPath, string extractPath)
        {
            // upgrade
            if (ServiceExists(serviceName))
            {
                Stop(serviceName);

                _directoryRepository.ExtractZip("ServiceIO.zip", extractPath);

                Start(serviceName);

                return $"Service {serviceName} alredy exists upgrading";
            }
            else
            {
                // new install
                _directoryRepository.ExtractZip("ServiceIO.zip", extractPath);

                CreateService(serviceName, installPath);

                Start(serviceName);

                return $"Service {serviceName} was installed";
            }
        }



    }
}
