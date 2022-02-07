using System;
using System.Numerics;

namespace pkuManager.Formats.Fields.LambdaFields;

public class LambdaIntegralField : LambdaField<BigInteger>, IIntegralField
{
    public BigInteger? Max { get; }
    public BigInteger? Min { get; }

    public LambdaIntegralField(Func<BigInteger> get, Action<BigInteger> set,
        BigInteger? max = null, BigInteger? min = null) : base(get, set)
    {
        Max = max;
        Min = min;
    }

    public LambdaIntegralField(IIntegralField wrappedField, Func<BigInteger, BigInteger> getModifier,
        Func<BigInteger, BigInteger> setModifier) : base(wrappedField, getModifier, setModifier)
    {
        Max = wrappedField.Max;
        Min = wrappedField.Min;
    }
}