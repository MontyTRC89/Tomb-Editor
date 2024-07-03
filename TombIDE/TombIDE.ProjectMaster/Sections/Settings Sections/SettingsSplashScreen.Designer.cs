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
			button_Change = new DarkUI.Controls.DarkButton();
			button_Preview = new DarkUI.Controls.DarkButton();
			button_Remove = new DarkUI.Controls.DarkButton();
			label_01 = new DarkUI.Controls.DarkLabel();
			label_Blank = new DarkUI.Controls.DarkLabel();
			label_NotSupported = new DarkUI.Controls.DarkLabel();
			panel_Preview = new System.Windows.Forms.Panel();
			sectionPanel = new DarkUI.Controls.DarkSectionPanel();
			panel1 = new System.Windows.Forms.Panel();
			button_AdjustFrame = new DarkUI.Controls.DarkButton();
			panel_Preview.SuspendLayout();
			sectionPanel.SuspendLayout();
			SuspendLayout();
			// 
			// button_Change
			// 
			button_Change.Checked = false;
			button_Change.Location = new System.Drawing.Point(476, 200);
			button_Change.Margin = new System.Windows.Forms.Padding(3, 3, 6, 6);
			button_Change.Name = "button_Change";
			button_Change.Size = new System.Drawing.Size(155, 26);
			button_Change.TabIndex = 2;
			button_Change.Text = "Change image...";
			button_Change.Click += button_Change_Click;
			// 
			// button_Preview
			// 
			button_Preview.Checked = false;
			button_Preview.Location = new System.Drawing.Point(476, 37);
			button_Preview.Margin = new System.Windows.Forms.Padding(3, 12, 6, 6);
			button_Preview.Name = "button_Preview";
			button_Preview.Size = new System.Drawing.Size(155, 26);
			button_Preview.TabIndex = 0;
			button_Preview.Text = "Live Preview";
			button_Preview.Click += button_Preview_Click;
			// 
			// button_Remove
			// 
			button_Remove.Checked = false;
			button_Remove.Location = new System.Drawing.Point(476, 235);
			button_Remove.Margin = new System.Windows.Forms.Padding(3, 3, 6, 6);
			button_Remove.Name = "button_Remove";
			button_Remove.Size = new System.Drawing.Size(155, 26);
			button_Remove.TabIndex = 3;
			button_Remove.Text = "Remove (use blank)";
			button_Remove.Click += button_Remove_Click;
			// 
			// label_01
			// 
			label_01.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
			label_01.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
			label_01.Location = new System.Drawing.Point(476, 69);
			label_01.Margin = new System.Windows.Forms.Padding(3, 0, 6, 3);
			label_01.Name = "label_01";
			label_01.Size = new System.Drawing.Size(155, 79);
			label_01.TabIndex = 1;
			label_01.Text = "Supported image sizes:\r\n\r\n- 512x256 px (Small)\r\n- 768x384 px (Medium)\r\n- 1024x512 px (Large)";
			// 
			// label_Blank
			// 
			label_Blank.Dock = System.Windows.Forms.DockStyle.Fill;
			label_Blank.Font = new System.Drawing.Font("Segoe UI", 48F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
			label_Blank.ForeColor = System.Drawing.Color.Gray;
			label_Blank.Location = new System.Drawing.Point(0, 0);
			label_Blank.Name = "label_Blank";
			label_Blank.Size = new System.Drawing.Size(458, 228);
			label_Blank.TabIndex = 0;
			label_Blank.Text = "BLANK";
			label_Blank.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			label_Blank.Visible = false;
			// 
			// label_NotSupported
			// 
			label_NotSupported.Dock = System.Windows.Forms.DockStyle.Fill;
			label_NotSupported.ForeColor = System.Drawing.Color.Gray;
			label_NotSupported.Location = new System.Drawing.Point(0, 0);
			label_NotSupported.Name = "label_NotSupported";
			label_NotSupported.Size = new System.Drawing.Size(458, 228);
			label_NotSupported.TabIndex = 1;
			label_NotSupported.Text = "Legacy projects don't support splash screens.";
			label_NotSupported.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			label_NotSupported.Visible = false;
			// 
			// panel_Preview
			// 
			panel_Preview.BackColor = System.Drawing.Color.FromArgb(48, 48, 48);
			panel_Preview.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
			panel_Preview.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			panel_Preview.Controls.Add(label_NotSupported);
			panel_Preview.Controls.Add(label_Blank);
			panel_Preview.Location = new System.Drawing.Point(7, 31);
			panel_Preview.Margin = new System.Windows.Forms.Padding(6);
			panel_Preview.Name = "panel_Preview";
			panel_Preview.Size = new System.Drawing.Size(460, 230);
			panel_Preview.TabIndex = 4;
			// 
			// sectionPanel
			// 
			sectionPanel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			sectionPanel.Controls.Add(panel1);
			sectionPanel.Controls.Add(button_AdjustFrame);
			sectionPanel.Controls.Add(button_Preview);
			sectionPanel.Controls.Add(label_01);
			sectionPanel.Controls.Add(button_Remove);
			sectionPanel.Controls.Add(button_Change);
			sectionPanel.Controls.Add(panel_Preview);
			sectionPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			sectionPanel.Location = new System.Drawing.Point(0, 0);
			sectionPanel.Name = "sectionPanel";
			sectionPanel.SectionHeader = "Splash Screen Image";
			sectionPanel.Size = new System.Drawing.Size(640, 270);
			sectionPanel.TabIndex = 0;
			// 
			// panel1
			// 
			panel1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			panel1.Location = new System.Drawing.Point(476, 189);
			panel1.Name = "panel1";
			panel1.Size = new System.Drawing.Size(155, 1);
			panel1.TabIndex = 6;
			// 
			// button_AdjustFrame
			// 
			button_AdjustFrame.Checked = false;
			button_AdjustFrame.Location = new System.Drawing.Point(476, 154);
			button_AdjustFrame.Margin = new System.Windows.Forms.Padding(3, 3, 6, 6);
			button_AdjustFrame.Name = "button_AdjustFrame";
			button_AdjustFrame.Size = new System.Drawing.Size(155, 26);
			button_AdjustFrame.TabIndex = 5;
			button_AdjustFrame.Text = "Customize...";
			button_AdjustFrame.Click += button_AdjustFrame_Click;
			// 
			// SettingsSplashScreen
			// 
			AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
			Controls.Add(sectionPanel);
			Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
			MaximumSize = new System.Drawing.Size(640, 270);
			MinimumSize = new System.Drawing.Size(640, 270);
			Name = "SettingsSplashScreen";
			Size = new System.Drawing.Size(640, 270);
			panel_Preview.ResumeLayout(false);
			sectionPanel.ResumeLayout(false);
			ResumeLayout(false);
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
		private DarkUI.Controls.DarkButton button_AdjustFrame;
		private System.Windows.Forms.Panel panel1;
	}
}
