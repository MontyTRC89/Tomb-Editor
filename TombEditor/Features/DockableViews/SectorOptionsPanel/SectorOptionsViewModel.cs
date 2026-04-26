#nullable enable

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Numerics;
using System.Windows.Input;
using TombLib.LevelData;
using TombLib.Rendering;
using TombLib.WPF;

namespace TombEditor.Features.DockableViews.SectorOptionsPanel;

public partial class SectorOptionsViewModel : ObservableObject
{
	private const float IconSwitchBrightnessThreshold = 0.8f;

	private const string FloorIcon = "/TombEditor;component/Resources/icons_sectortype/sectortype_Floor_1-16.png";
	private const string FloorIconNegative = "/TombEditor;component/Resources/icons_sectortype/sectortype_Floor_neg-16.png";
	private const string CeilingIcon = "/TombEditor;component/Resources/icons_sectortype/sectortype_Roof-16.png";
	private const string CeilingIconNegative = "/TombEditor;component/Resources/icons_sectortype/sectortype_Roof_neg-16.png";
	private const string BoxIcon = "/TombEditor;component/Resources/icons_sectortype/sectortype_Box-16.png";
	private const string BoxIconNegative = "/TombEditor;component/Resources/icons_sectortype/sectortype_Box_neg-16.png";
	private const string NotWalkableIcon = "/TombEditor;component/Resources/icons_sectortype/sectortype_NotWalkable-16.png";
	private const string NotWalkableIconNegative = "/TombEditor;component/Resources/icons_sectortype/sectortype_NotWalkable_neg-16.png";
	private const string MonkeyIcon = "/TombEditor;component/Resources/icons_sectortype/sectortype_Monkey-16.png";
	private const string MonkeyIconNegative = "/TombEditor;component/Resources/icons_sectortype/sectortype_Monkey_neg-16.png";
	private const string DeathIcon = "/TombEditor;component/Resources/icons_sectortype/sectortype_Death-16.png";
	private const string DeathIconNegative = "/TombEditor;component/Resources/icons_sectortype/sectortype_Death_neg-16.png";
	private const string PortalIcon = "/TombEditor;component/Resources/icons_sectortype/sectortype_Portal -16.png";
	private const string PortalIconNegative = "/TombEditor;component/Resources/icons_sectortype/sectortype_Portal_neg -16.png";
	private const string WallIcon = "/TombEditor;component/Resources/icons_sectortype/sectortype_Wall_1-16.png";
	private const string WallIconNegative = "/TombEditor;component/Resources/icons_sectortype/sectortype_Wall_neg-16.png";

	[ObservableProperty] private Vector4 floorColor;
	[ObservableProperty] private Vector4 boxColor;
	[ObservableProperty] private Vector4 notWalkableColor;
	[ObservableProperty] private Vector4 monkeyswingColor;
	[ObservableProperty] private Vector4 deathColor;
	[ObservableProperty] private Vector4 portalColor;
	[ObservableProperty] private Vector4 wallColor;

	[ObservableProperty] private string floorIconSource = FloorIcon;
	[ObservableProperty] private string ceilingIconSource = CeilingIcon;
	[ObservableProperty] private string boxIconSource = BoxIcon;
	[ObservableProperty] private string notWalkableIconSource = NotWalkableIcon;
	[ObservableProperty] private string monkeyIconSource = MonkeyIcon;
	[ObservableProperty] private string deathIconSource = DeathIcon;
	[ObservableProperty] private string portalIconSource = PortalIcon;
	[ObservableProperty] private string wallIconSource = WallIcon;

	[ObservableProperty] private bool supportsClimbing;
	[ObservableProperty] private bool supportsMonkeySwing;
	[ObservableProperty] private bool supportsBeetleCheckpoint;
	[ObservableProperty] private bool supportsTriggerTriggerer;

	[ObservableProperty] private string triggerTriggererIcon = TriggerTriggererIconTR4;
	[ObservableProperty] private string beetleIcon = BeetleIconTR4;

	private const string TriggerTriggererIconTR4 = "/TombEditor;component/Resources/icons_sectortype/sectortype_TriggerTriggerer-16.png";
	private const string TriggerTriggererIconTR3 = "/TombEditor;component/Resources/icons_sectortype/sectortype_MinecartLeft-16.png";
	private const string BeetleIconTR4 = "/TombEditor;component/Resources/icons_sectortype/sectortype_Beetle-16.png";
	private const string BeetleIconTR3 = "/TombEditor;component/Resources/icons_sectortype/sectortype_MinecartRight-16.png";

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

		var args = new CommandArgs(WPFUtils.GetWin32WindowOwner(), _editor);

