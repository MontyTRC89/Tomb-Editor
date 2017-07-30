using System;
using System.Collections.Generic;
using System.Linq;
using System.Drawing;
using System.IO;
using SharpDX.Toolkit.Graphics;
using TombLib.Wad;
using TombLib.IO;
using SharpDX;
using Color = System.Drawing.Color;
using System.Windows.Forms;
using System.Diagnostics;
using NLog;

namespace TombEditor.Geometry
{
    public class Level : IDisposable
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        private struct PrjSlot
        {
            public bool _present;
            public int _objectId;
            public string _name;
            public short _slotType;
        }

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

        public const short MaxSectorCoord = 255;
        public const short MaxNumberOfRooms = 512;
        public Room[] Rooms { get; } = new Room[MaxNumberOfRooms]; //Rooms in level

        public Dictionary<int, LevelTexture> TextureSamples { get; } =
            new Dictionary<int, LevelTexture>(); //Texture tiles

        public Dictionary<int, Texture2D> Textures { get; } =
            new Dictionary<int, Texture2D>(); //DirectX textures... For now just one texture atlas 2048x2048 pixel

        public Bitmap _textureMap; //The texture map on the CPU
        public Dictionary<int, Portal> Portals { get; } = new Dictionary<int, Portal>();
        public Dictionary<int, TriggerInstance> Triggers { get; } = new Dictionary<int, TriggerInstance>();

        public Dictionary<int, IObjectInstance> Objects { get; } =
            new Dictionary<int, IObjectInstance>(); //Objects (moveables, static meshes, sinks, camera, fly-by cameras, sound sources)

        public List<AnimatedTextureSet> AnimatedTextures { get; } = new List<AnimatedTextureSet>();
        public List<TextureSound> TextureSounds { get; } = new List<TextureSound>();
        public Wad Wad { get; private set; }
        public string TextureFile { get; private set; }
        public string WadFile { get; private set; }
        public bool MustSave { get; set; } // Used for Save and Save as logic
        public string FileName { get; private set; }

        public short GetRoomIndex(Room room)
        {
            for (short i = 0; i < MaxNumberOfRooms; ++i)
                if (Rooms[i] == room)
                    return i;
            return -1;
        }
        
        public HashSet<Room> GetConnectedRooms(Room startingRoom)
        {
            HashSet<Room> result = new HashSet<Room>();
            GetConnectedRoomsRecursively(result, startingRoom);
            if (startingRoom.Flipped && (startingRoom.AlternateRoom != -1))
                GetConnectedRoomsRecursively(result, Rooms[startingRoom.AlternateRoom]);
            return result;
        }

        private void GetConnectedRoomsRecursively(HashSet<Room> result, Room startingRoom)
        {
            result.Add(startingRoom);
            foreach (int PortalIndex in startingRoom.Portals)
            {
                Room room = Rooms[Portals[PortalIndex].AdjoiningRoom];
                if (!result.Contains(room))
                {
                    GetConnectedRoomsRecursively(result, room);
                    if (room.Flipped && (room.AlternateRoom != -1))
                        GetConnectedRoomsRecursively(result, Rooms[room.AlternateRoom]);
                }
            }
        }

        public IEnumerable<Room> GetVerticallyAscendingRoomList()
        {
            var roomList = new List<KeyValuePair<float, Room>>();
            foreach (Room room in Rooms)
                if (room != null)
                    roomList.Add(new KeyValuePair<float, Room>(room.Position.Y + room.GetHighestCorner(), room));
            var result = roomList
                .OrderBy((roomPair) => roomPair.Key) // don't use the Sort member function because it is unstable!
                .Select(roomKey => roomKey.Value).ToList();
            return result;
        }

        public int AddTexture(short x, short y, short w, short h, bool IsDoubleSided, bool IsTransparent)
        {
            short newX = x;
            short newY = y;

            // Step 1: check if there's another texture already in the list
            foreach (var texture in TextureSamples.Values)
            {
                if (texture.X == newX && (texture.Y + 256 * texture.Page) == newY && texture.Width == w &&
                    texture.Height == h
                    && texture.DoubleSided == IsDoubleSided && texture.Transparent == IsTransparent)
                    return texture.ID;
            }

            // Step 2: get the new texture ID
            int id = -1;
            for (int i = 0; i < TextureSamples.Count; i++)
            {
                if (!TextureSamples.ContainsKey(i) && id == -1)
                    id = i;
            }

            if (id == -1)
                id = TextureSamples.Count;

            // Step 3: if no compatible texture is found, then add a new texture tile
            short page = (short) Math.Floor(y / 256.0f);
            var newTexture = new LevelTexture
            {
                X = newX,
                Y = (short) (newY - page * 256),
                Width = w,
                Height = h,
                Page = page,
                ID = id,
                Transparent = IsTransparent,
                DoubleSided = IsDoubleSided
            };

            TextureSamples.Add(id, newTexture);

            return id;
        }

        public void Dispose()
        {
            // First clean the old data
            foreach (var texture in Textures.Values)
                texture.Dispose(); // Dispose DirectX texture and release GPU memory

            _textureMap?.Dispose();

            Wad?.Dispose();

            GC.Collect();
        }

        public void LoadTextureMap(string filename, GraphicsDevice device)
        {
            logger.Warn("Loading texture map");

            var watch = new Stopwatch();
            watch.Start();

            //Free old texture map...
            _textureMap?.Dispose();

            // Load texture map as a bitmap
            _textureMap = TombLib.Graphics.TextureLoad.LoadToBitmap(filename);
            Utils.ConvertTextureTo256Width(ref _textureMap);

            // Calculate the number of pages
            int numPages = (int) Math.Floor(_textureMap.Height / 256.0f);
            if (_textureMap.Height % 256 != 0)
                numPages++;

            logger.Debug("Building 2048x2048 texture atlas for DirectX");

            if (device != null)
            {
                // Copy the page in a temp bitmap. I generate a texture atlas, putting all texture pages inside 2048x2048 pixel 
                // textures.
                using (var tempBitmap = new Bitmap(2048, 2048, System.Drawing.Imaging.PixelFormat.Format32bppArgb))
                {
                    using (var g = Graphics.FromImage(tempBitmap))
                    {
                        int currentXblock = 0;
                        int currentYblock = 0;
                        for (int i = 0; i < numPages; i++)
                        {
                            var src = new System.Drawing.RectangleF(0, 256 * i, 256, 256);
                            var dest = new System.Drawing.RectangleF(currentXblock * 256, currentYblock * 256, 256, 256);

                            g.DrawImage(_textureMap, dest, src, GraphicsUnit.Pixel);

                            currentXblock++;
                            if (currentXblock != 8)
                                continue;

                            currentXblock = 0;
                            currentYblock++;
                        }
                    }

                    // Clean up DirectX texture
                    if (Textures.ContainsKey(0))
                    {
                        logger.Debug("Cleaning memory used by a previous texture map");

                        Textures[0].Dispose();
                        Textures.Remove(0);
                    }

                    // Create DirectX texture
                    Textures.Add(0, TombLib.Graphics.TextureLoad.LoadToTexture(device, tempBitmap));
                }
            }

            TextureFile = filename;

            watch.Stop();

            logger.Info("Texture map loaded");
            logger.Info("    Elapsed time: " + watch.ElapsedMilliseconds + " ms");
        }

        public void LoadWad(string filename, GraphicsDevice device)
        {
            // Load the WAD
            Wad = Wad.LoadWad(filename);
            WadFile = filename;

            Wad.GraphicsDevice = device;
            Wad.PrepareDataForDirectX();

            // Prepare vertex buffers and index buffers
            foreach (var moveable in Wad.Moveables.Values)
            {
                // Build the mesh tree
                moveable.BuildHierarchy();

                // If moveable has animations, then build the animation pose of first frame of animation 0
                if (moveable.Animations.Count > 0 && moveable.Animations[0].KeyFrames.Count > 0)
                    moveable.BuildAnimationPose(moveable.Animations[0].KeyFrames[0]);

                moveable.BuildBuffers();
            }

            foreach (var staticMesh in Wad.StaticMeshes.Values)
            {
                staticMesh.BuildBuffers();
            }
        }

        public int GetNewPortalId()
        {
            int i = 0;
            while (Portals.ContainsKey(i))
                ++i;
            return i;
        }

        public int GetNewTriggerId()
        {
            int i = 0;
            while (Triggers.ContainsKey(i))
                ++i;
            return i;
        }

        public int GetNewObjectId()
        {
            int i = 0;
            while (Objects.ContainsKey(i))
                ++i;
            return i;
        }

