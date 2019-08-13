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
			this.button_BuildGame = new DarkUI.Controls.DarkButton();
			this.button_DeleteLogs = new DarkUI.Controls.DarkButton();
			this.sectionPanel = new DarkUI.Controls.DarkSectionPanel();
			this.sectionPanel.SuspendLayout();
			this.SuspendLayout();
			// 
			// button_BuildGame
			// 
			this.button_BuildGame.Enabled = false;
			this.button_BuildGame.Location = new System.Drawing.Point(321, 34);
			this.button_BuildGame.Margin = new System.Windows.Forms.Padding(2, 9, 3, 3);
			this.button_BuildGame.Name = "button_BuildGame";
			this.button_BuildGame.Size = new System.Drawing.Size(313, 25);
			this.button_BuildGame.TabIndex = 1;
			this.button_BuildGame.Text = "Create a \"Ready To Play\" Game Build (Not Implemented)";
			// 
			// button_DeleteLogs
			// 
			this.button_DeleteLogs.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
			this.button_DeleteLogs.Location = new System.Drawing.Point(4, 34);
			this.button_DeleteLogs.Margin = new System.Windows.Forms.Padding(3, 9, 2, 3);
			this.button_DeleteLogs.Name = "button_DeleteLogs";
			this.button_DeleteLogs.Size = new System.Drawing.Size(313, 25);
			this.button_DeleteLogs.TabIndex = 0;
			this.button_DeleteLogs.Text = "Delete All Logs / Error Dumps from the Project Folder";
			this.button_DeleteLogs.Click += new System.EventHandler(this.button_DeleteLogs_Click);
			// 
			// sectionPanel
			// 
			this.sectionPanel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.sectionPanel.Controls.Add(this.button_BuildGame);
			this.sectionPanel.Controls.Add(this.button_DeleteLogs);
			this.sectionPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.sectionPanel.Location = new System.Drawing.Point(0, 0);
			this.sectionPanel.Name = "sectionPanel";
			this.sectionPanel.SectionHeader = "Special Functions";
			this.sectionPanel.Size = new System.Drawing.Size(640, 65);
			this.sectionPanel.TabIndex = 0;
			// 
			// SettingsSpecialFunctions
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
			this.Controls.Add(this.sectionPanel);
			this.MaximumSize = new System.Drawing.Size(640, 65);
			this.MinimumSize = new System.Drawing.Size(640, 65);
			this.Name = "SettingsSpecialFunctions";
			this.Size = new System.Drawing.Size(640, 65);
			this.sectionPanel.ResumeLayout(false);
			this.ResumeLayout(false);

		}

		#endregion

		private DarkUI.Controls.DarkButton button_BuildGame;
		private DarkUI.Controls.DarkButton button_DeleteLogs;
		private DarkUI.Controls.DarkSectionPanel sectionPanel;
	}
}
