﻿using NLog;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using TombEditor.Controls;
using TombLib.LevelData;
using TombLib.Rendering;

namespace TombEditor.WPF.ToolWindows;
/// <summary>
/// Interaction logic for SectorOptions.xaml
/// </summary>
public partial class SectorOptions : UserControl
{
	public TombEditor.ToolWindows.SectorOptions WinFormsLayer { get; }

	private readonly Editor _editor;
	private static readonly Logger logger = LogManager.GetCurrentClassLogger();

	private const float iconSwitchBrightnessThreshold = 0.8f;

	private Panel2DGrid panel2DGrid = new();

	public SectorOptions()
	{
		InitializeComponent();

		_editor = Editor.Instance;
		_editor.EditorEventRaised += EditorEventRaised;

		panel2DGrid.Room = _editor.SelectedRoom;
		Panel2DHost.Child = panel2DGrid;

		Loaded += SectorOptions_Loaded;

		Editor.Instance.RaiseEvent(new Editor.InitEvent());
	}

	private void SectorOptions_Loaded(object sender, RoutedEventArgs e)
	{
		CommandHandler.AssignCommandsToControls(Editor.Instance, this);
	}

	private void EditorEventRaised(IEditorEvent obj)
	{
		if (obj is Editor.SelectedRoomChangedEvent)
			panel2DGrid.Room = ((Editor.SelectedRoomChangedEvent)obj).Current;

		// Update tooltip texts
		if (obj is Editor.ConfigurationChangedEvent)
		{
			if (((Editor.ConfigurationChangedEvent)obj).UpdateKeyboardShortcuts)
				CommandHandler.AssignCommandsToControls(_editor, this, true);
		}

		// Update color scheme on buttons
		if (obj is Editor.ConfigurationChangedEvent || obj is Editor.InitEvent)
		{
			butFloor.Background = _editor.Configuration.UI_ColorScheme.ColorFloor.ToWPFColor();
			butFloor_Image.Source = new BitmapImage(new Uri((butFloor.Background.GetBrightness() > iconSwitchBrightnessThreshold) ? "pack://application:,,,/TombEditor.WPF;component/Resources/icons_sectortype/sectortype_Floor_neg-16.png" : "pack://application:,,,/TombEditor.WPF;component/Resources/icons_sectortype/sectortype_Floor_1-16.png"));
			butCeiling.Background = _editor.Configuration.UI_ColorScheme.ColorFloor.ToWPFColor();
			butCeiling_Image.Source = new BitmapImage(new Uri((butCeiling.Background.GetBrightness() > iconSwitchBrightnessThreshold) ? "pack://application:,,,/TombEditor.WPF;component/Resources/icons_sectortype/sectortype_Roof_neg-16.png" : "pack://application:,,,/TombEditor.WPF;component/Resources/icons_sectortype/sectortype_Roof-16.png"));
			butBox.Background = _editor.Configuration.UI_ColorScheme.ColorBox.ToWPFColor();
			butBox_Image.Source = new BitmapImage(new Uri((butBox.Background.GetBrightness() > iconSwitchBrightnessThreshold) ? "pack://application:,,,/TombEditor.WPF;component/Resources/icons_sectortype/sectortype_Box_neg-16.png" : "pack://application:,,,/TombEditor.WPF;component/Resources/icons_sectortype/sectortype_Box-16.png"));
			butNotWalkableBox.Background = _editor.Configuration.UI_ColorScheme.ColorNotWalkable.ToWPFColor();
			butNotWalkableBox_Image.Source = new BitmapImage(new Uri((butNotWalkableBox.Background.GetBrightness() > iconSwitchBrightnessThreshold) ? "pack://application:,,,/TombEditor.WPF;component/Resources/icons_sectortype/sectortype_NotWalkable_neg-16.png" : "pack://application:,,,/TombEditor.WPF;component/Resources/icons_sectortype/sectortype_NotWalkable-16.png"));
			butMonkey.Background = _editor.Configuration.UI_ColorScheme.ColorMonkey.ToWPFColor();
			butMonkey_Image.Source = new BitmapImage(new Uri((butMonkey.Background.GetBrightness() > iconSwitchBrightnessThreshold) ? "pack://application:,,,/TombEditor.WPF;component/Resources/icons_sectortype/sectortype_Monkey_neg-16.png" : "pack://application:,,,/TombEditor.WPF;component/Resources/icons_sectortype/sectortype_Monkey-16.png"));
			butDeath.Background = _editor.Configuration.UI_ColorScheme.ColorDeath.ToWPFColor();
			butDeath_Image.Source = new BitmapImage(new Uri((butDeath.Background.GetBrightness() > iconSwitchBrightnessThreshold) ? "pack://application:,,,/TombEditor.WPF;component/Resources/icons_sectortype/sectortype_Death_neg-16.png" : "pack://application:,,,/TombEditor.WPF;component/Resources/icons_sectortype/sectortype_Death-16.png"));
			butPortal.Background = _editor.Configuration.UI_ColorScheme.ColorPortal.ToWPFColor();
			butPortal_Image.Source = new BitmapImage(new Uri((butPortal.Background.GetBrightness() > iconSwitchBrightnessThreshold) ? "pack://application:,,,/TombEditor.WPF;component/Resources/icons_sectortype/sectortype_Portal_neg_ -16.png" : "pack://application:,,,/TombEditor.WPF;component/Resources/icons_sectortype/sectortype_Portal -16.png"));
			butWall.Background = _editor.Configuration.UI_ColorScheme.ColorWall.ToWPFColor();
			butWall_Image.Source = new BitmapImage(new Uri((butWall.Background.GetBrightness() > iconSwitchBrightnessThreshold) ? "pack://application:,,,/TombEditor.WPF;component/Resources/icons_sectortype/sectortype_Wall_neg-16.png" : "pack://application:,,,/TombEditor.WPF;component/Resources/icons_sectortype/sectortype_Wall_1-16.png"));
		}

		// Disable version-specific controls
		if (obj is Editor.InitEvent ||
			obj is Editor.GameVersionChangedEvent ||
			obj is Editor.LevelChangedEvent)
		{
			bool isTR2 = _editor.Level.Settings.GameVersion >= TRVersion.Game.TR2;
			bool isTR345 = _editor.Level.Settings.GameVersion >= TRVersion.Game.TR3;

			butClimbNegativeX.IsEnabled = isTR2;
			butClimbNegativeZ.IsEnabled = isTR2;
			butClimbPositiveX.IsEnabled = isTR2;
			butClimbPositiveZ.IsEnabled = isTR2;
			butMonkey.IsEnabled = isTR345;
			butFlagBeetle.IsEnabled = isTR345;
			butFlagTriggerTriggerer.IsEnabled = isTR345;

			if (_editor.Level.Settings.GameVersion >= TRVersion.Game.TR4)
			{
				butFlagTriggerTriggerer_Image.Source = new BitmapImage(new Uri("pack://application:,,,/TombEditor.WPF;component/Resources/icons_sectortype/sectortype_TriggerTriggerer-16.png"));
				butFlagBeetle_Image.Source = new BitmapImage(new Uri("pack://application:,,,/TombEditor.WPF;component/Resources/icons_sectortype/sectortype_Beetle-16.png"));
			}
			else
			{

				butFlagTriggerTriggerer_Image.Source = new BitmapImage(new Uri("pack://application:,,,/TombEditor.WPF;component/Resources/icons_sectortype/sectortype_MinecartLeft-16.png"));
				butFlagBeetle_Image.Source = new BitmapImage(new Uri("pack://application:,,,/TombEditor.WPF;component/Resources/icons_sectortype/sectortype_MinecartRight-16.png"));
			}
		}
	}

