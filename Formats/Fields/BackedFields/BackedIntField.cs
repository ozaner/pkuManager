using System.Numerics;

namespace pkuManager.Formats.Fields.BackedFields;

public class BackedIntField : BackedField<BigInteger>, IBoundable
{
    public BigInteger? Max { get; }
    public BigInteger? Min { get; }

    public BackedIntField(BigInteger? max = null, BigInteger? min = null) : base(min ?? 0)
    {
        Max = max;
        Min = min;
    }
}

public class BackedIntArrayField : BackedField<BigInteger[]>, IBoundable
{
    public BigInteger? Max { get; }
    public BigInteger? Min { get; }

    public BackedIntArrayField(BigInteger[] val = default, BigInteger? max = null, BigInteger? min = null) : base(val)
    {
        Max = max;
        Min = min;
    }
}