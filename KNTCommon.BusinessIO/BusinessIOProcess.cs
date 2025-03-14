using System;
using System.Threading;
using System.Threading.Tasks;
using KNTToolsAndAccessories;
using KNTCommon.BusinessIO.Repositories;
using KNTCommon.Business.Repositories;
using KNTCommon.BusinessIO.DTOs;
using KNTCommon.Data.Models;
using Microsoft.Extensions.DependencyInjection;
using DocumentFormat.OpenXml.Office.PowerPoint.Y2022.M08.Main;
using Microsoft.VisualBasic;

namespace KNTCommon.BusinessIO
{
    public class BusinessIOProcess
    {
        private readonly Tools t = new(); // Tools je vaš razred, ki mora biti ustrezno implementiran
        private bool procBusy = false;
        private readonly double TIMER_INTERVAL_NORMAL = 1000;

        private readonly IoTasksRepository? ioTasksRepository;
        private readonly ArchiveRepository? archiveRepository;
        private readonly ExportRepository? exportRepository;
        private readonly DumpRepository? dumpRepository;
        private readonly ParametersRepository? parametersRepository;

        private const int TASKID_ARCHIVE = 1;
        private const int TASKID_RESTORE = 2;
        private const int TASKID_EXPORT_EXCEL = 3;
        private const int TASKID_EXPORT_DUMP = 4;

        public BusinessIOProcess(IoTasksRepository _ioTasksRepository, ArchiveRepository _archiveRepository, ExportRepository _exportRepository, DumpRepository _dumpRepository, ParametersRepository _parametersRepository)
        {
            ioTasksRepository = _ioTasksRepository;
            archiveRepository = _archiveRepository;
            exportRepository = _exportRepository;
            dumpRepository = _dumpRepository;
            parametersRepository = _parametersRepository;
        }

        public async Task<bool> OnStartAsync(CancellationToken cancellationToken, string serviceVersion)
        {
            if (BusinessIoInit())
            {
#if DEBUG
                Console.WriteLine("BusinessIOProcess OnStart.");
#endif
                if(ioTasksRepository != null)
                    ioTasksRepository.IoTaskSetInfo(0, $"BusinessIOProcess OnStart version: {serviceVersion}.", Const.INFO);

                while (!cancellationToken.IsCancellationRequested)
                {
                    await OnElapsedTimeAsync();
                    await Task.Delay(TimeSpan.FromMilliseconds(TIMER_INTERVAL_NORMAL), cancellationToken);
                }

                return true;
            }
            else
            {
#if DEBUG
                Console.WriteLine("BusinessIoInit fault.");
#endif
                OnStop();
                return false;
            }
        }

        public bool BusinessIoInit()
        {
            bool ret = true;
            if (archiveRepository is null || ioTasksRepository is null)
                return false;

            // create archive database: TASKID_ARCHIVE
            string? error;
            if (archiveRepository.CheckOrCreateArchiveDb(out error))
            {
                ret = archiveRepository.CheckOrCreateArchiveTables(null, out error);
            }
            if (!ret)
            {
                ioTasksRepository.IoTaskSetInfo(TASKID_ARCHIVE, error ?? string.Empty, Const.ERROR);
            }

            return ret;
        }

