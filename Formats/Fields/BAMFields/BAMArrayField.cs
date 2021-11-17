using pkuManager.Utilities;
using System;
using System.Numerics;

namespace pkuManager.Formats.Fields.BAMFields;

public class BAMArrayField : ArrayField<BigInteger>, IByteOverridable
{
    private readonly BAMFieldInfo bfi;

    protected override BigInteger[] Value
    {
        get => bfi.BitType ? bfi.BAM.GetArray(bfi.StartByte, bfi.StartBit, bfi.BitLength, Length)
                           : bfi.BAM.GetArray(bfi.StartByte, bfi.ByteLength, Length);
        set
        {
            if (bfi.BitType)
                bfi.BAM.SetArray(bfi.StartByte, bfi.StartBit, bfi.BitLength, value, Length);
            else
                bfi.BAM.SetArray(bfi.StartByte, bfi.ByteLength, value, Length);
        }
    }

    public override int Length { get; }

    private BigInteger[] arraySetterBound(BigInteger[] vals)
    {
        if (vals.Length != Length)
            throw new ArgumentException("The vals to be set must have the same length as the field.", nameof(vals));
        foreach (var x in vals)
            bfi.SetterBound(x);
        return vals;
    }

    public BAMArrayField(ByteArrayManipulator bam, int startByte, int byteLength, int length,
        Func<BigInteger[], BigInteger[]> getter = null, Func<BigInteger[], BigInteger[]> setter = null) : base(getter, setter)
    {
        Length = length;
        bfi = new(bam, startByte, byteLength);
        CustomSetter = CustomSetter.Compose(arraySetterBound);
    }

    public BAMArrayField(ByteArrayManipulator bam, int startByte, int startBit, int bitLength, int length,
        Func<BigInteger[], BigInteger[]> getter = null, Func<BigInteger[], BigInteger[]> setter = null) : base(getter, setter)
    {
        Length = length;
        bfi = new(bam, startByte, startBit, bitLength);
        CustomSetter = CustomSetter.Compose(arraySetterBound);
    }

    public string GetOverride() => bfi.BitType ? $"Set Array {bfi.StartByte}:{bfi.StartBit}:{bfi.BitLength}"
                                               : $"Set Array {bfi.StartByte}:{bfi.ByteLength}";

    public T[] Get<T>() where T : struct
        => Array.ConvertAll(Get(), x => x.BigIntegerTo<T>());

    public T Get<T>(int index) where T : struct
        => Get(index).BigIntegerTo<T>();

    public void Set<T>(T[] vals) where T : struct
        => base.Set(Array.ConvertAll(vals, x => x.ToBigInteger()));

    public void Set<T>(T val, int index) where T : struct
        => base.Set(val.ToBigInteger(), index);
}