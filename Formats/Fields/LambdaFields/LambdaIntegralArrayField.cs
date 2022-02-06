using System;
using System.Numerics;

namespace pkuManager.Formats.Fields.LambdaFields;

public class LambdaIntegralArrayField : IntegralArrayField
{
    // Field
    protected override BigInteger[] Value { get => LambdaGet(); set => LambdaSet(value); }

    // ArrayField
    public override int Length => Value.Length;

    // IntegralArrayField
    public override BigInteger Max { get; }
    public override BigInteger Min { get; }

    //Lambda
    public Func<BigInteger[]> LambdaGet { get; }
    public Action<BigInteger[]> LambdaSet { get; }

    public LambdaIntegralArrayField(Func<BigInteger[]> get, Action<BigInteger[]> set, BigInteger max, BigInteger min, Func<BigInteger[], BigInteger[]> getter = null,
        Func<BigInteger[], BigInteger[]> setter = null) : base(getter, setter)
    {
        LambdaGet = get;
        LambdaSet = set;
        Max = max;
        Min = min;
    }
}