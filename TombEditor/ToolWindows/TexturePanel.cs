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
using TombEditor.Geometry;
using TombLib.Utils;

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

            cmbBlending.SelectedIndex = 0;
            cmbBump.SelectedIndex = 0;
            cmbTileSize.SelectedIndex = 1;
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
            {
                panelTextureMap.SelectedTexture = ((Editor.SelectedTexturesChangedEvent)obj).Current;
                butNoTexture.BackColorUseGeneric = !(panelTextureMap.SelectedTexture.Texture == TextureInvisible.Instance);
                SwitchBlendMode();
                SwitchBumpMode();
                SwitchDoubleSide();
            }

            // Center texture on texture map
            if (obj is Editor.SelectTextureAndCenterViewEvent)
            {
                var newTexture = ((Editor.SelectTextureAndCenterViewEvent)obj).Texture;

                if (newTexture.Texture is LevelTexture)
                {
                    butDoubleSide.BackColorUseGeneric = !newTexture.DoubleSided;
                    butNoTexture.BackColorUseGeneric = !(panelTextureMap.SelectedTexture.Texture == TextureInvisible.Instance);

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

                    switch (newTexture.BumpMode)
                    {
                        case BumpMapMode.None:
                        default:
                            cmbBump.SelectedIndex = 0;
                            break;
                        case BumpMapMode.Level1:
                            cmbBump.SelectedIndex = 1;
                            break;
                        case BumpMapMode.Level2:
                            cmbBump.SelectedIndex = 2;
                            break;
                    }

                    panelTextureMap.ShowTexture(newTexture);
                }
            }
        }

        private void butTextureSounds_Click(object sender, EventArgs e)
        {
            EditorActions.ShowTextureSoundsDialog(this);
        }

        private void butAnimationRanges_Click(object sender, EventArgs e)
        {
            EditorActions.ShowAnimationRangesDialog(this);
        }

        private void butDoubleSide_Click(object sender, EventArgs e)
        {
            if(panelTextureMap.VisibleTexture?.IsAvailable ?? false)
            {
                butDoubleSide.BackColorUseGeneric = !butDoubleSide.BackColorUseGeneric;
                SwitchDoubleSide();
            }
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

        private void cmbBump_SelectedIndexChanged(object sender, EventArgs e)
        {
            SwitchBumpMode();
        }

        private void cmbBlending_SelectedIndexChanged(object sender, EventArgs e)
        {
            SwitchBlendMode();
        }

        private void butRotate_Click(object sender, EventArgs e)
        {
            EditorActions.RotateSelectedTexture();
        }

        private void butMirror_Click(object sender, EventArgs e)
        {
            EditorActions.MirrorSelectedTexture();
        }

        private void butNoTexture_Click(object sender, EventArgs e)
        {
            _editor.SelectedTexture = new TextureArea { Texture = TextureInvisible.Instance };
        }

        private void SwitchBumpMode()
        {
            var selectedTexture = _editor.SelectedTexture;
            switch (cmbBump.SelectedIndex)
            {
                default:
                case 0:
                    selectedTexture.BumpMode = BumpMapMode.None;
                    break;
                case 1:
                    selectedTexture.BumpMode = BumpMapMode.Level1;
                    break;
                case 2:
                    selectedTexture.BumpMode = BumpMapMode.Level2;
                    break;
            }
            _editor.SelectedTexture = selectedTexture;
        }

        private void SwitchBlendMode()
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

        private void SwitchDoubleSide()
        {
            var selectedTexture = _editor.SelectedTexture;
            selectedTexture.DoubleSided = !butDoubleSide.BackColorUseGeneric;
            _editor.SelectedTexture = selectedTexture;
        }
    }
}
