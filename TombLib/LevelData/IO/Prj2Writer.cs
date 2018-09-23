using NLog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TombLib.IO;
using TombLib.Utils;
using TombLib.Wad;

namespace TombLib.LevelData.IO
{
    public static class Prj2Writer
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        public class Filter
        {
            public Predicate<Room> RoomPredicate;
            public bool FilterLevelSettings;
        }

        public static void SaveToPrj2(string filename, Level level, Filter filter = null)
        {
            using (var fileStream = new FileStream(filename, FileMode.Create, FileAccess.Write, FileShare.None))
                SaveToPrj2(fileStream, level, filter);
        }

        public static void SaveToPrj2(Stream stream, Level level, Filter filter = null)
        {
            using (var chunkIO = new ChunkWriter(Prj2Chunks.MagicNumber, stream, ChunkWriter.Compression.None))
            {
                // Index rooms
                var rooms = new Dictionary<Room, int>();
                for (int i = 0; i < level.Rooms.Length; ++i)
                    if (level.Rooms[i] != null && (filter?.RoomPredicate(level.Rooms[i]) ?? true))
                        rooms.Add(level.Rooms[i], i);

                LevelSettings settingsToSave = level.Settings;
                if (filter != null && filter.FilterLevelSettings)
                {
                    settingsToSave = new LevelSettings();
                    var copyInstance = new Room.CopyDependentLevelSettingsArgs(null, settingsToSave, level.Settings, false);
                    foreach (Room room in rooms.Keys)
                        room.CopyDependentLevelSettings(copyInstance);
                }

                // Write settings
                LevelSettingsIds levelSettingIds = WriteLevelSettings(chunkIO, settingsToSave);
                Dictionary<WadSoundInfo, int> soundInfos = WriteEmbeddedSoundInfoWad(chunkIO, rooms.Keys.SelectMany(room => room.Objects));

                // Write rooms
                WriteRooms(chunkIO, rooms, levelSettingIds, soundInfos);
                chunkIO.WriteChunkEnd();
            }
        }

        public static void SaveToPrj2OnlyObjects(Stream stream, Level level, IEnumerable<ObjectInstance> objects)
        {
            using (var chunkIO = new ChunkWriter(Prj2Chunks.MagicNumber, stream, ChunkWriter.Compression.None))
            {
                // Index objects
                var objectInstanceLookup = new Dictionary<ObjectInstance, int>();
                foreach (ObjectInstance objectInstance in objects)
                    objectInstanceLookup.Add(objectInstance, objectInstanceLookup.Count);

                // Write settings
                LevelSettings settingsToSave = new LevelSettings();
                Room.CopyDependentLevelSettingsArgs copyInstance = new Room.CopyDependentLevelSettingsArgs(null, settingsToSave, level.Settings, false);
                foreach (ObjectInstance instance in objects)
                    instance.CopyDependentLevelSettings(copyInstance);
                LevelSettingsIds levelSettingIds = WriteLevelSettings(chunkIO, settingsToSave);
                Dictionary<WadSoundInfo, int> soundInfos = WriteEmbeddedSoundInfoWad(chunkIO, objects);

                // Write objects
                WriteObjects(chunkIO, objects, new Dictionary<Room, int>(), levelSettingIds, objectInstanceLookup, soundInfos);
                chunkIO.WriteChunkEnd();
            }
        }

        private class LevelSettingsIds
        {
            public Dictionary<ImportedGeometry, int> ImportedGeometries { get; } = new Dictionary<ImportedGeometry, int>(new ReferenceEqualityComparer<ImportedGeometry>());
            public Dictionary<LevelTexture, int> LevelTextures { get; } = new Dictionary<LevelTexture, int>(new ReferenceEqualityComparer<LevelTexture>());
        }

