using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http.Headers;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using TombLib.LevelData;
using TombLib.LevelData.IO;
using TombLib.Utils;
using TombLib.Wad;
using TombLib.Wad.Catalog;

namespace ProjectConverter
{
    class Program
    {
        static bool ConvertProject(string source)
        {
            try
            {
                // Load new TombEngine reference Wad2
                var referenceWad = Wad2Loader.LoadFromFile("TombEngine.wad2", true);

                // Load level and all related resources
                var level = Prj2Loader.LoadFromPrj2(source, null);
                if (level == null)
                {
                    Console.WriteLine("Error while loading level");
                    return false;
                }

                // Now convert resources to new format
                var newWads = new List<ReferencedWad>();
                Dictionary<uint, uint> remappedSlots = new Dictionary<uint, uint>();
 
                string newFileName;
                string newPath;
                bool addedTimex = false;

                foreach (var wadRef in level.Settings.Wads)
                {
                    var wad = wadRef.Wad;
                    var newWad = new Wad2
                    {
                        GameVersion = TRVersion.Game.TombEngine,
                        SoundSystem = SoundSystem.Xml
                    };

                    uint animating = 1231;

                    // Copy all objects to new wad
                    foreach (var moveable in wad.Moveables)
                    {
                        var newId = TrCatalog.GetMoveableTombEngineSlot(TRVersion.Game.TR4, moveable.Key.TypeId);
                         uint newSlot;
                        if (newId == "")
                        {
                            newSlot = animating;
                            Console.ForegroundColor = ConsoleColor.Yellow;
                            Console.WriteLine("    Slot " + TrCatalog.GetMoveableName(TRVersion.Game.TR4, moveable.Key.TypeId) +
                                              " is not supported by TombEngine and it will be remapped to ANIMATING" + (animating - 1231 + 32));
                            Console.ResetColor();

                            animating++;
                        }
                        else
                        {
                            bool isMoveable;
                            var found = TrCatalog.GetItemIndex(TRVersion.Game.TombEngine, newId, out isMoveable);
                            if (!found.HasValue)
                            {
                                continue;
                            }
                            else
                                newSlot = found.Value;
                        }

                        string newSlotName = TrCatalog.GetMoveableName(TRVersion.Game.TombEngine, newSlot);

                        // We need to copy mesh 14 to 7 for {WEAPON}_ANIM for back weapons
                        if (newSlotName == "SHOTGUN_ANIM" || newSlotName == "CROSSBOW_ANIM" || newSlotName == "HK_ANIM" ||
                            newSlotName == "HARPOON_ANIM" || newSlotName == "GRENADE_ANIM" || newSlotName == "ROCKET_ANIM")
                        {
                            Console.ForegroundColor = ConsoleColor.Yellow;
                            Console.WriteLine("    Copying mesh #14 to mesh #7 for " + newSlotName);
                            Console.ResetColor();

                            var mesh = moveable.Value.Bones[14].Mesh.Clone(); ;
                            for (int i = 0; i < mesh.VertexPositions.Count; i++)
                            {
                                var pos = mesh.VertexPositions[i];
                                pos.Y += 256;
                                mesh.VertexPositions[i] = pos;
                            }
                            moveable.Value.Bones[7].Mesh = mesh;
                            moveable.Value.Meshes[7] = mesh;
                        }

                        // For holsters, we need to put holsters meshes in 1 and 4 and apply the same skeleton from LARA
                        if (newSlotName == "LARA_HOLSTERS" || newSlotName == "LARA_HOLSTERS_PISTOLS" ||
                            newSlotName == "LARA_HOLSTERS_UZIS" || newSlotName == "LARA_HOLSTERS_REVOLVER")
                        {
                            Console.ForegroundColor = ConsoleColor.Yellow;
                            Console.WriteLine("    Copying holsters meshes for " + newSlotName);
                            Console.ResetColor();

                            var laraMoveable = referenceWad.Moveables[new WadMoveableId(0)];
                            var newBones = new List<WadBone>();
                            var newMeshes = new List<WadMesh>();

                            foreach (var oldBone in laraMoveable.Bones)
                                newBones.Add(oldBone.Clone());

                            foreach (var oldMesh in laraMoveable.Meshes)
                                newMeshes.Add(oldMesh.Clone());

                            newBones[1].Mesh = moveable.Value.Bones[4].Mesh.Clone();
                            newMeshes[1] = newBones[1].Mesh;

                            newBones[4].Mesh = moveable.Value.Bones[8].Mesh.Clone();
                            newMeshes[4] = newBones[4].Mesh;

                            moveable.Value.Bones.Clear();
                            moveable.Value.Bones.AddRange(newBones);
                            moveable.Value.Meshes.Clear();
                            moveable.Value.Meshes.AddRange(newMeshes);
                        }

                        if (!addedTimex && 
                            (newSlotName== "MEMCARD_LOAD_INV_ITEM" || newSlotName== "MEMCARD_SAVE_INV_ITEM" ||
                            newSlotName == "PC_LOAD_INV_ITEM" || newSlotName == "PC_SAVE_INV_ITEM"))
                        {
                            Console.ForegroundColor = ConsoleColor.Yellow;
                            Console.WriteLine("    Adding TIMEX from reference Wad2");
                            Console.ResetColor();

                            newWad.Add(new WadMoveableId(987), referenceWad.Moveables[new WadMoveableId(987)]);
                            addedTimex = true;
                        }

                        if (!addedTimex && newSlotName == "TIMEX_ITEM")
                        {
                            Console.WriteLine(newId + " => [" + newSlot + "] " + newSlotName);
                            newWad.Add(new WadMoveableId(newSlot), moveable.Value);
                            addedTimex = true;
                        }
                        else if (newSlotName == "LARA")
                        {
                            Console.WriteLine(newId + " => [" + newSlot + "] " + newSlotName);
                            newWad.Add(new WadMoveableId(0), referenceWad.Moveables[new WadMoveableId(0)]);
                        }
                        else
                        {
                            Console.WriteLine(newId + " => [" + newSlot + "] " + newSlotName);
                            newWad.Add(new WadMoveableId(newSlot), moveable.Value);
                        }

                        if (!remappedSlots.ContainsKey(moveable.Key.TypeId))
                            remappedSlots.Add(moveable.Key.TypeId, newSlot);
                    }

                    // Copy all statics
                    foreach (var staticModel in wad.Statics)
                        newWad.Statics.Add(staticModel.Key, staticModel.Value);

                    // Copy all sprite sequences
                    foreach (var sequence in wad.SpriteSequences)
                    {
                        var newId = TrCatalog.GetMoveableTombEngineSlot(TRVersion.Game.TR4, sequence.Key.TypeId);
                        uint newSlot;
                        if (newId == "")
                        {
                            newSlot = animating;
                            animating++;
                        }
                        else
                        {
                            bool isMoveable;
                            var found = TrCatalog.GetItemIndex(TRVersion.Game.TombEngine, newId, out isMoveable);
                            if (!found.HasValue)
                            {
                                continue;
                            }
                            else
                                newSlot = found.Value;
                        }

                        string newSlotName = TrCatalog.GetSpriteSequenceName(TRVersion.Game.TombEngine, newSlot);
                        
                        Console.WriteLine(newId + ": => [" + newSlot + "] " + newSlotName);

                        if (newSlotName == "DEFAULT_SPRITES")
                        {
                            Console.ForegroundColor = ConsoleColor.Yellow;
                            Console.WriteLine("    DEFAULT_SPRITES found, adding new sprites slots from a reference Wad2");
                            Console.ResetColor();

                            // Open TombEngine sprites Wad2
                            foreach (var spr in referenceWad.SpriteSequences)
                            {
                                if (!newWad.Contains(spr.Key))
                                {
                                    Console.ForegroundColor = ConsoleColor.Yellow;
                                    Console.WriteLine("    Adding " + TrCatalog.GetSpriteSequenceName(TRVersion.Game.TombEngine, spr.Key.TypeId));
                                    Console.ResetColor();

                                    newWad.Add(spr.Key, spr.Value);
                                }
                            }
                        }
                        else
                        {
                            newWad.Add(new WadSpriteSequenceId(newSlot), sequence.Value);
                        }
                    }

                    // Copy all sounds
                    newWad.Sounds = wad.Sounds;

                    // Save the Wad2 
                    newFileName = Path.GetFileNameWithoutExtension(wad.FileName);
                    newPath = Path.Combine(
                        Path.GetDirectoryName(source),
                        Path.GetFileNameWithoutExtension(wadRef.Path) + "_TombEngine.wad2"
                        );


                    Console.WriteLine("Saving " + wadRef.Path + " to " + newPath);
                    Wad2Writer.SaveToFile(newWad, newPath);

                    newWads.Add(new ReferencedWad(
                        level.Settings,
                        level.Settings.MakeRelative(newPath, VariableType.LevelDirectory)
                        ));
                }

                level.Settings.Wads.Clear();
                level.Settings.Wads.AddRange(newWads);

                foreach (var room in level.Rooms)
                    if (room != null)
                        foreach (var instance in room.Objects)
                        {
                            if (instance is MoveableInstance)
                                if (!remappedSlots.ContainsKey(((MoveableInstance)instance).WadObjectId.TypeId))
                                    Console.WriteLine("Slot not found! " + ((MoveableInstance)instance).WadObjectId.TypeId);
                                else
                                    ((MoveableInstance)instance).WadObjectId = new WadMoveableId(remappedSlots[((MoveableInstance)instance).WadObjectId.TypeId]);
                        }
                level.Settings.GameVersion = TRVersion.Game.TombEngine;

                newPath = Path.Combine(
                    Path.GetDirectoryName(source), 
                    Path.GetFileNameWithoutExtension(source) + "_TombEngine.prj2");

                Prj2Writer.SaveToPrj2(newPath, level);


            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(ex.Message);
                Console.ResetColor();
                return false;
            }

            return true;
        }

