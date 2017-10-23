using System;
using System.Collections.Generic;
using System.Linq;
using SharpDX;
using TombEditor.Geometry;
using TombLib.Utils;
using System.IO;
using System.Diagnostics;
using System.Threading;
using NLog;

namespace TombEditor
{
    public interface IEditorEvent { };

    public interface IEditorProperyChangedEvent : IEditorEvent { }

    public interface IEditorCameraEvent : IEditorEvent { }

    public interface IEditorRoomChangedEvent : IEditorEvent
    {
        Room Room { get; }
    }

    public interface IEditorObjectChangedEvent : IEditorEvent
    {
        ObjectInstance Object { get; }
    }

    public class Editor : IDisposable
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        public static readonly System.Drawing.Color ColorFloor = System.Drawing.Color.FromArgb(0, 190, 190);
        public static readonly System.Drawing.Color ColorWall = System.Drawing.Color.FromArgb(0, 160, 0);
        public static readonly System.Drawing.Color ColorTrigger = System.Drawing.Color.FromArgb(200, 0, 200);
        public static readonly System.Drawing.Color ColorMonkey = System.Drawing.Color.FromArgb(255, 100, 100);
        public static readonly System.Drawing.Color ColorBox = System.Drawing.Color.FromArgb(100, 100, 100);
        public static readonly System.Drawing.Color ColorDeath = System.Drawing.Color.FromArgb(20, 240, 20);
        public static readonly System.Drawing.Color ColorClimb = System.Drawing.Color.FromArgb(0, 100, 0);
        public static readonly System.Drawing.Color ColorNotWalkable = System.Drawing.Color.FromArgb(0, 0, 150);

        public bool UnsavedChanges = true;

        public event Action<IEditorEvent> EditorEventRaised;

        public void RaiseEvent(IEditorEvent eventObj)
        {
            EditorEventRaised?.Invoke(eventObj);
        }

        // --- State of the editor ---
        // Unfortunately implementing this pattern is slightly elaborate in C#.
        // On the positive side, this allows us to catch any state changes from all known and unknown components
        // therefore being very flexible to future components and improveing state safety by guaranteed updates.

        public class LevelChangedEvent : IEditorProperyChangedEvent
        {
            public Level Previous { get; set; }
            public Level Current { get; set; }
        }
        private Level _level;
        public Level Level
        {
            get { return _level; }
            set
            {
                if (value == _level)
                    return;

                // Validate level
                int roomCount = value.Rooms.Count((room) => room != null);
                if (roomCount <= 0)
                    value.Rooms[0] = new Room(value, Room.MaxRoomDimensions, Room.MaxRoomDimensions, "Room 0");

                // Reset state that was related to the old level
                SelectedObject = null;
                ChosenItem = null;
                SelectedSectors = SectorSelection.None;
                Action = EditorAction.None;
                SelectedTexture = TextureArea.None;

                // Delete old level after the new level is set
                using (var previousLevel = Level)
                {
                    _level = value;
                    EditorEventRaised?.Invoke(new LevelChangedEvent { Previous = previousLevel, Current = value });
                }
                RoomListChange();
                SelectedRoom = _level.Rooms.First((room) => room != null);
                ResetCamera();
                LoadedWadsChange(value.Wad);
                LoadedTexturesChange();
                LoadedImportedGeometriesChange();
                UnsavedChanges = false;
                LevelFileNameChange();
            }
        }

        public class ActionChangedEvent : IEditorProperyChangedEvent
        {
            public EditorAction Previous { get; set; }
            public EditorAction Current { get; set; }
        }
        private EditorAction _action;
        public EditorAction Action
        {
            get { return _action; }
            set
            {
                if (value == _action)
                    return;
                var previous = _action;
                _action = value;
                RaiseEvent(new ActionChangedEvent { Previous = previous, Current = value });
            }
        }

        public class ChosenItemChangedEvent : IEditorProperyChangedEvent
        {
            public ItemType? Previous { get; set; }
            public ItemType? Current { get; set; }
        }
        private ItemType? _chosenItem;
        public ItemType? ChosenItem
        {
            get { return _chosenItem; }
            set
            {
                if (value == _chosenItem)
                    return;
                var previous = _chosenItem;
                _chosenItem = value;
                RaiseEvent(new ChosenItemChangedEvent { Previous = previous, Current = value });
            }
        }

