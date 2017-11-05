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
            if (obj is Editor.ModeChangedEvent)
                SwitchMode(((Editor.ModeChangedEvent)obj).Current);

            if (obj is Editor.ConfigurationChangedEvent)
                Location = ((Editor.ConfigurationChangedEvent)obj).Current.Rendering3D_ToolboxPosition;
        }

        private void SwitchMode(EditorMode mode)
        {
            toolFill.Visible = mode == EditorMode.FaceEdit;
            toolGroup.Visible = mode == EditorMode.FaceEdit;
            toolEraser.Visible = mode == EditorMode.FaceEdit;
            toolInvisibility.Visible = mode == EditorMode.FaceEdit;
            toolSeparator1.Visible = mode == EditorMode.FaceEdit;
            toolFlatten.Visible = mode == EditorMode.Geometry;
            toolShovel.Visible = mode == EditorMode.Geometry;
            toolSmooth.Visible = mode == EditorMode.Geometry;
            toolPalette.AutoSize = true;
            Height = toolPalette.Size.Height + Padding.Size.Height;
            Visible = (mode == EditorMode.FaceEdit) || (mode == EditorMode.Geometry);
            FixPosition();

            // Select classic winroomedit controls by default
            SwitchTool(mode == EditorMode.FaceEdit ? EditorToolType.Brush : EditorToolType.Selection);
        }

        private void SwitchTool(EditorToolType tool = EditorToolType.Selection)
        {
            toolSelection.Checked = tool == EditorToolType.Selection;
            toolBrush.Checked = tool == EditorToolType.Brush;
            toolPencil.Checked = tool == EditorToolType.Pencil;
            toolFill.Checked = tool == EditorToolType.Fill;
            toolGroup.Checked = tool == EditorToolType.Group;
            toolShovel.Checked = tool == EditorToolType.Shovel;
            toolFlatten.Checked = tool == EditorToolType.Flatten;
            toolSmooth.Checked = tool == EditorToolType.Smooth;

            EditorActions.SwitchTool(new EditorTool { Tool = tool, Texture = _editor.Tool.Texture } );
        }

        private void SwitchTexture(EditorTextureType texture = EditorTextureType.Normal)
        {
            var textureToUse = _editor.Tool.Texture;
            if (texture == textureToUse)
                textureToUse = EditorTextureType.Normal;
            else
                textureToUse = texture;

            toolEraser.Checked = textureToUse == EditorTextureType.Null;
            toolInvisibility.Checked = textureToUse == EditorTextureType.Invisible;

            EditorActions.SwitchTool(new EditorTool { Tool = _editor.Tool.Tool, Texture = textureToUse });
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

        private void toolInvisibility_Click(object sender, EventArgs e)
        {
            SwitchTexture(EditorTextureType.Invisible);
        }

        private void toolEraser_Click(object sender, EventArgs e)
        {
            SwitchTexture(EditorTextureType.Null);
        }
    }
}
