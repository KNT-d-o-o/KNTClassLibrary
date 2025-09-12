using AutoMapper;
using KNTCommon.Business.Interface;
using KNTCommon.Business.Models;
using KNTCommon.Business.DTOs;

using KNTCommon.Data.Models;
using KNTToolsAndAccessories;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;
using Clifton.Lib;
using KNTCommon.Business.Repositories;
using KNTSMM.Data.Models;
using KNTCommon.Business.Extension;
using Org.BouncyCastle.Asn1.Cms;
using static Org.BouncyCastle.Crypto.Engines.SM2Engine;

namespace KNTSMM.Business.Repositories
{
    public class MaintenanceRepository : IMaintenanceRepository
    {
        private readonly IMapper _autoMapper;
        private readonly Localization _localization;
        private readonly Tools t = new();

        public MaintenanceRepository(IMapper automapper, Localization localization)
        {
            _autoMapper = automapper;
            _localization = localization;
        }

        public async Task<List<ParameterDTO>> GetArchiveParameters()
        {
            using var context = new EdnKntControllerMysqlContext();
            
            var parameterDTO = await _autoMapper.ProjectTo<ParameterDTO>(context.Parameters.Where(x => new List<string>() { "archiveDays", "archiveStep" }.Contains(x.ParName!) )).ToListAsync();

            return parameterDTO;
        }
        public async Task<List<IoTasksDTO>> GetIoTasksAsync(int loggedPower)
        {
            using var context = new EdnKntControllerMysqlContext();

            // TODO use privileges, SharedContainerCommon.LoggedPower is temp solution            
            var ioTasksDTOs = await _autoMapper.ProjectTo<IoTasksDTO>(context.IoTasks.Where(x => (loggedPower==1 || new List<int>() { 1, 3, 5 }.Contains(x.IoTaskId)))).ToListAsync();
            
            foreach(var ioTasksDTO in ioTasksDTOs)
                await ExpandIoTasksDTO(ioTasksDTO);            

            return ioTasksDTOs;
        }

        // should be obsolite
        public async Task<IoTasksDTO> GetIoTaskAsync(int ioTaskId)
        {
            using var context = new EdnKntControllerMysqlContext();
            var ioTasksDTO = await _autoMapper.ProjectTo<IoTasksDTO>(context.IoTasks.Where(x => x.IoTaskId == ioTaskId)).FirstOrDefaultAsync();
            await ExpandIoTasksDTO(ioTasksDTO);
            return ioTasksDTO;
        }

        public async Task<IoTasksDTO> GetIoTaskAsync(int ioTaskType, int ioTaskMode)
        {
            using var context = new EdnKntControllerMysqlContext();
            var ioTasksDTO = await _autoMapper.ProjectTo<IoTasksDTO>(context.IoTasks.Where(x => x.IoTaskType == ioTaskType && x.IoTaskMode == ioTaskMode)).FirstOrDefaultAsync();
            await ExpandIoTasksDTO(ioTasksDTO);
            

            return ioTasksDTO;
        }

