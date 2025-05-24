using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using MvvmDialogs;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using TombLib.GeometryIO;
using TombLib.Views;

namespace TombLib.Forms.ViewModels;

public enum GeometryIOSettingsType
{
	Export,
	Import,
	AnimationImport,
}

public partial class GeometryIOSettingsViewModel : ObservableObject
{
	private const string UnsavedPresetName = "-- Unsaved --";

	// View model properties
	[ObservableProperty]
	[NotifyPropertyChangedFor(nameof(MatchingPreset))]
	private bool _isExport;

	[ObservableProperty]
	[NotifyPropertyChangedFor(nameof(MatchingPreset))]
	private bool _isRoomExport;

	[ObservableProperty]
	[NotifyPropertyChangedFor(nameof(MatchingPreset))]
	private bool _processGeometry = true;

	[ObservableProperty]
	[NotifyPropertyChangedFor(nameof(MatchingPreset))]
	private bool _processUntexturedGeometry;

	[ObservableProperty]
	[NotifyPropertyChangedFor(nameof(MatchingPreset))]
	private bool _processAnimations;

	// Axis properties
	[ObservableProperty]
	[NotifyPropertyChangedFor(nameof(MatchingPreset))]
	private bool _swapXYAxes;

	[ObservableProperty]
	[NotifyPropertyChangedFor(nameof(MatchingPreset))]
	private bool _swapXZAxes;

	[ObservableProperty]
	[NotifyPropertyChangedFor(nameof(MatchingPreset))]
	private bool _swapYZAxes;

	[ObservableProperty]
	[NotifyPropertyChangedFor(nameof(MatchingPreset))]
	private bool _invertXAxis;

	[ObservableProperty]
	[NotifyPropertyChangedFor(nameof(MatchingPreset))]
	private bool _invertYAxis;

	[ObservableProperty]
	[NotifyPropertyChangedFor(nameof(MatchingPreset))]
	private bool _invertZAxis;

	[ObservableProperty]
	[NotifyPropertyChangedFor(nameof(MatchingPreset))]
	private bool _invertFaces;

	// Size properties
	[ObservableProperty]
	[NotifyPropertyChangedFor(nameof(IsValid))]
	[NotifyPropertyChangedFor(nameof(MatchingPreset))]
	private float _scale = 1.0f;

	// Texture mapping properties
	[ObservableProperty]
	[NotifyPropertyChangedFor(nameof(MatchingPreset))]
	private bool _invertVCoordinate;

	[ObservableProperty]
	[NotifyPropertyChangedFor(nameof(MatchingPreset))]
	private bool _uvMapped;

	[ObservableProperty]
	[NotifyPropertyChangedFor(nameof(MatchingPreset))]
	private bool _wrapUV;

	[ObservableProperty]
	[NotifyPropertyChangedFor(nameof(MatchingPreset))]
	private bool _premultiplyUV;

	// Misc properties
	[ObservableProperty]
	[NotifyPropertyChangedFor(nameof(MatchingPreset))]
	private bool _vertexColorLight;

	[ObservableProperty]
	[NotifyPropertyChangedFor(nameof(MatchingPreset))]
	private bool _packTextures;

	[ObservableProperty]
	[NotifyPropertyChangedFor(nameof(MatchingPreset))]
	private bool _padPackedTextures;

	[ObservableProperty]
	[NotifyPropertyChangedFor(nameof(MatchingPreset))]
	private bool _sortByName;

	// Preset properties
	[ObservableProperty] private IOGeometrySettingsPreset _selectedPreset;

	// Custom preset for when settings don't match any preset
	private IOGeometrySettingsPreset _unsavedPreset;

	public ObservableCollection<IOGeometrySettingsPreset> Presets { get; } = new();

	public bool IsValid => Scale > 0;

