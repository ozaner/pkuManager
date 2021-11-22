using System;

namespace pkuManager.Formats.Fields.BackedFields;

public class BackedArrayField<T> : ArrayField<T>
{
    protected override T[] Value { get; set; }

    public override int Length => Value.Length;

    public BackedArrayField(Func<T[], T[]> getter = null, Func<T[], T[]> setter = null)
        : base(getter, setter) { }

    public BackedArrayField(int length, Func<T[], T[]> getter = null, Func<T[], T[]> setter = null)
        : this(getter, setter) => Set(new T[length]);
}