﻿using DarkUI.Forms;
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
using System.Xml;
using System.Xml.Serialization;
using System.Numerics;
using TombLib.LevelData;
using TombLib.LevelData.IO;
using TombLib.GeometryIO.Importers;
using TombLib.Wad.Catalog;

namespace WadTool
{
    public static class WadActions
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        public static void LoadWad(WadToolClass tool, IWin32Window owner, bool destination, string fileName)
        {
            bool isWad2 = Path.GetExtension(fileName).ToLower() == ".wad2";

            // Load the WAD/Wad2
            Wad2 newWad = null;
            try
            {
                newWad = Wad2.ImportFromFile(fileName, true, new GraphicalDialogHandler(owner), tool.Configuration.Tool_AllowTRNGDecryption);
                if (isWad2 && newWad.SoundSystem == SoundSystem.Dynamic)
                {
                    if (DarkMessageBox.Show(owner, "This Wad2 is using the old dynamic sound system and needs to be converted " +
                                            "to the new Xml sound system. A backup copy will be created under the same directory. " +
                                            "Do you want to continue?",
                                            "Convert Wad2", MessageBoxButtons.YesNo,
                                            MessageBoxIcon.Question) != DialogResult.Yes)
                        return;

                    File.Copy(fileName, fileName + ".bak", true);
                    if (!FileFormatConversions.ConvertWad2ToNewSoundFormat(fileName, fileName))
                    {
                        tool.SendMessage("Converting the file failed!", PopupType.Error);
                        return;
                    }

                    if (newWad.HasUnknownData)
                        tool.SendMessage("Loaded wad2 is of newer version.\nSome data was lost. Don't save this wad2 and use newest version of Wad Tool.", PopupType.Warning);

                    newWad = Wad2.ImportFromFile(fileName, true, new GraphicalDialogHandler(owner), tool.Configuration.Tool_AllowTRNGDecryption);
                }
            }
            catch (OperationCanceledException)
            {
                return;
            }
            catch (Exception exc)
            {
                logger.Info(exc, "Unable to load " + (destination ? "destination" : "source") + " file from '" + fileName + "'.");
                tool.SendMessage("Loading the file failed! \n" + exc.Message, PopupType.Error);
                return;
            }

            // Set wad
            if (destination)
            {
                tool.DestinationWad = newWad;
                tool.ToggleUnsavedChanges(false);
            }
            else
            {
                if (!isWad2) newWad.FileName = fileName;  // Keep original path only for source old wad, as it's not saveable
                tool.SourceWad = newWad;
            }
        }

