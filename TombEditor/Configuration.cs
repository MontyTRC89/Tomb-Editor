using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TombEditor
{
    public class Configuration
    {
        public int DrawRoomsMaxDepth = 6;

        public static string GetDefaultPath()
        {
            return Path.GetDirectoryName(Application.ExecutablePath) + "\\TombEditor.ini";
        }

        public static Configuration LoadFrom(string path)
        {
            Configuration result = new Configuration();
            result.DrawRoomsMaxDepth = Int32.Parse(Read(path, "NumRoomsToDraw", "Configuration", "6"));
            return result;
        }

        [DllImport("kernel32", CharSet = CharSet.Unicode)]
        private static extern long WritePrivateProfileString(string Section, string Key, string Value, string FilePath);

        [DllImport("kernel32", CharSet = CharSet.Unicode)]
        private static extern int GetPrivateProfileString(string Section, string Key, string Default, StringBuilder RetVal, int Size, string FilePath);

        private static string Read(string path, string Key, string Section, string Default)
        {
            var RetVal = new StringBuilder(255);

            GetPrivateProfileString(Section, Key, "", RetVal, 255, path);
            string result = RetVal.ToString();

            return (result != "" ? result : Default);
        }

        private static void Write(string path, string Key, string Value, string Section)
        {
            WritePrivateProfileString(Section, Key, Value, path);
        }

        private static void DeleteKey(string path, string Key, string Section)
        {
            Write(path, Key, null, Section);
        }

        private static void DeleteSection(string path, string Section)
        {
            Write(path, null, null, Section);
        }

        private static bool KeyExists(string path, string Key, string Section)
        {
            return Read(path, Key, Section, "").Length > 0;
        }
    }
}
