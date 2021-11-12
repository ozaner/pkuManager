using pkuManager.Utilities;
using System.Numerics;

namespace pkuManager.Formats.Fields.BAMFields
{
    public class BAMFieldInfo
    {
        public ByteArrayManipulator BAM { get; }
        public bool BitType { get; }

        public int StartByte { get; }
        public int ByteLength { get; }
        public int StartBit { get; }
        public int BitLength { get; }


        private BAMFieldInfo(ByteArrayManipulator bam, bool bitType, int startByte)
        {
            BAM = bam;
            BitType = bitType;
            StartByte = startByte;
        }

        public BAMFieldInfo(ByteArrayManipulator bam, int startByte, int byteLength) : this(bam, false, startByte)
        {
            ByteLength = byteLength;
        }

        public BAMFieldInfo(ByteArrayManipulator bam, int startByte, int startBit, int bitLength) : this(bam, true, startByte)
        {
            StartBit = startBit;
            BitLength = bitLength;
        }


        public BigInteger Max => BigInteger.Pow(2, BitType ? BitLength : 8 * ByteLength) - 1;

        public BigInteger Min => 0;
    }
}
