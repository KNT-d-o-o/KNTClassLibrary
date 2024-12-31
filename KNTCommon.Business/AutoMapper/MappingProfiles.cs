using AutoMapper;
using KNTCommon.Business.DTOs;
using KNTCommon.Data.Models;
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
        }
    }
}
