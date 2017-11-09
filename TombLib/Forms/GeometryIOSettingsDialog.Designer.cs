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
            this.cbFlipZ = new DarkUI.Controls.DarkCheckBox();
            this.cbFlipY = new DarkUI.Controls.DarkCheckBox();
            this.cbFlipX = new DarkUI.Controls.DarkCheckBox();
            this.cbSwapYZ = new DarkUI.Controls.DarkCheckBox();
            this.cbSwapXZ = new DarkUI.Controls.DarkCheckBox();
            this.cbSwapXY = new DarkUI.Controls.DarkCheckBox();
            this.nmScale = new DarkUI.Controls.DarkNumericUpDown();
            this.darkLabel1 = new DarkUI.Controls.DarkLabel();
            this.cbPremultiplyUV = new DarkUI.Controls.DarkCheckBox();
            this.cbWrapUV = new DarkUI.Controls.DarkCheckBox();
            this.cbFlipUV_V = new DarkUI.Controls.DarkCheckBox();
            this.lblPreset = new DarkUI.Controls.DarkLabel();
            this.cmbPresetList = new DarkUI.Controls.DarkComboBox();
            this.panelContents = new DarkUI.Controls.DarkSectionPanel();
            this.darkGroupBox3 = new DarkUI.Controls.DarkGroupBox();
            this.darkGroupBox2 = new DarkUI.Controls.DarkGroupBox();
            this.darkGroupBox1 = new DarkUI.Controls.DarkGroupBox();
            this.cbInvertFaces = new DarkUI.Controls.DarkCheckBox();
            this.cbDivide = new DarkUI.Controls.DarkCheckBox();
            ((System.ComponentModel.ISupportInitialize)(this.nmScale)).BeginInit();
            this.panelContents.SuspendLayout();
            this.darkGroupBox3.SuspendLayout();
            this.darkGroupBox2.SuspendLayout();
            this.darkGroupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // butOK
            // 
            this.butOK.Location = new System.Drawing.Point(117, 235);
            this.butOK.Name = "butOK";
            this.butOK.Size = new System.Drawing.Size(84, 23);
            this.butOK.TabIndex = 0;
            this.butOK.Text = "Ok";
            this.butOK.Click += new System.EventHandler(this.butOK_Click);
            // 
            // butCancel
            // 
            this.butCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.butCancel.Location = new System.Drawing.Point(207, 235);
            this.butCancel.Name = "butCancel";
            this.butCancel.Size = new System.Drawing.Size(84, 23);
            this.butCancel.TabIndex = 1;
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
            this.nmScale.Location = new System.Drawing.Point(47, 21);
            this.nmScale.Maximum = new decimal(new int[] {
            2048,
            0,
            0,
            0});
            this.nmScale.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.nmScale.MousewheelSingleIncrement = true;
            this.nmScale.Name = "nmScale";
            this.nmScale.Size = new System.Drawing.Size(97, 22);
            this.nmScale.TabIndex = 1;
            this.nmScale.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            // 
            // darkLabel1
            // 
            this.darkLabel1.AutoSize = true;
            this.darkLabel1.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.darkLabel1.Location = new System.Drawing.Point(5, 23);
            this.darkLabel1.Name = "darkLabel1";
            this.darkLabel1.Size = new System.Drawing.Size(36, 13);
            this.darkLabel1.TabIndex = 0;
            this.darkLabel1.Text = "Scale:";
            // 
            // cbPremultiplyUV
            // 
            this.cbPremultiplyUV.Location = new System.Drawing.Point(6, 67);
            this.cbPremultiplyUV.Name = "cbPremultiplyUV";
            this.cbPremultiplyUV.Size = new System.Drawing.Size(139, 17);
            this.cbPremultiplyUV.TabIndex = 3;
            this.cbPremultiplyUV.Text = "Premultiply UV";
            // 
            // cbWrapUV
            // 
            this.cbWrapUV.Location = new System.Drawing.Point(6, 44);
            this.cbWrapUV.Name = "cbWrapUV";
            this.cbWrapUV.Size = new System.Drawing.Size(139, 17);
            this.cbWrapUV.TabIndex = 2;
            this.cbWrapUV.Text = "Wrap UV";
            // 
            // cbFlipUV_V
            // 
            this.cbFlipUV_V.Location = new System.Drawing.Point(6, 21);
            this.cbFlipUV_V.Name = "cbFlipUV_V";
            this.cbFlipUV_V.Size = new System.Drawing.Size(139, 17);
            this.cbFlipUV_V.TabIndex = 1;
            this.cbFlipUV_V.Text = "Invert V coordinate";
            // 
            // lblPreset
            // 
            this.lblPreset.AutoSize = true;
            this.lblPreset.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.lblPreset.Location = new System.Drawing.Point(3, 196);
            this.lblPreset.Name = "lblPreset";
            this.lblPreset.Size = new System.Drawing.Size(41, 13);
            this.lblPreset.TabIndex = 5;
            this.lblPreset.Text = "Preset:";
            // 
            // cmbPresetList
            // 
            this.cmbPresetList.FormattingEnabled = true;
            this.cmbPresetList.Location = new System.Drawing.Point(50, 192);
            this.cmbPresetList.Name = "cmbPresetList";
            this.cmbPresetList.Size = new System.Drawing.Size(227, 23);
            this.cmbPresetList.TabIndex = 6;
            this.cmbPresetList.Text = null;
            this.cmbPresetList.SelectedIndexChanged += new System.EventHandler(this.cmbPresetList_SelectedIndexChanged);
            // 
            // panelContents
            // 
            this.panelContents.Controls.Add(this.darkGroupBox3);
            this.panelContents.Controls.Add(this.darkGroupBox2);
            this.panelContents.Controls.Add(this.darkGroupBox1);
            this.panelContents.Controls.Add(this.cmbPresetList);
            this.panelContents.Controls.Add(this.lblPreset);
            this.panelContents.Location = new System.Drawing.Point(8, 8);
            this.panelContents.Name = "panelContents";
            this.panelContents.SectionHeader = null;
            this.panelContents.Size = new System.Drawing.Size(283, 221);
            this.panelContents.TabIndex = 7;
            // 
            // darkGroupBox3
            // 
            this.darkGroupBox3.Controls.Add(this.cbPremultiplyUV);
            this.darkGroupBox3.Controls.Add(this.cbFlipUV_V);
            this.darkGroupBox3.Controls.Add(this.cbWrapUV);
            this.darkGroupBox3.Location = new System.Drawing.Point(126, 83);
            this.darkGroupBox3.Name = "darkGroupBox3";
            this.darkGroupBox3.Size = new System.Drawing.Size(151, 88);
            this.darkGroupBox3.TabIndex = 8;
            this.darkGroupBox3.TabStop = false;
            this.darkGroupBox3.Text = "Texture mapping";
            // 
            // darkGroupBox2
            // 
            this.darkGroupBox2.Controls.Add(this.cbDivide);
            this.darkGroupBox2.Controls.Add(this.nmScale);
            this.darkGroupBox2.Controls.Add(this.darkLabel1);
            this.darkGroupBox2.Location = new System.Drawing.Point(126, 6);
            this.darkGroupBox2.Name = "darkGroupBox2";
            this.darkGroupBox2.Size = new System.Drawing.Size(151, 71);
            this.darkGroupBox2.TabIndex = 8;
            this.darkGroupBox2.TabStop = false;
            this.darkGroupBox2.Text = "Size";
            // 
            // darkGroupBox1
            // 
            this.darkGroupBox1.Controls.Add(this.cbInvertFaces);
            this.darkGroupBox1.Controls.Add(this.cbSwapXY);
            this.darkGroupBox1.Controls.Add(this.cbFlipZ);
            this.darkGroupBox1.Controls.Add(this.cbSwapXZ);
            this.darkGroupBox1.Controls.Add(this.cbFlipY);
            this.darkGroupBox1.Controls.Add(this.cbSwapYZ);
            this.darkGroupBox1.Controls.Add(this.cbFlipX);
            this.darkGroupBox1.Location = new System.Drawing.Point(6, 6);
            this.darkGroupBox1.Name = "darkGroupBox1";
            this.darkGroupBox1.Size = new System.Drawing.Size(114, 180);
            this.darkGroupBox1.TabIndex = 8;
            this.darkGroupBox1.TabStop = false;
            this.darkGroupBox1.Text = "Axis";
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
            // cbDivide
            // 
            this.cbDivide.AutoSize = true;
            this.cbDivide.Location = new System.Drawing.Point(8, 49);
            this.cbDivide.Name = "cbDivide";
            this.cbDivide.Size = new System.Drawing.Size(134, 17);
            this.cbDivide.TabIndex = 2;
            this.cbDivide.Text = "Divide by scale factor";
            // 
            // GeometryIOSettingsDialog
            // 
            this.AcceptButton = this.butOK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSize = true;
            this.CancelButton = this.butCancel;
            this.ClientSize = new System.Drawing.Size(298, 265);
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
            ((System.ComponentModel.ISupportInitialize)(this.nmScale)).EndInit();
            this.panelContents.ResumeLayout(false);
            this.panelContents.PerformLayout();
            this.darkGroupBox3.ResumeLayout(false);
            this.darkGroupBox2.ResumeLayout(false);
            this.darkGroupBox2.PerformLayout();
            this.darkGroupBox1.ResumeLayout(false);
            this.darkGroupBox1.PerformLayout();
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
        private DarkUI.Controls.DarkLabel darkLabel1;
        private DarkUI.Controls.DarkCheckBox cbFlipUV_V;
        private DarkUI.Controls.DarkCheckBox cbWrapUV;
        private DarkUI.Controls.DarkCheckBox cbPremultiplyUV;
        private DarkUI.Controls.DarkLabel lblPreset;
        private DarkUI.Controls.DarkComboBox cmbPresetList;
        private DarkUI.Controls.DarkSectionPanel panelContents;
        private DarkUI.Controls.DarkGroupBox darkGroupBox3;
        private DarkUI.Controls.DarkGroupBox darkGroupBox2;
        private DarkUI.Controls.DarkGroupBox darkGroupBox1;
        private DarkUI.Controls.DarkCheckBox cbInvertFaces;
        private DarkUI.Controls.DarkCheckBox cbDivide;
    }
}