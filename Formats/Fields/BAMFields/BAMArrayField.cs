using pkuManager.Utilities;
using System;
using System.Numerics;

namespace pkuManager.Formats.Fields.BAMFields;

public class BAMArrayField : IntegralArrayField, IBAMField, IByteOverridable
{
    // Field
    protected override BigInteger[] Value
    {
        get => BitType ? BAM.GetArray(StartByte, StartBit, BitLength, Length)
                       : BAM.GetArray(StartByte, ByteLength, Length);
        set
        {
            if (BitType)
                BAM.SetArray(StartByte, StartBit, BitLength, value, Length);
            else
                BAM.SetArray(StartByte, ByteLength, value, Length);
        }
    }

    // ArrayField
    public override int Length { get; }

    // IntegralArrayField
    public override BigInteger Max => (this as IBAMField).GetMax();
    public override BigInteger Min => (this as IBAMField).GetMin();

    // IBAMField
    public ByteArrayManipulator BAM { get; }
    public bool BitType { get; }
    public int StartByte { get; }
    public int ByteLength { get; }
    public int StartBit { get; }
    public int BitLength { get; }


    public BAMArrayField(ByteArrayManipulator bam, int startByte, int byteLength, int length,
        Func<BigInteger[], BigInteger[]> getter = null, Func<BigInteger[], BigInteger[]> setter = null) : base(getter, setter)
    {
        BAM = bam;
        BitType = false;
        StartByte = startByte;
        ByteLength = byteLength;
        Length = length;
    }

    public BAMArrayField(ByteArrayManipulator bam, int startByte, int startBit, int bitLength, int length,
        Func<BigInteger[], BigInteger[]> getter = null, Func<BigInteger[], BigInteger[]> setter = null) : base(getter, setter)
    {
        BAM = bam;
        BitType = true;
        StartByte = startByte;
        StartBit = startBit;
        BitLength = bitLength;
        Length = length;
    }

    public string GetOverride() => BitType ? $"Set Array {StartByte}:{StartBit}:{BitLength}"
                                           : $"Set Array {StartByte}:{ByteLength}";
}