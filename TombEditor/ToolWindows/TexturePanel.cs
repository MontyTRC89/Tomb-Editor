﻿using DarkUI.Docking;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using TombEditor.Forms;
using TombLib.Forms;
using TombLib.LevelData;
using TombLib.Utils;

namespace TombEditor.ToolWindows
{
    public partial class TexturePanel : DarkToolWindow
    {
        private static readonly List<string> _blendingModeDescriptions = new List<string>()
        {
            "Normal",
            "Add",
            "Subtract",
            "Exclude",
            "Screen",
            "Lighten"
        };

        private readonly Editor _editor;

        public TexturePanel()
        {
            InitializeComponent();
            CommandHandler.AssignCommandsToControls(Editor.Instance, this, toolTip);

            _editor = Editor.Instance;
            _editor.EditorEventRaised += EditorEventRaised;

            butDeleteTexture.Enabled =
            butBrowseTexture.Enabled =
            butAnimationRanges.Enabled =
            butBumpMaps.Enabled =
            butTextureSounds.Enabled = false;

            panelTextureMap.SelectedTextureChanged += delegate
            { _editor.SelectedTexture = panelTextureMap.SelectedTexture; };

            // Populate selection tile size 
            for (int i = 5; i <= 8; i++)
                cmbTileSize.Items.Add((float)(Math.Pow(2, i)));

            cmbBlending.SelectedIndex = 0;
            if (cmbTileSize.Items.Contains(_editor.Configuration.TextureMap_TileSelectionSize))
                cmbTileSize.SelectedItem = _editor.Configuration.TextureMap_TileSelectionSize;
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
            // Disable version-specific controls
            if (obj is Editor.InitEvent ||
                obj is Editor.GameVersionChangedEvent ||
                obj is Editor.LevelChangedEvent)
                UpdateUI();

            // Update texture map
            if (obj is Editor.SelectedTexturesChangedEvent)
            {
                var e = (Editor.SelectedTexturesChangedEvent)obj;

                LevelTexture toSelect = e.Current.Texture as LevelTexture;
                if (toSelect != null)
                    comboCurrentTexture.SelectedItem = toSelect;
                panelTextureMap.SelectedTexture = e.Current;

                UpdateTextureControls(e.Current);
            }

            // Center texture on texture map
            if (obj is Editor.SelectTextureAndCenterViewEvent)
            {
                var newTexture = ((Editor.SelectTextureAndCenterViewEvent)obj).Texture;
                comboCurrentTexture.SelectedItem = newTexture.Texture as LevelTexture;
                panelTextureMap.ShowTexture(((Editor.SelectTextureAndCenterViewEvent)obj).Texture);

                if (newTexture.Texture is LevelTexture)
                {
                    UpdateTextureControls(newTexture);
                    panelTextureMap.ShowTexture(newTexture);
                    MakeActive();
                }
            }

            // Reset texture map
            if (obj is Editor.LevelChangedEvent)
            {
                comboCurrentTexture.Items.Clear();
                comboCurrentTexture.Items.AddRange(_editor.Level.Settings.Textures.ToArray());
                comboCurrentTexture.SelectedItem = _editor.Level.Settings.Textures.FirstOrDefault();
            }
            
            if (obj is Editor.LoadedTexturesChangedEvent)
            {
                // Populate current texture combo box
                LevelTexture current = comboCurrentTexture.SelectedItem as LevelTexture;
                comboCurrentTexture.Items.Clear();
                comboCurrentTexture.Items.AddRange(_editor.Level.Settings.Textures.ToArray());
                if (_editor.Level.Settings.Textures.Contains(current))
                    comboCurrentTexture.SelectedItem = current;
                else
                    panelTextureMap.ResetVisibleTexture(null);
                if (((Editor.LoadedTexturesChangedEvent)obj).NewToSelect != null)
                    comboCurrentTexture.SelectedItem = ((Editor.LoadedTexturesChangedEvent)obj).NewToSelect;
                panelTextureMap.Invalidate();
            }

            // Update tooltip texts
            if (obj is Editor.ConfigurationChangedEvent)
            {
                if (((Editor.ConfigurationChangedEvent)obj).UpdateKeyboardShortcuts)
                    CommandHandler.AssignCommandsToControls(_editor, this, toolTip, true);

                if (cmbTileSize.Items.Contains(_editor.Configuration.TextureMap_TileSelectionSize))
                    cmbTileSize.SelectedItem = _editor.Configuration.TextureMap_TileSelectionSize;
            }
        }

        private void UpdateUI()
        {
            butDeleteTexture.Enabled =
            butBrowseTexture.Enabled =
            butAnimationRanges.Enabled = comboCurrentTexture.SelectedItem != null;

            butTextureSounds.Enabled = comboCurrentTexture.SelectedItem != null && 
                _editor.Level.Settings.GameVersion.Legacy() >= TRVersion.Game.TR3;

            butBumpMaps.Enabled = comboCurrentTexture.SelectedItem != null &&
                (_editor.Level.Settings.GameVersion.Legacy() == TRVersion.Game.TR4 ||
                 _editor.Level.Settings.GameVersion == TRVersion.Game.TR5Main);

            cmbBlending.Enabled = _editor.Level.Settings.GameVersion > TRVersion.Game.TR2;

            RepopulateBlendingModes();
        }

