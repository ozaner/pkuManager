using pkuManager.Formats.Fields;
using pkuManager.Formats.Modules.Templates;
using System.Numerics;
using static pkuManager.Alerts.Alert;
using static pkuManager.Formats.PorterDirective;

namespace pkuManager.Formats.Modules.Tags;

public interface Level_O
{
    public IField<BigInteger> Level { get; }
    public int Level_Default => 1;
}

public interface Level_E : Tag
{
    [PorterDirective(ProcessingPhase.FirstPass)]
    public void ExportLevel()
    {
        Level_O levelObj = Data as Level_O;
        AlertType at = NumericTagUtil.ExportNumericTag(pku.Level, levelObj.Level, levelObj.Level_Default);
        Warnings.Add(NumericTagUtil.GetNumericAlert("Level", at, levelObj.Level_Default, levelObj.Level as IBoundable));
    }
}