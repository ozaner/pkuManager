using pkuManager.Utilities;
using System;
using System.Numerics;

namespace pkuManager.Formats.Fields;

public abstract class IntegralArrayField : ArrayField<BigInteger>
{
    public abstract BigInteger Max { get; }
    public abstract BigInteger Min { get; }

    protected IntegralArrayField(Func<BigInteger[], BigInteger[]> getter, Func<BigInteger[], BigInteger[]> setter)
        : base(getter, setter) => CustomSetter = CustomSetter.Compose(SetterArrayBound);

    protected BigInteger[] SetterArrayBound(BigInteger[] vals)
    {
        foreach (var val in vals)
        {
            if (val > Max)
                throw new ArgumentOutOfRangeException(nameof(val), "Passed value is greater than maximum.");
            else if (val < Min)
                throw new ArgumentOutOfRangeException(nameof(val), "Passed value is less than minimum.");
        }
        return vals;
    }


    // Cast Accessors
    public T[] GetAs<T>() where T : struct
        => Array.ConvertAll(Get(), x => x.BigIntegerTo<T>());

    public void SetAs<T>(T[] vals) where T : struct
        => Set(Array.ConvertAll(vals, x => x.ToBigInteger()));

    public T GetAs<T>(int i) where T : struct
        => Get(i).BigIntegerTo<T>();

    public void SetAs<T>(T val, int i) where T : struct
        => Set(val.ToBigInteger(), i);
}