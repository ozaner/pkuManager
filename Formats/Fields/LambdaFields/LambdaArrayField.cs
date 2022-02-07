using System;

namespace pkuManager.Formats.Fields.LambdaFields;

public class LambdaArrayField<T> : LambdaField<T[]>, IArrayField<T>
{
    public LambdaArrayField(Func<T[]> get, Action<T[]> set) : base(get, set) { }

    public LambdaArrayField(IArrayField<T> wrappedField, Func<T[], T[]> getModifier, Func<T[], T[]> setModifier)
        : base(wrappedField, getModifier, setModifier) { }
}