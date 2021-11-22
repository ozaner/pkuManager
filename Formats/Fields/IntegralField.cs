using pkuManager.Utilities;
using System;
using System.Numerics;

namespace pkuManager.Formats.Fields;

public abstract class IntegralField : Field<BigInteger>
{
    public abstract BigInteger Max { get; }
    public abstract BigInteger Min { get; }

    protected IntegralField(Func<BigInteger, BigInteger> getter, Func<BigInteger, BigInteger> setter)
        : base(getter, setter) => CustomSetter = CustomSetter.Compose(SetterBound);

    protected BigInteger SetterBound(BigInteger val)
    {
        if (val > Max)
            throw new ArgumentOutOfRangeException(nameof(val), "Passed value is greater than maximum.");
        else if (val < Min)
            throw new ArgumentOutOfRangeException(nameof(val), "Passed value is less than minimum.");
        else
            return val;
    }


    // Cast Accessors
    public T GetAs<T>() where T : struct
        => Get().BigIntegerTo<T>();

    public void SetAs<T>(T val) where T : struct
        => Set(val.ToBigInteger());
}