using DarkUI.Forms;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using TombLib.Forms;
using TombLib.Wad;
using TombLib.Utils;
using NLog;
using TombLib.GeometryIO;
using TombLib.Graphics;

namespace WadTool
{
    public static class WadActions
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        public static void LoadWad(WadToolClass tool, IWin32Window owner, bool destination)
        {
            // Open the file dialog
            string selectedFilePath;
            using (var dialog = new OpenFileDialog())
            {
                string previousFilePath;
                if (destination)
                    previousFilePath = tool.DestinationWad?.FileName;
                else
                    previousFilePath = tool.SourceWad?.FileName;

                if (!string.IsNullOrWhiteSpace(previousFilePath))
                    try
                    {
                        dialog.InitialDirectory = Path.GetDirectoryName(previousFilePath);
                        dialog.FileName = Path.GetFileName(previousFilePath);
                    }
                    catch
                    {
                        // ignored
                    }

                dialog.Filter = Wad2.WadFormatExtensions.GetFilter();
                dialog.Title = "Open " + (destination ? "destination" : "source") + " WAD - Wad2 - Level";
                if (dialog.ShowDialog(owner) != DialogResult.OK)
                    return;
                selectedFilePath = dialog.FileName;
            }

            // Load the WAD/Wad2
            Wad2 newWad = null;
            try
            {
                newWad = Wad2.ImportFromFile(selectedFilePath, tool.Configuration.OldWadSoundPaths2
                    .Select(soundPath => tool.Configuration.ParseVariables(soundPath)), new GraphicalDialogHandler(owner));
            }
            catch (OperationCanceledException)
            {
                return;
            }
            catch (Exception exc)
            {
                logger.Info(exc, "Unable to load " + (destination ? "destination" : "source") + " file from '" + selectedFilePath + "'.");
                DarkMessageBox.Show(owner, "Loading the file failed! \n" + exc.Message, "Loading failed", MessageBoxIcon.Error);
                return;
            }

            // Set wad
            if (destination)
                tool.DestinationWad = newWad;
            else
                tool.SourceWad = newWad;
        }

        public static void SaveWad(WadToolClass tool, IWin32Window owner, Wad2 wadToSave, bool ask)
        {
            if (wadToSave == null)
            {
                DarkMessageBox.Show(owner, "You don't have a valid opened Wad", "Error", MessageBoxIcon.Error);
                return;
            }

            // Figure out the output path
            string outPath = wadToSave.FileName;
            if (!string.IsNullOrWhiteSpace(wadToSave.FileName))
                try
                {
                    outPath = Path.ChangeExtension(outPath, "wad2");
                }
                catch
                {
                    // ignored
                }


            // Ask the  about it
            if (ask || string.IsNullOrWhiteSpace(outPath))
                using (var dialog = new SaveFileDialog())
                {
                    if (!string.IsNullOrWhiteSpace(outPath))
                    {
                        dialog.InitialDirectory = Path.GetDirectoryName(outPath);
                        dialog.FileName = Path.GetFileName(outPath);
                    }
                    dialog.Filter = Wad2Writer.FileFormats.GetFilter();
                    dialog.Title = "Save Wad2";
                    dialog.AddExtension = true;
                    if (dialog.ShowDialog(owner) != DialogResult.OK)
                        return;
                    outPath = dialog.FileName;
                }

            // Save the wad2
            try
            {
                Wad2Writer.SaveToFile(wadToSave, outPath);
            }
            catch (Exception exc)
            {
                logger.Warn(exc, "Unable to save to '" + outPath + "'");
                DarkMessageBox.Show(owner, "Unable to save to '" + outPath + "'.   " + exc, "Unable to save.");
            }
        }

        public static void CreateNewWad(WadToolClass tool, IWin32Window owner)
        {
            using (var form = new FormNewWad2())
            {
                if (form.ShowDialog(owner) == DialogResult.Cancel)
                    return;

                tool.DestinationWad = new Wad2 { SuggestedGameVersion = form.Version };
            }
        }

