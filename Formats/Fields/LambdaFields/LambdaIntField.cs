using System;
using System.Numerics;

namespace pkuManager.Formats.Fields.LambdaFields;

public class LambdaIntField : LambdaField<BigInteger>, IBoundable
{
    public BigInteger? Max { get; }
    public BigInteger? Min { get; }

    public LambdaIntField(Func<BigInteger> get, Action<BigInteger> set,
        BigInteger? max = null, BigInteger? min = null) : base(get, set)
    {
        Max = max;
        Min = min;
    }

    public LambdaIntField(IField<BigInteger> wrappedField, Func<BigInteger, BigInteger> getModifier,
        Func<BigInteger, BigInteger> setModifier) : base(wrappedField, getModifier, setModifier)
    {
        if (wrappedField is IBoundable boundedField)
        {
            Max = boundedField.Max;
            Min = boundedField.Min;
        }
    }
}

public class LambdaIntArrayField : LambdaField<BigInteger[]>, IBoundable
{
    public BigInteger? Max { get; }
    public BigInteger? Min { get; }

    public LambdaIntArrayField(Func<BigInteger[]> get, Action<BigInteger[]> set,
        BigInteger? max = null, BigInteger? min = null) : base(get, set)
    {
        Max = max;
        Min = min;
    }

    public LambdaIntArrayField(IField<BigInteger[]> wrappedField, Func<BigInteger[], BigInteger[]> getModifier,
        Func<BigInteger[], BigInteger[]> setModifier) : base(wrappedField, getModifier, setModifier)
    {
        if (wrappedField is IBoundable boundedField)
        {
            Max = boundedField.Max;
            Min = boundedField.Min;
        }
    }
}