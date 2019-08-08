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
			this.button_Install = new System.Windows.Forms.ToolStripButton();
			this.button_Uninstall = new System.Windows.Forms.ToolStripButton();
			this.label_01 = new DarkUI.Controls.DarkLabel();
			this.label_02 = new DarkUI.Controls.DarkLabel();
			this.panel_List = new System.Windows.Forms.Panel();
			this.treeView = new DarkUI.Controls.DarkTreeView();
			this.toolStrip = new DarkUI.Controls.DarkToolStrip();
			this.panel_Logo = new System.Windows.Forms.Panel();
			this.panel_Properties = new System.Windows.Forms.Panel();
			this.tabControl = new System.Windows.Forms.CustomTabControl();
			this.tabPage_Overview = new System.Windows.Forms.TabPage();
			this.textBox_DLLName = new System.Windows.Forms.TextBox();
			this.textBox_Title = new System.Windows.Forms.TextBox();
			this.tabPage_Description = new System.Windows.Forms.TabPage();
			this.richTextBox_Description = new System.Windows.Forms.RichTextBox();
			this.tabPage_Changelog = new System.Windows.Forms.TabPage();
			this.richTextBox_Changelog = new System.Windows.Forms.RichTextBox();
			this.sectionPanel = new DarkUI.Controls.DarkSectionPanel();
			this.panel_List.SuspendLayout();
			this.toolStrip.SuspendLayout();
			this.panel_Properties.SuspendLayout();
			this.tabControl.SuspendLayout();
			this.tabPage_Overview.SuspendLayout();
			this.tabPage_Description.SuspendLayout();
			this.tabPage_Changelog.SuspendLayout();
			this.sectionPanel.SuspendLayout();
			this.SuspendLayout();
			// 
			// button_Install
			// 
			this.button_Install.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
			this.button_Install.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.button_Install.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
			this.button_Install.Image = global::TombIDE.ProjectMaster.Properties.Resources.general_plus_math_16;
			this.button_Install.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.button_Install.Name = "button_Install";
			this.button_Install.Size = new System.Drawing.Size(23, 25);
			this.button_Install.Text = "Install New Plugin";
			// 
			// button_Uninstall
			// 
			this.button_Uninstall.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
			this.button_Uninstall.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.button_Uninstall.Enabled = false;
			this.button_Uninstall.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
			this.button_Uninstall.Image = global::TombIDE.ProjectMaster.Properties.Resources.general_trash_16;
			this.button_Uninstall.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.button_Uninstall.Name = "button_Uninstall";
			this.button_Uninstall.Size = new System.Drawing.Size(23, 25);
			this.button_Uninstall.Text = "Uninstall Plugin";
			// 
			// label_01
			// 
			this.label_01.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.label_01.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
			this.label_01.Location = new System.Drawing.Point(10, 211);
			this.label_01.Margin = new System.Windows.Forms.Padding(9, 3, 0, 3);
			this.label_01.Name = "label_01";
			this.label_01.Size = new System.Drawing.Size(59, 20);
			this.label_01.TabIndex = 1;
			this.label_01.Text = "Plugin title:";
			this.label_01.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// label_02
			// 
			this.label_02.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.label_02.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
			this.label_02.Location = new System.Drawing.Point(10, 237);
			this.label_02.Margin = new System.Windows.Forms.Padding(9, 3, 0, 6);
			this.label_02.Name = "label_02";
			this.label_02.Size = new System.Drawing.Size(59, 20);
			this.label_02.TabIndex = 3;
			this.label_02.Text = "DLL name:";
			this.label_02.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// panel_List
			// 
			this.panel_List.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.panel_List.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.panel_List.Controls.Add(this.treeView);
			this.panel_List.Controls.Add(this.toolStrip);
			this.panel_List.Location = new System.Drawing.Point(1, 25);
			this.panel_List.Margin = new System.Windows.Forms.Padding(0);
			this.panel_List.Name = "panel_List";
			this.panel_List.Size = new System.Drawing.Size(256, 292);
			this.panel_List.TabIndex = 0;
			// 
			// treeView
			// 
			this.treeView.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(48)))), ((int)(((byte)(48)))), ((int)(((byte)(48)))));
			this.treeView.Dock = System.Windows.Forms.DockStyle.Fill;
			this.treeView.Location = new System.Drawing.Point(0, 28);
			this.treeView.Margin = new System.Windows.Forms.Padding(6);
			this.treeView.MaxDragChange = 20;
			this.treeView.Name = "treeView";
			this.treeView.Size = new System.Drawing.Size(254, 262);
			this.treeView.TabIndex = 0;
			// 
			// toolStrip
			// 
			this.toolStrip.AutoSize = false;
			this.toolStrip.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
			this.toolStrip.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
			this.toolStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.button_Install,
            this.button_Uninstall});
			this.toolStrip.Location = new System.Drawing.Point(0, 0);
			this.toolStrip.Name = "toolStrip";
			this.toolStrip.Padding = new System.Windows.Forms.Padding(5, 0, 1, 0);
			this.toolStrip.Size = new System.Drawing.Size(254, 28);
			this.toolStrip.TabIndex = 1;
			// 
			// panel_Logo
			// 
			this.panel_Logo.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.panel_Logo.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(48)))), ((int)(((byte)(48)))), ((int)(((byte)(48)))));
			this.panel_Logo.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
			this.panel_Logo.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.panel_Logo.Location = new System.Drawing.Point(6, 6);
			this.panel_Logo.Margin = new System.Windows.Forms.Padding(6, 6, 6, 3);
			this.panel_Logo.Name = "panel_Logo";
			this.panel_Logo.Size = new System.Drawing.Size(380, 199);
			this.panel_Logo.TabIndex = 0;
			// 
			// panel_Properties
			// 
			this.panel_Properties.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.panel_Properties.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.panel_Properties.Controls.Add(this.tabControl);
			this.panel_Properties.Location = new System.Drawing.Point(256, 25);
			this.panel_Properties.Name = "panel_Properties";
			this.panel_Properties.Size = new System.Drawing.Size(402, 292);
			this.panel_Properties.TabIndex = 1;
			// 
			// tabControl
			// 
			this.tabControl.Controls.Add(this.tabPage_Overview);
			this.tabControl.Controls.Add(this.tabPage_Description);
			this.tabControl.Controls.Add(this.tabPage_Changelog);
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
			this.tabControl.Location = new System.Drawing.Point(0, 0);
			this.tabControl.Name = "tabControl";
			this.tabControl.SelectedIndex = 0;
			this.tabControl.Size = new System.Drawing.Size(400, 290);
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
			this.tabPage_Overview.Size = new System.Drawing.Size(392, 263);
			this.tabPage_Overview.TabIndex = 0;
			this.tabPage_Overview.Text = "Overview";
			// 
			// textBox_DLLName
			// 
			this.textBox_DLLName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.textBox_DLLName.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(48)))), ((int)(((byte)(48)))), ((int)(((byte)(48)))));
			this.textBox_DLLName.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.textBox_DLLName.Location = new System.Drawing.Point(75, 237);
			this.textBox_DLLName.Margin = new System.Windows.Forms.Padding(6, 3, 6, 6);
			this.textBox_DLLName.Name = "textBox_DLLName";
			this.textBox_DLLName.ReadOnly = true;
			this.textBox_DLLName.Size = new System.Drawing.Size(311, 20);
			this.textBox_DLLName.TabIndex = 4;
			// 
			// textBox_Title
			// 
			this.textBox_Title.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.textBox_Title.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(48)))), ((int)(((byte)(48)))), ((int)(((byte)(48)))));
			this.textBox_Title.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.textBox_Title.Location = new System.Drawing.Point(75, 211);
			this.textBox_Title.Margin = new System.Windows.Forms.Padding(6, 3, 6, 3);
			this.textBox_Title.Name = "textBox_Title";
			this.textBox_Title.ReadOnly = true;
			this.textBox_Title.Size = new System.Drawing.Size(311, 20);
			this.textBox_Title.TabIndex = 2;
			// 
			// tabPage_Description
			// 
			this.tabPage_Description.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
			this.tabPage_Description.Controls.Add(this.richTextBox_Description);
			this.tabPage_Description.Location = new System.Drawing.Point(4, 23);
			this.tabPage_Description.Name = "tabPage_Description";
			this.tabPage_Description.Size = new System.Drawing.Size(392, 263);
			this.tabPage_Description.TabIndex = 1;
			this.tabPage_Description.Text = "Description";
			// 
			// richTextBox_Description
			// 
			this.richTextBox_Description.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(48)))), ((int)(((byte)(48)))), ((int)(((byte)(48)))));
			this.richTextBox_Description.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.richTextBox_Description.Dock = System.Windows.Forms.DockStyle.Fill;
			this.richTextBox_Description.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
			this.richTextBox_Description.ForeColor = System.Drawing.Color.Gainsboro;
			this.richTextBox_Description.Location = new System.Drawing.Point(0, 0);
			this.richTextBox_Description.Name = "richTextBox_Description";
			this.richTextBox_Description.ReadOnly = true;
			this.richTextBox_Description.RightMargin = 3;
			this.richTextBox_Description.Size = new System.Drawing.Size(392, 263);
			this.richTextBox_Description.TabIndex = 0;
			this.richTextBox_Description.Text = "";
			// 
			// tabPage_Changelog
			// 
			this.tabPage_Changelog.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
			this.tabPage_Changelog.Controls.Add(this.richTextBox_Changelog);
			this.tabPage_Changelog.Location = new System.Drawing.Point(4, 23);
			this.tabPage_Changelog.Name = "tabPage_Changelog";
			this.tabPage_Changelog.Size = new System.Drawing.Size(392, 263);
			this.tabPage_Changelog.TabIndex = 2;
			this.tabPage_Changelog.Text = "Changelog";
			// 
			// richTextBox_Changelog
			// 
			this.richTextBox_Changelog.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(48)))), ((int)(((byte)(48)))), ((int)(((byte)(48)))));
			this.richTextBox_Changelog.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.richTextBox_Changelog.Dock = System.Windows.Forms.DockStyle.Fill;
			this.richTextBox_Changelog.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
			this.richTextBox_Changelog.ForeColor = System.Drawing.Color.Gainsboro;
			this.richTextBox_Changelog.Location = new System.Drawing.Point(0, 0);
			this.richTextBox_Changelog.Name = "richTextBox_Changelog";
			this.richTextBox_Changelog.ReadOnly = true;
			this.richTextBox_Changelog.RightMargin = 3;
			this.richTextBox_Changelog.Size = new System.Drawing.Size(392, 263);
			this.richTextBox_Changelog.TabIndex = 0;
			this.richTextBox_Changelog.Text = "";
			// 
			// sectionPanel
			// 
			this.sectionPanel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.sectionPanel.Controls.Add(this.panel_Properties);
			this.sectionPanel.Controls.Add(this.panel_List);
			this.sectionPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.sectionPanel.Location = new System.Drawing.Point(0, 0);
			this.sectionPanel.Name = "sectionPanel";
			this.sectionPanel.SectionHeader = "Project Plugins (Not implemented)";
			this.sectionPanel.Size = new System.Drawing.Size(662, 320);
			this.sectionPanel.TabIndex = 0;
			// 
			// SectionPluginList
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
			this.Controls.Add(this.sectionPanel);
			this.Name = "SectionPluginList";
			this.Size = new System.Drawing.Size(662, 320);
			this.panel_List.ResumeLayout(false);
			this.toolStrip.ResumeLayout(false);
			this.toolStrip.PerformLayout();
			this.panel_Properties.ResumeLayout(false);
			this.tabControl.ResumeLayout(false);
			this.tabPage_Overview.ResumeLayout(false);
			this.tabPage_Overview.PerformLayout();
			this.tabPage_Description.ResumeLayout(false);
			this.tabPage_Changelog.ResumeLayout(false);
			this.sectionPanel.ResumeLayout(false);
			this.ResumeLayout(false);

		}

		#endregion

		private DarkUI.Controls.DarkLabel label_01;
		private DarkUI.Controls.DarkLabel label_02;
		private DarkUI.Controls.DarkSectionPanel sectionPanel;
		private DarkUI.Controls.DarkToolStrip toolStrip;
		private DarkUI.Controls.DarkTreeView treeView;
		private System.Windows.Forms.CustomTabControl tabControl;
		private System.Windows.Forms.Panel panel_List;
		private System.Windows.Forms.Panel panel_Logo;
		private System.Windows.Forms.Panel panel_Properties;
		private System.Windows.Forms.RichTextBox richTextBox_Changelog;
		private System.Windows.Forms.RichTextBox richTextBox_Description;
		private System.Windows.Forms.TabPage tabPage_Changelog;
		private System.Windows.Forms.TabPage tabPage_Description;
		private System.Windows.Forms.TabPage tabPage_Overview;
		private System.Windows.Forms.TextBox textBox_DLLName;
		private System.Windows.Forms.TextBox textBox_Title;
		private System.Windows.Forms.ToolStripButton button_Install;
		private System.Windows.Forms.ToolStripButton button_Uninstall;
	}
}
