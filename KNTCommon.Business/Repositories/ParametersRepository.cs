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
                t.LogEvent("KNTLeakTester.Business.Repositories.ParametersRepository #1 " + ex.Message);
            }

            return ret;
        }

        public string GetParametersStr(string key)
        {
            string? ret = string.Empty;
            try
            {
                using (var context = new EdnKntControllerMysqlContext())
                {
                    ret = context.Parameters.Where(x => x.ParName == key).First().ParValue;
                }
            }
            catch (Exception ex)
            {
                t.LogEvent("KNTLeakTester.Business.Repositories.ParametersRepository #2 " + ex.Message);
            }

            if (ret is null)
                ret = string.Empty;
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
                t.LogEvent("KNTLeakTester.Business.Repositories.ParametersRepository #3 " + ex.Message);
                return false;
            }
            return true;
        }

    }
}

