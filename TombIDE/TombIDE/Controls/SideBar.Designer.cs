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
			components = new System.ComponentModel.Container();
			var resources = new System.ComponentModel.ComponentResourceManager(typeof(SideBar));
			flowLayoutPanel_Fixed = new System.Windows.Forms.FlowLayoutPanel();
			button_ExitProject = new System.Windows.Forms.Button();
			separator_01 = new DarkUI.Controls.DarkSeparator();
			panelButton_LevelManager = new System.Windows.Forms.Panel();
			label_Levels = new DarkUI.Controls.DarkLabel();
			panel_Icon_Levels = new System.Windows.Forms.Panel();
			panelButton_ScriptingStudio = new System.Windows.Forms.Panel();
			label_Script = new DarkUI.Controls.DarkLabel();
			panel_Icon_Script = new System.Windows.Forms.Panel();
			panelButton_PluginManager = new System.Windows.Forms.Panel();
			label_Plugins = new DarkUI.Controls.DarkLabel();
			panel_Icon_Plugins = new System.Windows.Forms.Panel();
			panelButton_Miscellaneous = new System.Windows.Forms.Panel();
			label_Misc = new DarkUI.Controls.DarkLabel();
			panel_Icon_Misc = new System.Windows.Forms.Panel();
			separator_02 = new DarkUI.Controls.DarkSeparator();
			button_LaunchGame = new System.Windows.Forms.Button();
			button_OpenDirectory = new System.Windows.Forms.Button();
			separator_03 = new DarkUI.Controls.DarkSeparator();
			button_Special = new System.Windows.Forms.Button();
			tableLayoutPanel_Main = new System.Windows.Forms.TableLayoutPanel();
			flowLayoutPanel_Programs = new System.Windows.Forms.FlowLayoutPanel();
			button_AddProgram = new System.Windows.Forms.Button();
			timer_ScriptButtonBlinking = new System.Windows.Forms.Timer(components);
			contextMenu_ProgramButton = new DarkUI.Controls.DarkContextMenu();
			menuItem_DeleteButton = new System.Windows.Forms.ToolStripMenuItem();
			toolTip = new System.Windows.Forms.ToolTip(components);
			contextMenu_TIDE = new DarkUI.Controls.DarkContextMenu();
			button_BackToStart = new System.Windows.Forms.ToolStripMenuItem();
			toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
			button_Publish = new System.Windows.Forms.ToolStripMenuItem();
			button_Update = new System.Windows.Forms.ToolStripMenuItem();
			toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
			button_Exit = new System.Windows.Forms.ToolStripMenuItem();
			flowLayoutPanel_Fixed.SuspendLayout();
			panelButton_LevelManager.SuspendLayout();
			panelButton_ScriptingStudio.SuspendLayout();
			panelButton_PluginManager.SuspendLayout();
			panelButton_Miscellaneous.SuspendLayout();
			tableLayoutPanel_Main.SuspendLayout();
			flowLayoutPanel_Programs.SuspendLayout();
			contextMenu_ProgramButton.SuspendLayout();
			contextMenu_TIDE.SuspendLayout();
			SuspendLayout();
			// 
			// flowLayoutPanel_Fixed
			// 
			flowLayoutPanel_Fixed.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
			flowLayoutPanel_Fixed.AutoSize = true;
			flowLayoutPanel_Fixed.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			flowLayoutPanel_Fixed.Controls.Add(button_ExitProject);
			flowLayoutPanel_Fixed.Controls.Add(separator_01);
			flowLayoutPanel_Fixed.Controls.Add(panelButton_LevelManager);
			flowLayoutPanel_Fixed.Controls.Add(panelButton_ScriptingStudio);
			flowLayoutPanel_Fixed.Controls.Add(panelButton_PluginManager);
			flowLayoutPanel_Fixed.Controls.Add(panelButton_Miscellaneous);
			flowLayoutPanel_Fixed.Controls.Add(separator_02);
			flowLayoutPanel_Fixed.Controls.Add(button_LaunchGame);
			flowLayoutPanel_Fixed.Controls.Add(button_OpenDirectory);
			flowLayoutPanel_Fixed.Controls.Add(separator_03);
			flowLayoutPanel_Fixed.Controls.Add(button_Special);
			flowLayoutPanel_Fixed.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
			flowLayoutPanel_Fixed.Location = new System.Drawing.Point(0, 0);
			flowLayoutPanel_Fixed.Margin = new System.Windows.Forms.Padding(0);
			flowLayoutPanel_Fixed.Name = "flowLayoutPanel_Fixed";
			flowLayoutPanel_Fixed.Size = new System.Drawing.Size(48, 459);
			flowLayoutPanel_Fixed.TabIndex = 0;
			flowLayoutPanel_Fixed.WrapContents = false;
			// 
			// button_ExitProject
			// 
			button_ExitProject.FlatAppearance.BorderSize = 0;
			button_ExitProject.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			button_ExitProject.ForeColor = System.Drawing.Color.FromArgb(48, 48, 48);
			button_ExitProject.Image = (System.Drawing.Image)resources.GetObject("button_ExitProject.Image");
			button_ExitProject.Location = new System.Drawing.Point(3, 6);
			button_ExitProject.Margin = new System.Windows.Forms.Padding(3, 6, 3, 3);
			button_ExitProject.Name = "button_ExitProject";
			button_ExitProject.Size = new System.Drawing.Size(42, 42);
			button_ExitProject.TabIndex = 0;
			toolTip.SetToolTip(button_ExitProject, "More Actions...");
			button_ExitProject.MouseUp += button_ExitProject_MouseUp;
			// 
			// separator_01
			// 
			separator_01.Anchor = System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
			separator_01.Location = new System.Drawing.Point(3, 54);
			separator_01.Name = "separator_01";
			separator_01.Size = new System.Drawing.Size(42, 2);
			separator_01.TabIndex = 1;
			// 
			// panelButton_LevelManager
			// 
			panelButton_LevelManager.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
			panelButton_LevelManager.Controls.Add(label_Levels);
			panelButton_LevelManager.Controls.Add(panel_Icon_Levels);
			panelButton_LevelManager.Location = new System.Drawing.Point(0, 65);
			panelButton_LevelManager.Margin = new System.Windows.Forms.Padding(0, 6, 0, 6);
			panelButton_LevelManager.Name = "panelButton_LevelManager";
			panelButton_LevelManager.Size = new System.Drawing.Size(48, 48);
			panelButton_LevelManager.TabIndex = 2;
			// 
			// label_Levels
			// 
			label_Levels.Cursor = System.Windows.Forms.Cursors.Hand;
			label_Levels.Dock = System.Windows.Forms.DockStyle.Fill;
			label_Levels.Font = new System.Drawing.Font("Segoe UI", 6.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
			label_Levels.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
			label_Levels.Location = new System.Drawing.Point(0, 32);
			label_Levels.Margin = new System.Windows.Forms.Padding(0);
			label_Levels.Name = "label_Levels";
			label_Levels.Padding = new System.Windows.Forms.Padding(2, 0, 0, 0);
			label_Levels.Size = new System.Drawing.Size(48, 16);
			label_Levels.TabIndex = 4;
			label_Levels.Text = "Levels";
			label_Levels.TextAlign = System.Drawing.ContentAlignment.TopCenter;
			toolTip.SetToolTip(label_Levels, "Level Manager");
			label_Levels.Click += panelButton_LevelManager_Click;
			// 
			// panel_Icon_Levels
			// 
			panel_Icon_Levels.BackgroundImage = Properties.Resources.ide_master_30;
			panel_Icon_Levels.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
			panel_Icon_Levels.Cursor = System.Windows.Forms.Cursors.Hand;
			panel_Icon_Levels.Dock = System.Windows.Forms.DockStyle.Top;
			panel_Icon_Levels.Location = new System.Drawing.Point(0, 0);
			panel_Icon_Levels.Margin = new System.Windows.Forms.Padding(0);
			panel_Icon_Levels.Name = "panel_Icon_Levels";
			panel_Icon_Levels.Size = new System.Drawing.Size(48, 32);
			panel_Icon_Levels.TabIndex = 3;
			toolTip.SetToolTip(panel_Icon_Levels, "Level Manager");
			panel_Icon_Levels.Click += panelButton_LevelManager_Click;
			// 
			// panelButton_ScriptingStudio
			// 
			panelButton_ScriptingStudio.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
			panelButton_ScriptingStudio.Controls.Add(label_Script);
			panelButton_ScriptingStudio.Controls.Add(panel_Icon_Script);
			panelButton_ScriptingStudio.Location = new System.Drawing.Point(0, 125);
			panelButton_ScriptingStudio.Margin = new System.Windows.Forms.Padding(0, 6, 0, 6);
			panelButton_ScriptingStudio.Name = "panelButton_ScriptingStudio";
			panelButton_ScriptingStudio.Size = new System.Drawing.Size(48, 48);
			panelButton_ScriptingStudio.TabIndex = 3;
			// 
			// label_Script
			// 
			label_Script.Cursor = System.Windows.Forms.Cursors.Hand;
			label_Script.Dock = System.Windows.Forms.DockStyle.Fill;
			label_Script.Font = new System.Drawing.Font("Segoe UI", 6.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
			label_Script.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
			label_Script.Location = new System.Drawing.Point(0, 32);
			label_Script.Margin = new System.Windows.Forms.Padding(0);
			label_Script.Name = "label_Script";
			label_Script.Padding = new System.Windows.Forms.Padding(2, 0, 0, 0);
			label_Script.Size = new System.Drawing.Size(48, 16);
			label_Script.TabIndex = 3;
			label_Script.Text = "Script";
			label_Script.TextAlign = System.Drawing.ContentAlignment.TopCenter;
			toolTip.SetToolTip(label_Script, "Scripting Studio");
			label_Script.Click += panelButton_ScriptingStudio_Click;
			// 
			// panel_Icon_Script
			// 
			panel_Icon_Script.BackgroundImage = Properties.Resources.ide_script_30;
			panel_Icon_Script.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
			panel_Icon_Script.Cursor = System.Windows.Forms.Cursors.Hand;
			panel_Icon_Script.Dock = System.Windows.Forms.DockStyle.Top;
			panel_Icon_Script.Location = new System.Drawing.Point(0, 0);
			panel_Icon_Script.Margin = new System.Windows.Forms.Padding(0);
			panel_Icon_Script.Name = "panel_Icon_Script";
			panel_Icon_Script.Size = new System.Drawing.Size(48, 32);
			panel_Icon_Script.TabIndex = 2;
			toolTip.SetToolTip(panel_Icon_Script, "Scripting Studio");
			panel_Icon_Script.Click += panelButton_ScriptingStudio_Click;
			// 
			// panelButton_PluginManager
			// 
			panelButton_PluginManager.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
			panelButton_PluginManager.Controls.Add(label_Plugins);
			panelButton_PluginManager.Controls.Add(panel_Icon_Plugins);
			panelButton_PluginManager.Location = new System.Drawing.Point(0, 185);
			panelButton_PluginManager.Margin = new System.Windows.Forms.Padding(0, 6, 0, 6);
			panelButton_PluginManager.Name = "panelButton_PluginManager";
			panelButton_PluginManager.Size = new System.Drawing.Size(48, 48);
			panelButton_PluginManager.TabIndex = 4;
			// 
			// label_Plugins
			// 
			label_Plugins.Cursor = System.Windows.Forms.Cursors.Hand;
			label_Plugins.Dock = System.Windows.Forms.DockStyle.Fill;
			label_Plugins.Font = new System.Drawing.Font("Segoe UI", 6.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
			label_Plugins.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
			label_Plugins.Location = new System.Drawing.Point(0, 32);
			label_Plugins.Margin = new System.Windows.Forms.Padding(0);
			label_Plugins.Name = "label_Plugins";
			label_Plugins.Padding = new System.Windows.Forms.Padding(2, 0, 0, 0);
			label_Plugins.Size = new System.Drawing.Size(48, 16);
			label_Plugins.TabIndex = 2;
			label_Plugins.Text = "Plugins";
			label_Plugins.TextAlign = System.Drawing.ContentAlignment.TopCenter;
			toolTip.SetToolTip(label_Plugins, "Plugin Manager");
			label_Plugins.Click += panelButton_PluginManager_Click;
			// 
			// panel_Icon_Plugins
			// 
			panel_Icon_Plugins.BackgroundImage = Properties.Resources.ide_plugin_30;
			panel_Icon_Plugins.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
			panel_Icon_Plugins.Cursor = System.Windows.Forms.Cursors.Hand;
			panel_Icon_Plugins.Dock = System.Windows.Forms.DockStyle.Top;
			panel_Icon_Plugins.Location = new System.Drawing.Point(0, 0);
			panel_Icon_Plugins.Margin = new System.Windows.Forms.Padding(0);
			panel_Icon_Plugins.Name = "panel_Icon_Plugins";
			panel_Icon_Plugins.Size = new System.Drawing.Size(48, 32);
			panel_Icon_Plugins.TabIndex = 1;
			toolTip.SetToolTip(panel_Icon_Plugins, "Plugin Manager");
			panel_Icon_Plugins.Click += panelButton_PluginManager_Click;
			// 
			// panelButton_Miscellaneous
			// 
			panelButton_Miscellaneous.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
			panelButton_Miscellaneous.Controls.Add(label_Misc);
			panelButton_Miscellaneous.Controls.Add(panel_Icon_Misc);
			panelButton_Miscellaneous.Location = new System.Drawing.Point(0, 245);
			panelButton_Miscellaneous.Margin = new System.Windows.Forms.Padding(0, 6, 0, 6);
			panelButton_Miscellaneous.Name = "panelButton_Miscellaneous";
			panelButton_Miscellaneous.Size = new System.Drawing.Size(48, 48);
			panelButton_Miscellaneous.TabIndex = 5;
			// 
			// label_Misc
			// 
			label_Misc.Cursor = System.Windows.Forms.Cursors.Hand;
			label_Misc.Dock = System.Windows.Forms.DockStyle.Fill;
			label_Misc.Font = new System.Drawing.Font("Segoe UI", 6.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
			label_Misc.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
			label_Misc.Location = new System.Drawing.Point(0, 32);
			label_Misc.Margin = new System.Windows.Forms.Padding(0);
			label_Misc.Name = "label_Misc";
			label_Misc.Padding = new System.Windows.Forms.Padding(2, 0, 0, 0);
			label_Misc.Size = new System.Drawing.Size(48, 16);
			label_Misc.TabIndex = 1;
			label_Misc.Text = "Utilities";
			label_Misc.TextAlign = System.Drawing.ContentAlignment.TopCenter;
			toolTip.SetToolTip(label_Misc, "Utilities");
			label_Misc.Click += panelButton_Miscellaneous_Click;
			// 
			// panel_Icon_Misc
			// 
			panel_Icon_Misc.BackgroundImage = Properties.Resources.ide_projectmanager;
			panel_Icon_Misc.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
			panel_Icon_Misc.Cursor = System.Windows.Forms.Cursors.Hand;
			panel_Icon_Misc.Dock = System.Windows.Forms.DockStyle.Top;
			panel_Icon_Misc.Location = new System.Drawing.Point(0, 0);
			panel_Icon_Misc.Margin = new System.Windows.Forms.Padding(0);
			panel_Icon_Misc.Name = "panel_Icon_Misc";
			panel_Icon_Misc.Size = new System.Drawing.Size(48, 32);
			panel_Icon_Misc.TabIndex = 0;
			toolTip.SetToolTip(panel_Icon_Misc, "Utilities");
			panel_Icon_Misc.Click += panelButton_Miscellaneous_Click;
			// 
			// separator_02
			// 
			separator_02.Anchor = System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
			separator_02.Location = new System.Drawing.Point(3, 302);
			separator_02.Name = "separator_02";
			separator_02.Size = new System.Drawing.Size(42, 2);
			separator_02.TabIndex = 6;
			separator_02.Text = "darkSeparator2";
			// 
			// button_LaunchGame
			// 
			button_LaunchGame.FlatAppearance.BorderSize = 0;
			button_LaunchGame.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			button_LaunchGame.ForeColor = System.Drawing.Color.FromArgb(48, 48, 48);
			button_LaunchGame.Image = Properties.Resources.general_edit_16;
			button_LaunchGame.Location = new System.Drawing.Point(3, 310);
			button_LaunchGame.Name = "button_LaunchGame";
			button_LaunchGame.Size = new System.Drawing.Size(42, 42);
			button_LaunchGame.TabIndex = 7;
			toolTip.SetToolTip(button_LaunchGame, "Launch game (F4)");
			button_LaunchGame.Click += button_LaunchGame_Click;
			// 
			// button_OpenDirectory
			// 
			button_OpenDirectory.FlatAppearance.BorderSize = 0;
			button_OpenDirectory.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			button_OpenDirectory.ForeColor = System.Drawing.Color.FromArgb(48, 48, 48);
			button_OpenDirectory.Image = Properties.Resources.ide_folder_30;
			button_OpenDirectory.Location = new System.Drawing.Point(3, 358);
			button_OpenDirectory.Name = "button_OpenDirectory";
			button_OpenDirectory.Size = new System.Drawing.Size(42, 42);
			button_OpenDirectory.TabIndex = 8;
			toolTip.SetToolTip(button_OpenDirectory, "Open project directory (F3)");
			button_OpenDirectory.Click += button_OpenDirectory_Click;
			// 
			// separator_03
			// 
			separator_03.Anchor = System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
			separator_03.Location = new System.Drawing.Point(3, 406);
			separator_03.Name = "separator_03";
			separator_03.Size = new System.Drawing.Size(42, 2);
			separator_03.TabIndex = 9;
			separator_03.Text = "darkSeparator3";
			// 
			// button_Special
			// 
			button_Special.FlatAppearance.BorderSize = 0;
			button_Special.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			button_Special.ForeColor = System.Drawing.Color.FromArgb(48, 48, 48);
			button_Special.Image = Properties.Resources.general_edit_16;
			button_Special.Location = new System.Drawing.Point(3, 414);
			button_Special.Name = "button_Special";
			button_Special.Size = new System.Drawing.Size(42, 42);
			button_Special.TabIndex = 10;
			// 
			// tableLayoutPanel_Main
			// 
			tableLayoutPanel_Main.AutoSize = true;
			tableLayoutPanel_Main.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			tableLayoutPanel_Main.ColumnCount = 1;
			tableLayoutPanel_Main.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			tableLayoutPanel_Main.Controls.Add(flowLayoutPanel_Fixed, 0, 0);
			tableLayoutPanel_Main.Controls.Add(flowLayoutPanel_Programs, 0, 1);
			tableLayoutPanel_Main.Location = new System.Drawing.Point(0, 0);
			tableLayoutPanel_Main.Margin = new System.Windows.Forms.Padding(0);
			tableLayoutPanel_Main.Name = "tableLayoutPanel_Main";
			tableLayoutPanel_Main.RowCount = 2;
			tableLayoutPanel_Main.RowStyles.Add(new System.Windows.Forms.RowStyle());
			tableLayoutPanel_Main.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
			tableLayoutPanel_Main.Size = new System.Drawing.Size(48, 507);
			tableLayoutPanel_Main.TabIndex = 1;
			// 
			// flowLayoutPanel_Programs
			// 
			flowLayoutPanel_Programs.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
			flowLayoutPanel_Programs.AutoSize = true;
			flowLayoutPanel_Programs.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			flowLayoutPanel_Programs.Controls.Add(button_AddProgram);
			flowLayoutPanel_Programs.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
			flowLayoutPanel_Programs.Location = new System.Drawing.Point(0, 459);
			flowLayoutPanel_Programs.Margin = new System.Windows.Forms.Padding(0);
			flowLayoutPanel_Programs.Name = "flowLayoutPanel_Programs";
			flowLayoutPanel_Programs.Size = new System.Drawing.Size(48, 48);
			flowLayoutPanel_Programs.TabIndex = 1;
			flowLayoutPanel_Programs.WrapContents = false;
			flowLayoutPanel_Programs.MouseMove += panel_Programs_MouseMove;
			flowLayoutPanel_Programs.MouseUp += panel_Programs_MouseUp;
			// 
			// button_AddProgram
			// 
			button_AddProgram.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			button_AddProgram.ForeColor = System.Drawing.Color.FromArgb(48, 48, 48);
			button_AddProgram.Image = Properties.Resources.general_plus_math_16;
			button_AddProgram.Location = new System.Drawing.Point(3, 3);
			button_AddProgram.Name = "button_AddProgram";
			button_AddProgram.Size = new System.Drawing.Size(42, 42);
			button_AddProgram.TabIndex = 11;
			toolTip.SetToolTip(button_AddProgram, "Add program shortcut");
			button_AddProgram.Click += button_AddProgram_Click;
			// 
			// timer_ScriptButtonBlinking
			// 
			timer_ScriptButtonBlinking.Tick += timer_ScriptButtonBlinking_Tick;
			// 
			// contextMenu_ProgramButton
			// 
			contextMenu_ProgramButton.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
			contextMenu_ProgramButton.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
			contextMenu_ProgramButton.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { menuItem_DeleteButton });
			contextMenu_ProgramButton.Name = "contextMenu_ProgramButton";
			contextMenu_ProgramButton.Size = new System.Drawing.Size(156, 26);
			// 
			// menuItem_DeleteButton
			// 
			menuItem_DeleteButton.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
			menuItem_DeleteButton.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
			menuItem_DeleteButton.Image = Properties.Resources.general_trash_16;
			menuItem_DeleteButton.Name = "menuItem_DeleteButton";
			menuItem_DeleteButton.Size = new System.Drawing.Size(155, 22);
			menuItem_DeleteButton.Text = "Delete Shortcut";
			menuItem_DeleteButton.Click += menuItem_DeleteButton_Click;
			// 
			// contextMenu_TIDE
			// 
			contextMenu_TIDE.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
			contextMenu_TIDE.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
			contextMenu_TIDE.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { button_BackToStart, toolStripSeparator1, button_Publish, button_Update, toolStripSeparator2, button_Exit });
			contextMenu_TIDE.Name = "contextMenu_TIDE";
			contextMenu_TIDE.Size = new System.Drawing.Size(305, 106);
			// 
			// button_BackToStart
			// 
			button_BackToStart.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
			button_BackToStart.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
			button_BackToStart.Image = Properties.Resources.general_ArrowUp_16;
			button_BackToStart.Name = "button_BackToStart";
			button_BackToStart.Size = new System.Drawing.Size(304, 22);
			button_BackToStart.Text = "Back to Start Window...";
			button_BackToStart.Click += button_BackToStart_Click;
			// 
			// toolStripSeparator1
			// 
			toolStripSeparator1.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
			toolStripSeparator1.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
			toolStripSeparator1.Margin = new System.Windows.Forms.Padding(0, 0, 0, 1);
			toolStripSeparator1.Name = "toolStripSeparator1";
			toolStripSeparator1.Size = new System.Drawing.Size(301, 6);
			// 
			// button_Publish
			// 
			button_Publish.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
			button_Publish.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
			button_Publish.Image = (System.Drawing.Image)resources.GetObject("button_Publish.Image");
			button_Publish.Name = "button_Publish";
			button_Publish.Size = new System.Drawing.Size(304, 22);
			button_Publish.Text = "Create a \"Ready to Publish\" Game Archive...";
			button_Publish.Click += button_Publish_Click;
			// 
			// button_Update
			// 
			button_Update.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
			button_Update.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
			button_Update.Image = (System.Drawing.Image)resources.GetObject("button_Update.Image");
			button_Update.Name = "button_Update";
			button_Update.Size = new System.Drawing.Size(304, 22);
			button_Update.Text = "Reapply Engine Update...";
			button_Update.Click += button_Update_Click;
			// 
			// toolStripSeparator2
			// 
			toolStripSeparator2.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
			toolStripSeparator2.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
			toolStripSeparator2.Margin = new System.Windows.Forms.Padding(0, 0, 0, 1);
			toolStripSeparator2.Name = "toolStripSeparator2";
			toolStripSeparator2.Size = new System.Drawing.Size(301, 6);
			// 
			// button_Exit
			// 
			button_Exit.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
			button_Exit.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
			button_Exit.Image = (System.Drawing.Image)resources.GetObject("button_Exit.Image");
			button_Exit.Name = "button_Exit";
			button_Exit.Size = new System.Drawing.Size(304, 22);
			button_Exit.Text = "Exit TombIDE";
			button_Exit.Click += button_Exit_Click;
			// 
			// SideBar
			// 
			AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			AutoScroll = true;
			BackColor = System.Drawing.Color.FromArgb(48, 48, 48);
			BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			Controls.Add(tableLayoutPanel_Main);
			Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
			Name = "SideBar";
			Size = new System.Drawing.Size(256, 512);
			flowLayoutPanel_Fixed.ResumeLayout(false);
			panelButton_LevelManager.ResumeLayout(false);
			panelButton_ScriptingStudio.ResumeLayout(false);
			panelButton_PluginManager.ResumeLayout(false);
			panelButton_Miscellaneous.ResumeLayout(false);
			tableLayoutPanel_Main.ResumeLayout(false);
			tableLayoutPanel_Main.PerformLayout();
			flowLayoutPanel_Programs.ResumeLayout(false);
			contextMenu_ProgramButton.ResumeLayout(false);
			contextMenu_TIDE.ResumeLayout(false);
			ResumeLayout(false);
			PerformLayout();
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
        private DarkUI.Controls.DarkContextMenu contextMenu_TIDE;
        private System.Windows.Forms.ToolStripMenuItem button_BackToStart;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripMenuItem button_Exit;
		private System.Windows.Forms.ToolStripMenuItem button_Publish;
		private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
		private System.Windows.Forms.ToolStripMenuItem button_Update;
		private DarkUI.Controls.DarkLabel label_Misc;
		private System.Windows.Forms.Panel panel_Icon_Misc;
		private DarkUI.Controls.DarkLabel label_Plugins;
		private System.Windows.Forms.Panel panel_Icon_Plugins;
		private DarkUI.Controls.DarkLabel label_Levels;
		private System.Windows.Forms.Panel panel_Icon_Levels;
		private DarkUI.Controls.DarkLabel label_Script;
		private System.Windows.Forms.Panel panel_Icon_Script;
	}
}
