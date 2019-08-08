namespace TombIDE.ProjectMaster
{
	partial class SettingsGameIcon
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
			this.label_01 = new DarkUI.Controls.DarkLabel();
			this.label_02 = new DarkUI.Controls.DarkLabel();
			this.panel_128 = new System.Windows.Forms.Panel();
			this.panel_16 = new System.Windows.Forms.Panel();
			this.panel_256 = new System.Windows.Forms.Panel();
			this.panel_48 = new System.Windows.Forms.Panel();
			this.panel_PreviewBackground = new System.Windows.Forms.Panel();
			this.radioButton_Dark = new DarkUI.Controls.DarkRadioButton();
			this.radioButton_Light = new DarkUI.Controls.DarkRadioButton();
			this.sectionPanel = new DarkUI.Controls.DarkSectionPanel();
			this.panel_PreviewBackground.SuspendLayout();
			this.sectionPanel.SuspendLayout();
			this.SuspendLayout();
			// 
			// button_Change
			// 
			this.button_Change.Location = new System.Drawing.Point(423, 240);
			this.button_Change.Margin = new System.Windows.Forms.Padding(3, 3, 6, 6);
			this.button_Change.Name = "button_Change";
			this.button_Change.Size = new System.Drawing.Size(208, 26);
			this.button_Change.TabIndex = 3;
			this.button_Change.Text = "Change...";
			this.button_Change.Click += new System.EventHandler(this.button_Change_Click);
			// 
			// button_Reset
			// 
			this.button_Reset.Location = new System.Drawing.Point(423, 275);
			this.button_Reset.Margin = new System.Windows.Forms.Padding(3, 3, 6, 6);
			this.button_Reset.Name = "button_Reset";
			this.button_Reset.Size = new System.Drawing.Size(208, 26);
			this.button_Reset.TabIndex = 4;
			this.button_Reset.Text = "Reset to default";
			this.button_Reset.Click += new System.EventHandler(this.button_Reset_Click);
			// 
			// label_01
			// 
			this.label_01.AutoSize = true;
			this.label_01.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
			this.label_01.Location = new System.Drawing.Point(423, 37);
			this.label_01.Margin = new System.Windows.Forms.Padding(3, 12, 3, 3);
			this.label_01.Name = "label_01";
			this.label_01.Size = new System.Drawing.Size(134, 13);
			this.label_01.TabIndex = 0;
			this.label_01.Text = "Preview background color:";
			// 
			// label_02
			// 
			this.label_02.AutoSize = true;
			this.label_02.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
			this.label_02.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
			this.label_02.Location = new System.Drawing.Point(423, 115);
			this.label_02.Margin = new System.Windows.Forms.Padding(3, 10, 3, 10);
			this.label_02.Name = "label_02";
			this.label_02.Size = new System.Drawing.Size(169, 112);
			this.label_02.TabIndex = 6;
			this.label_02.Text = "Your .ico file should contain\r\nthese sizes:\r\n\r\n- 256x256 px\r\n- 48x48 px\r\n- 32x32 " +
    "px\r\n- 16x16 px";
			// 
			// panel_128
			// 
			this.panel_128.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
			this.panel_128.Location = new System.Drawing.Point(271, 6);
			this.panel_128.Margin = new System.Windows.Forms.Padding(3, 6, 6, 3);
			this.panel_128.Name = "panel_128";
			this.panel_128.Size = new System.Drawing.Size(128, 128);
			this.panel_128.TabIndex = 1;
			// 
			// panel_16
			// 
			this.panel_16.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
			this.panel_16.Location = new System.Drawing.Point(325, 140);
			this.panel_16.Name = "panel_16";
			this.panel_16.Size = new System.Drawing.Size(16, 16);
			this.panel_16.TabIndex = 3;
			// 
			// panel_256
			// 
			this.panel_256.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
			this.panel_256.Location = new System.Drawing.Point(6, 6);
			this.panel_256.Margin = new System.Windows.Forms.Padding(6);
			this.panel_256.Name = "panel_256";
			this.panel_256.Size = new System.Drawing.Size(256, 256);
			this.panel_256.TabIndex = 0;
			// 
			// panel_48
			// 
			this.panel_48.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
			this.panel_48.Location = new System.Drawing.Point(271, 140);
			this.panel_48.Name = "panel_48";
			this.panel_48.Size = new System.Drawing.Size(48, 48);
			this.panel_48.TabIndex = 2;
			// 
			// panel_PreviewBackground
			// 
			this.panel_PreviewBackground.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(48)))), ((int)(((byte)(48)))), ((int)(((byte)(48)))));
			this.panel_PreviewBackground.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.panel_PreviewBackground.Controls.Add(this.panel_256);
			this.panel_PreviewBackground.Controls.Add(this.panel_48);
			this.panel_PreviewBackground.Controls.Add(this.panel_16);
			this.panel_PreviewBackground.Controls.Add(this.panel_128);
			this.panel_PreviewBackground.Location = new System.Drawing.Point(7, 31);
			this.panel_PreviewBackground.Margin = new System.Windows.Forms.Padding(6);
			this.panel_PreviewBackground.Name = "panel_PreviewBackground";
			this.panel_PreviewBackground.Size = new System.Drawing.Size(407, 270);
			this.panel_PreviewBackground.TabIndex = 5;
			// 
			// radioButton_Dark
			// 
			this.radioButton_Dark.Checked = true;
			this.radioButton_Dark.Location = new System.Drawing.Point(426, 56);
			this.radioButton_Dark.Margin = new System.Windows.Forms.Padding(6, 3, 3, 3);
			this.radioButton_Dark.Name = "radioButton_Dark";
			this.radioButton_Dark.Size = new System.Drawing.Size(50, 20);
			this.radioButton_Dark.TabIndex = 1;
			this.radioButton_Dark.TabStop = true;
			this.radioButton_Dark.Text = "Dark";
			this.radioButton_Dark.CheckedChanged += new System.EventHandler(this.radioButton_Dark_CheckedChanged);
			// 
			// radioButton_Light
			// 
			this.radioButton_Light.Location = new System.Drawing.Point(426, 82);
			this.radioButton_Light.Margin = new System.Windows.Forms.Padding(6, 3, 3, 3);
			this.radioButton_Light.Name = "radioButton_Light";
			this.radioButton_Light.Size = new System.Drawing.Size(50, 20);
			this.radioButton_Light.TabIndex = 2;
			this.radioButton_Light.Text = "Light";
			this.radioButton_Light.CheckedChanged += new System.EventHandler(this.radioButton_Light_CheckedChanged);
			// 
			// sectionPanel
			// 
			this.sectionPanel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.sectionPanel.Controls.Add(this.label_02);
			this.sectionPanel.Controls.Add(this.label_01);
			this.sectionPanel.Controls.Add(this.radioButton_Light);
			this.sectionPanel.Controls.Add(this.radioButton_Dark);
			this.sectionPanel.Controls.Add(this.panel_PreviewBackground);
			this.sectionPanel.Controls.Add(this.button_Reset);
			this.sectionPanel.Controls.Add(this.button_Change);
			this.sectionPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.sectionPanel.Location = new System.Drawing.Point(0, 0);
			this.sectionPanel.Name = "sectionPanel";
			this.sectionPanel.SectionHeader = "Game Icon";
			this.sectionPanel.Size = new System.Drawing.Size(640, 310);
			this.sectionPanel.TabIndex = 2;
			// 
			// SettingsGameIcon
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
			this.Controls.Add(this.sectionPanel);
			this.MaximumSize = new System.Drawing.Size(640, 310);
			this.MinimumSize = new System.Drawing.Size(640, 310);
			this.Name = "SettingsGameIcon";
			this.Size = new System.Drawing.Size(640, 310);
			this.panel_PreviewBackground.ResumeLayout(false);
			this.sectionPanel.ResumeLayout(false);
			this.sectionPanel.PerformLayout();
			this.ResumeLayout(false);

		}

		#endregion

		private DarkUI.Controls.DarkButton button_Change;
		private DarkUI.Controls.DarkButton button_Reset;
		private DarkUI.Controls.DarkLabel label_01;
		private DarkUI.Controls.DarkLabel label_02;
		private DarkUI.Controls.DarkRadioButton radioButton_Dark;
		private DarkUI.Controls.DarkRadioButton radioButton_Light;
		private DarkUI.Controls.DarkSectionPanel sectionPanel;
		private System.Windows.Forms.Panel panel_128;
		private System.Windows.Forms.Panel panel_16;
		private System.Windows.Forms.Panel panel_256;
		private System.Windows.Forms.Panel panel_48;
		private System.Windows.Forms.Panel panel_PreviewBackground;
	}
}
