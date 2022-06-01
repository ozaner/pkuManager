using pkuManager.Formats.Fields;
using pkuManager.Formats.Modules.Templates;
using System.Numerics;
using static pkuManager.Alerts.Alert;
using static pkuManager.Formats.PorterDirective;

namespace pkuManager.Formats.Modules.Tags;

public interface Met_Level_O
{
    public IField<BigInteger> Met_Level { get; }
}

public interface Met_Level_E : Tag
{
    [PorterDirective(ProcessingPhase.FirstPass)]
    public void ExportMet_Level()
    {
        Met_Level_O metlevelObj = Data as Met_Level_O;
        AlertType at = NumericTagUtil.ExportNumericTag(pku.Catch_Info.Met_Level, metlevelObj.Met_Level, 0);
        if (at is not AlertType.UNSPECIFIED) //ignore unspecified
            Warnings.Add(NumericTagUtil.GetNumericAlert("Met Level", at, 0, metlevelObj.Met_Level as IBoundable));
    }
}