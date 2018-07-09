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
                            switch (MessageBox.Show(owner, "The texture file '" + dialogDescription.Settings.MakeAbsolute(dialogDescription.Texture.Path) +
                        " could not be loaded: " + (dialogDescription.Texture.LoadException?.Message ?? "null") + ". \n" +
                        "Do you want to load a substituting file now?", "Open project",
                        MessageBoxButtons.YesNoCancel, MessageBoxIcon.Asterisk, MessageBoxDefaultButton.Button2))
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
                            switch (MessageBox.Show(owner, "The objects file '" + dialogDescription.Settings.MakeAbsolute(dialogDescription.Wad.Path) +
                            " could not be loaded: " + (dialogDescription.Wad.LoadException?.Message ?? "null") + ". \n" +
                            "Do you want to load a substituting file now?", "Open project",
                            MessageBoxButtons.YesNoCancel, MessageBoxIcon.Asterisk, MessageBoxDefaultButton.Button2))
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
            lock (_lock)
                HandleDialog(description, _owner);
        }
    }
}
