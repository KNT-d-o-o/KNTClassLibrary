using System;
using System.Collections.Generic;

namespace KNTCommon.Data.Models;

public partial class Parameter
{
    public int ParametersId { get; set; }

    public string? ParName { get; set; }

    public string? ParValue { get; set; }

    public string? ParNote { get; set; }
}
