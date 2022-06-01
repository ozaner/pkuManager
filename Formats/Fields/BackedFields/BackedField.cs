namespace pkuManager.Formats.Fields.BackedFields;

public class BackedField<T> : IField<T>
{
    public T Value { get; set; }

    public BackedField() { }

    public BackedField(T val) => Value = val;
}

public class BackedArrayField<T> : IField<T[]>
{
    public T[] Value { get; set; }

    public BackedArrayField() { }

    public BackedArrayField(T[] val) => Value = val;

    public BackedArrayField(int? length) : this(length.HasValue ? new T[length.Value] : null) { }
}