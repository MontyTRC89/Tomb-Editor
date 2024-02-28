using MvvmDialogs;
using System;
using System.Collections.Generic;
using System.Linq;
using TombLib.LevelData;
using TombLib.Utils;

namespace TombEditor.WPF;

public enum SelectionType
{
	None,
	Block,
	SpatialObject
}

public class ConfigurationChangedEventArgs : EventArgs
{
	public bool UpdateLayout { get; set; }
	public bool UpdateKeyboardShortcuts { get; set; }
	public bool UpdateToolBarLayout { get; set; }
	public bool Save { get; set; }
}

public sealed class ValueChangedEventArgs<T>(T previous, T current) : EventArgs
{
	public T Previous { get; init; } = previous;
	public T Current { get; init; } = current;
}

public delegate void ValueChangedEventHandler<T>(object? sender, ValueChangedEventArgs<T> e);

public interface IEditor : IDisposable
{
	/// <summary>
	/// The service used to locate and open Dialogs / Windows from ViewModels / Commands.
	/// </summary>
	IDialogService DialogService { get; }

	/// <summary>
	/// Everything about the editor's Undo / Redo system.
	/// </summary>
	EditorUndoManager UndoManager { get; }

	SectorColoringManager SectorColoringManager { get; }

	/// <summary>
	/// The configuration of the editor instance.
	/// </summary>
	Configuration Configuration { get; set; }

	/// <summary>
	/// Determines whether the editor should allow adjustable step height.
	/// </summary>
	bool IsPreciseGeometryAllowed { get; }

	/// <summary>
	/// The level of the current <see cref="IEditor"/> instance.
	/// <para><b>Note:</b> The life on an <see cref="IEditor"/> instance should end when the level is completely unloaded.</para>
	/// </summary>
	Level Level { get; }

	/// <summary>
	/// Statistics about the current level.
	/// </summary>
	StatisticSummary Stats { get; }

	/// <summary>
	/// Determines whether the level has unsaved changes.
	/// </summary>
	bool HasUnsavedChanges { get; set; }

	/// <summary>
	/// The current action which awaits user input to be completed (e.g. placing a chosen object).
	/// </summary>
	IEditorAction? CurrentAction { get; set; }

	/// <summary>
	/// The current mode of the editor (e.g. Geometry, Lighting, Texturing).
	/// </summary>
	EditorMode CurrentMode { get; set; }

	/// <summary>
	/// The currently used tool (e.g. Selection, Paint, Fill).
	/// </summary>
	EditorTool CurrentTool { get; set; }

	/// <summary>
	/// The rooms which have been selected by the user, using the 2D map.
	/// </summary>
	IReadOnlyList<Room?> SelectedRooms { get; set; }

	/// <summary>
	/// The currently edited room.
	/// </summary>
	Room? CurrentRoom { get; set; }

	/// <summary>
	/// The room which was selected before the current room.
	/// </summary>
	Room? PreviousRoom { get; }

	/// <summary>
	/// Determines whether the current selection is a sector (or group of sectors) or a spatial object.
	/// </summary>
	SelectionType ActiveSelectionType { get; }

	/// <summary>
	/// Represents selected sectors (marked as red) and the direction the white arrows are pointing towards.
	/// </summary>
	SectorSelection SelectedSectors { get; set; }

	/// <summary>
	/// Represents the sectors which are highlighted by hovering over them with the mouse (used for things like the 2x2 paint brush).
	/// </summary>
	SectorSelection HighlightedSectors { get; set; }

	/// <summary>
	/// The object which is currently selected by the user (e.g. a Movable, Static or ImportedGeometry).
	/// </summary>
	ObjectInstance? SelectedObject { get; set; }

	/// <summary>
	/// The object which is currently bookmarked by the user.
	/// </summary>
	ObjectInstance? BookmarkedObject { get; set; }

	/// <summary>
	/// The texture area which is currently selected in the Texture Panel.
	/// </summary>
	TextureArea SelectedTexture { get; set; }

	/// <summary>
	/// The currently chosen palette color, used for setting the color of light sources.
	/// </summary>
	ColorC CurrentPaletteColor { get; }

	/// <summary>
	/// The currently chosen item from the list of entries displayed in the UI (e.g. Item Browser ComboBox).
	/// </summary>
	ItemType? ChosenItem { get; set; }

	/// <summary>
	/// The currently chosen imported geometry from the list of entries displayed in the UI (ComboBox).
	/// </summary>
	ImportedGeometry? ChosenImportedGeometry { get; set; }

