using Moq;
using MvvmDialogs;
using TombLib.Forms.ViewModels;
using TombLib.GeometryIO;
using TombLib.Services.Abstract;

namespace TombLib.Test;

[TestClass]
public class GeometryIOSettingsWindowViewModelTests
{
	private const string TestCustomPresetPath = @"C:\TestPresets.xml";

	private Mock<ICustomGeometrySettingsPresetIOService> _mockPresetIOService = null!;
	private Mock<IDialogService> _mockDialogService = null!;
	private Mock<IMessageService> _mockMessageService = null!;

	[TestInitialize]
	public void TestInitialize()
	{
		_mockPresetIOService = new Mock<ICustomGeometrySettingsPresetIOService>();
		_mockDialogService = new Mock<IDialogService>();
		_mockMessageService = new Mock<IMessageService>();

		// Setup default behaviour for preset service
		_mockPresetIOService.Setup(x => x.LoadPresets(It.IsAny<string>()))
			.Returns(new List<IOGeometrySettingsPreset>());

		_mockPresetIOService.Setup(x => x.SavePresets(It.IsAny<string>(), It.IsAny<IEnumerable<IOGeometrySettingsPreset>>()))
			.Returns(true);
	}

	#region Constructor Tests

	[TestMethod]
	public void Constructor_ExportType_SetsCorrectPresetsAndProperties()
	{
		// Act
		var viewModel = new GeometryIOSettingsWindowViewModel(
			GeometryIOSettingsType.Export,
			TestCustomPresetPath,
			_mockPresetIOService.Object,
			_mockDialogService.Object,
			_mockMessageService.Object
		);

		// Assert
		Assert.IsTrue(viewModel.AvailablePresets.Count > 0);
		Assert.AreEqual(IOSettingsPresets.GeometryExportSettingsPresets.Count, viewModel.AvailablePresets.Count);
		Assert.IsNotNull(viewModel.SelectedPreset);

		_mockPresetIOService.Verify(x => x.LoadPresets(TestCustomPresetPath), Times.Once);
	}

	[TestMethod]
	public void Constructor_ImportType_SetsCorrectPresetsAndProperties()
	{
		// Act
		var viewModel = new GeometryIOSettingsWindowViewModel(
			GeometryIOSettingsType.Import,
			TestCustomPresetPath,
			_mockPresetIOService.Object,
			_mockDialogService.Object,
			_mockMessageService.Object
		);

		// Assert
		Assert.IsTrue(viewModel.AvailablePresets.Count > 0);
		Assert.AreEqual(IOSettingsPresets.GeometryImportSettingsPresets.Count, viewModel.AvailablePresets.Count);
		Assert.IsNotNull(viewModel.SelectedPreset);
	}

	[TestMethod]
	public void Constructor_AnimationImportType_SetsCorrectPresetsAndProperties()
	{
		// Act
		var viewModel = new GeometryIOSettingsWindowViewModel(
			GeometryIOSettingsType.AnimationImport,
			TestCustomPresetPath,
			_mockPresetIOService.Object,
			_mockDialogService.Object,
			_mockMessageService.Object
		);

		// Assert
		Assert.IsTrue(viewModel.AvailablePresets.Count > 0);
		Assert.AreEqual(IOSettingsPresets.AnimationSettingsPresets.Count, viewModel.AvailablePresets.Count);
		Assert.IsNotNull(viewModel.SelectedPreset);
	}

	[TestMethod]
	public void Constructor_WithCustomPresets_CombinesBuiltInAndCustomPresets()
	{
		// Arrange
		var customPresets = new List<IOGeometrySettingsPreset>
		{
			new("Custom Preset 1", new IOGeometrySettings { Scale = 2.0f }, true),
			new("Custom Preset 2", new IOGeometrySettings { Scale = 3.0f }, true)
		};

		_mockPresetIOService.Setup(x => x.LoadPresets(TestCustomPresetPath))
			.Returns(customPresets);

		// Act
		var viewModel = new GeometryIOSettingsWindowViewModel(
			GeometryIOSettingsType.Export,
			TestCustomPresetPath,
			_mockPresetIOService.Object,
			_mockDialogService.Object,
			_mockMessageService.Object
		);

		// Assert
		int expectedCount = IOSettingsPresets.GeometryExportSettingsPresets.Count + customPresets.Count;

		Assert.AreEqual(expectedCount, viewModel.AvailablePresets.Count);
		Assert.IsTrue(viewModel.AvailablePresets.Any(p => p.Name == "Custom Preset 1"));
		Assert.IsTrue(viewModel.AvailablePresets.Any(p => p.Name == "Custom Preset 2"));
	}

