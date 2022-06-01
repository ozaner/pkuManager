using pkuManager.Formats.Modules.MetaTags;
using pkuManager.Utilities;
using System.Numerics;

namespace pkuManager.Formats.Fields.BAMFields;

public class BAMIntegralField : BAMField, IIntField
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

    public override ByteOverrideCMD GetOverride()
        => BitType ? new ByteOverrideCMD(Value, StartByte, StartBit, ByteOrBitLength, BAM.VirtualIndices)
                   : new ByteOverrideCMD(Value, StartByte, ByteOrBitLength, BAM.VirtualIndices);
}