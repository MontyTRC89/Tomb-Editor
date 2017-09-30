using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using DarkUI.Docking;

namespace TombEditor.ToolWindows
{
    public partial class TexturePanel : DarkToolWindow
    {
        private Editor _editor;

        public TexturePanel()
        {
            InitializeComponent();

            _editor = Editor.Instance;
            _editor.EditorEventRaised += EditorEventRaised;
            
            panelTextureMap.SelectedTextureChanged += delegate { _editor.SelectedTexture = panelTextureMap.SelectedTexture; };
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
                _editor.EditorEventRaised -= EditorEventRaised;
            if (disposing && (components != null))
                components.Dispose();
            base.Dispose(disposing);
        }

        public void Initialize()
        {
            switch(_editor.Configuration.TextureMap_TileSelectionSize)
            {
                case 64.0f:
                    rbTileSize64.Checked = true;
                    break;
                case 128.0f:
                    rbTileSize128.Checked = true;
                    break;
                case 256.0f:
                    rbTileSize256.Checked = true;
                    break;
                default:
                    rbTileSize64.Checked = true;
                    _editor.Configuration.TextureMap_TileSelectionSize = 64.0f;
                    break;
            }
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

        private void lblLoadHelper_Click(object sender, EventArgs e)
        {
            EditorActions.LoadTextures(this.Parent);
        }

        private void rbTileSize64_CheckedChanged(object sender, EventArgs e)
        {
            if (rbTileSize64.Checked == true)
                _editor.Configuration.TextureMap_TileSelectionSize = 64.0f;
        }

        private void rbTileSize128_CheckedChanged(object sender, EventArgs e)
        {
            if (rbTileSize128.Checked == true)
                _editor.Configuration.TextureMap_TileSelectionSize = 128.0f;
        }

        private void rbTileSize256_CheckedChanged(object sender, EventArgs e)
        {
            if (rbTileSize256.Checked == true)
                _editor.Configuration.TextureMap_TileSelectionSize = 256.0f;
        }
    }
}
