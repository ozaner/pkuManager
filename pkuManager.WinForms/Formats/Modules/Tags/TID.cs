﻿using pkuManager.WinForms.Formats.Fields;
using pkuManager.WinForms.Formats.Modules.Templates;
using static pkuManager.WinForms.Alerts.Alert;
using static pkuManager.WinForms.Formats.PorterDirective;

namespace pkuManager.WinForms.Formats.Modules.Tags;

public interface TID_O
{
    public IIntField TID { get; }
}

public interface TID_E : Tag
{
    [PorterDirective(ProcessingPhase.FirstPass)]
    public void ExportTID()
    {
        TID_O tidObj = Data as TID_O;
        AlertType at = NumericTagUtil.ExportNumericTag(pku.Game_Info.TID, tidObj.TID, 0);
        if (at is not AlertType.UNSPECIFIED) //ignore unspecified
            Warnings.Add(NumericTagUtil.GetNumericAlert("TID", at, 0, tidObj.TID));
    }
}

public interface TID_I : Tag
{
    [PorterDirective(ProcessingPhase.FirstPass)]
    public void ImportTID()
        => NumericTagUtil.ImportNumericTag(pku.Game_Info.TID, (Data as TID_O).TID);
}