	// Property to find matching preset based on current settings
	public IOGeometrySettingsPreset MatchingPreset
	{
		get
		{
			IOGeometrySettingsPreset matchingPreset = FindMatchingPreset();

			if (matchingPreset is null)
			{
				if (_unsavedPreset is null)
				{
					_unsavedPreset = new IOGeometrySettingsPreset(UnsavedPresetName, null);
					Presets.Insert(0, _unsavedPreset);
				}

				return _unsavedPreset;
			}

			return matchingPreset;
		}
	}

	private readonly IMessenger _messenger;
	private readonly IDialogService _dialogService;

	public GeometryIOSettingsViewModel(IMessenger messenger, IDialogService dialogService, GeometryIOSettingsType type)
	{
		_messenger = messenger;
		_dialogService = dialogService;

		Presets = type switch
		{
			GeometryIOSettingsType.Export => new(IOSettingsPresets.GeometryExportSettingsPresets),
			GeometryIOSettingsType.Import => new(IOSettingsPresets.GeometryImportSettingsPresets),
			GeometryIOSettingsType.AnimationImport => new(IOSettingsPresets.AnimationSettingsPresets),
			_ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
		};

		SelectedPreset = Presets.FirstOrDefault();
	}

	// Helper method to find a preset that matches current settings
	private IOGeometrySettingsPreset FindMatchingPreset()
	{
		foreach (IOGeometrySettingsPreset preset in Presets)
		{
			if (preset.Name == UnsavedPresetName)
				continue;

			if (preset.Settings.SwapXY == SwapXYAxes &&
				preset.Settings.SwapXZ == SwapXZAxes &&
				preset.Settings.SwapYZ == SwapYZAxes &&
				preset.Settings.FlipX == InvertXAxis &&
				preset.Settings.FlipY == InvertYAxis &&
				preset.Settings.FlipZ == InvertZAxis &&
				preset.Settings.InvertFaces == InvertFaces &&
				preset.Settings.Scale == Scale &&
				preset.Settings.FlipUV_V == InvertVCoordinate &&
				preset.Settings.MappedUV == UvMapped &&
				preset.Settings.WrapUV == WrapUV &&
				preset.Settings.PremultiplyUV == PremultiplyUV &&
				preset.Settings.UseVertexColor == VertexColorLight &&
				preset.Settings.PackTextures == PackTextures &&
				preset.Settings.PadPackedTextures == PadPackedTextures &&
				preset.Settings.SortByName == SortByName)
			{
				return preset;
			}
		}

		return null;
	}

	// Update the preset selection whenever a setting changes
	private void UpdateSelectedPreset()
	{
		if (_applyingSelectedPreset)
			return;

		IOGeometrySettingsPreset matchingPreset = MatchingPreset;

		if (SelectedPreset != matchingPreset)
			SelectedPreset = matchingPreset;
	}

	partial void OnSwapXYAxesChanged(bool value) => UpdateSelectedPreset();
	partial void OnSwapXZAxesChanged(bool value) => UpdateSelectedPreset();
	partial void OnSwapYZAxesChanged(bool value) => UpdateSelectedPreset();
	partial void OnInvertXAxisChanged(bool value) => UpdateSelectedPreset();
	partial void OnInvertYAxisChanged(bool value) => UpdateSelectedPreset();
	partial void OnInvertZAxisChanged(bool value) => UpdateSelectedPreset();
	partial void OnInvertFacesChanged(bool value) => UpdateSelectedPreset();
	partial void OnScaleChanged(float value) => UpdateSelectedPreset();
	partial void OnInvertVCoordinateChanged(bool value) => UpdateSelectedPreset();
	partial void OnUvMappedChanged(bool value) => UpdateSelectedPreset();
	partial void OnWrapUVChanged(bool value) => UpdateSelectedPreset();
	partial void OnPremultiplyUVChanged(bool value) => UpdateSelectedPreset();
	partial void OnVertexColorLightChanged(bool value) => UpdateSelectedPreset();
	partial void OnPackTexturesChanged(bool value) => UpdateSelectedPreset();
	partial void OnPadPackedTexturesChanged(bool value) => UpdateSelectedPreset();
	partial void OnSortByNameChanged(bool value) => UpdateSelectedPreset();

