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
			butOK = new DarkUI.Controls.DarkButton();
			butCancel = new DarkUI.Controls.DarkButton();
			cbFlipZ = new DarkUI.Controls.DarkCheckBox();
			cbFlipY = new DarkUI.Controls.DarkCheckBox();
			cbFlipX = new DarkUI.Controls.DarkCheckBox();
			cbSwapYZ = new DarkUI.Controls.DarkCheckBox();
			cbSwapXZ = new DarkUI.Controls.DarkCheckBox();
			cbSwapXY = new DarkUI.Controls.DarkCheckBox();
			nmScale = new DarkUI.Controls.DarkNumericUpDown();
			lblScale = new DarkUI.Controls.DarkLabel();
			cbPremultiplyUV = new DarkUI.Controls.DarkCheckBox();
			cbWrapUV = new DarkUI.Controls.DarkCheckBox();
			cbFlipUV_V = new DarkUI.Controls.DarkCheckBox();
			lblPreset = new DarkUI.Controls.DarkLabel();
			cmbPresetList = new DarkUI.Controls.DarkComboBox();
			panelContents = new DarkUI.Controls.DarkSectionPanel();
			groupMisc = new DarkUI.Controls.DarkGroupBox();
			cbPadPackedTextures = new DarkUI.Controls.DarkCheckBox();
			cbPackTextures = new DarkUI.Controls.DarkCheckBox();
			cbSortByName = new DarkUI.Controls.DarkCheckBox();
			cbImportBakedLight = new DarkUI.Controls.DarkCheckBox();
			groupTextures = new DarkUI.Controls.DarkGroupBox();
			cbUVMap = new DarkUI.Controls.DarkCheckBox();
			groupSize = new DarkUI.Controls.DarkGroupBox();
			groupAxis = new DarkUI.Controls.DarkGroupBox();
			cbInvertFaces = new DarkUI.Controls.DarkCheckBox();
			cbKeepTexturesExternally = new DarkUI.Controls.DarkCheckBox();
			((System.ComponentModel.ISupportInitialize)nmScale).BeginInit();
			panelContents.SuspendLayout();
			groupMisc.SuspendLayout();
			groupTextures.SuspendLayout();
			groupSize.SuspendLayout();
			groupAxis.SuspendLayout();
			SuspendLayout();
			// 
			// butOK
			// 
			butOK.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
			butOK.Checked = false;
			butOK.Location = new System.Drawing.Point(141, 312);
			butOK.Name = "butOK";
			butOK.Size = new System.Drawing.Size(80, 23);
			butOK.TabIndex = 16;
			butOK.Text = "OK";
			butOK.Click += butOK_Click;
			// 
			// butCancel
			// 
			butCancel.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
			butCancel.Checked = false;
			butCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			butCancel.Location = new System.Drawing.Point(227, 312);
			butCancel.Name = "butCancel";
			butCancel.Size = new System.Drawing.Size(80, 23);
			butCancel.TabIndex = 17;
			butCancel.Text = "Cancel";
			// 
			// cbFlipZ
			// 
			cbFlipZ.AutoSize = true;
			cbFlipZ.Location = new System.Drawing.Point(6, 136);
			cbFlipZ.Name = "cbFlipZ";
			cbFlipZ.Size = new System.Drawing.Size(86, 17);
			cbFlipZ.TabIndex = 5;
			cbFlipZ.Text = "Invert Z axis";
			// 
			// cbFlipY
			// 
			cbFlipY.AutoSize = true;
			cbFlipY.Location = new System.Drawing.Point(6, 113);
			cbFlipY.Name = "cbFlipY";
			cbFlipY.Size = new System.Drawing.Size(85, 17);
			cbFlipY.TabIndex = 4;
			cbFlipY.Text = "Invert Y axis";
			// 
			// cbFlipX
			// 
			cbFlipX.AutoSize = true;
			cbFlipX.Location = new System.Drawing.Point(6, 90);
			cbFlipX.Name = "cbFlipX";
			cbFlipX.Size = new System.Drawing.Size(86, 17);
			cbFlipX.TabIndex = 3;
			cbFlipX.Text = "Invert X axis";
			// 
			// cbSwapYZ
			// 
			cbSwapYZ.AutoSize = true;
			cbSwapYZ.Location = new System.Drawing.Point(6, 67);
			cbSwapYZ.Name = "cbSwapYZ";
			cbSwapYZ.Size = new System.Drawing.Size(103, 17);
			cbSwapYZ.TabIndex = 2;
			cbSwapYZ.Text = "Swap Y / Z axes";
			// 
			// cbSwapXZ
			// 
			cbSwapXZ.AutoSize = true;
			cbSwapXZ.Location = new System.Drawing.Point(6, 44);
			cbSwapXZ.Name = "cbSwapXZ";
			cbSwapXZ.Size = new System.Drawing.Size(104, 17);
			cbSwapXZ.TabIndex = 1;
			cbSwapXZ.Text = "Swap X / Z axes";
			// 
			// cbSwapXY
			// 
			cbSwapXY.AutoSize = true;
			cbSwapXY.Location = new System.Drawing.Point(6, 21);
			cbSwapXY.Name = "cbSwapXY";
			cbSwapXY.Size = new System.Drawing.Size(103, 17);
			cbSwapXY.TabIndex = 0;
			cbSwapXY.Text = "Swap X / Y axes";
			// 
			// nmScale
			// 
			nmScale.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
			nmScale.DecimalPlaces = 4;
			nmScale.Increment = new decimal(new int[] { 25, 0, 0, 131072 });
			nmScale.IncrementAlternate = new decimal(new int[] { 5, 0, 0, 65536 });
			nmScale.Location = new System.Drawing.Point(47, 21);
			nmScale.LoopValues = false;
			nmScale.Maximum = new decimal(new int[] { 2048, 0, 0, 0 });
			nmScale.Minimum = new decimal(new int[] { 1, 0, 0, 262144 });
			nmScale.Name = "nmScale";
			nmScale.Size = new System.Drawing.Size(75, 22);
			nmScale.TabIndex = 8;
			nmScale.Value = new decimal(new int[] { 1, 0, 0, 0 });
			// 
			// lblScale
			// 
			lblScale.AutoSize = true;
			lblScale.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
			lblScale.Location = new System.Drawing.Point(5, 23);
			lblScale.Name = "lblScale";
			lblScale.Size = new System.Drawing.Size(36, 13);
			lblScale.TabIndex = 0;
			lblScale.Text = "Scale:";
			// 
			// cbPremultiplyUV
			// 
			cbPremultiplyUV.Location = new System.Drawing.Point(6, 90);
			cbPremultiplyUV.Name = "cbPremultiplyUV";
			cbPremultiplyUV.Size = new System.Drawing.Size(130, 17);
			cbPremultiplyUV.TabIndex = 11;
			cbPremultiplyUV.Text = "Premultiply UV";
			// 
			// cbWrapUV
			// 
			cbWrapUV.Location = new System.Drawing.Point(6, 67);
			cbWrapUV.Name = "cbWrapUV";
			cbWrapUV.Size = new System.Drawing.Size(130, 17);
			cbWrapUV.TabIndex = 10;
			cbWrapUV.Text = "Wrap UV";
			// 
			// cbFlipUV_V
			// 
			cbFlipUV_V.Location = new System.Drawing.Point(6, 21);
			cbFlipUV_V.Name = "cbFlipUV_V";
			cbFlipUV_V.Size = new System.Drawing.Size(130, 17);
			cbFlipUV_V.TabIndex = 9;
			cbFlipUV_V.Text = "Invert V coordinate";
			// 
			// lblPreset
			// 
			lblPreset.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
			lblPreset.AutoSize = true;
			lblPreset.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
			lblPreset.Location = new System.Drawing.Point(5, 275);
			lblPreset.Name = "lblPreset";
			lblPreset.Size = new System.Drawing.Size(41, 13);
			lblPreset.TabIndex = 5;
			lblPreset.Text = "Preset:";
			// 
			// cmbPresetList
			// 
			cmbPresetList.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
			cmbPresetList.FormattingEnabled = true;
			cmbPresetList.Location = new System.Drawing.Point(52, 271);
			cmbPresetList.Name = "cmbPresetList";
			cmbPresetList.Size = new System.Drawing.Size(243, 23);
			cmbPresetList.TabIndex = 15;
			cmbPresetList.SelectedIndexChanged += cmbPresetList_SelectedIndexChanged;
			// 
			// panelContents
			// 
			panelContents.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
			panelContents.Controls.Add(groupMisc);
			panelContents.Controls.Add(groupTextures);
			panelContents.Controls.Add(groupSize);
			panelContents.Controls.Add(groupAxis);
			panelContents.Controls.Add(cmbPresetList);
			panelContents.Controls.Add(lblPreset);
			panelContents.Location = new System.Drawing.Point(6, 6);
			panelContents.Name = "panelContents";
			panelContents.SectionHeader = null;
			panelContents.Size = new System.Drawing.Size(301, 300);
			panelContents.TabIndex = 7;
			// 
			// groupMisc
			// 
			groupMisc.Controls.Add(cbKeepTexturesExternally);
			groupMisc.Controls.Add(cbPadPackedTextures);
			groupMisc.Controls.Add(cbPackTextures);
			groupMisc.Controls.Add(cbSortByName);
			groupMisc.Controls.Add(cbImportBakedLight);
			groupMisc.Location = new System.Drawing.Point(140, 125);
			groupMisc.Name = "groupMisc";
			groupMisc.Size = new System.Drawing.Size(155, 140);
			groupMisc.TabIndex = 0;
			groupMisc.TabStop = false;
			groupMisc.Text = "Misc";
			// 
			// cbPadPackedTextures
			// 
			cbPadPackedTextures.AutoSize = true;
			cbPadPackedTextures.Location = new System.Drawing.Point(6, 67);
			cbPadPackedTextures.Name = "cbPadPackedTextures";
			cbPadPackedTextures.Size = new System.Drawing.Size(129, 17);
			cbPadPackedTextures.TabIndex = 10;
			cbPadPackedTextures.Text = "Pad packed textures";
			// 
			// cbPackTextures
			// 
			cbPackTextures.AutoSize = true;
			cbPackTextures.Location = new System.Drawing.Point(6, 44);
			cbPackTextures.Name = "cbPackTextures";
			cbPackTextures.Size = new System.Drawing.Size(93, 17);
			cbPackTextures.TabIndex = 9;
			cbPackTextures.Text = "Pack textures";
			// 
			// cbSortByName
			// 
			cbSortByName.AutoSize = true;
			cbSortByName.Location = new System.Drawing.Point(6, 91);
			cbSortByName.Name = "cbSortByName";
			cbSortByName.Size = new System.Drawing.Size(93, 17);
			cbSortByName.TabIndex = 8;
			cbSortByName.Text = "Sort by name";
			// 
			// cbImportBakedLight
			// 
			cbImportBakedLight.AutoSize = true;
			cbImportBakedLight.Location = new System.Drawing.Point(6, 21);
			cbImportBakedLight.Name = "cbImportBakedLight";
			cbImportBakedLight.Size = new System.Drawing.Size(113, 17);
			cbImportBakedLight.TabIndex = 7;
			cbImportBakedLight.Text = "Vertex color light";
			// 
			// groupTextures
			// 
			groupTextures.Controls.Add(cbUVMap);
			groupTextures.Controls.Add(cbPremultiplyUV);
			groupTextures.Controls.Add(cbFlipUV_V);
			groupTextures.Controls.Add(cbWrapUV);
			groupTextures.Location = new System.Drawing.Point(140, 6);
			groupTextures.Name = "groupTextures";
			groupTextures.Size = new System.Drawing.Size(155, 111);
			groupTextures.TabIndex = 8;
			groupTextures.TabStop = false;
			groupTextures.Text = "Texture mapping";
			// 
			// cbUVMap
			// 
			cbUVMap.Location = new System.Drawing.Point(6, 44);
			cbUVMap.Name = "cbUVMap";
			cbUVMap.Size = new System.Drawing.Size(130, 17);
			cbUVMap.TabIndex = 12;
			cbUVMap.Text = "UV Mapped";
			// 
			// groupSize
			// 
			groupSize.Controls.Add(nmScale);
			groupSize.Controls.Add(lblScale);
			groupSize.Location = new System.Drawing.Point(6, 192);
			groupSize.Name = "groupSize";
			groupSize.Size = new System.Drawing.Size(128, 73);
			groupSize.TabIndex = 8;
			groupSize.TabStop = false;
			groupSize.Text = "Size";
			// 
			// groupAxis
			// 
			groupAxis.Controls.Add(cbInvertFaces);
			groupAxis.Controls.Add(cbSwapXY);
			groupAxis.Controls.Add(cbFlipZ);
			groupAxis.Controls.Add(cbSwapXZ);
			groupAxis.Controls.Add(cbFlipY);
			groupAxis.Controls.Add(cbSwapYZ);
			groupAxis.Controls.Add(cbFlipX);
			groupAxis.Location = new System.Drawing.Point(6, 6);
			groupAxis.Name = "groupAxis";
			groupAxis.Size = new System.Drawing.Size(128, 180);
			groupAxis.TabIndex = 8;
			groupAxis.TabStop = false;
			groupAxis.Text = "Axis";
			// 
			// cbInvertFaces
			// 
			cbInvertFaces.AutoSize = true;
			cbInvertFaces.Location = new System.Drawing.Point(6, 159);
			cbInvertFaces.Name = "cbInvertFaces";
			cbInvertFaces.Size = new System.Drawing.Size(84, 17);
			cbInvertFaces.TabIndex = 6;
			cbInvertFaces.Text = "Invert faces";
			// 
			// cbKeepTexturesExternally
			// 
			cbKeepTexturesExternally.AutoSize = true;
			cbKeepTexturesExternally.Location = new System.Drawing.Point(6, 114);
			cbKeepTexturesExternally.Name = "cbKeepTexturesExternally";
			cbKeepTexturesExternally.Size = new System.Drawing.Size(147, 17);
			cbKeepTexturesExternally.TabIndex = 11;
			cbKeepTexturesExternally.Text = "Keep textures externally";
			// 
			// GeometryIOSettingsDialog
			// 
			AcceptButton = butOK;
			AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			AutoSize = true;
			CancelButton = butCancel;
			ClientSize = new System.Drawing.Size(313, 341);
			Controls.Add(panelContents);
			Controls.Add(butCancel);
			Controls.Add(butOK);
			FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
			MaximizeBox = false;
			MinimizeBox = false;
			Name = "GeometryIOSettingsDialog";
			ShowIcon = false;
			ShowInTaskbar = false;
			StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			((System.ComponentModel.ISupportInitialize)nmScale).EndInit();
			panelContents.ResumeLayout(false);
			panelContents.PerformLayout();
			groupMisc.ResumeLayout(false);
			groupMisc.PerformLayout();
			groupTextures.ResumeLayout(false);
			groupSize.ResumeLayout(false);
			groupSize.PerformLayout();
			groupAxis.ResumeLayout(false);
			groupAxis.PerformLayout();
			ResumeLayout(false);
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
        private DarkUI.Controls.DarkCheckBox cbPackTextures;
        private DarkUI.Controls.DarkCheckBox cbPadPackedTextures;
        private DarkUI.Controls.DarkCheckBox cbUVMap;
		private DarkUI.Controls.DarkCheckBox cbKeepTexturesExternally;
	}
}