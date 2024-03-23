using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Windows.Input;
using TombLib.LevelData;
using TombLib.Rendering;

namespace TombEditor.WPF.ViewModels;

public partial class RoomOptionsViewModel : ObservableObject
{
	public IEnumerable<string> Rooms => _editor.Level.Rooms.Select((room, index) => $"{index}: {room?.Name ?? "--- EMPTY ---"}");

	public string SelectedRoom
	{
		get => _editor.SelectedRoom is not null ? $"{Array.IndexOf(_editor.Level.Rooms, _editor.SelectedRoom)}: {_editor.SelectedRoom.Name}" : "";
		set
		{
			if (string.IsNullOrEmpty(value))
				return;

			int index = int.Parse(value.Split(':')[0]);
			Room selectedRoom = _editor.Level.Rooms[index];

			if (selectedRoom is null)
				EditorActions.MakeNewRoom(index);
			else
				_editor.SelectRoom(selectedRoom);
		}
	}

	[ObservableProperty] private bool skybox;
	[ObservableProperty] private bool wind;
	[ObservableProperty] private bool damage;
	[ObservableProperty] private bool cold;
	[ObservableProperty] private bool noPathfinding;
	[ObservableProperty] private bool noLensflare;

	public ICommand EditRoomNameCommand { get; }
	public ICommand AddNewRoom { get; }
	public ICommand DuplicateRoom { get; }
	public ICommand DeleteRooms { get; }
	public ICommand CropRoom { get; }
	public ICommand MoveRoomUp { get; }
	public ICommand SplitRoom { get; }
	public ICommand MoveRoomDown { get; }
	public ICommand SelectPreviousRoom { get; }
	public ICommand LockRoom { get; }
	public ICommand HideRoom { get; }

	private readonly Editor _editor;

	public RoomOptionsViewModel(Editor editor)
	{
		_editor = editor;
		_editor.EditorEventRaised += EditorEventRaised;

		EditRoomNameCommand = CommandHandler.GetCommand("EditRoomName", new CommandArgs(WPFUtils.GetWin32WindowFromCaller(this), _editor));
		AddNewRoom = CommandHandler.GetCommand("AddNewRoom", new CommandArgs(WPFUtils.GetWin32WindowFromCaller(this), _editor));
		DuplicateRoom = CommandHandler.GetCommand("DuplicateRoom", new CommandArgs(WPFUtils.GetWin32WindowFromCaller(this), _editor));
		DeleteRooms = CommandHandler.GetCommand("DeleteRooms", new CommandArgs(WPFUtils.GetWin32WindowFromCaller(this), _editor));
		CropRoom = CommandHandler.GetCommand("CropRoom", new CommandArgs(WPFUtils.GetWin32WindowFromCaller(this), _editor));
		MoveRoomUp = CommandHandler.GetCommand("MoveRoomUp", new CommandArgs(WPFUtils.GetWin32WindowFromCaller(this), _editor));
		SplitRoom = CommandHandler.GetCommand("SplitRoom", new CommandArgs(WPFUtils.GetWin32WindowFromCaller(this), _editor));
		MoveRoomDown = CommandHandler.GetCommand("MoveRoomDown", new CommandArgs(WPFUtils.GetWin32WindowFromCaller(this), _editor));
		SelectPreviousRoom = CommandHandler.GetCommand("SelectPreviousRoom", new CommandArgs(WPFUtils.GetWin32WindowFromCaller(this), _editor));
		LockRoom = CommandHandler.GetCommand("LockRoom", new CommandArgs(WPFUtils.GetWin32WindowFromCaller(this), _editor));
		HideRoom = CommandHandler.GetCommand("HideRoom", new CommandArgs(WPFUtils.GetWin32WindowFromCaller(this), _editor));
	}

	private void EditorEventRaised(IEditorEvent obj)
	{
		if (obj is Editor.RoomListChangedEvent)
			OnPropertyChanged(nameof(Rooms));
		else if (obj is Editor.SelectedRoomChangedEvent)
			OnPropertyChanged(nameof(SelectedRoom));
	}
}

public partial class SectorOptionsViewModel : ObservableObject
{
	[ObservableProperty] private Vector4 floorColor;
	[ObservableProperty] private Vector4 boxColor;
	[ObservableProperty] private Vector4 notWalkableColor;
	[ObservableProperty] private Vector4 monkeyswingColor;
	[ObservableProperty] private Vector4 deathColor;
	[ObservableProperty] private Vector4 portalColor;
	[ObservableProperty] private Vector4 wallColor;

	public ICommand SetFloorCommand { get; }
	public ICommand SetCeilingCommand { get; }
	public ICommand SetBoxCommand { get; }
	public ICommand SetNotWalkableCommand { get; }
	public ICommand SetMonkeyswingCommand { get; }
	public ICommand SetDeathCommand { get; }
	public ICommand AddPortalCommand { get; }
	public ICommand SetWallCommand { get; }
	public ICommand SetTriggerTriggererCommand { get; }
	public ICommand SetBeetleCheckpointCommand { get; }
	public ICommand SetClimbPositiveZCommand { get; }
	public ICommand SetClimbPositiveXCommand { get; }
	public ICommand SetClimbNegativeZCommand { get; }
	public ICommand SetClimbNegativeXCommand { get; }
	public ICommand AddGhostBlocksToSelectionCommand { get; }
	public ICommand ToggleForceFloorSolidCommand { get; }
	public ICommand FloorStepCommand { get; }
	public ICommand CeilingStepCommand { get; }
	public ICommand DiagonalWallCommand { get; }

