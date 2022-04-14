using pkuManager.Formats.Fields;
using pkuManager.Formats.Modules.Templates;
using pkuManager.Formats.pku;
using System.Numerics;
using static pkuManager.Formats.PorterDirective;

namespace pkuManager.Formats.Modules.Tags;

public interface Contest_Stats_O
{
    public IField<BigInteger[]> Contest_Stats { get; }
}

public interface Contest_Stats_E : MultiNumericTag_E
{
    public pkuObject pku { get; }

    public Contest_Stats_O Contest_Stats_Field { get; }

    [PorterDirective(ProcessingPhase.FirstPass)]
    public void ExportContest_Stats()
        => ExportMultiNumericTag("Contest Stats", TagUtil.STAT_NAMES, pku.Contest_Stats_Array,
            Contest_Stats_Field.Contest_Stats, 0, false);
}