using System.IO;

namespace TombEditor
{
    public static class ZLib
    {
        private static void CopyStream(Stream input, Stream output)
        {
            byte[] buffer = new byte[2048];
            do
            {
                int len = input.Read(buffer, 0, buffer.GetLength(0));
                if (len <= 0)
                    break;
                output.Write(buffer, 0, len);
            } while (true);
        }

        public static byte[] CompressData(Stream inStream)
        {
            using (var outStream = new MemoryStream())
            {
                using (var outZStream = new zlib.ZOutputStream(outStream, zlib.zlibConst.Z_BEST_COMPRESSION))
                    CopyStream(inStream, outZStream);
                return outStream.ToArray();
            }
        }

        public static byte[] DecompressData(Stream inStream)
        {
            using (var outStream = new MemoryStream())
                using (var outZStream = new zlib.ZOutputStream(inStream))
                {
                    CopyStream(inStream, outZStream);
                    outZStream.finish();
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
