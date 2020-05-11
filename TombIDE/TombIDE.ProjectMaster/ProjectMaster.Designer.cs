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
			this.components = new System.ComponentModel.Container();
			this.animationTimer = new System.Windows.Forms.Timer(this.components);
			this.button_HidePlugins = new DarkUI.Controls.DarkButton();
			this.button_ShowPlugins = new DarkUI.Controls.DarkButton();
			this.internalDLLFileWatcher = new System.IO.FileSystemWatcher();
			this.internalPluginFolderWatcher = new System.IO.FileSystemWatcher();
			this.levelFolderWatcher = new System.IO.FileSystemWatcher();
			this.prj2FileWatcher = new System.IO.FileSystemWatcher();
			this.projectDLLFileWatcher = new System.IO.FileSystemWatcher();
			this.section_LevelList = new TombIDE.ProjectMaster.SectionLevelList();
			this.section_LevelProperties = new TombIDE.ProjectMaster.SectionLevelProperties();
			this.section_PluginList = new TombIDE.ProjectMaster.SectionPluginList();
			this.section_ProjectInfo = new TombIDE.ProjectMaster.SectionProjectSettings();
			this.splitContainer_Info = new System.Windows.Forms.SplitContainer();
			this.splitContainer_Levels = new System.Windows.Forms.SplitContainer();
			this.splitContainer_Main = new System.Windows.Forms.SplitContainer();
			this.toolTip = new System.Windows.Forms.ToolTip(this.components);
			((System.ComponentModel.ISupportInitialize)(this.internalDLLFileWatcher)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.internalPluginFolderWatcher)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.levelFolderWatcher)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.prj2FileWatcher)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.projectDLLFileWatcher)).BeginInit();
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
			// animationTimer
			// 
			this.animationTimer.Interval = 1;
			this.animationTimer.Tick += new System.EventHandler(this.animationTimer_Tick);
			// 
			// button_HidePlugins
			// 
			this.button_HidePlugins.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.button_HidePlugins.Checked = false;
			this.button_HidePlugins.ForeColor = System.Drawing.Color.Gainsboro;
			this.button_HidePlugins.Location = new System.Drawing.Point(653, 3);
			this.button_HidePlugins.Name = "button_HidePlugins";
			this.button_HidePlugins.Size = new System.Drawing.Size(24, 24);
			this.button_HidePlugins.TabIndex = 1;
			this.button_HidePlugins.Text = "▼";
			this.toolTip.SetToolTip(this.button_HidePlugins, "Hide Project Plugins");
			this.button_HidePlugins.Click += new System.EventHandler(this.button_HidePlugins_Click);
			// 
			// button_ShowPlugins
			// 
			this.button_ShowPlugins.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.button_ShowPlugins.Checked = false;
			this.button_ShowPlugins.ForeColor = System.Drawing.Color.Gainsboro;
			this.button_ShowPlugins.Location = new System.Drawing.Point(616, 256);
			this.button_ShowPlugins.Margin = new System.Windows.Forms.Padding(3, 3, 32, 12);
			this.button_ShowPlugins.Name = "button_ShowPlugins";
			this.button_ShowPlugins.Size = new System.Drawing.Size(32, 32);
			this.button_ShowPlugins.TabIndex = 1;
			this.button_ShowPlugins.Text = "▲";
			this.toolTip.SetToolTip(this.button_ShowPlugins, "Show Project Plugins");
			this.button_ShowPlugins.Click += new System.EventHandler(this.button_ShowPlugins_Click);
			// 
			// internalDLLFileWatcher
			// 
			this.internalDLLFileWatcher.EnableRaisingEvents = true;
			this.internalDLLFileWatcher.Filter = "*.dll";
			this.internalDLLFileWatcher.IncludeSubdirectories = true;
			this.internalDLLFileWatcher.SynchronizingObject = this;
			this.internalDLLFileWatcher.Deleted += new System.IO.FileSystemEventHandler(this.internalDLLFileWatcher_Deleted);
			this.internalDLLFileWatcher.Renamed += new System.IO.RenamedEventHandler(this.internalDLLFileWatcher_Renamed);
			// 
			// internalPluginFolderWatcher
			// 
			this.internalPluginFolderWatcher.EnableRaisingEvents = true;
			this.internalPluginFolderWatcher.NotifyFilter = System.IO.NotifyFilters.DirectoryName;
			this.internalPluginFolderWatcher.SynchronizingObject = this;
			this.internalPluginFolderWatcher.Deleted += new System.IO.FileSystemEventHandler(this.internalPluginFolderWatcher_Deleted);
			this.internalPluginFolderWatcher.Renamed += new System.IO.RenamedEventHandler(this.internalPluginFolderWatcher_Renamed);
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
			// projectDLLFileWatcher
			// 
			this.projectDLLFileWatcher.EnableRaisingEvents = true;
			this.projectDLLFileWatcher.Filter = "*.dll";
			this.projectDLLFileWatcher.IncludeSubdirectories = true;
			this.projectDLLFileWatcher.SynchronizingObject = this;
			this.projectDLLFileWatcher.Changed += new System.IO.FileSystemEventHandler(this.projectDLLFileWatcher_Changed);
			this.projectDLLFileWatcher.Created += new System.IO.FileSystemEventHandler(this.projectDLLFileWatcher_Changed);
			this.projectDLLFileWatcher.Deleted += new System.IO.FileSystemEventHandler(this.projectDLLFileWatcher_Changed);
			this.projectDLLFileWatcher.Renamed += new System.IO.RenamedEventHandler(this.projectDLLFileWatcher_Renamed);
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
			this.splitContainer_Info.Panel1.Controls.Add(this.button_ShowPlugins);
			this.splitContainer_Info.Panel1.Controls.Add(this.section_ProjectInfo);
			this.splitContainer_Info.Panel1MinSize = 300;
			// 
			// splitContainer_Info.Panel2
			// 
			this.splitContainer_Info.Panel2.Controls.Add(this.button_HidePlugins);
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
			((System.ComponentModel.ISupportInitialize)(this.internalDLLFileWatcher)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.internalPluginFolderWatcher)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.levelFolderWatcher)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.prj2FileWatcher)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.projectDLLFileWatcher)).EndInit();
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

		private DarkUI.Controls.DarkButton button_HidePlugins;
		private DarkUI.Controls.DarkButton button_ShowPlugins;
		private SectionLevelList section_LevelList;
		private SectionLevelProperties section_LevelProperties;
		private SectionPluginList section_PluginList;
		private SectionProjectSettings section_ProjectInfo;
		private System.IO.FileSystemWatcher internalDLLFileWatcher;
		private System.IO.FileSystemWatcher internalPluginFolderWatcher;
		private System.IO.FileSystemWatcher levelFolderWatcher;
		private System.IO.FileSystemWatcher prj2FileWatcher;
		private System.IO.FileSystemWatcher projectDLLFileWatcher;
		private System.Windows.Forms.SplitContainer splitContainer_Info;
		private System.Windows.Forms.SplitContainer splitContainer_Levels;
		private System.Windows.Forms.SplitContainer splitContainer_Main;
		private System.Windows.Forms.Timer animationTimer;
		private System.Windows.Forms.ToolTip toolTip;
	}
}
