using pkuManager.Formats.Fields;
using pkuManager.Formats.Modules.Templates;
using System.Numerics;
using static pkuManager.Formats.PorterDirective;

namespace pkuManager.Formats.Modules.Tags;

public interface Contest_Stats_O
{
    public IField<BigInteger[]> Contest_Stats { get; }
}

public interface Contest_Stats_E : MultiNumericTag_E
{
    [PorterDirective(ProcessingPhase.FirstPass)]
    public void ExportContest_Stats()
        => ExportMultiNumericTag("Contest Stats", TagUtil.STAT_NAMES, pku.Contest_Stats_Array,
            (Data as Contest_Stats_O).Contest_Stats, 0, false);
}