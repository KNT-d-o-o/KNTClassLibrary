using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using AutoMapper.Configuration.Annotations;
using KNTSMM.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace KNTCommon.Business.DTOs;

public partial class CL_ModuleDTO
{
    public int ModuleId { get; set; }

    public required string ModuleName { get; set; }

    public bool Enabled { get; set; }

    public required string DescriptionLang { get; set; }

    [Ignore]
    public List<CL_ModuleFunctionalityDTO> ModuleFunctionalityDTO { get; set; } = new();
}