        public class ModeChangedEvent : IEditorProperyChangedEvent
        {
            public EditorMode Previous { get; set; }
            public EditorMode Current { get; set; }
        }
        private EditorMode _mode;
        public EditorMode Mode
        {
            get { return _mode; }
            set
            {
                if (value == _mode)
                    return;
                var previous = _mode;
                _mode = value;
                RaiseEvent(new ModeChangedEvent { Previous = previous, Current = value });
            }
        }

        public class SelectedRoomChangedEvent : IEditorProperyChangedEvent, IEditorRoomChangedEvent
        {
            public Room Previous { get; set; }
            public Room Current { get; set; }
            Room IEditorRoomChangedEvent.Room => Current;
        }
        private Room _selectedRoom;
        public Room SelectedRoom
        {
            get { return _selectedRoom; }
            set
            {
                if (value == _selectedRoom)
                    return;
                SelectedSectors = SectorSelection.None;
                var previous = _selectedRoom;
                _selectedRoom = value;
                RaiseEvent(new SelectedRoomChangedEvent { Previous = previous, Current = value });
            }
        }

        public bool IsSelectedRoomEvent(IEditorEvent eventObj)
        {
            if (eventObj == null)
                return false;
            var roomEvent = eventObj as IEditorRoomChangedEvent;
            if (roomEvent == null)
                return true;
            return (SelectedRoom != null) && (roomEvent.Room == SelectedRoom);
        }

        public class SelectedObjectChangedEvent : IEditorProperyChangedEvent
        {
            public ObjectInstance Previous { get; set; }
            public ObjectInstance Current { get; set; }
        }
        private ObjectInstance _selectedObject;
        public ObjectInstance SelectedObject
        {
            get { return _selectedObject; }
            set
            {
                if (value == _selectedObject)
                    return;
                var previous = _selectedObject;
                _selectedObject = value;
                RaiseEvent(new SelectedObjectChangedEvent { Previous = previous, Current = value });
            }
        }

        public class SelectedSectorsChangedEvent : IEditorProperyChangedEvent
        {
            public SectorSelection Previous { get; set; }
            public SectorSelection Current { get; set; }
        }
        private SectorSelection _selectedSectors = SectorSelection.None;
        public SectorSelection SelectedSectors
        {
            get { return _selectedSectors; }
            set
            {
                if (value == _selectedSectors)
                    return;
                var previous = _selectedSectors;
                _selectedSectors = value;
                RaiseEvent(new SelectedSectorsChangedEvent { Previous = previous, Current = value });
            }
        }

        public class SelectedTexturesChangedEvent : IEditorProperyChangedEvent
        {
            public TextureArea Previous { get; set; }
            public TextureArea Current { get; set; }
        }
        private TextureArea _selectedTexture = TextureArea.None;
        public TextureArea SelectedTexture
        {
            get { return _selectedTexture; }
            set
            {
                if (value == _selectedTexture)
                    return;
                var previous = _selectedTexture;
                _selectedTexture = value;
                RaiseEvent(new SelectedTexturesChangedEvent { Previous = previous, Current = value });
            }
        }

        public class ConfigurationChangedEvent : IEditorProperyChangedEvent
        {
            public Configuration Previous { get; set; }
            public Configuration Current { get; set; }
        }
        private Configuration _Configuration;
        public Configuration Configuration
        {
            get { return _Configuration; }
            set
            {
                if (value == _Configuration)
                    return;
                var previous = _Configuration;
                _Configuration = value;
                OnConfigurationChanged(previous, value);
                RaiseEvent(new ConfigurationChangedEvent { Previous = previous, Current = value });
            }
        }

        // This is invoked if the loaded wads changed for the level.
        public class LoadedWadsChangedEvent : IEditorEvent
        {
            public TombLib.Wad.Wad2 Current { get; set; }
        }
        public void LoadedWadsChange(TombLib.Wad.Wad2 wad)
        {
            RaiseEvent(new LoadedWadsChangedEvent { Current = wad });
        }

        // This is invoked if the loaded textures changed for the level.
        public class LoadedTexturesChangedEvent : IEditorEvent { }
        public void LoadedTexturesChange()
        {
            RaiseEvent(new LoadedTexturesChangedEvent { });
        }

