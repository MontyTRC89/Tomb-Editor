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
                SelectedSectors = SectorSelection.None;
                Action = null;
                HasUnsavedChanges = false;
                SelectedTexture = TextureArea.None;

                // Delete old level after the new level is set
                var previousLevel = Level;
                _level = value;
                EditorEventRaised?.Invoke(new LevelChangedEvent { Previous = previousLevel, Current = value });
                RoomListChange();
                SelectedRooms = new[] { _level.Rooms.First(room => room != null) };
                ResetCamera();
                LoadedWadsChange(false);
                LoadedTexturesChange(null, false);
                LoadedImportedGeometriesChange(false);
                LevelFileNameChange();

                // Start watching for file changes
                _levelSettingsWatcher?.WatchLevelSettings(_level.Settings);
                _levelSettingsWatcher?.RestartReloading();
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
        private EditorTool _tool = new EditorTool() { Tool = EditorToolType.Selection, TextureUVFixer = true };
        public EditorTool Tool
        {
            get { return _tool; }
            set
            {
                if (value == _tool)
                    return;
                var previous = _tool;
                _tool = value;
                RaiseEvent(new ToolChangedEvent { Previous = previous, Current = value });
            }
        }
        private EditorTool _lastGeometryTool = new EditorTool() { Tool = EditorToolType.Selection, TextureUVFixer = true };
        private EditorTool _lastFaceEditTool = new EditorTool() { Tool = EditorToolType.Brush, TextureUVFixer = true };

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
                var previous = _selectedRooms;
                _selectedRooms = value.ToArray();
                if (previous == null || previous[0] != _selectedRooms[0])
                    RaiseEvent(new SelectedRoomChangedEvent(previous, value));
                else
                    RaiseEvent(new SelectedRoomsChangedEvent { Previous = previous, Current = value });
                SelectedSectors = SectorSelection.None;
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
                if (value == _selectedObject)
                    return;

                // Check that selected object is a valid choice
                if (value != null)
                {
                    if (value.Room == null)
                        throw new ArgumentException("The object to be selected is not inside a room.");
                    if (Array.IndexOf(Level.Rooms, value.Room) == -1)
                        throw new ArgumentException("The object to be selected is not part of the level.");
                }
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
            public Configuration Previous { get; internal set; }
            public Configuration Current { get; internal set; }
            public bool UpdateKeyboardShortcuts { get; internal set; } = false;
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
                RaiseEvent(new ConfigurationChangedEvent { Previous = previous, Current = value });
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
        public class AnimatedTexturesChanged : IEditorEventCausesUnsavedChanges { }
        public void AnimatedTexturesChange()
        {
            RaiseEvent(new AnimatedTexturesChanged());
        }

        // This is invoked if the animated texture sets changed for the level.
        public class TextureSoundsChanged : IEditorEventCausesUnsavedChanges { }
        public void TextureSoundsChange()
        {
            RaiseEvent(new TextureSoundsChanged());
        }

        // This is invoked if the animated texture sets changed for the level.
        public class BumpmapsChanged : IEditorEventCausesUnsavedChanges { }
        public void BumpmapsChange()
        {
            RaiseEvent(new BumpmapsChanged());
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
        public class ResetCameraEvent : IEditorCameraEvent { }
        public void ResetCamera()
        {
            RaiseEvent(new ResetCameraEvent());
        }

        // Select a texture and center the view
        public class SelectTextureAndCenterViewEvent : IEditorEvent
        {
            public TextureArea Texture { get; internal set; }
        }
        public void SelectTextureAndCenterView(TextureArea texture)
        {
            RaiseEvent(new SelectTextureAndCenterViewEvent { Texture = texture });
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

        public class EditorQuitEvent : IEditorEvent { }
        public void Quit()
        {
            RaiseEvent(new EditorQuitEvent());
        }

        public class InitEvent : IEditorEvent { }


        // Change sector highlights
        public SectorColoringManager SectorColoringManager { get; private set; }

        // Notify all components that values of the configuration have changed
        // FIXME: Is it a hack?
        public void ConfigurationChange(bool updateKeyboardShortcuts = false)
        {
            RaiseEvent(new ConfigurationChangedEvent { Previous = _configuration, Current = _configuration, UpdateKeyboardShortcuts = updateKeyboardShortcuts });
        }

        // Select a room and (optonally) center the camera
        public void SelectRoom(Room newRoom)
        {
            if (SelectedRoom == newRoom)
                return;
            SelectedSectors = SectorSelection.None;
            SelectedRoom = newRoom;

            if(Configuration.Rendering3D_ResetCameraOnRoomSwitch)
                ResetCamera();
        }

        // Select rooms
        public void SelectRooms(IEnumerable<Room> newRooms)
        {
            if (newRooms.FirstOrDefault() != null)
                SelectedRooms = newRooms.ToList();
        }

        // Select rooms and center the camera
        public void SelectRoomsAndResetCamera(IEnumerable<Room> newRooms)
        {
            Room oldRoom = SelectedRoom;
            SelectRooms(newRooms);
            Room newRoom = SelectedRoom;
            if (oldRoom != newRoom)
                ResetCamera();
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
            bool animatedTexturesChanged = !newSettings.AnimatedTextureSets.SequenceEqual(_level.Settings.AnimatedTextureSets);
            bool levelFilenameChanged = newSettings.MakeAbsolute(newSettings.LevelFilePath) != _level.Settings.MakeAbsolute(_level.Settings.LevelFilePath);

            // Update the current settings
            _level.ApplyNewLevelSettings(newSettings, instance => ObjectChange(instance, ObjectChangeType.Change));

            // Update state
            if (importedGeometryChanged)
                LoadedImportedGeometriesChange(false);

            if (texturesChanged)
                LoadedTexturesChange(null, false);

            if (wadsChanged)
                LoadedWadsChange(false);

            if (animatedTexturesChanged)
                AnimatedTexturesChange();

            if (levelFilenameChanged)
                LevelFileNameChange();

            // Update file watchers
            if (importedGeometryChanged || texturesChanged || wadsChanged)
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
            public override IEnumerable<string> Files => new[] { Configuration.GetDefaultPath() };
            public override string Name => "Configuration";
            public override bool IsRepresentingSameObject(FileSystemWatcherManager.WatchedObj other) => other is ConfigurationWatchedObj;
            public override void TryReload(FileSystemWatcherManager sender, FileSystemWatcherManager.ReloadArgs e)
            {
                Configuration configuration = Configuration.Load(Configuration.GetDefaultPath());

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
                Configuration previous = ((ConfigurationChangedEvent)obj).Previous;
                Configuration current = ((ConfigurationChangedEvent)obj).Current;

                if (!_configurationIsLoadedFromFile)
                    current?.SaveTry();
                if (previous == null || current.AutoSave_TimeInSeconds != previous.AutoSave_TimeInSeconds)
                    _autoSavingTimer.Interval = current.AutoSave_TimeInSeconds * 1000;
                if (previous == null || current.AutoSave_Enable != previous.AutoSave_Enable)
                    _autoSavingTimer.Enabled = current.AutoSave_Enable && HasUnsavedChanges;
                if (current.Editor_ReloadFilesAutomaticallyWhenChanged != (_levelSettingsWatcher != null))
                    if (current.Editor_ReloadFilesAutomaticallyWhenChanged)
                    {
                        _levelSettingsWatcher = new LevelSettingsWatcher(
                            (sender, e) => LoadedTexturesChange(null, false),
                            (sender, e) => LoadedWadsChange(false),
                            (sender, e) => LoadedImportedGeometriesChange(true),
                            (sender, e) => LoadedImportedGeometriesChange(false),
                            SynchronizationContext);
                    }
                    else
                    {
                        _levelSettingsWatcher?.Dispose();
                        _levelSettingsWatcher = null;
                    }

                // Update coloring info
                SectorColoringManager.ColoringInfo.SectorColorScheme = current.UI_ColorScheme;
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
                    _lastGeometryTool = @event.Current;
                else
                    _lastFaceEditTool = @event.Current;
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
                    SelectRoom(_level.Rooms.Where(room => room != null).First());
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
                _levelSettingsWatcher.WatchLevelSettings(_level.Settings);

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
                        fileNameBase = "Unnamed";
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

                    // Save the level
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
            Level = level;
            SectorColoringManager = new SectorColoringManager(this);
            _configurationWatcher = new FileSystemWatcherManager();
            _configurationWatcher.UpdateAllFiles(new[] { new ConfigurationWatchedObj { Parent = this } });
            _autoSavingTimer.Tick += (sender, e) => AutoSave();

            EditorEventRaised += Editor_EditorEventRaised;
            _configurationIsLoadedFromFile = true;
            Editor_EditorEventRaised(new ConfigurationChangedEvent { Current = configuration, Previous = null });
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
            : this(synchronizationContext, configuration, Level.CreateSimpleLevel())
        { }

        public static Editor Instance;
    }
}
