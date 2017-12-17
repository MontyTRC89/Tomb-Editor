using NLog;
using SharpDX;
using SharpDX.Toolkit.Graphics;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using TombLib.Utils;
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
        public ScriptIdTable<IHasScriptID> GlobalScriptingIdsTable { get; } = new ScriptIdTable<IHasScriptID>();

        public static Level CreateSimpleLevel()
        {
            logger.Info("Creating new empty level");

            Level result = new Level();
            if (result.Rooms[0] == null)
                result.Rooms[0] = new Room(Room.MaxRoomDimensions, Room.MaxRoomDimensions, "Room 0");
            return result;
        }

        public HashSet<Room> GetConnectedRooms(IEnumerable<Room> startingRooms)
        {
            var result = new HashSet<Room>();
            foreach (Room startingRoom in startingRooms)
            {
                GetConnectedRoomsRecursively(result, startingRoom);
                GetConnectedRoomsRecursively(result, startingRoom.AlternateVersion);
            }
            return result;
        }

        public HashSet<Room> GetConnectedRooms(Room startingRoom)
        {
            var result = new HashSet<Room>();
            GetConnectedRoomsRecursively(result, startingRoom);
            GetConnectedRoomsRecursively(result, startingRoom.AlternateVersion);
            return result;
        }

        public List<TriggerInstance> GetAllTriggersPointingToObject(ObjectInstance instance)
        {
            var triggers = new List<TriggerInstance>();
            foreach (var room in Rooms.Where(room => room != null))
                foreach (var trigger in room.Triggers)
                    if (trigger.TargetObj == instance)
                        triggers.Add(trigger);
            return triggers;
        }

        private void GetConnectedRoomsRecursively(HashSet<Room> result, Room startingRoom)
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

        public IEnumerable<Room> GetVerticallyAscendingRoomList(Predicate<Room> checkRoom)
        {
            var editor = Editor.Instance;
            var roomList = new List<KeyValuePair<float, Room>>();
            foreach (Room room in Rooms)
                if (room != null && checkRoom(room))
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

        public void ReloadWad(IProgressReporter progressReporter)
        {
            string path = Settings.MakeAbsolute(Settings.WadFilePath);
            if (string.IsNullOrEmpty(path))
            {
                UnloadWad();
                return;
            }

            using (var wad = Wad)
            {
                var isWad2 = path.EndsWith(".wad2", StringComparison.InvariantCultureIgnoreCase);
                var newWad = new Wad2(TombRaiderVersion.TR4, !isWad2);
                try
                {
                    if (!isWad2)
                    {
                        List<string> soundPaths = new List<string>();
                        foreach (OldWadSoundPath path_ in Settings.OldWadSoundPaths)
                            soundPaths.Add(Settings.ParseVariables(path_.Path));

                        var oldWad = new Tr4Wad();
                        oldWad.LoadWad(path);
                        newWad = Tr4WadOperations.ConvertTr4Wad(oldWad, soundPaths, progressReporter);
                        if (newWad == null)
                        {
                            newWad?.Dispose();
                            return;
                        }
                    }
                    else
                    {
                        newWad = Wad2.LoadFromFile(path);
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
                ReloadWad(null);
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

        public IReadOnlyList<Room> TransformRooms(IEnumerable<Room> roomsToRotate, RectTransformation transformation, DrawingPoint center)
        {
            roomsToRotate = roomsToRotate.SelectMany(room => room.Versions).Distinct();
            Room[] oldRooms = roomsToRotate.ToArray();
            Vector3 worldCenter = new Vector3(center.X, 0, center.Y) * 1024.0f;

            // Copy rooms and sectors
            var newRooms = new Room[oldRooms.Length];
            for (int i = 0; i < oldRooms.Length; ++i)
            {
                Room oldRoom = oldRooms[i];

                // Create room
                DrawingPoint newSize = (transformation.QuadrantRotation % 2 == 0) ? oldRoom.SectorSize : new DrawingPoint(oldRoom.NumZSectors, oldRoom.NumXSectors);
                Room newRoom = oldRoom.Clone(this, obj => false); // This is a waste of computing power: All sectors are copied and immediately afterwards thrown away because the room needs to get resized.
                newRoom.Resize(this, new Rectangle(0, 0, newSize.X - 1, newSize.Y - 1));

                // Assign position
                Vector3 roomCenter = oldRoom.WorldPos + new Vector3(oldRoom.NumXSectors, 0, oldRoom.NumZSectors) * (1024.0f * 0.5f);
                roomCenter -= worldCenter;
                roomCenter = transformation.TransformVec3(roomCenter, oldRoom.NumXSectors, oldRoom.NumZSectors);
                roomCenter += worldCenter;
                newRoom.WorldPos = roomCenter - new Vector3(newSize.X, 0, newSize.Y) * (1024.0f * 0.5f);

                // Copy sectors
                for (int z = 0; z < oldRoom.NumZSectors; ++z)
                    for (int x = 0; x < oldRoom.NumXSectors; ++x)
                    {
                        DrawingPoint newSectorPosition = transformation.TransformIVec2(new DrawingPoint(x, z), oldRoom.SectorSize);
                        newRoom.Blocks[newSectorPosition.X, newSectorPosition.Y] = oldRoom.Blocks[x, z].Clone();
                        newRoom.Blocks[newSectorPosition.X, newSectorPosition.Y].Transform(transformation);
                    }

                newRooms[i] = newRoom;
            }

            // Move objects to new rooms
            for (int i = 0; i < oldRooms.Length; ++i)
            {
                Room oldRoom = oldRooms[i];
                Room newRoom = newRooms[i];

                // Copy objects
                List<ObjectInstance> objects = oldRoom.AnyObjects.ToList();
                foreach (ObjectInstance @object in objects)
                {
                    oldRoom.RemoveObjectAndSingularPortalAndKeepAlive(this, @object);
                    @object.Transform(transformation, oldRoom.SectorSize);
                    @object.TransformRoomReferences(room =>
                    {
                        int index = Array.IndexOf<Room>(oldRooms, room);
                        if (index == -1)
                            return room;
                        else
                            return newRooms[index];
                    });
                    newRoom.AddObjectAndSingularPortal(this, @object);
                }
            }

            // Assign new rooms
            for (int i = 0; i < oldRooms.Length; ++i)
            {
                if (oldRooms[i].AlternateRoom != null)
                    newRooms[i].AlternateRoom = newRooms[Array.IndexOf<Room>(oldRooms, oldRooms[i].AlternateRoom)];
                if (oldRooms[i].AlternateBaseRoom != null)
                    newRooms[i].AlternateBaseRoom = newRooms[Array.IndexOf<Room>(oldRooms, oldRooms[i].AlternateBaseRoom)];
            }
            for (int i = 0; i < oldRooms.Length; ++i)
            {
                int roomIndex = Array.IndexOf<Room>(Rooms, oldRooms[i]);
                Rooms[roomIndex] = newRooms[i];
            }

            return newRooms;
        }

        public IReadOnlyList<Room> TransformRooms(IEnumerable<Room> roomsToRotate, RectTransformation transformation)
        {
            IReadOnlyList<Room> rooms = roomsToRotate as IReadOnlyList<Room> ?? roomsToRotate.ToList();
            Rectangle coveredArea = new Rectangle(int.MaxValue, int.MaxValue, int.MinValue, int.MinValue);
            foreach (Room room in rooms)
                coveredArea = coveredArea.Union(room.WorldArea);
            return TransformRooms(rooms, transformation, new DrawingPoint((coveredArea.Left + coveredArea.Right) / 2, (coveredArea.Top + coveredArea.Bottom) / 2));
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

            // Level texture
            {
                // Reuse old level texture objects to keep references up to date
                var oldLookup = new Dictionary<LevelTexture.UniqueIDType, LevelTexture>();
                foreach (LevelTexture oldLevelTexture in oldSettings.Textures)
                    oldLookup.Add(oldLevelTexture.UniqueID, oldLevelTexture);
                for (int i = 0; i < newSettings.Textures.Count; ++i)
                {
                    LevelTexture newLevelTexture = newSettings.Textures[i];
                    LevelTexture oldLevelTexture;
                    if (oldLookup.TryGetValue(newLevelTexture.UniqueID, out oldLevelTexture))
                    {
                        oldLevelTexture.Assign(newLevelTexture);
                        newSettings.Textures[i] = oldLevelTexture;
                        oldLookup.Remove(oldLevelTexture.UniqueID); // The same object shouldn't be matched multiple times.
                    }
                }

                // Reset level texture objects if any objects are now missing
                if (oldLookup.Count != 0)
                    throw new NotImplementedException("Unfortunately we can't remove level textures safely from the level at the moment!" +
                        "However this should not be triggered currently because there is no GUI for multi texture management.");
            }

            // Update wads if necessary
            if (newSettings.MakeAbsolute(newSettings.WadFilePath) != oldSettings.MakeAbsolute(oldSettings.WadFilePath))
                ReloadObjectsTry();
        }
        public void ApplyNewLevelSettings(LevelSettings newSettings) => ApplyNewLevelSettings(newSettings, s => { });
    }
}
