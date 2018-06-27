using DarkUI.Docking;
using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
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

            _editor = Editor.Instance;
            _editor.EditorEventRaised += EditorEventRaised;

            butDeleteTexture.Enabled = butBrowseTexture.Enabled = butTextureSounds.Enabled = false;
            panelTextureMap.SelectedTextureChanged += delegate
            { _editor.SelectedTexture = panelTextureMap.SelectedTexture; };

            // Setup tile size options
            if (_editor.Configuration.TextureMap_DefaultTileSelectionSize == 64.0f)
                rbTileSize64.Checked = true;
            else if (_editor.Configuration.TextureMap_DefaultTileSelectionSize == 128.0f)
                rbTileSize128.Checked = true;
            else if (_editor.Configuration.TextureMap_DefaultTileSelectionSize == 256.0f)
                rbTileSize256.Checked = true;
            else
                rbTileSize32.Checked = true;
            panelTextureMap.TileSelectionSize = _editor.Configuration.TextureMap_DefaultTileSelectionSize;
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
                LevelTexture toSelect = ((Editor.SelectedTexturesChangedEvent)obj).Current.Texture as LevelTexture;
                if (toSelect != null)
                    comboCurrentTexture.SelectedItem = toSelect;
                panelTextureMap.SelectedTexture = ((Editor.SelectedTexturesChangedEvent)obj).Current;
            }

            // Center texture on texture map
            if (obj is Editor.SelectTextureAndCenterViewEvent)
            {
                comboCurrentTexture.SelectedItem = ((Editor.SelectTextureAndCenterViewEvent)obj).Texture.Texture as LevelTexture;
                panelTextureMap.ShowTexture(((Editor.SelectTextureAndCenterViewEvent)obj).Texture);
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

        private void butAnimationRanges_Click(object sender, EventArgs e)
        {
            using (var form = new FormAnimatedTextures(_editor, comboCurrentTexture.SelectedItem as LevelTexture))
                form.ShowDialog(this);
        }

        private void rbTileSize32_CheckedChanged(object sender, EventArgs e)
        {
            if (rbTileSize32.Checked)
                panelTextureMap.TileSelectionSize = 32.0f;
        }

        private void rbTileSize64_CheckedChanged(object sender, EventArgs e)
        {
            if (rbTileSize64.Checked)
                panelTextureMap.TileSelectionSize = 64.0f;
        }

        private void rbTileSize128_CheckedChanged(object sender, EventArgs e)
        {
            if (rbTileSize128.Checked)
                panelTextureMap.TileSelectionSize = 128.0f;
        }

        private void rbTileSize256_CheckedChanged(object sender, EventArgs e)
        {
            if (rbTileSize256.Checked)
                panelTextureMap.TileSelectionSize = 256.0f;
        }

        private void butDeleteTexture_Click(object sender, EventArgs e)
        {
            LevelTexture texture = comboCurrentTexture.SelectedItem as LevelTexture;
            if (texture != null)
                EditorActions.RemoveTexture(this, texture);
        }

        private void butAddTexture_Click(object sender, EventArgs e)
        {
            EditorActions.AddTexture(this);
        }

        private void butBrowseTexture_Click(object sender, EventArgs e)
        {
            EditorActions.AddTexture(this, comboCurrentTexture.SelectedItem as LevelTexture);
        }
    }
}