	#endregion Constructor Tests

	#region Property Tests

	[TestMethod]
	public void IsValid_PositiveScale_ReturnsTrue()
	{
		// Arrange
		var viewModel = CreateViewModel();

		// Act
		viewModel.Scale = 1.5f;

		// Assert
		Assert.IsTrue(viewModel.IsValid);
	}

	[TestMethod]
	public void IsValid_ZeroScale_ReturnsFalse()
	{
		// Arrange
		var viewModel = CreateViewModel();

		// Act
		viewModel.Scale = 0.0f;

		// Assert
		Assert.IsFalse(viewModel.IsValid);
	}

	[TestMethod]
	public void IsValid_NegativeScale_ReturnsFalse()
	{
		// Arrange
		var viewModel = CreateViewModel();

		// Act
		viewModel.Scale = -1.0f;

		// Assert
		Assert.IsFalse(viewModel.IsValid);
	}

	[TestMethod]
	public void IsSelectedPresetCustom_CustomPreset_ReturnsTrue()
	{
		// Arrange
		var viewModel = CreateViewModel();

		// Act
		viewModel.SelectedPreset = new IOGeometrySettingsPreset("Custom", new IOGeometrySettings(), true);

		// Assert
		Assert.IsTrue(viewModel.IsSelectedPresetCustom);
	}

	[TestMethod]
	public void IsSelectedPresetCustom_BuiltInPreset_ReturnsFalse()
	{
		// Arrange
		var viewModel = CreateViewModel();

		// Act
		viewModel.SelectedPreset = new IOGeometrySettingsPreset("Built-In", new IOGeometrySettings(), false);

		// Assert
		Assert.IsFalse(viewModel.IsSelectedPresetCustom);
	}

	[TestMethod]
	public void IsSelectedPresetUnsaved_UnsavedPreset_ReturnsTrue()
	{
		// Arrange
		var viewModel = CreateViewModel();

		// Act - Modify a setting to trigger unsaved preset creation
		viewModel.Scale = 999.0f;

		// Assert
		Assert.IsTrue(viewModel.IsSelectedPresetUnsaved);
	}

	#endregion Property Tests

	#region Preset Matching Tests

	[TestMethod]
	public void MatchingPreset_ExactMatch_ReturnsMatchingPreset()
	{
		// Arrange
		var viewModel = CreateViewModel();
		var targetPreset = viewModel.AvailablePresets.First();

		// Act - Apply the first preset's settings
		viewModel.SelectedPreset = targetPreset;

		// Assert
		Assert.AreEqual(targetPreset, viewModel.MatchingPreset);
	}

	[TestMethod]
	public void MatchingPreset_NoMatch_ReturnsUnsavedPreset()
	{
		// Arrange
		var viewModel = CreateViewModel();

		// Act - Set unique settings that don't match any preset
		viewModel.Scale = 999.0f;
		viewModel.InvertXAxis = true;
		viewModel.InvertYAxis = true;

		// Assert
		Assert.IsTrue(viewModel.MatchingPreset.Name.Contains("Custom Preset"));
	}

	[TestMethod]
	public void UpdateSelectedPreset_WhenSettingsChange_UpdatesSelectedPreset()
	{
		// Arrange
		var viewModel = CreateViewModel();
		var originalPreset = viewModel.SelectedPreset;

		// Act
		viewModel.Scale = 999.0f; // Change a setting

		// Assert
		Assert.AreNotEqual(originalPreset, viewModel.SelectedPreset);
		Assert.IsTrue(viewModel.IsSelectedPresetUnsaved);
	}

	#endregion Preset Matching Tests

