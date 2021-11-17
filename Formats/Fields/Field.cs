using System;

namespace pkuManager.Formats.Fields;

public abstract class Field<T>
{
    protected abstract T Value { get; set; }

    protected Func<T, T> CustomGetter;

    protected Func<T, T> CustomSetter;

    protected Field(Func<T, T> getter, Func<T, T> setter)
    {
        CustomGetter = getter ?? (x => x);
        CustomSetter = setter ?? (x => x);
    }

    public static implicit operator T(Field<T> f) => f.Get();

    public T Get() => CustomGetter(Value);

    public void Set(T val) => Value = CustomSetter(val);
}