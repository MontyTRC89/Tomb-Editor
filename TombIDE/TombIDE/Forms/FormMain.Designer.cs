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
			this.components = new System.ComponentModel.Container();
			this.button_AddProgram = new System.Windows.Forms.Button();
			this.button_LaunchGame = new System.Windows.Forms.Button();
			this.button_Leave = new System.Windows.Forms.Button();
			this.button_OpenFolder = new System.Windows.Forms.Button();
			this.button_Special = new System.Windows.Forms.Button();
			this.contextMenu_ProgramButton = new DarkUI.Controls.DarkContextMenu();
			this.menuItem_DeleteButton = new System.Windows.Forms.ToolStripMenuItem();
			this.label_Separator_01 = new DarkUI.Controls.DarkLabel();
			this.label_Separator_02 = new DarkUI.Controls.DarkLabel();
			this.label_Separator_03 = new DarkUI.Controls.DarkLabel();
			this.panel_CoverLoading = new System.Windows.Forms.Panel();
			this.panel_Main = new System.Windows.Forms.Panel();
			this.tablessTabControl = new TombLib.Controls.DarkTabbedContainer();
			this.tabPage_ProjectMaster = new System.Windows.Forms.TabPage();
			this.tabPage_ScriptingStudio = new System.Windows.Forms.TabPage();
			this.panel_Programs = new System.Windows.Forms.Panel();
			this.panelButton_ScriptingStudio = new System.Windows.Forms.Panel();
			this.panelButton_ProjectMaster = new System.Windows.Forms.Panel();
			this.timer_ScriptButtonBlinking = new System.Windows.Forms.Timer(this.components);
			this.toolTip = new System.Windows.Forms.ToolTip(this.components);
			this.contextMenu_ProgramButton.SuspendLayout();
			this.panel_Main.SuspendLayout();
			this.tablessTabControl.SuspendLayout();
			this.panel_Programs.SuspendLayout();
			this.SuspendLayout();
			// 
			// button_AddProgram
			// 
			this.button_AddProgram.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.button_AddProgram.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(48)))), ((int)(((byte)(48)))), ((int)(((byte)(48)))));
			this.button_AddProgram.Image = global::TombIDE.Properties.Resources.general_plus_math_16;
			this.button_AddProgram.Location = new System.Drawing.Point(3, 330);
			this.button_AddProgram.Name = "button_AddProgram";
			this.button_AddProgram.Size = new System.Drawing.Size(40, 40);
			this.button_AddProgram.TabIndex = 10;
			this.toolTip.SetToolTip(this.button_AddProgram, "Add Program Shortcut");
			this.button_AddProgram.Click += new System.EventHandler(this.button_AddProgram_Click);
			// 
			// button_LaunchGame
			// 
			this.button_LaunchGame.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.button_LaunchGame.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(48)))), ((int)(((byte)(48)))), ((int)(((byte)(48)))));
			this.button_LaunchGame.Image = global::TombIDE.Properties.Resources.general_edit_16;
			this.button_LaunchGame.Location = new System.Drawing.Point(2, 174);
			this.button_LaunchGame.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
			this.button_LaunchGame.Name = "button_LaunchGame";
			this.button_LaunchGame.Size = new System.Drawing.Size(42, 42);
			this.button_LaunchGame.TabIndex = 6;
			this.toolTip.SetToolTip(this.button_LaunchGame, "Launch Game (F4)");
			this.button_LaunchGame.Click += new System.EventHandler(this.button_LaunchGame_Click);
			// 
			// button_Leave
			// 
			this.button_Leave.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.button_Leave.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.button_Leave.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(48)))), ((int)(((byte)(48)))), ((int)(((byte)(48)))));
			this.button_Leave.Image = global::TombIDE.Properties.Resources.ide_back_30;
			this.button_Leave.Location = new System.Drawing.Point(2, 6);
			this.button_Leave.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
			this.button_Leave.Name = "button_Leave";
			this.button_Leave.Size = new System.Drawing.Size(42, 42);
			this.button_Leave.TabIndex = 1;
			this.toolTip.SetToolTip(this.button_Leave, "Exit Project");
			// 
			// button_OpenFolder
			// 
			this.button_OpenFolder.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.button_OpenFolder.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(48)))), ((int)(((byte)(48)))), ((int)(((byte)(48)))));
			this.button_OpenFolder.Image = global::TombIDE.Properties.Resources.ide_folder_30;
			this.button_OpenFolder.Location = new System.Drawing.Point(2, 222);
			this.button_OpenFolder.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
			this.button_OpenFolder.Name = "button_OpenFolder";
			this.button_OpenFolder.Size = new System.Drawing.Size(42, 42);
			this.button_OpenFolder.TabIndex = 7;
			this.toolTip.SetToolTip(this.button_OpenFolder, "Open Project Folder (F3)");
			this.button_OpenFolder.Click += new System.EventHandler(this.button_OpenFolder_Click);
			// 
			// button_Special
			// 
			this.button_Special.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.button_Special.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(48)))), ((int)(((byte)(48)))), ((int)(((byte)(48)))));
			this.button_Special.Image = global::TombIDE.Properties.Resources.general_edit_16;
			this.button_Special.Location = new System.Drawing.Point(2, 282);
			this.button_Special.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
			this.button_Special.Name = "button_Special";
			this.button_Special.Size = new System.Drawing.Size(42, 42);
			this.button_Special.TabIndex = 9;
			// 
			// contextMenu_ProgramButton
			// 
			this.contextMenu_ProgramButton.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
			this.contextMenu_ProgramButton.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
			this.contextMenu_ProgramButton.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.menuItem_DeleteButton});
			this.contextMenu_ProgramButton.Name = "contextMenu_ProgramButton";
			this.contextMenu_ProgramButton.Size = new System.Drawing.Size(156, 26);
			// 
			// menuItem_DeleteButton
			// 
			this.menuItem_DeleteButton.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
			this.menuItem_DeleteButton.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
			this.menuItem_DeleteButton.Image = global::TombIDE.Properties.Resources.general_trash_16;
			this.menuItem_DeleteButton.Name = "menuItem_DeleteButton";
			this.menuItem_DeleteButton.Size = new System.Drawing.Size(155, 22);
			this.menuItem_DeleteButton.Text = "Delete Shortcut";
			this.menuItem_DeleteButton.Click += new System.EventHandler(this.menuItem_DeleteButton_Click);
			// 
			// label_Separator_01
			// 
			this.label_Separator_01.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.label_Separator_01.ForeColor = System.Drawing.Color.Gray;
			this.label_Separator_01.Location = new System.Drawing.Point(3, 56);
			this.label_Separator_01.Margin = new System.Windows.Forms.Padding(3, 5, 3, 5);
			this.label_Separator_01.Name = "label_Separator_01";
			this.label_Separator_01.Size = new System.Drawing.Size(40, 2);
			this.label_Separator_01.TabIndex = 2;
			// 
			// label_Separator_02
			// 
			this.label_Separator_02.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.label_Separator_02.ForeColor = System.Drawing.Color.Gray;
			this.label_Separator_02.Location = new System.Drawing.Point(3, 164);
			this.label_Separator_02.Margin = new System.Windows.Forms.Padding(3, 5, 3, 5);
			this.label_Separator_02.Name = "label_Separator_02";
			this.label_Separator_02.Size = new System.Drawing.Size(40, 2);
			this.label_Separator_02.TabIndex = 5;
			// 
			// label_Separator_03
			// 
			this.label_Separator_03.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.label_Separator_03.ForeColor = System.Drawing.Color.Gray;
			this.label_Separator_03.Location = new System.Drawing.Point(3, 272);
			this.label_Separator_03.Margin = new System.Windows.Forms.Padding(3, 5, 3, 5);
			this.label_Separator_03.Name = "label_Separator_03";
			this.label_Separator_03.Size = new System.Drawing.Size(40, 2);
			this.label_Separator_03.TabIndex = 8;
			// 
			// panel_CoverLoading
			// 
			this.panel_CoverLoading.Dock = System.Windows.Forms.DockStyle.Fill;
			this.panel_CoverLoading.Location = new System.Drawing.Point(0, 0);
			this.panel_CoverLoading.Name = "panel_CoverLoading";
			this.panel_CoverLoading.Size = new System.Drawing.Size(1054, 601);
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
			this.panel_Main.Size = new System.Drawing.Size(1007, 601);
			this.panel_Main.TabIndex = 0;
			// 
			// tablessTabControl
			// 
			this.tablessTabControl.Controls.Add(this.tabPage_ProjectMaster);
			this.tablessTabControl.Controls.Add(this.tabPage_ScriptingStudio);
			this.tablessTabControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tablessTabControl.Location = new System.Drawing.Point(0, 0);
			this.tablessTabControl.Name = "tablessTabControl";
			this.tablessTabControl.SelectedIndex = 0;
			this.tablessTabControl.Size = new System.Drawing.Size(1005, 599);
			this.tablessTabControl.TabIndex = 0;
			// 
			// tabPage_ProjectMaster
			// 
			this.tabPage_ProjectMaster.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
			this.tabPage_ProjectMaster.Location = new System.Drawing.Point(4, 22);
			this.tabPage_ProjectMaster.Name = "tabPage_ProjectMaster";
			this.tabPage_ProjectMaster.Size = new System.Drawing.Size(997, 573);
			this.tabPage_ProjectMaster.TabIndex = 0;
			this.tabPage_ProjectMaster.Text = "Project Master";
			// 
			// tabPage_ScriptingStudio
			// 
			this.tabPage_ScriptingStudio.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
			this.tabPage_ScriptingStudio.Location = new System.Drawing.Point(4, 22);
			this.tabPage_ScriptingStudio.Name = "tabPage_ScriptingStudio";
			this.tabPage_ScriptingStudio.Size = new System.Drawing.Size(997, 573);
			this.tabPage_ScriptingStudio.TabIndex = 2;
			this.tabPage_ScriptingStudio.Text = "Scripting Studio";
			// 
			// panel_Programs
			// 
			this.panel_Programs.AutoScroll = true;
			this.panel_Programs.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(48)))), ((int)(((byte)(48)))), ((int)(((byte)(48)))));
			this.panel_Programs.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.panel_Programs.Controls.Add(this.button_OpenFolder);
			this.panel_Programs.Controls.Add(this.button_Special);
			this.panel_Programs.Controls.Add(this.label_Separator_03);
			this.panel_Programs.Controls.Add(this.button_LaunchGame);
			this.panel_Programs.Controls.Add(this.label_Separator_02);
			this.panel_Programs.Controls.Add(this.panelButton_ScriptingStudio);
			this.panel_Programs.Controls.Add(this.panelButton_ProjectMaster);
			this.panel_Programs.Controls.Add(this.label_Separator_01);
			this.panel_Programs.Controls.Add(this.button_Leave);
			this.panel_Programs.Controls.Add(this.button_AddProgram);
			this.panel_Programs.Dock = System.Windows.Forms.DockStyle.Fill;
			this.panel_Programs.Location = new System.Drawing.Point(0, 0);
			this.panel_Programs.Margin = new System.Windows.Forms.Padding(0);
			this.panel_Programs.Name = "panel_Programs";
			this.panel_Programs.Padding = new System.Windows.Forms.Padding(0, 3, 0, 0);
			this.panel_Programs.Size = new System.Drawing.Size(1054, 601);
			this.panel_Programs.TabIndex = 0;
			this.panel_Programs.MouseMove += new System.Windows.Forms.MouseEventHandler(this.panel_Programs_MouseMove);
			this.panel_Programs.MouseUp += new System.Windows.Forms.MouseEventHandler(this.panel_Programs_MouseUp);
			// 
			// panelButton_ScriptingStudio
			// 
			this.panelButton_ScriptingStudio.BackgroundImage = global::TombIDE.Properties.Resources.ide_script_30;
			this.panelButton_ScriptingStudio.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
			this.panelButton_ScriptingStudio.Location = new System.Drawing.Point(2, 114);
			this.panelButton_ScriptingStudio.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
			this.panelButton_ScriptingStudio.Name = "panelButton_ScriptingStudio";
			this.panelButton_ScriptingStudio.Size = new System.Drawing.Size(42, 42);
			this.panelButton_ScriptingStudio.TabIndex = 4;
			this.toolTip.SetToolTip(this.panelButton_ScriptingStudio, "Scripting Studio");
			this.panelButton_ScriptingStudio.Click += new System.EventHandler(this.panelButton_ScriptEditor_Click);
			// 
			// panelButton_ProjectMaster
			// 
			this.panelButton_ProjectMaster.BackgroundImage = global::TombIDE.Properties.Resources.ide_master_30;
			this.panelButton_ProjectMaster.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
			this.panelButton_ProjectMaster.Location = new System.Drawing.Point(2, 66);
			this.panelButton_ProjectMaster.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
			this.panelButton_ProjectMaster.Name = "panelButton_ProjectMaster";
			this.panelButton_ProjectMaster.Size = new System.Drawing.Size(42, 42);
			this.panelButton_ProjectMaster.TabIndex = 3;
			this.toolTip.SetToolTip(this.panelButton_ProjectMaster, "Project Master");
			this.panelButton_ProjectMaster.Click += new System.EventHandler(this.panelButton_ProjectMaster_Click);
			// 
			// timer_ScriptButtonBlinking
			// 
			this.timer_ScriptButtonBlinking.Tick += new System.EventHandler(this.timer_ScriptButtonBlinking_Tick);
			// 
			// FormMain
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(1054, 601);
			this.Controls.Add(this.panel_Main);
			this.Controls.Add(this.panel_Programs);
			this.Controls.Add(this.panel_CoverLoading);
			this.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
			this.KeyPreview = true;
			this.MinimumSize = new System.Drawing.Size(1070, 640);
			this.Name = "FormMain";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "TombIDE";
			this.contextMenu_ProgramButton.ResumeLayout(false);
			this.panel_Main.ResumeLayout(false);
			this.tablessTabControl.ResumeLayout(false);
			this.panel_Programs.ResumeLayout(false);
			this.ResumeLayout(false);

		}

		#endregion

		private DarkUI.Controls.DarkContextMenu contextMenu_ProgramButton;
		private DarkUI.Controls.DarkLabel label_Separator_01;
		private DarkUI.Controls.DarkLabel label_Separator_02;
		private DarkUI.Controls.DarkLabel label_Separator_03;
		private System.Windows.Forms.Button button_AddProgram;
		private System.Windows.Forms.Button button_LaunchGame;
		private System.Windows.Forms.Button button_Leave;
		private System.Windows.Forms.Button button_OpenFolder;
		private System.Windows.Forms.Button button_Special;
		private System.Windows.Forms.Panel panel_CoverLoading;
		private System.Windows.Forms.Panel panel_Main;
		private System.Windows.Forms.Panel panel_Programs;
		private System.Windows.Forms.Panel panelButton_ProjectMaster;
		private System.Windows.Forms.Panel panelButton_ScriptingStudio;
		private System.Windows.Forms.Timer timer_ScriptButtonBlinking;
		private System.Windows.Forms.ToolStripMenuItem menuItem_DeleteButton;
		private System.Windows.Forms.ToolTip toolTip;
		private TombLib.Controls.DarkTabbedContainer tablessTabControl;
		private System.Windows.Forms.TabPage tabPage_ProjectMaster;
		private System.Windows.Forms.TabPage tabPage_ScriptingStudio;
	}
}