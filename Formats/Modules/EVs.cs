using pkuManager.Formats.Fields;
using pkuManager.Formats.pkx;
using static pkuManager.Formats.PorterDirective;

namespace pkuManager.Formats.Modules;

public interface EVs_O
{
    public IntegralArrayField EVs { get; }
}

public interface EVs_E : MultiNumericTag
{
    public EVs_O Data { get; }

    public int EVs_Default => 0;
    public bool EVs_SilentUnspecified => true;

    [PorterDirective(ProcessingPhase.FirstPass)]
    public void ProcessEVs()
        => ProcessMultiNumericTag("EVs", pkxUtil.STAT_NAMES, pku.EVs_Array, Data.EVs, EVs_Default, EVs_SilentUnspecified);
}