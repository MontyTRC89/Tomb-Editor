using NLog;
using SharpDX.Toolkit.Graphics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;

namespace TombEditor.Geometry.IO
{
    public static class ResourceLoader
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        public static string BrowseTexture(string previousPath, string levelPath, IWin32Window owner)
        {
            using (var dialog = new OpenFileDialog())
            {
                dialog.CheckFileExists = true;
                dialog.CheckPathExists = true;
                if (!string.IsNullOrEmpty(previousPath))
                    dialog.InitialDirectory = Path.GetDirectoryName(previousPath);
                else if (!string.IsNullOrEmpty(levelPath))
                    dialog.InitialDirectory = Path.GetDirectoryName(levelPath);

                dialog.Filter =
                    "Recommended image files (*.png, *.tga, *.bmp)|*.png;*.tga;*.bmp|" +
                    "PNG images (*.png)|*.png|" +
                    "Targe images (*.tga)|*.tga|" +
                    "Bitmap images (*.bmp)|*.bmp|" +
                    "All files (*.*)|*";
                dialog.Title = "Load texture map";

                if (dialog.ShowDialog(owner) != DialogResult.OK)
                    return null;
                return dialog.FileName;
            }
        }

        public static string BrowseWad(string previousPath, string levelPath, IWin32Window owner)
        {
            using (var dialog = new OpenFileDialog())
            {
                dialog.CheckFileExists = true;
                dialog.CheckPathExists = true;
                if (!string.IsNullOrEmpty(previousPath))
                    dialog.InitialDirectory = Path.GetDirectoryName(previousPath);
                else if (!string.IsNullOrEmpty(levelPath))
                    dialog.InitialDirectory = Path.GetDirectoryName(levelPath);

                dialog.Filter = "Tomb Raider WAD (*.wad)|*.wad|All files (*.*)|*.*";
                dialog.Title = "Load texture map";

                if (dialog.ShowDialog(owner) != DialogResult.OK)
                    return null;
                return dialog.FileName;
            }
        }

        public static void TryLoadingTexture(Level level, string currentPath, GraphicsDevice device, IProgressReporter progressReporter)
        {
            if (!string.IsNullOrEmpty(currentPath))
            {
                bool retry;
                do
                {
                    retry = false;

                    // Resolve relative paths
                    if (!Path.IsPathRooted(currentPath))
                        currentPath = Path.Combine(Path.GetDirectoryName(level.FileName), currentPath);

                    // Try loading the file
                    try
                    {
                        level.LoadTexture(currentPath, device);
                    }
                    catch (Exception exc)
                    {
                        logger.Warn(exc, "Unable to load texture file \"" + currentPath + "\".");

                        progressReporter.InvokeGui(delegate (IWin32Window owner)
                            {
                                switch (MessageBox.Show(owner, "The texture file '" + currentPath + " could not be loaded. " +
                                    "Do you want to load a substituting file now?", "Open project",
                                    MessageBoxButtons.YesNoCancel, MessageBoxIcon.Asterisk, MessageBoxDefaultButton.Button2))
                                {
                                    case DialogResult.Yes:
                                        currentPath = BrowseTexture(currentPath, level.FileName, owner);
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
        }

        public static void TryLoadingWad(Level level, string currentPath, GraphicsDevice device, IProgressReporter progressReporter)
        {
            if (!string.IsNullOrEmpty(currentPath))
            {
                bool retry;
                do
                {
                    retry = false;

                    // Resolve relative paths
                    if (!Path.IsPathRooted(currentPath))
                        currentPath = Path.Combine(Path.GetDirectoryName(level.FileName), currentPath);

                    // Try loading the file
                    try
                    {
                        level.LoadWad(currentPath, device);
                    }
                    catch (Exception exc)
                    {
                        logger.Warn(exc, "Unable to load WAD file \"" + currentPath + "\".");

                        progressReporter.InvokeGui(delegate(IWin32Window owner)
                            {
                                switch (MessageBox.Show(owner, "The *.wad file '" + currentPath + " could not be loaded. " +
                                    "Do you want to load a substituting file now?", "Open project",
                                    MessageBoxButtons.YesNoCancel, MessageBoxIcon.Asterisk, MessageBoxDefaultButton.Button2))
                                {
                                    case DialogResult.Yes:
                                        currentPath = BrowseWad(currentPath, level.FileName, owner);
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
        }
    }
}
