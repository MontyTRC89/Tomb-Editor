using NLog;
using SharpDX.Toolkit.Graphics;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using TombLib.Wad;
using TombLib.Wad.Tr4Wad;

namespace TombEditor.Geometry
{
    public class Level : IDisposable
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        public const short MaxSectorCoord = 100;
        public const short MaxNumberOfRooms = 512;
        public Room[] Rooms { get; } = new Room[MaxNumberOfRooms]; //Rooms in level
        public Wad2 Wad { get; private set; }
        public LevelSettings Settings { get; private set; } = new LevelSettings();

        public static Level CreateSimpleLevel()
        {
            logger.Info("Creating new empty level");

            Level result = new Level();
            if (result.Rooms[0] == null)
                result.Rooms[0] = new Room(result, Room.MaxRoomDimensions, Room.MaxRoomDimensions, "Room 0");
            return result;
        }

        public HashSet<Room> GetConnectedRooms(Room startingRoom)
        {
            var result = new HashSet<Room>();
            GetConnectedRoomsRecursively(result, startingRoom);
            GetConnectedRoomsRecursively(result, startingRoom.AlternateVersion);
            return result;
        }

        private void GetConnectedRoomsRecursively(ISet<Room> result, Room startingRoom)
        {
            if ((startingRoom == null) || result.Contains(startingRoom))
                return;

            result.Add(startingRoom);
            foreach (var portal in startingRoom.Portals)
            {
                GetConnectedRoomsRecursively(result, portal.AdjoiningRoom);
                GetConnectedRoomsRecursively(result, portal.AdjoiningRoom?.AlternateVersion);
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
                .ThenBy((roomPair) => roomPair.Value.AlternateBaseRoom == null)
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
                Wad2 newWad = new Wad2(TombRaiderVersion.TR4);
                try
                {
                    if (path.EndsWith(".wad", StringComparison.InvariantCultureIgnoreCase))
                    {
                        List<string> soundPaths = new List<string>();
                        foreach (OldWadSoundPath path_ in Settings.OldWadSoundPaths)
                            soundPaths.Add(Settings.ParseVariables(path_.Path));

                        var oldWad = new Tr4Wad();
                        oldWad.LoadWad(path);
                        newWad = Tr4WadOperations.ConvertTr4Wad(oldWad, soundPaths);
                    }
                    else
                    {
                        newWad = Wad2.LoadFromStream(File.OpenRead(path));
                    }
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

        public void DeleteAlternateRoom(Room room)
        {
            for (int i = 0; i < Rooms.Length; ++i)
                if (Rooms[i] == room)
                {
                    // Remove all objects in the room
                    var objectsToRemove = room.AnyObjects.ToList();
                    foreach (var instance in objectsToRemove)
                        if (room.AlternateBaseRoom != null)
                            room.RemoveObjectAndSingularPortal(this, instance);
                        else
                            room.RemoveObject(this, instance);

                    // Remove all references to this room
                    Rooms[i] = null;
                    return;
                }
           throw new ArgumentException("The room does not belong to the level from which it should be removed.");
        }

        public void DeleteRoom(Room room)
        {
            DeleteAlternateRoom(room);
            if (room.AlternateVersion != null)
                DeleteAlternateRoom(room.AlternateVersion);
        }

        public void ApplyNewLevelSettings(LevelSettings newSettings, Action<ObjectInstance> objectChangedNotification)
        {
            LevelSettings oldSettings = Settings;
            Settings = newSettings;

            // Imported geometry
            {
                // Reuse old imported geometry objects to keep references up to date
                var oldLookup = new Dictionary<ImportedGeometry.UniqueIDType, ImportedGeometry>();
                foreach (ImportedGeometry oldImportedGeometry in oldSettings.ImportedGeometries)
                    oldLookup.Add(oldImportedGeometry.UniqueID, oldImportedGeometry);
                for (int i = 0; i < newSettings.ImportedGeometries.Count; ++i)
                {
                    ImportedGeometry newImportedGeometry = newSettings.ImportedGeometries[i];
                    ImportedGeometry oldImportedGeometry;
                    if (oldLookup.TryGetValue(newImportedGeometry.UniqueID, out oldImportedGeometry))
                    {
                        oldImportedGeometry.Assign(newImportedGeometry);
                        newSettings.ImportedGeometries[i] = oldImportedGeometry;
                        oldLookup.Remove(oldImportedGeometry.UniqueID); // The same object shouldn't be matched multiple times.
                    }
                }

                // Reset imported geometry objects if any objects are now missing
                if (oldLookup.Count != 0)
                    foreach (Room room in Rooms.Where(room => room != null))
                        foreach (var instance in room.Objects.OfType<ImportedGeometryInstance>())
                            if (oldLookup.ContainsKey(instance.Model.UniqueID))
                            {
                                instance.Model = null;
                                objectChangedNotification(instance);
                            }
            }

            // Update wads if necessary
            if (newSettings.MakeAbsolute(newSettings.WadFilePath) != oldSettings.MakeAbsolute(oldSettings.WadFilePath))
                ReloadObjectsTry();
        }
        public void ApplyNewLevelSettings(LevelSettings newSettings) => ApplyNewLevelSettings(newSettings, s => { });
    }
}
