namespace TombLib.GeometryIO
{
    partial class GeometryIOSettingsDialog
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;
        
        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.butOK = new DarkUI.Controls.DarkButton();
            this.butCancel = new DarkUI.Controls.DarkButton();
            this.darkSectionPanel1 = new DarkUI.Controls.DarkSectionPanel();
            this.cbInvertFaces = new DarkUI.Controls.DarkCheckBox();
            this.cbFlipZ = new DarkUI.Controls.DarkCheckBox();
            this.cbFlipY = new DarkUI.Controls.DarkCheckBox();
            this.cbFlipX = new DarkUI.Controls.DarkCheckBox();
            this.cbSwapYZ = new DarkUI.Controls.DarkCheckBox();
            this.cbSwapXZ = new DarkUI.Controls.DarkCheckBox();
            this.cbSwapXY = new DarkUI.Controls.DarkCheckBox();
            this.darkSectionPanel2 = new DarkUI.Controls.DarkSectionPanel();
            this.nmScale = new DarkUI.Controls.DarkNumericUpDown();
            this.darkLabel1 = new DarkUI.Controls.DarkLabel();
            this.darkSectionPanel3 = new DarkUI.Controls.DarkSectionPanel();
            this.cbPremultiplyUV = new DarkUI.Controls.DarkCheckBox();
            this.cbWrapUV = new DarkUI.Controls.DarkCheckBox();
            this.cbFlipUV_V = new DarkUI.Controls.DarkCheckBox();
            this.lblPreset = new DarkUI.Controls.DarkLabel();
            this.cmbPresetList = new DarkUI.Controls.DarkComboBox();
            this.panelContents = new DarkUI.Controls.DarkSectionPanel();
            this.darkSectionPanel1.SuspendLayout();
            this.darkSectionPanel2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nmScale)).BeginInit();
            this.darkSectionPanel3.SuspendLayout();
            this.panelContents.SuspendLayout();
            this.SuspendLayout();
            // 
            // butOK
            // 
            this.butOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.butOK.Location = new System.Drawing.Point(129, 235);
            this.butOK.Name = "butOK";
            this.butOK.Size = new System.Drawing.Size(84, 23);
            this.butOK.TabIndex = 0;
            this.butOK.Text = "Ok";
            this.butOK.Click += new System.EventHandler(this.butOK_Click);
            // 
            // butCancel
            // 
            this.butCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.butCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.butCancel.Location = new System.Drawing.Point(219, 235);
            this.butCancel.Name = "butCancel";
            this.butCancel.Size = new System.Drawing.Size(84, 23);
            this.butCancel.TabIndex = 1;
            this.butCancel.Text = "Cancel";
            // 
            // darkSectionPanel1
            // 
            this.darkSectionPanel1.Controls.Add(this.cbInvertFaces);
            this.darkSectionPanel1.Controls.Add(this.cbFlipZ);
            this.darkSectionPanel1.Controls.Add(this.cbFlipY);
            this.darkSectionPanel1.Controls.Add(this.cbFlipX);
            this.darkSectionPanel1.Controls.Add(this.cbSwapYZ);
            this.darkSectionPanel1.Controls.Add(this.cbSwapXZ);
            this.darkSectionPanel1.Controls.Add(this.cbSwapXY);
            this.darkSectionPanel1.Location = new System.Drawing.Point(7, 8);
            this.darkSectionPanel1.Name = "darkSectionPanel1";
            this.darkSectionPanel1.SectionHeader = "Axis";
            this.darkSectionPanel1.Size = new System.Drawing.Size(121, 172);
            this.darkSectionPanel1.TabIndex = 2;
            // 
            // cbInvertFaces
            // 
            this.cbInvertFaces.AutoSize = true;
            this.cbInvertFaces.Location = new System.Drawing.Point(10, 170);
            this.cbInvertFaces.Name = "cbInvertFaces";
            this.cbInvertFaces.Size = new System.Drawing.Size(84, 17);
            this.cbInvertFaces.TabIndex = 6;
            this.cbInvertFaces.Text = "Invert faces";
            this.cbInvertFaces.Visible = false;
            // 
            // cbFlipZ
            // 
            this.cbFlipZ.AutoSize = true;
            this.cbFlipZ.Location = new System.Drawing.Point(10, 147);
            this.cbFlipZ.Name = "cbFlipZ";
            this.cbFlipZ.Size = new System.Drawing.Size(86, 17);
            this.cbFlipZ.TabIndex = 5;
            this.cbFlipZ.Text = "Invert Z axis";
            // 
            // cbFlipY
            // 
            this.cbFlipY.AutoSize = true;
            this.cbFlipY.Location = new System.Drawing.Point(10, 124);
            this.cbFlipY.Name = "cbFlipY";
            this.cbFlipY.Size = new System.Drawing.Size(85, 17);
            this.cbFlipY.TabIndex = 4;
            this.cbFlipY.Text = "Invert Y axis";
            // 
            // cbFlipX
            // 
            this.cbFlipX.AutoSize = true;
            this.cbFlipX.Location = new System.Drawing.Point(10, 101);
            this.cbFlipX.Name = "cbFlipX";
            this.cbFlipX.Size = new System.Drawing.Size(86, 17);
            this.cbFlipX.TabIndex = 3;
            this.cbFlipX.Text = "Invert X axis";
            // 
            // cbSwapYZ
            // 
            this.cbSwapYZ.AutoSize = true;
            this.cbSwapYZ.Location = new System.Drawing.Point(10, 78);
            this.cbSwapYZ.Name = "cbSwapYZ";
            this.cbSwapYZ.Size = new System.Drawing.Size(103, 17);
            this.cbSwapYZ.TabIndex = 2;
            this.cbSwapYZ.Text = "Swap Y / Z axes";
            // 
            // cbSwapXZ
            // 
            this.cbSwapXZ.AutoSize = true;
            this.cbSwapXZ.Location = new System.Drawing.Point(10, 55);
            this.cbSwapXZ.Name = "cbSwapXZ";
            this.cbSwapXZ.Size = new System.Drawing.Size(104, 17);
            this.cbSwapXZ.TabIndex = 1;
            this.cbSwapXZ.Text = "Swap X / Z axes";
            // 
            // cbSwapXY
            // 
            this.cbSwapXY.AutoSize = true;
            this.cbSwapXY.Location = new System.Drawing.Point(10, 32);
            this.cbSwapXY.Name = "cbSwapXY";
            this.cbSwapXY.Size = new System.Drawing.Size(103, 17);
            this.cbSwapXY.TabIndex = 0;
            this.cbSwapXY.Text = "Swap X / Y axes";
            // 
            // darkSectionPanel2
            // 
            this.darkSectionPanel2.Controls.Add(this.nmScale);
            this.darkSectionPanel2.Controls.Add(this.darkLabel1);
            this.darkSectionPanel2.Location = new System.Drawing.Point(134, 8);
            this.darkSectionPanel2.Name = "darkSectionPanel2";
            this.darkSectionPanel2.SectionHeader = "Size";
            this.darkSectionPanel2.Size = new System.Drawing.Size(153, 63);
            this.darkSectionPanel2.TabIndex = 3;
            // 
            // nmScale
            // 
            this.nmScale.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(69)))), ((int)(((byte)(73)))), ((int)(((byte)(74)))));
            this.nmScale.DecimalPlaces = 4;
            this.nmScale.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.nmScale.Increment = new decimal(new int[] {
            25,
            0,
            0,
            131072});
            this.nmScale.IncrementAlternate = new decimal(new int[] {
            5,
            0,
            0,
            65536});
            this.nmScale.Location = new System.Drawing.Point(46, 32);
            this.nmScale.Maximum = new decimal(new int[] {
            128,
            0,
            0,
            0});
            this.nmScale.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            262144});
            this.nmScale.MousewheelSingleIncrement = true;
            this.nmScale.Name = "nmScale";
            this.nmScale.Size = new System.Drawing.Size(97, 22);
            this.nmScale.TabIndex = 1;
            this.nmScale.Value = new decimal(new int[] {
            1,
            0,
            0,
            196608});
            // 
            // darkLabel1
            // 
            this.darkLabel1.AutoSize = true;
            this.darkLabel1.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.darkLabel1.Location = new System.Drawing.Point(4, 34);
            this.darkLabel1.Name = "darkLabel1";
            this.darkLabel1.Size = new System.Drawing.Size(36, 13);
            this.darkLabel1.TabIndex = 0;
            this.darkLabel1.Text = "Scale:";
            // 
            // darkSectionPanel3
            // 
            this.darkSectionPanel3.Controls.Add(this.cbPremultiplyUV);
            this.darkSectionPanel3.Controls.Add(this.cbWrapUV);
            this.darkSectionPanel3.Controls.Add(this.cbFlipUV_V);
            this.darkSectionPanel3.Location = new System.Drawing.Point(134, 77);
            this.darkSectionPanel3.Name = "darkSectionPanel3";
            this.darkSectionPanel3.SectionHeader = "Texture mapping";
            this.darkSectionPanel3.Size = new System.Drawing.Size(153, 103);
            this.darkSectionPanel3.TabIndex = 4;
            // 
            // cbPremultiplyUV
            // 
            this.cbPremultiplyUV.Location = new System.Drawing.Point(10, 78);
            this.cbPremultiplyUV.Name = "cbPremultiplyUV";
            this.cbPremultiplyUV.Size = new System.Drawing.Size(139, 17);
            this.cbPremultiplyUV.TabIndex = 3;
            this.cbPremultiplyUV.Text = "Premultiply UV";
            // 
            // cbWrapUV
            // 
            this.cbWrapUV.Location = new System.Drawing.Point(10, 55);
            this.cbWrapUV.Name = "cbWrapUV";
            this.cbWrapUV.Size = new System.Drawing.Size(139, 17);
            this.cbWrapUV.TabIndex = 2;
            this.cbWrapUV.Text = "Wrap UV";
            // 
            // cbFlipUV_V
            // 
            this.cbFlipUV_V.Location = new System.Drawing.Point(10, 32);
            this.cbFlipUV_V.Name = "cbFlipUV_V";
            this.cbFlipUV_V.Size = new System.Drawing.Size(139, 17);
            this.cbFlipUV_V.TabIndex = 1;
            this.cbFlipUV_V.Text = "Invert V coordinate";
            // 
            // lblPreset
            // 
            this.lblPreset.AutoSize = true;
            this.lblPreset.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.lblPreset.Location = new System.Drawing.Point(3, 190);
            this.lblPreset.Name = "lblPreset";
            this.lblPreset.Size = new System.Drawing.Size(41, 13);
            this.lblPreset.TabIndex = 5;
            this.lblPreset.Text = "Preset:";
            // 
            // cmbPresetList
            // 
            this.cmbPresetList.FormattingEnabled = true;
            this.cmbPresetList.Location = new System.Drawing.Point(50, 186);
            this.cmbPresetList.Name = "cmbPresetList";
            this.cmbPresetList.Size = new System.Drawing.Size(237, 23);
            this.cmbPresetList.TabIndex = 6;
            this.cmbPresetList.Text = null;
            this.cmbPresetList.SelectedIndexChanged += new System.EventHandler(this.cmbPresetList_SelectedIndexChanged);
            // 
            // panelContents
            // 
            this.panelContents.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.panelContents.Controls.Add(this.darkSectionPanel1);
            this.panelContents.Controls.Add(this.cmbPresetList);
            this.panelContents.Controls.Add(this.darkSectionPanel2);
            this.panelContents.Controls.Add(this.lblPreset);
            this.panelContents.Controls.Add(this.darkSectionPanel3);
            this.panelContents.Location = new System.Drawing.Point(8, 8);
            this.panelContents.Name = "panelContents";
            this.panelContents.SectionHeader = null;
            this.panelContents.Size = new System.Drawing.Size(295, 218);
            this.panelContents.TabIndex = 7;
            // 
            // GeometryIOSettingsDialog
            // 
            this.AcceptButton = this.butOK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSize = true;
            this.CancelButton = this.butCancel;
            this.ClientSize = new System.Drawing.Size(311, 265);
            this.Controls.Add(this.panelContents);
            this.Controls.Add(this.butCancel);
            this.Controls.Add(this.butOK);
            this.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "GeometryIOSettingsDialog";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Import / export settings";
            this.darkSectionPanel1.ResumeLayout(false);
            this.darkSectionPanel1.PerformLayout();
            this.darkSectionPanel2.ResumeLayout(false);
            this.darkSectionPanel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nmScale)).EndInit();
            this.darkSectionPanel3.ResumeLayout(false);
            this.panelContents.ResumeLayout(false);
            this.panelContents.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private DarkUI.Controls.DarkButton butOK;
        private DarkUI.Controls.DarkButton butCancel;
        private DarkUI.Controls.DarkSectionPanel darkSectionPanel1;
        private DarkUI.Controls.DarkCheckBox cbSwapXY;
        private DarkUI.Controls.DarkCheckBox cbSwapXZ;
        private DarkUI.Controls.DarkCheckBox cbSwapYZ;
        private DarkUI.Controls.DarkCheckBox cbFlipZ;
        private DarkUI.Controls.DarkCheckBox cbFlipY;
        private DarkUI.Controls.DarkCheckBox cbFlipX;
        private DarkUI.Controls.DarkCheckBox cbInvertFaces;
        private DarkUI.Controls.DarkSectionPanel darkSectionPanel2;
        private DarkUI.Controls.DarkNumericUpDown nmScale;
        private DarkUI.Controls.DarkLabel darkLabel1;
        private DarkUI.Controls.DarkSectionPanel darkSectionPanel3;
        private DarkUI.Controls.DarkCheckBox cbFlipUV_V;
        private DarkUI.Controls.DarkCheckBox cbWrapUV;
        private DarkUI.Controls.DarkCheckBox cbPremultiplyUV;
        private DarkUI.Controls.DarkLabel lblPreset;
        private DarkUI.Controls.DarkComboBox cmbPresetList;
        private DarkUI.Controls.DarkSectionPanel panelContents;
    }
}