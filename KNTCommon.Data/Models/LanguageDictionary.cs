using System;
using System.Collections.Generic;

namespace KNTCommon.Data.Models;

public partial class LanguageDictionary
{
    public int LanguageDictionaryId { get; set; }

    public string? Key { get; set; }

    public string? code { get; set; }

    public string? English { get; set; }

    public string? Slovene { get; set; }

    public string? German { get; set; }

    public string? Croatian { get; set; }

    public string? Serbian { get; set; }
}
