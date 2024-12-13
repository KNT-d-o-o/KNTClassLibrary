using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KNTCommon.Business.DTOs
{
    public class UserCredentialsDTO
    {
        public int UserId;

        public string UserName = string.Empty;

        public string Password = string.Empty;

        public int? GroupId;

        public int? logout;
    }
}
