using pkuManager.Formats.Fields;
using pkuManager.Formats.pkx;
using System.Numerics;
using static pkuManager.Formats.PorterDirective;

namespace pkuManager.Formats.Modules;

public interface Contest_Stats_O
{
    public IField<BigInteger[]> Contest_Stats { get; }
}

public interface Contest_Stats_E : MultiNumericTag_E
{
    public Contest_Stats_O Contest_Stats_Field { get; }

    [PorterDirective(ProcessingPhase.FirstPass)]
    public void ProcessContest_Stats()
        => ProcessMultiNumericTag("Contest Stats", pkxUtil.STAT_NAMES, pku.Contest_Stats_Array,
            Contest_Stats_Field.Contest_Stats, 0, false);
}