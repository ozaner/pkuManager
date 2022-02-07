using pkuManager.Formats.Fields;
using System.Numerics;
using static pkuManager.Formats.PorterDirective;

namespace pkuManager.Formats.Modules;

public interface Met_Level_O
{
    public IField<BigInteger> Met_Level { get; }
}

public interface Met_Level_E : NumericTag_E
{
    public Met_Level_O Data { get; }

    public int Met_Level_Default => 0;
    public bool Met_Level_SilentUnspecified => true;

    [PorterDirective(ProcessingPhase.FirstPass)]
    protected void ProcessMet_Level()
        => ProcessNumericTag("Met Level", pku.Catch_Info.Met_Level, Data.Met_Level, Met_Level_Default, Met_Level_SilentUnspecified);
}