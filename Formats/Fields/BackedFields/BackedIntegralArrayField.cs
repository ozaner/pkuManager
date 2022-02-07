using System.Numerics;

namespace pkuManager.Formats.Fields.BackedFields;

public class BackedIntegralArrayField : BackedArrayField<BigInteger>, IIntegralArrayField
{
    public BigInteger? Max { get; }
    public BigInteger? Min { get; }

    public BackedIntegralArrayField(BigInteger? max = null, BigInteger? min = null) : base()
    {
        Max = max;
        Min = min;
    }

    public BackedIntegralArrayField(BigInteger[] vals, BigInteger? max = null, BigInteger? min = null) : base(vals)
    {
        Max = max;
        Min = min;
    }

    public BackedIntegralArrayField(int length, BigInteger? max = null, BigInteger? min = null) : base(new BigInteger[length])
    {
        Max = max;
        Min = min;
    }
}