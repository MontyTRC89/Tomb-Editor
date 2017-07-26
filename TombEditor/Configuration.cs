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
        [DllImport("kernel32", CharSet = CharSet.Unicode)]
        static extern long WritePrivateProfileString(string Section, string Key, string Value, string FilePath);

        [DllImport("kernel32", CharSet = CharSet.Unicode)]
        static extern int GetPrivateProfileString(string Section, string Key, string Default, StringBuilder RetVal, int Size, string FilePath);

        private static string _path = Path.GetDirectoryName(Application.ExecutablePath) + "\\TombEditor.ini";

        public static int DrawRoomsMaxDepth = 6;

        public static string Read(string Key, string Section, string Default)
        {
            var RetVal = new StringBuilder(255);

            GetPrivateProfileString(Section, Key, "", RetVal, 255, _path);
            string result = RetVal.ToString();

            return (result != "" ? result : Default);
        }

        public static void Write(string Key, string Value, string Section)
        {
            WritePrivateProfileString(Section, Key, Value, _path);
        }

        public static void DeleteKey(string Key, string Section)
        {
            Write(Key, null, Section);
        }

        public static void DeleteSection(string Section)
        {
            Write(null, null, Section);
        }

        public static bool KeyExists(string Key, string Section)
        {
            return Read(Key, Section, "").Length > 0;
        }

        public static void LoadConfiguration()
        {
            DrawRoomsMaxDepth = Int32.Parse(Read("NumRoomsToDraw", "Configuration", "6"));
        }
    }
}
