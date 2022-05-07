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
			this.panel_Background = new System.Windows.Forms.Panel();
			this.tableLayout = new System.Windows.Forms.TableLayoutPanel();
			this.tableLayout_Buttons = new System.Windows.Forms.TableLayoutPanel();
			this.button_Main_Import = new DarkUI.Controls.DarkButton();
			this.button_Main_Open = new DarkUI.Controls.DarkButton();
			this.button_Main_New = new DarkUI.Controls.DarkButton();
			this.panel_Bottom = new DarkUI.Controls.DarkPanel();
			this.button_OpenProject = new DarkUI.Controls.DarkButton();
			this.checkBox_Remember = new DarkUI.Controls.DarkCheckBox();
			this.treeView = new DarkUI.Controls.DarkTreeView();
			this.contextMenu_ProjectList = new DarkUI.Controls.DarkContextMenu();
			this.contextMenuItem_OpenProject = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
			this.contextMenuItem_OpenDirectory = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
			this.contextMenuItem_MoveUp = new System.Windows.Forms.ToolStripMenuItem();
			this.contextMenuItem_MoveDown = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
			this.contextMenuItem_Rename = new System.Windows.Forms.ToolStripMenuItem();
			this.contextMenuItem_Delete = new System.Windows.Forms.ToolStripMenuItem();
			this.panel_Logo = new System.Windows.Forms.Panel();
			this.panel_Background.SuspendLayout();
			this.tableLayout.SuspendLayout();
			this.tableLayout_Buttons.SuspendLayout();
			this.panel_Bottom.SuspendLayout();
			this.contextMenu_ProjectList.SuspendLayout();
			this.SuspendLayout();
			// 
			// panel_Background
			// 
			this.panel_Background.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.panel_Background.Controls.Add(this.tableLayout);
			this.panel_Background.Dock = System.Windows.Forms.DockStyle.Fill;
			this.panel_Background.Location = new System.Drawing.Point(0, 0);
			this.panel_Background.Name = "panel_Background";
			this.panel_Background.Size = new System.Drawing.Size(784, 511);
			this.panel_Background.TabIndex = 0;
			// 
			// tableLayout
			// 
			this.tableLayout.ColumnCount = 2;
			this.tableLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 360F));
			this.tableLayout.Controls.Add(this.tableLayout_Buttons, 1, 1);
			this.tableLayout.Controls.Add(this.panel_Bottom, 0, 2);
			this.tableLayout.Controls.Add(this.treeView, 0, 1);
			this.tableLayout.Controls.Add(this.panel_Logo, 0, 0);
			this.tableLayout.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tableLayout.Location = new System.Drawing.Point(0, 0);
			this.tableLayout.Margin = new System.Windows.Forms.Padding(0);
			this.tableLayout.Name = "tableLayout";
			this.tableLayout.RowCount = 3;
			this.tableLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 90F));
			this.tableLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 120F));
			this.tableLayout.Size = new System.Drawing.Size(782, 509);
			this.tableLayout.TabIndex = 1;
			// 
			// tableLayout_Buttons
			// 
			this.tableLayout_Buttons.ColumnCount = 1;
			this.tableLayout_Buttons.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayout_Buttons.Controls.Add(this.button_Main_Import, 0, 2);
			this.tableLayout_Buttons.Controls.Add(this.button_Main_Open, 0, 1);
			this.tableLayout_Buttons.Controls.Add(this.button_Main_New, 0, 0);
			this.tableLayout_Buttons.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tableLayout_Buttons.Location = new System.Drawing.Point(422, 90);
			this.tableLayout_Buttons.Margin = new System.Windows.Forms.Padding(0);
			this.tableLayout_Buttons.Name = "tableLayout_Buttons";
			this.tableLayout_Buttons.RowCount = 4;
			this.tableLayout_Buttons.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 80F));
			this.tableLayout_Buttons.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 80F));
			this.tableLayout_Buttons.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 80F));
			this.tableLayout_Buttons.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayout_Buttons.Size = new System.Drawing.Size(360, 299);
			this.tableLayout_Buttons.TabIndex = 1;
			// 
			// button_Main_Import
			// 
			this.button_Main_Import.Checked = false;
			this.button_Main_Import.Dock = System.Windows.Forms.DockStyle.Fill;
			this.button_Main_Import.Font = new System.Drawing.Font("Segoe UI", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
			this.button_Main_Import.Image = global::TombIDE.Properties.Resources.import_48;
			this.button_Main_Import.Location = new System.Drawing.Point(20, 163);
			this.button_Main_Import.Margin = new System.Windows.Forms.Padding(20, 3, 30, 3);
			this.button_Main_Import.Name = "button_Main_Import";
			this.button_Main_Import.Size = new System.Drawing.Size(310, 74);
			this.button_Main_Import.TabIndex = 2;
			this.button_Main_Import.Text = "Import project using a .exe file...";
			this.button_Main_Import.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
			this.button_Main_Import.Click += new System.EventHandler(this.button_Main_Import_Click);
			// 
			// button_Main_Open
			// 
			this.button_Main_Open.Checked = false;
			this.button_Main_Open.Dock = System.Windows.Forms.DockStyle.Fill;
			this.button_Main_Open.Font = new System.Drawing.Font("Segoe UI", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
			this.button_Main_Open.Image = global::TombIDE.Properties.Resources.open_48;
			this.button_Main_Open.Location = new System.Drawing.Point(20, 83);
			this.button_Main_Open.Margin = new System.Windows.Forms.Padding(20, 3, 30, 3);
			this.button_Main_Open.Name = "button_Main_Open";
			this.button_Main_Open.Size = new System.Drawing.Size(310, 74);
			this.button_Main_Open.TabIndex = 1;
			this.button_Main_Open.Text = "Open .trproj file...";
			this.button_Main_Open.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
			this.button_Main_Open.Click += new System.EventHandler(this.button_Main_Open_Click);
			// 
			// button_Main_New
			// 
			this.button_Main_New.Checked = false;
			this.button_Main_New.Dock = System.Windows.Forms.DockStyle.Fill;
			this.button_Main_New.Font = new System.Drawing.Font("Segoe UI", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
			this.button_Main_New.Image = global::TombIDE.Properties.Resources.add_48;
			this.button_Main_New.Location = new System.Drawing.Point(20, 3);
			this.button_Main_New.Margin = new System.Windows.Forms.Padding(20, 3, 30, 3);
			this.button_Main_New.Name = "button_Main_New";
			this.button_Main_New.Size = new System.Drawing.Size(310, 74);
			this.button_Main_New.TabIndex = 0;
			this.button_Main_New.Text = "Create a new project...";
			this.button_Main_New.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
			this.button_Main_New.Click += new System.EventHandler(this.button_Main_New_Click);
			// 
			// panel_Bottom
			// 
			this.panel_Bottom.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.tableLayout.SetColumnSpan(this.panel_Bottom, 2);
			this.panel_Bottom.Controls.Add(this.button_OpenProject);
			this.panel_Bottom.Controls.Add(this.checkBox_Remember);
			this.panel_Bottom.Dock = System.Windows.Forms.DockStyle.Fill;
			this.panel_Bottom.Location = new System.Drawing.Point(30, 409);
			this.panel_Bottom.Margin = new System.Windows.Forms.Padding(30, 20, 30, 30);
			this.panel_Bottom.Name = "panel_Bottom";
			this.panel_Bottom.Size = new System.Drawing.Size(722, 70);
			this.panel_Bottom.TabIndex = 2;
			// 
			// button_OpenProject
			// 
			this.button_OpenProject.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.button_OpenProject.Checked = false;
			this.button_OpenProject.Location = new System.Drawing.Point(552, 20);
			this.button_OpenProject.Margin = new System.Windows.Forms.Padding(0, 20, 20, 20);
			this.button_OpenProject.Name = "button_OpenProject";
			this.button_OpenProject.Size = new System.Drawing.Size(150, 30);
			this.button_OpenProject.TabIndex = 1;
			this.button_OpenProject.Text = "Open selected project";
			this.button_OpenProject.Click += new System.EventHandler(this.button_OpenProject_Click);
			// 
			// checkBox_Remember
			// 
			this.checkBox_Remember.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
			this.checkBox_Remember.Location = new System.Drawing.Point(20, 20);
			this.checkBox_Remember.Margin = new System.Windows.Forms.Padding(20, 20, 0, 20);
			this.checkBox_Remember.Name = "checkBox_Remember";
			this.checkBox_Remember.Size = new System.Drawing.Size(250, 30);
			this.checkBox_Remember.TabIndex = 0;
			this.checkBox_Remember.Text = "Remember project selection on next startup";
			// 
			// treeView
			// 
			this.treeView.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(48)))), ((int)(((byte)(48)))), ((int)(((byte)(48)))));
			this.treeView.ContextMenuStrip = this.contextMenu_ProjectList;
			this.treeView.Dock = System.Windows.Forms.DockStyle.Fill;
			this.treeView.ExpandOnDoubleClick = false;
			this.treeView.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
			this.treeView.ItemHeight = 40;
			this.treeView.Location = new System.Drawing.Point(30, 93);
			this.treeView.Margin = new System.Windows.Forms.Padding(30, 3, 3, 3);
			this.treeView.MaxDragChange = 40;
			this.treeView.Name = "treeView";
			this.treeView.ShowIcons = true;
			this.treeView.Size = new System.Drawing.Size(389, 293);
			this.treeView.TabIndex = 0;
			this.treeView.KeyDown += new System.Windows.Forms.KeyEventHandler(this.treeView_KeyDown);
			this.treeView.KeyUp += new System.Windows.Forms.KeyEventHandler(this.treeView_KeyUp);
			this.treeView.MouseClick += new System.Windows.Forms.MouseEventHandler(this.treeView_MouseClick);
			this.treeView.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.treeView_MouseDoubleClick);
			// 
			// contextMenu_ProjectList
			// 
			this.contextMenu_ProjectList.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
			this.contextMenu_ProjectList.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
			this.contextMenu_ProjectList.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.contextMenuItem_OpenProject,
            this.toolStripSeparator3,
            this.contextMenuItem_OpenDirectory,
            this.toolStripSeparator1,
            this.contextMenuItem_MoveUp,
            this.contextMenuItem_MoveDown,
            this.toolStripSeparator2,
            this.contextMenuItem_Rename,
            this.contextMenuItem_Delete});
			this.contextMenu_ProjectList.Name = "darkContextMenu1";
			this.contextMenu_ProjectList.Size = new System.Drawing.Size(213, 179);
			// 
			// contextMenuItem_OpenProject
			// 
			this.contextMenuItem_OpenProject.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
			this.contextMenuItem_OpenProject.Enabled = false;
			this.contextMenuItem_OpenProject.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.contextMenuItem_OpenProject.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(153)))), ((int)(((byte)(153)))), ((int)(((byte)(153)))));
			this.contextMenuItem_OpenProject.Name = "contextMenuItem_OpenProject";
			this.contextMenuItem_OpenProject.Size = new System.Drawing.Size(212, 22);
			this.contextMenuItem_OpenProject.Text = "Open selected project...";
			this.contextMenuItem_OpenProject.Click += new System.EventHandler(this.button_OpenProject_Click);
			// 
			// toolStripSeparator3
			// 
			this.toolStripSeparator3.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
			this.toolStripSeparator3.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
			this.toolStripSeparator3.Margin = new System.Windows.Forms.Padding(0, 0, 0, 1);
			this.toolStripSeparator3.Name = "toolStripSeparator3";
			this.toolStripSeparator3.Size = new System.Drawing.Size(209, 6);
			// 
			// contextMenuItem_OpenDirectory
			// 
			this.contextMenuItem_OpenDirectory.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
			this.contextMenuItem_OpenDirectory.Enabled = false;
			this.contextMenuItem_OpenDirectory.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(153)))), ((int)(((byte)(153)))), ((int)(((byte)(153)))));
			this.contextMenuItem_OpenDirectory.Image = global::TombIDE.Properties.Resources.forward_arrow_16;
			this.contextMenuItem_OpenDirectory.Name = "contextMenuItem_OpenDirectory";
			this.contextMenuItem_OpenDirectory.Size = new System.Drawing.Size(212, 22);
			this.contextMenuItem_OpenDirectory.Text = "Open project directory...";
			this.contextMenuItem_OpenDirectory.Click += new System.EventHandler(this.contextMenuItem_OpenDirectory_Click);
			// 
			// toolStripSeparator1
			// 
			this.toolStripSeparator1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
			this.toolStripSeparator1.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
			this.toolStripSeparator1.Margin = new System.Windows.Forms.Padding(0, 0, 0, 1);
			this.toolStripSeparator1.Name = "toolStripSeparator1";
			this.toolStripSeparator1.Size = new System.Drawing.Size(209, 6);
			// 
			// contextMenuItem_MoveUp
			// 
			this.contextMenuItem_MoveUp.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
			this.contextMenuItem_MoveUp.Enabled = false;
			this.contextMenuItem_MoveUp.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(153)))), ((int)(((byte)(153)))), ((int)(((byte)(153)))));
			this.contextMenuItem_MoveUp.Image = global::TombIDE.Properties.Resources.general_ArrowUp_16;
			this.contextMenuItem_MoveUp.Name = "contextMenuItem_MoveUp";
			this.contextMenuItem_MoveUp.Size = new System.Drawing.Size(212, 22);
			this.contextMenuItem_MoveUp.Text = "Move project up on list";
			this.contextMenuItem_MoveUp.Click += new System.EventHandler(this.contextMenuItem_MoveUp_Click);
			// 
			// contextMenuItem_MoveDown
			// 
			this.contextMenuItem_MoveDown.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
			this.contextMenuItem_MoveDown.Enabled = false;
			this.contextMenuItem_MoveDown.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(153)))), ((int)(((byte)(153)))), ((int)(((byte)(153)))));
			this.contextMenuItem_MoveDown.Image = global::TombIDE.Properties.Resources.general_ArrowDown_16;
			this.contextMenuItem_MoveDown.Name = "contextMenuItem_MoveDown";
			this.contextMenuItem_MoveDown.Size = new System.Drawing.Size(212, 22);
			this.contextMenuItem_MoveDown.Text = "Move project down on list";
			this.contextMenuItem_MoveDown.Click += new System.EventHandler(this.contextMenuItem_MoveDown_Click);
			// 
			// toolStripSeparator2
			// 
			this.toolStripSeparator2.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
			this.toolStripSeparator2.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
			this.toolStripSeparator2.Margin = new System.Windows.Forms.Padding(0, 0, 0, 1);
			this.toolStripSeparator2.Name = "toolStripSeparator2";
			this.toolStripSeparator2.Size = new System.Drawing.Size(209, 6);
			// 
			// contextMenuItem_Rename
			// 
			this.contextMenuItem_Rename.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
			this.contextMenuItem_Rename.Enabled = false;
			this.contextMenuItem_Rename.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(153)))), ((int)(((byte)(153)))), ((int)(((byte)(153)))));
			this.contextMenuItem_Rename.Image = global::TombIDE.Properties.Resources.general_edit_16;
			this.contextMenuItem_Rename.Name = "contextMenuItem_Rename";
			this.contextMenuItem_Rename.Size = new System.Drawing.Size(212, 22);
			this.contextMenuItem_Rename.Text = "Rename project...";
			this.contextMenuItem_Rename.Click += new System.EventHandler(this.contextMenuItem_Rename_Click);
			// 
			// contextMenuItem_Delete
			// 
			this.contextMenuItem_Delete.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
			this.contextMenuItem_Delete.Enabled = false;
			this.contextMenuItem_Delete.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(153)))), ((int)(((byte)(153)))), ((int)(((byte)(153)))));
			this.contextMenuItem_Delete.Image = global::TombIDE.Properties.Resources.general_trash_16;
			this.contextMenuItem_Delete.Name = "contextMenuItem_Delete";
			this.contextMenuItem_Delete.Size = new System.Drawing.Size(212, 22);
			this.contextMenuItem_Delete.Text = "Delete project from list";
			this.contextMenuItem_Delete.Click += new System.EventHandler(this.contextMenuItem_Delete_Click);
			// 
			// panel_Logo
			// 
			this.panel_Logo.BackgroundImage = global::TombIDE.Properties.Resources.TIDE_Logo;
			this.panel_Logo.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
			this.tableLayout.SetColumnSpan(this.panel_Logo, 2);
			this.panel_Logo.Dock = System.Windows.Forms.DockStyle.Left;
			this.panel_Logo.Location = new System.Drawing.Point(30, 30);
			this.panel_Logo.Margin = new System.Windows.Forms.Padding(30, 30, 0, 20);
			this.panel_Logo.Name = "panel_Logo";
			this.panel_Logo.Size = new System.Drawing.Size(145, 40);
			this.panel_Logo.TabIndex = 3;
			// 
			// FormStart
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(784, 511);
			this.Controls.Add(this.panel_Background);
			this.FlatBorder = true;
			this.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
			this.MinimumSize = new System.Drawing.Size(800, 550);
			this.Name = "FormStart";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "TombIDE - Start";
			this.panel_Background.ResumeLayout(false);
			this.tableLayout.ResumeLayout(false);
			this.tableLayout_Buttons.ResumeLayout(false);
			this.panel_Bottom.ResumeLayout(false);
			this.contextMenu_ProjectList.ResumeLayout(false);
			this.ResumeLayout(false);

		}

		#endregion

		private DarkUI.Controls.DarkButton button_Main_Import;
		private DarkUI.Controls.DarkButton button_Main_New;
		private DarkUI.Controls.DarkButton button_Main_Open;
		private DarkUI.Controls.DarkButton button_OpenProject;
		private DarkUI.Controls.DarkCheckBox checkBox_Remember;
		private DarkUI.Controls.DarkContextMenu contextMenu_ProjectList;
		private DarkUI.Controls.DarkPanel panel_Bottom;
		private DarkUI.Controls.DarkTreeView treeView;
		private System.Windows.Forms.Panel panel_Background;
		private System.Windows.Forms.Panel panel_Logo;
		private System.Windows.Forms.TableLayoutPanel tableLayout;
		private System.Windows.Forms.TableLayoutPanel tableLayout_Buttons;
		private System.Windows.Forms.ToolStripMenuItem contextMenuItem_Delete;
		private System.Windows.Forms.ToolStripMenuItem contextMenuItem_MoveDown;
		private System.Windows.Forms.ToolStripMenuItem contextMenuItem_MoveUp;
		private System.Windows.Forms.ToolStripMenuItem contextMenuItem_OpenDirectory;
		private System.Windows.Forms.ToolStripMenuItem contextMenuItem_OpenProject;
		private System.Windows.Forms.ToolStripMenuItem contextMenuItem_Rename;
		private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
		private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
		private System.Windows.Forms.ToolStripSeparator toolStripSeparator3;
	}
}