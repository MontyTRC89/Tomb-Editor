﻿using System;
using System.ComponentModel;
using System.Windows.Forms;
using TombEditor.Controls.ContextMenus;
using TombLib.Utils;

namespace TombEditor.Controls
{
    public partial class ToolBox : UserControl
    {
        public ToolStripLayoutStyle LayoutStyle
        {
            get { return toolStrip.LayoutStyle; }
            set
            {
                if (value == toolStrip.LayoutStyle)
                    return;
                else
                {
                    toolStrip.LayoutStyle = value;
                    if (value == ToolStripLayoutStyle.Flow)
                        toolStrip.Dock = DockStyle.Fill;
                    else
                    {
                        toolStrip.Dock = DockStyle.None;
                        toolStrip.AutoSize = true;
                    }
                }
            }
        }

        private readonly Editor _editor;
        private Timer _contextMenuTimer;

        public ToolBox()
        {
            InitializeComponent();

            _contextMenuTimer = new Timer();
            _contextMenuTimer.Interval = 300;
            _contextMenuTimer.Tick += ContextMenuTimer_Tick;


            if (LicenseManager.UsageMode == LicenseUsageMode.Runtime)
            {
                _editor = Editor.Instance;
                _editor.EditorEventRaised += EditorEventRaised;
            }
        }

        private void EditorEventRaised(IEditorEvent obj)
        {
            if (obj is Editor.ToolChangedEvent || obj is Editor.InitEvent)
            {
                EditorTool currentTool = _editor.Tool;

                toolSelection.Checked = currentTool.Tool == EditorToolType.Selection;
                toolBrush.Checked = currentTool.Tool == EditorToolType.Brush;
                toolPencil.Checked = currentTool.Tool == EditorToolType.Pencil;
                toolFill.Checked = currentTool.Tool == EditorToolType.Fill;
                toolGroup.Checked = currentTool.Tool == EditorToolType.Group;
                toolGridPaint.Checked = currentTool.Tool == EditorToolType.GridPaint;
                toolShovel.Checked = currentTool.Tool == EditorToolType.Shovel;
                toolFlatten.Checked = currentTool.Tool == EditorToolType.Flatten;
                toolSmooth.Checked = currentTool.Tool == EditorToolType.Smooth;
                toolDrag.Checked = currentTool.Tool == EditorToolType.Drag;
                toolRamp.Checked = currentTool.Tool == EditorToolType.Ramp;
                toolQuarterPipe.Checked = currentTool.Tool == EditorToolType.QuarterPipe;
                toolHalfPipe.Checked = currentTool.Tool == EditorToolType.HalfPipe;
                toolBowl.Checked = currentTool.Tool == EditorToolType.Bowl;
                toolPyramid.Checked = currentTool.Tool == EditorToolType.Pyramid;
                toolTerrain.Checked = currentTool.Tool == EditorToolType.Terrain;
                toolPortalDigger.Checked = currentTool.Tool == EditorToolType.PortalDigger;

                toolUVFixer.Checked = currentTool.TextureUVFixer;

                switch(currentTool.GridSize)
                {
                    case PaintGridSize.Grid2x2:
                        toolGridPaint.Image = Properties.Resources.toolbox_GridPaint2x2_16;
                        toolGridPaint.ToolTipText = "Grid Paint (2x2)";
                        break;
                    case PaintGridSize.Grid3x3:
                        toolGridPaint.Image = Properties.Resources.toolbox_GridPaint3x3_16;
                        toolGridPaint.ToolTipText = "Grid Paint (3x3)";
                        break;
                    case PaintGridSize.Grid4x4:
                        toolGridPaint.Image = Properties.Resources.toolbox_GridPaint4x4_16;
                        toolGridPaint.ToolTipText = "Grid Paint (4x4)";
                        break;
                }
            }

            if (obj is Editor.SelectedTexturesChangedEvent || obj is Editor.InitEvent)
            {
                toolEraser.Checked = _editor.SelectedTexture.Texture == null;
                toolInvisibility.Checked = _editor.SelectedTexture.Texture is TextureInvisible;
            }

            if (obj is Editor.ModeChangedEvent || obj is Editor.InitEvent)
            {
                EditorMode mode = _editor.Mode;
                bool geometryMode = mode == EditorMode.Geometry;

                toolFill.Visible = !geometryMode;
                toolGroup.Visible = !geometryMode;
                toolGridPaint.Visible = !geometryMode;
                toolEraser.Visible = !geometryMode;
                toolInvisibility.Visible = !geometryMode;
                toolUVFixer.Visible = !geometryMode;
                toolFlatten.Visible = geometryMode;
                toolShovel.Visible = geometryMode;
                toolSmooth.Visible = geometryMode;
                toolDrag.Visible = geometryMode;
                toolRamp.Visible = geometryMode;
                toolQuarterPipe.Visible = geometryMode;
                toolHalfPipe.Visible = geometryMode;
                toolBowl.Visible = geometryMode;
                toolPyramid.Visible = geometryMode;
                toolTerrain.Visible = geometryMode;
                toolPortalDigger.Visible = geometryMode;

                toolStrip.AutoSize = true;
                AutoSize = true;
                toolStrip.Visible = mode == EditorMode.FaceEdit || mode == EditorMode.Lighting || mode == EditorMode.Geometry;
            }
        }

        private void ContextMenuTimer_Tick(object sender, EventArgs e)
        {
            var _currentContextMenu = new GridPaintContextMenu(_editor, this);
            _currentContextMenu.Show(Cursor.Position);
            _contextMenuTimer.Stop();
        }

        private void SwitchTool(EditorToolType tool)
        {
            EditorTool currentTool = new EditorTool() { Tool = tool, TextureUVFixer = _editor.Tool.TextureUVFixer, GridSize = _editor.Tool.GridSize };
            _editor.Tool = currentTool;
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

        private void toolTerrain_Click(object sender, EventArgs e)
        {
            SwitchTool(EditorToolType.Terrain);
        }

        private void tooPaint2x2_Click(object sender, EventArgs e)
        {
            SwitchTool(EditorToolType.GridPaint);
        }
        
        private void toolPortalDigger_Click(object sender, EventArgs e)
        {
            SwitchTool(EditorToolType.PortalDigger);
        }

        private void toolInvisibility_Click(object sender, EventArgs e)
        {
            _editor.SelectedTexture = TextureArea.Invisible;
        }

        private void toolEraser_Click(object sender, EventArgs e)
        {
            _editor.SelectedTexture = TextureArea.None;
        }

        private void toolUVFixer_Click(object sender, EventArgs e)
        {
            EditorTool currentTool = new EditorTool() { Tool = _editor.Tool.Tool, TextureUVFixer = !_editor.Tool.TextureUVFixer, GridSize = _editor.Tool.GridSize };
            _editor.Tool = currentTool;
        }

        private void toolGridPaint_MouseUp(object sender, MouseEventArgs e)
        {
            _contextMenuTimer.Stop();
        }

        private void toolGridPaint_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Right)
                _contextMenuTimer.Start();
            else
                ContextMenuTimer_Tick(sender, e);
        }
    }
}
