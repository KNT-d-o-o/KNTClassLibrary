using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace KNTCommon.Data.Models;

public partial class CL_Module
{
    [Key]
    public int ModuleId { get; set; }

    public required string ModuleName { get; set; }

    public required string DescriptionLang { get; set; }

    public bool Enabled { get; set; }
}
