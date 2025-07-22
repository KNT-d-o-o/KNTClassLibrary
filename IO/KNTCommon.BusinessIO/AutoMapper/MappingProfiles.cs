using AutoMapper;
using KNTCommon.Business.DTOs;
using KNTCommon.BusinessIO.DTOs;
using KNTCommon.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KNTCommon.BusinessIO.AutoMapper
{
    public class MappingProfiles : Profile
    {
        public MappingProfiles()
        {
            CreateMap<IoTasks, IoTasksDTO>().ReverseMap();
            CreateMap<IoTasksDTO, IoTasks>().ReverseMap();

            CreateMap<IoTaskDetails, IoTaskDetailsDTO>().ReverseMap();
            CreateMap<IoTaskDetailsDTO, IoTaskDetails>().ReverseMap();

            CreateMap<IoTaskLogs, IoTaskLogsDTO>().ReverseMap();
            CreateMap<IoTaskLogsDTO, IoTaskLogs>().ReverseMap();

            CreateMap<Transactions, TransactionsDTO>().ReverseMap();
            CreateMap<TransactionsDTO, Transactions>().ReverseMap();

        }
    }
}
