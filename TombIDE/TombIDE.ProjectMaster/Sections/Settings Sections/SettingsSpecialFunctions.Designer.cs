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
			this.button_BuildArchive = new DarkUI.Controls.DarkButton();
			this.button_DeleteLogs = new DarkUI.Controls.DarkButton();
			this.button_RenameLauncher = new DarkUI.Controls.DarkButton();
			this.sectionPanel = new DarkUI.Controls.DarkSectionPanel();
			this.sectionPanel.SuspendLayout();
			this.SuspendLayout();
			// 
			// button_BuildArchive
			// 
			this.button_BuildArchive.Enabled = false;
			this.button_BuildArchive.Image = global::TombIDE.ProjectMaster.Properties.Resources.general_Open_16;
			this.button_BuildArchive.Location = new System.Drawing.Point(7, 96);
			this.button_BuildArchive.Margin = new System.Windows.Forms.Padding(6, 3, 6, 6);
			this.button_BuildArchive.Name = "button_BuildArchive";
			this.button_BuildArchive.Size = new System.Drawing.Size(624, 25);
			this.button_BuildArchive.TabIndex = 1;
			this.button_BuildArchive.Text = "Create a \"Ready To Play\" game archive (Not implemented)";
			this.button_BuildArchive.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
			// 
			// button_DeleteLogs
			// 
			this.button_DeleteLogs.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
			this.button_DeleteLogs.Image = global::TombIDE.ProjectMaster.Properties.Resources.general_trash_16;
			this.button_DeleteLogs.Location = new System.Drawing.Point(7, 65);
			this.button_DeleteLogs.Margin = new System.Windows.Forms.Padding(6, 3, 6, 3);
			this.button_DeleteLogs.Name = "button_DeleteLogs";
			this.button_DeleteLogs.Size = new System.Drawing.Size(624, 25);
			this.button_DeleteLogs.TabIndex = 0;
			this.button_DeleteLogs.Text = "Delete all logs and error dumps from the engine folder";
			this.button_DeleteLogs.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
			this.button_DeleteLogs.Click += new System.EventHandler(this.button_DeleteLogs_Click);
			// 
			// button_RenameLauncher
			// 
			this.button_RenameLauncher.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
			this.button_RenameLauncher.Image = global::TombIDE.ProjectMaster.Properties.Resources.general_edit_16;
			this.button_RenameLauncher.Location = new System.Drawing.Point(7, 34);
			this.button_RenameLauncher.Margin = new System.Windows.Forms.Padding(6, 9, 6, 3);
			this.button_RenameLauncher.Name = "button_RenameLauncher";
			this.button_RenameLauncher.Size = new System.Drawing.Size(624, 25);
			this.button_RenameLauncher.TabIndex = 2;
			this.button_RenameLauncher.Text = "Rename the launcher file...";
			this.button_RenameLauncher.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
			// 
			// sectionPanel
			// 
			this.sectionPanel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.sectionPanel.Controls.Add(this.button_RenameLauncher);
			this.sectionPanel.Controls.Add(this.button_BuildArchive);
			this.sectionPanel.Controls.Add(this.button_DeleteLogs);
			this.sectionPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.sectionPanel.Location = new System.Drawing.Point(0, 0);
			this.sectionPanel.Name = "sectionPanel";
			this.sectionPanel.SectionHeader = "Special Functions";
			this.sectionPanel.Size = new System.Drawing.Size(640, 130);
			this.sectionPanel.TabIndex = 0;
			// 
			// SettingsSpecialFunctions
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
			this.Controls.Add(this.sectionPanel);
			this.MaximumSize = new System.Drawing.Size(640, 130);
			this.MinimumSize = new System.Drawing.Size(640, 130);
			this.Name = "SettingsSpecialFunctions";
			this.Size = new System.Drawing.Size(640, 130);
			this.sectionPanel.ResumeLayout(false);
			this.ResumeLayout(false);

		}

		#endregion

		private DarkUI.Controls.DarkButton button_BuildArchive;
		private DarkUI.Controls.DarkButton button_DeleteLogs;
		private DarkUI.Controls.DarkButton button_RenameLauncher;
		private DarkUI.Controls.DarkSectionPanel sectionPanel;
	}
}
