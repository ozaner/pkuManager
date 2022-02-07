using System;
using System.Numerics;

namespace pkuManager.Formats.Fields.LambdaFields;

public class LambdaIntegralArrayField : LambdaField<BigInteger[]>, IIntegralArrayField
{
    public BigInteger? Max { get; }
    public BigInteger? Min { get; }

    public LambdaIntegralArrayField(Func<BigInteger[]> get, Action<BigInteger[]> set,
        BigInteger? max = null, BigInteger? min = null) : base(get, set)
    {
        Max = max;
        Min = min;
    }

    public LambdaIntegralArrayField(IIntegralArrayField wrappedField, Func<BigInteger[], BigInteger[]> getModifier,
        Func<BigInteger[], BigInteger[]> setModifier) : base(wrappedField, getModifier, setModifier)
    {
        Max = wrappedField.Max;
        Min = wrappedField.Min;
    }
}