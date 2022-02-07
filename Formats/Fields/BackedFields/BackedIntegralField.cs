using System.Numerics;

namespace pkuManager.Formats.Fields.BackedFields;

public class BackedIntegralField : BackedField<BigInteger>, IIntegralField
{
    public BigInteger? Max { get; }
    public BigInteger? Min { get; }

    public BackedIntegralField(BigInteger? max = null, BigInteger? min = null, BigInteger val = default) : base(val)
    {
        Max = max;
        Min = min;
    }
}