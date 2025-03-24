using AutoMapper;
using KNTCommon.Business.DTOs;
using KNTCommon.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySql.EntityFrameworkCore.Extensions;
using KNTToolsAndAccessories;
using System.ServiceProcess;

namespace KNTCommon.Business.Repositories
{
    public class ParametersRepository : IParametersRepository
    {
        private readonly IDbContextFactory<EdnKntControllerMysqlContext> ContextFactory;
        private readonly IMapper AutoMapper;
        private readonly Tools t = new();

        public ParametersRepository(IDbContextFactory<EdnKntControllerMysqlContext> factory, IMapper automapper)
        {
            ContextFactory = factory;
            AutoMapper = automapper;
        }

        public IEnumerable<ParameterDTO> GetParameters(string key)
        {
            var ret = new List<ParameterDTO>();
            try
            {
                using (var context = new EdnKntControllerMysqlContext())
                {
                    if (key == string.Empty)
                        ret = AutoMapper.Map<List<ParameterDTO>>(context.Parameters);
                    else
                        ret = AutoMapper.Map<List<ParameterDTO>>(context.Parameters.Where(x => x.ParName == key));
                    if (ret.Count == 0)
                    {
                        var exStr = "Parameters, no data found";
                        if (key != string.Empty)
                            exStr += $" ({key})";
                        throw new Exception(exStr);
                    }
                }
            }
            catch (Exception ex)
            {
                t.LogEvent("KNTCommon.Business.Repositories.ParametersRepository #1 " + ex.Message);
            }

            return ret;
        }

        public string GetParametersStr(string key)
        {
            string ret = string.Empty;
            try
            {
                using (var context = new EdnKntControllerMysqlContext())
                {
                    ret = context.Parameters.Where(x => x.ParName == key).First().ParValue ?? string.Empty;
                }
                if (ret is null)
                    ret = string.Empty;
            }
            catch (Exception ex)
            {
                t.LogEvent("KNTCommon.Business.Repositories.ParametersRepository #2 " + ex.Message);
            }

            return ret;
        }

        public bool UpdateParameters(string name, string value)
        {
            Parameter Parameters = new()
            {
                ParName = name,
                ParValue = value
            };

            try
            {
                using (var context = new EdnKntControllerMysqlContext())
                {
                    var par = context.Parameters.Where(x => x.ParName == Parameters.ParName).First();
                    if (par == null)
                        return false;
                    par.ParValue = Parameters.ParValue;
                    context.SaveChanges();
                }

            }
            catch (Exception ex)
            {
                t.LogEvent("KNTCommon.Business.Repositories.ParametersRepository #3 " + ex.Message);
                return false;
            }
            return true;
        }

        /// <summary>
        /// Get all services
        /// </summary>
        /// <returns></returns>
        public IEnumerable<ServiceControlDTO> GetServices()
        {
            var ret = new List<ServiceControlDTO>();
            try
            {
                using (var context = new EdnKntControllerMysqlContext())
                {
                    ret = AutoMapper.Map<List<ServiceControlDTO>>(context.ServiceControls).OrderBy(x => x.ServiceId).ToList();
                }
            }
            catch (Exception ex)
            {
                t.LogEvent("KNTCommon.Business.Repositories.ParametersRepository #4 " + ex.Message);
            }

            return ret;
        }

        public bool UpdateServiceStatus(string name, int status)
        {
            ServiceControl Service = new()
            {
                ServiceName = name,
                Status = status
            };

            try
            {
                using (var context = new EdnKntControllerMysqlContext())
                {
                    var par = context.ServiceControls.Where(x => x.ServiceName == Service.ServiceName).First();
                    if (par == null)
                        return false;
                    par.Status = Service.Status;
                    context.SaveChanges();
                }

            }
            catch (Exception ex)
            {
                t.LogEvent("KNTCommon.Business.Repositories.ParametersRepository #5 " + ex.Message);
                return false;
            }
            return true;
        }

        public int GetServiceStatus(string name)
        {
            int ret = -1;
            try
            {
                using (var context = new EdnKntControllerMysqlContext())
                {
                    ret = context.ServiceControls.Where(x => x.ServiceName == name).First().Status ?? -1;
                }
            }
            catch (Exception ex)
            {
                t.LogEvent("KNTCommon.Business.Repositories.ParametersRepository #6 " + ex.Message);
            }

            return ret;
        }


    }
}

