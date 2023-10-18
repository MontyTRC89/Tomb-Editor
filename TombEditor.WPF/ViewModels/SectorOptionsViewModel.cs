using NLog;
using ReactiveUI;
using System;
using System.Windows.Input;
using TombEditor.WPF.Commands;
using TombLib.Forms;
using TombLib.LevelData;

namespace TombEditor.WPF.ViewModels;

public class SectorOptionsViewModel : ViewModelBase
{
	private readonly Logger _logger = LogManager.GetCurrentClassLogger();

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
	public ICommand SetDiagonalFloorStepCommand { get; }
	public ICommand SetDiagonalCeilingStepCommand { get; }
	public ICommand SetDiagonalWallCommand { get; }

	public SectorOptionsViewModel(Editor editor)
	{
		SetFloorCommand = new SetFloorCommand(editor);
		SetCeilingCommand = new SetCeilingCommand(editor);
		SetBoxCommand = new SetSectorFlagsCommand(editor, BlockFlags.Box);
		SetNotWalkableCommand = new SetSectorFlagsCommand(editor, BlockFlags.NotWalkableFloor);
		SetMonkeyswingCommand = new SetSectorFlagsCommand(editor, BlockFlags.Monkey);
		SetDeathCommand = new SetSectorFlagsCommand(editor, BlockFlags.DeathFire);

		AddPortalCommand = ReactiveCommand.Create(() =>
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
		}, outputScheduler: RxApp.TaskpoolScheduler);

		SetWallCommand = new SetWallCommand(editor);

		SetTriggerTriggererCommand = new SetSectorFlagsCommand(editor, BlockFlags.TriggerTriggerer);
		SetBeetleCheckpointCommand = new SetSectorFlagsCommand(editor, BlockFlags.Beetle);
		SetClimbPositiveZCommand = new SetSectorFlagsCommand(editor, BlockFlags.ClimbPositiveZ);
		SetClimbPositiveXCommand = new SetSectorFlagsCommand(editor, BlockFlags.ClimbPositiveX);
		SetClimbNegativeZCommand = new SetSectorFlagsCommand(editor, BlockFlags.ClimbNegativeZ);
		SetClimbNegativeXCommand = new SetSectorFlagsCommand(editor, BlockFlags.ClimbNegativeX);

		AddGhostBlocksToSelectionCommand = ReactiveCommand.Create(() =>
		{
			if (!EditorActions.CheckForRoomAndBlockSelection(null))
				return;

			EditorActions.AddGhostBlocks(editor.SelectedRoom, editor.SelectedSectors.Area);
		}, outputScheduler: RxApp.TaskpoolScheduler);

		ToggleForceFloorSolidCommand = ReactiveCommand.Create(() =>
		{
			if (!EditorActions.CheckForRoomAndBlockSelection(null))
				return;

			EditorActions.ToggleForceFloorSolid(editor.SelectedRoom, editor.SelectedSectors.Area);
		}, outputScheduler: RxApp.TaskpoolScheduler);

		SetDiagonalFloorStepCommand = ReactiveCommand.Create(() =>
		{
			if (!EditorActions.CheckForRoomAndBlockSelection(null))
				return;

			EditorActions.SetDiagonalFloorSplit(editor.SelectedRoom, editor.SelectedSectors.Area);
		}, outputScheduler: RxApp.TaskpoolScheduler);

		SetDiagonalCeilingStepCommand = ReactiveCommand.Create(() =>
		{
			if (!EditorActions.CheckForRoomAndBlockSelection(null))
				return;

			EditorActions.SetDiagonalCeilingSplit(editor.SelectedRoom, editor.SelectedSectors.Area);
		}, outputScheduler: RxApp.TaskpoolScheduler);

		SetDiagonalWallCommand = ReactiveCommand.Create(() =>
		{
			if (!EditorActions.CheckForRoomAndBlockSelection(null))
				return;

			EditorActions.SetDiagonalWall(editor.SelectedRoom, editor.SelectedSectors.Area);
		}, outputScheduler: RxApp.TaskpoolScheduler);
	}
}