        public static Level LoadFromPrj(string filename, FormImportPRJ form, GraphicsDevice device)
        {
            GC.Collect();

            var level = new Level();

            try
            {
                // Open file
                var reader = new BinaryReaderEx(File.OpenRead(filename));

                form.ReportProgress(0, "Begin of PRJ import");

                logger.Warn("Opening Winroomedit PRJ file");

                // Check if it's a NGLE PRJ
                bool ngle = false;
                reader.BaseStream.Seek(reader.BaseStream.Length - 8, SeekOrigin.Begin);
                var bytesNgle = reader.ReadBytes(4);
                if (bytesNgle[0] == 0x4E && bytesNgle[1] == 0x47 && bytesNgle[2] == 0x4C && bytesNgle[3] == 0x45)
                {
                    form.ReportProgress(1, "This is a NGLE project");
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
                for (int i = 0; i < MaxNumberOfRooms; ++i)
                    level.Rooms[i] = null;

                var tempRooms = new Dictionary<int, PrjBlock[,]>();
                var flipInfos = new List<PrjFlipInfo>();

                form.ReportProgress(2, "Number of rooms: " + numRooms);
                double progress = 2;

                for (int i = 0; i < numRooms; i++)
                {
                    // Room is defined?
                    short defined = reader.ReadInt16();
                    if (defined == 0x01)
                        continue;

                    // Read room's name
                    string roomName = System.Text.Encoding.ASCII.GetString(reader.ReadBytes(80));

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

                    var room = new Room(level, posXBlocks, (int)(yPos / 256.0f), posZBlocks, (byte)numXBlocks,
                        (byte)numZBlocks, 0);

                    short numPortals = reader.ReadInt16();
                    var portalThings = new short[numPortals];

                    for (int j = 0; j < numPortals; j++)
                    {
                        portalThings[j] = reader.ReadInt16();
                    }

                    var tmpPortals = new List<int>();

                    logger.Info("    Portals: " + numPortals);

                    for (int j = 0; j < numPortals; j++)
                    {
                        ushort direction = reader.ReadUInt16();
                        short portalX = reader.ReadInt16();
                        short portalZ = reader.ReadInt16();
                        short portalXBlocks = reader.ReadInt16();
                        short portalZBlocks = reader.ReadInt16();
                        reader.ReadInt16();
                        short portalRoom = reader.ReadInt16();
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

                        p.MemberOfFlippedRoom = (p.Room != i);
                        p.Room = (short)i;
                        p.PrjRealRoom = (short)i;

                        p.PrjThingIndex = portalThings[j];
                        p.PrjOtherThingIndex = portalSlot;

                        tmpPortals.Add(p.ID);

                        level.Portals.Add(p.ID, p);
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
                        short objRoom = reader.ReadInt16();
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
                                var instance = new MoveableInstance(objectsThings[j], objRoom)
                                {
                                    Bits =
                                    {
                                        [0] = (objOcb & 0x0002) != 0,
                                        [1] = (objOcb & 0x0004) != 0,
                                        [2] = (objOcb & 0x0008) != 0,
                                        [3] = (objOcb & 0x0010) != 0,
                                        [4] = (objOcb & 0x0020) != 0
                                    },
                                    Invisible = (objOcb & 0x0001) != 0,
                                    ClearBody = (objOcb & 0x0080) != 0,
                                    ObjectID = objSlot,
                                    X = (byte)(objPosX),
                                    Z = (byte)(objPosZ),
                                    Y = (short)objLongY,
                                    OCB = objTimer
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

                                level.Objects.Add(instance.ID, instance);
                                room.Moveables.Add(instance.ID);
                            }
                            else
                            {
                                StaticMeshInstance instance = new StaticMeshInstance(objectsThings[j], objRoom);

                                instance.Bits[0] = (objOcb & 0x0002) != 0;
                                instance.Bits[1] = (objOcb & 0x0004) != 0;
                                instance.Bits[2] = (objOcb & 0x0008) != 0;
                                instance.Bits[3] = (objOcb & 0x0010) != 0;
                                instance.Bits[4] = (objOcb & 0x0020) != 0;
                                instance.Invisible = (objOcb & 0x0001) != 0;
                                instance.ClearBody = (objOcb & 0x0080) != 0;
                                instance.ObjectID = objSlot - (ngle ? 520 : 465);
                                instance.X = (byte)(objPosX);
                                instance.Z = (byte)(objPosZ);
                                instance.Y = (short)objLongY;

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

                                level.Objects.Add(instance.ID, instance);
                                room.StaticMeshes.Add(instance.ID);
                            }
                        }
                        else
                        {
                            short currentRoom = (short)i;

                            var trigger = new TriggerInstance(level.GetNewTriggerId(), currentRoom)
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

                            trigger.Bits[4] = (triggerFlags & 0x0002) == 0;
                            trigger.Bits[3] = (triggerFlags & 0x0004) == 0;
                            trigger.Bits[2] = (triggerFlags & 0x0008) == 0;
                            trigger.Bits[1] = (triggerFlags & 0x0010) == 0;
                            trigger.Bits[0] = (triggerFlags & 0x0020) == 0;
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
                                    trigger.TargetType = TriggerTargetType.CutsceneOrParameterNG;
                                    break;
                                case 14:
                                    trigger.TargetType = TriggerTargetType.FMV;
                                    break;
                            }

                            level.Triggers.Add(trigger.ID, trigger);
                        }
                    }

                    room.AmbientLight = Color.FromArgb(255, reader.ReadByte(), reader.ReadByte(), reader.ReadByte());
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
                        short objRoom = reader.ReadInt16();
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

                                var light = new Light
                                {
                                    Color = Color.FromArgb(255, lightR, lightG, lightB),
                                    Cutoff = lightCut,
                                    Len = lightLen,
                                    DirectionX = 360.0f - lightX,
                                    DirectionY = lightY + 90.0f
                                };
                                if (light.DirectionY >= 360)
                                    light.DirectionY = light.DirectionY - 360.0f;
                                light.Active = (lightOn == 0x01);
                                light.In = lightIn;
                                light.Out = lightOut;
                                light.Intensity = lightIntensity / 8192.0f;

                                light.X = (byte)(objPosX);
                                light.Z = (byte)(objPosZ);
                                light.Y = (short)objLongY;

                                switch (objectType)
                                {
                                    case 0x4000:
                                        light.Type = LightType.Light;
                                        light.In /= 1024.0f;
                                        light.Out /= 1024.0f;
                                        break;
                                    case 0x6000:
                                        light.Type = LightType.Shadow;
                                        light.In /= 1024.0f;
                                        light.Out /= 1024.0f;
                                        break;
                                    case 0x4200:
                                        light.Type = LightType.Sun;
                                        break;
                                    case 0x5000:
                                        light.In = 512.0f;
                                        light.Out = 1536.0f;
                                        light.Type = LightType.Effect;
                                        break;
                                    case 0x4100:
                                        light.Type = LightType.Spot;
                                        light.Len /= 1024.0f;
                                        light.Cutoff /= 1024.0f;
                                        break;
                                    case 0x4020:
                                        light.In /= 1024.0f;
                                        light.Out /= 1024.0f;
                                        light.Type = LightType.FogBulb;
                                        break;
                                }

                                room.Lights.Add(light);
                                break;
                            case 0x4c00:
                                var sound = new SoundInstance(objectsThings2[j], objRoom)
                                {
                                    SoundID = objSlot,
                                    X = (byte)(objPosX),
                                    Z = (byte)(objPosZ),
                                    Y = (short)objLongY
                                };


                                level.Objects.Add(sound.ID, sound);
                                break;
                            case 0x4400:
                                var sink = new SinkInstance(objectsThings2[j], objRoom);

                                sink.X = (byte)(objPosX);
                                sink.Z = (byte)(objPosZ);
                                sink.Y = (short)objLongY;

                                sink.Strength = (short)(objTimer / 2);

                                level.Objects.Add(sink.ID, sink);
                                room.Sinks.Add(sink.ID);
                                break;
                            case 0x4800:
                            case 0x4080:
                                {
                                    var camera = new CameraInstance(objectsThings2[j], objRoom)
                                    {
                                        Timer = objTimer,
                                        Fixed = (objectType == 0x4080),
                                        X = (byte)(objPosX),
                                        Z = (byte)(objPosZ),
                                        Y = (short)objLongY
                                    };

                                    level.Objects.Add(camera.ID, camera);
                                    room.Cameras.Add(camera.ID);
                                }
                                break;
                            case 0x4040:
                                {
                                    var camera = new FlybyCameraInstance(objectsThings2[j], objRoom)
                                    {
                                        Timer = objTimer,
                                        Sequence = (short)((objSlot & 0xe000) >> 13),
                                        Number = (short)((objSlot & 0x1f00) >> 8),
                                        FOV = (short)((objSlot & 0x00ff)),
                                        Roll = objRoll,
                                        Speed = (short)(objSpeed / 655),
                                        X = (byte)(objPosX),
                                        Z = (byte)(objPosZ),
                                        Y = (short)objLongY,
                                        DirectionX = (short)(-objUnk),
                                        DirectionY = (short)(objFacing + 90)
                                    };

                                    if (camera.DirectionY >= 360)
                                        camera.DirectionY = (short)(camera.DirectionY - 360);

                                    camera.Flags[0] = ((objOcb & 0x01) != 0);
                                    camera.Flags[1] = ((objOcb & 0x02) != 0);
                                    camera.Flags[2] = ((objOcb & 0x04) != 0);
                                    camera.Flags[3] = ((objOcb & 0x08) != 0);
                                    camera.Flags[4] = ((objOcb & 0x10) != 0);
                                    camera.Flags[5] = ((objOcb & 0x20) != 0);
                                    camera.Flags[6] = ((objOcb & 0x40) != 0);
                                    camera.Flags[7] = ((objOcb & 0x80) != 0);
                                    camera.Flags[8] = ((objOcb & 0x0100) != 0);
                                    camera.Flags[9] = ((objOcb & 0x0200) != 0);
                                    camera.Flags[10] = ((objOcb & 0x0400) != 0);
                                    camera.Flags[11] = ((objOcb & 0x0800) != 0);
                                    camera.Flags[12] = ((objOcb & 0x1000) != 0);
                                    camera.Flags[13] = ((objOcb & 0x2000) != 0);
                                    camera.Flags[14] = ((objOcb & 0x4000) != 0);
                                    camera.Flags[15] = ((objOcb & 0x8000) != 0);

                                    level.Objects.Add(camera.ID, camera);
                                    room.FlyByCameras.Add(camera.ID);
                                }
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
                    room.AlternateRoom = -1;
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

                    sbyte deltaCeilingMain = (sbyte)(lowest + 20);

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

                            room.Blocks[x, z] = new Block(typ, BlockFlags.None, 20)
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

                            room.Ceiling = 20;

                            room.Blocks[x, z].WSFaces[0] = (sbyte)(b._wsFaces[0]);
                            room.Blocks[x, z].WSFaces[1] = (sbyte)(b._wsFaces[3]);
                            room.Blocks[x, z].WSFaces[2] = (sbyte)(b._wsFaces[2]);
                            room.Blocks[x, z].WSFaces[3] = (sbyte)(b._wsFaces[1]);

                            room.Blocks[x, z].RFFaces[0] = (sbyte)(b._rfFaces[0]);
                            room.Blocks[x, z].RFFaces[1] = (sbyte)(b._rfFaces[3]);
                            room.Blocks[x, z].RFFaces[2] = (sbyte)(b._rfFaces[2]);
                            room.Blocks[x, z].RFFaces[3] = (sbyte)(b._rfFaces[1]);

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

                    // Fix lights
                    foreach (var light in room.Lights)
                    {
                        light.Position = new Vector3((room.NumXSectors - 1) * 1024.0f - light.Position.X + 512.0f,
                            light.Position.Y,
                            light.Position.Z + 512.0f);
                    }

                    level.Rooms[i] = room;

                    progress += (i / (float)numRooms * 0.28f);

                    form.ReportProgress((int)progress, "");
                }

                form.ReportProgress(30, "Rooms loaded");

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


                // Read TGA string
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

                logger.Debug("Texture map: " + textureFilename);

                if (textureFilename == "" || !File.Exists(textureFilename))
                {
                    logger.Error("Can't find texture map!");

                    if (DarkUI.Forms.DarkMessageBox.ShowWarning(
                            "The texture file '" + textureFilename +
                            " could not be found. Do you want to browse it or cancel importing?",
                            "Open project", DarkUI.Forms.DarkDialogButton.YesNo) != DialogResult.Yes)
                    {
                        logger.Error("PRJ import canceled");
                        reader.Close();
                        return null;
                    }

                    // Ask for TGA file
                    textureFilename = form.OpenTGA();
                    if (textureFilename == "")
                    {
                        logger.Error("PRJ import canceled");
                        reader.Close();
                        return null;
                    }
                }

                level.LoadTextureMap(textureFilename, device);

                form.ReportProgress(50, "Converted '" + textureFilename + "' to PNG format");

                // Read textures
                int numTextures = reader.ReadInt32();

                form.ReportProgress(52, "Loading textures");
                form.ReportProgress(52, "    Number of textures: " + numTextures);

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

                // Read WAD string
                stringBuffer = new byte[255];
                sb = 0;
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

                if (wadName == "" || !File.Exists(wadName))
                {
                    if (DarkUI.Forms.DarkMessageBox.ShowWarning(
                            "The WAD file '" + wadName +
                            " could not be found. Do you want to browse it or cancel importing?",
                            "Open project", DarkUI.Forms.DarkDialogButton.YesNo) != DialogResult.Yes)
                    {
                        reader.Close();
                        return null;
                    }

                    // Ask for WAD file
                    wadName = form.OpenWAD();
                    if (wadName == "")
                    {
                        reader.Close();
                        return null;
                    }
                }

                string wadPath = Path.GetDirectoryName(wadName);
                string wadBase = Path.GetFileNameWithoutExtension(wadName);

                form.ReportProgress(55, "Loading WAD '" + wadName + "'");

                level.LoadWad(wadPath + "\\" + wadBase + ".wad", device);

                form.ReportProgress(60, "WAD loaded");

                int numSlots = reader.ReadInt32();

                var writerSlots = new StreamWriter(File.OpenWrite("slots.txt"));

                for (int i = 0; i < numSlots; i++)
                {
                    var slot = new PrjSlot();

                    short slotType = reader.ReadInt16();
                    if (slotType == 0x00)
                    {
                        writerSlots.WriteLine(i + "\t" + "NOT DEFINED");

                        continue;
                    }

                    stringBuffer = new byte[255];
                    sb = 0;
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

                    slot._objectId = objectId;
                    slot._name = slotName;
                    slot._slotType = slotType;
                    slot._present = true;

                    writerSlots.WriteLine(i + "\t" + slotName + "\t" + slotType + "\t" + objectId);
                }

                writerSlots.Flush();
                writerSlots.Close();

                // Read animated textures
                form.ReportProgress(61, "Loading animated textures and texture sounds");
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
                form.ReportProgress(65, "Flipping reference system");

                int minX = level.Rooms.Where(room => room != null).Select(room => (int)room.Position.X)
                    .Concat(new[] { 1024 }).Min();

                foreach (var room in level.Rooms.Where(room => room != null))
                {
                    room.Position = new Vector3(room.Position.X - minX - room.NumXSectors + 10,
                        room.Position.Y,
                        room.Position.Z);
                }

                form.ReportProgress(67, "Building flipped rooms table");

                for (int i = 0; i < level.Rooms.Length; i++)
                {
                    if (level.Rooms[i] == null)
                        continue;

                    foreach (var info in flipInfos)
                    {
                        if (info._baseRoom == i)
                        {
                            level.Rooms[i].Flipped = true;
                            level.Rooms[i].AlternateRoom = info._flipRoom;
                            level.Rooms[i].AlternateGroup = info._group;
                        }

                        if (info._flipRoom != i)
                            continue;

                        level.Rooms[i].Flipped = true;
                        level.Rooms[i].BaseRoom = info._baseRoom;
                        level.Rooms[i].AlternateGroup = info._group;
                        level.Rooms[i].Position = new Vector3(level.Rooms[info._baseRoom].Position.X,
                            level.Rooms[info._baseRoom].Position.Y,
                            level.Rooms[info._baseRoom].Position.Z);
                    }
                }

                // Fix objects
                form.ReportProgress(70, "Fixing objects positions and data");
                foreach (var instance in level.Objects.Values.ToList())
                {
                    instance.X = (byte)(level.Rooms[instance.Room].NumXSectors - instance.X - 1);

                    instance.Position = new Vector3(instance.X * 1024 + 512,
                        -instance.Y - level.Rooms[instance.Room].Position.Y * 256,
                        instance.Z * 1024 + 512);

                    switch (instance.Type)
                    {
                        case ObjectInstanceType.Moveable:
                            var moveable = (MoveableInstance)instance;
                            moveable.Model = level.Wad.Moveables[(uint)moveable.ObjectID];
                            level.Objects[instance.ID] = moveable;
                            break;
                        case ObjectInstanceType.StaticMesh:
                            var staticMesh = (StaticMeshInstance)instance;
                            staticMesh.Model = level.Wad.StaticMeshes[(uint)staticMesh.ObjectID];
                            level.Objects[instance.ID] = staticMesh;
                            break;
                        default:
                            level.Objects[instance.ID] = instance;
                            break;
                    }
                }

                var triggersToRemove = new List<int>();

                // Fix triggers
                form.ReportProgress(73, "Fixing triggers");
                foreach (var instance in level.Triggers.Values.ToList())
                {
                    if (instance.TargetType == TriggerTargetType.Object && !level.Objects.ContainsKey(instance.Target))
                    {
                        triggersToRemove.Add(instance.ID);
                        continue;
                    }

                    if (instance.X < 1)
                        instance.X = 1;
                    if (instance.X > level.Rooms[instance.Room].NumXSectors - 2)
                        instance.X = (byte)(level.Rooms[instance.Room].NumXSectors - 2);
                    if (instance.Z < 1)
                        instance.Z = 1;
                    if (instance.Z > level.Rooms[instance.Room].NumZSectors - 2)
                        instance.Z = (byte)(level.Rooms[instance.Room].NumZSectors - 2);

                    instance.X = (byte)(level.Rooms[instance.Room].NumXSectors - instance.X - instance.NumXBlocks);

                    for (int x = instance.X; x < instance.X + instance.NumXBlocks; x++)
                    {
                        for (int z = instance.Z; z < instance.Z + instance.NumZBlocks; z++)
                        {
                            level.Rooms[instance.Room].Blocks[x, z].Triggers.Add(instance.ID);
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
                        ((MoveableInstance)level.Objects[instance.Target]).ObjectID == 422)
                    {
                        instance.TargetType = TriggerTargetType.Target;
                    }

                    if (instance.TargetType == TriggerTargetType.Object &&
                        level.Objects[instance.Target].Type == ObjectInstanceType.Sink)
                    {
                        instance.TargetType = TriggerTargetType.Sink;
                    }

                    level.Triggers[instance.ID] = instance;
                }

                if (triggersToRemove.Count != 0)
                {
                    form.ReportProgress(75, "Found invalid triggers");
                    foreach (int trigger in triggersToRemove)
                    {
                        form.ReportProgress(75, "    Deleted trigger #" + trigger + " in room " +
                                                level.Triggers[trigger].Room);
                        level.Triggers.Remove(trigger);
                    }
                }

                // Fix portals
                form.ReportProgress(76, "Building portals");
                foreach (var currentPortal in level.Portals.Values.ToList())
                {
                    currentPortal.X = (byte)(level.Rooms[currentPortal.Room].NumXSectors - currentPortal.NumXBlocks -
                                              currentPortal.X);
                }

                foreach (var currentPortal in level.Portals.Values.ToList())
                {
                    foreach (var otherPortal in level.Portals.Values.ToList())
                    {
                        if (ReferenceEquals(currentPortal, otherPortal))
                            continue;

                        if (currentPortal.PrjOtherThingIndex != otherPortal.PrjThingIndex)
                            continue;

                        currentPortal.PrjAdjusted = true;
                        otherPortal.PrjAdjusted = true;

                        int idCurrentRoom = currentPortal.Room;

                        var currentRoom = level.Rooms[idCurrentRoom];
                        var otherRoom = level.Rooms[otherPortal.Room];

                        if (currentPortal.Direction == PortalDirection.North ||
                            currentPortal.Direction == PortalDirection.South ||
                            currentPortal.Direction == PortalDirection.East ||
                            currentPortal.Direction == PortalDirection.West)
                        {
                            for (int x = currentPortal.X; x < currentPortal.X + currentPortal.NumXBlocks; x++)
                            {
                                for (int z = currentPortal.Z; z < currentPortal.Z + currentPortal.NumZBlocks; z++)
                                {
                                    level.Rooms[idCurrentRoom].Blocks[x, z].WallPortal = currentPortal.ID;
                                }
                            }

                            for (int x = otherPortal.X; x < otherPortal.X + otherPortal.NumXBlocks; x++)
                            {
                                for (int z = otherPortal.Z; z < otherPortal.Z + otherPortal.NumZBlocks; z++)
                                {
                                    level.Rooms[otherPortal.Room].Blocks[x, z].WallPortal = otherPortal.ID;
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

                            int otherXmin = xMin + (int)(level.Rooms[idCurrentRoom].Position.X -
                                                          level.Rooms[otherPortal.Room].Position.X);
                            int otherXmax = xMax + (int)(level.Rooms[idCurrentRoom].Position.X -
                                                          level.Rooms[otherPortal.Room].Position.X);
                            int otherZmin = zMin + (int)(level.Rooms[idCurrentRoom].Position.Z -
                                                          level.Rooms[otherPortal.Room].Position.Z);
                            int otherZmax = zMax + (int)(level.Rooms[idCurrentRoom].Position.Z -
                                                          level.Rooms[otherPortal.Room].Position.Z);

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

                                        level.Rooms[currentPortal.Room].Blocks[x, z].FloorPortal = currentPortal.ID;

                                        int h1 = currentRoom.Blocks[x, z].QAFaces[0];
                                        int h2 = currentRoom.Blocks[x, z].QAFaces[1];
                                        int h3 = currentRoom.Blocks[x, z].QAFaces[2];
                                        int h4 = currentRoom.Blocks[x, z].QAFaces[3];

                                        int lh1 = otherRoom.Ceiling + otherRoom.Blocks[lowX, lowZ].WSFaces[0];
                                        int lh2 = otherRoom.Ceiling + otherRoom.Blocks[lowX, lowZ].WSFaces[1];
                                        int lh3 = otherRoom.Ceiling + otherRoom.Blocks[lowX, lowZ].WSFaces[2];
                                        int lh4 = otherRoom.Ceiling + otherRoom.Blocks[lowX, lowZ].WSFaces[3];

                                        bool defined;

                                        if (Room.IsQuad(x, z, h1, h2, h3, h4, true) && h1 == minHeight &&
                                            otherRoom.Blocks[lowX, lowZ].Type != BlockType.Wall && lh1 == maxHeight &&
                                            currentRoom.Blocks[x, z].Type != BlockType.Wall)
                                        {
                                            level.Rooms[currentPortal.Room].Blocks[x, z].IsFloorSolid = false;
                                            defined = true;
                                        }
                                        else
                                        {
                                            level.Rooms[idCurrentRoom].Blocks[x, z].IsFloorSolid = true;
                                            defined = false;
                                        }

                                        if (Room.IsQuad(x, z, lh1, lh2, lh3, lh4, true) && defined && lh1 == maxHeight)
                                        {
                                            level.Rooms[otherPortal.Room].Blocks[lowX, lowZ].IsCeilingSolid = false;
                                        }
                                        else
                                        {
                                            level.Rooms[otherPortal.Room].Blocks[lowX, lowZ].IsCeilingSolid = true;
                                        }
                                    }
                                    else
                                    {
                                        int minHeight = otherRoom.GetLowestCorner(otherXmin, otherZmin, otherXmax,
                                            otherZmax);
                                        int maxHeight = currentRoom.GetHighestCorner(xMin, zMin, xMax, zMax);

                                        level.Rooms[currentPortal.Room].Blocks[x, z].CeilingPortal = currentPortal.ID;

                                        int h1 = currentRoom.Ceiling + currentRoom.Blocks[x, z].WSFaces[0];
                                        int h2 = currentRoom.Ceiling + currentRoom.Blocks[x, z].WSFaces[1];
                                        int h3 = currentRoom.Ceiling + currentRoom.Blocks[x, z].WSFaces[2];
                                        int h4 = currentRoom.Ceiling + currentRoom.Blocks[x, z].WSFaces[3];

                                        int lh1 = otherRoom.Blocks[lowX, lowZ].QAFaces[0];
                                        int lh2 = otherRoom.Blocks[lowX, lowZ].QAFaces[1];
                                        int lh3 = otherRoom.Blocks[lowX, lowZ].QAFaces[2];
                                        int lh4 = otherRoom.Blocks[lowX, lowZ].QAFaces[3];

                                        bool defined;

                                        if (Room.IsQuad(x, z, h1, h2, h3, h4, true) && h1 == maxHeight &&
                                            otherRoom.Blocks[lowX, lowZ].Type != BlockType.Wall && lh1 == minHeight &&
                                            currentRoom.Blocks[x, z].Type != BlockType.Wall)
                                        {
                                            level.Rooms[idCurrentRoom].Blocks[x, z].IsCeilingSolid = false;
                                            defined = true;
                                        }
                                        else
                                        {
                                            level.Rooms[idCurrentRoom].Blocks[x, z].IsCeilingSolid = true;
                                            defined = false;
                                        }

                                        if (Room.IsQuad(x, z, lh1, lh2, lh3, lh4, true) && defined &&
                                            lh1 == minHeight /*&& otherRoom.Blocks[lowX, lowZ].Type != BlockType.Wall*/)
                                        {
                                            level.Rooms[otherPortal.Room].Blocks[lowX, lowZ].IsFloorSolid = false;
                                        }
                                        else
                                        {
                                            level.Rooms[otherPortal.Room].Blocks[lowX, lowZ].IsFloorSolid = true;
                                        }
                                    }
                                }
                            }
                        }

                        if ((!currentRoom.Flipped && !otherRoom.Flipped))
                        {
                            currentPortal.OtherID = otherPortal.ID;
                            otherPortal.OtherID = currentPortal.ID;
                            currentPortal.AdjoiningRoom = otherPortal.Room;
                            otherPortal.AdjoiningRoom = currentPortal.Room;
                        }
                        else if ((currentRoom.Flipped && otherRoom.Flipped))
                        {
                            currentPortal.OtherID = otherPortal.ID;
                            otherPortal.OtherID = currentPortal.ID;
                            currentPortal.AdjoiningRoom =
                                (otherRoom.BaseRoom != -1 ? otherRoom.BaseRoom : otherPortal.Room);
                            otherPortal.AdjoiningRoom =
                                (currentRoom.BaseRoom != -1 ? currentRoom.BaseRoom : currentPortal.Room);
                        }
                        else
                        {
                            if (!currentRoom.Flipped && otherRoom.Flipped)
                            {
                                if (otherRoom.AlternateRoom != -1)
                                {
                                    currentPortal.OtherID = otherPortal.ID;
                                    currentPortal.AdjoiningRoom = otherPortal.Room;
                                }
                                else
                                {
                                    currentPortal.AdjoiningRoom = otherRoom.BaseRoom;
                                }

                                otherPortal.OtherID = currentPortal.ID;
                                otherPortal.AdjoiningRoom = currentPortal.Room;
                            }
                            if (currentRoom.Flipped && !otherRoom.Flipped)
                            {
                                if (currentRoom.AlternateRoom != -1)
                                {
                                    otherPortal.OtherID = currentPortal.ID;
                                    otherPortal.AdjoiningRoom = currentPortal.Room;
                                }
                                else
                                {
                                    otherPortal.AdjoiningRoom = currentRoom.BaseRoom;
                                }

                                currentPortal.OtherID = otherPortal.ID;
                                currentPortal.AdjoiningRoom = otherPortal.Room;
                            }
                        }

                        level.Portals[currentPortal.ID] = currentPortal;
                        level.Portals[otherPortal.ID] = otherPortal;

                        break;
                    }
                }

                // Fix faces
                form.ReportProgress(85, "Building faces and geometry");
                for (int i = 0; i < level.Rooms.Length; i++)
                {
                    if (level.Rooms[i] == null)
                        continue;
                    var room = level.Rooms[i];

                    for (int j = 0; j < room.Lights.Count; j++)
                    {
                        var light = room.Lights[j];

                        light.X = (byte)(room.NumXSectors - light.X - 1);

                        light.Position = new Vector3(light.X * 1024 + 512,
                            -light.Y - level.Rooms[i].Position.Y * 256,
                            light.Z * 1024 + 512);
                        room.Lights[j] = light;
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
                                    prjFace._txtIndex = (short)(((prjFace._txtFlags & 0x03) << 8) + prjFace._txtIndex);

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
                                            level.TextureSamples[texture.ID] = texture;
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
                                            ID = level.TextureSamples.Count
                                        };
                                        //texture.UsageCount = 1;

                                        prjFace._newId = texture.ID;

                                        level.TextureSamples.Add(texture.ID, texture);
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
                                            uv[1] = new Vector2((xBlock * 256.0f + texture2.X + texture2.Width - 0.5f) / 2048.0f,
                                                (yBlock * 256.0f + texture2.Y + 0.5f) / 2048.0f);

                                            uv[2] = new Vector2((xBlock * 256.0f + texture2.X + texture2.Width - 0.5f) / 2048.0f,
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

                                                theBlock.Faces[faceIndex].TextureTriangle = TextureTileType.Rectangle;

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
            catch (Exception ex)
            {
                DarkUI.Forms.DarkMessageBox.ShowError(
                    "There was an error while importing the PRJ file. Message: " + ex.Message, "Error");
                return null;
            }

            foreach (var portal in level.Portals)
            {
                level.Rooms[portal.Value.Room].Portals.Add(portal.Key);
            }

            form.ReportProgress(95, "Building rooms");
            for (int i = 0; i < level.Rooms.Length; i++)
            {
                if (level.Rooms[i] == null)
                    continue;

                level.Rooms[i].BuildGeometry();
                level.Rooms[i].CalculateLightingForThisRoom();
                level.Rooms[i].UpdateBuffers();
            }

            form.ReportProgress(100, "Level loaded correctly!");

            GC.Collect();

            return level;
        }

        public static Level LoadFromPrj2(string filename, GraphicsDevice device, FormMain form)
        {
            Level level = new Level();

            try
            {
                var reader = new BinaryReaderEx(File.OpenRead(filename));

                // Check file version
                var buffer = reader.ReadBytes(4);
                if (buffer[0] == 0x50 && buffer[1] == 0x52 && buffer[2] == 0x4A && buffer[3] == 0x32)
                {
                    // PRJ2 senza compressione
                }
                else if (buffer[0] == 0x5A && buffer[1] == 0x52 && buffer[2] == 0x4A && buffer[3] == 0x32)
                {
                    // PRJ2 compresso
                    int uncompressedSize = reader.ReadInt32();
                    int compressedSize = reader.ReadInt32();
                    var projectData = reader.ReadBytes(compressedSize);
                    projectData = Utils.DecompressData(projectData);

                    var ms = new MemoryStream(projectData);
                    ms.Seek(0, SeekOrigin.Begin);
                    reader.Close();

                    reader = new BinaryReaderEx(ms);
                    reader.ReadInt32();
                }
                else
                {
                    reader.Close();
                    return null;
                }

                // Read version code (in the future it can be used to have multiple PRJ versions)
                int versionCode = reader.ReadInt32();

                // Read texture file
                int stringLength = reader.ReadInt32();
                level.TextureFile = System.Text.Encoding.UTF8.GetString(reader.ReadBytes(stringLength));

                // Check if texture file exists
                if (level.TextureFile == "" || !File.Exists(level.TextureFile))
                {
                    logger.Error("Can't find texture map!");
                    
                    if (DarkUI.Forms.DarkMessageBox.ShowWarning("The texture map '" + level.TextureFile + " could not be found. Do you want to browse it or cancel opening project?",
                                                                "Open project", DarkUI.Forms.DarkDialogButton.YesNo) != DialogResult.Yes)
                    {
                        logger.Error("PRJ2 loading canceled");
                        reader.Close();
                        return null;
                    }

                    // Ask for texture file
                    string textureFile = form.BrowseTextureMap();
                    if (textureFile == "")
                    {
                        logger.Error("PRJ2 loading canceled");
                        reader.Close();
                        return null;
                    }

                    level.TextureFile = textureFile;
                }

                //Read WAD file
                stringLength = reader.ReadInt32();
                level.WadFile = System.Text.Encoding.UTF8.GetString(reader.ReadBytes(stringLength));

                // Check if WAD file exists
                if (level.WadFile == "" || !File.Exists(level.WadFile))
                {
                    logger.Error("Can't find WAD!");

                    if (DarkUI.Forms.DarkMessageBox.ShowWarning("The WAD '" + level.WadFile + " could not be found. Do you want to browse it or cancel opening project?",
                                                                "Open project", DarkUI.Forms.DarkDialogButton.YesNo) != DialogResult.Yes)
                    {
                        logger.Error("PRJ2 loading canceled");
                        reader.Close();
                        return null;
                    }

                    // Ask for texture file
                    string wadFile = form.BrowseWAD();
                    if (wadFile == "")
                    {
                        logger.Error("PRJ2 loading canceled");
                        reader.Close();
                        return null;
                    }

                    level.WadFile = wadFile;
                }

                // Read fillers
                reader.ReadInt32();
                reader.ReadInt32();
                reader.ReadInt32();
                reader.ReadInt32();

                // Read textures
                int numTextures = reader.ReadInt32();
                for (int i = 0; i < numTextures; i++)
                {
                    var texture = new LevelTexture
                    {
                        ID = reader.ReadInt32(),
                        X = reader.ReadInt16(),
                        Y = reader.ReadInt16(),
                        Width = reader.ReadInt16(),
                        Height = reader.ReadInt16(),
                        Page = reader.ReadInt16()
                    };

                    /*texture.UsageCount =*/
                    reader.ReadInt32();
                    texture.Transparent = reader.ReadBoolean();
                    texture.DoubleSided = reader.ReadBoolean();

                    reader.ReadInt32();
                    reader.ReadInt32();
                    reader.ReadInt32();
                    reader.ReadInt32();
                    reader.ReadInt32();
                    reader.ReadInt32();
                    reader.ReadInt32();
                    reader.ReadInt32();

                    level.TextureSamples.Add(texture.ID, texture);
                }

                // Read portals
                int numPortals = reader.ReadInt32();
                for (int i = 0; i < numPortals; i++)
                {
                    var portal = new Portal(0, 0)
                    {
                        ID = reader.ReadInt32(),
                        OtherID = reader.ReadInt32(),
                        Room = reader.ReadInt16(),
                        AdjoiningRoom = reader.ReadInt16(),
                        Direction = (PortalDirection) reader.ReadByte(),
                        X = reader.ReadByte(),
                        Z = reader.ReadByte(),
                        NumXBlocks = reader.ReadByte(),
                        NumZBlocks = reader.ReadByte()
                    };

                    reader.ReadByte();
                    portal.MemberOfFlippedRoom = reader.ReadBoolean();
                    portal.Flipped = reader.ReadBoolean();
                    portal.OtherIDFlipped = reader.ReadInt32();

                    reader.ReadInt32();
                    reader.ReadInt32();
                    reader.ReadInt32();
                    reader.ReadInt32();

                    level.Portals.Add(portal.ID, portal);
                }

                // Read objects
                int numObjects = reader.ReadInt32();
                for (int i = 0; i < numObjects; i++)
                {
                    int objectId = reader.ReadInt32();
                    var objectType = (ObjectInstanceType) reader.ReadByte();

                    IObjectInstance o;

                    switch (objectType)
                    {
                        case ObjectInstanceType.Moveable:
                            o = new MoveableInstance(0, 0);
                            break;
                        case ObjectInstanceType.StaticMesh:
                            o = new StaticMeshInstance(0, 0);
                            break;
                        case ObjectInstanceType.Camera:
                            o = new CameraInstance(0, 0);
                            break;
                        case ObjectInstanceType.Sink:
                            o = new SinkInstance(0, 0);
                            break;
                        case ObjectInstanceType.Sound:
                            o = new SoundInstance(0, 0);
                            break;
                        case ObjectInstanceType.FlyByCamera:
                            o = new FlybyCameraInstance(0, 0);
                            break;
                        default:
                            reader.Close();
                            return null;
                    }

                    o.ID = objectId;
                    o.Type = objectType;
                    o.Position = new Vector3(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
                    o.Room = reader.ReadInt16();
                    o.OCB = reader.ReadInt16();
                    o.Rotation = reader.ReadInt16();
                    o.Invisible = reader.ReadBoolean();
                    o.ClearBody = reader.ReadBoolean();
                    o.Bits[0] = reader.ReadBoolean();
                    o.Bits[1] = reader.ReadBoolean();
                    o.Bits[2] = reader.ReadBoolean();
                    o.Bits[3] = reader.ReadBoolean();
                    o.Bits[4] = reader.ReadBoolean();

                    if (o.Type == ObjectInstanceType.StaticMesh)
                    {
                        ((StaticMeshInstance) o).ObjectID = reader.ReadInt32();
                        ((StaticMeshInstance) o).Color = Color.FromArgb(255, reader.ReadByte(), reader.ReadByte(),
                            reader.ReadByte());
                        reader.ReadBytes(1);
                    }

                    if (o.Type == ObjectInstanceType.Moveable)
                    {
                        ((MoveableInstance) o).ObjectID = reader.ReadInt32();
                        reader.ReadBytes(4);
                    }

                    if (o.Type == ObjectInstanceType.Camera)
                    {
                        ((CameraInstance) o).Fixed = reader.ReadBoolean();
                        reader.ReadBytes(7);
                    }

                    if (o.Type == ObjectInstanceType.Sink)
                    {
                        ((SinkInstance) o).Strength = reader.ReadInt16();
                        reader.ReadBytes(6);
                    }

                    if (o.Type == ObjectInstanceType.Sound)
                    {
                        ((SoundInstance) o).SoundID = reader.ReadInt16();
                        reader.ReadBytes(6);
                    }

                    /*if (o.Type == ObjectInstanceType.Fkl)
                    {
                        ((SoundInstance)o).SoundID = reader.ReadInt16();
                        reader.ReadBytes(6);
                    }*/

                    reader.ReadInt32();
                    reader.ReadInt32();
                    reader.ReadInt32();
                    reader.ReadInt32();

                    level.Objects.Add(o.ID, o);
                }

                reader.ReadInt32();
                reader.ReadInt32();
                reader.ReadInt32();
                reader.ReadInt32();

                // Read triggers
                int numTriggers = reader.ReadInt32();
                for (int i = 0; i < numTriggers; i++)
                {
                    var o = new TriggerInstance(0, 0)
                    {
                        ID = reader.ReadInt32(),
                        X = reader.ReadByte(),
                        Z = reader.ReadByte(),
                        NumXBlocks = reader.ReadByte(),
                        NumZBlocks = reader.ReadByte(),
                        TriggerType = (TriggerType) reader.ReadByte(),
                        TargetType = (TriggerTargetType) reader.ReadByte(),
                        Target = reader.ReadInt32(),
                        Timer = reader.ReadInt16(),
                        OneShot = reader.ReadBoolean(),
                        Bits =
                        {
                            [0] = reader.ReadBoolean(),
                            [1] = reader.ReadBoolean(),
                            [2] = reader.ReadBoolean(),
                            [3] = reader.ReadBoolean(),
                            [4] = reader.ReadBoolean()
                        },
                        Room = reader.ReadInt16()
                    };


                    reader.ReadInt16();
                    reader.ReadInt32();
                    reader.ReadInt32();
                    reader.ReadInt32();

                    level.Triggers.Add(o.ID, o);
                }

                // Read rooms
                int numRooms = reader.ReadInt32();
                for (int i = 0; i < numRooms; i++)
                {
                    string roomMagicWord = System.Text.Encoding.ASCII.GetString(reader.ReadBytes(4));

                    bool defined = reader.ReadBoolean();
                    if (!defined)
                    {
                        level.Rooms[i] = null;
                        continue;
                    }

                    var room = new Room(level)
                    {
                        Name = System.Text.Encoding.UTF8.GetString(reader.ReadBytes(100)).Trim(),
                        Position = new Vector3(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle()),
                        Ceiling = reader.ReadInt16(),
                        NumXSectors = reader.ReadByte(),
                        NumZSectors = reader.ReadByte()
                    };


                    room.Blocks = new Block[room.NumXSectors, room.NumZSectors];

                    for (int z = 0; z < room.NumZSectors; z++)
                    {
                        for (int x = 0; x < room.NumXSectors; x++)
                        {
                            var b = new Block(BlockType.Floor, BlockFlags.None, 0)
                            {
                                Type = (BlockType) reader.ReadByte(),
                                Flags = (BlockFlags) reader.ReadInt16(),
                                QAFaces =
                                {
                                    [0] = reader.ReadInt16(),
                                    [1] = reader.ReadInt16(),
                                    [2] = reader.ReadInt16(),
                                    [3] = reader.ReadInt16()
                                },
                                EDFaces =
                                {
                                    [0] = reader.ReadInt16(),
                                    [1] = reader.ReadInt16(),
                                    [2] = reader.ReadInt16(),
                                    [3] = reader.ReadInt16()
                                },
                                WSFaces =
                                {
                                    [0] = reader.ReadInt16(),
                                    [1] = reader.ReadInt16(),
                                    [2] = reader.ReadInt16(),
                                    [3] = reader.ReadInt16()
                                },
                                RFFaces =
                                {
                                    [0] = reader.ReadInt16(),
                                    [1] = reader.ReadInt16(),
                                    [2] = reader.ReadInt16(),
                                    [3] = reader.ReadInt16()
                                },
                                SplitFoorType = reader.ReadByte(),
                                SplitFloor = reader.ReadBoolean(),
                                SplitCeilingType = reader.ReadByte(),
                                SplitCeiling = reader.ReadBoolean(),
                                RealSplitFloor = reader.ReadByte(),
                                RealSplitCeiling = reader.ReadByte(),
                                Climb =
                                {
                                    [0] = reader.ReadBoolean(),
                                    [1] = reader.ReadBoolean(),
                                    [2] = reader.ReadBoolean(),
                                    [3] = reader.ReadBoolean()
                                },
                                FloorOpacity = (PortalOpacity) reader.ReadByte(),
                                CeilingOpacity = (PortalOpacity) reader.ReadByte(),
                                WallOpacity = (PortalOpacity) reader.ReadByte(),
                                FloorPortal = reader.ReadInt32(),
                                CeilingPortal = reader.ReadInt32(),
                                WallPortal = reader.ReadInt32(),
                                IsFloorSolid = reader.ReadBoolean(),
                                IsCeilingSolid = reader.ReadBoolean(),
                                NoCollisionFloor = reader.ReadBoolean(),
                                NoCollisionCeiling = reader.ReadBoolean()
                            };

                            foreach (var f in b.Faces)
                            {
                                f.Defined = reader.ReadBoolean();
                                f.Flipped = reader.ReadBoolean();
                                f.Texture = reader.ReadInt16();
                                f.Rotation = reader.ReadByte();
                                f.Transparent = reader.ReadBoolean();
                                f.DoubleSided = reader.ReadBoolean();
                                f.Invisible = reader.ReadBoolean();
                                f.NoCollision = reader.ReadBoolean();
                                f.TextureTriangle = (TextureTileType) reader.ReadByte();

                                for (int n = 0; n < 4; n++)
                                    f.RectangleUV[n] = new Vector2(reader.ReadSingle(), reader.ReadSingle());
                                for (int n = 0; n < 3; n++)
                                    f.TriangleUV[n] = new Vector2(reader.ReadSingle(), reader.ReadSingle());
                                for (int n = 0; n < 3; n++)
                                    f.TriangleUV2[n] = new Vector2(reader.ReadSingle(), reader.ReadSingle());

                                reader.ReadInt32();
                                reader.ReadInt32();
                                reader.ReadInt32();
                                reader.ReadInt32();
                            }

                            b.FloorDiagonalSplit = (DiagonalSplit) reader.ReadByte();
                            b.FloorDiagonalSplitType = (DiagonalSplitType) reader.ReadByte();
                            b.CeilingDiagonalSplit = (DiagonalSplit) reader.ReadByte();
                            b.CeilingDiagonalSplitType = (DiagonalSplitType) reader.ReadByte();

                            reader.ReadInt32();
                            reader.ReadInt32();
                            reader.ReadInt32();
                            reader.ReadInt32();

                            room.Blocks[x, z] = b;
                        }
                    }

                    int numLights = reader.ReadInt32();
                    for (int j = 0; j < numLights; j++)
                    {
                        var l = new Light
                        {
                            Type = (LightType) reader.ReadByte(),
                            Position = new Vector3(reader.ReadSingle(), reader.ReadSingle(),
                                reader.ReadSingle()),
                            Intensity = reader.ReadSingle(),
                            Color = Color.FromArgb(255, reader.ReadByte(), reader.ReadByte(), reader.ReadByte()),
                            In = reader.ReadSingle(),
                            Out = reader.ReadSingle(),
                            Len = reader.ReadSingle(),
                            Cutoff = reader.ReadSingle(),
                            DirectionX = reader.ReadSingle(),
                            DirectionY = reader.ReadSingle(),
                            Face = (BlockFaces) reader.ReadByte()
                        };


                        reader.ReadByte();
                        reader.ReadInt16();

                        room.Lights.Add(l);
                    }

                    room.AmbientLight = Color.FromArgb(255, reader.ReadByte(), reader.ReadByte(), reader.ReadByte());
                    room.Flipped = reader.ReadBoolean();
                    room.AlternateRoom = reader.ReadInt16();
                    room.AlternateGroup = reader.ReadInt16();
                    room.WaterLevel = reader.ReadInt16();
                    room.MistLevel = reader.ReadInt16();
                    room.ReflectionLevel = reader.ReadInt16();
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
                    room.Flipped = reader.ReadBoolean();
                    room.BaseRoom = reader.ReadInt16();
                    room.ExcludeFromPathFinding = reader.ReadBoolean();

                    reader.ReadInt32();
                    reader.ReadInt32();
                    reader.ReadInt32();

                    level.Rooms[i] = room;
                }

                int numAnimatedSets = reader.ReadInt32();
                for (int i = 0; i < numAnimatedSets; i++)
                {
                    var effect = (AnimatexTextureSetEffect) reader.ReadByte();
                    var aSet = new AnimatedTextureSet {Effect = effect};
                    int numAnimatedTextures = reader.ReadInt32();
                    for (int j = 0; j < numAnimatedTextures; j++)
                    {
                        short texturePage = reader.ReadInt16();
                        short textureX = reader.ReadInt16();
                        short textureY = reader.ReadInt16();

                        aSet.Textures.Add(new AnimatedTexture(textureX, textureY, texturePage));
                    }

                    level.AnimatedTextures.Add(aSet);
                }

                int numTextureSounds = reader.ReadInt32();
                for (int i = 0; i < numTextureSounds; i++)
                {
                    var txtSound = new TextureSound(reader.ReadInt16(), reader.ReadInt16(), reader.ReadInt16())
                    {
                        Sound = (TextureSounds) reader.ReadByte()
                    };

                    level.TextureSounds.Add(txtSound);
                }

                int numPaletteColors = reader.ReadInt32();
                Editor.Instance.Palette.Clear();
                for (int i = 0; i < numPaletteColors; i++)
                {
                    Editor.Instance.Palette.Add(Color.FromArgb(255, reader.ReadByte(), reader.ReadByte(),
                        reader.ReadByte()));
                }

                reader.ReadInt32();
                reader.ReadInt32();
                reader.ReadInt32();
                reader.ReadInt32();
                reader.ReadInt32();
                reader.ReadInt32();
                reader.ReadInt32();
                reader.ReadInt32();
            }
            catch (Exception)
            {
                return null;
            }

            // Now it's time to load texturs
            level.LoadTextureMap(level.TextureFile, device);

            // Now it's time to load WAD
            level.LoadWad(level.WadFile, device);

            // Now fill the structures loaded from PRJ2 
            for (int i = 0; i < level.Triggers.Count; i++)
            {
                var trigger = level.Triggers[i];

                for (int x = trigger.X; x < trigger.X + trigger.NumXBlocks; x++)
                {
                    for (int z = trigger.Z; z < trigger.Z + trigger.NumZBlocks; z++)
                    {
                        level.Rooms[trigger.Room].Blocks[x, z].Triggers.Add(trigger.ID);
                    }
                }
            }

            foreach (var obj in level.Objects.Values)
            {
                var objectType = obj.Type;
                short roomIndex = obj.Room;
                int objectId = obj.ID;

                switch (objectType)
                {
                    case ObjectInstanceType.Moveable:
                        level.Rooms[roomIndex].Moveables.Add(objectId);
                        break;
                    case ObjectInstanceType.StaticMesh:
                        level.Rooms[roomIndex].StaticMeshes.Add(objectId);
                        break;
                    case ObjectInstanceType.Camera:
                        level.Rooms[roomIndex].Cameras.Add(objectId);
                        break;
                    case ObjectInstanceType.Sink:
                        level.Rooms[roomIndex].Sinks.Add(objectId);
                        break;
                    case ObjectInstanceType.Sound:
                        level.Rooms[roomIndex].SoundSources.Add(objectId);
                        break;
                    case ObjectInstanceType.FlyByCamera:
                        level.Rooms[roomIndex].FlyByCameras.Add(objectId);
                        break;
                }

                if (objectType == ObjectInstanceType.Moveable)
                {
                    uint oid = (uint)((MoveableInstance)obj).ObjectID;
                    ((MoveableInstance)obj).Model = level.Wad.Moveables[oid];
                }

                else if (objectType == ObjectInstanceType.StaticMesh)
                {
                    uint oid = (uint)((StaticMeshInstance)obj).ObjectID;
                    ((StaticMeshInstance)obj).Model = level.Wad.StaticMeshes[oid];
                }
            }

            // Now build the real geometry and update DirectX buffers
            foreach (var room in level.Rooms.Where(room => room != null))
            {
                room.InitializeVerticesGrid();
                room.BuildGeometry();
                room.CalculateLightingForThisRoom();
                room.UpdateBuffers();
            }

            foreach (var portal in level.Portals)
            {
                level.Rooms[portal.Value.Room].Portals.Add(portal.Key);
            }

            level.FileName = filename;

            return level;
        }

        public static bool SaveToPrj2(string filename, Level level)
        {
            const byte filler8 = 0;
            const short filler16 = 0;
            const int filler32 = 0;

            try
            {
                var ms = new MemoryStream();
                byte[] projectData;
                using (var writer = new BinaryWriterEx(ms))
                {
                    // Write version
                    var version = new byte[] {0x50, 0x52, 0x4A, 0x32};
                    writer.Write(version);

                    const int versionCode = 2;
                    writer.Write(versionCode);

                    string textureFileName = Utils.GetRelativePath(filename, level.TextureFile);
                    string wadFileName = Utils.GetRelativePath(filename, level.WadFile);

                    // Write texture map
                    var textureFile = System.Text.Encoding.UTF8.GetBytes(textureFileName);
                    int numBytes = textureFile.Length;
                    writer.Write(numBytes);
                    writer.Write(textureFile);

                    var wadFile = System.Text.Encoding.UTF8.GetBytes(wadFileName);
                    numBytes = wadFile.Length;
                    writer.Write(numBytes);
                    writer.Write(wadFile);

                    writer.Write(filler32);
                    writer.Write(filler32);
                    writer.Write(filler32);
                    writer.Write(filler32);

                    // Write textures
                    int numTextures = level.TextureSamples.Count;
                    writer.Write(numTextures);
                    foreach (var txt in level.TextureSamples.Values)
                    {
                        writer.Write(txt.ID);
                        writer.Write(txt.X);
                        writer.Write(txt.Y);
                        writer.Write(txt.Width);
                        writer.Write(txt.Height);
                        writer.Write(txt.Page);
                        writer.Write(filler32);
                        writer.Write(txt.Transparent);
                        writer.Write(txt.DoubleSided);
                        writer.Write(filler32);
                        writer.Write(filler32);
                        writer.Write(filler32);
                        writer.Write(filler32);
                        writer.Write(filler32);
                        writer.Write(filler32);
                        writer.Write(filler32);
                        writer.Write(filler32);
                    }

                    // Write portals
                    int numPortals = level.Portals.Count;
                    writer.Write(numPortals);
                    foreach (var p in level.Portals.Values)
                    {
                        writer.Write(p.ID);
                        writer.Write(p.OtherID);
                        writer.Write(p.Room);
                        writer.Write(p.AdjoiningRoom);
                        writer.Write((byte) p.Direction);
                        writer.Write(p.X);
                        writer.Write(p.Z);
                        writer.Write(p.NumXBlocks);
                        writer.Write(p.NumZBlocks);
                        writer.Write(filler8);
                        writer.Write(p.MemberOfFlippedRoom);
                        writer.Write(p.Flipped);
                        writer.Write(p.OtherIDFlipped);
                        writer.Write(filler32);
                        writer.Write(filler32);
                        writer.Write(filler32);
                        writer.Write(filler32);
                    }

                    // Write objects: moveables, static meshes, cameras, sinks, sound sources
                    int numObjects = level.Objects.Count;
                    writer.Write(numObjects);
                    foreach (var o in level.Objects.Values)
                    {
                        writer.Write(o.ID);
                        writer.Write((byte) o.Type);
                        writer.Write(o.Position.X);
                        writer.Write(o.Position.Y);
                        writer.Write(o.Position.Z);
                        writer.Write(o.Room);
                        writer.Write(o.OCB);
                        writer.Write(o.Rotation);
                        writer.Write(o.Invisible);
                        writer.Write(o.ClearBody);
                        writer.Write(o.Bits[0]);
                        writer.Write(o.Bits[1]);
                        writer.Write(o.Bits[2]);
                        writer.Write(o.Bits[3]);
                        writer.Write(o.Bits[4]);

                        switch (o.Type)
                        {
                            case ObjectInstanceType.StaticMesh:
                                var sm = (StaticMeshInstance) o;
                                writer.Write(sm.Model.ObjectID);
                                writer.Write(sm.Color.R);
                                writer.Write(sm.Color.G);
                                writer.Write(sm.Color.B);

                                writer.Write(filler8);
                                break;
                            case ObjectInstanceType.Moveable:
                                var m = (MoveableInstance) o;
                                writer.Write(m.Model?.ObjectID ?? 0);

                                writer.Write(filler32);
                                break;
                            case ObjectInstanceType.Camera:
                                var c = (CameraInstance) o;
                                writer.Write(c.Fixed);

                                writer.Write(filler8);
                                writer.Write(filler8);
                                writer.Write(filler8);
                                writer.Write(filler32);
                                break;
                            case ObjectInstanceType.Sink:
                            {
                                var s = (SinkInstance) o;
                                writer.Write(s.Strength);

                                writer.Write(filler8);
                                writer.Write(filler8);
                                writer.Write(filler32);
                            }
                                break;
                            case ObjectInstanceType.Sound:
                            {
                                var s = (SoundInstance) o;
                                writer.Write(s.SoundID);

                                writer.Write(filler8);
                                writer.Write(filler8);
                                writer.Write(filler32);
                            }
                                break;
                        }

                        writer.Write(filler32);
                        writer.Write(filler32);
                        writer.Write(filler32);
                        writer.Write(filler32);
                    }

                    writer.Write(filler32);
                    writer.Write(filler32);
                    writer.Write(filler32);
                    writer.Write(filler32);

                    // Write triggers
                    int numTriggers = level.Triggers.Count;
                    writer.Write(numTriggers);
                    foreach (var o in level.Triggers.Values)
                    {
                        writer.Write(o.ID);
                        writer.Write(o.X);
                        writer.Write(o.Z);
                        writer.Write(o.NumXBlocks);
                        writer.Write(o.NumZBlocks);
                        writer.Write((byte) o.TriggerType);
                        writer.Write((byte) o.TargetType);
                        writer.Write(o.Target);
                        writer.Write(o.Timer);
                        writer.Write(o.OneShot);
                        writer.Write(o.Bits[0]);
                        writer.Write(o.Bits[1]);
                        writer.Write(o.Bits[2]);
                        writer.Write(o.Bits[3]);
                        writer.Write(o.Bits[4]);
                        writer.Write(o.Room);

                        writer.Write(filler16);
                        writer.Write(filler32);
                        writer.Write(filler32);
                        writer.Write(filler32);
                    }

                    // Write rooms
                    const int numRooms = 255;
                    writer.Write(numRooms);
                    for (int i = 0; i < numRooms; i++)
                    {
                        var roomMagicWord = System.Text.Encoding.ASCII.GetBytes("ROOM");
                        writer.Write(roomMagicWord);

                        var r = level.Rooms[i];

                        if (r == null)
                        {
                            writer.Write(false);
                            continue;
                        }
                        else
                        {
                            writer.Write(true);
                        }

                        if (r.Name == null)
                            r.Name = "Room " + i.ToString();

                        writer.Write(System.Text.Encoding.UTF8.GetBytes(r.Name.PadRight(100, ' ')));
                        writer.Write(r.Position.X);
                        writer.Write(r.Position.Y);
                        writer.Write(r.Position.Z);
                        writer.Write(r.Ceiling);
                        writer.Write(r.NumXSectors);
                        writer.Write(r.NumZSectors);

                        for (int z = 0; z < r.NumZSectors; z++)
                        {
                            for (int x = 0; x < r.NumXSectors; x++)
                            {
                                var b = r.Blocks[x, z];

                                writer.Write((byte) b.Type);
                                writer.Write((short) b.Flags);
                                for (int n = 0; n < 4; n++)
                                    writer.Write(b.QAFaces[n]);
                                for (int n = 0; n < 4; n++)
                                    writer.Write(b.EDFaces[n]);
                                for (int n = 0; n < 4; n++)
                                    writer.Write(b.WSFaces[n]);
                                for (int n = 0; n < 4; n++)
                                    writer.Write(b.RFFaces[n]);
                                writer.Write(b.SplitFoorType);
                                writer.Write(b.SplitFloor);
                                writer.Write(b.SplitCeilingType);
                                writer.Write(b.SplitCeiling);
                                writer.Write(b.RealSplitFloor);
                                writer.Write(b.RealSplitCeiling);
                                for (int n = 0; n < 4; n++)
                                    writer.Write(b.Climb[n]);
                                writer.Write((byte) b.FloorOpacity);
                                writer.Write((byte) b.CeilingOpacity);
                                writer.Write((byte) b.WallOpacity);
                                writer.Write(b.FloorPortal);
                                writer.Write(b.CeilingPortal);
                                writer.Write(b.WallPortal);
                                writer.Write(b.IsFloorSolid);
                                writer.Write(b.IsCeilingSolid);
                                writer.Write(b.NoCollisionFloor);
                                writer.Write(b.NoCollisionCeiling);

                                foreach (var f in b.Faces)
                                {
                                    writer.Write(f.Defined);
                                    writer.Write(f.Flipped);
                                    writer.Write(f.Texture);
                                    writer.Write(f.Rotation);
                                    writer.Write(f.Transparent);
                                    writer.Write(f.DoubleSided);
                                    writer.Write(f.Invisible);
                                    writer.Write(f.NoCollision);
                                    writer.Write((byte) f.TextureTriangle);
                                    for (int n = 0; n < 4; n++)
                                        writer.Write(f.RectangleUV[n]);
                                    for (int n = 0; n < 3; n++)
                                        writer.Write(f.TriangleUV[n]);
                                    for (int n = 0; n < 3; n++)
                                        writer.Write(f.TriangleUV2[n]);
                                    writer.Write(filler32);
                                    writer.Write(filler32);
                                    writer.Write(filler32);
                                    writer.Write(filler32);
                                }

                                writer.Write((byte) b.FloorDiagonalSplit);
                                writer.Write((byte) b.FloorDiagonalSplitType);
                                writer.Write((byte) b.CeilingDiagonalSplit);
                                writer.Write((byte) b.CeilingDiagonalSplitType);

                                writer.Write(filler32);
                                writer.Write(filler32);
                                writer.Write(filler32);
                                writer.Write(filler32);
                            }
                        }

                        int numLights = r.Lights.Count;
                        writer.Write(numLights);
                        foreach (var l in r.Lights)
                        {
                            writer.Write((byte) l.Type);
                            writer.Write(l.Position.X);
                            writer.Write(l.Position.Y);
                            writer.Write(l.Position.Z);
                            writer.Write(l.Intensity);
                            writer.Write(l.Color.R);
                            writer.Write(l.Color.G);
                            writer.Write(l.Color.B);
                            writer.Write(l.In);
                            writer.Write(l.Out);
                            writer.Write(l.Len);
                            writer.Write(l.Cutoff);
                            writer.Write(l.DirectionX);
                            writer.Write(l.DirectionY);

                            byte b = (byte) l.Face;
                            writer.Write(b);

                            writer.Write(filler8);
                            writer.Write(filler8);
                            writer.Write(filler8);
                        }

                        writer.Write((byte) r.AmbientLight.R);
                        writer.Write((byte) r.AmbientLight.G);
                        writer.Write((byte) r.AmbientLight.B);
                        writer.Write(r.Flipped);
                        writer.Write(r.AlternateRoom);
                        writer.Write(r.AlternateGroup);
                        writer.Write(r.WaterLevel);
                        writer.Write(r.MistLevel);
                        writer.Write(r.ReflectionLevel);
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
                        writer.Write(r.Flipped);
                        writer.Write(r.BaseRoom);
                        writer.Write(r.ExcludeFromPathFinding);

                        writer.Write(filler32);
                        writer.Write(filler32);
                        writer.Write(filler32);
                    }

                    // Write animated textures
                    int numAnimatedTextures = level.AnimatedTextures.Count;
                    writer.Write(numAnimatedTextures);
                    foreach (var textureSet in level.AnimatedTextures)
                    {
                        writer.Write((byte) textureSet.Effect);

                        int numTexturesInSet = textureSet.Textures.Count;
                        writer.Write(numTexturesInSet);

                        foreach (var texture in textureSet.Textures)
                        {
                            writer.Write(texture.Page);
                            writer.Write(texture.X);
                            writer.Write(texture.Y);
                        }
                    }

                    int numTextureSounds = level.TextureSounds.Count;
                    writer.Write(numTextureSounds);
                    foreach (var sound in level.TextureSounds)
                    {
                        writer.Write(sound.X);
                        writer.Write(sound.Y);
                        writer.Write(sound.Page);
                        writer.Write((byte) sound.Sound);
                    }

                    int numPalette = Editor.Instance.Palette.Count;
                    writer.Write(numPalette);
                    for (int i = 0; i < numPalette; i++)
                    {
                        writer.Write(Editor.Instance.Palette[i].R);
                        writer.Write(Editor.Instance.Palette[i].G);
                        writer.Write(Editor.Instance.Palette[i].B);
                    }

                    writer.Write(filler32);
                    writer.Write(filler32);
                    writer.Write(filler32);
                    writer.Write(filler32);
                    writer.Write(filler32);
                    writer.Write(filler32);
                    writer.Write(filler32);
                    writer.Write(filler32);
                    writer.Write(filler32);
                    writer.Write(filler32);
                    writer.Write(filler32);
                    writer.Write(filler32);
                    writer.Write(filler32);
                    writer.Write(filler32);
                    writer.Write(filler32);
                    writer.Write(filler32);

                    writer.Flush();

                    projectData = ms.ToArray();
                }

                using (var writer = new BinaryWriterEx(File.OpenWrite(filename)))
                {
                    writer.Write(projectData);
                    writer.Flush();
                }
            }
            catch (Exception ex)
            {
                return false;
            }

            level.FileName = filename;

            return true;
        }

        public void DeleteObject(int instance)
        {
            var triggersToDelete = new List<int>();

            // First I build a list of triggers to delete
            foreach (var trigger in Triggers.Values)
            {
                if ((trigger.TargetType == TriggerTargetType.Camera ||
                     trigger.TargetType == TriggerTargetType.FlyByCamera ||
                     trigger.TargetType == TriggerTargetType.Object || trigger.TargetType == TriggerTargetType.Sink) &&
                    trigger.Target == instance)
                {
                    triggersToDelete.Add(trigger.ID);
                }
            }

            // Then I clean sectors and delete triggers
            foreach (int t in triggersToDelete)
            {
                var trigger = Triggers[t];

                // Clean sectors
                for (int z = trigger.Z; z < trigger.Z + trigger.NumZBlocks; z++)
                {
                    for (int x = trigger.X; x < trigger.X + trigger.NumXBlocks; x++)
                    {
                        Rooms[trigger.Room].Blocks[x, z].Triggers.Remove(trigger.ID);
                    }
                }

                // Delete trigger
                Triggers.Remove(t);
            }
        }
    }
}
