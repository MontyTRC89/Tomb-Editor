using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Windows.Input;
using TombLib.Forms;

namespace TombEditor.WPF.ViewModels;

public partial class MainWindowViewModel : ObservableObject
{
	[ObservableProperty] private bool _isSectorOptionsPanelVisible;
	[ObservableProperty] private bool _isRoomOptionsPanelVisible;
	[ObservableProperty] private bool _isItemBrowserVisible;
	[ObservableProperty] private bool _isImportedGeometryBrowserVisible;
	[ObservableProperty] private bool _isTriggerListVisible;
	[ObservableProperty] private bool _isLightingPanelVisible;
	[ObservableProperty] private bool _isPalettePanelVisible;
	[ObservableProperty] private bool _isTexturePanelVisible;
	[ObservableProperty] private bool _isObjectListVisible;
	[ObservableProperty] private bool _isStatisticsPanelVisible;
	[ObservableProperty] private bool _isToolPaletteVisible;
	[ObservableProperty] private bool _isToolPaletteFloating;

	// File Menu

	public ICommand NewLevelCommand { get; }
	public ICommand OpenLevelCommand { get; }
	public ICommand SaveLevelCommand { get; }
	public ICommand SaveLevelAsCommand { get; }
	public ICommand ImportPrjCommand { get; }
	public ICommand ConvertLevelToTombEngineCommand { get; }
	public ICommand BuildAndPlayCommand { get; }
	public ICommand BuildLevelCommand { get; }
	public ICommand QuitEditorCommand { get; }

	// Edit Menu

	public ICommand UndoCommand { get; }
	public ICommand RedoCommand { get; }
	public ICommand CutCommand { get; }
	public ICommand CopyCommand { get; }
	public ICommand PasteCommand { get; }
	public ICommand StampObjectCommand { get; }
	public ICommand DeleteCommand { get; }
	public ICommand SelectAllCommand { get; }
	public ICommand BookmarkObjectCommand { get; }
	public ICommand SelectBookmarkedObject { get; }
	public ICommand EditObjectCommand { get; }
	public ICommand EditVolumeEventSetsCommand { get; }
	public ICommand EditGlobalEventSetsCommand { get; }
	public ICommand SearchCommand { get; }
	public ICommand SearchAndReplaceObjectsCommand { get; }

	// View Menu

	public ICommand ResetCameraCommand { get; }
	public ICommand RelocateCameraCommand { get; }
	public ICommand ToggleFlyModeCommand { get; }
	public ICommand DrawWhiteTextureLightingOnlyCommand { get; }
	public ICommand ShowRealTintForObjectsCommand { get; }

	// Rooms Menu

	public ICommand NewRoomUpCommand { get; }
	public ICommand NewRoomDownCommand { get; }
	public ICommand NewRoomLeftCommand { get; }
	public ICommand NewRoomRightCommand { get; }
	public ICommand NewRoomFrontCommand { get; }
	public ICommand NewRoomBackCommand { get; }
	public ICommand DuplicateRoomCommand { get; }
	public ICommand CropRoomCommand { get; }
	public ICommand SplitRoomCommand { get; }
	public ICommand MergeRoomsHorizontallyCommand { get; }
	public ICommand DeleteRoomsCommand { get; }
	public ICommand MoveRoomUpCommand { get; }
	public ICommand MoveRoomUp4ClicksCommand { get; }
	public ICommand MoveRoomDownCommand { get; }
	public ICommand MoveRoomDown4ClicksCommand { get; }
	public ICommand MoveRoomLeftCommand { get; }
	public ICommand MoveRoomRightCommand { get; }
	public ICommand MoveRoomForwardCommand { get; }
	public ICommand MoveRoomBackCommand { get; }
	public ICommand RotateRoomsClockwiseCommand { get; }
	public ICommand RotateRoomsCounterClockwiseCommand { get; }
	public ICommand MirrorRoomsXCommand { get; }
	public ICommand MirrorRoomsZCommand { get; }
	public ICommand SelectWaterRoomsCommand { get; }
	public ICommand SelectSkyRoomsCommand { get; }
	public ICommand SelectOutsideRoomsCommand { get; }
	public ICommand SelectQuicksandRoomsCommand { get; }
	public ICommand SelectConnectedRoomsCommand { get; }
	public ICommand SelectRoomsByTagsCommand { get; }
	public ICommand ExportRoomsCommand { get; }
	public ICommand ImportRoomsCommand { get; }
	public ICommand ApplyRoomPropertiesCommand { get; }

	// Items Menu

	public ICommand AddWadCommand { get; }
	public ICommand RemoveWadsCommand { get; }
	public ICommand ReloadWadsCommand { get; }
	public ICommand ReloadSoundsCommand { get; }
	public ICommand AddCameraCommand { get; }
	public ICommand AddFlybyCameraCommand { get; }
	public ICommand AddSpriteCommand { get; }
	public ICommand AddSinkCommand { get; }
	public ICommand AddSoundSourceCommand { get; }
	public ICommand AddImportedGeometryCommand { get; }
	public ICommand AddGhostBlockCommand { get; }
	public ICommand AddMemoCommand { get; }
	public ICommand AddPortalCommand { get; }
	public ICommand AddTriggerCommand { get; }
	public ICommand AddBoxVolumeCommand { get; }
	public ICommand AddSphereVolumeCommand { get; }
	public ICommand DeleteAllLightsCommand { get; }
	public ICommand DeleteAllObjectsCommand { get; }
	public ICommand DeleteAllTriggersCommand { get; }
	public ICommand DeleteMissingObjectsCommand { get; }
	public ICommand LocateItemCommand { get; }
	public ICommand MoveLaraCommand { get; }
	public ICommand SelectAllObjectsInAreaCommand { get; }
	public ICommand SelectFloorBelowObjectCommand { get; }
	public ICommand SplitSectorObjectOnSelectionCommand { get; }
	public ICommand SetStaticMeshesColorToRoomLightCommand { get; }
	public ICommand SetStaticMeshesColorCommand { get; }
	public ICommand MakeQuickItemGroupCommand { get; }
	public ICommand GetObjectStatisticsCommand { get; }
	public ICommand GenerateObjectNamesCommand { get; }

