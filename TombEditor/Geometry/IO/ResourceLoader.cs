using NLog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using System.Drawing;
using System.Drawing.Imaging;
using TombLib.Utils;

namespace TombEditor.Geometry.IO
{
    public static class ResourceLoader
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        public static string BrowseTextureFile(LevelSettings settings, string texturePath, IWin32Window owner)
        {
            string path = settings.MakeAbsolute(texturePath);
            using (var dialog = new OpenFileDialog())
            {
                dialog.CheckFileExists = true;
                dialog.CheckPathExists = true;
                if (!string.IsNullOrEmpty(texturePath))
                {
                    dialog.InitialDirectory = Path.GetDirectoryName(path);
                    dialog.FileName = Path.GetFileName(path);
                }
                else
                    dialog.InitialDirectory = path;

                dialog.Filter =
                    "Recommended image files (*.png, *.tga, *.bmp)|*.png;*.tga;*.bmp|" +
                    "PNG images (*.png)|*.png|" +
                    "Targe images (*.tga)|*.tga|" +
                    "Bitmap images (*.bmp)|*.bmp|" +
                    "All files (*.*)|*";
                dialog.Title = "Load texture map";

                if (dialog.ShowDialog(owner) != DialogResult.OK)
                    return texturePath;
                return settings.MakeRelative(dialog.FileName, VariableType.LevelDirectory);
            }
        }

        public static string BrowseObjectFile(LevelSettings settings, string wadPath, IWin32Window owner)
        {
            string path = settings.MakeAbsolute(wadPath);
            using (var dialog = new OpenFileDialog())
            {
                dialog.CheckFileExists = true;
                dialog.CheckPathExists = true;
                if (!string.IsNullOrEmpty(wadPath))
                {
                    dialog.InitialDirectory = Path.GetDirectoryName(path);
                    dialog.FileName = Path.GetFileName(path);
                }
                else
                    dialog.InitialDirectory = path;

                dialog.Filter = "Tomb Raider WAD (*.wad)|*.wad|All files (*.*)|*.*";
                dialog.Title = "Load object file (WAD)";

                if (dialog.ShowDialog(owner) != DialogResult.OK)
                    return wadPath;
                return settings.MakeRelative(dialog.FileName, VariableType.LevelDirectory);
            }
        }

        public static void CheckLoadedTexture(LevelSettings settings, LevelTexture texture, IProgressReporter progressReporter)
        {
            bool ignoreError = false;
            while ((texture.ImageLoadException != null) && !ignoreError)
                progressReporter.InvokeGui(delegate (IWin32Window owner)
                    {
                        switch (MessageBox.Show(owner, "The texture file '" + settings.MakeAbsolute(texture.Path) +
                            " could not be loaded: " + texture.ImageLoadException.Message + ". \n" +
                            "Do you want to load a substituting file now?", "Open project",
                            MessageBoxButtons.YesNoCancel, MessageBoxIcon.Asterisk, MessageBoxDefaultButton.Button2))
                        {
                            case DialogResult.Yes:
                                texture.SetPath(settings, BrowseTextureFile(settings, texture.Path, owner));
                                break;
                            case DialogResult.No:
                                ignoreError = true;
                                break;
                            case DialogResult.Cancel:
                                throw new OperationCanceledException("Canceled because texture was not loadable");
                        }
                    });
        }

        public static void TryLoadingObjects(Level level, IProgressReporter progressReporter)
        {
            if (string.IsNullOrEmpty(level.Settings.WadFilePath))
                return;
            
            do
            {
                // Try loading the file
                try
                {
                    level.ReloadWad();
                    return;
                }
                catch (Exception exc)
                {
                    string path = level.Settings.MakeAbsolute(level.Settings.WadFilePath);
                    logger.Warn(exc, "Unable to load object file \"" + path + "\".");

                    bool retry = false;
                    progressReporter.InvokeGui(delegate (IWin32Window owner)
                        {
                            switch (MessageBox.Show(owner, "The *.wad file '" + path + " could not be loaded. " +
                                "Do you want to load a substituting file now?", "Open project",
                                MessageBoxButtons.YesNoCancel, MessageBoxIcon.Asterisk, MessageBoxDefaultButton.Button2))
                            {
                                case DialogResult.Yes:
                                    string browsedPath = BrowseObjectFile(level.Settings, level.Settings.WadFilePath, owner);
                                    retry = browsedPath != level.Settings.WadFilePath;
                                    break;
                                case DialogResult.No:
                                    break;
                                case DialogResult.Cancel:
                                    throw new OperationCanceledException("Canceled because *.wad was not loadable");
                            }
                        });
                    if (!retry)
                        return;
                }
            } while (true);
        }

        public static ImageC LoadRawExtraTexture(string path)
        {
            using (FileStream reader = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                ImageC image;

                if (path.EndsWith(".raw"))
                {
                    // Raw file: 256² pixels with 24 bpp
                    byte[] data = new byte[256 * 256 * 3];
                    reader.Read(data, 0, data.Length);

                    image = ImageC.CreateNew(256, 256);
                    for (int i = 0; i < 256 * 256; ++i)
                        image.Set(i, data[i * 3 + 2], data[i * 3 + 1], data[i * 3]);
                }
                else if (path.EndsWith(".pc"))
                {
                    // Raw file: 256² pixels with 32 bpp
                    byte[] data = new byte[256 * 256 * 4];
                    reader.Read(data, 0, data.Length);

                    image = ImageC.CreateNew(256, 256);
                    for (int i = 0; i < 256 * 256; ++i)
                        image.Set(i, data[i * 4 + 2], data[i * 4 + 1], data[i * 4], data[i * 4 + 3]);
                }
                else
                    image = ImageC.FromStream(reader);

                if ((image.Width != 256) || (image.Height != 256))
                    throw new NotSupportedException("The texture's size must be 256 by 256 pixels. " +
                        "(The current texture '" + path + "' is " + image.Width + " by " + image.Height + " pixels)");
                return image;
            }
        }
    }
}
