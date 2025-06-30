using AutoMapper;
using KNTCommon.Business.DTOs;
using KNTCommon.Data.Models;
using KNTSMM.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KNTCommon.Business.AutoMapper
{
    public class MappingProfiles : Profile
    {
        public MappingProfiles()
        {
            CreateMap<Parameter, ParameterDTO>().ReverseMap();
            CreateMap<ParameterDTO, Parameter>().ReverseMap();

            CreateMap<User, UserCredentialsDTO>().ReverseMap();
            CreateMap<UserCredentialsDTO, User>().ReverseMap();

            CreateMap<User, UserDTO>().ReverseMap();
            CreateMap<UserDTO, User>().ReverseMap();

            CreateMap<UserGroup, UserGroupDTO>().ReverseMap();
            CreateMap<UserGroupDTO, UserGroup>().ReverseMap();

            CreateMap<ServiceControl, ServiceControlDTO>().ReverseMap();
            CreateMap<ServiceControlDTO, ServiceControl>().ReverseMap();

            CreateMap<Results, ResultsDTO>().ReverseMap();
            CreateMap<ResultsDTO, Results>().ReverseMap();

            CreateMap<IoTasks, IoTasksDTO>().ReverseMap();
            CreateMap<IoTasksDTO, IoTasks>().ReverseMap();

            CreateMap<IoTaskLogs, IoTaskLogsDTO>().ReverseMap();
            CreateMap<IoTaskLogsDTO, IoTaskLogs>().ReverseMap();

            CreateMap<CL_Module, CL_ModuleDTO>().ReverseMap();
            CreateMap<CL_ModuleDTO, CL_Module>().ReverseMap();

            CreateMap<CL_ModuleFunctionality, CL_ModuleFunctionalityDTO>().ReverseMap();
            CreateMap<CL_ModuleFunctionalityDTO, CL_ModuleFunctionality>().ReverseMap();
        }
    }
}
