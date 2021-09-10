using System.Text.RegularExpressions;

namespace TombLib.Utils
{
    public static class TextEncoding
    {
        public static string ConvertToANSI(this string source)
        {
            var src  = System.Text.Encoding.UTF8;
            var dest = System.Text.Encoding.GetEncoding(1252);
            return dest.GetString(System.Text.Encoding.Convert(src, dest, src.GetBytes(source)));
        }

        public static bool IsANSI(this string source)
        {
            var regex = new Regex("^[a-zA-Z0-9. -_?]*$");
            return regex.IsMatch(source);
        }
    }
}
