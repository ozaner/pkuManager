using System;
using System.Numerics;

namespace pkuManager.Formats.Fields.LambdaFields;

public class LambdaIntegralField : IntegralField
{
    // Field
    protected override BigInteger Value { get => LambdaGet(); set => LambdaSet(value); }

    // IntegralField
    public override BigInteger? Max { get; }
    public override BigInteger? Min { get; }

    //Lambda
    public Func<BigInteger> LambdaGet { get; }
    public Action<BigInteger> LambdaSet { get; }

    public LambdaIntegralField(Func<BigInteger> get, Action<BigInteger> set, BigInteger? max = null, BigInteger? min = null, Func<BigInteger, BigInteger> getter = null,
        Func<BigInteger, BigInteger> setter = null) : base(getter, setter)
    {
        LambdaGet = get;
        LambdaSet = set;
        Max = max;
        Min = min;
    }
}