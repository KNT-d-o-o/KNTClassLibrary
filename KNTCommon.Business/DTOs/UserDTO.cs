using KNTCommon.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KNTCommon.Business.DTOs
{
    public class UserDTO
    {
        public int UserId { get; set; }

        public string? UserName { get; set; }

        public byte[]? PasswordHash { get; set; }

        public byte[]? InitializationVector { get; set; }

        public int? logout { get; set; }

        public int? GroupId { get; set; }

        public List<UserGroupDTO>? UserGroup { get; set; }
        public List<ControlGroupDTO>? ControlGroup { get; set; }
    }

    public class ControlGroupDTO
    {
        public int ControlGroupId { get; set; }

        public string? ControlGroupName { get; set; }
    }

    public class UserNewDTO
    {
        public int? GroupId { get; set; }

        public string? UserName { get; set; }

        public string? Password{ get; set; }
    }
}
