using NLog;
using System;
using System.IO;
using System.Windows.Forms;
using TombLib.LevelData;
using TombLib.Utils;
using TombLib.Wad;

namespace TombLib.Forms
{
    public class GraphicalDialogHandler : IDialogHandler
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

                dialog.Filter = LevelTexture.FileExtensions.GetFilter();
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

                dialog.Filter = Wad2.WadFormatExtensions.GetFilter();
                dialog.Title = "Load object file (WAD)";

                if (dialog.ShowDialog(owner) != DialogResult.OK)
                    return wadPath;
                return settings.MakeRelative(dialog.FileName, VariableType.LevelDirectory);
            }
        }

        public static void HandleDialog(IDialogDescription dialogDescription_, IWin32Window owner)
        {
            if (dialogDescription_ is DialogDescriptonTextureUnloadable)
            {
                var dialogDescription = (DialogDescriptonTextureUnloadable)dialogDescription_;

                bool ignoreError = false;
                while (dialogDescription.Texture.LoadException != null && !ignoreError)
                    owner.InvokeIfNecessary(() =>
                    {
                        switch (MessageBox.Show(owner, "The texture file '" + dialogDescription.Settings.MakeAbsolute(dialogDescription.Texture.Path) +
                            " could not be loaded: " + (dialogDescription.Texture.LoadException?.Message ?? "null") + ". \n" +
                            "Do you want to load a substituting file now?", "Open project",
                            MessageBoxButtons.YesNoCancel, MessageBoxIcon.Asterisk, MessageBoxDefaultButton.Button2))
                        {
                            case DialogResult.Yes:
                                dialogDescription.Texture.SetPath(dialogDescription.Settings, BrowseTextureFile(dialogDescription.Settings, dialogDescription.Texture.Path, owner));
                                break;
                            case DialogResult.No:
                                ignoreError = true;
                                break;
                            case DialogResult.Cancel:
                                throw new OperationCanceledException("Canceled because texture was not loadable");
                        }
                    });
            }
            else if (dialogDescription_ is DialogDescriptonWadUnloadable)
            {
                var dialogDescription = (DialogDescriptonWadUnloadable)dialogDescription_;

                bool ignoreError = false;
                while (dialogDescription.Wad.LoadException != null && !ignoreError)
                    owner.InvokeIfNecessary(() =>
                    {
                        switch (MessageBox.Show(owner, "The objects file '" + dialogDescription.Settings.MakeAbsolute(dialogDescription.Wad.Path) +
                            " could not be loaded: " + (dialogDescription.Wad.LoadException?.Message ?? "null") + ". \n" +
                            "Do you want to load a substituting file now?", "Open project",
                            MessageBoxButtons.YesNoCancel, MessageBoxIcon.Asterisk, MessageBoxDefaultButton.Button2))
                        {
                            case DialogResult.Yes:
                                dialogDescription.Wad.SetPath(dialogDescription.Settings, BrowseObjectFile(dialogDescription.Settings, dialogDescription.Wad.Path, owner));
                                break;
                            case DialogResult.No:
                                ignoreError = true;
                                break;
                            case DialogResult.Cancel:
                                throw new OperationCanceledException("Canceled because wad file was not loadable");
                        }
                    });
            }
            else if (dialogDescription_ is DialogDescriptonMissingSounds)
            {
                var dialogDescription = (DialogDescriptonMissingSounds)dialogDescription_;

                DialogResult result = DialogResult.Cancel;
                owner.InvokeIfNecessary(() =>
                {
                    using (var form = new ImportTr4WadDialog(dialogDescription))
                        result = form.ShowDialog(owner);
                });

                if (result != DialogResult.OK)
                    throw new OperationCanceledException("Canceled because sounds are missing.");
            }
            else
            {
                logger.Info("Ignored dialog event of type " + dialogDescription_.GetType().FullName + ".");
            }
        }

        private readonly IWin32Window _owner;

        public GraphicalDialogHandler(IWin32Window owner)
        {
            _owner = owner;
        }

        void IDialogHandler.RaiseDialog(IDialogDescription description)
        {
            HandleDialog(description, _owner);
        }
    }
}
