#nullable enable

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Reflection;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MvvmDialogs;
using TombLib.LevelData;
using TombLib.Utils;
using TombLib.WPF;
using TombLib.WPF.Services;
using TombLib.WPF.Services.Abstract;
using WpfColor = System.Windows.Media.Color;

namespace TombEditor.Features.Dialogs.Options
{
    /// <summary>
    /// WPF port of <c>FormOptions</c> / <c>FormOptionsBase</c>. The dialog is data-driven: a schema of
    /// tabs -> groups -> <see cref="OptionItem"/>s is rendered by the view, and values are read from /
    /// written to <see cref="Configuration"/> by reflection on each option's <see cref="OptionItem.ConfigName"/>
    /// (the same convention the WinForms control <c>Tag</c> used), including the <c>UI_ColorScheme</c>
    /// sub-fields for the color options.
    /// </summary>
    public partial class OptionsWindowViewModel : ObservableObject, IModalDialogViewModel
    {
        private const string ColorSchemeProperty = "UI_ColorScheme";

        private readonly Editor _editor;
        private readonly object _config;
        private readonly IColorPickerService _colorPickerService;

        public ObservableCollection<OptionTab> Tabs { get; } = new();

        [ObservableProperty] private OptionTab? _selectedTab;
        [ObservableProperty] private bool? _dialogResult;

        private string _searchText = string.Empty;
        public string SearchText
        {
            get => _searchText;
            set { if (SetProperty(ref _searchText, value)) ApplyFilter(); }
        }

        public OptionsWindowViewModel(Editor editor, IColorPickerService? colorPickerService = null)
        {
            _editor = editor;
            _config = editor.Configuration;
            _colorPickerService = ServiceLocator.ResolveService(colorPickerService);

            BuildSchema();
            ReadConfigIntoItems(_config);
            SelectedTab = Tabs.FirstOrDefault();
        }

        // Commands.

        [RelayCommand]
        private void Apply()
        {
            WriteConfigFromItems();
            _editor.ConfigurationChange();
        }

        [RelayCommand]
        private void Ok()
        {
            WriteConfigFromItems();
            _editor.ConfigurationChange();
            DialogResult = true;
        }

        [RelayCommand]
        private void Cancel() => DialogResult = false;

        [RelayCommand]
        private void PickColor(OptionItem item)
        {
            var current = item.Value is WpfColor c ? c : WpfColor.FromRgb(0, 0, 0);

            Vector3? picked = _colorPickerService.PickColor(current.ToFloat3Color());
            if (picked.HasValue)
                item.Value = picked.Value.ToWPFColor();
        }

        [RelayCommand]
        private void PageDefaults()
        {
            if (SelectedTab is null)
                return;

            var defaults = Activator.CreateInstance(_config.GetType())!;
            ReadConfigIntoItems(defaults, SelectedTab);
        }

        // Reflection get / set (mirrors FormOptionsBase, with the UI_ColorScheme field fallback).

        private object? GetOption(object config, string name)
        {
            var prop = config.GetType().GetProperty(name);
            if (prop != null)
                return prop.GetValue(config);

            var scheme = config.GetType().GetProperty(ColorSchemeProperty)?.GetValue(config);
            return scheme?.GetType().GetField(name)?.GetValue(scheme);
        }

        private void SetOption(string name, object? value)
        {
            var prop = _config.GetType().GetProperty(name);
            if (prop != null)
            {
                prop.SetValue(_config, value);
                return;
            }

            var scheme = _config.GetType().GetProperty(ColorSchemeProperty)?.GetValue(_config);
            scheme?.GetType().GetField(name)?.SetValue(scheme, value);
        }

        private void ReadConfigIntoItems(object config, OptionTab? onlyTab = null)
        {
            var items = (onlyTab?.AllItems ?? Tabs.SelectMany(t => t.AllItems));

            foreach (var item in items)
            {
                if (item.Kind == OptionKind.ColorSchemePreset)
                    continue;

                var option = GetOption(config, item.ConfigName);
                if (option == null)
                    continue;

                switch (option)
                {
                    case bool b:
                        item.Value = b;
                        break;
                    case string s:
                        item.Value = item.Kind == OptionKind.Color ? (object)ParseHtmlColor(s) : s;
                        break;
                    case TRVersion.Game game:
                        item.Value = game;
                        break;
                    case int i:
                        item.Value = item.Kind == OptionKind.Combo ? ItemAt(item, i) : (object)(double)i;
                        break;
                    case float f:
                        item.Value = (double)f;
                        break;
                    case Vector4 v:
                        item.Value = v.ToWPFColor();
                        break;
                }
            }
        }

