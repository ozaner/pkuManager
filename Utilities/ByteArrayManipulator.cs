using System;

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
         * Reading Methods
         * ------------------------------------
        */
        /// <summary>
        /// Reads an unsigned value from the byte array.
        /// </summary>
        /// <param name="byteIndex">The index of the value.</param>
        /// <param name="byteLength">The length of the value in bytes. Must be a value from 0-4.</param>
        /// <returns>The <paramref name="byteLength"/> unsigned value at <paramref name="byteIndex"/>.</returns>
        private uint GetInt(int byteIndex, int byteLength)
        {
            uint powerOfTwo(int n) => (uint)1 << n;
            uint sum = 0;
            for (int i = 0; i < byteLength; i++)
                sum += ByteArray[byteIndex + i] * powerOfTwo(8 * (BigEndian ? (byteLength - 1 - i) : i));

            return sum;
        }

        /// <summary>
        /// Reads a bit level-value stored within an unsigned integer from the byte array.
        /// </summary>
        /// <param name="byteIndex">The index of the unsigned integer the value is stored in.</param>
        /// <param name="bitIndex">The index of the value's first bit relative to <paramref name="byteIndex"/>.</param>
        /// <param name="bitLength">The length of the value in bits.</param>
        /// <returns>A <paramref name="bitLength"/> unsigned value starting
        ///          <paramref name="bitIndex"/> bits after <paramref name="byteIndex"/>.</returns>
        private uint GetInt(int byteIndex, int bitIndex, int bitLength)
        {
            if (bitIndex is < 0 or > 32)
                throw new ArgumentException($"{nameof(bitIndex)} must be a value from 0-32.", nameof(bitIndex));

            if (bitIndex + bitLength is < 0 or > 32)
                throw new ArgumentException($"{nameof(bitIndex)} + {nameof(bitLength)} must be anything from 0-32.", nameof(bitLength));

            uint sum = 0;
            for (int i = 0; i < bitLength; i++)
            {
                int by = byteIndex + (bitIndex + i) / 8;
                int bi = (bitIndex + i) % 8;
                sum += (uint)((ByteArray[by] >> bi) & 1) << i;
            }
            return sum;
        }

        /// <summary>
        /// Reads a value of type <typeparamref name="T"/> from
        /// <see cref="ByteArray"/> at the <paramref name="byteIndex"/>.
        /// </summary>
        /// <typeparam name="T">A supported BAM value type.</typeparam>
        /// <param name="byteIndex">The index of the value.</param>
        /// <returns>The value at the given location.</returns>
        public T Get<T>(int byteIndex)
            => (T)Convert.ChangeType(GetInt(byteIndex, GetByteSize<T>()), typeof(T));

        /// <summary>
        /// Reads a value of type <typeparamref name="T"/> from <see cref="ByteArray"/>
        /// starting at the <paramref name="bitIndex"/> of <paramref name="byteIndex"/>.
        /// </summary>
        /// <param name="byteIndex">The index of the <typeparamref name="T"/> the value is stored in.</param>
        /// <param name="bitIndex">The index of the value's first bit relative to <paramref name="byteIndex"/>.</param>
        /// <param name="bitLength">The length of the value in bits.</param>
        /// <inheritdoc cref="Get{T}(int)"/>
        public T Get<T>(int byteIndex, int bitIndex, int bitLength = 1)
            => (T)Convert.ChangeType(GetInt(byteIndex, bitIndex, bitLength), typeof(T));

        /// <summary>
        /// Reads an array of values of type <typeparamref name="T"/> from <see cref="ByteArray"/>.
        /// </summary>
        /// <typeparam name="T">A supported BAM value type.</typeparam>
        /// <param name="byteIndex">The index of the first entry in the array.</param>
        /// <param name="length">The number of entries in the array.</param>
        /// <returns>An array of values starting at the given location.</returns>
        public T[] GetArray<T>(int byteIndex, int length)
        {
            T[] arr = new T[length];
            for (int i = 0; i < length; i++)
                arr[i] = Get<T>(byteIndex + i * GetByteSize<T>());
            return arr;
        }


        /* ------------------------------------
         * Writing Methods
         * ------------------------------------
        */
        /// <summary>
        /// Writes an unsigned integer to the byte array.<br/>
        /// Note that overflow will be truncated.
        /// </summary>
        /// <param name="value">The value to be set.</param>
        /// <param name="byteIndex">The index the value will be stored in.</param>
        /// <param name="byteLength">The length of the value in bytes. Must be a value from 1-4.</param>
        private void SetInt(uint value, int byteIndex, int byteLength)
        {
            for (int i = 0; i < byteLength; i++)
                ByteArray[byteIndex + i] = (byte)((value >> (8 * (BigEndian ? (byteLength - 1 - i) : i))) & 0xFF);
        }

        /// <summary>
        /// Writes a bit level-value stored within an unsigned integer to the byte array.
        /// </summary>
        /// <param name="byteIndex">The index of the unsigned integer the value is stored in.</param>
        /// <param name="bitIndex">The index of the value's first bit relative to <paramref name="byteIndex"/>.</param>
        /// <param name="bitLength">The length of the value in bits. Must be a value from 0-32.</param>
        private void SetInt(uint value, int byteIndex, int bitIndex, int bitLength)
        {
            if (bitIndex is < 0 or > 32)
                throw new ArgumentException($"{nameof(bitIndex)} must be a value from 0-32.", nameof(bitIndex));

            if (bitIndex + bitLength is < 0 or > 32)
                throw new ArgumentException($"{nameof(bitIndex)} + {nameof(bitLength)} must be anything from 0-32.", nameof(bitLength));

            for (int i = 0; i < bitLength; i++)
            {
                int by = byteIndex + (bitIndex + i) / 8;
                int bi = (bitIndex + i) % 8;
                uint temp = ByteArray[by];
                temp.SetBits(value.GetBits(i, 1), bi, 1);
                ByteArray[by] = (byte)temp;
            }
        }

        /// <summary>
        /// Writes a value of type <typeparamref name="T"/> to
        /// <see cref="ByteArray"/> at the <paramref name="byteIndex"/>.
        /// </summary>
        /// <typeparam name="T">A supported BAM value type.</typeparam>
        /// <param name="value">The value to set.</param>
        /// <param name="byteIndex">The index the value is to be stored in.</param>
        public void Set<T>(T value, int byteIndex)
            => SetInt((uint)Convert.ChangeType(value, typeof(uint)), byteIndex, GetByteSize<T>());

        /// <summary>
        /// Writes a value of type <typeparamref name="T"/> to <see cref="ByteArray"/>
        /// starting at the <paramref name="bitIndex"/> of <paramref name="byteIndex"/>.
        /// </summary>
        public void Set<T>(T value, int byteIndex, int bitIndex, int bitLength = 1)
            => SetInt((uint)Convert.ChangeType(value, typeof(uint)), byteIndex, bitIndex, bitLength);

        /// <summary>
        /// Writes an array of values of type <typeparamref name="T"/> to
        /// <see cref="ByteArray"/> starting at <paramref name="byteIndex"/>.
        /// </summary>
        /// <typeparam name="T">A supported BAM value type.</typeparam>
        /// <param name="values">The values to be set.</param>
        /// <param name="byteIndex">The index of the first entry in the array.</param>
        /// <param name="length">The number of entries in the array.</param>
        public void SetArray<T>(T[] values, int byteIndex, int length)
        {
            for (int i = 0; i < length; i++)
                Set(values[i], byteIndex + i * GetByteSize<T>());
        }

        /// <inheritdoc cref="SetArray{T}(T[], int, int)"/>
        public void SetArray<T>(T[] values, int byteIndex)
            => SetArray(values, byteIndex, values.Length);
    }
}
