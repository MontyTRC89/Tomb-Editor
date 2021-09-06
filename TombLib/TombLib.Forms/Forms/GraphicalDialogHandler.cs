using NLog;
using System;
using System.Windows.Forms;
using TombLib.LevelData;
using TombLib.Utils;
using TombLib.Wad;
using DarkUI.Forms;

namespace TombLib.Forms
{
    public class GraphicalDialogHandler : IDialogHandler
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();
        private object _lock = new object();

        public static void HandleDialog(IDialogDescription dialogDescription_, IWin32Window owner)
        {
            if (dialogDescription_ is DialogDescriptonTextureUnloadable)
            {
                var dialogDescription = (DialogDescriptonTextureUnloadable)dialogDescription_;

                if (dialogDescription.Texture.LoadException != null)
                    owner.InvokeIfNecessary(() =>
                    {
                        while (dialogDescription.Texture.LoadException != null)
                            switch (DarkMessageBox.Show(owner, "The texture file '" + dialogDescription.Settings.MakeAbsolute(dialogDescription.Texture.Path) +
                        " could not be loaded: " + (dialogDescription.Texture.LoadException?.Message ?? "null") + ". \n" +
                        "Do you want to load a substituting file now?", "Open project",
                        MessageBoxButtons.YesNoCancel, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button2))
                            {
                                case DialogResult.Yes:
                                    dialogDescription.Texture.SetPath(dialogDescription.Settings,
                                            LevelFileDialog.BrowseFile(owner, dialogDescription.Settings, dialogDescription.Texture.Path,
                                                "Load a texture", LevelTexture.FileExtensions, VariableType.LevelDirectory, false));
                                    break; // Don't unlock, we don't want to have other messages in the meantime.
                                case DialogResult.No:
                                    return;
                                case DialogResult.Cancel:
                                    throw new OperationCanceledException("Canceled because texture was not loadable");
                            }
                    });
            }
            else if (dialogDescription_ is DialogDescriptonWadUnloadable)
            {
                var dialogDescription = (DialogDescriptonWadUnloadable)dialogDescription_;

                if (dialogDescription.Wad.LoadException != null)
                    owner.InvokeIfNecessary(() =>
                    {
                        while (dialogDescription.Wad.LoadException != null)
                            switch (DarkMessageBox.Show(owner, "The objects file '" + dialogDescription.Settings.MakeAbsolute(dialogDescription.Wad.Path) +
                            " could not be loaded: " + (dialogDescription.Wad.LoadException?.Message ?? "null") + ". \n" +
                            "Do you want to load a substituting file now?", "Open project",
                            MessageBoxButtons.YesNoCancel, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button2))
                            {
                                case DialogResult.Yes:
                                    dialogDescription.Wad.SetPath(dialogDescription.Settings,
                                        LevelFileDialog.BrowseFile(owner, dialogDescription.Settings, dialogDescription.Wad.Path,
                                        "Load an object file (*.wad)", Wad2.WadFormatExtensions, VariableType.LevelDirectory, false));
                                    break; // Don't unlock, we don't want to have other messages in the meantime.
                                case DialogResult.No:
                                    return;
                                case DialogResult.Cancel:
                                    throw new OperationCanceledException("Canceled because wad file was not loadable");
                            }
                    });
            }
            else if (dialogDescription_ is DialogDescriptonSoundsCatalogUnloadable)
            {
                var dialogDescription = (DialogDescriptonSoundsCatalogUnloadable)dialogDescription_;

                if (dialogDescription.Sounds.LoadException != null)
                    owner.InvokeIfNecessary(() =>
                    {
                        while (dialogDescription.Sounds.LoadException != null)
                            switch (DarkMessageBox.Show(owner, "Sound catalog file '" + dialogDescription.Settings.MakeAbsolute(dialogDescription.Sounds.Path) +
                            " could not be loaded: " + (dialogDescription.Sounds.LoadException?.Message ?? "null") + ". \n" +
                            "Do you want to load a substituting file now?", "Open project",
                            MessageBoxButtons.YesNoCancel, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button2))
                            {
                                case DialogResult.Yes:
                                    dialogDescription.Sounds.SetPath(dialogDescription.Settings,
                                        LevelFileDialog.BrowseFile(owner, dialogDescription.Settings, dialogDescription.Sounds.Path,
                                        "Load a sound catalog (*.sfx)", ReferencedSoundCatalog.FileExtensions, VariableType.LevelDirectory, false));
                                    break; // Don't unlock, we don't want to have other messages in the meantime.
                                case DialogResult.No:
                                    return;
                                case DialogResult.Cancel:
                                    throw new OperationCanceledException("Canceled because sound catalog file was not loadable");
                            }
                    });
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
            lock (_lock)
                HandleDialog(description, _owner);
        }
    }
}