        int statusPrev = 0;
        public async Task OnElapsedTimeAsync()
        {
            if (procBusy) return;

            procBusy = true;

            try
            {
                await Task.Run(() =>
                {
#if DEBUG
                    Console.WriteLine("BusinessIOProcess OnElapsedTime.");
#endif
                    if (ioTasksRepository != null)
                    {
                        IEnumerable<IoTasksDTO> ioTasks = ioTasksRepository.GetIoTasks();

                        foreach (IoTasksDTO ioTask in ioTasks)
                        {
                            if (ioTask.ExecuteDateAndTime < DateTime.Now && ioTask.Status < 100)
                            {
#if DEBUG
                                Console.WriteLine($"{DateTime.Now} TO DO {ioTask.IoTaskName}");
#endif
                                switch (ioTask.IoTaskType)
                                {
                                    case TASKID_ARCHIVE: // archive
                                        ArchiveStep(ioTask);
                                        break;
                                    case TASKID_RESTORE: // restore
                                        RestoreStep(ioTask);
                                        break;
                                    case TASKID_EXPORT_EXCEL:
                                        ExportExcelStep(ioTask);
                                        break;
                                    case TASKID_EXPORT_DUMP:
                                        ExportDumpStep(ioTask);
                                        break;
                                }

                                break; // execute only first priority task
                            }
                        }
                        foreach (IoTasksDTO ioTask in ioTasks) // set remaining time info
                        {
                            if (ioTask.ExecuteDateAndTime > DateTime.Now)
                            {
                                if (ioTask.ExecuteDateAndTime != null)
                                {
                                    if (ioTask.IoTaskMode == 1) // cycling
                                    {
                                        if (ioTask.Status < 0) // minutes to next
                                        {
                                            int minutesTo = -(int)Math.Round(((TimeSpan)(ioTask.ExecuteDateAndTime - DateTime.Now)).TotalMinutes);

                                            if (minutesTo != statusPrev)
                                            {
                                                ioTasksRepository.IoTaskSetStatus(ioTask.IoTaskId, minutesTo);
                                                ioTasksRepository.IoTaskSetInfo(ioTask.IoTaskId, "Remaining minutes: " + minutesTo.ToString(), Const.NONE);
                                                statusPrev = minutesTo;
                                            }
                                        }
                                        else if (ioTask.Status >= 100 && ioTask.Status != statusPrev) // disable manual
                                        {
                                            ioTasksRepository.IoTaskSetInfo(ioTask.IoTaskId, "Disabled, " + DateTime.Now.ToString(), Const.WARNING);
                                            statusPrev = ioTask.Status;
                                        }
                                    }
                                }
                            }
                        }
                    }
                });
            }
            catch (Exception ex)
            {
                t.LogEvent($"Error in OnElapsedTime: {ex.Message}");
            }
            finally
            {
                procBusy = false;
            }
        }

        public void OnStop()
        {
            try
            {
#if DEBUG
                Console.WriteLine("BusinessIOProcess OnStop.");
#endif
                if (ioTasksRepository != null)
                    ioTasksRepository.IoTaskSetInfo(0, "BusinessIOProcess OnStop.", Const.INFO);
            }
            catch (Exception ex)
            {
                t.LogEvent($"Error in OnStop: {ex.Message}");
            }
        }

