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
            public int _newId;
            public bool _isFlipped;
        }

        private struct PrjBlock
        {
            public PrjFace[] _faces;
            public short _flags2;
            public short _flags3;
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
                        room.Position = new Vector3(128 - posXBlocks - numXBlocks, yPos / -256.0f, posZBlocks);
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

                            if (tempPortals.ContainsKey(portalThings[j])) continue;

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

                                tempBlock._flags2 = reader.ReadInt16();
                                tempBlock._flags3 = reader.ReadInt16();
                                block.FloorSplitDirectionToggled = tempBlock._flags3 != 0;

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
                            if (room == null) continue;

                            List<int> portalsForThisRoom = tempRoomPortals[level.Rooms.ReferenceIndexOf(room)];

                            foreach (var portalId in portalsForThisRoom)
                            {
                                room.AddObject(level, newPortals[portalId]);
                            }
                        }

                        progressReporter.ReportProgress(32, "Portals linked");
                    }

                    // Adjust opacities
                    {
                        for (int i = 0; i < level.Rooms.Length; i++)
                        {
                            if (level.Rooms[i] == null) continue;

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
                        level.Settings.TextureFilePath = level.Settings.MakeRelative(TryFindAbsolutePath(
                            level.Settings, textureFilename.Replace('\0', ' ').Trim()), VariableType.LevelDirectory);
                        ResourceLoader.TryLoadingTexture(level, progressReporter);
                        progressReporter.ReportProgress(50, "Loaded texture '" + level.Settings.TextureFilePath + "'");
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
                        string wadName = _encodingCodepageWindows.GetString(stringBuffer);
                        level.Settings.WadFilePath = level.Settings.MakeRelative(TryFindAbsolutePath(
                            level.Settings, Path.ChangeExtension(wadName.Replace('\0', ' ').Trim(), "wad")), VariableType.LevelDirectory);
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
                    
                    // Fix faces
                    progressReporter.ReportProgress(85, "Building faces and geometry");
                    for (int i = 0; i < level.Rooms.Length; i++)
                    {
                        var room = level.Rooms[i];
                        if (room == null)
                            continue;

                        for (int z = 0; z < room.NumZSectors; z++)
                        {
                            for (int x = 0; x < room.NumXSectors; x++)
                            {
                                var b = tempRooms[i][x, z];

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

                                tempRooms[i][x, z] = b;
                            }
                        }

                        room.BuildGeometry();

                        for (int z = 0; z < room.NumZSectors; z++)
                        {
                            for (int x = 0; x < room.NumXSectors; x++)
                            {
                                var newBlock = room.Blocks[x, z];
                                var prjBlock = tempRooms[i][x, z];

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

                                newBlock.FloorIsSplit = (isFloorSplitted && prjBlock._flags3 == 0x01);
                                
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
                                                            if (isFloor)
                                                            {
                                                                if (!theBlock.FloorSplitRealDirection)
                                                                {
                                                                    if (faceIndex == (int)BlockFaces.Floor)
                                                                    {
                                                                        theBlock.Faces[faceIndex].TriangleUV[0] = uv[1];
                                                                        theBlock.Faces[faceIndex].TriangleUV[1] = uv[2];
                                                                        theBlock.Faces[faceIndex].TriangleUV[2] = uv[0];

                                                                        newRot = (sbyte)(newRot + 2);
                                                                    }

                                                                    if (faceIndex == (int)BlockFaces.FloorTriangle2)
                                                                    {
                                                                        theBlock.Faces[faceIndex].TriangleUV2[0] = uv[1];
                                                                        theBlock.Faces[faceIndex].TriangleUV2[1] = uv[2];
                                                                        theBlock.Faces[faceIndex].TriangleUV2[2] = uv[0];

                                                                        newRot = (sbyte)(newRot + 1);
                                                                    }
                                                                }
                                                                else
                                                                {
                                                                    if (faceIndex == (int)BlockFaces.Floor)
                                                                    {
                                                                        theBlock.Faces[faceIndex].TriangleUV[0] = uv[1];
                                                                        theBlock.Faces[faceIndex].TriangleUV[1] = uv[2];
                                                                        theBlock.Faces[faceIndex].TriangleUV[2] = uv[0];
                                                                    }

                                                                    if (faceIndex == (int)BlockFaces.FloorTriangle2)
                                                                    {
                                                                        theBlock.Faces[faceIndex].TriangleUV2[0] = uv[1];
                                                                        theBlock.Faces[faceIndex].TriangleUV2[1] = uv[2];
                                                                        theBlock.Faces[faceIndex].TriangleUV2[2] = uv[0];

                                                                        newRot = (sbyte)(newRot + 1);
                                                                    }
                                                                }
                                                            }
                                                            else if (isCeiling)
                                                            {
                                                                if (!theBlock.CeilingSplitRealDirection)
                                                                {
                                                                    if (faceIndex == (int)BlockFaces.Ceiling)
                                                                    {
                                                                        theBlock.Faces[faceIndex].TriangleUV[0] = uv[1];
                                                                        theBlock.Faces[faceIndex].TriangleUV[1] = uv[2];
                                                                        theBlock.Faces[faceIndex].TriangleUV[2] = uv[0];

                                                                        newRot = (sbyte)(newRot + 2);
                                                                    }

                                                                    if (faceIndex == (int)BlockFaces.CeilingTriangle2)
                                                                    {
                                                                        theBlock.Faces[faceIndex].TriangleUV2[0] = uv[1];
                                                                        theBlock.Faces[faceIndex].TriangleUV2[1] = uv[2];
                                                                        theBlock.Faces[faceIndex].TriangleUV2[2] = uv[0];

                                                                        newRot = (sbyte)(newRot + 1);
                                                                    }
                                                                }
                                                                else
                                                                {
                                                                    if (faceIndex == (int)BlockFaces.Ceiling)
                                                                    {
                                                                        theBlock.Faces[faceIndex].TriangleUV[0] = uv[1];
                                                                        theBlock.Faces[faceIndex].TriangleUV[1] = uv[2];
                                                                        theBlock.Faces[faceIndex].TriangleUV[2] = uv[0];
                                                                    }

                                                                    if (faceIndex == (int)BlockFaces.CeilingTriangle2)
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
                                                            if (isFloor)
                                                            {
                                                                if (!theBlock.FloorSplitRealDirection)
                                                                {
                                                                    if (faceIndex == (int)BlockFaces.Floor)
                                                                    {
                                                                        theBlock.Faces[faceIndex].TriangleUV[0] = uv[0];
                                                                        theBlock.Faces[faceIndex].TriangleUV[1] = uv[3];
                                                                        theBlock.Faces[faceIndex].TriangleUV[2] = uv[1];

                                                                        newRot = (sbyte)(newRot + 2);
                                                                    }

                                                                    if (faceIndex == (int)BlockFaces.FloorTriangle2)
                                                                    {
                                                                        theBlock.Faces[faceIndex].TriangleUV2[0] = uv[0];
                                                                        theBlock.Faces[faceIndex].TriangleUV2[1] = uv[3];
                                                                        theBlock.Faces[faceIndex].TriangleUV2[2] = uv[1];

                                                                        newRot = (sbyte)(newRot + 1);
                                                                    }
                                                                }
                                                                else
                                                                {
                                                                    if (faceIndex == (int)BlockFaces.Floor)
                                                                    {
                                                                        theBlock.Faces[faceIndex].TriangleUV[0] = uv[0];
                                                                        theBlock.Faces[faceIndex].TriangleUV[1] = uv[3];
                                                                        theBlock.Faces[faceIndex].TriangleUV[2] = uv[1];
                                                                    }

                                                                    if (faceIndex == (int)BlockFaces.FloorTriangle2)
                                                                    {
                                                                        theBlock.Faces[faceIndex].TriangleUV2[0] = uv[0];
                                                                        theBlock.Faces[faceIndex].TriangleUV2[1] = uv[3];
                                                                        theBlock.Faces[faceIndex].TriangleUV2[2] = uv[1];

                                                                        newRot = (sbyte)(newRot + 1);
                                                                    }
                                                                }
                                                            }
                                                            else if (isCeiling)
                                                            {
                                                                if (!theBlock.CeilingSplitRealDirection)
                                                                {
                                                                    if (faceIndex == (int)BlockFaces.Ceiling)
                                                                    {
                                                                        theBlock.Faces[faceIndex].TriangleUV[0] = uv[0];
                                                                        theBlock.Faces[faceIndex].TriangleUV[1] = uv[3];
                                                                        theBlock.Faces[faceIndex].TriangleUV[2] = uv[1];

                                                                        newRot = (sbyte)(newRot + 2);
                                                                    }

                                                                    if (faceIndex == (int)BlockFaces.CeilingTriangle2)
                                                                    {
                                                                        theBlock.Faces[faceIndex].TriangleUV2[0] = uv[0];
                                                                        theBlock.Faces[faceIndex].TriangleUV2[1] = uv[3];
                                                                        theBlock.Faces[faceIndex].TriangleUV2[2] = uv[1];

                                                                        newRot = (sbyte)(newRot + 1);
                                                                    }
                                                                }
                                                                else
                                                                {
                                                                    if (faceIndex == (int)BlockFaces.Ceiling)
                                                                    {
                                                                        theBlock.Faces[faceIndex].TriangleUV[0] = uv[0];
                                                                        theBlock.Faces[faceIndex].TriangleUV[1] = uv[3];
                                                                        theBlock.Faces[faceIndex].TriangleUV[2] = uv[1];
                                                                    }

                                                                    if (faceIndex == (int)BlockFaces.CeilingTriangle2)
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
                                                            if (isFloor)
                                                            {
                                                                if (!theBlock.FloorSplitRealDirection)
                                                                {
                                                                    if (faceIndex == (int)BlockFaces.Floor)
                                                                    {
                                                                        theBlock.Faces[faceIndex].TriangleUV[0] = uv[2];
                                                                        theBlock.Faces[faceIndex].TriangleUV[1] = uv[1];
                                                                        theBlock.Faces[faceIndex].TriangleUV[2] = uv[3];

                                                                        newRot = (sbyte)(newRot + 2);
                                                                    }

                                                                    if (faceIndex == (int)BlockFaces.FloorTriangle2)
                                                                    {
                                                                        theBlock.Faces[faceIndex].TriangleUV2[0] = uv[2];
                                                                        theBlock.Faces[faceIndex].TriangleUV2[1] = uv[1];
                                                                        theBlock.Faces[faceIndex].TriangleUV2[2] = uv[3];

                                                                        newRot = (sbyte)(newRot + 1);
                                                                    }
                                                                }
                                                                else
                                                                {
                                                                    if (faceIndex == (int)BlockFaces.Floor)
                                                                    {
                                                                        theBlock.Faces[faceIndex].TriangleUV[0] = uv[2];
                                                                        theBlock.Faces[faceIndex].TriangleUV[1] = uv[1];
                                                                        theBlock.Faces[faceIndex].TriangleUV[2] = uv[3];
                                                                    }

                                                                    if (faceIndex == (int)BlockFaces.FloorTriangle2)
                                                                    {
                                                                        theBlock.Faces[faceIndex].TriangleUV2[0] = uv[2];
                                                                        theBlock.Faces[faceIndex].TriangleUV2[1] = uv[1];
                                                                        theBlock.Faces[faceIndex].TriangleUV2[2] = uv[3];

                                                                        newRot = (sbyte)(newRot + 1);
                                                                    }
                                                                }
                                                            }
                                                            else if (isCeiling)
                                                            {
                                                                if (!theBlock.CeilingSplitRealDirection)
                                                                {
                                                                    if (faceIndex == (int)BlockFaces.Ceiling)
                                                                    {
                                                                        theBlock.Faces[faceIndex].TriangleUV[0] = uv[2];
                                                                        theBlock.Faces[faceIndex].TriangleUV[1] = uv[1];
                                                                        theBlock.Faces[faceIndex].TriangleUV[2] = uv[3];

                                                                        newRot = (sbyte)(newRot + 2);
                                                                    }

                                                                    if (faceIndex == (int)BlockFaces.CeilingTriangle2)
                                                                    {
                                                                        theBlock.Faces[faceIndex].TriangleUV2[0] = uv[2];
                                                                        theBlock.Faces[faceIndex].TriangleUV2[1] = uv[1];
                                                                        theBlock.Faces[faceIndex].TriangleUV2[2] = uv[3];

                                                                        newRot = (sbyte)(newRot + 1);
                                                                    }
                                                                }
                                                                else
                                                                {
                                                                    if (faceIndex == (int)BlockFaces.Ceiling)
                                                                    {
                                                                        theBlock.Faces[faceIndex].TriangleUV[0] = uv[2];
                                                                        theBlock.Faces[faceIndex].TriangleUV[1] = uv[1];
                                                                        theBlock.Faces[faceIndex].TriangleUV[2] = uv[3];
                                                                    }

                                                                    if (faceIndex == (int)BlockFaces.CeilingTriangle2)
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
                                                            if (isFloor)
                                                            {
                                                                if (!theBlock.FloorSplitRealDirection)
                                                                {
                                                                    if (faceIndex == (int)BlockFaces.Floor)
                                                                    {
                                                                        theBlock.Faces[faceIndex].TriangleUV[0] = uv[3];
                                                                        theBlock.Faces[faceIndex].TriangleUV[1] = uv[2];
                                                                        theBlock.Faces[faceIndex].TriangleUV[2] = uv[0];

                                                                        newRot = (sbyte)(newRot + 2);
                                                                    }

                                                                    if (faceIndex == (int)BlockFaces.FloorTriangle2)
                                                                    {
                                                                        theBlock.Faces[faceIndex].TriangleUV2[0] = uv[3];
                                                                        theBlock.Faces[faceIndex].TriangleUV2[1] = uv[2];
                                                                        theBlock.Faces[faceIndex].TriangleUV2[2] = uv[0];

                                                                        newRot = (sbyte)(newRot + 1);
                                                                    }
                                                                }
                                                                else
                                                                {
                                                                    if (faceIndex == (int)BlockFaces.Floor)
                                                                    {
                                                                        theBlock.Faces[faceIndex].TriangleUV[0] = uv[3];
                                                                        theBlock.Faces[faceIndex].TriangleUV[1] = uv[2];
                                                                        theBlock.Faces[faceIndex].TriangleUV[2] = uv[0];
                                                                    }

                                                                    if (faceIndex == (int)BlockFaces.FloorTriangle2)
                                                                    {
                                                                        theBlock.Faces[faceIndex].TriangleUV2[0] = uv[3];
                                                                        theBlock.Faces[faceIndex].TriangleUV2[1] = uv[2];
                                                                        theBlock.Faces[faceIndex].TriangleUV2[2] = uv[0];

                                                                        newRot = (sbyte)(newRot + 1);
                                                                    }
                                                                }
                                                            }
                                                            else if (isCeiling)
                                                            {
                                                                if (!theBlock.CeilingSplitRealDirection)
                                                                {
                                                                    if (faceIndex == (int)BlockFaces.Ceiling)
                                                                    {
                                                                        theBlock.Faces[faceIndex].TriangleUV[0] = uv[3];
                                                                        theBlock.Faces[faceIndex].TriangleUV[1] = uv[2];
                                                                        theBlock.Faces[faceIndex].TriangleUV[2] = uv[0];

                                                                        newRot = (sbyte)(newRot + 2);
                                                                    }

                                                                    if (faceIndex == (int)BlockFaces.CeilingTriangle2)
                                                                    {
                                                                        theBlock.Faces[faceIndex].TriangleUV2[0] = uv[3];
                                                                        theBlock.Faces[faceIndex].TriangleUV2[1] = uv[2];
                                                                        theBlock.Faces[faceIndex].TriangleUV2[2] = uv[0];

                                                                        newRot = (sbyte)(newRot + 1);
                                                                    }
                                                                }
                                                                else
                                                                {
                                                                    if (faceIndex == (int)BlockFaces.Ceiling)
                                                                    {
                                                                        theBlock.Faces[faceIndex].TriangleUV[0] = uv[3];
                                                                        theBlock.Faces[faceIndex].TriangleUV[1] = uv[2];
                                                                        theBlock.Faces[faceIndex].TriangleUV[2] = uv[0];
                                                                    }

                                                                    if (faceIndex == (int)BlockFaces.CeilingTriangle2)
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
                                                    else // ===========================================================================================
                                                    {
                                                        if (theFace._txtTriangle == 0)
                                                        {
                                                            if (isFloor)
                                                            {
                                                                if (!theBlock.FloorSplitRealDirection)
                                                                {
                                                                    if (faceIndex == (int)BlockFaces.Floor) // CORRETTO
                                                                    {
                                                                        theBlock.Faces[faceIndex].TriangleUV[0] = uv[0];
                                                                        theBlock.Faces[faceIndex].TriangleUV[1] = uv[1];
                                                                        theBlock.Faces[faceIndex].TriangleUV[2] = uv[3];

                                                                        newRot = (sbyte)(newRot + 2);
                                                                    }

                                                                    if (faceIndex == (int)BlockFaces.FloorTriangle2) // CORRETTO
                                                                    {
                                                                        theBlock.Faces[faceIndex].TriangleUV2[0] = uv[0];
                                                                        theBlock.Faces[faceIndex].TriangleUV2[1] = uv[1];
                                                                        theBlock.Faces[faceIndex].TriangleUV2[2] = uv[3];

                                                                        newRot = (sbyte)(newRot + 1);
                                                                    }
                                                                }
                                                                else
                                                                {
                                                                    if (faceIndex == (int)BlockFaces.Floor) // CORRETTO
                                                                    {
                                                                        theBlock.Faces[faceIndex].TriangleUV[0] = uv[0];
                                                                        theBlock.Faces[faceIndex].TriangleUV[1] = uv[1];
                                                                        theBlock.Faces[faceIndex].TriangleUV[2] = uv[3];
                                                                    }

                                                                    if (faceIndex == (int)BlockFaces.FloorTriangle2
                                                                    ) // CORRETTO
                                                                    {
                                                                        theBlock.Faces[faceIndex].TriangleUV2[0] = uv[0];
                                                                        theBlock.Faces[faceIndex].TriangleUV2[1] = uv[1];
                                                                        theBlock.Faces[faceIndex].TriangleUV2[2] = uv[3];

                                                                        newRot = (sbyte)(newRot + 1);
                                                                    }
                                                                }
                                                            }
                                                            else if (isCeiling)
                                                            {
                                                                if (!theBlock.CeilingSplitRealDirection)
                                                                {
                                                                    if (faceIndex == (int)BlockFaces.Ceiling) // CORRETTO
                                                                    {
                                                                        theBlock.Faces[faceIndex].TriangleUV[0] = uv[0];
                                                                        theBlock.Faces[faceIndex].TriangleUV[1] = uv[1];
                                                                        theBlock.Faces[faceIndex].TriangleUV[2] = uv[3];

                                                                        newRot = (sbyte)(newRot + 2);
                                                                    }

                                                                    if (faceIndex == (int)BlockFaces.CeilingTriangle2
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
                                                                    if (faceIndex == (int)BlockFaces.Ceiling) // CORRETTO
                                                                    {
                                                                        theBlock.Faces[faceIndex].TriangleUV[0] = uv[0];
                                                                        theBlock.Faces[faceIndex].TriangleUV[1] = uv[1];
                                                                        theBlock.Faces[faceIndex].TriangleUV[2] = uv[3];
                                                                    }

                                                                    if (faceIndex == (int)BlockFaces.CeilingTriangle2
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
                                                            if (isFloor)
                                                            {
                                                                if (!theBlock.FloorSplitRealDirection)
                                                                {
                                                                    if (faceIndex == (int)BlockFaces.Floor) // CORRETTO
                                                                    {
                                                                        theBlock.Faces[faceIndex].TriangleUV[0] = uv[1];
                                                                        theBlock.Faces[faceIndex].TriangleUV[1] = uv[2];
                                                                        theBlock.Faces[faceIndex].TriangleUV[2] = uv[0];

                                                                        newRot = (sbyte)(newRot + 2);
                                                                    }

                                                                    if (faceIndex == (int)BlockFaces.FloorTriangle2
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
                                                                    if (faceIndex == (int)BlockFaces.Floor) // CORRETTO
                                                                    {
                                                                        theBlock.Faces[faceIndex].TriangleUV[0] = uv[1];
                                                                        theBlock.Faces[faceIndex].TriangleUV[1] = uv[2];
                                                                        theBlock.Faces[faceIndex].TriangleUV[2] = uv[0];
                                                                    }

                                                                    if (faceIndex == (int)BlockFaces.FloorTriangle2
                                                                    ) // CORRETTO
                                                                    {
                                                                        theBlock.Faces[faceIndex].TriangleUV2[0] = uv[1];
                                                                        theBlock.Faces[faceIndex].TriangleUV2[1] = uv[2];
                                                                        theBlock.Faces[faceIndex].TriangleUV2[2] = uv[0];

                                                                        newRot = (sbyte)(newRot + 1);
                                                                    }
                                                                }
                                                            }
                                                            else if (isCeiling)
                                                            {
                                                                if (!theBlock.CeilingSplitRealDirection)
                                                                {
                                                                    if (faceIndex == (int)BlockFaces.Ceiling) // CORRETTO
                                                                    {
                                                                        theBlock.Faces[faceIndex].TriangleUV[0] = uv[1];
                                                                        theBlock.Faces[faceIndex].TriangleUV[1] = uv[2];
                                                                        theBlock.Faces[faceIndex].TriangleUV[2] = uv[0];

                                                                        newRot = (sbyte)(newRot + 2);
                                                                    }

                                                                    if (faceIndex == (int)BlockFaces.CeilingTriangle2
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
                                                                    if (faceIndex == (int)BlockFaces.Ceiling) // CORRETTO
                                                                    {
                                                                        theBlock.Faces[faceIndex].TriangleUV[0] = uv[1];
                                                                        theBlock.Faces[faceIndex].TriangleUV[1] = uv[2];
                                                                        theBlock.Faces[faceIndex].TriangleUV[2] = uv[0];
                                                                    }

                                                                    if (faceIndex == (int)BlockFaces.CeilingTriangle2
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
                                                            if (isFloor)
                                                            {
                                                                if (!theBlock.FloorSplitRealDirection)
                                                                {
                                                                    if (faceIndex == (int)BlockFaces.Floor) // OK
                                                                    {
                                                                        theBlock.Faces[faceIndex].TriangleUV[0] = uv[3];
                                                                        theBlock.Faces[faceIndex].TriangleUV[1] = uv[0];
                                                                        theBlock.Faces[faceIndex].TriangleUV[2] = uv[2];

                                                                        newRot = (sbyte)(newRot + 2);
                                                                    }

                                                                    if (faceIndex == (int)BlockFaces.FloorTriangle2) // TODO
                                                                    {
                                                                        theBlock.Faces[faceIndex].TriangleUV2[0] = uv[3];
                                                                        theBlock.Faces[faceIndex].TriangleUV2[1] = uv[0];
                                                                        theBlock.Faces[faceIndex].TriangleUV2[2] = uv[2];

                                                                        newRot = (sbyte)(newRot + 1);
                                                                    }
                                                                }
                                                                else
                                                                {
                                                                    if (faceIndex == (int)BlockFaces.Floor) // OK
                                                                    {
                                                                        theBlock.Faces[faceIndex].TriangleUV[0] = uv[3];
                                                                        theBlock.Faces[faceIndex].TriangleUV[1] = uv[0];
                                                                        theBlock.Faces[faceIndex].TriangleUV[2] = uv[2];
                                                                    }

                                                                    if (faceIndex == (int)BlockFaces.FloorTriangle2) // OK
                                                                    {
                                                                        theBlock.Faces[faceIndex].TriangleUV2[0] = uv[3];
                                                                        theBlock.Faces[faceIndex].TriangleUV2[1] = uv[0];
                                                                        theBlock.Faces[faceIndex].TriangleUV2[2] = uv[2];

                                                                        newRot = (sbyte)(newRot + 1);
                                                                    }
                                                                }
                                                            }
                                                            else if (isCeiling)
                                                            {
                                                                if (!theBlock.CeilingSplitRealDirection)
                                                                {
                                                                    if (faceIndex == (int)BlockFaces.Ceiling) // OK
                                                                    {
                                                                        theBlock.Faces[faceIndex].TriangleUV[0] = uv[3];
                                                                        theBlock.Faces[faceIndex].TriangleUV[1] = uv[0];
                                                                        theBlock.Faces[faceIndex].TriangleUV[2] = uv[2];

                                                                        newRot = (sbyte)(newRot + 2);
                                                                    }

                                                                    if (faceIndex ==
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
                                                                    if (faceIndex == (int)BlockFaces.Ceiling) // OK
                                                                    {
                                                                        theBlock.Faces[faceIndex].TriangleUV[0] = uv[3];
                                                                        theBlock.Faces[faceIndex].TriangleUV[1] = uv[0];
                                                                        theBlock.Faces[faceIndex].TriangleUV[2] = uv[2];
                                                                    }

                                                                    if (faceIndex ==
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
                                                            if (isFloor)
                                                            {
                                                                if (!theBlock.FloorSplitRealDirection)
                                                                {
                                                                    if (faceIndex == (int)BlockFaces.Floor) // CORRETTO
                                                                    {
                                                                        theBlock.Faces[faceIndex].TriangleUV[0] = uv[2];
                                                                        theBlock.Faces[faceIndex].TriangleUV[1] = uv[3];
                                                                        theBlock.Faces[faceIndex].TriangleUV[2] = uv[1];

                                                                        newRot = (sbyte)(newRot + 2);
                                                                    }

                                                                    if (faceIndex == (int)BlockFaces.FloorTriangle2) // TODO
                                                                    {
                                                                        theBlock.Faces[faceIndex].TriangleUV2[0] = uv[2];
                                                                        theBlock.Faces[faceIndex].TriangleUV2[1] = uv[3];
                                                                        theBlock.Faces[faceIndex].TriangleUV2[2] = uv[1];

                                                                        newRot = (sbyte)(newRot + 1);
                                                                    }
                                                                }
                                                                else
                                                                {
                                                                    if (faceIndex == (int)BlockFaces.Floor) // CORRETTO
                                                                    {
                                                                        theBlock.Faces[faceIndex].TriangleUV[0] = uv[2];
                                                                        theBlock.Faces[faceIndex].TriangleUV[1] = uv[3];
                                                                        theBlock.Faces[faceIndex].TriangleUV[2] = uv[1];
                                                                    }

                                                                    if (faceIndex == (int)BlockFaces.FloorTriangle2
                                                                    ) // CORRETTO
                                                                    {
                                                                        theBlock.Faces[faceIndex].TriangleUV2[0] = uv[2];
                                                                        theBlock.Faces[faceIndex].TriangleUV2[1] = uv[3];
                                                                        theBlock.Faces[faceIndex].TriangleUV2[2] = uv[1];

                                                                        newRot = (sbyte)(newRot + 1);
                                                                    }
                                                                }
                                                            }
                                                            else if (isCeiling)
                                                            {
                                                                if (!theBlock.CeilingSplitRealDirection)
                                                                {
                                                                    if (faceIndex == (int)BlockFaces.Ceiling) // CORRETTO
                                                                    {
                                                                        theBlock.Faces[faceIndex].TriangleUV[0] = uv[2];
                                                                        theBlock.Faces[faceIndex].TriangleUV[1] = uv[3];
                                                                        theBlock.Faces[faceIndex].TriangleUV[2] = uv[1];

                                                                        newRot = (sbyte)(newRot + 2);
                                                                    }

                                                                    if (faceIndex ==
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
                                                                    if (faceIndex == (int)BlockFaces.Ceiling) // CORRETTO
                                                                    {
                                                                        theBlock.Faces[faceIndex].TriangleUV[0] = uv[2];
                                                                        theBlock.Faces[faceIndex].TriangleUV[1] = uv[3];
                                                                        theBlock.Faces[faceIndex].TriangleUV[2] = uv[1];
                                                                    }

                                                                    if (faceIndex == (int)BlockFaces.CeilingTriangle2
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
                            throw new Exception("Room '" + room + "' has a sector size of zero. This is invalid. Probably the room was referenced but never initialized.");
                
                progressReporter.ReportProgress(95, "Building rooms");

                foreach (var room in level.Rooms.Where(r => r != null))
                {
                    room.BuildGeometry();
                    room.CalculateLightingForThisRoom();
                    room.UpdateBuffers();
                }

                level.RebuildAllAlphaTests();

                progressReporter.ReportProgress(100, "Level loaded correctly!");

                return level;
            }
            catch
            { 
                level.Dispose(); // We log in the level above
                throw;
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
