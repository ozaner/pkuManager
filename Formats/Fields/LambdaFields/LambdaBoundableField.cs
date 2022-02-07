using System;

namespace pkuManager.Formats.Fields.LambdaFields;

public abstract class LambdaBoundableField<T, TElement> : LambdaField<T>, IBoundable<TElement>
    where TElement : struct, IComparable
{
    public TElement? Max { get; }
    public TElement? Min { get; }

    public LambdaBoundableField(Func<T> get, Action<T> set, TElement? max = null, TElement? min = null) : base(get, set)
    {
        Max = max;
        Min = min;
    }

    public LambdaBoundableField(IField<T> wrappedField, Func<T, T> getModifier, Func<T, T> setModifier)
        : base(wrappedField, getModifier, setModifier)
    {
        if (wrappedField is IBoundable<TElement> boundedField)
        {
            Max = boundedField.Max;
            Min = boundedField.Min;
        }
    }
}

public class LambdaBoundableField<T> : LambdaBoundableField<T, T> where T : struct, IComparable
{
    public LambdaBoundableField(Func<T> get, Action<T> set, T? max = null, T? min = null)
        : base(get, set, max, min) { }

    public LambdaBoundableField(IField<T> wrappedField, Func<T, T> getModifier, Func<T, T> setModifier)
        : base(wrappedField, getModifier, setModifier) { }
}

public class LambdaBoundableArrayField<T> : LambdaBoundableField<T[], T> where T : struct, IComparable
{
    public LambdaBoundableArrayField(Func<T[]> get, Action<T[]> set, T? max = null, T? min = null)
        : base(get, set, max, min) { }

    public LambdaBoundableArrayField(IField<T[]> wrappedField, Func<T[], T[]> getModifier, Func<T[], T[]> setModifier)
        : base(wrappedField, getModifier, setModifier) { }
}