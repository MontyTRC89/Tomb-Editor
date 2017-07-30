using System;
using System.Collections.Generic;
using System.Linq;
using System.Drawing;
using System.IO;
using SharpDX.Toolkit.Graphics;
using TombLib.Wad;
using TombLib.IO;
using System.Diagnostics;
using NLog;

namespace TombEditor.Geometry
{
    public class Level : IDisposable
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

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

        public Dictionary<int, ObjectInstance> Objects { get; } =
            new Dictionary<int, ObjectInstance>(); //Objects (moveables, static meshes, sinks, camera, fly-by cameras, sound sources)

        public List<AnimatedTextureSet> AnimatedTextures { get; } = new List<AnimatedTextureSet>();
        public List<TextureSound> TextureSounds { get; } = new List<TextureSound>();
        public Wad Wad { get; private set; }
        public string TextureFile { get; set; }
        public string WadFile { get; set; }
        public bool MustSave { get; set; } // Used for Save and Save as logic
        public string FileName { get; set; }

        public HashSet<Room> GetConnectedRooms(Room startingRoom)
        {
            var result = new HashSet<Room>();
            GetConnectedRoomsRecursively(result, startingRoom);
            if (startingRoom.Flipped && (startingRoom.AlternateRoom != null))
                GetConnectedRoomsRecursively(result, startingRoom.AlternateRoom);
            return result;
        }

        private void GetConnectedRoomsRecursively(ISet<Room> result, Room startingRoom)
        {
            result.Add(startingRoom);
            foreach (int portalIndex in startingRoom.Portals)
            {
                var room = Portals[portalIndex].AdjoiningRoom;
                if (!result.Contains(room))
                {
                    GetConnectedRoomsRecursively(result, room);
                    if (room.Flipped && (room.AlternateRoom != null))
                        GetConnectedRoomsRecursively(result, room.AlternateRoom);
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

        public int AddTexture(short x, short y, short w, short h, bool isDoubleSided, bool isTransparent)
        {
            short newX = x;
            short newY = y;

            // Step 1: check if there's another texture already in the list
            foreach (var texture in TextureSamples.Values)
            {
                if (texture.X == newX && (texture.Y + 256 * texture.Page) == newY && texture.Width == w &&
                    texture.Height == h
                    && texture.DoubleSided == isDoubleSided && texture.Transparent == isTransparent)
                    return texture.Id;
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
            var newTexture = new LevelTexture
            {
                X = newX,
                Y = (short)(newY - page * 256),
                Width = w,
                Height = h,
                Page = page,
                Id = id,
                Transparent = isTransparent,
                DoubleSided = isDoubleSided
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
            logger.Info("Loading texture map");

            var watch = new Stopwatch();
            watch.Start();

            //Free old texture map...
            _textureMap?.Dispose();

            // Load texture map as a bitmap
            _textureMap = TombLib.Graphics.TextureLoad.LoadToBitmap(filename);
            Utils.ConvertTextureTo256Width(ref _textureMap);

            // Calculate the number of pages
            int numPages = (int)Math.Floor(_textureMap.Height / 256.0f);
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
                            var dest = new System.Drawing.RectangleF(currentXblock * 256, currentYblock * 256, 256,
                                256);

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

        public static BinaryReaderEx CreatePrjReader(string filename)
        {
            var reader = new BinaryReaderEx(File.OpenRead(filename));

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
                projectData = Utils.DecompressData(projectData);

                var ms = new MemoryStream(projectData);
                ms.Seek(0, SeekOrigin.Begin);

                reader = new BinaryReaderEx(ms);
                reader.ReadInt32();
                return reader;
            }
            else
            {
                return null;
            }
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
                    triggersToDelete.Add(trigger.Id);
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
                        trigger.Room.Blocks[x, z].Triggers.Remove(trigger.Id);
                    }
                }

                // Delete trigger
                Triggers.Remove(t);
            }
        }

        public Room GetOrCreateRoom(int index)
        {
            if (index < 0 || index >= Rooms.Length)
                return null;

            return Rooms[index] ?? (Rooms[index] = new Room(this));
        }
    }
}
