using pkuManager.Utilities;

namespace pkuManager.Formats.Fields
{
    public abstract class BAMField<T> : INumeric<T>
    {
        protected ByteArrayManipulator BAM;
        protected int ByteIndex { get; }
        protected int TypeSize { get; }

        public T MaxValue { get; }
        public T MinValue { get; }

        //common constructor
        protected BAMField(ByteArrayManipulator bam, int byteIndex)
        {
            TypeSize = ByteArrayManipulator.GetByteSize<T>(); //also checks if T is valid
            BAM = bam;
            ByteIndex = byteIndex;
            (MinValue, MaxValue) = (0.CastTo<T>(), ((1L << GetBitCount()) - 1).CastTo<T>()); //doesn't work for ulong...
        }

        protected abstract int GetBitCount();
        public abstract string GetOverride();
    }

    public abstract class BAMValue<T> : BAMField<T>, INumericField<T>
    {
        protected BAMValue(ByteArrayManipulator bam, int byteIndex) : base(bam, byteIndex) { }

        public static implicit operator T(BAMValue<T> value) => value.Get();

        public abstract T Get();
        public abstract void Set(T val);
    }

    public class BAMByteValue<T> : BAMValue<T>
    {
        public BAMByteValue(ByteArrayManipulator bam, int byteIndex) : base(bam, byteIndex) { }

        public override void Set(T val) => BAM.Set(val, ByteIndex);
        public override T Get() => BAM.Get<T>(ByteIndex);

        protected override int GetBitCount() => TypeSize * 8;
        public override string GetOverride() => $"{typeof(T).Name} {ByteIndex}";
    }

    public class BAMBitValue<T> : BAMValue<T>
    {
        protected int BitIndex { get; }
        protected int BitLength { get; }

        public BAMBitValue(ByteArrayManipulator bam, int byteIndex, int bitIndex, int bitLength = 1) : base(bam, byteIndex)
        {
            BitIndex = bitIndex;
            BitLength = bitLength;
        }
        
        public override void Set(T val) => BAM.Set(val, ByteIndex, BitIndex, BitLength);
        public override T Get() => BAM.Get<T>(ByteIndex, BitIndex, BitLength);

        protected override int GetBitCount() => BitLength;
        public override string GetOverride() => $"{typeof(T).Name} {ByteIndex}:{BitIndex}{(BitLength is 1 ? 1 : "")}";
    }

    public class BAMArray<T> : BAMField<T>, INumericArray<T>
    {
        public int Length { get; }

        public BAMArray(ByteArrayManipulator bam, int byteIndex, int length) : base(bam, byteIndex)
            => Length = length;

        public static implicit operator T[](BAMArray<T> value) => value.Get();

        public T this[int index]
        {
            get => Get(index);
            set => Set(value, index);
        }

        public void Set(T[] vals) => BAM.SetArray(vals, ByteIndex, Length);
        public T[] Get() => BAM.GetArray<T>(ByteIndex, Length);

        public T Get(int index) => BAM.Get<T>(ByteIndex + TypeSize * index);
        public void Set(T val, int index) => BAM.Set(val, ByteIndex + TypeSize * index);
        
        protected override int GetBitCount() => TypeSize * Length;
        public override string GetOverride() => $"{typeof(T).Name}[] {ByteIndex}:{Length}";
    }
}
