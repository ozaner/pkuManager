using pkuManager.Formats.Fields;
using pkuManager.Formats.Modules.Templates;
using System.Numerics;
using static pkuManager.Formats.PorterDirective;

namespace pkuManager.Formats.Modules.Tags;

public interface TID_O
{
    public IField<BigInteger> TID { get; }
}

public interface TID_E : NumericTag_E
{
    public TID_O TID_Field { get; }

    [PorterDirective(ProcessingPhase.FirstPass)]
    public void ExportTID()
        => ExportNumericTag("TID", pku.Game_Info.TID, TID_Field.TID, 0, false);
}