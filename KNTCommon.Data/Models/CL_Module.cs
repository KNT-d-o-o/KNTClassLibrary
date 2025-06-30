using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace KNTSMM.Data.Models;

public partial class CL_Module
{
    [Key]
    public int ModuleId { get; set; }

    public string ModuleName { get; set; }

    public string DescriptionLang { get; set; }

    public bool Enabled { get; set; }
}