	// Textures Menu

	public ICommand AddTextureCommand { get; }
	public ICommand RemoveTexturesCommand { get; }
	public ICommand UnloadTexturesCommand { get; }
	public ICommand ReloadTexturesCommand { get; }
	public ICommand ConvertTexturesToPNGCommand { get; }
	public ICommand RemapTextureCommand { get; }
	public ICommand TextureFloorCommand { get; }
	public ICommand TextureWallsCommand { get; }
	public ICommand TextureCeilingCommand { get; }
	public ICommand ClearAllTexturesInRoomCommand { get; }
	public ICommand ClearAllTexturesInLevelCommand { get; }
	public ICommand SearchTexturesCommand { get; }
	public ICommand EditAnimationRangesCommand { get; }

	// Transform Menu

	public ICommand IncreaseStepHeightCommand { get; }
	public ICommand DecreaseStepHeightCommand { get; }
	public ICommand SmoothRandomFloorUpCommand { get; }
	public ICommand SmoothRandomFloorDownCommand { get; }
	public ICommand SmoothRandomCeilingUpCommand { get; }
	public ICommand SmoothRandomCeilingDownCommand { get; }
	public ICommand SharpRandomFloorUpCommand { get; }
	public ICommand SharpRandomFloorDownCommand { get; }
	public ICommand SharpRandomCeilingUpCommand { get; }
	public ICommand SharpRandomCeilingDownCommand { get; }
	public ICommand AverageFloorCommand { get; }
	public ICommand AverageCeilingCommand { get; }
	public ICommand FlattenFloorCommand { get; }
	public ICommand FlattenCeilingCommand { get; }
	public ICommand ResetGeometryCommand { get; }
	public ICommand GridWallsIn3Command { get; }
	public ICommand GridWallsIn5Command { get; }
	public ICommand GridWallsIn3SquaresCommand { get; }
	public ICommand GridWallsIn5SquaresCommand { get; }

	// Tools Menu

	public ICommand EditLevelSettingsCommand { get; }
	public ICommand EditOptionsCommand { get; }
	public ICommand EditKeyboardLayoutCommand { get; }
	public ICommand StartWadToolCommand { get; }
	public ICommand StartSoundToolCommand { get; }

	// Window Menu

	public ICommand ShowSectorOptionsCommand { get; }
	public ICommand ShowRoomOptionsCommand { get; }
	public ICommand ShowItemBrowserCommand { get; }
	public ICommand ShowImportedGeometryBrowserCommand { get; }
	public ICommand ShowTriggerListCommand { get; }
	public ICommand ShowLightingCommand { get; }
	public ICommand ShowPaletteCommand { get; }
	public ICommand ShowTexturePanelCommand { get; }
	public ICommand ShowObjectListCommand { get; }
	public ICommand ShowStatisticsCommand { get; }
	public ICommand ShowToolPaletteCommand { get; }
	public ICommand ShowToolPaletteFloatingCommand { get; }

	// Non-menu Commands

	public ICommand RaiseQA1ClickCommand { get; }
	public ICommand RaiseQA4ClickCommand { get; }
	public ICommand LowerQA1ClickCommand { get; }
	public ICommand LowerQA4ClickCommand { get; }

	public ICommand RaiseWS1ClickCommand { get; }
	public ICommand RaiseWS4ClickCommand { get; }
	public ICommand LowerWS1ClickCommand { get; }
	public ICommand LowerWS4ClickCommand { get; }

	public ICommand RaiseED1ClickCommand { get; }
	public ICommand RaiseED4ClickCommand { get; }
	public ICommand LowerED1ClickCommand { get; }
	public ICommand LowerED4ClickCommand { get; }

	public ICommand RaiseRF1ClickCommand { get; }
	public ICommand RaiseRF4ClickCommand { get; }
	public ICommand LowerRF1ClickCommand { get; }
	public ICommand LowerRF4ClickCommand { get; }

	public ICommand RaiseQA1ClickSmoothCommand { get; }
	public ICommand RaiseQA4ClickSmoothCommand { get; }
	public ICommand LowerQA1ClickSmoothCommand { get; }
	public ICommand LowerQA4ClickSmoothCommand { get; }

	public ICommand RaiseWS1ClickSmoothCommand { get; }
	public ICommand RaiseWS4ClickSmoothCommand { get; }
	public ICommand LowerWS1ClickSmoothCommand { get; }
	public ICommand LowerWS4ClickSmoothCommand { get; }

	public ICommand RaiseED1ClickSmoothCommand { get; }
	public ICommand RaiseED4ClickSmoothCommand { get; }
	public ICommand LowerED1ClickSmoothCommand { get; }
	public ICommand LowerED4ClickSmoothCommand { get; }

