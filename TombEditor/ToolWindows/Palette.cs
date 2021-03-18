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

            if (obj is Editor.SelectedObjectChangedEvent)
            {
                var o = obj as Editor.SelectedObjectChangedEvent;
                if (o.Current is IColorable)
                    lightPalette.PickColor(o.Current as IColorable);
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
