using pkuManager.Formats.Fields;
using pkuManager.Formats.Modules.Templates;
using System.Numerics;
using static pkuManager.Formats.PorterDirective;

namespace pkuManager.Formats.Modules.Tags;

public interface Met_Level_O
{
    public IField<BigInteger> Met_Level { get; }
}

public interface Met_Level_E : NumericTag_E
{
    public Met_Level_O Met_Level_Field { get; }

    [PorterDirective(ProcessingPhase.FirstPass)]
    public void ProcessMet_Level()
        => ProcessNumericTag("Met Level", pku.Catch_Info.Met_Level, Met_Level_Field.Met_Level, 0, false);
}