using System;
using System.Numerics;

namespace pkuManager.Formats.Fields.LambdaFields;

public class LambdaIntField : LambdaField<BigInteger>, IIntField
{
    public BigInteger? Max { get; }
    public BigInteger? Min { get; }

    public LambdaIntField(Func<BigInteger> get, Action<BigInteger> set,
        BigInteger? max = null, BigInteger? min = null) : base(get, set)
    {
        Max = max;
        Min = min;
    }

    public LambdaIntField(IIntField wrappedField, Func<BigInteger, BigInteger> getModifier,
        Func<BigInteger, BigInteger> setModifier) : base(wrappedField, getModifier, setModifier)
    {
        Max = wrappedField.Max;
        Min = wrappedField.Min;
    }
}

public class LambdaIntArrayField : LambdaField<BigInteger[]>, IIntArrayField
{
    public BigInteger? Max { get; }
    public BigInteger? Min { get; }

    public LambdaIntArrayField(Func<BigInteger[]> get, Action<BigInteger[]> set,
        BigInteger? max = null, BigInteger? min = null) : base(get, set)
    {
        Max = max;
        Min = min;
    }

    public LambdaIntArrayField(IIntArrayField wrappedField, Func<BigInteger[], BigInteger[]> getModifier,
        Func<BigInteger[], BigInteger[]> setModifier) : base(wrappedField, getModifier, setModifier)
    {
        Max = wrappedField.Max;
        Min = wrappedField.Min;
    }
}