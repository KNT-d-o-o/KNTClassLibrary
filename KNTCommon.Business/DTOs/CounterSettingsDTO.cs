using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace KNTCommon.Business.DTOs;

public partial class CounterSettingsDTO
{
    public int Panels_Counter_Visibility { get; set; }

    public int Panels_Counter_Selected_Tab { get; set; }
    
}