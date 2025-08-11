using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace KNTCommon.Business.DTOs;

public partial class APP_SettingDTO
{
    public int SettingId { get; set; }

    public string SettingKey { get; set; } = null!;

    public required string SettingValue { get; set; }

    public string? Comment { get; set; }
}
