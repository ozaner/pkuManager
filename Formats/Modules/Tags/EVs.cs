﻿using pkuManager.Alerts;
using pkuManager.Formats.Fields;
using pkuManager.Formats.Modules.Templates;
using System.Numerics;
using static pkuManager.Alerts.Alert;
using static pkuManager.Formats.PorterDirective;

namespace pkuManager.Formats.Modules.Tags;

public interface EVs_O
{
    public IField<BigInteger[]> EVs { get; }
}

public interface EVs_E : Tag
{
    [PorterDirective(ProcessingPhase.FirstPass)]
    public void ExportEVs()
    {
        var evs = (Data as EVs_O).EVs;
        AlertType[] ats = NumericTagUtil.ExportNumericArrayTag(pku.EVs_Array, evs, 0);
        Alert a = NumericTagUtil.GetNumericArrayAlert("EVs", TagUtil.STAT_NAMES, ats, evs as IBoundable, 0, true);
        Warnings.Add(a);
    }
}