	public ICommand RaiseRF1ClickSmoothCommand { get; }
	public ICommand RaiseRF4ClickSmoothCommand { get; }
	public ICommand LowerRF1ClickSmoothCommand { get; }
	public ICommand LowerRF4ClickSmoothCommand { get; }

	public ICommand RaiseYH1ClickCommand { get; }
	public ICommand RaiseYH4ClickCommand { get; }
	public ICommand LowerYH1ClickCommand { get; }
	public ICommand LowerYH4ClickCommand { get; }

	public ICommand RaiseUJ1ClickCommand { get; }
	public ICommand RaiseUJ4ClickCommand { get; }
	public ICommand LowerUJ1ClickCommand { get; }
	public ICommand LowerUJ4ClickCommand { get; }

	private readonly Editor _editor;

	public MainWindowViewModel(Editor editor)
	{
		_editor = editor;
		_editor.EditorEventRaised += EditorEventRaised;

		// File Menu

		NewLevelCommand = CommandHandler.GetCommand("NewLevel", new CommandArgs(WPFUtils.GetWin32WindowFromCaller(this), _editor));
		OpenLevelCommand = CommandHandler.GetCommand("OpenLevel", new CommandArgs(WPFUtils.GetWin32WindowFromCaller(this), _editor));
		SaveLevelCommand = CommandHandler.GetCommand("SaveLevel", new CommandArgs(WPFUtils.GetWin32WindowFromCaller(this), _editor));
		SaveLevelAsCommand = CommandHandler.GetCommand("SaveLevelAs", new CommandArgs(WPFUtils.GetWin32WindowFromCaller(this), _editor));
		ImportPrjCommand = CommandHandler.GetCommand("ImportPrj", new CommandArgs(WPFUtils.GetWin32WindowFromCaller(this), _editor));
		ConvertLevelToTombEngineCommand = CommandHandler.GetCommand("ConvertLevelToTombEngine", new CommandArgs(WPFUtils.GetWin32WindowFromCaller(this), _editor));
		BuildAndPlayCommand = CommandHandler.GetCommand("BuildAndPlay", new CommandArgs(WPFUtils.GetWin32WindowFromCaller(this), _editor));
		BuildLevelCommand = CommandHandler.GetCommand("BuildLevel", new CommandArgs(WPFUtils.GetWin32WindowFromCaller(this), _editor));
		QuitEditorCommand = CommandHandler.GetCommand("QuitEditor", new CommandArgs(WPFUtils.GetWin32WindowFromCaller(this), _editor));

		// Edit Menu

		UndoCommand = CommandHandler.GetCommand("Undo", new CommandArgs(WPFUtils.GetWin32WindowFromCaller(this), _editor));
		RedoCommand = CommandHandler.GetCommand("Redo", new CommandArgs(WPFUtils.GetWin32WindowFromCaller(this), _editor));
		CutCommand = CommandHandler.GetCommand("Cut", new CommandArgs(WPFUtils.GetWin32WindowFromCaller(this), _editor));
		CopyCommand = CommandHandler.GetCommand("Copy", new CommandArgs(WPFUtils.GetWin32WindowFromCaller(this), _editor));
		PasteCommand = CommandHandler.GetCommand("Paste", new CommandArgs(WPFUtils.GetWin32WindowFromCaller(this), _editor));
		StampObjectCommand = CommandHandler.GetCommand("StampObject", new CommandArgs(WPFUtils.GetWin32WindowFromCaller(this), _editor));
		DeleteCommand = CommandHandler.GetCommand("Delete", new CommandArgs(WPFUtils.GetWin32WindowFromCaller(this), _editor));
		SelectAllCommand = CommandHandler.GetCommand("SelectAll", new CommandArgs(WPFUtils.GetWin32WindowFromCaller(this), _editor));
		BookmarkObjectCommand = CommandHandler.GetCommand("BookmarkObject", new CommandArgs(WPFUtils.GetWin32WindowFromCaller(this), _editor));
		SelectBookmarkedObject = CommandHandler.GetCommand("SelectBookmarkedObject", new CommandArgs(WPFUtils.GetWin32WindowFromCaller(this), _editor));
		EditObjectCommand = CommandHandler.GetCommand("EditObject", new CommandArgs(WPFUtils.GetWin32WindowFromCaller(this), _editor));
		EditVolumeEventSetsCommand = CommandHandler.GetCommand("EditVolumeEventSets", new CommandArgs(WPFUtils.GetWin32WindowFromCaller(this), _editor));
		EditGlobalEventSetsCommand = CommandHandler.GetCommand("EditGlobalEventSets", new CommandArgs(WPFUtils.GetWin32WindowFromCaller(this), _editor));
		SearchCommand = CommandHandler.GetCommand("Search", new CommandArgs(WPFUtils.GetWin32WindowFromCaller(this), _editor));
		SearchAndReplaceObjectsCommand = CommandHandler.GetCommand("SearchAndReplaceObjects", new CommandArgs(WPFUtils.GetWin32WindowFromCaller(this), _editor));

		// View Menu

		ResetCameraCommand = CommandHandler.GetCommand("ResetCamera", new CommandArgs(WPFUtils.GetWin32WindowFromCaller(this), _editor));
		RelocateCameraCommand = CommandHandler.GetCommand("RelocateCamera", new CommandArgs(WPFUtils.GetWin32WindowFromCaller(this), _editor));
		ToggleFlyModeCommand = CommandHandler.GetCommand("ToggleFlyMode", new CommandArgs(WPFUtils.GetWin32WindowFromCaller(this), _editor));
		DrawWhiteTextureLightingOnlyCommand = CommandHandler.GetCommand("DrawWhiteTextureLightingOnly", new CommandArgs(WPFUtils.GetWin32WindowFromCaller(this), _editor));
		ShowRealTintForObjectsCommand = CommandHandler.GetCommand("ShowRealTintForObjects", new CommandArgs(WPFUtils.GetWin32WindowFromCaller(this), _editor));

		// Rooms Menu

		NewRoomUpCommand = CommandHandler.GetCommand("NewRoomUp", new CommandArgs(WPFUtils.GetWin32WindowFromCaller(this), _editor));
		NewRoomDownCommand = CommandHandler.GetCommand("NewRoomDown", new CommandArgs(WPFUtils.GetWin32WindowFromCaller(this), _editor));
		NewRoomLeftCommand = CommandHandler.GetCommand("NewRoomLeft", new CommandArgs(WPFUtils.GetWin32WindowFromCaller(this), _editor));
		NewRoomRightCommand = CommandHandler.GetCommand("NewRoomRight", new CommandArgs(WPFUtils.GetWin32WindowFromCaller(this), _editor));
		NewRoomFrontCommand = CommandHandler.GetCommand("NewRoomFront", new CommandArgs(WPFUtils.GetWin32WindowFromCaller(this), _editor));
		NewRoomBackCommand = CommandHandler.GetCommand("NewRoomBack", new CommandArgs(WPFUtils.GetWin32WindowFromCaller(this), _editor));
		DuplicateRoomCommand = CommandHandler.GetCommand("DuplicateRoom", new CommandArgs(WPFUtils.GetWin32WindowFromCaller(this), _editor));
		CropRoomCommand = CommandHandler.GetCommand("CropRoom", new CommandArgs(WPFUtils.GetWin32WindowFromCaller(this), _editor));
		SplitRoomCommand = CommandHandler.GetCommand("SplitRoom", new CommandArgs(WPFUtils.GetWin32WindowFromCaller(this), _editor));
		MergeRoomsHorizontallyCommand = CommandHandler.GetCommand("MergeRoomsHorizontally", new CommandArgs(WPFUtils.GetWin32WindowFromCaller(this), _editor));
		DeleteRoomsCommand = CommandHandler.GetCommand("DeleteRooms", new CommandArgs(WPFUtils.GetWin32WindowFromCaller(this), _editor));
		MoveRoomUpCommand = CommandHandler.GetCommand("MoveRoomUp", new CommandArgs(WPFUtils.GetWin32WindowFromCaller(this), _editor));
		MoveRoomUp4ClicksCommand = CommandHandler.GetCommand("MoveRoomUp4Clicks", new CommandArgs(WPFUtils.GetWin32WindowFromCaller(this), _editor));
		MoveRoomDownCommand = CommandHandler.GetCommand("MoveRoomDown", new CommandArgs(WPFUtils.GetWin32WindowFromCaller(this), _editor));
		MoveRoomDown4ClicksCommand = CommandHandler.GetCommand("MoveRoomDown4Clicks", new CommandArgs(WPFUtils.GetWin32WindowFromCaller(this), _editor));
		MoveRoomLeftCommand = CommandHandler.GetCommand("MoveRoomLeft", new CommandArgs(WPFUtils.GetWin32WindowFromCaller(this), _editor));
		MoveRoomRightCommand = CommandHandler.GetCommand("MoveRoomRight", new CommandArgs(WPFUtils.GetWin32WindowFromCaller(this), _editor));
		MoveRoomForwardCommand = CommandHandler.GetCommand("MoveRoomForward", new CommandArgs(WPFUtils.GetWin32WindowFromCaller(this), _editor));
		MoveRoomBackCommand = CommandHandler.GetCommand("MoveRoomBack", new CommandArgs(WPFUtils.GetWin32WindowFromCaller(this), _editor));
		RotateRoomsClockwiseCommand = CommandHandler.GetCommand("RotateRoomsClockwise", new CommandArgs(WPFUtils.GetWin32WindowFromCaller(this), _editor));
		RotateRoomsCounterClockwiseCommand = CommandHandler.GetCommand("RotateRoomsCounterClockwise", new CommandArgs(WPFUtils.GetWin32WindowFromCaller(this), _editor));
		MirrorRoomsXCommand = CommandHandler.GetCommand("MirrorRoomsX", new CommandArgs(WPFUtils.GetWin32WindowFromCaller(this), _editor));
		MirrorRoomsZCommand = CommandHandler.GetCommand("MirrorRoomsZ", new CommandArgs(WPFUtils.GetWin32WindowFromCaller(this), _editor));
		SelectWaterRoomsCommand = CommandHandler.GetCommand("SelectWaterRooms", new CommandArgs(WPFUtils.GetWin32WindowFromCaller(this), _editor));
		SelectSkyRoomsCommand = CommandHandler.GetCommand("SelectSkyRooms", new CommandArgs(WPFUtils.GetWin32WindowFromCaller(this), _editor));
		SelectOutsideRoomsCommand = CommandHandler.GetCommand("SelectOutsideRooms", new CommandArgs(WPFUtils.GetWin32WindowFromCaller(this), _editor));
		SelectQuicksandRoomsCommand = CommandHandler.GetCommand("SelectQuicksandRooms", new CommandArgs(WPFUtils.GetWin32WindowFromCaller(this), _editor));
		SelectConnectedRoomsCommand = CommandHandler.GetCommand("SelectConnectedRooms", new CommandArgs(WPFUtils.GetWin32WindowFromCaller(this), _editor));
		SelectRoomsByTagsCommand = CommandHandler.GetCommand("SelectRoomsByTags", new CommandArgs(WPFUtils.GetWin32WindowFromCaller(this), _editor));
		ExportRoomsCommand = CommandHandler.GetCommand("ExportRooms", new CommandArgs(WPFUtils.GetWin32WindowFromCaller(this), _editor));
		ImportRoomsCommand = CommandHandler.GetCommand("ImportRooms", new CommandArgs(WPFUtils.GetWin32WindowFromCaller(this), _editor));
		ApplyRoomPropertiesCommand = CommandHandler.GetCommand("ApplyRoomProperties", new CommandArgs(WPFUtils.GetWin32WindowFromCaller(this), _editor));

		// Items Menu

		AddWadCommand = CommandHandler.GetCommand("AddWad", new CommandArgs(WPFUtils.GetWin32WindowFromCaller(this), _editor));
		RemoveWadsCommand = CommandHandler.GetCommand("RemoveWads", new CommandArgs(WPFUtils.GetWin32WindowFromCaller(this), _editor));
		ReloadWadsCommand = CommandHandler.GetCommand("ReloadWads", new CommandArgs(WPFUtils.GetWin32WindowFromCaller(this), _editor));
		ReloadSoundsCommand = CommandHandler.GetCommand("ReloadSounds", new CommandArgs(WPFUtils.GetWin32WindowFromCaller(this), _editor));
		AddCameraCommand = CommandHandler.GetCommand("AddCamera", new CommandArgs(WPFUtils.GetWin32WindowFromCaller(this), _editor));
		AddFlybyCameraCommand = CommandHandler.GetCommand("AddFlybyCamera", new CommandArgs(WPFUtils.GetWin32WindowFromCaller(this), _editor));
		AddSpriteCommand = CommandHandler.GetCommand("AddSprite", new CommandArgs(WPFUtils.GetWin32WindowFromCaller(this), _editor));
		AddSinkCommand = CommandHandler.GetCommand("AddSink", new CommandArgs(WPFUtils.GetWin32WindowFromCaller(this), _editor));
		AddSoundSourceCommand = CommandHandler.GetCommand("AddSoundSource", new CommandArgs(WPFUtils.GetWin32WindowFromCaller(this), _editor));
		AddImportedGeometryCommand = CommandHandler.GetCommand("AddImportedGeometry", new CommandArgs(WPFUtils.GetWin32WindowFromCaller(this), _editor));
		AddGhostBlockCommand = CommandHandler.GetCommand("AddGhostBlock", new CommandArgs(WPFUtils.GetWin32WindowFromCaller(this), _editor));
		AddMemoCommand = CommandHandler.GetCommand("AddMemo", new CommandArgs(WPFUtils.GetWin32WindowFromCaller(this), _editor));
		AddPortalCommand = CommandHandler.GetCommand("AddPortal", new CommandArgs(WPFUtils.GetWin32WindowFromCaller(this), _editor));
		AddTriggerCommand = CommandHandler.GetCommand("AddTrigger", new CommandArgs(WPFUtils.GetWin32WindowFromCaller(this), _editor));
		AddBoxVolumeCommand = CommandHandler.GetCommand("AddBoxVolume", new CommandArgs(WPFUtils.GetWin32WindowFromCaller(this), _editor));
		AddSphereVolumeCommand = CommandHandler.GetCommand("AddSphereVolume", new CommandArgs(WPFUtils.GetWin32WindowFromCaller(this), _editor));
		DeleteAllLightsCommand = CommandHandler.GetCommand("DeleteAllLights", new CommandArgs(WPFUtils.GetWin32WindowFromCaller(this), _editor));
		DeleteAllObjectsCommand = CommandHandler.GetCommand("DeleteAllObjects", new CommandArgs(WPFUtils.GetWin32WindowFromCaller(this), _editor));
		DeleteAllTriggersCommand = CommandHandler.GetCommand("DeleteAllTriggers", new CommandArgs(WPFUtils.GetWin32WindowFromCaller(this), _editor));
		DeleteMissingObjectsCommand = CommandHandler.GetCommand("DeleteMissingObjects", new CommandArgs(WPFUtils.GetWin32WindowFromCaller(this), _editor));
		LocateItemCommand = CommandHandler.GetCommand("LocateItem", new CommandArgs(WPFUtils.GetWin32WindowFromCaller(this), _editor));
		MoveLaraCommand = CommandHandler.GetCommand("MoveLara", new CommandArgs(WPFUtils.GetWin32WindowFromCaller(this), _editor));
		SelectAllObjectsInAreaCommand = CommandHandler.GetCommand("SelectAllObjectsInArea", new CommandArgs(WPFUtils.GetWin32WindowFromCaller(this), _editor));
		SelectFloorBelowObjectCommand = CommandHandler.GetCommand("SelectFloorBelowObject", new CommandArgs(WPFUtils.GetWin32WindowFromCaller(this), _editor));
		SplitSectorObjectOnSelectionCommand = CommandHandler.GetCommand("SplitSectorObjectOnSelection", new CommandArgs(WPFUtils.GetWin32WindowFromCaller(this), _editor));
		SetStaticMeshesColorToRoomLightCommand = CommandHandler.GetCommand("SetStaticMeshesColorToRoomLight", new CommandArgs(WPFUtils.GetWin32WindowFromCaller(this), _editor));
		SetStaticMeshesColorCommand = CommandHandler.GetCommand("SetStaticMeshesColor", new CommandArgs(WPFUtils.GetWin32WindowFromCaller(this), _editor));
		MakeQuickItemGroupCommand = CommandHandler.GetCommand("MakeQuickItemGroup", new CommandArgs(WPFUtils.GetWin32WindowFromCaller(this), _editor));
		GetObjectStatisticsCommand = CommandHandler.GetCommand("GetObjectStatistics", new CommandArgs(WPFUtils.GetWin32WindowFromCaller(this), _editor));
		GenerateObjectNamesCommand = CommandHandler.GetCommand("GenerateObjectNames", new CommandArgs(WPFUtils.GetWin32WindowFromCaller(this), _editor));

		// Textures Menu

		AddTextureCommand = CommandHandler.GetCommand("AddTexture", new CommandArgs(WPFUtils.GetWin32WindowFromCaller(this), _editor));
		RemoveTexturesCommand = CommandHandler.GetCommand("RemoveTextures", new CommandArgs(WPFUtils.GetWin32WindowFromCaller(this), _editor));
		UnloadTexturesCommand = CommandHandler.GetCommand("UnloadTextures", new CommandArgs(WPFUtils.GetWin32WindowFromCaller(this), _editor));
		ReloadTexturesCommand = CommandHandler.GetCommand("ReloadTextures", new CommandArgs(WPFUtils.GetWin32WindowFromCaller(this), _editor));
		ConvertTexturesToPNGCommand = CommandHandler.GetCommand("ConvertTexturesToPNG", new CommandArgs(WPFUtils.GetWin32WindowFromCaller(this), _editor));
		RemapTextureCommand = CommandHandler.GetCommand("RemapTexture", new CommandArgs(WPFUtils.GetWin32WindowFromCaller(this), _editor));
		TextureFloorCommand = CommandHandler.GetCommand("TextureFloor", new CommandArgs(WPFUtils.GetWin32WindowFromCaller(this), _editor));
		TextureWallsCommand = CommandHandler.GetCommand("TextureWalls", new CommandArgs(WPFUtils.GetWin32WindowFromCaller(this), _editor));
		TextureCeilingCommand = CommandHandler.GetCommand("TextureCeiling", new CommandArgs(WPFUtils.GetWin32WindowFromCaller(this), _editor));
		ClearAllTexturesInRoomCommand = CommandHandler.GetCommand("ClearAllTexturesInRoom", new CommandArgs(WPFUtils.GetWin32WindowFromCaller(this), _editor));
		ClearAllTexturesInLevelCommand = CommandHandler.GetCommand("ClearAllTexturesInLevel", new CommandArgs(WPFUtils.GetWin32WindowFromCaller(this), _editor));
		SearchTexturesCommand = CommandHandler.GetCommand("SearchTextures", new CommandArgs(WPFUtils.GetWin32WindowFromCaller(this), _editor));
		EditAnimationRangesCommand = CommandHandler.GetCommand("EditAnimationRanges", new CommandArgs(WPFUtils.GetWin32WindowFromCaller(this), _editor));

		// Transform Menu

		IncreaseStepHeightCommand = CommandHandler.GetCommand("IncreaseStepHeight", new CommandArgs(WPFUtils.GetWin32WindowFromCaller(this), _editor));
		DecreaseStepHeightCommand = CommandHandler.GetCommand("DecreaseStepHeight", new CommandArgs(WPFUtils.GetWin32WindowFromCaller(this), _editor));
		SmoothRandomFloorUpCommand = CommandHandler.GetCommand("SmoothRandomFloorUp", new CommandArgs(WPFUtils.GetWin32WindowFromCaller(this), _editor));
		SmoothRandomFloorDownCommand = CommandHandler.GetCommand("SmoothRandomFloorDown", new CommandArgs(WPFUtils.GetWin32WindowFromCaller(this), _editor));
		SmoothRandomCeilingUpCommand = CommandHandler.GetCommand("SmoothRandomCeilingUp", new CommandArgs(WPFUtils.GetWin32WindowFromCaller(this), _editor));
		SmoothRandomCeilingDownCommand = CommandHandler.GetCommand("SmoothRandomCeilingDown", new CommandArgs(WPFUtils.GetWin32WindowFromCaller(this), _editor));
		SharpRandomFloorUpCommand = CommandHandler.GetCommand("SharpRandomFloorUp", new CommandArgs(WPFUtils.GetWin32WindowFromCaller(this), _editor));
		SharpRandomFloorDownCommand = CommandHandler.GetCommand("SharpRandomFloorDown", new CommandArgs(WPFUtils.GetWin32WindowFromCaller(this), _editor));
		SharpRandomCeilingUpCommand = CommandHandler.GetCommand("SharpRandomCeilingUp", new CommandArgs(WPFUtils.GetWin32WindowFromCaller(this), _editor));
		SharpRandomCeilingDownCommand = CommandHandler.GetCommand("SharpRandomCeilingDown", new CommandArgs(WPFUtils.GetWin32WindowFromCaller(this), _editor));
		AverageFloorCommand = CommandHandler.GetCommand("AverageFloor", new CommandArgs(WPFUtils.GetWin32WindowFromCaller(this), _editor));
		AverageCeilingCommand = CommandHandler.GetCommand("AverageCeiling", new CommandArgs(WPFUtils.GetWin32WindowFromCaller(this), _editor));
		FlattenFloorCommand = CommandHandler.GetCommand("FlattenFloor", new CommandArgs(WPFUtils.GetWin32WindowFromCaller(this), _editor));
		FlattenCeilingCommand = CommandHandler.GetCommand("FlattenCeiling", new CommandArgs(WPFUtils.GetWin32WindowFromCaller(this), _editor));
		ResetGeometryCommand = CommandHandler.GetCommand("ResetGeometry", new CommandArgs(WPFUtils.GetWin32WindowFromCaller(this), _editor));
		GridWallsIn3Command = CommandHandler.GetCommand("GridWallsIn3", new CommandArgs(WPFUtils.GetWin32WindowFromCaller(this), _editor));
		GridWallsIn5Command = CommandHandler.GetCommand("GridWallsIn5", new CommandArgs(WPFUtils.GetWin32WindowFromCaller(this), _editor));
		GridWallsIn3SquaresCommand = CommandHandler.GetCommand("GridWallsIn3Squares", new CommandArgs(WPFUtils.GetWin32WindowFromCaller(this), _editor));
		GridWallsIn5SquaresCommand = CommandHandler.GetCommand("GridWallsIn5Squares", new CommandArgs(WPFUtils.GetWin32WindowFromCaller(this), _editor));

		// Tools Menu

		EditLevelSettingsCommand = CommandHandler.GetCommand("EditLevelSettings", new CommandArgs(WPFUtils.GetWin32WindowFromCaller(this), _editor));
		EditOptionsCommand = CommandHandler.GetCommand("EditOptions", new CommandArgs(WPFUtils.GetWin32WindowFromCaller(this), _editor));
		EditKeyboardLayoutCommand = CommandHandler.GetCommand("EditKeyboardLayout", new CommandArgs(WPFUtils.GetWin32WindowFromCaller(this), _editor));
		StartWadToolCommand = CommandHandler.GetCommand("StartWadTool", new CommandArgs(WPFUtils.GetWin32WindowFromCaller(this), _editor));
		StartSoundToolCommand = CommandHandler.GetCommand("StartSoundTool", new CommandArgs(WPFUtils.GetWin32WindowFromCaller(this), _editor));

		// Non-menu Commands

		RaiseQA1ClickCommand = CommandHandler.GetCommand("RaiseQA1Click", new CommandArgs(WPFUtils.GetWin32WindowFromCaller(this), _editor));
		RaiseQA4ClickCommand = CommandHandler.GetCommand("RaiseQA4Click", new CommandArgs(WPFUtils.GetWin32WindowFromCaller(this), _editor));
		LowerQA1ClickCommand = CommandHandler.GetCommand("LowerQA1Click", new CommandArgs(WPFUtils.GetWin32WindowFromCaller(this), _editor));
		LowerQA4ClickCommand = CommandHandler.GetCommand("LowerQA4Click", new CommandArgs(WPFUtils.GetWin32WindowFromCaller(this), _editor));

		RaiseWS1ClickCommand = CommandHandler.GetCommand("RaiseWS1Click", new CommandArgs(WPFUtils.GetWin32WindowFromCaller(this), _editor));
		RaiseWS4ClickCommand = CommandHandler.GetCommand("RaiseWS4Click", new CommandArgs(WPFUtils.GetWin32WindowFromCaller(this), _editor));
		LowerWS1ClickCommand = CommandHandler.GetCommand("LowerWS1Click", new CommandArgs(WPFUtils.GetWin32WindowFromCaller(this), _editor));
		LowerWS4ClickCommand = CommandHandler.GetCommand("LowerWS4Click", new CommandArgs(WPFUtils.GetWin32WindowFromCaller(this), _editor));

		RaiseED1ClickCommand = CommandHandler.GetCommand("RaiseED1Click", new CommandArgs(WPFUtils.GetWin32WindowFromCaller(this), _editor));
		RaiseED4ClickCommand = CommandHandler.GetCommand("RaiseED4Click", new CommandArgs(WPFUtils.GetWin32WindowFromCaller(this), _editor));
		LowerED1ClickCommand = CommandHandler.GetCommand("LowerED1Click", new CommandArgs(WPFUtils.GetWin32WindowFromCaller(this), _editor));
		LowerED4ClickCommand = CommandHandler.GetCommand("LowerED4Click", new CommandArgs(WPFUtils.GetWin32WindowFromCaller(this), _editor));

		RaiseRF1ClickCommand = CommandHandler.GetCommand("RaiseRF1Click", new CommandArgs(WPFUtils.GetWin32WindowFromCaller(this), _editor));
		RaiseRF4ClickCommand = CommandHandler.GetCommand("RaiseRF4Click", new CommandArgs(WPFUtils.GetWin32WindowFromCaller(this), _editor));
		LowerRF1ClickCommand = CommandHandler.GetCommand("LowerRF1Click", new CommandArgs(WPFUtils.GetWin32WindowFromCaller(this), _editor));
		LowerRF4ClickCommand = CommandHandler.GetCommand("LowerRF4Click", new CommandArgs(WPFUtils.GetWin32WindowFromCaller(this), _editor));

		RaiseQA1ClickSmoothCommand = CommandHandler.GetCommand("RaiseQA1ClickSmooth", new CommandArgs(WPFUtils.GetWin32WindowFromCaller(this), _editor));
		RaiseQA4ClickSmoothCommand = CommandHandler.GetCommand("RaiseQA4ClickSmooth", new CommandArgs(WPFUtils.GetWin32WindowFromCaller(this), _editor));
		LowerQA1ClickSmoothCommand = CommandHandler.GetCommand("LowerQA1ClickSmooth", new CommandArgs(WPFUtils.GetWin32WindowFromCaller(this), _editor));
		LowerQA4ClickSmoothCommand = CommandHandler.GetCommand("LowerQA4ClickSmooth", new CommandArgs(WPFUtils.GetWin32WindowFromCaller(this), _editor));

		RaiseWS1ClickSmoothCommand = CommandHandler.GetCommand("RaiseWS1ClickSmooth", new CommandArgs(WPFUtils.GetWin32WindowFromCaller(this), _editor));
		RaiseWS4ClickSmoothCommand = CommandHandler.GetCommand("RaiseWS4ClickSmooth", new CommandArgs(WPFUtils.GetWin32WindowFromCaller(this), _editor));
		LowerWS1ClickSmoothCommand = CommandHandler.GetCommand("LowerWS1ClickSmooth", new CommandArgs(WPFUtils.GetWin32WindowFromCaller(this), _editor));
		LowerWS4ClickSmoothCommand = CommandHandler.GetCommand("LowerWS4ClickSmooth", new CommandArgs(WPFUtils.GetWin32WindowFromCaller(this), _editor));

		RaiseED1ClickSmoothCommand = CommandHandler.GetCommand("RaiseED1ClickSmooth", new CommandArgs(WPFUtils.GetWin32WindowFromCaller(this), _editor));
		RaiseED4ClickSmoothCommand = CommandHandler.GetCommand("RaiseED4ClickSmooth", new CommandArgs(WPFUtils.GetWin32WindowFromCaller(this), _editor));
		LowerED1ClickSmoothCommand = CommandHandler.GetCommand("LowerED1ClickSmooth", new CommandArgs(WPFUtils.GetWin32WindowFromCaller(this), _editor));
		LowerED4ClickSmoothCommand = CommandHandler.GetCommand("LowerED4ClickSmooth", new CommandArgs(WPFUtils.GetWin32WindowFromCaller(this), _editor));

		RaiseRF1ClickSmoothCommand = CommandHandler.GetCommand("RaiseRF1ClickSmooth", new CommandArgs(WPFUtils.GetWin32WindowFromCaller(this), _editor));
		RaiseRF4ClickSmoothCommand = CommandHandler.GetCommand("RaiseRF4ClickSmooth", new CommandArgs(WPFUtils.GetWin32WindowFromCaller(this), _editor));
		LowerRF1ClickSmoothCommand = CommandHandler.GetCommand("LowerRF1ClickSmooth", new CommandArgs(WPFUtils.GetWin32WindowFromCaller(this), _editor));
		LowerRF4ClickSmoothCommand = CommandHandler.GetCommand("LowerRF4ClickSmooth", new CommandArgs(WPFUtils.GetWin32WindowFromCaller(this), _editor));

		RaiseYH1ClickCommand = CommandHandler.GetCommand("RaiseYH1Click", new CommandArgs(WPFUtils.GetWin32WindowFromCaller(this), _editor));
		RaiseYH4ClickCommand = CommandHandler.GetCommand("RaiseYH4Click", new CommandArgs(WPFUtils.GetWin32WindowFromCaller(this), _editor));
		LowerYH1ClickCommand = CommandHandler.GetCommand("LowerYH1Click", new CommandArgs(WPFUtils.GetWin32WindowFromCaller(this), _editor));
		LowerYH4ClickCommand = CommandHandler.GetCommand("LowerYH4Click", new CommandArgs(WPFUtils.GetWin32WindowFromCaller(this), _editor));

		RaiseUJ1ClickCommand = CommandHandler.GetCommand("RaiseUJ1Click", new CommandArgs(WPFUtils.GetWin32WindowFromCaller(this), _editor));
		RaiseUJ4ClickCommand = CommandHandler.GetCommand("RaiseUJ4Click", new CommandArgs(WPFUtils.GetWin32WindowFromCaller(this), _editor));
		LowerUJ1ClickCommand = CommandHandler.GetCommand("LowerUJ1Click", new CommandArgs(WPFUtils.GetWin32WindowFromCaller(this), _editor));
		LowerUJ4ClickCommand = CommandHandler.GetCommand("LowerUJ4Click", new CommandArgs(WPFUtils.GetWin32WindowFromCaller(this), _editor));
	}

	private void EditorEventRaised(IEditorEvent obj)
	{
		if (obj is Editor.ConfigurationChangedEvent)
			KeyBindingsWrapper.Instance.Invalidate();
	}

	[RelayCommand]
	public void About()
	{
		using var form = new FormAbout(Properties.Resources.misc_AboutScreen_800) { StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen };
		form.ShowDialog(WPFUtils.GetWin32WindowFromCaller(this));
	}

	[RelayCommand]
	public void RestoreDefaultLayout()
	{
		Console.WriteLine("DEBUG");
	}
}
