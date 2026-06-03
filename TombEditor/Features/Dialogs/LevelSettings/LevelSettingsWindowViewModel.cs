#nullable enable

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Numerics;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MvvmDialogs;
using TombLib.Controls;
using TombLib.LevelData;
using TombLib.Utils;
using TombLib.Wad.Catalog;
using TombLib.WPF;
using static TombLib.LevelData.TRVersion;
using LevelSettingsData = TombLib.LevelData.LevelSettings;
using WpfColor = System.Windows.Media.Color;

namespace TombEditor.Features.Dialogs.LevelSettings
{
    /// <summary>
    /// WPF port of <c>FormLevelSettings</c> (phase 1: shell + Game + Paths tabs). Edits a clone of
    /// the level's <see cref="LevelSettingsData"/> and applies it via <c>Editor.UpdateLevelSettings</c>
    /// on Apply/OK, mirroring the WinForms form. The remaining tabs (Textures, Objects, sounds,
    /// imported geometry, static meshes, sky/font, misc) are ported in later phases.
    /// </summary>
    public partial class LevelSettingsWindowViewModel : ObservableObject, IModalDialogViewModel
    {
        private readonly Editor _editor;
        private readonly LevelSettingsData _settings;

        [ObservableProperty] private bool? _dialogResult;

        public IReadOnlyList<Game> GameVersions { get; } = AllVersions.ToList();
        public IReadOnlyList<PathVariableRow> PathVariables { get; }

        public LevelSettingsWindowViewModel(Editor editor)
        {
            _editor = editor;
            _settings = editor.Level.Settings.Clone();

            PathVariables = System.Enum.GetValues(typeof(VariableType))
                .Cast<VariableType>()
                .Select(t => LevelSettingsData.VariableCreate(t))
                .Select(name => new PathVariableRow(name, _settings.ParseVariables(name)))
                .ToList();

            BuildStaticMeshMerges();
        }

        // Game tab.

