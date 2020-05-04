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
			this.contextMenu = new DarkUI.Controls.DarkContextMenu();
			this.menuItem_NewFile = new System.Windows.Forms.ToolStripMenuItem();
			this.menuItem_NewFolder = new System.Windows.Forms.ToolStripMenuItem();
			this.separator_01 = new System.Windows.Forms.ToolStripSeparator();
			this.menuItem_OpenInEditor = new System.Windows.Forms.ToolStripMenuItem();
			this.separator_02 = new System.Windows.Forms.ToolStripSeparator();
			this.menuItem_Rename = new System.Windows.Forms.ToolStripMenuItem();
			this.menuItem_Delete = new System.Windows.Forms.ToolStripMenuItem();
			this.separator_03 = new System.Windows.Forms.ToolStripSeparator();
			this.menuItem_OpenInExplorer = new System.Windows.Forms.ToolStripMenuItem();
			this.sectionPanel = new DarkUI.Controls.DarkSectionPanel();
			this.treeView = new DarkUI.Controls.DarkTreeView();
			this.contextMenu.SuspendLayout();
			this.sectionPanel.SuspendLayout();
			this.SuspendLayout();
			// 
			// button_EditScript
			// 
			this.button_EditScript.Checked = false;
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
			this.button_EditStrings.Checked = false;
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
			this.button_OpenInExplorer.Checked = false;
			this.button_OpenInExplorer.Location = new System.Drawing.Point(4, 286);
			this.button_OpenInExplorer.Margin = new System.Windows.Forms.Padding(3, 0, 3, 3);
			this.button_OpenInExplorer.Name = "button_OpenInExplorer";
			this.button_OpenInExplorer.Size = new System.Drawing.Size(190, 23);
			this.button_OpenInExplorer.TabIndex = 3;
			this.button_OpenInExplorer.Text = "Open in File Explorer";
			this.button_OpenInExplorer.Click += new System.EventHandler(this.button_OpenInExplorer_Click);
			// 
			// contextMenu
			// 
			this.contextMenu.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
			this.contextMenu.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
			this.contextMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.menuItem_NewFile,
            this.menuItem_NewFolder,
            this.separator_01,
            this.menuItem_OpenInEditor,
            this.separator_02,
            this.menuItem_Rename,
            this.menuItem_Delete,
            this.separator_03,
            this.menuItem_OpenInExplorer});
			this.contextMenu.Name = "contextMenu";
			this.contextMenu.Size = new System.Drawing.Size(237, 157);
			// 
			// menuItem_NewFile
			// 
			this.menuItem_NewFile.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
			this.menuItem_NewFile.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
			this.menuItem_NewFile.Image = global::TombIDE.ScriptEditor.Properties.Resources.general_create_new_16;
			this.menuItem_NewFile.Name = "menuItem_NewFile";
			this.menuItem_NewFile.Size = new System.Drawing.Size(236, 22);
			this.menuItem_NewFile.Text = "New File...";
			this.menuItem_NewFile.Click += new System.EventHandler(this.menuItem_NewFile_Click);
			// 
			// menuItem_NewFolder
			// 
			this.menuItem_NewFolder.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
			this.menuItem_NewFolder.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
			this.menuItem_NewFolder.Image = global::TombIDE.ScriptEditor.Properties.Resources.general_folder_16;
			this.menuItem_NewFolder.Name = "menuItem_NewFolder";
			this.menuItem_NewFolder.Size = new System.Drawing.Size(236, 22);
			this.menuItem_NewFolder.Text = "New Folder...";
			this.menuItem_NewFolder.Click += new System.EventHandler(this.menuItem_NewFolder_Click);
			// 
			// separator_01
			// 
			this.separator_01.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
			this.separator_01.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
			this.separator_01.Margin = new System.Windows.Forms.Padding(0, 0, 0, 1);
			this.separator_01.Name = "separator_01";
			this.separator_01.Size = new System.Drawing.Size(233, 6);
			// 
			// menuItem_OpenInEditor
			// 
			this.menuItem_OpenInEditor.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
			this.menuItem_OpenInEditor.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
			this.menuItem_OpenInEditor.Name = "menuItem_OpenInEditor";
			this.menuItem_OpenInEditor.Size = new System.Drawing.Size(236, 22);
			this.menuItem_OpenInEditor.Text = "Open Selected File in Editor";
			this.menuItem_OpenInEditor.Click += new System.EventHandler(this.menuItem_OpenInEditor_Click);
			// 
			// separator_02
			// 
			this.separator_02.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
			this.separator_02.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
			this.separator_02.Margin = new System.Windows.Forms.Padding(0, 0, 0, 1);
			this.separator_02.Name = "separator_02";
			this.separator_02.Size = new System.Drawing.Size(233, 6);
			// 
			// menuItem_Rename
			// 
			this.menuItem_Rename.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
			this.menuItem_Rename.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
			this.menuItem_Rename.Image = global::TombIDE.ScriptEditor.Properties.Resources.general_edit_16;
			this.menuItem_Rename.Name = "menuItem_Rename";
			this.menuItem_Rename.Size = new System.Drawing.Size(236, 22);
			this.menuItem_Rename.Text = "Rename Selected Item";
			this.menuItem_Rename.Click += new System.EventHandler(this.menuItem_Rename_Click);
			// 
			// menuItem_Delete
			// 
			this.menuItem_Delete.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
			this.menuItem_Delete.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
			this.menuItem_Delete.Image = global::TombIDE.ScriptEditor.Properties.Resources.general_trash_16;
			this.menuItem_Delete.Name = "menuItem_Delete";
			this.menuItem_Delete.Size = new System.Drawing.Size(236, 22);
			this.menuItem_Delete.Text = "Delete Selected Item";
			this.menuItem_Delete.Click += new System.EventHandler(this.menuItem_Delete_Click);
			// 
			// separator_03
			// 
			this.separator_03.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
			this.separator_03.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
			this.separator_03.Margin = new System.Windows.Forms.Padding(0, 0, 0, 1);
			this.separator_03.Name = "separator_03";
			this.separator_03.Size = new System.Drawing.Size(233, 6);
			// 
			// menuItem_OpenInExplorer
			// 
			this.menuItem_OpenInExplorer.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
			this.menuItem_OpenInExplorer.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
			this.menuItem_OpenInExplorer.Image = global::TombIDE.ScriptEditor.Properties.Resources.forward_arrow_16;
			this.menuItem_OpenInExplorer.Name = "menuItem_OpenInExplorer";
			this.menuItem_OpenInExplorer.Size = new System.Drawing.Size(236, 22);
			this.menuItem_OpenInExplorer.Text = "Open Selected Item in Explorer";
			this.menuItem_OpenInExplorer.Click += new System.EventHandler(this.menuItem_OpenInExplorer_Click);
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
			this.treeView.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(48)))), ((int)(((byte)(48)))), ((int)(((byte)(48)))));
			this.treeView.ContextMenuStrip = this.contextMenu;
			this.treeView.EvenNodeColor = System.Drawing.Color.FromArgb(((int)(((byte)(52)))), ((int)(((byte)(52)))), ((int)(((byte)(52)))));
			this.treeView.FocusedNodeColor = System.Drawing.Color.FromArgb(((int)(((byte)(75)))), ((int)(((byte)(110)))), ((int)(((byte)(175)))));
			this.treeView.Location = new System.Drawing.Point(4, 54);
			this.treeView.MaxDragChange = 20;
			this.treeView.Name = "treeView";
			this.treeView.NonFocusedNodeColor = System.Drawing.Color.FromArgb(((int)(((byte)(92)))), ((int)(((byte)(92)))), ((int)(((byte)(92)))));
			this.treeView.OddNodeColor = System.Drawing.Color.FromArgb(((int)(((byte)(48)))), ((int)(((byte)(48)))), ((int)(((byte)(48)))));
			this.treeView.ShowIcons = true;
			this.treeView.Size = new System.Drawing.Size(190, 229);
			this.treeView.TabIndex = 2;
			this.treeView.SelectedNodesChanged += new System.EventHandler(this.treeView_SelectedNodesChanged);
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
			this.contextMenu.ResumeLayout(false);
			this.sectionPanel.ResumeLayout(false);
			this.ResumeLayout(false);

		}

		#endregion

		private DarkUI.Controls.DarkButton button_EditScript;
		private DarkUI.Controls.DarkButton button_EditStrings;
		private DarkUI.Controls.DarkButton button_OpenInExplorer;
		private DarkUI.Controls.DarkContextMenu contextMenu;
		private DarkUI.Controls.DarkSectionPanel sectionPanel;
		private DarkUI.Controls.DarkTreeView treeView;
		private System.Windows.Forms.ToolStripMenuItem menuItem_Delete;
		private System.Windows.Forms.ToolStripMenuItem menuItem_NewFile;
		private System.Windows.Forms.ToolStripMenuItem menuItem_NewFolder;
		private System.Windows.Forms.ToolStripMenuItem menuItem_OpenInEditor;
		private System.Windows.Forms.ToolStripMenuItem menuItem_OpenInExplorer;
		private System.Windows.Forms.ToolStripMenuItem menuItem_Rename;
		private System.Windows.Forms.ToolStripSeparator separator_01;
		private System.Windows.Forms.ToolStripSeparator separator_02;
		private System.Windows.Forms.ToolStripSeparator separator_03;
	}
}
