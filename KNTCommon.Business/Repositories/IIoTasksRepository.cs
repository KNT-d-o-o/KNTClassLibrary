using AutoMapper;
using KNTCommon.Business.DTOs;
using KNTCommon.Data.Models;
using Microsoft.EntityFrameworkCore;
using System.IO.Compression;

namespace KNTCommon.Business.Repositories
{
    public interface IIoTasksRepository
    {
        IEnumerable<IoTasksDTO> GetIoTasks();
        int GetIoTaskIdByTypeMode(int type, int mode);
        IoTasksDTO GetIoTaskByTypeMode(int type, int mode);
        IEnumerable<IoTaskDetailsDTO> GetIoTaskDetails(int taskId, bool ignoreNone);
        List<string> GetIoTaskDetailsPar1(int taskId);
        bool IoTaskSetInfo(int taskId, string str, int taskType);
        bool IoTaskSetStatus(int taskId, int status);
        bool IoTaskSetPar1(int taskId, string valStr);
        bool IoTaskStart(int taskId);
        bool IoTaskSetExecuteDateAndTime(int taskId, DateTime dateTime);
        bool IoTaskSetPar3(int taskId, int order, string val);
        bool IoTaskSetPar5(int taskId, int order, string val);
        bool CompressedFileToZip(string filePath);

    }
}
