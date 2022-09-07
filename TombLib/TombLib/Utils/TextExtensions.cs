using DarkUI.Forms;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace TombLib.Utils
{
    public static class TextExtensions
    {
        public const string QuoteChar = "\"";

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

        public static bool CheckAndWarnIfNotANSI(this string source, IWin32Window owner)
        {
            if (!source.IsANSI())
            {
                DarkMessageBox.Show(owner, "Filename or path is invalid. Please use standard characters.", "Wrong filename", MessageBoxIcon.Error);
                return false;
            }
            else
                return true;
        }

        public static string SplitCamelcase(this string source)
        {
            return Regex.Replace(source, "([a-z](?=[A-Z])|[a-z](?=[0-9])|[A-Z](?=[A-Z][a-z]))", "$1 ");
        }

        public static string Capitalize(this string source)
        {
            if (string.IsNullOrEmpty(source))
                return source;

            return source.First().ToString().ToUpper() + string.Join(string.Empty, source.Skip(1));
        }

        public static string Unquote(string source)
        {
            if (source.StartsWith(QuoteChar) && source.EndsWith(QuoteChar))
                return source.Substring(1, source.Length - 2);
            else
                return source;
        }

        public static string Quote(string source)
        {
                return QuoteChar + source + QuoteChar;
        }

        public static List<string> ExtractValues(string source)
        {
            return source.Split('"').Where((item, index) => index % 2 != 0).ToList();
        }
    }
}
