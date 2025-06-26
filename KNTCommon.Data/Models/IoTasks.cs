using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace KNTCommon.Data.Models;

public partial class IoTasks
{
    [Key]
    public int IoTaskId { get; set; }

    public string? IoTaskName { get; set; }

    public int IoTaskType { get; set; }

    public int IoTaskMode { get; set; }

    public int Priority { get; set; }

    public string? Par1 { get; set; }

    public string? TimeCriteria { get; set; }

    public DateTime? ExecuteDateAndTime { get; set; }

    public int? Status { get; set; }

    public string? Info { get; set; }
}

