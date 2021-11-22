using System;
using System.Numerics;

namespace pkuManager.Formats.Fields.BackedFields;

public class BackedIntegralField : IntegralField
{
    // Field
    protected override BigInteger Value { get; set; }

    // IntegralField
    public override BigInteger Max { get; }
    public override BigInteger Min { get; }


    public BackedIntegralField(BigInteger max, BigInteger min, Func<BigInteger, BigInteger> getter = null,
        Func<BigInteger, BigInteger> setter = null) : base(getter, setter)
    {
        Max = max;
        Min = min;
    }
}