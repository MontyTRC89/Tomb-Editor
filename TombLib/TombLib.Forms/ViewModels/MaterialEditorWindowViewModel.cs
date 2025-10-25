#nullable enable

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MvvmDialogs;
using MvvmDialogs.FrameworkDialogs.OpenFile;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using TombLib.LevelData;
using TombLib.Utils;
using TombLib.WPF.Services;
using TombLib.WPF.Services.Abstract;

namespace TombLib.Forms.ViewModels;

public partial class MaterialEditorWindowViewModel : ObservableObject, IModalDialogViewModel
{
	#region Properties and Fields

	[ObservableProperty] private bool? _dialogResult;

	/* Texture selection */

	[ObservableProperty] private bool _isTextureSelectionVisible = true;
	[ObservableProperty] private ObservableCollection<string> _textureList = [];
	[ObservableProperty] private string? _selectedTexture;

	/* Material data */

	[ObservableProperty] private MaterialData _materialData = new();
	[ObservableProperty] private string _materialFileName = string.Empty;
	private string _texturePath = string.Empty;

	/* Material type */

	[ObservableProperty] private ObservableCollection<string> _materialTypes = [];
	[ObservableProperty] private string _selectedMaterialType = "Default";

	/* Map paths */

	[ObservableProperty] private string _colorMapPath = string.Empty;
	[ObservableProperty] private string _normalMapPath = string.Empty;
	[ObservableProperty] private string _specularMapPath = string.Empty;
	[ObservableProperty] private string _ambientOcclusionMapPath = string.Empty;
	[ObservableProperty] private string _emissiveMapPath = string.Empty;
	[ObservableProperty] private string _roughnessMapPath = string.Empty;

	/* Map previews */

	[ObservableProperty] private BitmapImage? _colorMapPreview;
	[ObservableProperty] private BitmapImage? _normalMapPreview;
	[ObservableProperty] private BitmapImage? _specularMapPreview;
	[ObservableProperty] private BitmapImage? _ambientOcclusionMapPreview;
	[ObservableProperty] private BitmapImage? _emissiveMapPreview;
	[ObservableProperty] private BitmapImage? _roughnessMapPreview;

	/* Map background brushes (for error indication) */

	[ObservableProperty] private Brush _normalMapBackgroundBrush = Brushes.Transparent;
	[ObservableProperty] private Brush _specularMapBackgroundBrush = Brushes.Transparent;
	[ObservableProperty] private Brush _ambientOcclusionMapBackgroundBrush = Brushes.Transparent;
	[ObservableProperty] private Brush _emissiveMapBackgroundBrush = Brushes.Transparent;
	[ObservableProperty] private Brush _roughnessMapBackgroundBrush = Brushes.Transparent;

	/* Material parameters */

	[ObservableProperty] private float _normalMapStrength = 1.0f;
	[ObservableProperty] private float _specularIntensity = 1.0f;

	/*  "Has" map flags for button enabling */

	[ObservableProperty] private bool _hasNormalMap;
	[ObservableProperty] private bool _hasSpecularMap;
	[ObservableProperty] private bool _hasAmbientOcclusionMap;
	[ObservableProperty] private bool _hasEmissiveMap;
	[ObservableProperty] private bool _hasRoughnessMap;

	private readonly Brush _correctColor = Brushes.Transparent;
	private readonly Brush _wrongColor = new SolidColorBrush(Color.FromRgb(139, 69, 69));

	private bool _saveXml = false;
	private bool _loading = false;

	/* Services */

	private readonly IDialogService _dialogService;
	private readonly IMessageService _messageService;

	#endregion Properties and Fields

	public MaterialEditorWindowViewModel(Texture texture) : this([texture], texture)
	{ }

	public MaterialEditorWindowViewModel(
		IEnumerable<Texture> textureList,
		Texture? selectedTexture = null,
		IDialogService? dialogService = null,
		IMessageService? messageService = null)
	{
		// Services
		_dialogService = ServiceLocator.ResolveService(dialogService);
		_messageService = ServiceLocator.ResolveService(messageService);

		// Initialize material types
		foreach (MaterialType matType in Enum.GetValues<MaterialType>())
			MaterialTypes.Add(matType.ToString().SplitCamelcase());

		// Setup texture list
		if (textureList is not null)
		{
			foreach (var tex in textureList)
				TextureList.Add(tex.AbsolutePath);

			SelectedTexture = selectedTexture is not null && textureList.Contains(selectedTexture)
				? selectedTexture.AbsolutePath
				: TextureList.FirstOrDefault();
		}
		else
		{
			IsTextureSelectionVisible = false;
		}

		// Load initial material data
		if (SelectedTexture is not null)
			LoadMaterialForTexture(SelectedTexture);
	}

