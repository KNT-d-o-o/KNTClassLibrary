using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace KNTSMM.Data.Models;

public partial class CL_ArchiveMode
{
    [Key]
    public int ArchiveModeId { get; set; }

    public string DescriptionLang { get; set; } = null!;

    public string Name { get; set; } = null!;
}
