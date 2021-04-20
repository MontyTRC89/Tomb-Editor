namespace TombLib.Forms
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
            this.cbFlipZ = new DarkUI.Controls.DarkCheckBox();
            this.cbFlipY = new DarkUI.Controls.DarkCheckBox();
            this.cbFlipX = new DarkUI.Controls.DarkCheckBox();
            this.cbSwapYZ = new DarkUI.Controls.DarkCheckBox();
            this.cbSwapXZ = new DarkUI.Controls.DarkCheckBox();
            this.cbSwapXY = new DarkUI.Controls.DarkCheckBox();
            this.nmScale = new DarkUI.Controls.DarkNumericUpDown();
            this.lblScale = new DarkUI.Controls.DarkLabel();
            this.cbPremultiplyUV = new DarkUI.Controls.DarkCheckBox();
            this.cbWrapUV = new DarkUI.Controls.DarkCheckBox();
            this.cbFlipUV_V = new DarkUI.Controls.DarkCheckBox();
            this.lblPreset = new DarkUI.Controls.DarkLabel();
            this.cmbPresetList = new DarkUI.Controls.DarkComboBox();
            this.panelContents = new DarkUI.Controls.DarkSectionPanel();
            this.groupMisc = new DarkUI.Controls.DarkGroupBox();
            this.cbSortByName = new DarkUI.Controls.DarkCheckBox();
            this.cbImportBakedLight = new DarkUI.Controls.DarkCheckBox();
            this.groupTextures = new DarkUI.Controls.DarkGroupBox();
            this.groupSize = new DarkUI.Controls.DarkGroupBox();
            this.groupAxis = new DarkUI.Controls.DarkGroupBox();
            this.cbInvertFaces = new DarkUI.Controls.DarkCheckBox();
            ((System.ComponentModel.ISupportInitialize)(this.nmScale)).BeginInit();
            this.panelContents.SuspendLayout();
            this.groupMisc.SuspendLayout();
            this.groupTextures.SuspendLayout();
            this.groupSize.SuspendLayout();
            this.groupAxis.SuspendLayout();
            this.SuspendLayout();
            // 
            // butOK
            // 
            this.butOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.butOK.Checked = false;
            this.butOK.Location = new System.Drawing.Point(141, 269);
            this.butOK.Name = "butOK";
            this.butOK.Size = new System.Drawing.Size(80, 23);
            this.butOK.TabIndex = 16;
            this.butOK.Text = "OK";
            this.butOK.Click += new System.EventHandler(this.butOK_Click);
            // 
            // butCancel
            // 
            this.butCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.butCancel.Checked = false;
            this.butCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.butCancel.Location = new System.Drawing.Point(227, 269);
            this.butCancel.Name = "butCancel";
            this.butCancel.Size = new System.Drawing.Size(80, 23);
            this.butCancel.TabIndex = 17;
            this.butCancel.Text = "Cancel";
            // 
            // cbFlipZ
            // 
            this.cbFlipZ.AutoSize = true;
            this.cbFlipZ.Location = new System.Drawing.Point(6, 136);
            this.cbFlipZ.Name = "cbFlipZ";
            this.cbFlipZ.Size = new System.Drawing.Size(86, 17);
            this.cbFlipZ.TabIndex = 5;
            this.cbFlipZ.Text = "Invert Z axis";
            // 
            // cbFlipY
            // 
            this.cbFlipY.AutoSize = true;
            this.cbFlipY.Location = new System.Drawing.Point(6, 113);
            this.cbFlipY.Name = "cbFlipY";
            this.cbFlipY.Size = new System.Drawing.Size(85, 17);
            this.cbFlipY.TabIndex = 4;
            this.cbFlipY.Text = "Invert Y axis";
            // 
            // cbFlipX
            // 
            this.cbFlipX.AutoSize = true;
            this.cbFlipX.Location = new System.Drawing.Point(6, 90);
            this.cbFlipX.Name = "cbFlipX";
            this.cbFlipX.Size = new System.Drawing.Size(86, 17);
            this.cbFlipX.TabIndex = 3;
            this.cbFlipX.Text = "Invert X axis";
            // 
            // cbSwapYZ
            // 
            this.cbSwapYZ.AutoSize = true;
            this.cbSwapYZ.Location = new System.Drawing.Point(6, 67);
            this.cbSwapYZ.Name = "cbSwapYZ";
            this.cbSwapYZ.Size = new System.Drawing.Size(103, 17);
            this.cbSwapYZ.TabIndex = 2;
            this.cbSwapYZ.Text = "Swap Y / Z axes";
            // 
            // cbSwapXZ
            // 
            this.cbSwapXZ.AutoSize = true;
            this.cbSwapXZ.Location = new System.Drawing.Point(6, 44);
            this.cbSwapXZ.Name = "cbSwapXZ";
            this.cbSwapXZ.Size = new System.Drawing.Size(104, 17);
            this.cbSwapXZ.TabIndex = 1;
            this.cbSwapXZ.Text = "Swap X / Z axes";
            // 
            // cbSwapXY
            // 
            this.cbSwapXY.AutoSize = true;
            this.cbSwapXY.Location = new System.Drawing.Point(6, 21);
            this.cbSwapXY.Name = "cbSwapXY";
            this.cbSwapXY.Size = new System.Drawing.Size(103, 17);
            this.cbSwapXY.TabIndex = 0;
            this.cbSwapXY.Text = "Swap X / Y axes";
            // 
            // nmScale
            // 
            this.nmScale.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.nmScale.DecimalPlaces = 4;
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
            this.nmScale.Location = new System.Drawing.Point(47, 21);
            this.nmScale.LoopValues = false;
            this.nmScale.Maximum = new decimal(new int[] {
            2048,
            0,
            0,
            0});
            this.nmScale.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            262144});
            this.nmScale.Name = "nmScale";
            this.nmScale.Size = new System.Drawing.Size(101, 22);
            this.nmScale.TabIndex = 8;
            this.nmScale.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            // 
            // lblScale
            // 
            this.lblScale.AutoSize = true;
            this.lblScale.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.lblScale.Location = new System.Drawing.Point(5, 23);
            this.lblScale.Name = "lblScale";
            this.lblScale.Size = new System.Drawing.Size(36, 13);
            this.lblScale.TabIndex = 0;
            this.lblScale.Text = "Scale:";
            // 
            // cbPremultiplyUV
            // 
            this.cbPremultiplyUV.Location = new System.Drawing.Point(6, 67);
            this.cbPremultiplyUV.Name = "cbPremultiplyUV";
            this.cbPremultiplyUV.Size = new System.Drawing.Size(130, 17);
            this.cbPremultiplyUV.TabIndex = 11;
            this.cbPremultiplyUV.Text = "Premultiply UV";
            // 
            // cbWrapUV
            // 
            this.cbWrapUV.Location = new System.Drawing.Point(6, 44);
            this.cbWrapUV.Name = "cbWrapUV";
            this.cbWrapUV.Size = new System.Drawing.Size(130, 17);
            this.cbWrapUV.TabIndex = 10;
            this.cbWrapUV.Text = "Wrap UV";
            // 
            // cbFlipUV_V
            // 
            this.cbFlipUV_V.Location = new System.Drawing.Point(6, 21);
            this.cbFlipUV_V.Name = "cbFlipUV_V";
            this.cbFlipUV_V.Size = new System.Drawing.Size(130, 17);
            this.cbFlipUV_V.TabIndex = 9;
            this.cbFlipUV_V.Text = "Invert V coordinate";
            // 
            // lblPreset
            // 
            this.lblPreset.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.lblPreset.AutoSize = true;
            this.lblPreset.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.lblPreset.Location = new System.Drawing.Point(5, 232);
            this.lblPreset.Name = "lblPreset";
            this.lblPreset.Size = new System.Drawing.Size(41, 13);
            this.lblPreset.TabIndex = 5;
            this.lblPreset.Text = "Preset:";
            // 
            // cmbPresetList
            // 
            this.cmbPresetList.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.cmbPresetList.FormattingEnabled = true;
            this.cmbPresetList.Location = new System.Drawing.Point(52, 228);
            this.cmbPresetList.Name = "cmbPresetList";
            this.cmbPresetList.Size = new System.Drawing.Size(243, 23);
            this.cmbPresetList.TabIndex = 15;
            this.cmbPresetList.SelectedIndexChanged += new System.EventHandler(this.cmbPresetList_SelectedIndexChanged);
            // 
            // panelContents
            // 
            this.panelContents.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panelContents.Controls.Add(this.groupMisc);
            this.panelContents.Controls.Add(this.groupTextures);
            this.panelContents.Controls.Add(this.groupSize);
            this.panelContents.Controls.Add(this.groupAxis);
            this.panelContents.Controls.Add(this.cmbPresetList);
            this.panelContents.Controls.Add(this.lblPreset);
            this.panelContents.Location = new System.Drawing.Point(6, 6);
            this.panelContents.Name = "panelContents";
            this.panelContents.SectionHeader = null;
            this.panelContents.Size = new System.Drawing.Size(301, 257);
            this.panelContents.TabIndex = 7;
            // 
            // groupMisc
            // 
            this.groupMisc.Controls.Add(this.cbSortByName);
            this.groupMisc.Controls.Add(this.cbImportBakedLight);
            this.groupMisc.Location = new System.Drawing.Point(140, 157);
            this.groupMisc.Name = "groupMisc";
            this.groupMisc.Size = new System.Drawing.Size(155, 65);
            this.groupMisc.TabIndex = 0;
            this.groupMisc.TabStop = false;
            this.groupMisc.Text = "Misc";
            // 
            // cbSortByName
            // 
            this.cbSortByName.AutoSize = true;
            this.cbSortByName.Location = new System.Drawing.Point(6, 44);
            this.cbSortByName.Name = "cbSortByName";
            this.cbSortByName.Size = new System.Drawing.Size(93, 17);
            this.cbSortByName.TabIndex = 8;
            this.cbSortByName.Text = "Sort by name";
            // 
            // cbImportBakedLight
            // 
            this.cbImportBakedLight.AutoSize = true;
            this.cbImportBakedLight.Location = new System.Drawing.Point(6, 21);
            this.cbImportBakedLight.Name = "cbImportBakedLight";
            this.cbImportBakedLight.Size = new System.Drawing.Size(113, 17);
            this.cbImportBakedLight.TabIndex = 7;
            this.cbImportBakedLight.Text = "Vertex color light";
            // 
            // groupTextures
            // 
            this.groupTextures.Controls.Add(this.cbPremultiplyUV);
            this.groupTextures.Controls.Add(this.cbFlipUV_V);
            this.groupTextures.Controls.Add(this.cbWrapUV);
            this.groupTextures.Location = new System.Drawing.Point(140, 62);
            this.groupTextures.Name = "groupTextures";
            this.groupTextures.Size = new System.Drawing.Size(155, 89);
            this.groupTextures.TabIndex = 8;
            this.groupTextures.TabStop = false;
            this.groupTextures.Text = "Texture mapping";
            // 
            // groupSize
            // 
            this.groupSize.Controls.Add(this.nmScale);
            this.groupSize.Controls.Add(this.lblScale);
            this.groupSize.Location = new System.Drawing.Point(140, 6);
            this.groupSize.Name = "groupSize";
            this.groupSize.Size = new System.Drawing.Size(155, 50);
            this.groupSize.TabIndex = 8;
            this.groupSize.TabStop = false;
            this.groupSize.Text = "Size";
            // 
            // groupAxis
            // 
            this.groupAxis.Controls.Add(this.cbInvertFaces);
            this.groupAxis.Controls.Add(this.cbSwapXY);
            this.groupAxis.Controls.Add(this.cbFlipZ);
            this.groupAxis.Controls.Add(this.cbSwapXZ);
            this.groupAxis.Controls.Add(this.cbFlipY);
            this.groupAxis.Controls.Add(this.cbSwapYZ);
            this.groupAxis.Controls.Add(this.cbFlipX);
            this.groupAxis.Location = new System.Drawing.Point(6, 6);
            this.groupAxis.Name = "groupAxis";
            this.groupAxis.Size = new System.Drawing.Size(128, 216);
            this.groupAxis.TabIndex = 8;
            this.groupAxis.TabStop = false;
            this.groupAxis.Text = "Axis";
            // 
            // cbInvertFaces
            // 
            this.cbInvertFaces.AutoSize = true;
            this.cbInvertFaces.Location = new System.Drawing.Point(6, 159);
            this.cbInvertFaces.Name = "cbInvertFaces";
            this.cbInvertFaces.Size = new System.Drawing.Size(84, 17);
            this.cbInvertFaces.TabIndex = 6;
            this.cbInvertFaces.Text = "Invert faces";
            // 
            // GeometryIOSettingsDialog
            // 
            this.AcceptButton = this.butOK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSize = true;
            this.CancelButton = this.butCancel;
            this.ClientSize = new System.Drawing.Size(313, 298);
            this.Controls.Add(this.panelContents);
            this.Controls.Add(this.butCancel);
            this.Controls.Add(this.butOK);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "GeometryIOSettingsDialog";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            ((System.ComponentModel.ISupportInitialize)(this.nmScale)).EndInit();
            this.panelContents.ResumeLayout(false);
            this.panelContents.PerformLayout();
            this.groupMisc.ResumeLayout(false);
            this.groupMisc.PerformLayout();
            this.groupTextures.ResumeLayout(false);
            this.groupSize.ResumeLayout(false);
            this.groupSize.PerformLayout();
            this.groupAxis.ResumeLayout(false);
            this.groupAxis.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private DarkUI.Controls.DarkButton butOK;
        private DarkUI.Controls.DarkButton butCancel;
        private DarkUI.Controls.DarkCheckBox cbSwapXY;
        private DarkUI.Controls.DarkCheckBox cbSwapXZ;
        private DarkUI.Controls.DarkCheckBox cbSwapYZ;
        private DarkUI.Controls.DarkCheckBox cbFlipZ;
        private DarkUI.Controls.DarkCheckBox cbFlipY;
        private DarkUI.Controls.DarkCheckBox cbFlipX;
        private DarkUI.Controls.DarkNumericUpDown nmScale;
        private DarkUI.Controls.DarkLabel lblScale;
        private DarkUI.Controls.DarkCheckBox cbFlipUV_V;
        private DarkUI.Controls.DarkCheckBox cbWrapUV;
        private DarkUI.Controls.DarkCheckBox cbPremultiplyUV;
        private DarkUI.Controls.DarkLabel lblPreset;
        private DarkUI.Controls.DarkComboBox cmbPresetList;
        private DarkUI.Controls.DarkSectionPanel panelContents;
        private DarkUI.Controls.DarkGroupBox groupTextures;
        private DarkUI.Controls.DarkGroupBox groupSize;
        private DarkUI.Controls.DarkGroupBox groupAxis;
        private DarkUI.Controls.DarkCheckBox cbInvertFaces;
        private DarkUI.Controls.DarkGroupBox groupMisc;
        private DarkUI.Controls.DarkCheckBox cbImportBakedLight;
        private DarkUI.Controls.DarkCheckBox cbSortByName;
    }
}