namespace TombEditor.Controls
{
    partial class PanelRendering3D_ViewOptions
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
            this.darkToolStrip1 = new DarkUI.Controls.DarkToolStrip();
            this.toolStripLabel1 = new System.Windows.Forms.ToolStripLabel();
            this.butDrawMoveables = new System.Windows.Forms.ToolStripButton();
            this.butDrawStatics = new System.Windows.Forms.ToolStripButton();
            this.butDrawImportedGeometry = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator11 = new System.Windows.Forms.ToolStripSeparator();
            this.butDrawLightMeshes = new System.Windows.Forms.ToolStripButton();
            this.butDrawOther = new System.Windows.Forms.ToolStripButton();
            this.butDrawHorizon = new System.Windows.Forms.ToolStripButton();
            this.butDrawPortals = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator12 = new System.Windows.Forms.ToolStripSeparator();
            this.butDrawIllegalSlopes = new System.Windows.Forms.ToolStripButton();
            this.butDrawRoomNames = new System.Windows.Forms.ToolStripButton();
            this.darkToolStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // darkToolStrip1
            // 
            this.darkToolStrip1.AutoSize = false;
            this.darkToolStrip1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(50)))), ((int)(((byte)(53)))), ((int)(((byte)(55)))));
            this.darkToolStrip1.CanOverflow = false;
            this.darkToolStrip1.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.darkToolStrip1.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.darkToolStrip1.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.darkToolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripLabel1,
            this.butDrawMoveables,
            this.butDrawStatics,
            this.butDrawImportedGeometry,
            this.toolStripSeparator11,
            this.butDrawLightMeshes,
            this.butDrawOther,
            this.butDrawHorizon,
            this.butDrawPortals,
            this.toolStripSeparator12,
            this.butDrawIllegalSlopes,
            this.butDrawRoomNames});
            this.darkToolStrip1.Location = new System.Drawing.Point(0, 0);
            this.darkToolStrip1.Name = "darkToolStrip1";
            this.darkToolStrip1.Padding = new System.Windows.Forms.Padding(3, 1, 3, 0);
            this.darkToolStrip1.Size = new System.Drawing.Size(247, 24);
            this.darkToolStrip1.TabIndex = 18;
            this.darkToolStrip1.Text = "darkToolStrip1";
            this.darkToolStrip1.UseUIBackgroundColor = true;
            // 
            // toolStripLabel1
            // 
            this.toolStripLabel1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.toolStripLabel1.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.toolStripLabel1.Image = global::TombEditor.Properties.Resources.grip_vertical;
            this.toolStripLabel1.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.toolStripLabel1.Name = "toolStripLabel1";
            this.toolStripLabel1.Size = new System.Drawing.Size(16, 20);
            this.toolStripLabel1.MouseDown += new System.Windows.Forms.MouseEventHandler(this.toolStripLabel1_MouseDown);
            // 
            // butDrawMoveables
            // 
            this.butDrawMoveables.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.butDrawMoveables.Checked = true;
            this.butDrawMoveables.CheckState = System.Windows.Forms.CheckState.Checked;
            this.butDrawMoveables.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.butDrawMoveables.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.butDrawMoveables.Image = global::TombEditor.Properties.Resources.subtoolbar_icons8_Merry_Go_Round_48_copy;
            this.butDrawMoveables.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.butDrawMoveables.Name = "butDrawMoveables";
            this.butDrawMoveables.Size = new System.Drawing.Size(23, 20);
            this.butDrawMoveables.ToolTipText = "Draw moveables";
            this.butDrawMoveables.Click += new System.EventHandler(this.butDrawMoveables_Click);
            // 
            // butDrawStatics
            // 
            this.butDrawStatics.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.butDrawStatics.Checked = true;
            this.butDrawStatics.CheckState = System.Windows.Forms.CheckState.Checked;
            this.butDrawStatics.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.butDrawStatics.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.butDrawStatics.Image = global::TombEditor.Properties.Resources.subtoolbar_icons8_Obelisk_48_copy;
            this.butDrawStatics.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.butDrawStatics.Name = "butDrawStatics";
            this.butDrawStatics.Size = new System.Drawing.Size(23, 20);
            this.butDrawStatics.ToolTipText = "Draw static meshes";
            this.butDrawStatics.Click += new System.EventHandler(this.butDrawStatics_Click);
            // 
            // butDrawImportedGeometry
            // 
            this.butDrawImportedGeometry.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.butDrawImportedGeometry.Checked = true;
            this.butDrawImportedGeometry.CheckState = System.Windows.Forms.CheckState.Checked;
            this.butDrawImportedGeometry.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.butDrawImportedGeometry.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.butDrawImportedGeometry.Image = global::TombEditor.Properties.Resources.custom_geometry;
            this.butDrawImportedGeometry.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.butDrawImportedGeometry.Name = "butDrawImportedGeometry";
            this.butDrawImportedGeometry.Size = new System.Drawing.Size(23, 20);
            this.butDrawImportedGeometry.ToolTipText = "Draw imported geometry";
            this.butDrawImportedGeometry.Click += new System.EventHandler(this.butDrawImportedGeometry_Click);
            // 
            // toolStripSeparator11
            // 
            this.toolStripSeparator11.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.toolStripSeparator11.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.toolStripSeparator11.Margin = new System.Windows.Forms.Padding(0, 0, 2, 0);
            this.toolStripSeparator11.Name = "toolStripSeparator11";
            this.toolStripSeparator11.Size = new System.Drawing.Size(6, 23);
            // 
            // butDrawLightMeshes
            // 
            this.butDrawLightMeshes.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.butDrawLightMeshes.Checked = true;
            this.butDrawLightMeshes.CheckState = System.Windows.Forms.CheckState.Checked;
            this.butDrawLightMeshes.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.butDrawLightMeshes.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.butDrawLightMeshes.Image = global::TombEditor.Properties.Resources.LightPoint_16;
            this.butDrawLightMeshes.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.butDrawLightMeshes.Name = "butDrawLightMeshes";
            this.butDrawLightMeshes.Size = new System.Drawing.Size(23, 20);
            this.butDrawLightMeshes.ToolTipText = "Draw light meshes";
            this.butDrawLightMeshes.Click += new System.EventHandler(this.butDrawLightMeshes_Click);
            // 
            // butDrawOther
            // 
            this.butDrawOther.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.butDrawOther.Checked = true;
            this.butDrawOther.CheckState = System.Windows.Forms.CheckState.Checked;
            this.butDrawOther.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.butDrawOther.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.butDrawOther.Image = global::TombEditor.Properties.Resources.subtoolbar_icons8_Maintenance_Filled_50_copy;
            this.butDrawOther.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.butDrawOther.Name = "butDrawOther";
            this.butDrawOther.Size = new System.Drawing.Size(23, 20);
            this.butDrawOther.ToolTipText = "Draw other objects";
            this.butDrawOther.Click += new System.EventHandler(this.butDrawOther_Click);
            // 
            // butDrawHorizon
            // 
            this.butDrawHorizon.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.butDrawHorizon.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.butDrawHorizon.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.butDrawHorizon.Image = global::TombEditor.Properties.Resources.earth_element_16;
            this.butDrawHorizon.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.butDrawHorizon.Name = "butDrawHorizon";
            this.butDrawHorizon.Size = new System.Drawing.Size(23, 20);
            this.butDrawHorizon.ToolTipText = "Draw horizon";
            this.butDrawHorizon.Click += new System.EventHandler(this.butDrawHorizon_Click);
            // 
            // butDrawPortals
            // 
            this.butDrawPortals.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.butDrawPortals.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.butDrawPortals.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.butDrawPortals.Image = global::TombEditor.Properties.Resources.door_opened_16;
            this.butDrawPortals.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.butDrawPortals.Name = "butDrawPortals";
            this.butDrawPortals.Size = new System.Drawing.Size(23, 20);
            this.butDrawPortals.ToolTipText = "Draw portals";
            this.butDrawPortals.Click += new System.EventHandler(this.butDrawPortals_Click);
            // 
            // toolStripSeparator12
            // 
            this.toolStripSeparator12.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.toolStripSeparator12.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.toolStripSeparator12.Margin = new System.Windows.Forms.Padding(0, 0, 2, 0);
            this.toolStripSeparator12.Name = "toolStripSeparator12";
            this.toolStripSeparator12.Size = new System.Drawing.Size(6, 23);
            // 
            // butDrawIllegalSlopes
            // 
            this.butDrawIllegalSlopes.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.butDrawIllegalSlopes.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.butDrawIllegalSlopes.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.butDrawIllegalSlopes.Image = global::TombEditor.Properties.Resources.IllegalSlope1_16;
            this.butDrawIllegalSlopes.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.butDrawIllegalSlopes.Name = "butDrawIllegalSlopes";
            this.butDrawIllegalSlopes.Size = new System.Drawing.Size(23, 20);
            this.butDrawIllegalSlopes.ToolTipText = "Draw illegal slopes";
            this.butDrawIllegalSlopes.Click += new System.EventHandler(this.butDrawIllegalSlopes_Click);
            // 
            // butDrawRoomNames
            // 
            this.butDrawRoomNames.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.butDrawRoomNames.Checked = true;
            this.butDrawRoomNames.CheckState = System.Windows.Forms.CheckState.Checked;
            this.butDrawRoomNames.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.butDrawRoomNames.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.butDrawRoomNames.Image = global::TombEditor.Properties.Resources.generic_text_16;
            this.butDrawRoomNames.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.butDrawRoomNames.Name = "butDrawRoomNames";
            this.butDrawRoomNames.Size = new System.Drawing.Size(24, 24);
            this.butDrawRoomNames.ToolTipText = "Draw room names";
            // 
            // PanelRendering3D_ViewOptions
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.darkToolStrip1);
            this.Name = "PanelRendering3D_ViewOptions";
            this.Size = new System.Drawing.Size(247, 24);
            this.MouseUp += new System.Windows.Forms.MouseEventHandler(this.PanelRendering3D_ViewOptions_MouseUp);
            this.darkToolStrip1.ResumeLayout(false);
            this.darkToolStrip1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private DarkUI.Controls.DarkToolStrip darkToolStrip1;
        private System.Windows.Forms.ToolStripButton butDrawMoveables;
        private System.Windows.Forms.ToolStripButton butDrawStatics;
        private System.Windows.Forms.ToolStripButton butDrawImportedGeometry;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator11;
        private System.Windows.Forms.ToolStripButton butDrawLightMeshes;
        private System.Windows.Forms.ToolStripButton butDrawOther;
        private System.Windows.Forms.ToolStripButton butDrawHorizon;
        private System.Windows.Forms.ToolStripButton butDrawPortals;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator12;
        private System.Windows.Forms.ToolStripButton butDrawIllegalSlopes;
        private System.Windows.Forms.ToolStripButton butDrawRoomNames;
        private System.Windows.Forms.ToolStripLabel toolStripLabel1;
    }
}
