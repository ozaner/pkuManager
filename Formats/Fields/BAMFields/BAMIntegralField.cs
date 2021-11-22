using pkuManager.Utilities;
using System;
using System.Numerics;

namespace pkuManager.Formats.Fields.BAMFields;

public class BAMIntegralField : IntegralField, IBAMField, IByteOverridable
{
    // Field
    protected override BigInteger Value
    {
        get => BitType ? BAM.Get(StartByte, StartBit, BitLength)
                       : BAM.Get(StartByte, ByteLength);
        set
        {
            if (BitType)
                BAM.Set(value, StartByte, StartBit, BitLength);
            else
                BAM.Set(value, StartByte, ByteLength);
        }
    }

    // IntegralField
    public override BigInteger Max => (this as IBAMField).GetMax();
    public override BigInteger Min => (this as IBAMField).GetMin();

    // IBAMField
    public ByteArrayManipulator BAM { get; }
    public bool BitType { get; }
    public int StartByte { get; }
    public int ByteLength { get; }
    public int StartBit { get; }
    public int BitLength { get; }


    public BAMIntegralField(ByteArrayManipulator bam, int startByte, int byteLength,
        Func<BigInteger, BigInteger> getter = null, Func<BigInteger, BigInteger> setter = null) : base(getter, setter)
    {
        BAM = bam;
        BitType = false;
        StartByte = startByte;
        ByteLength = byteLength;
    }

    public BAMIntegralField(ByteArrayManipulator bam, int startByte, int startBit, int bitLength,
        Func<BigInteger, BigInteger> getter = null, Func<BigInteger, BigInteger> setter = null) : base(getter, setter)
    {
        BAM = bam;
        BitType = true;
        StartByte = startByte;
        StartBit = startBit;
        BitLength = bitLength;
    }

    public string GetOverride() => BitType ? $"Set {StartByte}:{StartBit}:{BitLength}"
                                           : $"Set {StartByte}:{ByteLength}";
}