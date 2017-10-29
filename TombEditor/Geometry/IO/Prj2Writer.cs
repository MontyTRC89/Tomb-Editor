using NLog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TombLib.IO;
using TombLib.Utils;

namespace TombEditor.Geometry.IO
{
    public static class Prj2Writer
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        public static void SaveToPrj2(string filename, Level level)
        {
            using (var fileStream = new FileStream(filename, FileMode.Create, FileAccess.Write, FileShare.None))
            using (var chunkIO = new ChunkWriter(Prj2Chunks.MagicNumber, fileStream, ChunkWriter.Compression.None))
                WriteLevel(chunkIO, level);
        }

        private class LevelSettingsIds
        {
            public Dictionary<ImportedGeometry, int> ImportedGeometries { get; } = new Dictionary<ImportedGeometry, int>();
            public Dictionary<LevelTexture, int> LevelTextures { get; } = new Dictionary<LevelTexture, int>();
        };

        private static void WriteLevel(ChunkWriter chunkIO, Level level)
        {
            // Write settings
            LevelSettingsIds levelSettingIds = WriteLevelSettings(chunkIO, level.Settings);

            // Collect rooms to save
            var rooms = new Dictionary<Room, int>();
            for (int i = 0; i < level.Rooms.Length; ++i)
                if (level.Rooms[i] != null)
                    rooms.Add(level.Rooms[i], i);

            // Write rooms
            WriteRooms(chunkIO, rooms, levelSettingIds);

            chunkIO.WriteChunkEnd();
        }

        private static LevelSettingsIds WriteLevelSettings(ChunkWriter chunkIO, LevelSettings settings)
        {
            var levelSettingIds = new LevelSettingsIds();
            chunkIO.WriteChunkWithChildren(Prj2Chunks.Settings, () =>
            {
                chunkIO.WriteChunkString(Prj2Chunks.WadFilePath, settings.WadFilePath ?? "");
                chunkIO.WriteChunkString(Prj2Chunks.FontTextureFilePath, settings.FontTextureFilePath ?? "");
                chunkIO.WriteChunkString(Prj2Chunks.SkyTextureFilePath, settings.SkyTextureFilePath ?? "");
                chunkIO.WriteChunkWithChildren(Prj2Chunks.OldWadSoundPaths, () =>
                {
                    foreach (OldWadSoundPath soundPath in settings.OldWadSoundPaths)
                        chunkIO.WriteChunkWithChildren(Prj2Chunks.OldWadSoundPath, () => chunkIO.WriteChunkString(Prj2Chunks.OldWadSoundPathPath, soundPath.Path));
                });
                chunkIO.WriteChunkString(Prj2Chunks.GameDirectory, settings.GameDirectory ?? "");
                chunkIO.WriteChunkString(Prj2Chunks.GameLevelFilePath, settings.GameLevelFilePath ?? "");
                chunkIO.WriteChunkString(Prj2Chunks.GameExecutableFilePath, settings.GameExecutableFilePath ?? "");
                chunkIO.WriteChunkBool(Prj2Chunks.GameExecutableSuppressAskingForOptions, settings.GameExecutableSuppressAskingForOptions);
                chunkIO.WriteChunkWithChildren(Prj2Chunks.Textures, () =>
                {
                    int index = 0;
                    foreach (LevelTexture texture in settings.Textures)
                    {
                        chunkIO.WriteChunkWithChildren(Prj2Chunks.LevelTexture, () =>
                        {
                            chunkIO.WriteChunkInt(Prj2Chunks.LevelTextureIndex, index);
                            chunkIO.WriteChunkString(Prj2Chunks.LevelTexturePath, texture.Path ?? "");
                            chunkIO.WriteChunkBool(Prj2Chunks.LevelTextureConvert512PixelsToDoubleRows, texture.Convert512PixelsToDoubleRows);
                            chunkIO.WriteChunkBool(Prj2Chunks.LevelTextureReplaceMagentaWithTransparency, texture.ReplaceMagentaWithTransparency);
                            chunkIO.WriteChunk(Prj2Chunks.LevelTextureSounds, () =>
                            {
                                chunkIO.Raw.Write(texture.TextureSoundWidth);
                                chunkIO.Raw.Write(texture.TextureSoundHeight);
                                for (int y = 0; y < texture.TextureSoundHeight; ++y)
                                    for (int x = 0; x < texture.TextureSoundWidth; ++x)
                                        chunkIO.Raw.Write((byte)(texture.GetTextureSound(x, y)));
                            });
                        });
                        levelSettingIds.LevelTextures.Add(texture, index++);
                    };
                }, long.MaxValue);
                chunkIO.WriteChunkWithChildren(Prj2Chunks.ImportedGeometries, () =>
                {
                    int index = 0;
                    foreach (ImportedGeometry importedGeometry in settings.ImportedGeometries)
                    {
                        chunkIO.WriteChunkWithChildren(Prj2Chunks.ImportedGeometry, () =>
                        {
                            chunkIO.WriteChunkInt(Prj2Chunks.ImportedGeometryIndex, index);
                            chunkIO.WriteChunkString(Prj2Chunks.ImportedGeometryName, importedGeometry.Info.Name);
                            chunkIO.WriteChunkString(Prj2Chunks.ImportedGeometryPath, importedGeometry.Info.Path);
                            chunkIO.WriteChunkFloat(Prj2Chunks.ImportedGeometryScale, importedGeometry.Info.Scale);
                        });
                        levelSettingIds.ImportedGeometries.Add(importedGeometry, index++);
                    }
                }, long.MaxValue);
                chunkIO.WriteChunkWithChildren(Prj2Chunks.AnimatedTextureSets, () =>
                {
                    foreach (AnimatedTextureSet set in settings.AnimatedTextureSets)
                        chunkIO.WriteChunkWithChildren(Prj2Chunks.AnimatedTextureSet, () =>
                        {
                            chunkIO.WriteChunkWithChildren(Prj2Chunks.AnimatedTextureFrames, () =>
                            {
                                foreach (AnimatedTextureFrame frame in set.Frames)
                                    chunkIO.WriteChunk(Prj2Chunks.AnimatedTextureFrame, () =>
                                    {
                                        LEB128.Write(chunkIO.Raw, levelSettingIds.LevelTextures[frame.Texture]);
                                        chunkIO.Raw.Write(frame.TexCoord0);
                                        chunkIO.Raw.Write(frame.TexCoord1);
                                        chunkIO.Raw.Write(frame.TexCoord2);
                                        chunkIO.Raw.Write(frame.TexCoord3);
                                    }, 120);
                            });
                        });
                }, long.MaxValue);
            }, long.MaxValue);

            return levelSettingIds;
        }

