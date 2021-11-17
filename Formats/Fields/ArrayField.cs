using System;

namespace pkuManager.Formats.Fields;

public abstract class ArrayField<T> : Field<T[]>
{
    public abstract int Length { get; }

    protected ArrayField(Func<T[], T[]> getter, Func<T[], T[]> setter) : base(getter, setter) { }

    public T this[int i]
    {
        get => Get(i);
        set => Set(value, i);
    }

    public T Get(int index) => Get()[index];

    public void Set(T val, int index)
    {
        T[] vals = Get();
        vals[index] = val;
        Set(vals);
    }
}