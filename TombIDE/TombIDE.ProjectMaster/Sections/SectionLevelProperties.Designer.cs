namespace TombIDE.ProjectMaster
{
	partial class SectionLevelProperties
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
			this.checkBox_ShowAllFiles = new DarkUI.Controls.DarkCheckBox();
			this.contextMenu = new DarkUI.Controls.DarkContextMenu();
			this.menuItem_Open = new System.Windows.Forms.ToolStripMenuItem();
			this.menuItem_OpenFolder = new System.Windows.Forms.ToolStripMenuItem();
			this.separator_01 = new System.Windows.Forms.ToolStripSeparator();
			this.radioButton_LatestFile = new DarkUI.Controls.DarkRadioButton();
			this.radioButton_SpecificFile = new DarkUI.Controls.DarkRadioButton();
			this.sectionPanel = new DarkUI.Controls.DarkSectionPanel();
			this.tabControl = new System.Windows.Forms.CustomTabControl();
			this.tabPage_LevelSettings = new System.Windows.Forms.TabPage();
			this.treeView_AllPrjFiles = new DarkUI.Controls.DarkTreeView();
			this.tabPage_Resources = new System.Windows.Forms.TabPage();
			this.treeView_Resources = new DarkUI.Controls.DarkTreeView();
			this.contextMenu.SuspendLayout();
			this.sectionPanel.SuspendLayout();
			this.tabControl.SuspendLayout();
			this.tabPage_LevelSettings.SuspendLayout();
			this.tabPage_Resources.SuspendLayout();
			this.SuspendLayout();
			// 
			// checkBox_ShowAllFiles
			// 
			this.checkBox_ShowAllFiles.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.checkBox_ShowAllFiles.Location = new System.Drawing.Point(9, 238);
			this.checkBox_ShowAllFiles.Name = "checkBox_ShowAllFiles";
			this.checkBox_ShowAllFiles.Size = new System.Drawing.Size(290, 18);
			this.checkBox_ShowAllFiles.TabIndex = 3;
			this.checkBox_ShowAllFiles.Text = "Show all .prj2 files (includes backup files)";
			this.checkBox_ShowAllFiles.CheckedChanged += new System.EventHandler(this.checkBox_ShowAllFiles_CheckedChanged);
			// 
			// contextMenu
			// 
			this.contextMenu.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
			this.contextMenu.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
			this.contextMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.menuItem_Open,
            this.menuItem_OpenFolder,
            this.separator_01});
			this.contextMenu.Name = "contextMenu";
			this.contextMenu.Size = new System.Drawing.Size(198, 55);
			// 
			// menuItem_Open
			// 
			this.menuItem_Open.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
			this.menuItem_Open.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
			this.menuItem_Open.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
			this.menuItem_Open.Name = "menuItem_Open";
			this.menuItem_Open.Size = new System.Drawing.Size(197, 22);
			this.menuItem_Open.Text = "Open";
			this.menuItem_Open.Click += new System.EventHandler(this.menuItem_Open_Click);
			// 
			// menuItem_OpenFolder
			// 
			this.menuItem_OpenFolder.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
			this.menuItem_OpenFolder.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
			this.menuItem_OpenFolder.Image = global::TombIDE.ProjectMaster.Properties.Resources.forward_arrow_16;
			this.menuItem_OpenFolder.Name = "menuItem_OpenFolder";
			this.menuItem_OpenFolder.Size = new System.Drawing.Size(197, 22);
			this.menuItem_OpenFolder.Text = "Open Folder in Explorer";
			this.menuItem_OpenFolder.Click += new System.EventHandler(this.menuItem_OpenFolder_Click);
			// 
			// separator_01
			// 
			this.separator_01.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
			this.separator_01.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
			this.separator_01.Margin = new System.Windows.Forms.Padding(0, 0, 0, 1);
			this.separator_01.Name = "separator_01";
			this.separator_01.Size = new System.Drawing.Size(194, 6);
			// 
			// radioButton_LatestFile
			// 
			this.radioButton_LatestFile.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.radioButton_LatestFile.Location = new System.Drawing.Point(9, 9);
			this.radioButton_LatestFile.Name = "radioButton_LatestFile";
			this.radioButton_LatestFile.Size = new System.Drawing.Size(290, 18);
			this.radioButton_LatestFile.TabIndex = 0;
			this.radioButton_LatestFile.TabStop = true;
			this.radioButton_LatestFile.Text = "Use the latest .prj2 file by date as the default level file";
			this.radioButton_LatestFile.CheckedChanged += new System.EventHandler(this.radioButton_LatestFile_CheckedChanged);
			// 
			// radioButton_SpecificFile
			// 
			this.radioButton_SpecificFile.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.radioButton_SpecificFile.Location = new System.Drawing.Point(9, 33);
			this.radioButton_SpecificFile.Name = "radioButton_SpecificFile";
			this.radioButton_SpecificFile.Size = new System.Drawing.Size(290, 18);
			this.radioButton_SpecificFile.TabIndex = 1;
			this.radioButton_SpecificFile.TabStop = true;
			this.radioButton_SpecificFile.Text = "Use a specific .prj2 file from the folder:";
			this.radioButton_SpecificFile.CheckedChanged += new System.EventHandler(this.radioButton_SpecificFile_CheckedChanged);
			// 
			// sectionPanel
			// 
			this.sectionPanel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.sectionPanel.Controls.Add(this.tabControl);
			this.sectionPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.sectionPanel.Location = new System.Drawing.Point(0, 0);
			this.sectionPanel.Name = "sectionPanel";
			this.sectionPanel.SectionHeader = "Selected Level Properties";
			this.sectionPanel.Size = new System.Drawing.Size(320, 320);
			this.sectionPanel.TabIndex = 0;
			// 
			// tabControl
			// 
			this.tabControl.Controls.Add(this.tabPage_LevelSettings);
			this.tabControl.Controls.Add(this.tabPage_Resources);
			this.tabControl.DisplayStyle = System.Windows.Forms.TabStyle.Dark;
			// 
			// 
			// 
			this.tabControl.DisplayStyleProvider.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(96)))), ((int)(((byte)(96)))), ((int)(((byte)(96)))));
			this.tabControl.DisplayStyleProvider.BorderColorHot = System.Drawing.Color.FromArgb(((int)(((byte)(96)))), ((int)(((byte)(96)))), ((int)(((byte)(96)))));
			this.tabControl.DisplayStyleProvider.BorderColorSelected = System.Drawing.Color.FromArgb(((int)(((byte)(96)))), ((int)(((byte)(96)))), ((int)(((byte)(96)))));
			this.tabControl.DisplayStyleProvider.CloserColor = System.Drawing.Color.White;
			this.tabControl.DisplayStyleProvider.CloserColorActive = System.Drawing.Color.FromArgb(((int)(((byte)(152)))), ((int)(((byte)(196)))), ((int)(((byte)(232)))));
			this.tabControl.DisplayStyleProvider.FocusTrack = false;
			this.tabControl.DisplayStyleProvider.HotTrack = false;
			this.tabControl.DisplayStyleProvider.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.tabControl.DisplayStyleProvider.Opacity = 1F;
			this.tabControl.DisplayStyleProvider.Overlap = 0;
			this.tabControl.DisplayStyleProvider.Padding = new System.Drawing.Point(6, 3);
			this.tabControl.DisplayStyleProvider.Radius = 10;
			this.tabControl.DisplayStyleProvider.ShowTabCloser = false;
			this.tabControl.DisplayStyleProvider.TextColor = System.Drawing.Color.FromArgb(((int)(((byte)(153)))), ((int)(((byte)(153)))), ((int)(((byte)(153)))));
			this.tabControl.DisplayStyleProvider.TextColorDisabled = System.Drawing.Color.FromArgb(((int)(((byte)(96)))), ((int)(((byte)(96)))), ((int)(((byte)(96)))));
			this.tabControl.DisplayStyleProvider.TextColorSelected = System.Drawing.Color.FromArgb(((int)(((byte)(152)))), ((int)(((byte)(196)))), ((int)(((byte)(232)))));
			this.tabControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tabControl.Enabled = false;
			this.tabControl.Location = new System.Drawing.Point(1, 25);
			this.tabControl.Name = "tabControl";
			this.tabControl.SelectedIndex = 0;
			this.tabControl.Size = new System.Drawing.Size(316, 292);
			this.tabControl.TabIndex = 0;
			this.tabControl.SelectedIndexChanged += new System.EventHandler(this.tabControl_SelectedIndexChanged);
			// 
			// tabPage_LevelSettings
			// 
			this.tabPage_LevelSettings.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
			this.tabPage_LevelSettings.Controls.Add(this.checkBox_ShowAllFiles);
			this.tabPage_LevelSettings.Controls.Add(this.treeView_AllPrjFiles);
			this.tabPage_LevelSettings.Controls.Add(this.radioButton_SpecificFile);
			this.tabPage_LevelSettings.Controls.Add(this.radioButton_LatestFile);
			this.tabPage_LevelSettings.Location = new System.Drawing.Point(4, 23);
			this.tabPage_LevelSettings.Name = "tabPage_LevelSettings";
			this.tabPage_LevelSettings.Padding = new System.Windows.Forms.Padding(6);
			this.tabPage_LevelSettings.Size = new System.Drawing.Size(308, 265);
			this.tabPage_LevelSettings.TabIndex = 0;
			this.tabPage_LevelSettings.Text = "Settings";
			// 
			// treeView_AllPrjFiles
			// 
			this.treeView_AllPrjFiles.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.treeView_AllPrjFiles.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(48)))), ((int)(((byte)(48)))), ((int)(((byte)(48)))));
			this.treeView_AllPrjFiles.Location = new System.Drawing.Point(9, 57);
			this.treeView_AllPrjFiles.MaxDragChange = 20;
			this.treeView_AllPrjFiles.Name = "treeView_AllPrjFiles";
			this.treeView_AllPrjFiles.Size = new System.Drawing.Size(290, 175);
			this.treeView_AllPrjFiles.TabIndex = 2;
			this.treeView_AllPrjFiles.SelectedNodesChanged += new System.EventHandler(this.treeView_AllPrjFiles_SelectedNodesChanged);
			// 
			// tabPage_Resources
			// 
			this.tabPage_Resources.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
			this.tabPage_Resources.Controls.Add(this.treeView_Resources);
			this.tabPage_Resources.Location = new System.Drawing.Point(4, 23);
			this.tabPage_Resources.Name = "tabPage_Resources";
			this.tabPage_Resources.Size = new System.Drawing.Size(308, 265);
			this.tabPage_Resources.TabIndex = 1;
			this.tabPage_Resources.Text = "Resources";
			// 
			// treeView_Resources
			// 
			this.treeView_Resources.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(48)))), ((int)(((byte)(48)))), ((int)(((byte)(48)))));
			this.treeView_Resources.Dock = System.Windows.Forms.DockStyle.Fill;
			this.treeView_Resources.ItemHeight = 30;
			this.treeView_Resources.Location = new System.Drawing.Point(0, 0);
			this.treeView_Resources.MaxDragChange = 30;
			this.treeView_Resources.Name = "treeView_Resources";
			this.treeView_Resources.ShowIcons = true;
			this.treeView_Resources.Size = new System.Drawing.Size(308, 265);
			this.treeView_Resources.TabIndex = 0;
			this.treeView_Resources.MouseClick += new System.Windows.Forms.MouseEventHandler(this.treeView_Resources_MouseClick);
			this.treeView_Resources.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.treeView_Resources_MouseDoubleClick);
			// 
			// SectionLevelProperties
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
			this.Controls.Add(this.sectionPanel);
			this.Name = "SectionLevelProperties";
			this.Size = new System.Drawing.Size(320, 320);
			this.contextMenu.ResumeLayout(false);
			this.sectionPanel.ResumeLayout(false);
			this.tabControl.ResumeLayout(false);
			this.tabPage_LevelSettings.ResumeLayout(false);
			this.tabPage_Resources.ResumeLayout(false);
			this.ResumeLayout(false);

		}

		#endregion

		private DarkUI.Controls.DarkCheckBox checkBox_ShowAllFiles;
		private DarkUI.Controls.DarkContextMenu contextMenu;
		private DarkUI.Controls.DarkRadioButton radioButton_LatestFile;
		private DarkUI.Controls.DarkRadioButton radioButton_SpecificFile;
		private DarkUI.Controls.DarkSectionPanel sectionPanel;
		private DarkUI.Controls.DarkTreeView treeView_AllPrjFiles;
		private DarkUI.Controls.DarkTreeView treeView_Resources;
		private System.Windows.Forms.CustomTabControl tabControl;
		private System.Windows.Forms.TabPage tabPage_LevelSettings;
		private System.Windows.Forms.TabPage tabPage_Resources;
		private System.Windows.Forms.ToolStripMenuItem menuItem_Open;
		private System.Windows.Forms.ToolStripMenuItem menuItem_OpenFolder;
		private System.Windows.Forms.ToolStripSeparator separator_01;
	}
}
