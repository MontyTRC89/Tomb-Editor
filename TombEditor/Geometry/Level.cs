using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.IO;
using System.Drawing.Drawing2D;
using SharpDX.Toolkit.Graphics;
using TombLib.Wad;
using TombLib.IO;
using TombEditor.Geometry;
using SharpDX;
using Color = System.Drawing.Color;
using TombEditor.Controls;
using System.ComponentModel;
using System.Windows.Forms;
using System.Diagnostics;

namespace TombEditor.Geometry
{
    public class Level
    {
        private struct prj_slot
        {
            public bool present;
            public int objectID;
            public string name;
            public short slotType;
        }

        private struct prj_flip_info
        {
            public short baseRoom;
            public short flipRoom;
            public short group;
        }

        private struct prj_face
        {
            public short txtType;
            public short txtIndex;
            public byte txtFlags;
            public byte txtRotation;
            public byte txtTriangle;
            public int NewID;
            public bool IsFlipped;
        }

        private struct prj_block
        {
            public short blockType;
            public short blockFlags1;
            public short blockYfloor;
            public short blockYceiling;
            public sbyte[] qaFaces;
            public sbyte[] wsFaces;
            public sbyte[] edFaces;
            public sbyte[] rfFaces;
            public prj_face[] faces;
            public short Flags2;
            public short Flags3;
        }

        private struct prj_tex_info
        {
            public byte X;
            public short Y;
            public byte Width;
            public byte Height;
        }

        public Room[] Rooms; //Rooms in level
        public Dictionary<int, LevelTexture> TextureSamples { get; set; } //Texture tiles
        public Dictionary<int, Texture2D> Textures { get; set; } //DirectX textures... For now just one texture atlas 2048x2048 pixel
        public Bitmap TextureMap { get; set; } //The texture map in PNG format
        public Dictionary<int, Portal> Portals { get; set; }
        public Dictionary<int, TriggerInstance> Triggers { get; set; }
        public Dictionary<int, IObjectInstance> Objects { get; set; } //Objects (moveables, static meshes, sinks, camera, fly-by cameras, sound sources)
        public List<AnimatedTextureSet> AnimatedTextures { get; set; }
        public List<TextureSound> TextureSounds { get; set; }
        public Wad Wad { get; set; }
        public string TextureFile { get; set; }
        public string WadFile { get; set; }
        public bool MustSave { get; set; } // Used for Save and Save as logic
        public string FileName { get; set; }
        private Editor _editor;

        public Level()
        {
            TextureSamples = new Dictionary<int, LevelTexture>();
            Textures = new Dictionary<int, Texture2D>();
            Portals = new Dictionary<int, Geometry.Portal>();
            Triggers = new Dictionary<int, Geometry.TriggerInstance>();
            Objects = new Dictionary<int, IObjectInstance>();
            AnimatedTextures = new List<AnimatedTextureSet>();
            TextureSounds = new List<TextureSound>();

            Rooms = new Room[512];

            _editor = Editor.Instance;
        }