        public static void LoadWadOpenFileDialog(WadToolClass tool, IWin32Window owner, bool destination)
        {
            // Open the file dialog
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
                LoadWad(tool, owner, destination, dialog.FileName);
            }
        }

        public static void SaveWad(WadToolClass tool, IWin32Window owner, Wad2 wadToSave, bool ask)
        {
            if (wadToSave == null)
            {
                tool.SendMessage("You have no wad opened. Nothing to save.", PopupType.Warning);
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


            // Ask about it
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
                // XML_SOUND_SYSTEM
                Wad2Writer.SaveToFile(wadToSave, outPath);

                // Immediately reload new wad, if it wasn't saved before (new or imported)
                if(wadToSave.FileName == null)
                    LoadWad(tool, owner, true, outPath);

                // Update last actual filename and call global event to update UI etc
                wadToSave.FileName = outPath;
                tool.ToggleUnsavedChanges(false);
            }
            catch (Exception exc)
            {
                logger.Warn(exc, "Unable to save to '" + outPath + "'");
                tool.SendMessage("Unable to save to '" + outPath + "'.   " + exc, PopupType.Error);
            }
        }

        public static void CreateNewWad(WadToolClass tool, IWin32Window owner)
        {
            using (var form = new FormNewWad2())
            {
                if (form.ShowDialog(owner) == DialogResult.Cancel)
                    return;

                tool.DestinationWad = new Wad2 { GameVersion = form.Version, SoundSystem = SoundSystem.Xml };
                tool.ToggleUnsavedChanges(false);
            }
        }

        public static bool LoadReferenceLevel(WadToolClass tool, IWin32Window owner, string path = null)
        {
            if (string.IsNullOrEmpty(path))
                path = LevelFileDialog.BrowseFile(owner, null, null, "Open Tomb Editor reference level", LevelSettings.FileFormatsLevel, null, false);
           
            if (string.IsNullOrEmpty(path))
                return false;

            tool.ReferenceLevel = Prj2Loader.LoadFromPrj2(path, null, new Prj2Loader.Settings { IgnoreTextures = true, IgnoreWads = true });

            if (tool.ReferenceLevel != null)
            {
                tool.Configuration.Tool_ReferenceProject = path;
                return true;
            }
            else
                return false;

        }

        public static void UnloadReferenceLevel(WadToolClass tool)
        {
            tool.ReferenceLevel = null;
            tool.Configuration.Tool_ReferenceProject = string.Empty;
        }

        public static IWadObjectId ChangeSlot(WadToolClass tool, IWin32Window owner)
        {
            if (tool.MainSelection?.WadArea == WadArea.Source)
                return null;

            Wad2 wad = tool.GetWad(tool.MainSelection?.WadArea);
            IWadObject wadObject = wad?.TryGet(tool.MainSelection?.Id);
            if (wad == null || wadObject == null)
            {
                tool.SendMessage("You must have an object selected", PopupType.Error);
                return null;
            }

            // Ask for the new slot
            using (var form = new FormSelectSlot(tool.DestinationWad, wadObject.Id))
            {
                if (form.ShowDialog(owner) != DialogResult.OK)
                    return null;
                if (form.NewId == wadObject.Id)
                    return null;
                if (wad.Contains(form.NewId))
                {
                    tool.SendMessage("The slot " + form.NewId.ToString(wad.GameVersion) + " is already occupied.", PopupType.Error);
                    return null;
                }
                wad.AssignNewId(wadObject.Id, form.NewId);
            }
            tool.WadChanged(tool.MainSelection.Value.WadArea);
            return wadObject.Id;
        }

        public static void ExportMesh(WadToolClass tool, IWin32Window owner, WadMesh m)
        {
            // Step 1: collect all textures
            /*var texturePieces = new Dictionary<Hash, WadTexture>();
            for (int i = 0; i < m.Polys.Count; i++)
            {
                var poly = m.Polys[i];

                // Create the new tile and compute hash
                var textureQuad = poly.Texture.GetRect(poly.Shape == WadPolygonShape.Triangle);
                var image = ImageC.CreateNew((int)textureQuad.Width, (int)textureQuad.Height);
                image.CopyFrom(0, 0, poly.Texture.Texture.Image, (int)textureQuad.X0, (int)textureQuad.Y0,
                    (int)(textureQuad.X0 + textureQuad.X1), (int)(textureQuad.Y0 + textureQuad.Y1));
                WadTexture text = new WadTexture(image);

                // Store the hash for the next steps
                poly.TextureHash = text.Hash;

                //Add uniquely the texture to the dictionary
                if (!texturePieces.ContainsKey(text.Hash))
                    texturePieces.Add(text.Hash, text);
            }

            // Step 2: collect textures in 256x256 pages
            int processedTextures = 0;
            var lastTexture = ImageC.CreateNew(256, 256);
            var packer = new RectPackerSimpleStack(new VectorInt2(256, 256));
            while (processedTextures < texturePieces.Count)
            {
                var texture = texturePieces.ElementAt(processedTextures).Value;
                var result = packer.TryAdd(new VectorInt2(texture.Image.Width, texture.Image.Height));
                if (!result.HasValue)
                {

                }
                processedTextures++;
            }

            var matOpaque = new IOMaterial(Material.Material_Opaque + "_" + j + "_" + i,
                                                                   texture,
                                                                   fileName,
                                                                   i,
                                                                   false,
                                                                   false,
                                                                   0);

            var matOpaqueDoubleSided = new IOMaterial(Material.Material_OpaqueDoubleSided + "_" + j + "_" + i,
                                                      texture,
                                                      fileName,
                                                      i,
                                                      false,
                                                      true,
                                                      0);

            var matAdditiveBlending = new IOMaterial(Material.Material_AdditiveBlending + "_" + j + "_" + i,
                                                     texture,
                                                     fileName,
                                                     i,
                                                     true,
                                                     false,
                                                     0);

            var matAdditiveBlendingDoubleSided = new IOMaterial(Material.Material_AdditiveBlendingDoubleSided + "_" + j + "_" + i,
                                                                texture,
                                                                fileName,
                                                                i,
                                                                true,
                                                                true,
                                                                0);

            var mesh = new IOMesh(m.Name);

            foreach (var poly in m.Polys)
            {

            }

            m.Polys[0].Texture.*/

            return;
        }

        public static void ImportModelAsStaticMesh(WadToolClass tool, IWin32Window owner)
        {
            if (tool.DestinationWad == null)
            {
                tool.SendMessage("You must have a wad opened", PopupType.Error);
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
                    form.AddPreset(IOSettingsPresets.GeometryImportSettingsPresets);
                    if (form.ShowDialog(owner) != DialogResult.OK)
                        return;

                    var @static = new WadStatic(tool.DestinationWad.GetFirstFreeStaticMesh());
                    var mesh = WadMesh.ImportFromExternalModel(dialog.FileName, form.Settings);
                    if (mesh == null)
                    {

                        tool.SendMessage("Error while loading the 3D model. Please check that the file " +
                                            "is one of the supported formats and that the meshes are textured", PopupType.Error);
                        return;
                    }
                    @static.Mesh = mesh;
                    @static.VisibilityBox = @static.Mesh.BoundingBox;
                    @static.CollisionBox = @static.Mesh.BoundingBox;
                    tool.DestinationWad.Statics.Add(@static.Id, @static);
                    tool.WadChanged(WadArea.Destination);
                }
            }
        }

        public static Wad2 ConvertWad2ToTR5Main(WadToolClass tool, IWin32Window owner, Wad2 src)
        {
            Wad2 dest = new Wad2();
            dest.GameVersion = TRVersion.Game.TR5Main;
            dest.SoundSystem = SoundSystem.Xml;

            foreach (var moveable in src.Moveables)
            {
                var compatibleSlot = TrCatalog.GetMoveableTR5MainSlot(src.GameVersion, moveable.Key.TypeId);
                if (compatibleSlot == "")
                    continue;

                bool isMoveable = false;
                var destId = TrCatalog.GetItemIndex(TRVersion.Game.TR5Main, compatibleSlot, out isMoveable);
                if (!destId.HasValue)
                    continue;

                var newId = new WadMoveableId(destId.Value);

                foreach (var animation in moveable.Value.Animations)
                    foreach (var command in animation.AnimCommands)
                        if (command.Type == WadAnimCommandType.PlaySound)
                        {
                            int id = command.Parameter2 & 0x3FFF;
                            id += TrCatalog.GetTR5MainSoundMapStart(src.GameVersion);
                            command.Parameter2 = (short)((command.Parameter2 & 0xC000) | (id & 0x3FFF));
                        }

                dest.Add(newId, moveable.Value);
            }

            foreach (var sequence in src.SpriteSequences)
            {
                var compatibleSlot = TrCatalog.GetSpriteSequenceTR5MainSlot(src.GameVersion, sequence.Key.TypeId);
                if (compatibleSlot == "")
                    continue;

                bool isMoveable = false;
                var destId = TrCatalog.GetItemIndex(TRVersion.Game.TR5Main, compatibleSlot, out isMoveable);
                if (!destId.HasValue)
                    continue;

                var newId = new WadSpriteSequenceId(destId.Value);

                dest.Add(newId, sequence.Value);
            }

            foreach (var staticObject in src.Statics)
            {
                dest.Add(staticObject.Key, staticObject.Value);
            }

            return dest;
        }

        public static List<IWadObjectId> CopyObject(WadToolClass tool, IWin32Window owner, List<IWadObjectId> objectIdsToMove, bool alwaysChooseId)
        {
            Wad2 sourceWad = tool.SourceWad;
            Wad2 destinationWad = tool.DestinationWad;

            if (destinationWad == null || sourceWad == null || objectIdsToMove.Count == 0)
            {
                tool.SendMessage("You must have two wads loaded and at least one source object selected.", PopupType.Error);
                return null;
            }

            var listInProgress = new List<uint>();

            // Figure out the new ids if there are any id collisions
            IWadObjectId[] newIds = objectIdsToMove.ToArray();

            // If destination is TR5Main, try to remap object IDs
            if (destinationWad.GameVersion == TRVersion.Game.TR5Main)
            {
                for (int i = 0; i < objectIdsToMove.Count; ++i)
                {
                    var objectId = objectIdsToMove[i];
                    if (objectId is WadMoveableId)
                    {
                        var moveableId = (WadMoveableId)objectId;

                        // Try to get a compatible slot
                        var newSlot = TrCatalog.GetMoveableTR5MainSlot(sourceWad.GameVersion, moveableId.TypeId);
                        if (newSlot == "")
                            continue;

                        // Get the new ID
                        bool isMoveable;
                        var newId = TrCatalog.GetItemIndex(destinationWad.GameVersion, newSlot, out isMoveable);
                        if (!newId.HasValue)
                            continue;

                        // Save the new ID
                        newIds[i] = new WadMoveableId(newId.Value);
                    }
                }
            }

            for (int i = 0; i < objectIdsToMove.Count; ++i)
            {
                if (!sourceWad.Contains(objectIdsToMove[i]))
                    continue;
                if (!alwaysChooseId)
                {
                    if (!destinationWad.Contains(newIds[i]))
                        if (!newIds.Take(i).Contains(newIds[i])) // There also must not be collisions with the other custom assigned ids.
                            continue;
                }

                bool askConfirm = !alwaysChooseId;

                // Ask for the new slot
                do
                {
                    DialogResult dialogResult;
                    if (askConfirm)
                    {
                        dialogResult = DarkMessageBox.Show(owner, "The id " + newIds[i].ToString(destinationWad.GameVersion) + " is already occupied in the destination wad.\n" +
                                                         "Do you want to replace it (Yes) or to select another Id (No)?",
                                                         "Occupied slot", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
                    }
                    else
                    {
                        dialogResult = DialogResult.No;
                    }

                    // From this time, always ask for confirm
                    askConfirm = true;

                    if (dialogResult == DialogResult.Cancel)
                        return null;
                    else if (dialogResult == DialogResult.No)
                    {
                        using (var form = new FormSelectSlot(destinationWad, newIds[i], listInProgress))
                        {
                            if (form.ShowDialog(owner) != DialogResult.OK)
                                return null;
                            if (destinationWad.Contains(form.NewId) || newIds.Take(i).Contains(form.NewId))
                            {
                                destinationWad.Remove(form.NewId);
                                tool.WadChanged(WadArea.Destination);
                            }
                            newIds[i] = form.NewId;

                            if (form.NewId is WadStaticId) listInProgress.Add(((WadStaticId)form.NewId).TypeId);
                            if (form.NewId is WadMoveableId) listInProgress.Add(((WadMoveableId)form.NewId).TypeId);
                            if (form.NewId is WadSpriteSequenceId) listInProgress.Add(((WadSpriteSequenceId)form.NewId).TypeId);

                            break;
                        }
                    }
                    else
                    {
                        destinationWad.Remove(objectIdsToMove[i]);
                        tool.WadChanged(WadArea.Destination);
                        break;
                    }
                } while (destinationWad.Contains(newIds[i]) || newIds.Take(i).Contains(newIds[i])); // There also must not be collisions with the other custom assigned ids.
            }

            // HACK: Until this is fixed... https://github.com/MontyTRC89/Tomb-Editor/issues/413
            // We just block copying of same object to another slot and warn user it's in another slot.
            var failedIdList = new List<IWadObjectId>();

            // Move objects
            for (int i = 0; i < objectIdsToMove.Count; ++i)
            {
                IWadObject obj = sourceWad.TryGet(objectIdsToMove[i]);
                if (obj == null)
                    continue;

                if (destinationWad.Contains(obj)) // Aforementioned HACK continues here - just add object which failed to copy to failed list
                    failedIdList.Add(newIds[i]);
                else
                {
                    destinationWad.Add(newIds[i], obj);

                    if (destinationWad.GameVersion == TRVersion.Game.TR5Main && obj is WadMoveable)
                    {
                        var moveable = obj as WadMoveable;
                        foreach (var animation in moveable.Animations)
                            foreach (var command in animation.AnimCommands)
                                if (command.Type == WadAnimCommandType.PlaySound)
                                {
                                    int id = command.Parameter2 & 0x3FFF;
                                    id += TrCatalog.GetTR5MainSoundMapStart(sourceWad.GameVersion);
                                    command.Parameter2 = (short)((command.Parameter2 & 0xC000) | (id & 0x3FFF));
                                }
                    }
                }
            }

            // Aforementioned HACK continues here - count amount of actually copied objects
            int actualAmountOfCopiedObjects = objectIdsToMove.Count - failedIdList.Count;
            if (actualAmountOfCopiedObjects > 0)
            {
                // Update the situation
                tool.WadChanged(WadArea.Destination);

                string infoString = (objectIdsToMove.Count == 1 ? "Object" : "Objects") + " successfully copied.";

                // Aforementioned HACK continues here - additionally inform user that some objects are in different slots.
                if (failedIdList.Count > 0)
                    infoString += "\n" + failedIdList.Count + " object" +
                        (failedIdList.Count > 1 ? "s" : "") + " weren't copied because " +
                        (failedIdList.Count > 1 ? "they are" : "it's") + " already in different slot" +
                        (failedIdList.Count > 1 ? "s." : ".");

                // Indicate that object is copied
                tool.SendMessage(infoString, PopupType.Info);
            }

            return newIds.Where(item => !failedIdList.Any(failed => failed == item)).ToList();
        }

        public static void EditObject(WadToolClass tool, IWin32Window owner, DeviceManager deviceManager)
        {
            Wad2 wad = tool.GetWad(tool.MainSelection?.WadArea);
            IWadObject wadObject = wad?.TryGet(tool.MainSelection?.Id);

            if (wadObject == null || tool.MainSelection?.WadArea == WadArea.Source)
                return;

            if (wad == null)
            {
                tool.SendMessage("You must have a wad loaded and an object selected.", PopupType.Error);
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
                using (var form = new FormAnimationEditor(tool, deviceManager, wad, ((WadMoveable)wadObject).Id))
                {
                    if (form.ShowDialog(owner) != DialogResult.OK)
                        return;
                    tool.WadChanged(tool.MainSelection.Value.WadArea);
                }
            }
            else if (wadObject is WadSpriteSequence)
            {
                using (var form = new FormSpriteSequenceEditor(tool, wad, (WadSpriteSequence)wadObject))
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

        public static IWadObjectId CreateObject(WadToolClass tool, IWin32Window owner, IWadObject initialWadObject)
        {
            Wad2 destinationWad = tool.DestinationWad;
            if (destinationWad == null)
            {
                tool.SendMessage("You must have a destination wad opened.", PopupType.Error);
                return null;
            }

            IWadObjectId result;

            using (var form = new FormSelectSlot(tool.DestinationWad, initialWadObject.Id))
            {
                if (form.ShowDialog(owner) != DialogResult.OK)
                    return null;
                if (destinationWad.Contains(form.NewId))
                {
                    tool.SendMessage("The slot " + form.NewId.ToString(destinationWad.GameVersion) + " is already occupied.", PopupType.Error);
                    return null;
                }

                if (initialWadObject is WadMoveable)
                {
                    var moveable = initialWadObject as WadMoveable;
                    var bone = new WadBone();
                    bone.Name = "Root";
                    bone.Mesh = CreateFakeMesh("Root");
                    moveable.Bones.Add(bone);
                }

                destinationWad.Add(form.NewId, initialWadObject);
                result = form.NewId;
            }

            tool.WadChanged(WadArea.Destination);
            tool.ToggleUnsavedChanges();

            return result;
        }

        public static WadMesh CreateFakeMesh(string name)
        {
            var mesh = new WadMesh();

            mesh.Name = name;

            mesh.VerticesPositions.Add(new Vector3(-1, 1, 1)); // 0
            mesh.VerticesPositions.Add(new Vector3(1, 1, 1)); // 1
            mesh.VerticesPositions.Add(new Vector3(1, 1, -1)); // 2
            mesh.VerticesPositions.Add(new Vector3(-1, 1, -1)); // 3
            mesh.VerticesPositions.Add(new Vector3(-1, -1, 1)); // 4
            mesh.VerticesPositions.Add(new Vector3(1, -1, 1)); // 5
            mesh.VerticesPositions.Add(new Vector3(1, -1, -1)); // 6
            mesh.VerticesPositions.Add(new Vector3(-1, -1, -1)); // 7

            for (int i = 0; i < 8; i++)
                mesh.VerticesNormals.Add(Vector3.Zero);

            var texture = new TextureArea();
            var image = ImageC.Magenta;
            texture.Texture = new WadTexture(image);
            texture.TexCoord0 = new Vector2(0, 0);
            texture.TexCoord1 = new Vector2(1, 0);
            texture.TexCoord2 = new Vector2(1, 1);
            texture.TexCoord3 = new Vector2(0, 1);

            mesh.Polys.Add(new WadPolygon
            {
                Index0 = 0,
                Index1 = 1,
                Index2 = 2,
                Index3 = 3,
                Shape = WadPolygonShape.Quad,
                Texture = texture
            });

            mesh.Polys.Add(new WadPolygon
            {
                Index0 = 2,
                Index1 = 1,
                Index2 = 5,
                Index3 = 6,
                Shape = WadPolygonShape.Quad,
                Texture = texture
            });

            mesh.Polys.Add(new WadPolygon
            {
                Index0 = 1,
                Index1 = 0,
                Index2 = 4,
                Index3 = 5,
                Shape = WadPolygonShape.Quad,
                Texture = texture
            });

            mesh.Polys.Add(new WadPolygon
            {
                Index0 = 0,
                Index1 = 3,
                Index2 = 7,
                Index3 = 4,
                Shape = WadPolygonShape.Quad,
                Texture = texture
            });

            mesh.Polys.Add(new WadPolygon
            {
                Index0 = 3,
                Index1 = 2,
                Index2 = 6,
                Index3 = 7,
                Shape = WadPolygonShape.Quad,
                Texture = texture
            });

            mesh.Polys.Add(new WadPolygon
            {
                Index0 = 7,
                Index1 = 6,
                Index2 = 5,
                Index3 = 4,
                Shape = WadPolygonShape.Quad,
                Texture = texture
            });

            return mesh;
        }

        public static bool ExportAnimationToXml(WadMoveable moveable, WadAnimation animation, string fileName)
        {
            try
            {
                if (File.Exists(fileName))
                    File.Delete(fileName);

                // Serialize the animation to XML
                var xmlSerializer = new XmlSerializer(typeof(WadAnimation));
                var xml = "";

                using (var sww = new StringWriter())
                {
                    using (var tw = new XmlTextWriter(sww))
                    {
                        tw.Formatting = Formatting.Indented;
                        tw.Indentation = 4;

                        using (var writer = XmlWriter.Create(tw))
                        {
                            xmlSerializer.Serialize(writer, animation);
                            xml = sww.ToString();
                        }
                    }
                }

                // Write XML to file
                using (var writer = new StreamWriter(File.OpenWrite(fileName)))
                    writer.Write(xml);

                return true;
            }
            catch (Exception exc)
            {
                logger.Warn(exc, "'ExportAnimationToXml' failed.");
                return false;
            }
        }

        public static WadAnimation ImportAnimationFromXml(WadToolClass tool, string fileName)
        {
            try
            {
                // Read animation from XML
                XmlSerializer deserializer = new XmlSerializer(typeof(WadAnimation));
                TextReader reader = new StreamReader(fileName);
                object obj = deserializer.Deserialize(reader);
                WadAnimation animation = (WadAnimation)obj;
                reader.Close();

                return animation;
            }
            catch (Exception exc)
            {
                tool.SendMessage("Unknown error while importing animation! Possible reason: not valid XML format.", PopupType.Error);
                logger.Warn(exc, "'ImportAnimationFromXml' failed.");
                return null;
            }
        }


        public static WadAnimation ImportAnimationFromModel(WadToolClass tool, IWin32Window owner, int nodeCount, string fileName)
        {
            IOModel tmpModel = null;

            // Import the model
            try
            {
                var settings = new IOGeometrySettings() { ProcessAnimations = true, ProcessGeometry = false };
                using (var form = new GeometryIOSettingsDialog(settings))
                {
                    form.AddPreset(IOSettingsPresets.AnimationSettingsPresets);
                    string resultingExtension = Path.GetExtension(fileName).ToLowerInvariant();

                    if (resultingExtension.Equals(".fbx"))
                        form.SelectPreset("3dsmax Filmbox (FBX)");
                    else if (resultingExtension.Equals(".dae"))
                        form.SelectPreset("3dsmax COLLADA");

                    if (form.ShowDialog(owner) != DialogResult.OK)
                        return null;

                    var importer = BaseGeometryImporter.CreateForFile(fileName, settings, null);
                    tmpModel = importer.ImportFromFile(fileName);

                    // We don't support animation importing from custom-written mqo importer yet...
                    if (importer is MetasequoiaImporter)
                    {
                        tool.SendMessage("Metasequoia importer isn't currently supported.", PopupType.Error);
                        return null;
                    }

                    // If no animations, return null
                    if (tmpModel.Animations.Count == 0)
                    {
                        tool.SendMessage("Selected file has no supported animations!", PopupType.Error);
                        return null;
                    }
                }
            }
            catch (Exception ex)
            {
                tool.SendMessage("Unknown error while importing animation. \n" + ex?.Message, PopupType.Error);
                logger.Warn(ex, "'ImportAnimationFromModel' failed.");
                return null;
            }

            IOAnimation animToImport;

            if (tmpModel.Animations.Count > 1)
                using (var dialog = new AnimationImportDialog(tmpModel.Animations.Select(o => o.Name).ToList()))
                {
                    dialog.ShowDialog(owner);
                    if (dialog.DialogResult == DialogResult.Cancel)
                        return null;
                    else
                        animToImport = tmpModel.Animations[dialog.AnimationToImport];
                }
            else
                animToImport = tmpModel.Animations[0];


            // Integrity check, for cases when something totally went wrong with assimp
            if (animToImport == null)
            {
                tool.SendMessage("Animation importer encountered serious error. No animation imported.", PopupType.Error);
                return null;
            }

            // Integrity check, is there any valid frames?
            if (animToImport.Frames.Count <= 0)
            {
                tool.SendMessage("Selected animation has no frames!", PopupType.Error);
                return null;
            }

            // Integrity check, number of bones = number of nodes?
            if (animToImport.NumNodes != nodeCount)
            {
                tool.SendMessage("Selected animation has different number of bones!", PopupType.Error);
                return null;
            }

            WadAnimation animation = new WadAnimation();
            animation.Name = animToImport.Name;

            foreach (var frame in animToImport.Frames)
            {
                var keyFrame = new WadKeyFrame();
                keyFrame.Offset = frame.Offset;
                frame.Angles.ForEach(angle => keyFrame.Angles.Add(new WadKeyFrameRotation() { Rotations = angle }));

                animation.KeyFrames.Add(keyFrame);
            }

            animation.EndFrame = (ushort)(animToImport.Frames.Count - 1);

            return animation;
        }

        public static void EditSkeletion(WadToolClass tool, IWin32Window owner)
        {
            if (tool.MainSelection?.WadArea == WadArea.Source)
                return;

            var wad = tool.GetWad(tool.MainSelection.Value.WadArea);
            var moveableId = (WadMoveableId)tool.MainSelection.Value.Id;
            using (var form = new FormSkeletonEditor(tool, DeviceManager.DefaultDeviceManager, wad, moveableId))
            {
                if (form.ShowDialog(owner) != DialogResult.OK)
                    return;
                tool.WadChanged(WadArea.Destination);
            }
        }
    }
}
