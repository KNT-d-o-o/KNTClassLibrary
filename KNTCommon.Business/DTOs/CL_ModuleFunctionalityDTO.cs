using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace KNTSMM.Data.Models;

public partial class CL_ModuleFunctionalityDTO
{
    public int FunctionalityId { get; set; }

    public int ModuleId { get; set; }

    public required string FunctionalityName { get; set; }

    public required string DescriptionLang { get; set; }

    public bool Enabled { get; set; }
}
