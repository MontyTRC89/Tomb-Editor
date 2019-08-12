namespace TombIDE.ProjectMaster
{
	partial class FormPluginLibrary
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
			this.button_Apply = new DarkUI.Controls.DarkButton();
			this.button_Cancel = new DarkUI.Controls.DarkButton();
			this.button_Delete = new System.Windows.Forms.ToolStripButton();
			this.button_Install = new DarkUI.Controls.DarkButton();
			this.button_OpenArchive = new System.Windows.Forms.ToolStripButton();
			this.button_OpenFolder = new System.Windows.Forms.ToolStripButton();
			this.button_Uninstall = new DarkUI.Controls.DarkButton();
			this.darkSectionPanel1 = new DarkUI.Controls.DarkSectionPanel();
			this.treeView_AvailablePlugins = new DarkUI.Controls.DarkTreeView();
			this.toolStrip = new DarkUI.Controls.DarkToolStrip();
			this.separator_01 = new System.Windows.Forms.ToolStripSeparator();
			this.darkSectionPanel2 = new DarkUI.Controls.DarkSectionPanel();
			this.treeView_Installed = new DarkUI.Controls.DarkTreeView();
			this.panel_01 = new System.Windows.Forms.Panel();
			this.panel_02 = new System.Windows.Forms.Panel();
			this.darkSectionPanel1.SuspendLayout();
			this.toolStrip.SuspendLayout();
			this.darkSectionPanel2.SuspendLayout();
			this.panel_01.SuspendLayout();
			this.panel_02.SuspendLayout();
			this.SuspendLayout();
			// 
			// button_Apply
			// 
			this.button_Apply.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.button_Apply.Location = new System.Drawing.Point(541, 8);
			this.button_Apply.Margin = new System.Windows.Forms.Padding(3, 9, 0, 0);
			this.button_Apply.Name = "button_Apply";
			this.button_Apply.Size = new System.Drawing.Size(75, 23);
			this.button_Apply.TabIndex = 0;
			this.button_Apply.Text = "Apply";
			// 
			// button_Cancel
			// 
			this.button_Cancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.button_Cancel.Location = new System.Drawing.Point(619, 8);
			this.button_Cancel.Margin = new System.Windows.Forms.Padding(3, 9, 0, 0);
			this.button_Cancel.Name = "button_Cancel";
			this.button_Cancel.Size = new System.Drawing.Size(75, 23);
			this.button_Cancel.TabIndex = 1;
			this.button_Cancel.Text = "Cancel";
			// 
			// button_Delete
			// 
			this.button_Delete.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
			this.button_Delete.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.button_Delete.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
			this.button_Delete.Image = global::TombIDE.ProjectMaster.Properties.Resources.general_trash_16;
			this.button_Delete.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.button_Delete.Name = "button_Delete";
			this.button_Delete.Size = new System.Drawing.Size(23, 25);
			this.button_Delete.Text = "Delete Plugin from TombIDE";
			// 
			// button_Install
			// 
			this.button_Install.Anchor = System.Windows.Forms.AnchorStyles.None;
			this.button_Install.Location = new System.Drawing.Point(336, 8);
			this.button_Install.Margin = new System.Windows.Forms.Padding(3, 0, 3, 3);
			this.button_Install.Name = "button_Install";
			this.button_Install.Size = new System.Drawing.Size(30, 204);
			this.button_Install.TabIndex = 2;
			this.button_Install.Text = ">>";
			this.button_Install.Click += new System.EventHandler(this.button_Install_Click);
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
			this.button_OpenArchive.Text = "Add Plugin from Archive";
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
			this.button_OpenFolder.Text = "Add Plugin from Folder";
			// 
			// button_Uninstall
			// 
			this.button_Uninstall.Anchor = System.Windows.Forms.AnchorStyles.None;
			this.button_Uninstall.Location = new System.Drawing.Point(336, 218);
			this.button_Uninstall.Margin = new System.Windows.Forms.Padding(3, 3, 3, 6);
			this.button_Uninstall.Name = "button_Uninstall";
			this.button_Uninstall.Size = new System.Drawing.Size(30, 204);
			this.button_Uninstall.TabIndex = 3;
			this.button_Uninstall.Text = "<<";
			this.button_Uninstall.Click += new System.EventHandler(this.button_Uninstall_Click);
			// 
			// darkSectionPanel1
			// 
			this.darkSectionPanel1.Controls.Add(this.treeView_AvailablePlugins);
			this.darkSectionPanel1.Controls.Add(this.toolStrip);
			this.darkSectionPanel1.Dock = System.Windows.Forms.DockStyle.Left;
			this.darkSectionPanel1.Location = new System.Drawing.Point(0, 0);
			this.darkSectionPanel1.Name = "darkSectionPanel1";
			this.darkSectionPanel1.SectionHeader = "Available Plugins (TombIDE)";
			this.darkSectionPanel1.Size = new System.Drawing.Size(330, 430);
			this.darkSectionPanel1.TabIndex = 0;
			// 
			// treeView_AvailablePlugins
			// 
			this.treeView_AvailablePlugins.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(48)))), ((int)(((byte)(48)))), ((int)(((byte)(48)))));
			this.treeView_AvailablePlugins.Dock = System.Windows.Forms.DockStyle.Fill;
			this.treeView_AvailablePlugins.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
			this.treeView_AvailablePlugins.ItemHeight = 40;
			this.treeView_AvailablePlugins.Location = new System.Drawing.Point(1, 53);
			this.treeView_AvailablePlugins.MaxDragChange = 40;
			this.treeView_AvailablePlugins.MultiSelect = true;
			this.treeView_AvailablePlugins.Name = "treeView_AvailablePlugins";
			this.treeView_AvailablePlugins.Size = new System.Drawing.Size(328, 376);
			this.treeView_AvailablePlugins.TabIndex = 0;
			// 
			// toolStrip
			// 
			this.toolStrip.AutoSize = false;
			this.toolStrip.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
			this.toolStrip.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
			this.toolStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.button_OpenArchive,
            this.button_OpenFolder,
            this.separator_01,
            this.button_Delete});
			this.toolStrip.Location = new System.Drawing.Point(1, 25);
			this.toolStrip.Name = "toolStrip";
			this.toolStrip.Padding = new System.Windows.Forms.Padding(5, 0, 1, 0);
			this.toolStrip.Size = new System.Drawing.Size(328, 28);
			this.toolStrip.TabIndex = 1;
			// 
			// separator_01
			// 
			this.separator_01.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
			this.separator_01.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
			this.separator_01.Margin = new System.Windows.Forms.Padding(0, 0, 2, 0);
			this.separator_01.Name = "separator_01";
			this.separator_01.Size = new System.Drawing.Size(6, 28);
			// 
			// darkSectionPanel2
			// 
			this.darkSectionPanel2.Controls.Add(this.treeView_Installed);
			this.darkSectionPanel2.Dock = System.Windows.Forms.DockStyle.Right;
			this.darkSectionPanel2.Location = new System.Drawing.Point(372, 0);
			this.darkSectionPanel2.Name = "darkSectionPanel2";
			this.darkSectionPanel2.SectionHeader = "Installed Plugins (Current Project)";
			this.darkSectionPanel2.Size = new System.Drawing.Size(330, 430);
			this.darkSectionPanel2.TabIndex = 1;
			// 
			// treeView_Installed
			// 
			this.treeView_Installed.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(48)))), ((int)(((byte)(48)))), ((int)(((byte)(48)))));
			this.treeView_Installed.Dock = System.Windows.Forms.DockStyle.Fill;
			this.treeView_Installed.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
			this.treeView_Installed.ItemHeight = 40;
			this.treeView_Installed.Location = new System.Drawing.Point(1, 25);
			this.treeView_Installed.MaxDragChange = 40;
			this.treeView_Installed.MultiSelect = true;
			this.treeView_Installed.Name = "treeView_Installed";
			this.treeView_Installed.Size = new System.Drawing.Size(328, 404);
			this.treeView_Installed.TabIndex = 0;
			// 
			// panel_01
			// 
			this.panel_01.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.panel_01.Controls.Add(this.button_Uninstall);
			this.panel_01.Controls.Add(this.button_Install);
			this.panel_01.Controls.Add(this.darkSectionPanel2);
			this.panel_01.Controls.Add(this.darkSectionPanel1);
			this.panel_01.Dock = System.Windows.Forms.DockStyle.Fill;
			this.panel_01.Location = new System.Drawing.Point(0, 0);
			this.panel_01.Name = "panel_01";
			this.panel_01.Size = new System.Drawing.Size(704, 432);
			this.panel_01.TabIndex = 0;
			// 
			// panel_02
			// 
			this.panel_02.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.panel_02.Controls.Add(this.button_Cancel);
			this.panel_02.Controls.Add(this.button_Apply);
			this.panel_02.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.panel_02.Location = new System.Drawing.Point(0, 432);
			this.panel_02.Name = "panel_02";
			this.panel_02.Size = new System.Drawing.Size(704, 41);
			this.panel_02.TabIndex = 1;
			// 
			// FormPluginLibrary
			// 
			this.AcceptButton = this.button_Apply;
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(704, 473);
			this.Controls.Add(this.panel_01);
			this.Controls.Add(this.panel_02);
			this.FlatBorder = true;
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
			this.MaximizeBox = false;
			this.Name = "FormPluginLibrary";
			this.ShowIcon = false;
			this.ShowInTaskbar = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "Install Plugin from Library";
			this.darkSectionPanel1.ResumeLayout(false);
			this.toolStrip.ResumeLayout(false);
			this.toolStrip.PerformLayout();
			this.darkSectionPanel2.ResumeLayout(false);
			this.panel_01.ResumeLayout(false);
			this.panel_02.ResumeLayout(false);
			this.ResumeLayout(false);

		}

		#endregion

		private DarkUI.Controls.DarkButton button_Apply;
		private DarkUI.Controls.DarkButton button_Cancel;
		private DarkUI.Controls.DarkButton button_Install;
		private DarkUI.Controls.DarkButton button_Uninstall;
		private DarkUI.Controls.DarkSectionPanel darkSectionPanel1;
		private DarkUI.Controls.DarkSectionPanel darkSectionPanel2;
		private DarkUI.Controls.DarkToolStrip toolStrip;
		private DarkUI.Controls.DarkTreeView treeView_AvailablePlugins;
		private DarkUI.Controls.DarkTreeView treeView_Installed;
		private System.Windows.Forms.Panel panel_01;
		private System.Windows.Forms.Panel panel_02;
		private System.Windows.Forms.ToolStripButton button_Delete;
		private System.Windows.Forms.ToolStripButton button_OpenArchive;
		private System.Windows.Forms.ToolStripButton button_OpenFolder;
		private System.Windows.Forms.ToolStripSeparator separator_01;
	}
}