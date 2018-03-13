using System.IO;

namespace TombLib.Utils
{
    public static class ZLib
    {
        public const int DefaultCompressionLevel = 10;

        public static byte[] CompressData(Stream inStream, int compressionLevel = DefaultCompressionLevel)
        {
            using (var outStream = new MemoryStream())
            {
                MiniZ.Functions.Compress(inStream, outStream, compressionLevel);
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

        public static byte[] CompressData(byte[] inData, int compressionLevel = DefaultCompressionLevel)
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
