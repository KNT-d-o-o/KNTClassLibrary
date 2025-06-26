using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KNTCommon.Business.DTOs;

public partial class IoTaskLogsDTO
{
    public int IoTaskLogId { get; set; }

    public int? IoTaskId { get; set; }

    public int? IoTaskLogType { get; set; }

    public string? Info { get; set; }

    public DateTime? DateAndTime { get; set; }

}