namespace TombIDE.ProjectMaster
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
			this.label_Blank = new DarkUI.Controls.DarkLabel();
			this.panel_Preview = new System.Windows.Forms.Panel();
			this.sectionPanel = new DarkUI.Controls.DarkSectionPanel();
			this.panel_Preview.SuspendLayout();
			this.sectionPanel.SuspendLayout();
			this.SuspendLayout();
			// 
			// button_Change
			// 
			this.button_Change.Location = new System.Drawing.Point(336, 175);
			this.button_Change.Margin = new System.Windows.Forms.Padding(3, 3, 6, 6);
			this.button_Change.Name = "button_Change";
			this.button_Change.Size = new System.Drawing.Size(295, 26);
			this.button_Change.TabIndex = 0;
			this.button_Change.Text = "Change...";
			this.button_Change.Click += new System.EventHandler(this.button_Change_Click);
			// 
			// button_Reset
			// 
			this.button_Reset.Location = new System.Drawing.Point(336, 245);
			this.button_Reset.Margin = new System.Windows.Forms.Padding(3, 3, 6, 6);
			this.button_Reset.Name = "button_Reset";
			this.button_Reset.Size = new System.Drawing.Size(295, 26);
			this.button_Reset.TabIndex = 2;
			this.button_Reset.Text = "Reset to default";
			this.button_Reset.Click += new System.EventHandler(this.button_Reset_Click);
			// 
			// button_UseBlank
			// 
			this.button_UseBlank.Location = new System.Drawing.Point(336, 210);
			this.button_UseBlank.Margin = new System.Windows.Forms.Padding(3, 3, 6, 6);
			this.button_UseBlank.Name = "button_UseBlank";
			this.button_UseBlank.Size = new System.Drawing.Size(295, 26);
			this.button_UseBlank.TabIndex = 1;
			this.button_UseBlank.Text = "Use blank (empty black screen)";
			this.button_UseBlank.Click += new System.EventHandler(this.button_UseBlank_Click);
			// 
			// label_Blank
			// 
			this.label_Blank.Dock = System.Windows.Forms.DockStyle.Fill;
			this.label_Blank.Font = new System.Drawing.Font("Microsoft Sans Serif", 72F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
			this.label_Blank.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
			this.label_Blank.Location = new System.Drawing.Point(0, 0);
			this.label_Blank.Name = "label_Blank";
			this.label_Blank.Size = new System.Drawing.Size(318, 238);
			this.label_Blank.TabIndex = 0;
			this.label_Blank.Text = "Blank";
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
			this.sectionPanel.Controls.Add(this.button_UseBlank);
			this.sectionPanel.Controls.Add(this.button_Reset);
			this.sectionPanel.Controls.Add(this.button_Change);
			this.sectionPanel.Controls.Add(this.panel_Preview);
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
			this.ResumeLayout(false);

		}

		#endregion

		private DarkUI.Controls.DarkButton button_Change;
		private DarkUI.Controls.DarkButton button_Reset;
		private DarkUI.Controls.DarkButton button_UseBlank;
		private DarkUI.Controls.DarkLabel label_Blank;
		private DarkUI.Controls.DarkSectionPanel sectionPanel;
		private System.Windows.Forms.Panel panel_Preview;
	}
}
