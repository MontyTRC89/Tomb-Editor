using System;
using System.Collections.Generic;
using System.IO;

namespace TombLib.Utils
{
    public static class ZLib
    {
        public static byte[] CompressData(Stream inStream)
        {
            using (var outStream = new MemoryStream())
            {
                MiniZ.Functions.Compress(inStream, outStream, int.MaxValue);
                return outStream.ToArray();
            }
        }

        public static byte[] DecompressData(Stream inStream)
        {
            using (var outStream = new MemoryStream())
            {
                MiniZ.Functions.Decompress(inStream, outStream);
                return outStream.ToArray();
            }
        }

        public static byte[] CompressData(byte[] inData)
        {
            using (var inStream = new MemoryStream(inData, false))
                return CompressData(inStream);
        }

        public static byte[] DecompressData(byte[] inData)
        {
            using (var inStream = new MemoryStream(inData, false))
                return DecompressData(inStream);
        }
    }
}
