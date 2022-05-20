namespace TombIDE.Controls
{
	partial class SideBar
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
			this.flowLayoutPanel_Fixed = new System.Windows.Forms.FlowLayoutPanel();
			this.button_ExitProject = new System.Windows.Forms.Button();
			this.separator_01 = new DarkUI.Controls.DarkSeparator();
			this.panelButton_LevelManager = new System.Windows.Forms.Panel();
			this.panelButton_ScriptingStudio = new System.Windows.Forms.Panel();
			this.panelButton_PluginManager = new System.Windows.Forms.Panel();
			this.panelButton_Miscellaneous = new System.Windows.Forms.Panel();
			this.separator_02 = new DarkUI.Controls.DarkSeparator();
			this.button_LaunchGame = new System.Windows.Forms.Button();
			this.button_OpenDirectory = new System.Windows.Forms.Button();
			this.separator_03 = new DarkUI.Controls.DarkSeparator();
			this.button_Special = new System.Windows.Forms.Button();
			this.tableLayoutPanel_Main = new System.Windows.Forms.TableLayoutPanel();
			this.flowLayoutPanel_Programs = new System.Windows.Forms.FlowLayoutPanel();
			this.button_AddProgram = new System.Windows.Forms.Button();
			this.timer_ScriptButtonBlinking = new System.Windows.Forms.Timer(this.components);
			this.contextMenu_ProgramButton = new DarkUI.Controls.DarkContextMenu();
			this.menuItem_DeleteButton = new System.Windows.Forms.ToolStripMenuItem();
			this.toolTip = new System.Windows.Forms.ToolTip(this.components);
			this.flowLayoutPanel_Fixed.SuspendLayout();
			this.tableLayoutPanel_Main.SuspendLayout();
			this.flowLayoutPanel_Programs.SuspendLayout();
			this.contextMenu_ProgramButton.SuspendLayout();
			this.SuspendLayout();
			// 
			// flowLayoutPanel_Fixed
			// 
			this.flowLayoutPanel_Fixed.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.flowLayoutPanel_Fixed.AutoSize = true;
			this.flowLayoutPanel_Fixed.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.flowLayoutPanel_Fixed.Controls.Add(this.button_ExitProject);
			this.flowLayoutPanel_Fixed.Controls.Add(this.separator_01);
			this.flowLayoutPanel_Fixed.Controls.Add(this.panelButton_LevelManager);
			this.flowLayoutPanel_Fixed.Controls.Add(this.panelButton_ScriptingStudio);
			this.flowLayoutPanel_Fixed.Controls.Add(this.panelButton_PluginManager);
			this.flowLayoutPanel_Fixed.Controls.Add(this.panelButton_Miscellaneous);
			this.flowLayoutPanel_Fixed.Controls.Add(this.separator_02);
			this.flowLayoutPanel_Fixed.Controls.Add(this.button_LaunchGame);
			this.flowLayoutPanel_Fixed.Controls.Add(this.button_OpenDirectory);
			this.flowLayoutPanel_Fixed.Controls.Add(this.separator_03);
			this.flowLayoutPanel_Fixed.Controls.Add(this.button_Special);
			this.flowLayoutPanel_Fixed.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
			this.flowLayoutPanel_Fixed.Location = new System.Drawing.Point(0, 0);
			this.flowLayoutPanel_Fixed.Margin = new System.Windows.Forms.Padding(0);
			this.flowLayoutPanel_Fixed.Name = "flowLayoutPanel_Fixed";
			this.flowLayoutPanel_Fixed.Size = new System.Drawing.Size(46, 411);
			this.flowLayoutPanel_Fixed.TabIndex = 0;
			this.flowLayoutPanel_Fixed.WrapContents = false;
			// 
			// button_ExitProject
			// 
			this.button_ExitProject.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.button_ExitProject.FlatAppearance.BorderSize = 0;
			this.button_ExitProject.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.button_ExitProject.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(48)))), ((int)(((byte)(48)))), ((int)(((byte)(48)))));
			this.button_ExitProject.Image = global::TombIDE.Properties.Resources.ide_back_30;
			this.button_ExitProject.Location = new System.Drawing.Point(2, 6);
			this.button_ExitProject.Margin = new System.Windows.Forms.Padding(2, 6, 2, 3);
			this.button_ExitProject.Name = "button_ExitProject";
			this.button_ExitProject.Size = new System.Drawing.Size(42, 42);
			this.button_ExitProject.TabIndex = 0;
			this.toolTip.SetToolTip(this.button_ExitProject, "Exit Project");
			// 
			// separator_01
			// 
			this.separator_01.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
			this.separator_01.Location = new System.Drawing.Point(2, 54);
			this.separator_01.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
			this.separator_01.Name = "separator_01";
			this.separator_01.Size = new System.Drawing.Size(42, 2);
			this.separator_01.TabIndex = 1;
			// 
			// panelButton_LevelManager
			// 
			this.panelButton_LevelManager.BackgroundImage = global::TombIDE.Properties.Resources.ide_master_30;
			this.panelButton_LevelManager.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
			this.panelButton_LevelManager.Location = new System.Drawing.Point(2, 62);
			this.panelButton_LevelManager.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
			this.panelButton_LevelManager.Name = "panelButton_LevelManager";
			this.panelButton_LevelManager.Size = new System.Drawing.Size(42, 42);
			this.panelButton_LevelManager.TabIndex = 2;
			this.toolTip.SetToolTip(this.panelButton_LevelManager, "Level Manager");
			this.panelButton_LevelManager.Click += new System.EventHandler(this.panelButton_LevelManager_Click);
			// 
			// panelButton_ScriptingStudio
			// 
			this.panelButton_ScriptingStudio.BackgroundImage = global::TombIDE.Properties.Resources.ide_script_30;
			this.panelButton_ScriptingStudio.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
			this.panelButton_ScriptingStudio.Location = new System.Drawing.Point(2, 110);
			this.panelButton_ScriptingStudio.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
			this.panelButton_ScriptingStudio.Name = "panelButton_ScriptingStudio";
			this.panelButton_ScriptingStudio.Size = new System.Drawing.Size(42, 42);
			this.panelButton_ScriptingStudio.TabIndex = 3;
			this.toolTip.SetToolTip(this.panelButton_ScriptingStudio, "Scripting Studio");
			this.panelButton_ScriptingStudio.Click += new System.EventHandler(this.panelButton_ScriptingStudio_Click);
			// 
			// panelButton_PluginManager
			// 
			this.panelButton_PluginManager.BackgroundImage = global::TombIDE.Properties.Resources.ide_plugin_30;
			this.panelButton_PluginManager.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
			this.panelButton_PluginManager.Location = new System.Drawing.Point(2, 158);
			this.panelButton_PluginManager.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
			this.panelButton_PluginManager.Name = "panelButton_PluginManager";
			this.panelButton_PluginManager.Size = new System.Drawing.Size(42, 42);
			this.panelButton_PluginManager.TabIndex = 4;
			this.toolTip.SetToolTip(this.panelButton_PluginManager, "Plugin Manager");
			this.panelButton_PluginManager.Click += new System.EventHandler(this.panelButton_PluginManager_Click);
			// 
			// panelButton_Miscellaneous
			// 
			this.panelButton_Miscellaneous.BackgroundImage = global::TombIDE.Properties.Resources.ide_projectmanager;
			this.panelButton_Miscellaneous.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
			this.panelButton_Miscellaneous.Location = new System.Drawing.Point(2, 206);
			this.panelButton_Miscellaneous.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
			this.panelButton_Miscellaneous.Name = "panelButton_Miscellaneous";
			this.panelButton_Miscellaneous.Size = new System.Drawing.Size(42, 42);
			this.panelButton_Miscellaneous.TabIndex = 5;
			this.toolTip.SetToolTip(this.panelButton_Miscellaneous, "Miscellaneous Functions & Properties");
			this.panelButton_Miscellaneous.Click += new System.EventHandler(this.panelButton_Miscellaneous_Click);
			// 
			// separator_02
			// 
			this.separator_02.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
			this.separator_02.Location = new System.Drawing.Point(2, 254);
			this.separator_02.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
			this.separator_02.Name = "separator_02";
			this.separator_02.Size = new System.Drawing.Size(42, 2);
			this.separator_02.TabIndex = 6;
			this.separator_02.Text = "darkSeparator2";
			// 
			// button_LaunchGame
			// 
			this.button_LaunchGame.FlatAppearance.BorderSize = 0;
			this.button_LaunchGame.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.button_LaunchGame.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(48)))), ((int)(((byte)(48)))), ((int)(((byte)(48)))));
			this.button_LaunchGame.Image = global::TombIDE.Properties.Resources.general_edit_16;
			this.button_LaunchGame.Location = new System.Drawing.Point(2, 262);
			this.button_LaunchGame.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
			this.button_LaunchGame.Name = "button_LaunchGame";
			this.button_LaunchGame.Size = new System.Drawing.Size(42, 42);
			this.button_LaunchGame.TabIndex = 7;
			this.toolTip.SetToolTip(this.button_LaunchGame, "Launch game (F4)");
			this.button_LaunchGame.Click += new System.EventHandler(this.button_LaunchGame_Click);
			// 
			// button_OpenDirectory
			// 
			this.button_OpenDirectory.FlatAppearance.BorderSize = 0;
			this.button_OpenDirectory.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.button_OpenDirectory.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(48)))), ((int)(((byte)(48)))), ((int)(((byte)(48)))));
			this.button_OpenDirectory.Image = global::TombIDE.Properties.Resources.ide_folder_30;
			this.button_OpenDirectory.Location = new System.Drawing.Point(2, 310);
			this.button_OpenDirectory.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
			this.button_OpenDirectory.Name = "button_OpenDirectory";
			this.button_OpenDirectory.Size = new System.Drawing.Size(42, 42);
			this.button_OpenDirectory.TabIndex = 8;
			this.toolTip.SetToolTip(this.button_OpenDirectory, "Open project directory (F3)");
			this.button_OpenDirectory.Click += new System.EventHandler(this.button_OpenDirectory_Click);
			// 
			// separator_03
			// 
			this.separator_03.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
			this.separator_03.Location = new System.Drawing.Point(2, 358);
			this.separator_03.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
			this.separator_03.Name = "separator_03";
			this.separator_03.Size = new System.Drawing.Size(42, 2);
			this.separator_03.TabIndex = 9;
			this.separator_03.Text = "darkSeparator3";
			// 
			// button_Special
			// 
			this.button_Special.FlatAppearance.BorderSize = 0;
			this.button_Special.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.button_Special.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(48)))), ((int)(((byte)(48)))), ((int)(((byte)(48)))));
			this.button_Special.Image = global::TombIDE.Properties.Resources.general_edit_16;
			this.button_Special.Location = new System.Drawing.Point(2, 366);
			this.button_Special.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
			this.button_Special.Name = "button_Special";
			this.button_Special.Size = new System.Drawing.Size(42, 42);
			this.button_Special.TabIndex = 10;
			this.button_Special.Click += new System.EventHandler(this.Special_LaunchFLEP);
			// 
			// tableLayoutPanel_Main
			// 
			this.tableLayoutPanel_Main.AutoSize = true;
			this.tableLayoutPanel_Main.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.tableLayoutPanel_Main.ColumnCount = 1;
			this.tableLayoutPanel_Main.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanel_Main.Controls.Add(this.flowLayoutPanel_Fixed, 0, 0);
			this.tableLayoutPanel_Main.Controls.Add(this.flowLayoutPanel_Programs, 0, 1);
			this.tableLayoutPanel_Main.Location = new System.Drawing.Point(0, 0);
			this.tableLayoutPanel_Main.Margin = new System.Windows.Forms.Padding(0);
			this.tableLayoutPanel_Main.Name = "tableLayoutPanel_Main";
			this.tableLayoutPanel_Main.RowCount = 2;
			this.tableLayoutPanel_Main.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel_Main.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanel_Main.Size = new System.Drawing.Size(46, 459);
			this.tableLayoutPanel_Main.TabIndex = 1;
			// 
			// flowLayoutPanel_Programs
			// 
			this.flowLayoutPanel_Programs.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.flowLayoutPanel_Programs.AutoSize = true;
			this.flowLayoutPanel_Programs.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.flowLayoutPanel_Programs.Controls.Add(this.button_AddProgram);
			this.flowLayoutPanel_Programs.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
			this.flowLayoutPanel_Programs.Location = new System.Drawing.Point(0, 411);
			this.flowLayoutPanel_Programs.Margin = new System.Windows.Forms.Padding(0);
			this.flowLayoutPanel_Programs.Name = "flowLayoutPanel_Programs";
			this.flowLayoutPanel_Programs.Size = new System.Drawing.Size(46, 48);
			this.flowLayoutPanel_Programs.TabIndex = 1;
			this.flowLayoutPanel_Programs.WrapContents = false;
			this.flowLayoutPanel_Programs.MouseMove += new System.Windows.Forms.MouseEventHandler(this.panel_Programs_MouseMove);
			this.flowLayoutPanel_Programs.MouseUp += new System.Windows.Forms.MouseEventHandler(this.panel_Programs_MouseUp);
			// 
			// button_AddProgram
			// 
			this.button_AddProgram.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.button_AddProgram.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(48)))), ((int)(((byte)(48)))), ((int)(((byte)(48)))));
			this.button_AddProgram.Image = global::TombIDE.Properties.Resources.general_plus_math_16;
			this.button_AddProgram.Location = new System.Drawing.Point(2, 3);
			this.button_AddProgram.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
			this.button_AddProgram.Name = "button_AddProgram";
			this.button_AddProgram.Size = new System.Drawing.Size(42, 42);
			this.button_AddProgram.TabIndex = 11;
			this.toolTip.SetToolTip(this.button_AddProgram, "Add program shortcut");
			this.button_AddProgram.Click += new System.EventHandler(this.button_AddProgram_Click);
			// 
			// timer_ScriptButtonBlinking
			// 
			this.timer_ScriptButtonBlinking.Tick += new System.EventHandler(this.timer_ScriptButtonBlinking_Tick);
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
			// SideBar
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.AutoScroll = true;
			this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(48)))), ((int)(((byte)(48)))), ((int)(((byte)(48)))));
			this.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.Controls.Add(this.tableLayoutPanel_Main);
			this.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.Name = "SideBar";
			this.Size = new System.Drawing.Size(464, 398);
			this.flowLayoutPanel_Fixed.ResumeLayout(false);
			this.tableLayoutPanel_Main.ResumeLayout(false);
			this.tableLayoutPanel_Main.PerformLayout();
			this.flowLayoutPanel_Programs.ResumeLayout(false);
			this.contextMenu_ProgramButton.ResumeLayout(false);
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private DarkUI.Controls.DarkSeparator separator_01;
		private DarkUI.Controls.DarkSeparator separator_02;
		private DarkUI.Controls.DarkSeparator separator_03;
		private System.Windows.Forms.Button button_AddProgram;
		private System.Windows.Forms.Button button_ExitProject;
		private System.Windows.Forms.Button button_LaunchGame;
		private System.Windows.Forms.Button button_OpenDirectory;
		private System.Windows.Forms.Button button_Special;
		private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel_Fixed;
		private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel_Programs;
		private System.Windows.Forms.Panel panelButton_LevelManager;
		private System.Windows.Forms.Panel panelButton_Miscellaneous;
		private System.Windows.Forms.Panel panelButton_PluginManager;
		private System.Windows.Forms.Panel panelButton_ScriptingStudio;
		private System.Windows.Forms.TableLayoutPanel tableLayoutPanel_Main;
		private System.Windows.Forms.Timer timer_ScriptButtonBlinking;
		private DarkUI.Controls.DarkContextMenu contextMenu_ProgramButton;
		private System.Windows.Forms.ToolStripMenuItem menuItem_DeleteButton;
		private System.Windows.Forms.ToolTip toolTip;
	}
}
