using pkuManager.Formats.Fields;
using pkuManager.Formats.Modules.Templates;
using System.Numerics;
using static pkuManager.Alerts.Alert;
using static pkuManager.Formats.PorterDirective;

namespace pkuManager.Formats.Modules.Tags;

public interface TID_O
{
    public IField<BigInteger> TID { get; }
}

public interface TID_E : Tag
{
    [PorterDirective(ProcessingPhase.FirstPass)]
    public void ExportTID()
    {
        TID_O tidObj = Data as TID_O;
        AlertType at = NumericTagUtil.ExportNumericTag(pku.Game_Info.TID, tidObj.TID, 0);
        if (at is not AlertType.UNSPECIFIED) //ignore unspecified
            Warnings.Add(NumericTagUtil.GetNumericAlert("TID", at, 0, tidObj.TID as IBoundable<BigInteger>));
    }
}