using pkuManager.Utilities;
using System;
using System.Numerics;

namespace pkuManager.Formats.Fields.BAMFields
{
    public class BAMIntegralField : IntegralField, IByteOverridable
    {
        private readonly BAMFieldInfo bfi;

        public BAMIntegralField(ByteArrayManipulator bam, int startByte, int byteLength,
            Func<BigInteger, BigInteger> getter = null, Func<BigInteger, BigInteger> setter = null) : base(getter, setter)
        {
            bfi = new BAMFieldInfo(bam, startByte, byteLength);
        }

        public BAMIntegralField(ByteArrayManipulator bam, int startByte, int startBit, int bitLength,
            Func<BigInteger, BigInteger> getter = null, Func<BigInteger, BigInteger> setter = null) : base(getter, setter)
        {
            bfi = new BAMFieldInfo(bam, startByte, startBit, bitLength);
        }

        protected override BigInteger GetRaw() => bfi.BitType ? bfi.BAM.Get(bfi.StartByte, bfi.StartBit, bfi.BitLength)
                                                              : bfi.BAM.Get(bfi.StartByte, bfi.ByteLength);

        protected override void SetRaw(BigInteger val)
        {
            if (bfi.BitType)
                bfi.BAM.Set(val, bfi.StartByte, bfi.StartBit, bfi.BitLength);
            else
                bfi.BAM.Set(val, bfi.StartByte, bfi.ByteLength);
        }

        public string GetOverride() => bfi.BitType ? $"Set {bfi.StartByte}:{bfi.StartBit}:{bfi.BitLength}"
                                                   : $"Set {bfi.StartByte}:{bfi.ByteLength}";

        public override BigInteger Max => bfi.Max;
        public override BigInteger Min => bfi.Min;
    }
}
