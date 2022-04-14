using pkuManager.Formats.Fields;
using pkuManager.Formats.Modules.Templates;
using pkuManager.Formats.pku;
using System.Numerics;
using static pkuManager.Formats.PorterDirective;

namespace pkuManager.Formats.Modules.Tags;

public interface EVs_O
{
    public IField<BigInteger[]> EVs { get; }
}

public interface EVs_E : MultiNumericTag_E
{
    public pkuObject pku { get; }

    public EVs_O EVs_Field { get; }

    [PorterDirective(ProcessingPhase.FirstPass)]
    public void ExportEVs()
        => ExportMultiNumericTag("EVs", TagUtil.STAT_NAMES, pku.EVs_Array, EVs_Field.EVs, 0, false);
}