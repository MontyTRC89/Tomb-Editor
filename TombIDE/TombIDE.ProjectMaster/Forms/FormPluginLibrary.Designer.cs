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
			this.button_Delete = new System.Windows.Forms.ToolStripButton();
			this.button_Install = new DarkUI.Controls.DarkButton();
			this.button_OpenArchive = new System.Windows.Forms.ToolStripButton();
			this.panel_01 = new System.Windows.Forms.Panel();
			this.treeView = new DarkUI.Controls.DarkTreeView();
			this.toolStrip = new DarkUI.Controls.DarkToolStrip();
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
			this.button_Delete.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
			this.button_Delete.Image = global::TombIDE.ProjectMaster.Properties.Resources.general_trash_16;
			this.button_Delete.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.button_Delete.Name = "button_Delete";
			this.button_Delete.Size = new System.Drawing.Size(23, 25);
			this.button_Delete.Text = "Delete Plugin";
			// 
			// button_Install
			// 
			this.button_Install.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.button_Install.Location = new System.Drawing.Point(8, 5);
			this.button_Install.Margin = new System.Windows.Forms.Padding(0, 3, 0, 6);
			this.button_Install.Name = "button_Install";
			this.button_Install.Size = new System.Drawing.Size(446, 28);
			this.button_Install.TabIndex = 0;
			this.button_Install.Text = "Install Selected Plugins to the Current Project";
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
			this.button_OpenArchive.Text = "Add Plugin from .zip / .rar Archive";
			this.button_OpenArchive.Click += new System.EventHandler(this.button_OpenArchive_Click);
			// 
			// panel_01
			// 
			this.panel_01.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.panel_01.Controls.Add(this.treeView);
			this.panel_01.Controls.Add(this.toolStrip);
			this.panel_01.Dock = System.Windows.Forms.DockStyle.Fill;
			this.panel_01.Location = new System.Drawing.Point(0, 0);
			this.panel_01.Name = "panel_01";
			this.panel_01.Size = new System.Drawing.Size(464, 601);
			this.panel_01.TabIndex = 0;
			// 
			// treeView
			// 
			this.treeView.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(48)))), ((int)(((byte)(48)))), ((int)(((byte)(48)))));
			this.treeView.Dock = System.Windows.Forms.DockStyle.Fill;
			this.treeView.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
			this.treeView.ItemHeight = 40;
			this.treeView.Location = new System.Drawing.Point(0, 28);
			this.treeView.MaxDragChange = 40;
			this.treeView.MultiSelect = true;
			this.treeView.Name = "treeView";
			this.treeView.Size = new System.Drawing.Size(462, 571);
			this.treeView.TabIndex = 0;
			// 
			// toolStrip
			// 
			this.toolStrip.AutoSize = false;
			this.toolStrip.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
			this.toolStrip.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
			this.toolStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.button_OpenArchive,
            this.button_Delete});
			this.toolStrip.Location = new System.Drawing.Point(0, 0);
			this.toolStrip.Name = "toolStrip";
			this.toolStrip.Padding = new System.Windows.Forms.Padding(5, 0, 1, 0);
			this.toolStrip.Size = new System.Drawing.Size(462, 28);
			this.toolStrip.TabIndex = 1;
			// 
			// panel_02
			// 
			this.panel_02.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.panel_02.Controls.Add(this.button_Install);
			this.panel_02.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.panel_02.Location = new System.Drawing.Point(0, 560);
			this.panel_02.Name = "panel_02";
			this.panel_02.Size = new System.Drawing.Size(464, 41);
			this.panel_02.TabIndex = 1;
			// 
			// FormPluginLibrary
			// 
			this.AcceptButton = this.button_Install;
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(464, 601);
			this.Controls.Add(this.panel_02);
			this.Controls.Add(this.panel_01);
			this.FlatBorder = true;
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
			this.MaximizeBox = false;
			this.Name = "FormPluginLibrary";
			this.ShowIcon = false;
			this.ShowInTaskbar = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "Install Plugin from Library";
			this.panel_01.ResumeLayout(false);
			this.toolStrip.ResumeLayout(false);
			this.toolStrip.PerformLayout();
			this.panel_02.ResumeLayout(false);
			this.ResumeLayout(false);

		}

		#endregion

		private DarkUI.Controls.DarkButton button_Install;
		private DarkUI.Controls.DarkToolStrip toolStrip;
		private DarkUI.Controls.DarkTreeView treeView;
		private System.Windows.Forms.Panel panel_01;
		private System.Windows.Forms.Panel panel_02;
		private System.Windows.Forms.ToolStripButton button_Delete;
		private System.Windows.Forms.ToolStripButton button_OpenArchive;
	}
}