using KNTCommon.Business.DTOs;
using KNTCommon.Business.Interface;
using KNTCommon.Data.Models;
using KNTSMM.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KNTCommon.Business.Repositories
{
    // NOTE module num must be same as ModuleId in db
    public enum ModuleType
    {
        Recipes = 1,
        Transactions,
        LeakTester,
        LeakTestingHelium,
        ForcePath,
    }

    // NOTE Functionality num must be same as FunctionalityId in db
    public enum FunctionalityType
    {
        Counter = 1,
    }

    public class SecurityManager
    {
        List<CL_Module> _modules { get; set; } = new();
        List<CL_ModuleFunctionality> _functionalitys { get; set; }  = new();

        public SecurityManager()
        {
            Reload();
        }

        public void Reload()
        {
            using var context = new EdnKntControllerMysqlContext();

            _modules = context.CL_Module.ToList();
            _functionalitys = context.CL_ModuleFunctionality.ToList();
        }

        public bool IsModuleEnabled(ModuleType moduleType) {
            return _modules.Where(x => x.ModuleId == (int)moduleType).Select(x => x.Enabled).FirstOrDefault();
        }

        public bool IsFunctionalityEnabled(FunctionalityType functionalityType) {
            return _functionalitys.Where(x => x.FunctionalityId == (int)functionalityType).Select(x => x.Enabled).FirstOrDefault(); ;
        }
    }
}
