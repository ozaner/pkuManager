using OneOf;
using pkuManager.WinForms.Formats.Fields;
using pkuManager.WinForms.Formats.Modules.Templates;
using static pkuManager.WinForms.Alerts.Alert;
using static pkuManager.WinForms.Formats.PorterDirective;

namespace pkuManager.WinForms.Formats.Modules.Tags;

public interface Met_Date_O
{
    OneOf<(IIntField Y, IIntField M, IIntField D), IIntField> Met_Date { get; }
}

public interface Met_Date_E : Tag
{
    [PorterDirective(ProcessingPhase.FirstPass)]
    public void ExportMet_Date()
    {
        AlertType at = DateTagUtil.ExportDate(pku.Catch_Info.Met_Date, (Data as Met_Date_O).Met_Date);
        Warnings.Add(DateTagUtil.GetDateAlert("Met Date", at));
    }
}