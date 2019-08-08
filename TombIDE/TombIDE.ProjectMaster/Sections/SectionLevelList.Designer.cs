namespace TombIDE.ProjectMaster
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
			this.button_Delete = new System.Windows.Forms.ToolStripButton();
			this.button_Import = new System.Windows.Forms.ToolStripButton();
			this.button_MoveDown = new System.Windows.Forms.ToolStripButton();
			this.button_MoveUp = new System.Windows.Forms.ToolStripButton();
			this.button_New = new System.Windows.Forms.ToolStripButton();
			this.button_OpenInExplorer = new System.Windows.Forms.ToolStripButton();
			this.button_Refresh = new System.Windows.Forms.ToolStripButton();
			this.button_Rename = new System.Windows.Forms.ToolStripButton();
			this.button_ViewFileNames = new System.Windows.Forms.ToolStripButton();
			this.contextMenu = new DarkUI.Controls.DarkContextMenu();
			this.menuItem_OpenLevel = new System.Windows.Forms.ToolStripMenuItem();
			this.sectionPanel = new DarkUI.Controls.DarkSectionPanel();
			this.treeView = new DarkUI.Controls.DarkTreeView();
			this.toolStrip = new DarkUI.Controls.DarkToolStrip();
			this.separator_01 = new System.Windows.Forms.ToolStripSeparator();
			this.separator_02 = new System.Windows.Forms.ToolStripSeparator();
			this.separator_03 = new System.Windows.Forms.ToolStripSeparator();
			this.contextMenu.SuspendLayout();
			this.sectionPanel.SuspendLayout();
			this.toolStrip.SuspendLayout();
			this.SuspendLayout();
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
			// button_Import
			// 
			this.button_Import.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
			this.button_Import.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.button_Import.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
			this.button_Import.Image = global::TombIDE.ProjectMaster.Properties.Resources.general_Import_16;
			this.button_Import.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.button_Import.Name = "button_Import";
			this.button_Import.Size = new System.Drawing.Size(23, 27);
			this.button_Import.Text = "Import Level...";
			this.button_Import.Click += new System.EventHandler(this.button_Import_Click);
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
			// button_New
			// 
			this.button_New.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
			this.button_New.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.button_New.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
			this.button_New.Image = global::TombIDE.ProjectMaster.Properties.Resources.general_plus_math_16;
			this.button_New.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.button_New.Name = "button_New";
			this.button_New.Size = new System.Drawing.Size(23, 27);
			this.button_New.Text = "Create New Level...";
			this.button_New.Click += new System.EventHandler(this.button_New_Click);
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
			// button_ViewFileNames
			// 
			this.button_ViewFileNames.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
			this.button_ViewFileNames.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
			this.button_ViewFileNames.Checked = true;
			this.button_ViewFileNames.CheckState = System.Windows.Forms.CheckState.Checked;
			this.button_ViewFileNames.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.button_ViewFileNames.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
			this.button_ViewFileNames.Image = global::TombIDE.ProjectMaster.Properties.Resources.asterisk_filled_16;
			this.button_ViewFileNames.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.button_ViewFileNames.Name = "button_ViewFileNames";
			this.button_ViewFileNames.Size = new System.Drawing.Size(23, 27);
			this.button_ViewFileNames.Text = "View File Names";
			this.button_ViewFileNames.Click += new System.EventHandler(this.button_ViewFileNames_Click);
			// 
			// contextMenu
			// 
			this.contextMenu.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
			this.contextMenu.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
			this.contextMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.menuItem_OpenLevel});
			this.contextMenu.Name = "contextMenu";
			this.contextMenu.Size = new System.Drawing.Size(218, 26);
			// 
			// menuItem_OpenLevel
			// 
			this.menuItem_OpenLevel.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
			this.menuItem_OpenLevel.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
			this.menuItem_OpenLevel.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
			this.menuItem_OpenLevel.Name = "menuItem_OpenLevel";
			this.menuItem_OpenLevel.Size = new System.Drawing.Size(217, 22);
			this.menuItem_OpenLevel.Text = "Open Level in TombEditor";
			this.menuItem_OpenLevel.Click += new System.EventHandler(this.menuItem_OpenLevel_Click);
			// 
			// sectionPanel
			// 
			this.sectionPanel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.sectionPanel.Controls.Add(this.treeView);
			this.sectionPanel.Controls.Add(this.toolStrip);
			this.sectionPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.sectionPanel.Location = new System.Drawing.Point(0, 0);
			this.sectionPanel.Name = "sectionPanel";
			this.sectionPanel.SectionHeader = "Level List";
			this.sectionPanel.Size = new System.Drawing.Size(320, 320);
			this.sectionPanel.TabIndex = 0;
			// 
			// treeView
			// 
			this.treeView.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(48)))), ((int)(((byte)(48)))), ((int)(((byte)(48)))));
			this.treeView.Dock = System.Windows.Forms.DockStyle.Fill;
			this.treeView.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
			this.treeView.ItemHeight = 40;
			this.treeView.Location = new System.Drawing.Point(1, 55);
			this.treeView.MaxDragChange = 40;
			this.treeView.Name = "treeView";
			this.treeView.Size = new System.Drawing.Size(316, 262);
			this.treeView.TabIndex = 0;
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
			this.toolStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.button_New,
            this.button_Import,
            this.separator_01,
            this.button_Rename,
            this.button_Delete,
            this.separator_02,
            this.button_MoveUp,
            this.button_MoveDown,
            this.separator_03,
            this.button_OpenInExplorer,
            this.button_ViewFileNames,
            this.button_Refresh});
			this.toolStrip.Location = new System.Drawing.Point(1, 25);
			this.toolStrip.Name = "toolStrip";
			this.toolStrip.Padding = new System.Windows.Forms.Padding(5, 0, 1, 0);
			this.toolStrip.Size = new System.Drawing.Size(316, 30);
			this.toolStrip.TabIndex = 1;
			// 
			// separator_01
			// 
			this.separator_01.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
			this.separator_01.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
			this.separator_01.Margin = new System.Windows.Forms.Padding(0, 0, 2, 0);
			this.separator_01.Name = "separator_01";
			this.separator_01.Size = new System.Drawing.Size(6, 30);
			// 
			// separator_02
			// 
			this.separator_02.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
			this.separator_02.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
			this.separator_02.Margin = new System.Windows.Forms.Padding(0, 0, 2, 0);
			this.separator_02.Name = "separator_02";
			this.separator_02.Size = new System.Drawing.Size(6, 30);
			// 
			// separator_03
			// 
			this.separator_03.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
			this.separator_03.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
			this.separator_03.Margin = new System.Windows.Forms.Padding(0, 0, 2, 0);
			this.separator_03.Name = "separator_03";
			this.separator_03.Size = new System.Drawing.Size(6, 30);
			// 
			// SectionLevelList
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
			this.Controls.Add(this.sectionPanel);
			this.Name = "SectionLevelList";
			this.Size = new System.Drawing.Size(320, 320);
			this.contextMenu.ResumeLayout(false);
			this.sectionPanel.ResumeLayout(false);
			this.toolStrip.ResumeLayout(false);
			this.toolStrip.PerformLayout();
			this.ResumeLayout(false);

		}

		#endregion

		private DarkUI.Controls.DarkContextMenu contextMenu;
		private DarkUI.Controls.DarkSectionPanel sectionPanel;
		private DarkUI.Controls.DarkToolStrip toolStrip;
		private DarkUI.Controls.DarkTreeView treeView;
		private System.Windows.Forms.ToolStripButton button_Delete;
		private System.Windows.Forms.ToolStripButton button_Import;
		private System.Windows.Forms.ToolStripButton button_MoveDown;
		private System.Windows.Forms.ToolStripButton button_MoveUp;
		private System.Windows.Forms.ToolStripButton button_New;
		private System.Windows.Forms.ToolStripButton button_OpenInExplorer;
		private System.Windows.Forms.ToolStripButton button_Refresh;
		private System.Windows.Forms.ToolStripButton button_Rename;
		private System.Windows.Forms.ToolStripButton button_ViewFileNames;
		private System.Windows.Forms.ToolStripMenuItem menuItem_OpenLevel;
		private System.Windows.Forms.ToolStripSeparator separator_01;
		private System.Windows.Forms.ToolStripSeparator separator_02;
		private System.Windows.Forms.ToolStripSeparator separator_03;
	}
}
