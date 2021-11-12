using pkuManager.Utilities;
using System;

namespace pkuManager.Formats.Fields.BAMFields;

public class BAMBoolField : Field<bool>, IByteOverridable
{
    private readonly BAMFieldInfo bfi;

    public BAMBoolField(ByteArrayManipulator bam, int startByte, int startBit,
        Func<bool, bool> getter = null, Func<bool, bool> setter = null) : base(getter, setter)
        => bfi = new BAMFieldInfo(bam, startByte, startBit, 1);

    protected override bool GetRaw()
        => bfi.BAM.Get<bool>(bfi.StartByte, bfi.StartBit, 1);

    protected override void SetRaw(bool val)
        => bfi.BAM.Set(val, bfi.StartByte, bfi.StartBit, 1);

    public string GetOverride() => $"Set {bfi.StartByte}:{bfi.StartBit}:1";
}