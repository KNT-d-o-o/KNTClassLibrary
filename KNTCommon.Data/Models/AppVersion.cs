using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace KNTCommon.Data.Models;

public partial class App_Version
{
    [Key]
    public int IdAppVersion { get; set; }

    public string VersionNumber { get; set; } = null!;

    public DateTime DateAndTime { get; set; }
}
