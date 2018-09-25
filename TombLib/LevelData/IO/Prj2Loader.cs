using NLog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using TombLib.IO;
using TombLib.Utils;
using TombLib.Wad;
using TombLib.Wad.Catalog;

namespace TombLib.LevelData.IO
{
    public static class Prj2Loader
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        public class Settings
        {
            public bool IgnoreWads = false;
            public bool IgnoreTextures = false;
        }

        public static Level LoadFromPrj2(string filename, IProgressReporter progressReporter) => LoadFromPrj2(filename, progressReporter, new Settings());
        public static Level LoadFromPrj2(string filename, IProgressReporter progressReporter, Settings loadSettings)
        {
            using (var fileStream = new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.Read))
                return LoadFromPrj2(filename, fileStream, progressReporter, loadSettings);
        }
        public static Level LoadFromPrj2(string filename, Stream stream, IProgressReporter progressReporter) => LoadFromPrj2(filename, stream, progressReporter, new Settings());
        public static Level LoadFromPrj2(string filename, Stream stream, IProgressReporter progressReporter, Settings loadSettings)
        {
            using (var chunkIO = new ChunkReader(Prj2Chunks.MagicNumber, stream))
            {
                LevelSettingsIds levelSettingsIds = new LevelSettingsIds();
                Level level = new Level();
                Wad2 embeddedSoundInfoWad = null;
                chunkIO.ReadChunks((id, chunkSize) =>
                {
                    LevelSettings settings = null;
                    if (LoadLevelSettings(chunkIO, id, filename, ref levelSettingsIds, ref settings, loadSettings, progressReporter))
                    {
                        level.ApplyNewLevelSettings(settings);
                        return true;
                    }
                    else if (LoadEmbeddedSoundInfoWad(chunkIO, id, ref embeddedSoundInfoWad, progressReporter))
                        return true;
                    else if (LoadRooms(chunkIO, id, level, levelSettingsIds, embeddedSoundInfoWad, progressReporter))
                        return true;
                    return false;
                });

                return level;
            }
        }

        public struct LoadedObjects
        {
            public LevelSettings Settings;
            public List<ObjectInstance> Objects;
        }
        public static LoadedObjects LoadFromPrj2OnlyObjects(string filename, Stream stream) => LoadFromPrj2OnlyObjects(filename, stream, new Settings());
        public static LoadedObjects LoadFromPrj2OnlyObjects(string filename, Stream stream, Settings loadSettings)
        {
            using (var chunkIO = new ChunkReader(Prj2Chunks.MagicNumber, stream))
            {
                LevelSettingsIds levelSettingsIds = new LevelSettingsIds();
                LoadedObjects loadedObjects = new LoadedObjects { Settings = new LevelSettings(), Objects = new List<ObjectInstance>() };
                Dictionary<long, ObjectInstance> newObjects = new Dictionary<long, ObjectInstance>();
                Wad2 embeddedSoundInfoWad = null;
                chunkIO.ReadChunks((id, chunkSize) =>
                {
                    if (LoadLevelSettings(chunkIO, id, filename, ref levelSettingsIds, ref loadedObjects.Settings, loadSettings))
                        return true;
                    else if (LoadEmbeddedSoundInfoWad(chunkIO, id, ref embeddedSoundInfoWad))
                        return true;
                    else if (LoadObjects(chunkIO, id, levelSettingsIds, 
                        obj => loadedObjects.Objects.Add(obj), newObjects, null, null, null, embeddedSoundInfoWad))
                        return true;
                    return false;
                });

                return loadedObjects;
            }
        }

        private class LevelSettingsIds
        {
            public Dictionary<long, ImportedGeometry> ImportedGeometries { get; set; } = new Dictionary<long, ImportedGeometry>();
            public Dictionary<long, LevelTexture> LevelTextures { get; set; } = new Dictionary<long, LevelTexture>();
        }

        private static bool LoadLevelSettings(ChunkReader chunkIO, ChunkId idOuter, string thisPath, ref LevelSettingsIds levelSettingsIdsOuter, ref LevelSettings settingsOuter, Settings loadingSettings, IProgressReporter progressReporter = null)
        {
            if (idOuter != Prj2Chunks.Settings)
                return false;

            LevelSettings settings = new LevelSettings { LevelFilePath = thisPath };
            var levelSettingsIds = new LevelSettingsIds();
            var WadsToLoad = new Dictionary<ReferencedWad, string>(new ReferenceEqualityComparer<ReferencedWad>());
            var importedGeometriesToLoad = new Dictionary<ImportedGeometry, ImportedGeometryInfo>(new ReferenceEqualityComparer<ImportedGeometry>());
            var levelTexturesToLoad = new Dictionary<LevelTexture, string>(new ReferenceEqualityComparer<LevelTexture>());

            chunkIO.ReadChunks((id, chunkSize) =>
            {
                if (id == Prj2Chunks.ObsoleteWadFilePath)
                {
                    progressReporter?.ReportInfo("Reading legacy wad");
                    string wadFilePath = chunkIO.ReadChunkString(chunkSize);
                    ReferencedWad wad = new ReferencedWad(settings, "");
                    WadsToLoad.Clear();
                    WadsToLoad.Add(wad, wadFilePath);
                    settings.Wads.Clear();
                    settings.Wads.Add(wad);
                }
                else if (id == Prj2Chunks.FontTextureFilePath)
                    settings.FontTextureFilePath = chunkIO.ReadChunkString(chunkSize);
                else if (id == Prj2Chunks.SkyTextureFilePath)
                    settings.SkyTextureFilePath = chunkIO.ReadChunkString(chunkSize);
                else if (id == Prj2Chunks.Tr5ExtraSpritesFilePath)
                    settings.Tr5ExtraSpritesFilePath = chunkIO.ReadChunkString(chunkSize);
                else if (id == Prj2Chunks.OldWadSoundPaths)
                {
                    bool Update1_0_8 = false;

                    var oldWadSoundPaths = new List<OldWadSoundPath>();
                    chunkIO.ReadChunks((id2, chunkSize2) =>
                    {
                        if (id2 == Prj2Chunks.OldWadSoundUpdateTag1_0_8)
                        {
                            Update1_0_8 = true;
                            return true;
                        }
                        if (id2 != Prj2Chunks.OldWadSoundPath)
                            return false;

                        var oldWadSoundPath = new OldWadSoundPath("");
                        chunkIO.ReadChunks((id3, chunkSize3) =>
                        {
                            if (id3 == Prj2Chunks.OldWadSoundPathPath)
                                oldWadSoundPath.Path = chunkIO.ReadChunkString(chunkSize3);
                            else
                                return false;
                            return true;
                        });
                        oldWadSoundPaths.Add(oldWadSoundPath);
                        return true;
                    });

                    // Legacy path update...
                    if (!Update1_0_8)
                    {
                        string oldSamplePath0 = LevelSettings.VariableCreate(VariableType.EditorDirectory) + LevelSettings.Dir + "sound";
                        string oldSamplePath1 = LevelSettings.VariableCreate(VariableType.EditorDirectory) + LevelSettings.Dir + "sounds";
                        string oldSamplePath2 = LevelSettings.VariableCreate(VariableType.EditorDirectory) + LevelSettings.Dir + "Sounds" + LevelSettings.Dir + "Samples";
                        string newSamplePath = LevelSettings.VariableCreate(VariableType.EditorDirectory) + LevelSettings.Dir + "Sounds" + LevelSettings.Dir + LevelSettings.VariableCreate(VariableType.SoundEngineVersion) + LevelSettings.Dir + "Samples";
                        for (int i = 0; i < oldWadSoundPaths.Count; ++i)
                            if (oldWadSoundPaths[i].Path.Equals(oldSamplePath0, StringComparison.InvariantCultureIgnoreCase) ||
                                oldWadSoundPaths[i].Path.Equals(oldSamplePath1, StringComparison.InvariantCultureIgnoreCase) ||
                                oldWadSoundPaths[i].Path.Equals(oldSamplePath2, StringComparison.InvariantCultureIgnoreCase))
                                oldWadSoundPaths[i].Path = newSamplePath;
                    }

                    settings.OldWadSoundPaths = oldWadSoundPaths;
                }
                else if (id == Prj2Chunks.GameDirectory)
                    settings.GameDirectory = chunkIO.ReadChunkString(chunkSize);
                else if (id == Prj2Chunks.GameLevelFilePath)
                    settings.GameLevelFilePath = chunkIO.ReadChunkString(chunkSize);
                else if (id == Prj2Chunks.GameExecutableFilePath)
                    settings.GameExecutableFilePath = chunkIO.ReadChunkString(chunkSize);
                else if (id == Prj2Chunks.GameEnableQuickStartFeature)
                    settings.GameEnableQuickStartFeature = chunkIO.ReadChunkBool(chunkSize);
                else if (id == Prj2Chunks.GameVersion)
                    settings.GameVersion = (GameVersion)chunkIO.ReadChunkLong(chunkSize);
                else if (id == Prj2Chunks.Tr5LaraType)
                    settings.Tr5LaraType = (Tr5LaraType)chunkIO.ReadChunkLong(chunkSize);
                else if (id == Prj2Chunks.Tr5Weather)
                    settings.Tr5WeatherType = (Tr5WeatherType)chunkIO.ReadChunkLong(chunkSize);
                else if (id == Prj2Chunks.DefaultAmbientLight)
                    settings.DefaultAmbientLight = chunkIO.ReadChunkVector3(chunkSize);
                else if (id == Prj2Chunks.ScriptDirectory)
                    settings.ScriptDirectory = chunkIO.ReadChunkString(chunkSize);
                else if (id == Prj2Chunks.Wads)
                {
                    progressReporter?.ReportInfo("Reading wads...");

                    var toLoad = new Dictionary<ReferencedWad, string>(new ReferenceEqualityComparer<ReferencedWad>());
                    var list = new List<ReferencedWad>(); // Order matters
                    chunkIO.ReadChunks((id2, chunkSize2) =>
                    {
                        if (id2 != Prj2Chunks.Wad)
                            return false;

                        string path = "";
                        ReferencedWad newWad = new ReferencedWad();
                        chunkIO.ReadChunks((id3, chunkSize3) =>
                        {
                            if (id3 == Prj2Chunks.WadPath)
                                path = chunkIO.ReadChunkString(chunkSize3); // Don't set the path right away, to not load the texture until all information is available.
                            else
                                return false;
                            return true;
                        });

                        // Add wad
                        list.Add(newWad);
                        toLoad.Add(newWad, path);
                        progressReporter?.ReportInfo("Wad successfully loaded: " + path);
                        return true;
                    });

                    WadsToLoad = toLoad;
                    settings.Wads = list;
                }
                else if (id == Prj2Chunks.Textures)
                {
                    progressReporter?.ReportInfo("Reading textures...");

                    var toLoad = new Dictionary<LevelTexture, string>(new ReferenceEqualityComparer<LevelTexture>());
                    var levelTextures = new Dictionary<long, LevelTexture>();
                    chunkIO.ReadChunks((id2, chunkSize2) =>
                    {
                        if (id2 != Prj2Chunks.LevelTexture)
                            return false;

                        string path = "";
                        LevelTexture levelTexture = new LevelTexture();
                        long levelTextureIndex = long.MinValue;
                        chunkIO.ReadChunks((id3, chunkSize3) =>
                        {
                            if (id3 == Prj2Chunks.LevelTextureIndex)
                                levelTextureIndex = chunkIO.ReadChunkLong(chunkSize3);
                            else if (id3 == Prj2Chunks.LevelTexturePath)
                                path = chunkIO.ReadChunkString(chunkSize3); // Don't set the path right away, to not load the texture until all information is available.
                            else if (id3 == Prj2Chunks.LevelTextureConvert512PixelsToDoubleRows)
                                levelTexture.SetConvert512PixelsToDoubleRows(settings, chunkIO.ReadChunkBool(chunkSize3));
                            else if (id3 == Prj2Chunks.LevelTextureReplaceMagentaWithTransparency)
                                levelTexture.SetReplaceMagentaWithTransparency(settings, chunkIO.ReadChunkBool(chunkSize3));
                            else if (id3 == Prj2Chunks.LevelTextureFootStepSounds)
                            {
                                int width = chunkIO.Raw.ReadInt32();
                                int height = chunkIO.Raw.ReadInt32();
                                levelTexture.ResizeFootStepSounds(width, height);
                                for (int y = 0; y < levelTexture.FootStepSoundHeight; ++y)
                                    for (int x = 0; x < levelTexture.FootStepSoundWidth; ++x)
                                    {
                                        byte textureSoundByte = chunkIO.Raw.ReadByte();
                                        if (textureSoundByte > 15)
                                            textureSoundByte = 15;
                                        levelTexture.SetFootStepSound(x, y, (TextureFootStepSound)textureSoundByte);
                                    }
                            }
                            else
                                return false;
                            return true;
                        });
                        levelTextures.Add(levelTextureIndex, levelTexture);
                        toLoad.Add(levelTexture, path);
                        progressReporter?.ReportInfo("Texture successfully loaded: " + path);
                        return true;
                    });
                    settings.Textures = levelTextures.Values.ToList();
                    levelSettingsIds.LevelTextures = levelTextures;
                    levelTexturesToLoad = toLoad;
                }
                else if (id == Prj2Chunks.ImportedGeometries)
                {
                    progressReporter?.ReportInfo("Reading imported geometry...");

                    var toLoad = new Dictionary<ImportedGeometry, ImportedGeometryInfo>(new ReferenceEqualityComparer<ImportedGeometry>());
                    var importedGeometries = new Dictionary<long, ImportedGeometry>();
                    chunkIO.ReadChunks((id2, chunkSize2) =>
                    {
                        if (id2 != Prj2Chunks.ImportedGeometry)
                            return false;

                        ImportedGeometryInfo importedGeometryInfo = ImportedGeometryInfo.Default;
                        long importedGeometryIndex = long.MinValue;
                        chunkIO.ReadChunks((id3, chunkSize3) =>
                        {
                            if (id3 == Prj2Chunks.ImportedGeometryIndex)
                                importedGeometryIndex = chunkIO.ReadChunkLong(chunkSize3);
                            else if (id3 == Prj2Chunks.ImportedGeometryPath)
                                importedGeometryInfo.Path = chunkIO.ReadChunkString(chunkSize3);
                            else if (id3 == Prj2Chunks.ImportedGeometryName)
                                importedGeometryInfo.Name = chunkIO.ReadChunkString(chunkSize3);
                            else if (id3 == Prj2Chunks.ImportedGeometryScale)
                                importedGeometryInfo.Scale = chunkIO.ReadChunkFloat(chunkSize3);
                            else if (id3 == Prj2Chunks.ImportedGeometryPosAxisFlags)
                            {
                                long flag = chunkIO.ReadChunkLong(chunkSize3);
                                importedGeometryInfo.SwapXY = (flag & 1) != 0;
                                importedGeometryInfo.SwapXZ = (flag & 2) != 0;
                                importedGeometryInfo.SwapYZ = (flag & 4) != 0;
                                importedGeometryInfo.FlipX = (flag & 8) != 0;
                                importedGeometryInfo.FlipY = (flag & 16) != 0;
                                importedGeometryInfo.FlipZ = (flag & 32) != 0;
                            }
                            else if (id3 == Prj2Chunks.ImportedGeometryTexAxisFlags)
                            {
                                long flag = chunkIO.ReadChunkLong(chunkSize3);
                                importedGeometryInfo.FlipUV_V = (flag & 4) != 0;
                            }
                            else if (id3 == Prj2Chunks.ImportedGeometryInvertFaces)
                                importedGeometryInfo.InvertFaces = chunkIO.ReadChunkBool(chunkSize3);
                            else
                                return false;
                            return true;
                        });

                        ImportedGeometry importedGeometry = new ImportedGeometry();
                        importedGeometries.Add(importedGeometryIndex, importedGeometry);
                        toLoad.Add(importedGeometry, importedGeometryInfo);
                        progressReporter?.ReportInfo("Imported geometry successfully loaded: " + importedGeometry.Info.Name);
                        return true;
                    });
                    settings.ImportedGeometries = importedGeometries.Values.ToList();
                    levelSettingsIds.ImportedGeometries = importedGeometries;
                    importedGeometriesToLoad = toLoad;
                }
                else if (id == Prj2Chunks.AnimatedTextureSets)
                {
                    var animatedTextureSets = new List<AnimatedTextureSet>();
                    chunkIO.ReadChunks((id2, chunkSize2) =>
                    {
                        if (id2 != Prj2Chunks.AnimatedTextureSet)
                            return false;

                        var set = new AnimatedTextureSet();
                        chunkIO.ReadChunks((id3, chunkSize3) =>
                        {
                            if (id3 == Prj2Chunks.AnimatedTextureSetName)
                                set.Name = chunkIO.ReadChunkString(chunkSize3);
                            else if (id3 == Prj2Chunks.AnimatedTextureSetExtraInfo) // Legacy!
                            {
                                set.AnimationType = (AnimatedTextureAnimationType)LEB128.ReadByte(chunkIO.Raw);
                                set.Fps = LEB128.ReadSByte(chunkIO.Raw);
                                set.UvRotate = LEB128.ReadSByte(chunkIO.Raw);
                            }
                            else if (id3 == Prj2Chunks.AnimatedTextureSetType)
                            {
                                set.AnimationType = (AnimatedTextureAnimationType)chunkIO.ReadChunkInt(chunkSize3);
                            }
                            else if (id3 == Prj2Chunks.AnimatedTextureSetFps)
                            {
                                set.Fps = chunkIO.ReadChunkFloat(chunkSize3);
                            }
                            else if (id3 == Prj2Chunks.AnimatedTextureSetUvRotate)
                            {
                                set.Fps = chunkIO.ReadChunkInt(chunkSize3);
                            }
                            else if (id3 == Prj2Chunks.AnimatedTextureFrames)
                            {
                                var frames = new List<AnimatedTextureFrame>();
                                chunkIO.ReadChunks((id4, chunkSize4) =>
                                {
                                    if (id4 != Prj2Chunks.AnimatedTextureFrame)
                                        return false;

                                    frames.Add(new AnimatedTextureFrame
                                    {
                                        Texture = levelSettingsIds.LevelTextures[LEB128.ReadLong(chunkIO.Raw)],
                                        TexCoord0 = chunkIO.Raw.ReadVector2(),
                                        TexCoord1 = chunkIO.Raw.ReadVector2(),
                                        TexCoord2 = chunkIO.Raw.ReadVector2(),
                                        TexCoord3 = chunkIO.Raw.ReadVector2(),
                                        Repeat = Math.Max(LEB128.ReadInt(chunkIO.Raw), 1)
                                    });
                                    return true;
                                });

                                set.Frames = frames;
                            }
                            else
                                return false;
                            return true;
                        });
                        animatedTextureSets.Add(set);
                        return true;
                    });
                    settings.AnimatedTextureSets = animatedTextureSets;
                }
                else
                    return false;
                return true;
            });

            // Load wads
            progressReporter?.ReportInfo("Loading wads into level");
            if (!loadingSettings.IgnoreWads)
                Parallel.ForEach(WadsToLoad, wad =>
                    wad.Key.SetPath(settings, wad.Value));

            // Load level textures
            progressReporter?.ReportInfo("Loading textures into level");
            if (!loadingSettings.IgnoreTextures)
                Parallel.ForEach(levelTexturesToLoad, levelTexture =>
                    levelTexture.Key.SetPath(settings, levelTexture.Value));

            // Load imported geoemtries
            progressReporter?.ReportInfo("Loading imported geometry into level");
            settings.ImportedGeometryUpdate(importedGeometriesToLoad);

            // Apply settings
            levelSettingsIdsOuter = levelSettingsIds;
            settingsOuter = settings;
            return true;
        }

        private static bool LoadEmbeddedSoundInfoWad(ChunkReader chunkIO, ChunkId idOuter, ref Wad2 embeddedSoundInfoWad, IProgressReporter progressReporter = null)
        {
            if (idOuter != Prj2Chunks.EmbeddedSoundInfoWad)
                return false;

            progressReporter?.ReportInfo("Loading embedded sound samples");
            embeddedSoundInfoWad = Wad2Loader.LoadFromStream(chunkIO.Raw.BaseStream);
            return true;
        }



        private static bool LoadRooms(ChunkReader chunkIO, ChunkId idOuter, Level level, LevelSettingsIds levelSettingsIds, Wad2 embeddedSoundInfoWad, IProgressReporter progressReporter = null)
        {
            if (idOuter != Prj2Chunks.Rooms)
                return false;

            List<KeyValuePair<long, Action<Room>>> roomLinkActions = new List<KeyValuePair<long, Action<Room>>>();
            Dictionary<long, Room> newRooms = new Dictionary<long, Room>();

            List<KeyValuePair<long, Action<ObjectInstance>>> objectLinkActions = new List<KeyValuePair<long, Action<ObjectInstance>>>();
            Dictionary<long, ObjectInstance> newObjects = new Dictionary<long, ObjectInstance>();

            progressReporter?.ReportInfo("Loading rooms");

            chunkIO.ReadChunks((id, chunkSize) =>
            {
                if (id != Prj2Chunks.Room)
                    return false;

                // Read room
                Room room = new Room(level, LEB128.ReadInt(chunkIO.Raw), LEB128.ReadInt(chunkIO.Raw), Vector3.One);
                long roomIndex = long.MinValue;
                chunkIO.ReadChunks((id2, chunkSize2) =>
                {
                    // Read basic room properties
                    if (id2 == Prj2Chunks.RoomIndex)
                        roomIndex = chunkIO.ReadChunkLong(chunkSize2);
                    else if (id2 == Prj2Chunks.RoomName)
                        room.Name = chunkIO.ReadChunkString(chunkSize2);
                    else if (id2 == Prj2Chunks.RoomPosition)
                        room.Position = VectorInt3.FromRounded(chunkIO.ReadChunkVector3(chunkSize2));

                    // Read sectors
                    else if (id2 == Prj2Chunks.RoomSectors)
                        chunkIO.ReadChunks((id3, chunkSize3) =>
                        {
                            if (id3 != Prj2Chunks.Sector)
                                return false;

                            int ReadPos = chunkIO.Raw.ReadInt32();
                            int x = ReadPos % room.NumXSectors;
                            int z = ReadPos / room.NumXSectors;
                            Block block = room.Blocks[x, z];

                            chunkIO.ReadChunks((id4, chunkSize4) =>
                                {
                                    if (id4 == Prj2Chunks.SectorProperties)
                                    {
                                        long flag = chunkIO.ReadChunkLong(chunkSize4);
                                        if ((flag & 1) != 0 && block.Type != BlockType.BorderWall)
                                            block.Type = BlockType.Wall;
                                        block.Flags = (BlockFlags)(flag >> 2);
                                        block.ForceFloorSolid = (flag & 2) != 0;
                                    }
                                    else if (id4 == Prj2Chunks.SectorFloor)
                                    {
                                        long flag = LEB128.ReadLong(chunkIO.Raw);
                                        for (BlockEdge edge = 0; edge < BlockEdge.Count; ++edge)
                                            block.Floor.SetHeight(edge, LEB128.ReadShort(chunkIO.Raw));
                                        for (BlockEdge edge = 0; edge < BlockEdge.Count; ++edge)
                                            block.SetHeight(BlockVertical.Ed, edge, LEB128.ReadShort(chunkIO.Raw));
                                        block.Floor.SplitDirectionIsXEqualsZ = (flag & 1) != 0;
                                        block.Floor.DiagonalSplit = (DiagonalSplit)(flag >> 1);
                                    }
                                    else if (id4 == Prj2Chunks.SectorCeiling)
                                    {
                                        long flag = LEB128.ReadLong(chunkIO.Raw);
                                        for (BlockEdge edge = 0; edge < BlockEdge.Count; ++edge)
                                            block.Ceiling.SetHeight(edge, LEB128.ReadShort(chunkIO.Raw));
                                        for (BlockEdge edge = 0; edge < BlockEdge.Count; ++edge)
                                            block.SetHeight(BlockVertical.Rf, edge, LEB128.ReadShort(chunkIO.Raw));
                                        block.Ceiling.SplitDirectionIsXEqualsZ = (flag & 1) != 0;
                                        block.Ceiling.DiagonalSplit = (DiagonalSplit)(flag >> 1);
                                    }
                                    else if (id4 == Prj2Chunks.TextureLevelTexture)
                                    {
                                        BlockFace face = (BlockFace)LEB128.ReadLong(chunkIO.Raw);

                                        var textureArea = new TextureArea();
                                        textureArea.TexCoord0 = chunkIO.Raw.ReadVector2();
                                        textureArea.TexCoord1 = chunkIO.Raw.ReadVector2();
                                        textureArea.TexCoord2 = chunkIO.Raw.ReadVector2();
                                        textureArea.TexCoord3 = chunkIO.Raw.ReadVector2();
                                        long blendFlag = LEB128.ReadLong(chunkIO.Raw);
                                        textureArea.BlendMode = (BlendMode)(blendFlag >> 1);
                                        textureArea.DoubleSided = (blendFlag & 1) != 0;
                                        textureArea.Texture = levelSettingsIds.LevelTextures.TryGetOrDefault(LEB128.ReadLong(chunkIO.Raw));

                                        block.SetFaceTexture(face, textureArea);
                                    }
                                    else if (id4 == Prj2Chunks.TextureInvisible)
                                    {
                                        BlockFace face = (BlockFace)LEB128.ReadLong(chunkIO.Raw);
                                        block.SetFaceTexture(face, new TextureArea { Texture = TextureInvisible.Instance });
                                    }
                                    else
                                        return false;
                                    return true;
                                });
                            return true;
                        });

                    // Read room properties
                    else if (id2 == Prj2Chunks.RoomAmbientLight)
                    {
                        room.AmbientLight = chunkIO.ReadChunkVector3(chunkSize2);
                    }
                    else if (id2 == Prj2Chunks.RoomFlagCold)
                        room.FlagCold = chunkIO.ReadChunkBool(chunkSize2);
                    else if (id2 == Prj2Chunks.RoomFlagDamage)
                        room.FlagDamage = chunkIO.ReadChunkBool(chunkSize2);
                    else if (id2 == Prj2Chunks.RoomFlagHorizon)
                        room.FlagHorizon = chunkIO.ReadChunkBool(chunkSize2);
                    else if (id2 == Prj2Chunks.RoomFlagOutside)
                        room.FlagOutside = chunkIO.ReadChunkBool(chunkSize2);
                    else if (id2 == Prj2Chunks.RoomFlagNoLensflare)
                        room.FlagNoLensflare = chunkIO.ReadChunkBool(chunkSize2);
                    else if (id2 == Prj2Chunks.RoomFlagExcludeFromPathFinding)
                        room.FlagExcludeFromPathFinding = chunkIO.ReadChunkBool(chunkSize2);
                    else if (id2 == Prj2Chunks.RoomWaterLevel)
                        room.WaterLevel = chunkIO.ReadChunkByte(chunkSize2);
                    else if (id2 == Prj2Chunks.RoomRainLevel)
                        room.RainLevel = chunkIO.ReadChunkByte(chunkSize2);
                    else if (id2 == Prj2Chunks.RoomSnowLevel)
                        room.SnowLevel = chunkIO.ReadChunkByte(chunkSize2);
                    else if (id2 == Prj2Chunks.RoomQuickSandLevel)
                        room.QuickSandLevel = chunkIO.ReadChunkByte(chunkSize2);
                    else if (id2 == Prj2Chunks.RoomMistLevel)
                        room.MistLevel = chunkIO.ReadChunkByte(chunkSize2);
                    else if (id2 == Prj2Chunks.RoomReflectionLevel)
                        room.ReflectionLevel = chunkIO.ReadChunkByte(chunkSize2);
                    else if (id2 == Prj2Chunks.RoomReverberation)
                        room.Reverberation = (Reverberation)chunkIO.ReadChunkByte(chunkSize2);
                    else if (id2 == Prj2Chunks.RoomLocked)
                        room.Locked = chunkIO.ReadChunkBool(chunkSize2);
                    else if (id2 == Prj2Chunks.RoomAlternate)
                    {
                        short alternateGroup = 1;
                        long alternateRoomIndex = -1;
                        chunkIO.ReadChunks((id3, chunkSize3) =>
                        {
                            if (id3 == Prj2Chunks.AlternateGroup)
                                alternateGroup = chunkIO.ReadChunkShort(chunkSize3);
                            else if (id3 == Prj2Chunks.AlternateRoom)
                                alternateRoomIndex = chunkIO.ReadChunkLong(chunkSize3);
                            else
                                return false;
                            return true;
                        });
                        roomLinkActions.Add(new KeyValuePair<long, Action<Room>>(alternateRoomIndex, alternateRoom =>
                            {
                                if (room.AlternateRoom != null)
                                    progressReporter?.ReportWarn("Room " + room + " has more than 1 flip room.");
                                else if (alternateRoom.AlternateBaseRoom != null)
                                    progressReporter?.ReportWarn("Room  " + alternateRoom + " is used for more than 1 flip room.");
                                else
                                {
                                    room.AlternateRoom = alternateRoom;
                                    room.AlternateGroup = alternateGroup;
                                    alternateRoom.AlternateBaseRoom = room;
                                    alternateRoom.AlternateGroup = alternateGroup;
                                }
                            }));
                    }
                    else if (LoadObjects(chunkIO, id2, levelSettingsIds, obj => room.AddObjectAndSingularPortal(level, obj), 
                            newObjects, room, roomLinkActions, objectLinkActions, embeddedSoundInfoWad))
                        return true;
                    else
                        return false;
                    return true;
                });

                // Add room
                if (roomIndex > 0 && roomIndex < level.Rooms.Length && level.Rooms[roomIndex] == null)
                    level.Rooms[roomIndex] = room;
                else
                    level.AssignRoomToFree(room);

                if (!newRooms.ContainsKey(roomIndex))
                    newRooms.Add(roomIndex, room);

                return true;
            });

            progressReporter?.ReportInfo("Linking rooms with objects");

            // Link rooms
            foreach (var roomLinkAction in roomLinkActions)
                try
                {
                    roomLinkAction.Value(newRooms.TryGetOrDefault(roomLinkAction.Key));
                }
                catch (Exception exc)
                {
                    logger.Error(exc, "An exception was raised while trying to perform room link action.");
                }

            // Link objects
            foreach (var objectLinkAction in objectLinkActions)
                try
                {
                    objectLinkAction.Value(newObjects.TryGetOrDefault(objectLinkAction.Key));
                }
                catch (Exception exc)
                {
                    logger.Error(exc, "An exception was raised while trying to perform room link objects.");
                }

            // Now build the real geometry and update geometry buffers
            progressReporter?.ReportInfo("Building world geometry");
            Parallel.ForEach(level.Rooms.Where(room => room != null), room => room.BuildGeometry());
            return true;
        }

        private static bool LoadObjects(ChunkReader chunkIO, ChunkId idOuter, LevelSettingsIds levelSettingsIds,
            Action<ObjectInstance> addObject, Dictionary<long, ObjectInstance> newObjects,
            Room room, List<KeyValuePair<long, Action<Room>>> roomLinkActions, 
            List<KeyValuePair<long, Action<ObjectInstance>>> objectLinkActions, Wad2 embeddedSoundInfoWad)
        {
            if (idOuter != Prj2Chunks.Objects)
                return false;

            chunkIO.ReadChunks((id3, chunkSize3) =>
            {
                long objectID = LEB128.ReadLong(chunkIO.Raw);
                if (id3 == Prj2Chunks.ObjectMovable)
                {
                    var instance = new MoveableInstance();
                    instance.Position = chunkIO.Raw.ReadVector3();
                    instance.RotationY = chunkIO.Raw.ReadSingle();
                    instance.ScriptId = ReadOptionalLEB128Int(chunkIO.Raw);
                    instance.WadObjectId = new Wad.WadMoveableId(chunkIO.Raw.ReadUInt32());
                    instance.Ocb = chunkIO.Raw.ReadInt16();
                    instance.Invisible = chunkIO.Raw.ReadBoolean();
                    instance.ClearBody = chunkIO.Raw.ReadBoolean();
                    instance.CodeBits = chunkIO.Raw.ReadByte();
                    addObject(instance);
                    newObjects.TryAdd(objectID, instance);
                }
                if (id3 == Prj2Chunks.ObjectMovable2)
                {
                    var instance = new MoveableInstance();
                    instance.Position = chunkIO.Raw.ReadVector3();
                    instance.RotationY = chunkIO.Raw.ReadSingle();
                    instance.ScriptId = ReadOptionalLEB128Int(chunkIO.Raw);
                    instance.WadObjectId = new Wad.WadMoveableId(chunkIO.Raw.ReadUInt32());
                    instance.Ocb = chunkIO.Raw.ReadInt16();
                    instance.Invisible = chunkIO.Raw.ReadBoolean();
                    instance.ClearBody = chunkIO.Raw.ReadBoolean();
                    instance.CodeBits = chunkIO.Raw.ReadByte();
                    chunkIO.ReadChunks((id4, chunkSize4) =>
                    {
                        if (id4 == Prj2Chunks.ObjectItemLuaId)
                        {
                            instance.LuaId = chunkIO.ReadChunkInt(chunkSize4);
                            return true;
                        }
                        return false;
                    });
                    addObject(instance);
                    newObjects.TryAdd(objectID, instance);                    
                }
                else if (id3 == Prj2Chunks.ObjectStatic)
                {
                    var instance = new StaticInstance();
                    newObjects.TryAdd(objectID, instance);
                    instance.Position = chunkIO.Raw.ReadVector3();
                    instance.RotationY = chunkIO.Raw.ReadSingle();
                    instance.ScriptId = ReadOptionalLEB128Int(chunkIO.Raw);
                    instance.WadObjectId = new Wad.WadStaticId(chunkIO.Raw.ReadUInt32());
                    instance.Color = chunkIO.Raw.ReadVector3();
                    chunkIO.Raw.ReadSingle(); // Unused 32 bit value
                    chunkIO.ReadChunks((id4, chunkSize4) =>
                    {
                        if (id4 == Prj2Chunks.ObjectItemLuaId)
                        {
                            instance.LuaId = chunkIO.ReadChunkInt(chunkSize4);
                            return true;
                        }
                        return false;
                    });
                    instance.Ocb = chunkIO.Raw.ReadUInt16();
                    addObject(instance);
                }
                else if (id3 == Prj2Chunks.ObjectCamera)
                {
                    var instance = new CameraInstance();
                    instance.Position = chunkIO.Raw.ReadVector3();
                    instance.ScriptId = ReadOptionalLEB128Int(chunkIO.Raw);
                    instance.Fixed = chunkIO.Raw.ReadBoolean();
                    addObject(instance);
                    newObjects.TryAdd(objectID, instance);
                }
                else if (id3 == Prj2Chunks.ObjectFlyBy)
                {
                    var instance = new FlybyCameraInstance();
                    instance.Position = chunkIO.Raw.ReadVector3();
                    instance.SetArbitaryRotationsYX(chunkIO.Raw.ReadSingle(), chunkIO.Raw.ReadSingle());
                    instance.Roll = chunkIO.Raw.ReadSingle();
                    instance.ScriptId = ReadOptionalLEB128Int(chunkIO.Raw);
                    instance.Speed = chunkIO.Raw.ReadSingle();
                    instance.Fov = chunkIO.Raw.ReadSingle();
                    instance.Flags = LEB128.ReadUShort(chunkIO.Raw);
                    instance.Number = LEB128.ReadUShort(chunkIO.Raw);
                    instance.Sequence = LEB128.ReadUShort(chunkIO.Raw);
                    instance.Timer = LEB128.ReadShort(chunkIO.Raw);
                    addObject(instance);
                    newObjects.TryAdd(objectID, instance);
                }
                else if (id3 == Prj2Chunks.ObjectSink)
                {
                    var instance = new SinkInstance();
                    instance.Position = chunkIO.Raw.ReadVector3();
                    instance.ScriptId = ReadOptionalLEB128Int(chunkIO.Raw);
                    instance.Strength = chunkIO.Raw.ReadInt16();
                    addObject(instance);
                    newObjects.TryAdd(objectID, instance);
                }
                else if (id3 == Prj2Chunks.ObjectSoundSource3 || id3 == Prj2Chunks.ObjectSoundSource2 || id3 == Prj2Chunks.ObjectSoundSource)
                {
                    var instance = new SoundSourceInstance();
                    instance.Position = chunkIO.Raw.ReadVector3();

                    if (id3 == Prj2Chunks.ObjectSoundSource)
                    {
                        instance.WadReferencedSoundName = TrCatalog.GetOriginalSoundName(WadGameVersion.TR4_TRNG, chunkIO.Raw.ReadUInt16());
                        chunkIO.Raw.ReadInt16(); // Unused
                        chunkIO.Raw.ReadByte(); // Unused
                    }
                    else if (id3 == Prj2Chunks.ObjectSoundSource2)
                    {
                        instance.WadReferencedSoundName = chunkIO.Raw.ReadString(); // Used wrong string type
                        chunkIO.Raw.ReadInt16(); // Unused
                        chunkIO.Raw.ReadByte(); // Unused
                    }
                    else
                    {
                        instance.WadReferencedSoundName = chunkIO.Raw.ReadStringUTF8();

                        // Use an embedded sound info
                        long soundInfoId = LEB128.ReadLong(chunkIO.Raw);
                        if (soundInfoId >= 0)
                            instance.EmbeddedSoundInfo = embeddedSoundInfoWad.FixedSoundInfos[new WadFixedSoundInfoId((uint)soundInfoId)].SoundInfo;
                    }

                    addObject(instance);
                    newObjects.TryAdd(objectID, instance);
                }
                else if (id3 == Prj2Chunks.ObjectLight)
                {
                    var instance = new LightInstance((LightType)LEB128.ReadLong(chunkIO.Raw));
                    instance.Position = chunkIO.Raw.ReadVector3();
                    instance.SetArbitaryRotationsYX(chunkIO.Raw.ReadSingle(), chunkIO.Raw.ReadSingle());
                    instance.Intensity = chunkIO.Raw.ReadSingle();
                    instance.Color = chunkIO.Raw.ReadVector3();
                    instance.InnerRange = chunkIO.Raw.ReadSingle();
                    instance.OuterRange = chunkIO.Raw.ReadSingle();
                    instance.InnerAngle = chunkIO.Raw.ReadSingle();
                    instance.OuterAngle = chunkIO.Raw.ReadSingle();
                    instance.Enabled = chunkIO.Raw.ReadBoolean();
                    instance.IsObstructedByRoomGeometry = chunkIO.Raw.ReadBoolean();
                    instance.IsDynamicallyUsed = chunkIO.Raw.ReadBoolean();
                    instance.IsStaticallyUsed = chunkIO.Raw.ReadBoolean();
                    addObject(instance);
                    newObjects.TryAdd(objectID, instance);
                }
                else if (id3 == Prj2Chunks.ObjectPortal)
                {
                    var area = new RectangleInt2(LEB128.ReadInt(chunkIO.Raw), LEB128.ReadInt(chunkIO.Raw), LEB128.ReadInt(chunkIO.Raw), LEB128.ReadInt(chunkIO.Raw));
                    var adjoiningRoomIndex = LEB128.ReadLong(chunkIO.Raw);
                    var direction = (PortalDirection)chunkIO.Raw.ReadByte();

                    // Create a replacement portal that uses the source room as a temporary placeholder
                    // If an issue comes up that prevents loading the second room, this placeholder will be used permanently.
                    var instance = new PortalInstance(area, direction, room);
                    instance.Opacity = (PortalOpacity)chunkIO.Raw.ReadByte();
                    roomLinkActions.Add(new KeyValuePair<long, Action<Room>>(adjoiningRoomIndex, adjoiningRoom => instance.AdjoiningRoom = adjoiningRoom ?? room));

                    addObject(instance);
                    newObjects.TryAdd(objectID, instance);
                }
                else if (id3 == Prj2Chunks.ObjectTrigger)
                {
                    var area = new RectangleInt2(LEB128.ReadInt(chunkIO.Raw), LEB128.ReadInt(chunkIO.Raw), LEB128.ReadInt(chunkIO.Raw), LEB128.ReadInt(chunkIO.Raw));
                    var instance = new TriggerInstance(area);
                    instance.TriggerType = (TriggerType)LEB128.ReadLong(chunkIO.Raw);
                    instance.TargetType = (TriggerTargetType)LEB128.ReadLong(chunkIO.Raw);
                    instance.Target = new TriggerParameterUshort(unchecked((ushort)LEB128.ReadShort(chunkIO.Raw)));
                    long targetObjectId = LEB128.ReadLong(chunkIO.Raw);

                    ushort realTimer = unchecked((ushort)LEB128.ReadShort(chunkIO.Raw));
                    ushort? timer, extra;
                    NG.NgParameterInfo.DecodeNGRealTimer(instance.TargetType, instance.TriggerType,
                        unchecked((ushort)targetObjectId), realTimer, out timer, out extra);
                    instance.Timer = timer == null ? null : new TriggerParameterUshort(timer.Value);
                    instance.Extra = timer == null ? null : new TriggerParameterUshort(extra.Value);

                    instance.CodeBits = (byte)(LEB128.ReadLong(chunkIO.Raw) & 0x1f);
                    instance.OneShot = chunkIO.Raw.ReadBoolean();
                    objectLinkActions.Add(new KeyValuePair<long, Action<ObjectInstance>>(targetObjectId, targetObj => instance.Target = targetObj));

                    addObject(instance);
                    newObjects.TryAdd(objectID, instance);
                }
                else if (id3 == Prj2Chunks.ObjectTrigger2)
                {
                    var area = new RectangleInt2(LEB128.ReadInt(chunkIO.Raw), LEB128.ReadInt(chunkIO.Raw), LEB128.ReadInt(chunkIO.Raw), LEB128.ReadInt(chunkIO.Raw));
                    var instance = new TriggerInstance(area);

                    Action<Action<ITriggerParameter>> readParameter = setTriggerParameter =>
                    {
                        long type = LEB128.ReadLong(chunkIO.Raw);
                        switch (type)
                        {
                            case 0:
                                setTriggerParameter(new TriggerParameterUshort(LEB128.ReadUShort(chunkIO.Raw)));
                                return;
                            case 1:
                                long objectId = LEB128.ReadLong(chunkIO.Raw);
                                objectLinkActions.Add(new KeyValuePair<long, Action<ObjectInstance>>(objectId, targetObj => setTriggerParameter(targetObj)));
                                return;
                            case 2:
                                long roomId = LEB128.ReadLong(chunkIO.Raw);
                                roomLinkActions.Add(new KeyValuePair<long, Action<Room>>(roomId, targetRoom => setTriggerParameter(targetRoom)));
                                return;
                            case -1:
                                setTriggerParameter(null);
                                return;
                            default:
                                throw new Exception("Unknown trigger target for '" + instance + "'");
                        }
                    };

                    chunkIO.ReadChunks((id4, chunkSize4) =>
                    {
                        if (id4 == Prj2Chunks.ObjectTrigger2Type)
                            instance.TriggerType = (TriggerType)chunkIO.ReadChunkLong(chunkSize4);
                        else if (id4 == Prj2Chunks.ObjectTrigger2TargetType)
                            instance.TargetType = (TriggerTargetType)chunkIO.ReadChunkLong(chunkSize4);
                        else if (id4 == Prj2Chunks.ObjectTrigger2Target)
                            readParameter(parameter => instance.Target = parameter);
                        else if (id4 == Prj2Chunks.ObjectTrigger2Timer)
                            readParameter(parameter => instance.Timer = parameter);
                        else if (id4 == Prj2Chunks.ObjectTrigger2Extra)
                            readParameter(parameter => instance.Extra = parameter);
                        else if (id4 == Prj2Chunks.ObjectTrigger2CodeBits)
                            instance.CodeBits = unchecked((byte)chunkIO.ReadChunkLong(chunkSize4));
                        else if (id4 == Prj2Chunks.ObjectTrigger2OneShot)
                            instance.OneShot = chunkIO.ReadChunkBool(chunkSize4);
                        else if (id4 == Prj2Chunks.ObjectTrigger2LuaScript)
                            instance.LuaScript = chunkIO.ReadChunkString(chunkSize4);
                        else
                            return false;
                        return true;
                    });

                    addObject(instance);
                    newObjects.TryAdd(objectID, instance);
                }
                else if (id3 == Prj2Chunks.ObjectImportedGeometry || id3 == Prj2Chunks.ObjectImportedGeometry2)
                {
                    var instance = new ImportedGeometryInstance();
                    instance.Position = chunkIO.Raw.ReadVector3();
                    instance.SetArbitaryRotationsYX(chunkIO.Raw.ReadSingle(), chunkIO.Raw.ReadSingle());
                    instance.Roll = chunkIO.Raw.ReadSingle();
                    instance.Scale = chunkIO.Raw.ReadSingle();
                    if (!(id3 == Prj2Chunks.ObjectImportedGeometry && chunkSize3 == 30)) // For some time we accidentally emitted MeshFilter but still emitted the old ObjectImportedGeometry chunk name unfortunately. Thus we need to check chunk size too.
                        instance.MeshFilter = chunkIO.Raw.ReadStringUTF8();
                    instance.Model = levelSettingsIds.ImportedGeometries.TryGetOrDefault(LEB128.ReadLong(chunkIO.Raw));
                    addObject(instance);
                    newObjects.TryAdd(objectID, instance);
                }
                else
                    return false;
                return true;
            });
            return true;
        }

        private static uint? ReadOptionalLEB128Int(BinaryReaderEx reader)
        {
            long read = LEB128.ReadLong(reader);
            if (read < 0)
                return null;
            else if (read > uint.MaxValue)
                return uint.MaxValue;
            else
                return (uint)read;
        }

        private static void TryAdd<K, T>(this Dictionary<K, T> this_, K key, T value)
        {
            if (!this_.ContainsKey(key))
                this_.Add(key, value);
        }
    }
}
