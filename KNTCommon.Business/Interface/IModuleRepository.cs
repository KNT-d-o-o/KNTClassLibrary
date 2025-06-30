using KNTCommon.Business.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KNTCommon.Business.Interface
{
    public interface IModuleRepository
    {
        Task<List<CL_ModuleDTO>> GetModulesAsync();

        Task<bool> SetModuleVisibility(List<CL_ModuleDTO> list);
    }
}