        public Game GameVersion
        {
            get => _settings.GameVersion;
            set
            {
                if (_settings.GameVersion == value)
                    return;
                _settings.GameVersion = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsTRNG));
                OnPropertyChanged(nameof(IsTombEngine));
                OnPropertyChanged(nameof(IsNotTombEngine));
                OnPropertyChanged(nameof(IsTR4Native));
                OnPropertyChanged(nameof(IsTR5));
                OnPropertyChanged(nameof(Supports16BitDithering));
                OnPropertyChanged(nameof(SupportsFontAndSky));
            }
        }

        public string GameExecutableFilePath { get => _settings.GameExecutableFilePath; set => SetSetting(_settings.GameExecutableFilePath, value, v => _settings.GameExecutableFilePath = v); }
        public string GameLevelFilePath { get => _settings.GameLevelFilePath; set => SetSetting(_settings.GameLevelFilePath, value, v => _settings.GameLevelFilePath = v); }
        public string ScriptDirectory { get => _settings.ScriptDirectory; set => SetSetting(_settings.ScriptDirectory, value, v => _settings.ScriptDirectory = v); }
        public string TenLuaScriptFile { get => _settings.TenLuaScriptFile; set => SetSetting(_settings.TenLuaScriptFile, value, v => _settings.TenLuaScriptFile = v); }

        public bool GameEnableQuickStartFeature { get => _settings.GameEnableQuickStartFeature; set => SetSetting(_settings.GameEnableQuickStartFeature, value, v => _settings.GameEnableQuickStartFeature = v); }
        public bool GameEnableExtraReverbPresets { get => _settings.GameEnableExtraReverbPresets; set => SetSetting(_settings.GameEnableExtraReverbPresets, value, v => _settings.GameEnableExtraReverbPresets = v); }
        public bool GameEnableExtraBlendingModes { get => _settings.GameEnableExtraBlendingModes ?? false; set => SetSetting(_settings.GameEnableExtraBlendingModes ?? false, value, v => _settings.GameEnableExtraBlendingModes = v); }

        // Paths tab.

        public string LevelFilePath { get => _settings.LevelFilePath; set => SetSetting(_settings.LevelFilePath, value, v => _settings.LevelFilePath = v); }
        public string GameDirectory { get => _settings.GameDirectory; set => SetSetting(_settings.GameDirectory, value, v => _settings.GameDirectory = v); }

        // Version-dependent visibility.

        public bool IsTRNG => _settings.GameVersion == Game.TRNG;
        public bool IsTombEngine => _settings.GameVersion == Game.TombEngine;
        public bool IsNotTombEngine => _settings.GameVersion != Game.TombEngine;
        public bool IsTR4Native => _settings.GameVersion.Native() == Game.TR4;
        public bool IsTR5 => _settings.GameVersion == Game.TR5;
        public bool Supports16BitDithering => _settings.GameVersion.Supports16BitDithering();
        public bool SupportsFontAndSky => _settings.GameVersion.SupportsFontAndSkySettings();

        // Sky & font tab.

        public IReadOnlyList<Tr5WeatherType> Tr5WeatherTypes { get; } = Enum.GetValues(typeof(Tr5WeatherType)).Cast<Tr5WeatherType>().ToList();
        public IReadOnlyList<Tr5LaraType> Tr5LaraTypes { get; } = Enum.GetValues(typeof(Tr5LaraType)).Cast<Tr5LaraType>().ToList();

        public Tr5WeatherType Tr5WeatherType { get => _settings.Tr5WeatherType; set => SetSetting(_settings.Tr5WeatherType, value, v => _settings.Tr5WeatherType = v); }
        public Tr5LaraType Tr5LaraType { get => _settings.Tr5LaraType; set => SetSetting(_settings.Tr5LaraType, value, v => _settings.Tr5LaraType = v); }

        public bool FontUseCustom
        {
            get => !string.IsNullOrEmpty(_settings.FontTextureFilePath);
            set => ToggleCustomResource(value, "Font.pc", () => _settings.FontTextureFilePath, p => _settings.FontTextureFilePath = p, nameof(FontUseCustom), nameof(FontTextureFilePath));
        }
        public string FontTextureFilePath { get => _settings.FontTextureFilePath ?? string.Empty; set => SetOptionalPath(value, () => _settings.FontTextureFilePath, p => _settings.FontTextureFilePath = p, nameof(FontTextureFilePath)); }

        public bool SkyUseCustom
        {
            get => !string.IsNullOrEmpty(_settings.SkyTextureFilePath);
            set => ToggleCustomResource(value, "pcsky.raw", () => _settings.SkyTextureFilePath, p => _settings.SkyTextureFilePath = p, nameof(SkyUseCustom), nameof(SkyTextureFilePath));
        }
        public string SkyTextureFilePath { get => _settings.SkyTextureFilePath ?? string.Empty; set => SetOptionalPath(value, () => _settings.SkyTextureFilePath, p => _settings.SkyTextureFilePath = p, nameof(SkyTextureFilePath)); }

        public bool Tr5SpritesUseCustom
        {
            get => !string.IsNullOrEmpty(_settings.Tr5ExtraSpritesFilePath);
            set => ToggleCustomResource(value, "Extra.Tr5.pc", () => _settings.Tr5ExtraSpritesFilePath, p => _settings.Tr5ExtraSpritesFilePath = p, nameof(Tr5SpritesUseCustom), nameof(Tr5ExtraSpritesFilePath));
        }
        public string Tr5ExtraSpritesFilePath { get => _settings.Tr5ExtraSpritesFilePath ?? string.Empty; set => SetOptionalPath(value, () => _settings.Tr5ExtraSpritesFilePath, p => _settings.Tr5ExtraSpritesFilePath = p, nameof(Tr5ExtraSpritesFilePath)); }

        // Misc tab.

        public IReadOnlyList<string> SampleRates { get; } = new[] { "11025", "22050", "44100", "48000" };
        public IReadOnlyList<string> LightQualities { get; } = new[] { "Low", "Medium", "High" };

        public double TexturePadding { get => _settings.TexturePadding; set => SetSetting<double>(_settings.TexturePadding, value, v => _settings.TexturePadding = (int)v); }
        public bool Dither16BitTextures { get => _settings.Dither16BitTextures; set => SetSetting(_settings.Dither16BitTextures, value, v => _settings.Dither16BitTextures = v); }
        public bool CompressTextures { get => _settings.CompressTextures; set => SetSetting(_settings.CompressTextures, value, v => _settings.CompressTextures = v); }
        public bool AgressiveTexturePacking { get => _settings.AgressiveTexturePacking; set => SetSetting(_settings.AgressiveTexturePacking, value, v => _settings.AgressiveTexturePacking = v); }
        public bool AgressiveFloordataPacking { get => _settings.AgressiveFloordataPacking; set => SetSetting(_settings.AgressiveFloordataPacking, value, v => _settings.AgressiveFloordataPacking = v); }
        public bool Room32BitLighting { get => _settings.Room32BitLighting; set => SetSetting(_settings.Room32BitLighting, value, v => _settings.Room32BitLighting = v); }
        public bool RemapAnimatedTextures { get => _settings.RemapAnimatedTextures; set => SetSetting(_settings.RemapAnimatedTextures, value, v => _settings.RemapAnimatedTextures = v); }
        public bool RearrangeVerticalRooms { get => _settings.RearrangeVerticalRooms; set => SetSetting(_settings.RearrangeVerticalRooms, value, v => _settings.RearrangeVerticalRooms = v); }
        public bool RemoveUnusedObjects { get => _settings.RemoveUnusedObjects; set => SetSetting(_settings.RemoveUnusedObjects, value, v => _settings.RemoveUnusedObjects = v); }
        public bool OverrideIndividualLightQualitySettings { get => _settings.OverrideIndividualLightQualitySettings; set => SetSetting(_settings.OverrideIndividualLightQualitySettings, value, v => _settings.OverrideIndividualLightQualitySettings = v); }
        public bool AutoAssignSoundsIfNoSelection { get => _settings.AutoAssignSoundsIfNoSelection; set => SetSetting(_settings.AutoAssignSoundsIfNoSelection, value, v => _settings.AutoAssignSoundsIfNoSelection = v); }

        public bool EnableCustomSampleRate
        {
            get => _settings.EnableCustomSampleRate;
            set { if (SetSetting(_settings.EnableCustomSampleRate, value, v => _settings.EnableCustomSampleRate = v)) OnPropertyChanged(nameof(CanEditSampleRate)); }
        }
        public bool CanEditSampleRate => _settings.EnableCustomSampleRate;

        public int DefaultLightQualityIndex
        {
            get => _settings.DefaultLightQuality == LightQuality.Default ? 0 : (int)_settings.DefaultLightQuality - 1;
            set { _settings.DefaultLightQuality = (LightQuality)(value + 1); OnPropertyChanged(); }
        }

        public string SelectedSampleRate
        {
            get => _settings.CustomSampleRate.ToString();
            set { if (int.TryParse(value, out int rate)) { _settings.CustomSampleRate = rate; OnPropertyChanged(); } }
        }

        public WpfColor AmbientLightColor => (_settings.DefaultAmbientLight * 0.5f).ToWPFColor();

        [RelayCommand]
        private void PickAmbientLight()
        {
            using var dialog = new RealtimeColorDialog
            {
                Color = (_settings.DefaultAmbientLight * 0.5f).ToWinFormsColor(),
                FullOpen = true
            };
            if (dialog.ShowDialog(Owner) != System.Windows.Forms.DialogResult.OK)
                return;

            _settings.DefaultAmbientLight = dialog.Color.ToFloat3Color() * 2.0f;
            OnPropertyChanged(nameof(AmbientLightColor));
        }

        // Browse commands.

        [RelayCommand]
        private void BrowseLevelFile()
            => SetIfBrowsed(LevelFileDialog.BrowseFile(Owner, _settings, _settings.LevelFilePath, "Select the level name", LevelSettingsData.FileFormatsLevel, null, true), nameof(LevelFilePath), v => LevelFilePath = v);

        [RelayCommand]
        private void BrowseGameDirectory()
            => SetIfBrowsed(LevelFileDialog.BrowseFolder(Owner, _settings, _settings.GameDirectory, "Select the game folder (should contain game .exe file)", VariableType.LevelDirectory), nameof(GameDirectory), v => GameDirectory = v);

        [RelayCommand]
        private void BrowseGameLevelFile()
            => SetIfBrowsed(LevelFileDialog.BrowseFile(Owner, _settings, _settings.GameLevelFilePath, "Select place for compiled level", LevelSettingsData.FileFormatsLevelCompiled, VariableType.GameDirectory, true), nameof(GameLevelFilePath), v => GameLevelFilePath = v);

        [RelayCommand]
        private void BrowseGameExecutable()
            => SetIfBrowsed(LevelFileDialog.BrowseFile(Owner, _settings, _settings.GameExecutableFilePath, "Select an executable", new[] { new FileFormat("Windows executables", "exe") }, VariableType.GameDirectory, false), nameof(GameExecutableFilePath), v => GameExecutableFilePath = v);

        [RelayCommand]
        private void BrowseScript()
            => SetIfBrowsed(LevelFileDialog.BrowseFolder(Owner, _settings, _settings.ScriptDirectory, "Select the script TXT files folder", VariableType.LevelDirectory), nameof(ScriptDirectory), v => ScriptDirectory = v);

        [RelayCommand]
        private void BrowseLua()
            => SetIfBrowsed(LevelFileDialog.BrowseFile(Owner, _settings, _settings.TenLuaScriptFile, "Select the Lua script file", new[] { new FileFormat("Lua script", "lua") }, VariableType.LevelDirectory, false), nameof(TenLuaScriptFile), v => TenLuaScriptFile = v);

        [RelayCommand]
        private void BrowseFont() => BrowseResource(() => _settings.FontTextureFilePath, p => _settings.FontTextureFilePath = p, "Select a font texture", nameof(FontTextureFilePath), nameof(FontUseCustom));

        [RelayCommand]
        private void BrowseSky() => BrowseResource(() => _settings.SkyTextureFilePath, p => _settings.SkyTextureFilePath = p, "Select a sky texture", nameof(SkyTextureFilePath), nameof(SkyUseCustom));

        [RelayCommand]
        private void BrowseTr5Sprites() => BrowseResource(() => _settings.Tr5ExtraSpritesFilePath, p => _settings.Tr5ExtraSpritesFilePath = p, "Select a TR5 extra sprites texture", nameof(Tr5ExtraSpritesFilePath), nameof(Tr5SpritesUseCustom));

        // Static meshes tab.

        public ObservableCollection<StaticMeshMergeRow> StaticMeshMerges { get; } = new();

        [RelayCommand]
        private void SelectAllStatics() => ToggleStatics(true, bypassShatterable: false);

        [RelayCommand]
        private void SelectAllButShatterStatics() => ToggleStatics(true, bypassShatterable: true);

        [RelayCommand]
        private void DeselectAllStatics() => ToggleStatics(false, bypassShatterable: false);

        private void ToggleStatics(bool value, bool bypassShatterable)
        {
            foreach (var row in StaticMeshMerges)
            {
                if (value && bypassShatterable && TrCatalog.IsStaticShatterable(_settings.GameVersion, row.MeshId))
                {
                    row.Merge = false;
                    continue;
                }

                row.Merge = value;
                if (!value)
                    row.InterpretShadesAsEffect = false;
            }
        }

        private void BuildStaticMeshMerges()
        {
            StaticMeshMerges.Clear();
            foreach (var staticMesh in _settings.WadGetAllStatics())
            {
                uint typeId = staticMesh.Value.Id.TypeId;
                var existing = _settings.AutoStaticMeshMerges.FirstOrDefault(e => e.meshId.Equals(typeId));
                StaticMeshMerges.Add(new StaticMeshMergeRow(existing ?? new AutoStaticMeshMergeEntry(typeId, false, false, false, false, _settings)));
            }
        }

        // Dialog commands.

        [RelayCommand]
        private void Apply()
        {
            CommitCollections();
            _editor.UpdateLevelSettings(_settings.Clone());
        }

        [RelayCommand]
        private void Ok()
        {
            CommitCollections();
            _editor.UpdateLevelSettings(_settings.Clone());
            DialogResult = true;
        }

        [RelayCommand]
        private void Cancel() => DialogResult = false;

        // Helpers.

        private static System.Windows.Forms.IWin32Window Owner => WinFormsDialogHelper.GetOpenFormOwner();

        private void CommitCollections()
        {
            _settings.AutoStaticMeshMerges.Clear();
            foreach (var row in StaticMeshMerges)
                if (row.Merge)
                    _settings.AutoStaticMeshMerges.Add(row.Entry.Clone()); // Clone(), otherwise merge entries won't update
        }

        private string GetLevelResourcePath(string file)
            => LevelSettingsData.VariableCreate(VariableType.LevelDirectory) + LevelSettingsData.Dir + file;

        private void ToggleCustomResource(bool useCustom, string defaultFile, Func<string?> get, Action<string?> set, string customProp, string pathProp)
        {
            if (useCustom == !string.IsNullOrEmpty(get()))
                return;
            set(useCustom ? GetLevelResourcePath(defaultFile) : null);
            OnPropertyChanged(customProp);
            OnPropertyChanged(pathProp);
        }

        private void SetOptionalPath(string value, Func<string?> get, Action<string?> set, string pathProp)
        {
            if (string.IsNullOrEmpty(get()) || string.IsNullOrEmpty(value) || get() == value)
                return; // auto mode, or no real change
            set(value);
            OnPropertyChanged(pathProp);
        }

        private void BrowseResource(Func<string?> get, Action<string?> set, string title, string pathProp, string customProp)
        {
            string? result = LevelFileDialog.BrowseFile(Owner, _settings, get(), title, LevelSettingsData.FileFormatsLoadRawExtraTexture, VariableType.LevelDirectory, false);
            if (result == null)
                return;
            set(result);
            OnPropertyChanged(pathProp);
            OnPropertyChanged(customProp);
        }

        private bool SetSetting<T>(T current, T value, System.Action<T> setter, [System.Runtime.CompilerServices.CallerMemberName] string? propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(current, value))
                return false;
            setter(value);
            OnPropertyChanged(propertyName);
            return true;
        }

        private static void SetIfBrowsed(string? result, string propertyName, System.Action<string> apply)
        {
            if (result != null)
                apply(result);
        }

        public sealed record PathVariableRow(string Variable, string Value);

        public partial class StaticMeshMergeRow : ObservableObject
        {
            public AutoStaticMeshMergeEntry Entry { get; }

            public StaticMeshMergeRow(AutoStaticMeshMergeEntry entry) => Entry = entry;

            public string StaticMesh => Entry.StaticMesh;
            public uint MeshId => Entry.meshId;

            public bool Merge { get => Entry.Merge; set { if (Entry.Merge == value) return; Entry.Merge = value; OnPropertyChanged(); } }
            public bool InterpretShadesAsEffect { get => Entry.InterpretShadesAsEffect; set { if (Entry.InterpretShadesAsEffect == value) return; Entry.InterpretShadesAsEffect = value; OnPropertyChanged(); } }
            public bool TintAsAmbient { get => Entry.TintAsAmbient; set { if (Entry.TintAsAmbient == value) return; Entry.TintAsAmbient = value; OnPropertyChanged(); } }
            public bool ClearShades { get => Entry.ClearShades; set { if (Entry.ClearShades == value) return; Entry.ClearShades = value; OnPropertyChanged(); } }
        }
    }
}
