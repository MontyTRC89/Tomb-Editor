namespace TombIDE.ProjectMaster
{
	partial class ProjectMaster
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
			this.levelFolderWatcher = new System.IO.FileSystemWatcher();
			this.prj2FileWatcher = new System.IO.FileSystemWatcher();
			this.section_LevelList = new TombIDE.ProjectMaster.SectionLevelList();
			this.section_LevelProperties = new TombIDE.ProjectMaster.SectionLevelProperties();
			this.section_PluginList = new TombIDE.ProjectMaster.SectionPluginList();
			this.section_ProjectInfo = new TombIDE.ProjectMaster.SectionProjectSettings();
			this.splitContainer_Info = new System.Windows.Forms.SplitContainer();
			this.splitContainer_Levels = new System.Windows.Forms.SplitContainer();
			this.splitContainer_Main = new System.Windows.Forms.SplitContainer();
			((System.ComponentModel.ISupportInitialize)(this.levelFolderWatcher)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.prj2FileWatcher)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.splitContainer_Info)).BeginInit();
			this.splitContainer_Info.Panel1.SuspendLayout();
			this.splitContainer_Info.Panel2.SuspendLayout();
			this.splitContainer_Info.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.splitContainer_Levels)).BeginInit();
			this.splitContainer_Levels.Panel1.SuspendLayout();
			this.splitContainer_Levels.Panel2.SuspendLayout();
			this.splitContainer_Levels.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.splitContainer_Main)).BeginInit();
			this.splitContainer_Main.Panel1.SuspendLayout();
			this.splitContainer_Main.Panel2.SuspendLayout();
			this.splitContainer_Main.SuspendLayout();
			this.SuspendLayout();
			// 
			// levelFolderWatcher
			// 
			this.levelFolderWatcher.EnableRaisingEvents = true;
			this.levelFolderWatcher.NotifyFilter = System.IO.NotifyFilters.DirectoryName;
			this.levelFolderWatcher.SynchronizingObject = this;
			this.levelFolderWatcher.Deleted += new System.IO.FileSystemEventHandler(this.levelFolderWatcher_Deleted);
			// 
			// prj2FileWatcher
			// 
			this.prj2FileWatcher.EnableRaisingEvents = true;
			this.prj2FileWatcher.Filter = "*.prj2";
			this.prj2FileWatcher.IncludeSubdirectories = true;
			this.prj2FileWatcher.SynchronizingObject = this;
			this.prj2FileWatcher.Deleted += new System.IO.FileSystemEventHandler(this.prj2FileWatcher_Deleted);
			// 
			// section_LevelList
			// 
			this.section_LevelList.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
			this.section_LevelList.Dock = System.Windows.Forms.DockStyle.Fill;
			this.section_LevelList.Location = new System.Drawing.Point(0, 0);
			this.section_LevelList.Name = "section_LevelList";
			this.section_LevelList.Size = new System.Drawing.Size(320, 300);
			this.section_LevelList.TabIndex = 0;
			// 
			// section_LevelProperties
			// 
			this.section_LevelProperties.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
			this.section_LevelProperties.Dock = System.Windows.Forms.DockStyle.Fill;
			this.section_LevelProperties.Location = new System.Drawing.Point(0, 0);
			this.section_LevelProperties.Name = "section_LevelProperties";
			this.section_LevelProperties.Size = new System.Drawing.Size(320, 295);
			this.section_LevelProperties.TabIndex = 0;
			// 
			// section_PluginList
			// 
			this.section_PluginList.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
			this.section_PluginList.Dock = System.Windows.Forms.DockStyle.Fill;
			this.section_PluginList.Location = new System.Drawing.Point(0, 0);
			this.section_PluginList.Name = "section_PluginList";
			this.section_PluginList.Size = new System.Drawing.Size(680, 296);
			this.section_PluginList.TabIndex = 0;
			// 
			// section_ProjectInfo
			// 
			this.section_ProjectInfo.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
			this.section_ProjectInfo.Dock = System.Windows.Forms.DockStyle.Fill;
			this.section_ProjectInfo.Location = new System.Drawing.Point(0, 0);
			this.section_ProjectInfo.Name = "section_ProjectInfo";
			this.section_ProjectInfo.Size = new System.Drawing.Size(680, 300);
			this.section_ProjectInfo.TabIndex = 0;
			// 
			// splitContainer_Info
			// 
			this.splitContainer_Info.Dock = System.Windows.Forms.DockStyle.Fill;
			this.splitContainer_Info.FixedPanel = System.Windows.Forms.FixedPanel.Panel2;
			this.splitContainer_Info.Location = new System.Drawing.Point(0, 0);
			this.splitContainer_Info.Name = "splitContainer_Info";
			this.splitContainer_Info.Orientation = System.Windows.Forms.Orientation.Horizontal;
			// 
			// splitContainer_Info.Panel1
			// 
			this.splitContainer_Info.Panel1.Controls.Add(this.section_ProjectInfo);
			this.splitContainer_Info.Panel1MinSize = 300;
			// 
			// splitContainer_Info.Panel2
			// 
			this.splitContainer_Info.Panel2.Controls.Add(this.section_PluginList);
			this.splitContainer_Info.Panel2MinSize = 294;
			this.splitContainer_Info.Size = new System.Drawing.Size(680, 600);
			this.splitContainer_Info.SplitterDistance = 300;
			this.splitContainer_Info.SplitterWidth = 5;
			this.splitContainer_Info.TabIndex = 0;
			// 
			// splitContainer_Levels
			// 
			this.splitContainer_Levels.Dock = System.Windows.Forms.DockStyle.Fill;
			this.splitContainer_Levels.Location = new System.Drawing.Point(0, 0);
			this.splitContainer_Levels.Name = "splitContainer_Levels";
			this.splitContainer_Levels.Orientation = System.Windows.Forms.Orientation.Horizontal;
			// 
			// splitContainer_Levels.Panel1
			// 
			this.splitContainer_Levels.Panel1.Controls.Add(this.section_LevelList);
			this.splitContainer_Levels.Panel1MinSize = 300;
			// 
			// splitContainer_Levels.Panel2
			// 
			this.splitContainer_Levels.Panel2.Controls.Add(this.section_LevelProperties);
			this.splitContainer_Levels.Panel2MinSize = 294;
			this.splitContainer_Levels.Size = new System.Drawing.Size(320, 600);
			this.splitContainer_Levels.SplitterDistance = 300;
			this.splitContainer_Levels.SplitterWidth = 5;
			this.splitContainer_Levels.TabIndex = 0;
			// 
			// splitContainer_Main
			// 
			this.splitContainer_Main.Dock = System.Windows.Forms.DockStyle.Fill;
			this.splitContainer_Main.Location = new System.Drawing.Point(0, 0);
			this.splitContainer_Main.Name = "splitContainer_Main";
			// 
			// splitContainer_Main.Panel1
			// 
			this.splitContainer_Main.Panel1.Controls.Add(this.splitContainer_Levels);
			this.splitContainer_Main.Panel1MinSize = 320;
			// 
			// splitContainer_Main.Panel2
			// 
			this.splitContainer_Main.Panel2.Controls.Add(this.splitContainer_Info);
			this.splitContainer_Main.Panel2MinSize = 680;
			this.splitContainer_Main.Size = new System.Drawing.Size(1005, 600);
			this.splitContainer_Main.SplitterDistance = 320;
			this.splitContainer_Main.SplitterIncrement = 5;
			this.splitContainer_Main.SplitterWidth = 5;
			this.splitContainer_Main.TabIndex = 0;
			// 
			// ProjectMaster
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
			this.Controls.Add(this.splitContainer_Main);
			this.Name = "ProjectMaster";
			this.Size = new System.Drawing.Size(1005, 600);
			((System.ComponentModel.ISupportInitialize)(this.levelFolderWatcher)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.prj2FileWatcher)).EndInit();
			this.splitContainer_Info.Panel1.ResumeLayout(false);
			this.splitContainer_Info.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.splitContainer_Info)).EndInit();
			this.splitContainer_Info.ResumeLayout(false);
			this.splitContainer_Levels.Panel1.ResumeLayout(false);
			this.splitContainer_Levels.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.splitContainer_Levels)).EndInit();
			this.splitContainer_Levels.ResumeLayout(false);
			this.splitContainer_Main.Panel1.ResumeLayout(false);
			this.splitContainer_Main.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.splitContainer_Main)).EndInit();
			this.splitContainer_Main.ResumeLayout(false);
			this.ResumeLayout(false);

		}

		#endregion

		private SectionLevelList section_LevelList;
		private SectionLevelProperties section_LevelProperties;
		private SectionPluginList section_PluginList;
		private SectionProjectSettings section_ProjectInfo;
		private System.IO.FileSystemWatcher levelFolderWatcher;
		private System.IO.FileSystemWatcher prj2FileWatcher;
		private System.Windows.Forms.SplitContainer splitContainer_Info;
		private System.Windows.Forms.SplitContainer splitContainer_Levels;
		private System.Windows.Forms.SplitContainer splitContainer_Main;
	}
}
