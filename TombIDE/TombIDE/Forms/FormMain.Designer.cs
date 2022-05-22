namespace TombIDE
{
	partial class FormMain
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
			this.panel_CoverLoading = new System.Windows.Forms.Panel();
			this.panel_Main = new System.Windows.Forms.Panel();
			this.tablessTabControl = new TombLib.Controls.DarkTabbedContainer();
			this.tabPage_LevelManager = new System.Windows.Forms.TabPage();
			this.tabPage_ScriptingStudio = new System.Windows.Forms.TabPage();
			this.tabPage_Plugins = new System.Windows.Forms.TabPage();
			this.tabPage_Misc = new System.Windows.Forms.TabPage();
			this.sideBar = new TombIDE.Controls.SideBar();
			this.panel_Main.SuspendLayout();
			this.tablessTabControl.SuspendLayout();
			this.SuspendLayout();
			// 
			// panel_CoverLoading
			// 
			this.panel_CoverLoading.Dock = System.Windows.Forms.DockStyle.Fill;
			this.panel_CoverLoading.Location = new System.Drawing.Point(0, 0);
			this.panel_CoverLoading.Name = "panel_CoverLoading";
			this.panel_CoverLoading.Size = new System.Drawing.Size(984, 601);
			this.panel_CoverLoading.TabIndex = 1;
			// 
			// panel_Main
			// 
			this.panel_Main.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.panel_Main.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
			this.panel_Main.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.panel_Main.Controls.Add(this.tablessTabControl);
			this.panel_Main.Location = new System.Drawing.Point(47, 0);
			this.panel_Main.Margin = new System.Windows.Forms.Padding(0);
			this.panel_Main.Name = "panel_Main";
			this.panel_Main.Size = new System.Drawing.Size(937, 601);
			this.panel_Main.TabIndex = 0;
			// 
			// tablessTabControl
			// 
			this.tablessTabControl.Controls.Add(this.tabPage_LevelManager);
			this.tablessTabControl.Controls.Add(this.tabPage_ScriptingStudio);
			this.tablessTabControl.Controls.Add(this.tabPage_Plugins);
			this.tablessTabControl.Controls.Add(this.tabPage_Misc);
			this.tablessTabControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tablessTabControl.Location = new System.Drawing.Point(0, 0);
			this.tablessTabControl.Name = "tablessTabControl";
			this.tablessTabControl.SelectedIndex = 0;
			this.tablessTabControl.Size = new System.Drawing.Size(935, 599);
			this.tablessTabControl.TabIndex = 0;
			// 
			// tabPage_LevelManager
			// 
			this.tabPage_LevelManager.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
			this.tabPage_LevelManager.Location = new System.Drawing.Point(4, 22);
			this.tabPage_LevelManager.Name = "tabPage_LevelManager";
			this.tabPage_LevelManager.Size = new System.Drawing.Size(927, 573);
			this.tabPage_LevelManager.TabIndex = 3;
			this.tabPage_LevelManager.Text = "Level Manager";
			// 
			// tabPage_ScriptingStudio
			// 
			this.tabPage_ScriptingStudio.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
			this.tabPage_ScriptingStudio.Location = new System.Drawing.Point(4, 22);
			this.tabPage_ScriptingStudio.Name = "tabPage_ScriptingStudio";
			this.tabPage_ScriptingStudio.Size = new System.Drawing.Size(926, 573);
			this.tabPage_ScriptingStudio.TabIndex = 2;
			this.tabPage_ScriptingStudio.Text = "Scripting Studio";
			// 
			// tabPage_Plugins
			// 
			this.tabPage_Plugins.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
			this.tabPage_Plugins.Location = new System.Drawing.Point(4, 22);
			this.tabPage_Plugins.Name = "tabPage_Plugins";
			this.tabPage_Plugins.Size = new System.Drawing.Size(926, 573);
			this.tabPage_Plugins.TabIndex = 4;
			this.tabPage_Plugins.Text = "Plugin Manager";
			// 
			// tabPage_Misc
			// 
			this.tabPage_Misc.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
			this.tabPage_Misc.Location = new System.Drawing.Point(4, 22);
			this.tabPage_Misc.Name = "tabPage_Misc";
			this.tabPage_Misc.Size = new System.Drawing.Size(926, 573);
			this.tabPage_Misc.TabIndex = 0;
			this.tabPage_Misc.Text = "Miscellaneous";
			// 
			// sideBar
			// 
			this.sideBar.AutoScroll = true;
			this.sideBar.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(48)))), ((int)(((byte)(48)))), ((int)(((byte)(48)))));
			this.sideBar.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.sideBar.Dock = System.Windows.Forms.DockStyle.Fill;
			this.sideBar.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.sideBar.Location = new System.Drawing.Point(0, 0);
			this.sideBar.Name = "sideBar";
			this.sideBar.Size = new System.Drawing.Size(984, 601);
			this.sideBar.TabIndex = 1;
			// 
			// FormMain
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(984, 601);
			this.Controls.Add(this.panel_Main);
			this.Controls.Add(this.sideBar);
			this.Controls.Add(this.panel_CoverLoading);
			this.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
			this.KeyPreview = true;
			this.MinimumSize = new System.Drawing.Size(800, 480);
			this.Name = "FormMain";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "TombIDE";
			this.panel_Main.ResumeLayout(false);
			this.tablessTabControl.ResumeLayout(false);
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.Panel panel_CoverLoading;
		private System.Windows.Forms.Panel panel_Main;
		private TombLib.Controls.DarkTabbedContainer tablessTabControl;
		private System.Windows.Forms.TabPage tabPage_Misc;
		private System.Windows.Forms.TabPage tabPage_ScriptingStudio;
		private System.Windows.Forms.TabPage tabPage_LevelManager;
		private System.Windows.Forms.TabPage tabPage_Plugins;
		private Controls.SideBar sideBar;
	}
}