        private void WriteConfigFromItems()
        {
            foreach (var item in Tabs.SelectMany(t => t.AllItems))
            {
                if (item.Kind == OptionKind.ColorSchemePreset)
                    continue;

                var option = GetOption(_config, item.ConfigName);
                if (option == null)
                    continue;

                switch (option)
                {
                    case bool when item.Value is bool b:
                        SetOption(item.ConfigName, b);
                        break;
                    case string when item.Kind == OptionKind.Color && item.Value is WpfColor color:
                        SetOption(item.ConfigName, ToHtmlColor(color));
                        break;
                    case string when item.Value is string s:
                        SetOption(item.ConfigName, s);
                        break;
                    case TRVersion.Game when item.Value is TRVersion.Game game:
                        SetOption(item.ConfigName, game);
                        break;
                    case int when item.Kind == OptionKind.Combo:
                        SetOption(item.ConfigName, IndexOf(item, item.Value));
                        break;
                    case int when item.Value is double d:
                        SetOption(item.ConfigName, (int)d);
                        break;
                    case float when item.Value is double d:
                        SetOption(item.ConfigName, (float)d);
                        break;
                    case Vector4 original when item.Value is WpfColor color:
                        SetOption(item.ConfigName, new Vector4(color.R / 255.0f, color.G / 255.0f, color.B / 255.0f, original.W));
                        break;
                }
            }
        }

        // Color scheme presets.

        public void ApplyColorSchemePreset(string presetName)
        {
            var scheme = _config.GetType().GetProperty(ColorSchemeProperty)?.GetValue(_config);
            if (scheme is null)
                return;

            var presetField = scheme.GetType().GetFields()
                .FirstOrDefault(f => f.FieldType == scheme.GetType() && f.Name == presetName);
            if (presetField is null)
                return;

            var preset = presetField.GetValue(null);
            if (preset is null)
                return;

            foreach (var item in Tabs.SelectMany(t => t.AllItems).Where(o => o.Kind == OptionKind.Color))
            {
                var field = preset.GetType().GetField(item.ConfigName);
                if (field != null && field.GetValue(preset) is Vector4 v)
                    item.Value = v.ToWPFColor();
            }
        }

        // Search filtering.

        private void ApplyFilter()
        {
            string query = (_searchText ?? string.Empty).Trim();

            foreach (var tab in Tabs)
            {
                bool anyTab = false;
                foreach (var group in tab.Groups)
                {
                    bool anyGroup = false;
                    foreach (var item in group.Items)
                    {
                        bool match = query.Length == 0 || item.Label.IndexOf(query, StringComparison.OrdinalIgnoreCase) >= 0;
                        item.IsVisible = match;
                        anyGroup |= match;
                    }
                    group.IsVisible = anyGroup;
                    anyTab |= anyGroup;
                }
                tab.IsVisible = anyTab;
            }
        }

        private static object? ItemAt(OptionItem item, int index)
            => item.Items != null && index >= 0 && index < item.Items.Count ? item.Items[index] : null;

        private static int IndexOf(OptionItem item, object? value)
        {
            if (item.Items == null || value == null)
                return 0;
            int index = item.Items.ToList().IndexOf(value);
            return index < 0 ? 0 : index;
        }

        private static WpfColor ParseHtmlColor(string html)
        {
            try
            {
                var c = ColorTranslator.FromHtml(html);
                return WpfColor.FromRgb(c.R, c.G, c.B);
            }
            catch
            {
                return WpfColor.FromRgb(0, 0, 0);
            }
        }

        private static string ToHtmlColor(WpfColor color)
            => ColorTranslator.ToHtml(System.Drawing.Color.FromArgb(color.R, color.G, color.B));

