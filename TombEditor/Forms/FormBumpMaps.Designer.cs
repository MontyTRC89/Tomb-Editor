using DarkUI.Controls;

namespace TombEditor.Forms
{
    partial class FormBumpMaps
    {
        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.textureMap = new TombEditor.Forms.FormBumpMaps.PanelTextureMapForBumpmaps();
            this.butAssignBumpmap = new DarkUI.Controls.DarkButton();
            this.butOk = new DarkUI.Controls.DarkButton();
            this.cmbBump = new DarkUI.Controls.DarkComboBox();
            this.cbUseCustomFile = new DarkUI.Controls.DarkCheckBox();
            this.lblCustomMapPath = new DarkUI.Controls.DarkLabel();
            this.SuspendLayout();
            // 
            // textureMap
            // 
            this.textureMap.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textureMap.Location = new System.Drawing.Point(8, 9);
            this.textureMap.Name = "textureMap";
            this.textureMap.Size = new System.Drawing.Size(409, 495);
            this.textureMap.TabIndex = 0;
            // 
            // butAssignBumpmap
            // 
            this.butAssignBumpmap.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.butAssignBumpmap.Location = new System.Drawing.Point(218, 533);
            this.butAssignBumpmap.Name = "butAssignBumpmap";
            this.butAssignBumpmap.Size = new System.Drawing.Size(113, 23);
            this.butAssignBumpmap.TabIndex = 2;
            this.butAssignBumpmap.Text = "Assign bumpmap";
            this.butAssignBumpmap.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.butAssignBumpmap.Click += new System.EventHandler(this.butAssignBumpmap_Click);
            // 
            // butOk
            // 
            this.butOk.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.butOk.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.butOk.Location = new System.Drawing.Point(337, 533);
            this.butOk.Name = "butOk";
            this.butOk.Size = new System.Drawing.Size(80, 23);
            this.butOk.TabIndex = 3;
            this.butOk.Text = "OK";
            this.butOk.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.butOk.Click += new System.EventHandler(this.butOk_Click);
            // 
            // cmbBump
            // 
            this.cmbBump.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.cmbBump.FormattingEnabled = true;
            this.cmbBump.Location = new System.Drawing.Point(8, 533);
            this.cmbBump.Name = "cmbBump";
            this.cmbBump.Size = new System.Drawing.Size(204, 23);
            this.cmbBump.TabIndex = 14;
            // 
            // cbUseCustomFile
            // 
            this.cbUseCustomFile.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.cbUseCustomFile.AutoCheck = false;
            this.cbUseCustomFile.AutoSize = true;
            this.cbUseCustomFile.Location = new System.Drawing.Point(8, 510);
            this.cbUseCustomFile.Name = "cbUseCustomFile";
            this.cbUseCustomFile.Size = new System.Drawing.Size(107, 17);
            this.cbUseCustomFile.TabIndex = 15;
            this.cbUseCustomFile.Text = "Use custom file:";
            this.cbUseCustomFile.MouseDown += new System.Windows.Forms.MouseEventHandler(this.cbUseCustomFile_MouseDown);
            // 
            // lblCustomMapPath
            // 
            this.lblCustomMapPath.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lblCustomMapPath.ForeColor = System.Drawing.Color.Gray;
            this.lblCustomMapPath.Location = new System.Drawing.Point(108, 512);
            this.lblCustomMapPath.Name = "lblCustomMapPath";
            this.lblCustomMapPath.Size = new System.Drawing.Size(304, 13);
            this.lblCustomMapPath.TabIndex = 16;
            // 
            // FormBumpMaps
            // 
            this.AcceptButton = this.butOk;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.butOk;
            this.ClientSize = new System.Drawing.Size(424, 562);
            this.Controls.Add(this.lblCustomMapPath);
            this.Controls.Add(this.cbUseCustomFile);
            this.Controls.Add(this.cmbBump);
            this.Controls.Add(this.butAssignBumpmap);
            this.Controls.Add(this.butOk);
            this.Controls.Add(this.textureMap);
            this.Font = new System.Drawing.Font("Segoe UI", 8.25F);
            this.MinimizeBox = false;
            this.Name = "FormBumpMaps";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Bumpmaps";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private FormBumpMaps.PanelTextureMapForBumpmaps textureMap;
        private DarkButton butAssignBumpmap;
        private DarkButton butOk;
        private DarkComboBox cmbBump;
        private DarkCheckBox cbUseCustomFile;
        private DarkLabel lblCustomMapPath;
    }
}