		SetFloorCommand = CommandHandler.GetCommand("SetFloor", args);
		SetCeilingCommand = CommandHandler.GetCommand("SetCeiling", args);
		SetBoxCommand = CommandHandler.GetCommand("SetBox", args);
		SetNotWalkableCommand = CommandHandler.GetCommand("SetNotWalkable", args);
		SetMonkeyswingCommand = CommandHandler.GetCommand("SetMonkeyswing", args);
		SetDeathCommand = CommandHandler.GetCommand("SetDeath", args);
		AddPortalCommand = CommandHandler.GetCommand("AddPortal", args);
		SetWallCommand = CommandHandler.GetCommand("SetWall", args);
		SetTriggerTriggererCommand = CommandHandler.GetCommand("SetTriggerTriggerer", args);
		SetBeetleCheckpointCommand = CommandHandler.GetCommand("SetBeetleCheckpoint", args);
		SetClimbPositiveZCommand = CommandHandler.GetCommand("SetClimbPositiveZ", args);
		SetClimbPositiveXCommand = CommandHandler.GetCommand("SetClimbPositiveX", args);
		SetClimbNegativeZCommand = CommandHandler.GetCommand("SetClimbNegativeZ", args);
		SetClimbNegativeXCommand = CommandHandler.GetCommand("SetClimbNegativeX", args);
		AddGhostBlocksToSelectionCommand = CommandHandler.GetCommand("AddGhostBlocksToSelection", args);
		ToggleForceFloorSolidCommand = CommandHandler.GetCommand("ToggleForceFloorSolid", args);
		FloorStepCommand = CommandHandler.GetCommand("SetDiagonalFloorStep", args);
		CeilingStepCommand = CommandHandler.GetCommand("SetDiagonalCeilingStep", args);
		DiagonalWallCommand = CommandHandler.GetCommand("SetDiagonalWall", args);

		SetButtonColors();
		UpdateVersionSpecificControls();
	}

	public void Cleanup()
		=> _editor.EditorEventRaised -= EditorEventRaised;

	private void EditorEventRaised(IEditorEvent obj)
	{
		if (obj is Editor.ConfigurationChangedEvent or Editor.InitEvent)
			SetButtonColors();

		if (obj is Editor.InitEvent or Editor.GameVersionChangedEvent or Editor.LevelChangedEvent)
			UpdateVersionSpecificControls();
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

		FloorIconSource = GetContrastAwareIcon(FloorColor, FloorIcon, FloorIconNegative);
		CeilingIconSource = GetContrastAwareIcon(FloorColor, CeilingIcon, CeilingIconNegative);
		BoxIconSource = GetContrastAwareIcon(BoxColor, BoxIcon, BoxIconNegative);
		NotWalkableIconSource = GetContrastAwareIcon(NotWalkableColor, NotWalkableIcon, NotWalkableIconNegative);
		MonkeyIconSource = GetContrastAwareIcon(MonkeyswingColor, MonkeyIcon, MonkeyIconNegative);
		DeathIconSource = GetContrastAwareIcon(DeathColor, DeathIcon, DeathIconNegative);
		PortalIconSource = GetContrastAwareIcon(PortalColor, PortalIcon, PortalIconNegative);
		WallIconSource = GetContrastAwareIcon(WallColor, WallIcon, WallIconNegative);
	}

	private static string GetContrastAwareIcon(Vector4 color, string defaultIcon, string negativeIcon)
		=> color.ToWPFBrush().GetBrightness() > IconSwitchBrightnessThreshold ? negativeIcon : defaultIcon;

	private void UpdateVersionSpecificControls()
	{
		var version = _editor.Level.Settings.GameVersion;
		bool isTR345 = version.Native() >= TRVersion.Game.TR3;

		SupportsClimbing = version.SupportsClimbing();
		SupportsMonkeySwing = version.SupportsMonkeySwing();
		SupportsBeetleCheckpoint = isTR345;
		SupportsTriggerTriggerer = isTR345;

		if (version.Native() >= TRVersion.Game.TR4)
		{
			TriggerTriggererIcon = TriggerTriggererIconTR4;
			BeetleIcon = BeetleIconTR4;
		}
		else
		{
			TriggerTriggererIcon = TriggerTriggererIconTR3;
			BeetleIcon = BeetleIconTR3;
		}
	}

	[RelayCommand]
	private void SetSectorColoringInfoPriority(SectorColoringType type)
	{
		if (!_editor.Configuration.UI_AutoSwitchSectorColoringInfo)
			return;

		_editor.SectorColoringManager.SetPriority(type);
	}
}