	private bool _applyingSelectedPreset;

	partial void OnSelectedPresetChanged(IOGeometrySettingsPreset value)
	{
		if (value is null || value.Name == UnsavedPresetName)
			return;

		_applyingSelectedPreset = true;

		// Update all settings based on the selected preset
		IsExport = value.Settings.Export;
		IsRoomExport = value.Settings.ExportRoom;
		ProcessGeometry = value.Settings.ProcessGeometry;
		ProcessUntexturedGeometry = value.Settings.ProcessUntexturedGeometry;
		ProcessAnimations = value.Settings.ProcessAnimations;
		SwapXYAxes = value.Settings.SwapXY;
		SwapXZAxes = value.Settings.SwapXZ;
		SwapYZAxes = value.Settings.SwapYZ;
		InvertXAxis = value.Settings.FlipX;
		InvertYAxis = value.Settings.FlipY;
		InvertZAxis = value.Settings.FlipZ;
		InvertFaces = value.Settings.InvertFaces;
		Scale = value.Settings.Scale;
		InvertVCoordinate = value.Settings.FlipUV_V;
		UvMapped = value.Settings.MappedUV;
		WrapUV = value.Settings.WrapUV;
		PremultiplyUV = value.Settings.PremultiplyUV;
		VertexColorLight = value.Settings.UseVertexColor;
		PackTextures = value.Settings.PackTextures;
		PadPackedTextures = value.Settings.PadPackedTextures;
		SortByName = value.Settings.SortByName;

		_applyingSelectedPreset = false;
	}

	[RelayCommand]
	private void Confirm()
	{
		if (!IsValid)
			return;

		// Save settings logic here
		CloseWindow(true);
	}

	[RelayCommand]
	private void SavePreset()
	{
		// Open a dialog to enter a name for the new preset
		var inputBox = new InputBoxViewModel(_messenger, "Save Preset", "Enter a name for the preset:", "TEST", UnsavedPresetName);
		_dialogService.ShowDialog(this, inputBox);

		var inputBoxWindow = new InputBoxWindow
		{
			DataContext = inputBox
		};

		if (inputBoxWindow.ShowDialog() == true)
		{
			string presetName = inputBox.Value;

			if (string.IsNullOrEmpty(presetName))
				return;

			// Check if a preset with the same name already exists
			if (Presets.Any(p => p.Name == presetName))
			{
				MessageBox.Show("A preset with this name already exists.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
				return;
			}

			// Create a new preset and add it to the list
			var newPreset = new IOGeometrySettingsPreset(presetName, new IOGeometrySettings
			{
				Export = IsExport,
				ExportRoom = IsRoomExport,
				ProcessGeometry = ProcessGeometry,
				ProcessUntexturedGeometry = ProcessUntexturedGeometry,
				ProcessAnimations = ProcessAnimations,
				SwapXY = SwapXYAxes,
				SwapXZ = SwapXZAxes,
				SwapYZ = SwapYZAxes,
				FlipX = InvertXAxis,
				FlipY = InvertYAxis,
				FlipZ = InvertZAxis,
				InvertFaces = InvertFaces,
				Scale = Scale,
				FlipUV_V = InvertVCoordinate,
				MappedUV = UvMapped,
				WrapUV = WrapUV,
				PremultiplyUV = PremultiplyUV,
				UseVertexColor = VertexColorLight,
				SortByName = SortByName,
				PackTextures = PackTextures,
				PadPackedTextures = PadPackedTextures
			});

			Presets.Add(newPreset);
			SelectedPreset = newPreset;
		}
	}

	private void CloseWindow(bool dialogResult)
	{
		// Find parent window and close it with dialog result
		if (Application.Current.Windows.OfType<Window>().FirstOrDefault(w => w.DataContext == this) is Window window)
		{
			window.DialogResult = dialogResult;
			window.Close();
		}
	}
}