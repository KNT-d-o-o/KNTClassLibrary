using System;
using System.Collections.Generic;

namespace KNTCommon.Data.Models;

public partial class LanguageDictionary
{
    public int LanguageDictionaryId { get; set; }

    public string? code { get; set; }

    public string? English { get; set; }

    public string? Slovenski { get; set; }

    public string? Deutsche { get; set; }

    public string? Francaise { get; set; }

    public string? Hrvatski { get; set; }

    public string? Srpski { get; set; }
}
