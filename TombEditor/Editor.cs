using System;
using System.Collections.Generic;
using System.Linq;
using SharpDX;
using TombEditor.Geometry;

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
        object Object { get; }
    }

    public class Editor
    {
        public static readonly System.Drawing.Color ColorFloor = System.Drawing.Color.FromArgb(0, 190, 190);
        public static readonly System.Drawing.Color ColorWall = System.Drawing.Color.FromArgb(0, 160, 0);
        public static readonly System.Drawing.Color ColorTrigger = System.Drawing.Color.FromArgb(200, 0, 200);
        public static readonly System.Drawing.Color ColorMonkey = System.Drawing.Color.FromArgb(255, 100, 100);
        public static readonly System.Drawing.Color ColorBox = System.Drawing.Color.FromArgb(100, 100, 100);
        public static readonly System.Drawing.Color ColorDeath = System.Drawing.Color.FromArgb(20, 240, 20);
        public static readonly System.Drawing.Color ColorClimb = System.Drawing.Color.FromArgb(0, 100, 0);
        public static readonly System.Drawing.Color ColorNoCollision = System.Drawing.Color.FromArgb(255, 128, 0, 0);
        public static readonly System.Drawing.Color ColorNotWalkable = System.Drawing.Color.FromArgb(0, 0, 150);

        public event Action<IEditorEvent> EditorEventRaised;

        public void RaiseEvent(IEditorEvent eventObj)
        {
            EditorEventRaised?.Invoke(eventObj);
        }


        // --- State of the editor ---
        // Unfortunately implementing this pattern is slightly elaborate in C#.
        // On the positive side, this allows us to catch any state changes from all known and unknown components
        // therefore being very flexible to future components and improveing state safety by guaranteed updates.

        public struct LevelChangedEvent : IEditorProperyChangedEvent
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
                    throw new NotSupportedException("A level must currently have at least one room to be used inside the editor.");

                // Reset state that was related to the old level
                SelectedObject = null;
                ChosenItem = null;
                SelectedSectors = SectorSelection.None;
                Action = EditorAction.None;
                SelectedTexture = TextureSelection.None;

                // Delete old level after the new level is set
                using (var previousLevel = Level)
                {
                    _level = value;
                    EditorEventRaised?.Invoke(new LevelChangedEvent { Previous = previousLevel, Current = value });
                }
                RoomListChange();
                SelectedRoom = _level.Rooms.First((room) => room != null);
                CenterCamera();
                LoadedWadsChange(value.Wad);
                LoadedTexturesChange();
                LevelFileNameChange();
            }
        }
        
        public struct ActionChangedEvent : IEditorProperyChangedEvent
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

        public struct ChosenItemChangedEvent : IEditorProperyChangedEvent
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

        public struct ModeChangedEvent : IEditorProperyChangedEvent
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

        public struct SelectedRoomChangedEvent : IEditorProperyChangedEvent, IEditorRoomChangedEvent
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
                SelectedObject = null;
                var previous = _selectedRoom;
                _selectedRoom = value;
                RaiseEvent(new SelectedRoomChangedEvent { Previous = previous, Current = value });
            }
        }

        public struct SelectedObjectChangedEvent : IEditorProperyChangedEvent
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

        public struct SelectedSectorsChangedEvent : IEditorProperyChangedEvent
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

        public struct SelectedTexturesChangedEvent : IEditorProperyChangedEvent
        {
            public TextureSelection Previous { get; set; }
            public TextureSelection Current { get; set; }
        }
        private TextureSelection _selectedTexture = TextureSelection.None;
        public TextureSelection SelectedTexture
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

        public struct ConfigurationChangedEvent : IEditorProperyChangedEvent
        {
            public Configuration Previous { get; set; }
            public Configuration Current { get; set; }
        }
        private Configuration _Configuration = Configuration.LoadOrUseDefault();
        public Configuration Configuration
        {
            get { return _Configuration; }
            set
            {
                if (value == _Configuration)
                    return;
                var previous = _Configuration;
                _Configuration = value;
                RaiseEvent(new ConfigurationChangedEvent { Previous = previous, Current = value });
            }
        }

        // This is invoked if the loaded wads changed for the level.
        public struct LoadedWadsChangedEvent : IEditorEvent
        {
            public TombLib.Wad.Wad Current { get; set; }
        }
        public void LoadedWadsChange(TombLib.Wad.Wad wad)
        {
            RaiseEvent(new LoadedWadsChangedEvent { Current = wad });
        }

        // This is invoked if the loaded textures changed for the level.
        public struct LoadedTexturesChangedEvent : IEditorEvent { }
        public void LoadedTexturesChange()
        {
            RaiseEvent(new LoadedTexturesChangedEvent { });
        }
        
        // This is invoked when ever the applied textures in a room change.
        // "null" can be passed, if it is not determinable what room changed.
        public struct RoomTextureChangedEvent : IEditorRoomChangedEvent
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
        public struct RoomGeometryChangedEvent : IEditorRoomChangedEvent
        {
            public Room Room { get; set; }
        }
        public void RoomGeometryChange(Room room)
        {
            RaiseEvent(new RoomGeometryChangedEvent { Room = room });
        }

        // This is invoked when the level is saved an the file name changed.
        public struct LevelFileNameChanged : IEditorEvent { }
        public void LevelFileNameChange()
        {
            RaiseEvent(new LevelFileNameChanged { });
        }

        // This is invoked when the amount of rooms is changed. (Rooms have been added or removed)
        // "null" can be passed, if it is not determinable what room changed.
        public struct RoomListChangedEvent : IEditorEvent { } 
        public void RoomListChange()
        {
            RaiseEvent(new RoomListChangedEvent { });
        }

        // This is invoked for all changes to room flags, "Reverbration", ...
        // "null" can be passed, if it is not determinable what room changed.
        public struct RoomPropertiesChangedEvent : IEditorRoomChangedEvent
        {
            public Room Room { get; set; }
        }
        public void RoomPropertiesChange(Room room)
        {
            RaiseEvent(new RoomPropertiesChangedEvent { Room = room });
        }

        // This is invoked for all changes to sectors. (eg setting a trigger, adding a portal, setting a sector to monkey, ...)
        // "null" can be passed, if it is not determinable what room changed.
        public struct RoomSectorPropertiesChangedEvent : IEditorRoomChangedEvent
        {
            public Room Room { get; set; }
        }
        public void RoomSectorPropertiesChange(Room room)
        {
            RaiseEvent(new RoomSectorPropertiesChangedEvent { Room = room });
        }

        // This is invoked for all changes to objects. (eg changing a light, changing a movable, moving a static, ...)
        // "null" can be passed, if it is not determinable what object changed.
        public struct ObjectChangedEvent : IEditorObjectChangedEvent
        {
            public object Object { get; set; }
        }
        public void ObjectChange(object object_)
        {
            RaiseEvent(new ObjectChangedEvent { Object = object_ });
        }

        // Move the camera to the center of a specific sector.
        public struct MoveCameraToSectorEvent : IEditorCameraEvent
        {
            public DrawingPoint Sector { get; set; }
        }
        public void MoveCameraToSector(DrawingPoint sector)
        {
            RaiseEvent(new MoveCameraToSectorEvent { Sector = sector });
        }

        // Center the camera inside the current room.
        public struct CenterCameraEvent : IEditorCameraEvent {}
        public void CenterCamera()
        {
            RaiseEvent(new CenterCameraEvent { });
        }

        // Notify all components that values of the configuration have changed
        public void ConfigurationChange()
        {
            RaiseEvent(new ConfigurationChangedEvent { Previous = _Configuration, Current = _Configuration });
        }

        // Select a room and center the camera
        public void SelectRoomAndCenterCamera(Room newRoom)
        {
            SelectedRoom = newRoom;
            CenterCamera();
        }

        // Show an object by going to the room it, selecting it and centering the camera appropriately.
        public void ShowObject(ObjectInstance objectInstance)
        {
            if (SelectedRoom != objectInstance.Room)
                SelectRoomAndCenterCamera(objectInstance.Room);
            SelectedObject = objectInstance;
        }

        // Call this methode to easily change the settings of the level.
        // All required update methods will be invoked automatically.
        public void UpdateLevelSettings(LevelSettings settings)
        {
            if ((_level == null) || settings == null)
                return;
            LevelSettings oldSettings = _level.Settings;
            _level.Settings = settings;

            // Update state
            if (settings.MakeAbsolute(settings.TextureFilePath) != oldSettings.MakeAbsolute(oldSettings.TextureFilePath))
            {
                _level.ReloadTextureTry();
                LoadedTexturesChange();
            }

            if (settings.MakeAbsolute(settings.WadFilePath) != oldSettings.MakeAbsolute(oldSettings.WadFilePath))
            {
                _level.ReloadObjectsTry();
                LoadedWadsChange(_level.Wad);
            }

            if (settings.MakeAbsolute(settings.LevelFilePath) != oldSettings.MakeAbsolute(oldSettings.LevelFilePath))
                LevelFileNameChange();
        }

        // Static instance
        private static Editor _instance;

        public static Editor Instance
        {
            get
            {
                if (_instance != null)
                    return _instance;
                else
                    return _instance = new Editor();
            }
        }
    }
}