        public static void ChangeSlot(WadToolClass tool, IWin32Window owner)
        {
            Wad2 wad = tool.GetWad(tool.MainSelection?.WadArea);
            IWadObject wadObject = wad?.TryGet(tool.MainSelection?.Id);
            if (wad == null || wadObject == null)
            {
                DarkMessageBox.Show(owner, "You must have an object selected", "Error", MessageBoxIcon.Error);
                return;
            }

            // Ask for the new slot
            using (var form = new FormSelectSlot(wadObject.Id, wad.SuggestedGameVersion))
            {
                if (form.ShowDialog(owner) != DialogResult.OK)
                    return;
                if (form.NewId == wadObject.Id)
                    return;
                if (wad.Contains(form.NewId))
                {
                    DarkMessageBox.Show(owner, "The slot " + form.NewId.ToString(wad.SuggestedGameVersion) + " is already occupied.", "Error", MessageBoxIcon.Error);
                    return;
                }
                wad.AssignNewId(wadObject.Id, form.NewId);
            }
            tool.WadChanged(tool.MainSelection.Value.WadArea);
        }

        public static void ImportModelAsStaticMesh(WadToolClass tool, IWin32Window owner)
        {
            if (tool.DestinationWad == null)
            {
                DarkMessageBox.Show(owner, "You must have a wad opened", "Error", MessageBoxIcon.Error);
                return;
            }

            using (FileDialog dialog = new OpenFileDialog())
            {
                try
                {
                    dialog.InitialDirectory = Path.GetDirectoryName(tool.DestinationWad.FileName);
                    dialog.FileName = Path.GetFileName(tool.DestinationWad.FileName);
                }
                catch
                {
                    // ignored
                }

                dialog.Filter = BaseGeometryImporter.FileExtensions.GetFilter();
                dialog.Title = "Select a 3D file that you want to see imported.";
                if (dialog.ShowDialog(owner) != DialogResult.OK)
                    return;

                using (var form = new GeometryIOSettingsDialog(new IOGeometrySettings()))
                {
                    form.AddPreset(IOSettingsPresets.SettingsPresets);
                    if (form.ShowDialog(owner) != DialogResult.OK)
                        return;

                    var @static = new WadStatic(tool.DestinationWad.GetFirstFreeStaticMesh());
                    @static.Mesh = WadMesh.ImportFromExternalModel(dialog.FileName, form.Settings);
                    @static.VisibilityBox = @static.Mesh.BoundingBox;
                    @static.CollisionBox = @static.Mesh.BoundingBox;
                    tool.DestinationWad.Statics.Add(@static.Id, @static);
                    tool.DestinationWadChanged();
                }
            }
        }

        public static void CopyObject(WadToolClass tool, IWin32Window owner, List<IWadObjectId> objectIdsToMove, bool alwaysChooseId)
        {
            Wad2 sourceWad = tool.SourceWad;
            Wad2 destinationWad = tool.DestinationWad;

            if (destinationWad == null || sourceWad == null || objectIdsToMove.Count == 0)
            {
                DarkMessageBox.Show(owner, "You must have two wad loaded and at least one source object selected.", "Error", MessageBoxIcon.Error);
                return;
            }

            // Figure out the new ids if there are any id collisions
            IWadObjectId[] newIds = objectIdsToMove.ToArray();
            for (int i = 0; i < objectIdsToMove.Count; ++i)
            {
                if (!sourceWad.Contains(objectIdsToMove[i]))
                    continue;
                if (!alwaysChooseId)
                    if (!destinationWad.Contains(newIds[i]))
                        if (!newIds.Take(i).Contains(newIds[i])) // There also must not be collisions with the other custom assigned ids.
                            continue;

                // Ask for the new slot
                do
                {
                    if (DarkMessageBox.Show(owner, "The id " + newIds[i].ToString(destinationWad.SuggestedGameVersion) + " is already occupied in the destination wad. You can now choose a new Id.",
                        "Occupied slot", MessageBoxButtons.OKCancel, MessageBoxIcon.Question) != DialogResult.OK)
                        return;
                    using (var form = new FormSelectSlot(newIds[i], destinationWad.SuggestedGameVersion))
                    {
                        if (form.ShowDialog(owner) != DialogResult.OK)
                            return;
                        newIds[i] = form.NewId;
                    }
                } while (destinationWad.Contains(newIds[i]) || newIds.Take(i).Contains(newIds[i])); // There also must not be collisions with the other custom assigned ids.
            }

            // Move objects
            for (int i = 0; i < objectIdsToMove.Count; ++i)
            {
                IWadObject @object = sourceWad.TryGet(objectIdsToMove[i]);
                if (@object == null)
                    continue;
                destinationWad.Add(newIds[i], @object);
            }

            // Update the situation
            tool.DestinationWadChanged();
        }

