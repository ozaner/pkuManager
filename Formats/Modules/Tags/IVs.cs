using pkuManager.Alerts;
using pkuManager.Formats.Fields;
using pkuManager.Formats.Modules.Templates;
using static pkuManager.Alerts.Alert;
using static pkuManager.Formats.PorterDirective;

namespace pkuManager.Formats.Modules.Tags;

public interface IVs_O
{
    public IIntArrayField IVs { get; }
    public int IVs_Default => 0;
}

public interface IVs_E : Tag
{
    public bool IVs_AlertIfUnspecified => true;

    [PorterDirective(ProcessingPhase.FirstPass)]
    public void ExportIVs()
    {
        IVs_O ivsObj = (Data as IVs_O);
        var ivs = ivsObj.IVs;
        AlertType[] ats = NumericTagUtil.ExportNumericArrayTag(pku.IVs_Array, ivs, ivsObj.IVs_Default);
        Alert a = NumericTagUtil.GetNumericArrayAlert("IVs", TagUtil.STAT_NAMES, ats, ivs, ivsObj.IVs_Default, IVs_AlertIfUnspecified);
        Warnings.Add(a);
    }
}