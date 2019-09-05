namespace TombIDE.ProjectMaster
{
	partial class SettingsSplashScreen
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
			this.label_Blank = new DarkUI.Controls.DarkLabel();
			this.panel_Preview = new System.Windows.Forms.Panel();
			this.sectionPanel = new DarkUI.Controls.DarkSectionPanel();
			this.button_Preview = new DarkUI.Controls.DarkButton();
			this.darkLabel1 = new DarkUI.Controls.DarkLabel();
			this.button_Remove = new DarkUI.Controls.DarkButton();
			this.panel_Preview.SuspendLayout();
			this.sectionPanel.SuspendLayout();
			this.SuspendLayout();
			// 
			// button_Change
			// 
			this.button_Change.Location = new System.Drawing.Point(336, 210);
			this.button_Change.Margin = new System.Windows.Forms.Padding(3, 3, 6, 6);
			this.button_Change.Name = "button_Change";
			this.button_Change.Size = new System.Drawing.Size(295, 26);
			this.button_Change.TabIndex = 0;
			this.button_Change.Text = "Change...";
			this.button_Change.Click += new System.EventHandler(this.button_Change_Click);
			// 
			// label_Blank
			// 
			this.label_Blank.Dock = System.Windows.Forms.DockStyle.Fill;
			this.label_Blank.Font = new System.Drawing.Font("Microsoft Sans Serif", 48F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
			this.label_Blank.ForeColor = System.Drawing.Color.Gray;
			this.label_Blank.Location = new System.Drawing.Point(0, 0);
			this.label_Blank.Name = "label_Blank";
			this.label_Blank.Size = new System.Drawing.Size(318, 238);
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
			this.panel_Preview.Size = new System.Drawing.Size(320, 240);
			this.panel_Preview.TabIndex = 3;
			// 
			// sectionPanel
			// 
			this.sectionPanel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.sectionPanel.Controls.Add(this.button_Preview);
			this.sectionPanel.Controls.Add(this.darkLabel1);
			this.sectionPanel.Controls.Add(this.button_Remove);
			this.sectionPanel.Controls.Add(this.button_Change);
			this.sectionPanel.Controls.Add(this.panel_Preview);
			this.sectionPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.sectionPanel.Location = new System.Drawing.Point(0, 0);
			this.sectionPanel.Name = "sectionPanel";
			this.sectionPanel.SectionHeader = "Splash Screen Image";
			this.sectionPanel.Size = new System.Drawing.Size(640, 280);
			this.sectionPanel.TabIndex = 0;
			// 
			// button_Preview
			// 
			this.button_Preview.Location = new System.Drawing.Point(336, 37);
			this.button_Preview.Margin = new System.Windows.Forms.Padding(3, 12, 6, 6);
			this.button_Preview.Name = "button_Preview";
			this.button_Preview.Size = new System.Drawing.Size(100, 26);
			this.button_Preview.TabIndex = 5;
			this.button_Preview.Text = "Live Preview...";
			this.button_Preview.Click += new System.EventHandler(this.button_Preview_Click);
			// 
			// darkLabel1
			// 
			this.darkLabel1.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
			this.darkLabel1.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
			this.darkLabel1.Location = new System.Drawing.Point(336, 75);
			this.darkLabel1.Margin = new System.Windows.Forms.Padding(3, 6, 6, 6);
			this.darkLabel1.Name = "darkLabel1";
			this.darkLabel1.Size = new System.Drawing.Size(295, 126);
			this.darkLabel1.TabIndex = 4;
			this.darkLabel1.Text = "Supported splash art sizes:\r\n\r\n- 512x256 px (Small)\r\n- 768x384 px (Medium)\r\n- 102" +
    "4x512 px (Large)";
			this.darkLabel1.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// button_Remove
			// 
			this.button_Remove.Location = new System.Drawing.Point(336, 245);
			this.button_Remove.Margin = new System.Windows.Forms.Padding(3, 3, 6, 6);
			this.button_Remove.Name = "button_Remove";
			this.button_Remove.Size = new System.Drawing.Size(295, 26);
			this.button_Remove.TabIndex = 2;
			this.button_Remove.Text = "Remove (use blank)";
			this.button_Remove.Click += new System.EventHandler(this.button_Remove_Click);
			// 
			// SettingsSplashScreen
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
			this.Controls.Add(this.sectionPanel);
			this.MaximumSize = new System.Drawing.Size(640, 280);
			this.MinimumSize = new System.Drawing.Size(640, 280);
			this.Name = "SettingsSplashScreen";
			this.Size = new System.Drawing.Size(640, 280);
			this.panel_Preview.ResumeLayout(false);
			this.sectionPanel.ResumeLayout(false);
			this.ResumeLayout(false);

		}

		#endregion

		private DarkUI.Controls.DarkButton button_Change;
		private DarkUI.Controls.DarkLabel label_Blank;
		private DarkUI.Controls.DarkSectionPanel sectionPanel;
		private System.Windows.Forms.Panel panel_Preview;
		private DarkUI.Controls.DarkButton button_Remove;
		private DarkUI.Controls.DarkLabel darkLabel1;
		private DarkUI.Controls.DarkButton button_Preview;
	}
}