        public static void EditObject(WadToolClass tool, IWin32Window owner, DeviceManager deviceManager)
        {
            Wad2 wad = tool.GetWad(tool.MainSelection?.WadArea);
            IWadObject wadObject = wad?.TryGet(tool.MainSelection?.Id);
            if (wad == null || wadObject == null)
            {
                DarkMessageBox.Show(owner, "You must have a wad loaded and an object selected", "Error", MessageBoxIcon.Error);
                return;
            }

            // Choose behaviour
            if (wadObject is WadStatic)
            {
                using (var form = new FormStaticEditor(tool, deviceManager, wad, (WadStatic)wadObject))
                    if (form.ShowDialog(owner) != DialogResult.OK)
                        return;
                tool.WadChanged(tool.MainSelection.Value.WadArea);
            }
            else if (wadObject is WadMoveable)
            {
                // TODO Some kind of mesh tree, animation, ... editor maybe?
                DarkMessageBox.Show(owner, "Sorry moveable editing is not supported right now! :(", "Feature not supported", MessageBoxIcon.Information);
            }
            else if (wadObject is WadFixedSoundInfo)
            {
                WadFixedSoundInfo fixedSoundInfo = (WadFixedSoundInfo)wadObject;
                using (var form = new FormFixedSoundInfoEditor { SoundInfo = fixedSoundInfo.SoundInfo })
                {
                    if (form.ShowDialog(owner) != DialogResult.OK)
                        return;
                    fixedSoundInfo.SoundInfo = form.SoundInfo;
                }
                tool.WadChanged(tool.MainSelection.Value.WadArea);
            }
            else if (wadObject is WadSpriteSequence)
            {
                using (var form = new FormSpriteSequenceEditor(wad, (WadSpriteSequence)wadObject))
                    if (form.ShowDialog(owner) != DialogResult.OK)
                        return;
                tool.WadChanged(tool.MainSelection.Value.WadArea);
            }
        }

        public static void DeleteObjects(WadToolClass tool, IWin32Window owner, WadArea wadArea, List<IWadObjectId> ObjectIdsToDelete)
        {
            if (ObjectIdsToDelete.Count == 0)
                return;
            if (DarkMessageBox.Show(owner, "You are about to delete " + ObjectIdsToDelete.Count + " objects. Are you sure?",
                "A question just in case...", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                return;

            Wad2 wad = tool.GetWad(wadArea);
            foreach (IWadObjectId id in ObjectIdsToDelete)
                wad.Remove(id);
            tool.WadChanged(wadArea);
        }

        public static void CreateObject(WadToolClass tool, IWin32Window owner, IWadObject initialWadObject)
        {
            Wad2 destinationWad = tool.DestinationWad;
            if (destinationWad == null)
            {
                DarkMessageBox.Show(owner, "You must have a destination wad open.", "Error", MessageBoxIcon.Error);
                return;
            }

            using (var form = new FormSelectSlot(initialWadObject.Id, destinationWad.SuggestedGameVersion))
            {
                if (form.ShowDialog(owner) != DialogResult.OK)
                    return;
                if (destinationWad.Contains(form.NewId))
                {
                    DarkMessageBox.Show(owner, "The slot " + form.NewId.ToString(destinationWad.SuggestedGameVersion) + " is already occupied.", "Error", MessageBoxIcon.Error);
                    return;
                }
                destinationWad.Add(form.NewId, initialWadObject);
            }

            tool.DestinationWadChanged();
        }

        public static void ShowSoundOverview(WadToolClass tool, IWin32Window owner, WadArea wadArea)
        {
            Wad2 wad = tool.GetWad(wadArea);
            if (wad == null)
            {
                DarkMessageBox.Show(owner, "You must have a " + wadArea.ToString().ToLower() + " wad loaded.", "Error", MessageBoxIcon.Error);
                return;
            }

            using (FormSoundOverview form = new FormSoundOverview(wad))
                if (form.ShowDialog(owner) != DialogResult.OK)
                    return;
            tool.WadChanged(wadArea);
        }
    }
}
