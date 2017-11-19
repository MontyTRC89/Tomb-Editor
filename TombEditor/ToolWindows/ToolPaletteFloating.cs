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
using TombLib.Utils;

namespace TombEditor.ToolWindows
{
    public partial class ToolPaletteFloating : DarkFloatingToolbox
    {
        private class InitEvent : IEditorEvent { }

        private Editor _editor;

        public ToolPaletteFloating()
        {
            InitializeComponent();

            _editor = Editor.Instance;
            _editor.EditorEventRaised += EditorEventRaised;
            EditorEventRaised(new InitEvent());
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
                toolRamp.Checked = currentTool.Tool == EditorToolType.Ramp;
                toolQuarterPipe.Checked = currentTool.Tool == EditorToolType.QuarterPipe;
                toolHalfPipe.Checked = currentTool.Tool == EditorToolType.HalfPipe;
                toolBowl.Checked = currentTool.Tool == EditorToolType.Bowl;
                toolPyramid.Checked = currentTool.Tool == EditorToolType.Pyramid;

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
                //toolSeparator1.Visible = mode == EditorMode.FaceEdit;
                toolSeparator2.Visible = mode == EditorMode.FaceEdit;
                toolFlatten.Visible = mode == EditorMode.Geometry;
                toolShovel.Visible = mode == EditorMode.Geometry;
                toolSmooth.Visible = mode == EditorMode.Geometry;
                toolDrag.Visible = mode == EditorMode.Geometry;
                toolRamp.Visible = mode == EditorMode.Geometry;
                toolQuarterPipe.Visible = mode == EditorMode.Geometry;
                toolHalfPipe.Visible = mode == EditorMode.Geometry;
                toolBowl.Visible = mode == EditorMode.Geometry;
                toolPyramid.Visible = mode == EditorMode.Geometry;

                toolPalette.AutoSize = true;
                Height = toolPalette.Size.Height + Padding.Size.Height;
                Visible = (mode == EditorMode.FaceEdit) || (mode == EditorMode.Geometry);
                FixPosition();

                // Select classic winroomedit controls by default
                SwitchTool(mode == EditorMode.FaceEdit ? _editor.Configuration.Tool_DefaultFaceEdit : _editor.Configuration.Tool_DefaultGeometry);
            }

            if (obj is Editor.ConfigurationChangedEvent)
                Location = ((Editor.ConfigurationChangedEvent)obj).Current.Rendering3D_ToolboxPosition;
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

        private void toolRamp_Click(object sender, EventArgs e)
        {
            SwitchTool(EditorToolType.Ramp);
        }

        private void toolQuarterPipe_Click(object sender, EventArgs e)
        {
            SwitchTool(EditorToolType.QuarterPipe);
        }

        private void toolHalfPipe_Click(object sender, EventArgs e)
        {
            SwitchTool(EditorToolType.HalfPipe);
        }

        private void toolBowl_Click(object sender, EventArgs e)
        {
            SwitchTool(EditorToolType.Bowl);
        }

        private void toolPyramid_Click(object sender, EventArgs e)
        {
            SwitchTool(EditorToolType.Pyramid);
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
