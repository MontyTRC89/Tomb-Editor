namespace TombEditor.ToolWindows
{
    partial class ToolPalette
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
            this.toolGroup = new System.Windows.Forms.ToolStripButton();
            this.toolSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.toolEraser = new System.Windows.Forms.ToolStripButton();
            this.toolInvisibility = new System.Windows.Forms.ToolStripButton();
            this.toolSeparator2 = new System.Windows.Forms.ToolStripSeparator();
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
            this.toolGroup,
            this.toolSeparator1,
            this.toolEraser,
            this.toolInvisibility,
            this.toolSeparator2,
            this.toolUVFixer});
            this.toolStrip.LayoutStyle = System.Windows.Forms.ToolStripLayoutStyle.Flow;
            this.toolStrip.Location = new System.Drawing.Point(0, 25);
            this.toolStrip.Name = "toolStrip";
            this.toolStrip.Padding = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.toolStrip.Size = new System.Drawing.Size(377, 27);
            this.toolStrip.TabIndex = 2;
            // 
            // toolSelection
            // 
            this.toolSelection.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.toolSelection.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolSelection.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.toolSelection.Image = global::TombEditor.Properties.Resources.Toolbox_Selection_16;
            this.toolSelection.ImageTransparentColor = System.Drawing.Color.Magenta;
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
            this.toolBrush.Image = global::TombEditor.Properties.Resources.Toolbox_Paint_16;
            this.toolBrush.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolBrush.Name = "toolBrush";
            this.toolBrush.Size = new System.Drawing.Size(23, 20);
            this.toolBrush.Text = "toolStripButton7";
            this.toolBrush.ToolTipText = "Brush";
            this.toolBrush.Click += new System.EventHandler(this.toolBrush_Click);
            // 
            // toolShovel
            // 
            this.toolShovel.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.toolShovel.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolShovel.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.toolShovel.Image = global::TombEditor.Properties.Resources.Toolbox_Shovel_16;
            this.toolShovel.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolShovel.Name = "toolShovel";
            this.toolShovel.Size = new System.Drawing.Size(23, 20);
            this.toolShovel.Text = "toolStripButton7";
            this.toolShovel.ToolTipText = "Shovel";
            this.toolShovel.Click += new System.EventHandler(this.toolShovel_Click);
            // 
            // toolPencil
            // 
            this.toolPencil.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.toolPencil.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolPencil.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.toolPencil.Image = global::TombEditor.Properties.Resources.Toolbox_Pencil_16;
            this.toolPencil.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolPencil.Name = "toolPencil";
            this.toolPencil.Size = new System.Drawing.Size(23, 20);
            this.toolPencil.Text = "toolStripButton7";
            this.toolPencil.ToolTipText = "Pencil";
            this.toolPencil.Click += new System.EventHandler(this.toolPencil_Click);
            // 
            // toolFlatten
            // 
            this.toolFlatten.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.toolFlatten.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolFlatten.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.toolFlatten.Image = global::TombEditor.Properties.Resources.Toolbox_Bulldozer_1_16;
            this.toolFlatten.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolFlatten.Name = "toolFlatten";
            this.toolFlatten.Size = new System.Drawing.Size(23, 20);
            this.toolFlatten.Text = "toolStripButton7";
            this.toolFlatten.ToolTipText = "Bulldozer";
            this.toolFlatten.Click += new System.EventHandler(this.toolFlatten_Click);
            // 
            // toolSmooth
            // 
            this.toolSmooth.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.toolSmooth.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolSmooth.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.toolSmooth.Image = global::TombEditor.Properties.Resources.Toolbox_Smooth_16;
            this.toolSmooth.ImageTransparentColor = System.Drawing.Color.Magenta;
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
            this.toolFill.Image = global::TombEditor.Properties.Resources.Toolbox_Fill_16;
            this.toolFill.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolFill.Name = "toolFill";
            this.toolFill.Size = new System.Drawing.Size(23, 20);
            this.toolFill.Text = "toolStripButton7";
            this.toolFill.ToolTipText = "Fill";
            this.toolFill.Click += new System.EventHandler(this.toolFill_Click);
            // 
            // toolGroup
            // 
            this.toolGroup.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.toolGroup.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolGroup.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.toolGroup.Image = global::TombEditor.Properties.Resources.Toolbox_GroupTexture_16;
            this.toolGroup.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolGroup.Name = "toolGroup";
            this.toolGroup.Size = new System.Drawing.Size(23, 20);
            this.toolGroup.ToolTipText = "Group textuing";
            this.toolGroup.Click += new System.EventHandler(this.toolGroup_Click);
            // 
            // toolSeparator1
            // 
            this.toolSeparator1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.toolSeparator1.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.toolSeparator1.Margin = new System.Windows.Forms.Padding(0, 0, 2, 0);
            this.toolSeparator1.Name = "toolSeparator1";
            this.toolSeparator1.Size = new System.Drawing.Size(6, 23);
            // 
            // toolEraser
            // 
            this.toolEraser.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.toolEraser.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolEraser.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.toolEraser.Image = global::TombEditor.Properties.Resources.Toolbox_Eraser_16;
            this.toolEraser.ImageTransparentColor = System.Drawing.Color.Magenta;
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
            this.toolInvisibility.Image = global::TombEditor.Properties.Resources.Toolbox_Invisible_16;
            this.toolInvisibility.ImageTransparentColor = System.Drawing.Color.Magenta;
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
            this.toolSeparator2.Size = new System.Drawing.Size(6, 23);
            // 
            // toolUVFixer
            // 
            this.toolUVFixer.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.toolUVFixer.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolUVFixer.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.toolUVFixer.Image = global::TombEditor.Properties.Resources.Toolbox_UVFixer_16;
            this.toolUVFixer.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolUVFixer.Name = "toolUVFixer";
            this.toolUVFixer.Size = new System.Drawing.Size(23, 20);
            this.toolUVFixer.Text = "toolStripButton1";
            this.toolUVFixer.ToolTipText = "Fix texture coordinates";
            this.toolUVFixer.Click += new System.EventHandler(this.toolUVFixer_Click);
            // 
            // ToolPalette
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.toolStrip);
            this.DefaultDockArea = DarkUI.Docking.DarkDockArea.Right;
            this.DockText = "Tools";
            this.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.MinimumSize = new System.Drawing.Size(28, 52);
            this.Name = "ToolPalette";
            this.SerializationKey = "ToolPalette";
            this.Size = new System.Drawing.Size(377, 52);
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
        private System.Windows.Forms.ToolStripButton toolEraser;
        private System.Windows.Forms.ToolStripButton toolInvisibility;
        private System.Windows.Forms.ToolStripSeparator toolSeparator1;
        private System.Windows.Forms.ToolStripSeparator toolSeparator2;
        private System.Windows.Forms.ToolStripButton toolUVFixer;
        private System.Windows.Forms.ToolStripButton toolGroup;
    }
}
