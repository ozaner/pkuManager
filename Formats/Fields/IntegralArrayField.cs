using pkuManager.Utilities;
using System;
using System.Numerics;

namespace pkuManager.Formats.Fields;

public abstract class IntegralArrayField: ArrayField<BigInteger>
{
    public abstract BigInteger Max { get; }

    public abstract BigInteger Min { get; }

    protected IntegralArrayField(Func<BigInteger[], BigInteger[]> getter = null,
        Func<BigInteger[], BigInteger[]> setter = null) : base(getter, setter)
    {
        BigInteger[] setterBound(BigInteger[] vals)
        {
            foreach (BigInteger val in vals)
            {
                if (val > Max)
                    throw new ArgumentOutOfRangeException(nameof(val), "Passed value is greater than maximum.");
                else if (val < Min)
                    throw new ArgumentOutOfRangeException(nameof(val), "Passed value is less than minimum.");
            }
            return vals;
        }
        CustomSetter = CustomSetter is null ? setterBound : CustomSetter.Compose(setterBound);
    }

    public T[] Get<T>() where T : struct
        => Array.ConvertAll(Get(), x => x.BigIntegerTo<T>());

    public T Get<T>(int index) where T : struct
        => Get(index).BigIntegerTo<T>();

    public void Set<T>(T[] vals) where T : struct
        => base.Set(Array.ConvertAll(vals, x => x.ToBigInteger()));

    public void Set<T>(T val, int index) where T : struct
        => base.Set(val.ToBigInteger(), index);
}