        public async Task ExpandIoTasksDTO(IoTasksDTO ioTasksDTO)
        {
            if (string.IsNullOrEmpty(ioTasksDTO.TimeCriteria))
                return;

            var dbSettings = $"{ioTasksDTO.TimeCriteria}|{(ioTasksDTO.ExecuteDateAndTime ?? DateTime.Now).ToMysqlFormat()}";

            // TODO is this wrong? path should be edit always
            //if (ioTasksDTO.IoTaskMode == 3)
            //    dbSettings += $"|{ioTasksDTO.Par1}";

            string[] parts = dbSettings.Split('|');

            if (!(parts.Length == 2 || parts.Length == 3)) throw new Exception("Invalid input format");

            string firstPart = parts[0];
            string secondPart = parts[1];            

            ioTasksDTO.TimeCriteriaModel.AutomaticArchiving = !firstPart.Contains("disabled");
            ioTasksDTO.TimeCriteriaModel.NextMonth = firstPart.StartsWith("NextMonth");

            string[] options = firstPart.Split(';');

            if (!string.IsNullOrEmpty(ioTasksDTO.Par1))
            {
                string[] paz = ioTasksDTO.Par1.Split(';');
                if (paz.Length > 1 && paz[1] == "zip")
                    ioTasksDTO.TimeCriteriaModel.SaveAsZip = true;

                ioTasksDTO.TimeCriteriaModel.ExportLocation = paz[0];
            }                

            if (ioTasksDTO.TimeCriteriaModel.NextMonth)
            {
                ioTasksDTO.TimeCriteriaModel.ArchiveMode = (int)ArchiveMode.SetDate;

                foreach (var option in options)
                {
                    if (option.StartsWith("Day="))
                        ioTasksDTO.TimeCriteriaModel.EveryMonthOnDay = int.Parse(option.Split('=')[1]);
                    if (option.StartsWith("Time="))
                    {
                        string[] partsTime = option.Split('=');
                        string timePart = partsTime[1].Trim();
                        DateTime time = DateTime.ParseExact(timePart, "HH:mm", CultureInfo.InvariantCulture);
                        ioTasksDTO.TimeCriteriaModel.EveryMonthOnHour = new DateTime(1850, 1, 1, time.Hour, time.Minute, 0);
                    }
                }
            } else
            {
                decimal.TryParse(options[0].Split('=')[1], out decimal tempTimeValue);

                if (options[0].StartsWith("AddMinutes="))
                {
                    ioTasksDTO.TimeCriteriaModel.AddMinutes = tempTimeValue;
                    ioTasksDTO.TimeCriteriaModel.ArchiveIntervalType = (int)ArchiveIntervalType.Minute;
                }                    
                else if (options[0].StartsWith("AddHours="))
                {
                    ioTasksDTO.TimeCriteriaModel.AddHours = tempTimeValue;
                    ioTasksDTO.TimeCriteriaModel.ArchiveIntervalType = (int)ArchiveIntervalType.Hour;
                }                    
                else if (options[0].StartsWith("AddDays="))
                {
                    ioTasksDTO.TimeCriteriaModel.AddDays = tempTimeValue;
                    ioTasksDTO.TimeCriteriaModel.ArchiveIntervalType = (int)ArchiveIntervalType.Day;                    
                }

                ioTasksDTO.TimeCriteriaModel.ArchiveIntervalTypeAmount = tempTimeValue;
                ioTasksDTO.TimeCriteriaModel.ArchiveMode = (int)ArchiveMode.Interval;

                foreach (var option in options)
                {
                    if (option.StartsWith("Time="))
                    {
                        string[] partsTime = option.Split('=');
                        string timePart = partsTime[1].Trim();
                        DateTime time = DateTime.ParseExact(timePart, "HH:mm", CultureInfo.InvariantCulture);
                        ioTasksDTO.TimeCriteriaModel.ArchiveIntervalSelectedHour = new DateTime(1850, 1, 1, time.Hour, time.Minute, 0);
                    }
                }

                ioTasksDTO.TimeCriteriaModel.ArchiveIntervalStartDate = DateTime.ParseExact(secondPart.Trim(), "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);

                //if (parts.Length >= 3)
                //    ioTasksDTO.TimeCriteriaModel.ExportLocation = parts[2];

                // default values
                if(ioTasksDTO.TimeCriteriaModel.ArchiveIntervalType is null)
                {
                    ioTasksDTO.TimeCriteriaModel.ArchiveIntervalType = (int)ArchiveIntervalType.Day;
                    ioTasksDTO.TimeCriteriaModel.AddDays = 1;
                }
            }

            await SetAdditionalData(ioTasksDTO);
        }

        void CombineIoTasksDTO(IoTasksDTO ioTasksDTO)
        {
            // reset
            //ioTasksDTO.Par1 = "";

            /*
            if (mode == 0)
            {
                increment = (int)numericUpDownArchiveSetDate.Value;
                start = dateTimePickerArchiveSetDate.Value;
            }
            else if (mode == 1)
            {
                increment = (int)numericUpDownArchiveInterval.Value;
                start = dateTimePickerArchiveInterval.Value.Date + dateTimePickerArchiveSetDateInt.Value.TimeOfDay;
            }


            */

            if (ioTasksDTO.TimeCriteriaModel.ArchiveMode == (int)ArchiveMode.SetDate)
            {
                var today = DateTime.Now;
                if (today.Day > ioTasksDTO.TimeCriteriaModel.EveryMonthOnDay)
                    today = today.AddMonths(1);

                var maxDay = DateTime.DaysInMonth(today.Year, today.Month);
                var validDay = Math.Min(ioTasksDTO.TimeCriteriaModel.EveryMonthOnDay!.Value, maxDay); // Use the closest valid day

                var runDate = new DateTime(today.Year, today.Month, validDay, ioTasksDTO.TimeCriteriaModel.EveryMonthOnHour.Value.Hour, ioTasksDTO.TimeCriteriaModel.EveryMonthOnHour.Value.Minute, 0);

                ioTasksDTO.TimeCriteria = $@"NextMonth;Day={ioTasksDTO.TimeCriteriaModel.EveryMonthOnDay};Time={ioTasksDTO.TimeCriteriaModel.EveryMonthOnHour.Value.ToString("HH:mm")}";
                ioTasksDTO.ExecuteDateAndTime = runDate;
            } else if (ioTasksDTO.TimeCriteriaModel.ArchiveMode == (int)ArchiveMode.Interval) {
                if (ioTasksDTO.TimeCriteriaModel.ArchiveIntervalType == (int)ArchiveIntervalType.Day)
                    ioTasksDTO.TimeCriteria = $"AddDays={ioTasksDTO.TimeCriteriaModel.AddDays}";
                else if(ioTasksDTO.TimeCriteriaModel.ArchiveIntervalType == (int)ArchiveIntervalType.Hour)
                    ioTasksDTO.TimeCriteria = $"AddHours={ioTasksDTO.TimeCriteriaModel.AddHours}";
                else if(ioTasksDTO.TimeCriteriaModel.ArchiveIntervalType == (int)ArchiveIntervalType.Minute)
                    ioTasksDTO.TimeCriteria = $"AddMinutes={ioTasksDTO.TimeCriteriaModel.AddMinutes}";

                ioTasksDTO.ExecuteDateAndTime = ioTasksDTO.TimeCriteriaModel.ArchiveIntervalStartDate!.Value.Date
                    .AddHours(ioTasksDTO.TimeCriteriaModel.ArchiveIntervalSelectedHour!.Value.Hour)
                    .AddMinutes(ioTasksDTO.TimeCriteriaModel.ArchiveIntervalSelectedHour!.Value.Minute);
                ioTasksDTO.TimeCriteria += $";Time={ioTasksDTO.ExecuteDateAndTime.Value.ToString("HH:mm")}";
            } else
            {
                throw new Exception($"Archive mode {ioTasksDTO.TimeCriteriaModel.ArchiveMode} is not supported!");
            }

            if (!ioTasksDTO.TimeCriteriaModel.AutomaticArchiving)
                ioTasksDTO.Status = 100;
            else
                ioTasksDTO.Status = -1;

            // TODO added this code, 
            if (!string.IsNullOrEmpty(ioTasksDTO.TimeCriteriaModel.ExportLocation))
            {
                ioTasksDTO.Par1 = ioTasksDTO.TimeCriteriaModel.ExportLocation;

                if (ioTasksDTO.TimeCriteriaModel.SaveAsZip)
                    ioTasksDTO.Par1 += ";zip";
            }


            /*
             * {10. 06. 2025 02:30:00} dodan en dan??
             * 
            if (archiveType == 3 && mode == 1 && incrementType == 0) // excel day interval: set to previous day
            {
                sql += $@"UPDATE 
                                iotaskdetails 
                            SET 
                                par3 = 'DateAndTime >= CURDATE() - INTERVAL {increment} DAY AND DateAndTime < CURDATE()'
                            WHERE
                                IoTaskId = (SELECT IoTaskId FROM iotasks WHERE IoTaskType = 3 AND IoTaskMode = 1)
                            ORDER BY
                                TaskDetailOrder
                            LIMIT 1;";
            }

            */


        }


        public async Task SetAdditionalData(IoTasksDTO ioTasksDTO)
        {
            using var context = new EdnKntControllerMysqlContext();
            // TODO cache
            var clIntervalType = await context.CL_ArchiveIntervalType.ToListAsync();
            var clArchiveMode = await context.CL_ArchiveMode.ToListAsync();

            var archiveIntervalType = clIntervalType.Where(x => x.ArchiveIntervalTypeId == (int?)ioTasksDTO.TimeCriteriaModel.ArchiveIntervalType).Select(x => x.DescriptionLang).FirstOrDefault();
            var archiveMode = clArchiveMode.Where(x => x.ArchiveModeId == (int)ioTasksDTO.TimeCriteriaModel.ArchiveMode).Select(x => x.DescriptionLang).FirstOrDefault();

            ioTasksDTO.TimeCriteriaModel.ArchiveIntervalTypeDescription = archiveIntervalType != null ? _localization.Get(archiveIntervalType) : null;
            ioTasksDTO.TimeCriteriaModel.ArchiveModeDescription = archiveMode != null ? _localization.Get(archiveMode) : null;
        }
        
        public async Task<List<IoTaskLogsDTO>> GetIoTasksLogsAsync(SearchPageArgs searchPageArgs)
        {
            using var context = new EdnKntControllerMysqlContext();
            var result = await _autoMapper.ProjectTo<IoTaskLogsDTO>(context.IoTaskLogs.Where(searchPageArgs).OrderByDescending(x => x.IoTaskLogId).Skip(searchPageArgs.Skip).Take(searchPageArgs.Take)).ToListAsync();

            return result;
        }

        public async Task<ArchiveMaintenanceExportDialogModel> GetIoTasksFilter(int iIoTaskId)
        {
            using var context = new EdnKntControllerMysqlContext();
            var ioTask = await context.IoTasks.Where(x => x.IoTaskId == iIoTaskId).FirstAsync();

            var archiveMaintenanceExportDialogModel = new ArchiveMaintenanceExportDialogModel();
            string[] paz = ioTask.Par1.Split(';');
            if (paz.Length > 1 && paz[1] == "zip")
                archiveMaintenanceExportDialogModel.SaveAsZip = true;

            return archiveMaintenanceExportDialogModel;
        }

        public async Task<bool> SetIoTasksFilter(ArchiveMaintenanceExportDialogModel archiveMaintenanceExportDialogModel)
        {            
            var par3 = string.Empty;
            var par5 = string.Empty;
                      
            if (archiveMaintenanceExportDialogModel.DumpAllTables)
                par5 = "getAll";

            if (archiveMaintenanceExportDialogModel.Advanced)
            {
                if (archiveMaintenanceExportDialogModel.FilterType == FilterTypeEnum.Date)
                    par3 = $"DateAndTime >= '{archiveMaintenanceExportDialogModel.DateFrom.ToMysqlFormat()}' AND DateAndTime < '{archiveMaintenanceExportDialogModel.DateTo.AddDays(1).ToMysqlFormat()}'";
                else if (archiveMaintenanceExportDialogModel.FilterType == FilterTypeEnum.TransactionId)
                    par3 = $"TransactionId >= '{archiveMaintenanceExportDialogModel.TransactionIdFrom}' AND TransactionId <= '{archiveMaintenanceExportDialogModel.TransactionIdTo}'";
            }
            else
                par3 = "TransactionId>0";            

            using var context = new EdnKntControllerMysqlContext();
            var ioTask = await context.IoTasks.Where(x => x.IoTaskType == 4 && x.IoTaskMode == 2).FirstAsync(); // TODO is this just for IoTaskId= 6?

            var ioTaskDetail = await context.IoTaskDetails.Where(x => x.IoTaskId == ioTask.IoTaskId && x.TaskDetailOrder == 1).FirstAsync();
            ioTaskDetail.Par5 = par5;
            ioTaskDetail.Par3 = par3;

            ioTask.Par1 = ioTask.Par1.Replace(";zip", "");
            if (archiveMaintenanceExportDialogModel.SaveAsZip)
                ioTask.Par1 += ";zip";

            await context.SaveChangesAsync();

            return true;
        }

        public async Task<bool> SetIoTasksAsStartAsync(int ioTaskId)
        {
            using var context = new EdnKntControllerMysqlContext();
            var ioTask = context.IoTasks.Where(x => x.IoTaskId == ioTaskId).First();

            ioTask.Status = -1;
            ioTask.ExecuteDateAndTime = DateTime.Now;
            await context.SaveChangesAsync();
            return true;
        }


        public async Task<IEnumerable<CL_ArchiveMode>> GetArchiveMode()
        {
            using var context = new EdnKntControllerMysqlContext();
            var result = await context.CL_ArchiveMode.ToListAsync();
            ReplaceDropdownTranslation(result, nameof(CL_ArchiveMode.DescriptionLang), nameof(CL_ArchiveIntervalType.DescriptionLang), _localization);
            return result;
        }

        public async Task<IEnumerable<CL_ArchiveIntervalType>> GetArchiveIntervalType()
        {
            using var context = new EdnKntControllerMysqlContext();
            var result = await context.CL_ArchiveIntervalType.ToListAsync();
            ReplaceDropdownTranslation(result, nameof(CL_ArchiveIntervalType.DescriptionLang), nameof(CL_ArchiveIntervalType.DescriptionLang), _localization);
            return result;
        } 

        public async Task<bool> SetIoTaskAsync(IoTasksDTO ioTasksDTO)
        {
            using var context = new EdnKntControllerMysqlContext();

            var ioTask = context.IoTasks.Where(x => x.IoTaskId == ioTasksDTO.IoTaskId).First();
            CombineIoTasksDTO(ioTasksDTO);
            var combinedIoTasks = _autoMapper.Map<IoTasks>(ioTasksDTO);

            ioTask.Par1 = combinedIoTasks.Par1;
            ioTask.TimeCriteria = combinedIoTasks.TimeCriteria;
            ioTask.ExecuteDateAndTime = combinedIoTasks.ExecuteDateAndTime;
            ioTask.Status = combinedIoTasks.Status;                       

            if (ioTasksDTO.IoTaskType == 3 && ioTasksDTO.TimeCriteriaModel.ArchiveMode == (int)ArchiveMode.Interval && ioTasksDTO.TimeCriteriaModel.ArchiveIntervalType == (int)ArchiveIntervalType.Day)// excel day interval: set to previous day
            {
                var ioTaskDetail = await context.IoTaskDetails.Where(x => x.IoTaskId == ioTasksDTO.IoTaskId).OrderBy(x => x.TaskDetailOrder).FirstAsync();
                
                ioTaskDetail.Par3 = $"'DateAndTime >= CURDATE() - INTERVAL {ioTasksDTO.TimeCriteriaModel.AddDays} DAY AND DateAndTime < CURDATE()'";
                // original ni ok? naj se nastavi po idju?
                /*
                var sql += $@"UPDATE 
                                iotaskdetails 
                            SET 
                                par3 = 'DateAndTime >= CURDATE() - INTERVAL {increment} DAY AND DateAndTime < CURDATE()'
                            WHERE
                                IoTaskId = (SELECT IoTaskId FROM iotasks WHERE IoTaskType = 3 AND IoTaskMode = 1)
                            ORDER BY
                                TaskDetailOrder
                            LIMIT 1;";
                */
            }

            await context.SaveChangesAsync();

            return true;
        }
        /*
        Task<bool> IMaintenanceRepository.SetIoTaskAsync(IoTasksDTO ioTasksDTO)
        {
            return SetIoTaskAsync(ioTasksDTO);
        }
        */

        void ReplaceDropdownTranslation<T>(List<T> data, string dataColumn, string replaceColumn, Localization localization)
        {
            if (data == null || data.Count == 0)
                return;

            foreach (var d in data)
            {
                var property = d.GetType().GetProperty(dataColumn);
                var value = property.GetValue(d).ToString();
                var loc = localization.Get(value);

                d.GetType().GetProperty(replaceColumn).SetValue(d, loc);
            }
        }

    }
}
