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
using TombLib.Utils;

namespace TombEditor.ToolWindows
{
    public partial class ToolPalette : DarkToolWindow
    {
        private class InitEvent : IEditorEvent { }

        private Editor _editor;

        public ToolPalette()
        {
            InitializeComponent();

            _editor = Editor.Instance;
            _editor.EditorEventRaised += EditorEventRaised;
            EditorEventRaised(new InitEvent());
        }

        private void EditorEventRaised(IEditorEvent obj)
        {
            if (obj is Editor.ToolChangedEvent || obj is InitEvent)
            {
                EditorTool currentTool = _editor.Tool;

                toolSelection.Checked = currentTool.Tool == EditorToolType.Selection;
                toolBrush.Checked = currentTool.Tool == EditorToolType.Brush;
                toolPencil.Checked = currentTool.Tool == EditorToolType.Pencil;
                toolFill.Checked = currentTool.Tool == EditorToolType.Fill;
                toolGroup.Checked = currentTool.Tool == EditorToolType.Group;
                toolShovel.Checked = currentTool.Tool == EditorToolType.Shovel;
                toolFlatten.Checked = currentTool.Tool == EditorToolType.Flatten;
                toolSmooth.Checked = currentTool.Tool == EditorToolType.Smooth;
                toolDrag.Checked = currentTool.Tool == EditorToolType.Drag;

                toolUVFixer.Checked = currentTool.TextureUVFixer;
            }

            if (obj is Editor.SelectedTexturesChangedEvent || obj is InitEvent)
            {
                toolEraser.Checked = _editor.SelectedTexture.Texture == null;
                toolInvisibility.Checked = _editor.SelectedTexture.Texture is TextureInvisible;
            }

            if (obj is Editor.ModeChangedEvent || obj is InitEvent)
            {
                EditorMode mode = _editor.Mode;

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
                toolStrip.AutoSize = true;
                Size = toolStrip.Size;
                toolStrip.Visible = (mode == EditorMode.FaceEdit) || (mode == EditorMode.Geometry);

                // Select classic winroomedit controls by default
                SwitchTool(mode == EditorMode.FaceEdit ? EditorToolType.Brush : EditorToolType.Selection);
            }
        }

        private void SwitchTool(EditorToolType tool)
        {
            EditorTool currentTool = _editor.Tool;
            currentTool.Tool = tool;
            EditorActions.SwitchTool(currentTool);
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

        private void toolGroup_Click(object sender, EventArgs e)
        {
            SwitchTool(EditorToolType.Group);
        }

        private void toolDrag_Click(object sender, EventArgs e)
        {
            SwitchTool(EditorToolType.Drag);
        }

        private void toolInvisibility_Click(object sender, EventArgs e)
        {
            _editor.SelectedTexture = new TextureArea { Texture = TextureInvisible.Instance };
        }

        private void toolEraser_Click(object sender, EventArgs e)
        {
            _editor.SelectedTexture = new TextureArea { Texture = null };
        }

        private void toolUVFixer_Click(object sender, EventArgs e)
        {
            EditorTool currentTool = _editor.Tool;
            currentTool.TextureUVFixer = !currentTool.TextureUVFixer;
            EditorActions.SwitchTool(currentTool);
        }
    }
}
