using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TombLib.Utils
{
    public static class PathC
    {
        public static string GetRelativePath(string baseDir, string fileName)
        {
            if (string.IsNullOrEmpty(baseDir))
                return Path.GetFullPath(fileName);

            // https://stackoverflow.com/questions/9042861/how-to-make-an-absolute-path-relative-to-a-particular-folder
            //
            // Roughly based on (slightly improved)
            //   https://sourceforge.net/p/syncproj/code/HEAD/tree/syncProj.cs#l976
            //   makeRelative
            baseDir = Path.GetFullPath(baseDir);
            fileName = Path.GetFullPath(Path.Combine(baseDir, fileName));

            var dictionarySeperators = new string[] { Path.DirectorySeparatorChar.ToString(), Path.AltDirectorySeparatorChar.ToString() };
            string[] baseDirArr = baseDir.Split(dictionarySeperators, StringSplitOptions.RemoveEmptyEntries);
            string[] fileNameArr = fileName.Split(dictionarySeperators, StringSplitOptions.RemoveEmptyEntries);

            int i = 0;
            for (; i < baseDirArr.Length && i < fileNameArr.Length; i++)
                if (string.Compare(baseDirArr[i], fileNameArr[i], true) != 0) // Case insensitive match
                    break;
            if (i == 0) // Cannot make relative path, for example if resides on different drive
                return null;

            var resultFolders = Enumerable.Repeat("..", Math.Max(0, baseDirArr.Length - i)).Concat(fileNameArr.Skip(i));
            string result = string.Join(Path.DirectorySeparatorChar.ToString(), resultFolders);
            return result;
        }
    }
}
