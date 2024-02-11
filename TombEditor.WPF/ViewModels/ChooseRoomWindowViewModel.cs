using CommunityToolkit.Mvvm.ComponentModel;
using MvvmDialogs;
using System;
using System.Collections.Generic;
using TombLib.LevelData;

namespace TombEditor.WPF.ViewModels;

public partial class ChooseRoomWindowViewModel : ObservableObject, IModalDialogViewModel
{
	[ObservableProperty]
	private bool? dialogResult;

	public Room? SelectedRoom { get; set; }

	public ChooseRoomWindowViewModel(string why, IEnumerable<Room> rooms, Action<Room> roomSelectionChanged)
	{

	}
}