	#region Command Tests

	[TestMethod]
	public void ConfirmCommand_ValidSettings_CanExecute()
	{
		// Arrange
		var viewModel = CreateViewModel();
		viewModel.Scale = 1.0f;

		// Act & Assert
		Assert.IsTrue(viewModel.ConfirmCommand.CanExecute(null));
	}

	[TestMethod]
	public void ConfirmCommand_InvalidSettings_CannotExecute()
	{
		// Arrange
		var viewModel = CreateViewModel();
		viewModel.Scale = 0.0f;

		// Act & Assert
		Assert.IsFalse(viewModel.ConfirmCommand.CanExecute(null));
	}

	[TestMethod]
	public void DeletePresetCommand_BuiltInPreset_CannotExecute()
	{
		// Arrange
		var viewModel = CreateViewModel();
		viewModel.SelectedPreset = viewModel.AvailablePresets.First(p => !p.IsCustom);

		// Act & Assert
		Assert.IsFalse(viewModel.DeletePresetCommand.CanExecute(null));
	}

	[TestMethod]
	public void DeletePresetCommand_CustomPreset_CanExecute()
	{
		// Arrange
		var customPreset = new IOGeometrySettingsPreset("Custom Test", new IOGeometrySettings(), true);
		var viewModel = CreateViewModel();

		viewModel.AvailablePresets.Add(customPreset);
		viewModel.SelectedPreset = customPreset;

		// Act & Assert
		Assert.IsTrue(viewModel.DeletePresetCommand.CanExecute(null));
	}

	[TestMethod]
	public void DeletePresetCommand_UserConfirms_DeletesPresetAndSavesConfig()
	{
		// Arrange
		var customPreset = new IOGeometrySettingsPreset("Custom Test", new IOGeometrySettings(), true);
		var viewModel = CreateViewModel();

		viewModel.AvailablePresets.Add(customPreset);
		viewModel.SelectedPreset = customPreset;

		_mockMessageService.Setup(x => x.ShowConfirmation(
			It.IsAny<string>(),
			It.IsAny<string>(),
			It.IsAny<bool?>(),
			It.IsAny<bool>()))
			.Returns(true);

		int originalPresetCount = viewModel.AvailablePresets.Count;

		// Act
		viewModel.DeletePresetCommand.Execute(null);

		// Assert
		Assert.AreEqual(originalPresetCount - 1, viewModel.AvailablePresets.Count);
		Assert.IsFalse(viewModel.AvailablePresets.Contains(customPreset));
		Assert.AreEqual(viewModel.AvailablePresets.First(), viewModel.SelectedPreset);

		_mockMessageService.Verify(x => x.ShowConfirmation(
			It.Is<string>(s => s.Contains("Custom Test")),
			It.IsAny<string>(),
			It.IsAny<bool?>(),
			It.IsAny<bool>()), Times.Once);

		_mockPresetIOService.Verify(x => x.SavePresets(
			TestCustomPresetPath,
			It.IsAny<IEnumerable<IOGeometrySettingsPreset>>()), Times.Once);
	}

	[TestMethod]
	public void DeletePresetCommand_UserCancels_DoesNotDeletePreset()
	{
		// Arrange
		var customPreset = new IOGeometrySettingsPreset("Custom Test", new IOGeometrySettings(), true);
		var viewModel = CreateViewModel();

		viewModel.AvailablePresets.Add(customPreset);
		viewModel.SelectedPreset = customPreset;

		_mockMessageService.Setup(x => x.ShowConfirmation(
			It.IsAny<string>(),
			It.IsAny<string>(),
			It.IsAny<bool?>(),
			It.IsAny<bool>()))
			.Returns(false);

		int originalPresetCount = viewModel.AvailablePresets.Count;

		// Act
		viewModel.DeletePresetCommand.Execute(null);

		// Assert
		Assert.AreEqual(originalPresetCount, viewModel.AvailablePresets.Count);
		Assert.IsTrue(viewModel.AvailablePresets.Contains(customPreset));
		Assert.AreEqual(customPreset, viewModel.SelectedPreset);

		_mockPresetIOService.Verify(x => x.SavePresets(
			It.IsAny<string>(),
			It.IsAny<IEnumerable<IOGeometrySettingsPreset>>()), Times.Never);
	}

