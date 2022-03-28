using pkuManager.Formats.Fields;
using pkuManager.Formats.pkx;
using System.Numerics;
using static pkuManager.Formats.PorterDirective;

namespace pkuManager.Formats.Modules;

public interface EVs_O
{
    public IField<BigInteger[]> EVs { get; }
}

public interface EVs_E : MultiNumericTag_E
{
    public EVs_O EVs_Field { get; }

    [PorterDirective(ProcessingPhase.FirstPass)]
    public void ProcessEVs()
        => ProcessMultiNumericTag("EVs", pkxUtil.STAT_NAMES, pku.EVs_Array, EVs_Field.EVs, 0, false);
}