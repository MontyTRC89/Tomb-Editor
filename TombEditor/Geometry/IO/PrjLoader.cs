using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using NLog;
using SharpDX;
using SharpDX.Toolkit.Graphics;
using TombLib.IO;
using Color = System.Drawing.Color;
using TombLib.Utils;

namespace TombEditor.Geometry.IO
{
    public class PrjLoader
    {
        private static readonly Encoding _encodingCodepageWindows = Encoding.GetEncoding(1252);
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
        }

        private struct PrjBlock
        {
            public PrjFace[] _faces;
            public PortalOpacity _floorOpacity;
            public PortalOpacity _ceilingOpacity;
            public PortalOpacity _wallOpacity;
        }

        private struct PrjTexInfo
        {
            public byte _x;
            public short _y;
            public byte _width;
            public byte _height;
        }

        private struct PrjPortal
        {
            //public Room _room;
            public Rectangle _area;
            public PortalDirection _direction;
            public short _oppositePortalId;
            public short _thisPortalId;
            public bool _memberOfFlippedRoom;
            public short _room;
            public short _prjRealRoom;
            public short _tempId;
            public bool _adjusted;
        }

        public static Level LoadFromPrj(string filename, IProgressReporter progressReporter)
        {
            var level = new Level();

            // Setup paths
            level.Settings.LevelFilePath = Path.ChangeExtension(filename, "prj2");

            string gameDirectory = FindGameDirectory(filename);
            progressReporter.ReportProgress(0, "Game directory: " + gameDirectory);
            level.Settings.GameDirectory = level.Settings.MakeRelative(gameDirectory, VariableType.LevelDirectory);

            try
            {
                // Open file
                using (var reader = new BinaryReaderEx(new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.Read)))
                {
                    progressReporter.ReportProgress(0, "Begin of PRJ import from " + filename);
                    logger.Debug("Opening Winroomedit PRJ file " + filename);

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
                    var tempPortals = new Dictionary<int, PrjPortal>();
                    var tempPortalsNew = new List<PrjPortal>();
                    var tempRoomPortals = new Dictionary<int, List<int>>();

                    progressReporter.ReportProgress(2, "Number of rooms: " + numRooms);

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
                        string roomName = _encodingCodepageWindows.GetString(roomNameBytes, 0, roomNameLength);

                        logger.Debug("Room #" + i);
                        logger.Debug("    Name: " + roomName);

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
                        room.Position = new Vector3(99 - posXBlocks - numXBlocks, yPos / -256.0f, posZBlocks);
                        level.Rooms[i] = room;

                        short numPortals = reader.ReadInt16();
                        var portalThings = new short[numPortals];

                        tempRoomPortals.Add(i, new List<int>());

                        for (int j = 0; j < numPortals; j++)
                        {
                            portalThings[j] = reader.ReadInt16();
                            tempRoomPortals[i].Add(portalThings[j]);
                        }

                        logger.Debug("    Portals: " + numPortals);

                        for (int j = 0; j < numPortals; j++)
                        {
                            ushort direction = reader.ReadUInt16();
                            short portalX = reader.ReadInt16();
                            short portalZ = reader.ReadInt16();
                            short portalXBlocks = reader.ReadInt16();
                            short portalZBlocks = reader.ReadInt16();
                            reader.ReadInt16();
                            short thisPortalRoomIndex = reader.ReadInt16();
                            var thisPortalRoom = level.GetOrCreateDummyRoom(thisPortalRoomIndex);
                            short portalOppositeSlot = reader.ReadInt16();

                            reader.ReadBytes(26);

                            PortalDirection directionEnum;
                            switch (direction)
                            {
                                case 0x0001:
                                    directionEnum = PortalDirection.East;
                                    break;
                                case 0x0002:
                                    directionEnum = PortalDirection.South;
                                    break;
                                case 0x0004:
                                    directionEnum = PortalDirection.Floor;
                                    break;
                                case 0xfffe:
                                    directionEnum = PortalDirection.West;
                                    break;
                                case 0xfffd:
                                    directionEnum = PortalDirection.North;
                                    break;
                                case 0xfffb:
                                    directionEnum = PortalDirection.Ceiling;
                                    break;
                                default:
                                    progressReporter.ReportWarn("Unknown portal direction value " + direction + " encountered in room #" + i + " '" + roomName + "'");
                                    continue;
                            }

                            if (tempPortals.ContainsKey(portalThings[j]))
                                continue;

                            tempPortals.Add(portalThings[j], new PrjPortal
                            {
                                //_room = room,
                                _area = GetArea(room, 0, portalX, portalZ, portalXBlocks, portalZBlocks),
                                _direction = directionEnum,
                                _oppositePortalId = portalOppositeSlot,
                                _thisPortalId = portalThings[j],
                                _prjRealRoom = thisPortalRoomIndex
                            });
                        }

                        short numObjects = reader.ReadInt16();
                        var objectsThings = new short[numObjects];

                        for (int j = 0; j < numObjects; j++)
                        {
                            objectsThings[j] = reader.ReadInt16();
                        }

                        logger.Debug("    Objects and Triggers: " + numObjects);

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

                            Vector3 position = new Vector3(room.NumXSectors * 1024 - objLongX, -objLongY - room.WorldPos.Y, objLongZ);

                            switch (objectType)
                            {
                                case 0x0008:
                                    if (objSlot >= 460 && objSlot <= 464)
                                        continue;

                                    int red = objTint & 0x001f;
                                    int green = (objTint & 0x03e0) >> 5;
                                    int blue = (objTint & 0x7c00) >> 10;

                                    if (objSlot < (ngle ? 520 : 465)) // TODO: a more flexible way to define this
                                    {
                                        var instance = new MoveableInstance()
                                        {
                                            ScriptId = unchecked((ushort)(objectsThings[j])),
                                            CodeBits = (byte)((objOcb >> 1) & 0x1f),
                                            Invisible = (objOcb & 0x0001) != 0,
                                            ClearBody = (objOcb & 0x0080) != 0,
                                            WadObjectId = unchecked((uint)objSlot),
                                            Position = position,
                                            Ocb = objTimer,
                                            RotationY = objFacing * (360.0f / 65535.0f) - 90.0f,
                                            Color = Color.FromArgb(255, red * 8, green * 8, blue * 8)
                                        };
                                        room.AddObject(level, instance);
                                    }
                                    else
                                    {
                                        var instance = new StaticInstance()
                                        {
                                            ScriptId = unchecked((ushort)(objectsThings[j])),
                                            WadObjectId = unchecked((uint)(objSlot - (ngle ? 520 : 465))),
                                            Position = position,
                                            RotationY = objFacing * (360.0f / 65535.0f) - 90.0f,
                                            Color = Color.FromArgb(255, red * 8, green * 8, blue * 8)
                                        };

                                        room.AddObject(level, instance);
                                    }
                                    break;

                                case 0x0010:
                                    short triggerType = reader.ReadInt16();
                                    short triggerItemNumber = reader.ReadInt16();
                                    short triggerTimer = reader.ReadInt16();
                                    short triggerFlags = reader.ReadInt16();
                                    short triggerItemType = reader.ReadInt16();

                                    TriggerType triggerTypeEnum;
                                    switch (triggerType)
                                    {
                                        case 0:
                                            triggerTypeEnum = TriggerType.Trigger;
                                            break;
                                        case 1:
                                            triggerTypeEnum = TriggerType.Pad;
                                            break;
                                        case 2:
                                            triggerTypeEnum = TriggerType.Switch;
                                            break;
                                        case 3:
                                            triggerTypeEnum = TriggerType.Key;
                                            break;
                                        case 4:
                                            triggerTypeEnum = TriggerType.Pickup;
                                            break;
                                        case 5:
                                            triggerTypeEnum = TriggerType.Heavy;
                                            break;
                                        case 6:
                                            triggerTypeEnum = TriggerType.Antipad;
                                            break;
                                        case 7:
                                            triggerTypeEnum = TriggerType.Combat;
                                            break;
                                        case 8:
                                            triggerTypeEnum = TriggerType.Dummy;
                                            break;
                                        case 9:
                                            triggerTypeEnum = TriggerType.Antitrigger;
                                            break;
                                        case 10:
                                            triggerTypeEnum = TriggerType.HeavySwitch;
                                            break;
                                        case 11:
                                            triggerTypeEnum = TriggerType.HeavyAntritrigger;
                                            break;
                                        case 12:
                                            triggerTypeEnum = TriggerType.Monkey;
                                            break;
                                        default:
                                            progressReporter.ReportWarn("Unknown trigger type " + triggerType + " encountered in room #" + i + " '" + roomName + "'");
                                            continue;
                                    }

                                    TriggerTargetType triggerTargetTypeEnum;
                                    switch (triggerItemType)
                                    {
                                        case 0:
                                            triggerTargetTypeEnum = TriggerTargetType.Object;
                                            break;
                                        case 1:
                                            triggerTargetTypeEnum = TriggerTargetType.Camera;
                                            break;
                                        case 2:
                                            triggerTargetTypeEnum = TriggerTargetType.Sink;
                                            break;
                                        case 3:
                                            triggerTargetTypeEnum = TriggerTargetType.FlipMap;
                                            break;
                                        case 4:
                                            triggerTargetTypeEnum = TriggerTargetType.FlipOn;
                                            break;
                                        case 5:
                                            triggerTargetTypeEnum = TriggerTargetType.FlipOff;
                                            break;
                                        case 6:
                                            triggerTargetTypeEnum = TriggerTargetType.Target;
                                            break;
                                        case 7:
                                            triggerTargetTypeEnum = TriggerTargetType.FinishLevel;
                                            break;
                                        case 8:
                                            triggerTargetTypeEnum = TriggerTargetType.PlayAudio;
                                            break;
                                        case 9:
                                            triggerTargetTypeEnum = TriggerTargetType.FlipEffect;
                                            break;
                                        case 10:
                                            triggerTargetTypeEnum = TriggerTargetType.Secret;
                                            break;
                                        case 11:
                                            triggerTargetTypeEnum = TriggerTargetType.ActionNg;
                                            break;
                                        case 12:
                                            triggerTargetTypeEnum = TriggerTargetType.FlyByCamera;
                                            break;
                                        case 13:
                                            triggerTargetTypeEnum = TriggerTargetType.ParameterNg;
                                            break;
                                        case 14:
                                            triggerTargetTypeEnum = TriggerTargetType.FmvNg;
                                            break;
                                        case 15:
                                            triggerTargetTypeEnum = TriggerTargetType.TimerfieldNg;
                                            break;
                                        default:
                                            triggerTargetTypeEnum = TriggerTargetType.FlipEffect;
                                            progressReporter.ReportWarn("Unknown trigger target type " + triggerItemType + " encountered in room #" + i + " '" + roomName + "'");
                                            continue;
                                    }

                                    var trigger = new TriggerInstance(GetArea(room, 1, objPosX, objPosZ, objSizeX, objSizeZ))
                                    {
                                        TriggerType = triggerTypeEnum,
                                        TargetType = triggerTargetTypeEnum,
                                        CodeBits = (byte)((~triggerFlags >> 1) & 0x1f),
                                        OneShot = (triggerFlags & 0x0001) != 0,
                                        Timer = triggerTimer,
                                        TargetData = triggerItemNumber
                                    };
                                    room.AddObject(level, trigger);
                                    break;

                                default:
                                    progressReporter.ReportWarn("Unknown object (first *.prj array) type " + objectType + " encountered in room #" + i + " '" + roomName + "'");
                                    continue;
                            }
                        }

                        room.AmbientLight = Color.FromArgb(255, reader.ReadByte(), reader.ReadByte(), reader.ReadByte());
                        reader.ReadByte();

                        short numObjects2 = reader.ReadInt16();
                        var objectsThings2 = new short[numObjects2];

                        logger.Debug("    Lights and other objects: " + numObjects2);

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

                            Vector3 position = new Vector3(room.NumXSectors * 1024 - objLongX, -objLongY - room.WorldPos.Y, objLongZ);

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
                                    float lightX = -reader.ReadSingle();
                                    float lightY = reader.ReadSingle() + 90.0f;
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
                                            lightIn /= 1024.0f;
                                            lightOut /= 1024.0f;
                                            break;
                                        default:
                                            throw new NotSupportedException("Unknown light type found inside *.prj file.");
                                    }

