#nullable enable

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Numerics;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MvvmDialogs;
using TombLib.Forms;
using TombLib.Forms.ViewModels;
using TombLib.Forms.Views;
using TombLib.GeometryIO;
using TombLib.LevelData;
using TombLib.Utils;
using TombLib.WPF;
using TombLib.WPF.Services;
using TombLib.WPF.Services.Abstract;
using TombEditor.Features.Dialogs.LevelSettings;
using LevelSettingsData = TombLib.LevelData.LevelSettings;
using WpfColor = System.Windows.Media.Color;

namespace TombEditor.Features.Dialogs.ImportedGeometryEditor
{
    /// <summary>
    /// WPF port of <c>FormImportedGeometry</c>: edits an <see cref="ImportedGeometryInstance"/> (assigned
    /// model, lighting model, color, flags) and lets the user manage the level's imported geometry list
    /// on a clone of the settings. On OK the caller applies <see cref="NewLevelSettings"/> and the
    /// modified instance, mirroring the WinForms dialog.
    /// </summary>
    public partial class ImportedGeometryWindowViewModel : ObservableObject, IModalDialogViewModel
    {
        private readonly ImportedGeometryInstance _instance;
        private readonly IMessageService _messageService;
        private readonly Vector3 _oldColor;
        private ImportedGeometry.UniqueIDType? _currentModel;

        public LevelSettingsData OldLevelSettings { get; }
        public LevelSettingsData NewLevelSettings { get; }

        public ObservableCollection<ImportedGeometryRow> Geometries { get; } = new();
        public IReadOnlyList<ImportedGeometryLightingModel> LightingModels { get; } =
            Enum.GetValues(typeof(ImportedGeometryLightingModel)).Cast<ImportedGeometryLightingModel>().ToList();

        [ObservableProperty] private bool? _dialogResult;
        [ObservableProperty] private ImportedGeometryRow? _selectedGeometry;
        [ObservableProperty] private string _assignedModelDisplay = "None";
        [ObservableProperty] private ImportedGeometryLightingModel _lightingModel;
        [ObservableProperty] private WpfColor _color;
        [ObservableProperty] private bool _sharpEdges;
        [ObservableProperty] private bool _hidden;
        [ObservableProperty] private bool _useAlphaTest;

        public bool IsTombEngine => OldLevelSettings.GameVersion == TRVersion.Game.TombEngine;

        public ImportedGeometryWindowViewModel(ImportedGeometryInstance instance, LevelSettingsData levelSettings)
        {
            _instance = instance;
            _messageService = ServiceLocator.ResolveService<IMessageService>();

            OldLevelSettings = levelSettings;
            NewLevelSettings = levelSettings.Clone();

            _currentModel = instance.Model?.UniqueID;
            _oldColor = instance.Color;

            _lightingModel = instance.LightingModel;
            _color = (instance.Color * 0.5f).ToWPFColor();
            _sharpEdges = instance.SharpEdges;
            _hidden = instance.Hidden;
            _useAlphaTest = IsTombEngine && instance.UseAlphaTestInsteadOfAlphaBlend;

            foreach (var geometry in NewLevelSettings.ImportedGeometries)
                Geometries.Add(new ImportedGeometryRow(NewLevelSettings, geometry));

            if (_currentModel != null)
                SelectedGeometry = Geometries.FirstOrDefault(g => g.Object.UniqueID == _currentModel);

            UpdateAssignedModelDisplay();
        }

        private void UpdateAssignedModelDisplay()
        {
            var model = NewLevelSettings.ImportedGeometryFromID(_currentModel);
            AssignedModelDisplay = model == null ? "None" : model.Info.Name + "   (" + model.Info.Path + ")";
        }

        [RelayCommand]
        private void Assign()
        {
            if (SelectedGeometry == null)
                return;
            _currentModel = SelectedGeometry.Object.UniqueID;
            UpdateAssignedModelDisplay();
        }

        [RelayCommand]
        private void AddImportedGeometry()
        {
            var paths = LevelFileDialog.BrowseFiles(Owner, NewLevelSettings, NewLevelSettings.LevelFilePath, "Select 3D files that you want to see imported.", BaseGeometryImporter.FileExtensions, VariableType.LevelDirectory).ToList();
            var importInfos = new List<KeyValuePair<ImportedGeometry, ImportedGeometryInfo>>();
            var config = Editor.Instance?.Configuration;

            foreach (string path in paths)
            {
                var ioViewModel = new GeometryIOSettingsWindowViewModel(IOSettingsPresets.GeometryImportSettingsPresets);
                ioViewModel.SelectPreset(config?.GeometryIO_LastUsedGeometryImportPresetName);

                var ioDialog = new GeometryIOSettingsWindow { DataContext = ioViewModel };
                ioDialog.SetOwner(Owner);
                ioDialog.ShowDialog();

                if (ioViewModel.DialogResult != true)
                    continue;

                if (config != null)
                    config.GeometryIO_LastUsedGeometryImportPresetName = ioViewModel.SelectedPreset?.Name;

                var info = new ImportedGeometryInfo(NewLevelSettings.MakeRelative(path, VariableType.LevelDirectory), ioViewModel.GetCurrentSettings());
                importInfos.Add(new KeyValuePair<ImportedGeometry, ImportedGeometryInfo>(new ImportedGeometry(), info));
            }

            if (importInfos.Count == 0)
                return;

            NewLevelSettings.ImportedGeometryUpdate(importInfos);
            NewLevelSettings.ImportedGeometries.AddRange(importInfos.Select(e => e.Key));
            foreach (var entry in importInfos)
                Geometries.Add(new ImportedGeometryRow(NewLevelSettings, entry.Key));
        }

        [RelayCommand]
        private void DeleteImportedGeometry()
        {
            if (SelectedGeometry == null)
                return;
            NewLevelSettings.ImportedGeometries.Remove(SelectedGeometry.Object);
            Geometries.Remove(SelectedGeometry);
        }

        [RelayCommand]
        private void PickColor()
        {
            EditorActions.EditColor(Owner, _instance, newColor => Color = newColor.ToWPFColor());
        }

        [RelayCommand]
        private void EditMaterials()
        {
            var model = NewLevelSettings.ImportedGeometryFromID(_currentModel);
            if (model == null)
            {
                _messageService.ShowError("You need to assign a model before opening the Material Editor.");
                return;
            }

            if (model.Textures.Count == 0 || model.LoadException != null)
            {
                _messageService.ShowError("Assigned model has no textures or was not loaded.");
                return;
            }

            using var form = new FormMaterialEditor(model.Textures, Editor.Instance.Configuration);
            form.ShowDialog();
        }

        [RelayCommand]
        private void Ok()
        {
            _instance.Model = OldLevelSettings.ImportedGeometryFromID(_currentModel) ?? NewLevelSettings.ImportedGeometryFromID(_currentModel);
            _instance.LightingModel = LightingModel;
            _instance.Color = new Vector3(Color.R / 255.0f, Color.G / 255.0f, Color.B / 255.0f) * 2.0f;
            _instance.SharpEdges = SharpEdges;
            _instance.Hidden = Hidden;
            _instance.UseAlphaTestInsteadOfAlphaBlend = UseAlphaTest;
            DialogResult = true;
        }

        [RelayCommand]
        private void Cancel()
        {
            _instance.Color = _oldColor;
            DialogResult = false;
        }

        private static System.Windows.Forms.IWin32Window Owner => WinFormsDialogHelper.GetOpenFormOwner();
    }
}
