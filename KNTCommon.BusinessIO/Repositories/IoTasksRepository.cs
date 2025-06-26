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
using System.Globalization;
using DocumentFormat.OpenXml.Vml.Office;
using System.IO.Compression;
using DocumentFormat.OpenXml.Office2021.DocumentTasks;

namespace KNTCommon.BusinessIO.Repositories
{
    public class IoTasksRepository
    {
        /// <summary>
        /// Tasks:
        ///  Archive (iotasks.IoTaskType = 1)
        ///  Restore (iotasks.IoTaskType = 2)
        ///  Export to Excel (iotasks.IoTaskType = 3)
        ///  DB Export Dump (iotasks.IoTaskType = 4)
        /// Task mode:
        ///  Cycling (iotasks.IoTaskMode = 1)
        ///  On demand (iotasks.IoTaskMode = 2)
        /// Priority:
        ///  A lower value indicates priority execution.
        /// iotasks.Par1:
        ///  path to export file (types 3, 4)
        ///  filepath + ' ' + zip means export to compressed zip (type 4)
        /// iotasks.TimeCriteria (separator between: ';'):
        ///  NextMonth: means time: <next-month yyyy-MM>-01 00:00:00
        ///  Day: means exactly day yyyy-MM-<day> HH:mm:ss
        ///  Time: means exactly time: yyyy-MM-dd <time HH:mm>:00
        ///  AddDays: set previous time +AddDays
        ///  AddHours: set previous time +AddHours
        ///  AddMinutes: set previous time +AddMinutes
        /// iotasks.ExecuteDateAndTime: next time to execute (+ iotasks.Status<100)
        /// iotasks.Status: <= 100 to execute (+ iotasks.ExecuteDateAndTime < Now) 
        /// </summary>

        private readonly IDbContextFactory<EdnKntControllerMysqlContext> Factory;
        private readonly IMapper AutoMapper;
        private readonly Tools t = new();

        public IoTasksRepository(IDbContextFactory<EdnKntControllerMysqlContext> factory, IMapper automapper)
        {
            Factory = factory;
            AutoMapper = automapper;
        }

        // get actual IO tasks: Priority > 0, Status < 0
        public IEnumerable<DTOs.IoTasksDTO> GetIoTasks()
        {
            var ret = new List<DTOs.IoTasksDTO>();
            try
            {
                using (var context = new EdnKntControllerMysqlContext())
                {
                    ret = AutoMapper.Map<List<DTOs.IoTasksDTO>>(context.IoTasks.Where(x => x.Priority > 0).OrderBy(x => x.Priority));
                }
            }
            catch (Exception ex)
            {
                t.LogEvent("KNTCommon.BusinessIO.Repositories.IoTasksRepository #1 " + ex.Message);
            }

            return ret;
        }

        // get task id by type and mode
        public int GetIoTaskIdByTypeMode(int type, int mode)
        {
            int id = 0;
            try
            {
                using (var context = new EdnKntControllerMysqlContext())
                {
                    var task = AutoMapper.Map<DTOs.IoTasksDTO>(context.IoTasks.Where(x => x.IoTaskType == type && x.IoTaskMode == mode).First());
                    id = task.IoTaskId;
                }
            }
            catch (Exception ex)
            {
                t.LogEvent("KNTCommon.BusinessIO.Repositories.IoTasksRepository #9 " + ex.Message);
            }

            return id;
        }

        // get IO tasks details except not existed table => Par5 = "none"
        public IEnumerable<IoTaskDetailsDTO> GetIoTaskDetails(int taskId, bool ignoreNone)
        {
            var ret = new List<IoTaskDetailsDTO>();
            try
            {
                using (var context = new EdnKntControllerMysqlContext())
                {
                    IEnumerable<IoTaskDetails> details = new List<IoTaskDetails>();
                    if(ignoreNone)
                        details = context.IoTaskDetails.Where(x => x.IoTaskId == taskId).OrderBy(x => x.TaskDetailOrder);
                    else
                        details = context.IoTaskDetails.Where(x => x.IoTaskId == taskId && x.Par5 != "none").OrderBy(x => x.TaskDetailOrder);

                    ret = AutoMapper.Map<List<IoTaskDetailsDTO>>(details);
                }
            }
            catch (Exception ex)
            {
                t.LogEvent("KNTCommon.BusinessIO.Repositories.IoTasksRepository #2 " + ex.Message);
            }

            return ret;
        }

        // get list of par1
        public List<string> GetIoTaskDetailsPar1(int taskId)
        {
            List<string> ret = new List<string>();
            try
            {
                using (var context = new EdnKntControllerMysqlContext())
                {
                    var details = context.IoTaskDetails.Where(x => x.IoTaskId == taskId).OrderBy(x => x.TaskDetailOrder);

                    // set to none if not exists
                    foreach (IoTaskDetails d in details)
                    {
                        if(d.Par1 != null)
                            ret.Add(d.Par1);
                    }
                }
            }
            catch (Exception ex)
            {
                t.LogEvent("KNTCommon.BusinessIO.Repositories.IoTasksRepository #10 " + ex.Message);
            }

            return ret;
        }

