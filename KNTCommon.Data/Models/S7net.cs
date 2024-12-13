using System;
using System.Collections.Generic;

namespace KNTCommon.Data.Models;

public partial class S7net
{
    public int S7NetId { get; set; }

    public int? Thread { get; set; }

    public string? Start { get; set; }

    public string? ThreadTriggered { get; set; }

    public string? StopTheCycle { get; set; }

    public string? InterruptTheProcess { get; set; }

    public string? ProgTriggered { get; set; }

    public string? SpecialInputs { get; set; }

    public string? MeasNumFromMachine { get; set; }

    public string? InputString { get; set; }

    public int? InputStringDb { get; set; }

    public int? InputStringStartAt { get; set; }

    public int? InputStringSize { get; set; }

    public string? GoodImpregnationScrap { get; set; }

    public string? ThreadMeasured { get; set; }

    public string? WaitMachine { get; set; }

    public string? Error { get; set; }

    public string? ProgNumMeasured { get; set; }

    public string? Phase { get; set; }

    public string? Warning { get; set; }

    public string? MeasNum { get; set; }

    public string? LeakResult { get; set; }

    public string? LeakNominalImpregnation { get; set; }

    public string? LeakNominalScrap { get; set; }

    public string? LeakNominalImpregnationMinus { get; set; }

    public string? LeakNominalScrapMinus { get; set; }

    public string? MachineId { get; set; }

    public string? SpecialOutputs { get; set; }

    public string? ByteString { get; set; }

    public int? ByteStringDb { get; set; }

    public int? ByteStringStartsAt { get; set; }

    public int? ByteStringSize { get; set; }
}
