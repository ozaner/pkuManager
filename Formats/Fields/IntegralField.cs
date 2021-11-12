using pkuManager.Utilities;
using System;
using System.Numerics;

namespace pkuManager.Formats.Fields;

public abstract class IntegralField : Field<BigInteger>
{
    public abstract BigInteger Max { get; }

    public abstract BigInteger Min { get; }

    protected IntegralField(Func<BigInteger, BigInteger> getter = null,
        Func<BigInteger, BigInteger> setter = null) : base(getter, setter)
    {
        BigInteger setterBound(BigInteger val)
        {
            if (val > Max)
                throw new ArgumentOutOfRangeException(nameof(val), "Passed value is greater than maximum.");
            else if (val < Min)
                throw new ArgumentOutOfRangeException(nameof(val), "Passed value is less than minimum.");
            else
                return val;
        }
        CustomSetter = CustomSetter is null ? setterBound : CustomSetter.Compose(setterBound);
    }

    public T Get<T>() where T : struct
        => Get().BigIntegerTo<T>();

    public void Set<T>(T val) where T : struct
        => base.Set(val.ToBigInteger());
}