using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using TombLib.LevelData;

namespace TombLib.Utils
{
    public class LevelFileDialog
    {
        // Returns 'null' if no file is chosen.
        public static string BrowseFile(IWin32Window owner, LevelSettings levelSettings, string currentPath, string title, IEnumerable<FileFormat> fileFormats, bool save, VariableType? baseDirType)
        {
            using (FileDialog dialog = save ? (FileDialog)new SaveFileDialog() : new OpenFileDialog())
            {
                dialog.Filter = fileFormats.GetFilter();
                dialog.Title = title;
                if (!string.IsNullOrEmpty(currentPath))
                {
                    string path = levelSettings?.MakeAbsolute(currentPath) ?? currentPath;
                    dialog.FileName = Path.GetFileName(path);
                    dialog.InitialDirectory = Path.GetDirectoryName(path);
                }
                if (dialog.ShowDialog(owner) != DialogResult.OK)
                    return null;
                return baseDirType != null && levelSettings != null ? levelSettings.MakeRelative(dialog.FileName, baseDirType.Value) : dialog.FileName;
            }
        }

        // Returns 'null' if no folder is chosen.
        public static string BrowseFolder(IWin32Window owner, LevelSettings levelSettings, string currentPath, string title, VariableType? baseDirType)
        {
            using (var dialog = new BrowseFolderDialog())
            {
                dialog.Title = title;
                if (!string.IsNullOrEmpty(currentPath))
                    dialog.InitialFolder = levelSettings?.MakeAbsolute(currentPath) ?? currentPath;
                if (dialog.ShowDialog(owner) != DialogResult.OK)
                    return null;
                return baseDirType != null && levelSettings != null ? levelSettings.MakeRelative(dialog.Folder, baseDirType.Value) : dialog.Folder;
            }
        }
    }
}
