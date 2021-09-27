using DarkUI.Forms;
using System.Text.RegularExpressions;
using System.Windows.Forms;

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
    }
}
