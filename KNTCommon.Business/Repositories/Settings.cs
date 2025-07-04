using AutoMapper;
using KNTCommon.Business.DTOs;
using KNTCommon.Business.Extension;
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
    public enum SettingType
    {
        Panels_Counter_Visibility = 1,
        Panels_Counter_Selected_Tab = 2,
    }

    public enum Panels_Counter_VisibilityType
    {
        All = 0,
        Active
    }

    public enum Panels_Counter_Selected_TabType
    {
        Items = 0,
        Program
    }

    public class Settings 
    {
        private readonly IMapper _autoMapper;
        private readonly Tools t = new();
        List<APP_Setting> _appSettings = new();        

        public Settings(IMapper automapper)
        {
            _autoMapper = automapper;
            Reload();
        }

        public void Reload()
        {
            using var context = new EdnKntControllerMysqlContext();
            _appSettings = context.APP_Setting.ToList();
        }

        public T GetValue<T>(SettingType settingType)
        {
            var settingName = Enum.GetName(typeof(SettingType), settingType);
            var value = _appSettings.Where(x => x.SettingKey == settingName).Select(x => x.SettingValue).First();
            return value.GetValue<T>();
        }
    }
}
