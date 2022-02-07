namespace pkuManager.Formats.Fields;

public interface IArrayField<T> : IField<T[]>
{
    public virtual int Length => Value.Length;
}