using NLog;
using SharpDX.Toolkit.Graphics;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using TombLib.Wad;

namespace TombEditor.Geometry
{
    public class Level : IDisposable
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        public const short MaxSectorCoord = 100;
        public const short MaxNumberOfRooms = 512;
        public Room[] Rooms { get; } = new Room[MaxNumberOfRooms]; //Rooms in level
        public Wad2 Wad { get; private set; }
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
        
        public void Dispose()
        {
            UnloadWad();
        }
        
        private void UnloadWad()
        {
            logger.Info("Reseting wad");
            Wad?.Dispose();
            Wad = null;
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
                if (path.ToLower().EndsWith("wad"))
                {
                    var newWad = new Wad2();

                    try
                    {
                        var oldWad = new TR4Wad();
                        oldWad.LoadWad(path);

                        newWad = WadOperations.ConvertTr4Wad(oldWad);

                        newWad.OriginalWad = oldWad;
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
                else
                {
                    var newWad = new Wad2();

                    try
                    {
                        newWad = Wad2.LoadFromStream(File.OpenRead(path));
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

        public void ReloadLevelTextures()
        {
            foreach (var texture in Settings.Textures)
                texture.Reload(Settings);
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

            // Remove all objects in the room
            var objectsToRemove = room.AnyObjects.ToList();
            foreach (var instance in objectsToRemove)
                room.RemoveObject(this, instance);
            
            // Remove all references to this room
            Rooms[roomIndex] = null;
        }
    }
}
