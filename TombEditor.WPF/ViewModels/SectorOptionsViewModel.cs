using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Numerics;
using System.Windows.Input;
using System.Windows.Media;
using TombLib.Rendering;

namespace TombEditor.WPF.ViewModels;

public partial class SectorOptionsViewModel : ObservableObject
{
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

	[ObservableProperty] private Vector4 floorColor;
	[ObservableProperty] private Vector4 boxColor;
	[ObservableProperty] private Vector4 notWalkableColor;
	[ObservableProperty] private Vector4 monkeyswingColor;
	[ObservableProperty] private Vector4 deathColor;
	[ObservableProperty] private Vector4 portalColor;
	[ObservableProperty] private Vector4 wallColor;

	public SectorOptionsViewModel(Editor editor)
	{
		_editor = editor;
		Configuration = _editor.Configuration;

		SetFloorCommand = CommandHandler.GetCommand("SetFloor", new CommandArgs(this, _editor));
		SetCeilingCommand = CommandHandler.GetCommand("SetCeiling", new CommandArgs(this, _editor));
		SetBoxCommand = CommandHandler.GetCommand("SetBox", new CommandArgs(this, _editor));
		SetNotWalkableCommand = CommandHandler.GetCommand("SetNotWalkable", new CommandArgs(this, _editor));
		SetMonkeyswingCommand = CommandHandler.GetCommand("SetMonkeyswing", new CommandArgs(this, _editor));
		SetDeathCommand = CommandHandler.GetCommand("SetDeath", new CommandArgs(this, _editor));
		AddPortalCommand = CommandHandler.GetCommand("AddPortal", new CommandArgs(this, _editor));
		SetWallCommand = CommandHandler.GetCommand("SetWall", new CommandArgs(this, _editor));
		SetTriggerTriggererCommand = CommandHandler.GetCommand("SetTriggerTriggerer", new CommandArgs(this, _editor));
		SetBeetleCheckpointCommand = CommandHandler.GetCommand("SetBeetleCheckpoint", new CommandArgs(this, _editor));
		SetClimbPositiveZCommand = CommandHandler.GetCommand("SetClimbPositiveZ", new CommandArgs(this, _editor));
		SetClimbPositiveXCommand = CommandHandler.GetCommand("SetClimbPositiveX", new CommandArgs(this, _editor));
		SetClimbNegativeZCommand = CommandHandler.GetCommand("SetClimbNegativeZ", new CommandArgs(this, _editor));
		SetClimbNegativeXCommand = CommandHandler.GetCommand("SetClimbNegativeX", new CommandArgs(this, _editor));
		AddGhostBlocksToSelectionCommand = CommandHandler.GetCommand("AddGhostBlocksToSelection", new CommandArgs(this, _editor));
		ToggleForceFloorSolidCommand = CommandHandler.GetCommand("ToggleForceFloorSolid", new CommandArgs(this, _editor));
		FloorStepCommand = CommandHandler.GetCommand("SetDiagonalFloorStep", new CommandArgs(this, _editor));
		CeilingStepCommand = CommandHandler.GetCommand("SetDiagonalCeilingStep", new CommandArgs(this, _editor));
		DiagonalWallCommand = CommandHandler.GetCommand("SetDiagonalWall", new CommandArgs(this, _editor));

		floorColor = _editor.Configuration.UI_ColorScheme.ColorFloor;
		boxColor = _editor.Configuration.UI_ColorScheme.ColorBox;
		notWalkableColor = _editor.Configuration.UI_ColorScheme.ColorNotWalkable;
		monkeyswingColor = _editor.Configuration.UI_ColorScheme.ColorMonkey;
		deathColor = _editor.Configuration.UI_ColorScheme.ColorDeath;
		portalColor = _editor.Configuration.UI_ColorScheme.ColorPortal;
		wallColor = _editor.Configuration.UI_ColorScheme.ColorWall;
	}

	[RelayCommand]
	private void SetSectorColoringInfoPriority(SectorColoringType type)
	{
		if (!_editor.Configuration.UI_AutoSwitchSectorColoringInfo)
			return;

		_editor.SectorColoringManager.SetPriority(type);
	}
}
