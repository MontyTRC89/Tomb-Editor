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
			panel_CoverLoading = new System.Windows.Forms.Panel();
			panel_Main = new System.Windows.Forms.Panel();
			tablessTabControl = new TombLib.Controls.DarkTabbedContainer();
			tabPage_LevelManager = new System.Windows.Forms.TabPage();
			tabPage_ScriptingStudio = new System.Windows.Forms.TabPage();
			tabPage_Plugins = new System.Windows.Forms.TabPage();
			tabPage_Misc = new System.Windows.Forms.TabPage();
			sideBar = new Controls.SideBar();
			panel_Main.SuspendLayout();
			tablessTabControl.SuspendLayout();
			SuspendLayout();
			// 
			// panel_CoverLoading
			// 
			panel_CoverLoading.Dock = System.Windows.Forms.DockStyle.Fill;
			panel_CoverLoading.Location = new System.Drawing.Point(0, 0);
			panel_CoverLoading.Name = "panel_CoverLoading";
			panel_CoverLoading.Size = new System.Drawing.Size(984, 601);
			panel_CoverLoading.TabIndex = 1;
			// 
			// panel_Main
			// 
			panel_Main.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
			panel_Main.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
			panel_Main.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			panel_Main.Controls.Add(tablessTabControl);
			panel_Main.Location = new System.Drawing.Point(49, 0);
			panel_Main.Margin = new System.Windows.Forms.Padding(0);
			panel_Main.Name = "panel_Main";
			panel_Main.Size = new System.Drawing.Size(935, 601);
			panel_Main.TabIndex = 0;
			// 
			// tablessTabControl
			// 
			tablessTabControl.Controls.Add(tabPage_LevelManager);
			tablessTabControl.Controls.Add(tabPage_ScriptingStudio);
			tablessTabControl.Controls.Add(tabPage_Plugins);
			tablessTabControl.Controls.Add(tabPage_Misc);
			tablessTabControl.Dock = System.Windows.Forms.DockStyle.Fill;
			tablessTabControl.Location = new System.Drawing.Point(0, 0);
			tablessTabControl.Name = "tablessTabControl";
			tablessTabControl.SelectedIndex = 0;
			tablessTabControl.Size = new System.Drawing.Size(933, 599);
			tablessTabControl.TabIndex = 0;
			// 
			// tabPage_LevelManager
			// 
			tabPage_LevelManager.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
			tabPage_LevelManager.Location = new System.Drawing.Point(4, 22);
			tabPage_LevelManager.Name = "tabPage_LevelManager";
			tabPage_LevelManager.Size = new System.Drawing.Size(925, 573);
			tabPage_LevelManager.TabIndex = 3;
			tabPage_LevelManager.Text = "Level Manager";
			// 
			// tabPage_ScriptingStudio
			// 
			tabPage_ScriptingStudio.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
			tabPage_ScriptingStudio.Location = new System.Drawing.Point(4, 22);
			tabPage_ScriptingStudio.Name = "tabPage_ScriptingStudio";
			tabPage_ScriptingStudio.Size = new System.Drawing.Size(926, 573);
			tabPage_ScriptingStudio.TabIndex = 2;
			tabPage_ScriptingStudio.Text = "Scripting Studio";
			// 
			// tabPage_Plugins
			// 
			tabPage_Plugins.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
			tabPage_Plugins.Location = new System.Drawing.Point(4, 22);
			tabPage_Plugins.Name = "tabPage_Plugins";
			tabPage_Plugins.Size = new System.Drawing.Size(926, 573);
			tabPage_Plugins.TabIndex = 4;
			tabPage_Plugins.Text = "Plugin Manager";
			// 
			// tabPage_Misc
			// 
			tabPage_Misc.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
			tabPage_Misc.Location = new System.Drawing.Point(4, 22);
			tabPage_Misc.Name = "tabPage_Misc";
			tabPage_Misc.Size = new System.Drawing.Size(926, 573);
			tabPage_Misc.TabIndex = 0;
			tabPage_Misc.Text = "Miscellaneous";
			// 
			// sideBar
			// 
			sideBar.AutoScroll = true;
			sideBar.BackColor = System.Drawing.Color.FromArgb(48, 48, 48);
			sideBar.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			sideBar.Dock = System.Windows.Forms.DockStyle.Fill;
			sideBar.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
			sideBar.Location = new System.Drawing.Point(0, 0);
			sideBar.Name = "sideBar";
			sideBar.Size = new System.Drawing.Size(984, 601);
			sideBar.TabIndex = 1;
			// 
			// FormMain
			// 
			AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			ClientSize = new System.Drawing.Size(984, 601);
			Controls.Add(panel_Main);
			Controls.Add(sideBar);
			Controls.Add(panel_CoverLoading);
			KeyPreview = true;
			MinimumSize = new System.Drawing.Size(800, 480);
			Name = "FormMain";
			StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			Text = "TombIDE";
			panel_Main.ResumeLayout(false);
			tablessTabControl.ResumeLayout(false);
			ResumeLayout(false);
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