	[TestMethod]
	public void SavePresetCommand_UserProvidesValidName_CreatesAndSavesNewPreset()
	{
		// Arrange
		var viewModel = CreateViewModel();
		viewModel.Scale = 2.5f;
		viewModel.InvertXAxis = true;

		var inputBoxViewModel = new InputBoxWindowViewModel(string.Empty, string.Empty, invalidNames: "-- Custom Preset --")
		{
			Value = "My Test Preset"
		};

		_mockDialogService.Setup(x => x.ShowDialog(viewModel, It.IsAny<InputBoxWindowViewModel>()))
			.Callback<object, object>((_, vm) => (vm as InputBoxWindowViewModel)!.Value = "My Test Preset")
			.Returns(true);

		int originalPresetCount = viewModel.AvailablePresets.Count;

		// Act
		viewModel.SavePresetCommand.Execute(null);

		// Assert
		Assert.AreEqual(originalPresetCount, viewModel.AvailablePresets.Count); // Should not change as unsaved preset is replaced

		var newPreset = viewModel.AvailablePresets.FirstOrDefault(p => p.Name == "My Test Preset");

		Assert.IsNotNull(newPreset);
		Assert.IsTrue(newPreset.IsCustom);
		Assert.AreEqual(2.5f, newPreset.Settings.Scale);
		Assert.IsTrue(newPreset.Settings.FlipX);
		Assert.AreEqual(newPreset, viewModel.SelectedPreset);

		_mockPresetIOService.Verify(x => x.SavePresets(
			TestCustomPresetPath,
			It.Is<IEnumerable<IOGeometrySettingsPreset>>(presets => presets.Any(p => p.Name == "My Test Preset"))),
			Times.Once);
	}

	[TestMethod]
	public void SavePresetCommand_UserCancelsDialog_DoesNotCreatePreset()
	{
		// Arrange
		var viewModel = CreateViewModel();

		_mockDialogService.Setup(x => x.ShowDialog(viewModel, It.IsAny<InputBoxWindowViewModel>()))
			.Returns(false);

		var originalCount = viewModel.AvailablePresets.Count;

		// Act
		viewModel.SavePresetCommand.Execute(null);

		// Assert
		Assert.AreEqual(originalCount, viewModel.AvailablePresets.Count);

		_mockPresetIOService.Verify(x => x.SavePresets(
			It.IsAny<string>(),
			It.IsAny<IEnumerable<IOGeometrySettingsPreset>>()), Times.Never);
	}

	[TestMethod]
	public void SavePresetCommand_UserProvidesEmptyName_DoesNotCreatePreset()
	{
		// Arrange
		var viewModel = CreateViewModel();

		_mockDialogService.Setup(x => x.ShowDialog(viewModel, It.IsAny<InputBoxWindowViewModel>()))
			.Callback<object, object>((_, vm) => (vm as InputBoxWindowViewModel)!.Value = string.Empty)
			.Returns(true);

		var originalCount = viewModel.AvailablePresets.Count;

		// Act
		viewModel.SavePresetCommand.Execute(null);

		// Assert
		Assert.AreEqual(originalCount, viewModel.AvailablePresets.Count);

		_mockPresetIOService.Verify(x => x.SavePresets(
			It.IsAny<string>(),
			It.IsAny<IEnumerable<IOGeometrySettingsPreset>>()), Times.Never);
	}

	[TestMethod]
	public void SavePresetCommand_UserProvidesWhitespaceOnlyName_DoesNotCreatePreset()
	{
		// Arrange
		var viewModel = CreateViewModel();

		_mockDialogService.Setup(x => x.ShowDialog(viewModel, It.IsAny<InputBoxWindowViewModel>()))
			.Callback<object, object>((_, vm) => (vm as InputBoxWindowViewModel)!.Value = "   ")
			.Returns(true);

		var originalCount = viewModel.AvailablePresets.Count;

		// Act
		viewModel.SavePresetCommand.Execute(null);

		// Assert
		Assert.AreEqual(originalCount, viewModel.AvailablePresets.Count);

		_mockPresetIOService.Verify(x => x.SavePresets(
			It.IsAny<string>(),
			It.IsAny<IEnumerable<IOGeometrySettingsPreset>>()), Times.Never);
	}

