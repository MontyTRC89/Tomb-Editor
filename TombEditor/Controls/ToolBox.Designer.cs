namespace TombEditor.Controls
{
    partial class ToolBox
    {
        /// <summary> 
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			toolStrip = new DarkUI.Controls.DarkToolStrip();
			toolSelection = new System.Windows.Forms.ToolStripButton();
			toolBrush = new System.Windows.Forms.ToolStripButton();
			toolShovel = new System.Windows.Forms.ToolStripButton();
			toolPencil = new System.Windows.Forms.ToolStripButton();
			toolFlatten = new System.Windows.Forms.ToolStripButton();
			toolSmooth = new System.Windows.Forms.ToolStripButton();
			toolFill = new System.Windows.Forms.ToolStripButton();
			toolGridPaint = new System.Windows.Forms.ToolStripButton();
			toolGroup = new System.Windows.Forms.ToolStripButton();
			toolSeparator1 = new System.Windows.Forms.ToolStripSeparator();
			toolDrag = new System.Windows.Forms.ToolStripButton();
			toolRamp = new System.Windows.Forms.ToolStripButton();
			toolQuarterPipe = new System.Windows.Forms.ToolStripButton();
			toolHalfPipe = new System.Windows.Forms.ToolStripButton();
			toolBowl = new System.Windows.Forms.ToolStripButton();
			toolPyramid = new System.Windows.Forms.ToolStripButton();
			toolTerrain = new System.Windows.Forms.ToolStripButton();
			toolEraser = new System.Windows.Forms.ToolStripButton();
			toolInvisibility = new System.Windows.Forms.ToolStripButton();
			toolSeparator2 = new System.Windows.Forms.ToolStripSeparator();
			toolPortalDigger = new System.Windows.Forms.ToolStripButton();
			toolUVFixer = new System.Windows.Forms.ToolStripButton();
			toolSeparator3 = new System.Windows.Forms.ToolStripSeparator();
			toolDecalsMode = new System.Windows.Forms.ToolStripButton();
			toolStrip.SuspendLayout();
			SuspendLayout();
			// 
			// toolStrip
			// 
			toolStrip.AutoSize = false;
			toolStrip.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
			toolStrip.CanOverflow = false;
			toolStrip.Dock = System.Windows.Forms.DockStyle.Fill;
			toolStrip.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
			toolStrip.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
			toolStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { toolSelection, toolBrush, toolShovel, toolPencil, toolFlatten, toolSmooth, toolFill, toolGridPaint, toolGroup, toolSeparator1, toolDrag, toolRamp, toolQuarterPipe, toolHalfPipe, toolBowl, toolPyramid, toolTerrain, toolEraser, toolInvisibility, toolSeparator2, toolPortalDigger, toolUVFixer, toolSeparator3, toolDecalsMode });
			toolStrip.LayoutStyle = System.Windows.Forms.ToolStripLayoutStyle.VerticalStackWithOverflow;
			toolStrip.Location = new System.Drawing.Point(0, 0);
			toolStrip.Name = "toolStrip";
			toolStrip.Padding = new System.Windows.Forms.Padding(1, 0, 1, 0);
			toolStrip.Size = new System.Drawing.Size(28, 523);
			toolStrip.TabIndex = 3;
			// 
			// toolSelection
			// 
			toolSelection.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
			toolSelection.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			toolSelection.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
			toolSelection.Image = Properties.Resources.toolbox_Selection_16;
			toolSelection.ImageTransparentColor = System.Drawing.Color.Magenta;
			toolSelection.Margin = new System.Windows.Forms.Padding(1);
			toolSelection.Name = "toolSelection";
			toolSelection.Size = new System.Drawing.Size(23, 20);
			toolSelection.ToolTipText = "Selection";
			toolSelection.Click += toolSelection_Click;
			// 
			// toolBrush
			// 
			toolBrush.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
			toolBrush.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			toolBrush.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
			toolBrush.Image = Properties.Resources.toolbox_Paint_16;
			toolBrush.ImageTransparentColor = System.Drawing.Color.Magenta;
			toolBrush.Margin = new System.Windows.Forms.Padding(1);
			toolBrush.Name = "toolBrush";
			toolBrush.Size = new System.Drawing.Size(23, 20);
			toolBrush.ToolTipText = "Brush";
			toolBrush.Click += toolBrush_Click;
			// 
			// toolShovel
			// 
			toolShovel.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
			toolShovel.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			toolShovel.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
			toolShovel.Image = Properties.Resources.toolbox_Shovel_16;
			toolShovel.ImageTransparentColor = System.Drawing.Color.Magenta;
			toolShovel.Margin = new System.Windows.Forms.Padding(1);
			toolShovel.Name = "toolShovel";
			toolShovel.Size = new System.Drawing.Size(23, 20);
			toolShovel.ToolTipText = "Shovel";
			toolShovel.Click += toolShovel_Click;
			// 
			// toolPencil
			// 
			toolPencil.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
			toolPencil.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			toolPencil.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
			toolPencil.Image = Properties.Resources.toolbox_Pencil_16;
			toolPencil.ImageTransparentColor = System.Drawing.Color.Magenta;
			toolPencil.Margin = new System.Windows.Forms.Padding(1);
			toolPencil.Name = "toolPencil";
			toolPencil.Size = new System.Drawing.Size(23, 20);
			toolPencil.ToolTipText = "Pencil";
			toolPencil.Click += toolPencil_Click;
			// 
			// toolFlatten
			// 
			toolFlatten.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
			toolFlatten.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			toolFlatten.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
			toolFlatten.Image = Properties.Resources.toolbox_Bulldozer_1_16;
			toolFlatten.ImageTransparentColor = System.Drawing.Color.Magenta;
			toolFlatten.Margin = new System.Windows.Forms.Padding(1);
			toolFlatten.Name = "toolFlatten";
			toolFlatten.Size = new System.Drawing.Size(23, 20);
			toolFlatten.ToolTipText = "Bulldozer";
			toolFlatten.Click += toolFlatten_Click;
			// 
			// toolSmooth
			// 
			toolSmooth.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
			toolSmooth.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			toolSmooth.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
			toolSmooth.Image = Properties.Resources.toolbox_Smooth_16;
			toolSmooth.ImageTransparentColor = System.Drawing.Color.Magenta;
			toolSmooth.Margin = new System.Windows.Forms.Padding(1);
			toolSmooth.Name = "toolSmooth";
			toolSmooth.Size = new System.Drawing.Size(23, 20);
			toolSmooth.ToolTipText = "Smooth";
			toolSmooth.Click += toolSmooth_Click;
			// 
			// toolFill
			// 
			toolFill.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
			toolFill.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			toolFill.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
			toolFill.Image = Properties.Resources.toolbox_Fill_16;
			toolFill.ImageTransparentColor = System.Drawing.Color.Magenta;
			toolFill.Margin = new System.Windows.Forms.Padding(1);
			toolFill.Name = "toolFill";
			toolFill.Size = new System.Drawing.Size(23, 20);
			toolFill.ToolTipText = "Fill";
			toolFill.Click += toolFill_Click;
			// 
			// toolGridPaint
			// 
			toolGridPaint.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
			toolGridPaint.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			toolGridPaint.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
			toolGridPaint.Image = Properties.Resources.toolbox_Paint2x2_16;
			toolGridPaint.ImageTransparentColor = System.Drawing.Color.Magenta;
			toolGridPaint.Margin = new System.Windows.Forms.Padding(1);
			toolGridPaint.Name = "toolGridPaint";
			toolGridPaint.Size = new System.Drawing.Size(23, 20);
			toolGridPaint.ToolTipText = "Grid Paint (2x2)";
			toolGridPaint.Click += tooPaint2x2_Click;
			toolGridPaint.MouseDown += toolGridPaint_MouseDown;
			toolGridPaint.MouseUp += toolGridPaint_MouseUp;
			// 
			// toolGroup
			// 
			toolGroup.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
			toolGroup.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			toolGroup.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
			toolGroup.Image = Properties.Resources.toolbox_GroupTexture_16;
			toolGroup.ImageTransparentColor = System.Drawing.Color.Magenta;
			toolGroup.Margin = new System.Windows.Forms.Padding(1);
			toolGroup.Name = "toolGroup";
			toolGroup.Size = new System.Drawing.Size(23, 20);
			toolGroup.ToolTipText = "Group Texturing";
			toolGroup.Click += toolGroup_Click;
			// 
			// toolSeparator1
			// 
			toolSeparator1.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
			toolSeparator1.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
			toolSeparator1.Margin = new System.Windows.Forms.Padding(0, 0, 2, 0);
			toolSeparator1.Name = "toolSeparator1";
			toolSeparator1.Size = new System.Drawing.Size(23, 6);
			// 
			// toolDrag
			// 
			toolDrag.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
			toolDrag.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			toolDrag.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
			toolDrag.Image = Properties.Resources.toolbox_Drag_16;
			toolDrag.ImageTransparentColor = System.Drawing.Color.Magenta;
			toolDrag.Margin = new System.Windows.Forms.Padding(1);
			toolDrag.Name = "toolDrag";
			toolDrag.Size = new System.Drawing.Size(23, 20);
			toolDrag.ToolTipText = "Drag";
			toolDrag.Click += toolDrag_Click;
			// 
			// toolRamp
			// 
			toolRamp.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
			toolRamp.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			toolRamp.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
			toolRamp.Image = Properties.Resources.toolbox_GroupRamp_16;
			toolRamp.ImageTransparentColor = System.Drawing.Color.Magenta;
			toolRamp.Margin = new System.Windows.Forms.Padding(1);
			toolRamp.Name = "toolRamp";
			toolRamp.Size = new System.Drawing.Size(23, 20);
			toolRamp.ToolTipText = "Ramp";
			toolRamp.Click += toolRamp_Click;
			// 
			// toolQuarterPipe
			// 
			toolQuarterPipe.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
			toolQuarterPipe.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			toolQuarterPipe.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
			toolQuarterPipe.Image = Properties.Resources.toolbox_GroupQuaterPipe_16;
			toolQuarterPipe.ImageTransparentColor = System.Drawing.Color.Magenta;
			toolQuarterPipe.Margin = new System.Windows.Forms.Padding(1);
			toolQuarterPipe.Name = "toolQuarterPipe";
			toolQuarterPipe.Size = new System.Drawing.Size(23, 20);
			toolQuarterPipe.ToolTipText = "Quarter Pipe";
			toolQuarterPipe.Click += toolQuarterPipe_Click;
			// 
			// toolHalfPipe
			// 
			toolHalfPipe.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
			toolHalfPipe.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			toolHalfPipe.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
			toolHalfPipe.Image = Properties.Resources.toolbox_GroupHalfPipe_16;
			toolHalfPipe.ImageTransparentColor = System.Drawing.Color.Magenta;
			toolHalfPipe.Margin = new System.Windows.Forms.Padding(1);
			toolHalfPipe.Name = "toolHalfPipe";
			toolHalfPipe.Size = new System.Drawing.Size(23, 20);
			toolHalfPipe.ToolTipText = "Half Pipe";
			toolHalfPipe.Click += toolHalfPipe_Click;
			// 
			// toolBowl
			// 
			toolBowl.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
			toolBowl.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			toolBowl.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
			toolBowl.Image = Properties.Resources.toolbox_GroupBowl_16;
			toolBowl.ImageTransparentColor = System.Drawing.Color.Magenta;
			toolBowl.Margin = new System.Windows.Forms.Padding(1);
			toolBowl.Name = "toolBowl";
			toolBowl.Size = new System.Drawing.Size(23, 20);
			toolBowl.ToolTipText = "Bowl";
			toolBowl.Click += toolBowl_Click;
			// 
			// toolPyramid
			// 
			toolPyramid.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
			toolPyramid.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			toolPyramid.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
			toolPyramid.Image = Properties.Resources.toolbox_GroupPyramid_16;
			toolPyramid.ImageTransparentColor = System.Drawing.Color.Magenta;
			toolPyramid.Margin = new System.Windows.Forms.Padding(1);
			toolPyramid.Name = "toolPyramid";
			toolPyramid.Size = new System.Drawing.Size(23, 20);
			toolPyramid.ToolTipText = "Pyramid";
			toolPyramid.Click += toolPyramid_Click;
			// 
			// toolTerrain
			// 
			toolTerrain.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
			toolTerrain.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			toolTerrain.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
			toolTerrain.Image = Properties.Resources.toolbox_GroupTerrain_16;
			toolTerrain.ImageTransparentColor = System.Drawing.Color.Magenta;
			toolTerrain.Margin = new System.Windows.Forms.Padding(1);
			toolTerrain.Name = "toolTerrain";
			toolTerrain.Size = new System.Drawing.Size(23, 20);
			toolTerrain.ToolTipText = "Terrain";
			toolTerrain.Click += toolTerrain_Click;
			// 
			// toolEraser
			// 
			toolEraser.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
			toolEraser.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			toolEraser.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
			toolEraser.Image = Properties.Resources.toolbox_Eraser_16;
			toolEraser.ImageTransparentColor = System.Drawing.Color.Magenta;
			toolEraser.Margin = new System.Windows.Forms.Padding(1);
			toolEraser.Name = "toolEraser";
			toolEraser.Size = new System.Drawing.Size(23, 20);
			toolEraser.ToolTipText = "Eraser";
			toolEraser.Click += toolEraser_Click;
			// 
			// toolInvisibility
			// 
			toolInvisibility.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
			toolInvisibility.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			toolInvisibility.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
			toolInvisibility.Image = Properties.Resources.toolbox_Invisible_16;
			toolInvisibility.ImageTransparentColor = System.Drawing.Color.Magenta;
			toolInvisibility.Margin = new System.Windows.Forms.Padding(1);
			toolInvisibility.Name = "toolInvisibility";
			toolInvisibility.Size = new System.Drawing.Size(23, 20);
			toolInvisibility.ToolTipText = "Invisibility";
			toolInvisibility.Click += toolInvisibility_Click;
			// 
			// toolSeparator2
			// 
			toolSeparator2.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
			toolSeparator2.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
			toolSeparator2.Margin = new System.Windows.Forms.Padding(0, 0, 2, 0);
			toolSeparator2.Name = "toolSeparator2";
			toolSeparator2.Size = new System.Drawing.Size(23, 6);
			// 
			// toolPortalDigger
			// 
			toolPortalDigger.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
			toolPortalDigger.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			toolPortalDigger.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
			toolPortalDigger.Image = Properties.Resources.toolbox_PortalDigger_16;
			toolPortalDigger.ImageTransparentColor = System.Drawing.Color.Magenta;
			toolPortalDigger.Margin = new System.Windows.Forms.Padding(1);
			toolPortalDigger.Name = "toolPortalDigger";
			toolPortalDigger.Size = new System.Drawing.Size(23, 20);
			toolPortalDigger.ToolTipText = "Portal Digger";
			toolPortalDigger.Click += toolPortalDigger_Click;
			// 
			// toolUVFixer
			// 
			toolUVFixer.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
			toolUVFixer.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			toolUVFixer.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
			toolUVFixer.Image = Properties.Resources.toolbox_UVFixer_16;
			toolUVFixer.ImageTransparentColor = System.Drawing.Color.Magenta;
			toolUVFixer.Margin = new System.Windows.Forms.Padding(1);
			toolUVFixer.Name = "toolUVFixer";
			toolUVFixer.Size = new System.Drawing.Size(23, 20);
			toolUVFixer.ToolTipText = "Fix texture coordinates";
			toolUVFixer.Click += toolUVFixer_Click;
			// 
			// toolSeparator3
			// 
			toolSeparator3.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
			toolSeparator3.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
			toolSeparator3.Margin = new System.Windows.Forms.Padding(0, 0, 2, 0);
			toolSeparator3.Name = "toolSeparator3";
			toolSeparator3.Size = new System.Drawing.Size(23, 6);
			// 
			// toolDecalsMode
			// 
			toolDecalsMode.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
			toolDecalsMode.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			toolDecalsMode.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
			toolDecalsMode.Image = Properties.Resources.general_sticker_16;
			toolDecalsMode.ImageTransparentColor = System.Drawing.Color.Magenta;
			toolDecalsMode.Margin = new System.Windows.Forms.Padding(1);
			toolDecalsMode.Name = "toolDecalsMode";
			toolDecalsMode.Size = new System.Drawing.Size(23, 20);
			toolDecalsMode.ToolTipText = "Toggle decals mode";
			toolDecalsMode.Click += toolDecalsMode_Click;
			// 
			// ToolBox
			// 
			AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
			AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			Controls.Add(toolStrip);
			Margin = new System.Windows.Forms.Padding(0);
			Name = "ToolBox";
			Size = new System.Drawing.Size(28, 523);
			toolStrip.ResumeLayout(false);
			toolStrip.PerformLayout();
			ResumeLayout(false);
		}

		#endregion

		private DarkUI.Controls.DarkToolStrip toolStrip;
        private System.Windows.Forms.ToolStripButton toolSelection;
        private System.Windows.Forms.ToolStripButton toolBrush;
        private System.Windows.Forms.ToolStripButton toolShovel;
        private System.Windows.Forms.ToolStripButton toolPencil;
        private System.Windows.Forms.ToolStripButton toolFlatten;
        private System.Windows.Forms.ToolStripButton toolSmooth;
        private System.Windows.Forms.ToolStripButton toolFill;
        private System.Windows.Forms.ToolStripButton toolGroup;
        private System.Windows.Forms.ToolStripSeparator toolSeparator1;
        private System.Windows.Forms.ToolStripButton toolDrag;
        private System.Windows.Forms.ToolStripButton toolRamp;
        private System.Windows.Forms.ToolStripButton toolQuarterPipe;
        private System.Windows.Forms.ToolStripButton toolHalfPipe;
        private System.Windows.Forms.ToolStripButton toolBowl;
        private System.Windows.Forms.ToolStripButton toolPyramid;
        private System.Windows.Forms.ToolStripButton toolTerrain;
        private System.Windows.Forms.ToolStripButton toolEraser;
        private System.Windows.Forms.ToolStripButton toolInvisibility;
        private System.Windows.Forms.ToolStripSeparator toolSeparator2;
        private System.Windows.Forms.ToolStripButton toolUVFixer;
        private System.Windows.Forms.ToolStripButton toolGridPaint;
        private System.Windows.Forms.ToolStripButton toolPortalDigger;
		private System.Windows.Forms.ToolStripSeparator toolSeparator3;
		private System.Windows.Forms.ToolStripButton toolDecalsMode;
	}
}
