using NLog;
using SharpDX.Toolkit.Graphics;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Numerics;
using TombLib.Graphics;
using TombLib.Utils;
using TombLib.Wad;
using TombLib.Wad.Tr4Wad;

namespace TombLib.LevelData
{
    public class Level : IDisposable
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        public const short MaxSectorCoord = 100;
        public const short MaxNumberOfRooms = 512;
        public Room[] Rooms { get; } = new Room[MaxNumberOfRooms]; //Rooms in level
        public Wad2 Wad { get; private set; }
        public Exception WadLoadingException { get; private set; }
        public LevelSettings Settings { get; private set; } = new LevelSettings();
        public ScriptIdTable<IHasScriptID> GlobalScriptingIdsTable { get; } = new ScriptIdTable<IHasScriptID>();

        public static Level CreateSimpleLevel()
        {
            logger.Info("Creating new empty level");

            Level result = new Level();
            if (result.Rooms[0] == null)
                result.Rooms[0] = new Room(result, Room.MaxRoomDimensions, Room.MaxRoomDimensions, result.Settings.DefaultAmbientLight, "Room 0");
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
                    if (trigger.IsPointingTo(instance))
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
            Wad?.Dispose();
            Wad = null;
            WadLoadingException = null;
        }

        public void ReloadWad() => ReloadWad(new ProgressReporterSimple());
        public void ReloadWad(IDialogHandler progressReporter)
        {
            string path = Settings.MakeAbsolute(Settings.WadFilePath);
            if (string.IsNullOrEmpty(path))
            {
                logger.Info("Reseting wad");
                Wad?.Dispose();
                Wad = null;
                WadLoadingException = null;
                return;
            }

            Wad2 newWad = null;
            try
            {
                newWad = Wad2.ImportFromFile(path, Settings.OldWadSoundPaths.Select(soundPath => Settings.ParseVariables(soundPath.Path)), progressReporter);
                newWad.GraphicsDevice = DeviceManager.DefaultDeviceManager.Device;
                newWad.PrepareDataForDirectX();
            }
            catch (Exception exc)
            {
                logger.Error(exc, "Loading *.wad failed.");
                newWad?.Dispose();
                Wad?.Dispose();
                Wad = null;
                WadLoadingException = exc;
                return;
            }
            Wad?.Dispose();
            Wad = newWad;
            WadLoadingException = null;
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
                    room.Delete(this);
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

        public void MergeFrom(Level otherLevel, bool unifyData, Action<LevelSettings> applyLevelSettings = null)
        {
            IReadOnlyList<ObjectInstance> oldObjects = Rooms.Where(room => room != null).SelectMany(room => room.AnyObjects).ToList();
            IReadOnlyList <Room> otherRooms = otherLevel.Rooms.Where(room => room != null).ToList();

            // Merge rooms
            for (int i = 0; i < otherRooms.Count; ++i)
                try
                {
                    AssignRoomToFree(otherRooms[i]);
                }
                catch
                { // If we fail, roll back the changes...
                    while (i > 0)
                        DeleteRoom(otherRooms[--i]);
                    throw;
                }

            // Merge dependencies like imported geometries, textures (and wads in the future?)
            LevelSettings newSettings = applyLevelSettings == null ? Settings : Settings.Clone();
            newSettings.Textures = Settings.Textures.ToList(); // Make sure the same references are used.
            newSettings.ImportedGeometries = Settings.ImportedGeometries.ToList();
            var copyInstance = new Room.CopyDependentLevelSettingsArgs(oldObjects, newSettings, otherLevel.Settings, unifyData);
            foreach (Room room in otherRooms)
                room.CopyDependentLevelSettings(copyInstance);
            applyLevelSettings?.Invoke(newSettings);
            GlobalScriptingIdsTable.MergeFrom(otherLevel.GlobalScriptingIdsTable, @object => @object.ScriptId = null);
            foreach (Room room in otherRooms)
                room.Level = this;
        }

        public IReadOnlyList<Room> TransformRooms(IEnumerable<Room> roomsToRotate, RectTransformation transformation, VectorInt2 center)
        {
            roomsToRotate = roomsToRotate.SelectMany(room => room.Versions).Distinct();
            Room[] oldRooms = roomsToRotate.ToArray();
            var worldCenter = new VectorInt3(center.X, 0, center.Y) * 1024;

            // Copy rooms and sectors
            var newRooms = new Room[oldRooms.Length];
            for (int i = 0; i < oldRooms.Length; ++i)
            {
                var oldRoom = oldRooms[i];

                // Create room
                var newSize = (transformation.QuadrantRotation % 2 == 0) ? oldRoom.SectorSize : new VectorInt2(oldRoom.NumZSectors, oldRoom.NumXSectors);
                var newRoom = oldRoom.Clone(this, obj => false); // This is a waste of computing power: All sectors are copied and immediately afterwards thrown away because the room needs to get resized.
                newRoom.Resize(this, new RectangleInt2(0, 0, newSize.X - 1, newSize.Y - 1));

                // Assign position
                var roomCenter = oldRoom.WorldPos + new VectorInt3(oldRoom.NumXSectors, 0, oldRoom.NumZSectors) * 512;
                roomCenter -= worldCenter;
                roomCenter = transformation.TransformVecInt3(roomCenter, oldRoom.NumXSectors, oldRoom.NumZSectors);
                roomCenter += worldCenter;
                newRoom.WorldPos = roomCenter - new VectorInt3(newSize.X, 0, newSize.Y) * 512;

                // Copy sectors
                for (int z = 0; z < oldRoom.NumZSectors; ++z)
                    for (int x = 0; x < oldRoom.NumXSectors; ++x)
                    {
                        VectorInt2 newSectorPosition = transformation.Transform(new VectorInt2(x, z), oldRoom.SectorSize);
                        newRoom.Blocks[newSectorPosition.X, newSectorPosition.Y] = oldRoom.Blocks[x, z].Clone();
                        newRoom.Blocks[newSectorPosition.X, newSectorPosition.Y].Transform(transformation, null,
                            (oldFace) => oldRoom.GetFaceShape(x, z, oldFace));
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
            RectangleInt2 coveredArea = new RectangleInt2(int.MaxValue, int.MaxValue, int.MinValue, int.MinValue);
            foreach (Room room in rooms)
                coveredArea = coveredArea.Union(room.WorldArea);
            return TransformRooms(rooms, transformation, new VectorInt2((coveredArea.X0 + coveredArea.X1) / 2, (coveredArea.Y0 + coveredArea.Y1) / 2));
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
                ReloadWad();
        }
        public void ApplyNewLevelSettings(LevelSettings newSettings) => ApplyNewLevelSettings(newSettings, s => { });
    }
}
