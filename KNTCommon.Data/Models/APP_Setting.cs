using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace KNTCommon.Data.Models;

public partial class APP_Setting
{
    [Key]
    public int SettingId { get; set; }

    public string SettingKey { get; set; } = null!;

    public string SettingValue { get; set; }

    public string? Comment { get; set; }
}