	/// <summary>
	/// Determines whether the editor's camera is currently in Fly Mode (WASD + Mouse movement).
	/// </summary>
	bool IsInFlyMode { get; set; }

	/// <summary>
	/// Determines whether selected objects should be tinted red to indicate that they are selected.
	/// </summary>
	bool IsTintingSelection { get; }

	/// <summary>
	/// Determines how much a surface should be raised / lowered by the next geometry action.
	/// <para><b>Note:</b> Legacy step height is 256 units.</para>
	/// </summary>
	int StepHeight { get; set; }

	/* Events */

	event EventHandler<ConfigurationChangedEventArgs>? ConfigurationChanged;

	event ValueChangedEventHandler<bool>? HasUnsavedChangesChanged;

	event ValueChangedEventHandler<IEditorAction?>? CurrentActionChanged;
	event ValueChangedEventHandler<EditorMode>? CurrentModeChanged;
	event ValueChangedEventHandler<EditorTool>? CurrentToolChanged;

	event ValueChangedEventHandler<Room?[]?>? SelectedRoomsChanged;
	event ValueChangedEventHandler<Room?>? CurrentRoomChanged;

	event ValueChangedEventHandler<SectorSelection>? SelectedSectorsChanged;
	event ValueChangedEventHandler<SectorSelection>? HighlightedSectorsChanged;

	event ValueChangedEventHandler<ObjectInstance?>? SelectedObjectChanged;
	event ValueChangedEventHandler<ObjectInstance?>? BookmarkedObjectChanged;

	event ValueChangedEventHandler<TextureArea>? SelectedTextureChanged;

	event ValueChangedEventHandler<ColorC>? CurrentPaletteColorChanged;

	event ValueChangedEventHandler<ItemType?>? ChosenItemChanged;
	event ValueChangedEventHandler<ImportedGeometry?>? ChosenImportedGeometryChanged;

	event ValueChangedEventHandler<bool>? IsInFlyModeChanged;
	event ValueChangedEventHandler<bool>? IsTintingSelectionChanged;

	event EventHandler<int>? StepHeightChanged;
}

public abstract class EditorBase : IEditor
{
	public IDialogService DialogService { get; }

	public EditorUndoManager UndoManager { get; }

	public SectorColoringManager SectorColoringManager { get; }

	private Configuration _configuration = new();
	public Configuration Configuration
	{
		get => _configuration;
		set
		{
			if (value == _configuration)
				return;

			_configuration = value;

			ConfigurationChanged?.Invoke(this, new() { UpdateLayout = true, UpdateKeyboardShortcuts = true, Save = true });
		}
	}

	public abstract bool IsPreciseGeometryAllowed { get; }

	public Level Level { get; }

	public StatisticSummary Stats { get; }

	private bool _hasUnsavedChanges;
	public bool HasUnsavedChanges
	{
		get => _hasUnsavedChanges;
		set
		{
			if (value == _hasUnsavedChanges)
				return;

			bool previous = _hasUnsavedChanges;
			_hasUnsavedChanges = value;

			HasUnsavedChangesChanged?.Invoke(this, new(previous, value));
		}
	}

	private IEditorAction? _currentAction;
	public IEditorAction? CurrentAction
	{
		get => _currentAction;
		set
		{
			if (value == _currentAction)
				return;

			if (value?.Equals(_currentAction) == true)
				return;

			IEditorAction? previous = _currentAction;
			_currentAction = value;

			CurrentActionChanged?.Invoke(this, new(previous, value));
		}
	}

	private EditorMode _mode = EditorMode.Geometry;
	public EditorMode CurrentMode
	{
		get => _mode;
		set
		{
			if (value == _mode)
				return;

			EditorMode previous = _mode;
			_mode = value;

			CurrentModeChanged?.Invoke(this, new(previous, value));
		}
	}

	private EditorTool _currentTool = new();
	public EditorTool CurrentTool
	{
		get => _currentTool;
		set
		{
			if (value.Tool == _currentTool.Tool && value.GridSize == _currentTool.GridSize && value.TextureUVFixer == _currentTool.TextureUVFixer) // TODO: Replace this with a proper == comparison
				return;

			EditorTool previous = _currentTool;
			_currentTool = value;

			CurrentToolChanged?.Invoke(this, new(previous, value));
		}
	}

