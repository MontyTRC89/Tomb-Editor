using DarkUI.Forms;
using System;
using System.Collections.Generic;
using System.IO;
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

        public static string[] SplitParenthesis(this string source)
        {
            return Regex.Matches(source, @"[^{},]+|\{[^{}]*\}").Select(m => m.Value).ToArray();
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
            if (string.IsNullOrEmpty(source))
                return source;

            if (source.StartsWith(QuoteChar) && source.EndsWith(QuoteChar))
                return source.Substring(1, source.Length - 2);
            else
                return source;
        }

        public static string Quote(string source)
        {
            if (string.IsNullOrEmpty(source))
                source = string.Empty;

            return QuoteChar + source + QuoteChar;
        }

        public static string ToLinuxPath(string source)
        {
            return source.Replace(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        }

        public static List<string> ExtractValues(string source)
        {
            if (string.IsNullOrEmpty(source))
                return new List<string>();

            return source.Split('"').Where((item, index) => index % 2 != 0).ToList();
        }

        public static string MultiLineToSingleLine(string source)
        {
            if (string.IsNullOrEmpty(source))
                return string.Empty;

            return source.Replace(Environment.NewLine, "\\n");
        }

        public static string SingleLineToMultiLine(string source)
        {
            if (string.IsNullOrEmpty(source))
                return string.Empty;

            return source.Replace("\\n", Environment.NewLine);
        }

        public static string ToDataSize(int dataSize)
        {
            return dataSize >= 1024 * 1024 ? $"{dataSize / (1024 * 1024)} MB" : $"{dataSize / 1024} KB";
        }
    }
}
