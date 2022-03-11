using pkuManager.Formats.Modules;
using pkuManager.Utilities;
using System.Numerics;

namespace pkuManager.Formats.Fields.BAMFields;

public abstract class BAMField : IBoundable<BigInteger>, IByteOverridable
{
    protected ByteArrayManipulator BAM { get; }
    protected bool BitType { get; }

    protected int StartByte { get; }
    protected int StartBit { get; }
    protected int ByteOrBitLength { get; }

    public BigInteger? Max => BigInteger.Pow(2, ByteOrBitLength * (BitType ? 1 : 8)) - 1;
    public BigInteger? Min => 0;

    protected bool IsWithinBounds(BigInteger val)
        => (!Max.HasValue || val <= Max) && (!Min.HasValue || val >= Min);

    private BAMField(ByteArrayManipulator bam, bool bitType, int startByte, int byteOrBitLength)
    {
        BAM = bam;
        BitType = bitType;
        StartByte = startByte;
        ByteOrBitLength = byteOrBitLength;
    }

    public BAMField(ByteArrayManipulator bam, int startByte, int byteLength)
        : this(bam, false, startByte, byteLength) { }

    public BAMField(ByteArrayManipulator bam, int startByte, int startBit, int bitLength)
        : this(bam, true, startByte, bitLength) => StartBit = startBit;

    public abstract ByteOverrideCMD GetOverride();
}

