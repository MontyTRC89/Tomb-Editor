﻿namespace TombIDE.ProjectMaster
{
	partial class SettingsStartupImage
	{
		private System.ComponentModel.IContainer components = null;

		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Component Designer generated code

		private void InitializeComponent()
		{
			this.button_Change = new DarkUI.Controls.DarkButton();
			this.button_Reset = new DarkUI.Controls.DarkButton();
			this.button_UseBlank = new DarkUI.Controls.DarkButton();
			this.label_01 = new DarkUI.Controls.DarkLabel();
			this.label_Blank = new DarkUI.Controls.DarkLabel();
			this.panel_Preview = new System.Windows.Forms.Panel();
			this.radioButton_Standard = new DarkUI.Controls.DarkRadioButton();
			this.radioButton_Wide = new DarkUI.Controls.DarkRadioButton();
			this.sectionPanel = new DarkUI.Controls.DarkSectionPanel();
			this.panel_Preview.SuspendLayout();
			this.sectionPanel.SuspendLayout();
			this.SuspendLayout();
			// 
			// button_Change
			// 
			this.button_Change.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.button_Change.Checked = false;
			this.button_Change.Location = new System.Drawing.Point(442, 175);
			this.button_Change.Margin = new System.Windows.Forms.Padding(3, 3, 6, 6);
			this.button_Change.Name = "button_Change";
			this.button_Change.Size = new System.Drawing.Size(189, 26);
			this.button_Change.TabIndex = 3;
			this.button_Change.Text = "Change...";
			this.button_Change.Click += new System.EventHandler(this.button_Change_Click);
			// 
			// button_Reset
			// 
			this.button_Reset.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.button_Reset.Checked = false;
			this.button_Reset.Location = new System.Drawing.Point(442, 245);
			this.button_Reset.Margin = new System.Windows.Forms.Padding(3, 3, 6, 6);
			this.button_Reset.Name = "button_Reset";
			this.button_Reset.Size = new System.Drawing.Size(189, 26);
			this.button_Reset.TabIndex = 5;
			this.button_Reset.Text = "Reset to default";
			this.button_Reset.Click += new System.EventHandler(this.button_Reset_Click);
			// 
			// button_UseBlank
			// 
			this.button_UseBlank.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.button_UseBlank.Checked = false;
			this.button_UseBlank.Location = new System.Drawing.Point(442, 210);
			this.button_UseBlank.Margin = new System.Windows.Forms.Padding(3, 3, 6, 6);
			this.button_UseBlank.Name = "button_UseBlank";
			this.button_UseBlank.Size = new System.Drawing.Size(189, 26);
			this.button_UseBlank.TabIndex = 4;
			this.button_UseBlank.Text = "Use blank (empty black screen)";
			this.button_UseBlank.Click += new System.EventHandler(this.button_UseBlank_Click);
			// 
			// label_01
			// 
			this.label_01.AutoSize = true;
			this.label_01.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
			this.label_01.Location = new System.Drawing.Point(442, 37);
			this.label_01.Margin = new System.Windows.Forms.Padding(3, 12, 3, 3);
			this.label_01.Name = "label_01";
			this.label_01.Size = new System.Drawing.Size(106, 13);
			this.label_01.TabIndex = 0;
			this.label_01.Text = "Preview aspect ratio:";
			// 
			// label_Blank
			// 
			this.label_Blank.Dock = System.Windows.Forms.DockStyle.Fill;
			this.label_Blank.Font = new System.Drawing.Font("Microsoft Sans Serif", 48F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
			this.label_Blank.ForeColor = System.Drawing.Color.Gray;
			this.label_Blank.Location = new System.Drawing.Point(0, 0);
			this.label_Blank.Name = "label_Blank";
			this.label_Blank.Size = new System.Drawing.Size(424, 238);
			this.label_Blank.TabIndex = 0;
			this.label_Blank.Text = "BLANK";
			this.label_Blank.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			this.label_Blank.Visible = false;
			// 
			// panel_Preview
			// 
			this.panel_Preview.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(48)))), ((int)(((byte)(48)))), ((int)(((byte)(48)))));
			this.panel_Preview.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
			this.panel_Preview.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.panel_Preview.Controls.Add(this.label_Blank);
			this.panel_Preview.Location = new System.Drawing.Point(7, 31);
			this.panel_Preview.Margin = new System.Windows.Forms.Padding(6);
			this.panel_Preview.Name = "panel_Preview";
			this.panel_Preview.Size = new System.Drawing.Size(426, 240);
			this.panel_Preview.TabIndex = 6;
			// 
			// radioButton_Standard
			// 
			this.radioButton_Standard.Location = new System.Drawing.Point(445, 82);
			this.radioButton_Standard.Margin = new System.Windows.Forms.Padding(6, 3, 3, 3);
			this.radioButton_Standard.Name = "radioButton_Standard";
			this.radioButton_Standard.Size = new System.Drawing.Size(50, 20);
			this.radioButton_Standard.TabIndex = 2;
			this.radioButton_Standard.Text = "4:3";
			this.radioButton_Standard.CheckedChanged += new System.EventHandler(this.radioButton_Standard_CheckedChanged);
			// 
			// radioButton_Wide
			// 
			this.radioButton_Wide.Checked = true;
			this.radioButton_Wide.Location = new System.Drawing.Point(445, 56);
			this.radioButton_Wide.Margin = new System.Windows.Forms.Padding(6, 3, 3, 3);
			this.radioButton_Wide.Name = "radioButton_Wide";
			this.radioButton_Wide.Size = new System.Drawing.Size(50, 20);
			this.radioButton_Wide.TabIndex = 1;
			this.radioButton_Wide.TabStop = true;
			this.radioButton_Wide.Text = "16:9";
			this.radioButton_Wide.CheckedChanged += new System.EventHandler(this.radioButton_Wide_CheckedChanged);
			// 
			// sectionPanel
			// 
			this.sectionPanel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.sectionPanel.Controls.Add(this.label_01);
			this.sectionPanel.Controls.Add(this.button_Change);
			this.sectionPanel.Controls.Add(this.radioButton_Standard);
			this.sectionPanel.Controls.Add(this.panel_Preview);
			this.sectionPanel.Controls.Add(this.button_Reset);
			this.sectionPanel.Controls.Add(this.button_UseBlank);
			this.sectionPanel.Controls.Add(this.radioButton_Wide);
			this.sectionPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.sectionPanel.Location = new System.Drawing.Point(0, 0);
			this.sectionPanel.Name = "sectionPanel";
			this.sectionPanel.SectionHeader = "Startup Loading Screen Image";
			this.sectionPanel.Size = new System.Drawing.Size(640, 280);
			this.sectionPanel.TabIndex = 0;
			// 
			// SettingsStartupImage
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
			this.Controls.Add(this.sectionPanel);
			this.MaximumSize = new System.Drawing.Size(640, 280);
			this.MinimumSize = new System.Drawing.Size(640, 280);
			this.Name = "SettingsStartupImage";
			this.Size = new System.Drawing.Size(640, 280);
			this.panel_Preview.ResumeLayout(false);
			this.sectionPanel.ResumeLayout(false);
			this.sectionPanel.PerformLayout();
			this.ResumeLayout(false);

		}

		#endregion

		private DarkUI.Controls.DarkButton button_Change;
		private DarkUI.Controls.DarkButton button_Reset;
		private DarkUI.Controls.DarkButton button_UseBlank;
		private DarkUI.Controls.DarkLabel label_01;
		private DarkUI.Controls.DarkLabel label_Blank;
		private DarkUI.Controls.DarkRadioButton radioButton_Standard;
		private DarkUI.Controls.DarkRadioButton radioButton_Wide;
		private DarkUI.Controls.DarkSectionPanel sectionPanel;
		private System.Windows.Forms.Panel panel_Preview;
	}
}