        // This is invoked if the loaded imported geometries changed for the level.
        public class LoadedImportedGeometriesChangedEvent : IEditorEvent { }
        public void LoadedImportedGeometriesChange()
        {
            RaiseEvent(new LoadedImportedGeometriesChangedEvent { });
        }

        // This is invoked when ever the applied textures in a room change.
        // "null" can be passed, if it is not determinable what room changed.
        public class RoomTextureChangedEvent : IEditorRoomChangedEvent
        {
            public Room Room { get; set; }
        }
        public void RoomTextureChange(Room room)
        {
            RaiseEvent(new RoomTextureChangedEvent { Room = room });
        }

        // This is invoked when ever the geometry of the room changed. (eg the room is moved, individual sectors are moved up or down, ...)
        // This is not invoked when other the properties of the room change
        // Textures, room properties like reverbration, objects changed, ...
        public class RoomGeometryChangedEvent : IEditorRoomChangedEvent
        {
            public Room Room { get; set; }
        }
        public void RoomGeometryChange(Room room)
        {
            RaiseEvent(new RoomGeometryChangedEvent { Room = room });
        }

        // This is invoked when the level is saved an the file name changed.
        public class LevelFileNameChangedEvent : IEditorEvent { }
        public void LevelFileNameChange()
        {
            RaiseEvent(new LevelFileNameChangedEvent { });
        }

        // This is invoked when any changes are made to level that needs to be saved.
        public class LevelNeedsToBeSavedEvent : IEditorEvent { }
        public void LevelNeedsToBeSaved()
        {
            RaiseEvent(new LevelNeedsToBeSavedEvent { });
        }

        // This is invoked when the amount of rooms is changed. (Rooms have been added or removed)
        // "null" can be passed, if it is not determinable what room changed.
        public class RoomListChangedEvent : IEditorEvent { }
        public void RoomListChange()
        {
            RaiseEvent(new RoomListChangedEvent { });
        }

        // This is invoked for all changes to room flags, "Reverbration", ...
        // "null" can be passed, if it is not determinable what room changed.
        public class RoomPropertiesChangedEvent : IEditorRoomChangedEvent
        {
            public Room Room { get; set; }
        }
        public void RoomPropertiesChange(Room room)
        {
            RaiseEvent(new RoomPropertiesChangedEvent { Room = room });
        }

        // This is invoked for all changes to sectors. (eg setting a trigger, adding a portal, setting a sector to monkey, ...)
        // "null" can be passed, if it is not determinable what room changed.
        public class RoomSectorPropertiesChangedEvent : IEditorRoomChangedEvent
        {
            public Room Room { get; set; }
        }
        public void RoomSectorPropertiesChange(Room room)
        {
            RaiseEvent(new RoomSectorPropertiesChangedEvent { Room = room });
        }

        // This is invoked for all changes to objects. (eg changing a light, changing a movable, moving a static, ...)
        // "null" can be passed, if it is not determinable what object changed.
        public class ObjectChangedEvent : IEditorObjectChangedEvent
        {
            public ObjectInstance Object { get; set; }
        }
        public void ObjectChange(ObjectInstance object_)
        {
            RaiseEvent(new ObjectChangedEvent { Object = object_ });
        }

        // Move the camera to the center of a specific sector.
        public class MoveCameraToSectorEvent : IEditorCameraEvent
        {
            public DrawingPoint Sector { get; set; }
        }
        public void MoveCameraToSector(DrawingPoint sector)
        {
            RaiseEvent(new MoveCameraToSectorEvent { Sector = sector });
        }

        // Center the camera inside the current room.
        public class ResetCameraEvent : IEditorCameraEvent { }
        public void ResetCamera()
        {
            RaiseEvent(new ResetCameraEvent { });
        }

        // Select a texture and center the view
        public class SelectTextureAndCenterViewEvent : IEditorEvent
        {
            public TextureArea Texture { get; set; }
        }
        public void SelectTextureAndCenterView(TextureArea texture)
        {
            RaiseEvent(new SelectTextureAndCenterViewEvent { Texture = texture });
        }

        // Notify all components that values of the configuration have changed
        public void ConfigurationChange()
        {
            OnConfigurationChanged(_Configuration, _Configuration);
            RaiseEvent(new ConfigurationChangedEvent { Previous = _Configuration, Current = _Configuration });
        }