	[TestMethod]
	public void SavePresetCommand_PresetNameAlreadyExists_ShowsErrorAndDoesNotCreatePreset()
	{
		// Arrange
		var viewModel = CreateViewModel();
		var existingPresetName = viewModel.AvailablePresets.First().Name;

		_mockDialogService.Setup(x => x.ShowDialog(viewModel, It.IsAny<InputBoxWindowViewModel>()))
			.Callback<object, object>((_, vm) => (vm as InputBoxWindowViewModel)!.Value = existingPresetName)
			.Returns(true);

		var originalCount = viewModel.AvailablePresets.Count;

		// Act
		viewModel.SavePresetCommand.Execute(null);

		// Assert
		Assert.AreEqual(originalCount, viewModel.AvailablePresets.Count);

		_mockMessageService.Verify(x => x.ShowError(It.IsAny<string>(), It.IsAny<string>()), Times.Once);

		_mockPresetIOService.Verify(x => x.SavePresets(
			It.IsAny<string>(),
			It.IsAny<IEnumerable<IOGeometrySettingsPreset>>()), Times.Never);
	}

	[TestMethod]
	public void SavePresetCommand_CreatesPresetWithAllCurrentSettings()
	{
		// Arrange
		var viewModel = CreateViewModel();

		// Set various properties to test they're all captured
		viewModel.Scale = 2.5f;
		viewModel.SwapXYAxes = true;
		viewModel.SwapXZAxes = false;
		viewModel.SwapYZAxes = true;
		viewModel.InvertXAxis = false;
		viewModel.InvertYAxis = true;
		viewModel.InvertZAxis = false;
		viewModel.InvertFaces = true;
		viewModel.InvertVCoordinate = false;
		viewModel.UvMapped = true;
		viewModel.WrapUV = false;
		viewModel.PremultiplyUV = true;
		viewModel.VertexColorLight = false;
		viewModel.PackTextures = true;
		viewModel.PadPackedTextures = false;
		viewModel.SortByName = true;

		_mockDialogService.Setup(x => x.ShowDialog(viewModel, It.IsAny<InputBoxWindowViewModel>()))
			.Callback<object, object>((_, vm) => (vm as InputBoxWindowViewModel)!.Value = "Complete Test Preset")
			.Returns(true);

		// Act
		viewModel.SavePresetCommand.Execute(null);

		// Assert
		var newPreset = viewModel.AvailablePresets.FirstOrDefault(p => p.Name == "Complete Test Preset");
		Assert.IsNotNull(newPreset);

		var settings = newPreset.Settings;
		Assert.AreEqual(2.5f, settings.Scale);
		Assert.AreEqual(true, settings.SwapXY);
		Assert.AreEqual(false, settings.SwapXZ);
		Assert.AreEqual(true, settings.SwapYZ);
		Assert.AreEqual(false, settings.FlipX);
		Assert.AreEqual(true, settings.FlipY);
		Assert.AreEqual(false, settings.FlipZ);
		Assert.AreEqual(true, settings.InvertFaces);
		Assert.AreEqual(false, settings.FlipUV_V);
		Assert.AreEqual(true, settings.MappedUV);
		Assert.AreEqual(false, settings.WrapUV);
		Assert.AreEqual(true, settings.PremultiplyUV);
		Assert.AreEqual(false, settings.UseVertexColor);
		Assert.AreEqual(true, settings.PackTextures);
		Assert.AreEqual(false, settings.PadPackedTextures);
		Assert.AreEqual(true, settings.SortByName);
	}

	#endregion Command Tests

	#region Property Change Tests

