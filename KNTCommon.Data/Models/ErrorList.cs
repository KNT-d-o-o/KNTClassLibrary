using System;
using System.Collections.Generic;

namespace KNTCommon.Data.Models;

public partial class ErrorList
{
    public int ErrorListId { get; set; }

    public DateTime? ErrorDateTime { get; set; }

    public string? ErrorDescription { get; set; }

    public string? ErrorType { get; set; }

    public int? ErrorMeasurementNum { get; set; }

    public int? Thread { get; set; }
}