        // Select a room and center the camera
        public void SelectRoomAndResetCamera(Room newRoom)
        {
            if (SelectedRoom == newRoom)
                return;

            SelectedRoom = newRoom;
            ResetCamera();
        }

        // Show an object by going to the room it, selecting it and centering the camera appropriately.
        public void ShowObject(ObjectInstance objectInstance)
        {
            if (SelectedRoom != objectInstance.Room)
                SelectRoomAndResetCamera(objectInstance.Room);
            SelectedObject = objectInstance;
        }

        // Call this methode to easily change the settings of the level.
        // All required update methods will be invoked automatically.
        public void UpdateLevelSettings(LevelSettings newSettings)
        {
            if ((_level == null) || newSettings == null)
                return;

            // Determine what will change when the new settings are applied
            // This has to be done now, because the old state will be lost after the new settings are applied
            bool importedGeometryChanged = !ImportedGeometry.AreListsEqual(newSettings.ImportedGeometries, _level.Settings.ImportedGeometries);
            bool texturesChanged = !LevelTexture.AreListsEqual(newSettings.Textures, _level.Settings.Textures);
            bool wadsChanged = newSettings.MakeAbsolute(newSettings.WadFilePath) != _level.Settings.MakeAbsolute(_level.Settings.WadFilePath);
            bool levelFilenameChanged = newSettings.MakeAbsolute(newSettings.LevelFilePath) != _level.Settings.MakeAbsolute(_level.Settings.LevelFilePath);

            // Update the current settings
            _level.ApplyNewLevelSettings(newSettings, (instance) => ObjectChange(instance));

            // Update state
            if (importedGeometryChanged)
                LoadedImportedGeometriesChange();

            if (texturesChanged)
                LoadedTexturesChange();

            if (wadsChanged)
                LoadedWadsChange(_level.Wad);

            if (levelFilenameChanged)
                LevelFileNameChange();
        }

        // Configuration
        FileSystemWatcher configurationWatcher = null;
        bool configurationIsLoadedFromFile = false;
        private void ConfigurationWatcher_Changed(object sender, FileSystemEventArgs e)
        {
            if (Path.GetFullPath(e.FullPath) == Path.GetFullPath(Configuration.FilePath))
            {
                Configuration configuration = Configuration;
                if (!Utils.RetryFor(500, () => configuration = Configuration.Load(Configuration.FilePath)))
                    logger.Warn("Unable to load configuration from '" + Path.GetFullPath(Configuration.FilePath) + "' after it changed.");

                // Update configuration
                SynchronizationContext.Post(o =>
                {
                    try
                    {
                        configurationIsLoadedFromFile = true; // Don't save the configuration again just yet
                        Configuration = configuration; }
                    finally
                    {
                        configurationIsLoadedFromFile = false;
                    }
                }, null);
            }
        }

        private void OnConfigurationChanged(Configuration previous, Configuration current)
        {
            if (!string.Equals(previous?.FilePath ?? "", current?.FilePath ?? "", StringComparison.InvariantCultureIgnoreCase))
            {
                configurationWatcher?.Dispose();
                if (!string.IsNullOrEmpty(current?.FilePath))
                {
                    configurationWatcher = new FileSystemWatcher(Path.GetDirectoryName(current.FilePath), Path.GetFileName(current.FilePath));
                    configurationWatcher.EnableRaisingEvents = true;
                    configurationWatcher.Created += ConfigurationWatcher_Changed;
                    configurationWatcher.Deleted += ConfigurationWatcher_Changed;
                    configurationWatcher.Renamed += ConfigurationWatcher_Changed;
                    configurationWatcher.Changed += ConfigurationWatcher_Changed;
                }
            }
            if (!configurationIsLoadedFromFile)
                current?.SaveTry();
        }

        public void Dispose()
        {
            configurationWatcher?.Dispose();
            Level?.Dispose();
        }

        // Construction
        public SynchronizationContext SynchronizationContext { get; }
        public Editor(SynchronizationContext synchronizationContext, Configuration configuration, Level level)
        {
            if (synchronizationContext == null)
                throw new ArgumentNullException("synchronizationContext");
            SynchronizationContext = synchronizationContext;
            Configuration = configuration;
            Level = level;
        }

        public Editor(SynchronizationContext synchronizationContext,  Configuration configuration)
            : this(synchronizationContext, configuration, Level.CreateSimpleLevel())
        { }

        public static Editor Instance;
    }
}
