using System;
using System.Collections.Generic;

namespace KNTCommon.Data.Models;

public partial class IoTaskDetails
{
    public int IoTaskDetailId { get; set; }

    public int IoTaskId { get; set; }

    public string? Par1 { get; set; }

    public string? Par2 { get; set; }

    public string? Par3 { get; set; }

    public string? Par4 { get; set; }

    public string? Par5 { get; set; }

    public string? Par6 { get; set; }

    public int TaskDetailOrder { get; set; }

    public string? Info { get; set; }

}

