using System;
using System.Threading;
using System.Threading.Tasks;
using KNTToolsAndAccessories;
using KNTCommon.BusinessIO.Repositories;
using KNTCommon.Business.Repositories;
using KNTCommon.BusinessIO.DTOs;
using KNTCommon.Data.Models;
using Microsoft.Extensions.DependencyInjection;

namespace KNTCommon.BusinessIO
{
    public class BusinessIOProcess
    {
        private readonly Tools t = new(); // Tools je vaš razred, ki mora biti ustrezno implementiran
        private bool procBusy = false;
        private readonly double TIMER_INTERVAL_NORMAL = 1000;

        private readonly IoTasksRepository? ioTasksRepository;
        private readonly ArchiveRepository? archiveRepository;
        private readonly ParametersRepository? parametersRepository;

        private const int TASKID_ARCHIVE = 1;
        private const int TASKID_RESTORE = 2;

        public BusinessIOProcess(IoTasksRepository _ioTasksRepository, ArchiveRepository _archiveRepository, ParametersRepository _parametersRepository)
        {
            ioTasksRepository = _ioTasksRepository;
            archiveRepository = _archiveRepository;
            parametersRepository = _parametersRepository;
        }

        public async Task<bool> OnStartAsync(CancellationToken cancellationToken)
        {
            if (BusinessIoInit())
            {
#if DEBUG
                Console.WriteLine("BusinessIOProcess OnStart.");
#endif

                // Simulacija začetne logike
                while (!cancellationToken.IsCancellationRequested)
                {
                    await OnElapsedTimeAsync();
                    await Task.Delay(TimeSpan.FromMilliseconds(TIMER_INTERVAL_NORMAL), cancellationToken);
                }

                return true;
            }
            else
            {
                Console.WriteLine("BusinessIoInit fault.");
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
                List<string> arcTables = new();
                IEnumerable<IoTaskDetailsDTO> archiveDetails = ioTasksRepository.GetIoTaskDetails(TASKID_ARCHIVE);
                foreach (var entry in archiveDetails)
                {
                    if(entry.Par1 != null && entry.Par5 != "none")
                        arcTables.Add(entry.Par1);
                }
                ioTasksRepository.GetIoTaskDetails(TASKID_RESTORE); // only for disable not existed tables

                ret = archiveRepository.CheckOrCreateArchiveTables(arcTables, out error);
            }
            if (!ret)
            {
                ioTasksRepository.IoTaskSetInfo(TASKID_ARCHIVE, error ?? string.Empty);
            }

            return ret;
        }

        public async Task OnElapsedTimeAsync()
        {
            if (procBusy) return;

            procBusy = true;

            try
            {
                // TODO: Implementacija vaše logike, ki se izvaja ob vsakem preteku časovnika
                await Task.Run(() =>
                {
                    // Simulacija dela
#if DEBUG
                    Console.WriteLine("BusinessIOProcess OnElapsedTime.");
#endif
                    if (ioTasksRepository != null)
                    {
                        IEnumerable<IoTasksDTO> ioTasks = ioTasksRepository.GetIoTasks();

                        foreach (IoTasksDTO ioTask in ioTasks)
                        {
                            if (ioTask.ExecuteDateAndTime < DateTime.Now)
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
                                }
                                //fstaaaaa to do



                                break; // execute only first priority task
                            }
                        }
                        foreach (IoTasksDTO ioTask in ioTasks) // set remaining time info
                        {
                            if (ioTask.ExecuteDateAndTime > DateTime.Now)
                            {
                                if (ioTask.ExecuteDateAndTime != null)
                                {
                                    ioTasksRepository.IoTaskSetInfo(ioTask.IoTaskId, "Remaining minutes: " + Math.Round(((TimeSpan)(ioTask.ExecuteDateAndTime - DateTime.Now)).TotalMinutes).ToString());
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
                        ioTasksRepository.IoTaskSetInfo(task.IoTaskId, "Archiving...");
                    }

                    // archive
                    if (taskDetailsA is not null)
                    {
                        List<string> AllTables = taskDetailsA.Where(td => td.Par5 != "none").Select(td => td.Par1 ?? string.Empty).ToList();

                        int stepArchived = archiveRepository.ArchiveTables(AllTables, taskDetailsA, dayWhereA ?? string.Empty, orderByA ?? string.Empty, archiveStep, taskDetailsA[0].Par4 ?? string.Empty);

                        if (stepArchived > 0)
                        {
                            noArchived += stepArchived;
                            ioTasksRepository.IoTaskSetInfo(task.IoTaskId, $"Completed: {noArchived * 100 / noToArchive}%");
                        }
                        else // end archiving
                        {
                            if (noToArchive > 0 && noArchived > 0) // optimize
                            {
                                archiveRepository.OptimizeTable(AllTables[tableOptimizedIdx]);
                                ioTasksRepository.IoTaskSetInfo(task.IoTaskId, $"Optimized: {(tableOptimizedIdx + 1) * 100 / AllTables.Count}%");
                                tableOptimizedIdx++;
                                if (tableOptimizedIdx == AllTables.Count)
                                {
                                    noToArchive = 0;
                                    tableOptimizedIdx = 0;
                                }
                            }
                            else
                            {
                             //   noToArchive = 0;
                                if (task.IoTaskMode == 1) // periodic
                                {
                                    ioTasksRepository.IoTaskSetExecuteDateAndTime(task.IoTaskId, DateTime.Now.AddDays(1).Date); // fstaaaaa maybe more days
                                }
                                else if (task.IoTaskMode == 2) // on demand
                                {
                                    ioTasksRepository.IoTaskSetStatus(task.IoTaskId, 1);
                                    ioTasksRepository.IoTaskSetInfo(task.IoTaskId, $"Completed, {DateTime.Now.ToString()}");
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
                        ioTasksRepository.IoTaskSetInfo(task.IoTaskId, "Restoring...");
                    }

                    // restore
                    if (taskDetailsR is not null)
                    {
                        List<string> AllTables = taskDetailsR.Where(td => td.Par5 != "none").Select(td => td.Par1 ?? string.Empty).ToList();

                        int stepRestored = archiveRepository.RestoreTables(AllTables, taskDetailsR, dayWhereR ?? string.Empty, orderByR ?? string.Empty, restoreStep, taskDetailsR[0].Par4 ?? string.Empty);

                        if (stepRestored > 0)
                        {
                            noRestored += stepRestored;
                            ioTasksRepository.IoTaskSetInfo(task.IoTaskId, $"Completed: {noRestored * 100 / noToRestore}%");
                        }
                        else // end restoring
                        {
                            noToRestore = 0;
                            if (task.IoTaskMode == 2) // only on demand
                            {
                                ioTasksRepository.IoTaskSetStatus(task.IoTaskId, 1);
                                ioTasksRepository.IoTaskSetInfo(task.IoTaskId, $"Completed, {DateTime.Now.ToString()}");
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                t.LogEvent("KNTCommon.BusinessIO.BusinessIOProcess #2 " + ex.Message);
            }
        }
    }
}