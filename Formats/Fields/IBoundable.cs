using System;

namespace pkuManager.Formats.Fields;

public interface IBoundable<T> where T: struct, IComparable
{
    public T? Max { get; }
    public T? Min { get; }
}