        // archive step procedure
        int noToArchive;
        int noArchived;
        string? dayBeforeStrA;
        string? dayWhereA;
        string? orderByA;
        int tableOptimizedIdx = 0;
        List<IoTaskDetailsDTO>? taskDetailsA;
        private void ArchiveStep(IoTasksDTO task)
        {
#if DEBUG
            Console.WriteLine($"ArchiveStep noToArchive: {noToArchive}, noArchived: {noArchived}");
#endif
            try
            {
                if (parametersRepository is null || archiveRepository is null || ioTasksRepository is null)
                    return;
                int archiveDays = Convert.ToInt32(parametersRepository.GetParametersStr("archiveDays"));
                int archiveStep = Convert.ToInt32(parametersRepository.GetParametersStr("archiveStep"));

                if (archiveDays > 0 && archiveStep > 0)
                {
                    if (noToArchive == 0)
                    {
                        dayBeforeStrA = DateTime.Now.AddDays(-archiveDays).Date.ToString("yyyy-MM-dd");
                        taskDetailsA = (List<IoTaskDetailsDTO>)ioTasksRepository.GetIoTaskDetails(task.IoTaskId);
                        dayWhereA = (taskDetailsA[0].Par3 ?? string.Empty).Replace("{dayBeforeStr}", dayBeforeStrA);
                        orderByA = (taskDetailsA[0].Par3 ?? string.Empty).Split('<')[0];

                        noToArchive = archiveRepository.CountRecordsBeforeArchiveRestore(taskDetailsA[0].Par1 ?? string.Empty, dayWhereA, taskDetailsA[0].Par4 ?? string.Empty, false);
                        noArchived = 0;
                        if (noToArchive > 0)
                        {
                            ioTasksRepository.IoTaskSetStatus(task.IoTaskId, 0);
                            ioTasksRepository.IoTaskSetInfo(task.IoTaskId, "Archiving...", Const.INFO);

                            if (taskDetailsA[0].Par5 == "copyIfNever" || taskDetailsA[0].Par5 == "copyAlways")
                            {
                                ioTasksRepository.IoTaskSetInfo(task.IoTaskId, $"Copy settings..., {DateTime.Now.ToString()}", Const.INFO);
                                archiveRepository.CopyOtherTables(taskDetailsA.Select(td => td.Par1 ?? string.Empty).ToList());
                                if (taskDetailsA[0].Par5 == "copyIfNever")
                                    ioTasksRepository.IoTaskSetPar5(task.IoTaskId, 1, "copy");
                            }
                        }
                    }

                    // archive
                    if (taskDetailsA is not null)
                    {
                        List<string> AllTables = taskDetailsA.Where(td => td.Par5 != "none").Select(td => td.Par1 ?? string.Empty).ToList();

                        int stepArchived = archiveRepository.ArchiveTables(AllTables, taskDetailsA, dayWhereA ?? string.Empty, orderByA ?? string.Empty, archiveStep, taskDetailsA[0].Par4 ?? string.Empty);

                        if (stepArchived > 0)
                        {
                            noArchived += stepArchived;

                            int percentDone = noArchived * 100 / noToArchive;
                            if (percentDone == 100) // not to complete
                                percentDone = 99;
                            ioTasksRepository.IoTaskSetStatus(task.IoTaskId, percentDone);
                            ioTasksRepository.IoTaskSetInfo(task.IoTaskId, $"Completed Archiving: {percentDone}%", Const.NONE);
                        }
                        else
                        {
                            if (noToArchive > 0 && noArchived > 0) // optimize
                            {
                                archiveRepository.OptimizeTable(AllTables[tableOptimizedIdx]);
                                int percentDone = (tableOptimizedIdx + 1) * 100 / AllTables.Count;
                                if(percentDone == 100) // not to complete
                                    percentDone = 99;
                                ioTasksRepository.IoTaskSetStatus(task.IoTaskId, percentDone);
                                ioTasksRepository.IoTaskSetInfo(task.IoTaskId, $"Optimized: {percentDone}%", Const.NONE);
                                tableOptimizedIdx++;
                                if (tableOptimizedIdx == AllTables.Count)
                                {
                                    noToArchive = 0;
                                    tableOptimizedIdx = 0;
                                }
                            }
                            else // end archiving
                            {
                                ioTasksRepository.IoTaskSetStatus(task.IoTaskId, 100);
                                ioTasksRepository.IoTaskSetInfo(task.IoTaskId, $"Completed Archiving, {DateTime.Now.ToString()}", Const.INFO);
                                if (task.IoTaskMode == 1) // periodic
                                {
                                    ioTasksRepository.IoTaskSetExecuteDateAndTime(task.IoTaskId, DateTime.Now);
                                    ioTasksRepository.IoTaskSetStatus(task.IoTaskId, -1);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                t.LogEvent("KNTCommon.BusinessIO.BusinessIOProcess #1 " + ex.Message);
            }
        }

        // restore step procedure
        int noToRestore;
        int noRestored;
        string? dayWhereR;
        string? orderByR;
        List<IoTaskDetailsDTO>? taskDetailsR;
        private void RestoreStep(IoTasksDTO task)
        {
            try
            {
                if (parametersRepository is null || archiveRepository is null || ioTasksRepository is null)
                    return;
                int restoreStep = Convert.ToInt32(parametersRepository.GetParametersStr("archiveStep"));

                if (restoreStep > 0)
                {
                    if (noToRestore == 0)
                    {
                        taskDetailsR = (List<IoTaskDetailsDTO>)ioTasksRepository.GetIoTaskDetails(task.IoTaskId);
                        dayWhereR = (taskDetailsR[0].Par3 ?? string.Empty);

                        string orderAll = taskDetailsR[0].Par3 ?? string.Empty;
                        if (orderAll.Length > 0)
                        {
                            orderByR = orderAll.Split('<', '>')[0];
                        }
                        else
                            orderByR = string.Empty;

                        noToRestore = archiveRepository.CountRecordsBeforeArchiveRestore(taskDetailsR[0].Par1 ?? string.Empty, dayWhereR, taskDetailsR[0].Par4 ?? string.Empty, true);
                        noRestored = 0;
                        if (noToRestore > 0)
                        {
                            ioTasksRepository.IoTaskSetStatus(task.IoTaskId, 0);
                            ioTasksRepository.IoTaskSetInfo(task.IoTaskId, "Restoring...", Const.INFO);
                        }
                    }

                    // restore
                    if (taskDetailsR is not null)
                    {
                        List<string> AllTables = taskDetailsR.Where(td => td.Par5 != "none").Select(td => td.Par1 ?? string.Empty).ToList();

                        int stepRestored = archiveRepository.RestoreTables(AllTables, taskDetailsR, dayWhereR ?? string.Empty, orderByR ?? string.Empty, restoreStep, taskDetailsR[0].Par4 ?? string.Empty);

                        bool complete = false;
                        if (stepRestored > 0)
                        {
                            noRestored += stepRestored;
                            int percentDone = noRestored * 100 / noToRestore;
                            if (percentDone == 100) // not to complete all
                            {
                                percentDone = 99;
                                complete = true;
                            }
                            ioTasksRepository.IoTaskSetStatus(task.IoTaskId, percentDone);
                            ioTasksRepository.IoTaskSetInfo(task.IoTaskId, $"Completed Restoring: {percentDone}%", Const.NONE);
                        }
                        if(complete) // end restoring
                        {
                            noToRestore = 0;
                            ioTasksRepository.IoTaskSetStatus(task.IoTaskId, 100);
                            ioTasksRepository.IoTaskSetInfo(task.IoTaskId, $"Completed Restoring, {DateTime.Now.ToString()}", Const.INFO);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                t.LogEvent("KNTCommon.BusinessIO.BusinessIOProcess #2 " + ex.Message);
            }
        }

        // export excel step procedure
        int noToExport;
        int stepExport;
        string? filePathXls;
        List<IoTaskDetailsDTO>? taskDetailsE;
        string? whereConditionE;
        string? nextDateTimeConditionE;
        private void ExportExcelStep(IoTasksDTO task)
        { 
            try
            {
                if (exportRepository is null || ioTasksRepository is null)
                    return;

                if (noToExport == 0)
                {
                    stepExport = 0;
                    taskDetailsE = (List<IoTaskDetailsDTO>)ioTasksRepository.GetIoTaskDetails(task.IoTaskId);
                    noToExport = taskDetailsE.Count;
                    filePathXls = (task.Par1 ?? string.Empty).Replace("{DateAndTime}", DateTime.Now.ToString("yyyyMMddHHmmss"));
                    if (noToExport > 0)
                    {
                        whereConditionE = taskDetailsE[0].Par3 ?? string.Empty;
                        if(whereConditionE.Contains("DateAndTime>"))
                            nextDateTimeConditionE = "DateAndTime>'" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") +"'";
                        if(nextDateTimeConditionE != null)
                            whereConditionE += ($" AND {nextDateTimeConditionE.Replace(">", "<")}");

                        ioTasksRepository.IoTaskSetStatus(task.IoTaskId, 0);
                        ioTasksRepository.IoTaskSetInfo(task.IoTaskId, "Exporting...", Const.INFO);
                    }
                }
                else
                {
                    if (taskDetailsE != null)
                    {
                        string tableName = taskDetailsE[stepExport].Par1 ?? string.Empty;
                        string order = taskDetailsE[stepExport].Par2 ?? string.Empty;

                        string errStr = string.Empty;

                        List<string> altData = new();
                        string altCols = taskDetailsE[stepExport].Par4 ?? string.Empty;
                        if(altCols.Length > 0)
                            altData = altCols.Split(';').ToList();

                        if (!exportRepository.ExportExcel(tableName, whereConditionE ?? string.Empty, order, filePathXls ?? "C:/tmp.xlsx", altData, out errStr))
                        {
                            ioTasksRepository.IoTaskSetInfo(task.IoTaskId, $"Error: {errStr}", Const.ERROR);
                        }

                        //fstaa to do test
                        bool complete = false;
                        if (stepExport + 1 < noToExport)
                        {
                            stepExport++;
                            int percentDone = stepExport * 100 / noToExport;
                            if (percentDone == 100) // not to complete all
                            {
                                percentDone = 99;
                                complete = true;
                            }
                            ioTasksRepository.IoTaskSetStatus(task.IoTaskId, percentDone);
                            ioTasksRepository.IoTaskSetInfo(task.IoTaskId, $"Completed Exporting: {percentDone}%", Const.NONE);
                        }
                        if(complete) // end exporting
                        {
                            ioTasksRepository.IoTaskSetStatus(task.IoTaskId, 100);
                            ioTasksRepository.IoTaskSetInfo(task.IoTaskId, $"Completed Exporting, {DateTime.Now.ToString()}", Const.INFO);
                            if (task.IoTaskMode == 1) // periodic
                            {
                                // set next datetime condition
                                if(nextDateTimeConditionE != null)
                                    ioTasksRepository.IoTaskSetPar3(task.IoTaskId, 1, nextDateTimeConditionE);

                                ioTasksRepository.IoTaskSetExecuteDateAndTime(task.IoTaskId, DateTime.Now);
                                ioTasksRepository.IoTaskSetStatus(task.IoTaskId, -1);
                            }
                            noToExport = 0;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                t.LogEvent("KNTCommon.BusinessIO.BusinessIOProcess #3 " + ex.Message);
            }
        }

        // DB dump step procedure
        int noToDump;
        int stepDump;
        string? filePathDump;
        List<IoTaskDetailsDTO>? taskDetailsD;
        bool toZip = false;
        private void ExportDumpStep(IoTasksDTO task)
        { 
            try
            {
                if (dumpRepository is null || ioTasksRepository is null)
                    return;

                if (noToDump == 0)
                {
                    stepDump = 0;
                    taskDetailsD = (List<IoTaskDetailsDTO>)ioTasksRepository.GetIoTaskDetails(task.IoTaskId);

                    if (taskDetailsD.Count == 0 || taskDetailsD[0].Par5 == "getAll") // dump all
                    {
                        List<string> tables = dumpRepository.GetTableList();

                        foreach (var table in tables)
                        {
                            bool tableDbDefined = false;
                            if (taskDetailsD[0].Par5 == "getAll") // skip DB defined tables
                            {
                                foreach (IoTaskDetailsDTO details in taskDetailsD)
                                {
                                    if (details.Par1 == table)
                                    {
                                        tableDbDefined = true;
                                        break;
                                    }
                                }
                            }

                            if (!tableDbDefined)
                            {
                                IoTaskDetailsDTO element = new();
                                element.IoTaskDetailId = 0;
                                element.IoTaskId = task.IoTaskId;
                                element.Par1 = table;

                                taskDetailsD.Add(element);
                            }
                        }
                    }

                    noToDump = taskDetailsD.Count;
                    string par1 = task.Par1 ?? string.Empty;
                    string[] pars1 = par1.Split(';');

                    if (pars1.Length > 1) // check for zip
                    {
                        if (pars1[1] == "zip")
                            toZip = true;
                    }

                    filePathDump = (pars1[0]).Replace("{DateAndTime}", DateTime.Now.ToString("yyyyMMddHHmmss"));
                    if (noToDump > 0)
                    {
                        ioTasksRepository.IoTaskSetStatus(task.IoTaskId, 0);
                        ioTasksRepository.IoTaskSetInfo(task.IoTaskId, "Dumping...", Const.INFO);
                    }
                }
                else
                {
                    if (taskDetailsD != null)
                    {
                        string tableName = taskDetailsD[stepDump].Par1 ?? string.Empty;
                        string whereColumn = taskDetailsD[stepDump].Par2 ?? string.Empty;
                        string whereCondition = taskDetailsD[stepDump].Par3 ?? string.Empty;

                        if (whereColumn != string.Empty) // join export
                        {
                            whereCondition = $"SELECT {whereColumn} FROM {whereCondition} WHERE {taskDetailsD[0].Par3}";
                        }

                        string errStr = string.Empty;

                        if (!dumpRepository.DumpTable(tableName, whereCondition, whereColumn, filePathDump ?? "C/:tmp.sql", out errStr))
                        {
                            ioTasksRepository.IoTaskSetInfo(task.IoTaskId, $"Error: {errStr}", Const.ERROR);
                        }

                        if (stepDump + 1 < noToDump)
                        {
                            stepDump++;
                            int percentDone = stepDump * 100 / noToDump;
                        //fstaa test    if (percentDone == 100) // not to complete all
                          //      percentDone = 99;
                            ioTasksRepository.IoTaskSetStatus(task.IoTaskId, percentDone);
                            ioTasksRepository.IoTaskSetInfo(task.IoTaskId, $"Completed Dumping: {percentDone}%", Const.NONE);
                        }
                        else // end exporting
                        {
                            if (toZip && filePathDump != null)
                                ioTasksRepository.CompressedFileToZip(filePathDump);

                            noToDump = 0;
                            ioTasksRepository.IoTaskSetStatus(task.IoTaskId, 100);
                            ioTasksRepository.IoTaskSetInfo(task.IoTaskId, $"Completed Dumping, {DateTime.Now.ToString()}", Const.INFO);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                t.LogEvent("KNTCommon.BusinessIO.BusinessIOProcess #4 " + ex.Message);
            }
        }

    }
}