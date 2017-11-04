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
    public partial class ToolPalette : DarkToolWindow
    {
        private Editor _editor;

        public ToolPalette()
        {
            InitializeComponent();

            _editor = Editor.Instance;
            _editor.EditorEventRaised += EditorEventRaised;

            SwitchMode(_editor.Mode);
        }

        private void EditorEventRaised(IEditorEvent obj)
        {
            if (obj is Editor.ModeChangedEvent)
                SwitchMode(((Editor.ModeChangedEvent)obj).Current);
        }

        private void SwitchMode(EditorMode mode)
        {
            toolFill.Visible = mode == EditorMode.FaceEdit;
            toolEraser.Visible = mode == EditorMode.FaceEdit;
            toolInvisibility.Visible = mode == EditorMode.FaceEdit;
            toolSeparator1.Visible = mode == EditorMode.FaceEdit;
            toolFlatten.Visible = mode == EditorMode.Geometry;
            toolShovel.Visible = mode == EditorMode.Geometry;
            toolSmooth.Visible = mode == EditorMode.Geometry;
            toolStrip.AutoSize = true;
            Size = toolStrip.Size;
            toolStrip.Visible = (mode == EditorMode.FaceEdit) || (mode == EditorMode.Geometry);

            // Select classic winroomedit controls by default
            SwitchTool(mode == EditorMode.FaceEdit ? EditorToolType.Brush : EditorToolType.Selection);
        }

        private void SwitchTool(EditorToolType tool = EditorToolType.Selection)
        {
            toolSelection.Checked = tool == EditorToolType.Selection;
            toolBrush.Checked = tool == EditorToolType.Brush;
            toolPencil.Checked = tool == EditorToolType.Pencil;
            toolFill.Checked = tool == EditorToolType.Fill;
            toolShovel.Checked = tool == EditorToolType.Shovel;
            toolFlatten.Checked = tool == EditorToolType.Flatten;
            toolSmooth.Checked = tool == EditorToolType.Smooth;

            EditorActions.SwitchTool(new EditorTool { Tool = tool, Texture = _editor.Tool.Texture });
        }

        private void SwitchTexture(EditorTextureType texture = EditorTextureType.Normal)
        {
            toolEraser.Checked = texture == EditorTextureType.Null;
            toolInvisibility.Checked = texture == EditorTextureType.Invisible;

            EditorActions.SwitchTool(new EditorTool { Tool = _editor.Tool.Tool, Texture = texture });
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
