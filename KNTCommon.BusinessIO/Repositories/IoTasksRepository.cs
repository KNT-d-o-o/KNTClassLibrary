using AutoMapper;
using KNTCommon.BusinessIO.DTOs;
using KNTCommon.Data.Models;
using KNTToolsAndAccessories;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;
using MySql.Data.MySqlClient;
using System.Data.SqlTypes;
using System.Numerics;
using Org.BouncyCastle.Ocsp;
using System.Data;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Mysqlx.Crud;
using MySqlX.XDevAPI.Common;
using System.Transactions;

namespace KNTCommon.BusinessIO.Repositories
{
    public class IoTasksRepository
    {
        private readonly IDbContextFactory<EdnKntControllerMysqlContext> Factory;
        private readonly IMapper AutoMapper;
        private readonly Tools t = new();

        public IoTasksRepository(IDbContextFactory<EdnKntControllerMysqlContext> factory, IMapper automapper)
        {
            Factory = factory;
            AutoMapper = automapper;
        }

        public IEnumerable<IoTasksDTO> GetIoTasks()
        {
            var ret = new List<IoTasksDTO>();
            try
            {
                using (var context = new EdnKntControllerMysqlContext())
                {
                    ret = AutoMapper.Map<List<IoTasksDTO>>(context.IoTasks.Where(x => x.Priority > 0 && x.Status < 0).OrderBy(x => x.Priority));
                }
            }
            catch (Exception ex)
            {
                t.LogEvent("KNTLeakTester.BusinessIO.Repositories.IoTasksRepository #1 " + ex.Message);
            }

            return ret;
        }

        public IEnumerable<IoTaskDetailsDTO> GetIoTaskDetails(int taskId)
        {
            var ret = new List<IoTaskDetailsDTO>();
            try
            {
                using (var context = new EdnKntControllerMysqlContext())
                {
                    var details = context.IoTaskDetails.Where(x => x.IoTaskId == taskId).OrderBy(x => x.TaskDetailOrder);

                    // set to none if not exists
                    foreach (IoTaskDetails t in details)
                    {
                        if (t.Par1 != null)
                        {
                            if (!TableExists(t.Par1))
                            {
                                t.Par5 = "none";
                            }
                        }
                    }
                    context.SaveChanges();

                    ret = AutoMapper.Map<List<IoTaskDetailsDTO>>(details);
                }
            }
            catch (Exception ex)
            {
                t.LogEvent("KNTLeakTester.BusinessIO.Repositories.IoTasksRepository #2 " + ex.Message);
            }

            return ret;
        }

        bool TableExists(string tableName)
        {
            try
            {
                using (var context = new EdnKntControllerMysqlContext())
                {
                    context.Database.ExecuteSqlRaw("SELECT 1 FROM " + tableName + " LIMIT 1");
                    return true;
                }
            }
            catch
            {
                return false;
            }
        }

        public bool IoTaskSetInfo(int taskId, string str)
        {
            var ret = true;
            try
            {
                using (var context = new EdnKntControllerMysqlContext())
                {
                    var task = context.IoTasks.Where(x => x.IoTaskId == taskId).FirstOrDefault();
                    if (task != null)
                    {
                        task.Info = str;
                        context.SaveChanges();
                    }
                }
            }
            catch (Exception ex)
            {
                t.LogEvent("KNTLeakTester.BusinessIO.Repositories.IoTasksRepository #3 " + ex.Message);
            }

            return ret;
        }

        public bool IoTaskSetStatus(int taskId, int status)
        {
            var ret = true;
            try
            {
                using (var context = new EdnKntControllerMysqlContext())
                {
                    var task = context.IoTasks.Where(x => x.IoTaskId == taskId).FirstOrDefault();
                    if (task != null)
                    {
                        task.Status = status;
                        context.SaveChanges();
                    }
                }
            }
            catch (Exception ex)
            {
                t.LogEvent("KNTLeakTester.BusinessIO.Repositories.IoTasksRepository #4 " + ex.Message);
            }
            return ret;
        }

        public bool IoTaskSetExecuteDateAndTime(int taskId, DateTime dateTime)
        {
            var ret = true;
            try
            {
                using (var context = new EdnKntControllerMysqlContext())
                {
                    var task = context.IoTasks.Where(x => x.IoTaskId == taskId).FirstOrDefault();
                    if (task != null)
                    {
                        task.ExecuteDateAndTime = dateTime;
                        context.SaveChanges();
                    }
                }
            }
            catch (Exception ex)
            {
                t.LogEvent("KNTLeakTester.BusinessIO.Repositories.IoTasksRepository #5 " + ex.Message);
            }
            return ret;
        }

    }
}
