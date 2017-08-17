using System;
using System.Collections.Generic;
using System.Linq;
using System.Drawing;
using SharpDX.Toolkit.Graphics;
using TombLib.Wad;
using System.Diagnostics;
using NLog;
using System.Drawing.Imaging;

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

        public Dictionary<int, Texture2D> DirectXTextures { get; } =
            new Dictionary<int, Texture2D>(); //For now just one texture atlas 2048x2048 pixel

        public Bitmap TextureMap { get; private set; } //The texture map on the CPU
        public IEnumerable<Portal> Portals => Rooms.Where(room => room != null).SelectMany(room => room.Portals);
        public Dictionary<int, TriggerInstance> Triggers { get; } = new Dictionary<int, TriggerInstance>();

        public Dictionary<int, PositionBasedObjectInstance> Objects { get; } =
            new Dictionary<int, PositionBasedObjectInstance>(); //Objects (moveables, static meshes, sinks, camera, fly-by cameras, sound sources)

        public List<AnimatedTextureSet> AnimatedTextures { get; } = new List<AnimatedTextureSet>();
        public List<TextureSound> TextureSounds { get; } = new List<TextureSound>();
        public Wad Wad { get; private set; }

        public LevelSettings Settings { get; set; } = new LevelSettings();

        public static Level CreateSimpleLevel()
        {
            logger.Info("Creating new empty level");

            Level result = new Level();
            if (result.Rooms[0] == null)
                result.Rooms[0] = new Room(result, 20, 20, "Room 0");
            return result;
        }

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
            foreach (var portal in startingRoom.Portals)
            {
                var room = portal.AdjoiningRoom;
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
            UnloadTexture();
            UnloadWad();
        }

        private void UnloadTexture()
        {
            logger.Info("Reseting texture");
            TextureMap?.Dispose();
            TextureMap = null;
            foreach (var texture in DirectXTextures)
                texture.Value.Dispose();
            DirectXTextures.Clear();
        }

        private void UnloadWad()
        {
            logger.Info("Reseting wad");
            Wad?.Dispose();
            Wad = null;
        }

        public void ReloadTexture()
        {
            string path = Settings.MakeAbsolute(Settings.TextureFilePath);
            if (string.IsNullOrEmpty(path))
            {
                UnloadTexture();
                return;
            }

            logger.Info("Loading texture");

            var watch = new Stopwatch();
            watch.Start();

            using (var oldTexture = TextureMap) //Free old texture map afterwards...
            {
                var newTextureMap = TombLib.Graphics.TextureLoad.LoadToBitmap(path);
                try
                {
                    Utils.ConvertTextureTo256Width(ref newTextureMap);

                    // Calculate the number of pages
                    int numPages = (int)Math.Floor(newTextureMap.Height / 256.0f);
                    if (newTextureMap.Height % 256 != 0)
                        numPages++;

                    logger.Debug("Building 2048x2048 texture atlas for DirectX");

                    // Copy the page in a temp bitmap. I generate a texture atlas, putting all texture pages inside 2048x2048 pixel 
                    // textures.
                    using (var tempBitmap = new Bitmap(2048, 2048, System.Drawing.Imaging.PixelFormat.Format32bppArgb))
                    {
                        tempBitmap.MakeTransparent(System.Drawing.Color.FromArgb(255, 255, 0, 255));
                        ColorMap[] colorMap = new ColorMap[1];
                        colorMap[0] = new ColorMap();
                        colorMap[0].OldColor = Color.FromArgb(255, 255, 0, 255);
                        colorMap[0].NewColor = Color.Transparent;
                        ImageAttributes attr = new ImageAttributes();
                        attr.SetRemapTable(colorMap);

                        using (var g = Graphics.FromImage(tempBitmap))
                        {
                            int currentXblock = 0;
                            int currentYblock = 0;
                            for (int i = 0; i < numPages; i++)
                            {
                                var src = new System.Drawing.Rectangle(0, 256 * i, 256, 256);
                                var dest = new System.Drawing.Rectangle(currentXblock * 256, currentYblock * 256, 256,
                                    256);

                                g.DrawImage(newTextureMap, dest, src.X, src.Y, src.Width, src.Height, GraphicsUnit.Pixel, attr);

                                currentXblock++;
                                if (currentXblock != 8)
                                    continue;

                                currentXblock = 0;
                                currentYblock++;
                            }
                        }

                        // Clean up DirectX texture
                        if (DirectXTextures.ContainsKey(0))
                        {
                            logger.Debug("Cleaning memory used by a previous texture map");

                            DirectXTextures[0].Dispose();
                            DirectXTextures.Remove(0);
                        }

                        // Create DirectX texture

                        DirectXTextures.Add(0, TombLib.Graphics.TextureLoad.LoadToTexture(DeviceManager.DefaultDeviceManager.Device, tempBitmap));
                    }
                }
                catch (Exception)
                {
                    newTextureMap?.Dispose();
                    throw;
                }
                TextureMap = newTextureMap;
            }
            
            watch.Stop();

            logger.Info("Texture map loaded");
            logger.Info("    Elapsed time: " + watch.ElapsedMilliseconds + " ms");
        }

        public void ReloadWad()
        {
            string path = Settings.MakeAbsolute(Settings.WadFilePath);
            if (string.IsNullOrEmpty(path))
            {
                UnloadWad();
                return;
            }

            using (var wad = Wad)
            {
                var newWad = Wad.LoadWad(path);
                try
                {
                    newWad.GraphicsDevice = DeviceManager.DefaultDeviceManager.Device;
                    newWad.PrepareDataForDirectX();
                }
                catch (Exception)
                {
                    newWad?.Dispose();
                    throw;
                }
                Wad = newWad;
            }
        }

        public void ReloadTextureTry()
        {
            try
            {
                ReloadTexture();
            }
            catch (Exception exc)
            {
                UnloadTexture();
                logger.Warn(exc, "Unable to load texture from '" + Settings.MakeAbsolute(Settings.TextureFilePath) + "'");
            }
        }

        public void ReloadObjectsTry()
        {
            try
            {
                ReloadWad();
            }
            catch (Exception exc)
            {
                UnloadWad();
                logger.Warn(exc, "Unable to load objects from '" + Settings.MakeAbsolute(Settings.WadFilePath) + "'");
            }
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
        
        public Room GetOrCreateDummyRoom(int index)
        {
            if (index < 0 || index >= Rooms.Length)
                return null;

            return Rooms[index] ?? (Rooms[index] = new Room(this, 1, 1));
        }

        public int GetFreeRoomIndex()
        {
            // Search the first free room
            for (int i = 0; i < Rooms.Length; i++)
                if (Rooms[i] == null)
                    return i;
            throw new Exception("A maximum number of " + Level.MaxNumberOfRooms + " rooms has been reached. Unable to add room.");
        }

        public void AssignRoomToFree(Room room)
        {
            Rooms[GetFreeRoomIndex()] = room;
        }

        public void DeleteRoom(Room room)
        {
            int roomIndex = Rooms.ReferenceIndexOf(room);
            if (roomIndex == -1)
                throw new ArgumentException("The room does not belong to the level from which should be removed.");

            // Collect all triggers and objects
                var objectsToRemove = new List<int>();
            var triggersToRemove = new List<int>();

            for (int i = 0; i < Objects.Count; i++)
            {
                var obj = Objects.ElementAt(i).Value;
                if (!ReferenceEquals(obj.Room, room))
                    continue;

                // We must remove that object. First try to find a trigger.
                for (int j = 0; j < Triggers.Count; j++)
                {
                    var trigger = Triggers.ElementAt(j).Value;

                    if (trigger.TargetType == TriggerTargetType.Camera && obj.Type == ObjectInstanceType.Camera &&
                        trigger.Target == obj.Id)
                    {
                        triggersToRemove.Add(trigger.Id);
                    }

                    if (trigger.TargetType == TriggerTargetType.FlyByCamera &&
                        obj.Type == ObjectInstanceType.FlyByCamera &&
                        trigger.Target == ((FlybyCameraInstance)obj).Sequence)
                    {
                        triggersToRemove.Add(trigger.Id);
                    }

                    if (trigger.TargetType == TriggerTargetType.Sink && obj.Type == ObjectInstanceType.Sink &&
                        trigger.Target == obj.Id)
                    {
                        triggersToRemove.Add(trigger.Id);
                    }

                    if (trigger.TargetType == TriggerTargetType.Object && obj.Type == ObjectInstanceType.Moveable &&
                        trigger.Target == obj.Id)
                    {
                        triggersToRemove.Add(trigger.Id);
                    }
                }

                // Remove the object
                objectsToRemove.Add(obj.Id);
            }

            // Remove objects and triggers
            foreach (int o in objectsToRemove)
                Objects.Remove(o);
            foreach (int t in triggersToRemove)
                Triggers.Remove(t);

            // Remove all references to this room
            Rooms[roomIndex] = null;
        }
    }
}
