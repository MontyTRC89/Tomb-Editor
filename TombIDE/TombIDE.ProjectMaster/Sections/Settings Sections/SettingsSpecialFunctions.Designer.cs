﻿namespace TombIDE.ProjectMaster
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
			this.button_BatchBuild = new DarkUI.Controls.DarkButton();
			this.textBox_LauncherName = new System.Windows.Forms.TextBox();
			this.sectionPanel.SuspendLayout();
			this.SuspendLayout();
			// 
			// button_BuildArchive
			// 
			this.button_BuildArchive.Checked = false;
			this.button_BuildArchive.Enabled = false;
			this.button_BuildArchive.Image = global::TombIDE.ProjectMaster.Properties.Resources.general_Open_16;
			this.button_BuildArchive.Location = new System.Drawing.Point(321, 96);
			this.button_BuildArchive.Margin = new System.Windows.Forms.Padding(6, 3, 6, 6);
			this.button_BuildArchive.Name = "button_BuildArchive";
			this.button_BuildArchive.Size = new System.Drawing.Size(310, 25);
			this.button_BuildArchive.TabIndex = 1;
			this.button_BuildArchive.Text = "Create a \"Ready To Play\" game archive (Not implem.)";
			this.button_BuildArchive.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
			// 
			// button_DeleteLogs
			// 
			this.button_DeleteLogs.Checked = false;
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
			this.button_RenameLauncher.Checked = false;
			this.button_RenameLauncher.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
			this.button_RenameLauncher.Image = global::TombIDE.ProjectMaster.Properties.Resources.general_edit_16;
			this.button_RenameLauncher.Location = new System.Drawing.Point(7, 34);
			this.button_RenameLauncher.Margin = new System.Windows.Forms.Padding(6, 9, 3, 3);
			this.button_RenameLauncher.Name = "button_RenameLauncher";
			this.button_RenameLauncher.Size = new System.Drawing.Size(192, 25);
			this.button_RenameLauncher.TabIndex = 2;
			this.button_RenameLauncher.Text = "Rename the launcher file...";
			this.button_RenameLauncher.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
			this.button_RenameLauncher.Click += new System.EventHandler(this.button_RenameLauncher_Click);
			// 
			// sectionPanel
			// 
			this.sectionPanel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.sectionPanel.Controls.Add(this.button_BatchBuild);
			this.sectionPanel.Controls.Add(this.textBox_LauncherName);
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
			// button_BatchBuild
			// 
			this.button_BatchBuild.Checked = false;
			this.button_BatchBuild.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
			this.button_BatchBuild.Image = global::TombIDE.ProjectMaster.Properties.Resources.general_ArrowDown_16;
			this.button_BatchBuild.Location = new System.Drawing.Point(7, 96);
			this.button_BatchBuild.Margin = new System.Windows.Forms.Padding(6, 3, 6, 6);
			this.button_BatchBuild.Name = "button_BatchBuild";
			this.button_BatchBuild.Size = new System.Drawing.Size(310, 25);
			this.button_BatchBuild.TabIndex = 4;
			this.button_BatchBuild.Text = "Rebuild all project levels at once. (Batch)";
			this.button_BatchBuild.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
			this.button_BatchBuild.Click += new System.EventHandler(this.button_BatchBuild_Click);
			// 
			// textBox_LauncherName
			// 
			this.textBox_LauncherName.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(48)))), ((int)(((byte)(48)))), ((int)(((byte)(48)))));
			this.textBox_LauncherName.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.textBox_LauncherName.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
			this.textBox_LauncherName.ForeColor = System.Drawing.Color.Gainsboro;
			this.textBox_LauncherName.Location = new System.Drawing.Point(205, 33);
			this.textBox_LauncherName.Margin = new System.Windows.Forms.Padding(3, 8, 6, 3);
			this.textBox_LauncherName.Name = "textBox_LauncherName";
			this.textBox_LauncherName.ReadOnly = true;
			this.textBox_LauncherName.Size = new System.Drawing.Size(426, 26);
			this.textBox_LauncherName.TabIndex = 3;
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
			this.sectionPanel.PerformLayout();
			this.ResumeLayout(false);

		}

		#endregion

		private DarkUI.Controls.DarkButton button_BuildArchive;
		private DarkUI.Controls.DarkButton button_DeleteLogs;
		private DarkUI.Controls.DarkButton button_RenameLauncher;
		private DarkUI.Controls.DarkSectionPanel sectionPanel;
		private System.Windows.Forms.TextBox textBox_LauncherName;
		private DarkUI.Controls.DarkButton button_BatchBuild;
	}
}
