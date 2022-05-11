namespace TombIDE.ProjectMaster
{
	partial class PluginManager
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
			this.splitContainer = new System.Windows.Forms.SplitContainer();
			this.tableLayoutPanel_List = new System.Windows.Forms.TableLayoutPanel();
			this.button_Download = new DarkUI.Controls.DarkButton();
			this.section_PluginList = new DarkUI.Controls.DarkSectionPanel();
			this.tableLayoutPanel_Details = new System.Windows.Forms.TableLayoutPanel();
			this.section_PluginDetails = new DarkUI.Controls.DarkSectionPanel();
			this.tableLayoutPanel3 = new System.Windows.Forms.TableLayoutPanel();
			this.panel_Logo = new System.Windows.Forms.Panel();
			this.textBox_DLLName = new System.Windows.Forms.TextBox();
			this.textBox_Title = new System.Windows.Forms.TextBox();
			this.section_Description = new DarkUI.Controls.DarkSectionPanel();
			this.richTextBox_Description = new System.Windows.Forms.RichTextBox();
			this.toolTip = new System.Windows.Forms.ToolTip(this.components);
			this.tableLayoutPanel_Main = new System.Windows.Forms.TableLayoutPanel();
			this.label_Title = new DarkUI.Controls.DarkLabel();
			this.panel_Icon = new System.Windows.Forms.Panel();
			this.contextMenu.SuspendLayout();
			this.toolStrip.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.splitContainer)).BeginInit();
			this.splitContainer.Panel1.SuspendLayout();
			this.splitContainer.Panel2.SuspendLayout();
			this.splitContainer.SuspendLayout();
			this.tableLayoutPanel_List.SuspendLayout();
			this.section_PluginList.SuspendLayout();
			this.tableLayoutPanel_Details.SuspendLayout();
			this.section_PluginDetails.SuspendLayout();
			this.tableLayoutPanel3.SuspendLayout();
			this.panel_Logo.SuspendLayout();
			this.section_Description.SuspendLayout();
			this.tableLayoutPanel_Main.SuspendLayout();
			this.SuspendLayout();
			// 
			// label_01
			// 
			this.label_01.Dock = System.Windows.Forms.DockStyle.Fill;
			this.label_01.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.label_01.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
			this.label_01.Location = new System.Drawing.Point(9, 140);
			this.label_01.Margin = new System.Windows.Forms.Padding(9, 3, 0, 3);
			this.label_01.Name = "label_01";
			this.label_01.Size = new System.Drawing.Size(66, 19);
			this.label_01.TabIndex = 1;
			this.label_01.Text = "Plugin title:";
			this.label_01.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// label_02
			// 
			this.label_02.Dock = System.Windows.Forms.DockStyle.Fill;
			this.label_02.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.label_02.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
			this.label_02.Location = new System.Drawing.Point(9, 165);
			this.label_02.Margin = new System.Windows.Forms.Padding(9, 3, 0, 6);
			this.label_02.Name = "label_02";
			this.label_02.Size = new System.Drawing.Size(66, 21);
			this.label_02.TabIndex = 3;
			this.label_02.Text = "DLL name:";
			this.label_02.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// label_NoInfo
			// 
			this.label_NoInfo.Dock = System.Windows.Forms.DockStyle.Fill;
			this.label_NoInfo.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
			this.label_NoInfo.ForeColor = System.Drawing.Color.Gray;
			this.label_NoInfo.Location = new System.Drawing.Point(0, 0);
			this.label_NoInfo.Name = "label_NoInfo";
			this.label_NoInfo.Size = new System.Drawing.Size(450, 126);
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
			this.label_NoLogo.Size = new System.Drawing.Size(450, 126);
			this.label_NoLogo.TabIndex = 0;
			this.label_NoLogo.Text = "No logo image found.";
			this.label_NoLogo.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			this.label_NoLogo.Visible = false;
			// 
			// treeView
			// 
			this.treeView.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(48)))), ((int)(((byte)(48)))), ((int)(((byte)(48)))));
			this.treeView.ContextMenuStrip = this.contextMenu;
			this.treeView.Dock = System.Windows.Forms.DockStyle.Fill;
			this.treeView.ExpandOnDoubleClick = false;
			this.treeView.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.treeView.Indent = 0;
			this.treeView.ItemHeight = 40;
			this.treeView.Location = new System.Drawing.Point(1, 53);
			this.treeView.Margin = new System.Windows.Forms.Padding(6, 3, 6, 6);
			this.treeView.MaxDragChange = 40;
			this.treeView.Name = "treeView";
			this.treeView.Size = new System.Drawing.Size(468, 434);
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
            this.button_Refresh});
			this.toolStrip.Location = new System.Drawing.Point(1, 25);
			this.toolStrip.Name = "toolStrip";
			this.toolStrip.Padding = new System.Windows.Forms.Padding(5, 0, 1, 0);
			this.toolStrip.Size = new System.Drawing.Size(468, 28);
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
			// splitContainer
			// 
			this.tableLayoutPanel_Main.SetColumnSpan(this.splitContainer, 2);
			this.splitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
			this.splitContainer.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.splitContainer.Location = new System.Drawing.Point(0, 80);
			this.splitContainer.Margin = new System.Windows.Forms.Padding(0);
			this.splitContainer.Name = "splitContainer";
			// 
			// splitContainer.Panel1
			// 
			this.splitContainer.Panel1.Controls.Add(this.tableLayoutPanel_List);
			this.splitContainer.Panel1.Padding = new System.Windows.Forms.Padding(30, 0, 10, 30);
			this.splitContainer.Panel1MinSize = 360;
			// 
			// splitContainer.Panel2
			// 
			this.splitContainer.Panel2.Controls.Add(this.tableLayoutPanel_Details);
			this.splitContainer.Panel2.Padding = new System.Windows.Forms.Padding(10, 0, 30, 30);
			this.splitContainer.Panel2MinSize = 360;
			this.splitContainer.Size = new System.Drawing.Size(1024, 560);
			this.splitContainer.SplitterDistance = 512;
			this.splitContainer.TabIndex = 1;
			// 
			// tableLayoutPanel_List
			// 
			this.tableLayoutPanel_List.ColumnCount = 1;
			this.tableLayoutPanel_List.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanel_List.Controls.Add(this.button_Download, 0, 1);
			this.tableLayoutPanel_List.Controls.Add(this.section_PluginList, 0, 0);
			this.tableLayoutPanel_List.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tableLayoutPanel_List.Location = new System.Drawing.Point(30, 0);
			this.tableLayoutPanel_List.Name = "tableLayoutPanel_List";
			this.tableLayoutPanel_List.RowCount = 2;
			this.tableLayoutPanel_List.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanel_List.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 40F));
			this.tableLayoutPanel_List.Size = new System.Drawing.Size(472, 530);
			this.tableLayoutPanel_List.TabIndex = 1;
			// 
			// button_Download
			// 
			this.button_Download.Checked = false;
			this.button_Download.Dock = System.Windows.Forms.DockStyle.Fill;
			this.button_Download.Image = global::TombIDE.ProjectMaster.Properties.Resources.general_ArrowDown_16;
			this.button_Download.Location = new System.Drawing.Point(0, 500);
			this.button_Download.Margin = new System.Windows.Forms.Padding(0, 10, 0, 0);
			this.button_Download.Name = "button_Download";
			this.button_Download.Size = new System.Drawing.Size(472, 30);
			this.button_Download.TabIndex = 2;
			this.button_Download.Text = "Download more plugins...";
			this.button_Download.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
			this.button_Download.Click += new System.EventHandler(this.button_Download_Click);
			// 
			// section_PluginList
			// 
			this.section_PluginList.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.section_PluginList.Controls.Add(this.treeView);
			this.section_PluginList.Controls.Add(this.toolStrip);
			this.section_PluginList.Dock = System.Windows.Forms.DockStyle.Fill;
			this.section_PluginList.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.section_PluginList.Location = new System.Drawing.Point(0, 0);
			this.section_PluginList.Margin = new System.Windows.Forms.Padding(0);
			this.section_PluginList.Name = "section_PluginList";
			this.section_PluginList.SectionHeader = "Installed Plugins List";
			this.section_PluginList.Size = new System.Drawing.Size(472, 490);
			this.section_PluginList.TabIndex = 0;
			// 
			// tableLayoutPanel_Details
			// 
			this.tableLayoutPanel_Details.ColumnCount = 1;
			this.tableLayoutPanel_Details.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanel_Details.Controls.Add(this.section_PluginDetails, 0, 0);
			this.tableLayoutPanel_Details.Controls.Add(this.section_Description, 0, 1);
			this.tableLayoutPanel_Details.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tableLayoutPanel_Details.Location = new System.Drawing.Point(10, 0);
			this.tableLayoutPanel_Details.Margin = new System.Windows.Forms.Padding(0);
			this.tableLayoutPanel_Details.Name = "tableLayoutPanel_Details";
			this.tableLayoutPanel_Details.RowCount = 2;
			this.tableLayoutPanel_Details.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 220F));
			this.tableLayoutPanel_Details.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanel_Details.Size = new System.Drawing.Size(468, 530);
			this.tableLayoutPanel_Details.TabIndex = 3;
			// 
			// section_PluginDetails
			// 
			this.section_PluginDetails.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.section_PluginDetails.Controls.Add(this.tableLayoutPanel3);
			this.section_PluginDetails.Dock = System.Windows.Forms.DockStyle.Fill;
			this.section_PluginDetails.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.section_PluginDetails.Location = new System.Drawing.Point(0, 0);
			this.section_PluginDetails.Margin = new System.Windows.Forms.Padding(0);
			this.section_PluginDetails.Name = "section_PluginDetails";
			this.section_PluginDetails.SectionHeader = "Selected Plugin Details";
			this.section_PluginDetails.Size = new System.Drawing.Size(468, 220);
			this.section_PluginDetails.TabIndex = 2;
			// 
			// tableLayoutPanel3
			// 
			this.tableLayoutPanel3.ColumnCount = 2;
			this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 75F));
			this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanel3.Controls.Add(this.panel_Logo, 0, 0);
			this.tableLayoutPanel3.Controls.Add(this.label_01, 0, 1);
			this.tableLayoutPanel3.Controls.Add(this.textBox_DLLName, 1, 2);
			this.tableLayoutPanel3.Controls.Add(this.label_02, 0, 2);
			this.tableLayoutPanel3.Controls.Add(this.textBox_Title, 1, 1);
			this.tableLayoutPanel3.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tableLayoutPanel3.Location = new System.Drawing.Point(1, 25);
			this.tableLayoutPanel3.Name = "tableLayoutPanel3";
			this.tableLayoutPanel3.RowCount = 3;
			this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 25F));
			this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
			this.tableLayoutPanel3.Size = new System.Drawing.Size(464, 192);
			this.tableLayoutPanel3.TabIndex = 5;
			// 
			// panel_Logo
			// 
			this.panel_Logo.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.panel_Logo.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(48)))), ((int)(((byte)(48)))), ((int)(((byte)(48)))));
			this.panel_Logo.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
			this.panel_Logo.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.tableLayoutPanel3.SetColumnSpan(this.panel_Logo, 2);
			this.panel_Logo.Controls.Add(this.label_NoInfo);
			this.panel_Logo.Controls.Add(this.label_NoLogo);
			this.panel_Logo.Location = new System.Drawing.Point(6, 6);
			this.panel_Logo.Margin = new System.Windows.Forms.Padding(6, 6, 6, 3);
			this.panel_Logo.Name = "panel_Logo";
			this.panel_Logo.Size = new System.Drawing.Size(452, 128);
			this.panel_Logo.TabIndex = 0;
			// 
			// textBox_DLLName
			// 
			this.textBox_DLLName.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(48)))), ((int)(((byte)(48)))), ((int)(((byte)(48)))));
			this.textBox_DLLName.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.textBox_DLLName.Dock = System.Windows.Forms.DockStyle.Fill;
			this.textBox_DLLName.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.textBox_DLLName.ForeColor = System.Drawing.Color.Gainsboro;
			this.textBox_DLLName.Location = new System.Drawing.Point(81, 165);
			this.textBox_DLLName.Margin = new System.Windows.Forms.Padding(6, 3, 6, 6);
			this.textBox_DLLName.Name = "textBox_DLLName";
			this.textBox_DLLName.ReadOnly = true;
			this.textBox_DLLName.Size = new System.Drawing.Size(377, 22);
			this.textBox_DLLName.TabIndex = 4;
			// 
			// textBox_Title
			// 
			this.textBox_Title.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(48)))), ((int)(((byte)(48)))), ((int)(((byte)(48)))));
			this.textBox_Title.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.textBox_Title.Dock = System.Windows.Forms.DockStyle.Fill;
			this.textBox_Title.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.textBox_Title.ForeColor = System.Drawing.Color.Gainsboro;
			this.textBox_Title.Location = new System.Drawing.Point(81, 140);
			this.textBox_Title.Margin = new System.Windows.Forms.Padding(6, 3, 6, 3);
			this.textBox_Title.Name = "textBox_Title";
			this.textBox_Title.ReadOnly = true;
			this.textBox_Title.Size = new System.Drawing.Size(377, 22);
			this.textBox_Title.TabIndex = 2;
			// 
			// section_Description
			// 
			this.section_Description.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.section_Description.Controls.Add(this.richTextBox_Description);
			this.section_Description.Dock = System.Windows.Forms.DockStyle.Fill;
			this.section_Description.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.section_Description.Location = new System.Drawing.Point(0, 235);
			this.section_Description.Margin = new System.Windows.Forms.Padding(0, 15, 0, 0);
			this.section_Description.Name = "section_Description";
			this.section_Description.SectionHeader = "Plugin Description";
			this.section_Description.Size = new System.Drawing.Size(468, 295);
			this.section_Description.TabIndex = 3;
			// 
			// richTextBox_Description
			// 
			this.richTextBox_Description.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(48)))), ((int)(((byte)(48)))), ((int)(((byte)(48)))));
			this.richTextBox_Description.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.richTextBox_Description.Dock = System.Windows.Forms.DockStyle.Fill;
			this.richTextBox_Description.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
			this.richTextBox_Description.ForeColor = System.Drawing.Color.Gainsboro;
			this.richTextBox_Description.Location = new System.Drawing.Point(1, 25);
			this.richTextBox_Description.Name = "richTextBox_Description";
			this.richTextBox_Description.ReadOnly = true;
			this.richTextBox_Description.Size = new System.Drawing.Size(464, 267);
			this.richTextBox_Description.TabIndex = 0;
			this.richTextBox_Description.Text = "";
			// 
			// tableLayoutPanel_Main
			// 
			this.tableLayoutPanel_Main.ColumnCount = 2;
			this.tableLayoutPanel_Main.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 70F));
			this.tableLayoutPanel_Main.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanel_Main.Controls.Add(this.label_Title, 1, 0);
			this.tableLayoutPanel_Main.Controls.Add(this.splitContainer, 0, 1);
			this.tableLayoutPanel_Main.Controls.Add(this.panel_Icon, 0, 0);
			this.tableLayoutPanel_Main.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tableLayoutPanel_Main.Location = new System.Drawing.Point(0, 0);
			this.tableLayoutPanel_Main.Margin = new System.Windows.Forms.Padding(0);
			this.tableLayoutPanel_Main.Name = "tableLayoutPanel_Main";
			this.tableLayoutPanel_Main.RowCount = 2;
			this.tableLayoutPanel_Main.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 80F));
			this.tableLayoutPanel_Main.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanel_Main.Size = new System.Drawing.Size(1024, 640);
			this.tableLayoutPanel_Main.TabIndex = 3;
			// 
			// label_Title
			// 
			this.label_Title.AutoSize = true;
			this.label_Title.Dock = System.Windows.Forms.DockStyle.Fill;
			this.label_Title.Font = new System.Drawing.Font("Segoe UI", 15.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.label_Title.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
			this.label_Title.Location = new System.Drawing.Point(71, 1);
			this.label_Title.Margin = new System.Windows.Forms.Padding(1);
			this.label_Title.Name = "label_Title";
			this.label_Title.Size = new System.Drawing.Size(952, 78);
			this.label_Title.TabIndex = 3;
			this.label_Title.Text = "Plugin Manager";
			this.label_Title.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// panel_Icon
			// 
			this.panel_Icon.BackgroundImage = global::TombIDE.ProjectMaster.Properties.Resources.ide_plugin_30;
			this.panel_Icon.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
			this.panel_Icon.Dock = System.Windows.Forms.DockStyle.Fill;
			this.panel_Icon.Location = new System.Drawing.Point(30, 20);
			this.panel_Icon.Margin = new System.Windows.Forms.Padding(30, 20, 0, 15);
			this.panel_Icon.Name = "panel_Icon";
			this.panel_Icon.Size = new System.Drawing.Size(40, 45);
			this.panel_Icon.TabIndex = 4;
			// 
			// PluginManager
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
			this.Controls.Add(this.tableLayoutPanel_Main);
			this.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.Name = "PluginManager";
			this.Size = new System.Drawing.Size(1024, 640);
			this.contextMenu.ResumeLayout(false);
			this.toolStrip.ResumeLayout(false);
			this.toolStrip.PerformLayout();
			this.splitContainer.Panel1.ResumeLayout(false);
			this.splitContainer.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.splitContainer)).EndInit();
			this.splitContainer.ResumeLayout(false);
			this.tableLayoutPanel_List.ResumeLayout(false);
			this.section_PluginList.ResumeLayout(false);
			this.tableLayoutPanel_Details.ResumeLayout(false);
			this.section_PluginDetails.ResumeLayout(false);
			this.tableLayoutPanel3.ResumeLayout(false);
			this.tableLayoutPanel3.PerformLayout();
			this.panel_Logo.ResumeLayout(false);
			this.section_Description.ResumeLayout(false);
			this.tableLayoutPanel_Main.ResumeLayout(false);
			this.tableLayoutPanel_Main.PerformLayout();
			this.ResumeLayout(false);

		}

		#endregion
		private DarkUI.Controls.DarkLabel label_01;
		private DarkUI.Controls.DarkLabel label_02;
		private DarkUI.Controls.DarkLabel label_NoInfo;
		private DarkUI.Controls.DarkLabel label_NoLogo;
		private DarkUI.Controls.DarkSectionPanel section_PluginList;
		private DarkUI.Controls.DarkTreeView treeView;
		private System.Windows.Forms.Panel panel_Logo;
		private System.Windows.Forms.RichTextBox richTextBox_Description;
		private System.Windows.Forms.TextBox textBox_DLLName;
		private System.Windows.Forms.TextBox textBox_Title;
		private System.Windows.Forms.ToolTip toolTip;
		private DarkUI.Controls.DarkContextMenu contextMenu;
		private System.Windows.Forms.ToolStripMenuItem menuItem_Remove;
		private System.Windows.Forms.SplitContainer splitContainer;
		private DarkUI.Controls.DarkToolStrip toolStrip;
		private System.Windows.Forms.ToolStripButton button_Remove;
		private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
		private System.Windows.Forms.ToolStripButton button_OpenDirectory;
		private System.Windows.Forms.ToolStripButton button_Refresh;
		private System.Windows.Forms.ToolStripMenuItem menuItem_OpenDirectory;
		private System.Windows.Forms.ToolStripSeparator toolStripSeparator3;
		private DarkUI.Controls.DarkSectionPanel section_PluginDetails;
		private System.Windows.Forms.TableLayoutPanel tableLayoutPanel_Main;
		private System.Windows.Forms.TableLayoutPanel tableLayoutPanel_Details;
		private System.Windows.Forms.TableLayoutPanel tableLayoutPanel3;
		private DarkUI.Controls.DarkSectionPanel section_Description;
		private DarkUI.Controls.DarkLabel label_Title;
		private System.Windows.Forms.Panel panel_Icon;
		private System.Windows.Forms.ToolStripButton button_OpenArchive;
		private System.Windows.Forms.ToolStripButton button_OpenFolder;
		private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
		private System.Windows.Forms.TableLayoutPanel tableLayoutPanel_List;
		private DarkUI.Controls.DarkButton button_Download;
	}
}
