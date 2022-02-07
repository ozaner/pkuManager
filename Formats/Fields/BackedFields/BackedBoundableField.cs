using System;

namespace pkuManager.Formats.Fields.BackedFields;

public abstract class BackedBoundableField<T, TElement> : BackedField<T>, IBoundable<TElement>
    where TElement : struct, IComparable
{
    public TElement? Max { get; }
    public TElement? Min { get; }

    public BackedBoundableField(T val = default, TElement? max = null, TElement? min = null) : base(val)
    {
        Max = max;
        Min = min;
    }
}

public class BackedBoundableField<T> : BackedBoundableField<T, T> where T : struct, IComparable
{
    public BackedBoundableField(T? max = null, T? min = null, T val = default)
        : base(val, max, min) { }
}

public class BackedBoundableArrayField<T> : BackedBoundableField<T[], T> where T : struct, IComparable
{
    public BackedBoundableArrayField(T[] val, T? max = null, T? min = null)
        : base(val, max, min) { }
}