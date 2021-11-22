using pkuManager.Utilities;
using System;

namespace pkuManager.Formats.Fields.BAMFields;

public class BAMBoolField : Field<bool>, IBAMField, IByteOverridable
{
    // IBAMField
    public ByteArrayManipulator BAM { get; }
    public bool BitType => true;
    public int StartByte { get; }
    public int StartBit { get; }
    public int BitLength => 1;
    public int ByteLength => throw new NotImplementedException();

    // Field
    protected override bool Value
    {
        get => BAM.Get<bool>(StartByte, StartBit, BitLength);
        set => BAM.Set(value, StartByte, StartBit, BitLength);
    }

    public BAMBoolField(ByteArrayManipulator bam, int startByte, int startBit,
        Func<bool, bool> getter = null, Func<bool, bool> setter = null) : base(getter, setter)
    {
        BAM = bam;
        StartByte = startByte;
        StartBit = startBit;
    }

    public string GetOverride() => $"Set {StartByte}:{StartBit}:{BitLength}";
}