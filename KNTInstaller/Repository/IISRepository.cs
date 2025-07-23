using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KNTInstaller.Repository
{
    public class IISRepository
    {
        PowerShellRepository _powerShellRepository;

        public IISRepository(PowerShellRepository powerShellRepository)
        {
            _powerShellRepository = powerShellRepository;
        }

        public void Install()
        {

        }

        public void CreateApp()
        {
            _powerShellRepository.RunPowerShell("IIS_create_SMM.ps1");
        }

        public void Stop()
        {
            _powerShellRepository.RunPowerShell("Set_IIS_Stop.ps1");
        }

        public void Start()
        {
            _powerShellRepository.RunPowerShell("Set_IIS_Start.ps1");
        }


    }
}
