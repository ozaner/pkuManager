using pkuManager.Formats.Fields;
using pkuManager.Formats.Modules.Templates;
using System.Numerics;
using static pkuManager.Formats.PorterDirective;

namespace pkuManager.Formats.Modules.Tags;

public interface Level_O
{
    public IField<BigInteger> Level { get; }
    public int Level_Default => 1;
}

public interface Level_E : NumericTag_E
{
    public Level_O Level_Field { get; }

    [PorterDirective(ProcessingPhase.FirstPass)]
    public void ExportLevel()
        => ExportNumericTag("Level", pku.Level, Level_Field.Level, Level_Field.Level_Default, true);
}