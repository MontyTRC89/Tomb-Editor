namespace TombIDE.ProjectMaster
{
	partial class ProjectMaster
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
			this.animationTimer = new System.Windows.Forms.Timer(this.components);
			this.toolTip = new System.Windows.Forms.ToolTip(this.components);
			this.button_ShowPlugins = new DarkUI.Controls.DarkButton();
			this.button_HidePlugins = new DarkUI.Controls.DarkButton();
			this.splitContainer_Info = new System.Windows.Forms.SplitContainer();
			this.section_ProjectInfo = new TombIDE.ProjectMaster.SectionProjectSettings();
			this.section_PluginList = new TombIDE.ProjectMaster.SectionPluginList();
			((System.ComponentModel.ISupportInitialize)(this.splitContainer_Info)).BeginInit();
			this.splitContainer_Info.Panel1.SuspendLayout();
			this.splitContainer_Info.Panel2.SuspendLayout();
			this.splitContainer_Info.SuspendLayout();
			this.SuspendLayout();
			// 
			// animationTimer
			// 
			this.animationTimer.Interval = 1;
			this.animationTimer.Tick += new System.EventHandler(this.animationTimer_Tick);
			// 
			// button_ShowPlugins
			// 
			this.button_ShowPlugins.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.button_ShowPlugins.Checked = false;
			this.button_ShowPlugins.ForeColor = System.Drawing.Color.Gainsboro;
			this.button_ShowPlugins.Location = new System.Drawing.Point(911, 246);
			this.button_ShowPlugins.Margin = new System.Windows.Forms.Padding(3, 3, 32, 12);
			this.button_ShowPlugins.Name = "button_ShowPlugins";
			this.button_ShowPlugins.Size = new System.Drawing.Size(32, 32);
			this.button_ShowPlugins.TabIndex = 1;
			this.button_ShowPlugins.Text = "▲";
			this.toolTip.SetToolTip(this.button_ShowPlugins, "Show Project Plugins");
			this.button_ShowPlugins.Click += new System.EventHandler(this.button_ShowPlugins_Click);
			// 
			// button_HidePlugins
			// 
			this.button_HidePlugins.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.button_HidePlugins.Checked = false;
			this.button_HidePlugins.ForeColor = System.Drawing.Color.Gainsboro;
			this.button_HidePlugins.Location = new System.Drawing.Point(948, 13);
			this.button_HidePlugins.Name = "button_HidePlugins";
			this.button_HidePlugins.Size = new System.Drawing.Size(24, 24);
			this.button_HidePlugins.TabIndex = 1;
			this.button_HidePlugins.Text = "▼";
			this.toolTip.SetToolTip(this.button_HidePlugins, "Hide Project Plugins");
			this.button_HidePlugins.Click += new System.EventHandler(this.button_HidePlugins_Click);
			// 
			// splitContainer_Info
			// 
			this.splitContainer_Info.Dock = System.Windows.Forms.DockStyle.Fill;
			this.splitContainer_Info.FixedPanel = System.Windows.Forms.FixedPanel.Panel2;
			this.splitContainer_Info.Location = new System.Drawing.Point(0, 0);
			this.splitContainer_Info.Name = "splitContainer_Info";
			this.splitContainer_Info.Orientation = System.Windows.Forms.Orientation.Horizontal;
			// 
			// splitContainer_Info.Panel1
			// 
			this.splitContainer_Info.Panel1.Controls.Add(this.button_ShowPlugins);
			this.splitContainer_Info.Panel1.Controls.Add(this.section_ProjectInfo);
			this.splitContainer_Info.Panel1.Padding = new System.Windows.Forms.Padding(30, 30, 30, 10);
			this.splitContainer_Info.Panel1MinSize = 300;
			// 
			// splitContainer_Info.Panel2
			// 
			this.splitContainer_Info.Panel2.Controls.Add(this.button_HidePlugins);
			this.splitContainer_Info.Panel2.Controls.Add(this.section_PluginList);
			this.splitContainer_Info.Panel2.Padding = new System.Windows.Forms.Padding(30, 10, 30, 30);
			this.splitContainer_Info.Panel2MinSize = 294;
			this.splitContainer_Info.Size = new System.Drawing.Size(1005, 600);
			this.splitContainer_Info.SplitterDistance = 300;
			this.splitContainer_Info.SplitterWidth = 5;
			this.splitContainer_Info.TabIndex = 1;
			// 
			// section_ProjectInfo
			// 
			this.section_ProjectInfo.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
			this.section_ProjectInfo.Dock = System.Windows.Forms.DockStyle.Fill;
			this.section_ProjectInfo.Location = new System.Drawing.Point(30, 30);
			this.section_ProjectInfo.Margin = new System.Windows.Forms.Padding(0);
			this.section_ProjectInfo.Name = "section_ProjectInfo";
			this.section_ProjectInfo.Size = new System.Drawing.Size(945, 260);
			this.section_ProjectInfo.TabIndex = 0;
			// 
			// section_PluginList
			// 
			this.section_PluginList.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
			this.section_PluginList.Dock = System.Windows.Forms.DockStyle.Fill;
			this.section_PluginList.Location = new System.Drawing.Point(30, 10);
			this.section_PluginList.Margin = new System.Windows.Forms.Padding(0);
			this.section_PluginList.Name = "section_PluginList";
			this.section_PluginList.Size = new System.Drawing.Size(945, 256);
			this.section_PluginList.TabIndex = 0;
			// 
			// ProjectMaster
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
			this.Controls.Add(this.splitContainer_Info);
			this.Name = "ProjectMaster";
			this.Size = new System.Drawing.Size(1005, 600);
			this.splitContainer_Info.Panel1.ResumeLayout(false);
			this.splitContainer_Info.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.splitContainer_Info)).EndInit();
			this.splitContainer_Info.ResumeLayout(false);
			this.ResumeLayout(false);

		}

		#endregion

		private DarkUI.Controls.DarkButton button_HidePlugins;
		private DarkUI.Controls.DarkButton button_ShowPlugins;
		private SectionPluginList section_PluginList;
		private SectionProjectSettings section_ProjectInfo;
		private System.Windows.Forms.SplitContainer splitContainer_Info;
		private System.Windows.Forms.Timer animationTimer;
		private System.Windows.Forms.ToolTip toolTip;
	}
}
