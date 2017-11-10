using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using DarkUI.Controls;

namespace TombEditor.ToolWindows
{
    public partial class ToolPaletteFloating : DarkFloatingToolbox
    {
        private Editor _editor;

        public ToolPaletteFloating()
        {
            InitializeComponent();

            _editor = Editor.Instance;
            _editor.EditorEventRaised += EditorEventRaised;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _editor.EditorEventRaised -= EditorEventRaised;
                if (components != null)
                    components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void EditorEventRaised(IEditorEvent obj)
        {
            if(obj is Editor.ToolChangedEvent)
            {
                var currentTool = ((Editor.ToolChangedEvent)obj).Current;

                toolSelection.Checked = currentTool.Tool == EditorToolType.Selection;
                toolBrush.Checked = currentTool.Tool == EditorToolType.Brush;
                toolPencil.Checked = currentTool.Tool == EditorToolType.Pencil;
                toolFill.Checked = currentTool.Tool == EditorToolType.Fill;
                toolGroup.Checked = currentTool.Tool == EditorToolType.Group;
                toolShovel.Checked = currentTool.Tool == EditorToolType.Shovel;
                toolFlatten.Checked = currentTool.Tool == EditorToolType.Flatten;
                toolSmooth.Checked = currentTool.Tool == EditorToolType.Smooth;
                toolDrag.Checked = currentTool.Tool == EditorToolType.Drag;

                toolEraser.Checked = currentTool.Texture == EditorTextureType.Null;
                toolInvisibility.Checked = currentTool.Texture == EditorTextureType.Invisible;

                toolUVFixer.Checked = currentTool.TextureUVFixer;
            }

            if (obj is Editor.ModeChangedEvent)
            {
                var mode = ((Editor.ModeChangedEvent)obj).Current;

                toolFill.Visible = mode == EditorMode.FaceEdit;
                toolGroup.Visible = mode == EditorMode.FaceEdit;
                toolEraser.Visible = mode == EditorMode.FaceEdit;
                toolInvisibility.Visible = mode == EditorMode.FaceEdit;
                toolUVFixer.Visible = mode == EditorMode.FaceEdit;
                toolSeparator1.Visible = mode == EditorMode.FaceEdit;
                toolSeparator2.Visible = mode == EditorMode.FaceEdit;
                toolFlatten.Visible = mode == EditorMode.Geometry;
                toolShovel.Visible = mode == EditorMode.Geometry;
                toolSmooth.Visible = mode == EditorMode.Geometry;
                toolDrag.Visible = mode == EditorMode.Geometry;
                toolPalette.AutoSize = true;
                Height = toolPalette.Size.Height + Padding.Size.Height;
                Visible = (mode == EditorMode.FaceEdit) || (mode == EditorMode.Geometry);
                FixPosition();

                // Select classic winroomedit controls by default
                SwitchTool(mode == EditorMode.FaceEdit ? EditorToolType.Brush : EditorToolType.Selection);
            }

            if (obj is Editor.ConfigurationChangedEvent)
                Location = ((Editor.ConfigurationChangedEvent)obj).Current.Rendering3D_ToolboxPosition;
        }

        private void SwitchTool(EditorToolType tool = EditorToolType.Selection)
        {
            EditorActions.SwitchTool(new EditorTool { Tool = tool, Texture = _editor.Tool.Texture, TextureUVFixer = _editor.Tool.TextureUVFixer } );
        }

        private void SwitchTexture(EditorTextureType texture = EditorTextureType.Normal)
        {
            var textureToUse = _editor.Tool.Texture;
            if (texture == textureToUse)
                textureToUse = EditorTextureType.Normal;
            else
                textureToUse = texture;

            EditorActions.SwitchTool(new EditorTool { Tool = _editor.Tool.Tool, Texture = textureToUse, TextureUVFixer = _editor.Tool.TextureUVFixer });
        }

        private void toolSelection_Click(object sender, EventArgs e)
        {
            SwitchTool(EditorToolType.Selection);
        }

        private void toolBrush_Click(object sender, EventArgs e)
        {
            SwitchTool(EditorToolType.Brush);
        }

        private void toolPencil_Click(object sender, EventArgs e)
        {
            SwitchTool(EditorToolType.Pencil);
        }

        private void toolShovel_Click(object sender, EventArgs e)
        {
            SwitchTool(EditorToolType.Shovel);
        }

        private void toolFlatten_Click(object sender, EventArgs e)
        {
            SwitchTool(EditorToolType.Flatten);
        }

        private void toolFill_Click(object sender, EventArgs e)
        {
            SwitchTool(EditorToolType.Fill);
        }

        private void toolGroup_Click(object sender, EventArgs e)
        {
            SwitchTool(EditorToolType.Group);
        }

        private void toolSmooth_Click(object sender, EventArgs e)
        {
            SwitchTool(EditorToolType.Smooth);
        }

        private void toolDrag_Click(object sender, EventArgs e)
        {
            SwitchTool(EditorToolType.Drag);
        }

        private void toolInvisibility_Click(object sender, EventArgs e)
        {
            SwitchTexture(EditorTextureType.Invisible);
        }

        private void toolEraser_Click(object sender, EventArgs e)
        {
            SwitchTexture(EditorTextureType.Null);
        }

        private void toolUVFixer_Click(object sender, EventArgs e)
        {
            EditorActions.SwitchTool(new EditorTool { Tool = _editor.Tool.Tool, Texture = _editor.Tool.Texture, TextureUVFixer = !_editor.Tool.TextureUVFixer });
        }
    }
}
