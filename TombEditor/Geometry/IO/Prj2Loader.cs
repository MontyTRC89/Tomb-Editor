using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using NLog;
using SharpDX;
using SharpDX.Toolkit.Graphics;
using Color = System.Drawing.Color;
using TombLib.IO;
using TombLib.Utils;

namespace TombEditor.Geometry.IO
{
    public static class Prj2Loader
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();
        
        public static Level LoadFromPrj2(string filename, IProgressReporter progressReporter)
        {
          //  throw new NotSupportedException();
            
            var level = new Level();

            var portalsList = new List<Portal>();
            var triggersList = new List<TriggerInstance>();
            var objectsList = new List<ObjectInstance>();

            Prj2ChunkType chunkType;

            try
            {
                using (var reader = CreatePrjReader(filename))
                {
                    level.Settings.LevelFilePath = filename;

                    // Read version code (in the future it can be used to have multiple PRJ versions)
                    int versionCode = reader.ReadInt32();

                    // Read resource files
                    level.Settings.TextureFilePath = reader.ReadStringUTF8();
                    level.Settings.WadFilePath = reader.ReadStringUTF8();
                    level.Settings.FontTextureFilePath = reader.ReadStringUTF8();
                    level.Settings.SkyTextureFilePath = reader.ReadStringUTF8();
                    level.Settings.GameDirectory = reader.ReadStringUTF8();
                    level.Settings.GameLevelFilePath = reader.ReadStringUTF8();
                    level.Settings.GameExecutableFilePath = reader.ReadStringUTF8();
                    level.Settings.SoundPaths.Clear();
                    int soundPathCount = reader.ReadInt32();
                    for (int i = 0; i < soundPathCount; ++i)
                        level.Settings.SoundPaths.Add(new SoundPath(reader.ReadStringUTF8()));
                    level.Settings.IgnoreMissingSounds = reader.ReadBoolean();

                   // ResourceLoader.TryLoadingTexture(level, progressReporter);
                    ResourceLoader.TryLoadingObjects(level, progressReporter);

                    // Read fillers
                    reader.ReadBytes(16);

                    // Read portals
                    uint numPortals = reader.ReadUInt32();
                    for (int i = 0; i < numPortals; i++)
                    {
                        var roomIndex = reader.ReadUInt16();
                        var adjoiningRoomIndex = reader.ReadUInt16();

                        var direction = (PortalDirection)reader.ReadUInt16();

                        var left = reader.ReadInt32();
                        var top = reader.ReadInt32();
                        var right = reader.ReadInt32();
                        var bottom = reader.ReadInt32();
                        var area = new Rectangle(left, top, right, bottom);

                        reader.ReadBytes(16);

                        // Check for other chunks
                        chunkType = (Prj2ChunkType)reader.ReadUInt16();
                        if (chunkType != Prj2ChunkType.NoExtraChunk)
                        {
                            // TODO: logic for reading in the future other chunks
                        }

                        var portal = new Portal(area, direction, null);
                        portal.RoomIndex = roomIndex;
                        portal.AdjoiningRoomIndex = adjoiningRoomIndex;

                        portalsList.Add(portal);
                    }

                    // Read objects
                    uint numObjects = reader.ReadUInt32();
                    for (int i = 0; i < numObjects; i++)
                    {
                        var objType = (Prj2ObjectType)reader.ReadUInt16();
                        var roomIndex = reader.ReadUInt16();

                        switch (objType)
                        {
                            case Prj2ObjectType.Moveable:
                                var moveable = new MoveableInstance();

                                moveable.RoomIndex = roomIndex;
                                moveable.WadObjectId = reader.ReadUInt32();
                                moveable.Position = reader.ReadVector3();
                                moveable.RotationY = reader.ReadSingle();
                                moveable.RotationYRadians = reader.ReadSingle();
                                moveable.Ocb = reader.ReadInt16();
                                moveable.Invisible = reader.ReadBoolean();
                                moveable.ClearBody = reader.ReadBoolean();
                                moveable.CodeBits = reader.ReadByte();
                                moveable.Color = reader.ReadVector4();

                                 reader.ReadBytes(8);

                                // Check for other chunks
                                chunkType = (Prj2ChunkType)reader.ReadUInt16();
                                if (chunkType != Prj2ChunkType.NoExtraChunk)
                                {
                                    // TODO: logic for reading in the future other chunks
                                }

                                objectsList.Add(moveable);

                                break;

                            case Prj2ObjectType.Static:
                                var staticMesh = new StaticInstance();

                                staticMesh.RoomIndex = roomIndex;
                                staticMesh.WadObjectId = reader.ReadUInt32();
                                staticMesh.Position = reader.ReadVector3();
                                staticMesh.RotationY = reader.ReadSingle();
                                staticMesh.RotationYRadians = reader.ReadSingle();
                                staticMesh.Ocb = reader.ReadInt16();
                                staticMesh.Color = reader.ReadVector4();

                                reader.ReadBytes(8);

                                // Check for other chunks
                                chunkType = (Prj2ChunkType)reader.ReadUInt16();
                                if (chunkType != Prj2ChunkType.NoExtraChunk)
                                {
                                    // TODO: logic for reading in the future other chunks
                                }

                                objectsList.Add(staticMesh);

                                break;

                            case Prj2ObjectType.Camera:
                                var camera = new CameraInstance();

                                camera.RoomIndex = roomIndex;
                                camera.Position = reader.ReadVector3();
                                camera.Flags = reader.ReadUInt16();
                                camera.Number = reader.ReadUInt16();
                                camera.Sequence = reader.ReadUInt16();
                                camera.Roll = reader.ReadSingle();
                                camera.Speed = reader.ReadSingle();
                                camera.Timer = reader.ReadInt16();
                                camera.Fov = reader.ReadSingle();
                                camera.Fixed = reader.ReadBoolean();

                                reader.ReadBytes(8);

                                // Check for other chunks
                                chunkType = (Prj2ChunkType)reader.ReadUInt16();
                                if (chunkType != Prj2ChunkType.NoExtraChunk)
                                {
                                    // TODO: logic for reading in the future other chunks
                                }

                                objectsList.Add(camera);

                                break;

                            case Prj2ObjectType.FlybyCamera:
                                var flybyCamera = new FlybyCameraInstance();

                                flybyCamera.RoomIndex = roomIndex;
                                flybyCamera.Position = reader.ReadVector3();
                                flybyCamera.Flags = reader.ReadUInt16();
                                flybyCamera.Number = reader.ReadUInt16();
                                flybyCamera.Sequence = reader.ReadUInt16();
                                flybyCamera.Roll = reader.ReadSingle();
                                flybyCamera.Speed = reader.ReadSingle();
                                flybyCamera.Timer = reader.ReadInt16();
                                flybyCamera.Fov = reader.ReadSingle();
                                flybyCamera.RotationX = reader.ReadSingle();
                                flybyCamera.RotationY = reader.ReadSingle();

                                reader.ReadBytes(8);

                                // Check for other chunks
                                chunkType = (Prj2ChunkType)reader.ReadUInt16();
                                if (chunkType != Prj2ChunkType.NoExtraChunk)
                                {
                                    // TODO: logic for reading in the future other chunks
                                }

                                objectsList.Add(flybyCamera);

                                break;

                            case Prj2ObjectType.Sink:
                                var sink = new SinkInstance();

                                sink.RoomIndex = roomIndex;
                                sink.Position = reader.ReadVector3();
                                sink.Strength = reader.ReadInt16();

                                reader.ReadBytes(8);

                                // Check for other chunks
                                chunkType = (Prj2ChunkType)reader.ReadUInt16();
                                if (chunkType != Prj2ChunkType.NoExtraChunk)
                                {
                                    // TODO: logic for reading in the future other chunks
                                }

                                objectsList.Add(sink);

                                break;

                            case Prj2ObjectType.SoundSource:
                                var sound = new SoundSourceInstance();

                                sound.RoomIndex = roomIndex;
                                sound.Position = reader.ReadVector3();
                                sound.SoundId = reader.ReadInt16();
                                sound.Flags = reader.ReadInt16();
                                sound.CodeBits = reader.ReadByte();

                                reader.ReadBytes(8);

                                // Check for other chunks
                                chunkType = (Prj2ChunkType)reader.ReadUInt16();
                                if (chunkType != Prj2ChunkType.NoExtraChunk)
                                {
                                    // TODO: logic for reading in the future other chunks
                                }

                                objectsList.Add(sound);

                                break;
                        }
                    }

                    // Read triggers
                    uint numTriggers = reader.ReadUInt32();
                    for (int i = 0; i < numTriggers; i++)
                    {
                        var roomIndex = reader.ReadUInt16();
                        var trigger = new TriggerInstance(new Rectangle(reader.ReadInt32(),
                                                                        reader.ReadInt32(),
                                                                        reader.ReadInt32(),
                                                                        reader.ReadInt32()));
                        trigger.RoomIndex = roomIndex;
                        trigger.TriggerType = (TriggerType)reader.ReadUInt16();
                        trigger.TargetType = (TriggerTargetType)reader.ReadUInt16();
                        trigger.TargetData = reader.ReadInt16();

                        var targetObject = reader.ReadInt32();
                        if (targetObject != -1) trigger.TargetObj = objectsList[targetObject];

                        trigger.Timer = reader.ReadInt16();
                        trigger.CodeBits = reader.ReadByte();
                        trigger.OneShot = reader.ReadBoolean();

                        reader.ReadBytes(8);

                        // Check for other chunks
                        chunkType = (Prj2ChunkType)reader.ReadUInt16();
                        if (chunkType != Prj2ChunkType.NoExtraChunk)
                        {
                            // TODO: logic for reading in the future other chunks
                        }

                        triggersList.Add(trigger);
                    }

                    // Read rooms
                    uint numRooms = reader.ReadUInt32();
                    for (int i = 0; i < numRooms; i++)
                    {
                        bool defined = reader.ReadBoolean();
                        if (!defined) continue;

                        var roomName = reader.ReadStringUTF8();
                        var position = reader.ReadVector3();
                        var numXsectors = reader.ReadByte();
                        var numZsectors = reader.ReadByte();

                        var room = new Room(level, numXsectors, numZsectors, roomName);
                        room.Position = position;

                        for (int z = 0; z < numZsectors; z++)
                        {
                            for (int x = 0; x < numXsectors; x++)
                            {
                                var b = new Block(0, 12);

                                b.Type = (BlockType)reader.ReadUInt16();
                                b.Flags = (BlockFlags)reader.ReadUInt16();

                                for (int j = 0; j < 4; j++) b.QAFaces[j] = reader.ReadInt16();
                                for (int j = 0; j < 4; j++) b.EDFaces[j] = reader.ReadInt16();
                                for (int j = 0; j < 4; j++) b.WSFaces[j] = reader.ReadInt16();
                                for (int j = 0; j < 4; j++) b.RFFaces[j] = reader.ReadInt16();

                                var floorPortal = reader.ReadInt32();
                                if (floorPortal != -1) b.FloorPortal = portalsList[floorPortal];
                                b.FloorOpacity = (PortalOpacity)reader.ReadUInt16();

                                var ceilingPortal = reader.ReadInt32();
                                if (ceilingPortal != -1) b.CeilingPortal = portalsList[ceilingPortal];
                                b.CeilingOpacity = (PortalOpacity)reader.ReadUInt16();
                                
                                var wallPortal = reader.ReadInt32();
                                if (wallPortal != -1) b.WallPortal = portalsList[wallPortal];
                                b.WallOpacity = (PortalOpacity)reader.ReadUInt16();

                                b.NoCollisionFloor = reader.ReadBoolean();
                                b.NoCollisionCeiling = reader.ReadBoolean();

                                b.FloorDiagonalSplit = (DiagonalSplit)reader.ReadUInt16();
                                b.CeilingDiagonalSplit = (DiagonalSplit)reader.ReadUInt16();
                                b.FloorSplitDirectionToggled = reader.ReadBoolean();
                                b.CeilingSplitDirectionToggled = reader.ReadBoolean();

                                for (int f = 0; f < 29; f++)
                                {
                                    bool textured = reader.ReadBoolean();
                                    if (textured)
                                    {
                                        var texture = new TextureArea();

                                        texture.TexCoord0 = reader.ReadVector2();
                                        texture.TexCoord1 = reader.ReadVector2();
                                        texture.TexCoord2 = reader.ReadVector2();
                                        texture.TexCoord3 = reader.ReadVector2();
                                        texture.BlendMode = (BlendMode)reader.ReadUInt16();
                                        texture.DoubleSided = reader.ReadBoolean();

                                        reader.ReadBytes(8);

                                        // Check for other chunks
                                        chunkType = (Prj2ChunkType)reader.ReadUInt16();
                                        if (chunkType != Prj2ChunkType.NoExtraChunk)
                                        {
                                            // TODO: logic for reading in the future other chunks
                                        }

                                        b.SetFaceTexture((BlockFace)f, texture);
                                    }
                                }

                                reader.ReadBytes(32);

                                // Check for other chunks
                                chunkType = (Prj2ChunkType)reader.ReadUInt16();
                                if (chunkType != Prj2ChunkType.NoExtraChunk)
                                {
                                    // TODO: logic for reading in the future other chunks
                                }
                            }
                        }

                        uint numLights = reader.ReadUInt32();
                        for (int j = 0; j < numLights; j++)
                        {
                            var l = new Light((LightType)reader.ReadUInt16());

                            l.Position = reader.ReadVector3();
                            l.Intensity = reader.ReadSingle();
                            l.Color = reader.ReadVector3();
                            l.In = reader.ReadSingle();
                            l.Out = reader.ReadSingle();
                            l.Len = reader.ReadSingle();
                            l.Cutoff = reader.ReadSingle();
                            var rotationX = reader.ReadSingle();
                            var rotationY = reader.ReadSingle();
                            l.SetArbitaryRotationsYX(rotationY, rotationX);
                            l.Enabled = reader.ReadBoolean();
                            l.CastsShadows = reader.ReadBoolean();
                            l.IsDynamicallyUsed = reader.ReadBoolean();
                            l.IsStaticallyUsed = reader.ReadBoolean();

                            reader.ReadBytes(8);

                            // Check for other chunks
                            chunkType = (Prj2ChunkType)reader.ReadUInt16();
                            if (chunkType != Prj2ChunkType.NoExtraChunk)
                            {
                                // TODO: logic for reading in the future other chunks
                            }

                            room.AddObject(level, l);
                        }

                        room.AmbientLight = reader.ReadVector4();
                        room.AlternateGroup = reader.ReadInt16();
                        room.AlternateRoomIndex = reader.ReadInt32();
                        room.AlternateBaseRoomIndex = reader.ReadInt32();
                        room.FlagCold = reader.ReadBoolean();
                        room.FlagDamage = reader.ReadBoolean();
                        room.FlagHorizon = reader.ReadBoolean();
                        room.FlagMist = reader.ReadBoolean();
                        room.FlagOutside = reader.ReadBoolean();
                        room.FlagRain = reader.ReadBoolean();
                        room.FlagReflection = reader.ReadBoolean();
                        room.FlagSnow = reader.ReadBoolean();
                        room.FlagWater = reader.ReadBoolean();
                        room.FlagQuickSand = reader.ReadBoolean();
                        room.ExcludeFromPathFinding = reader.ReadBoolean();
                        room.WaterLevel = reader.ReadInt16();
                        room.MistLevel = reader.ReadInt16();
                        room.ReflectionLevel = reader.ReadInt16();
                        room.Reverberation = (Reverberation)reader.ReadUInt16();

                        reader.ReadBytes(64);

                        // Check for other chunks
                        chunkType = (Prj2ChunkType)reader.ReadUInt16();
                        if (chunkType != Prj2ChunkType.NoExtraChunk)
                        {
                            // TODO: logic for reading in the future other chunks
                        }

                        level.Rooms[i] = room;
                    }

                    uint numAnimatedTextures = reader.ReadUInt32();
                    uint numTextureSounds = reader.ReadUInt32();

                    reader.ReadBytes(256);

                    // Check for other chunks
                    chunkType = (Prj2ChunkType)reader.ReadUInt16();
                    if (chunkType != Prj2ChunkType.NoExtraChunk)
                    {
                        // TODO: logic for reading in the future other chunks
                    }
                }

                // Now link everything
                foreach (var obj in objectsList)
                    obj.Room = level.Rooms[obj.RoomIndex];

                foreach (var trigger in triggersList)
                {
                    trigger.Room = level.Rooms[trigger.RoomIndex];
                    trigger.Room.AddObject(level, trigger);

                    /*for (int x = trigger.Area.X; x < trigger.Area.X + trigger.Area.Width; x++)
                    {
                        for (int z = trigger.Area.Y; z < trigger.Area.Y + trigger.Area.Height; z++)
                        {*/

                    /* }
                 }*/
                }

                foreach (var portal in portalsList)
                {
                    portal.Room = level.Rooms[portal.RoomIndex];
                    portal.AdjoiningRoom = level.Rooms[portal.AdjoiningRoomIndex];
                    portal.Room.AddObject(level, portal);
                }

                // Check that there are uninitialized objects
               /* foreach (Room room in level.Rooms.Where(room => room != null))
                {
                    if ((room.NumXSectors <= 0) && (room.NumZSectors <= 0))
                        throw new Exception("Room " + level.Rooms.ReferenceIndexOf(room) + " has a sector size of zero. This is invalid. Probably the room was referenced but never initialized.");

                    foreach (var currentPortal in room.Portals)
                        if ((currentPortal.AdjoiningRoom == null) || (currentPortal.Room == null))
                            throw new Exception("There appears to be an uninitialized portal.");
                }*/
            }
            catch (Exception ex)
            {
                logger.Error(ex);
                return null;
            }
            
            // Now build the real geometry and update DirectX buffers
            foreach (var room in level.Rooms.Where(room => room != null))
                room.UpdateCompletely();

            return level;
        }
        