	private Room?[] _selectedRooms = [];
	public IReadOnlyList<Room?> SelectedRooms
	{
		get => _selectedRooms;
		set
		{
			if (_selectedRooms?.SequenceEqual(value) == true)
				return;

			if (value.Count == 0)
				throw new ArgumentException("The selected room list must contain at least 1 room.");

			if (value.Any(room => room == null))
				throw new ArgumentNullException(nameof(value), "The selected room list may not contain null.");

			var roomSet = new HashSet<Room?>([null]);

			if (value.Any(room => !roomSet.Add(room)))
				throw new ArgumentNullException(nameof(value), "The selected room list may not contain duplicates.");

			// Backup first selected room index into previous room variable.
			// Using TRTomb's CurrentRoomChanged event is unreliable, because directly referencing Room
			// object may break if a couple of rooms were deleted in a row in 2D map.

			if (Level?.Rooms is not null && _selectedRooms is not null)
			{
				_previousRoomIndex = Array.FindIndex(Level.Rooms, item => item == _selectedRooms[0]);

				if (_previousRoomIndex == -1)
					_previousRoomIndex = 0; // Always jump to first room in case something went wrong
			}
			else
				_previousRoomIndex = 0; // Always jump to first room in case something went wrong

			Room?[]? previous = _selectedRooms;
			_selectedRooms = [.. value];

			if (previous is null || previous[0] != _selectedRooms[0])
				CurrentRoomChanged?.Invoke(this, new(previous?[0], _selectedRooms[0]));
			else
				SelectedRoomsChanged?.Invoke(this, new(previous, _selectedRooms));

			// Reset sector selection
			SelectedSectors = SectorSelection.None;

			// Keep last selected room index in level settings
			if (Level is not null)
				Level.Settings.LastSelectedRoom = Array.FindIndex(Level.Rooms, item => item == _selectedRooms[0]);
		}
	}

	public Room? CurrentRoom
	{
		get => _selectedRooms?[0];
		set
		{
			if (value == _selectedRooms[0])
				return;

			SelectedRooms = new[] { value };
		}
	}

	private int _previousRoomIndex;
	public Room? PreviousRoom => Level?.Rooms?[_previousRoomIndex];

	public SelectionType ActiveSelectionType { get; private set; } = SelectionType.None;

	private SectorSelection _selectedSectors = SectorSelection.None;
	public SectorSelection SelectedSectors
	{
		get => _selectedSectors;
		set
		{
			if (value != SectorSelection.None)
				ActiveSelectionType = SelectionType.Block;

			if (value == _selectedSectors)
				return;

			SectorSelection previous = _selectedSectors;
			_selectedSectors = value;

			SelectedSectorsChanged?.Invoke(this, new(previous, value));
		}
	}

	private SectorSelection _highlightedSectors = SectorSelection.None;
	public SectorSelection HighlightedSectors
	{
		get => _highlightedSectors;
		set
		{
			if (value == _highlightedSectors)
				return;

			SectorSelection previous = _highlightedSectors;
			_highlightedSectors = value;

			HighlightedSectorsChanged?.Invoke(this, new(previous, value));
		}
	}

	private ObjectInstance? _selectedObject;
	public ObjectInstance? SelectedObject
	{
		get => _selectedObject;
		set
		{
			// Check that selected object is a valid choice
			if (value is not null)
			{
				if (value.Room is null)
					throw new ArgumentException("The object to be selected is not inside a room.");

				if (Array.IndexOf(Level.Rooms, value.Room) == -1)
					throw new ArgumentException("The object to be selected is not part of the level.");
			}

			if (value is not null and ISpatial)
				ActiveSelectionType = SelectionType.SpatialObject;

			if (value == _selectedObject)
				return;

			ObjectInstance? previous = _selectedObject;
			_selectedObject = value;

			SelectedObjectChanged?.Invoke(this, new(previous, value));
		}
	}

	private ObjectInstance? _bookmarkedObject;
	public ObjectInstance? BookmarkedObject
	{
		get => Level?.ExistingRooms.SelectMany(room => room.AnyObjects).Contains(_bookmarkedObject) == true // Check that it's still part of the project
			? _bookmarkedObject
			: null;
		set
		{
			if (value == _bookmarkedObject)
				return;

			ObjectInstance? previous = _bookmarkedObject;
			_bookmarkedObject = value;

			BookmarkedObjectChanged?.Invoke(this, new(previous, value));
		}
	}

	private TextureArea _selectedTexture = TextureArea.None;
	public TextureArea SelectedTexture
	{
		get => _selectedTexture;
		set
		{
			if (value == _selectedTexture)
				return;

			TextureArea previous = _selectedTexture;
			_selectedTexture = value;

			SelectedTextureChanged?.Invoke(this, new(previous, value));
		}
	}

	public ColorC CurrentPaletteColor { get; private set; } = new ColorC(128, 128, 128);

