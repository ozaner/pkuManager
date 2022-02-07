namespace pkuManager.Formats.Fields.BackedFields;

public class BackedField<T> : IField<T>
{
    public T Value { get; set; }

    public BackedField() { }

    public BackedField(T val) => Value = val;
}