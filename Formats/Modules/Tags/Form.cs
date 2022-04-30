﻿using OneOf;
using pkuManager.Alerts;
using pkuManager.Formats.Fields;
using pkuManager.Utilities;
using System.Numerics;
using static pkuManager.Formats.PorterDirective;

namespace pkuManager.Formats.Modules.Tags;

public interface Form_O
{
    public OneOf<IField<BigInteger>, IField<string>> Form { get; }
}

public interface Form_E : Tag
{
    [PorterDirective(ProcessingPhase.FirstPass)]
    public void ExportForm() => ExportFormBase();

    public void ExportFormBase()
    {
        (Data as Form_O).Form.Switch(
            x => x.SetAs(DexUtil.GetSpeciesIndexedValue<int?>(pku, FormatName, "Form Indices").Value), //int index
            x => x.Value = DexUtil.GetSpeciesIndexedValue<string>(pku, FormatName, "Form Indices") //string index
        );
        Warnings.Add(GetFormAlert(pku));
    }

    public Alert GetFormAlert(DexUtil.SFA sfa) => null;
}