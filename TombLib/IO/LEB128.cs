using System;

namespace TombLib.IO
{
    // https://en.wikipedia.org/wiki/LEB128
    internal static class LEB128
    {
        public const long MaximumSize1Byte = 128L - 1;
        public const long MaximumSize2Byte = 128L * 128 - 1;
        public const long MaximumSize3Byte = 128L * 128 * 128 - 1;
        public const long MaximumSize4Byte = 128L * 128 * 128 * 128 - 1;
        public const long MaximumSize5Byte = 128L * 128 * 128 * 128 * 128 - 1;
        public const long MaximumSize6Byte = 128L * 128 * 128 * 128 * 128 * 128 - 1;
        public const long MaximumSize7Byte = 128L * 128 * 128 * 128 * 128 * 128 * 128 - 1;
        public const long MaximumSize8Byte = 128L * 128 * 128 * 128 * 128 * 128 * 128 * 128 - 1;
        public const long MaximumSize9Byte = long.MaxValue;

        public const int MaxLEB128Length = 9;

        public static long ReadULong(BinaryReaderEx stream)
        {
            long result = 0;
            for (int i = 0; i < MaxLEB128Length; ++i)
            {
                byte currentByte = stream.ReadByte();
                result |= ((long)(currentByte & 0x7F)) << (i * 7);
                if ((currentByte & 0x80) == 0)
                    break;
            }
            return result;
        }

        public static void WriteULong(BinaryWriterEx stream, long value, long maximumSize)
        {
            if ((value < 0) || (value > maximumSize))
                throw new ArgumentOutOfRangeException("value");

            do
            {
                byte currentByte = (byte)(value & 0x7F);
                value >>= 7;
                maximumSize >>= 7;
                if (maximumSize > 0)
                    currentByte |= 0x80;
                stream.Write(currentByte);
            } while (maximumSize > 0);
        }

        public static void WriteULong(BinaryWriterEx stream, long value)
        {
            WriteULong(stream, value, value);
        }
    }
}
