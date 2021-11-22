using System;
using System.Numerics;

namespace pkuManager.Formats.Fields.BackedFields;

public class BackedIntegralArrayField : IntegralArrayField
{
    // Field
    protected override BigInteger[] Value { get; set; }

    // ArrayField
    public override int Length => Value.Length;

    // IntegralArrayField
    public override BigInteger Max { get; }
    public override BigInteger Min { get; }


    public BackedIntegralArrayField(BigInteger max, BigInteger min, Func<BigInteger[], BigInteger[]> getter = null,
        Func<BigInteger[], BigInteger[]> setter = null) : base(getter, setter)
    {
        Max = max;
        Min = min;
    }

    public BackedIntegralArrayField(BigInteger max, BigInteger min, int length, Func<BigInteger[], BigInteger[]> getter = null,
        Func<BigInteger[], BigInteger[]> setter = null) : base(getter, setter)
    {
        Max = max;
        Min = min;
        Set(new BigInteger[length]);
    }
}