	private void but_MouseEnter(object sender, RoutedEventArgs e)
	{
		SetSectorColoringInfoPriority(sender as Control);
	}

	private void SetSectorColoringInfoPriority(Control button)
	{
		if (!_editor.Configuration.UI_AutoSwitchSectorColoringInfo)
			return;

		if (button == butBox)
			_editor.SectorColoringManager.SetPriority(SectorColoringType.Box);
		else if (button == butDeath)
			_editor.SectorColoringManager.SetPriority(SectorColoringType.Death);
		else if (button == butMonkey)
			_editor.SectorColoringManager.SetPriority(SectorColoringType.Monkey);
		else if (button == butFlagBeetle)
			_editor.SectorColoringManager.SetPriority(SectorColoringType.Beetle);
		else if (button == butFlagTriggerTriggerer)
			_editor.SectorColoringManager.SetPriority(SectorColoringType.TriggerTriggerer);
		else if (button == butNotWalkableBox)
			_editor.SectorColoringManager.SetPriority(SectorColoringType.NotWalkableFloor);
		else if (button == butPortal)
			_editor.SectorColoringManager.SetPriority(SectorColoringType.Portal);
		else if (button == butForceSolidFloor)
			_editor.SectorColoringManager.SetPriority(SectorColoringType.ForceFloorSolid);
		else if (button == butClimbNegativeX ||
				 button == butClimbNegativeZ ||
				 button == butClimbPositiveX ||
				 button == butClimbPositiveZ)
			_editor.SectorColoringManager.SetPriority(SectorColoringType.Climb);
		else if (button == butWall)
			_editor.SectorColoringManager.SetPriority(SectorColoringType.Wall);
		else
			_editor.SectorColoringManager.SetPriority(SectorColoringType.Trigger);
	}
}