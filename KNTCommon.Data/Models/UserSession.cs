using System;
using System.Collections.Generic;

namespace KNTCommon.Data.Models;

public partial class UserSession
{
    public int UserSessionId { get; set; }

    public int? UsersId { get; set; }

    public bool? IsLogin { get; set; }

    public DateOnly? SessionDate { get; set; }

    public virtual User? User { get; set; }
}