        [STAThread]
        static void Main(string[] args)
        {
            Console.WriteLine("TombEngine Project Converter Beta");
            Console.WriteLine("Created by MontyTRC");
            Console.WriteLine("=================================");
            Console.WriteLine("");

            string fileName = "";
            if (args.Length >= 1)
            {
                fileName = args[0];
            }
            else
            {
                using (var dialog = new OpenFileDialog())
                {
                    dialog.Filter = "Tomb Editor Project (*.prj2)|*.prj2";
                    dialog.Title = "Select project to convert";
                    if (dialog.ShowDialog() == DialogResult.Cancel)
                        return;
                    fileName = dialog.FileName;
                }
            }

            if (!File.Exists(fileName))
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("The specified project doesn't exist");
                Console.ResetColor();
                Console.WriteLine("Press a key to exit");
                Console.ReadKey();
                return;
            }

            Console.WriteLine("This tool will convert your project to TombEngine format");
            Console.WriteLine("It will be saved in the same path with a different name");
            Console.WriteLine("Do you want to continue? [yn]");
            
            var key = Console.ReadKey();
            if (key.Key != ConsoleKey.Y)
            {
                return;
            }

            Console.WriteLine("");

            TrCatalog.LoadCatalog("Catalogs\\TRCatalog.xml");
            
