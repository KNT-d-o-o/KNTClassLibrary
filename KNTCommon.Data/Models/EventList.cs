using System;
using System.Collections.Generic;

namespace KNTCommon.Data.Models;

public partial class EventList
{
    public int EventListId { get; set; }

    public int? EventSequence { get; set; }

    public DateTime? EventDateTime { get; set; }

    public string? EventDescription { get; set; }

    public int? Thread { get; set; }
}
