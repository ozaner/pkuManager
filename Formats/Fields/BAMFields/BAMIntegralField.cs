using pkuManager.Utilities;
using System;
using System.Numerics;

namespace pkuManager.Formats.Fields.BAMFields;

public class BAMIntegralField : Field<BigInteger>, IByteOverridable
{
    private readonly BAMFieldInfo bfi;

    protected override BigInteger Value
    {
        get => bfi.BitType ? bfi.BAM.Get(bfi.StartByte, bfi.StartBit, bfi.BitLength)
                           : bfi.BAM.Get(bfi.StartByte, bfi.ByteLength);
        set
        {
            if (bfi.BitType)
                bfi.BAM.Set(value, bfi.StartByte, bfi.StartBit, bfi.BitLength);
            else
                bfi.BAM.Set(value, bfi.StartByte, bfi.ByteLength);
        }
    }

    public BAMIntegralField(ByteArrayManipulator bam, int startByte, int byteLength,
        Func<BigInteger, BigInteger> getter = null, Func<BigInteger, BigInteger> setter = null) : base(getter, setter)
    {
        bfi = new BAMFieldInfo(bam, startByte, byteLength);
        CustomSetter = CustomSetter.Compose(bfi.SetterBound);
    }

    public BAMIntegralField(ByteArrayManipulator bam, int startByte, int startBit, int bitLength,
        Func<BigInteger, BigInteger> getter = null, Func<BigInteger, BigInteger> setter = null) : base(getter, setter)
    {
        bfi = new BAMFieldInfo(bam, startByte, startBit, bitLength);
        CustomSetter = CustomSetter.Compose(bfi.SetterBound);
    }

    public string GetOverride() => bfi.BitType ? $"Set {bfi.StartByte}:{bfi.StartBit}:{bfi.BitLength}"
                                               : $"Set {bfi.StartByte}:{bfi.ByteLength}";

    public T Get<T>() where T : struct
        => Get().BigIntegerTo<T>();

    public void Set<T>(T val) where T : struct
        => base.Set(val.ToBigInteger());
}