using pkuManager.Utilities;
using System;

namespace pkuManager.Formats.Fields.BAMFields;

public class BAMBoolField : Field<bool>, IByteOverridable
{
    private readonly BAMFieldInfo bfi;

    protected override bool Value
    {
        get => bfi.BAM.Get<bool>(bfi.StartByte, bfi.StartBit, 1);
        set => bfi.BAM.Set(value, bfi.StartByte, bfi.StartBit, 1);
    }

    public BAMBoolField(ByteArrayManipulator bam, int startByte, int startBit,
        Func<bool, bool> getter = null, Func<bool, bool> setter = null) : base(getter, setter)
        => bfi = new BAMFieldInfo(bam, startByte, startBit, 1);

    public string GetOverride() => $"Set {bfi.StartByte}:{bfi.StartBit}:1";
}