	partial void OnSelectedTextureChanged(string? value)
	{
		if (value is null || !_loading)
			return;

		if (_saveXml)
		{
			var result = _messageService.ShowConfirmation("Save changes to current material?", "Confirm changes");

			if (result)
				SaveMaterialProperties();

			_saveXml = false;
		}

		LoadMaterialForTexture(value);
	}

	partial void OnSelectedMaterialTypeChanged(string value)
	{
		if (_loading)
			return;

		var materialType = (MaterialType)MaterialTypes.IndexOf(value);
		MaterialData.Type = materialType;

		LoadMaterialProperties();
		_saveXml = true;
	}

	partial void OnNormalMapStrengthChanged(float value)
	{
		if (_loading)
			return;

		_saveXml = true;
	}

	partial void OnSpecularIntensityChanged(float value)
	{
		if (_loading)
			return;

		_saveXml = true;
	}

	private void LoadMaterialForTexture(string texturePath)
	{
		_texturePath = texturePath;

		try
		{
			MaterialData = MaterialData.TrySidecarLoadOrLoadExisting(texturePath);
			LoadMaterialInUI();
		}
		catch
		{
			_messageService.ShowError("There was an error while loading the selected material. Using default.");

			MaterialData = new MaterialData() { ColorMap = texturePath };
			_saveXml = true;

			LoadMaterialInUI();
		}
	}

	private void LoadMaterialInUI()
	{
		if (MaterialData is null)
			return;

		_loading = true;

		// Set texture paths and previews
		SetTexturePath(MaterialData.ColorMap, SetColorMapPath, SetColorMapPreview);
		SetTexturePath(MaterialData.NormalMap, SetNormalMapPath, SetNormalMapPreview);
		SetTexturePath(MaterialData.SpecularMap, SetSpecularMapPath, SetSpecularMapPreview);
		SetTexturePath(MaterialData.AmbientOcclusionMap, SetAmbientOcclusionMapPath, SetAmbientOcclusionMapPreview);
		SetTexturePath(MaterialData.EmissiveMap, SetEmissiveMapPath, SetEmissiveMapPreview);
		SetTexturePath(MaterialData.RoughnessMap, SetRoughnessMapPath, SetRoughnessMapPreview);

		// Update background colors based on file existence
		UpdateMapBackgroundBrush(MaterialData.NormalMap, MaterialData.IsNormalMapFound, SetNormalMapBackgroundBrush);
		UpdateMapBackgroundBrush(MaterialData.SpecularMap, MaterialData.IsSpecularMapFound, SetSpecularMapBackgroundBrush);
		UpdateMapBackgroundBrush(MaterialData.AmbientOcclusionMap, MaterialData.IsAmbientOcclusionMapFound, SetAmbientOcclusionMapBackgroundBrush);
		UpdateMapBackgroundBrush(MaterialData.EmissiveMap, MaterialData.IsEmissiveMapFound, SetEmissiveMapBackgroundBrush);
		UpdateMapBackgroundBrush(MaterialData.RoughnessMap, MaterialData.IsRoughnessMapFound, SetRoughnessMapBackgroundBrush);

		// Update has map flags
		HasNormalMap = !string.IsNullOrEmpty(MaterialData.NormalMap);
		HasSpecularMap = !string.IsNullOrEmpty(MaterialData.SpecularMap);
		HasAmbientOcclusionMap = !string.IsNullOrEmpty(MaterialData.AmbientOcclusionMap);
		HasEmissiveMap = !string.IsNullOrEmpty(MaterialData.EmissiveMap);
		HasRoughnessMap = !string.IsNullOrEmpty(MaterialData.RoughnessMap);

		// Set material type
		SelectedMaterialType = MaterialTypes[(int)MaterialData.Type];

		// Load material parameters
		LoadMaterialProperties();

		_loading = false;
	}