        public int AddTexture(short x, short y, short w, short h)
        {
            short newX = (short)x;
            short newY = (short)y;

            // Step 1: check if there's another texture already in the list
            for (int i = 0; i < TextureSamples.Count; i++)
            {
                LevelTexture texture = TextureSamples.ElementAt(i).Value;
                if (texture.X == newX && (texture.Y + 256 * texture.Page) == newY && texture.Width == w && texture.Height == h
                    && texture.DoubleSided == _editor.DoubleSided && texture.Transparent == _editor.Transparent)
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
            short page = (short)Math.Floor(y / 256.0f);
            LevelTexture newTexture = new LevelTexture();
            newTexture.X = newX;
            newTexture.Y = (short)(newY - page * 256);
            newTexture.Width = w;
            newTexture.Height = h;
            newTexture.Page = page;
            newTexture.ID = id;
            newTexture.Transparent = _editor.Transparent;
            newTexture.DoubleSided = _editor.DoubleSided;

            TextureSamples.Add(id, newTexture);

            return id;
        }

        public void DisposeLevel()
        {
            // First clean the old data
            if (Textures != null)
            {
                for (int i = 0; i < Textures.Count; i++)
                {
                    // Dispose DirectX texture and release GPU memory
                    Textures.ElementAt(i).Value.Dispose();
                }
            }

            if (TextureMap != null)
                TextureMap.Dispose();
            TextureMap = null;

            Textures = new Dictionary<int, Texture2D>();

            if (Wad != null)
                Wad.DisposeWad();

            GC.Collect();
        }

        public void LoadTextureMap(string filename)
        {
            Debug.Log("Loading texture map", DebugType.Warning);

            Stopwatch watch = new Stopwatch();
            watch.Start();

            // Load texture map as a bitmap
            Bitmap bmp = (Bitmap)Bitmap.FromFile(filename);

            // Calculate the number of pages
            int numPages = (int)Math.Floor(bmp.Height / 256.0f);
            if (bmp.Height % 256 != 0)
                numPages++;

            int currentXblock = 0;
            int currentYblock = 0;

            Debug.Log("Building 2048x2048 texture atlas for DirectX");

            // Copy the page in a temp bitmap. I generate a texture atlas, putting all texture pages inside 2048x2048 pixel 
            // textures.
            Bitmap tempBitmap = new Bitmap(2048, 2048, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

            Graphics g = Graphics.FromImage(tempBitmap);

            for (int i = 0; i < numPages; i++)
            {
                System.Drawing.RectangleF src = new System.Drawing.RectangleF(0, 256 * i, 256, 256);
                System.Drawing.RectangleF dest = new System.Drawing.RectangleF(currentXblock * 256, currentYblock * 256, 256, 256);

                g.DrawImage(bmp, dest, src, GraphicsUnit.Pixel);

                currentXblock++;
                if (currentXblock == 8)
                {
                    currentXblock = 0;
                    currentYblock++;
                }
            }

            // Create DirectX texture
            MemoryStream outputTexture = new MemoryStream();
            tempBitmap.Save(outputTexture, System.Drawing.Imaging.ImageFormat.Png);
            outputTexture.Seek(0, SeekOrigin.Begin);
            Texture2D newTexture = TombLib.Graphics.TextureLoad.FromStream(_editor.GraphicsDevice, outputTexture);

            if (TextureMap != null)
            {
                Debug.Log("Cleaning memory used by a previous texture map");

                TextureMap.Dispose();
                TextureMap = null;
                Textures[0].Dispose();
                Textures.Remove(0);
            }

            // Add texture to the dictionary
            Textures.Add(0, newTexture);

            // Clean used memory
            outputTexture.Close();
            tempBitmap.Dispose();

            TextureFile = filename;
            TextureMap = bmp;

            watch.Stop();

            Debug.Log("Texture map loaded", DebugType.Success);
            Debug.Log("    Elapsed time: " + watch.ElapsedMilliseconds + " ms");
        }

        public void LoadWad(string filename)
        {
            // Load the WAD
            Wad = Wad.LoadWad(filename);
            WadFile = filename;

            Wad.GraphicsDevice = _editor.GraphicsDevice;
            Wad.PrepareDataForDirectX();

            // Prepare vertex buffers and index buffers
            for (int i = 0; i < Wad.Moveables.Count; i++)
            {
                // Build the mesh tree
                Wad.Moveables.ElementAt(i).Value.BuildHierarchy();

                // If moveable has animations, then build the animation pose of first frame of animation 0
                if (Wad.Moveables.ElementAt(i).Value.Animations.Count > 0 && Wad.Moveables.ElementAt(i).Value.Animations[0].KeyFrames.Count > 0)
                    Wad.Moveables.ElementAt(i).Value.BuildAnimationPose(Wad.Moveables.ElementAt(i).Value.Animations[0].KeyFrames[0]);

                Wad.Moveables.ElementAt(i).Value.BuildBuffers();
            }

            for (int i = 0; i < Wad.StaticMeshes.Count; i++)
            {
                Wad.StaticMeshes.ElementAt(i).Value.BuildBuffers();
            }
        }

        public int GetNewPortalID()
        {
            int i = 0;

            while (true)
            {
                if (!Portals.ContainsKey(i))
                    return i;
                i++;
            }
        }

        public int GetNewTriggerID()
        {
            int i = 0;

            while (true)
            {
                if (!Triggers.ContainsKey(i))
                    return i;
                i++;
            }
        }

        public int GetNewObjectID()
        {
            int i = 0;

            while (true)
            {
                if (!Objects.ContainsKey(i))
                    return i;
                i++;
            }
        }

        public static Level LoadFromPrj(string filename, FormImportPRJ form)
        {
            GC.Collect();

            Level level = new Level();

            try
            {
                // Open file
                BinaryReaderEx reader = new BinaryReaderEx(File.OpenRead(filename));

                form.ReportProgress(0, "Begin of PRJ import");

                Debug.Log("Opening Winroomedit PRJ file", DebugType.Warning);

                // Check if it's a NGLE PRJ
                bool ngle = false;
                reader.BaseStream.Seek(reader.BaseStream.Length - 8, SeekOrigin.Begin);
                byte[] bytesNGLE = reader.ReadBytes(4);
                if (bytesNGLE[0] == 0x4E && bytesNGLE[1] == 0x47 && bytesNGLE[2] == 0x4C && bytesNGLE[3] == 0x45)
                {
                    form.ReportProgress(1, "This is a NGLE project");
                    Debug.Log("NGLE Project");
                    ngle = true;
                }

                reader.BaseStream.Seek(0, SeekOrigin.Begin);

                // Version
                reader.ReadBytes(12);

                // Number of rooms
                int numRooms = reader.ReadInt32();
                Debug.Log("Number of rooms: " + numRooms);

                // Now read the first info about rooms, at the end of the PRJ there will be another block
                level.Rooms = new Room[Editor.MaxNumberOfRooms];

                Dictionary<int, prj_block[,]> tempRooms = new Dictionary<int, prj_block[,]>();
                Dictionary<int, int> flippedRooms = new Dictionary<int, int>();
                List<prj_flip_info> flipInfos = new List<Geometry.Level.prj_flip_info>();
                List<List<int>> portalRef = new List<List<int>>();

                form.ReportProgress(2, "Number of rooms: " + numRooms);
                double progress = 2;

                for (int i = 0; i < numRooms; i++)
                {
                    // Room is defined?
                    short defined = reader.ReadInt16();
                    if (defined == 0x01)
                        continue;

                    // Read room's name
                    string roomName = System.Text.UTF8Encoding.ASCII.GetString(reader.ReadBytes(80));

                    Debug.Log("Room #" + i, DebugType.Warning);
                    Debug.Log("    Name: " + roomName);

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

                    Room room = new Room(level, posXBlocks, (int)(yPos / 256.0f), posZBlocks, (byte)numXBlocks, (byte)numZBlocks, 0);

                    short numPortals = reader.ReadInt16();
                    short[] portalThings = new short[numPortals];

                    for (int j = 0; j < numPortals; j++)
                    {
                        portalThings[j] = reader.ReadInt16();
                    }

                    List<int> tmpPortals = new List<int>();

                    Debug.Log("    Portals: " + numPortals);

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

                        byte[] portalBuffer = reader.ReadBytes(26);

                        Portal p = new Portal(level.GetNewPortalID(), portalRoom);

                        p.X = (byte)portalX;
                        p.Z = (byte)portalZ;
                        p.NumXBlocks = (byte)portalXBlocks;
                        p.NumZBlocks = (byte)portalZBlocks;

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

                    portalRef.Add(tmpPortals);

                    short numObjects = reader.ReadInt16();
                    short[] objectsThings = new short[numObjects];

                    for (int j = 0; j < numObjects; j++)
                    {
                        objectsThings[j] = reader.ReadInt16();
                    }

                    Debug.Log("    Objects and Triggers: " + numObjects);

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
                        short objOCB = reader.ReadInt16();
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
                                MoveableInstance instance = new MoveableInstance(objectsThings[j], objRoom);

                                instance.Bits[0] = (objOCB & 0x0002) != 0;
                                instance.Bits[1] = (objOCB & 0x0004) != 0;
                                instance.Bits[2] = (objOCB & 0x0008) != 0;
                                instance.Bits[3] = (objOCB & 0x0010) != 0;
                                instance.Bits[4] = (objOCB & 0x0020) != 0;
                                instance.Invisible = (objOCB & 0x0001) != 0;
                                instance.ClearBody = (objOCB & 0x0080) != 0;
                                instance.ObjectID = objSlot;
                                instance.X = (byte)(objPosX);
                                instance.Z = (byte)(objPosZ);
                                instance.Y = (short)objLongY;
                                instance.OCB = objTimer;

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

                                instance.Bits[0] = (objOCB & 0x0002) != 0;
                                instance.Bits[1] = (objOCB & 0x0004) != 0;
                                instance.Bits[2] = (objOCB & 0x0008) != 0;
                                instance.Bits[3] = (objOCB & 0x0010) != 0;
                                instance.Bits[4] = (objOCB & 0x0020) != 0;
                                instance.Invisible = (objOCB & 0x0001) != 0;
                                instance.ClearBody = (objOCB & 0x0080) != 0;
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

                            TriggerInstance trigger = new TriggerInstance(level.GetNewTriggerID(), currentRoom);

                            trigger.X = (byte)objPosX;
                            trigger.Z = (byte)objPosZ;
                            trigger.NumXBlocks = (byte)objSizeX;
                            trigger.NumZBlocks = (byte)objSizeZ;
                            trigger.Target = triggerItemNumber;

                            if (triggerType == 0)
                                trigger.TriggerType = TriggerType.Trigger;
                            if (triggerType == 1)
                                trigger.TriggerType = TriggerType.Pad;
                            if (triggerType == 2)
                                trigger.TriggerType = TriggerType.Switch;
                            if (triggerType == 3)
                                trigger.TriggerType = TriggerType.Key;
                            if (triggerType == 4)
                                trigger.TriggerType = TriggerType.Pickup;
                            if (triggerType == 5)
                                trigger.TriggerType = TriggerType.Heavy;
                            if (triggerType == 6)
                                trigger.TriggerType = TriggerType.Antipad;
                            if (triggerType == 7)
                                trigger.TriggerType = TriggerType.Combat;
                            if (triggerType == 8)
                                trigger.TriggerType = TriggerType.Dummy;
                            if (triggerType == 9)
                                trigger.TriggerType = TriggerType.Antitrigger;
                            if (triggerType == 10)
                                trigger.TriggerType = TriggerType.HeavySwitch;
                            if (triggerType == 11)
                                trigger.TriggerType = TriggerType.HeavyAntritrigger;
                            if (triggerType == 12)
                                trigger.TriggerType = TriggerType.Monkey;

                            trigger.Bits[4] = (triggerFlags & 0x0002) == 0;
                            trigger.Bits[3] = (triggerFlags & 0x0004) == 0;
                            trigger.Bits[2] = (triggerFlags & 0x0008) == 0;
                            trigger.Bits[1] = (triggerFlags & 0x0010) == 0;
                            trigger.Bits[0] = (triggerFlags & 0x0020) == 0;
                            trigger.OneShot = (triggerFlags & 0x0001) != 0;

                            trigger.Timer = triggerTimere;

                            if (triggerItemType == 0)
                                trigger.TargetType = TriggerTargetType.Object;
                            if (triggerItemType == 3)
                                trigger.TargetType = TriggerTargetType.FlipMap;
                            if (triggerItemType == 4)
                                trigger.TargetType = TriggerTargetType.FlipOn;
                            if (triggerItemType == 5)
                                trigger.TargetType = TriggerTargetType.FlipOff;
                            if (triggerItemType == 6)
                                trigger.TargetType = TriggerTargetType.Target;
                            if (triggerItemType == 7)
                                trigger.TargetType = TriggerTargetType.FinishLevel;
                            if (triggerItemType == 8)
                                trigger.TargetType = TriggerTargetType.PlayAudio;
                            if (triggerItemType == 9)
                                trigger.TargetType = TriggerTargetType.FlipEffect;
                            if (triggerItemType == 10)
                                trigger.TargetType = TriggerTargetType.Secret;
                            if (triggerItemType == 12)
                                trigger.TargetType = TriggerTargetType.FlyByCamera;
                            if (triggerItemType == 13)
                                trigger.TargetType = TriggerTargetType.CutsceneOrParameterNG;
                            if (triggerItemType == 14)
                                trigger.TargetType = TriggerTargetType.FMV;

                            level.Triggers.Add(trigger.ID, trigger);
                        }
                    }

                    room.AmbientLight = Color.FromArgb(255, reader.ReadByte(), reader.ReadByte(), reader.ReadByte());
                    reader.ReadByte();

                    short numObjects2 = reader.ReadInt16();
                    short[] objectsThings2 = new short[numObjects2];

                    Debug.Log("    Lights and other objects: " + numObjects2);

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
                        short objOCB = reader.ReadInt16();

                        if (objectType == 0x4000 || objectType == 0x6000 || objectType == 0x4200 ||
                            objectType == 0x5000 || objectType == 0x4100 || objectType == 0x4020)
                        {
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

                            Light light = new Light();
                            light.Color = Color.FromArgb(255, lightR, lightG, lightB);
                            light.Cutoff = lightCut;
                            light.Len = lightLen;
                            light.DirectionX = 360.0f - lightX;
                            light.DirectionY = lightY + 90.0f;
                            if (light.DirectionY >= 360)
                                light.DirectionY = light.DirectionY - 360.0f;
                            light.Active = (lightOn == 0x01);
                            light.In = lightIn;
                            light.Out = lightOut;
                            light.Intensity = lightIntensity / 8192.0f;

                            light.X = (byte)(objPosX);
                            light.Z = (byte)(objPosZ);
                            light.Y = (short)objLongY;

                            if (objectType == 0x4000)
                            {
                                light.Type = LightType.Light;
                                light.In /= 1024.0f;
                                light.Out /= 1024.0f;
                            }

                            if (objectType == 0x6000)
                            {
                                light.Type = LightType.Shadow;
                                light.In /= 1024.0f;
                                light.Out /= 1024.0f;
                            }

                            if (objectType == 0x4200)
                                light.Type = LightType.Sun;

                            if (objectType == 0x5000)
                            {
                                light.In = 512.0f;
                                light.Out = 1536.0f;
                                light.Type = LightType.Effect;
                            }

                            if (objectType == 0x4100)
                            {
                                light.Type = LightType.Spot;
                                light.Len /= 1024.0f;
                                light.Cutoff /= 1024.0f;
                            }

                            if (objectType == 0x4020)
                            {
                                light.In /= 1024.0f;
                                light.Out /= 1024.0f;
                                light.Type = LightType.FogBulb;
                            }

                            room.Lights.Add(light);
                        }
                        else if (objectType == 0x4c00)
                        {
                            SoundInstance sound = new SoundInstance(objectsThings2[j], objRoom);

                            sound.SoundID = objSlot;
                            sound.X = (byte)(objPosX);
                            sound.Z = (byte)(objPosZ);
                            sound.Y = (short)objLongY;

                            level.Objects.Add(sound.ID, sound);
                        }
                        else if (objectType == 0x4400)
                        {
                            SinkInstance sink = new SinkInstance(objectsThings2[j], objRoom);

                            sink.X = (byte)(objPosX);
                            sink.Z = (byte)(objPosZ);
                            sink.Y = (short)objLongY;

                            sink.Strength = (short)(objTimer / 2);

                            level.Objects.Add(sink.ID, sink);
                            room.Sinks.Add(sink.ID);
                        }
                        else if (objectType == 0x4800 || objectType == 0x4080)
                        {
                            CameraInstance camera = new CameraInstance(objectsThings2[j], objRoom);

                            camera.Timer = objTimer;
                            camera.Fixed = (objectType == 0x4080);

                            camera.X = (byte)(objPosX);
                            camera.Z = (byte)(objPosZ);
                            camera.Y = (short)objLongY;

                            level.Objects.Add(camera.ID, camera);
                            room.Cameras.Add(camera.ID);
                        }
                        else if (objectType == 0x4040)
                        {
                            FlybyCameraInstance camera = new FlybyCameraInstance(objectsThings2[j], objRoom);

                            camera.Timer = objTimer;

                            camera.Sequence = (short)((objSlot & 0xe000) >> 13);
                            camera.Number = (short)((objSlot & 0x1f00) >> 8);
                            camera.FOV = (short)((objSlot & 0x00ff));
                            camera.Roll = objRoll;
                            camera.Speed = (short)(objSpeed / 655);

                            camera.X = (byte)(objPosX);
                            camera.Z = (byte)(objPosZ);
                            camera.Y = (short)objLongY;

                            camera.DirectionX = (short)(-objUnk);
                            camera.DirectionY = (short)(objFacing + 90);
                            if (camera.DirectionY >= 360)
                                camera.DirectionY = (short)(camera.DirectionY - 360);

                            camera.Flags[0] = ((objOCB & 0x01) != 0);
                            camera.Flags[1] = ((objOCB & 0x02) != 0);
                            camera.Flags[2] = ((objOCB & 0x04) != 0);
                            camera.Flags[3] = ((objOCB & 0x08) != 0);
                            camera.Flags[4] = ((objOCB & 0x10) != 0);
                            camera.Flags[5] = ((objOCB & 0x20) != 0);
                            camera.Flags[6] = ((objOCB & 0x40) != 0);
                            camera.Flags[7] = ((objOCB & 0x80) != 0);
                            camera.Flags[8] = ((objOCB & 0x0100) != 0);
                            camera.Flags[9] = ((objOCB & 0x0200) != 0);
                            camera.Flags[10] = ((objOCB & 0x0400) != 0);
                            camera.Flags[11] = ((objOCB & 0x0800) != 0);
                            camera.Flags[12] = ((objOCB & 0x1000) != 0);
                            camera.Flags[13] = ((objOCB & 0x2000) != 0);
                            camera.Flags[14] = ((objOCB & 0x4000) != 0);
                            camera.Flags[15] = ((objOCB & 0x8000) != 0);

                            level.Objects.Add(camera.ID, camera);
                            room.FlyByCameras.Add(camera.ID);
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
                        prj_flip_info info = new Geometry.Level.prj_flip_info();

                        info.baseRoom = (short)i;
                        info.flipRoom = flipRoom;
                        info.group = (short)(flags2 & 0xff);

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

                    prj_block[,] tempBlocks = new prj_block[numXBlocks, numZBlocks];

                    for (int z = 0; z < room.NumZSectors; z++)
                    {
                        for (int x = 0; x < room.NumXSectors; x++)
                        {
                            prj_block b = new Geometry.Level.prj_block();

                            b.blockType = reader.ReadInt16();
                            b.blockFlags1 = reader.ReadInt16();
                            b.blockYfloor = reader.ReadInt16();
                            b.blockYceiling = reader.ReadInt16();

                            b.qaFaces = new sbyte[4];
                            for (int k = 0; k < 4; k++)
                                b.qaFaces[k] = reader.ReadSByte();

                            b.wsFaces = new sbyte[4];
                            for (int k = 0; k < 4; k++)
                                b.wsFaces[k] = reader.ReadSByte();

                            b.edFaces = new sbyte[4];
                            for (int k = 0; k < 4; k++)
                                b.edFaces[k] = reader.ReadSByte();

                            b.rfFaces = new sbyte[4];
                            for (int k = 0; k < 4; k++)
                                b.rfFaces[k] = reader.ReadSByte();

                            b.faces = new prj_face[14];

                            for (int j = 0; j < 14; j++)
                            {
                                prj_face fc = new prj_face();

                                fc.txtType = reader.ReadInt16();
                                fc.txtIndex = reader.ReadByte();
                                fc.txtFlags = reader.ReadByte();
                                fc.txtRotation = reader.ReadByte();
                                fc.txtTriangle = reader.ReadByte();
                                reader.ReadInt16();

                                b.faces[j] = (fc);
                            }

                            b.Flags2 = reader.ReadInt16();
                            b.Flags3 = reader.ReadInt16();

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
                            prj_block b = tempBlocks[x, z];

                            if (b.blockYfloor < lowest)
                                lowest = b.blockYfloor;
                            if (b.blockYceiling > highest)
                                highest = b.blockYceiling;
                        }
                    }

                    room.Position = new SharpDX.Vector3(-room.Position.X, lowest, room.Position.Z);

                    sbyte deltaCeilingMain = (sbyte)(lowest + 20);

                    for (int z = 0; z < room.NumZSectors; z++)
                    {
                        for (int x = 0; x < room.NumXSectors; x++)
                        {
                            prj_block b = tempBlocks[room.NumXSectors - 1 - x, z];

                            sbyte deltaFloor = (sbyte)(b.blockYfloor - lowest);
                            sbyte deltaCeiling = (sbyte)(deltaCeilingMain - b.blockYceiling);

                            for (int j = 0; j < 4; j++)
                                b.qaFaces[j] += deltaFloor;
                            for (int j = 0; j < 4; j++)
                                b.edFaces[j] += deltaFloor;
                            for (int j = 0; j < 4; j++)
                                b.wsFaces[j] -= deltaCeiling;
                            for (int j = 0; j < 4; j++)
                                b.rfFaces[j] -= deltaCeiling;

                            BlockType typ = BlockType.Floor;
                            if (b.blockType == 0x01)
                                typ = BlockType.Floor;
                            if (b.blockType == 0x1e)
                                typ = BlockType.BorderWall;
                            if (b.blockType == 0x0e)
                                typ = BlockType.Wall;
                            if (b.blockType == 0x06)
                                typ = BlockType.BorderWall; // BlockType.WallPortal;
                            if (b.blockType == 0x03)
                                typ = BlockType.Floor; // BlockType.FloorPortal;
                            if (b.blockType == 0x05)
                                typ = BlockType.Floor; // BlockType.CeilingPortal;
                            if (b.blockType == 0x07)
                                typ = BlockType.Floor; // BlockType.FloorPortal;

                            room.Blocks[x, z] = new Block(level, room, typ, BlockFlags.None, 20);

                            room.Blocks[x, z].QAFaces[0] = b.qaFaces[3];
                            room.Blocks[x, z].QAFaces[1] = b.qaFaces[0];
                            room.Blocks[x, z].QAFaces[2] = b.qaFaces[1];
                            room.Blocks[x, z].QAFaces[3] = b.qaFaces[2];

                            room.Blocks[x, z].EDFaces[0] = b.edFaces[3];
                            room.Blocks[x, z].EDFaces[1] = b.edFaces[0];
                            room.Blocks[x, z].EDFaces[2] = b.edFaces[1];
                            room.Blocks[x, z].EDFaces[3] = b.edFaces[2];

                            room.Ceiling = 20;

                            room.Blocks[x, z].WSFaces[0] = (sbyte)(b.wsFaces[0]);
                            room.Blocks[x, z].WSFaces[1] = (sbyte)(b.wsFaces[3]);
                            room.Blocks[x, z].WSFaces[2] = (sbyte)(b.wsFaces[2]);
                            room.Blocks[x, z].WSFaces[3] = (sbyte)(b.wsFaces[1]);

                            room.Blocks[x, z].RFFaces[0] = (sbyte)(b.rfFaces[0]);
                            room.Blocks[x, z].RFFaces[1] = (sbyte)(b.rfFaces[3]);
                            room.Blocks[x, z].RFFaces[2] = (sbyte)(b.rfFaces[2]);
                            room.Blocks[x, z].RFFaces[3] = (sbyte)(b.rfFaces[1]);

                            room.Blocks[x, z].SplitFoorType = (byte)b.Flags3;

                            if ((b.blockFlags1 & 0x4000) != 0)
                                room.Blocks[x, z].Flags |= BlockFlags.Monkey;
                            if ((b.blockFlags1 & 0x0020) != 0)
                                room.Blocks[x, z].Flags |= BlockFlags.Box;
                            if ((b.blockFlags1 & 0x0010) != 0)
                                room.Blocks[x, z].Flags |= BlockFlags.Death;
                            if ((b.blockFlags1 & 0x0200) != 0)
                                room.Blocks[x, z].Climb[2] = true;
                            if ((b.blockFlags1 & 0x0100) != 0)
                                room.Blocks[x, z].Climb[1] = true;
                            if ((b.blockFlags1 & 0x0080) != 0)
                                room.Blocks[x, z].Climb[0] = true;
                            if ((b.blockFlags1 & 0x0040) != 0)
                                room.Blocks[x, z].Climb[3] = true;

                            if ((x == 0 || z == 0 || x == room.NumXSectors - 1 || z == room.NumZSectors - 1))
                            {
                                if ((b.blockFlags1 & 0x0008) == 0x0008 && (b.blockFlags1 & 0x1000) == 0)
                                    room.Blocks[x, z].WallOpacity = PortalOpacity.Opacity1;
                                if ((b.blockFlags1 & 0x0008) == 0x0008 && (b.blockFlags1 & 0x1000) == 0x1000)
                                    room.Blocks[x, z].WallOpacity = PortalOpacity.Opacity2;
                            }
                            else
                            {
                                if ((b.blockFlags1 & 0x0002) == 0x0002)
                                    room.Blocks[x, z].FloorOpacity = PortalOpacity.Opacity1;

                                if ((b.blockFlags1 & 0x0004) == 0x0004)
                                    room.Blocks[x, z].CeilingOpacity = PortalOpacity.Opacity1;

                                if ((b.blockFlags1 & 0x0800) == 0x0800)
                                    room.Blocks[x, z].FloorOpacity = PortalOpacity.Opacity2;

                                if ((b.blockFlags1 & 0x0400) == 0x0400)
                                    room.Blocks[x, z].CeilingOpacity = PortalOpacity.Opacity2;
                            }
                        }
                    }

                    // Fix lights
                    for (int j = 0; j < room.Lights.Count; j++)
                    {
                        room.Lights[j].Position = new SharpDX.Vector3((room.NumXSectors - 1) * 1024.0f - room.Lights[j].Position.X + 512.0f,
                                                                      room.Lights[j].Position.Y,
                                                                      room.Lights[j].Position.Z + 512.0f);
                    }

                    level.Rooms[i] = room;

                    progress += (i / (float)numRooms * 0.28f);

                    form.ReportProgress((int)progress, "");
                }

                form.ReportProgress(30, "Rooms loaded");

                Debug.Log("All rooms loaded", DebugType.Success);

                // Read unused things indices
                //      byte[] bufIndices=reader.ReadBytes(13136);

                int dwNumThings = reader.ReadInt32();  // number of things in the map
                int dwMaxThings = reader.ReadInt32();  // always 2000
                reader.ReadBytes(dwMaxThings * 4);

                int dwNumLights = reader.ReadInt32();  // number of lights in the map
                reader.ReadBytes(768 * 4);

                int dwNumTriggers = reader.ReadInt32();    // number of triggers in the map
                reader.ReadBytes(512 * 4);



                // Read TGA string
                byte[] stringBuffer = new byte[255];
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

                string tgaName = System.Text.UTF8Encoding.ASCII.GetString(stringBuffer);
                tgaName = tgaName.Replace('\0', ' ').Trim();

                Debug.Log("Texture map: " + tgaName);

                if (tgaName == "" || !File.Exists(tgaName))
                {
                    Debug.Log("Can't find texture map!", DebugType.Error);

                    if (DarkUI.Forms.DarkMessageBox.ShowWarning("The TGA file '" + tgaName + " could not be found. Do you want to browse it or cancel importing?",
                                                                "Open project", DarkUI.Forms.DarkDialogButton.YesNo) != DialogResult.Yes)
                    {
                        Debug.Log("PRJ import canceled", DebugType.Error);
                        reader.Close();
                        return null;
                    }

                    // Ask for TGA file
                    tgaName = form.OpenTGA();
                    if (tgaName == "")
                    {
                        Debug.Log("PRJ import canceled", DebugType.Error);
                        reader.Close();
                        return null;
                    }
                }

                string pngName = "";
                if (!Utils.ConvertTGAtoPNG(tgaName, out pngName))
                    return null;

                level.LoadTextureMap(pngName);

                form.ReportProgress(50, "Converted '" + tgaName + "' to PNG format");

                // Read textures
                int numTextures = reader.ReadInt32();

                form.ReportProgress(52, "Loading textures");
                form.ReportProgress(52, "    Number of textures: " + numTextures);

                List<prj_tex_info> tempTextures = new List<prj_tex_info>();
                for (int t = 0; t < numTextures; t++)
                {
                    prj_tex_info tmpTxt = new prj_tex_info();

                    tmpTxt.X = reader.ReadByte();
                    tmpTxt.Y = reader.ReadInt16();
                    reader.ReadInt16();
                    tmpTxt.Width = reader.ReadByte();
                    reader.ReadByte();
                    tmpTxt.Height = reader.ReadByte();

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

                string wadName = System.Text.UTF8Encoding.ASCII.GetString(stringBuffer);
                wadName = wadName.Replace('\0', ' ').Trim();

                if (wadName == "" || !File.Exists(wadName))
                {
                    if (DarkUI.Forms.DarkMessageBox.ShowWarning("The WAD file '" + wadName + " could not be found. Do you want to browse it or cancel importing?",
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

                level.LoadWad(wadPath + "\\" + wadBase + ".wad");

                form.ReportProgress(60, "WAD loaded");

                int numSlots = reader.ReadInt32();
                List<prj_slot> slots = new List<Geometry.Level.prj_slot>();

                StreamWriter writerSlots = new StreamWriter(File.OpenWrite("slots.txt"));

                for (int i = 0; i < numSlots; i++)
                {
                    prj_slot slot = new Geometry.Level.prj_slot();

                    short slotType = reader.ReadInt16();
                    if (slotType == 0x00)
                    {
                        slots.Add(slot);

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

                    string slotName = System.Text.UTF8Encoding.ASCII.GetString(stringBuffer);
                    slotName = slotName.Replace('\0', ' ').Trim();

                    int objectId = reader.ReadInt32();

                    reader.ReadBytes(108);

                    slot.objectID = objectId;
                    slot.name = slotName;
                    slot.slotType = slotType;
                    slot.present = true;

                    writerSlots.WriteLine(i + "\t" + slotName + "\t" + slotType + "\t" + objectId);

                    slots.Add(slot);
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

                    if (defined == 1)
                    {
                        AnimatedTextureSet aSet = new AnimatedTextureSet();

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
                }

                for (int i = 0; i < 256; i++)
                {
                    int relative = i % 16;

                    int txtY = (int)Math.Floor(relative / 4.0f);
                    int txtX = relative - 4 * txtY;

                    txtX *= 64;
                    txtY *= 64;

                    int tile = (int)Math.Floor(i / 16.0f);

                    TextureSound txtSound = new TextureSound((short)txtX, (short)txtY, (short)tile);
                    txtSound.Sound = (TextureSounds)(reader.ReadByte());

                    level.TextureSounds.Add(txtSound);
                }

                // Fix rooms coordinates (in TRLE reference system is messed up...)
                form.ReportProgress(65, "Flipping reference system");

                int minX = 1024;
                for (int i = 0; i < level.Rooms.Length; i++)
                {
                    if (level.Rooms[i] == null)
                        continue;
                    if (level.Rooms[i].Position.X < minX)
                        minX = (int)level.Rooms[i].Position.X;
                }

                for (int i = 0; i < level.Rooms.Length; i++)
                {
                    if (level.Rooms[i] == null)
                        continue;
                    level.Rooms[i].Position = new SharpDX.Vector3(level.Rooms[i].Position.X - minX - level.Rooms[i].NumXSectors + 10,
                                                                  level.Rooms[i].Position.Y,
                                                                  level.Rooms[i].Position.Z);
                }

                form.ReportProgress(67, "Building flipped rooms table");

                for (int i = 0; i < level.Rooms.Length; i++)
                {
                    if (level.Rooms[i] == null)
                        continue;

                    for (int j = 0; j < flipInfos.Count; j++)
                    {
                        prj_flip_info info = flipInfos[j];

                        if (info.baseRoom == i)
                        {
                            level.Rooms[i].Flipped = true;
                            level.Rooms[i].AlternateRoom = info.flipRoom;
                            level.Rooms[i].AlternateGroup = info.group;
                        }

                        if (info.flipRoom == i)
                        {
                            level.Rooms[i].Flipped = true;
                            level.Rooms[i].BaseRoom = info.baseRoom;
                            level.Rooms[i].AlternateGroup = info.group;
                            level.Rooms[i].Position = new Vector3(level.Rooms[info.baseRoom].Position.X,
                                                                  level.Rooms[info.baseRoom].Position.Y,
                                                                  level.Rooms[info.baseRoom].Position.Z);
                        }
                    }
                }

                // Fix objects
                form.ReportProgress(70, "Fixing objects positions and data");
                for (int i = 0; i < level.Objects.Count; i++)
                {
                    IObjectInstance instance = level.Objects.ElementAt(i).Value;

                    //if (instance.X < 1) instance.X = 1;
                    //if (instance.X > level.Rooms[instance.Room].NumXSectors - 2) instance.X = (byte)(level.Rooms[instance.Room].NumXSectors - 2);
                    //if (instance.Z < 1) instance.Z = 1;
                    //if (instance.Z > level.Rooms[instance.Room].NumZSectors - 2) instance.Z = (byte)(level.Rooms[instance.Room].NumZSectors - 2);

                    instance.X = (byte)(level.Rooms[instance.Room].NumXSectors - instance.X - 1);

                    instance.Position = new Vector3(instance.X * 1024 + 512,
                                                  -instance.Y - level.Rooms[instance.Room].Position.Y * 256,
                                                  instance.Z * 1024 + 512);

                    if (instance.Type == ObjectInstanceType.Moveable)
                    {
                        MoveableInstance moveable = (MoveableInstance)instance;
                        moveable.Model = level.Wad.Moveables[(uint)moveable.ObjectID];
                        level.Objects[instance.ID] = moveable;
                    }
                    else if (instance.Type == ObjectInstanceType.StaticMesh)
                    {
                        StaticMeshInstance staticMesh = (StaticMeshInstance)instance;
                        staticMesh.Model = level.Wad.StaticMeshes[(uint)staticMesh.ObjectID];
                        level.Objects[instance.ID] = staticMesh;
                    }
                    else
                    {
                        level.Objects[instance.ID] = instance;
                    }
                }

                List<int> triggersToRemove = new List<int>();

                // Fix triggers
                form.ReportProgress(73, "Fixing triggers");
                for (int i = 0; i < level.Triggers.Count; i++)
                {
                    TriggerInstance instance = level.Triggers.ElementAt(i).Value;

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

                    if (instance.TargetType == TriggerTargetType.Object && level.Objects[instance.Target].Type == ObjectInstanceType.FlyByCamera)
                    {
                        instance.TargetType = TriggerTargetType.FlyByCamera;
                        instance.Target = ((FlybyCameraInstance)level.Objects[instance.Target]).Sequence;
                    }

                    if (instance.TargetType == TriggerTargetType.Object && level.Objects[instance.Target].Type == ObjectInstanceType.Camera)
                    {
                        instance.TargetType = TriggerTargetType.Camera;
                    }

                    if (instance.TargetType == TriggerTargetType.Object &&
                        level.Objects[instance.Target].Type == ObjectInstanceType.Moveable &&
                        ((MoveableInstance)level.Objects[instance.Target]).ObjectID == 422)
                    {
                        instance.TargetType = TriggerTargetType.Target;
                    }

                    if (instance.TargetType == TriggerTargetType.Object && level.Objects[instance.Target].Type == ObjectInstanceType.Sink)
                    {
                        instance.TargetType = TriggerTargetType.Sink;
                    }

                    level.Triggers[instance.ID] = instance;
                }

                if (triggersToRemove.Count != 0)
                {
                    form.ReportProgress(75, "Found invalid triggers");
                    for (int i = 0; i < triggersToRemove.Count; i++)
                    {
                        form.ReportProgress(75, "    Deleted trigger #" + triggersToRemove[i] + " in room " +
                                            level.Triggers[triggersToRemove[i]].Room);
                        level.Triggers.Remove(triggersToRemove[i]);
                    }
                }

                // Fix portals
                form.ReportProgress(76, "Building portals");
                for (int i = 0; i < level.Portals.Count; i++)
                {
                    Portal currentPortal = level.Portals.ElementAt(i).Value;

                    currentPortal.X = (byte)(level.Rooms[currentPortal.Room].NumXSectors - currentPortal.NumXBlocks - currentPortal.X);
                    level.Portals[i] = currentPortal;
                }

                for (int i = 0; i < level.Portals.Count; i++)
                {
                    Portal currentPortal = level.Portals.ElementAt(i).Value;

                    for (int j = 0; j < level.Portals.Count; j++)
                    {
                        if (i == j)
                            continue;

                        Portal otherPortal = level.Portals.ElementAt(j).Value;

                        if (currentPortal.PrjOtherThingIndex == otherPortal.PrjThingIndex)
                        {
                            currentPortal.PrjAdjusted = true;
                            otherPortal.PrjAdjusted = true;

                            int idCurrentRoom = currentPortal.Room;

                            Room currentRoom = level.Rooms[idCurrentRoom];
                            Room otherRoom = level.Rooms[otherPortal.Room];

                            if (currentPortal.Direction == PortalDirection.North || currentPortal.Direction == PortalDirection.South ||
                                currentPortal.Direction == PortalDirection.East || currentPortal.Direction == PortalDirection.West)
                            {
                                for (int x = currentPortal.X; x < currentPortal.X + currentPortal.NumXBlocks; x++)
                                {
                                    for (int z = currentPortal.Z; z < currentPortal.Z + currentPortal.NumZBlocks; z++)
                                    {
                                        //level.Rooms[idCurrentRoom].Blocks[x, z].Type = BlockType.WallPortal;
                                        level.Rooms[idCurrentRoom].Blocks[x, z].WallPortal = currentPortal.ID;
                                    }
                                }

                                for (int x = otherPortal.X; x < otherPortal.X + otherPortal.NumXBlocks; x++)
                                {
                                    for (int z = otherPortal.Z; z < otherPortal.Z + otherPortal.NumZBlocks; z++)
                                    {
                                        // level.Rooms[otherPortal.Room].Blocks[x, z].Type = BlockType.WallPortal;
                                        level.Rooms[otherPortal.Room].Blocks[x, z].WallPortal = otherPortal.ID;
                                    }
                                }
                            }

                            if (currentPortal.Direction == PortalDirection.Floor || currentPortal.Direction == PortalDirection.Ceiling)
                            {
                                int xMin = currentPortal.X;
                                int xMax = currentPortal.X + currentPortal.NumXBlocks;
                                int zMin = currentPortal.Z;
                                int zMax = currentPortal.Z + currentPortal.NumZBlocks;

                                int otherXmin = xMin + (int)(level.Rooms[idCurrentRoom].Position.X - level.Rooms[otherPortal.Room].Position.X);
                                int otherXmax = xMax + (int)(level.Rooms[idCurrentRoom].Position.X - level.Rooms[otherPortal.Room].Position.X);
                                int otherZmin = zMin + (int)(level.Rooms[idCurrentRoom].Position.Z - level.Rooms[otherPortal.Room].Position.Z);
                                int otherZmax = zMax + (int)(level.Rooms[idCurrentRoom].Position.Z - level.Rooms[otherPortal.Room].Position.Z);

                                for (int x = xMin; x < xMax; x++)
                                {
                                    for (int z = zMin; z < zMax; z++)
                                    {
                                        int lowX = x + (int)(currentRoom.Position.X - otherRoom.Position.X);
                                        int lowZ = z + (int)(currentRoom.Position.Z - otherRoom.Position.Z);

                                        if (currentPortal.Direction == PortalDirection.Floor)
                                        {
                                            int minHeight = currentRoom.GetLowestCorner(xMin, zMin, xMax, zMax);
                                            int maxHeight = otherRoom.GetHighestCorner(otherXmin, otherZmin, otherXmax, otherZmax);

                                            level.Rooms[currentPortal.Room].Blocks[x, z].FloorPortal = i;

                                            int h1 = currentRoom.Blocks[x, z].QAFaces[0];
                                            int h2 = currentRoom.Blocks[x, z].QAFaces[1];
                                            int h3 = currentRoom.Blocks[x, z].QAFaces[2];
                                            int h4 = currentRoom.Blocks[x, z].QAFaces[3];

                                            int lh1 = otherRoom.Ceiling + otherRoom.Blocks[lowX, lowZ].WSFaces[0];
                                            int lh2 = otherRoom.Ceiling + otherRoom.Blocks[lowX, lowZ].WSFaces[1];
                                            int lh3 = otherRoom.Ceiling + otherRoom.Blocks[lowX, lowZ].WSFaces[2];
                                            int lh4 = otherRoom.Ceiling + otherRoom.Blocks[lowX, lowZ].WSFaces[3];

                                            bool defined = false;

                                            if (Room.IsQuad(x, z, h1, h2, h3, h4, true) && h1 == minHeight && otherRoom.Blocks[lowX, lowZ].Type != BlockType.Wall && lh1 == maxHeight &&
                                                currentRoom.Blocks[x, z].Type != BlockType.Wall)
                                            {
                                                /*if (level.Rooms[idCurrentRoom].Blocks[x, z].Type == BlockType.Floor)
                                                    level.Rooms[idCurrentRoom].Blocks[x, z].Type = BlockType.FloorPortal;
                                                else if (level.Rooms[idCurrentRoom].Blocks[x, z].Type == BlockType.CeilingPortal)
                                                    level.Rooms[idCurrentRoom].Blocks[x, z].Type = BlockType.Portal;*/

                                                level.Rooms[currentPortal.Room].Blocks[x, z].IsFloorSolid = false;
                                                defined = true;
                                            }
                                            else
                                            {
                                                /*if (level.Rooms[idCurrentRoom].Blocks[x, z].Type == BlockType.Floor)
                                                    level.Rooms[idCurrentRoom].Blocks[x, z].Type = BlockType.FloorPortal;
                                                else if (level.Rooms[idCurrentRoom].Blocks[x, z].Type == BlockType.CeilingPortal)
                                                    level.Rooms[idCurrentRoom].Blocks[x, z].Type = BlockType.Portal;*/

                                                level.Rooms[idCurrentRoom].Blocks[x, z].IsFloorSolid = true;
                                                defined = false;
                                            }

                                            if (Room.IsQuad(x, z, lh1, lh2, lh3, lh4, true) && defined && lh1 == maxHeight)
                                            {
                                                /*if (level.Rooms[otherPortal.Room].Blocks[lowX, lowZ].Type == BlockType.Floor)
                                                    level.Rooms[otherPortal.Room].Blocks[lowX, lowZ].Type = BlockType.CeilingPortal;
                                                else if (level.Rooms[otherPortal.Room].Blocks[lowX, lowZ].Type == BlockType.FloorPortal)
                                                    level.Rooms[otherPortal.Room].Blocks[lowX, lowZ].Type = BlockType.Portal;*/

                                                level.Rooms[otherPortal.Room].Blocks[lowX, lowZ].IsCeilingSolid = false;
                                            }
                                            else
                                            {
                                                level.Rooms[otherPortal.Room].Blocks[lowX, lowZ].IsCeilingSolid = true;
                                            }
                                        }
                                        else
                                        {
                                            int minHeight = otherRoom.GetLowestCorner(otherXmin, otherZmin, otherXmax, otherZmax);
                                            int maxHeight = currentRoom.GetHighestCorner(xMin, zMin, xMax, zMax);

                                            level.Rooms[currentPortal.Room].Blocks[x, z].CeilingPortal = i;

                                            int h1 = currentRoom.Ceiling + currentRoom.Blocks[x, z].WSFaces[0];
                                            int h2 = currentRoom.Ceiling + currentRoom.Blocks[x, z].WSFaces[1];
                                            int h3 = currentRoom.Ceiling + currentRoom.Blocks[x, z].WSFaces[2];
                                            int h4 = currentRoom.Ceiling + currentRoom.Blocks[x, z].WSFaces[3];

                                            int lh1 = otherRoom.Blocks[lowX, lowZ].QAFaces[0];
                                            int lh2 = otherRoom.Blocks[lowX, lowZ].QAFaces[1];
                                            int lh3 = otherRoom.Blocks[lowX, lowZ].QAFaces[2];
                                            int lh4 = otherRoom.Blocks[lowX, lowZ].QAFaces[3];

                                            bool defined = false;

                                            if (Room.IsQuad(x, z, h1, h2, h3, h4, true) && h1 == maxHeight && otherRoom.Blocks[lowX, lowZ].Type != BlockType.Wall && lh1 == minHeight &&
                                                currentRoom.Blocks[x, z].Type != BlockType.Wall)
                                            {
                                                /*if (level.Rooms[idCurrentRoom].Blocks[x, z].Type == BlockType.Floor)
                                                    level.Rooms[idCurrentRoom].Blocks[x, z].Type = BlockType.CeilingPortal;
                                                else if (level.Rooms[idCurrentRoom].Blocks[x, z].Type == BlockType.FloorPortal)
                                                    level.Rooms[idCurrentRoom].Blocks[x, z].Type = BlockType.Portal;*/

                                                level.Rooms[idCurrentRoom].Blocks[x, z].IsCeilingSolid = false;
                                                defined = true;
                                            }
                                            else
                                            {
                                                /*if (level.Rooms[idCurrentRoom].Blocks[x, z].Type == BlockType.Floor)
                                                    level.Rooms[idCurrentRoom].Blocks[x, z].Type = BlockType.CeilingPortal;
                                                else if (level.Rooms[idCurrentRoom].Blocks[x, z].Type == BlockType.FloorPortal)
                                                    level.Rooms[idCurrentRoom].Blocks[x, z].Type = BlockType.Portal;*/

                                                level.Rooms[idCurrentRoom].Blocks[x, z].IsCeilingSolid = true;
                                                defined = false;
                                            }

                                            if (Room.IsQuad(x, z, lh1, lh2, lh3, lh4, true) && defined && lh1 == minHeight /*&& otherRoom.Blocks[lowX, lowZ].Type != BlockType.Wall*/)
                                            {
                                                /*if (level.Rooms[otherPortal.Room].Blocks[lowX, lowZ].Type == BlockType.Floor)
                                                    level.Rooms[otherPortal.Room].Blocks[lowX, lowZ].Type = BlockType.FloorPortal;
                                                else if (level.Rooms[otherPortal.Room].Blocks[lowX, lowZ].Type == BlockType.CeilingPortal)
                                                    level.Rooms[otherPortal.Room].Blocks[lowX, lowZ].Type = BlockType.Portal;*/

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
                                currentPortal.AdjoiningRoom = (otherRoom.BaseRoom != -1 ? otherRoom.BaseRoom : otherPortal.Room);
                                otherPortal.AdjoiningRoom = (currentRoom.BaseRoom != -1 ? currentRoom.BaseRoom : currentPortal.Room);
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
                }

                // Fix faces
                form.ReportProgress(85, "Building faces and geometry");
                for (int i = 0; i < level.Rooms.Length; i++)
                {
                    if (level.Rooms[i] == null)
                        continue;
                    Room room = level.Rooms[i];

                    for (int j = 0; j < room.Lights.Count; j++)
                    {
                        Light light = room.Lights[j];

                        /*if (light.X < 1) light.X = 1;
                        if (light.X > room.NumXSectors - 2) light.X = (byte)(room.NumXSectors - 2);
                        if (light.Z < 1) light.Z = 1;
                        if (light.Z > room.NumZSectors - 2) light.Z = (byte)(room.NumZSectors - 2);*/

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
                            prj_block b = tempRooms[i][room.NumXSectors - 1 - x, z];

                            //if (room.Blocks[x, z].FloorPortal != -1 && ) room.Blocks[x, z].IsCeilingSolid = true;
                            // if (room.Blocks[x, z].CeilingPortal != -1) room.Blocks[x, z].IsFloorSolid = true;


                            for (int j = 0; j < 14; j++)
                            {
                                prj_face prjFace = b.faces[j];

                                bool isFlipped = (prjFace.txtFlags & 0x80) == 0x80;
                                bool isTransparent = (prjFace.txtFlags & 0x08) != 0;
                                bool isDoubleSided = (prjFace.txtFlags & 0x04) != 0;

                                prjFace.IsFlipped = isFlipped;

                                if (prjFace.txtType == 0x0007)
                                {
                                    prjFace.txtIndex = (short)(((prjFace.txtFlags & 0x03) << 8) + prjFace.txtIndex);

                                    prj_tex_info texInfo = tempTextures[prjFace.txtIndex];
                                    bool textureFound = false;

                                    byte newX = (byte)(texInfo.X % 256);
                                    byte newY = (byte)(texInfo.Y % 256);
                                    short tile = (short)Math.Floor((float)texInfo.Y / 256);

                                    for (int m = 0; m < level.TextureSamples.Count; m++)
                                    {
                                        LevelTexture currentTexture = level.TextureSamples.ElementAt(m).Value;
                                        if (currentTexture.X == newX && currentTexture.Y == newY &&
                                            currentTexture.Transparent == isTransparent && currentTexture.DoubleSided == isDoubleSided &&
                                            currentTexture.Width == (texInfo.Width + 1) && currentTexture.Height == (texInfo.Height + 1) &&
                                            currentTexture.Page == tile)
                                        {
                                            prjFace.NewID = level.TextureSamples.ElementAt(m).Key;
                                            LevelTexture texture = level.TextureSamples.ElementAt(m).Value;
                                            // texture.UsageCount++;
                                            level.TextureSamples[texture.ID] = texture;
                                            textureFound = true;
                                            break;
                                        }
                                    }

                                    if (!textureFound)
                                    {
                                        LevelTexture texture = new LevelTexture();
                                        texture.X = newX;
                                        texture.Y = newY;
                                        texture.Page = tile;
                                        //texture.UsageCount = 1;
                                        texture.Transparent = isTransparent;
                                        texture.DoubleSided = isDoubleSided;
                                        texture.Width = (short)(texInfo.Width + 1);
                                        texture.Height = (short)(texInfo.Height + 1);
                                        texture.ID = level.TextureSamples.Count;

                                        prjFace.NewID = texture.ID;

                                        level.TextureSamples.Add(texture.ID, texture);
                                    }
                                }
                                else
                                {

                                }

                                b.faces[j] = prjFace;
                            }

                            tempRooms[i][room.NumXSectors - 1 - x, z] = b;
                        }
                    }

                    room.BuildGeometry();

                    for (int z = 0; z < room.NumZSectors; z++)
                    {
                        for (int x = 0; x < room.NumXSectors; x++)
                        {
                            Block newBlock = room.Blocks[x, z];
                            prj_block prjBlock = tempRooms[i][room.NumXSectors - 1 - x, z];

                            newBlock.NoCollisionFloor = (((prjBlock.Flags2 & 0x04) != 0) || ((prjBlock.Flags2 & 0x02) != 0));
                            newBlock.NoCollisionCeiling = (((prjBlock.Flags2 & 0x10) != 0) || ((prjBlock.Flags2 & 0x08) != 0));

                            if ((prjBlock.Flags2 & 0x0040) != 0)
                                newBlock.Flags |= BlockFlags.Beetle;
                            if ((prjBlock.Flags2 & 0x0020) != 0)
                                newBlock.Flags |= BlockFlags.TriggerTriggerer;

                            short h1 = room.Blocks[x, z].QAFaces[0];
                            short h2 = room.Blocks[x, z].QAFaces[1];
                            short h3 = room.Blocks[x, z].QAFaces[2];
                            short h4 = room.Blocks[x, z].QAFaces[3];

                            bool isFloorSplitted = !Room.IsQuad(x, z, h1, h2, h3, h4, false);

                            newBlock.SplitFloor = (isFloorSplitted && prjBlock.Flags3 == 0x01);

                            for (int n = 0; n < 14; n++)
                            {
                                prj_face theFace = prjBlock.faces[n];
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
                                    if (newBlock.Faces[(int)BlockFaces.SouthRF].Defined || newBlock.Faces[(int)BlockFaces.SouthWS].Defined)
                                    {
                                        if (newBlock.Faces[(int)BlockFaces.SouthRF].Defined && !newBlock.Faces[(int)BlockFaces.SouthWS].Defined)
                                        {
                                            faceIndex = (int)BlockFaces.SouthRF;
                                            otherBlock = null;
                                        }
                                        else if (!newBlock.Faces[(int)BlockFaces.SouthRF].Defined && newBlock.Faces[(int)BlockFaces.SouthWS].Defined)
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

                                            if (otherBlock.Faces[(int)BlockFaces.NorthRF].Defined && !otherBlock.Faces[(int)BlockFaces.NorthWS].Defined)
                                            {
                                                faceIndex = (int)BlockFaces.NorthRF;
                                            }
                                            else if (!otherBlock.Faces[(int)BlockFaces.NorthRF].Defined && otherBlock.Faces[(int)BlockFaces.NorthWS].Defined)
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
                                    if (newBlock.Faces[(int)BlockFaces.EastRF].Defined || newBlock.Faces[(int)BlockFaces.EastWS].Defined)
                                    {
                                        if (newBlock.Faces[(int)BlockFaces.EastRF].Defined && !newBlock.Faces[(int)BlockFaces.EastWS].Defined)
                                        {
                                            faceIndex = (int)BlockFaces.EastRF;
                                            otherBlock = null;
                                        }
                                        else if (!newBlock.Faces[(int)BlockFaces.EastRF].Defined && newBlock.Faces[(int)BlockFaces.EastWS].Defined)
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

                                            if (otherBlock.Faces[(int)BlockFaces.WestRF].Defined && !otherBlock.Faces[(int)BlockFaces.WestWS].Defined)
                                            {
                                                faceIndex = (int)BlockFaces.WestRF;
                                            }
                                            else if (!otherBlock.Faces[(int)BlockFaces.WestRF].Defined && otherBlock.Faces[(int)BlockFaces.WestWS].Defined)
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
                                    if (newBlock.Faces[(int)BlockFaces.SouthRF].Defined || newBlock.Faces[(int)BlockFaces.SouthWS].Defined)
                                    {
                                        if (newBlock.Faces[(int)BlockFaces.SouthRF].Defined && newBlock.Faces[(int)BlockFaces.SouthWS].Defined)
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

                                            if (otherBlock.Faces[(int)BlockFaces.NorthRF].Defined && otherBlock.Faces[(int)BlockFaces.NorthWS].Defined)
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
                                    if (newBlock.Faces[(int)BlockFaces.EastRF].Defined || newBlock.Faces[(int)BlockFaces.EastWS].Defined)
                                    {
                                        if (newBlock.Faces[(int)BlockFaces.EastRF].Defined && newBlock.Faces[(int)BlockFaces.EastWS].Defined)
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

                                            if (otherBlock.Faces[(int)BlockFaces.WestRF].Defined && otherBlock.Faces[(int)BlockFaces.WestWS].Defined)
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

                                Block theBlock = (otherBlock != null ? otherBlock : newBlock);

                                if (faceIndex != -1)
                                {
                                    bool isFloor = (faceIndex == (int)BlockFaces.Floor || faceIndex == (int)BlockFaces.FloorTriangle2);
                                    bool isCeiling = (faceIndex == (int)BlockFaces.Ceiling || faceIndex == (int)BlockFaces.CeilingTriangle2);

                                    if (theFace.txtType == 0x07)
                                    {
                                        LevelTexture texture2 = level.TextureSamples[(short)theFace.NewID];
                                        Vector2[] UV = new Vector2[4];

                                        int yBlock = (int)(texture2.Page / 8);
                                        int xBlock = (int)(texture2.Page % 8);

                                        UV[0] = new Vector2((xBlock * 256 + texture2.X) / 2048.0f, (yBlock * 256 + texture2.Y) / 2048.0f);
                                        UV[1] = new Vector2((xBlock * 256 + texture2.X + texture2.Width) / 2048.0f, (yBlock * 256 + texture2.Y) / 2048.0f);
                                        ;
                                        UV[2] = new Vector2((xBlock * 256 + texture2.X + texture2.Width) / 2048.0f, (yBlock * 256 + texture2.Y + texture2.Height) / 2048.0f);
                                        UV[3] = new Vector2((xBlock * 256 + texture2.X) / 2048.0f, (yBlock * 256 + texture2.Y + texture2.Height) / 2048.0f);

                                        sbyte newRot = (sbyte)(theFace.txtRotation);
                                        newRot++;

                                        if (theBlock.Faces[faceIndex].Shape == BlockFaceShape.Rectangle)
                                            newRot = (sbyte)(newRot % 4);
                                        if (theBlock.Faces[faceIndex].Shape == BlockFaceShape.Triangle)
                                            newRot = (sbyte)(newRot % 3);

                                        if (theBlock.Faces[faceIndex].Defined && theBlock.Faces[faceIndex].Shape == BlockFaceShape.Triangle)
                                        {
                                            if (theFace.IsFlipped)
                                            {
                                                if (theFace.txtTriangle == 0)
                                                {
                                                    if (isFloor || isCeiling)
                                                    {
                                                        if (theBlock.RealSplitFloor == 0)
                                                        {
                                                            if (faceIndex == (int)BlockFaces.Floor || faceIndex == (int)BlockFaces.Ceiling)
                                                            {
                                                                theBlock.Faces[faceIndex].TriangleUV[0] = UV[1];
                                                                theBlock.Faces[faceIndex].TriangleUV[1] = UV[2];
                                                                theBlock.Faces[faceIndex].TriangleUV[2] = UV[0];

                                                                newRot = (sbyte)(newRot + 2);
                                                            }

                                                            if (faceIndex == (int)BlockFaces.FloorTriangle2 || faceIndex == (int)BlockFaces.CeilingTriangle2)
                                                            {
                                                                theBlock.Faces[faceIndex].TriangleUV2[0] = UV[1];
                                                                theBlock.Faces[faceIndex].TriangleUV2[1] = UV[2];
                                                                theBlock.Faces[faceIndex].TriangleUV2[2] = UV[0];

                                                                newRot = (sbyte)(newRot + 1);
                                                            }
                                                        }
                                                        else
                                                        {
                                                            if (faceIndex == (int)BlockFaces.Floor || faceIndex == (int)BlockFaces.Ceiling)
                                                            {
                                                                theBlock.Faces[faceIndex].TriangleUV[0] = UV[1];
                                                                theBlock.Faces[faceIndex].TriangleUV[1] = UV[2];
                                                                theBlock.Faces[faceIndex].TriangleUV[2] = UV[0];
                                                            }

                                                            if (faceIndex == (int)BlockFaces.FloorTriangle2 || faceIndex == (int)BlockFaces.CeilingTriangle2)
                                                            {
                                                                theBlock.Faces[faceIndex].TriangleUV2[0] = UV[1];
                                                                theBlock.Faces[faceIndex].TriangleUV2[1] = UV[2];
                                                                theBlock.Faces[faceIndex].TriangleUV2[2] = UV[0];

                                                                newRot = (sbyte)(newRot + 1);
                                                            }
                                                        }
                                                    }
                                                    else
                                                    {
                                                        theBlock.Faces[faceIndex].TriangleUV[0] = UV[1];
                                                        theBlock.Faces[faceIndex].TriangleUV[1] = UV[2];
                                                        theBlock.Faces[faceIndex].TriangleUV[2] = UV[0];

                                                        newRot = (sbyte)(newRot + 2);
                                                    }

                                                    theBlock.Faces[faceIndex].TextureTriangle = TextureTileType.TriangleNW;
                                                }

                                                if (theFace.txtTriangle == 1)
                                                {
                                                    if (isFloor || isCeiling)
                                                    {
                                                        if (theBlock.RealSplitFloor == 0)
                                                        {
                                                            if (faceIndex == (int)BlockFaces.Floor || faceIndex == (int)BlockFaces.Ceiling)
                                                            {
                                                                theBlock.Faces[faceIndex].TriangleUV[0] = UV[0];
                                                                theBlock.Faces[faceIndex].TriangleUV[1] = UV[3];
                                                                theBlock.Faces[faceIndex].TriangleUV[2] = UV[1];

                                                                newRot = (sbyte)(newRot + 2);
                                                            }

                                                            if (faceIndex == (int)BlockFaces.FloorTriangle2 || faceIndex == (int)BlockFaces.CeilingTriangle2)
                                                            {
                                                                theBlock.Faces[faceIndex].TriangleUV2[0] = UV[0];
                                                                theBlock.Faces[faceIndex].TriangleUV2[1] = UV[3];
                                                                theBlock.Faces[faceIndex].TriangleUV2[2] = UV[1];

                                                                newRot = (sbyte)(newRot + 1);
                                                            }
                                                        }
                                                        else
                                                        {
                                                            if (faceIndex == (int)BlockFaces.Floor || faceIndex == (int)BlockFaces.Ceiling)
                                                            {
                                                                theBlock.Faces[faceIndex].TriangleUV[0] = UV[0];
                                                                theBlock.Faces[faceIndex].TriangleUV[1] = UV[3];
                                                                theBlock.Faces[faceIndex].TriangleUV[2] = UV[1];
                                                            }

                                                            if (faceIndex == (int)BlockFaces.FloorTriangle2 || faceIndex == (int)BlockFaces.CeilingTriangle2)
                                                            {
                                                                theBlock.Faces[faceIndex].TriangleUV2[0] = UV[0];
                                                                theBlock.Faces[faceIndex].TriangleUV2[1] = UV[3];
                                                                theBlock.Faces[faceIndex].TriangleUV2[2] = UV[1];

                                                                newRot = (sbyte)(newRot + 1);
                                                            }
                                                        }
                                                    }
                                                    else
                                                    {
                                                        theBlock.Faces[faceIndex].TriangleUV[0] = UV[0];
                                                        theBlock.Faces[faceIndex].TriangleUV[1] = UV[3];
                                                        theBlock.Faces[faceIndex].TriangleUV[2] = UV[1];

                                                        newRot = (sbyte)(newRot + 2);
                                                    }

                                                    theBlock.Faces[faceIndex].TextureTriangle = TextureTileType.TriangleNE;
                                                }

                                                if (theFace.txtTriangle == 3)
                                                {
                                                    if (isFloor || isCeiling)
                                                    {
                                                        if (theBlock.RealSplitFloor == 0)
                                                        {
                                                            if (faceIndex == (int)BlockFaces.Floor || faceIndex == (int)BlockFaces.Ceiling)
                                                            {
                                                                theBlock.Faces[faceIndex].TriangleUV[0] = UV[2];
                                                                theBlock.Faces[faceIndex].TriangleUV[1] = UV[1];
                                                                theBlock.Faces[faceIndex].TriangleUV[2] = UV[3];

                                                                newRot = (sbyte)(newRot + 2);
                                                            }

                                                            if (faceIndex == (int)BlockFaces.FloorTriangle2 || faceIndex == (int)BlockFaces.CeilingTriangle2)
                                                            {
                                                                theBlock.Faces[faceIndex].TriangleUV2[0] = UV[2];
                                                                theBlock.Faces[faceIndex].TriangleUV2[1] = UV[1];
                                                                theBlock.Faces[faceIndex].TriangleUV2[2] = UV[3];

                                                                newRot = (sbyte)(newRot + 1);
                                                            }
                                                        }
                                                        else
                                                        {
                                                            if (faceIndex == (int)BlockFaces.Floor || faceIndex == (int)BlockFaces.Ceiling)
                                                            {
                                                                theBlock.Faces[faceIndex].TriangleUV[0] = UV[2];
                                                                theBlock.Faces[faceIndex].TriangleUV[1] = UV[1];
                                                                theBlock.Faces[faceIndex].TriangleUV[2] = UV[3];
                                                            }

                                                            if (faceIndex == (int)BlockFaces.FloorTriangle2 || faceIndex == (int)BlockFaces.CeilingTriangle2)
                                                            {
                                                                theBlock.Faces[faceIndex].TriangleUV2[0] = UV[2];
                                                                theBlock.Faces[faceIndex].TriangleUV2[1] = UV[1];
                                                                theBlock.Faces[faceIndex].TriangleUV2[2] = UV[3];

                                                                newRot = (sbyte)(newRot + 1);
                                                            }
                                                        }
                                                    }
                                                    else
                                                    {
                                                        theBlock.Faces[faceIndex].TriangleUV[0] = UV[2];
                                                        theBlock.Faces[faceIndex].TriangleUV[1] = UV[1];
                                                        theBlock.Faces[faceIndex].TriangleUV[2] = UV[3];

                                                        newRot = (sbyte)(newRot + 2);
                                                    }

                                                    theBlock.Faces[faceIndex].TextureTriangle = TextureTileType.TriangleSW;
                                                }

                                                if (theFace.txtTriangle == 2)
                                                {
                                                    if (isFloor || isCeiling)
                                                    {
                                                        if (theBlock.RealSplitFloor == 0)
                                                        {
                                                            if (faceIndex == (int)BlockFaces.Floor || faceIndex == (int)BlockFaces.Ceiling)
                                                            {
                                                                theBlock.Faces[faceIndex].TriangleUV[0] = UV[3];
                                                                theBlock.Faces[faceIndex].TriangleUV[1] = UV[2];
                                                                theBlock.Faces[faceIndex].TriangleUV[2] = UV[0];

                                                                newRot = (sbyte)(newRot + 2);
                                                            }

                                                            if (faceIndex == (int)BlockFaces.FloorTriangle2 || faceIndex == (int)BlockFaces.CeilingTriangle2)
                                                            {
                                                                theBlock.Faces[faceIndex].TriangleUV2[0] = UV[3];
                                                                theBlock.Faces[faceIndex].TriangleUV2[1] = UV[2];
                                                                theBlock.Faces[faceIndex].TriangleUV2[2] = UV[0];

                                                                newRot = (sbyte)(newRot + 1);
                                                            }
                                                        }
                                                        else
                                                        {
                                                            if (faceIndex == (int)BlockFaces.Floor || faceIndex == (int)BlockFaces.Ceiling)
                                                            {
                                                                theBlock.Faces[faceIndex].TriangleUV[0] = UV[3];
                                                                theBlock.Faces[faceIndex].TriangleUV[1] = UV[2];
                                                                theBlock.Faces[faceIndex].TriangleUV[2] = UV[0];
                                                            }

                                                            if (faceIndex == (int)BlockFaces.FloorTriangle2 || faceIndex == (int)BlockFaces.CeilingTriangle2)
                                                            {
                                                                theBlock.Faces[faceIndex].TriangleUV2[0] = UV[3];
                                                                theBlock.Faces[faceIndex].TriangleUV2[1] = UV[2];
                                                                theBlock.Faces[faceIndex].TriangleUV2[2] = UV[0];

                                                                newRot = (sbyte)(newRot + 1);
                                                            }
                                                        }
                                                    }
                                                    else
                                                    {
                                                        theBlock.Faces[faceIndex].TriangleUV[0] = UV[3];
                                                        theBlock.Faces[faceIndex].TriangleUV[1] = UV[2];
                                                        theBlock.Faces[faceIndex].TriangleUV[2] = UV[0];

                                                        newRot = (sbyte)(newRot + 2);
                                                    }

                                                    theBlock.Faces[faceIndex].TextureTriangle = TextureTileType.TriangleSE;
                                                }
                                            }
                                            else
                                            {
                                                if (theFace.txtTriangle == 0)
                                                {
                                                    if (isFloor || isCeiling)
                                                    {
                                                        if (theBlock.RealSplitFloor == 0)
                                                        {
                                                            if (faceIndex == (int)BlockFaces.Floor || faceIndex == (int)BlockFaces.Ceiling) // CORRETTO
                                                            {
                                                                theBlock.Faces[faceIndex].TriangleUV[0] = UV[0];
                                                                theBlock.Faces[faceIndex].TriangleUV[1] = UV[1];
                                                                theBlock.Faces[faceIndex].TriangleUV[2] = UV[3];

                                                                newRot = (sbyte)(newRot + 2);
                                                            }

                                                            if (faceIndex == (int)BlockFaces.FloorTriangle2 || faceIndex == (int)BlockFaces.CeilingTriangle2) // CORRETTO
                                                            {
                                                                theBlock.Faces[faceIndex].TriangleUV2[0] = UV[0];
                                                                theBlock.Faces[faceIndex].TriangleUV2[1] = UV[1];
                                                                theBlock.Faces[faceIndex].TriangleUV2[2] = UV[3];

                                                                newRot = (sbyte)(newRot + 1);
                                                            }
                                                        }
                                                        else
                                                        {
                                                            if (faceIndex == (int)BlockFaces.Floor || faceIndex == (int)BlockFaces.Ceiling) // CORRETTO
                                                            {
                                                                theBlock.Faces[faceIndex].TriangleUV[0] = UV[0];
                                                                theBlock.Faces[faceIndex].TriangleUV[1] = UV[1];
                                                                theBlock.Faces[faceIndex].TriangleUV[2] = UV[3];
                                                            }

                                                            if (faceIndex == (int)BlockFaces.FloorTriangle2 || faceIndex == (int)BlockFaces.CeilingTriangle2) // CORRETTO
                                                            {
                                                                theBlock.Faces[faceIndex].TriangleUV2[0] = UV[0];
                                                                theBlock.Faces[faceIndex].TriangleUV2[1] = UV[1];
                                                                theBlock.Faces[faceIndex].TriangleUV2[2] = UV[3];

                                                                newRot = (sbyte)(newRot + 1);
                                                            }
                                                        }
                                                    }
                                                    else
                                                    {
                                                        theBlock.Faces[faceIndex].TriangleUV[0] = UV[0];
                                                        theBlock.Faces[faceIndex].TriangleUV[1] = UV[1];
                                                        theBlock.Faces[faceIndex].TriangleUV[2] = UV[3];

                                                        newRot = (sbyte)(newRot + 2);
                                                    }

                                                    theBlock.Faces[faceIndex].TextureTriangle = TextureTileType.TriangleNW;
                                                }

                                                if (theFace.txtTriangle == 1)
                                                {
                                                    if (isFloor || isCeiling)
                                                    {
                                                        if (theBlock.RealSplitFloor == 0)
                                                        {
                                                            if (faceIndex == (int)BlockFaces.Floor || faceIndex == (int)BlockFaces.Ceiling) // CORRETTO
                                                            {
                                                                theBlock.Faces[faceIndex].TriangleUV[0] = UV[1];
                                                                theBlock.Faces[faceIndex].TriangleUV[1] = UV[2];
                                                                theBlock.Faces[faceIndex].TriangleUV[2] = UV[0];

                                                                newRot = (sbyte)(newRot + 2);
                                                            }

                                                            if (faceIndex == (int)BlockFaces.FloorTriangle2 || faceIndex == (int)BlockFaces.CeilingTriangle2) // CORRETTO
                                                            {
                                                                theBlock.Faces[faceIndex].TriangleUV2[0] = UV[1];
                                                                theBlock.Faces[faceIndex].TriangleUV2[1] = UV[2];
                                                                theBlock.Faces[faceIndex].TriangleUV2[2] = UV[0];

                                                                newRot = (sbyte)(newRot + 1);
                                                            }
                                                        }
                                                        else
                                                        {
                                                            if (faceIndex == (int)BlockFaces.Floor || faceIndex == (int)BlockFaces.Ceiling) // CORRETTO
                                                            {
                                                                theBlock.Faces[faceIndex].TriangleUV[0] = UV[1];
                                                                theBlock.Faces[faceIndex].TriangleUV[1] = UV[2];
                                                                theBlock.Faces[faceIndex].TriangleUV[2] = UV[0];
                                                            }

                                                            if (faceIndex == (int)BlockFaces.FloorTriangle2 || faceIndex == (int)BlockFaces.CeilingTriangle2) // CORRETTO
                                                            {
                                                                theBlock.Faces[faceIndex].TriangleUV2[0] = UV[1];
                                                                theBlock.Faces[faceIndex].TriangleUV2[1] = UV[2];
                                                                theBlock.Faces[faceIndex].TriangleUV2[2] = UV[0];

                                                                newRot = (sbyte)(newRot + 1);
                                                            }
                                                        }
                                                    }
                                                    else
                                                    {
                                                        theBlock.Faces[faceIndex].TriangleUV[0] = UV[1];
                                                        theBlock.Faces[faceIndex].TriangleUV[1] = UV[2];
                                                        theBlock.Faces[faceIndex].TriangleUV[2] = UV[0];

                                                        newRot = (sbyte)(newRot + 1);
                                                    }

                                                    theBlock.Faces[faceIndex].TextureTriangle = TextureTileType.TriangleNE;
                                                }

                                                if (theFace.txtTriangle == 3)
                                                {
                                                    if (isFloor || isCeiling)
                                                    {
                                                        if (theBlock.RealSplitFloor == 0)
                                                        {
                                                            if (faceIndex == (int)BlockFaces.Floor || faceIndex == (int)BlockFaces.Ceiling) // OK
                                                            {
                                                                theBlock.Faces[faceIndex].TriangleUV[0] = UV[3];
                                                                theBlock.Faces[faceIndex].TriangleUV[1] = UV[0];
                                                                theBlock.Faces[faceIndex].TriangleUV[2] = UV[2];

                                                                newRot = (sbyte)(newRot + 2);
                                                            }

                                                            if (faceIndex == (int)BlockFaces.FloorTriangle2 || faceIndex == (int)BlockFaces.CeilingTriangle2)  // TODO
                                                            {
                                                                theBlock.Faces[faceIndex].TriangleUV2[0] = UV[3];
                                                                theBlock.Faces[faceIndex].TriangleUV2[1] = UV[0];
                                                                theBlock.Faces[faceIndex].TriangleUV2[2] = UV[2];

                                                                newRot = (sbyte)(newRot + 1);
                                                            }
                                                        }
                                                        else
                                                        {
                                                            if (faceIndex == (int)BlockFaces.Floor || faceIndex == (int)BlockFaces.Ceiling) // OK
                                                            {
                                                                theBlock.Faces[faceIndex].TriangleUV[0] = UV[3];
                                                                theBlock.Faces[faceIndex].TriangleUV[1] = UV[0];
                                                                theBlock.Faces[faceIndex].TriangleUV[2] = UV[2];
                                                            }

                                                            if (faceIndex == (int)BlockFaces.FloorTriangle2 || faceIndex == (int)BlockFaces.CeilingTriangle2)  // OK
                                                            {
                                                                theBlock.Faces[faceIndex].TriangleUV2[0] = UV[3];
                                                                theBlock.Faces[faceIndex].TriangleUV2[1] = UV[0];
                                                                theBlock.Faces[faceIndex].TriangleUV2[2] = UV[2];

                                                                newRot = (sbyte)(newRot + 1);
                                                            }
                                                        }
                                                    }
                                                    else
                                                    {
                                                        theBlock.Faces[faceIndex].TriangleUV[0] = UV[3];
                                                        theBlock.Faces[faceIndex].TriangleUV[1] = UV[0];
                                                        theBlock.Faces[faceIndex].TriangleUV[2] = UV[2];
                                                    }

                                                    theBlock.Faces[faceIndex].TextureTriangle = TextureTileType.TriangleSW;
                                                }

                                                if (theFace.txtTriangle == 2)
                                                {
                                                    if (isFloor || isCeiling)
                                                    {
                                                        if (theBlock.RealSplitFloor == 0)
                                                        {
                                                            if (faceIndex == (int)BlockFaces.Floor || faceIndex == (int)BlockFaces.Ceiling)  // CORRETTO
                                                            {
                                                                theBlock.Faces[faceIndex].TriangleUV[0] = UV[2];
                                                                theBlock.Faces[faceIndex].TriangleUV[1] = UV[3];
                                                                theBlock.Faces[faceIndex].TriangleUV[2] = UV[1];

                                                                newRot = (sbyte)(newRot + 2);
                                                            }

                                                            if (faceIndex == (int)BlockFaces.FloorTriangle2 || faceIndex == (int)BlockFaces.CeilingTriangle2)  // TODO
                                                            {
                                                                theBlock.Faces[faceIndex].TriangleUV2[0] = UV[2];
                                                                theBlock.Faces[faceIndex].TriangleUV2[1] = UV[3];
                                                                theBlock.Faces[faceIndex].TriangleUV2[2] = UV[1];

                                                                newRot = (sbyte)(newRot + 1);
                                                            }
                                                        }
                                                        else
                                                        {
                                                            if (faceIndex == (int)BlockFaces.Floor || faceIndex == (int)BlockFaces.Ceiling) // CORRETTO
                                                            {
                                                                theBlock.Faces[faceIndex].TriangleUV[0] = UV[2];
                                                                theBlock.Faces[faceIndex].TriangleUV[1] = UV[3];
                                                                theBlock.Faces[faceIndex].TriangleUV[2] = UV[1];
                                                            }

                                                            if (faceIndex == (int)BlockFaces.FloorTriangle2 || faceIndex == (int)BlockFaces.CeilingTriangle2) // CORRETTO
                                                            {
                                                                theBlock.Faces[faceIndex].TriangleUV2[0] = UV[2];
                                                                theBlock.Faces[faceIndex].TriangleUV2[1] = UV[3];
                                                                theBlock.Faces[faceIndex].TriangleUV2[2] = UV[1];

                                                                newRot = (sbyte)(newRot + 1);
                                                            }
                                                        }
                                                    }
                                                    else
                                                    {
                                                        theBlock.Faces[faceIndex].TriangleUV[0] = UV[2];
                                                        theBlock.Faces[faceIndex].TriangleUV[1] = UV[3];
                                                        theBlock.Faces[faceIndex].TriangleUV[2] = UV[1];

                                                        newRot = (sbyte)(newRot + 1);
                                                    }

                                                    theBlock.Faces[faceIndex].TextureTriangle = TextureTileType.TriangleSE;
                                                }
                                            }

                                            newRot = (sbyte)(newRot % 3);

                                            for (int rot = 0; rot < newRot; rot++)
                                            {
                                                if (faceIndex != (int)BlockFaces.FloorTriangle2 && faceIndex != (int)BlockFaces.CeilingTriangle2)
                                                {
                                                    Vector2 temp3 = theBlock.Faces[faceIndex].TriangleUV[2];
                                                    theBlock.Faces[faceIndex].TriangleUV[2] = theBlock.Faces[faceIndex].TriangleUV[1];
                                                    theBlock.Faces[faceIndex].TriangleUV[1] = theBlock.Faces[faceIndex].TriangleUV[0];
                                                    theBlock.Faces[faceIndex].TriangleUV[0] = temp3;
                                                }

                                                if (faceIndex == (int)BlockFaces.FloorTriangle2 || faceIndex == (int)BlockFaces.CeilingTriangle2)
                                                {
                                                    Vector2 temp3 = theBlock.Faces[faceIndex].TriangleUV2[2];
                                                    theBlock.Faces[faceIndex].TriangleUV2[2] = theBlock.Faces[faceIndex].TriangleUV2[1];
                                                    theBlock.Faces[faceIndex].TriangleUV2[1] = theBlock.Faces[faceIndex].TriangleUV2[0];
                                                    theBlock.Faces[faceIndex].TriangleUV2[0] = temp3;
                                                }
                                            }

                                            theBlock.Faces[faceIndex].Transparent = level.TextureSamples[theFace.NewID].Transparent;
                                            theBlock.Faces[faceIndex].DoubleSided = level.TextureSamples[theFace.NewID].DoubleSided;
                                            theBlock.Faces[faceIndex].Flipped = theFace.IsFlipped;
                                            theBlock.Faces[faceIndex].Rotation = (byte)(newRot);
                                        }
                                        else
                                        {
                                            if (theFace.IsFlipped)
                                            {
                                                Vector2 temp = UV[0];
                                                UV[0] = UV[1];
                                                UV[1] = temp;

                                                temp = UV[2];
                                                UV[2] = UV[3];
                                                UV[3] = temp;
                                            }

                                            newRot += adjustRotation;
                                            if (newRot < 0)
                                                newRot = (sbyte)(3 - newRot);
                                            if (newRot > 3)
                                                newRot = (sbyte)(newRot % 3);

                                            for (int rot = 0; rot < newRot; rot++)
                                            {
                                                Vector2 temp = UV[3];
                                                UV[3] = UV[2];
                                                UV[2] = UV[1];
                                                UV[1] = UV[0];
                                                UV[0] = temp;
                                            }

                                            theBlock.Faces[faceIndex].RectangleUV[0] = UV[0];
                                            theBlock.Faces[faceIndex].RectangleUV[1] = UV[1];
                                            theBlock.Faces[faceIndex].RectangleUV[2] = UV[2];
                                            theBlock.Faces[faceIndex].RectangleUV[3] = UV[3];

                                            theBlock.Faces[faceIndex].TextureTriangle = TextureTileType.Rectangle;

                                            theBlock.Faces[faceIndex].Transparent = level.TextureSamples[theFace.NewID].Transparent;
                                            theBlock.Faces[faceIndex].DoubleSided = level.TextureSamples[theFace.NewID].DoubleSided;
                                            theBlock.Faces[faceIndex].Flipped = theFace.IsFlipped;
                                            theBlock.Faces[faceIndex].Rotation = (byte)newRot;
                                        }

                                        theBlock.Faces[faceIndex].Texture = (short)theFace.NewID;
                                    }
                                    else if (theFace.txtType == 0x00)
                                    {
                                        theBlock.Faces[faceIndex].Texture = -1;
                                    }
                                    else
                                    {
                                        theBlock.Faces[faceIndex].Invisible = true;
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
                DarkUI.Forms.DarkMessageBox.ShowError("There was an error while importing the PRJ file. Message: " + ex.Message, "Error");
                return null;
            }

            for (int i = 0; i < level.Portals.Count; i++)
            {
                level.Rooms[level.Portals.ElementAt(i).Value.Room].Portals.Add(level.Portals.ElementAt(i).Key);
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

        public static Level LoadFromPrj2(string filename)
        {
            Level level = new Level();

            try
            {
                BinaryReaderEx reader = new BinaryReaderEx(File.OpenRead(filename));

                // Check file version
                byte[] buffer = reader.ReadBytes(4);
                if (buffer[0] == 0x50 && buffer[1] == 0x52 && buffer[2] == 0x4A && buffer[3] == 0x32)
                {
                    // PRJ2 senza compressione
                }
                else if (buffer[0] == 0x5A && buffer[1] == 0x52 && buffer[2] == 0x4A && buffer[3] == 0x32)
                {
                    // PRJ2 compresso
                    int uncompressedSize = reader.ReadInt32();
                    int compressedSize = reader.ReadInt32();
                    byte[] projectData = reader.ReadBytes(compressedSize);
                    projectData = Utils.DecompressData(projectData);

                    MemoryStream ms = new MemoryStream(projectData);
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

                // Little hack
                if (!File.Exists(level.TextureFile))
                    level.TextureFile = "coastal.png";

                /* if (level.TextureFile == "" || !File.Exists(level.TextureFile))
                 {
                     Debug.Log("Can't find texture map!", DebugType.Error);

                     if (DarkUI.Forms.DarkMessageBox.ShowWarning("The texture map '" + level.TextureFile + " could not be found. Do you want to browse it or cancel opening project?",
                                                                 "Open project", DarkUI.Forms.DarkDialogButton.YesNo) != DialogResult.Yes)
                     {
                         Debug.Log("PRJ2 loading canceled", DebugType.Error);
                         reader.Close();
                         return null;
                     }

                     // Ask for TGA file
                     tgaName = form.OpenTGA();
                     if (tgaName == "")
                     {
                         Debug.Log("PRJ import canceled", DebugType.Error);
                         reader.Close();
                         return null;
                     }
                 }*/

                //Read WAD file
                stringLength = reader.ReadInt32();
                level.WadFile = System.Text.Encoding.UTF8.GetString(reader.ReadBytes(stringLength));
                if (!File.Exists(level.WadFile))
                    level.WadFile = "graphics\\wads\\coastal.wad";

                // Read fillers
                reader.ReadInt32();
                reader.ReadInt32();
                reader.ReadInt32();
                reader.ReadInt32();

                // Read textures
                int numTextures = reader.ReadInt32();
                for (int i = 0; i < numTextures; i++)
                {
                    LevelTexture texture = new LevelTexture();

                    texture.ID = reader.ReadInt32();
                    texture.X = reader.ReadInt16();
                    texture.Y = reader.ReadInt16();
                    texture.Width = reader.ReadInt16();
                    texture.Height = reader.ReadInt16();
                    texture.Page = reader.ReadInt16();
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
                    Portal portal = new Portal(0, 0);

                    portal.ID = reader.ReadInt32();
                    portal.OtherID = reader.ReadInt32();
                    portal.Room = reader.ReadInt16();
                    portal.AdjoiningRoom = reader.ReadInt16();
                    portal.Direction = (PortalDirection)reader.ReadByte();
                    portal.X = reader.ReadByte();
                    portal.Z = reader.ReadByte();
                    portal.NumXBlocks = reader.ReadByte();
                    portal.NumZBlocks = reader.ReadByte();
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
                    int objectID = reader.ReadInt32();
                    ObjectInstanceType objectType = (ObjectInstanceType)reader.ReadByte();

                    IObjectInstance o;

                    if (objectType == ObjectInstanceType.Moveable)
                    {
                        o = new MoveableInstance(0, 0);
                    }
                    else if (objectType == ObjectInstanceType.StaticMesh)
                    {
                        o = new StaticMeshInstance(0, 0);
                    }
                    else if (objectType == ObjectInstanceType.Camera)
                    {
                        o = new CameraInstance(0, 0);
                    }
                    else if (objectType == ObjectInstanceType.Sink)
                    {
                        o = new SinkInstance(0, 0);
                    }
                    else if (objectType == ObjectInstanceType.Sound)
                    {
                        o = new SoundInstance(0, 0);
                    }
                    else if (objectType == ObjectInstanceType.FlyByCamera)
                    {
                        o = new FlybyCameraInstance(0, 0);
                    }
                    else
                    {
                        reader.Close();
                        return null;
                    }

                    o.ID = objectID;
                    o.Type = objectType;
                    o.Position = new SharpDX.Vector3(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
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
                        ((StaticMeshInstance)o).ObjectID = reader.ReadInt32();
                        ((StaticMeshInstance)o).Color = Color.FromArgb(255, reader.ReadByte(), reader.ReadByte(), reader.ReadByte());
                        reader.ReadBytes(1);
                    }

                    if (o.Type == ObjectInstanceType.Moveable)
                    {
                        ((MoveableInstance)o).ObjectID = reader.ReadInt32();
                        reader.ReadBytes(4);
                    }

                    if (o.Type == ObjectInstanceType.Camera)
                    {
                        ((CameraInstance)o).Fixed = reader.ReadBoolean();
                        reader.ReadBytes(7);
                    }

                    if (o.Type == ObjectInstanceType.Sink)
                    {
                        ((SinkInstance)o).Strength = reader.ReadInt16();
                        reader.ReadBytes(6);
                    }

                    if (o.Type == ObjectInstanceType.Sound)
                    {
                        ((SoundInstance)o).SoundID = reader.ReadInt16();
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
                    TriggerInstance o = new TriggerInstance(0, 0);

                    o.ID = reader.ReadInt32();
                    o.X = reader.ReadByte();
                    o.Z = reader.ReadByte();
                    o.NumXBlocks = reader.ReadByte();
                    o.NumZBlocks = reader.ReadByte();
                    o.TriggerType = (TriggerType)reader.ReadByte();
                    o.TargetType = (TriggerTargetType)reader.ReadByte();
                    o.Target = reader.ReadInt32();
                    o.Timer = reader.ReadInt16();
                    o.OneShot = reader.ReadBoolean();
                    o.Bits[0] = reader.ReadBoolean();
                    o.Bits[1] = reader.ReadBoolean();
                    o.Bits[2] = reader.ReadBoolean();
                    o.Bits[3] = reader.ReadBoolean();
                    o.Bits[4] = reader.ReadBoolean();
                    o.Room = reader.ReadInt16();

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
                    string roomMagicWord = System.Text.ASCIIEncoding.ASCII.GetString(reader.ReadBytes(4));

                    bool defined = reader.ReadBoolean();
                    if (!defined)
                    {
                        level.Rooms[i] = null;
                        continue;
                    }

                    Room room = new Room(level);

                    room.Name = System.Text.Encoding.UTF8.GetString(reader.ReadBytes(100)).Trim();
                    room.Position = new SharpDX.Vector3(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
                    room.Ceiling = reader.ReadInt16();
                    room.NumXSectors = reader.ReadByte();
                    room.NumZSectors = reader.ReadByte();

                    room.Blocks = new Block[room.NumXSectors, room.NumZSectors];

                    for (int z = 0; z < room.NumZSectors; z++)
                    {
                        for (int x = 0; x < room.NumXSectors; x++)
                        {
                            Block b = new Block(level, room, BlockType.Floor, BlockFlags.None, 0);

                            b.Type = (BlockType)reader.ReadByte();
                            b.Flags = (BlockFlags)reader.ReadInt16();

                            b.QAFaces[0] = reader.ReadInt16();
                            b.QAFaces[1] = reader.ReadInt16();
                            b.QAFaces[2] = reader.ReadInt16();
                            b.QAFaces[3] = reader.ReadInt16();

                            b.EDFaces[0] = reader.ReadInt16();
                            b.EDFaces[1] = reader.ReadInt16();
                            b.EDFaces[2] = reader.ReadInt16();
                            b.EDFaces[3] = reader.ReadInt16();

                            b.WSFaces[0] = reader.ReadInt16();
                            b.WSFaces[1] = reader.ReadInt16();
                            b.WSFaces[2] = reader.ReadInt16();
                            b.WSFaces[3] = reader.ReadInt16();

                            b.RFFaces[0] = reader.ReadInt16();
                            b.RFFaces[1] = reader.ReadInt16();
                            b.RFFaces[2] = reader.ReadInt16();
                            b.RFFaces[3] = reader.ReadInt16();

                            b.SplitFoorType = reader.ReadByte();
                            b.SplitFloor = reader.ReadBoolean();
                            b.SplitCeilingType = reader.ReadByte();
                            b.SplitCeiling = reader.ReadBoolean();
                            b.RealSplitFloor = reader.ReadByte();
                            b.RealSplitCeiling = reader.ReadByte();

                            b.Climb[0] = reader.ReadBoolean();
                            b.Climb[1] = reader.ReadBoolean();
                            b.Climb[2] = reader.ReadBoolean();
                            b.Climb[3] = reader.ReadBoolean();

                            b.FloorOpacity = (PortalOpacity)reader.ReadByte();
                            b.CeilingOpacity = (PortalOpacity)reader.ReadByte();
                            b.WallOpacity = (PortalOpacity)reader.ReadByte();

                            b.FloorPortal = reader.ReadInt32();
                            b.CeilingPortal = reader.ReadInt32();
                            b.WallPortal = reader.ReadInt32();
                            b.IsFloorSolid = reader.ReadBoolean();
                            b.IsCeilingSolid = reader.ReadBoolean();
                            b.NoCollisionFloor = reader.ReadBoolean();
                            b.NoCollisionCeiling = reader.ReadBoolean();

                            for (int j = 0; j < b.Faces.Length; j++)
                            {
                                BlockFace f = b.Faces[j];

                                b.Faces[j].Defined = reader.ReadBoolean();
                                b.Faces[j].Flipped = reader.ReadBoolean();
                                b.Faces[j].Texture = reader.ReadInt16();
                                b.Faces[j].Rotation = reader.ReadByte();
                                b.Faces[j].Transparent = reader.ReadBoolean();
                                b.Faces[j].DoubleSided = reader.ReadBoolean();
                                b.Faces[j].Invisible = reader.ReadBoolean();
                                b.Faces[j].NoCollision = reader.ReadBoolean();
                                b.Faces[j].TextureTriangle = (TextureTileType)reader.ReadByte();

                                for (int n = 0; n < 4; n++)
                                    b.Faces[j].RectangleUV[n] = new SharpDX.Vector2(reader.ReadSingle(), reader.ReadSingle());
                                for (int n = 0; n < 3; n++)
                                    b.Faces[j].TriangleUV[n] = new SharpDX.Vector2(reader.ReadSingle(), reader.ReadSingle());
                                for (int n = 0; n < 3; n++)
                                    b.Faces[j].TriangleUV2[n] = new SharpDX.Vector2(reader.ReadSingle(), reader.ReadSingle());

                                reader.ReadInt32();
                                reader.ReadInt32();
                                reader.ReadInt32();
                                reader.ReadInt32();
                            }

                            b.FloorDiagonalSplit = (DiagonalSplit)reader.ReadByte();
                            b.FloorDiagonalSplitType = (DiagonalSplitType)reader.ReadByte();
                            b.CeilingDiagonalSplit = (DiagonalSplit)reader.ReadByte();
                            b.CeilingDiagonalSplitType = (DiagonalSplitType)reader.ReadByte();

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
                        Light l = new Geometry.Light();

                        l.Type = (LightType)reader.ReadByte();
                        l.Position = new SharpDX.Vector3(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
                        l.Intensity = reader.ReadSingle();
                        l.Color = Color.FromArgb(255, reader.ReadByte(), reader.ReadByte(), reader.ReadByte());
                        l.In = reader.ReadSingle();
                        l.Out = reader.ReadSingle();
                        l.Len = reader.ReadSingle();
                        l.Cutoff = reader.ReadSingle();
                        l.DirectionX = reader.ReadSingle();
                        l.DirectionY = reader.ReadSingle();
                        l.Face = (BlockFaces)reader.ReadByte();

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
                    AnimatexTextureSetEffect effect = (AnimatexTextureSetEffect)reader.ReadByte();
                    AnimatedTextureSet aSet = new AnimatedTextureSet();
                    aSet.Effect = effect;
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
                    TextureSound txtSound = new TextureSound(reader.ReadInt16(), reader.ReadInt16(), reader.ReadInt16());
                    txtSound.Sound = (TextureSounds)reader.ReadByte();

                    level.TextureSounds.Add(txtSound);
                }

                int numPaletteColors = reader.ReadInt32();
                Editor.Instance.Palette.Clear();
                for (int i = 0; i < numPaletteColors; i++)
                {
                    Editor.Instance.Palette.Add(Color.FromArgb(255, reader.ReadByte(), reader.ReadByte(), reader.ReadByte()));
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
            level.LoadTextureMap(level.TextureFile);

            // Now it's time to load WAD
            level.LoadWad(level.WadFile);

            // Now fill the structures loaded from PRJ2 
            for (int i = 0; i < level.Triggers.Count; i++)
            {
                TriggerInstance trigger = level.Triggers[i];

                for (int x = trigger.X; x < trigger.X + trigger.NumXBlocks; x++)
                {
                    for (int z = trigger.Z; z < trigger.Z + trigger.NumZBlocks; z++)
                    {
                        level.Rooms[trigger.Room].Blocks[x, z].Triggers.Add(trigger.ID);
                    }
                }
            }

            for (int i = 0; i < level.Objects.Count; i++)
            {
                ObjectInstanceType objectType = level.Objects.ElementAt(i).Value.Type;
                short roomIndex = level.Objects.ElementAt(i).Value.Room;
                int objectID = level.Objects.ElementAt(i).Value.ID;

                if (objectType == ObjectInstanceType.Moveable)
                    level.Rooms[roomIndex].Moveables.Add(objectID);
                if (objectType == ObjectInstanceType.StaticMesh)
                    level.Rooms[roomIndex].StaticMeshes.Add(objectID);
                if (objectType == ObjectInstanceType.Camera)
                    level.Rooms[roomIndex].Cameras.Add(objectID);
                if (objectType == ObjectInstanceType.Sink)
                    level.Rooms[roomIndex].Sinks.Add(objectID);
                if (objectType == ObjectInstanceType.Sound)
                    level.Rooms[roomIndex].SoundSources.Add(objectID);
                if (objectType == ObjectInstanceType.FlyByCamera)
                    level.Rooms[roomIndex].FlyByCameras.Add(objectID);

                if (objectType == ObjectInstanceType.Moveable)
                {
                    uint oid = (uint)((MoveableInstance)level.Objects.ElementAt(i).Value).ObjectID;
                    ((MoveableInstance)level.Objects.ElementAt(i).Value).Model = level.Wad.Moveables[oid];
                }

                if (objectType == ObjectInstanceType.StaticMesh)
                {
                    uint oid = (uint)((StaticMeshInstance)level.Objects.ElementAt(i).Value).ObjectID;
                    ((StaticMeshInstance)level.Objects.ElementAt(i).Value).Model = level.Wad.StaticMeshes[oid];
                }
            }

            // Now build the real geometry and update DirectX buffers
            for (int i = 0; i < level.Rooms.Length; i++)
            {
                if (level.Rooms[i] != null)
                {
                    level.Rooms[i].InitializeVerticesGrid();
                    level.Rooms[i].BuildGeometry();
                    level.Rooms[i].CalculateLightingForThisRoom();
                    level.Rooms[i].UpdateBuffers();
                }
            }

            for (int i = 0; i < level.Portals.Count; i++)
            {
                level.Rooms[level.Portals.ElementAt(i).Value.Room].Portals.Add(level.Portals.ElementAt(i).Key);
            }

            level.FileName = filename;

            return level;
        }

        public static bool SaveToPrj2(string filename, Level level)
        {
            byte filler8 = 0;
            short filler16 = 0;
            int filler32 = 0;

            try
            {
                MemoryStream ms = new MemoryStream();
                BinaryWriterEx writer = new BinaryWriterEx(ms);

                // Write version
                byte[] version = new byte[] { 0x50, 0x52, 0x4A, 0x32 };
                writer.Write(version);

                int versionCode = 2;
                writer.Write(versionCode);

                // Write texture map
                byte[] textureFile = System.Text.Encoding.UTF8.GetBytes(level.TextureFile);
                int numBytes = (int)textureFile.Length;
                writer.Write(numBytes);
                writer.Write(textureFile);

                byte[] wadFile = System.Text.Encoding.UTF8.GetBytes(level.WadFile);
                numBytes = (int)wadFile.Length;
                writer.Write(numBytes);
                writer.Write(wadFile);

                writer.Write(filler32);
                writer.Write(filler32);
                writer.Write(filler32);
                writer.Write(filler32);

                // Write textures
                int numTextures = (int)level.TextureSamples.Count;
                writer.Write(numTextures);
                for (int i = 0; i < level.TextureSamples.Count; i++)
                {
                    LevelTexture txt = level.TextureSamples.ElementAt(i).Value;

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
                int numPortals = (int)level.Portals.Count;
                writer.Write(numPortals);
                for (int i = 0; i < level.Portals.Count; i++)
                {
                    Portal p = level.Portals.ElementAt(i).Value;

                    writer.Write(p.ID);
                    writer.Write(p.OtherID);
                    writer.Write(p.Room);
                    writer.Write(p.AdjoiningRoom);
                    writer.Write((byte)p.Direction);
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
                int numObjects = (int)level.Objects.Count;
                writer.Write(numObjects);
                for (int i = 0; i < level.Objects.Count; i++)
                {
                    IObjectInstance o = level.Objects.ElementAt(i).Value;

                    writer.Write(o.ID);
                    writer.Write((byte)o.Type);
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

                    if (o.Type == ObjectInstanceType.StaticMesh)
                    {
                        StaticMeshInstance sm = (StaticMeshInstance)o;
                        writer.Write(sm.Model.ObjectID);
                        writer.Write(sm.Color.R);
                        writer.Write(sm.Color.G);
                        writer.Write(sm.Color.B);

                        writer.Write(filler8);
                    }

                    if (o.Type == ObjectInstanceType.Moveable)
                    {
                        MoveableInstance m = (MoveableInstance)o;
                        writer.Write(m.Model != null ? m.Model.ObjectID : 0);

                        writer.Write(filler32);
                    }

                    if (o.Type == ObjectInstanceType.Camera)
                    {
                        CameraInstance c = (CameraInstance)o;
                        writer.Write(c.Fixed);

                        writer.Write(filler8);
                        writer.Write(filler8);
                        writer.Write(filler8);
                        writer.Write(filler32);
                    }

                    if (o.Type == ObjectInstanceType.Sink)
                    {
                        SinkInstance s = (SinkInstance)o;
                        writer.Write(s.Strength);

                        writer.Write(filler8);
                        writer.Write(filler8);
                        writer.Write(filler32);
                    }

                    if (o.Type == ObjectInstanceType.Sound)
                    {
                        SoundInstance s = (SoundInstance)o;
                        writer.Write(s.SoundID);

                        writer.Write(filler8);
                        writer.Write(filler8);
                        writer.Write(filler32);
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
                int numTriggers = (int)level.Triggers.Count;
                writer.Write(numTriggers);
                for (int i = 0; i < level.Triggers.Count; i++)
                {
                    TriggerInstance o = level.Triggers.ElementAt(i).Value;

                    writer.Write(o.ID);
                    writer.Write(o.X);
                    writer.Write(o.Z);
                    writer.Write(o.NumXBlocks);
                    writer.Write(o.NumZBlocks);
                    writer.Write((byte)o.TriggerType);
                    writer.Write((byte)o.TargetType);
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
                int numRooms = 255;
                writer.Write(numRooms);
                for (int i = 0; i < numRooms; i++)
                {
                    byte[] roomMagicWord = System.Text.ASCIIEncoding.ASCII.GetBytes("ROOM");
                    writer.Write(roomMagicWord);

                    Room r = level.Rooms[i];

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
                            Block b = r.Blocks[x, z];

                            writer.Write((byte)b.Type);
                            writer.Write((short)b.Flags);
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
                            writer.Write((byte)b.FloorOpacity);
                            writer.Write((byte)b.CeilingOpacity);
                            writer.Write((byte)b.WallOpacity);
                            writer.Write(b.FloorPortal);
                            writer.Write(b.CeilingPortal);
                            writer.Write(b.WallPortal);
                            writer.Write(b.IsFloorSolid);
                            writer.Write(b.IsCeilingSolid);
                            writer.Write(b.NoCollisionFloor);
                            writer.Write(b.NoCollisionCeiling);

                            for (int j = 0; j < b.Faces.Length; j++)
                            {
                                BlockFace f = b.Faces[j];

                                writer.Write(f.Defined);
                                writer.Write(f.Flipped);
                                writer.Write(f.Texture);
                                writer.Write(f.Rotation);
                                writer.Write(f.Transparent);
                                writer.Write(f.DoubleSided);
                                writer.Write(f.Invisible);
                                writer.Write(f.NoCollision);
                                writer.Write((byte)f.TextureTriangle);
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

                            writer.Write((byte)b.FloorDiagonalSplit);
                            writer.Write((byte)b.FloorDiagonalSplitType);
                            writer.Write((byte)b.CeilingDiagonalSplit);
                            writer.Write((byte)b.CeilingDiagonalSplitType);

                            writer.Write(filler32);
                            writer.Write(filler32);
                            writer.Write(filler32);
                            writer.Write(filler32);
                        }
                    }

                    int numLights = (int)r.Lights.Count;
                    writer.Write(numLights);
                    for (int j = 0; j < numLights; j++)
                    {
                        Light l = r.Lights[j];

                        writer.Write((byte)l.Type);
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

                        byte b = (byte)l.Face;
                        writer.Write(b);

                        writer.Write(filler8);
                        writer.Write(filler8);
                        writer.Write(filler8);
                    }

                    writer.Write((byte)r.AmbientLight.R);
                    writer.Write((byte)r.AmbientLight.G);
                    writer.Write((byte)r.AmbientLight.B);
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
                int numAnimatedTextures = (int)level.AnimatedTextures.Count;
                writer.Write(numAnimatedTextures);
                for (int i = 0; i < level.AnimatedTextures.Count; i++)
                {
                    writer.Write((byte)level.AnimatedTextures[i].Effect);

                    int numTexturesInSet = (int)level.AnimatedTextures[i].Textures.Count;
                    writer.Write(numTexturesInSet);

                    for (int j = 0; j < level.AnimatedTextures[i].Textures.Count; j++)
                    {
                        writer.Write(level.AnimatedTextures[i].Textures[j].Page);
                        writer.Write(level.AnimatedTextures[i].Textures[j].X);
                        writer.Write(level.AnimatedTextures[i].Textures[j].Y);
                    }
                }

                int numTextureSounds = (int)level.TextureSounds.Count;
                writer.Write(numTextureSounds);
                for (int i = 0; i < level.TextureSounds.Count; i++)
                {
                    writer.Write(level.TextureSounds[i].X);
                    writer.Write(level.TextureSounds[i].Y);
                    writer.Write(level.TextureSounds[i].Page);
                    writer.Write((byte)level.TextureSounds[i].Sound);
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

                byte[] projectData = ms.ToArray();

                writer.Flush();
                writer.Close();

                writer = new BinaryWriterEx(File.OpenWrite(filename));

                /*/if (false)
                {
                    int uncompressedSize = projectData.Length;
                    projectData = Utils.CompressDataZLIB(projectData);
                    int compressedSize = projectData.Length;

                    version = new byte[] { 0x5A, 0x52, 0x4A, 0x32 };

                    writer.Write(version);
                    writer.Write(uncompressedSize);
                    writer.Write(compressedSize);
                    writer.Write(projectData);
                }
                else*/
                {
                    writer.Write(projectData);
                }

                writer.Flush();
                writer.Close();
            }
            catch (Exception)
            {
                return false;
            }

            level.FileName = filename;

            return true;
        }

        private static sbyte AdjustRotation(sbyte rot, int inc)
        {
            return rot;
        }

        public void DeleteObject(int instance)
        {
            List<int> triggersToDelete = new List<int>();

            // First I build a list of triggers to delete
            for (int i = 0; i < Triggers.Count; i++)
            {
                TriggerInstance trigger = Triggers.ElementAt(i).Value;

                if ((trigger.TargetType == TriggerTargetType.Camera || trigger.TargetType == TriggerTargetType.FlyByCamera ||
                    trigger.TargetType == TriggerTargetType.Object || trigger.TargetType == TriggerTargetType.Sink) &&
                    trigger.Target == instance)
                {
                    triggersToDelete.Add(trigger.ID);
                }
            }

            // Then I clean sectors and delete triggers
            for (int i = 0; i < triggersToDelete.Count; i++)
            {
                TriggerInstance trigger = Triggers[triggersToDelete[i]];

                // Clean sectors
                for (int z = trigger.Z; z < trigger.Z + trigger.NumZBlocks; z++)
                {
                    for (int x = trigger.X; x < trigger.X + trigger.NumXBlocks; x++)
                    {
                        Rooms[trigger.Room].Blocks[x, z].Triggers.Remove(trigger.ID);
                    }
                }

                // Delete trigger
                Triggers.Remove(triggersToDelete[i]);
            }
        }
    }
}
