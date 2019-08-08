namespace TombIDE
{
	partial class FormStart
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormStart));
			this.button_Delete = new System.Windows.Forms.ToolStripButton();
			this.button_Import = new System.Windows.Forms.ToolStripButton();
			this.button_MoveDown = new System.Windows.Forms.ToolStripButton();
			this.button_MoveUp = new System.Windows.Forms.ToolStripButton();
			this.button_New = new System.Windows.Forms.ToolStripButton();
			this.button_Open = new System.Windows.Forms.ToolStripButton();
			this.button_OpenFolder = new System.Windows.Forms.ToolStripButton();
			this.button_OpenProject = new DarkUI.Controls.DarkButton();
			this.button_Rename = new System.Windows.Forms.ToolStripButton();
			this.checkBox_Remember = new DarkUI.Controls.DarkCheckBox();
			this.panel_01 = new System.Windows.Forms.Panel();
			this.treeView = new DarkUI.Controls.DarkTreeView();
			this.toolStrip = new DarkUI.Controls.DarkToolStrip();
			this.separator_01 = new System.Windows.Forms.ToolStripSeparator();
			this.separator_02 = new System.Windows.Forms.ToolStripSeparator();
			this.separator_03 = new System.Windows.Forms.ToolStripSeparator();
			this.panel_02 = new System.Windows.Forms.Panel();
			this.panel_01.SuspendLayout();
			this.toolStrip.SuspendLayout();
			this.panel_02.SuspendLayout();
			this.SuspendLayout();
			// 
			// button_Delete
			// 
			this.button_Delete.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
			this.button_Delete.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.button_Delete.Enabled = false;
			this.button_Delete.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
			this.button_Delete.Image = global::TombIDE.Properties.Resources.general_trash_16;
			this.button_Delete.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.button_Delete.Name = "button_Delete";
			this.button_Delete.Size = new System.Drawing.Size(23, 27);
			this.button_Delete.Text = "Delete Project from List";
			this.button_Delete.Click += new System.EventHandler(this.button_Delete_Click);
			// 
			// button_Import
			// 
			this.button_Import.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
			this.button_Import.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.button_Import.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
			this.button_Import.Image = global::TombIDE.Properties.Resources.general_Import_16;
			this.button_Import.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.button_Import.Name = "button_Import";
			this.button_Import.Size = new System.Drawing.Size(23, 27);
			this.button_Import.Text = "Import Project from .exe";
			this.button_Import.Click += new System.EventHandler(this.button_Import_Click);
			// 
			// button_MoveDown
			// 
			this.button_MoveDown.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
			this.button_MoveDown.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.button_MoveDown.Enabled = false;
			this.button_MoveDown.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
			this.button_MoveDown.Image = global::TombIDE.Properties.Resources.general_ArrowDown_16;
			this.button_MoveDown.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.button_MoveDown.Name = "button_MoveDown";
			this.button_MoveDown.Size = new System.Drawing.Size(23, 27);
			this.button_MoveDown.Text = "Move Project Down on List";
			this.button_MoveDown.Click += new System.EventHandler(this.button_MoveDown_Click);
			// 
			// button_MoveUp
			// 
			this.button_MoveUp.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
			this.button_MoveUp.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.button_MoveUp.Enabled = false;
			this.button_MoveUp.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
			this.button_MoveUp.Image = global::TombIDE.Properties.Resources.general_ArrowUp_16;
			this.button_MoveUp.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.button_MoveUp.Name = "button_MoveUp";
			this.button_MoveUp.Size = new System.Drawing.Size(23, 27);
			this.button_MoveUp.Text = "Move Project Up on List";
			this.button_MoveUp.Click += new System.EventHandler(this.button_MoveUp_Click);
			// 
			// button_New
			// 
			this.button_New.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
			this.button_New.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.button_New.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
			this.button_New.Image = global::TombIDE.Properties.Resources.general_plus_math_16;
			this.button_New.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.button_New.Name = "button_New";
			this.button_New.Size = new System.Drawing.Size(23, 27);
			this.button_New.Text = "Create New Project...";
			this.button_New.Click += new System.EventHandler(this.button_New_Click);
			// 
			// button_Open
			// 
			this.button_Open.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
			this.button_Open.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.button_Open.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
			this.button_Open.Image = global::TombIDE.Properties.Resources.general_Open_16;
			this.button_Open.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.button_Open.Name = "button_Open";
			this.button_Open.Size = new System.Drawing.Size(23, 27);
			this.button_Open.Text = "Open .trproj File";
			this.button_Open.Click += new System.EventHandler(this.button_Open_Click);
			// 
			// button_OpenFolder
			// 
			this.button_OpenFolder.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
			this.button_OpenFolder.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.button_OpenFolder.Enabled = false;
			this.button_OpenFolder.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
			this.button_OpenFolder.Image = global::TombIDE.Properties.Resources.forward_arrow_16;
			this.button_OpenFolder.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.button_OpenFolder.Name = "button_OpenFolder";
			this.button_OpenFolder.Size = new System.Drawing.Size(23, 27);
			this.button_OpenFolder.Text = "Open Project Folder in Explorer";
			this.button_OpenFolder.Click += new System.EventHandler(this.button_OpenFolder_Click);
			// 
			// button_OpenProject
			// 
			this.button_OpenProject.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.button_OpenProject.Enabled = false;
			this.button_OpenProject.Location = new System.Drawing.Point(358, 5);
			this.button_OpenProject.Margin = new System.Windows.Forms.Padding(3, 3, 0, 6);
			this.button_OpenProject.Name = "button_OpenProject";
			this.button_OpenProject.Size = new System.Drawing.Size(128, 28);
			this.button_OpenProject.TabIndex = 1;
			this.button_OpenProject.Text = "Open Selected Project";
			this.button_OpenProject.Click += new System.EventHandler(this.button_OpenProject_Click);
			// 
			// button_Rename
			// 
			this.button_Rename.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
			this.button_Rename.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.button_Rename.Enabled = false;
			this.button_Rename.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
			this.button_Rename.Image = global::TombIDE.Properties.Resources.general_edit_16;
			this.button_Rename.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.button_Rename.Name = "button_Rename";
			this.button_Rename.Size = new System.Drawing.Size(23, 27);
			this.button_Rename.Text = "Rename Project...";
			this.button_Rename.Click += new System.EventHandler(this.button_Rename_Click);
			// 
			// checkBox_Remember
			// 
			this.checkBox_Remember.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.checkBox_Remember.Location = new System.Drawing.Point(11, 5);
			this.checkBox_Remember.Margin = new System.Windows.Forms.Padding(3, 3, 3, 6);
			this.checkBox_Remember.Name = "checkBox_Remember";
			this.checkBox_Remember.Size = new System.Drawing.Size(200, 28);
			this.checkBox_Remember.TabIndex = 0;
			this.checkBox_Remember.Text = "Remember project choice on startup";
			// 
			// panel_01
			// 
			this.panel_01.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.panel_01.Controls.Add(this.treeView);
			this.panel_01.Controls.Add(this.toolStrip);
			this.panel_01.Dock = System.Windows.Forms.DockStyle.Fill;
			this.panel_01.Location = new System.Drawing.Point(0, 0);
			this.panel_01.Name = "panel_01";
			this.panel_01.Size = new System.Drawing.Size(496, 432);
			this.panel_01.TabIndex = 0;
			// 
			// treeView
			// 
			this.treeView.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(48)))), ((int)(((byte)(48)))), ((int)(((byte)(48)))));
			this.treeView.Dock = System.Windows.Forms.DockStyle.Fill;
			this.treeView.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
			this.treeView.ItemHeight = 40;
			this.treeView.Location = new System.Drawing.Point(0, 30);
			this.treeView.MaxDragChange = 40;
			this.treeView.Name = "treeView";
			this.treeView.ShowIcons = true;
			this.treeView.Size = new System.Drawing.Size(494, 400);
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
            this.button_Open,
            this.button_Import,
            this.separator_01,
            this.button_Rename,
            this.button_Delete,
            this.separator_02,
            this.button_MoveUp,
            this.button_MoveDown,
            this.separator_03,
            this.button_OpenFolder});
			this.toolStrip.Location = new System.Drawing.Point(0, 0);
			this.toolStrip.Name = "toolStrip";
			this.toolStrip.Padding = new System.Windows.Forms.Padding(5, 0, 1, 0);
			this.toolStrip.Size = new System.Drawing.Size(494, 30);
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
			// panel_02
			// 
			this.panel_02.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.panel_02.Controls.Add(this.button_OpenProject);
			this.panel_02.Controls.Add(this.checkBox_Remember);
			this.panel_02.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.panel_02.Location = new System.Drawing.Point(0, 432);
			this.panel_02.Name = "panel_02";
			this.panel_02.Size = new System.Drawing.Size(496, 41);
			this.panel_02.TabIndex = 1;
			// 
			// FormStart
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(496, 473);
			this.Controls.Add(this.panel_01);
			this.Controls.Add(this.panel_02);
			this.FlatBorder = true;
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.MinimumSize = new System.Drawing.Size(512, 512);
			this.Name = "FormStart";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "TombIDE - Start";
			this.panel_01.ResumeLayout(false);
			this.toolStrip.ResumeLayout(false);
			this.toolStrip.PerformLayout();
			this.panel_02.ResumeLayout(false);
			this.ResumeLayout(false);

		}

		#endregion

		private DarkUI.Controls.DarkButton button_OpenProject;
		private DarkUI.Controls.DarkCheckBox checkBox_Remember;
		private DarkUI.Controls.DarkToolStrip toolStrip;
		private DarkUI.Controls.DarkTreeView treeView;
		private System.Windows.Forms.Panel panel_01;
		private System.Windows.Forms.Panel panel_02;
		private System.Windows.Forms.ToolStripButton button_Delete;
		private System.Windows.Forms.ToolStripButton button_Import;
		private System.Windows.Forms.ToolStripButton button_MoveDown;
		private System.Windows.Forms.ToolStripButton button_MoveUp;
		private System.Windows.Forms.ToolStripButton button_New;
		private System.Windows.Forms.ToolStripButton button_Open;
		private System.Windows.Forms.ToolStripButton button_OpenFolder;
		private System.Windows.Forms.ToolStripButton button_Rename;
		private System.Windows.Forms.ToolStripSeparator separator_01;
		private System.Windows.Forms.ToolStripSeparator separator_02;
		private System.Windows.Forms.ToolStripSeparator separator_03;
	}
}