                                    var light = new Light(lightType)
                                    {
                                        Position = position,
                                        Color = Color.FromArgb(255, lightR, lightG, lightB),
                                        Cutoff = lightCut,
                                        Len = lightLen,
                                        Enabled = lightOn == 0x01,
                                        In = lightIn,
                                        Out = lightOut,
                                        Intensity = lightIntensity / 8192.0f,
                                    };
                                    light.SetArbitaryRotationsYX(lightY, lightX);
                                    room.AddObject(level, light);
                                    break;
                                case 0x4c00:
                                    var sound = new SoundSourceInstance()
                                    {
                                        SoundId = objSlot,
                                        Position = position
                                    };

                                    room.AddObject(level, sound);
                                    break;
                                case 0x4400:
                                    var sink = new SinkInstance()
                                    {
                                        ScriptId = unchecked((ushort)(objectsThings2[j])),
                                        Strength = (short)(objTimer / 2),
                                        Position = position
                                    };

                                    room.AddObject(level, sink);
                                    break;
                                case 0x4800:
                                case 0x4080:
                                    var camera = new CameraInstance()
                                    {
                                        ScriptId = unchecked((ushort)(objectsThings2[j])),
                                        Timer = objTimer,
                                        Fixed = (objectType == 0x4080),
                                        Position = position
                                    };

                                    room.AddObject(level, camera);
                                    break;
                                case 0x4040:
                                    var flybyCamera = new FlybyCameraInstance()
                                    {
                                        ScriptId = unchecked((ushort)(objectsThings2[j])),
                                        Timer = unchecked((ushort)objTimer),
                                        Sequence = (byte)((objSlot & 0xe000) >> 13),
                                        Number = (byte)((objSlot & 0x1f00) >> 8),
                                        Fov = (short)(objSlot & 0x00ff),
                                        Roll = objRoll,
                                        Speed = objSpeed / 655.0f,
                                        Position = position,
                                        RotationX = -objUnk,
                                        RotationY = objFacing + 90,
                                        Flags = unchecked((ushort)objOcb)
                                    };

