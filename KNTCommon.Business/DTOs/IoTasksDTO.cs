using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using AutoMapper.Configuration.Annotations;
using Microsoft.EntityFrameworkCore;

namespace KNTCommon.Business.DTOs;

public partial class IoTasksDTO
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

    [Ignore]
    public TimeCriteriaModel TimeCriteriaModel { get; set; } = new();
}


public partial class TimeCriteriaModel
{
    public bool AutomaticArchiving { get; set; }
    public int ArchiveMode { get; set; }
    public bool NextMonth { get; set; }
    public int? EveryMonthOnDay { get; set; }
    public decimal? AddMinutes { get; set; }
    public decimal? AddDays { get; set; }
    public decimal? AddHours { get; set; }
    public int? Time { get; set; }
    public DateTime? EveryMonthOnHour { get; set; }
    public DateTime? ArchiveIntervalSelectedHour { get; set; }
    public DateTime? ArchiveIntervalStartDate { get; set; }
    public string? ExportLocation { get; set; }
    public int? ArchiveIntervalType { get; set; }

    public bool SaveAsZip { get; set; }


    public decimal? ArchiveIntervalTypeAmount { get; set; }
    public string? ArchiveIntervalTypeDescription { get; set; }        
    public string? ArchiveModeDescription { get; set; }
}

public enum ArchiveIntervalType
{
    Day =1,
    Hour,
    Minute
}

public enum ArchiveMode
{
    SetDate = 1,
    Interval
}