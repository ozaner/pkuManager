using System;
using System.Collections;

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
        public bool BigEndian { get; protected set; }

        /// <summary>
        /// The underlying byte array this BAM is wrapping.
        /// </summary>
        public byte[] ByteArray;

        /// <summary>
        /// The total number of bytes in the byte array.
        /// </summary>
        public int Length { get { return ByteArray.Length;  } }

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
        /// Implicit casts a BAM to its underlying byte array.
        /// </summary>
        /// <param name="BAM">The BAM to be casted to a byte array.</param>
        public static implicit operator byte[](ByteArrayManipulator BAM)
        {
            return BAM.ByteArray;
        }

        /// <summary>
        /// Reads a non-negative integer from the byte array.
        /// </summary>
        /// <param name="byteIndex">The index of the value.</param>
        /// <param name="byteLength">The length of the value in bytes. Must be a value from 1-4.</param>
        /// <returns>A <see cref="uint"/> of the requested integer.</returns>
        public uint GetUInt(int byteIndex, int byteLength)
        {
            if (byteLength < 1 || byteLength > 4)
                throw new ArgumentException("byteLength must be between 1-4.");

            uint sum = 0;
            for (int i = 0; i < byteLength; i++)
                sum += ByteArray[byteIndex + i] * PowerOfTwo(8 * (BigEndian ? (byteLength - 1 - i) : i));
            
            return sum;
        }

        /// <summary>
        /// Reads a non-negative integer from the byte array.
        /// </summary>
        /// <param name="byteIndex">The index of the value.</param>
        /// <param name="bitIndex">The index of the value's first bit relative to <paramref name="byteIndex"/>.<br/>
        ///                        This can be larger than 7, in which case the value starts at a proceeding byte.</param>
        /// <param name="bitLength">The length of the value in bits. Must be a value from 1-32.</param>
        /// <returns>A <see cref="uint"/> of the requested integer.</returns>
        public uint GetUInt(int byteIndex, int bitIndex, int bitLength)
        {
            if (bitLength < 1 || bitLength > 32)
                throw new ArgumentException("bitLength must be between 1-32.");

            if (BigEndian)
                throw new NotImplementedException($"{nameof(GetUInt)}(int, int, int) does not support BigEndian.");

            byteIndex += bitIndex / 8;
            bitIndex %= 8;

            uint sum = 0;
            for (int i = 0; i < bitLength; i++)
            {
                byte mask = (byte)(1 << bitIndex);
                if ((ByteArray[byteIndex] & mask) > 0) //if any bit after masking is 1
                    sum += PowerOfTwo(i);

                bitIndex++;
                if (bitIndex == 8)
                {
                    bitIndex = 0;
                    byteIndex++;
                }
            }
            return sum;
        }

        /// <summary>
        /// Reads a boolean from the byte array.
        /// </summary>
        /// <param name="byteIndex">The index of the byte the value is stored in.</param>
        /// <param name="bitIndex">The index of the value relative to <paramref name="byteIndex"/>.<br/>
        ///                        This can be larger than 7, in which case the value is in a proceeding byte.</param>
        /// <returns>The requested <see cref="bool"/>.</returns>
        public bool GetBool(int byteIndex, int bitIndex)
        {
            return GetUInt(byteIndex, bitIndex, 1) > 0;
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

        /// <summary>
        /// Writes a non-negative integer to the byte array.<br/>
        /// Note that overflow will be truncated.
        /// </summary>
        /// <param name="value">The value to be set.</param>
        /// <param name="byteIndex">The index the value will be stored in.</param>
        /// <param name="byteLength">The length of the value in bytes. Must be a value from 1-4.</param>
        public void SetUInt(int value, int byteIndex, int byteLength)
        {
            if (value < 0)
                throw new ArgumentException("Can only use SetUInt with positive integers.");
            SetUInt(Convert.ToUInt32(value), byteIndex, byteLength);
        }

        /// <summary>
        /// Writes a non-negative integer to the byte array.<br/>
        /// Note that overflow will be truncated.
        /// </summary>
        /// <param name="value">The value to be set.</param>
        /// <param name="byteIndex">The index the value will be stored in.</param>
        /// <param name="byteLength">The length of the value in bytes. Must be a value from 1-4.</param>
        public void SetUInt(uint value, int byteIndex, int byteLength)
        {
            if (byteLength < 1 || byteLength > 4)
                throw new ArgumentException("byteLength must be between 1-4.");

            for (int i = 0; i < byteLength; i++)
                ByteArray[byteIndex + i] = (byte)((value >> (8 * (BigEndian ? (byteLength - 1 - i) : i))) & 0xFF);
        }

        /// <summary>
        /// Writes a non-negative integer to the byte array.<br/>
        /// Note that overflow will be truncated.
        /// </summary>
        /// <param name="value">The value to be set. Negative values are not allowed.</param>
        /// <param name="byteIndex">The index the value will be stored in.</param>
        /// <param name="bitIndex">The index of the value's first bit relative to <paramref name="byteIndex"/>.<br/>
        ///                        This can be larger than 7, in which case the value will start at a proceeding byte.</param>
        /// <param name="bitLength">The length of the value in bits. Must be a value from 1-32.</param>
        public void SetUInt(int value, int byteIndex, int bitIndex, int bitLength)
        {
            if (value < 0)
                throw new ArgumentException("Can only use SetUInt with positive integers.");
            if (bitLength < 1 || bitLength > 32)
                throw new ArgumentException("bitLength must be between 1-32.");
            if (BigEndian)
                throw new NotImplementedException($"{nameof(SetUInt)}(int, int, int, int) does not support BigEndian.");

            byteIndex += bitIndex / 8;
            bitIndex %= 8;

            BitArray ba = new BitArray(new int[] { value });
            for (int i = 0; i < bitLength; i++)
            {
                byte mask = (byte)(1 << bitIndex);
                if (ba[i])
                    ByteArray[byteIndex] |= mask; // set to 1
                else
                    ByteArray[byteIndex] &= (byte)~mask; // Set to 0

                bitIndex++;
                if (bitIndex == 8)
                {
                    bitIndex = 0;
                    byteIndex++;
                }
            }
        }

        /// <summary>
        /// Writes a boolean to the byte array.<br/>
        /// </summary>
        /// <param name="value">The value to be set.</param>
        /// <param name="byteIndex">The index the byte the value will be stored in.</param>
        /// <param name="bitIndex">The index of the value relative to <paramref name="byteIndex"/>.<br/>
        ///                        This can be larger than 7, in which case the value will be in a proceeding byte.</param>
        public void SetBool(bool value, int byteIndex, int bitIndex)
        {
            SetUInt(Convert.ToInt32(value), byteIndex, bitIndex, 1);
        }

        /// <summary>
        /// Writes a contiguous range of bytes to the byte array.
        /// </summary>
        /// <param name="value">The bytes to be written.</param>
        /// <param name="byteIndex">The index the bytes will be stored in.</param>
        public void SetBytes(byte[] value, int byteIndex)
        {
            value.CopyTo(ByteArray, byteIndex);
        }

        //Returns a power of two (only non-negative powers work)
        private static uint PowerOfTwo(int power)
        {
            return (uint)1 << power;
        }
    }
}
