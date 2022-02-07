using pkuManager.Utilities;

namespace pkuManager.Formats.Fields.BAMFields;

public class BAMBoolField : BAMField, IField<bool>, IByteOverridable
{
    public bool Value
    {
        get => BAM.Get<bool>(StartByte, StartBit, ByteOrBitLength);
        set => BAM.Set(value, StartByte, StartBit, ByteOrBitLength);
    }

    public BAMBoolField(ByteArrayManipulator bam, int startByte, int startBit)
        : base(bam, startByte, startBit, 1) { }

    public string GetOverride() => $"Set {StartByte}:{StartBit}:{ByteOrBitLength}";
}