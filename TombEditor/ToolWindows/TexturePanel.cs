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
            panelTextureMap.Configuration = _editor.Configuration;
            panelTextureMap.SelectedTextureChanged += delegate { _editor.SelectedTexture = panelTextureMap.SelectedTexture; };
        }

        private void EditorEventRaised(IEditorEvent obj)
        {
            // Update texture map
            if (obj is Editor.SelectedTexturesChangedEvent)
                panelTextureMap.SelectedTexture = ((Editor.SelectedTexturesChangedEvent)obj).Current;

            // Reset texture map
            if ((obj is Editor.LevelChangedEvent) || (obj is Editor.LoadedTexturesChangedEvent))
                panelTextureMap.ResetVisibleTexture(_editor.Level.Settings.Textures.Count > 0 ? _editor.Level.Settings.Textures[0] : null);

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

        private void butBump_Click(object sender, EventArgs e)
        {

        }
    }
}