        // Schema.

        private void BuildSchema()
        {
            var gameVersions = TRVersion.AllVersions.Cast<object>().ToList();
            var fonts = System.Drawing.FontFamily.Families.Select(f => (object)f.Name).ToList();
            var objectQuality = new List<object> { "High", "Medium", "Low" };
            var globalEvents = Event.GlobalEventTypes.Select(e => (object)e.ToString().SplitCamelcase()).ToList();
            var volumeEvents = Event.VolumeEventTypes.Select(e => (object)e.ToString().SplitCamelcase()).ToList();
            var eventMode = new List<object> { "Level script functions", "Node editor" };

            var presetNames = GetColorSchemePresetNames();

            // General
            Tabs.Add(Tab("General",
                Group("Misc",
                    Bool("Enable step height controls for unsupported engines", "Editor_EnableStepHeightControlsForUnsupportedEngines"),
                    Bool("Enable logging", "Log_WriteToFile"),
                    Num("Number of daily log files in history:", "Log_ArchiveN")),
                Group("Defaults",
                    Bool("Grid border walls in new rooms", "Editor_GridNewRoom"),
                    Num("Default new room size (with border walls):", "Editor_DefaultNewRoomSize", 3, 31),
                    Combo("Default game version for new projects:", "Editor_DefaultProjectGameVersion", gameVersions),
                    Bool("Use half-pixel UV correction on PRJ import", "Editor_UseHalfPixelCorrectionOnPrjImport"),
                    Bool("Respect T4Larson's mousepatch flyby handling on PRJ import", "Editor_RespectFlybyPatchOnPrjImport")),
                Group("System",
                    Bool("Allow multiple instances", "Editor_AllowMultipleInstances"),
                    Bool("Open last project on editor start-up", "Editor_OpenLastProjectOnStartup"),
                    Num("Undo / redo depth:", "Editor_UndoDepth", 1, 1000),
                    Bool("Reload resources automatically when changed", "Editor_ReloadFilesAutomaticallyWhenChanged")),
                Group("Autosave",
                    Bool("Enable autosave", "AutoSave_Enable"),
                    Num("Autosave interval, in seconds:", "AutoSave_TimeInSeconds", 0, 10000),
                    Bool("Put date first in autosave name", "AutoSave_NamePutDateFirst"),
                    Text("Date-time format:", "AutoSave_DateTimeFormat"),
                    Bool("Cleanup autosaves when amount is reached:", "AutoSave_CleanupEnable"),
                    Num("Maximum autosaves to keep:", "AutoSave_CleanupMaxAutoSaves"))));

            // User interface
            Tabs.Add(Tab("User interface",
                Group("General",
                    Bool("Automatically pick selected object color to palette", "Palette_PickColorFromSelectedObject"),
                    Bool("Add descriptions for autogenerated rooms", "UI_GenerateRoomDescriptions"),
                    Bool("Warn before deleting objects", "UI_WarnBeforeDeletingObjects"),
                    Bool("Automatically enable skybox if invisible texture is applied", "UI_AutoSwitchRoomToOutsideOnAppliedInvisibleTexture"),
                    Bool("Autofill \"Key\", \"Switch\" and \"Dummy\" trigger types", "UI_AutoFillTriggerTypesForSwitchAndKey"),
                    Bool("Set or unset selected area's sector flags at once", "UI_SetAttributesAtOnce"),
                    Bool("Discard selection on editor mode switch", "UI_DiscardSelectionOnModeSwitch"),
                    Bool("Apply sector flags to bottom room through portals", "UI_ProbeAttributesThroughPortals"),
                    Bool("Autoswitch sector coloring on property hover", "UI_AutoSwitchSectorColoringInfo"),
                    Bool("Non-intrusive message when trying to move locked room", "UI_OnlyShowSmallMessageWhenRoomIsLocked")),
                Group("Color scheme",
                    Preset("Color scheme preset:", presetNames),
                    Num("UI brightness (requires restart):", "UI_FormColor_Brightness", 50, 100, 5, 0, "%"),
                    Color("UI button highlight:", "UI_FormColor_ButtonHighlight"),
                    Color("2D map - moved rooms:", "Color2DRoomsMoved"),
                    Color("2D map - rooms below current:", "Color2DRoomsBelow"),
                    Color("2D map - rooms above current:", "Color2DRoomsAbove"),
                    Color("Slide direction:", "ColorSlideDirection"),
                    Color("Illegal slope:", "ColorIllegalSlope"),
                    Color("Selection:", "ColorSelection"),
                    Color("Force solid floor:", "ColorForceSolidFloor"),
                    Color("Trigger triggerer mark:", "ColorTriggerTriggerer"),
                    Color("Beetle mark:", "ColorBeetle"),
                    Color("Not walkable:", "ColorNotWalkable"),
                    Color("Death:", "ColorDeath"),
                    Color("Box:", "ColorBox"),
                    Color("Climb:", "ColorClimb"),
                    Color("Monkeyswing:", "ColorMonkey"),
                    Color("Trigger:", "ColorTrigger"),
                    Color("Upper wall section:", "ColorWallUpper"),
                    Color("Lower wall section:", "ColorWallLower"),
                    Color("Center wall section:", "ColorWall"),
                    Color("Border wall:", "ColorBorderWall"),
                    Color("Floor:", "ColorFloor"),
                    Color("3D portal face highlight:", "ColorPortalFace"),
                    Color("2D portal highlight:", "ColorPortal"),
                    Color("2D portal effect highlight:", "ColorPortalEffect"),
                    Color("Flipped room background:", "ColorFlipRoom"),
                    Color("2D background:", "Color2DBackground"),
                    Color("3D background:", "Color3DBackground"))));

            // 3D window
            Tabs.Add(Tab("3D window",
                Group("Text",
                    Bool("Draw dark rectangular text overlay", "Rendering3D_DrawFontOverlays"),
                    Bool("Bold", "Rendering3D_FontIsBold"),
                    Num("Font size (requires restart):", "Rendering3D_FontSize", 0, 1000, 1, 2),
                    Combo("Font (requires restart):", "Rendering3D_FontName", fonts)),
                Group("Rendering",
                    Combo("Object rendering quality (requires restart):", "Rendering3D_ObjectQuality", objectQuality),
                    Bool("Use winroomedit cardinal direction hints", "Rendering3D_UseRoomEditorDirections"),
                    Bool("Animate ghost block unfolding", "Rendering3D_AnimateGhostBlockUnfolding"),
                    Bool("Use real Light Quality for preview", "Rendering3D_HighQualityLightPreview"),
                    Bool("Automatically bookmark selected object", "Rendering3D_AutoBookmarkSelectedObject"),
                    Bool("Automatically switch current room on mouse action", "Rendering3D_AutoswitchCurrentRoom"),
                    Bool("Allow selection in any room", "Rendering3D_SelectObjectsInAnyRoom"),
                    Bool("Animate camera on reset", "Rendering3D_AnimateCameraOnReset"),
                    Bool("Show room bounds rectangle", "Rendering3D_AlwaysShowCurrentRoomBounds"),
                    Bool("Allow texturing in Lighting mode", "Rendering3D_AllowTexturingInLightingMode"),
                    Bool("Animate camera on relocation", "Rendering3D_AnimateCameraOnRelocation"),
                    Bool("Animate camera on double-click room switch", "Rendering3D_AnimateCameraOnDoubleClickRoomSwitch"),
                    Bool("Use flat icons for service objects", "Rendering3D_UseSpritesForServiceObjects"),
                    Bool("Show FPS", "Rendering3D_ShowFPS"),
                    Bool("Reset camera on room switch", "Rendering3D_ResetCameraOnRoomSwitch"),
                    Bool("Use antialiasing (requires restart)", "Rendering3D_Antialias"),
                    Num("Sector outline width:", "Rendering3D_LineWidth"),
                    Num("Maximum portal room depth in 'Draw portals' mode:", "Rendering3D_DrawRoomsMaxDepth"),
                    Num("Field of view:", "Rendering3D_FieldOfView", 10, 179))));

            // 3D controls
            Tabs.Add(Tab("3D controls",
                Group("Other",
                    Num("Fly Mode move speed:", "Rendering3D_FlyModeMoveSpeed")),
                Group("Mouse controls",
                    Bool("Warp cursor on edges", "Rendering3D_CursorWarping"),
                    Bool("Invert mouse zoom", "Rendering3D_InvertMouseZoom"),
                    Num("Mouse drag tools sensitivity:", "Rendering3D_DragMouseSensitivity", 0, 1, 0.01, 2),
                    Num("Mouse rotation speed:", "Rendering3D_NavigationSpeedMouseRotate"),
                    Num("Mouse move speed:", "Rendering3D_NavigationSpeedMouseTranslate", 0, 500000),
                    Num("Mouse drag zoom speed:", "Rendering3D_NavigationSpeedMouseZoom", 0, 500000),
                    Num("Mouse wheel zoom speed:", "Rendering3D_NavigationSpeedMouseWheelZoom")),
                Group("Keyboard controls",
                    Num("Keyboard zoom speed:", "Rendering3D_NavigationSpeedKeyZoom", 0, 10000),
                    Num("Keyboard rotation speed:", "Rendering3D_NavigationSpeedKeyRotate", 0, 10, 0.01, 2))));

            // Gizmo
            Tabs.Add(Tab("Gizmo",
                Group("Gizmo",
                    Num("Line thickness:", "Gizmo_LineThickness", 0, 1000),
                    Num("Scaling cube size:", "Gizmo_ScaleCubeSize", 0, 1000),
                    Num("Centering cube size:", "Gizmo_CenterCubeSize", 0, 1000),
                    Num("Translation cone size:", "Gizmo_TranslationConeSize", 0, 1000),
                    Num("Gizmo size:", "Gizmo_Size", 0, 10000))));

            // Item browser
            Tabs.Add(Tab("Item browser",
                Group("Item browser",
                    Bool("Animate item preview", "RenderingItem_Animate"),
                    Bool("Show hint in case current object exists in several wads", "RenderingItem_ShowMultipleWadsPrompt"),
                    Bool("Hide internally used objects from item list", "RenderingItem_HideInternalObjects"),
                    Bool("Use antialiasing (requires restart)", "RenderingItem_Antialias"),
                    Num("Field of view:", "RenderingItem_FieldOfView", 10, 179),
                    Num("Mouse rotation speed:", "RenderingItem_NavigationSpeedMouseRotate"),
                    Num("Mouse move speed:", "RenderingItem_NavigationSpeedMouseTranslate", 0, 2000),
                    Num("Mouse drag zoom speed:", "RenderingItem_NavigationSpeedMouseZoom", 0, 2000),
                    Num("Mouse wheel zoom speed:", "RenderingItem_NavigationSpeedMouseWheelZoom"))));

            // 2D window
            Tabs.Add(Tab("2D window",
                Group("2D window",
                    Num("Keyboard zoom speed:", "Map2D_NavigationSpeedKeyZoom", 0.05, 10, 0.05, 2),
                    Num("Keyboard move speed:", "Map2D_NavigationSpeedKeyMove", 0, 500),
                    Num("Mouse drag zoom speed:", "Map2D_NavigationSpeedMouseZoom", 0, 1_000_000, 0.05, 2),
                    Num("Minimum zoom factor:", "Map2D_NavigationMinZoom", 0.05, 1_000_000, 0.05, 2),
                    Num("Mouse wheel zoom speed:", "Map2D_NavigationSpeedMouseWheelZoom", 0.0001, 1, 0.0001, 4),
                    Num("Maximum zoom factor:", "Map2D_NavigationMaxZoom", 0, 5000))));

            // Texture map
            Tabs.Add(Tab("Texture map",
                Group("Texture map",
                    Bool("Warn if double-sided texture is applied to single-sided face", "TextureMap_WarnAboutIncorrectAttributes"),
                    Bool("Reset attributes on new selection", "TextureMap_ResetAttributesOnNewSelection"),
                    Bool("Pick texture without attributes", "TextureMap_PickTextureWithoutAttributes"),
                    Bool("Scroll texture with mouse wheel instead of zooming it", "TextureMap_MouseWheelMovesTheTextureInsteadOfZooming"),
                    Bool("Draw selection direction indicators", "TextureMap_DrawSelectionDirectionIndicators"),
                    Num("Keyboard zoom speed:", "TextureMap_NavigationSpeedKeyZoom", 0, 1_000_000, 0.05, 2),
                    Num("Keyboard move speed:", "TextureMap_NavigationSpeedKeyMove", 0, 500),
                    Num("Mouse drag zoom speed:", "TextureMap_NavigationSpeedMouseZoom", 0, 1_000_000, 0.05, 2),
                    Num("Minimum zoom factor:", "TextureMap_NavigationMinZoom", 0.05, 1_000_000, 0.05, 2),
                    Num("Mouse wheel zoom speed:", "TextureMap_NavigationSpeedMouseWheelZoom", 0, 1_000_000, 0.0005, 4),
                    Num("Maximum zoom factor:", "TextureMap_NavigationMaxZoom", 0, 5000))));

            // Node editor
            Tabs.Add(Tab("Node editor",
                Group("Node editor",
                    Bool("Show side grips for nodes", "NodeEditor_ShowGrips"),
                    Bool("Draw node editor links as ropes", "NodeEditor_LinksAsRopes"),
                    Combo("Default editing mode:", "NodeEditor_DefaultEventMode", eventMode),
                    Combo("Default event to edit:", "NodeEditor_DefaultEventToEdit", volumeEvents),
                    Combo("Default global event to edit:", "NodeEditor_DefaultGlobalEventToEdit", globalEvents),
                    Num("Node graph size:", "NodeEditor_Size", 1, 1024, 0.05, 0),
                    Num("Node graph grid step:", "NodeEditor_GridStep", 1, 64, 0.0005, 0),
                    Num("Default node width:", "NodeEditor_DefaultNodeWidth", 128, 1024))));

            // Drop options whose exact config property wasn't resolved (placeholders) - only keep ones
            // that actually exist on the configuration so reflection read/write stays correct.
            foreach (var tab in Tabs)
                foreach (var group in tab.Groups)
                    group.Items.RemoveAll(o => o.Kind != OptionKind.ColorSchemePreset && GetOption(_config, o.ConfigName) == null);
        }

