using System;
using System.Numerics;

namespace pkuManager.Utilities
{
    /// <summary>
    /// BAM for short, this is a data structure that supports a variety of read and write operations on byte arrays.<br/>
    /// The effect on the underlying array is dependent on it's specified endianness.
    /// </summary>
    public class ByteArrayManipulator
    {
        /// <summary>
        /// Whether this BAM is big-endian. It is little-endian otherwise.
        /// </summary>
        public bool BigEndian { get; }

        /// <summary>
        /// The underlying byte array this BAM is wrapping.
        /// </summary>
        public byte[] ByteArray;

        /// <summary>
        /// The total number of bytes in the byte array.
        /// </summary>
        public int Length { get => ByteArray.Length; }

        /// <summary>
        /// Constructs a new BAM with an empty underlying byte array.
        /// </summary>
        /// <param name="size">The size of the empty underlying array.</param>
        /// <param name="bigEndian">The endianness of the new BAM.</param>
        public ByteArrayManipulator(int size, bool bigEndian) : this(new byte[size], bigEndian) { }

        /// <summary>
        /// Constructs a BAM with a pre-existing byte array. The same object is used, a copy is not created.
        /// </summary>
        /// <param name="byteArray">The byte array to underlie the BAM.</param>
        /// <param name="bigEndian">The endianess of the new BAM.</param>
        public ByteArrayManipulator(byte[] byteArray, bool bigEndian)
        {
            BigEndian = bigEndian;
            ByteArray = byteArray;
        }

        /// <summary>
        /// Implicitly casts a BAM to its underlying byte array.
        /// </summary>
        /// <param name="BAM">The BAM to be casted to a byte array.</param>
        public static implicit operator byte[](ByteArrayManipulator BAM)
            => BAM.ByteArray;

        /// <summary>
        /// Returns the size, in bytes, of a supported value type.
        /// </summary>
        /// <typeparam name="T">A supported BAM value type.</typeparam>
        /// <returns>The size, in bytes, of the <typeparamref name="T"/> type.</returns>
        public static int GetByteSize<T>() => Type.GetTypeCode(typeof(T)) switch
        {
            TypeCode.UInt32 => sizeof(uint),
            TypeCode.UInt16 => sizeof(ushort),
            TypeCode.Char => sizeof(char),
            TypeCode.Byte => sizeof(byte),
            TypeCode.Boolean => sizeof(bool),
            _ => throw new ArgumentException("Invalid BAM data type.", nameof(T))
        };


        /* ------------------------------------
         * Reading Methods - Single Value
         * ------------------------------------
        */
        /// <summary>
        /// Reads an unsigned value from the byte array.
        /// </summary>
        /// <param name="byteIndex">The index of the value.</param>
        /// <param name="byteLength">The length of the value in bytes.</param>
        /// <returns>The <paramref name="byteLength"/> unsigned value at <paramref name="byteIndex"/>.</returns>
        public BigInteger Get(int byteIndex, int byteLength)
        {
            BigInteger sum = 0;
            for (int i = 0; i < byteLength; i++)
                sum += ByteArray[byteIndex + i] * BigInteger.Pow(2, 8 * (BigEndian ? (byteLength - 1 - i) : i));

            return sum;
        }

        /// <summary>
        /// Reads a bit level-value stored from the byte array.
        /// </summary>
        /// <param name="byteIndex">The index of the first byte the value is stored in.</param>
        /// <param name="bitIndex">The index of the value's first bit relative
        ///                        to <paramref name="byteIndex"/>.</param>
        /// <param name="bitLength">The length of the value in bits.</param>
        /// <returns>A <paramref name="bitLength"/> unsigned value starting
        ///          <paramref name="bitIndex"/> bits after <paramref name="byteIndex"/>.</returns>
        public BigInteger Get(int byteIndex, int bitIndex, int bitLength)
        {
            BigInteger sum = 0;
            for (int i = 0; i < bitLength; i++)
            {
                int by = byteIndex + (bitIndex + i) / 8;
                int bi = (bitIndex + i) % 8;
                sum += (BigInteger)((ByteArray[by] >> bi) & 1) << i;
            }
            return sum;
        }

