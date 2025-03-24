using System;
using System.Collections.Generic;

namespace KNTCommon.Data.Models;

public partial class ServiceControl
{
    public int ServiceId { get; set; }

    public required string ServiceName { get; set; }

    public string? ServiceTitle { get; set; }

    public int? Status { get; set; }

}
