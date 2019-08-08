namespace TombIDE.ProjectMaster
{
	partial class SectionProjectSettings
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
			this.components = new System.ComponentModel.Container();
			this.panel_Background = new System.Windows.Forms.Panel();
			this.settings_Logo = new TombIDE.ProjectMaster.SettingsLogo();
			this.settings_StartupImage = new TombIDE.ProjectMaster.SettingsStartupImage();
			this.settings_GameIcon = new TombIDE.ProjectMaster.SettingsGameIcon();
			this.settings_ProjectInfo = new TombIDE.ProjectMaster.SettingsProjectInfo();
			this.sectionPanel = new DarkUI.Controls.DarkSectionPanel();
			this.toolTip = new System.Windows.Forms.ToolTip(this.components);
			this.panel_Background.SuspendLayout();
			this.sectionPanel.SuspendLayout();
			this.SuspendLayout();
			// 
			// panel_Background
			// 
			this.panel_Background.AutoScroll = true;
			this.panel_Background.AutoScrollMargin = new System.Drawing.Size(0, 9);
			this.panel_Background.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(48)))), ((int)(((byte)(48)))), ((int)(((byte)(48)))));
			this.panel_Background.Controls.Add(this.settings_Logo);
			this.panel_Background.Controls.Add(this.settings_StartupImage);
			this.panel_Background.Controls.Add(this.settings_GameIcon);
			this.panel_Background.Controls.Add(this.settings_ProjectInfo);
			this.panel_Background.Dock = System.Windows.Forms.DockStyle.Fill;
			this.panel_Background.Location = new System.Drawing.Point(1, 25);
			this.panel_Background.Name = "panel_Background";
			this.panel_Background.Size = new System.Drawing.Size(658, 1192);
			this.panel_Background.TabIndex = 0;
			// 
			// settings_Logo
			// 
			this.settings_Logo.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
			this.settings_Logo.Location = new System.Drawing.Point(9, 903);
			this.settings_Logo.Margin = new System.Windows.Forms.Padding(9, 3, 9, 9);
			this.settings_Logo.MaximumSize = new System.Drawing.Size(640, 280);
			this.settings_Logo.MinimumSize = new System.Drawing.Size(640, 280);
			this.settings_Logo.Name = "settings_Logo";
			this.settings_Logo.Size = new System.Drawing.Size(640, 280);
			this.settings_Logo.TabIndex = 3;
			// 
			// settings_StartupImage
			// 
			this.settings_StartupImage.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
			this.settings_StartupImage.Location = new System.Drawing.Point(9, 611);
			this.settings_StartupImage.Margin = new System.Windows.Forms.Padding(9, 3, 9, 9);
			this.settings_StartupImage.MaximumSize = new System.Drawing.Size(640, 280);
			this.settings_StartupImage.MinimumSize = new System.Drawing.Size(640, 280);
			this.settings_StartupImage.Name = "settings_StartupImage";
			this.settings_StartupImage.Size = new System.Drawing.Size(640, 280);
			this.settings_StartupImage.TabIndex = 2;
			// 
			// settings_GameIcon
			// 
			this.settings_GameIcon.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
			this.settings_GameIcon.Location = new System.Drawing.Point(9, 289);
			this.settings_GameIcon.Margin = new System.Windows.Forms.Padding(9, 3, 9, 9);
			this.settings_GameIcon.MaximumSize = new System.Drawing.Size(640, 310);
			this.settings_GameIcon.MinimumSize = new System.Drawing.Size(640, 310);
			this.settings_GameIcon.Name = "settings_GameIcon";
			this.settings_GameIcon.Size = new System.Drawing.Size(640, 310);
			this.settings_GameIcon.TabIndex = 1;
			// 
			// settings_ProjectInfo
			// 
			this.settings_ProjectInfo.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
			this.settings_ProjectInfo.Location = new System.Drawing.Point(9, 9);
			this.settings_ProjectInfo.Margin = new System.Windows.Forms.Padding(9);
			this.settings_ProjectInfo.MaximumSize = new System.Drawing.Size(640, 268);
			this.settings_ProjectInfo.MinimumSize = new System.Drawing.Size(640, 268);
			this.settings_ProjectInfo.Name = "settings_ProjectInfo";
			this.settings_ProjectInfo.Size = new System.Drawing.Size(640, 268);
			this.settings_ProjectInfo.TabIndex = 0;
			// 
			// sectionPanel
			// 
			this.sectionPanel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.sectionPanel.Controls.Add(this.panel_Background);
			this.sectionPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.sectionPanel.Location = new System.Drawing.Point(0, 0);
			this.sectionPanel.Name = "sectionPanel";
			this.sectionPanel.SectionHeader = "General Project Settings";
			this.sectionPanel.Size = new System.Drawing.Size(662, 1220);
			this.sectionPanel.TabIndex = 0;
			// 
			// toolTip
			// 
			this.toolTip.AutomaticDelay = 0;
			this.toolTip.UseAnimation = false;
			this.toolTip.UseFading = false;
			// 
			// SectionProjectSettings
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
			this.Controls.Add(this.sectionPanel);
			this.Name = "SectionProjectSettings";
			this.Size = new System.Drawing.Size(662, 1220);
			this.panel_Background.ResumeLayout(false);
			this.sectionPanel.ResumeLayout(false);
			this.ResumeLayout(false);

		}

		#endregion

		private DarkUI.Controls.DarkSectionPanel sectionPanel;
		private SettingsGameIcon settings_GameIcon;
		private SettingsLogo settings_Logo;
		private SettingsProjectInfo settings_ProjectInfo;
		private SettingsStartupImage settings_StartupImage;
		private System.Windows.Forms.Panel panel_Background;
		private System.Windows.Forms.ToolTip toolTip;
	}
}