        /// <summary>
        /// set task info and task log info
        /// </summary>
        /// <param name="taskId"></param>
        /// <param name="str"></param>
        /// <param name="taskType"></param>
        /// <returns></returns>
        public bool IoTaskSetInfo(int taskId, string str, int taskType)
        {
            var ret = true;
            try
            {
                using (var context = new EdnKntControllerMysqlContext())
                {
                    if (taskId > 0)
                    {
                        var task = context.IoTasks.Where(x => x.IoTaskId == taskId).FirstOrDefault();
                        if (task != null)
                        {
                            task.Info = str;
                        }
                    }

                    if (taskType != Const.NONE)
                    {
                        var logEntry = new IoTaskLogs
                        {
                            IoTaskId = taskId,
                            IoTaskLogType = taskType,
                            Info = str,
                            DateAndTime = DateTime.Now
                        };
                        context.IoTaskLogs.Add(logEntry);
                    }

                    context.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                t.LogEvent("KNTCommon.BusinessIO.Repositories.IoTasksRepository #3 " + ex.Message);
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
                t.LogEvent("KNTCommon.BusinessIO.Repositories.IoTasksRepository #4 " + ex.Message);
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

                    // set time depend of criteria
                    if (task != null && task.TimeCriteria != null && task.TimeCriteria.Length > 0)
                    {
                        List<string> timeCriteria = task.TimeCriteria.Split(';').ToList();

                        if (task.ExecuteDateAndTime is not null)
                        {
                            DateTime dateTimeTmp = (DateTime)task.ExecuteDateAndTime;

                            foreach (string criteria in timeCriteria)
                            {
                                List<string> cArray = criteria.Split('=').ToList();
                                switch (cArray[0])
                                {
                                    case "NextMonth":
                                        dateTime = new DateTime(dateTimeTmp.Year, dateTimeTmp.Month, 1).AddMonths(1);
                                        dateTimeTmp = dateTime;
                                        break;
                                    case "Day":
                                        dateTime = new DateTime(dateTimeTmp.Year, dateTimeTmp.Month, Convert.ToInt32(cArray[1]),
                                            dateTimeTmp.Hour, dateTimeTmp.Minute, dateTimeTmp.Second);
                                        dateTimeTmp = dateTime;
                                        break;
                                    case "Time":
                                        List<string> hhmm = cArray[1].Split(':').ToList();
                                        dateTime = new DateTime(dateTimeTmp.Year, dateTimeTmp.Month, dateTimeTmp.Day,
                                            Convert.ToInt32(hhmm[0]), Convert.ToInt32(hhmm[1]), dateTimeTmp.Second);
                                        dateTimeTmp = dateTime;
                                        break;
                                    case "AddDays":
                                        dateTime = dateTimeTmp.AddDays(Convert.ToInt32(cArray[1]));
                                        dateTimeTmp = dateTime;
                                        break;
                                    case "AddHours":
                                        dateTime = dateTimeTmp.AddHours(Convert.ToInt32(cArray[1]));
                                        dateTimeTmp = dateTime;
                                        break;
                                    case "AddMinutes":
                                        dateTime = dateTimeTmp.AddMinutes(Convert.ToInt32(cArray[1]));
                                        dateTimeTmp = dateTime;
                                        break;
                                }
                            }
                        }
                    }

                    if (task != null)
                    {
                        task.ExecuteDateAndTime = dateTime;
                        context.SaveChanges();
                    }
                }
            }
            catch (Exception ex)
            {
                t.LogEvent("KNTCommon.BusinessIO.Repositories.IoTasksRepository #5 " + ex.Message);
            }
            return ret;
        }

        public bool IoTaskSetPar3(int taskId, int order, string val)
        {
            var ret = true;
            try
            {
                using (var context = new EdnKntControllerMysqlContext())
                {
                    var taskD = context.IoTaskDetails.Where(x => x.IoTaskId == taskId && x.TaskDetailOrder == order).FirstOrDefault();
                    if (taskD != null)
                    {
                        taskD.Par3 = val;
                        context.SaveChanges();
                    }
                }
            }
            catch (Exception ex)
            {
                t.LogEvent("KNTCommon.BusinessIO.Repositories.IoTasksRepository #8 " + ex.Message);
            }
            return ret;
        }

        public bool IoTaskSetPar5(int taskId, int order, string val)
        {
            var ret = true;
            try
            {
                using (var context = new EdnKntControllerMysqlContext())
                {
                    var taskD = context.IoTaskDetails.Where(x => x.IoTaskId == taskId && x.TaskDetailOrder == order).FirstOrDefault();
                    if (taskD != null)
                    {
                        taskD.Par5 = val;
                        context.SaveChanges();
                    }
                }
            }
            catch (Exception ex)
            {
                t.LogEvent("KNTCommon.BusinessIO.Repositories.IoTasksRepository #7 " + ex.Message);
            }
            return ret;
        }


        // move file to compressed zip
        public bool CompressedFileToZip(string filePath)
        {
            string zipPath = filePath.Split(new string[] { ".sql" }, StringSplitOptions.None)[0] + ".zip";

            try
            {
                using (FileStream zipToCreate = new(zipPath, FileMode.Create))
                {
                    using (ZipArchive archive = new(zipToCreate, ZipArchiveMode.Create))
                    {
                        archive.CreateEntryFromFile(filePath, Path.GetFileName(filePath));
                    }
                }
                File.Delete(filePath);
            }
            catch (Exception ex)
            {
                t.LogEvent("KNTCommon.BusinessIO.Repositories.IoTasksRepository #6 " + ex.Message);
            }
            return true;
        }

    }
}
