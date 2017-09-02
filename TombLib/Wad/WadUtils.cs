using BrandonHaynes.Security.SipHash;

namespace TombLib.Wad
{
    public static class WadUtils
    {
        public static char ToHexDigit(int i)
        {
            if (i < 10)
                return (char)(i + '0');
            return (char)(i - 10 + 'A');
        }

        public static string ToHexString(byte[] bytes)
        {
            var chars = new char[bytes.Length * 2 + 2];

            chars[0] = '0';
            chars[1] = 'x';

            for (int i = 0; i < bytes.Length; i++)
            {
                chars[2 * i + 2] = ToHexDigit(bytes[i] / 16);
                chars[2 * i + 3] = ToHexDigit(bytes[i] % 16);
            }

            return new string(chars);
        }

        public static string Hash(byte[] data)
        {
            byte[] key = { 0x00, 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08, 0x09, 0x0a, 0x0b, 0x0c, 0x0d, 0x0e, 0x0f };

            using (var h = new SipHash(key))
            {
                return ToHexString(h.ComputeHash(data));
            }
        }
    }
}
