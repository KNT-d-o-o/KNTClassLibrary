using KNTCommon.Business.DTOs;
using KNTCommon.Business.Models;
using KNTCommon.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KNTCommon.Business.Interface
{
    public interface IMaintenanceRepository
    {
        Task<List<ParameterDTO>> GetArchiveParameters();
        Task<List<IoTasksDTO>> GetIoTasksAsync(int loggedPower);

        Task<IoTasksDTO> GetIoTaskAsync(int ioTaskId);
        Task ExpandIoTasksDTO(IoTasksDTO ioTasksDTO);

        Task<List<IoTaskLogsDTO>> GetIoTasksLogsAsync(SearchPageArgs searchPageArgs);

        Task<bool> SetIoTasksAsStartAsync(int ioTaskId);

        Task<ArchiveMaintenanceExportDialogModel> GetIoTasksFilter(int iIoTaskId);
        Task<bool> SetIoTasksFilter(ArchiveMaintenanceExportDialogModel archiveMaintenanceExportDialogModel);




        Task<IEnumerable<CL_ArchiveMode>> GetArchiveMode();
        Task<IEnumerable<CL_ArchiveIntervalType>> GetArchiveIntervalType();



        Task<bool> SetIoTaskAsync(IoTasksDTO ioTasksDTO);
    }
}