	[TestMethod]
	public void PropertyChanges_UpdateSelectedPreset()
	{
		// Arrange
		var viewModel = CreateViewModel();
		var originalPreset = viewModel.SelectedPreset;

		// Act - Test various property changes
		viewModel.SwapXYAxes = !viewModel.SwapXYAxes;
		Assert.AreNotEqual(originalPreset, viewModel.SelectedPreset, "SwapXYAxes change should update preset.");

		// Reset and test another property
		viewModel.SelectedPreset = originalPreset;
		viewModel.InvertXAxis = !viewModel.InvertXAxis;
		Assert.AreNotEqual(originalPreset, viewModel.SelectedPreset, "InvertXAxis change should update preset.");

		// Reset and test scale
		viewModel.SelectedPreset = originalPreset;
		viewModel.Scale++;

		Assert.AreNotEqual(originalPreset, viewModel.SelectedPreset, "Scale change should update preset.");
	}

	[TestMethod]
	public void SelectedPresetChanged_UpdatesAllProperties()
	{
		// Arrange
		var viewModel = CreateViewModel();

		var testSettings = new IOGeometrySettings
		{
			Export = true,
			ExportRoom = true,
			ProcessGeometry = false,
			SwapXY = true,
			SwapXZ = true,
			FlipX = true,
			Scale = 5.0f,
			FlipUV_V = false,
			MappedUV = false,
			UseVertexColor = false,
			PackTextures = false
		};

		// Act
		viewModel.SelectedPreset = new IOGeometrySettingsPreset("Test Preset", testSettings);

		// Assert
		Assert.AreEqual(testSettings.Export, viewModel.IsExport);
		Assert.AreEqual(testSettings.ExportRoom, viewModel.IsRoomExport);
		Assert.AreEqual(testSettings.ProcessGeometry, viewModel.ProcessGeometry);
		Assert.AreEqual(testSettings.SwapXY, viewModel.SwapXYAxes);
		Assert.AreEqual(testSettings.SwapXZ, viewModel.SwapXZAxes);
		Assert.AreEqual(testSettings.FlipX, viewModel.InvertXAxis);
		Assert.AreEqual(testSettings.Scale, viewModel.Scale);
		Assert.AreEqual(testSettings.FlipUV_V, viewModel.InvertVCoordinate);
		Assert.AreEqual(testSettings.MappedUV, viewModel.UvMapped);
		Assert.AreEqual(testSettings.UseVertexColor, viewModel.VertexColorLight);
		Assert.AreEqual(testSettings.PackTextures, viewModel.PackTextures);
	}

	[TestMethod]
	public void PropertyChanged_NotifiesCorrectProperties()
	{
		// Arrange
		var viewModel = CreateViewModel();
		var propertyChangedEvents = new List<string?>();

		viewModel.PropertyChanged += (sender, e) => propertyChangedEvents.Add(e.PropertyName);

		// Act
		viewModel.Scale = 2.0f;

		// Assert
		Assert.IsTrue(propertyChangedEvents.Contains(nameof(viewModel.Scale)));
		Assert.IsTrue(propertyChangedEvents.Contains(nameof(viewModel.IsValid)));
		Assert.IsTrue(propertyChangedEvents.Contains(nameof(viewModel.MatchingPreset)));
	}

	[TestMethod]
	public void AllAxisProperties_CanBeSetAndRetrieved()
	{
		// Arrange
		var viewModel = CreateViewModel();

		// Act & Assert
		viewModel.SwapXYAxes = true;
		Assert.IsTrue(viewModel.SwapXYAxes);

		viewModel.SwapXZAxes = true;
		Assert.IsTrue(viewModel.SwapXZAxes);

		viewModel.SwapYZAxes = true;
		Assert.IsTrue(viewModel.SwapYZAxes);

		viewModel.InvertXAxis = true;
		Assert.IsTrue(viewModel.InvertXAxis);

		viewModel.InvertYAxis = true;
		Assert.IsTrue(viewModel.InvertYAxis);

		viewModel.InvertZAxis = false;
		Assert.IsFalse(viewModel.InvertZAxis);

		viewModel.InvertFaces = true;
		Assert.IsTrue(viewModel.InvertFaces);
	}

