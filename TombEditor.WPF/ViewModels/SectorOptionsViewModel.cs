using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NLog;
using System.Windows.Input;
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

		//SetFloorCommand = new SetSurfaceCommand(this, editor, false, _logger);
		//SetCeilingCommand = new SetSurfaceCommand(this, editor, true, _logger);
		//SetBoxCommand = new SetSectorFlagsCommand(this, editor, BlockFlags.Box, _logger);
		//SetNotWalkableCommand = new SetSectorFlagsCommand(this, editor, BlockFlags.NotWalkableFloor, _logger);
		//SetMonkeyswingCommand = new SetSectorFlagsCommand(this, editor, BlockFlags.Monkey, _logger);
		//SetDeathCommand = new SetSectorFlagsCommand(this, editor, BlockFlags.DeathFire, _logger);
		//AddPortalCommand = new AddPortalCommand(this, editor, _logger);
		//SetWallCommand = new SetWallCommand(this, editor, _logger);
		//SetTriggerTriggererCommand = new SetSectorFlagsCommand(this, editor, BlockFlags.TriggerTriggerer, _logger);
		//SetBeetleCheckpointCommand = new SetSectorFlagsCommand(this, editor, BlockFlags.Beetle, _logger);
		//SetClimbPositiveZCommand = new SetSectorFlagsCommand(this, editor, BlockFlags.ClimbPositiveZ, _logger);
		//SetClimbPositiveXCommand = new SetSectorFlagsCommand(this, editor, BlockFlags.ClimbPositiveX, _logger);
		//SetClimbNegativeZCommand = new SetSectorFlagsCommand(this, editor, BlockFlags.ClimbNegativeZ, _logger);
		//SetClimbNegativeXCommand = new SetSectorFlagsCommand(this, editor, BlockFlags.ClimbNegativeX, _logger);
		//AddGhostBlocksToSelectionCommand = new AddGhostBlocksCommand(this, editor, _logger);
		//ToggleForceFloorSolidCommand = new ToggleForceFloorSolidCommand(this, editor, _logger);
		//FloorStepCommand = new SetDiagonalFloorStepCommand(this, editor, _logger);
		//CeilingStepCommand = new SetDiagonalCeilingStepCommand(this, editor, _logger);
		//DiagonalWallCommand = new SetDiagonalWallCommand(this, editor, _logger);
	}

	[RelayCommand]
	private void SetSectorColoringInfoPriority(SectorColoringType type)
	{
		if (!_editor.Configuration.UI_AutoSwitchSectorColoringInfo)
			return;

		_editor.SectorColoringManager.SetPriority(type);
	}
}
