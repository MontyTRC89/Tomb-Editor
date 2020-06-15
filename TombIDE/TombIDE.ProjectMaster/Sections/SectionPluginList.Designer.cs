﻿namespace TombIDE.ProjectMaster
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
			this.button_ManagePlugins = new DarkUI.Controls.DarkButton();
			this.button_OpenInExplorer = new DarkUI.Controls.DarkButton();
			this.label_01 = new DarkUI.Controls.DarkLabel();
			this.label_02 = new DarkUI.Controls.DarkLabel();
			this.label_NoInfo = new DarkUI.Controls.DarkLabel();
			this.label_NoLogo = new DarkUI.Controls.DarkLabel();
			this.panel_List = new System.Windows.Forms.Panel();
			this.treeView = new DarkUI.Controls.DarkTreeView();
			this.panel_Logo = new System.Windows.Forms.Panel();
			this.panel_Properties = new System.Windows.Forms.Panel();
			this.tabControl = new System.Windows.Forms.CustomTabControl();
			this.tabPage_Overview = new System.Windows.Forms.TabPage();
			this.textBox_DLLName = new System.Windows.Forms.TextBox();
			this.textBox_Title = new System.Windows.Forms.TextBox();
			this.tabPage_Description = new System.Windows.Forms.TabPage();
			this.richTextBox_Description = new System.Windows.Forms.RichTextBox();
			this.sectionPanel = new DarkUI.Controls.DarkSectionPanel();
			this.toolTip = new System.Windows.Forms.ToolTip(this.components);
			this.contextMenu = new DarkUI.Controls.DarkContextMenu();
			this.menuItem_Uninstall = new System.Windows.Forms.ToolStripMenuItem();
			this.panel_List.SuspendLayout();
			this.panel_Logo.SuspendLayout();
			this.panel_Properties.SuspendLayout();
			this.tabControl.SuspendLayout();
			this.tabPage_Overview.SuspendLayout();
			this.tabPage_Description.SuspendLayout();
			this.sectionPanel.SuspendLayout();
			this.contextMenu.SuspendLayout();
			this.SuspendLayout();
			// 
			// button_ManagePlugins
			// 
			this.button_ManagePlugins.Checked = false;
			this.button_ManagePlugins.Image = global::TombIDE.ProjectMaster.Properties.Resources.general_edit_16;
			this.button_ManagePlugins.Location = new System.Drawing.Point(6, 6);
			this.button_ManagePlugins.Margin = new System.Windows.Forms.Padding(6, 6, 0, 3);
			this.button_ManagePlugins.Name = "button_ManagePlugins";
			this.button_ManagePlugins.Size = new System.Drawing.Size(213, 23);
			this.button_ManagePlugins.TabIndex = 1;
			this.button_ManagePlugins.Text = "Manage Plugins";
			this.button_ManagePlugins.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
			this.button_ManagePlugins.Click += new System.EventHandler(this.button_ManagePlugins_Click);
			// 
			// button_OpenInExplorer
			// 
			this.button_OpenInExplorer.ButtonStyle = DarkUI.Controls.DarkButtonStyle.Flat;
			this.button_OpenInExplorer.Checked = false;
			this.button_OpenInExplorer.Enabled = false;
			this.button_OpenInExplorer.Image = global::TombIDE.ProjectMaster.Properties.Resources.forward_arrow_16;
			this.button_OpenInExplorer.Location = new System.Drawing.Point(224, 5);
			this.button_OpenInExplorer.Margin = new System.Windows.Forms.Padding(5, 5, 5, 2);
			this.button_OpenInExplorer.Name = "button_OpenInExplorer";
			this.button_OpenInExplorer.Size = new System.Drawing.Size(25, 25);
			this.button_OpenInExplorer.TabIndex = 2;
			this.toolTip.SetToolTip(this.button_OpenInExplorer, "Open Selected Plugin Folder");
			this.button_OpenInExplorer.Click += new System.EventHandler(this.button_OpenInExplorer_Click);
			// 
			// label_01
			// 
			this.label_01.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.label_01.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
			this.label_01.Location = new System.Drawing.Point(10, 212);
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
			this.label_02.Location = new System.Drawing.Point(10, 238);
			this.label_02.Margin = new System.Windows.Forms.Padding(9, 3, 0, 6);
			this.label_02.Name = "label_02";
			this.label_02.Size = new System.Drawing.Size(59, 20);
			this.label_02.TabIndex = 3;
			this.label_02.Text = "DLL name:";
			this.label_02.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// label_NoInfo
			// 
			this.label_NoInfo.Dock = System.Windows.Forms.DockStyle.Fill;
			this.label_NoInfo.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
			this.label_NoInfo.ForeColor = System.Drawing.Color.Gray;
			this.label_NoInfo.Location = new System.Drawing.Point(0, 0);
			this.label_NoInfo.Name = "label_NoInfo";
			this.label_NoInfo.Size = new System.Drawing.Size(378, 198);
			this.label_NoInfo.TabIndex = 1;
			this.label_NoInfo.Text = "Please select a plugin from the list to view its info.";
			this.label_NoInfo.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// label_NoLogo
			// 
			this.label_NoLogo.Dock = System.Windows.Forms.DockStyle.Fill;
			this.label_NoLogo.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
			this.label_NoLogo.ForeColor = System.Drawing.Color.Gray;
			this.label_NoLogo.Location = new System.Drawing.Point(0, 0);
			this.label_NoLogo.Name = "label_NoLogo";
			this.label_NoLogo.Size = new System.Drawing.Size(378, 198);
			this.label_NoLogo.TabIndex = 0;
			this.label_NoLogo.Text = "No logo image found.";
			this.label_NoLogo.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			this.label_NoLogo.Visible = false;
			// 
			// panel_List
			// 
			this.panel_List.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.panel_List.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.panel_List.Controls.Add(this.button_OpenInExplorer);
			this.panel_List.Controls.Add(this.button_ManagePlugins);
			this.panel_List.Controls.Add(this.treeView);
			this.panel_List.Location = new System.Drawing.Point(1, 25);
			this.panel_List.Margin = new System.Windows.Forms.Padding(0);
			this.panel_List.Name = "panel_List";
			this.panel_List.Size = new System.Drawing.Size(256, 292);
			this.panel_List.TabIndex = 0;
			// 
			// treeView
			// 
			this.treeView.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.treeView.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(48)))), ((int)(((byte)(48)))), ((int)(((byte)(48)))));
			this.treeView.ContextMenuStrip = this.contextMenu;
			this.treeView.EvenNodeColor = System.Drawing.Color.FromArgb(((int)(((byte)(52)))), ((int)(((byte)(52)))), ((int)(((byte)(52)))));
			this.treeView.FocusedNodeColor = System.Drawing.Color.FromArgb(((int)(((byte)(75)))), ((int)(((byte)(110)))), ((int)(((byte)(175)))));
			this.treeView.Indent = 0;
			this.treeView.ItemHeight = 30;
			this.treeView.Location = new System.Drawing.Point(0, 35);
			this.treeView.Margin = new System.Windows.Forms.Padding(6, 3, 6, 6);
			this.treeView.MaxDragChange = 30;
			this.treeView.Name = "treeView";
			this.treeView.NonFocusedNodeColor = System.Drawing.Color.FromArgb(((int)(((byte)(92)))), ((int)(((byte)(92)))), ((int)(((byte)(92)))));
			this.treeView.OddNodeColor = System.Drawing.Color.FromArgb(((int)(((byte)(48)))), ((int)(((byte)(48)))), ((int)(((byte)(48)))));
			this.treeView.Size = new System.Drawing.Size(254, 255);
			this.treeView.TabIndex = 0;
			this.treeView.SelectedNodesChanged += new System.EventHandler(this.treeView_SelectedNodesChanged);
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
			this.panel_Logo.Size = new System.Drawing.Size(380, 200);
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
			this.textBox_DLLName.ForeColor = System.Drawing.Color.Gainsboro;
			this.textBox_DLLName.Location = new System.Drawing.Point(75, 238);
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
			this.textBox_Title.ForeColor = System.Drawing.Color.Gainsboro;
			this.textBox_Title.Location = new System.Drawing.Point(75, 212);
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
			this.richTextBox_Description.Size = new System.Drawing.Size(392, 263);
			this.richTextBox_Description.TabIndex = 0;
			this.richTextBox_Description.Text = "";
			// 
			// sectionPanel
			// 
			this.sectionPanel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.sectionPanel.Controls.Add(this.panel_Properties);
			this.sectionPanel.Controls.Add(this.panel_List);
			this.sectionPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.sectionPanel.Location = new System.Drawing.Point(0, 0);
			this.sectionPanel.Name = "sectionPanel";
			this.sectionPanel.SectionHeader = "Project Plugins";
			this.sectionPanel.Size = new System.Drawing.Size(662, 320);
			this.sectionPanel.TabIndex = 0;
			// 
			// contextMenu
			// 
			this.contextMenu.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
			this.contextMenu.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
			this.contextMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.menuItem_Uninstall});
			this.contextMenu.Name = "contextMenu";
			this.contextMenu.Size = new System.Drawing.Size(190, 48);
			this.contextMenu.Opening += new System.ComponentModel.CancelEventHandler(this.contextMenu_Opening);
			// 
			// menuItem_Uninstall
			// 
			this.menuItem_Uninstall.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
			this.menuItem_Uninstall.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
			this.menuItem_Uninstall.Image = global::TombIDE.ProjectMaster.Properties.Resources.general_trash_16;
			this.menuItem_Uninstall.Name = "menuItem_Uninstall";
			this.menuItem_Uninstall.Size = new System.Drawing.Size(189, 22);
			this.menuItem_Uninstall.Text = "Uninstall from Project";
			this.menuItem_Uninstall.Click += new System.EventHandler(this.menuItem_Uninstall_Click);
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
			this.panel_Logo.ResumeLayout(false);
			this.panel_Properties.ResumeLayout(false);
			this.tabControl.ResumeLayout(false);
			this.tabPage_Overview.ResumeLayout(false);
			this.tabPage_Overview.PerformLayout();
			this.tabPage_Description.ResumeLayout(false);
			this.sectionPanel.ResumeLayout(false);
			this.contextMenu.ResumeLayout(false);
			this.ResumeLayout(false);

		}

		#endregion

		private DarkUI.Controls.DarkButton button_ManagePlugins;
		private DarkUI.Controls.DarkButton button_OpenInExplorer;
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
		private System.Windows.Forms.ToolStripMenuItem menuItem_Uninstall;
	}
}
