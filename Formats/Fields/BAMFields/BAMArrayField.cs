using pkuManager.Utilities;
using System.Numerics;

namespace pkuManager.Formats.Fields.BAMFields;

public class BAMArrayField : BAMField, IIntegralArrayField, IByteOverridable
{
    public int Length { get; set; }

    public BigInteger[] Value
    {
        get => BitType ? BAM.GetArray(StartByte, StartBit, ByteOrBitLength, Length)
                       : BAM.GetArray(StartByte, ByteOrBitLength, Length);
        set
        {
            if (BitType)
                BAM.SetArray(StartByte, StartBit, ByteOrBitLength, value, Length);
            else
                BAM.SetArray(StartByte, ByteOrBitLength, value, Length);
        }
    }

    public BAMArrayField(ByteArrayManipulator bam, int startByte, int byteLength, int length)
        : base(bam, startByte, byteLength) => Length = length;

    public BAMArrayField(ByteArrayManipulator bam, int startByte, int startBit, int bitLength, int length)
        : base(bam, startByte, startBit, bitLength) => Length = length;

    public string GetOverride() => BitType ? $"Set Array {StartByte}:{StartBit}:{ByteOrBitLength}"
                                           : $"Set Array {StartByte}:{ByteOrBitLength}";
}