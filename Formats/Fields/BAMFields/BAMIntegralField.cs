using pkuManager.Utilities;
using System.Numerics;

namespace pkuManager.Formats.Fields.BAMFields;

public class BAMIntegralField : BAMField, IField<BigInteger>
{
    public BigInteger Value
    {
        get => BitType ? BAM.Get(StartByte, StartBit, ByteOrBitLength)
                       : BAM.Get(StartByte, ByteOrBitLength);
        set
        {
            if (BitType)
                BAM.Set(value, StartByte, StartBit, ByteOrBitLength);
            else
                BAM.Set(value, StartByte, ByteOrBitLength);
        }
    }

    public BAMIntegralField(ByteArrayManipulator bam, int startByte, int byteLength)
        : base(bam, startByte, byteLength) { }

    public BAMIntegralField(ByteArrayManipulator bam, int startByte, int startBit, int bitLength)
        : base(bam, startByte, startBit, bitLength) { }

    public override string GetOverride() => BitType ? $"Set {StartByte}:{StartBit}:{ByteOrBitLength}"
                                                    : $"Set {StartByte}:{ByteOrBitLength}";
}