                                    room.AddObject(level, flybyCamera);
                                    break;
                                default:
                                    progressReporter.ReportWarn("Unknown object (second *.prj array) type " + objectType + " encountered in room #" + i + " '" + roomName + "'");
                                    continue;
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
                            for (int x = room.NumXSectors - 1; x >= 0; x--)
                            {
                                short blockType = reader.ReadInt16();
                                short blockFlags1 = reader.ReadInt16();
                                short blockYfloor = reader.ReadInt16();
                                short blockYceiling = reader.ReadInt16();

                                var block = room.Blocks[x, z];
                                switch (blockType)
                                {
                                    case 0x01:
                                    case 0x05:
                                    case 0x07:
                                    case 0x03:
                                        block.Type = BlockType.Floor;
                                        break;
                                    case 0x1e:
                                        block.Type = BlockType.BorderWall;
                                        break;
                                    case 0x0e:
                                        block.Type = BlockType.Wall;
                                        break;
                                    case 0x06:
                                        block.Type = BlockType.BorderWall;
                                        break;
                                    default:
                                        block.Type = BlockType.Floor;
                                        break;
                                }

                                block.QAFaces[1] = (short)(reader.ReadSByte() + blockYfloor);
                                block.QAFaces[2] = (short)(reader.ReadSByte() + blockYfloor);
                                block.QAFaces[3] = (short)(reader.ReadSByte() + blockYfloor);
                                block.QAFaces[0] = (short)(reader.ReadSByte() + blockYfloor);

                                block.WSFaces[0] = (short)(reader.ReadSByte() + blockYceiling);
                                block.WSFaces[3] = (short)(reader.ReadSByte() + blockYceiling);
                                block.WSFaces[2] = (short)(reader.ReadSByte() + blockYceiling);
                                block.WSFaces[1] = (short)(reader.ReadSByte() + blockYceiling);

                                block.EDFaces[1] = (short)(reader.ReadSByte() + blockYfloor);
                                block.EDFaces[2] = (short)(reader.ReadSByte() + blockYfloor);
                                block.EDFaces[3] = (short)(reader.ReadSByte() + blockYfloor);
                                block.EDFaces[0] = (short)(reader.ReadSByte() + blockYfloor);

                                block.RFFaces[0] = (short)(reader.ReadSByte() + blockYceiling);
                                block.RFFaces[3] = (short)(reader.ReadSByte() + blockYceiling);
                                block.RFFaces[2] = (short)(reader.ReadSByte() + blockYceiling);
                                block.RFFaces[1] = (short)(reader.ReadSByte() + blockYceiling);

                                if ((blockFlags1 & 0x4000) != 0)
                                    block.Flags |= BlockFlags.Monkey;
                                if ((blockFlags1 & 0x0020) != 0)
                                    block.Flags |= BlockFlags.Box;
                                if ((blockFlags1 & 0x0010) != 0)
                                    block.Flags |= BlockFlags.DeathFire;
                                if ((blockFlags1 & 0x0200) != 0)
                                    block.Flags |= BlockFlags.ClimbNegativeX;
                                if ((blockFlags1 & 0x0100) != 0)
                                    block.Flags |= BlockFlags.ClimbPositiveZ;
                                if ((blockFlags1 & 0x0080) != 0)
                                    block.Flags |= BlockFlags.ClimbPositiveX;
                                if ((blockFlags1 & 0x0040) != 0)
                                    block.Flags |= BlockFlags.ClimbNegativeZ;

                                // Read temp blocks that contain texturing informations that will be needed later
                                var tempBlock = new PrjBlock { _faces = new PrjFace[14] };
                                for (int j = 0; j < 14; j++)
                                {
                                    tempBlock._faces[j] = new PrjFace
                                    {
                                        _txtType = reader.ReadInt16(),
                                        _txtIndex = reader.ReadByte(),
                                        _txtFlags = reader.ReadByte(),
                                        _txtRotation = reader.ReadByte(),
                                        _txtTriangle = reader.ReadByte()
                                    };
                                    reader.ReadInt16();
                                }

                                if ((x == 0 || z == 0 || x == room.NumXSectors - 1 || z == room.NumZSectors - 1))
                                {
                                    if ((blockFlags1 & 0x0008) == 0x0008 && (blockFlags1 & 0x1000) == 0)
                                        tempBlock._wallOpacity = PortalOpacity.Opacity1;
                                    if ((blockFlags1 & 0x0008) == 0x0008 && (blockFlags1 & 0x1000) == 0x1000)
                                        tempBlock._wallOpacity = PortalOpacity.Opacity2;
                                }
                                else
                                {
                                    if ((blockFlags1 & 0x0002) == 0x0002)
                                        tempBlock._floorOpacity = PortalOpacity.Opacity1;

                                    if ((blockFlags1 & 0x0004) == 0x0004)
                                        tempBlock._ceilingOpacity = PortalOpacity.Opacity1;

                                    if ((blockFlags1 & 0x0800) == 0x0800)
                                        tempBlock._floorOpacity = PortalOpacity.Opacity2;

                                    if ((blockFlags1 & 0x0400) == 0x0400)
                                        tempBlock._ceilingOpacity = PortalOpacity.Opacity2;
                                }

                                // Read more flags
                                short blockFlags2 = reader.ReadInt16();
                                short blockFlags3 = reader.ReadInt16();

                                block.NoCollisionFloor = (blockFlags2 & 0x06) != 0;
                                block.NoCollisionCeiling = (blockFlags2 & 0x18) != 0;

                                if ((blockFlags2 & 0x0040) != 0)
                                    block.Flags |= BlockFlags.Beetle;
                                if ((blockFlags2 & 0x0020) != 0)
                                    block.Flags |= BlockFlags.TriggerTriggerer;
                                block.FloorSplitDirectionToggled = (blockFlags3 & 0x1) != 0;

                                tempBlocks[x, z] = tempBlock;
                            }
                        tempRooms.Add(i, tempBlocks);

                        room.NormalizeRoomY();

                        System.Diagnostics.Debug.Assert(level.GetOrCreateDummyRoom(i) == room);

