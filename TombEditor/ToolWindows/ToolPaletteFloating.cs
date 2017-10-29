using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using TombEditor.Controls;

namespace TombEditor.ToolWindows
{
    public partial class ToolPaletteFloating : FloatingToolbox
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
            toolEraser.Visible = mode == EditorMode.FaceEdit;
            toolInvisibility.Visible = mode == EditorMode.FaceEdit;
            toolFlatten.Visible = mode == EditorMode.Geometry;
            toolShovel.Visible = mode == EditorMode.Geometry;
            toolPalette.AutoSize = true;
            Size = toolPalette.Size + Padding.Size;
            Visible = (mode == EditorMode.FaceEdit) || (mode == EditorMode.Geometry);
            FixPosition();

            // Select classic winroomedit controls by default
            SwitchTool(mode == EditorMode.FaceEdit ? EditorTool.Brush : EditorTool.Selection);
        }

        private void SwitchTool(EditorTool tool = EditorTool.Selection)
        {
            toolSelection.Checked = tool == EditorTool.Selection;
            toolBrush.Checked = tool == EditorTool.Brush;
            toolPencil.Checked = tool == EditorTool.Pencil;
            toolFill.Checked = tool == EditorTool.Fill;
            toolShovel.Checked = tool == EditorTool.Shovel;
            toolFlatten.Checked = tool == EditorTool.Flatten;
            toolSmooth.Checked = tool == EditorTool.Smooth;
            toolEraser.Checked = tool == EditorTool.Eraser;
            toolInvisibility.Checked = tool == EditorTool.Invisibility;

            EditorActions.SwitchTool(tool);
        }

        private void toolSelection_Click(object sender, EventArgs e)
        {
            SwitchTool(EditorTool.Selection);
        }

        private void toolBrush_Click(object sender, EventArgs e)
        {
            SwitchTool(EditorTool.Brush);
        }

        private void toolPencil_Click(object sender, EventArgs e)
        {
            SwitchTool(EditorTool.Pencil);
        }

        private void toolShovel_Click(object sender, EventArgs e)
        {
            SwitchTool(EditorTool.Shovel);
        }

        private void toolFlatten_Click(object sender, EventArgs e)
        {
            SwitchTool(EditorTool.Flatten);
        }

        private void toolFill_Click(object sender, EventArgs e)
        {
            SwitchTool(EditorTool.Fill);
        }

        private void toolInvisibility_Click(object sender, EventArgs e)
        {
            SwitchTool(EditorTool.Invisibility);
        }

        private void toolEraser_Click(object sender, EventArgs e)
        {
            SwitchTool(EditorTool.Eraser);
        }

        private void toolSmooth_Click(object sender, EventArgs e)
        {
            SwitchTool(EditorTool.Smooth);
        }
    }
}
