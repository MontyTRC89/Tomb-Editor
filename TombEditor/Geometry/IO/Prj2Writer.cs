using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using NLog;
using TombLib.IO;
using TombLib.Graphics;

namespace TombEditor.Geometry.IO
{
    public enum Prj2ChunkType : ushort
    {
        NoExtraChunk = 0xcdcd
    }

    public enum Prj2ObjectType : ushort
    {
        Moveable,
        Static,
        Camera,
        FlybyCamera,
        Sink,
        SoundSource,
        RoomGeometry,
        Light
    }

    public enum Prj2FaceTextureMode : ushort
    {
        NoTexture,
        Texture,
        InvisibleColor
    }

    public class Prj2Writer
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        public static bool SaveToPrj2(string filename, Level level)
        {
            const byte filler8 = 0;
            const short filler16 = 0;
            const int filler32 = 0;

            ushort chunkMagicWord;

            // First collect all shared lists so we can save references as indices
            var portalsList = new List<Portal>();
            var triggersList = new List<TriggerInstance>();
            var objectsList = new List<ObjectInstance>();
            var modelsList = new List<string>();

            foreach(var room in level.Rooms)
            {
                if (room == null) continue;

                foreach (var trigger in room.Triggers)
                    if (!triggersList.Contains(trigger))
                        triggersList.Add(trigger);

                foreach (var portal in room.Portals)
                    if (!portalsList.Contains(portal))
                        portalsList.Add(portal);

                foreach (var obj in room.Objects)
                    if (!objectsList.Contains(obj) && 
                        obj.GetType() != typeof(Light) && 
                        obj.GetType() != typeof(RoomGeometryInstance))
                        objectsList.Add(obj);
            }

            for (int m = 0; m < GeometryImporterExporter.Models.Count; m++)
                modelsList.Add(GeometryImporterExporter.Models.ElementAt(m).Key);

            try
            {
                var ms = new MemoryStream();
                byte[] projectData;
                using (var writer = new BinaryWriterEx(ms))
                {
                    // Write version
                    var version = new byte[] { 0x50, 0x52, 0x4A, 0x32 };
                    writer.Write(version);

                    const int versionCode = 2;
                    writer.Write(versionCode);

                    // Write settings
                    writer.WriteStringUTF8(level.Settings.TextureFilePath ?? "");
                    writer.WriteStringUTF8(level.Settings.WadFilePath ?? "");
                    writer.WriteStringUTF8(level.Settings.FontTextureFilePath ?? "");
                    writer.WriteStringUTF8(level.Settings.SkyTextureFilePath ?? "");
                    writer.WriteStringUTF8(level.Settings.GameDirectory ?? "");
                    writer.WriteStringUTF8(level.Settings.GameLevelFilePath ?? "");
                    writer.WriteStringUTF8(level.Settings.GameExecutableFilePath ?? "");
                    writer.Write(level.Settings.SoundPaths.Count);
                    foreach (SoundPath path in level.Settings.SoundPaths)
                        writer.WriteStringUTF8(path.Path ?? "");
                    writer.Write(level.Settings.IgnoreMissingSounds);

                    writer.WriteFiller(0x00, 16);

                    // Write imported references
                    uint numModels = (uint)GeometryImporterExporter.Models.Count;
                    writer.Write(numModels);
                    for (int i = 0; i < numModels; i++)
                    {
                        writer.WriteStringUTF8(GeometryImporterExporter.Models.ElementAt(i).Key);
                        writer.Write(GeometryImporterExporter.Models.ElementAt(i).Value.Scale);
                    }

                    // Write portals
                    writer.Write(portalsList.Count);
                    foreach (var p in portalsList)
                    {
                        writer.Write((ushort)level.Rooms.ReferenceIndexOf(p.Room));
                        writer.Write((ushort)level.Rooms.ReferenceIndexOf(p.AdjoiningRoom));
                        writer.Write((ushort)p.Direction);
                        writer.Write(p.Area.Left);
                        writer.Write(p.Area.Top);
                        writer.Write(p.Area.Right);
                        writer.Write(p.Area.Bottom);

                        writer.WriteFiller(0x00, 16);

                        // No more data, in future we can expand the structure using chunks
                        chunkMagicWord = (ushort)Prj2ChunkType.NoExtraChunk;
                        writer.Write(chunkMagicWord);
                    }

                    // Write objects: moveables, static meshes, cameras, sinks, sound sources
                    int numObjects = 0;
                    foreach (var o in objectsList)
                        if (o.GetType() != typeof(Light))
                            numObjects++;

                    writer.Write(numObjects);
                    foreach (var o in objectsList)
                    {
                        if (o.GetType() == typeof(MoveableInstance))
                        {
                            MoveableInstance instance = (MoveableInstance)o;

                            writer.Write((ushort)Prj2ObjectType.Moveable);
                            writer.Write((ushort)level.Rooms.ReferenceIndexOf(o.Room));
                            writer.Write(instance.ItemType.Id);
                            writer.Write(instance.Position);
                            writer.Write(instance.RotationY);
                            writer.Write(instance.RotationYRadians);
                            writer.Write(instance.Ocb);
                            writer.Write(instance.Invisible);
                            writer.Write(instance.ClearBody);
                            writer.Write(instance.CodeBits);
                            writer.Write(instance.Color);

                            writer.WriteFiller(0x00, 8);

                            // No more data, in future we can expand the structure using chunks
                            chunkMagicWord = (ushort)Prj2ChunkType.NoExtraChunk;
                            writer.Write(chunkMagicWord); 
                        }
                        else if (o.GetType() == typeof(StaticInstance))
                        {
                            StaticInstance instance = (StaticInstance)o;

                            writer.Write((ushort)Prj2ObjectType.Static);
                            writer.Write((ushort)level.Rooms.ReferenceIndexOf(o.Room));
                            writer.Write(instance.ItemType.Id);
                            writer.Write(instance.Position);
                            writer.Write(instance.RotationY);
                            writer.Write(instance.RotationYRadians);
                            writer.Write(instance.Ocb);
                            writer.Write(instance.Color);

                            writer.WriteFiller(0x00, 8);

                            // No more data, in future we can expand the structure using chunks
                            chunkMagicWord = (ushort)Prj2ChunkType.NoExtraChunk;
                            writer.Write(chunkMagicWord);
                        }
                        else if (o.GetType() == typeof(CameraInstance))
                        {
                            CameraInstance instance = (CameraInstance)o;

                            writer.Write((ushort)Prj2ObjectType.Camera);
                            writer.Write((ushort)level.Rooms.ReferenceIndexOf(o.Room));
                            writer.Write(instance.Position);
                            writer.Write(instance.Flags);
                            writer.Write(instance.Number);
                            writer.Write(instance.Sequence);
                            writer.Write(instance.Roll);
                            writer.Write(instance.Speed);
                            writer.Write(instance.Timer);
                            writer.Write(instance.Fov);
                            writer.Write(instance.Fixed);

                            writer.WriteFiller(0x00, 8);

                            // No more data, in future we can expand the structure using chunks
                            chunkMagicWord = (ushort)Prj2ChunkType.NoExtraChunk;
                            writer.Write(chunkMagicWord);
                        }
                        else if (o.GetType() == typeof(FlybyCameraInstance))
                        {
                            FlybyCameraInstance instance = (FlybyCameraInstance)o;

                            writer.Write((ushort)Prj2ObjectType.FlybyCamera);
                            writer.Write((ushort)level.Rooms.ReferenceIndexOf(o.Room));
                            writer.Write(instance.Position);
                            writer.Write(instance.Flags);
                            writer.Write(instance.Number);
                            writer.Write(instance.Sequence);
                            writer.Write(instance.Roll);
                            writer.Write(instance.Speed);
                            writer.Write(instance.Timer);
                            writer.Write(instance.Fov);
                            writer.Write(instance.RotationX);
                            writer.Write(instance.RotationY);

                            writer.WriteFiller(0x00, 8);

                            // No more data, in future we can expand the structure using chunks
                            chunkMagicWord = (ushort)Prj2ChunkType.NoExtraChunk;
                            writer.Write(chunkMagicWord);
                        }
                        else if (o.GetType() == typeof(SinkInstance))
                        {
                            SinkInstance instance = (SinkInstance)o;

                            writer.Write((ushort)Prj2ObjectType.Sink);
                            writer.Write((ushort)level.Rooms.ReferenceIndexOf(o.Room));
                            writer.Write(instance.Position);
                            writer.Write(instance.Strength);

                            writer.WriteFiller(0x00, 8);

                            // No more data, in future we can expand the structure using chunks
                            chunkMagicWord = (ushort)Prj2ChunkType.NoExtraChunk;
                            writer.Write(chunkMagicWord);
                        }
                        else if (o.GetType() == typeof(SoundSourceInstance))
                        {
                            SoundSourceInstance instance = (SoundSourceInstance)o;

                            writer.Write((ushort)Prj2ObjectType.SoundSource);
                            writer.Write((ushort)level.Rooms.ReferenceIndexOf(o.Room));
                            writer.Write(instance.Position);
                            writer.Write(instance.SoundId);
                            writer.Write(instance.Flags);
                            writer.Write(instance.CodeBits);

                            writer.WriteFiller(0x00, 8);

                            // No more data, in future we can expand the structure using chunks
                            chunkMagicWord = (ushort)Prj2ChunkType.NoExtraChunk;
                            writer.Write(chunkMagicWord);
                        }
                    }

                    // Write triggers
                    int numTriggers = triggersList.Count;
                    writer.Write(numTriggers);
                    foreach (var t in triggersList)
                    {
                        writer.Write((ushort)level.Rooms.ReferenceIndexOf(t.Room));
                        writer.Write(t.Area.Left);
                        writer.Write(t.Area.Top);
                        writer.Write(t.Area.Right);
                        writer.Write(t.Area.Bottom);
                        writer.Write((ushort)t.TriggerType);
                        writer.Write((ushort)t.TargetType);
                        writer.Write(t.TargetData);
                        writer.Write((t.TargetObj != null ? (int)objectsList.IndexOf(t.TargetObj) : (int)-1));
                        writer.Write(t.Timer);
                        writer.Write(t.CodeBits);
                        writer.Write(t.OneShot);

                        writer.WriteFiller(0x00, 8);

                        // No more data, in future we can expand the structure using chunks
                        chunkMagicWord = (ushort)Prj2ChunkType.NoExtraChunk;
                        writer.Write(chunkMagicWord);
                    }

                    // Write rooms
                    int numRooms = level.Rooms.Length;
                    writer.Write(numRooms);
                    for (int i = 0; i < numRooms; i++)
                    {
                        var r = level.Rooms[i];
                        writer.Write(r != null);
                        if (r == null)
                            continue;
                        
                        writer.WriteStringUTF8(r.Name);
                        writer.Write(r.Position);
                        writer.Write(r.NumXSectors);
                        writer.Write(r.NumZSectors);

                        for (int z = 0; z < r.NumZSectors; z++)
                        {
                            for (int x = 0; x < r.NumXSectors; x++)
                            {
                                var b = r.Blocks[x, z];

                                writer.Write((ushort)b.Type);
                                writer.Write((ushort)b.Flags);

                                for (int n = 0; n < 4; n++)
                                    writer.Write(b.QAFaces[n]);
                                for (int n = 0; n < 4; n++)
                                    writer.Write(b.EDFaces[n]);
                                for (int n = 0; n < 4; n++)
                                    writer.Write(b.WSFaces[n]);
                                for (int n = 0; n < 4; n++)
                                    writer.Write(b.RFFaces[n]);

                                writer.Write((b.FloorPortal!=null ? (int)portalsList.IndexOf(b.FloorPortal) : (int)-1));
                                writer.Write((ushort)b.FloorOpacity);
                                writer.Write((b.CeilingPortal != null ? (int)portalsList.IndexOf(b.CeilingPortal) : (int)-1));
                                writer.Write((ushort)b.CeilingOpacity);
                                writer.Write((b.WallPortal != null ? (int)portalsList.IndexOf(b.WallPortal) : (int)-1));
                                writer.Write((ushort)b.WallOpacity);
                                writer.Write(b.NoCollisionFloor);
                                writer.Write(b.NoCollisionCeiling);
                                writer.Write((ushort)b.FloorDiagonalSplit);
                                writer.Write((ushort)b.CeilingDiagonalSplit);
                                writer.Write(b.FloorSplitDirectionToggled);
                                writer.Write(b.CeilingSplitDirectionToggled);

                                for (int f = 0; f < 29; f++)
                                {
                                    var texture = b.GetFaceTexture((BlockFace)f);

                                    var mode = Prj2FaceTextureMode.NoTexture;
                                    if (texture.TextureIsInvisble)
                                    {
                                        mode = Prj2FaceTextureMode.InvisibleColor;
                                    }
                                    else
                                    {
                                        if (!texture.TextureIsUnavailable)
                                            mode = Prj2FaceTextureMode.Texture;
                                        else
                                            mode = Prj2FaceTextureMode.NoTexture;
                                    }

                                    writer.Write((ushort)mode);

                                    if (mode == Prj2FaceTextureMode.Texture)
                                    {
                                        writer.Write(texture.TexCoord0);
                                        writer.Write(texture.TexCoord1);
                                        writer.Write(texture.TexCoord2);
                                        writer.Write(texture.TexCoord3);
                                        writer.Write((ushort)texture.BlendMode);
                                        writer.Write(texture.DoubleSided);

                                        writer.WriteFiller(0x00, 8);

                                        // No more data, in future we can expand the structure using chunks
                                        chunkMagicWord = (ushort)Prj2ChunkType.NoExtraChunk;
                                        writer.Write(chunkMagicWord);
                                    }
                                }

                                writer.WriteFiller(0x00, 32);

                                // No more data, in future we can expand the structure using chunks
                                chunkMagicWord = (ushort)Prj2ChunkType.NoExtraChunk;
                                writer.Write(chunkMagicWord);
                            }
                        }

                        // Write lights
                        uint numLights = 0;
                        foreach (var o in r.Objects)
                            if (o.GetType() == typeof(Light))
                                numLights++;
                        writer.Write(numLights);

                        foreach (var o in r.Objects)
                        {
                            if (o.GetType() == typeof(Light))
                            {
                                var l = (Light)o;

                                writer.Write((ushort)l.Type);
                                writer.Write(l.Position);
                                writer.Write(l.Intensity);
                                writer.Write(l.Color);
                                writer.Write(l.In);
                                writer.Write(l.Out);
                                writer.Write(l.Len);
                                writer.Write(l.Cutoff);
                                writer.Write(l.RotationX);
                                writer.Write(l.RotationY);
                                writer.Write(l.Enabled);
                                writer.Write(l.CastsShadows);
                                writer.Write(l.IsDynamicallyUsed);
                                writer.Write(l.IsStaticallyUsed);

                                writer.WriteFiller(0x00, 8);

                                // No more data, in future we can expand the structure using chunks
                                chunkMagicWord = (ushort)Prj2ChunkType.NoExtraChunk;
                                writer.Write(chunkMagicWord);
                            }
                        }

                        // Write imported geometry
                        uint numImportedGeometry = 0;
                        foreach (var o in r.Objects)
                            if (o.GetType() == typeof(RoomGeometryInstance))
                                numImportedGeometry++;
                        writer.Write(numImportedGeometry);

                        foreach (var o in r.Objects)
                        {
                            if (o.GetType() == typeof(RoomGeometryInstance))
                            {
                                RoomGeometryInstance instance = (RoomGeometryInstance)o;

                                writer.Write(instance.Position);
                                writer.Write((uint)modelsList.IndexOf(instance.Model.Name));

                                writer.WriteFiller(0x00, 8);

                                // No more data, in future we can expand the structure using chunks
                                chunkMagicWord = (ushort)Prj2ChunkType.NoExtraChunk;
                                writer.Write(chunkMagicWord);
                            }
                        }

                        // Write some references
                        uint numPortalsForThisRoom = (uint)r.Portals.Count();
                        writer.Write(numPortalsForThisRoom);
                        foreach (var portalInRoom in r.Portals)
                            writer.Write((uint)portalsList.IndexOf(portalInRoom));

                        uint numTriggersForThisRoom = (uint)r.Triggers.Count();
                        writer.Write(numTriggersForThisRoom);
                        foreach (var triggerInRoom in r.Triggers)
                            writer.Write((uint)triggersList.IndexOf(triggerInRoom));

                        writer.Write(r.AmbientLight);
                        writer.Write(r.AlternateGroup);
                        writer.Write((r.AlternateRoom != null ? (int)level.Rooms.ReferenceIndexOf(r.AlternateRoom) : (int)-1));
                        writer.Write((r.AlternateBaseRoom != null ? (int)level.Rooms.ReferenceIndexOf(r.AlternateBaseRoom) : (int)-1));
                        writer.Write(r.FlagCold);
                        writer.Write(r.FlagDamage);
                        writer.Write(r.FlagHorizon);
                        writer.Write(r.FlagMist);
                        writer.Write(r.FlagOutside);
                        writer.Write(r.FlagRain);
                        writer.Write(r.FlagReflection);
                        writer.Write(r.FlagSnow);
                        writer.Write(r.FlagWater);
                        writer.Write(r.FlagQuickSand);
                        writer.Write(r.ExcludeFromPathFinding);
                        writer.Write(r.WaterLevel);
                        writer.Write(r.MistLevel);
                        writer.Write(r.ReflectionLevel);
                        writer.Write((ushort)r.Reverberation);

                        writer.WriteFiller(0x00, 64);

                        // No more data, in future we can expand the structure using chunks
                        chunkMagicWord = (ushort)Prj2ChunkType.NoExtraChunk;
                        writer.Write(chunkMagicWord);
                    }

                    // Write animated textures
                    /*int numAnimatedTextures = level.AnimatedTextures.Count;
                    writer.Write(numAnimatedTextures);
                    foreach (var textureSet in level.AnimatedTextures)
                    {
                        writer.Write((byte)textureSet.Effect);

                        int numTexturesInSet = textureSet.Textures.Count;
                        writer.Write(numTexturesInSet);

                        foreach (var texture in textureSet.Textures)
                        {
                            writer.Write(texture.Page);
                            writer.Write(texture.X);
                            writer.Write(texture.Y);
                        }
                    }
                    */

                    // TODO: waiting for final animated textures code
                    uint numAnimatedTextures = 0;
                    writer.Write(numAnimatedTextures);

                    uint numTextureSounds = 0; // level.TextureSounds.Count;
                    writer.Write(numTextureSounds);
                    /*foreach (var sound in level.TextureSounds)
                    {
                        writer.Write(sound.X);
                        writer.Write(sound.Y);
                        writer.Write(sound.Page);
                        writer.Write((byte)sound.Sound);
                    }*/

                    writer.WriteFiller(0x00, 256);

                    // No more data, in future we can expand the structure using chunks
                    chunkMagicWord = (ushort)Prj2ChunkType.NoExtraChunk;
                    writer.Write(chunkMagicWord);

                    projectData = ms.ToArray();
                }

                using (var writer = new FileStream(filename, FileMode.Create, FileAccess.Write, FileShare.None))
                    writer.Write(projectData, 0, projectData.Length);
            }
            catch (Exception ex)
            {
                logger.Error(ex);
                return false;
            }

            return true;
        }

        private class IdResolver<T>
        {
            private readonly Dictionary<T, int> _idList = new Dictionary<T, int>();
            public int this[T obj]
            {
                get
                {
                    if (obj == null)
                        return -1;
                    if (!_idList.ContainsKey(obj))
                        _idList.Add(obj, _idList.Count);
                    return _idList[obj];
                }
            }
        }
    }
}
