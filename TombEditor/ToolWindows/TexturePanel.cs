using DarkUI.Docking;
using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using TombLib.Utils;        // FIXME OLD
using TombEditor.Forms;
using TombLib.LevelData;

namespace TombEditor.ToolWindows
{
    public partial class TexturePanel : DarkToolWindow
    {
        private readonly Editor _editor;

        public TexturePanel()
        {
            InitializeComponent();
            CommandHandler.AssignCommandsToButtons(Editor.Instance, this, toolTip);

            _editor = Editor.Instance;
            _editor.EditorEventRaised += EditorEventRaised;

            butDeleteTexture.Enabled = butBrowseTexture.Enabled = butTextureSounds.Enabled = false;
            panelTextureMap.SelectedTextureChanged += delegate
            { _editor.SelectedTexture = panelTextureMap.SelectedTexture; };

            cmbBlending.SelectedIndex = 0;
            cmbTileSize.SelectedIndex = 1;
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
                    butDoubleSide.BackColorUseGeneric = !newTexture.DoubleSided;

                    switch (newTexture.BlendMode)
                    {
                        case BlendMode.Normal:
                        default:
                            cmbBlending.SelectedIndex = 0;
                            break;
                        case BlendMode.Additive:
                            cmbBlending.SelectedIndex = 1;
                            break;
                        case BlendMode.Subtract:
                            cmbBlending.SelectedIndex = 2;
                            break;
                        case BlendMode.Exclude:
                            cmbBlending.SelectedIndex = 3;
                            break;
                        case BlendMode.Screen:
                            cmbBlending.SelectedIndex = 4;
                            break;
                        case BlendMode.Lighten:
                            cmbBlending.SelectedIndex = 5;
                            break;
                    };

                    panelTextureMap.ShowTexture(newTexture);
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
                    CommandHandler.AssignCommandsToButtons(_editor, this, toolTip, true);
            }
        }

        private void comboCurrentTexture_SelectedValueChanged(object sender, EventArgs e)
        {
            if (panelTextureMap.VisibleTexture != comboCurrentTexture.SelectedItem)
                panelTextureMap.ResetVisibleTexture(comboCurrentTexture.SelectedItem as LevelTexture);
            butDeleteTexture.Enabled = butBrowseTexture.Enabled = butTextureSounds.Enabled = comboCurrentTexture.SelectedItem != null;
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

        private void cmbTileSize_SelectedIndexChanged(object sender, EventArgs e)
        {
            switch(cmbTileSize.SelectedIndex)
            {
                case 0:
                    panelTextureMap.TileSelectionSize = 32.0f;
                    break;
                default:
                case 1:
                    panelTextureMap.TileSelectionSize = 64.0f;
                    break;
                case 2:
                    panelTextureMap.TileSelectionSize = 128.0f;
                    break;
                case 3:
                    panelTextureMap.TileSelectionSize = 256.0f;
                    break;
            }
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
            butDoubleSide.BackColorUseGeneric = !texture.DoubleSided;

            switch (texture.BlendMode)
            {
                case BlendMode.Normal:
                default:
                    cmbBlending.SelectedIndex = 0;
                    break;
                case BlendMode.Additive:
                    cmbBlending.SelectedIndex = 1;
                    break;
                case BlendMode.Subtract:
                    cmbBlending.SelectedIndex = 2;
                    break;
                case BlendMode.Exclude:
                    cmbBlending.SelectedIndex = 3;
                    break;
                case BlendMode.Screen:
                    cmbBlending.SelectedIndex = 4;
                    break;
                case BlendMode.Lighten:
                    cmbBlending.SelectedIndex = 5;
                    break;
            };
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
    }
}