        private static LevelSettingsIds WriteLevelSettings(ChunkWriter chunkIO, LevelSettings settings)
        {
            var levelSettingIds = new LevelSettingsIds();
            using (var chunkSettings = chunkIO.WriteChunk(Prj2Chunks.Settings, long.MaxValue))
            {
                chunkIO.WriteChunkString(Prj2Chunks.FontTextureFilePath, settings.FontTextureFilePath ?? "");
                chunkIO.WriteChunkString(Prj2Chunks.SkyTextureFilePath, settings.SkyTextureFilePath ?? "");
                chunkIO.WriteChunkString(Prj2Chunks.Tr5ExtraSpritesFilePath, settings.Tr5ExtraSpritesFilePath ?? "");
                using (var chunkOldWadSoundPaths = chunkIO.WriteChunk(Prj2Chunks.OldWadSoundPaths))
                {
                    chunkIO.WriteChunkEmpty(Prj2Chunks.OldWadSoundUpdateTag1_0_8);
                    foreach (OldWadSoundPath soundPath in settings.OldWadSoundPaths)
                        using (var chunkOldWadSoundPath = chunkIO.WriteChunk(Prj2Chunks.OldWadSoundPath))
                        {
                            chunkIO.WriteChunkString(Prj2Chunks.OldWadSoundPathPath, soundPath.Path);
                            chunkIO.WriteChunkEnd();
                        }
                    chunkIO.WriteChunkEnd();
                }
                chunkIO.WriteChunkString(Prj2Chunks.GameDirectory, settings.GameDirectory ?? "");
                chunkIO.WriteChunkString(Prj2Chunks.GameLevelFilePath, settings.GameLevelFilePath ?? "");
                chunkIO.WriteChunkString(Prj2Chunks.GameExecutableFilePath, settings.GameExecutableFilePath ?? "");
                chunkIO.WriteChunkBool(Prj2Chunks.GameEnableQuickStartFeature, settings.GameEnableQuickStartFeature);
                chunkIO.WriteChunkInt(Prj2Chunks.GameVersion, (long)settings.GameVersion);
                chunkIO.WriteChunkInt(Prj2Chunks.Tr5LaraType, (long)settings.Tr5LaraType);
                chunkIO.WriteChunkInt(Prj2Chunks.Tr5Weather, (long)settings.Tr5WeatherType);
                chunkIO.WriteChunkVector3(Prj2Chunks.DefaultAmbientLight, settings.DefaultAmbientLight);
                chunkIO.WriteChunkString(Prj2Chunks.ScriptDirectory, settings.ScriptDirectory ?? "");
                using (var chunkWads = chunkIO.WriteChunk(Prj2Chunks.Wads, long.MaxValue))
                {
                    foreach (ReferencedWad wad in settings.Wads)
                        using (var chunkWad = chunkIO.WriteChunk(Prj2Chunks.Wad))
                        {
                            chunkIO.WriteChunkString(Prj2Chunks.WadPath, wad.Path ?? "");
                            chunkIO.WriteChunkEnd();
                        }
                    chunkIO.WriteChunkEnd();
                }
                using (var chunkTextures = chunkIO.WriteChunk(Prj2Chunks.Textures, long.MaxValue))
                {
                    int index = 0;
                    foreach (LevelTexture texture in settings.Textures)
                    {
                        using (var chunkLevelTexture = chunkIO.WriteChunk(Prj2Chunks.LevelTexture))
                        {
                            chunkIO.WriteChunkInt(Prj2Chunks.LevelTextureIndex, index);
                            chunkIO.WriteChunkString(Prj2Chunks.LevelTexturePath, texture.Path ?? "");
                            chunkIO.WriteChunkBool(Prj2Chunks.LevelTextureConvert512PixelsToDoubleRows, texture.Convert512PixelsToDoubleRows);
                            chunkIO.WriteChunkBool(Prj2Chunks.LevelTextureReplaceMagentaWithTransparency, texture.ReplaceMagentaWithTransparency);
                            using (var chunkLevelTextureFootStepSounds = chunkIO.WriteChunk(Prj2Chunks.LevelTextureFootStepSounds))
                            {
                                chunkIO.Raw.Write(texture.FootStepSoundWidth);
                                chunkIO.Raw.Write(texture.FootStepSoundHeight);
                                for (int y = 0; y < texture.FootStepSoundHeight; ++y)
                                    for (int x = 0; x < texture.FootStepSoundWidth; ++x)
                                        chunkIO.Raw.Write((byte)texture.GetFootStepSound(x, y));
                            }
                            chunkIO.WriteChunkEnd();
                        }
                        levelSettingIds.LevelTextures.Add(texture, index++);
                    }
                    chunkIO.WriteChunkEnd();
                }
                using (var chunkImportedGeometries = chunkIO.WriteChunk(Prj2Chunks.ImportedGeometries, long.MaxValue))
                {
                    int index = 0;
                    foreach (ImportedGeometry importedGeometry in settings.ImportedGeometries)
                    {
                        using (var chunkImportedGeometry = chunkIO.WriteChunk(Prj2Chunks.ImportedGeometry))
                        {
                            chunkIO.WriteChunkInt(Prj2Chunks.ImportedGeometryIndex, index);
                            chunkIO.WriteChunkString(Prj2Chunks.ImportedGeometryName, importedGeometry.Info.Name);
                            chunkIO.WriteChunkString(Prj2Chunks.ImportedGeometryPath, importedGeometry.Info.Path);
                            chunkIO.WriteChunkFloat(Prj2Chunks.ImportedGeometryScale, importedGeometry.Info.Scale);
                            chunkIO.WriteChunkInt(Prj2Chunks.ImportedGeometryPosAxisFlags,
                                (importedGeometry.Info.SwapXY ? 1 : 0) |
                                (importedGeometry.Info.SwapXZ ? 2 : 0) |
                                (importedGeometry.Info.SwapYZ ? 4 : 0) |
                                (importedGeometry.Info.FlipX ? 8 : 0) |
                                (importedGeometry.Info.FlipY ? 16 : 0) |
                                (importedGeometry.Info.FlipZ ? 32 : 0));
                            chunkIO.WriteChunkInt(Prj2Chunks.ImportedGeometryTexAxisFlags, importedGeometry.Info.FlipUV_V ? 4 : 0);
                            chunkIO.WriteChunkBool(Prj2Chunks.ImportedGeometryInvertFaces, importedGeometry.Info.InvertFaces);
                            chunkIO.WriteChunkEnd();
                        }
                        levelSettingIds.ImportedGeometries.Add(importedGeometry, index++);
                    }
                    chunkIO.WriteChunkEnd();
                }
                using (var chunkAnimatedTextureSets = chunkIO.WriteChunk(Prj2Chunks.AnimatedTextureSets, long.MaxValue))
                {
                    foreach (AnimatedTextureSet set in settings.AnimatedTextureSets)
                        using (var chunkAnimatedTextureSet = chunkIO.WriteChunk(Prj2Chunks.AnimatedTextureSet))
                        {
                            chunkIO.WriteChunkString(Prj2Chunks.AnimatedTextureSetName, set.Name ?? "");
                            chunkIO.WriteChunkInt(Prj2Chunks.AnimatedTextureSetType, (int)set.AnimationType);
                            chunkIO.WriteChunkFloat(Prj2Chunks.AnimatedTextureSetFps, set.Fps);
                            chunkIO.WriteChunkInt(Prj2Chunks.AnimatedTextureSetUvRotate, set.UvRotate);
                            using (var chunkAnimatedTextureFrames = chunkIO.WriteChunk(Prj2Chunks.AnimatedTextureFrames))
                            {
                                foreach (AnimatedTextureFrame frame in set.Frames)
                                    using (var chunkAnimatedTextureFrame = chunkIO.WriteChunk(Prj2Chunks.AnimatedTextureFrame, 120))
                                    {
                                        LEB128.Write(chunkIO.Raw, levelSettingIds.LevelTextures[frame.Texture]);
                                        chunkIO.Raw.Write(frame.TexCoord0);
                                        chunkIO.Raw.Write(frame.TexCoord1);
                                        chunkIO.Raw.Write(frame.TexCoord2);
                                        chunkIO.Raw.Write(frame.TexCoord3);
                                        LEB128.Write(chunkIO.Raw, frame.Repeat);
                                    }

                                chunkIO.WriteChunkEnd();
                            }
                            chunkIO.WriteChunkEnd();
                        }
                    chunkIO.WriteChunkEnd();
                }
                chunkIO.WriteChunkEnd();
            };

            return levelSettingIds;
        }

