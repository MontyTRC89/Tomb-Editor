#nullable enable

using System.Collections.Generic;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MvvmDialogs;
using TombLib.LevelData;
using TombLib.Utils;
using TombLib.WPF;
using static TombLib.LevelData.TRVersion;
using LevelSettingsData = TombLib.LevelData.LevelSettings;

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
                OnPropertyChanged(nameof(IsTR4Native));
                OnPropertyChanged(nameof(IsTR5));
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
        public bool IsTR4Native => _settings.GameVersion.Native() == Game.TR4;
        public bool IsTR5 => _settings.GameVersion == Game.TR5;

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

        // Dialog commands.

        [RelayCommand]
        private void Apply() => _editor.UpdateLevelSettings(_settings.Clone());

        [RelayCommand]
        private void Ok()
        {
            _editor.UpdateLevelSettings(_settings.Clone());
            DialogResult = true;
        }

        [RelayCommand]
        private void Cancel() => DialogResult = false;

        // Helpers.

        private static System.Windows.Forms.IWin32Window Owner => WinFormsDialogHelper.GetOpenFormOwner();

        private void SetSetting<T>(T current, T value, System.Action<T> setter, [System.Runtime.CompilerServices.CallerMemberName] string? propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(current, value))
                return;
            setter(value);
            OnPropertyChanged(propertyName);
        }

        private static void SetIfBrowsed(string? result, string propertyName, System.Action<string> apply)
        {
            if (result != null)
                apply(result);
        }

        public sealed record PathVariableRow(string Variable, string Value);
    }
}
