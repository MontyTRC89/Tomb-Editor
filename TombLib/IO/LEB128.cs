using System;
using System.IO;
using System.Runtime.CompilerServices;

namespace TombLib.IO
{
    // https://en.wikipedia.org/wiki/LEB128
    public static class LEB128
    {
        public const long MaximumSize1Byte = 64L - 1;
        public const long MaximumSize2Byte = 64L * 128 - 1;
        public const long MaximumSize3Byte = 64L * 128 * 128 - 1;
        public const long MaximumSize4Byte = 64L * 128 * 128 * 128 - 1;
        public const long MaximumSize5Byte = 64L * 128 * 128 * 128 * 128 - 1;
        public const long MaximumSize6Byte = 64L * 128 * 128 * 128 * 128 * 128 - 1;
        public const long MaximumSize7Byte = 64L * 128 * 128 * 128 * 128 * 128 * 128 - 1;
        public const long MaximumSize8Byte = 64L * 128 * 128 * 128 * 128 * 128 * 128 * 128 - 1;
        public const long MaximumSize9Byte = 64L * 128 * 128 * 128 * 128 * 128 * 128 * 128 * 128 - 1;
        public const long MaximumSize10Byte = long.MaxValue;

        public const int MaxLEB128Length = 10;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Write(BinaryWriterFast stream, long value, long maximumSize)
        {
            do
            {
                // Write byte
                byte currentByte = unchecked((byte)(value & 0x7F));
                if (maximumSize >> 6 == 0 || maximumSize >> 6 == -1)
                {
                    stream.Write(currentByte);

                    if (value >> 6 != 0 && value >> 6 != -1)
                        throw new OverflowException("Unable to write integer because the available space overflowed.");

                    return;
                }
                stream.Write((byte)(currentByte | 0x80));

                // Move data to next byte
                value >>= 7;
                maximumSize >>= 7;
            } while (true);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Write(BinaryWriterFast stream, long value) => Write(stream, value, value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static sbyte GetLength(BinaryWriterFast stream, long value)
        {
            sbyte length = 1;
            value >>= 6;
            while (value > 0)
            {
                value >>= 7;
                length += 1;
            }
            return length;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static long ReadLong(BinaryReader stream)
        {
            long result = 0;
            int currentShift = 0;

            byte currentByte;
            do
            {
                currentByte = stream.ReadByte();
                result |= (long)(currentByte & 0x7F) << currentShift;
                currentShift += 7;
            } while ((currentByte & 0x80) != 0);

            // Sign extend
            int shift = 64 - currentShift;
            if (shift > 0)
                result = (result << shift) >> shift;
            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int ReadInt(BinaryReader stream) => (int)Math.Min(Math.Max(ReadLong(stream), int.MinValue), int.MaxValue);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static short ReadShort(BinaryReader stream) => (short)Math.Min(Math.Max(ReadLong(stream), short.MinValue), short.MaxValue);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static sbyte ReadSByte(BinaryReader stream) => (sbyte)Math.Min(Math.Max(ReadLong(stream), sbyte.MinValue), sbyte.MaxValue);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint ReadUInt(BinaryReader stream) => (uint)Math.Min(Math.Max(ReadLong(stream), 0), uint.MaxValue);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ushort ReadUShort(BinaryReader stream) => (ushort)Math.Min(Math.Max(ReadLong(stream), 0), ushort.MaxValue);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static byte ReadByte(BinaryReader stream) => (byte)Math.Min(Math.Max(ReadLong(stream), 0), byte.MaxValue);
    }
}
