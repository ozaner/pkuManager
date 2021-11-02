namespace pkuManager.Formats.Fields
{
    public interface IFormatField<T>
    {
        public abstract T Get();
        public abstract void Set(T val);
    }

    public interface IFormatArray<T> : IFormatField<T[]>
    {
        public abstract T Get(int index);
        public abstract void Set(T val, int index);

        public abstract int Length { get; }
    }

    public interface INumeric<T>
    {
        public abstract T MaxValue { get; }
        public abstract T MinValue { get; }
    }

    public interface INumericField<T> : IFormatField<T>, INumeric<T> { }
    public interface INumericArray<T> : IFormatArray<T>, INumeric<T> { }
}