        private static Dictionary<WadSoundInfo, int> WriteEmbeddedSoundInfoWad(ChunkWriter chunkIO, IEnumerable<ObjectInstance> objects)
        {
            // Collect embedded sound infos
            var soundInfoList = new List<WadSoundInfo>();
            var SoundInfos = new Dictionary<WadSoundInfo, int>();
            foreach (SoundSourceInstance soundSource in objects.OfType<SoundSourceInstance>())
                if (soundSource.EmbeddedSoundInfo != null)
                    if (!SoundInfos.ContainsKey(soundSource.EmbeddedSoundInfo))
                    {
                        SoundInfos.Add(soundSource.EmbeddedSoundInfo, soundInfoList.Count);
                        soundInfoList.Add(soundSource.EmbeddedSoundInfo);
                    }

            // Write embedded sound info wad
            if (soundInfoList.Count > 0)
            {
                Wad2 tempEmbeddedWad = new Wad2();
                for (int i = 0; i < soundInfoList.Count; ++i)
                {
                    var id = new WadFixedSoundInfoId((uint)i);
                    tempEmbeddedWad.FixedSoundInfos.Add(id, new WadFixedSoundInfo(id) { SoundInfo = soundInfoList[i] });
                }
                using (var chunk = chunkIO.WriteChunk(Prj2Chunks.EmbeddedSoundInfoWad, long.MaxValue))
                    Wad2Writer.SaveToBinaryWriterFast(tempEmbeddedWad, chunkIO.Raw);
            }

            return SoundInfos;
        }


