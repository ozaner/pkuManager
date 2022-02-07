using pkuManager.Formats.Fields;
using System.Numerics;
using static pkuManager.Formats.PorterDirective;

namespace pkuManager.Formats.Modules;

public interface TID_O
{
    public IField<BigInteger> TID { get; }
}

public interface TID_E : NumericTag_E
{
    public TID_O Data { get; }

    public int TID_Default => 0;
    public bool TID_SilentUnspecified => true;

    [PorterDirective(ProcessingPhase.FirstPass)]
    protected void ProcessTID()
        => ProcessNumericTag("TID", pku.Game_Info.TID, Data.TID, TID_Default, TID_SilentUnspecified);
}