        private void RepopulateBlendingModes()
        {
            cmbBlending.Items.Clear();

            // For TR4, TRNG and TR5Main we can add all types
            if (_editor.Level.Settings.GameVersion.Legacy() == TRVersion.Game.TR4 ||
                _editor.Level.Settings.GameVersion == TRVersion.Game.TR5Main)
            {
                _blendingModeDescriptions.ForEach(item => cmbBlending.Items.Add(item));
            }
            else
            {
                // Type 0 exists everywhere
                cmbBlending.Items.Add(_blendingModeDescriptions[0]);

                // Additive blending is for TR3-5 only
                if (_editor.Level.Settings.GameVersion >= TRVersion.Game.TR3)
                    cmbBlending.Items.Add(_blendingModeDescriptions[1]);
            }

            // Restore current blending mode
            UpdateTextureControls(_editor.SelectedTexture);
        }

        private void comboCurrentTexture_SelectedValueChanged(object sender, EventArgs e)
        {
            if (panelTextureMap.VisibleTexture != comboCurrentTexture.SelectedItem)
            {
                var selectedTexture = comboCurrentTexture.SelectedItem as LevelTexture;
                panelTextureMap.ResetVisibleTexture(comboCurrentTexture.SelectedItem as LevelTexture);
                _editor.SelectedLevelTextureChanged(selectedTexture);
            }
            UpdateUI();
        }

        private void comboCurrentTexture_DropDown(object sender, EventArgs e)
        {
            // Make the combo box as wide as possible
            Point screenPointLeft = comboCurrentTexture.PointToScreen(new Point(0, 0));
            Rectangle screenPointRight = Screen.GetBounds(comboCurrentTexture.PointToScreen(new Point(0, comboCurrentTexture.Width)));
            comboCurrentTexture.DropDownWidth = screenPointRight.Right - screenPointLeft.X - 15; // Margin
        }

        private void butTextureSounds_Click(object sender, EventArgs e)
        {
            LevelTexture texture = comboCurrentTexture.SelectedItem as LevelTexture;
            if (texture != null)
                using (var form = new FormFootStepSounds(_editor, texture))
                    form.ShowDialog(this);
        }

        private void butBumpMaps_Click(object sender, EventArgs e)
        {
            LevelTexture texture = comboCurrentTexture.SelectedItem as LevelTexture;
            if (texture != null)
                using (var form = new FormBumpMaps(_editor, texture))
                    form.ShowDialog(this);
        }

        private void cmbBlending_SelectedIndexChanged(object sender, EventArgs e)
        {
            var selectedTexture = _editor.SelectedTexture;
            switch (cmbBlending.SelectedIndex)
            {
                default:
                case 0:
                    selectedTexture.BlendMode = BlendMode.Normal;
                    break;
                case 1:
                    selectedTexture.BlendMode = BlendMode.Additive;
                    break;
                case 2:
                    selectedTexture.BlendMode = BlendMode.Subtract;
                    break;
                case 3:
                    selectedTexture.BlendMode = BlendMode.Exclude;
                    break;
                case 4:
                    selectedTexture.BlendMode = BlendMode.Screen;
                    break;
                case 5:
                    selectedTexture.BlendMode = BlendMode.Lighten;
                    break;
            }
            _editor.SelectedTexture = selectedTexture;
        }

        private void UpdateTextureControls(TextureArea texture)
        {
            butDoubleSide.Checked = texture.DoubleSided;

            int newIndex = 0;
            switch (texture.BlendMode)
            {
                case BlendMode.Normal:
                default:
                    newIndex = 0;
                    break;
                case BlendMode.Additive:
                    newIndex = 1;
                    break;
                case BlendMode.Subtract:
                    newIndex = 2;
                    break;
                case BlendMode.Exclude:
                    newIndex = 3;
                    break;
                case BlendMode.Screen:
                    newIndex = 4;
                    break;
                case BlendMode.Lighten:
                    newIndex = 5;
                    break;
            };

            if (newIndex < cmbBlending.Items.Count)
                cmbBlending.SelectedIndex = newIndex;
            else
                cmbBlending.SelectedIndex = -1;
        }

        private void butDeleteTexture_Click(object sender, EventArgs e)
        {
            LevelTexture texture = comboCurrentTexture.SelectedItem as LevelTexture;
            if (texture != null)
                EditorActions.RemoveTexture(this, texture);
        }

        private void butBrowseTexture_Click(object sender, EventArgs e)
        {
            LevelTexture texture = comboCurrentTexture.SelectedItem as LevelTexture;
            if (texture != null)
                EditorActions.UpdateTextureFilepath(this, texture);
        }

        private void cmbTileSize_SelectionChangeCommitted(object sender, EventArgs e) => EditorActions.SetSelectionTileSize((float)cmbTileSize.SelectedItem);

        private void butSearch_Click(object sender, EventArgs e)
        {
            var searchPopUp = new PopUpSearch(comboCurrentTexture);
            searchPopUp.Show(this);
        }
    }
}
