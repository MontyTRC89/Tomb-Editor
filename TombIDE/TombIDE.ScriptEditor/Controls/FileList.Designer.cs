namespace TombIDE.ScriptEditor.Controls
{
	partial class FileList
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
			this.button_EditScript = new DarkUI.Controls.DarkButton();
			this.button_EditStrings = new DarkUI.Controls.DarkButton();
			this.button_OpenInExplorer = new DarkUI.Controls.DarkButton();
			this.scriptFolderWatcher = new System.IO.FileSystemWatcher();
			this.sectionPanel = new DarkUI.Controls.DarkSectionPanel();
			this.treeView = new DarkUI.Controls.DarkTreeView();
			((System.ComponentModel.ISupportInitialize)(this.scriptFolderWatcher)).BeginInit();
			this.sectionPanel.SuspendLayout();
			this.SuspendLayout();
			// 
			// button_EditScript
			// 
			this.button_EditScript.Location = new System.Drawing.Point(4, 28);
			this.button_EditScript.Margin = new System.Windows.Forms.Padding(3, 3, 0, 0);
			this.button_EditScript.Name = "button_EditScript";
			this.button_EditScript.Size = new System.Drawing.Size(94, 23);
			this.button_EditScript.TabIndex = 0;
			this.button_EditScript.Text = "Edit Script File";
			this.button_EditScript.Click += new System.EventHandler(this.button_EditScript_Click);
			// 
			// button_EditStrings
			// 
			this.button_EditStrings.Location = new System.Drawing.Point(100, 28);
			this.button_EditStrings.Margin = new System.Windows.Forms.Padding(2, 3, 3, 0);
			this.button_EditStrings.Name = "button_EditStrings";
			this.button_EditStrings.Size = new System.Drawing.Size(94, 23);
			this.button_EditStrings.TabIndex = 1;
			this.button_EditStrings.Text = "Edit Strings";
			this.button_EditStrings.Click += new System.EventHandler(this.button_EditStrings_Click);
			// 
			// button_OpenInExplorer
			// 
			this.button_OpenInExplorer.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.button_OpenInExplorer.Location = new System.Drawing.Point(4, 286);
			this.button_OpenInExplorer.Margin = new System.Windows.Forms.Padding(3, 0, 3, 3);
			this.button_OpenInExplorer.Name = "button_OpenInExplorer";
			this.button_OpenInExplorer.Size = new System.Drawing.Size(190, 23);
			this.button_OpenInExplorer.TabIndex = 3;
			this.button_OpenInExplorer.Text = "Open in File Explorer";
			this.button_OpenInExplorer.Click += new System.EventHandler(this.button_OpenInExplorer_Click);
			// 
			// scriptFolderWatcher
			// 
			this.scriptFolderWatcher.EnableRaisingEvents = true;
			this.scriptFolderWatcher.IncludeSubdirectories = true;
			this.scriptFolderWatcher.SynchronizingObject = this;
			this.scriptFolderWatcher.Changed += new System.IO.FileSystemEventHandler(this.FolderWatcher_Changed);
			this.scriptFolderWatcher.Created += new System.IO.FileSystemEventHandler(this.FolderWatcher_Changed);
			this.scriptFolderWatcher.Deleted += new System.IO.FileSystemEventHandler(this.FolderWatcher_Changed);
			this.scriptFolderWatcher.Renamed += new System.IO.RenamedEventHandler(this.FolderWatcher_Renamed);
			// 
			// sectionPanel
			// 
			this.sectionPanel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.sectionPanel.Controls.Add(this.button_OpenInExplorer);
			this.sectionPanel.Controls.Add(this.button_EditStrings);
			this.sectionPanel.Controls.Add(this.button_EditScript);
			this.sectionPanel.Controls.Add(this.treeView);
			this.sectionPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.sectionPanel.Location = new System.Drawing.Point(0, 0);
			this.sectionPanel.Name = "sectionPanel";
			this.sectionPanel.SectionHeader = "File List";
			this.sectionPanel.Size = new System.Drawing.Size(200, 315);
			this.sectionPanel.TabIndex = 0;
			this.sectionPanel.Text = "Project Explorer";
			this.sectionPanel.Resize += new System.EventHandler(this.sectionPanel_Resize);
			// 
			// treeView
			// 
			this.treeView.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
			| System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.treeView.Location = new System.Drawing.Point(4, 54);
			this.treeView.MaxDragChange = 20;
			this.treeView.Name = "treeView";
			this.treeView.ShowIcons = true;
			this.treeView.Size = new System.Drawing.Size(190, 229);
			this.treeView.TabIndex = 2;
			this.treeView.DoubleClick += new System.EventHandler(this.treeView_DoubleClick);
			// 
			// FileList
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
			this.Controls.Add(this.sectionPanel);
			this.Name = "FileList";
			this.Size = new System.Drawing.Size(200, 315);
			((System.ComponentModel.ISupportInitialize)(this.scriptFolderWatcher)).EndInit();
			this.sectionPanel.ResumeLayout(false);
			this.ResumeLayout(false);

		}

		#endregion

		private DarkUI.Controls.DarkButton button_EditScript;
		private DarkUI.Controls.DarkButton button_EditStrings;
		private DarkUI.Controls.DarkButton button_OpenInExplorer;
		private DarkUI.Controls.DarkSectionPanel sectionPanel;
		private DarkUI.Controls.DarkTreeView treeView;
		private System.IO.FileSystemWatcher scriptFolderWatcher;
	}
}
