using NLog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using TombLib;
using TombLib.Forms;
using TombLib.LevelData;
using TombLib.LevelData.IO;
using TombLib.Rendering;
using TombLib.Utils;

namespace TombEditor
{
    public interface IEditorEvent { }

    public interface IEditorPropertyChangedEvent : IEditorEvent { }

    public interface IEditorCameraEvent : IEditorEvent { }

    public interface IEditorEventCausesUnsavedChanges : IEditorEvent { }

    public interface IEditorRoomChangedEvent : IEditorEventCausesUnsavedChanges
    {
        Room Room { get; }
    }

    public enum ObjectChangeType
    {
        Add,
        Remove,
        Change
    }

    public enum LastSelectionType
    {
        None,
        Block,
        SpatialObject
    }

    public interface IEditorObjectChangedEvent : IEditorEventCausesUnsavedChanges
    {
        Room Room { get; }
        ObjectInstance Object { get; }
        ObjectChangeType ChangeType { get; }
    }

    public class Editor : IDisposable
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        public event Action<IEditorEvent> EditorEventRaised;

        public void RaiseEvent(IEditorEvent eventObj)
        {
            SynchronizationContext.Send(eventObj_ => EditorEventRaised?.Invoke((IEditorEvent)eventObj_), eventObj);
        }

        // --- State of the editor ---
        // Unfortunately implementing this pattern is slightly elaborate in C#.
        // On the positive side, this allows us to catch any state changes from all known and unknown components
        // therefore being very flexible to future components and improveing state safety by guaranteed updates.

        public class LevelChangedEvent : IEditorPropertyChangedEvent
        {
            public Level Previous { get; internal set; }
            public Level Current { get; internal set; }
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
                int roomCount = value.Rooms.Count(room => room != null);
                if (roomCount <= 0)
                    value.Rooms[0] = new Room(value, Room.DefaultRoomDimensions, Room.DefaultRoomDimensions,
                                              _level.Settings.DefaultAmbientLight, "Room 0");

                // Reset state that was related to the old level
                _levelSettingsWatcher?.StopReloading();
                SelectedObject = null;
                ChosenItem = null;
                ChosenImportedGeometry = null;
                SelectedSectors = SectorSelection.None;
                Action = null;
                SelectedTexture = TextureArea.None;
                UndoManager?.ClearAll();

                // Delete old level after the new level is set
                var previousLevel = Level;
                _level = value;
                EditorEventRaised?.Invoke(new LevelChangedEvent { Previous = previousLevel, Current = value });
                RoomListChange();
                SelectLastOrDefaultRoom();
                ResetCamera(true);
                LoadedWadsChange(false);
                LoadedSoundsCatalogsChange(false);
                LoadedTexturesChange(null, false);
                LoadedImportedGeometriesChange(false);
                LevelFileNameChange();

                // Start watching for file changes
                _levelSettingsWatcher?.WatchLevelSettings(_level.Settings);
                _levelSettingsWatcher?.RestartReloading();