        private static void WriteRooms(ChunkWriter chunkIO, Dictionary<Room, int> rooms, LevelSettingsIds levelSettingIds, Dictionary<WadSoundInfo, int> soundInfos)
        {
            // Allocate object indices
            var objectInstanceLookup = new Dictionary<ObjectInstance, int>();
            foreach (Room room in rooms.Keys)
                foreach (ObjectInstance objectInstance in room.AnyObjects)
                    objectInstanceLookup.Add(objectInstance, objectInstanceLookup.Count);

            // Save
            using (var chunkRooms = chunkIO.WriteChunk(Prj2Chunks.Rooms, long.MaxValue))
            {
                foreach (Room room in rooms.Keys)
                    using (var chunkRoom = chunkIO.WriteChunk(Prj2Chunks.Room, long.MaxValue))
                    {
                        LEB128.Write(chunkIO.Raw, room.NumXSectors);
                        LEB128.Write(chunkIO.Raw, room.NumZSectors);

                        // Write basic properties
                        chunkIO.WriteChunkInt(Prj2Chunks.RoomIndex, rooms.TryGetOrDefault(room, -1));
                        chunkIO.WriteChunkString(Prj2Chunks.RoomName, room.Name);
                        chunkIO.WriteChunkVector3(Prj2Chunks.RoomPosition, room.Position);

                        // Write sectors
                        using (var chunkRoomSectors = chunkIO.WriteChunk(Prj2Chunks.RoomSectors))
                        {
                            for (int z = 0; z < room.NumZSectors; z++)
                                for (int x = 0; x < room.NumXSectors; x++)
                                    using (var chunkSector = chunkIO.WriteChunk(Prj2Chunks.Sector, LEB128.MaximumSize2Byte))
                                    {
                                        chunkIO.Raw.Write(x + z * room.NumXSectors);
                                        var b = room.Blocks[x, z];

                                        long combinedFlag = (b.IsAnyWall ? 1L : 0) | (b.ForceFloorSolid ? 2L : 0) | ((long)b.Flags << 2);
                                        chunkIO.WriteChunkInt(Prj2Chunks.SectorProperties, combinedFlag);
                                        using (var chunkSectorFloor = chunkIO.WriteChunk(Prj2Chunks.SectorFloor, LEB128.MaximumSize1Byte))
                                        {
                                            long flag = (b.Floor.SplitDirectionIsXEqualsZ ? 1L : 0) | ((long)b.Floor.DiagonalSplit << 1);
                                            LEB128.Write(chunkIO.Raw, flag);
                                            for (BlockEdge edge = 0; edge < BlockEdge.Count; ++edge)
                                                LEB128.Write(chunkIO.Raw, b.Floor.GetHeight(edge));
                                            for (BlockEdge edge = 0; edge < BlockEdge.Count; ++edge)
                                                LEB128.Write(chunkIO.Raw, b.GetHeight(BlockVertical.Ed, edge));
                                        }
                                        using (var chunkSectorCeiling = chunkIO.WriteChunk(Prj2Chunks.SectorCeiling, LEB128.MaximumSize1Byte))
                                        {
                                            long flag = (b.Ceiling.SplitDirectionIsXEqualsZ ? 1L : 0) | ((long)b.Ceiling.DiagonalSplit << 1);
                                            LEB128.Write(chunkIO.Raw, flag);
                                            for (BlockEdge edge = 0; edge < BlockEdge.Count; ++edge)
                                                LEB128.Write(chunkIO.Raw, b.Ceiling.GetHeight(edge));
                                            for (BlockEdge edge = 0; edge < BlockEdge.Count; ++edge)
                                                LEB128.Write(chunkIO.Raw, b.GetHeight(BlockVertical.Rf, edge));
                                        }
                                        for (BlockFace face = 0; face < BlockFace.Count; face++)
                                        {
                                            var texture = b.GetFaceTexture(face);
                                            if (texture.Texture == null)
                                                continue;

                                            if (texture.Texture is LevelTexture)
                                                using (var chunkTextureLevelTexture = chunkIO.WriteChunk(Prj2Chunks.TextureLevelTexture, LEB128.MaximumSize1Byte))
                                                {
                                                    int textureIndex = levelSettingIds.LevelTextures[(LevelTexture)texture.Texture];

                                                    LEB128.Write(chunkIO.Raw, (long)face);
                                                    chunkIO.Raw.Write(texture.TexCoord0);
                                                    chunkIO.Raw.Write(texture.TexCoord1);
                                                    chunkIO.Raw.Write(texture.TexCoord2);
                                                    chunkIO.Raw.Write(texture.TexCoord3);
                                                    LEB128.Write(chunkIO.Raw, (texture.DoubleSided ? 1L : 0) | ((long)texture.BlendMode << 1));
                                                    LEB128.Write(chunkIO.Raw, textureIndex);
                                                }
                                            else if (texture.Texture == TextureInvisible.Instance)
                                                chunkIO.WriteChunkInt(Prj2Chunks.TextureInvisible, (long)face);
                                            else
                                                throw new NotSupportedException("Unsupported texture type " + texture.Texture.GetType().Name);
                                        }
                                        chunkIO.WriteChunkEnd();
                                    }
                            chunkIO.WriteChunkEnd();
                        }

                        // Write room properties
                        chunkIO.WriteChunkVector3(Prj2Chunks.RoomAmbientLight, room.AmbientLight);
                        chunkIO.WriteChunkBool(Prj2Chunks.RoomFlagCold, room.FlagCold);
                        chunkIO.WriteChunkBool(Prj2Chunks.RoomFlagDamage, room.FlagDamage);
                        chunkIO.WriteChunkBool(Prj2Chunks.RoomFlagHorizon, room.FlagHorizon);
                        chunkIO.WriteChunkBool(Prj2Chunks.RoomFlagOutside, room.FlagOutside);
                        chunkIO.WriteChunkBool(Prj2Chunks.RoomFlagNoLensflare, room.FlagNoLensflare);
                        chunkIO.WriteChunkBool(Prj2Chunks.RoomFlagExcludeFromPathFinding, room.FlagExcludeFromPathFinding);
                        chunkIO.WriteChunkInt(Prj2Chunks.RoomWaterLevel, room.WaterLevel);
                        chunkIO.WriteChunkInt(Prj2Chunks.RoomMistLevel, room.MistLevel);
                        chunkIO.WriteChunkInt(Prj2Chunks.RoomReflectionLevel, room.ReflectionLevel);
                        chunkIO.WriteChunkInt(Prj2Chunks.RoomReverberation, (int)room.Reverberation);
                        chunkIO.WriteChunkBool(Prj2Chunks.RoomLocked, room.Locked);
                        chunkIO.WriteChunkInt(Prj2Chunks.RoomRainLevel, room.RainLevel);
                        chunkIO.WriteChunkInt(Prj2Chunks.RoomSnowLevel, room.SnowLevel);
                        chunkIO.WriteChunkInt(Prj2Chunks.RoomQuickSandLevel, room.QuickSandLevel);
                        if (room.AlternateRoom != null && rooms.ContainsKey(room.AlternateRoom))
                            using (var chunkRoomAlternate = chunkIO.WriteChunk(Prj2Chunks.RoomAlternate, LEB128.MaximumSize1Byte))
                            {
                                chunkIO.WriteChunkInt(Prj2Chunks.AlternateGroup, room.AlternateGroup);
                                chunkIO.WriteChunkInt(Prj2Chunks.AlternateRoom, rooms[room.AlternateRoom]);
                                chunkIO.WriteChunkEnd();
                            }

                        // Write room objects
                        WriteObjects(chunkIO, room.AnyObjects, rooms, levelSettingIds, objectInstanceLookup, soundInfos);
                        chunkIO.WriteChunkEnd();
                    }
                chunkIO.WriteChunkEnd();
            }
        }