        /// <typeparam name="T">The type of the value to read/be returned.</typeparam>
        /// <inheritdoc cref="Get(int, int)"/>
        public T Get<T>(int byteIndex) where T : struct
            => Get(byteIndex, GetByteSize<T>()).BigIntegerTo<T>();

        /// <typeparam name="T">The type of the value to be returned.</typeparam>
        /// <inheritdoc cref="Get(int, int, int)"/>
        public T Get<T>(int byteIndex, int bitIndex, int bitLength) where T : struct
            => Get(byteIndex, bitIndex, bitLength).BigIntegerTo<T>();


        /* ------------------------------------
         * Reading Methods - Arrays
         * ------------------------------------
        */
        /// <summary>
        /// Reads an array of unsigned values from the byte array.
        /// </summary>
        /// <param name="byteIndex">The index of the first value.</param>
        /// <param name="byteLength">The length of each value in bytes.</param>
        /// <param name="length">The number of elements in the array.</param>
        /// <returns>An array of values read from the byte array, specified by the parameters.</returns>
        public BigInteger[] GetArray(int byteIndex, int byteLength, int length)
        {
            BigInteger[] arr = new BigInteger[length];
            for (int i = 0; i < length; i++)
                arr[i] = Get(byteIndex + i * byteLength, byteLength);
            return arr;
        }

        /// <summary>
        /// Reads an array of bit level-values stored in the byte array.
        /// </summary>
        /// <param name="byteIndex">The index of the first byte the first value is stored in.</param>
        /// <param name="bitIndex">The index of the first value's first bit relative
        ///                        to <paramref name="byteIndex"/>.</param>
        /// <param name="bitLength">The length of each value in bits.</param>
        /// <inheritdoc cref="GetArray(int, int, int)"/>
        public BigInteger[] GetArray(int byteIndex, int bitIndex, int bitLength, int length)
        {
            BigInteger[] arr = new BigInteger[length];
            for (int i = 0; i < length; i++)
                arr[i] = Get(byteIndex, bitIndex + i * bitLength, bitLength);
            return arr;
        }

        /// <typeparam name="T">The type of the values to read/be returned.</typeparam>
        /// <inheritdoc cref="GetArray(int, int, int)"/>
        public T[] GetArray<T>(int byteIndex, int length) where T : struct
            => Array.ConvertAll(GetArray(byteIndex, GetByteSize<T>(), length), x => x.BigIntegerTo<T>());

        /// <typeparam name="T">The type of the values to be returned.</typeparam>
        /// <inheritdoc cref="GetArray(int, int, int, int)"/>
        public T[] GetArray<T>(int byteIndex, int bitIndex, int bitLength, int length) where T : struct
            => Array.ConvertAll(GetArray(byteIndex, bitIndex, bitLength, length), x => x.BigIntegerTo<T>());


        /* ------------------------------------
         * Writing Methods - Single Value
         * ------------------------------------
        */
        /// <summary>
        /// Writes an unsigned value to the byte array.<br/>
        /// Note that overflow will be truncated.
        /// </summary>
        /// <param name="value">The value to be set, must be positive.</param>
        /// <param name="byteIndex">The index of the value.</param>
        /// <param name="byteLength">The length of the value in bytes.</param>
        public void Set(BigInteger value, int byteIndex, int byteLength)
        {
            for (int i = 0; i < byteLength; i++)
                ByteArray[byteIndex + i] = (byte)((value >> (8 * (BigEndian ? (byteLength - 1 - i) : i))) & 0xFF);
        }

