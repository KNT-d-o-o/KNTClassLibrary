using AutoMapper;
using KNTCommon.Business.DTOs;
using KNTCommon.Business.Interface;
using KNTCommon.Data.Models;
using KNTSMM.Data.Models;
using KNTToolsAndAccessories;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KNTCommon.Business.Repositories
{
    public class ModuleRepository : IModuleRepository
    {
        private readonly IMapper _autoMapper;
        private readonly Tools t = new();
        SecurityManager _securityManager;

        public ModuleRepository(IMapper automapper, SecurityManager securityManager)
        {
            _autoMapper = automapper;
            _securityManager = securityManager;
        }

        public async Task<List<CL_ModuleDTO>> GetModulesAsync()
        {
            using var context = new EdnKntControllerMysqlContext();
            var results = await _autoMapper.ProjectTo<CL_ModuleDTO>(context.CL_Module).ToListAsync();

            var functionalitys = await _autoMapper.ProjectTo<CL_ModuleFunctionalityDTO>(context.CL_ModuleFunctionality).ToListAsync();

            foreach(var result in results)
            {
                result.ModuleFunctionalityDTO = functionalitys.Where(x => x.ModuleId == result.ModuleId).ToList();
            }

            return results;
        }

        public async Task<bool> SetModuleVisibility(List<CL_ModuleDTO> moduleDTOs)
        {
            using var context = new EdnKntControllerMysqlContext();
            var modules = await context.CL_Module.ToListAsync();
            var moduleFunctionalitys = await context.CL_ModuleFunctionality.ToListAsync();

            foreach (var module in modules)
            {
                var tmpModule = moduleDTOs.Where(x => x.ModuleId == module.ModuleId).First();
                module.Enabled = tmpModule.Enabled;
            }

            foreach (var moduleFunctionality in moduleFunctionalitys)
            {
                var tmpModule = moduleDTOs.Where(x => x.ModuleId == moduleFunctionality.ModuleId).First();
                var tmpFunctionality = tmpModule.ModuleFunctionalityDTO.Where(x => x.FunctionalityId == moduleFunctionality.FunctionalityId).First();
                moduleFunctionality.Enabled = tmpFunctionality.Enabled;
            }

            await context.SaveChangesAsync();
            _securityManager.Reload();

            return true;
        }



    }
}