            if (!ConvertProject(fileName))
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("There was an error while converting your project");
                Console.ResetColor();
                Console.WriteLine("Press a key to exit");
                Console.ReadKey();
                return;
            }

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("");
            Console.WriteLine("Your project was converted successfully!");
            Console.WriteLine("Some notes about the conversion:");
            Console.WriteLine("");
            Console.WriteLine("- Enemies now have their MESHSWAP slots (for example, BADDY2 -> MESHSWAP_BADDY2), please use WadTool for remapping manually these slots");
            Console.WriteLine("- A very few number of slots are not supported, and they are added as ANIMATING above 16");
            Console.WriteLine("- TombEngine uses different slots for every kind of sprite (fire, smoke...). They were already added from a reference Wad2");
            Console.WriteLine("- The soundmap works like WAD130, so you will not have troubles with sounds, but you could need to open level settings and adjust paths of SFX, XML and samples");
            Console.WriteLine("- NG triggers will not be compild as they are not supported by TombEngine");
            Console.WriteLine("- TombEngine requires our LARA object because we have fixed state changes and added new states for new moves, so please copy again your animations with WadTool");
            Console.WriteLine("- Weapon anims and holsters objects have changed a bit but the converter already converted them automatically to the new format");
            Console.WriteLine("");
            Console.ResetColor();

            Console.WriteLine("Press a key to exit");
            Console.ReadKey();
        }
    }
}
