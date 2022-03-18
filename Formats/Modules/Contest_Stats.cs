using pkuManager.Formats.Fields;
using pkuManager.Formats.pkx;
using System.Numerics;
using static pkuManager.Formats.PorterDirective;

namespace pkuManager.Formats.Modules;

public interface Contest_Stats_O
{
    public IField<BigInteger[]> Contest_Stats { get; }
}

public interface Contest_Stats_E : MultiNumericTag
{
    public Contest_Stats_O Contest_Stats_Field { get; }

    public int Contest_Stats_Default => 0;
    public bool Contest_Stats_SilentUnspecified => true;

    [PorterDirective(ProcessingPhase.FirstPass)]
    public void ProcessContest_Stats()
        => ProcessMultiNumericTag("Contest Stats", pkxUtil.STAT_NAMES, pku.Contest_Stats_Array, Contest_Stats_Field.Contest_Stats,
                                  Contest_Stats_Default, Contest_Stats_SilentUnspecified);
}