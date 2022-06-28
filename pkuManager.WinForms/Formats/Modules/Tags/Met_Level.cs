using pkuManager.WinForms.Formats.Fields;
using pkuManager.WinForms.Formats.Modules.Templates;
using static pkuManager.WinForms.Alerts.Alert;
using static pkuManager.WinForms.Formats.PorterDirective;

namespace pkuManager.WinForms.Formats.Modules.Tags;

public interface Met_Level_O
{
    public IIntField Met_Level { get; }
}

public interface Met_Level_E : Tag
{
    [PorterDirective(ProcessingPhase.FirstPass)]
    public void ExportMet_Level()
    {
        Met_Level_O metlevelObj = Data as Met_Level_O;
        AlertType at = NumericTagUtil.ExportNumericTag(pku.Catch_Info.Met_Level, metlevelObj.Met_Level, 0);
        if (at is not AlertType.UNSPECIFIED) //ignore unspecified
            Warnings.Add(NumericTagUtil.GetNumericAlert("Met Level", at, 0, metlevelObj.Met_Level));
    }
}