                        progressReporter.ReportProgress(i / ((float)numRooms) * 28.0f, "");
                    }

                    progressReporter.ReportProgress(30, "Rooms loaded");

                    // Link portals
                    {
                        progressReporter.ReportProgress(31, "Link portals");

                        Dictionary<int, Portal> newPortals = new Dictionary<int, Portal>();

                        foreach (PrjPortal prjPortal in tempPortals.Values)
                        {
                            if (!tempPortals.ContainsKey(prjPortal._oppositePortalId))
                            {
                                progressReporter.ReportWarn("A portal in room '" + prjPortal._room + "' refers to an invalid opposite portal.");
                                continue;
                            }

                            Room adjoiningRoom = level.Rooms[tempPortals[prjPortal._oppositePortalId]._prjRealRoom];
                            Portal p = new Portal(prjPortal._area, prjPortal._direction, adjoiningRoom);
                            newPortals.Add(prjPortal._thisPortalId, p);
                        }

                        foreach (var room in level.Rooms)
                        {
                            if (room == null)
                                continue;

                            List<int> portalsForThisRoom = tempRoomPortals[level.Rooms.ReferenceIndexOf(room)];

                            foreach (var portalId in portalsForThisRoom)
                            {
                                room.AddObject(level, newPortals[portalId]);
                            }
                        }

                        progressReporter.ReportProgress(32, "Portals linked");
                    }

                    // Adjust portal opacities
                    {
                        for (int i = 0; i < level.Rooms.Length; i++)
                        {
                            if (level.Rooms[i] == null)
                                continue;

                            var blocks = tempRooms[i];

                            for (int x = 0; x < level.Rooms[i].NumXSectors; x++)
                            {
                                for (int z = 0; z < level.Rooms[i].NumZSectors; z++)
                                {
                                    level.Rooms[i].Blocks[x, z].FloorOpacity = blocks[x, z]._floorOpacity;
                                    level.Rooms[i].Blocks[x, z].CeilingOpacity = blocks[x, z]._ceilingOpacity;
                                    level.Rooms[i].Blocks[x, z].WallOpacity = blocks[x, z]._wallOpacity;
                                }
                            }
                        }
                    }

                    // Link triggers
                    {
                        progressReporter.ReportProgress(31, "Link triggers");

                        // Build lookup table for IDs
                        Dictionary<ushort, PositionBasedObjectInstance> objectLookup =
                            level.Rooms.Where(room => room != null)
                            .SelectMany(room => room.Objects)
                            .Where(instance => instance is IHasScriptID)
                            .ToDictionary(instance => ((IHasScriptID)instance).ScriptId.Value);

                        // Lookup objects from IDs for all triggers
                        foreach (Room room in level.Rooms.Where(room => room != null))
                            foreach (var instance in room.Triggers.ToList())
                                switch (instance.TargetType)
                                {
                                    case TriggerTargetType.Object:
                                    case TriggerTargetType.Target:
                                    case TriggerTargetType.Camera:
                                    case TriggerTargetType.FlyByCamera:
                                    case TriggerTargetType.Sink:
                                        ushort triggerTargetId = unchecked((ushort)(instance.TargetData));
                                        if (!objectLookup.ContainsKey(triggerTargetId))
                                        {
                                            progressReporter.ReportWarn("Trigger '" + instance + "' in '" + instance.Room + "' refers to an object with ID " + triggerTargetId + " that is unavailable.");
                                            room.RemoveObject(level, instance);
                                            continue;
                                        }

                                        instance.TargetObj = objectLookup[triggerTargetId];
                                        instance.TargetData = 0;

                                        // Sinks and cameras are classified as 'object's most of time for some reason.
                                        // We have to fix that.
                                        if (instance.TargetObj is FlybyCameraInstance)
                                            instance.TargetType = TriggerTargetType.FlyByCamera;
                                        if (instance.TargetObj is SinkInstance)
                                            instance.TargetType = TriggerTargetType.Sink;
                                        if (instance.TargetObj is CameraInstance)
                                            instance.TargetType = TriggerTargetType.Camera;
                                        break;
                                }
                        progressReporter.ReportProgress(35, "Triggers linked");
                    }

                    // Ignore unused things indices

                    int dwNumThings = reader.ReadInt32(); // number of things in the map
                    int dwMaxThings = reader.ReadInt32(); // always 2000
                    reader.ReadBytes(dwMaxThings * 4);

                    int dwNumLights = reader.ReadInt32(); // number of lights in the map
                    reader.ReadBytes(768 * 4);

                    int dwNumTriggers = reader.ReadInt32(); // number of triggers in the map
                    reader.ReadBytes(512 * 4);

                    // Read texture
                    bool isTextureNA;
                    LevelTexture texture;
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

                        string textureFilename = _encodingCodepageWindows.GetString(stringBuffer);
                        isTextureNA = textureFilename.StartsWith("NA");
                        if (string.IsNullOrEmpty(textureFilename) || isTextureNA)
                            texture = new LevelTexture();
                        else
                            texture = new LevelTexture(level.Settings, level.Settings.MakeRelative(TryFindAbsolutePath(
                                level.Settings, textureFilename.Trim('\0', ' ')), VariableType.LevelDirectory), true);
                        if (texture.Image.Width != 256)
                            texture.SetConvert512PixelsToDoubleRows(level.Settings, false); // Only use this compatibility thing if actually needed
                        level.Settings.Textures.Add(texture);
                        ResourceLoader.CheckLoadedTexture(level.Settings, texture, progressReporter);
                        progressReporter.ReportProgress(50, "Loaded texture '" + texture.Path + "'");
                    }

                    // Read textures
                    var tempTextures = new List<PrjTexInfo>();
                    if (!isTextureNA)
                    {
                        int numTextures = reader.ReadInt32();

                        progressReporter.ReportProgress(52, "Loading textures");
                        progressReporter.ReportProgress(52, "    Number of textures: " + numTextures);

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
                        string wadName = _encodingCodepageWindows.GetString(stringBuffer);
                        if (string.IsNullOrEmpty(wadName) || wadName.StartsWith("NA"))
                            level.Settings.WadFilePath = "";
                        else
                            level.Settings.WadFilePath = level.Settings.MakeRelative(TryFindAbsolutePath(
                                level.Settings, Path.ChangeExtension(wadName.Trim('\0', ' '), "wad")), VariableType.LevelDirectory);
                        ResourceLoader.TryLoadingObjects(level, progressReporter);
                        progressReporter.ReportProgress(60, "Loaded WAD '" + level.Settings.WadFilePath + "'");
                    }

                    // Setup paths to customized fonts and the skys
                    if (!string.IsNullOrEmpty(level.Settings.WadFilePath))
                    {
                        string objectFilePath = level.Settings.MakeAbsolute(level.Settings.WadFilePath);

                        string fontPcFilePath = Path.Combine(Path.GetDirectoryName(objectFilePath), "Font.pc");
                        if (File.Exists(fontPcFilePath))
                            level.Settings.FontTextureFilePath = level.Settings.MakeRelative(fontPcFilePath, VariableType.LevelDirectory);

                        string wadSkyFilePath = Path.ChangeExtension(objectFilePath, "raw");
                        string genericSkyFilePath = Path.Combine(Path.GetDirectoryName(objectFilePath), "pcsky.raw");
                        if (File.Exists(wadSkyFilePath))
                            level.Settings.SkyTextureFilePath = level.Settings.MakeRelative(wadSkyFilePath, VariableType.LevelDirectory);
                        else if (File.Exists(genericSkyFilePath))
                            level.Settings.SkyTextureFilePath = level.Settings.MakeRelative(genericSkyFilePath, VariableType.LevelDirectory);

                        string soundPath = Path.Combine(Path.GetDirectoryName(objectFilePath), "../../sound/Samples");
                        level.Settings.SoundPaths[0].Path = level.Settings.MakeRelative(soundPath, VariableType.LevelDirectory);
                        level.Settings.IgnoreMissingSounds = true;
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

                            string slotName = _encodingCodepageWindows.GetString(stringBuffer);
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

                    int TODO_ANIMATED_TEXTURE_IMPORT;
                    /*for (int i = 0; i < 40; i++)
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
                    }*/

                    int TODO_SOUND_TEXTURE_IMPORT;
                    /*for (int i = 0; i < 256; i++)
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
                    }*/

                    // Connect flip rooms
                    progressReporter.ReportProgress(67, "Prcessing flip rooms table");

                    for (int i = 0; i < level.Rooms.Length; i++)
                    {
                        if (level.Rooms[i] == null)
                            continue;

                        var room = level.Rooms[i];

                        foreach (var info in flipInfos)
                        {
                            if (info._baseRoom == i)
                            {
                                room.AlternateRoom = level.Rooms[info._flipRoom];
                                room.AlternateGroup = info._group;
                            }

                            if (info._flipRoom != i)
                                continue;

                            room.AlternateBaseRoom = level.Rooms[info._baseRoom];
                            room.AlternateGroup = info._group;
                            room.Position = new Vector3(level.Rooms[info._baseRoom].Position.X,
                                level.Rooms[info._baseRoom].Position.Y,
                                level.Rooms[info._baseRoom].Position.Z);
                        }
                    }

                    // Build geometry
                    progressReporter.ReportProgress(80, "Building geometry");
                    foreach (var room in level.Rooms.Where(room => room != null))
                        room.BuildGeometry();

                    // Fix faces
                    progressReporter.ReportProgress(85, "Texturize faces");
                    for (int i = 0; i < level.Rooms.GetLength(0); i++)
                    {
                        var room = level.Rooms[i];
                        if (room == null)
                            continue;
                        
                        for (int z = 0; z < room.NumZSectors; z++)
                            for (int x = 0; x < room.NumXSectors; x++)
                            {
                                var prjBlock = tempRooms[i][x, z];

                                // 0: BLOCK_TEX_FLOOR
                                LoadTextureArea(room, x, z, BlockFace.Floor, texture, tempTextures, prjBlock._faces[0], 0);

                                // 1: BLOCK_TEX_CEILING
                                LoadTextureArea(room, x, z, BlockFace.Ceiling, texture, tempTextures, prjBlock._faces[1], 0);

                                // 2: BLOCK_TEX_N4 (North QA)
                                if (room.IsFaceDefined(x, z, BlockFace.SouthQA))
                                {
                                    LoadTextureArea(room, x, z, BlockFace.SouthQA, texture, tempTextures, prjBlock._faces[2], -1);
                                }
                                else
                                {
                                    if (z > 0)
                                        LoadTextureArea(room, x, z - 1, BlockFace.NorthQA, texture, tempTextures, prjBlock._faces[2], -1);
                                }

                                // 3: BLOCK_TEX_N1 (North RF)
                                if (room.IsFaceDefined(x, z, BlockFace.SouthRF) ||
                                    room.IsFaceDefined(x, z, BlockFace.SouthWS))
                                {
                                    if (room.IsFaceDefined(x, z, BlockFace.SouthRF) &&
                                        !room.IsFaceDefined(x, z, BlockFace.SouthWS))
                                    {
                                        LoadTextureArea(room, x, z, BlockFace.SouthRF, texture, tempTextures, prjBlock._faces[3], -1);
                                    }
                                    else if (!room.IsFaceDefined(x, z, BlockFace.SouthRF) &&
                                        room.IsFaceDefined(x, z, BlockFace.SouthWS))
                                    {
                                        LoadTextureArea(room, x, z, BlockFace.SouthWS, texture, tempTextures, prjBlock._faces[3], -1);
                                    }
                                    else
                                    {
                                        LoadTextureArea(room, x, z, BlockFace.SouthRF, texture, tempTextures, prjBlock._faces[3], -1);
                                    }
                                }
                                else
                                {
                                    if (z > 0)
                                        if (room.IsFaceDefined(x, z - 1, BlockFace.NorthRF) &&
                                            !room.IsFaceDefined(x, z - 1, BlockFace.NorthWS))
                                        {
                                            LoadTextureArea(room, x, z - 1, BlockFace.NorthRF, texture, tempTextures, prjBlock._faces[3], -1);
                                        }
                                        else if (!room.IsFaceDefined(x, z - 1, BlockFace.NorthRF) &&
                                            room.IsFaceDefined(x, z - 1, BlockFace.NorthWS))
                                        {
                                            LoadTextureArea(room, x, z - 1, BlockFace.NorthWS, texture, tempTextures, prjBlock._faces[3], -1);
                                        }
                                        else
                                        {
                                            LoadTextureArea(room, x, z - 1, BlockFace.NorthRF, texture, tempTextures, prjBlock._faces[3], -1);
                                        }
                                }

                                // 4: BLOCK_TEX_N3 (North middle)
                                if (room.IsFaceDefined(x, z, BlockFace.SouthMiddle))
                                {
                                    LoadTextureArea(room, x, z, BlockFace.SouthMiddle, texture, tempTextures, prjBlock._faces[4], -1);
                                }
                                else
                                {
                                    if (z > 0)
                                        LoadTextureArea(room, x, z - 1, BlockFace.NorthMiddle, texture, tempTextures, prjBlock._faces[4], -1);
                                }

                                // 5: BLOCK_TEX_W4 (West QA)
                                if (room.IsFaceDefined(x, z, BlockFace.EastQA))
                                {
                                    LoadTextureArea(room, x, z, BlockFace.EastQA, texture, tempTextures, prjBlock._faces[5], -1);
                                }
                                else
                                {
                                    if (x < room.NumXSectors - 1)
                                        LoadTextureArea(room, x + 1, z, BlockFace.WestQA, texture, tempTextures, prjBlock._faces[5], -1);
                                }

                                // 6: BLOCK_TEX_W1 (West RF)
                                if (room.IsFaceDefined(x, z, BlockFace.EastRF) ||
                                    room.IsFaceDefined(x, z, BlockFace.EastWS))
                                {
                                    if (room.IsFaceDefined(x, z, BlockFace.EastRF) &&
                                        !room.IsFaceDefined(x, z, BlockFace.EastWS))
                                    {
                                        LoadTextureArea(room, x, z, BlockFace.EastRF, texture, tempTextures, prjBlock._faces[6], -1);
                                    }
                                    else if (!room.IsFaceDefined(x, z, BlockFace.EastRF) &&
                                         room.IsFaceDefined(x, z, BlockFace.EastWS))
                                    {
                                        LoadTextureArea(room, x, z, BlockFace.EastWS, texture, tempTextures, prjBlock._faces[6], -1);
                                    }
                                    else
                                    {
                                        LoadTextureArea(room, x, z, BlockFace.EastRF, texture, tempTextures, prjBlock._faces[6], -1);
                                    }
                                }
                                else
                                {
                                    if (x < room.NumXSectors - 1)
                                        if (room.IsFaceDefined(x + 1, z, BlockFace.WestRF) &&
                                            !room.IsFaceDefined(x + 1, z, BlockFace.WestWS))
                                        {
                                            LoadTextureArea(room, x + 1, z, BlockFace.WestRF, texture, tempTextures, prjBlock._faces[6], -1);
                                        }
                                        else if (!room.IsFaceDefined(x + 1, z, BlockFace.WestRF) &&
                                             room.IsFaceDefined(x + 1, z, BlockFace.WestWS))
                                        {
                                            LoadTextureArea(room, x + 1, z, BlockFace.WestWS, texture, tempTextures, prjBlock._faces[6], -1);
                                        }
                                        else
                                        {
                                            LoadTextureArea(room, x + 1, z, BlockFace.WestRF, texture, tempTextures, prjBlock._faces[6], -1);
                                        }
                                }

                                // 7: BLOCK_TEX_W3 (West middle)
                                if (room.IsFaceDefined(x, z, BlockFace.EastMiddle))
                                {
                                    LoadTextureArea(room, x, z, BlockFace.EastMiddle, texture, tempTextures, prjBlock._faces[7], -1);
                                }
                                else
                                {
                                    if (x < room.NumXSectors - 1)
                                        LoadTextureArea(room, x + 1, z, BlockFace.WestMiddle, texture, tempTextures, prjBlock._faces[7], -1);
                                }

                                // 8: BLOCK_TEX_F_NENW (Floor Triangle 2)
                                LoadTextureArea(room, x, z, BlockFace.FloorTriangle2, texture, tempTextures, prjBlock._faces[8], 0);

                                // 9: BLOCK_TEX_C_NENW (Ceiling Triangle 2)
                                LoadTextureArea(room, x, z, BlockFace.CeilingTriangle2, texture, tempTextures, prjBlock._faces[9], 0);

                                // 10: BLOCK_TEX_N5 (North ED)
                                if (room.IsFaceDefined(x, z, BlockFace.SouthED))
                                {
                                    LoadTextureArea(room, x, z, BlockFace.SouthED, texture, tempTextures, prjBlock._faces[10], -1);
                                }
                                else
                                {
                                    if (z > 0)
                                        LoadTextureArea(room, x, z - 1, BlockFace.NorthED, texture, tempTextures, prjBlock._faces[10], -1);
                                }

                                // 11: BLOCK_TEX_N2 (North WS)
                                if (room.IsFaceDefined(x, z, BlockFace.SouthRF) ||
                                    room.IsFaceDefined(x, z, BlockFace.SouthWS))
                                {
                                    if (room.IsFaceDefined(x, z, BlockFace.SouthRF) &&
                                        room.IsFaceDefined(x, z, BlockFace.SouthWS))
                                    {
                                        LoadTextureArea(room, x, z, BlockFace.SouthWS, texture, tempTextures, prjBlock._faces[11], -1);
                                    }
                                }
                                else
                                {
                                    if (z > 0)
                                        if (room.IsFaceDefined(x, z - 1, BlockFace.NorthRF) &&
                                            room.IsFaceDefined(x, z - 1, BlockFace.NorthWS))
                                        {
                                            LoadTextureArea(room, x, z - 1, BlockFace.NorthWS, texture, tempTextures, prjBlock._faces[11], -1);
                                        }
                                }

                                // 12: BLOCK_TEX_W5
                                if (room.IsFaceDefined(x, z, BlockFace.EastED))
                                {
                                    LoadTextureArea(room, x, z, BlockFace.EastED, texture, tempTextures, prjBlock._faces[12], -1);
                                }
                                else
                                {
                                    if (x < room.NumXSectors - 1)
                                        LoadTextureArea(room, x + 1, z, BlockFace.WestED, texture, tempTextures, prjBlock._faces[12], -1);
                                }

                                // 13: BLOCK_TEX_W2 (West WS)
                                if (room.IsFaceDefined(x, z, BlockFace.EastRF) ||
                                    room.IsFaceDefined(x, z, BlockFace.EastWS))
                                {
                                    if (room.IsFaceDefined(x, z, BlockFace.EastRF) &&
                                        room.IsFaceDefined(x, z, BlockFace.EastWS))
                                    {
                                        LoadTextureArea(room, x, z, BlockFace.EastWS, texture, tempTextures, prjBlock._faces[13], -1);
                                    }
                                }
                                else
                                {
                                    if (x < room.NumXSectors - 1)
                                        if (room.IsFaceDefined(x + 1, z, BlockFace.WestRF) &&
                                            room.IsFaceDefined(x + 1, z, BlockFace.WestWS))
                                        {
                                            LoadTextureArea(room, x + 1, z, BlockFace.WestWS, texture, tempTextures, prjBlock._faces[13], -1);
                                        }
                                }
                            }
                    }
                }

                // Check that there are uninitialized rooms
                foreach (Room room in level.Rooms)
                    if (room != null)
                        if ((room.NumXSectors <= 0) && (room.NumZSectors <= 0))
                            throw new Exception("Room '" + room + "' has a sector size of zero. This is invalid. Probably the room was referenced but never initialized.");

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

        #pragma warning disable 0675 // Disable warning about bitwise or
        private static void LoadTextureArea(Room room, int x, int z, BlockFace face, LevelTexture levelTexture, List<PrjTexInfo> tempTextures, PrjFace prjFace, int adjustRotation)
        {
            Block block = room.Blocks[x, z];
            bool isFloor = face == BlockFace.Floor || face == BlockFace.FloorTriangle2;
            bool isCeiling = face == BlockFace.Ceiling || face == BlockFace.CeilingTriangle2;

            switch (levelTexture == null ? 0 : prjFace._txtType)
            {
                case 0x0000: // TYPE_TEXTURE_NONE
                default:
                    block.SetFaceTexture(face, new TextureArea { });
                    return;
                case 0x0003: // TYPE_TEXTURE_COLOR
                    block.SetFaceTexture(face, new TextureArea { Texture = TextureInvisible.Instance });
                    return;
                case 0x0007: // TYPE_TEXTURE_TILE
                    int texIndex = ((prjFace._txtFlags & 0x03) << 8) | prjFace._txtIndex;
                    PrjTexInfo texInfo = tempTextures[texIndex];
                    
                    var uv = new Vector2[]
                    {
                        new Vector2(
                            texInfo._x + 0.5f,
                            texInfo._y + 0.5f),
                        new Vector2(
                            texInfo._x + texInfo._width + 0.5f,
                            texInfo._y + 0.5f),
                        new Vector2(
                            texInfo._x + texInfo._width + 0.5f,
                            texInfo._y + texInfo._height + 0.5f),
                        new Vector2(
                            texInfo._x + 0.5f,
                            texInfo._y + texInfo._height + 0.5f)
                    };

                    TextureArea texture;
                    texture.Texture = levelTexture;
                    texture.DoubleSided = (prjFace._txtFlags & 0x04) != 0;
                    texture.BlendMode = (prjFace._txtFlags & 0x08) != 0 ? BlendMode.Additive : BlendMode.Normal;
                    texture.TexCoord0 = new Vector2(0);
                    texture.TexCoord1 = new Vector2(0);
                    texture.TexCoord2 = new Vector2(0);
                    texture.TexCoord3 = new Vector2(0);

                    int txtRot = prjFace._txtRotation + 1;
                    if (room.GetFaceVertexRange(x, z, face).Count == 3)
                    {
                        txtRot = txtRot % 3;
                        if ((prjFace._txtFlags & 0x80) != 0) // is flipped
                        {
                            if (prjFace._txtTriangle == 0)
                            {
                                if (isFloor)
                                {
                                    if (!block.FloorSplitDirectionIsXEqualsY)
                                    {
                                        if (face == BlockFace.Floor)
                                        {
                                            texture.TexCoord0 = uv[1];
                                            texture.TexCoord1 = uv[2];
                                            texture.TexCoord2 = uv[0];

                                            txtRot += 2;
                                        }

                                        if (face == BlockFace.FloorTriangle2)
                                        {
                                            texture.TexCoord0 = uv[1];
                                            texture.TexCoord1 = uv[2];
                                            texture.TexCoord2 = uv[0];

                                            txtRot += 1;
                                        }
                                    }
                                    else
                                    {
                                        if (face == BlockFace.Floor)
                                        {
                                            texture.TexCoord0 = uv[1];
                                            texture.TexCoord1 = uv[2];
                                            texture.TexCoord2 = uv[0];
                                        }

                                        if (face == BlockFace.FloorTriangle2)
                                        {
                                            texture.TexCoord0 = uv[1];
                                            texture.TexCoord1 = uv[2];
                                            texture.TexCoord2 = uv[0];

                                            txtRot += 1;
                                        }
                                    }
                                }
                                else if (isCeiling)
                                {
                                    if (!block.CeilingSplitDirectionIsXEqualsY)
                                    {
                                        if (face == BlockFace.Ceiling)
                                        {
                                            texture.TexCoord0 = uv[1];
                                            texture.TexCoord1 = uv[2];
                                            texture.TexCoord2 = uv[0];

                                            txtRot += 2;
                                        }

                                        if (face == BlockFace.CeilingTriangle2)
                                        {
                                            texture.TexCoord0 = uv[1];
                                            texture.TexCoord1 = uv[2];
                                            texture.TexCoord2 = uv[0];

                                            txtRot += 1;
                                        }
                                    }
                                    else
                                    {
                                        if (face == BlockFace.Ceiling)
                                        {
                                            texture.TexCoord0 = uv[1];
                                            texture.TexCoord1 = uv[2];
                                            texture.TexCoord2 = uv[0];
                                        }

                                        if (face == BlockFace.CeilingTriangle2)
                                        {
                                            texture.TexCoord0 = uv[1];
                                            texture.TexCoord1 = uv[2];
                                            texture.TexCoord2 = uv[0];

                                            txtRot += 1;
                                        }
                                    }
                                }
                                else
                                {
                                    texture.TexCoord0 = uv[1];
                                    texture.TexCoord1 = uv[2];
                                    texture.TexCoord2 = uv[0];

                                    txtRot += 2;
                                }
                            }

                            if (prjFace._txtTriangle == 1)
                            {
                                if (isFloor)
                                {
                                    if (!block.FloorSplitDirectionIsXEqualsY)
                                    {
                                        if (face == BlockFace.Floor)
                                        {
                                            texture.TexCoord0 = uv[0];
                                            texture.TexCoord1 = uv[3];
                                            texture.TexCoord2 = uv[1];

                                            txtRot += 2;
                                        }

                                        if (face == BlockFace.FloorTriangle2)
                                        {
                                            texture.TexCoord0 = uv[0];
                                            texture.TexCoord1 = uv[3];
                                            texture.TexCoord2 = uv[1];

                                            txtRot += 1;
                                        }
                                    }
                                    else
                                    {
                                        if (face == BlockFace.Floor)
                                        {
                                            texture.TexCoord0 = uv[0];
                                            texture.TexCoord1 = uv[3];
                                            texture.TexCoord2 = uv[1];
                                        }

                                        if (face == BlockFace.FloorTriangle2)
                                        {
                                            texture.TexCoord0 = uv[0];
                                            texture.TexCoord1 = uv[3];
                                            texture.TexCoord2 = uv[1];

                                            txtRot += 1;
                                        }
                                    }
                                }
                                else if (isCeiling)
                                {
                                    if (!block.CeilingSplitDirectionIsXEqualsY)
                                    {
                                        if (face == BlockFace.Ceiling)
                                        {
                                            texture.TexCoord0 = uv[0];
                                            texture.TexCoord1 = uv[3];
                                            texture.TexCoord2 = uv[1];

                                            txtRot += 2;
                                        }

                                        if (face == BlockFace.CeilingTriangle2)
                                        {
                                            texture.TexCoord0 = uv[0];
                                            texture.TexCoord1 = uv[3];
                                            texture.TexCoord2 = uv[1];

                                            txtRot += 1;
                                        }
                                    }
                                    else
                                    {
                                        if (face == BlockFace.Ceiling)
                                        {
                                            texture.TexCoord0 = uv[0];
                                            texture.TexCoord1 = uv[3];
                                            texture.TexCoord2 = uv[1];
                                        }

                                        if (face == BlockFace.CeilingTriangle2)
                                        {
                                            texture.TexCoord0 = uv[0];
                                            texture.TexCoord1 = uv[3];
                                            texture.TexCoord2 = uv[1];

                                            txtRot += 1;
                                        }
                                    }
                                }
                                else
                                {
                                    texture.TexCoord0 = uv[0];
                                    texture.TexCoord1 = uv[3];
                                    texture.TexCoord2 = uv[1];

                                    txtRot += 2;
                                }
                            }

                            if (prjFace._txtTriangle == 3)
                            {
                                if (isFloor)
                                {
                                    if (!block.FloorSplitDirectionIsXEqualsY)
                                    {
                                        if (face == BlockFace.Floor)
                                        {
                                            texture.TexCoord0 = uv[2];
                                            texture.TexCoord1 = uv[1];
                                            texture.TexCoord2 = uv[3];

                                            txtRot += 2;
                                        }

                                        if (face == BlockFace.FloorTriangle2)
                                        {
                                            texture.TexCoord0 = uv[2];
                                            texture.TexCoord1 = uv[1];
                                            texture.TexCoord2 = uv[3];

                                            txtRot += 1;
                                        }
                                    }
                                    else
                                    {
                                        if (face == BlockFace.Floor)
                                        {
                                            texture.TexCoord0 = uv[2];
                                            texture.TexCoord1 = uv[1];
                                            texture.TexCoord2 = uv[3];
                                        }

                                        if (face == BlockFace.FloorTriangle2)
                                        {
                                            texture.TexCoord0 = uv[2];
                                            texture.TexCoord1 = uv[1];
                                            texture.TexCoord2 = uv[3];

                                            txtRot += 1;
                                        }
                                    }
                                }
                                else if (isCeiling)
                                {
                                    if (!block.CeilingSplitDirectionIsXEqualsY)
                                    {
                                        if (face == BlockFace.Ceiling)
                                        {
                                            texture.TexCoord0 = uv[2];
                                            texture.TexCoord1 = uv[1];
                                            texture.TexCoord2 = uv[3];

                                            txtRot += 2;
                                        }

                                        if (face == BlockFace.CeilingTriangle2)
                                        {
                                            texture.TexCoord0 = uv[2];
                                            texture.TexCoord1 = uv[1];
                                            texture.TexCoord2 = uv[3];

                                            txtRot += 1;
                                        }
                                    }
                                    else
                                    {
                                        if (face == BlockFace.Ceiling)
                                        {
                                            texture.TexCoord0 = uv[2];
                                            texture.TexCoord1 = uv[1];
                                            texture.TexCoord2 = uv[3];
                                        }

                                        if (face == BlockFace.CeilingTriangle2)
                                        {
                                            texture.TexCoord0 = uv[2];
                                            texture.TexCoord1 = uv[1];
                                            texture.TexCoord2 = uv[3];

                                            txtRot += 1;
                                        }
                                    }
                                }
                                else
                                {
                                    texture.TexCoord0 = uv[2];
                                    texture.TexCoord1 = uv[1];
                                    texture.TexCoord2 = uv[3];

                                    txtRot += 2;
                                }
                            }

                            if (prjFace._txtTriangle == 2)
                            {
                                if (isFloor)
                                {
                                    if (!block.FloorSplitDirectionIsXEqualsY)
                                    {
                                        if (face == BlockFace.Floor)
                                        {
                                            texture.TexCoord0 = uv[3];
                                            texture.TexCoord1 = uv[2];
                                            texture.TexCoord2 = uv[0];

                                            txtRot += 2;
                                        }

                                        if (face == BlockFace.FloorTriangle2)
                                        {
                                            texture.TexCoord0 = uv[3];
                                            texture.TexCoord1 = uv[2];
                                            texture.TexCoord2 = uv[0];

                                            txtRot += 1;
                                        }
                                    }
                                    else
                                    {
                                        if (face == BlockFace.Floor)
                                        {
                                            texture.TexCoord0 = uv[3];
                                            texture.TexCoord1 = uv[2];
                                            texture.TexCoord2 = uv[0];
                                        }

                                        if (face == BlockFace.FloorTriangle2)
                                        {
                                            texture.TexCoord0 = uv[3];
                                            texture.TexCoord1 = uv[2];
                                            texture.TexCoord2 = uv[0];

                                            txtRot += 1;
                                        }
                                    }
                                }
                                else if (isCeiling)
                                {
                                    if (!block.CeilingSplitDirectionIsXEqualsY)
                                    {
                                        if (face == BlockFace.Ceiling)
                                        {
                                            texture.TexCoord0 = uv[3];
                                            texture.TexCoord1 = uv[2];
                                            texture.TexCoord2 = uv[0];

                                            txtRot += 2;
                                        }

                                        if (face == BlockFace.CeilingTriangle2)
                                        {
                                            texture.TexCoord0 = uv[3];
                                            texture.TexCoord1 = uv[2];
                                            texture.TexCoord2 = uv[0];

                                            txtRot += 1;
                                        }
                                    }
                                    else
                                    {
                                        if (face == BlockFace.Ceiling)
                                        {
                                            texture.TexCoord0 = uv[3];
                                            texture.TexCoord1 = uv[2];
                                            texture.TexCoord2 = uv[0];
                                        }

                                        if (face == BlockFace.CeilingTriangle2)
                                        {
                                            texture.TexCoord0 = uv[3];
                                            texture.TexCoord1 = uv[2];
                                            texture.TexCoord2 = uv[0];

                                            txtRot += 1;
                                        }
                                    }
                                }
                                else
                                {
                                    texture.TexCoord0 = uv[3];
                                    texture.TexCoord1 = uv[2];
                                    texture.TexCoord2 = uv[0];

                                    txtRot += 2;
                                }
                            }
                        }
                        else // ===========================================================================================
                        {
                            if (prjFace._txtTriangle == 0)
                            {
                                if (isFloor)
                                {
                                    if (!block.FloorSplitDirectionIsXEqualsY)
                                    {
                                        if (face == BlockFace.Floor) // CORRETTO
                                        {
                                            texture.TexCoord0 = uv[0];
                                            texture.TexCoord1 = uv[1];
                                            texture.TexCoord2 = uv[3];

                                            txtRot += 2;
                                        }

                                        if (face == BlockFace.FloorTriangle2) // CORRETTO
                                        {
                                            texture.TexCoord0 = uv[0];
                                            texture.TexCoord1 = uv[1];
                                            texture.TexCoord2 = uv[3];

                                            txtRot += 1;
                                        }
                                    }
                                    else
                                    {
                                        if (face == BlockFace.Floor) // CORRETTO
                                        {
                                            texture.TexCoord0 = uv[0];
                                            texture.TexCoord1 = uv[1];
                                            texture.TexCoord2 = uv[3];
                                        }

                                        if (face == BlockFace.FloorTriangle2) // CORRETTO
                                        {
                                            texture.TexCoord0 = uv[0];
                                            texture.TexCoord1 = uv[1];
                                            texture.TexCoord2 = uv[3];

                                            txtRot += 1;
                                        }
                                    }
                                }
                                else if (isCeiling)
                                {
                                    if (!block.CeilingSplitDirectionIsXEqualsY)
                                    {
                                        if (face == BlockFace.Ceiling) // CORRETTO
                                        {
                                            texture.TexCoord0 = uv[0];
                                            texture.TexCoord1 = uv[1];
                                            texture.TexCoord2 = uv[3];

                                            txtRot += 2;
                                        }

                                        if (face == BlockFace.CeilingTriangle2) // CORRETTO
                                        {
                                            texture.TexCoord0 = uv[0];
                                            texture.TexCoord1 = uv[1];
                                            texture.TexCoord2 = uv[3];

                                            txtRot += 1;
                                        }
                                    }
                                    else
                                    {
                                        if (face == BlockFace.Ceiling) // CORRETTO
                                        {
                                            texture.TexCoord0 = uv[0];
                                            texture.TexCoord1 = uv[1];
                                            texture.TexCoord2 = uv[3];
                                        }

                                        if (face == BlockFace.CeilingTriangle2) // CORRETTO
                                        {
                                            texture.TexCoord0 = uv[0];
                                            texture.TexCoord1 = uv[1];
                                            texture.TexCoord2 = uv[3];

                                            txtRot += 1;
                                        }
                                    }
                                }
                                else
                                {
                                    texture.TexCoord0 = uv[0];
                                    texture.TexCoord1 = uv[1];
                                    texture.TexCoord2 = uv[3];

                                    txtRot += 2;
                                }
                            }

                            if (prjFace._txtTriangle == 1)
                            {
                                if (isFloor)
                                {
                                    if (!block.FloorSplitDirectionIsXEqualsY)
                                    {
                                        if (face == BlockFace.Floor) // CORRETTO
                                        {
                                            texture.TexCoord0 = uv[1];
                                            texture.TexCoord1 = uv[2];
                                            texture.TexCoord2 = uv[0];

                                            txtRot += 2;
                                        }

                                        if (face == BlockFace.FloorTriangle2) // CORRETTO
                                        {
                                            texture.TexCoord0 = uv[1];
                                            texture.TexCoord1 = uv[2];
                                            texture.TexCoord2 = uv[0];

                                            txtRot += 1;
                                        }
                                    }
                                    else
                                    {
                                        if (face == BlockFace.Floor) // CORRETTO
                                        {
                                            texture.TexCoord0 = uv[1];
                                            texture.TexCoord1 = uv[2];
                                            texture.TexCoord2 = uv[0];
                                        }

                                        if (face == BlockFace.FloorTriangle2) // CORRETTO
                                        {
                                            texture.TexCoord0 = uv[1];
                                            texture.TexCoord1 = uv[2];
                                            texture.TexCoord2 = uv[0];

                                            txtRot += 1;
                                        }
                                    }
                                }
                                else if (isCeiling)
                                {
                                    if (!block.CeilingSplitDirectionIsXEqualsY)
                                    {
                                        if (face == BlockFace.Ceiling) // CORRETTO
                                        {
                                            texture.TexCoord0 = uv[1];
                                            texture.TexCoord1 = uv[2];
                                            texture.TexCoord2 = uv[0];

                                            txtRot += 2;
                                        }

                                        if (face == BlockFace.CeilingTriangle2) // CORRETTO
                                        {
                                            texture.TexCoord0 = uv[1];
                                            texture.TexCoord1 = uv[2];
                                            texture.TexCoord2 = uv[0];

                                            txtRot += 1;
                                        }
                                    }
                                    else
                                    {
                                        if (face == BlockFace.Ceiling) // CORRETTO
                                        {
                                            texture.TexCoord0 = uv[1];
                                            texture.TexCoord1 = uv[2];
                                            texture.TexCoord2 = uv[0];
                                        }

                                        if (face == BlockFace.CeilingTriangle2) // CORRETTO
                                        {
                                            texture.TexCoord0 = uv[1];
                                            texture.TexCoord1 = uv[2];
                                            texture.TexCoord2 = uv[0];

                                            txtRot += 1;
                                        }
                                    }
                                }
                                else
                                {
                                    texture.TexCoord0 = uv[1];
                                    texture.TexCoord1 = uv[2];
                                    texture.TexCoord2 = uv[0];

                                    txtRot += 2;
                                }
                            }

                            if (prjFace._txtTriangle == 3)
                            {
                                if (isFloor)
                                {
                                    if (!block.FloorSplitDirectionIsXEqualsY)
                                    {
                                        if (face == BlockFace.Floor) // OK
                                        {
                                            texture.TexCoord0 = uv[3];
                                            texture.TexCoord1 = uv[0];
                                            texture.TexCoord2 = uv[2];

                                            txtRot += 2;
                                        }

                                        if (face == BlockFace.FloorTriangle2) // TODO
                                        {
                                            texture.TexCoord0 = uv[3];
                                            texture.TexCoord1 = uv[0];
                                            texture.TexCoord2 = uv[2];

                                            txtRot += 1;
                                        }
                                    }
                                    else
                                    {
                                        if (face == BlockFace.Floor) // OK
                                        {
                                            texture.TexCoord0 = uv[3];
                                            texture.TexCoord1 = uv[0];
                                            texture.TexCoord2 = uv[2];
                                        }

                                        if (face == BlockFace.FloorTriangle2) // OK
                                        {
                                            texture.TexCoord0 = uv[3];
                                            texture.TexCoord1 = uv[0];
                                            texture.TexCoord2 = uv[2];

                                            txtRot += 1;
                                        }
                                    }
                                }
                                else if (isCeiling)
                                {
                                    if (!block.CeilingSplitDirectionIsXEqualsY)
                                    {
                                        if (face == BlockFace.Ceiling) // OK
                                        {
                                            texture.TexCoord0 = uv[3];
                                            texture.TexCoord1 = uv[0];
                                            texture.TexCoord2 = uv[2];

                                            txtRot += 2;
                                        }

                                        if (face ==
                                            BlockFace.CeilingTriangle2) // TODO
                                        {
                                            texture.TexCoord0 = uv[3];
                                            texture.TexCoord1 = uv[0];
                                            texture.TexCoord2 = uv[2];

                                            txtRot += 1;
                                        }
                                    }
                                    else
                                    {
                                        if (face == BlockFace.Ceiling) // OK
                                        {
                                            texture.TexCoord0 = uv[3];
                                            texture.TexCoord1 = uv[0];
                                            texture.TexCoord2 = uv[2];
                                        }

                                        if (face ==
                                            BlockFace.CeilingTriangle2) // OK
                                        {
                                            texture.TexCoord0 = uv[3];
                                            texture.TexCoord1 = uv[0];
                                            texture.TexCoord2 = uv[2];

                                            txtRot += 1;
                                        }
                                    }
                                }
                                else
                                {
                                    texture.TexCoord0 = uv[3];
                                    texture.TexCoord1 = uv[0];
                                    texture.TexCoord2 = uv[2];

                                    txtRot += 2;
                                }
                            }

                            if (prjFace._txtTriangle == 2)
                            {
                                if (isFloor)
                                {
                                    if (!block.FloorSplitDirectionIsXEqualsY)
                                    {
                                        if (face == BlockFace.Floor) // CORRETTO
                                        {
                                            texture.TexCoord0 = uv[2];
                                            texture.TexCoord1 = uv[3];
                                            texture.TexCoord2 = uv[1];

                                            txtRot += 2;
                                        }

                                        if (face == BlockFace.FloorTriangle2) // TODO
                                        {
                                            texture.TexCoord0 = uv[2];
                                            texture.TexCoord1 = uv[3];
                                            texture.TexCoord2 = uv[1];

                                            txtRot += 1;
                                        }
                                    }
                                    else
                                    {
                                        if (face == BlockFace.Floor) // CORRETTO
                                        {
                                            texture.TexCoord0 = uv[2];
                                            texture.TexCoord1 = uv[3];
                                            texture.TexCoord2 = uv[1];
                                        }

                                        if (face == BlockFace.FloorTriangle2) // CORRETTO
                                        {
                                            texture.TexCoord0 = uv[2];
                                            texture.TexCoord1 = uv[3];
                                            texture.TexCoord2 = uv[1];

                                            txtRot += 1;
                                        }
                                    }
                                }
                                else if (isCeiling)
                                {
                                    if (!block.CeilingSplitDirectionIsXEqualsY)
                                    {
                                        if (face == BlockFace.Ceiling) // CORRETTO
                                        {
                                            texture.TexCoord0 = uv[2];
                                            texture.TexCoord1 = uv[3];
                                            texture.TexCoord2 = uv[1];

                                            txtRot += 2;
                                        }

                                        if (face == BlockFace.CeilingTriangle2) // TODO
                                        {
                                            texture.TexCoord0 = uv[2];
                                            texture.TexCoord1 = uv[3];
                                            texture.TexCoord2 = uv[1];

                                            txtRot += 1;
                                        }
                                    }
                                    else
                                    {
                                        if (face == BlockFace.Ceiling) // CORRETTO
                                        {
                                            texture.TexCoord0 = uv[2];
                                            texture.TexCoord1 = uv[3];
                                            texture.TexCoord2 = uv[1];
                                        }

                                        if (face == BlockFace.CeilingTriangle2) // CORRETTO
                                        {
                                            texture.TexCoord0 = uv[2];
                                            texture.TexCoord1 = uv[3];
                                            texture.TexCoord2 = uv[1];

                                            txtRot += 1;
                                        }
                                    }
                                }
                                else
                                {
                                    texture.TexCoord0 = uv[2];
                                    texture.TexCoord1 = uv[3];
                                    texture.TexCoord2 = uv[1];

                                    txtRot += 2;
                                }
                            }
                        }

                        txtRot += 1;
                        txtRot = (sbyte)(txtRot % 3);

                        for (int rot = 0; rot < txtRot; rot++)
                        {
                            var temp3 = texture.TexCoord2;
                            texture.TexCoord2 = texture.TexCoord1;
                            texture.TexCoord1 = texture.TexCoord0;
                            texture.TexCoord0 = temp3;
                        }

                        texture.TexCoord3 = texture.TexCoord2;
                    }
                    else
                    {
                        txtRot = txtRot % 4;
                        if ((prjFace._txtFlags & 0x80) != 0) // is flipped
                        {
                            var temp = uv[0];
                            uv[0] = uv[1];
                            uv[1] = temp;

                            temp = uv[2];
                            uv[2] = uv[3];
                            uv[3] = temp;
                        }

                        txtRot += adjustRotation;
                        if (txtRot < 0)
                            txtRot = (sbyte)(3 - txtRot);
                        if (txtRot > 3)
                            txtRot = (sbyte)(txtRot % 3);

                        txtRot += 1;
                        for (int rot = 0; rot < txtRot; rot++)
                        {
                            var temp = uv[3];
                            uv[3] = uv[2];
                            uv[2] = uv[1];
                            uv[1] = uv[0];
                            uv[0] = temp;
                        }

                        texture.TexCoord0 = uv[0];
                        texture.TexCoord1 = uv[1];
                        texture.TexCoord2 = uv[2];
                        texture.TexCoord3 = uv[3];
                    }

                    block.SetFaceTexture(face, texture);
                    return;
            }
        }
        
        private static Rectangle GetArea(Room room, int roomBorder, int objPosX, int objPosZ, int objSizeX, int objSizeZ)
        {
            int realObjPosX = (room.NumXSectors - objSizeX) - objPosX;
            int startX = Math.Max(roomBorder, Math.Min(room.NumXSectors - 1 - roomBorder, realObjPosX));
            int startZ = Math.Max(roomBorder, Math.Min(room.NumZSectors - 1 - roomBorder, objPosZ));
            int endX = Math.Max(startX, Math.Min(room.NumXSectors - 1 - roomBorder, realObjPosX + objSizeX - 1));
            int endZ = Math.Max(startZ, Math.Min(room.NumZSectors - 1 - roomBorder, objPosZ + objSizeZ - 1));
            return new Rectangle(startX, startZ, endX, endZ);
        }

        private static string FindGameDirectory(string filename)
        {
            string directory = filename;
            while (!string.IsNullOrEmpty(directory))
            {
                if (File.Exists(Path.Combine(directory, "Tomb4.exe")) ||
                    File.Exists(Path.Combine(directory, "script.dat")))
                {
                    return directory;
                }
                directory = Path.GetDirectoryName(directory);
            }
            return Path.GetDirectoryName(filename);
        }

        private static string TryFindAbsolutePath(LevelSettings levelSettings, string filename)
        {
            try
            {
                // Is the file easily found?
                if (File.Exists(filename))
                    return filename;

                string[] filePathComponents = filename.Split(new char[] { '\\', '/' });
                string[] levelPathComponents = levelSettings.GetVariable(VariableType.LevelDirectory).Split(new char[] { '\\', '/' });

                // Try to go up 2 directories to find file (works in original levels)
                // If it turns out that many people have directory structures incompatible to this assumptions
                // we can add more suffisticated options here in the future.
                int filePathCheckDepth = Math.Min(3, filePathComponents.GetLength(0) - 1);
                int levelPathCheckDepth = Math.Min(2, levelPathComponents.GetLength(0) - 1);
                for (int levelPathUntil = 0; levelPathUntil <= levelPathCheckDepth; ++levelPathUntil)
                    for (int filePathAfter = 1; filePathAfter <= filePathCheckDepth; ++filePathAfter)
                    {
                        var basePath = levelPathComponents.Take(levelPathComponents.GetLength(0) - levelPathUntil);
                        var filePath = filePathComponents.Skip(filePathComponents.GetLength(0) - filePathAfter);
                        string filepathSuggestion = string.Join(LevelSettings.Dir.ToString(), basePath.Union(filePath));
                        if (File.Exists(filepathSuggestion))
                            return filepathSuggestion;
                    }
            }
            catch (Exception exc)
            {
                logger.Error(exc, "TryFindAbsolutePath failed");
                // In cas of an error we can just give up to find the absolute path alreasy
                // and prompt the user for the file path.
            }
            return filename;
        }
    }
}
