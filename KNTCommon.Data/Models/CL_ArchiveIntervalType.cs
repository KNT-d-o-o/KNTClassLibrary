using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace KNTCommon.Data.Models;

public partial class CL_ArchiveIntervalType
{
    [Key]
    public int ArchiveIntervalTypeId { get; set; }

    public string DescriptionLang { get; set; } = null!;
}