        private static void WriteRooms(ChunkWriter chunkIO, Dictionary<Room, int> rooms, LevelSettingsIds levelSettingIds)
        {
            // Allocate object indices
            var objectInstanceLookup = new Dictionary<ObjectInstance, int>();
            foreach (Room room in rooms.Keys)
                foreach (ObjectInstance objectInstance in room.AnyObjects)
                    objectInstanceLookup.Add(objectInstance, objectInstanceLookup.Count);

            // Save
            chunkIO.WriteChunkWithChildren(Prj2Chunks.Rooms, () =>
            {
                foreach (Room room in rooms.Keys)
                    chunkIO.WriteChunkWithChildren(Prj2Chunks.Room, () =>
                    {
                        LEB128.Write(chunkIO.Raw, room.NumXSectors);
                        LEB128.Write(chunkIO.Raw, room.NumZSectors);

                        // Write basic properties
                        chunkIO.WriteChunkInt(Prj2Chunks.RoomIndex, rooms.TryGetOrDefault(room, -1));
                        chunkIO.WriteChunkString(Prj2Chunks.RoomName, room.Name);
                        chunkIO.WriteChunkVector3(Prj2Chunks.RoomPosition, room.Position);

                        // Write sectors
                        chunkIO.WriteChunkWithChildren(Prj2Chunks.RoomSectors, () =>
                        {
                            for (int z = 0; z < room.NumZSectors; z++)
                                for (int x = 0; x < room.NumXSectors; x++)
                                    chunkIO.WriteChunkWithChildren(Prj2Chunks.Sector, () =>
                                    {
                                        chunkIO.Raw.Write((int)(x + z * room.NumXSectors));
                                        var b = room.Blocks[x, z];

                                        long combinedFlag = (b.IsAnyWall ? 1L : 0) | (b.ForceFloorSolid ? 2L : 0) | ((long)b.Flags << 2);
                                        chunkIO.WriteChunkInt(Prj2Chunks.SectorProperties, combinedFlag);
                                        chunkIO.WriteChunk(Prj2Chunks.SectorFloor, () =>
                                        {
                                            long flag = (b.FloorSplitDirectionIsXEqualsZ ? 1L : 0) | ((long)b.FloorDiagonalSplit << 1);
                                            LEB128.Write(chunkIO.Raw, flag);
                                            for (int n = 0; n < 4; n++)
                                                LEB128.Write(chunkIO.Raw, b.QAFaces[n]);
                                            for (int n = 0; n < 4; n++)
                                                LEB128.Write(chunkIO.Raw, b.EDFaces[n]);
                                        }, LEB128.MaximumSize1Byte);
                                        chunkIO.WriteChunk(Prj2Chunks.SectorCeiling, () =>
                                        {
                                            long flag = (b.CeilingSplitDirectionIsXEqualsZ ? 1L : 0) | ((long)b.CeilingDiagonalSplit << 1);
                                            LEB128.Write(chunkIO.Raw, flag);
                                            for (int n = 0; n < 4; n++)
                                                LEB128.Write(chunkIO.Raw, b.WSFaces[n]);
                                            for (int n = 0; n < 4; n++)
                                                LEB128.Write(chunkIO.Raw, b.RFFaces[n]);
                                        }, LEB128.MaximumSize1Byte);
                                        for (BlockFace face = 0; face < Block.FaceCount; face++)
                                        {
                                            var texture = b.GetFaceTexture(face);
                                            if (texture.Texture == null)
                                                continue;

                                            if (texture.Texture is LevelTexture)
                                            {
                                                chunkIO.WriteChunk(Prj2Chunks.TextureLevelTexture, () =>
                                                    {
                                                        int textureIndex = levelSettingIds.LevelTextures[(LevelTexture)texture.Texture];

                                                        LEB128.Write(chunkIO.Raw, (long)face);
                                                        chunkIO.Raw.Write(texture.TexCoord0);
                                                        chunkIO.Raw.Write(texture.TexCoord1);
                                                        chunkIO.Raw.Write(texture.TexCoord2);
                                                        chunkIO.Raw.Write(texture.TexCoord3);
                                                        LEB128.Write(chunkIO.Raw, (texture.DoubleSided ? 1L : 0) | ((long)texture.BlendMode << 1));
                                                        LEB128.Write(chunkIO.Raw, textureIndex);
                                                    }, LEB128.MaximumSize1Byte);
                                            }
                                            else if (texture.Texture == TextureInvisible.Instance)
                                                chunkIO.WriteChunkInt(Prj2Chunks.TextureInvisible, (long)face);
                                            else
                                                throw new NotSupportedException("Unsupported texture type " + texture.Texture.GetType().Name);
                                        }
                                    }, LEB128.MaximumSize2Byte);
                        });

                        // Write room properties
                        chunkIO.WriteChunkVector4(Prj2Chunks.RoomAmbientLight, room.AmbientLight);
                        chunkIO.WriteChunkBool(Prj2Chunks.RoomFlagCold, room.FlagCold);
                        chunkIO.WriteChunkBool(Prj2Chunks.RoomFlagDamage, room.FlagDamage);
                        chunkIO.WriteChunkBool(Prj2Chunks.RoomFlagHorizon, room.FlagHorizon);
                        chunkIO.WriteChunkBool(Prj2Chunks.RoomFlagOutside, room.FlagOutside);
                        chunkIO.WriteChunkBool(Prj2Chunks.RoomFlagNoLensflare, room.FlagNoLensflare);
                        chunkIO.WriteChunkBool(Prj2Chunks.RoomFlagRain, room.FlagRain);
                        chunkIO.WriteChunkBool(Prj2Chunks.RoomFlagSnow, room.FlagSnow);
                        chunkIO.WriteChunkBool(Prj2Chunks.RoomFlagQuickSand, room.FlagQuickSand);
                        chunkIO.WriteChunkBool(Prj2Chunks.RoomFlagExcludeFromPathFinding, room.FlagExcludeFromPathFinding);
                        chunkIO.WriteChunkInt(Prj2Chunks.RoomWaterLevel, room.WaterLevel);
                        chunkIO.WriteChunkInt(Prj2Chunks.RoomMistLevel, room.MistLevel);
                        chunkIO.WriteChunkInt(Prj2Chunks.RoomReflectionLevel, room.ReflectionLevel);
                        chunkIO.WriteChunkInt(Prj2Chunks.RoomReverberation, (int)room.Reverberation);
                        if ((room.AlternateRoom != null) && rooms.ContainsKey(room.AlternateRoom))
                            chunkIO.WriteChunkWithChildren(Prj2Chunks.RoomAlternate, () =>
                            {
                                chunkIO.WriteChunkInt(Prj2Chunks.AlternateGroup, room.AlternateGroup);
                                chunkIO.WriteChunkInt(Prj2Chunks.AlternateRoom, rooms[room.AlternateRoom]);
                            }, LEB128.MaximumSize1Byte);

                        // Write room objects
                        chunkIO.WriteChunkWithChildren(Prj2Chunks.Objects, () =>
                        {
                            foreach (var o in room.AnyObjects)
                            {
                                if (o is MoveableInstance)
                                    chunkIO.WriteChunk(Prj2Chunks.ObjectMovable, () =>
                                    {
                                        var instance = (MoveableInstance)o;
                                        LEB128.Write(chunkIO.Raw, objectInstanceLookup.TryGetOrDefault(instance, -1));
                                        chunkIO.Raw.Write(instance.Position);
                                        chunkIO.Raw.Write(instance.RotationY);
                                        LEB128.Write(chunkIO.Raw, instance.ScriptId ?? -1);
                                        chunkIO.Raw.Write(instance.WadObjectId);
                                        chunkIO.Raw.Write(instance.Ocb);
                                        chunkIO.Raw.Write(instance.Invisible);
                                        chunkIO.Raw.Write(instance.ClearBody);
                                        chunkIO.Raw.Write(instance.CodeBits);
                                        chunkIO.Raw.Write(instance.Color);
                                    }, LEB128.MaximumSize1Byte);
                                else if (o is StaticInstance)
                                    chunkIO.WriteChunk(Prj2Chunks.ObjectStatic, () =>
                                    {
                                        var instance = (StaticInstance)o;
                                        LEB128.Write(chunkIO.Raw, objectInstanceLookup.TryGetOrDefault(instance, -1));
                                        chunkIO.Raw.Write(instance.Position);
                                        chunkIO.Raw.Write(instance.RotationY);
                                        LEB128.Write(chunkIO.Raw, instance.ScriptId ?? -1);
                                        chunkIO.Raw.Write(instance.WadObjectId);
                                        chunkIO.Raw.Write(instance.Color);
                                        chunkIO.Raw.Write(instance.Ocb);
                                    }, LEB128.MaximumSize1Byte);
                                else if (o is CameraInstance)
                                    chunkIO.WriteChunk(Prj2Chunks.ObjectCamera, () =>
                                    {
                                        var instance = (CameraInstance)o;
                                        LEB128.Write(chunkIO.Raw, objectInstanceLookup.TryGetOrDefault(instance, -1));
                                        chunkIO.Raw.Write(instance.Position);
                                        LEB128.Write(chunkIO.Raw, instance.ScriptId ?? -1);
                                        chunkIO.Raw.Write(instance.Fixed);
                                    });
                                else if (o is FlybyCameraInstance)
                                    chunkIO.WriteChunk(Prj2Chunks.ObjectFlyBy, () =>
                                    {
                                        var instance = (FlybyCameraInstance)o;
                                        LEB128.Write(chunkIO.Raw, objectInstanceLookup.TryGetOrDefault(instance, -1));
                                        chunkIO.Raw.Write(instance.Position);
                                        chunkIO.Raw.Write(instance.RotationY);
                                        chunkIO.Raw.Write(instance.RotationX);
                                        chunkIO.Raw.Write(instance.Roll);
                                        LEB128.Write(chunkIO.Raw, instance.ScriptId ?? -1);
                                        chunkIO.Raw.Write(instance.Speed);
                                        chunkIO.Raw.Write(instance.Fov);
                                        LEB128.Write(chunkIO.Raw, instance.Flags);
                                        LEB128.Write(chunkIO.Raw, instance.Number);
                                        LEB128.Write(chunkIO.Raw, instance.Sequence);
                                        LEB128.Write(chunkIO.Raw, instance.Timer);
                                    });
                                else if (o is SinkInstance)
                                    chunkIO.WriteChunk(Prj2Chunks.ObjectSink, () =>
                                    {
                                        var instance = (SinkInstance)o;
                                        LEB128.Write(chunkIO.Raw, objectInstanceLookup.TryGetOrDefault(instance, -1));
                                        chunkIO.Raw.Write(instance.Position);
                                        LEB128.Write(chunkIO.Raw, instance.ScriptId ?? -1);
                                        chunkIO.Raw.Write(instance.Strength);
                                    });
                                else if (o is SoundSourceInstance)
                                    chunkIO.WriteChunk(Prj2Chunks.ObjectSoundSource, () =>
                                    {
                                        var instance = (SoundSourceInstance)o;
                                        LEB128.Write(chunkIO.Raw, objectInstanceLookup.TryGetOrDefault(instance, -1));
                                        chunkIO.Raw.Write(instance.Position);
                                        chunkIO.Raw.Write(instance.SoundId);
                                        chunkIO.Raw.Write(instance.Flags);
                                        chunkIO.Raw.Write(instance.CodeBits);
                                    });
                                else if (o is LightInstance)
                                    chunkIO.WriteChunk(Prj2Chunks.ObjectLight, () =>
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
                                    });
                                else if (o is PortalInstance && rooms.ContainsKey(((PortalInstance)o).AdjoiningRoom))
                                    chunkIO.WriteChunk(Prj2Chunks.ObjectPortal, () =>
                                    {
                                        var instance = (PortalInstance)o;
                                        LEB128.Write(chunkIO.Raw, objectInstanceLookup.TryGetOrDefault(instance, -1));
                                        LEB128.Write(chunkIO.Raw, instance.Area.Left);
                                        LEB128.Write(chunkIO.Raw, instance.Area.Top);
                                        LEB128.Write(chunkIO.Raw, instance.Area.Right);
                                        LEB128.Write(chunkIO.Raw, instance.Area.Bottom);
                                        LEB128.Write(chunkIO.Raw, rooms[instance.AdjoiningRoom]);
                                        chunkIO.Raw.Write((byte)instance.Direction);
                                        chunkIO.Raw.Write((byte)instance.Opacity);
                                    });
                                else if (o is TriggerInstance)
                                    chunkIO.WriteChunk(Prj2Chunks.ObjectTrigger, () =>
                                    {
                                        var instance = (TriggerInstance)o;
                                        LEB128.Write(chunkIO.Raw, objectInstanceLookup.TryGetOrDefault(instance, -1));
                                        LEB128.Write(chunkIO.Raw, instance.Area.Left);
                                        LEB128.Write(chunkIO.Raw, instance.Area.Top);
                                        LEB128.Write(chunkIO.Raw, instance.Area.Right);
                                        LEB128.Write(chunkIO.Raw, instance.Area.Bottom);
                                        LEB128.Write(chunkIO.Raw, (long)instance.TriggerType);
                                        LEB128.Write(chunkIO.Raw, (long)instance.TargetType);
                                        LEB128.Write(chunkIO.Raw, instance.TargetData);
                                        LEB128.Write(chunkIO.Raw, instance.TargetObj == null ? -1 : objectInstanceLookup.TryGetOrDefault(instance.TargetObj, -1));
                                        LEB128.Write(chunkIO.Raw, instance.Timer);
                                        LEB128.Write(chunkIO.Raw, instance.CodeBits);
                                        chunkIO.Raw.Write(instance.OneShot);
                                    });
                                else if (o is ImportedGeometryInstance)
                                    chunkIO.WriteChunk(Prj2Chunks.ObjectImportedGeometry, () =>
                                    {
                                        var instance = (ImportedGeometryInstance)o;
                                        LEB128.Write(chunkIO.Raw, objectInstanceLookup.TryGetOrDefault(instance, -1));
                                        chunkIO.Raw.Write(instance.Position);
                                        chunkIO.Raw.Write(instance.RotationY);
                                        chunkIO.Raw.Write(instance.RotationX);
                                        chunkIO.Raw.Write(instance.Roll);
                                        chunkIO.Raw.Write(instance.Scale);
                                        LEB128.Write(chunkIO.Raw, levelSettingIds.ImportedGeometries[instance.Model]);
                                    });
                                else
                                    logger.Warn("Object " + o + " not supported.");
                            }
                        }, long.MaxValue);
                    }, long.MaxValue);
            }, long.MaxValue);
        }
    }
}