        private List<object> GetColorSchemePresetNames()
        {
            var scheme = _config.GetType().GetProperty(ColorSchemeProperty)?.GetValue(_config);
            if (scheme is null)
                return new List<object>();

            return scheme.GetType().GetFields(BindingFlags.Public | BindingFlags.Static)
                .Where(f => f.FieldType == scheme.GetType())
                .Select(f => (object)f.Name)
                .ToList();
        }

        // Schema helpers.

        private static OptionTab Tab(string header, params OptionGroup[] groups)
        {
            var tab = new OptionTab { Header = header };
            tab.Groups.AddRange(groups);
            return tab;
        }

        private static OptionGroup Group(string header, params OptionItem?[] items)
        {
            var group = new OptionGroup { Header = header };
            group.Items.AddRange(items.Where(i => i != null)!);
            return group;
        }

        private static OptionItem? Bool(string label, string cfg, bool skip = false)
            => skip ? null : new OptionItem { Kind = OptionKind.Bool, Label = label, ConfigName = cfg };

        private static OptionItem? Num(string label, string cfg, double min = 0, double max = 1_000_000, double inc = 1, int dec = 0, string? suffix = null, bool skip = false)
            => skip ? null : new OptionItem { Kind = OptionKind.Number, Label = label, ConfigName = cfg, Minimum = min, Maximum = max, Increment = inc, DecimalPlaces = dec, Suffix = suffix };

        private static OptionItem Text(string label, string cfg)
            => new() { Kind = OptionKind.Text, Label = label, ConfigName = cfg };

        private static OptionItem Combo(string label, string cfg, IReadOnlyList<object> items)
            => new() { Kind = OptionKind.Combo, Label = label, ConfigName = cfg, Items = items };

        private static OptionItem Color(string label, string cfg)
            => new() { Kind = OptionKind.Color, Label = label, ConfigName = cfg };

        private static OptionItem Preset(string label, IReadOnlyList<object> items)
            => new() { Kind = OptionKind.ColorSchemePreset, Label = label, Items = items };
    }
}