                // Reset unsaved changes flag (needed to be last cause certain events override it)
                HasUnsavedChanges = false;
            }
        }

        public class ActionChangedEvent : IEditorPropertyChangedEvent
        {
            public IEditorAction Previous { get; internal set; }
            public IEditorAction Current { get; internal set; }
        }
        private IEditorAction _action;
        public IEditorAction Action
        {
            get { return _action; }
            set
            {
                if (value == _action)
                    return;
                if (value != null && _action != null && value.Equals(_action))
                    return;
                var previous = _action;
                _action = value;
                RaiseEvent(new ActionChangedEvent { Previous = previous, Current = value });
            }
        }

        public class ChosenItemChangedEvent : IEditorPropertyChangedEvent
        {
            public ItemType? Previous { get; internal set; }
            public ItemType? Current { get; internal set; }
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

        public class ChosenImportedGeometryChangedEvent : IEditorPropertyChangedEvent
        {
            public ImportedGeometry Previous { get; internal set; }
            public ImportedGeometry Current { get; internal set; }
        }
        private ImportedGeometry _chosenImportedGeometry = null;
        public ImportedGeometry ChosenImportedGeometry
        {
            get { return _chosenImportedGeometry; }
            set
            {
                if (value == _chosenImportedGeometry)
                    return;
                var previous = _chosenImportedGeometry;
                _chosenImportedGeometry = value;
                RaiseEvent(new ChosenImportedGeometryChangedEvent { Previous = previous, Current = value });
            }
        }

        public class ModeChangedEvent : IEditorPropertyChangedEvent
        {
            public EditorMode Previous { get; internal set; }
            public EditorMode Current { get; internal set; }
        }
        private EditorMode _mode = EditorMode.Geometry;
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

        public class ToolChangedEvent : IEditorPropertyChangedEvent
        {
            public EditorTool Previous { get; internal set; }
            public EditorTool Current { get; internal set; }
        }
        private EditorTool _tool = new EditorTool();
        public EditorTool Tool
        {
            get { return _tool; }
            set
            {
                if (value.Tool == _tool.Tool && value.GridSize == _tool.GridSize && value.TextureUVFixer == _tool.TextureUVFixer)
                    return;
                var previous = _tool;
                _tool = value;
                RaiseEvent(new ToolChangedEvent { Previous = previous, Current = value });
            }
        }
        private EditorTool _lastGeometryTool = new EditorTool();
        private EditorTool _lastFaceEditTool = new EditorTool();

        public LastSelectionType LastSelection = LastSelectionType.None;

        public class SelectedRoomsChangedEvent : IEditorPropertyChangedEvent
        {
            public IReadOnlyList<Room> Previous { get; internal set; }
            public IReadOnlyList<Room> Current { get; internal set; }
        }
        private Room[] _selectedRooms;
        public IReadOnlyList<Room> SelectedRooms
        {
            get { return _selectedRooms; }
            set
            {
                if (_selectedRooms != null && _selectedRooms.SequenceEqual(value))
                    return;
                if (value.Count <= 0)
                    throw new ArgumentException("The selected room list must contain at least 1 room.");
                if (value.Any(room => room == null))
                    throw new ArgumentNullException(nameof(value), "The selected room list may not contain null.");
                var roomSet = new HashSet<Room>(new Room[] { null });
                if (value.Any(room => !roomSet.Add(room)))
                    throw new ArgumentNullException(nameof(value), "The selected room list may not contain duplicates.");

                // Backup first selected room index into previous room variable.
                // Using TRTomb's SelectedRoomChangedEvent is unreliable, because directly referencing Room
                // object may break if a couple of rooms were deleted in a row in 2D map.

                if (Level != null && Level.Rooms != null && _selectedRooms != null)
                {
                    _previousRoom = Array.FindIndex(Level.Rooms, item => item == _selectedRooms[0]);
                    if (_previousRoom == -1) _previousRoom = 0; // Always jump to first room in case something went wrong
                }
                else
                    _previousRoom = 0; // Always jump to first room in case something went wrong

                var previous = _selectedRooms;
                _selectedRooms = value.ToArray();
                if (previous == null || previous[0] != _selectedRooms[0])
                    RaiseEvent(new SelectedRoomChangedEvent(previous, value));
                else
                    RaiseEvent(new SelectedRoomsChangedEvent { Previous = previous, Current = value });

                // Reset sector selection
                SelectedSectors = SectorSelection.None;

                // Keep last selected room index in level settings
                Level.Settings.LastSelectedRoom = Array.FindIndex(Level.Rooms, item => item == _selectedRooms[0]);
            }
        }
        public bool SelectedRoomsContains(Room room) => Array.IndexOf(_selectedRooms, room) != -1;

        public class SelectedRoomChangedEvent : SelectedRoomsChangedEvent
        {
            public new Room Previous => base.Previous[0];
            public new Room Current => base.Current[0];
            internal SelectedRoomChangedEvent(IReadOnlyList<Room> previous, IReadOnlyList<Room> current)
            {
                base.Current = current;
                base.Previous = previous;
            }
        }

        public Room SelectedRoom
        {
            get { return _selectedRooms[0]; }
            set
            {
                if (value == _selectedRooms[0])
                    return;
                SelectedRooms = new[] { value };
            }
        }

        public Room PreviousRoom { get { return Level?.Rooms?[_previousRoom] ?? null; } }
        private int _previousRoom;

        public bool IsSelectedRoomEvent(IEditorEvent eventObj)
        {
            if (eventObj == null)
                return false;
            var roomEvent = eventObj as IEditorRoomChangedEvent;
            if (roomEvent == null)
                return true;
            return SelectedRoom != null && roomEvent.Room == SelectedRoom;
        }

        public class SelectedObjectChangedEvent : IEditorPropertyChangedEvent
        {
            public ObjectInstance Previous { get; internal set; }
            public ObjectInstance Current { get; internal set; }
        }
        private ObjectInstance _selectedObject;
        public ObjectInstance SelectedObject
        {
            get { return _selectedObject; }
            set
            {
                // Check that selected object is a valid choice
                if (value != null)
                {
                    if (value.Room == null)
                        throw new ArgumentException("The object to be selected is not inside a room.");
                    if (Array.IndexOf(Level.Rooms, value.Room) == -1)
                        throw new ArgumentException("The object to be selected is not part of the level.");
                }

                if (value != null && value is ISpatial)
                    LastSelection = LastSelectionType.SpatialObject;

                if (value == _selectedObject)
                    return;

                var previous = _selectedObject;
                _selectedObject = value;
                RaiseEvent(new SelectedObjectChangedEvent { Previous = previous, Current = value });
            }
        }

        public class SelectedSectorsChangedEvent : IEditorPropertyChangedEvent
        {
            public SectorSelection Previous { get; internal set; }
            public SectorSelection Current { get; internal set; }
        }
        private SectorSelection _selectedSectors = SectorSelection.None;
        public SectorSelection SelectedSectors
        {
            get { return _selectedSectors; }
            set
            {
                if (value != SectorSelection.None)
                    LastSelection = LastSelectionType.Block;

                if (value == _selectedSectors)
                    return;
                var previous = _selectedSectors;
                _selectedSectors = value;
                RaiseEvent(new SelectedSectorsChangedEvent { Previous = previous, Current = value });
            }
        }

        public class HighlightedSectorChangedEvent : IEditorPropertyChangedEvent
        {
            public SectorSelection Previous { get; internal set; }
            public SectorSelection Current { get; internal set; }
        }
        private SectorSelection _highlightedSectors = SectorSelection.None;
        public SectorSelection HighlightedSectors
        {
            get { return _highlightedSectors; }
            set
            {
                if (value == _highlightedSectors)
                    return;
                var previous = _highlightedSectors;
                _highlightedSectors = value;
                RaiseEvent(new HighlightedSectorChangedEvent { Previous = previous, Current = value });
            }
        }

        public class SelectedTexturesChangedEvent : IEditorPropertyChangedEvent
        {
            public TextureArea Previous { get; internal set; }
            public TextureArea Current { get; internal set; }
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

        public class ConfigurationChangedEvent : IEditorPropertyChangedEvent
        {
            public bool UpdateLayout { get; internal set; } = false;
            public bool UpdateKeyboardShortcuts { get; internal set; } = false;
            public bool UpdateToolbarLayout { get; internal set; } = false;
            public bool Save { get; internal set; } = false;
        }
        private Configuration _configuration;
        public Configuration Configuration
        {
            get { return _configuration; }
            set
            {
                if (value == _configuration)
                    return;
                var previous = _configuration;
                _configuration = value;
                RaiseEvent(new ConfigurationChangedEvent { UpdateLayout = true, UpdateKeyboardShortcuts = true, Save = true });
            }
        }

        public class BookmarkedObjectChanged : IEditorPropertyChangedEvent
        {
            public ObjectInstance Previous { get; internal set; }
            public ObjectInstance Current { get; internal set; }
        }
        private ObjectInstance _bookmarkedObject = null;
        public ObjectInstance BookmarkedObject
        {
            get
            {
                // Check that it's still part of the project
                if (Level == null)
                    return null;
                if (!Level.Rooms.Where(room => room != null).SelectMany(room => room.AnyObjects).Contains(_bookmarkedObject))
                    return null;
                return _bookmarkedObject;
            }
            set
            {
                if (value == _bookmarkedObject)
                    return;
                var previous = _bookmarkedObject;
                _bookmarkedObject = value;
                RaiseEvent(new BookmarkedObjectChanged { Previous = previous, Current = value });
            }
        }

        public class HasUnsavedChangesChangedEvent : IEditorPropertyChangedEvent
        {
            public bool Previous { get; internal set; }
            public bool Current { get; internal set; }
        }

        private bool _hasUnsavedChanges;
        public bool HasUnsavedChanges
        {
            get { return _hasUnsavedChanges; }
            set
            {
                if (value == _hasUnsavedChanges)
                    return;
                var previous = _hasUnsavedChanges;
                _hasUnsavedChanges = value;
                RaiseEvent(new HasUnsavedChangesChangedEvent { Previous = previous, Current = value });
            }
        }

        // This is invoked if the loaded wads changed for the level.
        public interface IUpdateLevelSettingsFileWatcher { bool UpdateLevelSettingsFileWatcher { get; set; } }
        public class LoadedWadsChangedEvent : IEditorEventCausesUnsavedChanges, IUpdateLevelSettingsFileWatcher { public bool UpdateLevelSettingsFileWatcher { get; set; } }
        public void LoadedWadsChange(bool updateLevelSettingsFileWatcher = true)
        {
            RaiseEvent(new LoadedWadsChangedEvent { UpdateLevelSettingsFileWatcher = updateLevelSettingsFileWatcher });
        }

        public class LoadedSoundsCatalogsChangedEvent : IEditorEventCausesUnsavedChanges, IUpdateLevelSettingsFileWatcher { public bool UpdateLevelSettingsFileWatcher { get; set; } }
        public void LoadedSoundsCatalogsChange(bool updateLevelSettingsFileWatcher = true)
        {
            RaiseEvent(new LoadedSoundsCatalogsChangedEvent { UpdateLevelSettingsFileWatcher = updateLevelSettingsFileWatcher });
        }

        // This is invoked if the loaded textures changed for the level.
        public class LoadedTexturesChangedEvent : IEditorEventCausesUnsavedChanges, IUpdateLevelSettingsFileWatcher { public LevelTexture NewToSelect { get; set; } = null; public bool UpdateLevelSettingsFileWatcher { get; set; } }
        public void LoadedTexturesChange(LevelTexture newToSelect = null, bool updateLevelSettingsFileWatcher = true)
        {
            RaiseEvent(new LoadedTexturesChangedEvent { NewToSelect = newToSelect, UpdateLevelSettingsFileWatcher = updateLevelSettingsFileWatcher });
        }

        // This is invoked if the loaded imported geometries changed for the level.
        public class LoadedImportedGeometriesChangedEvent : IEditorEventCausesUnsavedChanges, IUpdateLevelSettingsFileWatcher { public bool UpdateLevelSettingsFileWatcher { get; set; } }
        public void LoadedImportedGeometriesChange(bool updateLevelSettingsFileWatcher = true)
        {
            RaiseEvent(new LoadedImportedGeometriesChangedEvent { UpdateLevelSettingsFileWatcher = updateLevelSettingsFileWatcher });
        }

        // This is invoked if the animated texture sets changed for the level.
        public class AnimatedTexturesChangedEvent : IEditorEventCausesUnsavedChanges { }
        public void AnimatedTexturesChange()
        {
            RaiseEvent(new AnimatedTexturesChangedEvent());
        }

        // This is invoked if merged static list was changed for the level.
        public class MergedStaticsChangedEvent : IEditorEventCausesUnsavedChanges { }
        public void MergedStaticsChange()
        {
            RaiseEvent(new MergedStaticsChangedEvent());
        }

        // This is invoked if the animated texture sets changed for the level.
        public class TextureSoundsChangedEvent : IEditorEventCausesUnsavedChanges { }
        public void TextureSoundsChange()
        {
            RaiseEvent(new TextureSoundsChangedEvent());
        }

        // This is invoked if the animated texture sets changed for the level.
        public class BumpmapsChangedEvent : IEditorEventCausesUnsavedChanges { }
        public void BumpmapsChange()
        {
            RaiseEvent(new BumpmapsChangedEvent());
        }

        // This is invoked if game version is changed for the level.
        public class GameVersionChangedEvent : IEditorEventCausesUnsavedChanges { }
        public void GameVersionChange()
        {
            RaiseEvent(new GameVersionChangedEvent());
        }

        // This is invoked after an autosave
        public class AutosaveEvent : IEditorEvent
        {
            public string FileName { get; set; }
            public Exception Exception { get; set; }
            public DateTime Time { get; set; }
        }
        public void AutoSaveCompleted(Exception exception, string fileName, DateTime time)
        {
            RaiseEvent(new AutosaveEvent { Exception = exception, FileName = fileName, Time = time });
        }

        // This is invoked when ever the applied textures in a room change.
        // "null" can be passed, if it is not determinable what room changed.
        public class RoomTextureChangedEvent : IEditorRoomChangedEvent
        {
            public Room Room { get; internal set; }
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
            public Room Room { get; internal set; }
        }
        public void RoomGeometryChange(Room room)
        {
            RaiseEvent(new RoomGeometryChangedEvent { Room = room });
        }

        // This is invoked when the level is saved an the file name changed.
        public class LevelFileNameChangedEvent : IEditorEvent { }
        public void LevelFileNameChange()
        {
            RaiseEvent(new LevelFileNameChangedEvent());
        }

        // This is invoked when the amount of rooms is changed. (Rooms have been added or removed)
        // "null" can be passed, if it is not determinable what room changed.
        public class RoomListChangedEvent : IEditorEventCausesUnsavedChanges { }
        public void RoomListChange()
        {
            RaiseEvent(new RoomListChangedEvent());
        }

        // This is invoked for all changes to room flags, "Reverbration", ...
        // "null" can be passed, if it is not determinable what room changed.
        public class RoomPropertiesChangedEvent : IEditorRoomChangedEvent
        {
            public Room Room { get; internal set; }
        }
        public void RoomPropertiesChange(Room room)
        {
            if (room == null)
                throw new ArgumentNullException();
            RaiseEvent(new RoomPropertiesChangedEvent { Room = room });
        }

        // This is invoked for all changes to sectors. (eg setting a trigger, adding a portal, setting a sector to monkey, ...)
        // "null" can be passed, if it is not determinable what room changed.
        public class RoomSectorPropertiesChangedEvent : IEditorRoomChangedEvent
        {
            public Room Room { get; internal set; }
        }
        public void RoomSectorPropertiesChange(Room room)
        {
            if (room == null)
                throw new ArgumentNullException();
            RaiseEvent(new RoomSectorPropertiesChangedEvent { Room = room });
        }

        // This is invoked for all changes to objects. (eg changing a light, changing a moveable, moving a static, ...)
        // "null" can be passed, if it is not determinable what object changed.
        public class ObjectChangedEvent : IEditorObjectChangedEvent
        {
            public Room Room { get; internal set; }
            public ObjectInstance Object { get; internal set; }
            public ObjectChangeType ChangeType { get; internal set; }
        }
        public void ObjectChange(ObjectInstance @object, ObjectChangeType changeType)
        {
            ObjectChange(@object, changeType, @object.Room);
        }
        public void ObjectChange(ObjectInstance @object, ObjectChangeType changeType, Room room)
        {
            if (room == null || @object == null)
                throw new ArgumentNullException();
            RaiseEvent(new ObjectChangedEvent { Room = room, Object = @object, ChangeType = changeType });
        }
        public void ObjectChange(IEnumerable<ObjectInstance> objects, ObjectChangeType changeType)
        {
            foreach (ObjectInstance @object in objects)
                ObjectChange(@object, changeType, @object.Room);
        }
        public void ObjectChange(IEnumerable<ObjectInstance> objects, ObjectChangeType changeType, Room room)
        {
            foreach (ObjectInstance @object in objects)
                ObjectChange(@object, changeType, room);
        }

        // Move the camera to the center of a specific sector.
        public class MoveCameraToSectorEvent : IEditorCameraEvent
        {
            public VectorInt2 Sector { get; set; }
        }
        public void MoveCameraToSector(VectorInt2 sector)
        {
            RaiseEvent(new MoveCameraToSectorEvent { Sector = sector });
        }

        // Center the camera inside the current room.
        public class ResetCameraEvent : IEditorCameraEvent
        {
            public bool NewCamera { get; set; }
        }
        public void ResetCamera(bool newCamera = false)
        {
            RaiseEvent(new ResetCameraEvent { NewCamera = newCamera });
        }

        // Toggle FlyMode
        public class ToggleFlyModeEvent : IEditorCameraEvent
        {
            public bool FlyModeState { get; set; }
        }
        public void ToggleFlyMode(bool state)
        {
            RaiseEvent(new ToggleFlyModeEvent { FlyModeState = state });
        }
        public bool FlyMode { get; set; } = false;

        // Toggle hidden selection (during color picking)
        public class HideSelectionEvent : IEditorEvent
        {
            public bool HideSelection { get; set; }
        }
        public void ToggleHiddenSelection(bool state)
        {
            HiddenSelection = state;
            RaiseEvent(new HideSelectionEvent { HideSelection = state });
        }
        public bool HiddenSelection { get; private set; } = false;
        
        // Last used palette colour
        public class LastUsedPaletteColourChangedEvent : IEditorEvent
        {
            public ColorC Colour { get; set; }
        }
        public void LastUsedPaletteColourChange(ColorC color)
        {
            LastUsedPaletteColour = color;
            RaiseEvent(new LastUsedPaletteColourChangedEvent { Colour = color });
        }
        public ColorC LastUsedPaletteColour { get; private set; } = new ColorC(128, 128, 128);

        // For resetting global palette to default
        public class ResetPaletteEvent : IEditorEvent { }

        // Select a texture and center the view
        public class SelectTextureAndCenterViewEvent : IEditorEvent
        {
            public TextureArea Texture { get; internal set; }
        }
        public void SelectTextureAndCenterView(TextureArea texture)
        {
            RaiseEvent(new SelectTextureAndCenterViewEvent { Texture = texture });
        }

        // Select different texture set
        public class SelectedLevelTextureChangedSetEvent : IEditorEvent
        {
            public LevelTexture Texture { get; internal set; }
        }
        public void SelectedLevelTextureChanged(LevelTexture texture)
        {
            RaiseEvent(new SelectedLevelTextureChangedSetEvent { Texture = texture });
        }

        // Send message
        public class MessageEvent : IEditorEvent
        {
            public string Message { get; internal set; }
            public PopupType Type { get; internal set; }
        }
        public void SendMessage(string message = "", PopupType type = PopupType.None)
        {
            RaiseEvent(new MessageEvent { Message = message, Type = type });
        }

        // Init / quit editor events
        public class InitEvent : IEditorEvent { }
        public class EditorQuitEvent : IEditorEvent { }
        public void Quit()
        {
            RaiseEvent(new EditorQuitEvent());
        }

        // Main window focus event
        public class EditorFocusedEvent : IEditorEvent { }
        public void Focus()
        {
            RaiseEvent(new EditorFocusedEvent());
        }

        // Undo-redo manager
        public EditorUndoManager UndoManager { get; private set; }

        public class UndoStackChangedEvent : IEditorEvent
        {
            public bool UndoPossible { get; set; }
            public bool RedoPossible { get; set; }
            public bool UndoReversible { get; set; }
            public bool RedoReversible { get; set; }
        }
        public void UndoStackChanged()
        {
            RaiseEvent(new UndoStackChangedEvent() { UndoPossible = UndoManager.UndoPossible, RedoPossible = UndoManager.RedoPossible,
                UndoReversible = UndoManager.UndoReversible, RedoReversible = UndoManager.UndoReversible });
        }

        // Change sector highlights
        public SectorColoringManager SectorColoringManager { get; private set; }

        public void ConfigurationChange(bool updateKeyboardShortcuts = false, bool updateLayout = false, bool updateToolbarLayout = false, bool save = false)
        {
            RaiseEvent(new ConfigurationChangedEvent
            {
                UpdateKeyboardShortcuts = updateKeyboardShortcuts,
                UpdateLayout = updateLayout,
                Save = save,
                UpdateToolbarLayout = updateToolbarLayout
            });
        }

        // Select a room and (optonally) center the camera
        public void SelectRoom(Room newRoom)
        {
            if (SelectedRoom == newRoom)
                return;
            SelectedSectors = SectorSelection.None;
            SelectedRoom = newRoom;

            if (Configuration.Rendering3D_ResetCameraOnRoomSwitch)
                ResetCamera(true);
        }

        // Select rooms
        public void SelectRooms(IEnumerable<Room> newRooms)
        {
            if (newRooms?.FirstOrDefault() != null)
                SelectedRooms = newRooms.ToList();
        }

        // Select rooms and center the camera
        public void SelectRoomsAndResetCamera(IEnumerable<Room> newRooms)
        {
            if (newRooms == null)
                return;

            Room oldRoom = SelectedRoom;
            SelectRooms(newRooms);
            Room newRoom = SelectedRoom;
            if (oldRoom != newRoom)
                ResetCamera(true);
        }

        // Select last used room or first available room if not available
        private void SelectLastOrDefaultRoom()
        {
            if (_level.Rooms[_level.Settings.LastSelectedRoom] != null)
                SelectedRooms = new[] { _level.Rooms[_level.Settings.LastSelectedRoom] };
            else
                SelectedRooms = new[] { _level.Rooms.First(room => room != null) };
        }

        // Show an object by going to the room it, selecting it and centering the camera appropriately.
        public void ShowObject(ObjectInstance objectInstance)
        {
            if (SelectedRoom != objectInstance.Room)
                SelectRoom(objectInstance.Room);
            SelectedObject = objectInstance;
        }

        // Call this methode to easily change the settings of the level.
        // All required update methods will be invoked automatically.
        public void UpdateLevelSettings(LevelSettings newSettings)
        {
            if (_level == null || newSettings == null)
                return;

            // Determine what will change when the new settings are applied
            // This has to be done now, because the old state will be lost after the new settings are applied
            bool importedGeometryChanged = !newSettings.ImportedGeometries.SequenceEqual(_level.Settings.ImportedGeometries);
            bool texturesChanged = !newSettings.Textures.SequenceEqual(_level.Settings.Textures);
            bool wadsChanged = !newSettings.Wads.SequenceEqual(_level.Settings.Wads);
            bool soundsChanged = !newSettings.SoundsCatalogs.SequenceEqual(_level.Settings.SoundsCatalogs);
            bool animatedTexturesChanged = !newSettings.AnimatedTextureSets.SequenceEqual(_level.Settings.AnimatedTextureSets);
            bool mergedStaticsChanged = !newSettings.AutoStaticMeshMerges.SequenceEqual(_level.Settings.AutoStaticMeshMerges);
            bool levelFilenameChanged = newSettings.MakeAbsolute(newSettings.LevelFilePath) != _level.Settings.MakeAbsolute(_level.Settings.LevelFilePath);
            bool gameVersionChanged = newSettings.GameVersion != _level.Settings.GameVersion;

            // Update the current settings
            _level.ApplyNewLevelSettings(newSettings, instance => ObjectChange(instance, ObjectChangeType.Change));

            // Update state
            if (importedGeometryChanged)
                LoadedImportedGeometriesChange(false);

            if (texturesChanged)
                LoadedTexturesChange(null, false);

            if (wadsChanged)
                LoadedWadsChange(false);

            if (soundsChanged)
                LoadedSoundsCatalogsChange(false);

            if (animatedTexturesChanged)
                AnimatedTexturesChange();

            if (mergedStaticsChanged)
                MergedStaticsChange();

            if (levelFilenameChanged)
                LevelFileNameChange();

            if (gameVersionChanged)
                GameVersionChange();

            // Update file watchers
            if (importedGeometryChanged || texturesChanged || wadsChanged || soundsChanged)
                _levelSettingsWatcher?.WatchLevelSettings(_level.Settings);
        }

        // Configuration
        private LevelSettingsWatcher _levelSettingsWatcher;
        private FileSystemWatcherManager _configurationWatcher;
        private bool _configurationIsLoadedFromFile = false;

        private class ConfigurationWatchedObj : FileSystemWatcherManager.WatchedObj
        {
            public Editor Parent;
            public override IEnumerable<string> Directories => null;
            public override IEnumerable<string> Files { get { return new[] { Parent.Configuration.GetDefaultPath() }; } }

            public override string Name => "Configuration";
            public override bool IsRepresentingSameObject(FileSystemWatcherManager.WatchedObj other) => other is ConfigurationWatchedObj;
            public override void TryReload(FileSystemWatcherManager sender, FileSystemWatcherManager.ReloadArgs e)
            {
                Configuration configuration = Parent.Configuration.Load<Configuration>(Parent.Configuration.GetDefaultPath());

                // Update configuration
                Parent.SynchronizationContext.Send(o =>
                {
                    try
                    {
                        Parent._configurationIsLoadedFromFile = true; // Don't save the configuration in a loop.
                        Parent.Configuration = configuration;
                    }
                    finally
                    {
                        Parent._configurationIsLoadedFromFile = false;
                    }
                }, null);
            }
        }

        private void Editor_EditorEventRaised(IEditorEvent obj)
        {
            // Update configuration watcher
            if (obj is ConfigurationChangedEvent)
            {
                if (((ConfigurationChangedEvent)obj).Save && !_configurationIsLoadedFromFile)
                    Configuration.SaveTry();

                _autoSavingTimer.Interval = Configuration.AutoSave_TimeInSeconds * 1000;
                _autoSavingTimer.Enabled = Configuration.AutoSave_Enable && HasUnsavedChanges;

                if (Configuration.Editor_ReloadFilesAutomaticallyWhenChanged != (_levelSettingsWatcher != null))
                    if (Configuration.Editor_ReloadFilesAutomaticallyWhenChanged)
                    {
                        _levelSettingsWatcher = new LevelSettingsWatcher(
                            (sender, e) => LoadedTexturesChange(null, false),
                            (sender, e) => LoadedWadsChange(false),
                            (sender, e) => LoadedSoundsCatalogsChange(false),
                            (sender, e) => LoadedImportedGeometriesChange(true),
                            SynchronizationContext);
                    }
                    else
                    {
                        _levelSettingsWatcher?.Dispose();
                        _levelSettingsWatcher = null;
                    }

                // Update coloring info
                SectorColoringManager.ColoringInfo.SectorColorScheme = Configuration.UI_ColorScheme;

                // Resize undo stack if needed
                UndoManager.Resize(Configuration.Editor_UndoDepth);

                // Update tools
                _lastFaceEditTool = Configuration.UI_LastTexturingTool;
                _lastGeometryTool = Configuration.UI_LastGeometryTool;

                if (Mode == EditorMode.Geometry)
                    Tool = Configuration.UI_LastGeometryTool;
                else
                    Tool = Configuration.UI_LastTexturingTool;
            }

            // Reset notifications, when changeing between 2D and 3D mode
            // Also reset selected sectors if wanted and restore last tool for desired mode
            if (obj is ModeChangedEvent)
            {
                var @event = (ModeChangedEvent)obj;
                if ((@event.Previous == EditorMode.Map2D) != (@event.Current == EditorMode.Map2D))
                    SendMessage();
                if (Configuration.UI_DiscardSelectionOnModeSwitch)
                    SelectedSectors = SectorSelection.None;

                if (@event.Current == EditorMode.Geometry)
                    Tool = _lastGeometryTool;
                else
                    Tool = _lastFaceEditTool;
            }

            // Backup last used tool for next mode
            if(obj is ToolChangedEvent)
            {
                var @event = (ToolChangedEvent)obj;
                if (Mode == EditorMode.Geometry)
                {
                    _lastGeometryTool = @event.Current;
                    Configuration.UI_LastGeometryTool = _lastGeometryTool;
                }    
                else
                {
                    _lastFaceEditTool = @event.Current;
                    Configuration.UI_LastTexturingTool = _lastFaceEditTool;
                }
            }

            // Reset highlight, because otherwise it will stuck until _toolHandler.Disengage() is
            // explicitly called
            if (obj is ModeChangedEvent || obj is ToolChangedEvent)
                HighlightedSectors = SectorSelection.None;

            // Update room selection so that no deleted rooms are selected
            if (obj is RoomListChangedEvent)
            {
                List<Room> newSelection = SelectedRooms.Intersect(_level.Rooms.Where(room => room != null)).ToList();
                if (newSelection.FirstOrDefault() == null)
                    SelectLastOrDefaultRoom();
                else if (newSelection.Contains(SelectedRoom))
                    SelectRooms(newSelection);
                else
                    SelectRoomsAndResetCamera(newSelection);

                if (SelectedObject != null && !newSelection.Contains(SelectedObject.Room))
                    SelectedObject = null;
            }

            // Update unsaved changes state
            if (obj is IEditorEventCausesUnsavedChanges)
            {
                HasUnsavedChanges = true;
                _autoSavingTimer.Enabled = _configuration.AutoSave_Enable;
            }

            // Make sure an object that was removed isn't selected
            if ((obj as IEditorObjectChangedEvent)?.ChangeType == ObjectChangeType.Remove)
            {
                if (((IEditorObjectChangedEvent)obj).Object == SelectedObject)
                    SelectedObject = null;
            }

            // Update level settings watcher
            if (obj is IUpdateLevelSettingsFileWatcher && ((IUpdateLevelSettingsFileWatcher)obj).UpdateLevelSettingsFileWatcher)
                _levelSettingsWatcher?.WatchLevelSettings(_level.Settings);

            // Update bookmarks
            if (obj is LevelChangedEvent ||
                (obj is ObjectChangedEvent &&
                    ((ObjectChangedEvent)obj).ChangeType == ObjectChangeType.Remove) &&
                    ((ObjectChangedEvent)obj).Object == _bookmarkedObject)
            {
                BookmarkedObject = null;
            }
        }

        public class LevelCompilationCompletedEvent : IEditorEvent
        {
            public string InfoString { get; set; }
        }

        // Auto saving
        private readonly System.Windows.Forms.Timer _autoSavingTimer = new System.Windows.Forms.Timer();
        private volatile bool _currentlyAutoSaving;
        private void AutoSave()
        {
            Level level = Level; // Copy the member variables to local variables so that the we will have slightly higher chance to succeed in the parallel thread.
            Configuration configuration = Configuration;
            if (!configuration.AutoSave_Enable)
                return;
            if (!HasUnsavedChanges || level == null)
                return;

            // Start a new thread
            // This may be a little bit unsafe and produce a corrupt file but let's hope for the best :/
            var threadAutosave = new Thread(() =>
            {
                if (_currentlyAutoSaving)
                    return;
                try
                {
                    _currentlyAutoSaving = true;

                    // Figure out data folder
                    string path;
                    string fileNameBase;
                    if (string.IsNullOrEmpty(level.Settings.LevelFilePath))
                    {
                        path = Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location);
                        fileNameBase = "Untitled";
                    }
                    else
                    {
                        path = Path.GetDirectoryName(level.Settings.LevelFilePath);
                        fileNameBase = Path.GetFileNameWithoutExtension(level.Settings.LevelFilePath);
                    }

                    // Create the new filename
                    DateTime Now = DateTime.Now;
                    string fileName;
                    if (configuration.AutoSave_NamePutDateFirst)
                        fileName = Now.ToString(configuration.AutoSave_DateTimeFormat, System.Globalization.CultureInfo.CurrentCulture) + configuration.AutoSave_NameSeparator + fileNameBase;
                    else
                        fileName = fileNameBase + configuration.AutoSave_NameSeparator + Now.ToString(configuration.AutoSave_DateTimeFormat, System.Globalization.CultureInfo.CurrentCulture);
                    fileName = Path.Combine(path, fileName + ".prj2");

                    // Save project
                    Prj2Writer.SaveToPrj2(fileName, level);

                    // Consider cleaning up directory
                    if (configuration.AutoSave_CleanupEnable)
                    {
                        // Get all compatible projects
                        var directory = new DirectoryInfo(path);
                        var filesOrdered = directory.EnumerateFiles(configuration.AutoSave_NamePutDateFirst ? "*" + configuration.AutoSave_NameSeparator + fileNameBase + ".prj2" : fileNameBase + configuration.AutoSave_NameSeparator + "*.prj2")
                                                    .OrderBy(d => d.Name)
                                                    .Select(d => d.Name)
                                                    .ToList();

                        // Clean a bit the directory
                        int numFilesToDelete = filesOrdered.Count - configuration.AutoSave_CleanupMaxAutoSaves;
                        for (int i = 0; i < numFilesToDelete; i++)
                            File.Delete(Path.Combine(path, filesOrdered[i]));
                    }

                    AutoSaveCompleted(null, fileName, Now);
                }
                catch (Exception exc)
                {
                    logger.Error(exc, "Auto save failed!");
                    AutoSaveCompleted(exc, "", DateTime.Now);
                }
                finally
                {
                    _currentlyAutoSaving = false;
                }
            });
            threadAutosave.Start();
        }

        // Construction
        public SynchronizationContext SynchronizationContext { get; }
        public RenderingDevice RenderingDevice = TombLib.Graphics.DeviceManager.DefaultDeviceManager.Device;

        public Editor(SynchronizationContext synchronizationContext, Configuration configuration, Level level)
        {
            if (synchronizationContext == null)
                throw new ArgumentNullException(nameof(synchronizationContext));
            SynchronizationContext = synchronizationContext;
            Configuration = configuration;
            SectorColoringManager = new SectorColoringManager(this);
            UndoManager = new EditorUndoManager(this, configuration.Editor_UndoDepth);
            Level = level;
            _configurationWatcher = new FileSystemWatcherManager();
            _configurationWatcher.UpdateAllFiles(new[] { new ConfigurationWatchedObj { Parent = this } });
            _autoSavingTimer.Tick += (sender, e) => AutoSave();

            EditorEventRaised += Editor_EditorEventRaised;
            _configurationIsLoadedFromFile = true;
            Editor_EditorEventRaised(new ConfigurationChangedEvent { UpdateKeyboardShortcuts = true });
            _configurationIsLoadedFromFile = false;
        }

        public void Dispose()
        {
            SectorColoringManager?.Dispose();
            _autoSavingTimer?.Dispose();
            _levelSettingsWatcher?.Dispose();
            _configurationWatcher?.Dispose();
        }

        public Editor(SynchronizationContext synchronizationContext, Configuration configuration)
            : this(synchronizationContext, configuration, Level.CreateSimpleLevel(configuration.Editor_DefaultProjectGameVersion))
        { }

        public static Editor Instance;
    }
}
