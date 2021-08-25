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

        //Returns a power of two (only non-negative powers work)
        private static uint PowerOfTwo(int power)
        {
            return (uint)1 << power;
        }


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
            uint sum = 0;
            for (int i = 0; i < byteLength; i++)
                sum += ByteArray[byteIndex + i] * PowerOfTwo(8 * (BigEndian ? (byteLength - 1 - i) : i));

            return sum;
        }

        /// <summary>
        /// Reads a bit level-value stored within an unsigned integer from the byte array.
        /// </summary>
        /// <param name="byteIndex">The index of the unsigned integer the value is stored in.</param>
        /// <param name="byteLength">The length of the unsigned integer in bytes. Must be a value from 0-4.</param>
        /// <param name="bitIndex">The index of the value's first bit relative to <paramref name="byteIndex"/>.</param>
        /// <param name="bitLength">The length of the value in bits.</param>
        /// <returns>A <paramref name="bitLength"/> unsigned value starting
        ///          <paramref name="bitIndex"/> bits after <paramref name="byteIndex"/>.</returns>
        private uint GetIntBits(int byteIndex, int byteLength, int bitIndex, int bitLength)
        {
            if (byteLength > 4 || byteLength < 0)
                throw new ArgumentException($"{nameof(byteLength)} must be a value from 0-4.", nameof(byteLength));

            if (bitIndex > byteLength*8 || bitIndex < 0)
                throw new ArgumentException($"{nameof(bitIndex)} must be a value from 0-32.", nameof(bitIndex));

            if (bitIndex + bitLength > byteLength * 8 || bitIndex + bitLength < 0)
                throw new ArgumentException($"{nameof(bitIndex)} + {nameof(bitLength)} must be anything from 0-32.", nameof(bitLength));

            uint source = GetInt(byteIndex, byteLength);
            return source.GetBits(bitIndex, bitLength);
        }

        /// <summary>
        /// Reads a 4-byte unsigned integer from the byte array.
        /// </summary>
        /// <inheritdoc cref = "GetInt(int, int)"/>
        public uint GetUInt(int byteIndex)
        {
            return GetInt(byteIndex, 4);
        }

        /// <summary>
        /// Reads a bit-level value stored within a 4-byte unsigned integer from the byte array.
        /// </summary>
        /// <inheritdoc cref = "GetIntBits(int, int, int, int)"/>
        public uint GetUIntBits(int byteIndex, int bitIndex, int bitLength)
        {
            return GetIntBits(byteIndex, 4, bitIndex, bitLength);
        }

        /// <summary>
        /// Reads a 2-byte unsigned integer from the byte array.
        /// </summary>
        /// <inheritdoc cref = "GetInt(int, int)"/>
        public ushort GetUShort(int byteIndex)
        {
            return (ushort)GetInt(byteIndex, 2);
        }

        /// <summary>
        /// Reads a bit-level value stored within a 2-byte unsigned integer from the byte array.
        /// </summary>
        /// <inheritdoc cref = "GetIntBits(int, int, int, int)"/>
        public ushort GetUShortBits(int byteIndex, int bitIndex, int bitLength)
        {
            return (ushort)GetIntBits(byteIndex, 2, bitIndex, bitLength);
        }

        /// <summary>
        /// Reads a 1-byte unsigned integer from the byte array.
        /// </summary>
        /// <inheritdoc cref = "GetInt(int, int)"/>
        public byte GetByte(int byteIndex)
        {
            return (byte)GetInt(byteIndex, 1);
        }

        /// <summary>
        /// Reads a bit-level value stored within a 1-byte unsigned integer from the byte array.
        /// </summary>
        /// <inheritdoc cref = "GetIntBits(int, int, int, int)"/>
        public byte GetByteBits(int byteIndex, int bitIndex, int bitLength)
        {
            return (byte)GetIntBits(byteIndex, 1, bitIndex, bitLength);
        }

        /// <summary>
        /// Reads a boolean from the byte array.
        /// </summary>
        /// <returns>The <see cref="bool"/> at the given <paramref name="bitIndex"/>
        ///          and <paramref name="byteIndex"/>.</returns>
        /// <inheritdoc cref = "GetIntBits(int, int, int, int)"/>
        public bool GetBool(int byteIndex, int bitIndex)
        {
            return GetByteBits(byteIndex, bitIndex, 1) > 0;
        }

        /// <summary>
        /// Reads a contiguous range of bytes from the byte array.
        /// </summary>
        /// <param name="byteIndex">The index of the range's first byte.</param>
        /// <param name="byteLength">The length of the range.</param>
        /// <returns>The requested array of <see cref="byte"/>s.</returns>
        public byte[] GetBytes(int byteIndex, int byteLength)
        {
            byte[] temp = new byte[byteLength];
            Array.Copy(ByteArray, byteIndex, temp, 0, byteLength);
            return temp;
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
        /// <param name="byteLength">The length of the unsigned integer in bytes. Must be a value from 0-4.</param>
        /// <param name="bitIndex">The index of the value's first bit relative to <paramref name="byteIndex"/>.</param>
        /// <param name="bitLength">The length of the value in bits. Must be a value from 0-32.</param>
        private void SetIntBits(uint value, int byteIndex, int byteLength, int bitIndex, int bitLength)
        {
            if (byteLength > 4 || byteLength < 0)
                throw new ArgumentException($"{nameof(byteLength)} must be a value from 0-4.", nameof(byteLength));

            if (bitIndex > byteLength * 8 || bitIndex < 0)
                throw new ArgumentException($"{nameof(bitIndex)} must be a value from 0-32.", nameof(bitIndex));

            if (bitIndex + bitLength > byteLength * 8 || bitIndex + bitLength < 0)
                throw new ArgumentException($"{nameof(bitIndex)} + {nameof(bitLength)} must be anything from 0-32.", nameof(bitLength));

            uint destination = GetInt(byteIndex, byteLength);
            destination.SetBits(value, bitIndex, bitLength);
            SetInt(destination, byteIndex, byteLength);
        }

        /// <summary>
        /// Writes a 4-byte unsigned integer to the byte array.
        /// </summary>
        /// <inheritdoc cref = "SetInt(int, int)"/>
        public void SetUInt(uint value, int byteIndex)
        {
            SetInt(value, byteIndex, 4);
        }

        /// <summary>
        /// Writes a bit-level value stored within a 4-byte unsigned integer from the byte array.
        /// </summary>
        /// <inheritdoc cref = "SetIntBits(int, int, int, int)"/>
        public void SetUIntBits(uint value, int byteIndex, int bitIndex, int bitLength)
        {
            SetIntBits(value, byteIndex, 4, bitIndex, bitLength);
        }

        /// <summary>
        /// Writes a 2-byte unsigned integer to the byte array.
        /// </summary>
        /// <inheritdoc cref = "SetInt(int, int)"/>
        public void SetUShort(ushort value, int byteIndex)
        {
            SetInt(value, byteIndex, 2);
        }

        /// <summary>
        /// Writes a bit-level value stored within a 2-byte unsigned integer from the byte array.
        /// </summary>
        /// <inheritdoc cref = "SetIntBits(int, int, int, int)"/>
        public void SetUShortBits(ushort value, int byteIndex, int bitIndex, int bitLength)
        {
            SetIntBits(value, byteIndex, 2, bitIndex, bitLength);
        }

        /// <summary>
        /// Writes a 4-byte unsigned integer to the byte array.
        /// </summary>
        /// <inheritdoc cref = "SetInt(int, int)"/>
        public void SetByte(byte value, int byteIndex)
        {
            SetInt(value, byteIndex, 1);
        }

        /// <summary>
        /// Writes a bit-level value stored within a 1-byte unsigned integer from the byte array.
        /// </summary>
        /// <inheritdoc cref = "SetIntBits(int, int, int, int)"/>
        public void SetByteBits(byte value, int byteIndex, int bitIndex, int bitLength)
        {
            SetIntBits(value, byteIndex, 1, bitIndex, bitLength);
        }

        /// <summary>
        /// Writes a boolean to the byte array.
        /// </summary>
        /// <param name="bitIndex">The index of the value relative to <paramref name="byteIndex"/>.</param>
        /// <inheritdoc cref = "GetIntBits(int, int, int, int)"/>
        public void SetBool(bool value, int byteIndex, int bitIndex)
        {
            SetByteBits(Convert.ToByte(value), byteIndex, bitIndex, 1);
        }

        /// <summary>
        /// Writes a contiguous range of bytes to the byte array.
        /// </summary>
        /// <param name="value">The bytes to be written.</param>
        /// <param name="byteIndex">The index the bytes will be stored in.</param>
        /// <param name="length">The number of bytes, starting from index 0, to copy from <paramref name="value"/>.</param>
        public void SetBytes(byte[] value, int byteIndex, int length)
        {
            Array.Copy(value, 0, ByteArray, byteIndex, length);
        }

        /// <inheritdoc cref = "SetBytes(byte[], int, int)"/>
        public void SetBytes(byte[] value, int byteIndex)
        {
            SetBytes(value, byteIndex, value.Length);
        }
    }
}