	[TestMethod]
	public void AllTextureProperties_CanBeSetAndRetrieved()
	{
		// Arrange
		var viewModel = CreateViewModel();

		// Act & Assert
		viewModel.InvertVCoordinate = false;
		Assert.IsFalse(viewModel.InvertVCoordinate);

		viewModel.UvMapped = false;
		Assert.IsFalse(viewModel.UvMapped);

		viewModel.WrapUV = false;
		Assert.IsFalse(viewModel.WrapUV);

		viewModel.PremultiplyUV = false;
		Assert.IsFalse(viewModel.PremultiplyUV);
	}

	[TestMethod]
	public void AllMiscProperties_CanBeSetAndRetrieved()
	{
		// Arrange
		var viewModel = CreateViewModel();

		// Act & Assert
		viewModel.VertexColorLight = false;
		Assert.IsFalse(viewModel.VertexColorLight);

		viewModel.PackTextures = false;
		Assert.IsFalse(viewModel.PackTextures);

		viewModel.PadPackedTextures = false;
		Assert.IsFalse(viewModel.PadPackedTextures);

		viewModel.SortByName = false;
		Assert.IsFalse(viewModel.SortByName);
	}

	#endregion Property Change Tests

	#region Unsaved Preset Management Tests

	[TestMethod]
	public void UnsavedPreset_CreatedWhenNeeded_RemovedWhenNotNeeded()
	{
		// Arrange
		var viewModel = CreateViewModel();
		int originalCount = viewModel.AvailablePresets.Count;

		// Act - Modify settings to create unsaved preset
		viewModel.Scale = 999.0f;
		int countWithUnsaved = viewModel.AvailablePresets.Count;

		// Switch back to a real preset
		viewModel.SelectedPreset = viewModel.AvailablePresets.First(p => p.Name != "-- Custom Preset --");
		int countAfterSwitch = viewModel.AvailablePresets.Count;

		// Assert
		Assert.AreEqual(originalCount + 1, countWithUnsaved, "Unsaved preset should be created.");
		Assert.AreEqual(originalCount, countAfterSwitch, "Unsaved preset should be removed.");
	}

	#endregion Unsaved Preset Management Tests

	#region Integration Tests

	[TestMethod]
	public void FindMatchingPreset_WithComplexSettingsCombination_FindsCorrectMatch()
	{
		// Arrange
		var viewModel = CreateViewModel(GeometryIOSettingsType.Import);

		// Use settings that match the "Metasequoia MQO unscaled" preset
		viewModel.Scale = 1.0f;
		viewModel.InvertZAxis = true;
		viewModel.InvertFaces = false;
		viewModel.InvertVCoordinate = false;
		viewModel.PremultiplyUV = true;
		viewModel.WrapUV = true;
		viewModel.VertexColorLight = true;

		// Act
		var matchingPreset = viewModel.MatchingPreset;

		// Assert
		Assert.IsNotNull(matchingPreset);
		Assert.AreEqual("Metasequoia MQO unscaled", matchingPreset.Name);
	}

	[TestMethod]
	public void GeometryIOSettingsType_DeterminesCorrectPresetLists()
	{
		// Test Export
		var exportViewModel = CreateViewModel(GeometryIOSettingsType.Export);
		Assert.AreEqual(IOSettingsPresets.GeometryExportSettingsPresets.Count, exportViewModel.AvailablePresets.Count);

		// Test Import
		var importViewModel = CreateViewModel(GeometryIOSettingsType.Import);
		Assert.AreEqual(IOSettingsPresets.GeometryImportSettingsPresets.Count, importViewModel.AvailablePresets.Count);

		// Test Animation Import
		var animViewModel = CreateViewModel(GeometryIOSettingsType.AnimationImport);
		Assert.AreEqual(IOSettingsPresets.AnimationSettingsPresets.Count, animViewModel.AvailablePresets.Count);
	}

	#endregion Integration Tests

	#region Helper Methods

	private GeometryIOSettingsWindowViewModel CreateViewModel(GeometryIOSettingsType type = GeometryIOSettingsType.Export)
	{
		return new GeometryIOSettingsWindowViewModel(
			type,
			TestCustomPresetPath,
			_mockPresetIOService.Object,
			_mockDialogService.Object,
			_mockMessageService.Object
		);
	}

	#endregion Helper Methods
}
