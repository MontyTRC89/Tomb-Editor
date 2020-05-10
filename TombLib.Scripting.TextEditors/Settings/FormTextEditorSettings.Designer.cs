namespace TombLib.Scripting.TextEditors.Forms
{
	partial class FormTextEditorSettings
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
			this.button_ResetDefault = new DarkUI.Controls.DarkButton();
			this.panel_Buttons = new System.Windows.Forms.Panel();
			this.panel_Main = new System.Windows.Forms.Panel();
			this.tablessTabControl = new TombLib.Controls.DarkTabbedContainer();
			this.tabPage_Global = new System.Windows.Forms.TabPage();
			this.tabPage_ClassicScript = new System.Windows.Forms.TabPage();
			this.settingsClassicScript = new TombLib.Scripting.TextEditors.Controls.Settings.SettingsClassicScript();
			this.tabPage_Lua = new System.Windows.Forms.TabPage();
			this.treeView = new DarkUI.Controls.DarkTreeView();
			this.panel_Buttons.SuspendLayout();
			this.panel_Main.SuspendLayout();
			this.tablessTabControl.SuspendLayout();
			this.tabPage_ClassicScript.SuspendLayout();
			this.SuspendLayout();
			// 
			// button_Apply
			// 
			this.button_Apply.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.button_Apply.Checked = false;
			this.button_Apply.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.button_Apply.Location = new System.Drawing.Point(726, 8);
			this.button_Apply.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
			this.button_Apply.Name = "button_Apply";
			this.button_Apply.Size = new System.Drawing.Size(75, 24);
			this.button_Apply.TabIndex = 1;
			this.button_Apply.Text = "Apply";
			this.button_Apply.Click += new System.EventHandler(this.button_Apply_Click);
			// 
			// button_Cancel
			// 
			this.button_Cancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.button_Cancel.Checked = false;
			this.button_Cancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.button_Cancel.Location = new System.Drawing.Point(807, 8);
			this.button_Cancel.Margin = new System.Windows.Forms.Padding(3, 6, 6, 6);
			this.button_Cancel.Name = "button_Cancel";
			this.button_Cancel.Size = new System.Drawing.Size(75, 24);
			this.button_Cancel.TabIndex = 0;
			this.button_Cancel.Text = "Cancel";
			// 
			// button_ResetDefault
			// 
			this.button_ResetDefault.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.button_ResetDefault.Checked = false;
			this.button_ResetDefault.Location = new System.Drawing.Point(6, 8);
			this.button_ResetDefault.Margin = new System.Windows.Forms.Padding(6, 6, 0, 6);
			this.button_ResetDefault.Name = "button_ResetDefault";
			this.button_ResetDefault.Size = new System.Drawing.Size(150, 24);
			this.button_ResetDefault.TabIndex = 2;
			this.button_ResetDefault.Text = "Reset settings to default";
			this.button_ResetDefault.Click += new System.EventHandler(this.button_ResetDefault_Click);
			// 
			// panel_Buttons
			// 
			this.panel_Buttons.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.panel_Buttons.Controls.Add(this.button_ResetDefault);
			this.panel_Buttons.Controls.Add(this.button_Apply);
			this.panel_Buttons.Controls.Add(this.button_Cancel);
			this.panel_Buttons.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.panel_Buttons.Location = new System.Drawing.Point(0, 414);
			this.panel_Buttons.Name = "panel_Buttons";
			this.panel_Buttons.Size = new System.Drawing.Size(890, 40);
			this.panel_Buttons.TabIndex = 3;
			// 
			// panel_Main
			// 
			this.panel_Main.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.panel_Main.Controls.Add(this.tablessTabControl);
			this.panel_Main.Controls.Add(this.treeView);
			this.panel_Main.Dock = System.Windows.Forms.DockStyle.Fill;
			this.panel_Main.Location = new System.Drawing.Point(0, 0);
			this.panel_Main.Name = "panel_Main";
			this.panel_Main.Size = new System.Drawing.Size(890, 414);
			this.panel_Main.TabIndex = 4;
			// 
			// tablessTabControl
			// 
			this.tablessTabControl.Controls.Add(this.tabPage_Global);
			this.tablessTabControl.Controls.Add(this.tabPage_ClassicScript);
			this.tablessTabControl.Controls.Add(this.tabPage_Lua);
			this.tablessTabControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tablessTabControl.Location = new System.Drawing.Point(168, 0);
			this.tablessTabControl.Name = "tablessTabControl";
			this.tablessTabControl.SelectedIndex = 0;
			this.tablessTabControl.Size = new System.Drawing.Size(720, 412);
			this.tablessTabControl.TabIndex = 4;
			// 
			// tabPage_Global
			// 
			this.tabPage_Global.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
			this.tabPage_Global.Location = new System.Drawing.Point(4, 22);
			this.tabPage_Global.Name = "tabPage_Global";
			this.tabPage_Global.Size = new System.Drawing.Size(712, 386);
			this.tabPage_Global.TabIndex = 0;
			this.tabPage_Global.Text = "Global";
			// 
			// tabPage_ClassicScript
			// 
			this.tabPage_ClassicScript.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
			this.tabPage_ClassicScript.Controls.Add(this.settingsClassicScript);
			this.tabPage_ClassicScript.Location = new System.Drawing.Point(4, 22);
			this.tabPage_ClassicScript.Name = "tabPage_ClassicScript";
			this.tabPage_ClassicScript.Size = new System.Drawing.Size(712, 386);
			this.tabPage_ClassicScript.TabIndex = 1;
			this.tabPage_ClassicScript.Text = "Classic Script";
			// 
			// settingsClassicScript
			// 
			this.settingsClassicScript.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(63)))), ((int)(((byte)(65)))), ((int)(((byte)(69)))));
			this.settingsClassicScript.Dock = System.Windows.Forms.DockStyle.Fill;
			this.settingsClassicScript.Location = new System.Drawing.Point(0, 0);
			this.settingsClassicScript.MaximumSize = new System.Drawing.Size(720, 412);
			this.settingsClassicScript.MinimumSize = new System.Drawing.Size(720, 412);
			this.settingsClassicScript.Name = "settingsClassicScript";
			this.settingsClassicScript.Size = new System.Drawing.Size(720, 412);
			this.settingsClassicScript.TabIndex = 0;
			// 
			// tabPage_Lua
			// 
			this.tabPage_Lua.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
			this.tabPage_Lua.Location = new System.Drawing.Point(4, 22);
			this.tabPage_Lua.Name = "tabPage_Lua";
			this.tabPage_Lua.Size = new System.Drawing.Size(712, 386);
			this.tabPage_Lua.TabIndex = 2;
			this.tabPage_Lua.Text = "Lua";
			// 
			// treeView
			// 
			this.treeView.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(48)))), ((int)(((byte)(48)))), ((int)(((byte)(48)))));
			this.treeView.Dock = System.Windows.Forms.DockStyle.Left;
			this.treeView.EvenNodeColor = System.Drawing.Color.FromArgb(((int)(((byte)(48)))), ((int)(((byte)(48)))), ((int)(((byte)(48)))));
			this.treeView.FocusedNodeColor = System.Drawing.Color.FromArgb(((int)(((byte)(75)))), ((int)(((byte)(110)))), ((int)(((byte)(175)))));
			this.treeView.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
			this.treeView.ItemHeight = 40;
			this.treeView.Location = new System.Drawing.Point(0, 0);
			this.treeView.MaxDragChange = 40;
			this.treeView.Name = "treeView";
			this.treeView.NonFocusedNodeColor = System.Drawing.Color.FromArgb(((int)(((byte)(92)))), ((int)(((byte)(92)))), ((int)(((byte)(92)))));
			this.treeView.OddNodeColor = System.Drawing.Color.FromArgb(((int)(((byte)(52)))), ((int)(((byte)(52)))), ((int)(((byte)(52)))));
			this.treeView.Size = new System.Drawing.Size(168, 412);
			this.treeView.TabIndex = 3;
			this.treeView.SelectedNodesChanged += new System.EventHandler(this.treeView_SelectedNodesChanged);
			// 
			// FormTextEditorSettings
			// 
			this.AcceptButton = this.button_Apply;
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
			this.CancelButton = this.button_Cancel;
			this.ClientSize = new System.Drawing.Size(890, 454);
			this.Controls.Add(this.panel_Main);
			this.Controls.Add(this.panel_Buttons);
			this.FlatBorder = true;
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
			this.MaximizeBox = false;
			this.Name = "FormTextEditorSettings";
			this.ShowIcon = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "Text Editor Settings";
			this.panel_Buttons.ResumeLayout(false);
			this.panel_Main.ResumeLayout(false);
			this.tablessTabControl.ResumeLayout(false);
			this.tabPage_ClassicScript.ResumeLayout(false);
			this.ResumeLayout(false);

		}

		#endregion

		private Controls.Settings.SettingsClassicScript settingsClassicScript;
		private DarkUI.Controls.DarkButton button_Apply;
		private DarkUI.Controls.DarkButton button_Cancel;
		private DarkUI.Controls.DarkButton button_ResetDefault;
		private DarkUI.Controls.DarkTreeView treeView;
		private System.Windows.Forms.Panel panel_Buttons;
		private System.Windows.Forms.Panel panel_Main;
		private System.Windows.Forms.TabPage tabPage_ClassicScript;
		private System.Windows.Forms.TabPage tabPage_Global;
		private System.Windows.Forms.TabPage tabPage_Lua;
		private TombLib.Controls.DarkTabbedContainer tablessTabControl;
	}
}