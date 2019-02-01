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
            this.toolStrip = new DarkUI.Controls.DarkToolStrip();
            this.toolSelection = new System.Windows.Forms.ToolStripButton();
            this.toolBrush = new System.Windows.Forms.ToolStripButton();
            this.toolShovel = new System.Windows.Forms.ToolStripButton();
            this.toolPencil = new System.Windows.Forms.ToolStripButton();
            this.toolFlatten = new System.Windows.Forms.ToolStripButton();
            this.toolSmooth = new System.Windows.Forms.ToolStripButton();
            this.toolFill = new System.Windows.Forms.ToolStripButton();
            this.toolPaint2x2 = new System.Windows.Forms.ToolStripButton();
            this.toolGroup = new System.Windows.Forms.ToolStripButton();
            this.toolSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.toolDrag = new System.Windows.Forms.ToolStripButton();
            this.toolRamp = new System.Windows.Forms.ToolStripButton();
            this.toolQuarterPipe = new System.Windows.Forms.ToolStripButton();
            this.toolHalfPipe = new System.Windows.Forms.ToolStripButton();
            this.toolBowl = new System.Windows.Forms.ToolStripButton();
            this.toolPyramid = new System.Windows.Forms.ToolStripButton();
            this.toolTerrain = new System.Windows.Forms.ToolStripButton();
            this.toolEraser = new System.Windows.Forms.ToolStripButton();
            this.toolInvisibility = new System.Windows.Forms.ToolStripButton();
            this.toolSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.toolPortalDigger = new System.Windows.Forms.ToolStripButton();
            this.toolUVFixer = new System.Windows.Forms.ToolStripButton();
            this.toolStrip.SuspendLayout();
            this.SuspendLayout();
            // 
            // toolStrip
            // 
            this.toolStrip.AutoSize = false;
            this.toolStrip.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.toolStrip.CanOverflow = false;
            this.toolStrip.Dock = System.Windows.Forms.DockStyle.Fill;
            this.toolStrip.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.toolStrip.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.toolStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolSelection,
            this.toolBrush,
            this.toolShovel,
            this.toolPencil,
            this.toolFlatten,
            this.toolSmooth,
            this.toolFill,
            this.toolPaint2x2,
            this.toolGroup,
            this.toolSeparator1,
            this.toolDrag,
            this.toolRamp,
            this.toolQuarterPipe,
            this.toolHalfPipe,
            this.toolBowl,
            this.toolPyramid,
            this.toolTerrain,
            this.toolEraser,
            this.toolInvisibility,
            this.toolSeparator2,
            this.toolPortalDigger,
            this.toolUVFixer});
            this.toolStrip.LayoutStyle = System.Windows.Forms.ToolStripLayoutStyle.VerticalStackWithOverflow;
            this.toolStrip.Location = new System.Drawing.Point(0, 0);
            this.toolStrip.Name = "toolStrip";
            this.toolStrip.Padding = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.toolStrip.Size = new System.Drawing.Size(28, 453);
            this.toolStrip.TabIndex = 3;
            // 
            // toolSelection
            // 
            this.toolSelection.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.toolSelection.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolSelection.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.toolSelection.Image = global::TombEditor.Properties.Resources.toolbox_Selection_16;
            this.toolSelection.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolSelection.Margin = new System.Windows.Forms.Padding(1);
            this.toolSelection.Name = "toolSelection";
            this.toolSelection.Size = new System.Drawing.Size(23, 20);
            this.toolSelection.ToolTipText = "Selection";
            this.toolSelection.Click += new System.EventHandler(this.toolSelection_Click);
            // 
            // toolBrush
            // 
            this.toolBrush.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.toolBrush.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolBrush.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.toolBrush.Image = global::TombEditor.Properties.Resources.toolbox_Paint_16;
            this.toolBrush.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolBrush.Margin = new System.Windows.Forms.Padding(1);
            this.toolBrush.Name = "toolBrush";
            this.toolBrush.Size = new System.Drawing.Size(23, 20);
            this.toolBrush.ToolTipText = "Brush";
            this.toolBrush.Click += new System.EventHandler(this.toolBrush_Click);
            // 
            // toolShovel
            // 
            this.toolShovel.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.toolShovel.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolShovel.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.toolShovel.Image = global::TombEditor.Properties.Resources.toolbox_Shovel_16;
            this.toolShovel.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolShovel.Margin = new System.Windows.Forms.Padding(1);
            this.toolShovel.Name = "toolShovel";
            this.toolShovel.Size = new System.Drawing.Size(23, 20);
            this.toolShovel.ToolTipText = "Shovel";
            this.toolShovel.Click += new System.EventHandler(this.toolShovel_Click);
            // 
            // toolPencil
            // 
            this.toolPencil.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.toolPencil.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolPencil.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.toolPencil.Image = global::TombEditor.Properties.Resources.toolbox_Pencil_16;
            this.toolPencil.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolPencil.Margin = new System.Windows.Forms.Padding(1);
            this.toolPencil.Name = "toolPencil";
            this.toolPencil.Size = new System.Drawing.Size(23, 20);
            this.toolPencil.ToolTipText = "Pencil";
            this.toolPencil.Click += new System.EventHandler(this.toolPencil_Click);
            // 
            // toolFlatten
            // 
            this.toolFlatten.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.toolFlatten.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolFlatten.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.toolFlatten.Image = global::TombEditor.Properties.Resources.toolbox_Bulldozer_1_16;
            this.toolFlatten.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolFlatten.Margin = new System.Windows.Forms.Padding(1);
            this.toolFlatten.Name = "toolFlatten";
            this.toolFlatten.Size = new System.Drawing.Size(23, 20);
            this.toolFlatten.ToolTipText = "Bulldozer";
            this.toolFlatten.Click += new System.EventHandler(this.toolFlatten_Click);
            // 
            // toolSmooth
            // 
            this.toolSmooth.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.toolSmooth.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolSmooth.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.toolSmooth.Image = global::TombEditor.Properties.Resources.toolbox_Smooth_16;
            this.toolSmooth.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolSmooth.Margin = new System.Windows.Forms.Padding(1);
            this.toolSmooth.Name = "toolSmooth";
            this.toolSmooth.Size = new System.Drawing.Size(23, 20);
            this.toolSmooth.ToolTipText = "Smooth";
            this.toolSmooth.Click += new System.EventHandler(this.toolSmooth_Click);
            // 
            // toolFill
            // 
            this.toolFill.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.toolFill.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolFill.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.toolFill.Image = global::TombEditor.Properties.Resources.toolbox_Fill_16;
            this.toolFill.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolFill.Margin = new System.Windows.Forms.Padding(1);
            this.toolFill.Name = "toolFill";
            this.toolFill.Size = new System.Drawing.Size(23, 20);
            this.toolFill.ToolTipText = "Fill";
            this.toolFill.Click += new System.EventHandler(this.toolFill_Click);
            // 
            // toolPaint2x2
            // 
            this.toolPaint2x2.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.toolPaint2x2.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolPaint2x2.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.toolPaint2x2.Image = global::TombEditor.Properties.Resources.toolbox_Paint2x2_16;
            this.toolPaint2x2.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolPaint2x2.Margin = new System.Windows.Forms.Padding(1);
            this.toolPaint2x2.Name = "toolPaint2x2";
            this.toolPaint2x2.Size = new System.Drawing.Size(23, 20);
            this.toolPaint2x2.ToolTipText = "Grid Paint";
            this.toolPaint2x2.Click += new System.EventHandler(this.tooPaint2x2_Click);
            // 
            // toolGroup
            // 
            this.toolGroup.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.toolGroup.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolGroup.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.toolGroup.Image = global::TombEditor.Properties.Resources.toolbox_GroupTexture_16;
            this.toolGroup.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolGroup.Margin = new System.Windows.Forms.Padding(1);
            this.toolGroup.Name = "toolGroup";
            this.toolGroup.Size = new System.Drawing.Size(23, 20);
            this.toolGroup.ToolTipText = "Group Textuing";
            this.toolGroup.Click += new System.EventHandler(this.toolGroup_Click);
            // 
            // toolSeparator1
            // 
            this.toolSeparator1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.toolSeparator1.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.toolSeparator1.Margin = new System.Windows.Forms.Padding(0, 0, 2, 0);
            this.toolSeparator1.Name = "toolSeparator1";
            this.toolSeparator1.Size = new System.Drawing.Size(23, 6);
            // 
            // toolDrag
            // 
            this.toolDrag.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.toolDrag.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolDrag.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.toolDrag.Image = global::TombEditor.Properties.Resources.toolbox_Drag_16;
            this.toolDrag.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolDrag.Margin = new System.Windows.Forms.Padding(1);
            this.toolDrag.Name = "toolDrag";
            this.toolDrag.Size = new System.Drawing.Size(23, 20);
            this.toolDrag.ToolTipText = "Drag";
            this.toolDrag.Click += new System.EventHandler(this.toolDrag_Click);
            // 
            // toolRamp
            // 
            this.toolRamp.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.toolRamp.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolRamp.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.toolRamp.Image = global::TombEditor.Properties.Resources.toolbox_GroupRamp_16;
            this.toolRamp.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolRamp.Margin = new System.Windows.Forms.Padding(1);
            this.toolRamp.Name = "toolRamp";
            this.toolRamp.Size = new System.Drawing.Size(23, 20);
            this.toolRamp.ToolTipText = "Ramp";
            this.toolRamp.Click += new System.EventHandler(this.toolRamp_Click);
            // 
            // toolQuarterPipe
            // 
            this.toolQuarterPipe.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.toolQuarterPipe.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolQuarterPipe.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.toolQuarterPipe.Image = global::TombEditor.Properties.Resources.toolbox_GroupQuaterPipe_16;
            this.toolQuarterPipe.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolQuarterPipe.Margin = new System.Windows.Forms.Padding(1);
            this.toolQuarterPipe.Name = "toolQuarterPipe";
            this.toolQuarterPipe.Size = new System.Drawing.Size(23, 20);
            this.toolQuarterPipe.ToolTipText = "Quarter Pipe";
            this.toolQuarterPipe.Click += new System.EventHandler(this.toolQuarterPipe_Click);
            // 
            // toolHalfPipe
            // 
            this.toolHalfPipe.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.toolHalfPipe.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolHalfPipe.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.toolHalfPipe.Image = global::TombEditor.Properties.Resources.toolbox_GroupHalfPipe_16;
            this.toolHalfPipe.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolHalfPipe.Margin = new System.Windows.Forms.Padding(1);
            this.toolHalfPipe.Name = "toolHalfPipe";
            this.toolHalfPipe.Size = new System.Drawing.Size(23, 20);
            this.toolHalfPipe.ToolTipText = "Half Pipe";
            this.toolHalfPipe.Click += new System.EventHandler(this.toolHalfPipe_Click);
            // 
            // toolBowl
            // 
            this.toolBowl.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.toolBowl.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolBowl.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.toolBowl.Image = global::TombEditor.Properties.Resources.toolbox_GroupBowl_16;
            this.toolBowl.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolBowl.Margin = new System.Windows.Forms.Padding(1);
            this.toolBowl.Name = "toolBowl";
            this.toolBowl.Size = new System.Drawing.Size(23, 20);
            this.toolBowl.ToolTipText = "Bowl";
            this.toolBowl.Click += new System.EventHandler(this.toolBowl_Click);
            // 
            // toolPyramid
            // 
            this.toolPyramid.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.toolPyramid.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolPyramid.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.toolPyramid.Image = global::TombEditor.Properties.Resources.toolbox_GroupPyramid_16;
            this.toolPyramid.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolPyramid.Margin = new System.Windows.Forms.Padding(1);
            this.toolPyramid.Name = "toolPyramid";
            this.toolPyramid.Size = new System.Drawing.Size(23, 20);
            this.toolPyramid.ToolTipText = "Pyramid";
            this.toolPyramid.Click += new System.EventHandler(this.toolPyramid_Click);
            // 
            // toolTerrain
            // 
            this.toolTerrain.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.toolTerrain.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolTerrain.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.toolTerrain.Image = global::TombEditor.Properties.Resources.toolbox_GroupTerrain_16;
            this.toolTerrain.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolTerrain.Margin = new System.Windows.Forms.Padding(1);
            this.toolTerrain.Name = "toolTerrain";
            this.toolTerrain.Size = new System.Drawing.Size(23, 20);
            this.toolTerrain.ToolTipText = "Terrain";
            this.toolTerrain.Click += new System.EventHandler(this.toolTerrain_Click);
            // 
            // toolEraser
            // 
            this.toolEraser.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.toolEraser.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolEraser.Enabled = true;
            this.toolEraser.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.toolEraser.Image = global::TombEditor.Properties.Resources.toolbox_Eraser_16;
            this.toolEraser.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolEraser.Margin = new System.Windows.Forms.Padding(1);
            this.toolEraser.Name = "toolEraser";
            this.toolEraser.Size = new System.Drawing.Size(23, 20);
            this.toolEraser.ToolTipText = "Eraser";
            this.toolEraser.Click += new System.EventHandler(this.toolEraser_Click);
            // 
            // toolInvisibility
            // 
            this.toolInvisibility.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.toolInvisibility.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolInvisibility.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.toolInvisibility.Image = global::TombEditor.Properties.Resources.toolbox_Invisible_16;
            this.toolInvisibility.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolInvisibility.Margin = new System.Windows.Forms.Padding(1);
            this.toolInvisibility.Name = "toolInvisibility";
            this.toolInvisibility.Size = new System.Drawing.Size(23, 20);
            this.toolInvisibility.ToolTipText = "Invisibility";
            this.toolInvisibility.Click += new System.EventHandler(this.toolInvisibility_Click);
            // 
            // toolSeparator2
            // 
            this.toolSeparator2.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.toolSeparator2.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.toolSeparator2.Margin = new System.Windows.Forms.Padding(0, 0, 2, 0);
            this.toolSeparator2.Name = "toolSeparator2";
            this.toolSeparator2.Size = new System.Drawing.Size(23, 6);
            // 
            // toolPortalDigger
            // 
            this.toolPortalDigger.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.toolPortalDigger.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolPortalDigger.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.toolPortalDigger.Image = global::TombEditor.Properties.Resources.toolbox_PortalDigger_16;
            this.toolPortalDigger.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolPortalDigger.Margin = new System.Windows.Forms.Padding(1);
            this.toolPortalDigger.Name = "toolPortalDigger";
            this.toolPortalDigger.Size = new System.Drawing.Size(23, 20);
            this.toolPortalDigger.ToolTipText = "Portal Digger";
            this.toolPortalDigger.Click += new System.EventHandler(this.toolPortalDigger_Click);
            // 
            // toolUVFixer
            // 
            this.toolUVFixer.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.toolUVFixer.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolUVFixer.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.toolUVFixer.Image = global::TombEditor.Properties.Resources.toolbox_UVFixer_16;
            this.toolUVFixer.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolUVFixer.Margin = new System.Windows.Forms.Padding(1);
            this.toolUVFixer.Name = "toolUVFixer";
            this.toolUVFixer.Size = new System.Drawing.Size(23, 20);
            this.toolUVFixer.Text = "toolStripButton1";
            this.toolUVFixer.ToolTipText = "Fix texture coordinates";
            this.toolUVFixer.Click += new System.EventHandler(this.toolUVFixer_Click);
            // 
            // ToolBox
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.Controls.Add(this.toolStrip);
            this.Margin = new System.Windows.Forms.Padding(0);
            this.Name = "ToolBox";
            this.Size = new System.Drawing.Size(28, 453);
            this.toolStrip.ResumeLayout(false);
            this.toolStrip.PerformLayout();
            this.ResumeLayout(false);

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
        private System.Windows.Forms.ToolStripButton toolPaint2x2;
        private System.Windows.Forms.ToolStripButton toolPortalDigger;
    }
}
