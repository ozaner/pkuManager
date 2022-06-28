using pkuManager.WinForms.Utilities;

namespace pkuManager.WinForms.Formats.Fields.BAMFields;

public class BAMBoolField : BAMIntegralField, IField<bool>
{
    //bool form of Value
    public bool ValueAsBool
    {
        get => BAM.Get<bool>(StartByte, StartBit, ByteOrBitLength);
        set => BAM.Set(value, StartByte, StartBit, ByteOrBitLength);
    }

    bool IField<bool>.Value
    {
        get => ValueAsBool;
        set => ValueAsBool = value;
    }

    public BAMBoolField(ByteArrayManipulator bam, int startByte, int startBit)
        : base(bam, startByte, startBit, 1) { }
}