using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using TombLib.LevelData;

namespace TombEditor.WPF.ViewModels;

public partial class RoomOptionsViewModel : ObservableObject
{
	public IEnumerable<string> Rooms => _editor.Level.Rooms.Select((room, index) => $"{index}: {room?.Name ?? Localizer.Instance["EmptyRoom"]}");

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
