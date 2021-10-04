namespace pkuManager.Formats.Fields
{
    public class BasicField<T> : IFormatField<T>
    {
        protected T backingVal;
        public void Set(T val) => backingVal = val;
        public T Get() => backingVal;

        public static implicit operator T(BasicField<T> value) => value.Get();
    }

    public class BasicArray<T> : IFormatArray<T>
    {
        protected T[] backingVal;
        public void Set(T[] val) => backingVal = val;
        public T[] Get() => backingVal;

        public static implicit operator T[](BasicArray<T> value) => value.Get();

        public T this[int index]
        {
            get => Get(index);
            set => Set(value, index);
        }

        public T Get(int index) => backingVal[index];
        public void Set(T val, int index) => backingVal[index] = val;
        public int Length { get => backingVal.Length; }
    }

    public class NumericField<T> : BasicField<T>, INumericField<T>
    {
        public T MaxValue { get; }
        public T MinValue { get; }

        public NumericField()
            => (MinValue, MaxValue) = INumeric<T>.GetDefaultExtrema();
        public NumericField(T max) : base()
            => MaxValue = max;
        public NumericField(T max, T min)
            => (MinValue, MaxValue) = (min, max);
    }

    public class NumericArray<T> : BasicArray<T>, INumericArray<T>
    {
        public T MaxValue { get; }
        public T MinValue { get; }

        public NumericArray()
            => (MinValue, MaxValue) = INumeric<T>.GetDefaultExtrema();
        public NumericArray(T max) : base()
            => MaxValue = max;
        public NumericArray(T max, T min)
            => (MinValue, MaxValue) = (min, max);
    }
}
