namespace pkuManager.Utilities
{
    public abstract class BAMProperty<T>
    {
        protected ByteArrayManipulator BAM;
        protected int ByteIndex { get; }
        protected int TypeSize { get; }

        //common constructor
        protected BAMProperty(ByteArrayManipulator bam, int byteIndex)
        {
            TypeSize = ByteArrayManipulator.GetByteSize<T>(); //also checks if T is valid
            BAM = bam;
            ByteIndex = byteIndex;
        }

        protected abstract int GetBitCount();

        public uint GetMaxValue() => (uint)((1L << GetBitCount()) - 1);
    }

    public abstract class BAMValue<T> : BAMProperty<T>
    {
        protected BAMValue(ByteArrayManipulator bam, int byteIndex) : base(bam, byteIndex) { }
        public static implicit operator T(BAMValue<T> value) => value.Get();
        public abstract T Get();
        public abstract void Set(T val);
        public abstract (string, T) GetOverride();
    }

    public class BAMByteValue<T> : BAMValue<T>
    {
        public BAMByteValue(ByteArrayManipulator bam, int byteIndex) : base(bam, byteIndex) { }

        public override void Set(T val) => BAM.Set(val, ByteIndex);
        public override T Get() => BAM.Get<T>(ByteIndex);
        public override (string, T) GetOverride()
            => ($"{typeof(T).Name} {ByteIndex}", Get());
        protected override int GetBitCount() => TypeSize * 8;
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
        public override (string, T) GetOverride()
            => ($"{typeof(T).Name} {ByteIndex}:{BitIndex}{(BitLength is 1 ? 1 : "")}", Get());
        protected override int GetBitCount() => BitLength;
    }

    public class BAMArray<T> : BAMProperty<T>
    {
        protected int Length { get; }

        public BAMArray(ByteArrayManipulator bam, int byteIndex, int length) : base(bam, byteIndex)
        {
            Length = length;
        }
        public static implicit operator T[](BAMArray<T> value) => value.Get();

        public void Set(T[] vals) => BAM.SetArray(vals, ByteIndex, Length);
        public T[] Get() => BAM.GetArray<T>(ByteIndex, Length);
        public (string, T[]) GetOverride()
            => ($"{typeof(T).Name}[] {ByteIndex}:{Length}", Get());
        protected override int GetBitCount() => TypeSize * Length;
    }
}