	private ItemType? _chosenItem;
	public ItemType? ChosenItem
	{
		get => _chosenItem;
		set
		{
			if (value == _chosenItem)
				return;

			ItemType? previous = _chosenItem;
			_chosenItem = value;

			ChosenItemChanged?.Invoke(this, new(previous, value));
		}
	}

	private ImportedGeometry? _chosenImportedGeometry;
	public ImportedGeometry? ChosenImportedGeometry
	{
		get => _chosenImportedGeometry;
		set
		{
			if (value == _chosenImportedGeometry)
				return;

			ImportedGeometry? previous = _chosenImportedGeometry;
			_chosenImportedGeometry = value;

			ChosenImportedGeometryChanged?.Invoke(this, new(previous, value));
		}
	}

	public bool IsInFlyMode { get; set; }

	public bool IsTintingSelection { get; private set; }

	public int StepHeight { get; set; }

	public event EventHandler<ConfigurationChangedEventArgs>? ConfigurationChanged;
	public event ValueChangedEventHandler<bool>? HasUnsavedChangesChanged;
	public event ValueChangedEventHandler<IEditorAction?>? CurrentActionChanged;
	public event ValueChangedEventHandler<EditorMode>? CurrentModeChanged;
	public event ValueChangedEventHandler<EditorTool>? CurrentToolChanged;
	public event ValueChangedEventHandler<Room?[]?>? SelectedRoomsChanged;
	public event ValueChangedEventHandler<Room?>? CurrentRoomChanged;
	public event ValueChangedEventHandler<SectorSelection>? SelectedSectorsChanged;
	public event ValueChangedEventHandler<SectorSelection>? HighlightedSectorsChanged;
	public event ValueChangedEventHandler<ObjectInstance?>? SelectedObjectChanged;
	public event ValueChangedEventHandler<ObjectInstance?>? BookmarkedObjectChanged;
	public event ValueChangedEventHandler<TextureArea>? SelectedTextureChanged;
	public event ValueChangedEventHandler<ColorC>? CurrentPaletteColorChanged;
	public event ValueChangedEventHandler<ItemType?>? ChosenItemChanged;
	public event ValueChangedEventHandler<ImportedGeometry?>? ChosenImportedGeometryChanged;
	public event ValueChangedEventHandler<bool>? IsInFlyModeChanged;
	public event ValueChangedEventHandler<bool>? IsTintingSelectionChanged;
	public event EventHandler<int>? StepHeightChanged;

	private EditorTool _lastGeometryTool = new();
	private EditorTool _lastFaceEditTool = new();

	protected EditorBase(Level level)
	{
		Level = level;

		// Validate level
		int roomCount = Level.Rooms.Count(room => room != null);

		if (roomCount <= 0)
			Level.Rooms[0] = new Room(Level, Room.DefaultRoomDimensions, Room.DefaultRoomDimensions, Level.Settings.DefaultAmbientLight, "Room 0");
	}

	public void ResetCamera(bool newCamera = false)
	{
		RaiseEvent(new ResetCameraEvent { NewCamera = newCamera });
	}

	/// <summary>
	/// Select a room and (optionally) center the camera
	/// </summary>
	public void SelectRoom(Room newRoom)
	{
		if (CurrentRoom == newRoom)
			return;

		SelectedSectors = SectorSelection.None;
		CurrentRoom = newRoom;

		if (Configuration.Rendering3D_ResetCameraOnRoomSwitch)
			ResetCamera(true);
	}

	public void SelectRooms(IEnumerable<Room> newRooms, bool prioritizeCurrentRoom = false)
	{
		Room? firstRoom = CurrentRoom;

		if (newRooms?.FirstOrDefault() is not null)
		{
			SelectedRooms = prioritizeCurrentRoom && firstRoom is not null && newRooms.Contains(firstRoom)
				? [.. newRooms.OrderBy(r => r != firstRoom)]
				: [.. newRooms];
		}
	}

	public void SelectRoomsAndResetCamera(IEnumerable<Room> newRooms)
	{
		if (newRooms is null)
			return;

		Room? oldRoom = CurrentRoom;
		SelectRooms(newRooms);

		Room? newRoom = CurrentRoom;

		if (oldRoom != newRoom)
			ResetCamera(true);
	}

	private void SelectLastOrDefaultRoom()
	{
		SelectedRooms = Level.Settings.LastSelectedRoom >= 0 && Level.Rooms[Level.Settings.LastSelectedRoom] is not null
			? new[] { Level.Rooms[Level.Settings.LastSelectedRoom] }
			: new[] { Level.Rooms.First(room => room != null) };
	}

	public abstract void Dispose();
}
