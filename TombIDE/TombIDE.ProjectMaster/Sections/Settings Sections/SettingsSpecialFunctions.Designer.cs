namespace TombIDE.ProjectMaster
{
	partial class SettingsSpecialFunctions
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
			button_BuildArchive = new DarkUI.Controls.DarkButton();
			button_DeleteLogs = new DarkUI.Controls.DarkButton();
			button_RenameLauncher = new DarkUI.Controls.DarkButton();
			sectionPanel = new DarkUI.Controls.DarkSectionPanel();
			textBox_LauncherName = new System.Windows.Forms.TextBox();
			sectionPanel.SuspendLayout();
			SuspendLayout();
			// 
			// button_BuildArchive
			// 
			button_BuildArchive.Checked = false;
			button_BuildArchive.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
			button_BuildArchive.Image = Properties.Resources.archive_folder_16;
			button_BuildArchive.Location = new System.Drawing.Point(7, 96);
			button_BuildArchive.Margin = new System.Windows.Forms.Padding(6, 3, 6, 6);
			button_BuildArchive.Name = "button_BuildArchive";
			button_BuildArchive.Size = new System.Drawing.Size(624, 25);
			button_BuildArchive.TabIndex = 1;
			button_BuildArchive.Text = "Create a \"Ready To Publish\" Game Archive...";
			button_BuildArchive.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
			button_BuildArchive.Click += button_BuildArchive_Click;
			// 
			// button_DeleteLogs
			// 
			button_DeleteLogs.Checked = false;
			button_DeleteLogs.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
			button_DeleteLogs.Image = Properties.Resources.general_trash_16;
			button_DeleteLogs.Location = new System.Drawing.Point(7, 65);
			button_DeleteLogs.Margin = new System.Windows.Forms.Padding(6, 3, 6, 3);
			button_DeleteLogs.Name = "button_DeleteLogs";
			button_DeleteLogs.Size = new System.Drawing.Size(624, 25);
			button_DeleteLogs.TabIndex = 0;
			button_DeleteLogs.Text = "Delete all logs and error dumps from the engine folder";
			button_DeleteLogs.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
			button_DeleteLogs.Click += button_DeleteLogs_Click;
			// 
			// button_RenameLauncher
			// 
			button_RenameLauncher.Checked = false;
			button_RenameLauncher.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
			button_RenameLauncher.Image = Properties.Resources.general_edit_16;
			button_RenameLauncher.Location = new System.Drawing.Point(7, 32);
			button_RenameLauncher.Margin = new System.Windows.Forms.Padding(6, 9, 3, 3);
			button_RenameLauncher.Name = "button_RenameLauncher";
			button_RenameLauncher.Size = new System.Drawing.Size(192, 27);
			button_RenameLauncher.TabIndex = 2;
			button_RenameLauncher.Text = "Rename the launcher file...";
			button_RenameLauncher.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
			button_RenameLauncher.Click += button_RenameLauncher_Click;
			// 
			// sectionPanel
			// 
			sectionPanel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			sectionPanel.Controls.Add(textBox_LauncherName);
			sectionPanel.Controls.Add(button_RenameLauncher);
			sectionPanel.Controls.Add(button_BuildArchive);
			sectionPanel.Controls.Add(button_DeleteLogs);
			sectionPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			sectionPanel.Location = new System.Drawing.Point(0, 0);
			sectionPanel.Name = "sectionPanel";
			sectionPanel.SectionHeader = "Special Functions";
			sectionPanel.Size = new System.Drawing.Size(640, 130);
			sectionPanel.TabIndex = 0;
			// 
			// textBox_LauncherName
			// 
			textBox_LauncherName.BackColor = System.Drawing.Color.FromArgb(48, 48, 48);
			textBox_LauncherName.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			textBox_LauncherName.Font = new System.Drawing.Font("Segoe UI", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
			textBox_LauncherName.ForeColor = System.Drawing.Color.Gainsboro;
			textBox_LauncherName.Location = new System.Drawing.Point(205, 32);
			textBox_LauncherName.Margin = new System.Windows.Forms.Padding(3, 8, 6, 3);
			textBox_LauncherName.Name = "textBox_LauncherName";
			textBox_LauncherName.ReadOnly = true;
			textBox_LauncherName.Size = new System.Drawing.Size(426, 27);
			textBox_LauncherName.TabIndex = 3;
			// 
			// SettingsSpecialFunctions
			// 
			AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
			Controls.Add(sectionPanel);
			Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
			MaximumSize = new System.Drawing.Size(640, 130);
			MinimumSize = new System.Drawing.Size(640, 130);
			Name = "SettingsSpecialFunctions";
			Size = new System.Drawing.Size(640, 130);
			sectionPanel.ResumeLayout(false);
			sectionPanel.PerformLayout();
			ResumeLayout(false);
		}

		#endregion

		private DarkUI.Controls.DarkButton button_BuildArchive;
		private DarkUI.Controls.DarkButton button_DeleteLogs;
		private DarkUI.Controls.DarkButton button_RenameLauncher;
		private DarkUI.Controls.DarkSectionPanel sectionPanel;
		private System.Windows.Forms.TextBox textBox_LauncherName;
	}
}
