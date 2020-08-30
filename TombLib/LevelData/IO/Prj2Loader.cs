using NLog;
using System;
using System.Collections.Generic;
using System.Drawing;
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
            public bool IgnoreSoundsCatalogs = false;
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
            using (var chunkIO = new ChunkReader(Prj2Chunks.MagicNumber, stream, Prj2Chunks.ChunkList))
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

                level.Settings.HasUnknownData = chunkIO.UnknownChunksFound;
                return level;
            }
        }

        public struct LoadedObjects
        {
            public LevelSettings Settings;
            public List<ObjectInstance> Objects;
        }

        public static LevelSettings LoadFromPrj2OnlySettings(string filename, Stream stream) =>
            LoadFromPrj2OnlyObjects(filename, null, stream, new Settings()).Settings;
        public static LoadedObjects LoadFromPrj2OnlyObjects(string filename, Level level, Stream stream, Settings loadSettings)
        {
            var roomLinkActions     = new List<KeyValuePair<long, Action<Room>>>();
            var objectLinkActions   = new List<KeyValuePair<long, Action<ObjectInstance>>>();
            var objectMapDictionary = new Dictionary<long, ObjectInstance>();

            using (var chunkIO = new ChunkReader(Prj2Chunks.MagicNumber, stream, Prj2Chunks.ChunkList))
            {
                LevelSettingsIds levelSettingsIds = new LevelSettingsIds();
                LoadedObjects loadedObjects = new LoadedObjects { Settings = new LevelSettings(), Objects = new List<ObjectInstance>() };
                Wad2 embeddedSoundInfoWad = null;

                chunkIO.ReadChunks((id, chunkSize) =>
                {
                    if (LoadLevelSettings(chunkIO, id, filename, ref levelSettingsIds, ref loadedObjects.Settings, loadSettings))
                        return true;
                    else if (LoadEmbeddedSoundInfoWad(chunkIO, id, ref embeddedSoundInfoWad))
                        return true;
                    else if (LoadObjects(chunkIO, id, levelSettingsIds,
                        obj => { }, objectMapDictionary, null, roomLinkActions, objectLinkActions, embeddedSoundInfoWad))
                        return true;
                    return false;
                });

                if (level != null)
                {

                    // Link objects
                    foreach (var objectLinkAction in objectLinkActions)
                        try
                        {
                            // Decode room index and truncated object hash from lookup key.

                            var roomID = objectLinkAction.Key & 1023;
                            int   hash = (int)((objectLinkAction.Key & 0xFFFFFC00) >> 10);
                            bool found = false;

                            if (level.Rooms[roomID] != null)
                            {
                                var objects = level.Rooms[roomID].Objects;
                                foreach (var obj in objects)
                                    if (hash == (obj.GetHashCode() & 0x3FFFFF))
                                    {
                                        objectLinkAction.Value(obj);
                                        found = true;
                                        break;
                                    }

                                if (!found)
                                    throw new InvalidDataException();
                            }
                            else
                                throw new InvalidDataException();

                        }
                        catch (Exception exc)
                        {
                            logger.Error(exc, "An exception was raised while trying to perform room link action.");
                            return loadedObjects;
                        }

                    // Link rooms
                    foreach (var roomLinkAction in roomLinkActions)
                        try
                        {
                            if (level.Rooms[roomLinkAction.Key] != null)
                                roomLinkAction.Value(level.Rooms[roomLinkAction.Key]);
                            else
                                throw new InvalidDataException();
                        }
                        catch (Exception exc)
                        {
                            logger.Error(exc, "An exception was raised while trying to perform room link action.");
                            return loadedObjects;
                        }
                }

                loadedObjects.Objects = objectMapDictionary.Values.ToList();
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
            var SoundsCatalogsToLoad = new Dictionary<ReferencedSoundsCatalog, string>(new ReferenceEqualityComparer<ReferencedSoundsCatalog>());
            var importedGeometriesToLoad = new Dictionary<ImportedGeometry, ImportedGeometryInfo>(new ReferenceEqualityComparer<ImportedGeometry>());
            var levelTexturesToLoad = new Dictionary<LevelTexture, string>(new ReferenceEqualityComparer<LevelTexture>());

            bool foundSoundSystem = false;

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
                else if (id == Prj2Chunks.SoundSystem)
                {
                    foundSoundSystem = true;
                    settings.SoundSystem = (SoundSystem)chunkIO.ReadChunkInt(chunkSize);
                }
                else if (id == Prj2Chunks.LastRoom)
                    settings.LastSelectedRoom = chunkIO.ReadChunkInt(chunkSize);
                else if (id == Prj2Chunks.FontTextureFilePath)
                    settings.FontTextureFilePath = chunkIO.ReadChunkString(chunkSize);
                else if (id == Prj2Chunks.SkyTextureFilePath)
                    settings.SkyTextureFilePath = chunkIO.ReadChunkString(chunkSize);
                else if (id == Prj2Chunks.Tr5ExtraSpritesFilePath)
                    settings.Tr5ExtraSpritesFilePath = chunkIO.ReadChunkString(chunkSize);
                else if (id == Prj2Chunks.OldWadSoundPaths)
                {
                    bool Update1_0_8 = false;

                    var oldWadSoundPaths = new List<WadSoundPath>();
                    chunkIO.ReadChunks((id2, chunkSize2) =>
                    {
                        if (id2 == Prj2Chunks.OldWadSoundUpdateTag1_0_8)
                        {
                            Update1_0_8 = true;
                            return true;
                        }
                        if (id2 != Prj2Chunks.OldWadSoundPath)
                            return false;

                        var oldWadSoundPath = new WadSoundPath("");
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

                    settings.WadSoundPaths = oldWadSoundPaths;
                }
                else if (id == Prj2Chunks.SoundsCatalogs)
                {
                    progressReporter?.ReportInfo("Reading sound catalogs...");

                    var toLoad = new Dictionary<ReferencedSoundsCatalog, string>(new ReferenceEqualityComparer<ReferencedSoundsCatalog>());
                    var list = new List<ReferencedSoundsCatalog>(); // Order matters
                    chunkIO.ReadChunks((id2, chunkSize2) =>
                    {
                        if (id2 != Prj2Chunks.SoundsCatalog)
                            return false;

                        string path = "";
                        ReferencedSoundsCatalog newSounds = new ReferencedSoundsCatalog();
                        chunkIO.ReadChunks((id3, chunkSize3) =>
                        {
                            if (id3 == Prj2Chunks.SoundsCatalogPath)
                                path = chunkIO.ReadChunkString(chunkSize3); // Don't set the path right away until all information is available.
                            else
                                return false;
                            return true;
                        });

                        // Remap stock catalogs to new location (since version 1.3.6)
                        var oldPrefix = "$(EditorDirectory)\\Catalogs\\";
                        if (path.StartsWith(oldPrefix))
                            path = path.Replace(oldPrefix, "$(EditorDirectory)\\Assets\\SoundCatalogs\\");

                        // Add catalog
                        list.Add(newSounds);
                        toLoad.Add(newSounds, path);
                        progressReporter?.ReportInfo("Sound catalog successfully loaded: " + path);
                        return true;
                    });

                    SoundsCatalogsToLoad = toLoad;
                    settings.SoundsCatalogs = list;
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
                    settings.GameVersion = (TRVersion.Game)chunkIO.ReadChunkLong(chunkSize);
                else if (id == Prj2Chunks.Tr5LaraType)
                    settings.Tr5LaraType = (Tr5LaraType)chunkIO.ReadChunkLong(chunkSize);
                else if (id == Prj2Chunks.Tr5Weather)
                    settings.Tr5WeatherType = (Tr5WeatherType)chunkIO.ReadChunkLong(chunkSize);
                else if (id == Prj2Chunks.TexturePadding)
                    settings.TexturePadding = chunkIO.ReadChunkInt(chunkSize);
                else if (id == Prj2Chunks.Dither16BitTextures)
                    settings.Dither16BitTextures = chunkIO.ReadChunkBool(chunkSize);
                else if (id == Prj2Chunks.RemapAnimatedTextures)
                    settings.RemapAnimatedTextures = chunkIO.ReadChunkBool(chunkSize);
                else if (id == Prj2Chunks.AgressiveTexturePacking)
                    settings.AgressiveTexturePacking = chunkIO.ReadChunkBool(chunkSize);
                else if (id == Prj2Chunks.AgressiveFloordataPacking)
                    settings.AgressiveFloordataPacking = chunkIO.ReadChunkBool(chunkSize);
                else if (id == Prj2Chunks.DefaultAmbientLight)
                    settings.DefaultAmbientLight = chunkIO.ReadChunkVector3(chunkSize);
                else if (id == Prj2Chunks.DefaultLightQuality)
                    settings.DefaultLightQuality = (LightQuality)chunkIO.ReadChunkLong(chunkSize);
                else if (id == Prj2Chunks.OverrideLightQuality)
                    settings.OverrideIndividualLightQualitySettings = chunkIO.ReadChunkBool(chunkSize);
                else if (id == Prj2Chunks.ScriptDirectory)
                    settings.ScriptDirectory = chunkIO.ReadChunkString(chunkSize);
                else if (id == Prj2Chunks.SelectedSounds)
                {
                    chunkIO.ReadChunks((id2, chunkSize2) =>
                    {
                        if (id2 != Prj2Chunks.SelectedSound)
                            return false;

                        settings.SelectedSounds.Add(chunkIO.ReadChunkInt(chunkSize2));
                        return true;
                    });
                }
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
                            else if (id3 == Prj2Chunks.LevelTextureCustomBumpmapPath)
                                levelTexture.BumpPath = chunkIO.ReadChunkString(chunkSize3);
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
                            else if (id3 == Prj2Chunks.LevelTextureBumpmaps)
                            {
                                int width = chunkIO.Raw.ReadInt32();
                                int height = chunkIO.Raw.ReadInt32();
                                levelTexture.ResizeBumpMappingInfos(width, height);
                                for (int y = 0; y < levelTexture.BumpMappingHeight; ++y)
                                    for (int x = 0; x < levelTexture.BumpMappingWidth; ++x)
                                    {
                                        byte bumpMappingByte = chunkIO.Raw.ReadByte();
                                        if (bumpMappingByte > 3)
                                            bumpMappingByte = 3;
                                        levelTexture.SetBumpMappingLevel(x, y, (BumpMappingLevel)bumpMappingByte);
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
                        progressReporter?.ReportInfo("Imported geometry successfully loaded: " + importedGeometryInfo.Name);
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
                                if (set.Fps == 0.0f)
                                    set.Fps = 15.0f;
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
                                set.UvRotate = chunkIO.ReadChunkInt(chunkSize3);
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
                else if (id == Prj2Chunks.AutoMergeStaticMeshes)
                {
                    chunkIO.ReadChunks((id2, size) =>
                    {
                        if (id2 == Prj2Chunks.AutoMergeStaticMeshEntry)
                        {
                            uint value = chunkIO.Raw.ReadUInt32();
                            bool vertexShades = chunkIO.Raw.ReadBoolean();
                            settings.AutoStaticMeshMerges.Add(new AutoStaticMeshMergeEntry(value, true, vertexShades, false, false, settings));
                            return true;
                        }
                        else if (id2 == Prj2Chunks.AutoMergeStaticMeshEntry2)
                        {
                            uint value = chunkIO.Raw.ReadUInt32();
                            bool vertexShades = chunkIO.Raw.ReadBoolean();
                            bool tintAsAmbient = chunkIO.Raw.ReadBoolean();
                            settings.AutoStaticMeshMerges.Add(new AutoStaticMeshMergeEntry(value, true, vertexShades, tintAsAmbient, false, settings));
                            return true;
                        }
                        else if (id2 == Prj2Chunks.AutoMergeStaticMeshEntry3)
                        {
                            uint value = chunkIO.Raw.ReadUInt32();
                            bool vertexShades = chunkIO.Raw.ReadBoolean();
                            bool tintAsAmbient = chunkIO.Raw.ReadBoolean();
                            bool clearShades = chunkIO.Raw.ReadBoolean();
                            settings.AutoStaticMeshMerges.Add(new AutoStaticMeshMergeEntry(value, true, vertexShades, tintAsAmbient, clearShades, settings));
                            return true;
                        }
                        else return false;
                    });
                }
                else if (id == Prj2Chunks.Palette)
                {
                    var colorCount = chunkIO.Raw.ReadUInt16();
                    var colorList = new List<ColorC>();

                    for (int i = 0; i < colorCount; i++)
                    {
                        var r = chunkIO.Raw.ReadByte();
                        var g = chunkIO.Raw.ReadByte();
                        var b = chunkIO.Raw.ReadByte();
                        colorList.Add(new ColorC(r, g, b));
                    }
                    settings.Palette = colorList;
                }
                else
                    return false;
                return true;
            });

            if (!foundSoundSystem)
                settings.SoundSystem = SoundSystem.Dynamic;

            // Load wads
            progressReporter?.ReportInfo("Loading wads into level");
            if (!loadingSettings.IgnoreWads)
                Parallel.ForEach(WadsToLoad, wad =>
                    wad.Key.SetPath(settings, wad.Value));

            // Load sound catalogs
            progressReporter?.ReportInfo("Loading sound catalogs into level");
            if (!loadingSettings.IgnoreSoundsCatalogs)
                Parallel.ForEach(SoundsCatalogsToLoad, catalog =>
                    catalog.Key.SetPath(settings, catalog.Value));

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
                    else if (id2 == Prj2Chunks.RoomTags)
                    {
                        var tags = System.Text.Encoding.UTF8.GetString(chunkIO.ReadChunkArrayOfBytes(chunkSize2)).Split(' ');
                        if (tags.Count() == 1 && string.IsNullOrEmpty(tags[0]))
                            room.Properties.Tags = new List<string>();
                        else
                            room.Properties.Tags = tags.ToList();
                    }


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
                                    else if (id4 == Prj2Chunks.TextureLevelTexture ||
                                             id4 == Prj2Chunks.TextureLevelTexture2)
                                    {
                                        BlockFace face = (BlockFace)LEB128.ReadLong(chunkIO.Raw);

                                        var textureArea = new TextureArea();
                                        textureArea.TexCoord0 = chunkIO.Raw.ReadVector2();
                                        textureArea.TexCoord1 = chunkIO.Raw.ReadVector2();
                                        textureArea.TexCoord2 = chunkIO.Raw.ReadVector2();
                                        textureArea.TexCoord3 = chunkIO.Raw.ReadVector2();

                                        if(id4 == Prj2Chunks.TextureLevelTexture2)
                                        {
                                            textureArea.ParentArea.Start = chunkIO.Raw.ReadVector2();
                                            textureArea.ParentArea.End = chunkIO.Raw.ReadVector2();
                                        }

                                        long blendFlag = LEB128.ReadLong(chunkIO.Raw);
                                        textureArea.BlendMode = (BlendMode)(blendFlag >> 1);
                                        textureArea.DoubleSided = (blendFlag & 1) != 0;
                                        textureArea.Texture = levelSettingsIds.LevelTextures.TryGetOrDefault(LEB128.ReadLong(chunkIO.Raw));

                                        block.SetFaceTexture(face, textureArea);
                                    }
                                    else if (id4 == Prj2Chunks.TextureInvisible)
                                    {
                                        BlockFace face = (BlockFace)LEB128.ReadLong(chunkIO.Raw);
                                        block.SetFaceTexture(face, TextureArea.Invisible);
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
                        room.Properties.AmbientLight = chunkIO.ReadChunkVector3(chunkSize2);
                    }
                    else if (id2 == Prj2Chunks.RoomFlagCold)
                        room.Properties.FlagCold = chunkIO.ReadChunkBool(chunkSize2);
                    else if (id2 == Prj2Chunks.RoomFlagDamage)
                        room.Properties.FlagDamage = chunkIO.ReadChunkBool(chunkSize2);
                    else if (id2 == Prj2Chunks.RoomFlagHorizon)
                        room.Properties.FlagHorizon = chunkIO.ReadChunkBool(chunkSize2);
                    else if (id2 == Prj2Chunks.RoomFlagOutside)
                        room.Properties.FlagOutside = chunkIO.ReadChunkBool(chunkSize2);
                    else if (id2 == Prj2Chunks.RoomFlagNoLensflare)
                        room.Properties.FlagNoLensflare = chunkIO.ReadChunkBool(chunkSize2);
                    else if (id2 == Prj2Chunks.RoomFlagExcludeFromPathFinding)
                        room.Properties.FlagExcludeFromPathFinding = chunkIO.ReadChunkBool(chunkSize2);
                    else if (id2 == Prj2Chunks.RoomLightInterpolationMode)
                        room.Properties.LightInterpolationMode = (RoomLightInterpolationMode)chunkIO.ReadChunkInt(chunkSize2);
                    else if (id2 == Prj2Chunks.RoomWaterLevel) // DEPRECATED
                    {
                        var val = chunkIO.ReadChunkByte(chunkSize2);
                        if(val > 0)
                        {
                            room.Properties.Type = RoomType.Water;
                            room.Properties.LightEffect = RoomLightEffect.Default;
                            room.Properties.LightEffectStrength = val;
                        }
                    }
                    else if (id2 == Prj2Chunks.RoomRainLevel) // DEPRECATED
                    {
                        var val = chunkIO.ReadChunkByte(chunkSize2);
                        if (val > 0)
                        {
                            room.Properties.Type = RoomType.Rain;
                            room.Properties.LightEffect = RoomLightEffect.Default;
                            room.Properties.TypeStrength = (byte)(val - 1);
                        }
                    }
                    else if (id2 == Prj2Chunks.RoomSnowLevel) // DEPRECATED
                    {
                        var val = chunkIO.ReadChunkByte(chunkSize2);
                        if (val > 0)
                        {
                            room.Properties.Type = RoomType.Snow;
                            room.Properties.LightEffect = RoomLightEffect.Default;
                            room.Properties.TypeStrength = (byte)(val - 1);
                        }
                    }
                    else if (id2 == Prj2Chunks.RoomQuickSandLevel) // DEPRECATED
                    {
                        var val = chunkIO.ReadChunkByte(chunkSize2);
                        if (val > 0)
                        {
                            room.Properties.Type = RoomType.Quicksand;
                            room.Properties.LightEffect = RoomLightEffect.Default;
                            room.Properties.LightEffectStrength = val;
                        }
                    }
                    else if (id2 == Prj2Chunks.RoomMistLevel) // DEPRECATED
                    {
                        var val = chunkIO.ReadChunkByte(chunkSize2);
                        if (val > 0)
                        {
                            room.Properties.LightEffect = RoomLightEffect.Mist;
                            room.Properties.LightEffectStrength = val;
                        }
                    }
                    else if (id2 == Prj2Chunks.RoomReflectionLevel) // DEPRECATED
                    {
                        var val = chunkIO.ReadChunkByte(chunkSize2);

                        // HACK: prioritize mist setting over reflection one.
                        // Comment: this was VERY bad design choice to keep MistLevel/ReflectionLevel separately.
                        // Let's hope people won't be confused on opening their old prj2s.

                        if (val > 0 && room.Properties.LightEffect != RoomLightEffect.Glow)
                        {
                            room.Properties.LightEffect = RoomLightEffect.Reflection;
                            room.Properties.LightEffectStrength = val;
                        }
                    }
                    else if (id2 == Prj2Chunks.RoomType)
                        room.Properties.Type = (RoomType)chunkIO.ReadChunkByte(chunkSize2);
                    else if (id2 == Prj2Chunks.RoomTypeStrength)
                        room.Properties.TypeStrength = chunkIO.ReadChunkByte(chunkSize2);
                    else if (id2 == Prj2Chunks.RoomLightEffect)
                        room.Properties.LightEffect = (RoomLightEffect)chunkIO.ReadChunkByte(chunkSize2);
                    else if (id2 == Prj2Chunks.RoomLightEffectStrength)
                    {
                        room.Properties.LightEffectStrength = (byte)(chunkIO.ReadChunkByte(chunkSize2) + 1);
                        if (room.Properties.LightEffectStrength > 4) room.Properties.LightEffectStrength = 4;
                    }
                    else if (id2 == Prj2Chunks.RoomLightEffectStrength2)
                    {
                        room.Properties.LightEffectStrength = chunkIO.ReadChunkByte(chunkSize2);
                        if (room.Properties.LightEffectStrength > 4) room.Properties.LightEffectStrength = 4;
                    }
                    else if (id2 == Prj2Chunks.RoomReverberation)
                        room.Properties.Reverberation = (Reverberation)chunkIO.ReadChunkByte(chunkSize2);
                    else if (id2 == Prj2Chunks.RoomLocked)
                        room.Properties.Locked = chunkIO.ReadChunkBool(chunkSize2);
                    else if (id2 == Prj2Chunks.RoomHidden)
                        room.Properties.Hidden = chunkIO.ReadChunkBool(chunkSize2);
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
                else if (id3 == Prj2Chunks.ObjectMovable2)
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
                else if (id3 == Prj2Chunks.ObjectMovable3)
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
                    instance.Color = chunkIO.Raw.ReadVector3();
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
                    instance.Ocb = chunkIO.Raw.ReadInt16();
                    addObject(instance);
                }
                else if (id3 == Prj2Chunks.ObjectStatic2)
                {
                    var instance = new StaticInstance();
                    newObjects.TryAdd(objectID, instance);
                    instance.Position = chunkIO.Raw.ReadVector3();
                    instance.RotationY = chunkIO.Raw.ReadSingle();
                    instance.ScriptId = ReadOptionalLEB128Int(chunkIO.Raw);
                    instance.WadObjectId = new Wad.WadStaticId(chunkIO.Raw.ReadUInt32());
                    instance.Color = chunkIO.Raw.ReadVector3();
                    chunkIO.Raw.ReadSingle(); // Unused 32 bit value
                    instance.Ocb = chunkIO.Raw.ReadInt16(); chunkIO.ReadChunks((id4, chunkSize4) =>
                    {
                        if (id4 == Prj2Chunks.ObjectItemLuaId)
                        {
                            instance.LuaId = chunkIO.ReadChunkInt(chunkSize4);
                            return true;
                        }
                        return false;
                    });
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
                else if (id3 == Prj2Chunks.ObjectCamera2)
                {
                    var instance = new CameraInstance();
                    instance.Position = chunkIO.Raw.ReadVector3();
                    instance.ScriptId = ReadOptionalLEB128Int(chunkIO.Raw);
                    instance.Fixed = chunkIO.Raw.ReadBoolean();
                    instance.MoveTimer = chunkIO.Raw.ReadByte();
                    addObject(instance);
                    newObjects.TryAdd(objectID, instance);
                }
                else if (id3 == Prj2Chunks.ObjectSprite)
                {
                    var instance = new SpriteInstance();
                    instance.Position = chunkIO.Raw.ReadVector3();
                    instance.SpriteID = chunkIO.Raw.ReadUInt16();
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
                else if (id3 == Prj2Chunks.ObjectFlyBy2) // Obsolete; LuaScript is unused with new script concept.
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
                    chunkIO.ReadChunks((id4, chunkSize4) =>
                    {
                        if (id4 == Prj2Chunks.ObjectFlyBy2LuaScript)
                        {
                            chunkIO.ReadChunkString(chunkSize4);
                            return true;
                        }
                        return false;
                    });

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
                else if (id3 == Prj2Chunks.ObjectSoundSource  ||
                         id3 == Prj2Chunks.ObjectSoundSource2 ||
                         id3 == Prj2Chunks.ObjectSoundSource3 ||
                         id3 == Prj2Chunks.ObjectSoundSource4 ||
                         id3 == Prj2Chunks.ObjectSoundSourceFinal)
                {
                    var instance = new SoundSourceInstance();
                    instance.Position = chunkIO.Raw.ReadVector3();

                    if (id3 == Prj2Chunks.ObjectSoundSource)
                    {
                        instance.WadReferencedSoundName = TrCatalog.GetOriginalSoundName(TRVersion.Game.TR4, chunkIO.Raw.ReadUInt16());
                        chunkIO.Raw.ReadInt16(); // Unused
                        chunkIO.Raw.ReadByte(); // Unused
                    }
                    else if (id3 == Prj2Chunks.ObjectSoundSource2)
                    {
                        instance.WadReferencedSoundName = chunkIO.Raw.ReadString(); // Used wrong string type
                        chunkIO.Raw.ReadInt16(); // Unused
                        chunkIO.Raw.ReadByte(); // Unused
                    }
                    else if (id3 == Prj2Chunks.ObjectSoundSource4)
                    {
                        instance.SoundId = chunkIO.Raw.ReadInt32();  
                        chunkIO.Raw.ReadInt16(); // Unused
                        chunkIO.Raw.ReadByte(); // Unused
                    }
                    else if (id3 == Prj2Chunks.ObjectSoundSourceFinal)
                    {
                        instance.SoundId = chunkIO.Raw.ReadInt32();
                        instance.PlayMode = (SoundSourcePlayMode)chunkIO.Raw.ReadInt32();
                        chunkIO.Raw.ReadInt16(); // Unused
                        chunkIO.Raw.ReadByte(); // Unused
                    }
                    else
                    {
                        instance.WadReferencedSoundName = chunkIO.Raw.ReadStringUTF8();

                        // Use an embedded sound info
                        long soundInfoId = LEB128.ReadLong(chunkIO.Raw);
                        if (soundInfoId >= 0)
                            instance.EmbeddedSoundInfo = embeddedSoundInfoWad.FixedSoundInfosObsolete[new WadFixedSoundInfoId((uint)soundInfoId)].SoundInfo;
                    }

                    addObject(instance);
                    newObjects.TryAdd(objectID, instance);
                }
                else if (id3 == Prj2Chunks.ObjectLight3 ||
                         id3 == Prj2Chunks.ObjectLight4)
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
                    instance.IsUsedForImportedGeometry = chunkIO.Raw.ReadBoolean();
                    instance.Quality = (LightQuality)chunkIO.Raw.ReadByte();

                    // Remap fog bulb intensity from red color
                    if (id3 == Prj2Chunks.ObjectLight3 && instance.Type == LightType.FogBulb)
                    {
                        instance.Intensity = instance.Color.X;
                        instance.Color = Vector3.One;
                    }

                    addObject(instance);
                    newObjects.TryAdd(objectID, instance);
                }
                else if (id3 == Prj2Chunks.ObjectLight2)
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
                    instance.IsUsedForImportedGeometry = chunkIO.Raw.ReadBoolean();
                    instance.Quality = LightQuality.Default;

                    // Remap fog bulb intensity from red color
                    if (instance.Type == LightType.FogBulb)
                    {
                        instance.Intensity = instance.Color.X;
                        instance.Color = Vector3.One;
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
                    instance.IsUsedForImportedGeometry = instance.IsStaticallyUsed; // Expected behaviour for legacy prj2s
                    instance.Quality = LightQuality.Default;

                    // Remap fog bulb intensity from red color
                    if (instance.Type == LightType.FogBulb)
                    {
                        instance.Intensity = instance.Color.X;
                        instance.Color = Vector3.One;
                    }

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
                else if (id3 == Prj2Chunks.ObjectGhostBlock)
                {
                    int x = LEB128.ReadInt(chunkIO.Raw);
                    int y = LEB128.ReadInt(chunkIO.Raw);

                    var instance = new GhostBlockInstance();
                    instance.SectorPosition = new VectorInt2(x, y);

                    instance.Floor.XnZn   = LEB128.ReadShort(chunkIO.Raw);
                    instance.Floor.XnZp   = LEB128.ReadShort(chunkIO.Raw);
                    instance.Floor.XpZn   = LEB128.ReadShort(chunkIO.Raw);
                    instance.Floor.XpZp   = LEB128.ReadShort(chunkIO.Raw);
                    instance.Ceiling.XnZn = LEB128.ReadShort(chunkIO.Raw);
                    instance.Ceiling.XnZp = LEB128.ReadShort(chunkIO.Raw);
                    instance.Ceiling.XpZn = LEB128.ReadShort(chunkIO.Raw);
                    instance.Ceiling.XpZp = LEB128.ReadShort(chunkIO.Raw);

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
                   
                    instance.CodeBits = (byte)(LEB128.ReadLong(chunkIO.Raw) & 0x1f);
                    instance.OneShot = chunkIO.Raw.ReadBoolean();

                    // NB: it seems that this structure is abandoned now
                    ushort? timer, extra;
                    NG.NgParameterInfo.DecodeNGRealTimer(instance.TargetType, instance.TriggerType,
                        unchecked((ushort)targetObjectId), realTimer, instance.CodeBits, out timer, out extra);
                    instance.Timer = timer == null ? null : new TriggerParameterUshort(timer.Value);
                    instance.Extra = extra == null ? null : new TriggerParameterUshort(extra.Value);

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
                        else
                            return false;
                        return true;
                    });

                    addObject(instance);
                    newObjects.TryAdd(objectID, instance);
                }
                else if (id3 == Prj2Chunks.ObjectImportedGeometry || 
                         id3 == Prj2Chunks.ObjectImportedGeometry2)
                {
                    var instance = new ImportedGeometryInstance();

                    instance.Position = chunkIO.Raw.ReadVector3();
                    instance.SetArbitaryRotationsYX(chunkIO.Raw.ReadSingle(), chunkIO.Raw.ReadSingle());
                    instance.Roll = chunkIO.Raw.ReadSingle();
                    instance.Scale = chunkIO.Raw.ReadSingle();

                    // For some time we accidentally emitted MeshFilter but still emitted the old ObjectImportedGeometry 
                    // chunk name unfortunately. Thus we need to check chunk size too.
                    if (!(id3 == Prj2Chunks.ObjectImportedGeometry && chunkSize3 == 30))
                        chunkIO.Raw.ReadStringUTF8(); // DEPRECATED: MeshFilter

                    instance.Model = levelSettingsIds.ImportedGeometries.TryGetOrDefault(LEB128.ReadLong(chunkIO.Raw));
                    addObject(instance);
                    newObjects.TryAdd(objectID, instance);
                }
                else if (id3 == Prj2Chunks.ObjectImportedGeometry3)
                {
                    var instance = new ImportedGeometryInstance();

                    instance.Position = chunkIO.Raw.ReadVector3();
                    instance.SetArbitaryRotationsYX(chunkIO.Raw.ReadSingle(), chunkIO.Raw.ReadSingle());
                    instance.Roll = chunkIO.Raw.ReadSingle();
                    instance.Scale = chunkIO.Raw.ReadSingle();
                    instance.Model = levelSettingsIds.ImportedGeometries.TryGetOrDefault(LEB128.ReadLong(chunkIO.Raw));

                    chunkIO.ReadChunks((id4, chunkSize4) =>
                    {
                        if (id4 == Prj2Chunks.ObjectImportedGeometryLightingModel)
                            instance.LightingModel = (ImportedGeometryLightingModel)chunkIO.ReadChunkInt(chunkSize4);
                        else if (id4 == Prj2Chunks.ObjectImportedGeometrySharpEdges)
                            instance.SharpEdges = chunkIO.Raw.ReadBoolean();
                        else if (id4 == Prj2Chunks.ObjectImportedGeometryHidden)
                            instance.Hidden = chunkIO.Raw.ReadBoolean();
                        else if (id4 == Prj2Chunks.ObjectImportedGeometryMeshFilter)
                            chunkIO.Raw.ReadStringUTF8(); // DEPRECATED: MeshFilter
                        else
                            return false;
                        return true;
                    });

                    addObject(instance);
                    newObjects.TryAdd(objectID, instance);                    
                }
                else if (id3 == Prj2Chunks.ObjectTriggerVolumeTest)
                {
                    var instanceType = (VolumeShape)chunkIO.Raw.ReadByte();
                    var scripts = new VolumeScriptInstance();

                    VolumeInstance instance;

                    switch (instanceType)
                    {
                        case VolumeShape.Box:
                            {
                                instance = new BoxVolumeInstance();
                                var bv = instance as BoxVolumeInstance;
                                bv.Size = chunkIO.Raw.ReadVector3();
                                bv.RotationY = chunkIO.Raw.ReadSingle();
                                bv.RotationX = chunkIO.Raw.ReadSingle();
                            }
                            break;
                        case VolumeShape.Prism:
                            {
                                instance = new PrismVolumeInstance();
                                var pv = instance as PrismVolumeInstance;
                                pv.Scale = chunkIO.Raw.ReadSingle();
                                pv.RotationY = chunkIO.Raw.ReadSingle();
                            }
                            break;
                        case VolumeShape.Sphere:
                            {
                                instance = new SphereVolumeInstance();
                                var sv = instance as SphereVolumeInstance;
                                sv.Scale = chunkIO.Raw.ReadSingle();
                            }
                            break;
                        default:
                            return false;
                    }

                    instance.Position = chunkIO.Raw.ReadVector3();
                    instance.Activators = (VolumeActivators)chunkIO.Raw.ReadUInt16();
                    scripts.Name = chunkIO.Raw.ReadStringUTF8();
                    scripts.Environment = chunkIO.Raw.ReadStringUTF8();
                    scripts.OnEnter = chunkIO.Raw.ReadStringUTF8();
                    scripts.OnInside = chunkIO.Raw.ReadStringUTF8();
                    scripts.OnLeave = chunkIO.Raw.ReadStringUTF8();

                    instance.Scripts = scripts;

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
