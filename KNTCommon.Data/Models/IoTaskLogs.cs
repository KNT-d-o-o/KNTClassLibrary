using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KNTCommon.Data.Models;

public partial class IoTaskLogs
{
    [Key] // Primary key
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)] // AUTO_INCREMENT
    public int IoTaskLogId { get; set; }

    public int? IoTaskId { get; set; }

    public int? IoTaskLogType { get; set; }

    public string? Info { get; set; }

    public DateTime? DateAndTime { get; set; }

}