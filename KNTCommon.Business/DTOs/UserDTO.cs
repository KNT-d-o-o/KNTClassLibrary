using KNTCommon.Data.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KNTCommon.Business.DTOs
{
    public class UserDTO
    {
        [Key]
        public int UsersId { get; set; }

        public string? UserId { get; set; } = null!;

        public string? UserName { get; set; }

        public string? UserGroup { get; set; }

        public int? GroupId { get; set; }

        public byte[]? PasswordHash { get; set; }

        public byte[]? InitializationVector { get; set; }

        public int? Login { get; set; }

        public int? a1 { get; set; }

        public int? a2 { get; set; }

        public int? a3 { get; set; }

        public int? a4 { get; set; }

        public int? a5 { get; set; }

        public int? a6 { get; set; }

        public int? a7 { get; set; }

        public int? a8 { get; set; }

        public int? a9 { get; set; }

        public int? a10 { get; set; }

        public int? a11 { get; set; }

        public int? a12 { get; set; }

        public int? a13 { get; set; }

        public int? a14 { get; set; }

        public int? a15 { get; set; }

        public int? a16 { get; set; }

        public int? a17 { get; set; }

        public int? a18 { get; set; }

        public int? a19 { get; set; }

        public int? a20 { get; set; }

        public int? a21 { get; set; }

        public int? a22 { get; set; }

        public int? a23 { get; set; }

        public int? a24 { get; set; }

        public int? a25 { get; set; }

        public int? a26 { get; set; }

        public int? a27 { get; set; }

        public int? a28 { get; set; }

        public int? a29 { get; set; }

        public int? a30 { get; set; }

        public int? a31 { get; set; }

        public int? a32 { get; set; }

        public int? a33 { get; set; }

        public int? a34 { get; set; }

        public int? a35 { get; set; }

        public int? a36 { get; set; }

        public int? a37 { get; set; }

        public int? a38 { get; set; }

        public int? a39 { get; set; }

        public int? a40 { get; set; }

        public int? a41 { get; set; }

        public int? a42 { get; set; }

        public int? a43 { get; set; }

        public int? a44 { get; set; }

        public int? a45 { get; set; }

        public int? a46 { get; set; }

        public int? a47 { get; set; }

        public int? a48 { get; set; }

        public int? a49 { get; set; }

        public int? a50 { get; set; }

        public int? logout { get; set; }

        public string? RfIdCard { get; set; }

        public string? RfidCardCom { get; set; }

        public DateTime? DateAndTimeLogin { get; set; }

        public string? Password { get; set; }


        public virtual ICollection<UserSession> UserSessions { get; set; } = new List<UserSession>();

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