	private void LoadMaterialProperties()
	{
		switch (MaterialData.Type)
		{
			case MaterialType.Default:
				NormalMapStrength = MaterialData.Parameters0.X;
				SpecularIntensity = MaterialData.Parameters0.Y;
				break;
		}
	}

	private static void SetTexturePath(string? texturePath, Action<string> setPath, Action<BitmapImage?> setPreview)
	{
		// Set the path property
		setPath(texturePath ?? string.Empty);

		// Load texture preview
		LoadTexturePreview(texturePath, setPreview);
	}

	private void UpdateMapBackgroundBrush(string? mapPath, bool isFound, Action<Brush> setBrush)
	{
		if (string.IsNullOrEmpty(mapPath))
		{
			setBrush(_correctColor);
		}
		else
		{
			var brush = isFound ? _correctColor : _wrongColor;
			setBrush(brush);
		}
	}

	private static void LoadTexturePreview(string? path, Action<BitmapImage?> setPreview)
	{
		BitmapImage? preview = null;

		if (!string.IsNullOrEmpty(path) && File.Exists(path))
		{
			try
			{
				preview = new BitmapImage();
				preview.BeginInit();
				preview.UriSource = new Uri(path, UriKind.Absolute);
				preview.CacheOption = BitmapCacheOption.OnLoad;
				preview.EndInit();
				preview.Freeze();
			}
			catch
			{
				preview = null;
			}
		}

		setPreview(preview);
	}

	private void SetColorMapPath(string value) => ColorMapPath = value;
	private void SetNormalMapPath(string value) => NormalMapPath = value;
	private void SetSpecularMapPath(string value) => SpecularMapPath = value;
	private void SetAmbientOcclusionMapPath(string value) => AmbientOcclusionMapPath = value;
	private void SetEmissiveMapPath(string value) => EmissiveMapPath = value;
	private void SetRoughnessMapPath(string value) => RoughnessMapPath = value;

	private void SetColorMapPreview(BitmapImage? value) => ColorMapPreview = value;
	private void SetNormalMapPreview(BitmapImage? value) => NormalMapPreview = value;
	private void SetSpecularMapPreview(BitmapImage? value) => SpecularMapPreview = value;
	private void SetAmbientOcclusionMapPreview(BitmapImage? value) => AmbientOcclusionMapPreview = value;
	private void SetEmissiveMapPreview(BitmapImage? value) => EmissiveMapPreview = value;
	private void SetRoughnessMapPreview(BitmapImage? value) => RoughnessMapPreview = value;

	private void SetNormalMapBackgroundBrush(Brush value) => NormalMapBackgroundBrush = value;
	private void SetSpecularMapBackgroundBrush(Brush value) => SpecularMapBackgroundBrush = value;
	private void SetAmbientOcclusionMapBackgroundBrush(Brush value) => AmbientOcclusionMapBackgroundBrush = value;
	private void SetEmissiveMapBackgroundBrush(Brush value) => EmissiveMapBackgroundBrush = value;
	private void SetRoughnessMapBackgroundBrush(Brush value) => RoughnessMapBackgroundBrush = value;

	private void BrowseTexture(Action<string> setPath, Action<BitmapImage?> setPreview, Action<Brush> setBrush, Action<bool> setHasFlag)
	{
		var texturePath = BrowseForTexture();

		if (!string.IsNullOrEmpty(texturePath))
		{
			SetTexturePath(texturePath, setPath, setPreview);
			UpdateMapBackgroundBrush(texturePath, File.Exists(texturePath), setBrush);
			setHasFlag(true);

			_saveXml = true;
		}
	}

	private void ClearTexture(Action<string> setPath, Action<BitmapImage?> setPreview, Action<Brush> setBrush, Action<bool> setHasFlag)
	{
		SetTexturePath(string.Empty, setPath, setPreview);
		UpdateMapBackgroundBrush(string.Empty, true, setBrush);
		setHasFlag(false);

		_saveXml = true;
	}

	private string? BrowseForTexture()
	{
		var settings = new OpenFileDialogSettings
		{
			Title = "Browse Texture",
			Filter = ImageC.FileExtensions.GetFilter()
		};

		return _dialogService.ShowOpenFileDialog(this, settings) == true
			? settings.FileName
			: null;
	}

