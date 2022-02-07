namespace pkuManager.Formats.Fields.BackedFields;

public class BackedArrayField<T> : BackedField<T[]>, IArrayField<T>
{
    public BackedArrayField() : base() { }

    public BackedArrayField(T[] vals) : base(vals) { }

    public BackedArrayField(int length) : base(new T[length]) { }
}