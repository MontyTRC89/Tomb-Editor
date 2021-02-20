namespace TombIDE.ScriptEditor.ToolWindows
{
	partial class FileExplorer
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
			this.contextMenu = new DarkUI.Controls.DarkContextMenu();
			this.menuItem_NewFile = new System.Windows.Forms.ToolStripMenuItem();
			this.menuItem_NewFolder = new System.Windows.Forms.ToolStripMenuItem();
			this.separator_01 = new System.Windows.Forms.ToolStripSeparator();
			this.menuItem_ViewCode = new System.Windows.Forms.ToolStripMenuItem();
			this.menuItem_ViewInEditor = new System.Windows.Forms.ToolStripMenuItem();
			this.separator_02 = new System.Windows.Forms.ToolStripSeparator();
			this.menuItem_Rename = new System.Windows.Forms.ToolStripMenuItem();
			this.menuItem_Delete = new System.Windows.Forms.ToolStripMenuItem();
			this.separator_03 = new System.Windows.Forms.ToolStripSeparator();
			this.menuItem_OpenInExplorer = new System.Windows.Forms.ToolStripMenuItem();
			this.fileSystemWatcher = new System.IO.FileSystemWatcher();
			this.folderWatcher = new System.IO.FileSystemWatcher();
			this.treeView = new DarkUI.Controls.DarkTreeView();
			this.contextMenu.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.fileSystemWatcher)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.folderWatcher)).BeginInit();
			this.SuspendLayout();
			// 
			// contextMenu
			// 
			this.contextMenu.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
			this.contextMenu.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
			this.contextMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.menuItem_NewFile,
            this.menuItem_NewFolder,
            this.separator_01,
            this.menuItem_ViewInEditor,
            this.menuItem_ViewCode,
            this.separator_02,
            this.menuItem_Rename,
            this.menuItem_Delete,
            this.separator_03,
            this.menuItem_OpenInExplorer});
			this.contextMenu.Name = "contextMenu";
			this.contextMenu.Size = new System.Drawing.Size(190, 201);
			// 
			// menuItem_NewFile
			// 
			this.menuItem_NewFile.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
			this.menuItem_NewFile.ForeColor = System.Drawing.Color.Gainsboro;
			this.menuItem_NewFile.Image = global::TombIDE.ScriptEditor.Properties.Resources.New_16;
			this.menuItem_NewFile.Name = "menuItem_NewFile";
			this.menuItem_NewFile.Size = new System.Drawing.Size(189, 22);
			this.menuItem_NewFile.Text = "New File...";
			this.menuItem_NewFile.Click += new System.EventHandler(this.menuItem_NewFile_Click);
			// 
			// menuItem_NewFolder
			// 
			this.menuItem_NewFolder.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
			this.menuItem_NewFolder.ForeColor = System.Drawing.Color.Gainsboro;
			this.menuItem_NewFolder.Image = global::TombIDE.ScriptEditor.Properties.Resources.Folder_16;
			this.menuItem_NewFolder.Name = "menuItem_NewFolder";
			this.menuItem_NewFolder.Size = new System.Drawing.Size(189, 22);
			this.menuItem_NewFolder.Text = "New Folder...";
			this.menuItem_NewFolder.Click += new System.EventHandler(this.menuItem_NewFolder_Click);
			// 
			// separator_01
			// 
			this.separator_01.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
			this.separator_01.ForeColor = System.Drawing.Color.Gainsboro;
			this.separator_01.Margin = new System.Windows.Forms.Padding(0, 0, 0, 1);
			this.separator_01.Name = "separator_01";
			this.separator_01.Size = new System.Drawing.Size(186, 6);
			// 
			// menuItem_ViewCode
			// 
			this.menuItem_ViewCode.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
			this.menuItem_ViewCode.Enabled = false;
			this.menuItem_ViewCode.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(153)))), ((int)(((byte)(153)))), ((int)(((byte)(153)))));
			this.menuItem_ViewCode.Name = "menuItem_ViewCode";
			this.menuItem_ViewCode.Size = new System.Drawing.Size(189, 22);
			this.menuItem_ViewCode.Text = "View Code";
			this.menuItem_ViewCode.Click += new System.EventHandler(this.menuItem_ViewCode_Click);
			// 
			// menuItem_ViewInEditor
			// 
			this.menuItem_ViewInEditor.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
			this.menuItem_ViewInEditor.Enabled = false;
			this.menuItem_ViewInEditor.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(153)))), ((int)(((byte)(153)))), ((int)(((byte)(153)))));
			this.menuItem_ViewInEditor.Name = "menuItem_ViewInEditor";
			this.menuItem_ViewInEditor.Size = new System.Drawing.Size(189, 22);
			this.menuItem_ViewInEditor.Text = "View in Default Editor";
			this.menuItem_ViewInEditor.Click += new System.EventHandler(this.menuItem_ViewInEditor_Click);
			// 
			// separator_02
			// 
			this.separator_02.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
			this.separator_02.ForeColor = System.Drawing.Color.Gainsboro;
			this.separator_02.Margin = new System.Windows.Forms.Padding(0, 0, 0, 1);
			this.separator_02.Name = "separator_02";
			this.separator_02.Size = new System.Drawing.Size(186, 6);
			// 
			// menuItem_Rename
			// 
			this.menuItem_Rename.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
			this.menuItem_Rename.Enabled = false;
			this.menuItem_Rename.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(153)))), ((int)(((byte)(153)))), ((int)(((byte)(153)))));
			this.menuItem_Rename.Image = global::TombIDE.ScriptEditor.Properties.Resources.Edit_16;
			this.menuItem_Rename.Name = "menuItem_Rename";
			this.menuItem_Rename.Size = new System.Drawing.Size(189, 22);
			this.menuItem_Rename.Text = "Rename Item...";
			this.menuItem_Rename.Click += new System.EventHandler(this.menuItem_Rename_Click);
			// 
			// menuItem_Delete
			// 
			this.menuItem_Delete.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
			this.menuItem_Delete.Enabled = false;
			this.menuItem_Delete.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(153)))), ((int)(((byte)(153)))), ((int)(((byte)(153)))));
			this.menuItem_Delete.Image = global::TombIDE.ScriptEditor.Properties.Resources.Trash_16;
			this.menuItem_Delete.Name = "menuItem_Delete";
			this.menuItem_Delete.Size = new System.Drawing.Size(189, 22);
			this.menuItem_Delete.Text = "Delete Item";
			this.menuItem_Delete.Click += new System.EventHandler(this.menuItem_Delete_Click);
			// 
			// separator_03
			// 
			this.separator_03.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
			this.separator_03.ForeColor = System.Drawing.Color.Gainsboro;
			this.separator_03.Margin = new System.Windows.Forms.Padding(0, 0, 0, 1);
			this.separator_03.Name = "separator_03";
			this.separator_03.Size = new System.Drawing.Size(186, 6);
			// 
			// menuItem_OpenInExplorer
			// 
			this.menuItem_OpenInExplorer.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
			this.menuItem_OpenInExplorer.Enabled = false;
			this.menuItem_OpenInExplorer.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(153)))), ((int)(((byte)(153)))), ((int)(((byte)(153)))));
			this.menuItem_OpenInExplorer.Image = global::TombIDE.ScriptEditor.Properties.Resources.ForwardArrow_16;
			this.menuItem_OpenInExplorer.Name = "menuItem_OpenInExplorer";
			this.menuItem_OpenInExplorer.Size = new System.Drawing.Size(189, 22);
			this.menuItem_OpenInExplorer.Text = "Open Item in Explorer";
			this.menuItem_OpenInExplorer.Click += new System.EventHandler(this.menuItem_OpenInExplorer_Click);
			// 
			// fileSystemWatcher
			// 
			this.fileSystemWatcher.EnableRaisingEvents = true;
			this.fileSystemWatcher.IncludeSubdirectories = true;
			this.fileSystemWatcher.SynchronizingObject = this;
			this.fileSystemWatcher.Changed += new System.IO.FileSystemEventHandler(this.OnFileChanged);
			this.fileSystemWatcher.Created += new System.IO.FileSystemEventHandler(this.OnFileCreated);
			this.fileSystemWatcher.Deleted += new System.IO.FileSystemEventHandler(this.OnFileDeleted);
			this.fileSystemWatcher.Renamed += new System.IO.RenamedEventHandler(this.OnFileRenamed);
			// 
			// folderWatcher
			// 
			this.folderWatcher.EnableRaisingEvents = true;
			this.folderWatcher.IncludeSubdirectories = true;
			this.folderWatcher.NotifyFilter = System.IO.NotifyFilters.DirectoryName;
			this.folderWatcher.SynchronizingObject = this;
			this.folderWatcher.Changed += new System.IO.FileSystemEventHandler(this.OnFileChanged);
			this.folderWatcher.Created += new System.IO.FileSystemEventHandler(this.OnFileCreated);
			this.folderWatcher.Deleted += new System.IO.FileSystemEventHandler(this.OnFileDeleted);
			this.folderWatcher.Renamed += new System.IO.RenamedEventHandler(this.OnFileRenamed);
			// 
			// treeView
			// 
			this.treeView.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.treeView.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(48)))), ((int)(((byte)(48)))), ((int)(((byte)(48)))));
			this.treeView.ContextMenuStrip = this.contextMenu;
			this.treeView.EvenNodeColor = System.Drawing.Color.FromArgb(((int)(((byte)(52)))), ((int)(((byte)(52)))), ((int)(((byte)(52)))));
			this.treeView.FocusedNodeColor = System.Drawing.Color.FromArgb(((int)(((byte)(75)))), ((int)(((byte)(110)))), ((int)(((byte)(175)))));
			this.treeView.Location = new System.Drawing.Point(3, 28);
			this.treeView.MaxDragChange = 20;
			this.treeView.Name = "treeView";
			this.treeView.NonFocusedNodeColor = System.Drawing.Color.FromArgb(((int)(((byte)(92)))), ((int)(((byte)(92)))), ((int)(((byte)(92)))));
			this.treeView.OddNodeColor = System.Drawing.Color.FromArgb(((int)(((byte)(48)))), ((int)(((byte)(48)))), ((int)(((byte)(48)))));
			this.treeView.ShowIcons = true;
			this.treeView.Size = new System.Drawing.Size(219, 194);
			this.treeView.TabIndex = 0;
			this.treeView.SelectedNodesChanged += new System.EventHandler(this.treeView_SelectedNodesChanged);
			this.treeView.DoubleClick += new System.EventHandler(this.treeView_DoubleClick);
			// 
			// FileExplorer
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
			this.Controls.Add(this.treeView);
			this.DockText = "File Explorer";
			this.MinimumSize = new System.Drawing.Size(225, 225);
			this.Name = "FileExplorer";
			this.Size = new System.Drawing.Size(225, 225);
			this.contextMenu.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.fileSystemWatcher)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.folderWatcher)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

		private DarkUI.Controls.DarkContextMenu contextMenu;
		private DarkUI.Controls.DarkTreeView treeView;
		private System.IO.FileSystemWatcher fileSystemWatcher;
		private System.IO.FileSystemWatcher folderWatcher;
		private System.Windows.Forms.ToolStripMenuItem menuItem_Delete;
		private System.Windows.Forms.ToolStripMenuItem menuItem_NewFile;
		private System.Windows.Forms.ToolStripMenuItem menuItem_NewFolder;
		private System.Windows.Forms.ToolStripMenuItem menuItem_ViewInEditor;
		private System.Windows.Forms.ToolStripMenuItem menuItem_OpenInExplorer;
		private System.Windows.Forms.ToolStripMenuItem menuItem_Rename;
		private System.Windows.Forms.ToolStripSeparator separator_01;
		private System.Windows.Forms.ToolStripSeparator separator_02;
		private System.Windows.Forms.ToolStripSeparator separator_03;
		private System.Windows.Forms.ToolStripMenuItem menuItem_ViewCode;
	}
}