        /// <summary>
        /// Writes an array of bit level-values to the byte array.<br/>
        /// Note that overflow will be truncated.
        /// </summary>
        /// <param name="value">The value to be set, must be positive.</param>
        /// <param name="byteIndex">The index of the first byte the value is stored in.</param>
        /// <param name="bitIndex">The index of the first value's first bit relative
        ///                        to <paramref name="byteIndex"/>.</param>
        /// <param name="bitLength">The length of each value in bits.</param>
        public void Set(BigInteger value, int byteIndex, int bitIndex, int bitLength)
        {
            for (int i = 0; i < bitLength; i++)
            {
                int by = byteIndex + (bitIndex + i) / 8;
                int bi = (bitIndex + i) % 8;
                BigInteger temp = ByteArray[by];
                temp.SetBits(value.GetBits(i, 1), bi, 1);
                ByteArray[by] = (byte)temp;
            }
        }

        /// <typeparam name="T">A supported BAM value type.</typeparam>
        /// <inheritdoc cref="Set(BigInteger, int, int)"/>
        public void Set<T>(T value, int byteIndex) where T : struct
            => Set(value.ToBigInteger(), byteIndex, GetByteSize<T>());

        /// <typeparam name="T">A supported BAM value type.</typeparam>
        /// <inheritdoc cref="Set(BigInteger, int, int, int)"/>
        public void Set<T>(T value, int byteIndex, int bitIndex, int bitLength) where T : struct
            => Set(value.ToBigInteger(), byteIndex, bitIndex, bitLength);


        /* ------------------------------------
         * Writing Methods - Arrays
         * ------------------------------------
        */
        /// <summary>
        /// Writes an array of unsigned values to the byte array.<br/>
        /// Note that overflow will be truncated.
        /// </summary>
        /// <param name="values">The array of values to be set, must all be positive.</param>
        /// <param name="byteIndex">The index of the value.</param>
        /// <param name="byteLength">The length of the value in bytes.</param>
        /// <param name="length">The number of elements in the array.
        ///                      Uses the length of <paramref name="values"/> by default.</param>
        public void SetArray(int byteIndex, int byteLength, BigInteger[] values, int? length = null)
        {
            if (length is null)
                length = values.Length;
            for (int i = 0; i < length; i++)
                Set(values[i], byteIndex + i * byteLength, byteLength);
        }

        /// <summary>
        /// Writes an array of bit level-values to the byte array.<br/>
        /// Note that overflow will be truncated.
        /// </summary>
        /// <param name="byteIndex">The index of the first byte the first value is stored in.</param>
        /// <param name="bitIndex">The index of the first value's first bit relative
        ///                        to <paramref name="byteIndex"/>.</param>
        /// <param name="bitLength">The length of each value in bits.</param>
        /// <inheritdoc cref="SetArray(int, int, BigInteger[], int?)"/>
        public void SetArray(int byteIndex, int bitIndex, int bitLength, BigInteger[] values, int? length = null)
        {
            if (length is null)
                length = values.Length;
            for (int i = 0; i < length; i++)
                Set(values[i], byteIndex, bitIndex + i * bitLength, bitLength);
        }

        /// <typeparam name="T">A supported BAM value type.</typeparam>
        /// <inheritdoc cref="SetArray(int, int, BigInteger[], int?)"/>
        public void SetArray<T>(int byteIndex, T[] values, int? length = null) where T : struct
            => SetArray(byteIndex, GetByteSize<T>(), Array.ConvertAll(values, x => x.ToBigInteger()), length);

        /// <typeparam name="T">A supported BAM value type.</typeparam>
        /// <inheritdoc cref="SetArray(int, int, int, BigInteger[], int?)"/>
        public void SetArray<T>(int byteIndex, int bitIndex, int bitLength, T[] values, int? length = null) where T : struct
            => SetArray(byteIndex, bitIndex, bitLength, Array.ConvertAll(values, x => x.ToBigInteger()), length);
    }
}
