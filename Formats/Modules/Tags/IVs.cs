using pkuManager.Formats.Fields;
using pkuManager.Formats.Modules.Templates;
using System.Numerics;
using static pkuManager.Formats.PorterDirective;

namespace pkuManager.Formats.Modules.Tags;

public interface IVs_O
{
    public IField<BigInteger[]> IVs { get; }
    public int IVs_Default => 0;
}

public interface IVs_E : MultiNumericTag_E
{
    public bool IVs_AlertIfUnspecified => true;

    [PorterDirective(ProcessingPhase.FirstPass)]
    public void ExportIVs()
    {
        IVs_O ivsObj = Data as IVs_O;
        ExportMultiNumericTag("IVs", TagUtil.STAT_NAMES, pku.IVs_Array, ivsObj.IVs, ivsObj.IVs_Default, IVs_AlertIfUnspecified);
    }
}