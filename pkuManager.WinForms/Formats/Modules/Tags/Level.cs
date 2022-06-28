using pkuManager.WinForms.Formats.Fields;
using pkuManager.WinForms.Formats.Modules.Templates;
using static pkuManager.WinForms.Alerts.Alert;
using static pkuManager.WinForms.Formats.PorterDirective;

namespace pkuManager.WinForms.Formats.Modules.Tags;

public interface Level_O
{
    public IIntField Level { get; }
    public int Level_Default => 1;
}

public interface Level_E : Tag
{
    [PorterDirective(ProcessingPhase.FirstPass)]
    public void ExportLevel()
    {
        Level_O levelObj = Data as Level_O;
        AlertType at = NumericTagUtil.ExportNumericTag(pku.Level, levelObj.Level, levelObj.Level_Default);
        Warnings.Add(NumericTagUtil.GetNumericAlert("Level", at, levelObj.Level_Default, levelObj.Level));
    }
}