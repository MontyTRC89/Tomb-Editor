using NLog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using System.Drawing;
using System.Drawing.Imaging;

namespace TombEditor.Geometry.IO
{
    public static class ResourceLoader
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        public static bool BrowseTextureFile(LevelSettings settings, IWin32Window owner)
        {
            string path = settings.MakeAbsolute(settings.TextureFilePath);
            using (var dialog = new OpenFileDialog())
            {
                dialog.CheckFileExists = true;
                dialog.CheckPathExists = true;
                if (!string.IsNullOrEmpty(settings.TextureFilePath))
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
                    return false;
                settings.TextureFilePath = settings.MakeRelative(dialog.FileName, VariableType.LevelDirectory);
            }
            return true;
        }

        public static bool BrowseObjectFile(LevelSettings settings, IWin32Window owner)
        {
            string path = settings.MakeAbsolute(settings.WadFilePath);
            using (var dialog = new OpenFileDialog())
            {
                dialog.CheckFileExists = true;
                dialog.CheckPathExists = true;
                if (!string.IsNullOrEmpty(settings.WadFilePath))
                {
                    dialog.InitialDirectory = Path.GetDirectoryName(path);
                    dialog.FileName = Path.GetFileName(path);
                }
                else
                    dialog.InitialDirectory = path;

                dialog.Filter = "Tomb Raider WAD (*.wad)|*.wad|All files (*.*)|*.*";
                dialog.Title = "Load object file (WAD)";

                if (dialog.ShowDialog(owner) != DialogResult.OK)
                    return false;
                settings.WadFilePath = settings.MakeRelative(dialog.FileName, VariableType.LevelDirectory);
            }
            return true;
        }

        public static void TryLoadingTexture(Level level, IProgressReporter progressReporter)
        {
            if (string.IsNullOrEmpty(level.Settings.TextureFilePath))
                return;

            bool retry;
            do
            {
                retry = false;

                // Try loading the file
                try
                {
                    level.ReloadTexture();
                }
                catch (Exception exc)
                {
                    string path = level.Settings.MakeAbsolute(level.Settings.TextureFilePath);
                    logger.Warn(exc, "Unable to load texture file \"" + path + "\".");

                    progressReporter.InvokeGui(delegate (IWin32Window owner)
                        {
                            switch (MessageBox.Show(owner, "The texture file '" + path + " could not be loaded. " +
                                "Do you want to load a substituting file now?", "Open project",
                                MessageBoxButtons.YesNoCancel, MessageBoxIcon.Asterisk, MessageBoxDefaultButton.Button2))
                            {
                                case DialogResult.Yes:
                                    if (BrowseTextureFile(level.Settings, owner))
                                        retry = true;
                                    break;
                                case DialogResult.No:
                                    break;
                                case DialogResult.Cancel:
                                    throw new OperationCanceledException("Canceled because texture was not loadable");
                            }
                        });
                }
            } while (retry);
        }

        public static void TryLoadingObjects(Level level, IProgressReporter progressReporter)
        {
            if (string.IsNullOrEmpty(level.Settings.WadFilePath))
                return;

            bool retry;
            do
            {
                retry = false;

                // Try loading the file
                try
                {
                    level.ReloadWad();
                }
                catch (Exception exc)
                {
                    string path = level.Settings.MakeAbsolute(level.Settings.WadFilePath);
                    logger.Warn(exc, "Unable to load object file \"" + path + "\".");

                    progressReporter.InvokeGui(delegate (IWin32Window owner)
                        {
                            switch (MessageBox.Show(owner, "The *.wad file '" + path + " could not be loaded. " +
                                "Do you want to load a substituting file now?", "Open project",
                                MessageBoxButtons.YesNoCancel, MessageBoxIcon.Asterisk, MessageBoxDefaultButton.Button2))
                            {
                                case DialogResult.Yes:
                                    if (BrowseObjectFile(level.Settings, owner))
                                        retry = true;
                                    break;
                                case DialogResult.No:
                                    break;
                                case DialogResult.Cancel:
                                    throw new OperationCanceledException("Canceled because *.wad was not loadable");
                            }
                        });
                }
            } while (retry);
        }

        public static Bitmap LoadRawExtraTexture(string path)
        {
            using (FileStream reader = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                Bitmap bmp = null;
                try
                {
                    if (path.EndsWith(".raw"))
                    {
                        // Raw file: 256² pixels with 24 bpp
                        byte[] data = new byte[256 * 256 * 3];
                        reader.Read(data, 0, data.Length);

                        bmp = new Bitmap(256, 256, PixelFormat.Format24bppRgb);
                        BitmapData lockData = bmp.LockBits(new Rectangle(0, 0, 256, 256), ImageLockMode.WriteOnly, PixelFormat.Format24bppRgb);
                        System.Runtime.InteropServices.Marshal.Copy(data, 0, lockData.Scan0, data.Length);
                        bmp.UnlockBits(lockData);
                    }
                    else if (path.EndsWith(".pc"))
                    {
                        // Raw file: 256² pixels with 32 bpp
                        byte[] data = new byte[256 * 256 * 4];
                        reader.Read(data, 0, data.Length);
                        
                        bmp = new Bitmap(256, 256, PixelFormat.Format32bppArgb);
                        BitmapData lockData = bmp.LockBits(new Rectangle(0, 0, 256, 256), ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);
                        System.Runtime.InteropServices.Marshal.Copy(data, 0, lockData.Scan0, data.Length);
                        bmp.UnlockBits(lockData);
                    }
                    else
                        bmp = TombLib.Graphics.TextureLoad.LoadToBitmap(reader);

                    if ((bmp.Width != 256) || (bmp.Height != 256))
                        throw new NotSupportedException("The texture's size must be 256 by 256 pixels. " +
                            "(The current texture '" + path + "' is " + bmp.Width + " by " + bmp.Height + " pixels)");
                }
                catch
                {
                    bmp?.Dispose();
                    throw;
                }
                return bmp;
            }
        }
    }
}
