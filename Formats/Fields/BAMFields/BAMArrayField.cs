using pkuManager.Utilities;
using System;
using System.Numerics;

namespace pkuManager.Formats.Fields.BAMFields;

public class BAMArrayField : IntegralArrayField, IByteOverridable
{
    private readonly BAMFieldInfo bfi;

    public override int Length { get; }

    public BAMArrayField(ByteArrayManipulator bam, int startByte, int byteLength, int length,
        Func<BigInteger[], BigInteger[]> getter = null, Func<BigInteger[], BigInteger[]> setter = null) : base(getter, setter)
    {
        Length = length;
        bfi = new BAMFieldInfo(bam, startByte, byteLength);
    }

    public BAMArrayField(ByteArrayManipulator bam, int startByte, int startBit, int bitLength, int length,
        Func<BigInteger[], BigInteger[]> getter = null, Func<BigInteger[], BigInteger[]> setter = null) : base(getter, setter)
    {
        Length = length;
        bfi = new BAMFieldInfo(bam, startByte, startBit, bitLength);
    }

    protected override BigInteger[] GetRaw()
        => bfi.BitType ? bfi.BAM.GetArray(bfi.StartByte, bfi.StartBit, bfi.BitLength, Length)
                       : bfi.BAM.GetArray(bfi.StartByte, bfi.ByteLength, Length);

    protected override void SetRaw(BigInteger[] vals)
    {
        if (bfi.BitType)
            bfi.BAM.SetArray(bfi.StartByte, bfi.StartBit, bfi.BitLength, vals, Length);
        else
            bfi.BAM.SetArray(bfi.StartByte, bfi.ByteLength, vals, Length);
    }

    public string GetOverride() => bfi.BitType ? $"Set Array {bfi.StartByte}:{bfi.StartBit}:{bfi.BitLength}"
                                               : $"Set Array {bfi.StartByte}:{bfi.ByteLength}";

    public override BigInteger Max => bfi.Max;
    public override BigInteger Min => bfi.Min;
}