using pkuManager.Utilities;
using System;

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

        protected static (T, T) Caster(object min, object max)
                => (min.CastTo<T>(), max.CastTo<T>());

        public static (T, T) GetDefaultExtrema()
        {
            return Type.GetTypeCode(typeof(T)) switch
            {
                TypeCode.UInt32 => Caster(uint.MinValue, uint.MaxValue), //uint
                TypeCode.UInt16 => Caster(ushort.MinValue, ushort.MaxValue), //ushort
                TypeCode.Byte => Caster(byte.MinValue, byte.MaxValue), //byte
                TypeCode.Int32 => Caster(int.MinValue, int.MaxValue), //int
                TypeCode.Boolean => Caster(false, true), //bool
                _ => throw new ArgumentException($"The given type {nameof(T)} is not a supported numeric type.", nameof(T))
            };
        }
    }

    public interface INumericField<T> : IFormatField<T>, INumeric<T> { }
    public interface INumericArray<T> : IFormatArray<T>, INumeric<T> { }
}
