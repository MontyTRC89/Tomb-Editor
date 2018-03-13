using DarkUI.Docking;
using System;

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

            panelTextureMap.SelectedTextureChanged += delegate { _editor.SelectedTexture = panelTextureMap.SelectedTexture; };

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
            if (disposing && (components != null))
                components.Dispose();
            base.Dispose(disposing);
        }

        private void EditorEventRaised(IEditorEvent obj)
        {
            // Update texture map
            if (obj is Editor.SelectedTexturesChangedEvent)
                panelTextureMap.SelectedTexture = ((Editor.SelectedTexturesChangedEvent)obj).Current;

            // Center texture on texture map
            if (obj is Editor.SelectTextureAndCenterViewEvent)
                panelTextureMap.ShowTexture(((Editor.SelectTextureAndCenterViewEvent)obj).Texture);
        }

        private void butTextureSounds_Click(object sender, EventArgs e)
        {
            EditorActions.ShowTextureSoundsDialog(this);
        }

        private void butAnimationRanges_Click(object sender, EventArgs e)
        {
            EditorActions.ShowAnimationRangesDialog(this);
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
    }
}
