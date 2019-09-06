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
			this.button_Preview = new DarkUI.Controls.DarkButton();
			this.button_Remove = new DarkUI.Controls.DarkButton();
			this.label_01 = new DarkUI.Controls.DarkLabel();
			this.label_Blank = new DarkUI.Controls.DarkLabel();
			this.label_NotSupported = new DarkUI.Controls.DarkLabel();
			this.panel_Preview = new System.Windows.Forms.Panel();
			this.sectionPanel = new DarkUI.Controls.DarkSectionPanel();
			this.panel_Preview.SuspendLayout();
			this.sectionPanel.SuspendLayout();
			this.SuspendLayout();
			// 
			// button_Change
			// 
			this.button_Change.Location = new System.Drawing.Point(476, 200);
			this.button_Change.Margin = new System.Windows.Forms.Padding(3, 3, 6, 6);
			this.button_Change.Name = "button_Change";
			this.button_Change.Size = new System.Drawing.Size(155, 26);
			this.button_Change.TabIndex = 2;
			this.button_Change.Text = "Change...";
			this.button_Change.Click += new System.EventHandler(this.button_Change_Click);
			// 
			// button_Preview
			// 
			this.button_Preview.Location = new System.Drawing.Point(476, 37);
			this.button_Preview.Margin = new System.Windows.Forms.Padding(3, 12, 6, 6);
			this.button_Preview.Name = "button_Preview";
			this.button_Preview.Size = new System.Drawing.Size(155, 26);
			this.button_Preview.TabIndex = 0;
			this.button_Preview.Text = "Live Preview";
			this.button_Preview.Click += new System.EventHandler(this.button_Preview_Click);
			// 
			// button_Remove
			// 
			this.button_Remove.Location = new System.Drawing.Point(476, 235);
			this.button_Remove.Margin = new System.Windows.Forms.Padding(3, 3, 6, 6);
			this.button_Remove.Name = "button_Remove";
			this.button_Remove.Size = new System.Drawing.Size(155, 26);
			this.button_Remove.TabIndex = 3;
			this.button_Remove.Text = "Remove (use blank)";
			this.button_Remove.Click += new System.EventHandler(this.button_Remove_Click);
			// 
			// label_01
			// 
			this.label_01.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
			this.label_01.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
			this.label_01.Location = new System.Drawing.Point(476, 72);
			this.label_01.Margin = new System.Windows.Forms.Padding(3, 3, 6, 6);
			this.label_01.Name = "label_01";
			this.label_01.Size = new System.Drawing.Size(155, 119);
			this.label_01.TabIndex = 1;
			this.label_01.Text = "Supported image sizes:\r\n\r\n- 512x256 px (Small)\r\n- 768x384 px (Medium)\r\n- 1024x512" +
    " px (Large)";
			this.label_01.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// label_Blank
			// 
			this.label_Blank.Dock = System.Windows.Forms.DockStyle.Fill;
			this.label_Blank.Font = new System.Drawing.Font("Microsoft Sans Serif", 48F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
			this.label_Blank.ForeColor = System.Drawing.Color.Gray;
			this.label_Blank.Location = new System.Drawing.Point(0, 0);
			this.label_Blank.Name = "label_Blank";
			this.label_Blank.Size = new System.Drawing.Size(458, 228);
			this.label_Blank.TabIndex = 0;
			this.label_Blank.Text = "BLANK";
			this.label_Blank.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			this.label_Blank.Visible = false;
			// 
			// label_NotSupported
			// 
			this.label_NotSupported.Dock = System.Windows.Forms.DockStyle.Fill;
			this.label_NotSupported.ForeColor = System.Drawing.Color.Gray;
			this.label_NotSupported.Location = new System.Drawing.Point(0, 0);
			this.label_NotSupported.Name = "label_NotSupported";
			this.label_NotSupported.Size = new System.Drawing.Size(458, 228);
			this.label_NotSupported.TabIndex = 1;
			this.label_NotSupported.Text = "Legacy projects don\'t support splash screens.";
			this.label_NotSupported.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			this.label_NotSupported.Visible = false;
			// 
			// panel_Preview
			// 
			this.panel_Preview.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(48)))), ((int)(((byte)(48)))), ((int)(((byte)(48)))));
			this.panel_Preview.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
			this.panel_Preview.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.panel_Preview.Controls.Add(this.label_NotSupported);
			this.panel_Preview.Controls.Add(this.label_Blank);
			this.panel_Preview.Location = new System.Drawing.Point(7, 31);
			this.panel_Preview.Margin = new System.Windows.Forms.Padding(6);
			this.panel_Preview.Name = "panel_Preview";
			this.panel_Preview.Size = new System.Drawing.Size(460, 230);
			this.panel_Preview.TabIndex = 4;
			// 
			// sectionPanel
			// 
			this.sectionPanel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.sectionPanel.Controls.Add(this.button_Preview);
			this.sectionPanel.Controls.Add(this.label_01);
			this.sectionPanel.Controls.Add(this.button_Remove);
			this.sectionPanel.Controls.Add(this.button_Change);
			this.sectionPanel.Controls.Add(this.panel_Preview);
			this.sectionPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.sectionPanel.Location = new System.Drawing.Point(0, 0);
			this.sectionPanel.Name = "sectionPanel";
			this.sectionPanel.SectionHeader = "Splash Screen Image";
			this.sectionPanel.Size = new System.Drawing.Size(640, 270);
			this.sectionPanel.TabIndex = 0;
			// 
			// SettingsSplashScreen
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
			this.Controls.Add(this.sectionPanel);
			this.MaximumSize = new System.Drawing.Size(640, 270);
			this.MinimumSize = new System.Drawing.Size(640, 270);
			this.Name = "SettingsSplashScreen";
			this.Size = new System.Drawing.Size(640, 270);
			this.panel_Preview.ResumeLayout(false);
			this.sectionPanel.ResumeLayout(false);
			this.ResumeLayout(false);

		}

		#endregion

		private DarkUI.Controls.DarkButton button_Change;
		private DarkUI.Controls.DarkButton button_Preview;
		private DarkUI.Controls.DarkButton button_Remove;
		private DarkUI.Controls.DarkLabel label_01;
		private DarkUI.Controls.DarkLabel label_Blank;
		private DarkUI.Controls.DarkLabel label_NotSupported;
		private DarkUI.Controls.DarkSectionPanel sectionPanel;
		private System.Windows.Forms.Panel panel_Preview;
	}
}
