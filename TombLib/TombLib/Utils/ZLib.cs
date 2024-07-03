using System.IO;
using System.IO.Compression;

namespace TombLib.Utils
{
    public static class ZLib
    {

        public static byte[] CompressData(Stream inStream, CompressionLevel level = CompressionLevel.Optimal)
        {
            using (var outStream = new MemoryStream())
            {
                using (var compressStream = new ZLibStream(outStream, level, false))
                {
                    inStream.CopyTo(compressStream);
                }
                return outStream.ToArray();

            }
        }

        public static byte[] DecompressData(Stream inStream)
        {
            using (var outStream = new MemoryStream())
            {
                using (var decompressStream = new ZLibStream(inStream, CompressionMode.Decompress))
                {
                    decompressStream.CopyTo(outStream);
                }
                return outStream.ToArray();
            }
        }

        public static byte[] CompressData(byte[] inData, CompressionLevel compressionLevel = CompressionLevel.Optimal)
        {
            using (var inStream = new MemoryStream(inData, false))
                return CompressData(inStream, compressionLevel);
        }

        public static byte[] DecompressData(byte[] inData)
        {
            using (var inStream = new MemoryStream(inData, false))
                return DecompressData(inStream);
        }
    }
}
