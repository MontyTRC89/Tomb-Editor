using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using NLog;
using SharpDX;
using SharpDX.Toolkit.Graphics;
using TombLib.IO;
using Color = System.Drawing.Color;

namespace TombEditor.Geometry.IO
{
    public class PrjLoader
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        private struct PrjFlipInfo
        {
            public short _baseRoom;
            public short _flipRoom;
            public short _group;
        }

        private struct PrjFace
        {
            public short _txtType;
            public short _txtIndex;
            public byte _txtFlags;
            public byte _txtRotation;
            public byte _txtTriangle;
            public int _newId;
            public bool _isFlipped;
        }

        private struct PrjBlock
        {
            public short _blockType;
            public short _blockFlags1;
            public short _blockYfloor;
            public short _blockYceiling;
            public sbyte[] _qaFaces;
            public sbyte[] _wsFaces;
            public sbyte[] _edFaces;
            public sbyte[] _rfFaces;
            public PrjFace[] _faces;
            public short _flags2;
            public short _flags3;
        }

        private struct PrjTexInfo
        {
            public byte _x;
            public short _y;
            public byte _width;
            public byte _height;
        }

        private struct PrjPortalThingIndex
        {
            public short _thisThingIndex;
            public short _otherThingIndex;
        };
        
        public static Level LoadFromPrj(string filename, GraphicsDevice device, IProgressReporter progressReporter)
        {
            var level = new Level();

            try
            {
                // Open file
                using (var reader = new BinaryReaderEx(File.OpenRead(filename)))
                {
                    level.FileName = Path.ChangeExtension(filename, "prj2");

                    progressReporter.ReportProgress(0, "Begin of PRJ import");

                    logger.Warn("Opening Winroomedit PRJ file");

                    // Check if it's a NGLE PRJ
                    bool ngle = false;
                    reader.BaseStream.Seek(reader.BaseStream.Length - 8, SeekOrigin.Begin);
                    var bytesNgle = reader.ReadBytes(4);
                    if (bytesNgle[0] == 0x4E && bytesNgle[1] == 0x47 && bytesNgle[2] == 0x4C && bytesNgle[3] == 0x45)
                    {
                        progressReporter.ReportProgress(1, "This is a NGLE project");
                        logger.Debug("NGLE Project");
                        ngle = true;
                    }

                    reader.BaseStream.Seek(0, SeekOrigin.Begin);

                    // Version
                    reader.ReadBytes(12);

                    // Number of rooms
                    int numRooms = reader.ReadInt32();
                    logger.Debug("Number of rooms: " + numRooms);

                    // Now read the first info about rooms, at the end of the PRJ there will be another block
                    for (int i = 0; i < Level.MaxNumberOfRooms; ++i)
                        level.Rooms[i] = null;

                    var tempRooms = new Dictionary<int, PrjBlock[,]>();
                    var flipInfos = new List<PrjFlipInfo>();

                    progressReporter.ReportProgress(2, "Number of rooms: " + numRooms);
                    double progress = 2;

                    var portalThingIndices = new Dictionary<Portal, PrjPortalThingIndex>();

                    for (int i = 0; i < numRooms; i++)
                    {
                        // Room is defined?
                        short defined = reader.ReadInt16();
                        if (defined == 0x01)
                            continue;

                        // Read room's name
                        byte[] roomNameBytes = reader.ReadBytes(80);
                        int roomNameLength = 0;
                        for (; roomNameLength < 80; ++roomNameLength)
                            if (roomNameBytes[roomNameLength] == 0)
                                break;
                        string roomName = System.Text.Encoding.ASCII.GetString(roomNameBytes, 0, roomNameLength);

                        logger.Warn("Room #" + i);
                        logger.Info("    Name: " + roomName);

                        // Read position
                        int zPos = reader.ReadInt32();
                        int yPos = reader.ReadInt32();
                        int xPos = reader.ReadInt32();
                        int yPos2 = reader.ReadInt32();

                        reader.ReadBytes(2);
                        reader.ReadInt16();
                        reader.ReadInt16();

                        short numXBlocks = reader.ReadInt16();
                        short numZBlocks = reader.ReadInt16();
                        short posXBlocks = reader.ReadInt16();
                        short posZBlocks = reader.ReadInt16();

                        reader.ReadInt16();

                        var room = new Room(level, numXBlocks, numZBlocks, roomName);
                        room.Position = new Vector3(posXBlocks, yPos / 256.0f, posZBlocks);
                        level.Rooms[i] = room;

                        short numPortals = reader.ReadInt16();
                        var portalThings = new short[numPortals];

                        for (int j = 0; j < numPortals; j++)
                        {
                            portalThings[j] = reader.ReadInt16();
                        }

                        logger.Info("    Portals: " + numPortals);

                        for (int j = 0; j < numPortals; j++)
                        {
                            ushort direction = reader.ReadUInt16();
                            short portalX = reader.ReadInt16();
                            short portalZ = reader.ReadInt16();
                            short portalXBlocks = reader.ReadInt16();
                            short portalZBlocks = reader.ReadInt16();
                            reader.ReadInt16();
                            var portalRoom = level.GetOrCreateDummyRoom(reader.ReadInt16());
                            short portalSlot = reader.ReadInt16();

                            var portalBuffer = reader.ReadBytes(26);

                            var p = new Portal(level.GetNewPortalId(), portalRoom)
                            {
                                X = (byte)portalX,
                                Z = (byte)portalZ,
                                NumXBlocks = (byte)portalXBlocks,
                                NumZBlocks = (byte)portalZBlocks
                            };


                            if (direction == 0x0001)
                                p.Direction = PortalDirection.East;
                            if (direction == 0x0002)
                                p.Direction = PortalDirection.South;
                            if (direction == 0x0004)
                                p.Direction = PortalDirection.Floor;
                            if (direction == 0xfffe)
                                p.Direction = PortalDirection.West;
                            if (direction == 0xfffd)
                                p.Direction = PortalDirection.North;
                            if (direction == 0xfffb)
                                p.Direction = PortalDirection.Ceiling;

                            p.MemberOfFlippedRoom = !ReferenceEquals(p.Room, room);
                            p.Room = room;

                            portalThingIndices.Add(p, new PrjPortalThingIndex
                            {
                                _thisThingIndex = portalThings[j],
                                _otherThingIndex = portalSlot
                            });

                            level.Portals.Add(p.Id, p);
                        }

                        short numObjects = reader.ReadInt16();
                        var objectsThings = new short[numObjects];

                        for (int j = 0; j < numObjects; j++)
                        {
                            objectsThings[j] = reader.ReadInt16();
                        }

                        logger.Info("    Objects and Triggers: " + numObjects);

                        for (int j = 0; j < numObjects; j++)
                        {
                            short objectType = reader.ReadInt16();
                            short objPosX = reader.ReadInt16();
                            short objPosZ = reader.ReadInt16();
                            short objSizeX = reader.ReadInt16();
                            short objSizeZ = reader.ReadInt16();
                            short objPosY = reader.ReadInt16();
                            var objRoom = reader.ReadInt16(); // level.GetOrCreateDummyRoom(reader.ReadInt16());
                            short objSlot = reader.ReadInt16();
                            short objOcb = reader.ReadInt16();
                            short objOrientation = reader.ReadInt16();

                            int objLongZ = reader.ReadInt32();
                            int objLongY = reader.ReadInt32();
                            int objLongX = reader.ReadInt32();

                            short objUnk = reader.ReadInt16();
                            short objFacing = reader.ReadInt16();
                            short objRoll = reader.ReadInt16();
                            short objTint = reader.ReadInt16();
                            short objTimer = reader.ReadInt16();

                            short triggerType = 0;
                            short triggerItemNumber = 0;
                            short triggerTimere = 0;
                            short triggerFlags = 0;
                            short triggerItemType = 0;

                            if (objectType == 0x0010)
                            {
                                triggerType = reader.ReadInt16();
                                triggerItemNumber = reader.ReadInt16();
                                triggerTimere = reader.ReadInt16();
                                triggerFlags = reader.ReadInt16();
                                triggerItemType = reader.ReadInt16();
                            }

                            if (objectType == 0x0008)
                            {
                                if (objSlot >= 460 && objSlot <= 464)
                                {
                                    continue;
                                }

                                if (objSlot < (ngle ? 520 : 465)) // TODO: a more flexible way to define this
                                {
                                    var instance = new MoveableInstance(objectsThings[j], room)
                                    {
                                        CodeBits = (byte)((objOcb >> 1) & 0x1f),
                                        Invisible = (objOcb & 0x0001) != 0,
                                        ClearBody = (objOcb & 0x0080) != 0,
                                        WadObjectId = unchecked((uint)objSlot),
                                        Position = new Vector3(objPosX, objLongY, objPosZ),
                                        Ocb = objTimer
                                    };


                                    objFacing = (short)((objFacing >> 8) & 0xff);

                                    if (objFacing == 0x00)
                                        instance.Rotation = 270;
                                    if (objFacing == 0x20)
                                        instance.Rotation = 315;
                                    if (objFacing == 0x40)
                                        instance.Rotation = 0;
                                    if (objFacing == 0x60)
                                        instance.Rotation = 45;
                                    if (objFacing == 0x80)
                                        instance.Rotation = 90;
                                    if (objFacing == 0xa0)
                                        instance.Rotation = 135;
                                    if (objFacing == 0xc0)
                                        instance.Rotation = 180;
                                    if (objFacing == 0xe0)
                                        instance.Rotation = 225;

                                    level.Objects.Add(instance.Id, instance);
                                    room.Moveables.Add(instance.Id);
                                }
                                else
                                {
                                    var instance = new StaticInstance(objectsThings[j], room)
                                    {
                                        WadObjectId = unchecked((uint)(objSlot - (ngle ? 520 : 465))),
                                        Position = new Vector3(objPosX, objLongY, objPosZ)
                                    };

                                    objFacing = (short)((objFacing >> 8) & 0xff);

                                    if (objFacing == 0x00)
                                        instance.Rotation = 270;
                                    if (objFacing == 0x20)
                                        instance.Rotation = 315;
                                    if (objFacing == 0x40)
                                        instance.Rotation = 0;
                                    if (objFacing == 0x60)
                                        instance.Rotation = 45;
                                    if (objFacing == 0x80)
                                        instance.Rotation = 90;
                                    if (objFacing == 0xa0)
                                        instance.Rotation = 135;
                                    if (objFacing == 0xc0)
                                        instance.Rotation = 180;
                                    if (objFacing == 0xe0)
                                        instance.Rotation = 225;

                                    byte red = (byte)(objTint & 0x001f);
                                    byte green = (byte)((objTint & 0x03e0) >> 5);
                                    byte blu = (byte)((objTint & 0x7c00) >> 10);

                                    instance.Color = Color.FromArgb(255, red * 8, green * 8, blu * 8);

                                    level.Objects.Add(instance.Id, instance);
                                    room.Statics.Add(instance.Id);
                                }
                            }
                            else
                            {
                                var trigger = new TriggerInstance(level.GetNewTriggerId(), level.GetOrCreateDummyRoom(i))
                                {
                                    X = (byte)objPosX,
                                    Z = (byte)objPosZ,
                                    NumXBlocks = (byte)objSizeX,
                                    NumZBlocks = (byte)objSizeZ,
                                    Target = triggerItemNumber
                                };


                                switch (triggerType)
                                {
                                    case 0:
                                        trigger.TriggerType = TriggerType.Trigger;
                                        break;
                                    case 1:
                                        trigger.TriggerType = TriggerType.Pad;
                                        break;
                                    case 2:
                                        trigger.TriggerType = TriggerType.Switch;
                                        break;
                                    case 3:
                                        trigger.TriggerType = TriggerType.Key;
                                        break;
                                    case 4:
                                        trigger.TriggerType = TriggerType.Pickup;
                                        break;
                                    case 5:
                                        trigger.TriggerType = TriggerType.Heavy;
                                        break;
                                    case 6:
                                        trigger.TriggerType = TriggerType.Antipad;
                                        break;
                                    case 7:
                                        trigger.TriggerType = TriggerType.Combat;
                                        break;
                                    case 8:
                                        trigger.TriggerType = TriggerType.Dummy;
                                        break;
                                    case 9:
                                        trigger.TriggerType = TriggerType.Antitrigger;
                                        break;
                                    case 10:
                                        trigger.TriggerType = TriggerType.HeavySwitch;
                                        break;
                                    case 11:
                                        trigger.TriggerType = TriggerType.HeavyAntritrigger;
                                        break;
                                    case 12:
                                        trigger.TriggerType = TriggerType.Monkey;
                                        break;
                                }

                                trigger.CodeBits = (byte)((~triggerFlags >> 1) & 0x1f);
                                trigger.OneShot = (triggerFlags & 0x0001) != 0;

                                trigger.Timer = triggerTimere;

                                switch (triggerItemType)
                                {
                                    case 0:
                                        trigger.TargetType = TriggerTargetType.Object;
                                        break;
                                    case 3:
                                        trigger.TargetType = TriggerTargetType.FlipMap;
                                        break;
                                    case 4:
                                        trigger.TargetType = TriggerTargetType.FlipOn;
                                        break;
                                    case 5:
                                        trigger.TargetType = TriggerTargetType.FlipOff;
                                        break;
                                    case 6:
                                        trigger.TargetType = TriggerTargetType.Target;
                                        break;
                                    case 7:
                                        trigger.TargetType = TriggerTargetType.FinishLevel;
                                        break;
                                    case 8:
                                        trigger.TargetType = TriggerTargetType.PlayAudio;
                                        break;
                                    case 9:
                                        trigger.TargetType = TriggerTargetType.FlipEffect;
                                        break;
                                    case 10:
                                        trigger.TargetType = TriggerTargetType.Secret;
                                        break;
                                    case 12:
                                        trigger.TargetType = TriggerTargetType.FlyByCamera;
                                        break;
                                    case 13:
                                        trigger.TargetType = TriggerTargetType.CutsceneOrParameterNg;
                                        break;
                                    case 14:
                                        trigger.TargetType = TriggerTargetType.Fmv;
                                        break;
                                }

                                level.Triggers.Add(trigger.Id, trigger);
                            }
                        }

                        room.AmbientLight =
                            Color.FromArgb(255, reader.ReadByte(), reader.ReadByte(), reader.ReadByte());
                        reader.ReadByte();

                        short numObjects2 = reader.ReadInt16();
                        var objectsThings2 = new short[numObjects2];

                        logger.Info("    Lights and other objects: " + numObjects2);

                        for (int j = 0; j < numObjects2; j++)
                        {
                            objectsThings2[j] = reader.ReadInt16();
                        }

                        for (int j = 0; j < numObjects2; j++)
                        {
                            short objectType = reader.ReadInt16();
                            short objPosX = reader.ReadInt16();
                            short objPosZ = reader.ReadInt16();
                            short objSizeX = reader.ReadInt16();
                            short objSizeZ = reader.ReadInt16();
                            short objPosY = reader.ReadInt16();
                            var objRoom = reader.ReadInt16(); // level.GetOrCreateDummyRoom(reader.ReadInt16());
                            short objSlot = reader.ReadInt16();
                            short objTimer = reader.ReadInt16();
                            short objOrientation = reader.ReadInt16();

                            int objLongZ = reader.ReadInt32();
                            int objLongY = reader.ReadInt32();
                            int objLongX = reader.ReadInt32();

                            short objUnk = reader.ReadInt16();
                            short objFacing = reader.ReadInt16();
                            short objRoll = reader.ReadInt16();
                            short objSpeed = reader.ReadInt16();
                            short objOcb = reader.ReadInt16();

                            switch (objectType)
                            {
                                case 0x4000:
                                case 0x6000:
                                case 0x4200:
                                case 0x5000:
                                case 0x4100:
                                case 0x4020:
                                    // Light
                                    short lightIntensity = reader.ReadInt16();
                                    float lightIn = reader.ReadSingle();
                                    float lightOut = reader.ReadSingle();
                                    float lightX = reader.ReadSingle();
                                    float lightY = reader.ReadSingle();
                                    float lightLen = reader.ReadSingle();
                                    float lightCut = reader.ReadSingle();
                                    byte lightR = reader.ReadByte();
                                    byte lightG = reader.ReadByte();
                                    byte lightB = reader.ReadByte();
                                    byte lightOn = reader.ReadByte();

                                    LightType lightType;
                                    switch (objectType)
                                    {
                                        case 0x4000:
                                            lightType = LightType.Light;
                                            lightIn /= 1024.0f;
                                            lightOut /= 1024.0f;
                                            break;
                                        case 0x6000:
                                            lightType = LightType.Shadow;
                                            lightIn /= 1024.0f;
                                            lightOut /= 1024.0f;
                                            break;
                                        case 0x4200:
                                            lightType = LightType.Sun;
                                            break;
                                        case 0x5000:
                                            lightType = LightType.Effect;
                                            break;
                                        case 0x4100:
                                            lightType = LightType.Spot;
                                            lightLen /= 1024.0f;
                                            lightCut /= 1024.0f;
                                            break;
                                        case 0x4020:
                                            lightType = LightType.FogBulb;
                                            break;
                                        default:
                                            throw new NotSupportedException("Unknown light type found inside *.prj file.");
                                    }

                                    var light = new Light(lightType,
                                        new Vector3(objPosX * 1024.0f + 512.0f, -objLongY, objPosZ * 1024.0f + 512.0f))
                                    {
                                        Color = Color.FromArgb(255, lightR, lightG, lightB),
                                        Cutoff = lightCut,
                                        Len = lightLen,
                                        DirectionX = 360.0f - lightX,
                                        DirectionY = lightY + 90.0f,
                                        Enabled = lightOn == 0x01,
                                        In = lightIn,
                                        Out = lightOut,
                                        Intensity = lightIntensity / 8192.0f,
                                    };
                                    if (light.DirectionY >= 360)
                                        light.DirectionY = light.DirectionY - 360.0f;

                                    room.Lights.Add(light);
                                    break;
                                case 0x4c00:
                                    var sound = new SoundSourceInstance(objectsThings2[j], room)
                                    {
                                        SoundId = objSlot,
                                        Position = new Vector3(objPosX, objLongY, objPosZ)
                                    };


                                    level.Objects.Add(sound.Id, sound);
                                    break;
                                case 0x4400:
                                    var sink = new SinkInstance(objectsThings2[j], room)
                                    {
                                        Strength = (short)(objTimer / 2),
                                        Position = new Vector3(objPosX, objLongY, objPosZ)
                                    };

                                    level.Objects.Add(sink.Id, sink);
                                    room.Sinks.Add(sink.Id);
                                    break;
                                case 0x4800:
                                case 0x4080:
                                    var camera = new CameraInstance(objectsThings2[j], room)
                                    {
                                        Timer = objTimer,
                                        Fixed = (objectType == 0x4080),
                                        Position = new Vector3(objPosX, objLongY, objPosZ)
                                    };

                                    level.Objects.Add(camera.Id, camera);
                                    room.Cameras.Add(camera.Id);
                                    break;
                                case 0x4040:
                                    var flybyCamera = new FlybyCameraInstance(objectsThings2[j], room)
                                    {
                                        Timer = unchecked((ushort)objTimer),
                                        Sequence = (byte)((objSlot & 0xe000) >> 13),
                                        Number = (byte)((objSlot & 0x1f00) >> 8),
                                        Fov = (short)(objSlot & 0x00ff),
                                        Roll = objRoll,
                                        Speed = objSpeed / 655.0f,
                                        Position = new Vector3(objPosX, objLongY, objPosZ),
                                        RotationX = -objUnk,
                                        RotationY = objFacing + 90,
                                        Flags = unchecked((ushort)objOcb)
                                    };

                                    if (flybyCamera.RotationY >= 360) flybyCamera.RotationY = (short)(flybyCamera.RotationY - 360);
                                    
                                    level.Objects.Add(flybyCamera.Id, flybyCamera);
                                    room.FlyByCameras.Add(flybyCamera.Id);
                                    break;
                            }
                        }

                        short flipRoom = reader.ReadInt16();
                        short flags1 = reader.ReadInt16();
                        byte waterLevel = reader.ReadByte();
                        byte mistLevel = reader.ReadByte();
                        byte reflectionLevel = reader.ReadByte();
                        short flags2 = reader.ReadInt16();

                        room.WaterLevel = waterLevel;
                        room.MistLevel = mistLevel;
                        room.ReflectionLevel = reflectionLevel;

                        if (flipRoom != -1)
                        {
                            var info = new PrjFlipInfo
                            {
                                _baseRoom = (short)i,
                                _flipRoom = flipRoom,
                                _group = (short)(flags2 & 0xff)
                            };

                            flipInfos.Add(info);
                        }

                        room.Flipped = false;
                        room.AlternateRoom = null;
                        room.AlternateGroup = -1;

                        if ((flags1 & 0x0200) != 0)
                            room.FlagReflection = true;
                        if ((flags1 & 0x0100) != 0)
                            room.FlagMist = true;
                        if ((flags1 & 0x0080) != 0)
                            room.FlagQuickSand = true;
                        if ((flags1 & 0x0020) != 0)
                            room.FlagOutside = true;
                        if ((flags1 & 0x0008) != 0)
                            room.FlagHorizon = true;
                        if ((flags1 & 0x0001) != 0)
                            room.FlagWater = true;

                        var tempBlocks = new PrjBlock[numXBlocks, numZBlocks];

                        for (int z = 0; z < room.NumZSectors; z++)
                        {
                            for (int x = 0; x < room.NumXSectors; x++)
                            {
                                var b = new PrjBlock
                                {
                                    _blockType = reader.ReadInt16(),
                                    _blockFlags1 = reader.ReadInt16(),
                                    _blockYfloor = reader.ReadInt16(),
                                    _blockYceiling = reader.ReadInt16(),
                                    _qaFaces = new sbyte[4]
                                };

                                for (int k = 0; k < 4; k++)
                                    b._qaFaces[k] = reader.ReadSByte();

                                b._wsFaces = new sbyte[4];
                                for (int k = 0; k < 4; k++)
                                    b._wsFaces[k] = reader.ReadSByte();

                                b._edFaces = new sbyte[4];
                                for (int k = 0; k < 4; k++)
                                    b._edFaces[k] = reader.ReadSByte();

                                b._rfFaces = new sbyte[4];
                                for (int k = 0; k < 4; k++)
                                    b._rfFaces[k] = reader.ReadSByte();

                                b._faces = new PrjFace[14];

                                for (int j = 0; j < 14; j++)
                                {
                                    var fc = new PrjFace
                                    {
                                        _txtType = reader.ReadInt16(),
                                        _txtIndex = reader.ReadByte(),
                                        _txtFlags = reader.ReadByte(),
                                        _txtRotation = reader.ReadByte(),
                                        _txtTriangle = reader.ReadByte()
                                    };

                                    reader.ReadInt16();

                                    b._faces[j] = (fc);
                                }

                                b._flags2 = reader.ReadInt16();
                                b._flags3 = reader.ReadInt16();

                                tempBlocks[x, z] = b;
                            }
                        }

                        tempRooms.Add(i, tempBlocks);

                        short lowest = 1024;
                        short highest = -1024;

                        for (int z = 1; z < room.NumZSectors - 1; z++)
                        {
                            for (int x = 1; x < room.NumXSectors - 1; x++)
                            {
                                var b = tempBlocks[x, z];

                                if (b._blockYfloor < lowest)
                                    lowest = b._blockYfloor;
                                if (b._blockYceiling > highest)
                                    highest = b._blockYceiling;
                            }
                        }

                        room.Position = new Vector3(-room.Position.X, lowest, room.Position.Z);

                        sbyte deltaCeilingMain = (sbyte)lowest;

                        for (int z = 0; z < room.NumZSectors; z++)
                        {
                            for (int x = 0; x < room.NumXSectors; x++)
                            {
                                var b = tempBlocks[room.NumXSectors - 1 - x, z];

                                sbyte deltaFloor = (sbyte)(b._blockYfloor - lowest);
                                sbyte deltaCeiling = (sbyte)(deltaCeilingMain - b._blockYceiling);

                                for (int j = 0; j < 4; j++)
                                    b._qaFaces[j] += deltaFloor;
                                for (int j = 0; j < 4; j++)
                                    b._edFaces[j] += deltaFloor;
                                for (int j = 0; j < 4; j++)
                                    b._wsFaces[j] -= deltaCeiling;
                                for (int j = 0; j < 4; j++)
                                    b._rfFaces[j] -= deltaCeiling;

                                var typ = BlockType.Floor;
                                switch (b._blockType)
                                {
                                    case 0x01:
                                        typ = BlockType.Floor;
                                        break;
                                    case 0x1e:
                                        typ = BlockType.BorderWall;
                                        break;
                                    case 0x0e:
                                        typ = BlockType.Wall;
                                        break;
                                    case 0x06:
                                        typ = BlockType.BorderWall; // BlockType.WallPortal;
                                        break;
                                    case 0x03:
                                        typ = BlockType.Floor; // BlockType.FloorPortal;
                                        break;
                                    case 0x05:
                                        typ = BlockType.Floor; // BlockType.CeilingPortal;
                                        break;
                                    case 0x07:
                                        typ = BlockType.Floor; // BlockType.FloorPortal;
                                        break;
                                }

                                room.Blocks[x, z] = new Block(typ, BlockFlags.None)
                                {
                                    QAFaces =
                                {
                                    [0] = b._qaFaces[3],
                                    [1] = b._qaFaces[0],
                                    [2] = b._qaFaces[1],
                                    [3] = b._qaFaces[2]
                                },
                                    EDFaces =
                                {
                                    [0] = b._edFaces[3],
                                    [1] = b._edFaces[0],
                                    [2] = b._edFaces[1],
                                    [3] = b._edFaces[2]
                                }
                                };

                                room.Blocks[x, z].WSFaces[0] = b._wsFaces[0];
                                room.Blocks[x, z].WSFaces[1] = b._wsFaces[3];
                                room.Blocks[x, z].WSFaces[2] = b._wsFaces[2];
                                room.Blocks[x, z].WSFaces[3] = b._wsFaces[1];

                                room.Blocks[x, z].RFFaces[0] = b._rfFaces[0];
                                room.Blocks[x, z].RFFaces[1] = b._rfFaces[3];
                                room.Blocks[x, z].RFFaces[2] = b._rfFaces[2];
                                room.Blocks[x, z].RFFaces[3] = b._rfFaces[1];

                                room.Blocks[x, z].SplitFoorType = (byte)b._flags3;

                                if ((b._blockFlags1 & 0x4000) != 0)
                                    room.Blocks[x, z].Flags |= BlockFlags.Monkey;
                                if ((b._blockFlags1 & 0x0020) != 0)
                                    room.Blocks[x, z].Flags |= BlockFlags.Box;
                                if ((b._blockFlags1 & 0x0010) != 0)
                                    room.Blocks[x, z].Flags |= BlockFlags.Death;
                                if ((b._blockFlags1 & 0x0200) != 0)
                                    room.Blocks[x, z].Climb[2] = true;
                                if ((b._blockFlags1 & 0x0100) != 0)
                                    room.Blocks[x, z].Climb[1] = true;
                                if ((b._blockFlags1 & 0x0080) != 0)
                                    room.Blocks[x, z].Climb[0] = true;
                                if ((b._blockFlags1 & 0x0040) != 0)
                                    room.Blocks[x, z].Climb[3] = true;

                                if ((x == 0 || z == 0 || x == room.NumXSectors - 1 || z == room.NumZSectors - 1))
                                {
                                    if ((b._blockFlags1 & 0x0008) == 0x0008 && (b._blockFlags1 & 0x1000) == 0)
                                        room.Blocks[x, z].WallOpacity = PortalOpacity.Opacity1;
                                    if ((b._blockFlags1 & 0x0008) == 0x0008 && (b._blockFlags1 & 0x1000) == 0x1000)
                                        room.Blocks[x, z].WallOpacity = PortalOpacity.Opacity2;
                                }
                                else
                                {
                                    if ((b._blockFlags1 & 0x0002) == 0x0002)
                                        room.Blocks[x, z].FloorOpacity = PortalOpacity.Opacity1;

                                    if ((b._blockFlags1 & 0x0004) == 0x0004)
                                        room.Blocks[x, z].CeilingOpacity = PortalOpacity.Opacity1;

                                    if ((b._blockFlags1 & 0x0800) == 0x0800)
                                        room.Blocks[x, z].FloorOpacity = PortalOpacity.Opacity2;

                                    if ((b._blockFlags1 & 0x0400) == 0x0400)
                                        room.Blocks[x, z].CeilingOpacity = PortalOpacity.Opacity2;
                                }
                            }
                        }

                        System.Diagnostics.Debug.Assert(ReferenceEquals(level.GetOrCreateDummyRoom(i), room));

                        progress += (i / (float)numRooms * 0.28f);

                        progressReporter.ReportProgress((int)progress, "");
                    }

                    progressReporter.ReportProgress(30, "Rooms loaded");

                    logger.Info("All rooms loaded");

                    // Read unused things indices
                    //      byte[] bufIndices=reader.ReadBytes(13136);

                    int dwNumThings = reader.ReadInt32(); // number of things in the map
                    int dwMaxThings = reader.ReadInt32(); // always 2000
                    reader.ReadBytes(dwMaxThings * 4);

                    int dwNumLights = reader.ReadInt32(); // number of lights in the map
                    reader.ReadBytes(768 * 4);

                    int dwNumTriggers = reader.ReadInt32(); // number of triggers in the map
                    reader.ReadBytes(512 * 4);


                    // Read texture
                    {
                        var stringBuffer = new byte[255];
                        int sb = 0;
                        while (true)
                        {
                            byte s = reader.ReadByte();
                            if (s == 0x20)
                                break;
                            if (s == 0x00)
                                continue;
                            stringBuffer[sb] = s;
                            sb++;
                        }

                        string textureFilename = System.Text.Encoding.ASCII.GetString(stringBuffer);
                        textureFilename = textureFilename.Replace('\0', ' ').Trim();
                        ResourceLoader.TryLoadingTexture(level, textureFilename, device, progressReporter);
                        progressReporter.ReportProgress(50, "Loaded texture '" + textureFilename + "'");
                    }

                    // Read textures
                    int numTextures = reader.ReadInt32();

                    progressReporter.ReportProgress(52, "Loading textures");
                    progressReporter.ReportProgress(52, "    Number of textures: " + numTextures);

                    var tempTextures = new List<PrjTexInfo>();
                    for (int t = 0; t < numTextures; t++)
                    {
                        var tmpTxt = new PrjTexInfo
                        {
                            _x = reader.ReadByte(),
                            _y = reader.ReadInt16()
                        };

                        reader.ReadInt16();
                        tmpTxt._width = reader.ReadByte();
                        reader.ReadByte();
                        tmpTxt._height = reader.ReadByte();

                        tempTextures.Add(tmpTxt);
                    }

                    // Read WAD file
                    {
                        var stringBuffer = new byte[255];
                        int sb = 0;
                        while (true)
                        {
                            byte s = reader.ReadByte();
                            if (s == 0x20)
                                break;
                            if (s == 0x00)
                                continue;
                            stringBuffer[sb] = s;
                            sb++;
                        }
                        string wadName = System.Text.Encoding.ASCII.GetString(stringBuffer);
                        wadName = wadName.Replace('\0', ' ').Trim();
                        ResourceLoader.TryLoadingWad(level, wadName, device, progressReporter);
                        progressReporter.ReportProgress(60, "WAD loaded");
                    }

                    // Write slots
                    const bool writeSlots = false;
                    using (var writerSlots = writeSlots ? new StreamWriter("slots.txt") : null)
                    {
                        int numSlots = reader.ReadInt32();
                        for (int i = 0; i < numSlots; i++)
                        {
                            short slotType = reader.ReadInt16();
                            if (slotType == 0x00)
                            {
                                writerSlots?.WriteLine(i + "\t" + "NOT DEFINED");
                                continue;
                            }

                            var stringBuffer = new byte[255];
                            int sb = 0;
                            while (true)
                            {
                                byte s = reader.ReadByte();
                                if (s == 0x20)
                                    break;
                                if (s == 0x00)
                                    continue;
                                stringBuffer[sb] = s;
                                sb++;
                            }

                            string slotName = System.Text.Encoding.ASCII.GetString(stringBuffer);
                            slotName = slotName.Replace('\0', ' ').Trim();

                            int objectId = reader.ReadInt32();

                            reader.ReadBytes(108);
                            writerSlots?.WriteLine(i + "\t" + slotName + "\t" + slotType + "\t" + objectId);
                        }
                    }

                    // Read animated textures
                    progressReporter.ReportProgress(61, "Loading animated textures and texture sounds");
                    int numAnimationRanges = reader.ReadInt32();
                    for (int i = 0; i < 40; i++)
                        reader.ReadInt32();
                    for (int i = 0; i < 256; i++)
                        reader.ReadInt32();

                    for (int i = 0; i < 40; i++)
                    {
                        int defined = reader.ReadInt32();
                        int firstTexture = reader.ReadInt32();
                        int lastTexture = reader.ReadInt32();

                        if (defined != 1)
                            continue;

                        var aSet = new AnimatedTextureSet();

                        for (int j = firstTexture; j <= lastTexture; j++)
                        {
                            int relative = j % 16;

                            int txtY = (int)Math.Floor(relative / 4.0f);
                            int txtX = relative - 4 * txtY;

                            txtX *= 64;
                            txtY *= 64;

                            int tile = (int)Math.Floor(j / 16.0f);

                            AnimatedTexture aTexture = new AnimatedTexture((short)txtX, (short)txtY, (short)tile);
                            aSet.Textures.Add(aTexture);
                        }

                        level.AnimatedTextures.Add(aSet);
                    }

                    for (int i = 0; i < 256; i++)
                    {
                        int relative = i % 16;

                        int txtY = (int)Math.Floor(relative / 4.0f);
                        int txtX = relative - 4 * txtY;

                        txtX *= 64;
                        txtY *= 64;

                        int tile = (int)Math.Floor(i / 16.0f);

                        var txtSound = new TextureSound((short)txtX, (short)txtY, (short)tile)
                        {
                            Sound = (TextureSounds)(reader.ReadByte())
                        };

                        level.TextureSounds.Add(txtSound);
                    }

                    // Fix rooms coordinates (in TRLE reference system is messed up...)
                    progressReporter.ReportProgress(65, "Flipping reference system");

                    int minX = level.Rooms.Where(room => room != null).Select(room => (int)room.Position.X)
                        .Concat(new[] { 1024 }).Min();

                    foreach (var room in level.Rooms.Where(room => room != null))
                    {
                        room.Position = new Vector3(room.Position.X - minX - room.NumXSectors + 10,
                            room.Position.Y,
                            room.Position.Z);
                    }

                    progressReporter.ReportProgress(67, "Building flipped rooms table");

                    for (int i = 0; i < level.Rooms.Length; i++)
                    {
                        if (level.Rooms[i] == null)
                            continue;

                        var room = level.Rooms[i];

                        foreach (var info in flipInfos)
                        {
                            if (info._baseRoom == i)
                            {
                                room.Flipped = true;
                                room.AlternateRoom = level.Rooms[info._flipRoom];
                                room.AlternateGroup = info._group;
                            }

                            if (info._flipRoom != i)
                                continue;

                            room.Flipped = true;
                            room.BaseRoom = level.Rooms[info._baseRoom];
                            room.AlternateGroup = info._group;
                            room.Position = new Vector3(level.Rooms[info._baseRoom].Position.X,
                                level.Rooms[info._baseRoom].Position.Y,
                                level.Rooms[info._baseRoom].Position.Z);
                        }
                    }

                    // Fix objects
                    progressReporter.ReportProgress(70, "Fixing objects positions and data");
                    foreach (var instance in level.Objects.Values.ToList())
                    {
                        instance.Position = new Vector3(
                            (instance.Room.NumXSectors - instance.Position.X - 1) * 1024 + 512,
                            -instance.Position.Y - instance.Room.Position.Y * 256,
                            instance.Position.Z * 1024 + 512);

                        switch (instance.Type)
                        {
                            case ObjectInstanceType.Moveable:
                                var moveable = (MoveableInstance)instance;
                                moveable.WadObjectId = moveable.WadObjectId;
                                level.Objects[instance.Id] = moveable;
                                break;
                            case ObjectInstanceType.Static:
                                var staticMesh = (StaticInstance)instance;
                                staticMesh.WadObjectId = staticMesh.WadObjectId;
                                level.Objects[instance.Id] = staticMesh;
                                break;
                            default:
                                level.Objects[instance.Id] = instance;
                                break;
                        }
                    }

                    var triggersToRemove = new List<int>();

                    // Fix triggers
                    progressReporter.ReportProgress(73, "Fixing triggers");
                    foreach (var instance in level.Triggers.Values.ToList())
                    {
                        if (instance.TargetType == TriggerTargetType.Object &&
                            !level.Objects.ContainsKey(instance.Target))
                        {
                            triggersToRemove.Add(instance.Id);
                            continue;
                        }

                        if (instance.X < 1)
                            instance.X = 1;
                        if (instance.X > instance.Room.NumXSectors - 2)
                            instance.X = (byte)(instance.Room.NumXSectors - 2);
                        if (instance.Z < 1)
                            instance.Z = 1;
                        if (instance.Z > instance.Room.NumZSectors - 2)
                            instance.Z = (byte)(instance.Room.NumZSectors - 2);

                        instance.X = (byte)(instance.Room.NumXSectors - instance.X - instance.NumXBlocks);

                        for (int x = instance.X; x < instance.X + instance.NumXBlocks; x++)
                        {
                            for (int z = instance.Z; z < instance.Z + instance.NumZBlocks; z++)
                            {
                                instance.Room.Blocks[x, z].Triggers.Add(instance.Id);
                            }
                        }

                        if (instance.TargetType == TriggerTargetType.Object &&
                            level.Objects[instance.Target].Type == ObjectInstanceType.FlyByCamera)
                        {
                            instance.TargetType = TriggerTargetType.FlyByCamera;
                            instance.Target = ((FlybyCameraInstance)level.Objects[instance.Target]).Sequence;
                        }

                        if (instance.TargetType == TriggerTargetType.Object &&
                            level.Objects[instance.Target].Type == ObjectInstanceType.Camera)
                        {
                            instance.TargetType = TriggerTargetType.Camera;
                        }

                        if (instance.TargetType == TriggerTargetType.Object &&
                            level.Objects[instance.Target].Type == ObjectInstanceType.Moveable &&
                            ((MoveableInstance)level.Objects[instance.Target]).WadObjectId == 422)
                        {
                            instance.TargetType = TriggerTargetType.Target;
                        }

                        if (instance.TargetType == TriggerTargetType.Object &&
                            level.Objects[instance.Target].Type == ObjectInstanceType.Sink)
                        {
                            instance.TargetType = TriggerTargetType.Sink;
                        }

                        level.Triggers[instance.Id] = instance;
                    }

                    if (triggersToRemove.Count != 0)
                    {
                        progressReporter.ReportProgress(75, "Found invalid triggers");
                        foreach (int trigger in triggersToRemove)
                        {
                            progressReporter.ReportProgress(75, "    Deleted trigger #" + trigger + " in room " +
                                                    level.Rooms.ReferenceIndexOf(level.Triggers[trigger].Room));
                            level.Triggers.Remove(trigger);
                        }
                    }

                    // Fix portals
                    progressReporter.ReportProgress(76, "Building portals");
                    foreach (var currentPortal in level.Portals.Values.ToList())
                    {
                        currentPortal.X = (byte)(currentPortal.Room.NumXSectors - currentPortal.NumXBlocks -
                                                    currentPortal.X);
                    }

                    foreach (var currentPortal in level.Portals.Values.ToList())
                    {
                        foreach (var otherPortal in level.Portals.Values.ToList())
                        {
                            if (ReferenceEquals(currentPortal, otherPortal))
                                continue;

                            if (portalThingIndices[currentPortal]._otherThingIndex != portalThingIndices[otherPortal]._thisThingIndex)
                                continue;

                            var currentRoom = currentPortal.Room;
                            var otherRoom = otherPortal.Room;

                            if (currentPortal.Direction == PortalDirection.North ||
                                currentPortal.Direction == PortalDirection.South ||
                                currentPortal.Direction == PortalDirection.East ||
                                currentPortal.Direction == PortalDirection.West)
                            {
                                for (int x = currentPortal.X; x < currentPortal.X + currentPortal.NumXBlocks; x++)
                                {
                                    for (int z = currentPortal.Z; z < currentPortal.Z + currentPortal.NumZBlocks; z++)
                                    {
                                        currentRoom.Blocks[x, z].WallPortal = currentPortal.Id;
                                    }
                                }

                                for (int x = otherPortal.X; x < otherPortal.X + otherPortal.NumXBlocks; x++)
                                {
                                    for (int z = otherPortal.Z; z < otherPortal.Z + otherPortal.NumZBlocks; z++)
                                    {
                                        otherPortal.Room.Blocks[x, z].WallPortal = otherPortal.Id;
                                    }
                                }
                            }

                            if (currentPortal.Direction == PortalDirection.Floor ||
                                currentPortal.Direction == PortalDirection.Ceiling)
                            {
                                int xMin = currentPortal.X;
                                int xMax = currentPortal.X + currentPortal.NumXBlocks;
                                int zMin = currentPortal.Z;
                                int zMax = currentPortal.Z + currentPortal.NumZBlocks;

                                int otherXmin = xMin + (int)(currentRoom.Position.X -
                                                                otherPortal.Room.Position.X);
                                int otherXmax = xMax + (int)(currentRoom.Position.X -
                                                                otherPortal.Room.Position.X);
                                int otherZmin = zMin + (int)(currentRoom.Position.Z -
                                                                otherPortal.Room.Position.Z);
                                int otherZmax = zMax + (int)(currentRoom.Position.Z -
                                                                otherPortal.Room.Position.Z);

                                for (int x = xMin; x < xMax; x++)
                                {
                                    for (int z = zMin; z < zMax; z++)
                                    {
                                        int lowX = x + (int)(currentRoom.Position.X - otherRoom.Position.X);
                                        int lowZ = z + (int)(currentRoom.Position.Z - otherRoom.Position.Z);

                                        if (currentPortal.Direction == PortalDirection.Floor)
                                        {
                                            int minHeight = currentRoom.GetLowestCorner(xMin, zMin, xMax, zMax);
                                            int maxHeight = otherRoom.GetHighestCorner(otherXmin, otherZmin, otherXmax,
                                                otherZmax);

                                            currentPortal.Room.Blocks[x, z].FloorPortal = currentPortal.Id;

                                            int h1 = currentRoom.Blocks[x, z].QAFaces[0];
                                            int h2 = currentRoom.Blocks[x, z].QAFaces[1];
                                            int h3 = currentRoom.Blocks[x, z].QAFaces[2];
                                            int h4 = currentRoom.Blocks[x, z].QAFaces[3];

                                            int lh1 = otherRoom.Blocks[lowX, lowZ].WSFaces[0];
                                            int lh2 = otherRoom.Blocks[lowX, lowZ].WSFaces[1];
                                            int lh3 = otherRoom.Blocks[lowX, lowZ].WSFaces[2];
                                            int lh4 = otherRoom.Blocks[lowX, lowZ].WSFaces[3];

                                            bool defined;

                                            if (Room.IsQuad(x, z, h1, h2, h3, h4, true) && h1 == minHeight &&
                                                otherRoom.Blocks[lowX, lowZ].Type != BlockType.Wall &&
                                                lh1 == maxHeight &&
                                                currentRoom.Blocks[x, z].Type != BlockType.Wall)
                                            {
                                                currentPortal.Room.Blocks[x, z].IsFloorSolid = false;
                                                defined = true;
                                            }
                                            else
                                            {
                                                currentRoom.Blocks[x, z].IsFloorSolid = true;
                                                defined = false;
                                            }

                                            if (Room.IsQuad(x, z, lh1, lh2, lh3, lh4, true) && defined &&
                                                lh1 == maxHeight)
                                            {
                                                otherPortal.Room.Blocks[lowX, lowZ].IsCeilingSolid = false;
                                            }
                                            else
                                            {
                                                otherPortal.Room.Blocks[lowX, lowZ].IsCeilingSolid = true;
                                            }
                                        }
                                        else
                                        {
                                            int minHeight = otherRoom.GetLowestCorner(otherXmin, otherZmin, otherXmax,
                                                otherZmax);
                                            int maxHeight = currentRoom.GetHighestCorner(xMin, zMin, xMax, zMax);

                                            currentPortal.Room.Blocks[x, z].CeilingPortal = currentPortal.Id;

                                            int h1 = currentRoom.Blocks[x, z].WSFaces[0];
                                            int h2 = currentRoom.Blocks[x, z].WSFaces[1];
                                            int h3 = currentRoom.Blocks[x, z].WSFaces[2];
                                            int h4 = currentRoom.Blocks[x, z].WSFaces[3];

                                            int lh1 = otherRoom.Blocks[lowX, lowZ].QAFaces[0];
                                            int lh2 = otherRoom.Blocks[lowX, lowZ].QAFaces[1];
                                            int lh3 = otherRoom.Blocks[lowX, lowZ].QAFaces[2];
                                            int lh4 = otherRoom.Blocks[lowX, lowZ].QAFaces[3];

                                            bool defined;

                                            if (Room.IsQuad(x, z, h1, h2, h3, h4, true) && h1 == maxHeight &&
                                                otherRoom.Blocks[lowX, lowZ].Type != BlockType.Wall &&
                                                lh1 == minHeight &&
                                                currentRoom.Blocks[x, z].Type != BlockType.Wall)
                                            {
                                                currentRoom.Blocks[x, z].IsCeilingSolid = false;
                                                defined = true;
                                            }
                                            else
                                            {
                                                currentRoom.Blocks[x, z].IsCeilingSolid = true;
                                                defined = false;
                                            }

                                            if (Room.IsQuad(x, z, lh1, lh2, lh3, lh4, true) && defined &&
                                                lh1 ==
                                                minHeight /*&& otherRoom.Blocks[lowX, lowZ].Type != BlockType.Wall*/)
                                            {
                                                otherPortal.Room.Blocks[lowX, lowZ].IsFloorSolid = false;
                                            }
                                            else
                                            {
                                                otherPortal.Room.Blocks[lowX, lowZ].IsFloorSolid = true;
                                            }
                                        }
                                    }
                                }
                            }

                            if ((!currentRoom.Flipped && !otherRoom.Flipped))
                            {
                                currentPortal.OtherId = otherPortal.Id;
                                otherPortal.OtherId = currentPortal.Id;
                                currentPortal.AdjoiningRoom = otherPortal.Room;
                                otherPortal.AdjoiningRoom = currentPortal.Room;
                            }
                            else if ((currentRoom.Flipped && otherRoom.Flipped))
                            {
                                currentPortal.OtherId = otherPortal.Id;
                                otherPortal.OtherId = currentPortal.Id;
                                currentPortal.AdjoiningRoom =
                                    otherRoom.BaseRoom ?? otherPortal.Room;
                                otherPortal.AdjoiningRoom =
                                    currentRoom.BaseRoom ?? currentPortal.Room;
                            }
                            else
                            {
                                if (!currentRoom.Flipped && otherRoom.Flipped)
                                {
                                    if (otherRoom.AlternateRoom != null)
                                    {
                                        currentPortal.OtherId = otherPortal.Id;
                                        currentPortal.AdjoiningRoom = otherPortal.Room;
                                    }
                                    else
                                    {
                                        currentPortal.AdjoiningRoom = otherRoom.BaseRoom;
                                    }

                                    otherPortal.OtherId = currentPortal.Id;
                                    otherPortal.AdjoiningRoom = currentPortal.Room;
                                }
                                if (currentRoom.Flipped && !otherRoom.Flipped)
                                {
                                    if (currentRoom.AlternateRoom != null)
                                    {
                                        otherPortal.OtherId = currentPortal.Id;
                                        otherPortal.AdjoiningRoom = currentPortal.Room;
                                    }
                                    else
                                    {
                                        otherPortal.AdjoiningRoom = currentRoom.BaseRoom;
                                    }

                                    currentPortal.OtherId = otherPortal.Id;
                                    currentPortal.AdjoiningRoom = otherPortal.Room;
                                }
                            }

                            level.Portals[currentPortal.Id] = currentPortal;
                            level.Portals[otherPortal.Id] = otherPortal;

                            break;
                        }
                    }

                    // Fix faces
                    progressReporter.ReportProgress(85, "Building faces and geometry");
                    for (int i = 0; i < level.Rooms.Length; i++)
                    {
                        var room = level.Rooms[i];
                        if (room == null)
                            continue;

                        for (int j = 0; j < room.Lights.Count; j++)
                        {
                            var light = room.Lights[j];
                            light.Position = new Vector3(
                                room.NumXSectors * 1024.0f - light.Position.X,
                                light.Position.Y - room.Position.Y * 256,
                                light.Position.Z);
                        }

                        for (int z = 0; z < room.NumZSectors; z++)
                        {
                            for (int x = 0; x < room.NumXSectors; x++)
                            {
                                var b = tempRooms[i][room.NumXSectors - 1 - x, z];

                                for (int j = 0; j < 14; j++)
                                {
                                    var prjFace = b._faces[j];

                                    bool isFlipped = (prjFace._txtFlags & 0x80) == 0x80;
                                    bool isTransparent = (prjFace._txtFlags & 0x08) != 0;
                                    bool isDoubleSided = (prjFace._txtFlags & 0x04) != 0;

                                    prjFace._isFlipped = isFlipped;

                                    if (prjFace._txtType == 0x0007)
                                    {
                                        prjFace._txtIndex =
                                            (short)(((prjFace._txtFlags & 0x03) << 8) + prjFace._txtIndex);

                                        PrjTexInfo texInfo = tempTextures[prjFace._txtIndex];
                                        bool textureFound = false;

                                        byte newX = (byte)(texInfo._x % 256);
                                        byte newY = (byte)(texInfo._y % 256);
                                        short tile = (short)Math.Floor((float)texInfo._y / 256);

                                        foreach (var sample in level.TextureSamples)
                                        {
                                            var currentTexture = sample.Value;
                                            if (currentTexture.X == newX && currentTexture.Y == newY &&
                                                currentTexture.Transparent == isTransparent &&
                                                currentTexture.DoubleSided == isDoubleSided &&
                                                currentTexture.Width == (texInfo._width + 1) &&
                                                currentTexture.Height == (texInfo._height + 1) &&
                                                currentTexture.Page == tile)
                                            {
                                                prjFace._newId = sample.Key;
                                                var texture = sample.Value;
                                                // texture.UsageCount++;
                                                level.TextureSamples[texture.Id] = texture;
                                                textureFound = true;
                                                break;
                                            }
                                        }

                                        if (!textureFound)
                                        {
                                            var texture = new LevelTexture
                                            {
                                                X = newX,
                                                Y = newY,
                                                Page = tile,
                                                Transparent = isTransparent,
                                                DoubleSided = isDoubleSided,
                                                Width = (short)(texInfo._width + 1),
                                                Height = (short)(texInfo._height + 1),
                                                Id = level.TextureSamples.Count
                                            };
                                            //texture.UsageCount = 1;

                                            prjFace._newId = texture.Id;

                                            level.TextureSamples.Add(texture.Id, texture);
                                        }
                                    }

                                    b._faces[j] = prjFace;
                                }

                                tempRooms[i][room.NumXSectors - 1 - x, z] = b;
                            }
                        }

                        room.BuildGeometry();

                        for (int z = 0; z < room.NumZSectors; z++)
                        {
                            for (int x = 0; x < room.NumXSectors; x++)
                            {
                                var newBlock = room.Blocks[x, z];
                                var prjBlock = tempRooms[i][room.NumXSectors - 1 - x, z];

                                newBlock.NoCollisionFloor =
                                    (((prjBlock._flags2 & 0x04) != 0) || ((prjBlock._flags2 & 0x02) != 0));
                                newBlock.NoCollisionCeiling =
                                    (((prjBlock._flags2 & 0x10) != 0) || ((prjBlock._flags2 & 0x08) != 0));

                                if ((prjBlock._flags2 & 0x0040) != 0)
                                    newBlock.Flags |= BlockFlags.Beetle;
                                if ((prjBlock._flags2 & 0x0020) != 0)
                                    newBlock.Flags |= BlockFlags.TriggerTriggerer;

                                short h1 = room.Blocks[x, z].QAFaces[0];
                                short h2 = room.Blocks[x, z].QAFaces[1];
                                short h3 = room.Blocks[x, z].QAFaces[2];
                                short h4 = room.Blocks[x, z].QAFaces[3];

                                bool isFloorSplitted = !Room.IsQuad(x, z, h1, h2, h3, h4, false);

                                newBlock.SplitFloor = (isFloorSplitted && prjBlock._flags3 == 0x01);

                                for (int n = 0; n < 14; n++)
                                {
                                    var theFace = prjBlock._faces[n];
                                    Block otherBlock = null;
                                    sbyte adjustRotation = 0;

                                    int faceIndex = -1;

                                    int x2 = -1;
                                    int z2 = -1;

                                    // BLOCK_TEX_FLOOR
                                    if (n == 0)
                                    {
                                        faceIndex = (int)BlockFaces.Floor;
                                        otherBlock = null;
                                    }

                                    // BLOCK_TEX_CEILING
                                    if (n == 1)
                                    {
                                        faceIndex = (int)BlockFaces.Ceiling;
                                        otherBlock = null;
                                    }

                                    // BLOCK_TEX_N4 (North QA)
                                    if (n == 2)
                                    {
                                        if (newBlock.Faces[(int)BlockFaces.SouthQA].Defined)
                                        {
                                            faceIndex = (int)BlockFaces.SouthQA;
                                            otherBlock = null;
                                        }
                                        else
                                        {
                                            if (z > 0)
                                            {
                                                faceIndex = (int)BlockFaces.NorthQA;
                                                otherBlock = room.Blocks[x, z - 1];
                                                x2 = x;
                                                z2 = z - 1;
                                            }
                                        }

                                        adjustRotation = -1;
                                    }

                                    // BLOCK_TEX_N1 (North RF)
                                    if (n == 3)
                                    {
                                        if (newBlock.Faces[(int)BlockFaces.SouthRF].Defined ||
                                            newBlock.Faces[(int)BlockFaces.SouthWS].Defined)
                                        {
                                            if (newBlock.Faces[(int)BlockFaces.SouthRF].Defined &&
                                                !newBlock.Faces[(int)BlockFaces.SouthWS].Defined)
                                            {
                                                faceIndex = (int)BlockFaces.SouthRF;
                                                otherBlock = null;
                                            }
                                            else if (!newBlock.Faces[(int)BlockFaces.SouthRF].Defined &&
                                                        newBlock.Faces[(int)BlockFaces.SouthWS].Defined)
                                            {
                                                faceIndex = (int)BlockFaces.SouthWS;
                                                otherBlock = null;
                                            }
                                            else
                                            {
                                                faceIndex = (int)BlockFaces.SouthRF;
                                                otherBlock = null;
                                            }
                                        }
                                        else
                                        {
                                            if (z > 0)
                                            {
                                                otherBlock = room.Blocks[x, z - 1];

                                                if (otherBlock.Faces[(int)BlockFaces.NorthRF].Defined &&
                                                    !otherBlock.Faces[(int)BlockFaces.NorthWS].Defined)
                                                {
                                                    faceIndex = (int)BlockFaces.NorthRF;
                                                }
                                                else if (!otherBlock.Faces[(int)BlockFaces.NorthRF].Defined &&
                                                            otherBlock.Faces[(int)BlockFaces.NorthWS].Defined)
                                                {
                                                    faceIndex = (int)BlockFaces.NorthWS;
                                                }
                                                else
                                                {
                                                    faceIndex = (int)BlockFaces.NorthRF;
                                                }

                                                x2 = x;
                                                z2 = z - 1;
                                            }
                                        }

                                        adjustRotation = -1;
                                    }

                                    // BLOCK_TEX_N3 (North middle)
                                    if (n == 4)
                                    {
                                        if (newBlock.Faces[(int)BlockFaces.SouthMiddle].Defined)
                                        {
                                            faceIndex = (int)BlockFaces.SouthMiddle;
                                            otherBlock = null;
                                        }
                                        else
                                        {
                                            if (z > 0)
                                            {
                                                faceIndex = (int)BlockFaces.NorthMiddle;
                                                otherBlock = room.Blocks[x, z - 1];
                                                x2 = x;
                                                z2 = z - 1;
                                            }
                                        }

                                        adjustRotation = -1;
                                    }

                                    // BLOCK_TEX_W4 (West QA)
                                    if (n == 5)
                                    {
                                        if (newBlock.Faces[(int)BlockFaces.EastQA].Defined)
                                        {
                                            faceIndex = (int)BlockFaces.EastQA;
                                            otherBlock = null;
                                        }
                                        else
                                        {
                                            if (x < room.NumXSectors - 1)
                                            {
                                                faceIndex = (int)BlockFaces.WestQA;
                                                otherBlock = room.Blocks[x + 1, z];
                                                x2 = x + 1;
                                                z2 = z;
                                            }
                                        }

                                        adjustRotation = -1;
                                    }

                                    // BLOCK_TEX_W1 (West RF)
                                    if (n == 6)
                                    {
                                        if (newBlock.Faces[(int)BlockFaces.EastRF].Defined ||
                                            newBlock.Faces[(int)BlockFaces.EastWS].Defined)
                                        {
                                            if (newBlock.Faces[(int)BlockFaces.EastRF].Defined &&
                                                !newBlock.Faces[(int)BlockFaces.EastWS].Defined)
                                            {
                                                faceIndex = (int)BlockFaces.EastRF;
                                                otherBlock = null;
                                            }
                                            else if (!newBlock.Faces[(int)BlockFaces.EastRF].Defined &&
                                                        newBlock.Faces[(int)BlockFaces.EastWS].Defined)
                                            {
                                                faceIndex = (int)BlockFaces.EastWS;
                                                otherBlock = null;
                                            }
                                            else
                                            {
                                                faceIndex = (int)BlockFaces.EastRF;
                                                otherBlock = null;
                                            }
                                        }
                                        else
                                        {
                                            if (x < room.NumXSectors - 1)
                                            {
                                                otherBlock = room.Blocks[x + 1, z];

                                                if (otherBlock.Faces[(int)BlockFaces.WestRF].Defined &&
                                                    !otherBlock.Faces[(int)BlockFaces.WestWS].Defined)
                                                {
                                                    faceIndex = (int)BlockFaces.WestRF;
                                                }
                                                else if (!otherBlock.Faces[(int)BlockFaces.WestRF].Defined &&
                                                            otherBlock.Faces[(int)BlockFaces.WestWS].Defined)
                                                {
                                                    faceIndex = (int)BlockFaces.WestWS;
                                                }
                                                else
                                                {
                                                    faceIndex = (int)BlockFaces.WestRF;
                                                }

                                                x2 = x + 1;
                                                z2 = z;
                                            }
                                        }

                                        adjustRotation = -1;
                                    }

                                    // BLOCK_TEX_W3 (West middle)
                                    if (n == 7)
                                    {
                                        if (newBlock.Faces[(int)BlockFaces.EastMiddle].Defined)
                                        {
                                            faceIndex = (int)BlockFaces.EastMiddle;
                                            otherBlock = null;
                                        }
                                        else
                                        {
                                            if (x < room.NumXSectors - 1)
                                            {
                                                faceIndex = (int)BlockFaces.WestMiddle;
                                                otherBlock = room.Blocks[x + 1, z];
                                                x2 = x + 1;
                                                z2 = z;
                                            }
                                        }

                                        adjustRotation = -1;
                                    }

                                    // BLOCK_TEX_N5 (North ED)
                                    if (n == 10)
                                    {
                                        if (newBlock.Faces[(int)BlockFaces.SouthED].Defined)
                                        {
                                            faceIndex = (int)BlockFaces.SouthED;
                                            otherBlock = null;
                                        }
                                        else
                                        {
                                            if (z > 0)
                                            {
                                                faceIndex = (int)BlockFaces.NorthED;
                                                otherBlock = room.Blocks[x, z - 1];
                                                x2 = x;
                                                z2 = z - 1;
                                            }
                                        }

                                        adjustRotation = -1;
                                    }

                                    // BLOCK_TEX_N2 (North WS)
                                    if (n == 11)
                                    {
                                        if (newBlock.Faces[(int)BlockFaces.SouthRF].Defined ||
                                            newBlock.Faces[(int)BlockFaces.SouthWS].Defined)
                                        {
                                            if (newBlock.Faces[(int)BlockFaces.SouthRF].Defined &&
                                                newBlock.Faces[(int)BlockFaces.SouthWS].Defined)
                                            {
                                                faceIndex = (int)BlockFaces.SouthWS;
                                                otherBlock = null;
                                            }
                                        }
                                        else
                                        {
                                            if (z > 0)
                                            {
                                                otherBlock = room.Blocks[x, z - 1];

                                                if (otherBlock.Faces[(int)BlockFaces.NorthRF].Defined &&
                                                    otherBlock.Faces[(int)BlockFaces.NorthWS].Defined)
                                                {
                                                    faceIndex = (int)BlockFaces.NorthWS;
                                                }

                                                x2 = x;
                                                z2 = z - 1;
                                            }
                                        }

                                        adjustRotation = -1;
                                    }

                                    // BLOCK_TEX_W5 (West ED)
                                    if (n == 12)
                                    {
                                        if (newBlock.Faces[(int)BlockFaces.EastED].Defined)
                                        {
                                            faceIndex = (int)BlockFaces.EastED;
                                            otherBlock = null;
                                        }
                                        else
                                        {
                                            if (x < room.NumXSectors - 1)
                                            {
                                                faceIndex = (int)BlockFaces.WestED;
                                                otherBlock = room.Blocks[x + 1, z];
                                                x2 = x + 1;
                                                z2 = z;
                                            }
                                        }

                                        adjustRotation = -1;
                                    }

                                    // BLOCK_TEX_W2 (West WS)
                                    if (n == 13)
                                    {
                                        if (newBlock.Faces[(int)BlockFaces.EastRF].Defined ||
                                            newBlock.Faces[(int)BlockFaces.EastWS].Defined)
                                        {
                                            if (newBlock.Faces[(int)BlockFaces.EastRF].Defined &&
                                                newBlock.Faces[(int)BlockFaces.EastWS].Defined)
                                            {
                                                faceIndex = (int)BlockFaces.EastWS;
                                                otherBlock = null;
                                            }
                                        }
                                        else
                                        {
                                            if (x < room.NumXSectors - 1)
                                            {
                                                otherBlock = room.Blocks[x + 1, z];

                                                if (otherBlock.Faces[(int)BlockFaces.WestRF].Defined &&
                                                    otherBlock.Faces[(int)BlockFaces.WestWS].Defined)
                                                {
                                                    faceIndex = (int)BlockFaces.WestWS;
                                                }

                                                x2 = x + 1;
                                                z2 = z;
                                            }
                                        }

                                        adjustRotation = -1;
                                    }


                                    // BLOCK_TEX_F_NENW (Floor Triangle 2)
                                    if (n == 8)
                                    {
                                        faceIndex = (int)BlockFaces.FloorTriangle2;
                                        otherBlock = null;
                                    }

                                    // BLOCK_TEX_C_NENW (Ceiling Triangle 2)
                                    if (n == 9)
                                    {
                                        faceIndex = (int)BlockFaces.CeilingTriangle2;
                                        otherBlock = null;
                                    }

                                    var theBlock = otherBlock ?? newBlock;

                                    if (faceIndex != -1)
                                    {
                                        bool isFloor = (faceIndex == (int)BlockFaces.Floor ||
                                                        faceIndex == (int)BlockFaces.FloorTriangle2);
                                        bool isCeiling = (faceIndex == (int)BlockFaces.Ceiling ||
                                                            faceIndex == (int)BlockFaces.CeilingTriangle2);

                                        switch (theFace._txtType)
                                        {
                                            case 0x07:
                                                var texture2 = level.TextureSamples[(short)theFace._newId];
                                                var uv = new Vector2[4];

                                                int yBlock = texture2.Page / 8;
                                                int xBlock = texture2.Page % 8;

                                                uv[0] = new Vector2((xBlock * 256.0f + texture2.X + 0.5f) / 2048.0f,
                                                    (yBlock * 256.0f + texture2.Y + 0.5f) / 2048.0f);
                                                uv[1] = new Vector2(
                                                    (xBlock * 256.0f + texture2.X + texture2.Width - 0.5f) / 2048.0f,
                                                    (yBlock * 256.0f + texture2.Y + 0.5f) / 2048.0f);

                                                uv[2] = new Vector2(
                                                    (xBlock * 256.0f + texture2.X + texture2.Width - 0.5f) / 2048.0f,
                                                    (yBlock * 256.0f + texture2.Y + texture2.Height - 0.5f) / 2048.0f);
                                                uv[3] = new Vector2((xBlock * 256.0f + texture2.X + 0.5f) / 2048.0f,
                                                    (yBlock * 256.0f + texture2.Y + texture2.Height - 0.5f) / 2048.0f);

                                                sbyte newRot = (sbyte)(theFace._txtRotation);
                                                newRot++;

                                                if (theBlock.Faces[faceIndex].Shape == BlockFaceShape.Rectangle)
                                                    newRot = (sbyte)(newRot % 4);
                                                if (theBlock.Faces[faceIndex].Shape == BlockFaceShape.Triangle)
                                                    newRot = (sbyte)(newRot % 3);

                                                if (theBlock.Faces[faceIndex].Defined && theBlock.Faces[faceIndex].Shape ==
                                                    BlockFaceShape.Triangle)
                                                {
                                                    if (theFace._isFlipped)
                                                    {
                                                        if (theFace._txtTriangle == 0)
                                                        {
                                                            if (isFloor || isCeiling)
                                                            {
                                                                if (theBlock.RealSplitFloor == 0)
                                                                {
                                                                    if (faceIndex == (int)BlockFaces.Floor ||
                                                                        faceIndex == (int)BlockFaces.Ceiling)
                                                                    {
                                                                        theBlock.Faces[faceIndex].TriangleUV[0] = uv[1];
                                                                        theBlock.Faces[faceIndex].TriangleUV[1] = uv[2];
                                                                        theBlock.Faces[faceIndex].TriangleUV[2] = uv[0];

                                                                        newRot = (sbyte)(newRot + 2);
                                                                    }

                                                                    if (faceIndex == (int)BlockFaces.FloorTriangle2 ||
                                                                        faceIndex == (int)BlockFaces.CeilingTriangle2)
                                                                    {
                                                                        theBlock.Faces[faceIndex].TriangleUV2[0] = uv[1];
                                                                        theBlock.Faces[faceIndex].TriangleUV2[1] = uv[2];
                                                                        theBlock.Faces[faceIndex].TriangleUV2[2] = uv[0];

                                                                        newRot = (sbyte)(newRot + 1);
                                                                    }
                                                                }
                                                                else
                                                                {
                                                                    if (faceIndex == (int)BlockFaces.Floor ||
                                                                        faceIndex == (int)BlockFaces.Ceiling)
                                                                    {
                                                                        theBlock.Faces[faceIndex].TriangleUV[0] = uv[1];
                                                                        theBlock.Faces[faceIndex].TriangleUV[1] = uv[2];
                                                                        theBlock.Faces[faceIndex].TriangleUV[2] = uv[0];
                                                                    }

                                                                    if (faceIndex == (int)BlockFaces.FloorTriangle2 ||
                                                                        faceIndex == (int)BlockFaces.CeilingTriangle2)
                                                                    {
                                                                        theBlock.Faces[faceIndex].TriangleUV2[0] = uv[1];
                                                                        theBlock.Faces[faceIndex].TriangleUV2[1] = uv[2];
                                                                        theBlock.Faces[faceIndex].TriangleUV2[2] = uv[0];

                                                                        newRot = (sbyte)(newRot + 1);
                                                                    }
                                                                }
                                                            }
                                                            else
                                                            {
                                                                theBlock.Faces[faceIndex].TriangleUV[0] = uv[1];
                                                                theBlock.Faces[faceIndex].TriangleUV[1] = uv[2];
                                                                theBlock.Faces[faceIndex].TriangleUV[2] = uv[0];

                                                                newRot = (sbyte)(newRot + 2);
                                                            }

                                                            theBlock.Faces[faceIndex].TextureTriangle =
                                                                TextureTileType.TriangleNW;
                                                        }

                                                        if (theFace._txtTriangle == 1)
                                                        {
                                                            if (isFloor || isCeiling)
                                                            {
                                                                if (theBlock.RealSplitFloor == 0)
                                                                {
                                                                    if (faceIndex == (int)BlockFaces.Floor ||
                                                                        faceIndex == (int)BlockFaces.Ceiling)
                                                                    {
                                                                        theBlock.Faces[faceIndex].TriangleUV[0] = uv[0];
                                                                        theBlock.Faces[faceIndex].TriangleUV[1] = uv[3];
                                                                        theBlock.Faces[faceIndex].TriangleUV[2] = uv[1];

                                                                        newRot = (sbyte)(newRot + 2);
                                                                    }

                                                                    if (faceIndex == (int)BlockFaces.FloorTriangle2 ||
                                                                        faceIndex == (int)BlockFaces.CeilingTriangle2)
                                                                    {
                                                                        theBlock.Faces[faceIndex].TriangleUV2[0] = uv[0];
                                                                        theBlock.Faces[faceIndex].TriangleUV2[1] = uv[3];
                                                                        theBlock.Faces[faceIndex].TriangleUV2[2] = uv[1];

                                                                        newRot = (sbyte)(newRot + 1);
                                                                    }
                                                                }
                                                                else
                                                                {
                                                                    if (faceIndex == (int)BlockFaces.Floor ||
                                                                        faceIndex == (int)BlockFaces.Ceiling)
                                                                    {
                                                                        theBlock.Faces[faceIndex].TriangleUV[0] = uv[0];
                                                                        theBlock.Faces[faceIndex].TriangleUV[1] = uv[3];
                                                                        theBlock.Faces[faceIndex].TriangleUV[2] = uv[1];
                                                                    }

                                                                    if (faceIndex == (int)BlockFaces.FloorTriangle2 ||
                                                                        faceIndex == (int)BlockFaces.CeilingTriangle2)
                                                                    {
                                                                        theBlock.Faces[faceIndex].TriangleUV2[0] = uv[0];
                                                                        theBlock.Faces[faceIndex].TriangleUV2[1] = uv[3];
                                                                        theBlock.Faces[faceIndex].TriangleUV2[2] = uv[1];

                                                                        newRot = (sbyte)(newRot + 1);
                                                                    }
                                                                }
                                                            }
                                                            else
                                                            {
                                                                theBlock.Faces[faceIndex].TriangleUV[0] = uv[0];
                                                                theBlock.Faces[faceIndex].TriangleUV[1] = uv[3];
                                                                theBlock.Faces[faceIndex].TriangleUV[2] = uv[1];

                                                                newRot = (sbyte)(newRot + 2);
                                                            }

                                                            theBlock.Faces[faceIndex].TextureTriangle =
                                                                TextureTileType.TriangleNE;
                                                        }

                                                        if (theFace._txtTriangle == 3)
                                                        {
                                                            if (isFloor || isCeiling)
                                                            {
                                                                if (theBlock.RealSplitFloor == 0)
                                                                {
                                                                    if (faceIndex == (int)BlockFaces.Floor ||
                                                                        faceIndex == (int)BlockFaces.Ceiling)
                                                                    {
                                                                        theBlock.Faces[faceIndex].TriangleUV[0] = uv[2];
                                                                        theBlock.Faces[faceIndex].TriangleUV[1] = uv[1];
                                                                        theBlock.Faces[faceIndex].TriangleUV[2] = uv[3];

                                                                        newRot = (sbyte)(newRot + 2);
                                                                    }

                                                                    if (faceIndex == (int)BlockFaces.FloorTriangle2 ||
                                                                        faceIndex == (int)BlockFaces.CeilingTriangle2)
                                                                    {
                                                                        theBlock.Faces[faceIndex].TriangleUV2[0] = uv[2];
                                                                        theBlock.Faces[faceIndex].TriangleUV2[1] = uv[1];
                                                                        theBlock.Faces[faceIndex].TriangleUV2[2] = uv[3];

                                                                        newRot = (sbyte)(newRot + 1);
                                                                    }
                                                                }
                                                                else
                                                                {
                                                                    if (faceIndex == (int)BlockFaces.Floor ||
                                                                        faceIndex == (int)BlockFaces.Ceiling)
                                                                    {
                                                                        theBlock.Faces[faceIndex].TriangleUV[0] = uv[2];
                                                                        theBlock.Faces[faceIndex].TriangleUV[1] = uv[1];
                                                                        theBlock.Faces[faceIndex].TriangleUV[2] = uv[3];
                                                                    }

                                                                    if (faceIndex == (int)BlockFaces.FloorTriangle2 ||
                                                                        faceIndex == (int)BlockFaces.CeilingTriangle2)
                                                                    {
                                                                        theBlock.Faces[faceIndex].TriangleUV2[0] = uv[2];
                                                                        theBlock.Faces[faceIndex].TriangleUV2[1] = uv[1];
                                                                        theBlock.Faces[faceIndex].TriangleUV2[2] = uv[3];

                                                                        newRot = (sbyte)(newRot + 1);
                                                                    }
                                                                }
                                                            }
                                                            else
                                                            {
                                                                theBlock.Faces[faceIndex].TriangleUV[0] = uv[2];
                                                                theBlock.Faces[faceIndex].TriangleUV[1] = uv[1];
                                                                theBlock.Faces[faceIndex].TriangleUV[2] = uv[3];

                                                                newRot = (sbyte)(newRot + 2);
                                                            }

                                                            theBlock.Faces[faceIndex].TextureTriangle =
                                                                TextureTileType.TriangleSW;
                                                        }

                                                        if (theFace._txtTriangle == 2)
                                                        {
                                                            if (isFloor || isCeiling)
                                                            {
                                                                if (theBlock.RealSplitFloor == 0)
                                                                {
                                                                    if (faceIndex == (int)BlockFaces.Floor ||
                                                                        faceIndex == (int)BlockFaces.Ceiling)
                                                                    {
                                                                        theBlock.Faces[faceIndex].TriangleUV[0] = uv[3];
                                                                        theBlock.Faces[faceIndex].TriangleUV[1] = uv[2];
                                                                        theBlock.Faces[faceIndex].TriangleUV[2] = uv[0];

                                                                        newRot = (sbyte)(newRot + 2);
                                                                    }

                                                                    if (faceIndex == (int)BlockFaces.FloorTriangle2 ||
                                                                        faceIndex == (int)BlockFaces.CeilingTriangle2)
                                                                    {
                                                                        theBlock.Faces[faceIndex].TriangleUV2[0] = uv[3];
                                                                        theBlock.Faces[faceIndex].TriangleUV2[1] = uv[2];
                                                                        theBlock.Faces[faceIndex].TriangleUV2[2] = uv[0];

                                                                        newRot = (sbyte)(newRot + 1);
                                                                    }
                                                                }
                                                                else
                                                                {
                                                                    if (faceIndex == (int)BlockFaces.Floor ||
                                                                        faceIndex == (int)BlockFaces.Ceiling)
                                                                    {
                                                                        theBlock.Faces[faceIndex].TriangleUV[0] = uv[3];
                                                                        theBlock.Faces[faceIndex].TriangleUV[1] = uv[2];
                                                                        theBlock.Faces[faceIndex].TriangleUV[2] = uv[0];
                                                                    }

                                                                    if (faceIndex == (int)BlockFaces.FloorTriangle2 ||
                                                                        faceIndex == (int)BlockFaces.CeilingTriangle2)
                                                                    {
                                                                        theBlock.Faces[faceIndex].TriangleUV2[0] = uv[3];
                                                                        theBlock.Faces[faceIndex].TriangleUV2[1] = uv[2];
                                                                        theBlock.Faces[faceIndex].TriangleUV2[2] = uv[0];

                                                                        newRot = (sbyte)(newRot + 1);
                                                                    }
                                                                }
                                                            }
                                                            else
                                                            {
                                                                theBlock.Faces[faceIndex].TriangleUV[0] = uv[3];
                                                                theBlock.Faces[faceIndex].TriangleUV[1] = uv[2];
                                                                theBlock.Faces[faceIndex].TriangleUV[2] = uv[0];

                                                                newRot = (sbyte)(newRot + 2);
                                                            }

                                                            theBlock.Faces[faceIndex].TextureTriangle =
                                                                TextureTileType.TriangleSE;
                                                        }
                                                    }
                                                    else
                                                    {
                                                        if (theFace._txtTriangle == 0)
                                                        {
                                                            if (isFloor || isCeiling)
                                                            {
                                                                if (theBlock.RealSplitFloor == 0)
                                                                {
                                                                    if (faceIndex == (int)BlockFaces.Floor ||
                                                                        faceIndex == (int)BlockFaces.Ceiling) // CORRETTO
                                                                    {
                                                                        theBlock.Faces[faceIndex].TriangleUV[0] = uv[0];
                                                                        theBlock.Faces[faceIndex].TriangleUV[1] = uv[1];
                                                                        theBlock.Faces[faceIndex].TriangleUV[2] = uv[3];

                                                                        newRot = (sbyte)(newRot + 2);
                                                                    }

                                                                    if (faceIndex == (int)BlockFaces.FloorTriangle2 ||
                                                                        faceIndex == (int)BlockFaces.CeilingTriangle2
                                                                    ) // CORRETTO
                                                                    {
                                                                        theBlock.Faces[faceIndex].TriangleUV2[0] = uv[0];
                                                                        theBlock.Faces[faceIndex].TriangleUV2[1] = uv[1];
                                                                        theBlock.Faces[faceIndex].TriangleUV2[2] = uv[3];

                                                                        newRot = (sbyte)(newRot + 1);
                                                                    }
                                                                }
                                                                else
                                                                {
                                                                    if (faceIndex == (int)BlockFaces.Floor ||
                                                                        faceIndex == (int)BlockFaces.Ceiling) // CORRETTO
                                                                    {
                                                                        theBlock.Faces[faceIndex].TriangleUV[0] = uv[0];
                                                                        theBlock.Faces[faceIndex].TriangleUV[1] = uv[1];
                                                                        theBlock.Faces[faceIndex].TriangleUV[2] = uv[3];
                                                                    }

                                                                    if (faceIndex == (int)BlockFaces.FloorTriangle2 ||
                                                                        faceIndex == (int)BlockFaces.CeilingTriangle2
                                                                    ) // CORRETTO
                                                                    {
                                                                        theBlock.Faces[faceIndex].TriangleUV2[0] = uv[0];
                                                                        theBlock.Faces[faceIndex].TriangleUV2[1] = uv[1];
                                                                        theBlock.Faces[faceIndex].TriangleUV2[2] = uv[3];

                                                                        newRot = (sbyte)(newRot + 1);
                                                                    }
                                                                }
                                                            }
                                                            else
                                                            {
                                                                theBlock.Faces[faceIndex].TriangleUV[0] = uv[0];
                                                                theBlock.Faces[faceIndex].TriangleUV[1] = uv[1];
                                                                theBlock.Faces[faceIndex].TriangleUV[2] = uv[3];

                                                                newRot = (sbyte)(newRot + 2);
                                                            }

                                                            theBlock.Faces[faceIndex].TextureTriangle =
                                                                TextureTileType.TriangleNW;
                                                        }

                                                        if (theFace._txtTriangle == 1)
                                                        {
                                                            if (isFloor || isCeiling)
                                                            {
                                                                if (theBlock.RealSplitFloor == 0)
                                                                {
                                                                    if (faceIndex == (int)BlockFaces.Floor ||
                                                                        faceIndex == (int)BlockFaces.Ceiling) // CORRETTO
                                                                    {
                                                                        theBlock.Faces[faceIndex].TriangleUV[0] = uv[1];
                                                                        theBlock.Faces[faceIndex].TriangleUV[1] = uv[2];
                                                                        theBlock.Faces[faceIndex].TriangleUV[2] = uv[0];

                                                                        newRot = (sbyte)(newRot + 2);
                                                                    }

                                                                    if (faceIndex == (int)BlockFaces.FloorTriangle2 ||
                                                                        faceIndex == (int)BlockFaces.CeilingTriangle2
                                                                    ) // CORRETTO
                                                                    {
                                                                        theBlock.Faces[faceIndex].TriangleUV2[0] = uv[1];
                                                                        theBlock.Faces[faceIndex].TriangleUV2[1] = uv[2];
                                                                        theBlock.Faces[faceIndex].TriangleUV2[2] = uv[0];

                                                                        newRot = (sbyte)(newRot + 1);
                                                                    }
                                                                }
                                                                else
                                                                {
                                                                    if (faceIndex == (int)BlockFaces.Floor ||
                                                                        faceIndex == (int)BlockFaces.Ceiling) // CORRETTO
                                                                    {
                                                                        theBlock.Faces[faceIndex].TriangleUV[0] = uv[1];
                                                                        theBlock.Faces[faceIndex].TriangleUV[1] = uv[2];
                                                                        theBlock.Faces[faceIndex].TriangleUV[2] = uv[0];
                                                                    }

                                                                    if (faceIndex == (int)BlockFaces.FloorTriangle2 ||
                                                                        faceIndex == (int)BlockFaces.CeilingTriangle2
                                                                    ) // CORRETTO
                                                                    {
                                                                        theBlock.Faces[faceIndex].TriangleUV2[0] = uv[1];
                                                                        theBlock.Faces[faceIndex].TriangleUV2[1] = uv[2];
                                                                        theBlock.Faces[faceIndex].TriangleUV2[2] = uv[0];

                                                                        newRot = (sbyte)(newRot + 1);
                                                                    }
                                                                }
                                                            }
                                                            else
                                                            {
                                                                theBlock.Faces[faceIndex].TriangleUV[0] = uv[1];
                                                                theBlock.Faces[faceIndex].TriangleUV[1] = uv[2];
                                                                theBlock.Faces[faceIndex].TriangleUV[2] = uv[0];

                                                                newRot = (sbyte)(newRot + 2);
                                                            }

                                                            theBlock.Faces[faceIndex].TextureTriangle =
                                                                TextureTileType.TriangleNE;
                                                        }

                                                        if (theFace._txtTriangle == 3)
                                                        {
                                                            if (isFloor || isCeiling)
                                                            {
                                                                if (theBlock.RealSplitFloor == 0)
                                                                {
                                                                    if (faceIndex == (int)BlockFaces.Floor ||
                                                                        faceIndex == (int)BlockFaces.Ceiling) // OK
                                                                    {
                                                                        theBlock.Faces[faceIndex].TriangleUV[0] = uv[3];
                                                                        theBlock.Faces[faceIndex].TriangleUV[1] = uv[0];
                                                                        theBlock.Faces[faceIndex].TriangleUV[2] = uv[2];

                                                                        newRot = (sbyte)(newRot + 2);
                                                                    }

                                                                    if (faceIndex == (int)BlockFaces.FloorTriangle2 ||
                                                                        faceIndex ==
                                                                        (int)BlockFaces.CeilingTriangle2) // TODO
                                                                    {
                                                                        theBlock.Faces[faceIndex].TriangleUV2[0] = uv[3];
                                                                        theBlock.Faces[faceIndex].TriangleUV2[1] = uv[0];
                                                                        theBlock.Faces[faceIndex].TriangleUV2[2] = uv[2];

                                                                        newRot = (sbyte)(newRot + 1);
                                                                    }
                                                                }
                                                                else
                                                                {
                                                                    if (faceIndex == (int)BlockFaces.Floor ||
                                                                        faceIndex == (int)BlockFaces.Ceiling) // OK
                                                                    {
                                                                        theBlock.Faces[faceIndex].TriangleUV[0] = uv[3];
                                                                        theBlock.Faces[faceIndex].TriangleUV[1] = uv[0];
                                                                        theBlock.Faces[faceIndex].TriangleUV[2] = uv[2];
                                                                    }

                                                                    if (faceIndex == (int)BlockFaces.FloorTriangle2 ||
                                                                        faceIndex ==
                                                                        (int)BlockFaces.CeilingTriangle2) // OK
                                                                    {
                                                                        theBlock.Faces[faceIndex].TriangleUV2[0] = uv[3];
                                                                        theBlock.Faces[faceIndex].TriangleUV2[1] = uv[0];
                                                                        theBlock.Faces[faceIndex].TriangleUV2[2] = uv[2];

                                                                        newRot = (sbyte)(newRot + 1);
                                                                    }
                                                                }
                                                            }
                                                            else
                                                            {
                                                                theBlock.Faces[faceIndex].TriangleUV[0] = uv[3];
                                                                theBlock.Faces[faceIndex].TriangleUV[1] = uv[0];
                                                                theBlock.Faces[faceIndex].TriangleUV[2] = uv[2];

                                                                newRot = (sbyte)(newRot + 2);
                                                            }

                                                            theBlock.Faces[faceIndex].TextureTriangle =
                                                                TextureTileType.TriangleSW;
                                                        }

                                                        if (theFace._txtTriangle == 2)
                                                        {
                                                            if (isFloor || isCeiling)
                                                            {
                                                                if (theBlock.RealSplitFloor == 0)
                                                                {
                                                                    if (faceIndex == (int)BlockFaces.Floor ||
                                                                        faceIndex == (int)BlockFaces.Ceiling) // CORRETTO
                                                                    {
                                                                        theBlock.Faces[faceIndex].TriangleUV[0] = uv[2];
                                                                        theBlock.Faces[faceIndex].TriangleUV[1] = uv[3];
                                                                        theBlock.Faces[faceIndex].TriangleUV[2] = uv[1];

                                                                        newRot = (sbyte)(newRot + 2);
                                                                    }

                                                                    if (faceIndex == (int)BlockFaces.FloorTriangle2 ||
                                                                        faceIndex ==
                                                                        (int)BlockFaces.CeilingTriangle2) // TODO
                                                                    {
                                                                        theBlock.Faces[faceIndex].TriangleUV2[0] = uv[2];
                                                                        theBlock.Faces[faceIndex].TriangleUV2[1] = uv[3];
                                                                        theBlock.Faces[faceIndex].TriangleUV2[2] = uv[1];

                                                                        newRot = (sbyte)(newRot + 1);
                                                                    }
                                                                }
                                                                else
                                                                {
                                                                    if (faceIndex == (int)BlockFaces.Floor ||
                                                                        faceIndex == (int)BlockFaces.Ceiling) // CORRETTO
                                                                    {
                                                                        theBlock.Faces[faceIndex].TriangleUV[0] = uv[2];
                                                                        theBlock.Faces[faceIndex].TriangleUV[1] = uv[3];
                                                                        theBlock.Faces[faceIndex].TriangleUV[2] = uv[1];
                                                                    }

                                                                    if (faceIndex == (int)BlockFaces.FloorTriangle2 ||
                                                                        faceIndex == (int)BlockFaces.CeilingTriangle2
                                                                    ) // CORRETTO
                                                                    {
                                                                        theBlock.Faces[faceIndex].TriangleUV2[0] = uv[2];
                                                                        theBlock.Faces[faceIndex].TriangleUV2[1] = uv[3];
                                                                        theBlock.Faces[faceIndex].TriangleUV2[2] = uv[1];

                                                                        newRot = (sbyte)(newRot + 1);
                                                                    }
                                                                }
                                                            }
                                                            else
                                                            {
                                                                theBlock.Faces[faceIndex].TriangleUV[0] = uv[2];
                                                                theBlock.Faces[faceIndex].TriangleUV[1] = uv[3];
                                                                theBlock.Faces[faceIndex].TriangleUV[2] = uv[1];

                                                                newRot = (sbyte)(newRot + 2);
                                                            }

                                                            theBlock.Faces[faceIndex].TextureTriangle =
                                                                TextureTileType.TriangleSE;
                                                        }
                                                    }

                                                    newRot = (sbyte)(newRot % 3);

                                                    for (int rot = 0; rot < newRot; rot++)
                                                    {
                                                        if (faceIndex != (int)BlockFaces.FloorTriangle2 &&
                                                            faceIndex != (int)BlockFaces.CeilingTriangle2)
                                                        {
                                                            var temp3 = theBlock.Faces[faceIndex].TriangleUV[2];
                                                            theBlock.Faces[faceIndex].TriangleUV[2] =
                                                                theBlock.Faces[faceIndex].TriangleUV[1];
                                                            theBlock.Faces[faceIndex].TriangleUV[1] =
                                                                theBlock.Faces[faceIndex].TriangleUV[0];
                                                            theBlock.Faces[faceIndex].TriangleUV[0] = temp3;
                                                        }

                                                        if (faceIndex == (int)BlockFaces.FloorTriangle2 ||
                                                            faceIndex == (int)BlockFaces.CeilingTriangle2)
                                                        {
                                                            var temp3 = theBlock.Faces[faceIndex].TriangleUV2[2];
                                                            theBlock.Faces[faceIndex].TriangleUV2[2] =
                                                                theBlock.Faces[faceIndex].TriangleUV2[1];
                                                            theBlock.Faces[faceIndex].TriangleUV2[1] =
                                                                theBlock.Faces[faceIndex].TriangleUV2[0];
                                                            theBlock.Faces[faceIndex].TriangleUV2[0] = temp3;
                                                        }
                                                    }

                                                    theBlock.Faces[faceIndex].Transparent =
                                                        level.TextureSamples[theFace._newId].Transparent;
                                                    theBlock.Faces[faceIndex].DoubleSided =
                                                        level.TextureSamples[theFace._newId].DoubleSided;
                                                    theBlock.Faces[faceIndex].Flipped = theFace._isFlipped;
                                                    theBlock.Faces[faceIndex].Rotation = (byte)(newRot);
                                                }
                                                else
                                                {
                                                    if (theFace._isFlipped)
                                                    {
                                                        var temp = uv[0];
                                                        uv[0] = uv[1];
                                                        uv[1] = temp;

                                                        temp = uv[2];
                                                        uv[2] = uv[3];
                                                        uv[3] = temp;
                                                    }

                                                    newRot += adjustRotation;
                                                    if (newRot < 0)
                                                        newRot = (sbyte)(3 - newRot);
                                                    if (newRot > 3)
                                                        newRot = (sbyte)(newRot % 3);

                                                    for (int rot = 0; rot < newRot; rot++)
                                                    {
                                                        var temp = uv[3];
                                                        uv[3] = uv[2];
                                                        uv[2] = uv[1];
                                                        uv[1] = uv[0];
                                                        uv[0] = temp;
                                                    }

                                                    theBlock.Faces[faceIndex].RectangleUV[0] = uv[0];
                                                    theBlock.Faces[faceIndex].RectangleUV[1] = uv[1];
                                                    theBlock.Faces[faceIndex].RectangleUV[2] = uv[2];
                                                    theBlock.Faces[faceIndex].RectangleUV[3] = uv[3];

                                                    theBlock.Faces[faceIndex].TextureTriangle =
                                                        TextureTileType.Rectangle;

                                                    theBlock.Faces[faceIndex].Transparent =
                                                        level.TextureSamples[theFace._newId].Transparent;
                                                    theBlock.Faces[faceIndex].DoubleSided =
                                                        level.TextureSamples[theFace._newId].DoubleSided;
                                                    theBlock.Faces[faceIndex].Flipped = theFace._isFlipped;
                                                    theBlock.Faces[faceIndex].Rotation = (byte)newRot;
                                                }

                                                theBlock.Faces[faceIndex].Texture = (short)theFace._newId;
                                                break;
                                            case 0x00:
                                                theBlock.Faces[faceIndex].Texture = -1;
                                                break;
                                            default:
                                                theBlock.Faces[faceIndex].Invisible = true;
                                                break;
                                        }
                                    }

                                    if (otherBlock != null)
                                    {
                                        room.Blocks[x2, z2] = otherBlock;
                                    }
                                    else
                                    {
                                        room.Blocks[x, z] = newBlock;
                                    }
                                }

                                room.Blocks[x, z] = newBlock;
                            }
                        }
                    }
                }

                // Check that there are uninitialized rooms
                foreach (Room room in level.Rooms)
                    if (room != null)
                        if ((room.NumXSectors <= 0) && (room.NumZSectors <= 0))
                            throw new Exception("Room " + level.Rooms.ReferenceIndexOf(room) + " has a sector size of zero. This is invalid. Probably the room was referenced but never initialized.");

                foreach (var portal in level.Portals)
                    portal.Value.Room.Portals.Add(portal.Key);

                progressReporter.ReportProgress(95, "Building rooms");
                foreach (var room in level.Rooms.Where(r => r != null))
                {
                    room.BuildGeometry();
                    room.CalculateLightingForThisRoom();
                    room.UpdateBuffers();
                }

                progressReporter.ReportProgress(100, "Level loaded correctly!");

                return level;
            }
            catch
            { 
                level.Dispose(); // We log in the level above
                throw;
            }
        }
    }
}