        private static void WriteObjects(ChunkWriter chunkIO, IEnumerable<ObjectInstance> objects,
            Dictionary<Room, int> rooms, LevelSettingsIds levelSettingIds,
            Dictionary<ObjectInstance, int> objectInstanceLookup, Dictionary<WadSoundInfo, int> soundInfos)
        {
            using (var chunkObjects = chunkIO.WriteChunk(Prj2Chunks.Objects, long.MaxValue))
            {
                foreach (var o in objects)
                {
                    if (o is MoveableInstance)
                        using (var chunk = chunkIO.WriteChunk(Prj2Chunks.ObjectMovable, LEB128.MaximumSize1Byte))
                        {
                            var instance = (MoveableInstance)o;
                            LEB128.Write(chunkIO.Raw, objectInstanceLookup.TryGetOrDefault(instance, -1));
                            chunkIO.Raw.Write(instance.Position);
                            chunkIO.Raw.Write(instance.RotationY);
                            LEB128.Write(chunkIO.Raw, (long?)instance.ScriptId ?? -1);
                            chunkIO.Raw.Write(instance.WadObjectId.TypeId);
                            chunkIO.Raw.Write(instance.Ocb);
                            chunkIO.Raw.Write(instance.Invisible);
                            chunkIO.Raw.Write(instance.ClearBody);
                            chunkIO.Raw.Write(instance.CodeBits);
                        }
                    else if (o is StaticInstance)
                        using (var chunk = chunkIO.WriteChunk(Prj2Chunks.ObjectStatic, LEB128.MaximumSize1Byte))
                        {
                            var instance = (StaticInstance)o;
                            LEB128.Write(chunkIO.Raw, objectInstanceLookup.TryGetOrDefault(instance, -1));
                            chunkIO.Raw.Write(instance.Position);
                            chunkIO.Raw.Write(instance.RotationY);
                            LEB128.Write(chunkIO.Raw, (long?)instance.ScriptId ?? -1);
                            chunkIO.Raw.Write(instance.WadObjectId.TypeId);
                            chunkIO.Raw.Write(instance.Color);
                            chunkIO.Raw.Write((int)0); // Unused 32 bit value
                            chunkIO.Raw.Write(instance.Ocb);
                        }
                    else if (o is CameraInstance)
                        using (var chunk = chunkIO.WriteChunk(Prj2Chunks.ObjectCamera, LEB128.MaximumSize1Byte))
                        {
                            var instance = (CameraInstance)o;
                            LEB128.Write(chunkIO.Raw, objectInstanceLookup.TryGetOrDefault(instance, -1));
                            chunkIO.Raw.Write(instance.Position);
                            LEB128.Write(chunkIO.Raw, (long?)instance.ScriptId ?? -1);
                            chunkIO.Raw.Write(instance.Fixed);
                        }
                    else if (o is FlybyCameraInstance)
                        using (var chunk = chunkIO.WriteChunk(Prj2Chunks.ObjectFlyBy, LEB128.MaximumSize2Byte))
                        {
                            var instance = (FlybyCameraInstance)o;
                            LEB128.Write(chunkIO.Raw, objectInstanceLookup.TryGetOrDefault(instance, -1));
                            chunkIO.Raw.Write(instance.Position);
                            chunkIO.Raw.Write(instance.RotationY);
                            chunkIO.Raw.Write(instance.RotationX);
                            chunkIO.Raw.Write(instance.Roll);
                            LEB128.Write(chunkIO.Raw, (long?)instance.ScriptId ?? -1);
                            chunkIO.Raw.Write(instance.Speed);
                            chunkIO.Raw.Write(instance.Fov);
                            LEB128.Write(chunkIO.Raw, instance.Flags);
                            LEB128.Write(chunkIO.Raw, instance.Number);
                            LEB128.Write(chunkIO.Raw, instance.Sequence);
                            LEB128.Write(chunkIO.Raw, instance.Timer);
                        }
                    else if (o is SinkInstance)
                        using (var chunk = chunkIO.WriteChunk(Prj2Chunks.ObjectSink, LEB128.MaximumSize1Byte))
                        {
                            var instance = (SinkInstance)o;
                            LEB128.Write(chunkIO.Raw, objectInstanceLookup.TryGetOrDefault(instance, -1));
                            chunkIO.Raw.Write(instance.Position);
                            LEB128.Write(chunkIO.Raw, (long?)instance.ScriptId ?? -1);
                            chunkIO.Raw.Write(instance.Strength);
                        }
                    else if (o is SoundSourceInstance)
                        using (var chunk = chunkIO.WriteChunk(Prj2Chunks.ObjectSoundSource3, LEB128.MaximumSize1Byte))
                        {
                            var instance = (SoundSourceInstance)o;
                            LEB128.Write(chunkIO.Raw, objectInstanceLookup.TryGetOrDefault(instance, -1));
                            chunkIO.Raw.Write(instance.Position);
                            chunkIO.Raw.WriteStringUTF8(instance.WadReferencedSoundName ?? "");
                            LEB128.Write(chunkIO.Raw, instance.EmbeddedSoundInfo == null ? -1 : soundInfos[instance.EmbeddedSoundInfo]);
                        }
                    else if (o is LightInstance)
                        using (var chunk = chunkIO.WriteChunk(Prj2Chunks.ObjectLight, LEB128.MaximumSize2Byte))
                        {
                            var instance = (LightInstance)o;
                            LEB128.Write(chunkIO.Raw, objectInstanceLookup.TryGetOrDefault(instance, -1));
                            LEB128.Write(chunkIO.Raw, (long)instance.Type);
                            chunkIO.Raw.Write(instance.Position);
                            chunkIO.Raw.Write(instance.RotationY);
                            chunkIO.Raw.Write(instance.RotationX);
                            chunkIO.Raw.Write(instance.Intensity);
                            chunkIO.Raw.Write(instance.Color);
                            chunkIO.Raw.Write(instance.InnerRange);
                            chunkIO.Raw.Write(instance.OuterRange);
                            chunkIO.Raw.Write(instance.InnerAngle);
                            chunkIO.Raw.Write(instance.OuterAngle);
                            chunkIO.Raw.Write(instance.Enabled);
                            chunkIO.Raw.Write(instance.IsObstructedByRoomGeometry);
                            chunkIO.Raw.Write(instance.IsDynamicallyUsed);
                            chunkIO.Raw.Write(instance.IsStaticallyUsed);
                        }
                    else if (o is PortalInstance && rooms.ContainsKey(((PortalInstance)o).AdjoiningRoom))
                        using (var chunk = chunkIO.WriteChunk(Prj2Chunks.ObjectPortal, LEB128.MaximumSize2Byte))
                        {
                            var instance = (PortalInstance)o;
                            LEB128.Write(chunkIO.Raw, objectInstanceLookup.TryGetOrDefault(instance, -1));
                            LEB128.Write(chunkIO.Raw, instance.Area.X0);
                            LEB128.Write(chunkIO.Raw, instance.Area.Y0);
                            LEB128.Write(chunkIO.Raw, instance.Area.X1);
                            LEB128.Write(chunkIO.Raw, instance.Area.Y1);
                            LEB128.Write(chunkIO.Raw, rooms[instance.AdjoiningRoom]);
                            chunkIO.Raw.Write((byte)instance.Direction);
                            chunkIO.Raw.Write((byte)instance.Opacity);
                        }
                    else if (o is TriggerInstance)
                        using (var chunk = chunkIO.WriteChunk(Prj2Chunks.ObjectTrigger2, LEB128.MaximumSize2Byte))
                        {
                            Action<ITriggerParameter> writeTriggerParameter = (ITriggerParameter parameter) =>
                            {
                                if (parameter is TriggerParameterUshort)
                                {
                                    LEB128.Write(chunkIO.Raw, 0);
                                    LEB128.Write(chunkIO.Raw, ((TriggerParameterUshort)parameter).Key);
                                }
                                else if (parameter is ObjectInstance)
                                {
                                    LEB128.Write(chunkIO.Raw, 1);
                                    LEB128.Write(chunkIO.Raw, objectInstanceLookup.TryGetOrDefault((ObjectInstance)parameter));
                                }
                                else if (parameter is Room)
                                {
                                    LEB128.Write(chunkIO.Raw, 2);
                                    LEB128.Write(chunkIO.Raw, rooms.TryGetOrDefault((Room)parameter));
                                }
                                else
                                {
                                    LEB128.Write(chunkIO.Raw, -1);
                                };
                            };

                            var instance = (TriggerInstance)o;
                            LEB128.Write(chunkIO.Raw, objectInstanceLookup.TryGetOrDefault(instance, -1));
                            LEB128.Write(chunkIO.Raw, instance.Area.X0);
                            LEB128.Write(chunkIO.Raw, instance.Area.Y0);
                            LEB128.Write(chunkIO.Raw, instance.Area.X1);
                            LEB128.Write(chunkIO.Raw, instance.Area.Y1);
                            chunkIO.WriteChunkInt(Prj2Chunks.ObjectTrigger2Type, (long)instance.TriggerType);
                            chunkIO.WriteChunkInt(Prj2Chunks.ObjectTrigger2TargetType, (long)instance.TargetType);
                            chunkIO.WriteChunk(Prj2Chunks.ObjectTrigger2Target, () => writeTriggerParameter(instance.Target), 32);
                            chunkIO.WriteChunk(Prj2Chunks.ObjectTrigger2Timer, () => writeTriggerParameter(instance.Timer), 32);
                            chunkIO.WriteChunk(Prj2Chunks.ObjectTrigger2Extra, () => writeTriggerParameter(instance.Extra), 32);
                            chunkIO.WriteChunkInt(Prj2Chunks.ObjectTrigger2CodeBits, instance.CodeBits);
                            chunkIO.WriteChunkBool(Prj2Chunks.ObjectTrigger2OneShot, instance.OneShot);
                            chunkIO.WriteChunkEnd();
                        }
                    else if (o is ImportedGeometryInstance)
                        using (var chunk = chunkIO.WriteChunk(Prj2Chunks.ObjectImportedGeometry2, LEB128.MaximumSize2Byte))
                        {
                            var instance = (ImportedGeometryInstance)o;
                            LEB128.Write(chunkIO.Raw, objectInstanceLookup.TryGetOrDefault(instance, -1));
                            chunkIO.Raw.Write(instance.Position);
                            chunkIO.Raw.Write(instance.RotationY);
                            chunkIO.Raw.Write(instance.RotationX);
                            chunkIO.Raw.Write(instance.Roll);
                            chunkIO.Raw.Write(instance.Scale);
                            chunkIO.Raw.WriteStringUTF8(instance.MeshFilter);
                            LEB128.Write(chunkIO.Raw, instance.Model == null ? -1 : levelSettingIds.ImportedGeometries[instance.Model]);
                        }
                    else
                        logger.Warn("Object " + o + " not supported.");
                }
                chunkIO.WriteChunkEnd();
            };
        }
    }
}
