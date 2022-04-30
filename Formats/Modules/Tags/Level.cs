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
    [PorterDirective(ProcessingPhase.FirstPass)]
    public void ExportLevel()
    {
        Level_O levelObj = Data as Level_O;
        ExportNumericTag("Level", pku.Level, levelObj.Level, levelObj.Level_Default, true);
    }
}