	private readonly Editor _editor;

	public SectorOptionsViewModel(Editor editor)
	{
		_editor = editor;
		_editor.EditorEventRaised += EditorEventRaised;

		SetFloorCommand = CommandHandler.GetCommand("SetFloor", new CommandArgs(WPFUtils.GetWin32WindowFromCaller(this), _editor));
		SetCeilingCommand = CommandHandler.GetCommand("SetCeiling", new CommandArgs(WPFUtils.GetWin32WindowFromCaller(this), _editor));
		SetBoxCommand = CommandHandler.GetCommand("SetBox", new CommandArgs(WPFUtils.GetWin32WindowFromCaller(this), _editor));
		SetNotWalkableCommand = CommandHandler.GetCommand("SetNotWalkable", new CommandArgs(WPFUtils.GetWin32WindowFromCaller(this), _editor));
		SetMonkeyswingCommand = CommandHandler.GetCommand("SetMonkeyswing", new CommandArgs(WPFUtils.GetWin32WindowFromCaller(this), _editor));
		SetDeathCommand = CommandHandler.GetCommand("SetDeath", new CommandArgs(WPFUtils.GetWin32WindowFromCaller(this), _editor));
		AddPortalCommand = CommandHandler.GetCommand("AddPortal", new CommandArgs(WPFUtils.GetWin32WindowFromCaller(this), _editor));
		SetWallCommand = CommandHandler.GetCommand("SetWall", new CommandArgs(WPFUtils.GetWin32WindowFromCaller(this), _editor));
		SetTriggerTriggererCommand = CommandHandler.GetCommand("SetTriggerTriggerer", new CommandArgs(WPFUtils.GetWin32WindowFromCaller(this), _editor));
		SetBeetleCheckpointCommand = CommandHandler.GetCommand("SetBeetleCheckpoint", new CommandArgs(WPFUtils.GetWin32WindowFromCaller(this), _editor));
		SetClimbPositiveZCommand = CommandHandler.GetCommand("SetClimbPositiveZ", new CommandArgs(WPFUtils.GetWin32WindowFromCaller(this), _editor));
		SetClimbPositiveXCommand = CommandHandler.GetCommand("SetClimbPositiveX", new CommandArgs(WPFUtils.GetWin32WindowFromCaller(this), _editor));
		SetClimbNegativeZCommand = CommandHandler.GetCommand("SetClimbNegativeZ", new CommandArgs(WPFUtils.GetWin32WindowFromCaller(this), _editor));
		SetClimbNegativeXCommand = CommandHandler.GetCommand("SetClimbNegativeX", new CommandArgs(WPFUtils.GetWin32WindowFromCaller(this), _editor));
		AddGhostBlocksToSelectionCommand = CommandHandler.GetCommand("AddGhostBlocksToSelection", new CommandArgs(WPFUtils.GetWin32WindowFromCaller(this), _editor));
		ToggleForceFloorSolidCommand = CommandHandler.GetCommand("ToggleForceFloorSolid", new CommandArgs(WPFUtils.GetWin32WindowFromCaller(this), _editor));
		FloorStepCommand = CommandHandler.GetCommand("SetDiagonalFloorStep", new CommandArgs(WPFUtils.GetWin32WindowFromCaller(this), _editor));
		CeilingStepCommand = CommandHandler.GetCommand("SetDiagonalCeilingStep", new CommandArgs(WPFUtils.GetWin32WindowFromCaller(this), _editor));
		DiagonalWallCommand = CommandHandler.GetCommand("SetDiagonalWall", new CommandArgs(WPFUtils.GetWin32WindowFromCaller(this), _editor));

		SetButtonColors();
	}

	private void EditorEventRaised(IEditorEvent obj)
	{
		if (obj is Editor.ConfigurationChangedEvent)
			SetButtonColors();
	}

	private void SetButtonColors()
	{
		FloorColor = _editor.Configuration.UI_ColorScheme.ColorFloor;
		BoxColor = _editor.Configuration.UI_ColorScheme.ColorBox;
		NotWalkableColor = _editor.Configuration.UI_ColorScheme.ColorNotWalkable;
		MonkeyswingColor = _editor.Configuration.UI_ColorScheme.ColorMonkey;
		DeathColor = _editor.Configuration.UI_ColorScheme.ColorDeath;
		PortalColor = _editor.Configuration.UI_ColorScheme.ColorPortal;
		WallColor = _editor.Configuration.UI_ColorScheme.ColorWall;
	}

	[RelayCommand]
	private void SetSectorColoringInfoPriority(SectorColoringType type)
	{
		if (!_editor.Configuration.UI_AutoSwitchSectorColoringInfo)
			return;

		_editor.SectorColoringManager.SetPriority(type);
	}
}
