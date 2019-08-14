namespace TombIDE.ProjectMaster
{
	partial class FormPluginLibrary
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

		#region Windows Form Designer generated code

		private void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			this.button_Close = new DarkUI.Controls.DarkButton();
			this.button_Delete = new System.Windows.Forms.ToolStripButton();
			this.button_Download = new DarkUI.Controls.DarkButton();
			this.button_Install = new DarkUI.Controls.DarkButton();
			this.button_OpenArchive = new System.Windows.Forms.ToolStripButton();
			this.button_OpenFolder = new System.Windows.Forms.ToolStripButton();
			this.button_OpenInExplorer = new System.Windows.Forms.ToolStripButton();
			this.button_Refresh = new System.Windows.Forms.ToolStripButton();
			this.button_Uninstall = new DarkUI.Controls.DarkButton();
			this.dllFileWatcher = new System.IO.FileSystemWatcher();
			this.panel_01 = new System.Windows.Forms.Panel();
			this.sectionPanel_Installed = new DarkUI.Controls.DarkSectionPanel();
			this.treeView_Installed = new DarkUI.Controls.DarkTreeView();
			this.sectionPanel_Available = new DarkUI.Controls.DarkSectionPanel();
			this.treeView_AvailablePlugins = new DarkUI.Controls.DarkTreeView();
			this.toolStrip = new DarkUI.Controls.DarkToolStrip();
			this.separator_01 = new System.Windows.Forms.ToolStripSeparator();
			this.separator_02 = new System.Windows.Forms.ToolStripSeparator();
			this.panel_02 = new System.Windows.Forms.Panel();
			this.pluginFolderWatcher = new System.IO.FileSystemWatcher();
			this.toolTip = new System.Windows.Forms.ToolTip(this.components);
			((System.ComponentModel.ISupportInitialize)(this.dllFileWatcher)).BeginInit();
			this.panel_01.SuspendLayout();
			this.sectionPanel_Installed.SuspendLayout();
			this.sectionPanel_Available.SuspendLayout();
			this.toolStrip.SuspendLayout();
			this.panel_02.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.pluginFolderWatcher)).BeginInit();
			this.SuspendLayout();
			// 
			// button_Close
			// 
			this.button_Close.Location = new System.Drawing.Point(646, 5);
			this.button_Close.Margin = new System.Windows.Forms.Padding(3, 3, 0, 6);
			this.button_Close.Name = "button_Close";
			this.button_Close.Size = new System.Drawing.Size(128, 28);
			this.button_Close.TabIndex = 0;
			this.button_Close.Text = "Close";
			// 
			// button_Delete
			// 
			this.button_Delete.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
			this.button_Delete.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.button_Delete.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
			this.button_Delete.Image = global::TombIDE.ProjectMaster.Properties.Resources.general_trash_16;
			this.button_Delete.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.button_Delete.Name = "button_Delete";
			this.button_Delete.Size = new System.Drawing.Size(23, 25);
			this.button_Delete.Text = "Delete Plugin from TombIDE";
			// 
			// button_Download
			// 
			this.button_Download.Location = new System.Drawing.Point(8, 5);
			this.button_Download.Margin = new System.Windows.Forms.Padding(0, 3, 0, 6);
			this.button_Download.Name = "button_Download";
			this.button_Download.Size = new System.Drawing.Size(128, 28);
			this.button_Download.TabIndex = 2;
			this.button_Download.Text = "Download Plugins";
			this.button_Download.Click += new System.EventHandler(this.button_Download_Click);
			// 
			// button_Install
			// 
			this.button_Install.Anchor = System.Windows.Forms.AnchorStyles.None;
			this.button_Install.Location = new System.Drawing.Point(376, 8);
			this.button_Install.Margin = new System.Windows.Forms.Padding(3, 0, 3, 3);
			this.button_Install.Name = "button_Install";
			this.button_Install.Size = new System.Drawing.Size(30, 248);
			this.button_Install.TabIndex = 2;
			this.button_Install.Text = ">>";
			this.toolTip.SetToolTip(this.button_Install, "Install Plugin to the Current Project");
			this.button_Install.Click += new System.EventHandler(this.button_Install_Click);
			// 
			// button_OpenArchive
			// 
			this.button_OpenArchive.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
			this.button_OpenArchive.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.button_OpenArchive.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
			this.button_OpenArchive.Image = global::TombIDE.ProjectMaster.Properties.Resources.general_Import_16;
			this.button_OpenArchive.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.button_OpenArchive.Name = "button_OpenArchive";
			this.button_OpenArchive.Size = new System.Drawing.Size(23, 25);
			this.button_OpenArchive.Text = "Add Plugin from Archive";
			this.button_OpenArchive.Click += new System.EventHandler(this.button_OpenArchive_Click);
			// 
			// button_OpenFolder
			// 
			this.button_OpenFolder.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
			this.button_OpenFolder.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.button_OpenFolder.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
			this.button_OpenFolder.Image = global::TombIDE.ProjectMaster.Properties.Resources.general_Open_16;
			this.button_OpenFolder.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.button_OpenFolder.Name = "button_OpenFolder";
			this.button_OpenFolder.Size = new System.Drawing.Size(23, 25);
			this.button_OpenFolder.Text = "Add Plugin from Folder";
			this.button_OpenFolder.Click += new System.EventHandler(this.Button_OpenFolder_Click);
			// 
			// button_OpenInExplorer
			// 
			this.button_OpenInExplorer.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
			this.button_OpenInExplorer.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.button_OpenInExplorer.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
			this.button_OpenInExplorer.Image = global::TombIDE.ProjectMaster.Properties.Resources.forward_arrow_16;
			this.button_OpenInExplorer.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.button_OpenInExplorer.Name = "button_OpenInExplorer";
			this.button_OpenInExplorer.Size = new System.Drawing.Size(23, 25);
			this.button_OpenInExplorer.Text = "Open Plugins Folder in Explorer";
			// 
			// button_Refresh
			// 
			this.button_Refresh.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
			this.button_Refresh.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
			this.button_Refresh.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.button_Refresh.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
			this.button_Refresh.Image = global::TombIDE.ProjectMaster.Properties.Resources.actions_refresh_16;
			this.button_Refresh.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.button_Refresh.Name = "button_Refresh";
			this.button_Refresh.Size = new System.Drawing.Size(23, 25);
			this.button_Refresh.Text = "Refresh List";
			// 
			// button_Uninstall
			// 
			this.button_Uninstall.Anchor = System.Windows.Forms.AnchorStyles.None;
			this.button_Uninstall.Location = new System.Drawing.Point(376, 262);
			this.button_Uninstall.Margin = new System.Windows.Forms.Padding(3, 3, 3, 6);
			this.button_Uninstall.Name = "button_Uninstall";
			this.button_Uninstall.Size = new System.Drawing.Size(30, 248);
			this.button_Uninstall.TabIndex = 3;
			this.button_Uninstall.Text = "<<";
			this.toolTip.SetToolTip(this.button_Uninstall, "Uninstall Plugin from the Current Project");
			this.button_Uninstall.Click += new System.EventHandler(this.button_Uninstall_Click);
			// 
			// dllFileWatcher
			// 
			this.dllFileWatcher.EnableRaisingEvents = true;
			this.dllFileWatcher.Filter = "*.dll";
			this.dllFileWatcher.IncludeSubdirectories = true;
			this.dllFileWatcher.SynchronizingObject = this;
			this.dllFileWatcher.Deleted += new System.IO.FileSystemEventHandler(this.dllFileWatcher_Deleted);
			// 
			// panel_01
			// 
			this.panel_01.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.panel_01.Controls.Add(this.button_Uninstall);
			this.panel_01.Controls.Add(this.button_Install);
			this.panel_01.Controls.Add(this.sectionPanel_Installed);
			this.panel_01.Controls.Add(this.sectionPanel_Available);
			this.panel_01.Dock = System.Windows.Forms.DockStyle.Fill;
			this.panel_01.Location = new System.Drawing.Point(0, 0);
			this.panel_01.Name = "panel_01";
			this.panel_01.Size = new System.Drawing.Size(784, 520);
			this.panel_01.TabIndex = 0;
			// 
			// sectionPanel_Installed
			// 
			this.sectionPanel_Installed.Controls.Add(this.treeView_Installed);
			this.sectionPanel_Installed.Dock = System.Windows.Forms.DockStyle.Right;
			this.sectionPanel_Installed.Location = new System.Drawing.Point(412, 0);
			this.sectionPanel_Installed.Name = "sectionPanel_Installed";
			this.sectionPanel_Installed.SectionHeader = "Installed Plugins (Current Project)";
			this.sectionPanel_Installed.Size = new System.Drawing.Size(370, 518);
			this.sectionPanel_Installed.TabIndex = 1;
			// 
			// treeView_Installed
			// 
			this.treeView_Installed.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(48)))), ((int)(((byte)(48)))), ((int)(((byte)(48)))));
			this.treeView_Installed.Dock = System.Windows.Forms.DockStyle.Fill;
			this.treeView_Installed.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
			this.treeView_Installed.ItemHeight = 40;
			this.treeView_Installed.Location = new System.Drawing.Point(1, 25);
			this.treeView_Installed.MaxDragChange = 40;
			this.treeView_Installed.MultiSelect = true;
			this.treeView_Installed.Name = "treeView_Installed";
			this.treeView_Installed.Size = new System.Drawing.Size(368, 492);
			this.treeView_Installed.TabIndex = 0;
			// 
			// sectionPanel_Available
			// 
			this.sectionPanel_Available.Controls.Add(this.treeView_AvailablePlugins);
			this.sectionPanel_Available.Controls.Add(this.toolStrip);
			this.sectionPanel_Available.Dock = System.Windows.Forms.DockStyle.Left;
			this.sectionPanel_Available.Location = new System.Drawing.Point(0, 0);
			this.sectionPanel_Available.Name = "sectionPanel_Available";
			this.sectionPanel_Available.SectionHeader = "Available Plugins (TombIDE)";
			this.sectionPanel_Available.Size = new System.Drawing.Size(370, 518);
			this.sectionPanel_Available.TabIndex = 0;
			// 
			// treeView_AvailablePlugins
			// 
			this.treeView_AvailablePlugins.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(48)))), ((int)(((byte)(48)))), ((int)(((byte)(48)))));
			this.treeView_AvailablePlugins.Dock = System.Windows.Forms.DockStyle.Fill;
			this.treeView_AvailablePlugins.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
			this.treeView_AvailablePlugins.ItemHeight = 40;
			this.treeView_AvailablePlugins.Location = new System.Drawing.Point(1, 53);
			this.treeView_AvailablePlugins.MaxDragChange = 40;
			this.treeView_AvailablePlugins.MultiSelect = true;
			this.treeView_AvailablePlugins.Name = "treeView_AvailablePlugins";
			this.treeView_AvailablePlugins.Size = new System.Drawing.Size(368, 464);
			this.treeView_AvailablePlugins.TabIndex = 0;
			// 
			// toolStrip
			// 
			this.toolStrip.AutoSize = false;
			this.toolStrip.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
			this.toolStrip.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
			this.toolStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.button_OpenArchive,
            this.button_OpenFolder,
            this.separator_01,
            this.button_Delete,
            this.separator_02,
            this.button_OpenInExplorer,
            this.button_Refresh});
			this.toolStrip.Location = new System.Drawing.Point(1, 25);
			this.toolStrip.Name = "toolStrip";
			this.toolStrip.Padding = new System.Windows.Forms.Padding(5, 0, 1, 0);
			this.toolStrip.Size = new System.Drawing.Size(368, 28);
			this.toolStrip.TabIndex = 1;
			// 
			// separator_01
			// 
			this.separator_01.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
			this.separator_01.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
			this.separator_01.Margin = new System.Windows.Forms.Padding(0, 0, 2, 0);
			this.separator_01.Name = "separator_01";
			this.separator_01.Size = new System.Drawing.Size(6, 28);
			// 
			// separator_02
			// 
			this.separator_02.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
			this.separator_02.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
			this.separator_02.Margin = new System.Windows.Forms.Padding(0, 0, 2, 0);
			this.separator_02.Name = "separator_02";
			this.separator_02.Size = new System.Drawing.Size(6, 28);
			// 
			// panel_02
			// 
			this.panel_02.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.panel_02.Controls.Add(this.button_Download);
			this.panel_02.Controls.Add(this.button_Close);
			this.panel_02.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.panel_02.Location = new System.Drawing.Point(0, 520);
			this.panel_02.Name = "panel_02";
			this.panel_02.Size = new System.Drawing.Size(784, 41);
			this.panel_02.TabIndex = 1;
			// 
			// pluginFolderWatcher
			// 
			this.pluginFolderWatcher.EnableRaisingEvents = true;
			this.pluginFolderWatcher.NotifyFilter = System.IO.NotifyFilters.DirectoryName;
			this.pluginFolderWatcher.SynchronizingObject = this;
			// 
			// FormPluginLibrary
			// 
			this.AcceptButton = this.button_Close;
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(784, 561);
			this.Controls.Add(this.panel_01);
			this.Controls.Add(this.panel_02);
			this.FlatBorder = true;
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
			this.MaximizeBox = false;
			this.Name = "FormPluginLibrary";
			this.ShowIcon = false;
			this.ShowInTaskbar = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "Install Plugin from Library";
			((System.ComponentModel.ISupportInitialize)(this.dllFileWatcher)).EndInit();
			this.panel_01.ResumeLayout(false);
			this.sectionPanel_Installed.ResumeLayout(false);
			this.sectionPanel_Available.ResumeLayout(false);
			this.toolStrip.ResumeLayout(false);
			this.toolStrip.PerformLayout();
			this.panel_02.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.pluginFolderWatcher)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

		private DarkUI.Controls.DarkButton button_Close;
		private DarkUI.Controls.DarkButton button_Download;
		private DarkUI.Controls.DarkButton button_Install;
		private DarkUI.Controls.DarkButton button_Uninstall;
		private DarkUI.Controls.DarkSectionPanel sectionPanel_Available;
		private DarkUI.Controls.DarkSectionPanel sectionPanel_Installed;
		private DarkUI.Controls.DarkToolStrip toolStrip;
		private DarkUI.Controls.DarkTreeView treeView_AvailablePlugins;
		private DarkUI.Controls.DarkTreeView treeView_Installed;
		private System.IO.FileSystemWatcher dllFileWatcher;
		private System.IO.FileSystemWatcher pluginFolderWatcher;
		private System.Windows.Forms.Panel panel_01;
		private System.Windows.Forms.Panel panel_02;
		private System.Windows.Forms.ToolStripButton button_Delete;
		private System.Windows.Forms.ToolStripButton button_OpenArchive;
		private System.Windows.Forms.ToolStripButton button_OpenFolder;
		private System.Windows.Forms.ToolStripButton button_OpenInExplorer;
		private System.Windows.Forms.ToolStripButton button_Refresh;
		private System.Windows.Forms.ToolStripSeparator separator_01;
		private System.Windows.Forms.ToolStripSeparator separator_02;
		private System.Windows.Forms.ToolTip toolTip;
	}
}