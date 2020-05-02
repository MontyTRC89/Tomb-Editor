using System;
using System.Collections.Generic;
using DarkUI.Docking;
using TombLib.LevelData;
using TombLib.Utils;

namespace TombEditor.ToolWindows
{
    public partial class Palette : DarkToolWindow
    {
        private readonly Editor _editor;
        private List<ColorC> _lastTexturePalette;

        public Palette()
        {
            InitializeComponent();

            CommandHandler.AssignCommandsToControls(Editor.Instance, this, toolTip);
            _editor = Editor.Instance;
            _editor.EditorEventRaised += EditorEventRaised;

            // Reset palette to default and prepare controls
            lightPalette.LoadPalette(LevelSettings.LoadPalette());
            UpdateControls(false);

            // Update palette
            lightPalette.SelectedColorChanged += delegate
            {
                if (_editor.SelectedObject is LightInstance)
                {
                    var light = _editor.SelectedObject as LightInstance;
                    light.Color = lightPalette.SelectedColor.ToFloat3Color() * 2.0f;
                    _editor.SelectedRoom.RebuildLighting(_editor.Configuration.Rendering3D_HighQualityLightPreview);
                    _editor.ObjectChange(light, ObjectChangeType.Change);
                }
                else if (_editor.SelectedObject is StaticInstance)
                {
                    var instance = _editor.SelectedObject as StaticInstance;
                    instance.Color = lightPalette.SelectedColor.ToFloat3Color() * 2.0f;
                    _editor.ObjectChange(instance, ObjectChangeType.Change);
                }
                else if (_editor.Level.Settings.GameVersion == TRVersion.Game.TR5Main && _editor.SelectedObject is MoveableInstance)
                {
                    var instance = _editor.SelectedObject as MoveableInstance;
                    instance.Color = lightPalette.SelectedColor.ToFloat3Color() * 2.0f;
                    _editor.ObjectChange(instance, ObjectChangeType.Change);
                }
                _editor.LastUsedPaletteColourChange(lightPalette.SelectedColor);
            };

            // Copy palette into level settings on update
            lightPalette.PaletteChanged += delegate { _editor.Level.Settings.Palette = lightPalette.Palette; };

            // Hook into palette mouse events to temporarily hide selection highlight
            lightPalette.MouseDown += delegate 
            {
                // Additionally save undo in case we're editing selected light colour
                if (_editor.SelectedObject is LightInstance)
                    _editor.UndoManager.PushObjectPropertyChanged((LightInstance)_editor.SelectedObject);
                _editor.ToggleHiddenSelection(true);
            };

            lightPalette.MouseUp += delegate 
            {
                _editor.ToggleHiddenSelection(false);
            };
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
                _editor.EditorEventRaised -= EditorEventRaised;
            if (disposing && components != null)
                components.Dispose();
            base.Dispose(disposing);
        }

        private void EditorEventRaised(IEditorEvent obj)
        {
            if (obj is Editor.LevelChangedEvent)
            {
                _lastTexturePalette = null;
                lightPalette.LoadPalette(((Editor.LevelChangedEvent)obj).Current.Settings.Palette);
            }

            if (obj is Editor.ResetPaletteEvent)
            {
                if (!butSampleFromTextures.Checked)
                {
                    lightPalette.LoadPalette(LevelSettings.LoadPalette());
                    _editor.Level.Settings.Palette = lightPalette.Palette;
                }
            }

            if (obj is Editor.SelectedLevelTextureChangedSetEvent)
            {
                var o = (Editor.SelectedLevelTextureChangedSetEvent)obj;
                _lastTexturePalette = new List<ColorC>(o.Texture.Image.Palette);
                UpdateControls();
            }

            if (obj is Editor.ConfigurationChangedEvent)
            {
                var o = (Editor.ConfigurationChangedEvent)obj;
                if (((Editor.ConfigurationChangedEvent)obj).UpdateKeyboardShortcuts)
                    CommandHandler.AssignCommandsToControls(_editor, this, toolTip, true);
                UpdateControls();
            }
        }

        private void UpdateControls(bool reloadPalette = true)
        {
            bool useTexture = _editor.Configuration.Palette_TextureSamplingMode;

            butSampleFromTextures.Checked =  useTexture;
            butResetToDefaults.Enabled    = !useTexture;
            lightPalette.Editable         = !useTexture;

            if (reloadPalette)
            {
                if (useTexture && _lastTexturePalette != null && _lastTexturePalette.Count > 0)
                    lightPalette.LoadPalette(_lastTexturePalette);
                else
                    lightPalette.LoadPalette(_editor.Level.Settings.Palette);

                // Also update last colour on editor so next created light will receive updated colour.
                _editor.LastUsedPaletteColourChange(lightPalette.SelectedColor);
            }
        }

        protected override void OnSizeChanged(EventArgs e)
        {
            base.OnSizeChanged(e);

            if (ClientSize.Width >= ClientSize.Height * 2)
                paletteToolBar.Dock = System.Windows.Forms.DockStyle.Left;
            else
                paletteToolBar.Dock = System.Windows.Forms.DockStyle.Top;
        }
    }
}
