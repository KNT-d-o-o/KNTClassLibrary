using System;
using System.Collections.Generic;

namespace KNTCommon.Data.Models;

public partial class User
{
    public int UserId { get; set; }

    public string? UserName { get; set; }

    public byte[]? PasswordHash { get; set; }

    public byte[]? InitializationVector { get; set; }

    public int? logout { get; set; }

    public int? GroupId { get; set; }

    public virtual ICollection<UserSession> UserSessions { get; set; } = new List<UserSession>();

}
