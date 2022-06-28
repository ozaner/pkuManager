using System.Numerics;

namespace pkuManager.WinForms.Formats.Fields.BackedFields;

public class BackedIntField : BackedField<BigInteger>, IIntField
{
    public BigInteger? Max { get; }
    public BigInteger? Min { get; }

    public BackedIntField(BigInteger? max = null, BigInteger? min = null) : base(0)
    {
        Max = max;
        Min = min;
    }
}

public class BackedIntArrayField : BackedArrayField<BigInteger>, IIntArrayField
{
    public BigInteger? Max { get; }
    public BigInteger? Min { get; }

    public BackedIntArrayField(int? length = null, BigInteger? max = null, BigInteger? min = null) : base(length)
    {
        Max = max;
        Min = min;
    }
}