namespace TombIDE.ProjectMaster
{
	partial class SectionPluginList
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
			this.label_01 = new DarkUI.Controls.DarkLabel();
			this.label_02 = new DarkUI.Controls.DarkLabel();
			this.label_NoInfo = new DarkUI.Controls.DarkLabel();
			this.label_NoLogo = new DarkUI.Controls.DarkLabel();
			this.panel_List = new System.Windows.Forms.Panel();
			this.treeView = new DarkUI.Controls.DarkTreeView();
			this.contextMenu = new DarkUI.Controls.DarkContextMenu();
			this.menuItem_OpenDirectory = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
			this.menuItem_Remove = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStrip = new DarkUI.Controls.DarkToolStrip();
			this.button_OpenArchive = new System.Windows.Forms.ToolStripButton();
			this.button_OpenFolder = new System.Windows.Forms.ToolStripButton();
			this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
			this.button_Remove = new System.Windows.Forms.ToolStripButton();
			this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
			this.button_OpenDirectory = new System.Windows.Forms.ToolStripButton();
			this.button_Refresh = new System.Windows.Forms.ToolStripButton();
			this.button_Download = new System.Windows.Forms.ToolStripButton();
			this.splitContainer = new System.Windows.Forms.SplitContainer();
			this.panel_Properties = new System.Windows.Forms.Panel();
			this.tabControl = new System.Windows.Forms.CustomTabControl();
			this.tabPage_Overview = new System.Windows.Forms.TabPage();
			this.textBox_DLLName = new System.Windows.Forms.TextBox();
			this.textBox_Title = new System.Windows.Forms.TextBox();
			this.panel_Logo = new System.Windows.Forms.Panel();
			this.tabPage_Description = new System.Windows.Forms.TabPage();
			this.richTextBox_Description = new System.Windows.Forms.RichTextBox();
			this.sectionPanel = new DarkUI.Controls.DarkSectionPanel();
			this.toolTip = new System.Windows.Forms.ToolTip(this.components);
			this.panel_List.SuspendLayout();
			this.contextMenu.SuspendLayout();
			this.toolStrip.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.splitContainer)).BeginInit();
			this.splitContainer.Panel1.SuspendLayout();
			this.splitContainer.Panel2.SuspendLayout();
			this.splitContainer.SuspendLayout();
			this.panel_Properties.SuspendLayout();
			this.tabControl.SuspendLayout();
			this.tabPage_Overview.SuspendLayout();
			this.panel_Logo.SuspendLayout();
			this.tabPage_Description.SuspendLayout();
			this.sectionPanel.SuspendLayout();
			this.SuspendLayout();
			// 
			// label_01
			// 
			this.label_01.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.label_01.AutoSize = true;
			this.label_01.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.label_01.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
			this.label_01.Location = new System.Drawing.Point(8, 214);
			this.label_01.Margin = new System.Windows.Forms.Padding(9, 3, 0, 3);
			this.label_01.Name = "label_01";
			this.label_01.Size = new System.Drawing.Size(66, 13);
			this.label_01.TabIndex = 1;
			this.label_01.Text = "Plugin title:";
			// 
			// label_02
			// 
			this.label_02.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.label_02.AutoSize = true;
			this.label_02.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.label_02.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
			this.label_02.Location = new System.Drawing.Point(15, 241);
			this.label_02.Margin = new System.Windows.Forms.Padding(9, 3, 0, 6);
			this.label_02.Name = "label_02";
			this.label_02.Size = new System.Drawing.Size(59, 13);
			this.label_02.TabIndex = 3;
			this.label_02.Text = "DLL name:";
			// 
			// label_NoInfo
			// 
			this.label_NoInfo.Dock = System.Windows.Forms.DockStyle.Fill;
			this.label_NoInfo.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
			this.label_NoInfo.ForeColor = System.Drawing.Color.Gray;
			this.label_NoInfo.Location = new System.Drawing.Point(0, 0);
			this.label_NoInfo.Name = "label_NoInfo";
			this.label_NoInfo.Size = new System.Drawing.Size(618, 198);
			this.label_NoInfo.TabIndex = 1;
			this.label_NoInfo.Text = "Please select a plugin from the list to view its info.";
			this.label_NoInfo.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// label_NoLogo
			// 
			this.label_NoLogo.Dock = System.Windows.Forms.DockStyle.Fill;
			this.label_NoLogo.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
			this.label_NoLogo.ForeColor = System.Drawing.Color.Gray;
			this.label_NoLogo.Location = new System.Drawing.Point(0, 0);
			this.label_NoLogo.Name = "label_NoLogo";
			this.label_NoLogo.Size = new System.Drawing.Size(618, 198);
			this.label_NoLogo.TabIndex = 0;
			this.label_NoLogo.Text = "No logo image found.";
			this.label_NoLogo.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			this.label_NoLogo.Visible = false;
			// 
			// panel_List
			// 
			this.panel_List.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.panel_List.Controls.Add(this.treeView);
			this.panel_List.Controls.Add(this.toolStrip);
			this.panel_List.Dock = System.Windows.Forms.DockStyle.Fill;
			this.panel_List.Location = new System.Drawing.Point(0, 0);
			this.panel_List.Margin = new System.Windows.Forms.Padding(0);
			this.panel_List.Name = "panel_List";
			this.panel_List.Size = new System.Drawing.Size(250, 292);
			this.panel_List.TabIndex = 0;
			// 
			// treeView
			// 
			this.treeView.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(48)))), ((int)(((byte)(48)))), ((int)(((byte)(48)))));
			this.treeView.ContextMenuStrip = this.contextMenu;
			this.treeView.Dock = System.Windows.Forms.DockStyle.Fill;
			this.treeView.ExpandOnDoubleClick = false;
			this.treeView.Indent = 0;
			this.treeView.ItemHeight = 30;
			this.treeView.Location = new System.Drawing.Point(0, 28);
			this.treeView.Margin = new System.Windows.Forms.Padding(6, 3, 6, 6);
			this.treeView.MaxDragChange = 30;
			this.treeView.Name = "treeView";
			this.treeView.Size = new System.Drawing.Size(248, 262);
			this.treeView.TabIndex = 0;
			this.treeView.SelectedNodesChanged += new System.EventHandler(this.treeView_SelectedNodesChanged);
			// 
			// contextMenu
			// 
			this.contextMenu.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
			this.contextMenu.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
			this.contextMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.menuItem_OpenDirectory,
            this.toolStripSeparator3,
            this.menuItem_Remove});
			this.contextMenu.Name = "contextMenu";
			this.contextMenu.Size = new System.Drawing.Size(191, 55);
			// 
			// menuItem_OpenDirectory
			// 
			this.menuItem_OpenDirectory.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
			this.menuItem_OpenDirectory.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
			this.menuItem_OpenDirectory.Image = global::TombIDE.ProjectMaster.Properties.Resources.forward_arrow_16;
			this.menuItem_OpenDirectory.Name = "menuItem_OpenDirectory";
			this.menuItem_OpenDirectory.Size = new System.Drawing.Size(190, 22);
			this.menuItem_OpenDirectory.Text = "Open plugin directory";
			this.menuItem_OpenDirectory.Click += new System.EventHandler(this.button_OpenDirectory_Click);
			// 
			// toolStripSeparator3
			// 
			this.toolStripSeparator3.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
			this.toolStripSeparator3.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
			this.toolStripSeparator3.Margin = new System.Windows.Forms.Padding(0, 0, 0, 1);
			this.toolStripSeparator3.Name = "toolStripSeparator3";
			this.toolStripSeparator3.Size = new System.Drawing.Size(187, 6);
			// 
			// menuItem_Remove
			// 
			this.menuItem_Remove.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
			this.menuItem_Remove.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
			this.menuItem_Remove.Image = global::TombIDE.ProjectMaster.Properties.Resources.general_trash_16;
			this.menuItem_Remove.Name = "menuItem_Remove";
			this.menuItem_Remove.Size = new System.Drawing.Size(190, 22);
			this.menuItem_Remove.Text = "Remove from project";
			this.menuItem_Remove.Click += new System.EventHandler(this.button_Remove_Click);
			// 
			// toolStrip
			// 
			this.toolStrip.AutoSize = false;
			this.toolStrip.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
			this.toolStrip.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
			this.toolStrip.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
			this.toolStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.button_OpenArchive,
            this.button_OpenFolder,
            this.toolStripSeparator1,
            this.button_Remove,
            this.toolStripSeparator2,
            this.button_OpenDirectory,
            this.button_Refresh,
            this.button_Download});
			this.toolStrip.Location = new System.Drawing.Point(0, 0);
			this.toolStrip.Name = "toolStrip";
			this.toolStrip.Padding = new System.Windows.Forms.Padding(5, 0, 1, 0);
			this.toolStrip.Size = new System.Drawing.Size(248, 28);
			this.toolStrip.TabIndex = 1;
			this.toolStrip.Text = "darkToolStrip1";
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
			this.button_OpenArchive.Text = "Install plugin from .zip archive...";
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
			this.button_OpenFolder.Text = "Install plugin from folder...";
			this.button_OpenFolder.Click += new System.EventHandler(this.button_OpenFolder_Click);
			// 
			// toolStripSeparator1
			// 
			this.toolStripSeparator1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
			this.toolStripSeparator1.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
			this.toolStripSeparator1.Margin = new System.Windows.Forms.Padding(0, 0, 2, 0);
			this.toolStripSeparator1.Name = "toolStripSeparator1";
			this.toolStripSeparator1.Size = new System.Drawing.Size(6, 28);
			// 
			// button_Remove
			// 
			this.button_Remove.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
			this.button_Remove.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.button_Remove.Enabled = false;
			this.button_Remove.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
			this.button_Remove.Image = global::TombIDE.ProjectMaster.Properties.Resources.general_trash_16;
			this.button_Remove.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.button_Remove.Name = "button_Remove";
			this.button_Remove.Size = new System.Drawing.Size(23, 25);
			this.button_Remove.Text = "Remove from project...";
			this.button_Remove.Click += new System.EventHandler(this.button_Remove_Click);
			// 
			// toolStripSeparator2
			// 
			this.toolStripSeparator2.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
			this.toolStripSeparator2.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
			this.toolStripSeparator2.Margin = new System.Windows.Forms.Padding(0, 0, 2, 0);
			this.toolStripSeparator2.Name = "toolStripSeparator2";
			this.toolStripSeparator2.Size = new System.Drawing.Size(6, 28);
			// 
			// button_OpenDirectory
			// 
			this.button_OpenDirectory.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
			this.button_OpenDirectory.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.button_OpenDirectory.Enabled = false;
			this.button_OpenDirectory.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
			this.button_OpenDirectory.Image = global::TombIDE.ProjectMaster.Properties.Resources.forward_arrow_16;
			this.button_OpenDirectory.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.button_OpenDirectory.Name = "button_OpenDirectory";
			this.button_OpenDirectory.Size = new System.Drawing.Size(23, 25);
			this.button_OpenDirectory.Text = "Open plugin directory...";
			this.button_OpenDirectory.Click += new System.EventHandler(this.button_OpenDirectory_Click);
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
			this.button_Refresh.Text = "Refresh plugin list";
			this.button_Refresh.Click += new System.EventHandler(this.button_Refresh_Click);
			// 
			// button_Download
			// 
			this.button_Download.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
			this.button_Download.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
			this.button_Download.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.button_Download.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
			this.button_Download.Image = global::TombIDE.ProjectMaster.Properties.Resources.general_ArrowDown_16;
			this.button_Download.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.button_Download.Name = "button_Download";
			this.button_Download.Size = new System.Drawing.Size(23, 25);
			this.button_Download.Text = "Download plugins...";
			this.button_Download.Click += new System.EventHandler(this.button_Download_Click);
			// 
			// splitContainer
			// 
			this.splitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
			this.splitContainer.Location = new System.Drawing.Point(1, 25);
			this.splitContainer.Name = "splitContainer";
			// 
			// splitContainer.Panel1
			// 
			this.splitContainer.Panel1.Controls.Add(this.panel_List);
			this.splitContainer.Panel1MinSize = 250;
			// 
			// splitContainer.Panel2
			// 
			this.splitContainer.Panel2.Controls.Add(this.panel_Properties);
			this.splitContainer.Panel2MinSize = 400;
			this.splitContainer.Size = new System.Drawing.Size(896, 292);
			this.splitContainer.SplitterDistance = 250;
			this.splitContainer.TabIndex = 1;
			// 
			// panel_Properties
			// 
			this.panel_Properties.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.panel_Properties.Controls.Add(this.tabControl);
			this.panel_Properties.Dock = System.Windows.Forms.DockStyle.Fill;
			this.panel_Properties.Location = new System.Drawing.Point(0, 0);
			this.panel_Properties.Name = "panel_Properties";
			this.panel_Properties.Size = new System.Drawing.Size(642, 292);
			this.panel_Properties.TabIndex = 1;
			// 
			// tabControl
			// 
			this.tabControl.Controls.Add(this.tabPage_Overview);
			this.tabControl.Controls.Add(this.tabPage_Description);
			this.tabControl.DisplayStyle = System.Windows.Forms.TabStyle.Dark;
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
			this.tabControl.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.tabControl.Location = new System.Drawing.Point(0, 0);
			this.tabControl.Name = "tabControl";
			this.tabControl.SelectedIndex = 0;
			this.tabControl.Size = new System.Drawing.Size(640, 290);
			this.tabControl.TabIndex = 0;
			// 
			// tabPage_Overview
			// 
			this.tabPage_Overview.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
			this.tabPage_Overview.Controls.Add(this.label_02);
			this.tabPage_Overview.Controls.Add(this.label_01);
			this.tabPage_Overview.Controls.Add(this.textBox_DLLName);
			this.tabPage_Overview.Controls.Add(this.textBox_Title);
			this.tabPage_Overview.Controls.Add(this.panel_Logo);
			this.tabPage_Overview.Location = new System.Drawing.Point(4, 23);
			this.tabPage_Overview.Name = "tabPage_Overview";
			this.tabPage_Overview.Size = new System.Drawing.Size(632, 263);
			this.tabPage_Overview.TabIndex = 0;
			this.tabPage_Overview.Text = "Overview";
			// 
			// textBox_DLLName
			// 
			this.textBox_DLLName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.textBox_DLLName.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(48)))), ((int)(((byte)(48)))), ((int)(((byte)(48)))));
			this.textBox_DLLName.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.textBox_DLLName.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.textBox_DLLName.ForeColor = System.Drawing.Color.Gainsboro;
			this.textBox_DLLName.Location = new System.Drawing.Point(75, 239);
			this.textBox_DLLName.Margin = new System.Windows.Forms.Padding(6, 3, 6, 6);
			this.textBox_DLLName.Name = "textBox_DLLName";
			this.textBox_DLLName.ReadOnly = true;
			this.textBox_DLLName.Size = new System.Drawing.Size(550, 22);
			this.textBox_DLLName.TabIndex = 4;
			// 
			// textBox_Title
			// 
			this.textBox_Title.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.textBox_Title.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(48)))), ((int)(((byte)(48)))), ((int)(((byte)(48)))));
			this.textBox_Title.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.textBox_Title.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.textBox_Title.ForeColor = System.Drawing.Color.Gainsboro;
			this.textBox_Title.Location = new System.Drawing.Point(75, 212);
			this.textBox_Title.Margin = new System.Windows.Forms.Padding(6, 3, 6, 3);
			this.textBox_Title.Name = "textBox_Title";
			this.textBox_Title.ReadOnly = true;
			this.textBox_Title.Size = new System.Drawing.Size(550, 22);
			this.textBox_Title.TabIndex = 2;
			// 
			// panel_Logo
			// 
			this.panel_Logo.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.panel_Logo.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(48)))), ((int)(((byte)(48)))), ((int)(((byte)(48)))));
			this.panel_Logo.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
			this.panel_Logo.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.panel_Logo.Controls.Add(this.label_NoInfo);
			this.panel_Logo.Controls.Add(this.label_NoLogo);
			this.panel_Logo.Location = new System.Drawing.Point(6, 6);
			this.panel_Logo.Margin = new System.Windows.Forms.Padding(6, 6, 6, 3);
			this.panel_Logo.Name = "panel_Logo";
			this.panel_Logo.Size = new System.Drawing.Size(620, 200);
			this.panel_Logo.TabIndex = 0;
			// 
			// tabPage_Description
			// 
			this.tabPage_Description.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
			this.tabPage_Description.Controls.Add(this.richTextBox_Description);
			this.tabPage_Description.Location = new System.Drawing.Point(4, 23);
			this.tabPage_Description.Name = "tabPage_Description";
			this.tabPage_Description.Size = new System.Drawing.Size(632, 263);
			this.tabPage_Description.TabIndex = 1;
			this.tabPage_Description.Text = "Description";
			// 
			// richTextBox_Description
			// 
			this.richTextBox_Description.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(48)))), ((int)(((byte)(48)))), ((int)(((byte)(48)))));
			this.richTextBox_Description.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.richTextBox_Description.Dock = System.Windows.Forms.DockStyle.Fill;
			this.richTextBox_Description.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
			this.richTextBox_Description.ForeColor = System.Drawing.Color.Gainsboro;
			this.richTextBox_Description.Location = new System.Drawing.Point(0, 0);
			this.richTextBox_Description.Name = "richTextBox_Description";
			this.richTextBox_Description.ReadOnly = true;
			this.richTextBox_Description.Size = new System.Drawing.Size(632, 263);
			this.richTextBox_Description.TabIndex = 0;
			this.richTextBox_Description.Text = "";
			// 
			// sectionPanel
			// 
			this.sectionPanel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.sectionPanel.Controls.Add(this.splitContainer);
			this.sectionPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.sectionPanel.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.sectionPanel.Location = new System.Drawing.Point(0, 0);
			this.sectionPanel.Name = "sectionPanel";
			this.sectionPanel.SectionHeader = "Project Plugins";
			this.sectionPanel.Size = new System.Drawing.Size(900, 320);
			this.sectionPanel.TabIndex = 0;
			// 
			// SectionPluginList
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
			this.Controls.Add(this.sectionPanel);
			this.Name = "SectionPluginList";
			this.Size = new System.Drawing.Size(900, 320);
			this.panel_List.ResumeLayout(false);
			this.contextMenu.ResumeLayout(false);
			this.toolStrip.ResumeLayout(false);
			this.toolStrip.PerformLayout();
			this.splitContainer.Panel1.ResumeLayout(false);
			this.splitContainer.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.splitContainer)).EndInit();
			this.splitContainer.ResumeLayout(false);
			this.panel_Properties.ResumeLayout(false);
			this.tabControl.ResumeLayout(false);
			this.tabPage_Overview.ResumeLayout(false);
			this.tabPage_Overview.PerformLayout();
			this.panel_Logo.ResumeLayout(false);
			this.tabPage_Description.ResumeLayout(false);
			this.sectionPanel.ResumeLayout(false);
			this.ResumeLayout(false);

		}

		#endregion
		private DarkUI.Controls.DarkLabel label_01;
		private DarkUI.Controls.DarkLabel label_02;
		private DarkUI.Controls.DarkLabel label_NoInfo;
		private DarkUI.Controls.DarkLabel label_NoLogo;
		private DarkUI.Controls.DarkSectionPanel sectionPanel;
		private DarkUI.Controls.DarkTreeView treeView;
		private System.Windows.Forms.CustomTabControl tabControl;
		private System.Windows.Forms.Panel panel_List;
		private System.Windows.Forms.Panel panel_Logo;
		private System.Windows.Forms.Panel panel_Properties;
		private System.Windows.Forms.RichTextBox richTextBox_Description;
		private System.Windows.Forms.TabPage tabPage_Description;
		private System.Windows.Forms.TabPage tabPage_Overview;
		private System.Windows.Forms.TextBox textBox_DLLName;
		private System.Windows.Forms.TextBox textBox_Title;
		private System.Windows.Forms.ToolTip toolTip;
		private DarkUI.Controls.DarkContextMenu contextMenu;
		private System.Windows.Forms.ToolStripMenuItem menuItem_Remove;
		private System.Windows.Forms.SplitContainer splitContainer;
		private DarkUI.Controls.DarkToolStrip toolStrip;
		private System.Windows.Forms.ToolStripButton button_OpenArchive;
		private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
		private System.Windows.Forms.ToolStripButton button_Remove;
		private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
		private System.Windows.Forms.ToolStripButton button_OpenDirectory;
		private System.Windows.Forms.ToolStripButton button_Refresh;
		private System.Windows.Forms.ToolStripButton button_Download;
		private System.Windows.Forms.ToolStripMenuItem menuItem_OpenDirectory;
		private System.Windows.Forms.ToolStripSeparator toolStripSeparator3;
		private System.Windows.Forms.ToolStripButton button_OpenFolder;
	}
}