        private static BinaryReaderEx CreatePrjReader(string filename)
        {
            var reader = new BinaryReaderEx(new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.Read));

            // Check file version
            var buffer = reader.ReadBytes(4);
            if (buffer[0] == 0x50 && buffer[1] == 0x52 && buffer[2] == 0x4A && buffer[3] == 0x32)
            {
                // PRJ2 senza compressione
                return reader;
            }
            else if (buffer[0] == 0x5A && buffer[1] == 0x52 && buffer[2] == 0x4A && buffer[3] == 0x32)
            {
                // PRJ2 compresso
                int uncompressedSize = reader.ReadInt32();
                int compressedSize = reader.ReadInt32();
                var projectData = reader.ReadBytes(compressedSize);
                projectData = ZLib.DecompressData(projectData);

                var ms = new MemoryStream(projectData);
                ms.Seek(0, SeekOrigin.Begin);

                reader = new BinaryReaderEx(ms);
                reader.ReadInt32();
                return reader;
            }
            else
            {
                throw new NotSupportedException("The header of the *.prj2 file was unrecognizable.");
            }
        }

        private class IdResolver<T> where T : class
        {
            private Func<T> _createT;
            private readonly Dictionary<int, T> _portalList = new Dictionary<int, T>();

            public IdResolver(Func<T> createT)
            {
                _createT = createT;
            }

            public T this[int id]
            {
                get
                {
                    if (id == -1)
                        return null;

                    if (_portalList.ContainsKey(id))
                        return _portalList[id];

                    T newT = _createT();
                    _portalList.Add(id, newT);
                    return newT;
                }
            }
        }
    }
}
