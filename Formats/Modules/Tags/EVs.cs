using pkuManager.Formats.Fields;
using pkuManager.Formats.Modules.Templates;
using System.Numerics;
using static pkuManager.Formats.PorterDirective;

namespace pkuManager.Formats.Modules.Tags;

public interface EVs_O
{
    public IField<BigInteger[]> EVs { get; }
}

public interface EVs_E : MultiNumericTag_E
{
    [PorterDirective(ProcessingPhase.FirstPass)]
    public void ExportEVs()
    {
        EVs_O evsObj = Data as EVs_O;
        ExportMultiNumericTag("EVs", TagUtil.STAT_NAMES, pku.EVs_Array, evsObj.EVs, 0, false);
    }
}