namespace TombEditor.Forms
{
    partial class FormPortal
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

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			butCancel = new DarkUI.Controls.DarkButton();
			butOk = new DarkUI.Controls.DarkButton();
			comboPortalEffect = new DarkUI.Controls.DarkComboBox();
			darkLabel2 = new DarkUI.Controls.DarkLabel();
			cbReflectSprites = new DarkUI.Controls.DarkCheckBox();
			cbReflectLights = new DarkUI.Controls.DarkCheckBox();
			cbReflectStatics = new DarkUI.Controls.DarkCheckBox();
			cbReflectMoveables = new DarkUI.Controls.DarkCheckBox();
			darkLabel1 = new DarkUI.Controls.DarkLabel();
			comboRefractionStrength = new DarkUI.Controls.DarkComboBox();
			this.comboWaterDirection = new DarkUI.Controls.DarkComboBox();
			darkLabel3 = new DarkUI.Controls.DarkLabel();
			darkLabel4 = new DarkUI.Controls.DarkLabel();
			darkTabbedContainer1 = new TombLib.Controls.DarkTabbedContainer();
			tabPage1 = new System.Windows.Forms.TabPage();
			tabPage2 = new System.Windows.Forms.TabPage();
			numWaterSpeed = new DarkUI.Controls.DarkNumericUpDown();
			darkTabbedContainer1.SuspendLayout();
			tabPage1.SuspendLayout();
			tabPage2.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)numWaterSpeed).BeginInit();
			SuspendLayout();
			// 
			// butCancel
			// 
			butCancel.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
			butCancel.Checked = false;
			butCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			butCancel.Location = new System.Drawing.Point(285, 166);
			butCancel.Name = "butCancel";
			butCancel.Size = new System.Drawing.Size(80, 23);
			butCancel.TabIndex = 2;
			butCancel.Text = "Cancel";
			butCancel.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
			butCancel.Click += butCancel_Click;
			// 
			// butOk
			// 
			butOk.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
			butOk.Checked = false;
			butOk.Location = new System.Drawing.Point(199, 166);
			butOk.Name = "butOk";
			butOk.Size = new System.Drawing.Size(80, 23);
			butOk.TabIndex = 1;
			butOk.Text = "OK";
			butOk.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
			butOk.Click += butOk_Click;
			// 
			// comboPortalEffect
			// 
			comboPortalEffect.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
			comboPortalEffect.FormattingEnabled = true;
			comboPortalEffect.Location = new System.Drawing.Point(87, 9);
			comboPortalEffect.Name = "comboPortalEffect";
			comboPortalEffect.Size = new System.Drawing.Size(278, 23);
			comboPortalEffect.TabIndex = 10;
			comboPortalEffect.SelectedIndexChanged += comboPortalEffect_SelectedIndexChanged;
			// 
			// darkLabel2
			// 
			darkLabel2.AutoSize = true;
			darkLabel2.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
			darkLabel2.Location = new System.Drawing.Point(7, 12);
			darkLabel2.Name = "darkLabel2";
			darkLabel2.Size = new System.Drawing.Size(72, 13);
			darkLabel2.TabIndex = 11;
			darkLabel2.Text = "Portal effect:";
			// 
			// cbReflectSprites
			// 
			cbReflectSprites.AutoSize = true;
			cbReflectSprites.Location = new System.Drawing.Point(6, 52);
			cbReflectSprites.Name = "cbReflectSprites";
			cbReflectSprites.Size = new System.Drawing.Size(98, 17);
			cbReflectSprites.TabIndex = 3;
			cbReflectSprites.Text = "Reflect sprites";
			// 
			// cbReflectLights
			// 
			cbReflectLights.AutoSize = true;
			cbReflectLights.Location = new System.Drawing.Point(6, 75);
			cbReflectLights.Name = "cbReflectLights";
			cbReflectLights.Size = new System.Drawing.Size(138, 17);
			cbReflectLights.TabIndex = 2;
			cbReflectLights.Text = "Reflect dynamic lights";
			// 
			// cbReflectStatics
			// 
			cbReflectStatics.AutoSize = true;
			cbReflectStatics.Location = new System.Drawing.Point(6, 29);
			cbReflectStatics.Name = "cbReflectStatics";
			cbReflectStatics.Size = new System.Drawing.Size(96, 17);
			cbReflectStatics.TabIndex = 1;
			cbReflectStatics.Text = "Reflect statics";
			// 
			// cbReflectMoveables
			// 
			cbReflectMoveables.AutoSize = true;
			cbReflectMoveables.Location = new System.Drawing.Point(6, 6);
			cbReflectMoveables.Name = "cbReflectMoveables";
			cbReflectMoveables.Size = new System.Drawing.Size(118, 17);
			cbReflectMoveables.TabIndex = 0;
			cbReflectMoveables.Text = "Reflect moveables";
			// 
			// darkLabel1
			// 
			darkLabel1.AutoSize = true;
			darkLabel1.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
			darkLabel1.Location = new System.Drawing.Point(6, 9);
			darkLabel1.Name = "darkLabel1";
			darkLabel1.Size = new System.Drawing.Size(110, 13);
			darkLabel1.TabIndex = 0;
			darkLabel1.Text = "Refraction strength:";
			// 
			// comboRefractionStrength
			// 
			comboRefractionStrength.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
			comboRefractionStrength.FormattingEnabled = true;
			comboRefractionStrength.Items.AddRange(new object[] { "Low", "Medium", "High" });
			comboRefractionStrength.Location = new System.Drawing.Point(144, 6);
			comboRefractionStrength.Name = "comboRefractionStrength";
			comboRefractionStrength.Size = new System.Drawing.Size(199, 23);
			comboRefractionStrength.TabIndex = 1;
			// 
			// comboWaterDirection
			// 
			this.comboWaterDirection.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
			this.comboWaterDirection.FormattingEnabled = true;
			this.comboWaterDirection.Items.AddRange(new object[] { "North (+Z)", "East (+X)", "South (-Z)", "West (-X)" });
			this.comboWaterDirection.Location = new System.Drawing.Point(144, 35);
			this.comboWaterDirection.Name = "comboWaterDirection";
			this.comboWaterDirection.Size = new System.Drawing.Size(199, 23);
			this.comboWaterDirection.TabIndex = 3;
			// 
			// darkLabel3
			// 
			darkLabel3.AutoSize = true;
			darkLabel3.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
			darkLabel3.Location = new System.Drawing.Point(6, 38);
			darkLabel3.Name = "darkLabel3";
			darkLabel3.Size = new System.Drawing.Size(90, 13);
			darkLabel3.TabIndex = 2;
			darkLabel3.Text = "Water direction:";
			// 
			// darkLabel4
			// 
			darkLabel4.AutoSize = true;
			darkLabel4.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
			darkLabel4.Location = new System.Drawing.Point(8, 66);
			darkLabel4.Name = "darkLabel4";
			darkLabel4.Size = new System.Drawing.Size(130, 13);
			darkLabel4.TabIndex = 5;
			darkLabel4.Text = "Water speed (units/sec):";
			// 
			// darkTabbedContainer1
			// 
			darkTabbedContainer1.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
			darkTabbedContainer1.Controls.Add(tabPage1);
			darkTabbedContainer1.Controls.Add(tabPage2);
			darkTabbedContainer1.Location = new System.Drawing.Point(7, 38);
			darkTabbedContainer1.Name = "darkTabbedContainer1";
			darkTabbedContainer1.SelectedIndex = 0;
			darkTabbedContainer1.Size = new System.Drawing.Size(358, 122);
			darkTabbedContainer1.TabIndex = 14;
			// 
			// tabPage1
			// 
			tabPage1.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
			tabPage1.Controls.Add(cbReflectSprites);
			tabPage1.Controls.Add(cbReflectMoveables);
			tabPage1.Controls.Add(cbReflectStatics);
			tabPage1.Controls.Add(cbReflectLights);
			tabPage1.Location = new System.Drawing.Point(4, 22);
			tabPage1.Name = "tabPage1";
			tabPage1.Padding = new System.Windows.Forms.Padding(3);
			tabPage1.Size = new System.Drawing.Size(350, 96);
			tabPage1.TabIndex = 0;
			tabPage1.Text = "tabPage1";
			// 
			// tabPage2
			// 
			tabPage2.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
			tabPage2.Controls.Add(numWaterSpeed);
			tabPage2.Controls.Add(darkLabel4);
			tabPage2.Controls.Add(comboRefractionStrength);
			tabPage2.Controls.Add(darkLabel1);
			tabPage2.Controls.Add(this.comboWaterDirection);
			tabPage2.Controls.Add(darkLabel3);
			tabPage2.Location = new System.Drawing.Point(4, 22);
			tabPage2.Name = "tabPage2";
			tabPage2.Padding = new System.Windows.Forms.Padding(3);
			tabPage2.Size = new System.Drawing.Size(350, 96);
			tabPage2.TabIndex = 1;
			tabPage2.Text = "tabPage2";
			// 
			// numWaterSpeed
			// 
			numWaterSpeed.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
			numWaterSpeed.IncrementAlternate = new decimal(new int[] { 10, 0, 0, 65536 });
			numWaterSpeed.Location = new System.Drawing.Point(144, 64);
			numWaterSpeed.LoopValues = false;
			numWaterSpeed.Name = "numWaterSpeed";
			numWaterSpeed.Size = new System.Drawing.Size(199, 22);
			numWaterSpeed.TabIndex = 6;
			// 
			// FormPortal
			// 
			AcceptButton = butOk;
			AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			CancelButton = butCancel;
			ClientSize = new System.Drawing.Size(373, 197);
			Controls.Add(darkTabbedContainer1);
			Controls.Add(darkLabel2);
			Controls.Add(comboPortalEffect);
			Controls.Add(butCancel);
			Controls.Add(butOk);
			FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			MaximizeBox = false;
			MinimizeBox = false;
			Name = "FormPortal";
			ShowIcon = false;
			ShowInTaskbar = false;
			StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			Text = "Edit portal";
			darkTabbedContainer1.ResumeLayout(false);
			tabPage1.ResumeLayout(false);
			tabPage1.PerformLayout();
			tabPage2.ResumeLayout(false);
			tabPage2.PerformLayout();
			((System.ComponentModel.ISupportInitialize)numWaterSpeed).EndInit();
			ResumeLayout(false);
			PerformLayout();
		}

		#endregion
		private DarkUI.Controls.DarkButton butCancel;
        private DarkUI.Controls.DarkButton butOk;
        private DarkUI.Controls.DarkComboBox comboPortalEffect;
        private DarkUI.Controls.DarkLabel darkLabel2;
        private DarkUI.Controls.DarkGroupBox darkGroupBox1;
        private DarkUI.Controls.DarkCheckBox cbReflectStatics;
        private DarkUI.Controls.DarkCheckBox cbReflectMoveables;
        private DarkUI.Controls.DarkCheckBox cbReflectLights;
        private DarkUI.Controls.DarkCheckBox cbReflectSprites;
		private DarkUI.Controls.DarkGroupBox darkGroupBox2;
		private DarkUI.Controls.DarkComboBox comboWaterDirection;
		private DarkUI.Controls.DarkLabel darkLabel3;
		private DarkUI.Controls.DarkComboBox comboRefractionStrength;
		private DarkUI.Controls.DarkLabel darkLabel1;
		private DarkUI.Controls.DarkLabel darkLabel4;
		private TombLib.Controls.DarkTabbedContainer darkTabbedContainer1;
		private System.Windows.Forms.TabPage tabPage1;
		private System.Windows.Forms.TabPage tabPage2;
		private DarkUI.Controls.DarkNumericUpDown numWaterSpeed;
	}
}