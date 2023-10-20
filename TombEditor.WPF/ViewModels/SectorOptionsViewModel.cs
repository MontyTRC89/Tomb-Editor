using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NLog;
using System;
using System.Windows.Input;
using TombEditor.WPF.Commands;
using TombLib.Forms;
using TombLib.LevelData;
using TombLib.Rendering;

namespace TombEditor.WPF.ViewModels;

public partial class SectorOptionsViewModel : ObservableObject
{
	private readonly Logger _logger = LogManager.GetCurrentClassLogger();
	private readonly Editor _editor;

	public Configuration Configuration { get; }

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

	public SectorOptionsViewModel(Editor editor)
	{
		_editor = editor;
		Configuration = _editor.Configuration;

		SetFloorCommand = new SetFloorCommand(editor);
		SetCeilingCommand = new SetCeilingCommand(editor);
		SetBoxCommand = new SetSectorFlagsCommand(editor, BlockFlags.Box);
		SetNotWalkableCommand = new SetSectorFlagsCommand(editor, BlockFlags.NotWalkableFloor);
		SetMonkeyswingCommand = new SetSectorFlagsCommand(editor, BlockFlags.Monkey);
		SetDeathCommand = new SetSectorFlagsCommand(editor, BlockFlags.DeathFire);

		AddPortalCommand = new RelayCommand(() =>
		{
			if (EditorActions.CheckForRoomAndBlockSelection(null))
			{
				try
				{
					EditorActions.AddPortal(editor.SelectedRoom, editor.SelectedSectors.Area, null);
				}
				catch (Exception exc)
				{
					_logger.Error(exc, "Unable to create portal");
					editor.SendMessage("Unable to create portal. \nException: " + exc.Message, PopupType.Error);
				}
			}
		});

		SetWallCommand = new SetWallCommand(editor);

		SetTriggerTriggererCommand = new SetSectorFlagsCommand(editor, BlockFlags.TriggerTriggerer);
		SetBeetleCheckpointCommand = new SetSectorFlagsCommand(editor, BlockFlags.Beetle);
		SetClimbPositiveZCommand = new SetSectorFlagsCommand(editor, BlockFlags.ClimbPositiveZ);
		SetClimbPositiveXCommand = new SetSectorFlagsCommand(editor, BlockFlags.ClimbPositiveX);
		SetClimbNegativeZCommand = new SetSectorFlagsCommand(editor, BlockFlags.ClimbNegativeZ);
		SetClimbNegativeXCommand = new SetSectorFlagsCommand(editor, BlockFlags.ClimbNegativeX);

		AddGhostBlocksToSelectionCommand = new RelayCommand(() =>
		{
			if (!EditorActions.CheckForRoomAndBlockSelection(null))
				return;

			EditorActions.AddGhostBlocks(editor.SelectedRoom, editor.SelectedSectors.Area);
		});

		ToggleForceFloorSolidCommand = new RelayCommand(() =>
		{
			if (!EditorActions.CheckForRoomAndBlockSelection(null))
				return;

			EditorActions.ToggleForceFloorSolid(editor.SelectedRoom, editor.SelectedSectors.Area);
		});

		FloorStepCommand = new RelayCommand(() =>
		{
			if (!EditorActions.CheckForRoomAndBlockSelection(null))
				return;

			EditorActions.SetDiagonalFloorSplit(editor.SelectedRoom, editor.SelectedSectors.Area);
		});

		CeilingStepCommand = new RelayCommand(() =>
		{
			if (!EditorActions.CheckForRoomAndBlockSelection(null))
				return;

			EditorActions.SetDiagonalCeilingSplit(editor.SelectedRoom, editor.SelectedSectors.Area);
		});

		DiagonalWallCommand = new RelayCommand(() =>
		{
			if (!EditorActions.CheckForRoomAndBlockSelection(null))
				return;

			EditorActions.SetDiagonalWall(editor.SelectedRoom, editor.SelectedSectors.Area);
		});
	}

	[RelayCommand]
	private void SetSectorColoringInfoPriority(SectorColoringType type)
	{
		if (!_editor.Configuration.UI_AutoSwitchSectorColoringInfo)
			return;

		_editor.SectorColoringManager.SetPriority(type);
	}
}
