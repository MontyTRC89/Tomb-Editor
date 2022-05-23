﻿namespace TombIDE.ProjectMaster
{
	partial class SectionLevelList
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
			this.sectionPanel = new DarkUI.Controls.DarkSectionPanel();
			this.label_Hint = new DarkUI.Controls.DarkLabel();
			this.treeView = new DarkUI.Controls.DarkTreeView();
			this.toolStrip = new DarkUI.Controls.DarkToolStrip();
			this.toolStripButton2 = new System.Windows.Forms.ToolStripButton();
			this.toolStripButton1 = new System.Windows.Forms.ToolStripButton();
			this.separator_04 = new System.Windows.Forms.ToolStripSeparator();
			this.button_OpenInTE = new System.Windows.Forms.ToolStripButton();
			this.button_Rebuild = new System.Windows.Forms.ToolStripButton();
			this.separator_01 = new System.Windows.Forms.ToolStripSeparator();
			this.button_Rename = new System.Windows.Forms.ToolStripButton();
			this.button_Delete = new System.Windows.Forms.ToolStripButton();
			this.separator_02 = new System.Windows.Forms.ToolStripSeparator();
			this.button_MoveUp = new System.Windows.Forms.ToolStripButton();
			this.button_MoveDown = new System.Windows.Forms.ToolStripButton();
			this.separator_03 = new System.Windows.Forms.ToolStripSeparator();
			this.button_OpenInExplorer = new System.Windows.Forms.ToolStripButton();
			this.button_Refresh = new System.Windows.Forms.ToolStripButton();
			this.contextMenu = new DarkUI.Controls.DarkContextMenu();
			this.menuItem_OpenLevel = new System.Windows.Forms.ToolStripMenuItem();
			this.menuItem_Rebuild = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
			this.menuItem_OpenDirectory = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
			this.menuItem_MoveUp = new System.Windows.Forms.ToolStripMenuItem();
			this.menuItem_MoveDown = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
			this.menuItem_Rename = new System.Windows.Forms.ToolStripMenuItem();
			this.menuItem_Delete = new System.Windows.Forms.ToolStripMenuItem();
			this.sectionPanel.SuspendLayout();
			this.toolStrip.SuspendLayout();
			this.contextMenu.SuspendLayout();
			this.SuspendLayout();
			// 
			// sectionPanel
			// 
			this.sectionPanel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.sectionPanel.Controls.Add(this.label_Hint);
			this.sectionPanel.Controls.Add(this.treeView);
			this.sectionPanel.Controls.Add(this.toolStrip);
			this.sectionPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.sectionPanel.Location = new System.Drawing.Point(0, 0);
			this.sectionPanel.Name = "sectionPanel";
			this.sectionPanel.SectionHeader = "Level List";
			this.sectionPanel.Size = new System.Drawing.Size(320, 320);
			this.sectionPanel.TabIndex = 0;
			// 
			// label_Hint
			// 
			this.label_Hint.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(48)))), ((int)(((byte)(48)))), ((int)(((byte)(48)))));
			this.label_Hint.Cursor = System.Windows.Forms.Cursors.Hand;
			this.label_Hint.Dock = System.Windows.Forms.DockStyle.Fill;
			this.label_Hint.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
			this.label_Hint.ForeColor = System.Drawing.Color.Gray;
			this.label_Hint.Location = new System.Drawing.Point(1, 55);
			this.label_Hint.Name = "label_Hint";
			this.label_Hint.Size = new System.Drawing.Size(316, 262);
			this.label_Hint.TabIndex = 2;
			this.label_Hint.Text = "Click here to create a new level.\r\n\r\nYou may also click the \' + \' button instead." +
    "";
			this.label_Hint.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			this.label_Hint.Click += new System.EventHandler(this.label_Hint_Click);
			// 
			// treeView
			// 
			this.treeView.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(48)))), ((int)(((byte)(48)))), ((int)(((byte)(48)))));
			this.treeView.Dock = System.Windows.Forms.DockStyle.Fill;
			this.treeView.ExpandOnDoubleClick = false;
			this.treeView.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
			this.treeView.ItemHeight = 64;
			this.treeView.Location = new System.Drawing.Point(1, 55);
			this.treeView.MaxDragChange = 64;
			this.treeView.Name = "treeView";
			this.treeView.OverrideEvenColor = System.Drawing.Color.FromArgb(((int)(((byte)(48)))), ((int)(((byte)(48)))), ((int)(((byte)(48)))));
			this.treeView.OverrideOddColor = System.Drawing.Color.FromArgb(((int)(((byte)(44)))), ((int)(((byte)(44)))), ((int)(((byte)(44)))));
			this.treeView.Size = new System.Drawing.Size(316, 262);
			this.treeView.TabIndex = 0;
			this.treeView.SelectedNodesChanged += new System.EventHandler(this.treeView_SelectedNodesChanged);
			this.treeView.KeyDown += new System.Windows.Forms.KeyEventHandler(this.treeView_KeyDown);
			this.treeView.KeyUp += new System.Windows.Forms.KeyEventHandler(this.treeView_KeyUp);
			this.treeView.MouseClick += new System.Windows.Forms.MouseEventHandler(this.treeView_MouseClick);
			this.treeView.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.treeView_MouseDoubleClick);
			// 
			// toolStrip
			// 
			this.toolStrip.AutoSize = false;
			this.toolStrip.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
			this.toolStrip.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
			this.toolStrip.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
			this.toolStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripButton2,
            this.toolStripButton1,
            this.separator_04,
            this.button_OpenInTE,
            this.button_Rebuild,
            this.separator_01,
            this.button_Rename,
            this.button_Delete,
            this.separator_02,
            this.button_MoveUp,
            this.button_MoveDown,
            this.separator_03,
            this.button_OpenInExplorer,
            this.button_Refresh});
			this.toolStrip.Location = new System.Drawing.Point(1, 25);
			this.toolStrip.Name = "toolStrip";
			this.toolStrip.Padding = new System.Windows.Forms.Padding(5, 0, 1, 0);
			this.toolStrip.Size = new System.Drawing.Size(316, 30);
			this.toolStrip.TabIndex = 1;
			// 
			// toolStripButton2
			// 
			this.toolStripButton2.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
			this.toolStripButton2.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.toolStripButton2.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
			this.toolStripButton2.Image = global::TombIDE.ProjectMaster.Properties.Resources.general_plus_math_16;
			this.toolStripButton2.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.toolStripButton2.Name = "toolStripButton2";
			this.toolStripButton2.Size = new System.Drawing.Size(23, 27);
			this.toolStripButton2.Text = "Create a new level...";
			this.toolStripButton2.Click += new System.EventHandler(this.button_New_Click);
			// 
			// toolStripButton1
			// 
			this.toolStripButton1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
			this.toolStripButton1.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.toolStripButton1.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
			this.toolStripButton1.Image = global::TombIDE.ProjectMaster.Properties.Resources.general_Import_16;
			this.toolStripButton1.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.toolStripButton1.Name = "toolStripButton1";
			this.toolStripButton1.Size = new System.Drawing.Size(23, 27);
			this.toolStripButton1.Text = "Import an existing level...";
			this.toolStripButton1.Click += new System.EventHandler(this.button_Import_Click);
			// 
			// separator_04
			// 
			this.separator_04.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
			this.separator_04.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
			this.separator_04.Margin = new System.Windows.Forms.Padding(0, 0, 2, 0);
			this.separator_04.Name = "separator_04";
			this.separator_04.Size = new System.Drawing.Size(6, 30);
			// 
			// button_OpenInTE
			// 
			this.button_OpenInTE.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
			this.button_OpenInTE.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.button_OpenInTE.Enabled = false;
			this.button_OpenInTE.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
			this.button_OpenInTE.Image = global::TombIDE.ProjectMaster.Properties.Resources.TE_icon;
			this.button_OpenInTE.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.button_OpenInTE.Name = "button_OpenInTE";
			this.button_OpenInTE.Size = new System.Drawing.Size(23, 27);
			this.button_OpenInTE.Text = "Open in Tomb Editor...";
			this.button_OpenInTE.Click += new System.EventHandler(this.menuItem_OpenLevel_Click);
			// 
			// button_Rebuild
			// 
			this.button_Rebuild.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
			this.button_Rebuild.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.button_Rebuild.Enabled = false;
			this.button_Rebuild.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
			this.button_Rebuild.Image = global::TombIDE.ProjectMaster.Properties.Resources.actions_compile_16;
			this.button_Rebuild.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.button_Rebuild.Name = "button_Rebuild";
			this.button_Rebuild.Size = new System.Drawing.Size(23, 27);
			this.button_Rebuild.Text = "Re-build Level...";
			this.button_Rebuild.Click += new System.EventHandler(this.button_Rebuild_Click);
			// 
			// separator_01
			// 
			this.separator_01.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
			this.separator_01.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
			this.separator_01.Margin = new System.Windows.Forms.Padding(0, 0, 2, 0);
			this.separator_01.Name = "separator_01";
			this.separator_01.Size = new System.Drawing.Size(6, 30);
			// 
			// button_Rename
			// 
			this.button_Rename.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
			this.button_Rename.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.button_Rename.Enabled = false;
			this.button_Rename.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
			this.button_Rename.Image = global::TombIDE.ProjectMaster.Properties.Resources.general_edit_16;
			this.button_Rename.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.button_Rename.Name = "button_Rename";
			this.button_Rename.Size = new System.Drawing.Size(23, 27);
			this.button_Rename.Text = "Rename Level...";
			this.button_Rename.Click += new System.EventHandler(this.button_Rename_Click);
			// 
			// button_Delete
			// 
			this.button_Delete.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
			this.button_Delete.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.button_Delete.Enabled = false;
			this.button_Delete.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
			this.button_Delete.Image = global::TombIDE.ProjectMaster.Properties.Resources.general_trash_16;
			this.button_Delete.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.button_Delete.Name = "button_Delete";
			this.button_Delete.Size = new System.Drawing.Size(23, 27);
			this.button_Delete.Text = "Delete Level";
			this.button_Delete.Click += new System.EventHandler(this.button_Delete_Click);
			// 
			// separator_02
			// 
			this.separator_02.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
			this.separator_02.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
			this.separator_02.Margin = new System.Windows.Forms.Padding(0, 0, 2, 0);
			this.separator_02.Name = "separator_02";
			this.separator_02.Size = new System.Drawing.Size(6, 30);
			// 
			// button_MoveUp
			// 
			this.button_MoveUp.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
			this.button_MoveUp.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.button_MoveUp.Enabled = false;
			this.button_MoveUp.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
			this.button_MoveUp.Image = global::TombIDE.ProjectMaster.Properties.Resources.general_ArrowUp_16;
			this.button_MoveUp.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.button_MoveUp.Name = "button_MoveUp";
			this.button_MoveUp.Size = new System.Drawing.Size(23, 27);
			this.button_MoveUp.Text = "Move Level Up on List";
			this.button_MoveUp.TextImageRelation = System.Windows.Forms.TextImageRelation.Overlay;
			this.button_MoveUp.Click += new System.EventHandler(this.button_MoveUp_Click);
			// 
			// button_MoveDown
			// 
			this.button_MoveDown.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
			this.button_MoveDown.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.button_MoveDown.Enabled = false;
			this.button_MoveDown.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
			this.button_MoveDown.Image = global::TombIDE.ProjectMaster.Properties.Resources.general_ArrowDown_16;
			this.button_MoveDown.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.button_MoveDown.Name = "button_MoveDown";
			this.button_MoveDown.Size = new System.Drawing.Size(23, 27);
			this.button_MoveDown.Text = "Move Level Down on List";
			this.button_MoveDown.Click += new System.EventHandler(this.button_MoveDown_Click);
			// 
			// separator_03
			// 
			this.separator_03.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
			this.separator_03.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
			this.separator_03.Margin = new System.Windows.Forms.Padding(0, 0, 2, 0);
			this.separator_03.Name = "separator_03";
			this.separator_03.Size = new System.Drawing.Size(6, 30);
			// 
			// button_OpenInExplorer
			// 
			this.button_OpenInExplorer.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
			this.button_OpenInExplorer.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.button_OpenInExplorer.Enabled = false;
			this.button_OpenInExplorer.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
			this.button_OpenInExplorer.Image = global::TombIDE.ProjectMaster.Properties.Resources.forward_arrow_16;
			this.button_OpenInExplorer.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.button_OpenInExplorer.Name = "button_OpenInExplorer";
			this.button_OpenInExplorer.Size = new System.Drawing.Size(23, 27);
			this.button_OpenInExplorer.Text = "Open Level Folder in Explorer";
			this.button_OpenInExplorer.Click += new System.EventHandler(this.button_OpenInExplorer_Click);
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
			this.button_Refresh.Size = new System.Drawing.Size(23, 27);
			this.button_Refresh.Text = "Refresh List";
			this.button_Refresh.Click += new System.EventHandler(this.button_Refresh_Click);
			// 
			// contextMenu
			// 
			this.contextMenu.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
			this.contextMenu.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
			this.contextMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.menuItem_OpenLevel,
            this.menuItem_Rebuild,
            this.toolStripSeparator3,
            this.menuItem_OpenDirectory,
            this.toolStripSeparator1,
            this.menuItem_MoveUp,
            this.menuItem_MoveDown,
            this.toolStripSeparator2,
            this.menuItem_Rename,
            this.menuItem_Delete});
			this.contextMenu.Name = "contextMenu";
			this.contextMenu.Size = new System.Drawing.Size(218, 179);
			// 
			// menuItem_OpenLevel
			// 
			this.menuItem_OpenLevel.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
			this.menuItem_OpenLevel.Enabled = false;
			this.menuItem_OpenLevel.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
			this.menuItem_OpenLevel.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
			this.menuItem_OpenLevel.Image = global::TombIDE.ProjectMaster.Properties.Resources.TE_icon;
			this.menuItem_OpenLevel.Name = "menuItem_OpenLevel";
			this.menuItem_OpenLevel.Size = new System.Drawing.Size(217, 22);
			this.menuItem_OpenLevel.Text = "Open level in Tomb Editor";
			this.menuItem_OpenLevel.Click += new System.EventHandler(this.menuItem_OpenLevel_Click);
			// 
			// menuItem_Rebuild
			// 
			this.menuItem_Rebuild.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
			this.menuItem_Rebuild.Enabled = false;
			this.menuItem_Rebuild.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
			this.menuItem_Rebuild.Image = global::TombIDE.ProjectMaster.Properties.Resources.actions_compile_16;
			this.menuItem_Rebuild.Name = "menuItem_Rebuild";
			this.menuItem_Rebuild.Size = new System.Drawing.Size(217, 22);
			this.menuItem_Rebuild.Text = "Re-build level...";
			this.menuItem_Rebuild.Click += new System.EventHandler(this.button_Rebuild_Click);
			// 
			// toolStripSeparator3
			// 
			this.toolStripSeparator3.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
			this.toolStripSeparator3.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
			this.toolStripSeparator3.Margin = new System.Windows.Forms.Padding(0, 0, 0, 1);
			this.toolStripSeparator3.Name = "toolStripSeparator3";
			this.toolStripSeparator3.Size = new System.Drawing.Size(214, 6);
			// 
			// menuItem_OpenDirectory
			// 
			this.menuItem_OpenDirectory.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
			this.menuItem_OpenDirectory.Enabled = false;
			this.menuItem_OpenDirectory.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
			this.menuItem_OpenDirectory.Image = global::TombIDE.ProjectMaster.Properties.Resources.forward_arrow_16;
			this.menuItem_OpenDirectory.Name = "menuItem_OpenDirectory";
			this.menuItem_OpenDirectory.Size = new System.Drawing.Size(217, 22);
			this.menuItem_OpenDirectory.Text = "Open level directory...";
			this.menuItem_OpenDirectory.Click += new System.EventHandler(this.button_OpenInExplorer_Click);
			// 
			// toolStripSeparator1
			// 
			this.toolStripSeparator1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
			this.toolStripSeparator1.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
			this.toolStripSeparator1.Margin = new System.Windows.Forms.Padding(0, 0, 0, 1);
			this.toolStripSeparator1.Name = "toolStripSeparator1";
			this.toolStripSeparator1.Size = new System.Drawing.Size(214, 6);
			// 
			// menuItem_MoveUp
			// 
			this.menuItem_MoveUp.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
			this.menuItem_MoveUp.Enabled = false;
			this.menuItem_MoveUp.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
			this.menuItem_MoveUp.Image = global::TombIDE.ProjectMaster.Properties.Resources.general_ArrowUp_16;
			this.menuItem_MoveUp.Name = "menuItem_MoveUp";
			this.menuItem_MoveUp.Size = new System.Drawing.Size(217, 22);
			this.menuItem_MoveUp.Text = "Move level up on list";
			this.menuItem_MoveUp.Click += new System.EventHandler(this.button_MoveUp_Click);
			// 
			// menuItem_MoveDown
			// 
			this.menuItem_MoveDown.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
			this.menuItem_MoveDown.Enabled = false;
			this.menuItem_MoveDown.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
			this.menuItem_MoveDown.Image = global::TombIDE.ProjectMaster.Properties.Resources.general_ArrowDown_16;
			this.menuItem_MoveDown.Name = "menuItem_MoveDown";
			this.menuItem_MoveDown.Size = new System.Drawing.Size(217, 22);
			this.menuItem_MoveDown.Text = "Move level down on list";
			this.menuItem_MoveDown.Click += new System.EventHandler(this.button_MoveDown_Click);
			// 
			// toolStripSeparator2
			// 
			this.toolStripSeparator2.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
			this.toolStripSeparator2.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
			this.toolStripSeparator2.Margin = new System.Windows.Forms.Padding(0, 0, 0, 1);
			this.toolStripSeparator2.Name = "toolStripSeparator2";
			this.toolStripSeparator2.Size = new System.Drawing.Size(214, 6);
			// 
			// menuItem_Rename
			// 
			this.menuItem_Rename.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
			this.menuItem_Rename.Enabled = false;
			this.menuItem_Rename.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
			this.menuItem_Rename.Image = global::TombIDE.ProjectMaster.Properties.Resources.general_edit_16;
			this.menuItem_Rename.Name = "menuItem_Rename";
			this.menuItem_Rename.Size = new System.Drawing.Size(217, 22);
			this.menuItem_Rename.Text = "Rename level...";
			this.menuItem_Rename.Click += new System.EventHandler(this.button_Rename_Click);
			// 
			// menuItem_Delete
			// 
			this.menuItem_Delete.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
			this.menuItem_Delete.Enabled = false;
			this.menuItem_Delete.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
			this.menuItem_Delete.Image = global::TombIDE.ProjectMaster.Properties.Resources.general_trash_16;
			this.menuItem_Delete.Name = "menuItem_Delete";
			this.menuItem_Delete.Size = new System.Drawing.Size(217, 22);
			this.menuItem_Delete.Text = "Delete level...";
			this.menuItem_Delete.Click += new System.EventHandler(this.button_Delete_Click);
			// 
			// SectionLevelList
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
			this.Controls.Add(this.sectionPanel);
			this.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.Name = "SectionLevelList";
			this.Size = new System.Drawing.Size(320, 320);
			this.sectionPanel.ResumeLayout(false);
			this.toolStrip.ResumeLayout(false);
			this.toolStrip.PerformLayout();
			this.contextMenu.ResumeLayout(false);
			this.ResumeLayout(false);

		}

		#endregion

		private DarkUI.Controls.DarkContextMenu contextMenu;
		private DarkUI.Controls.DarkLabel label_Hint;
		private DarkUI.Controls.DarkSectionPanel sectionPanel;
		private DarkUI.Controls.DarkToolStrip toolStrip;
		private DarkUI.Controls.DarkTreeView treeView;
		private System.Windows.Forms.ToolStripButton button_Delete;
		private System.Windows.Forms.ToolStripButton button_MoveDown;
		private System.Windows.Forms.ToolStripButton button_MoveUp;
		private System.Windows.Forms.ToolStripButton button_OpenInExplorer;
		private System.Windows.Forms.ToolStripButton button_OpenInTE;
		private System.Windows.Forms.ToolStripButton button_Refresh;
		private System.Windows.Forms.ToolStripButton button_Rename;
		private System.Windows.Forms.ToolStripMenuItem menuItem_Rebuild;
		private System.Windows.Forms.ToolStripMenuItem menuItem_Delete;
		private System.Windows.Forms.ToolStripMenuItem menuItem_MoveDown;
		private System.Windows.Forms.ToolStripMenuItem menuItem_MoveUp;
		private System.Windows.Forms.ToolStripMenuItem menuItem_OpenDirectory;
		private System.Windows.Forms.ToolStripMenuItem menuItem_OpenLevel;
		private System.Windows.Forms.ToolStripMenuItem menuItem_Rename;
		private System.Windows.Forms.ToolStripSeparator separator_01;
		private System.Windows.Forms.ToolStripSeparator separator_02;
		private System.Windows.Forms.ToolStripSeparator separator_03;
		private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
		private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
		private System.Windows.Forms.ToolStripSeparator toolStripSeparator3;
		private System.Windows.Forms.ToolStripButton button_Rebuild;
		private System.Windows.Forms.ToolStripSeparator separator_04;
		private System.Windows.Forms.ToolStripButton toolStripButton2;
		private System.Windows.Forms.ToolStripButton toolStripButton1;
	}
}