	private void SaveMaterialProperties()
	{
		if (!_saveXml)
			return;

		string externalMaterialDataPath = Path.Combine(
			Path.GetDirectoryName(_texturePath) ?? string.Empty,
			Path.GetFileNameWithoutExtension(_texturePath) + ".xml");

		var materialData = new MaterialData
		{
			Type = (MaterialType)MaterialTypes.IndexOf(SelectedMaterialType),
			ColorMap = _texturePath,
			NormalMap = NormalMapPath,
			SpecularMap = SpecularMapPath,
			EmissiveMap = EmissiveMapPath,
			AmbientOcclusionMap = AmbientOcclusionMapPath,
			RoughnessMap = RoughnessMapPath
		};

		switch (materialData.Type)
		{
			case MaterialType.Default:
				materialData.Parameters0 = new Vector4(
					NormalMapStrength,
					SpecularIntensity,
					0.0f,
					0.0f);
				break;
		}

		try
		{
			if (File.Exists(externalMaterialDataPath))
				File.Delete(externalMaterialDataPath);

			MaterialData.SaveToXml(externalMaterialDataPath, materialData);
			MaterialFileName = externalMaterialDataPath;
		}
		catch (Exception)
		{
			_messageService.ShowError($"An error occurred while saving XML material file to '{externalMaterialDataPath}'.");
		}
	}

	#region Commands

	[RelayCommand]
	private void BrowseNormalMap()
		=> BrowseTexture(SetNormalMapPath, SetNormalMapPreview, SetNormalMapBackgroundBrush,
			value => HasNormalMap = value);

	[RelayCommand]
	private void ClearNormalMap()
		=> ClearTexture(SetNormalMapPath, SetNormalMapPreview, SetNormalMapBackgroundBrush,
			value => HasNormalMap = value);

	[RelayCommand]
	private void BrowseSpecularMap()
		=> BrowseTexture(SetSpecularMapPath, SetSpecularMapPreview, SetSpecularMapBackgroundBrush,
			value => HasSpecularMap = value);

	[RelayCommand]
	private void ClearSpecularMap()
		=> ClearTexture(SetSpecularMapPath, SetSpecularMapPreview, SetSpecularMapBackgroundBrush,
			value => HasSpecularMap = value);

	[RelayCommand]
	private void BrowseAmbientOcclusionMap()
		=> BrowseTexture(SetAmbientOcclusionMapPath, SetAmbientOcclusionMapPreview, SetAmbientOcclusionMapBackgroundBrush,
			value => HasAmbientOcclusionMap = value);

	[RelayCommand]
	private void ClearAmbientOcclusionMap()
		=> ClearTexture(SetAmbientOcclusionMapPath, SetAmbientOcclusionMapPreview, SetAmbientOcclusionMapBackgroundBrush,
			value => HasAmbientOcclusionMap = value);

	[RelayCommand]
	private void BrowseEmissiveMap()
		=> BrowseTexture(SetEmissiveMapPath, SetEmissiveMapPreview, SetEmissiveMapBackgroundBrush,
			value => HasEmissiveMap = value);

	[RelayCommand]
	private void ClearEmissiveMap()
		=> ClearTexture(SetEmissiveMapPath, SetEmissiveMapPreview, SetEmissiveMapBackgroundBrush,
			value => HasEmissiveMap = value);

	[RelayCommand]
	private void BrowseRoughnessMap()
		=> BrowseTexture(SetRoughnessMapPath, SetRoughnessMapPreview, SetRoughnessMapBackgroundBrush,
			value => HasRoughnessMap = value);

	[RelayCommand]
	private void ClearRoughnessMap()
		=> ClearTexture(SetRoughnessMapPath, SetRoughnessMapPreview, SetRoughnessMapBackgroundBrush,
			value => HasRoughnessMap = value);

	[RelayCommand]
	private void Confirm()
	{
		SaveMaterialProperties();

		DialogResult = true;
		_dialogService.Close(this);
	}

	[RelayCommand]
	private void Cancel()
	{
		DialogResult = false;
		_dialogService.Close(this);
	}

	#endregion Commands
}
