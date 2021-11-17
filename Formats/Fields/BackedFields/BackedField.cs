using System;

namespace pkuManager.Formats.Fields.BackedFields;

public class BackedField<T> : Field<T>
{
    protected override T Value { get; set; }

    public BackedField(Func<T, T> getter = null, Func<T, T> setter = null)
        : base(getter, setter) { }
}