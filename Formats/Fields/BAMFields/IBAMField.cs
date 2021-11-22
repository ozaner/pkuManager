using pkuManager.Utilities;
using System.Numerics;

namespace pkuManager.Formats.Fields.BAMFields;

public interface IBAMField
{
    public ByteArrayManipulator BAM { get; }
    public bool BitType { get; }

    public int StartByte { get; }
    public int ByteLength { get; }
    public int StartBit { get; }
    public int BitLength { get; }

    public BigInteger GetMax() => BigInteger.Pow(2, BitType ? BitLength : 8 * ByteLength) - 1;
    public BigInteger GetMin() => 0;
}