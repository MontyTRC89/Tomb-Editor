namespace TombIDE.ProjectMaster
{
	partial class SettingsProjectInfo
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
			this.button_ChangeLevelsPath = new DarkUI.Controls.DarkButton();
			this.button_ChangeScriptPath = new DarkUI.Controls.DarkButton();
			this.button_OpenEngineFolder = new DarkUI.Controls.DarkButton();
			this.button_OpenLevelsFolder = new DarkUI.Controls.DarkButton();
			this.button_OpenProjectFolder = new DarkUI.Controls.DarkButton();
			this.button_OpenScriptFolder = new DarkUI.Controls.DarkButton();
			this.checkBox_FullPaths = new DarkUI.Controls.DarkCheckBox();
			this.label_01 = new DarkUI.Controls.DarkLabel();
			this.label_02 = new DarkUI.Controls.DarkLabel();
			this.label_03 = new DarkUI.Controls.DarkLabel();
			this.label_04 = new DarkUI.Controls.DarkLabel();
			this.label_05 = new DarkUI.Controls.DarkLabel();
			this.sectionPanel = new DarkUI.Controls.DarkSectionPanel();
			this.textBox_EnginePath = new System.Windows.Forms.TextBox();
			this.textBox_ProjectName = new System.Windows.Forms.TextBox();
			this.textBox_LevelsPath = new System.Windows.Forms.TextBox();
			this.textBox_ProjectPath = new System.Windows.Forms.TextBox();
			this.textBox_ScriptPath = new System.Windows.Forms.TextBox();
			this.toolTip = new System.Windows.Forms.ToolTip(this.components);
			this.sectionPanel.SuspendLayout();
			this.SuspendLayout();
			// 
			// button_ChangeLevelsPath
			// 
			this.button_ChangeLevelsPath.Checked = false;
			this.button_ChangeLevelsPath.Image = global::TombIDE.ProjectMaster.Properties.Resources.general_edit_16;
			this.button_ChangeLevelsPath.Location = new System.Drawing.Point(573, 257);
			this.button_ChangeLevelsPath.Name = "button_ChangeLevelsPath";
			this.button_ChangeLevelsPath.Size = new System.Drawing.Size(26, 26);
			this.button_ChangeLevelsPath.TabIndex = 14;
			this.toolTip.SetToolTip(this.button_ChangeLevelsPath, "Change...");
			this.button_ChangeLevelsPath.Click += new System.EventHandler(this.button_ChangeLevelsPath_Click);
			// 
			// button_ChangeScriptPath
			// 
			this.button_ChangeScriptPath.Checked = false;
			this.button_ChangeScriptPath.Image = global::TombIDE.ProjectMaster.Properties.Resources.general_edit_16;
			this.button_ChangeScriptPath.Location = new System.Drawing.Point(573, 206);
			this.button_ChangeScriptPath.Name = "button_ChangeScriptPath";
			this.button_ChangeScriptPath.Size = new System.Drawing.Size(26, 26);
			this.button_ChangeScriptPath.TabIndex = 10;
			this.toolTip.SetToolTip(this.button_ChangeScriptPath, "Change...");
			this.button_ChangeScriptPath.Click += new System.EventHandler(this.button_ChangeScriptPath_Click);
			// 
			// button_OpenEngineFolder
			// 
			this.button_OpenEngineFolder.Checked = false;
			this.button_OpenEngineFolder.Image = global::TombIDE.ProjectMaster.Properties.Resources.forward_arrow_16;
			this.button_OpenEngineFolder.Location = new System.Drawing.Point(605, 155);
			this.button_OpenEngineFolder.Margin = new System.Windows.Forms.Padding(3, 3, 6, 3);
			this.button_OpenEngineFolder.Name = "button_OpenEngineFolder";
			this.button_OpenEngineFolder.Size = new System.Drawing.Size(26, 26);
			this.button_OpenEngineFolder.TabIndex = 7;
			this.toolTip.SetToolTip(this.button_OpenEngineFolder, "Open in Explorer");
			this.button_OpenEngineFolder.Click += new System.EventHandler(this.button_OpenEngineFolder_Click);
			// 
			// button_OpenLevelsFolder
			// 
			this.button_OpenLevelsFolder.Checked = false;
			this.button_OpenLevelsFolder.Image = global::TombIDE.ProjectMaster.Properties.Resources.forward_arrow_16;
			this.button_OpenLevelsFolder.Location = new System.Drawing.Point(605, 257);
			this.button_OpenLevelsFolder.Margin = new System.Windows.Forms.Padding(3, 3, 6, 3);
			this.button_OpenLevelsFolder.Name = "button_OpenLevelsFolder";
			this.button_OpenLevelsFolder.Size = new System.Drawing.Size(26, 26);
			this.button_OpenLevelsFolder.TabIndex = 15;
			this.toolTip.SetToolTip(this.button_OpenLevelsFolder, "Open in Explorer");
			this.button_OpenLevelsFolder.Click += new System.EventHandler(this.button_OpenLevelsFolder_Click);
			// 
			// button_OpenProjectFolder
			// 
			this.button_OpenProjectFolder.Checked = false;
			this.button_OpenProjectFolder.Image = global::TombIDE.ProjectMaster.Properties.Resources.forward_arrow_16;
			this.button_OpenProjectFolder.Location = new System.Drawing.Point(605, 104);
			this.button_OpenProjectFolder.Margin = new System.Windows.Forms.Padding(3, 3, 6, 3);
			this.button_OpenProjectFolder.Name = "button_OpenProjectFolder";
			this.button_OpenProjectFolder.Size = new System.Drawing.Size(26, 26);
			this.button_OpenProjectFolder.TabIndex = 4;
			this.toolTip.SetToolTip(this.button_OpenProjectFolder, "Open in Explorer");
			this.button_OpenProjectFolder.Click += new System.EventHandler(this.button_OpenProjectFolder_Click);
			// 
			// button_OpenScriptFolder
			// 
			this.button_OpenScriptFolder.Checked = false;
			this.button_OpenScriptFolder.Image = global::TombIDE.ProjectMaster.Properties.Resources.forward_arrow_16;
			this.button_OpenScriptFolder.Location = new System.Drawing.Point(605, 206);
			this.button_OpenScriptFolder.Margin = new System.Windows.Forms.Padding(3, 3, 6, 3);
			this.button_OpenScriptFolder.Name = "button_OpenScriptFolder";
			this.button_OpenScriptFolder.Size = new System.Drawing.Size(26, 26);
			this.button_OpenScriptFolder.TabIndex = 11;
			this.toolTip.SetToolTip(this.button_OpenScriptFolder, "Open in Explorer");
			this.button_OpenScriptFolder.Click += new System.EventHandler(this.button_OpenScriptFolder_Click);
			// 
			// checkBox_FullPaths
			// 
			this.checkBox_FullPaths.Location = new System.Drawing.Point(7, 292);
			this.checkBox_FullPaths.Margin = new System.Windows.Forms.Padding(6, 6, 3, 8);
			this.checkBox_FullPaths.Name = "checkBox_FullPaths";
			this.checkBox_FullPaths.Size = new System.Drawing.Size(250, 17);
			this.checkBox_FullPaths.TabIndex = 16;
			this.checkBox_FullPaths.Text = "Expand $(ProjectDirectory) keys into full paths";
			this.checkBox_FullPaths.CheckedChanged += new System.EventHandler(this.checkBox_FullPaths_CheckedChanged);
			// 
			// label_01
			// 
			this.label_01.AutoSize = true;
			this.label_01.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
			this.label_01.Location = new System.Drawing.Point(7, 37);
			this.label_01.Margin = new System.Windows.Forms.Padding(6, 12, 0, 0);
			this.label_01.Name = "label_01";
			this.label_01.Size = new System.Drawing.Size(72, 13);
			this.label_01.TabIndex = 0;
			this.label_01.Text = "Project name:";
			// 
			// label_02
			// 
			this.label_02.AutoSize = true;
			this.label_02.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
			this.label_02.Location = new System.Drawing.Point(7, 88);
			this.label_02.Margin = new System.Windows.Forms.Padding(6, 6, 0, 0);
			this.label_02.Name = "label_02";
			this.label_02.Size = new System.Drawing.Size(83, 13);
			this.label_02.TabIndex = 2;
			this.label_02.Text = "Project location:";
			// 
			// label_03
			// 
			this.label_03.AutoSize = true;
			this.label_03.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
			this.label_03.Location = new System.Drawing.Point(7, 139);
			this.label_03.Margin = new System.Windows.Forms.Padding(6, 6, 0, 0);
			this.label_03.Name = "label_03";
			this.label_03.Size = new System.Drawing.Size(83, 13);
			this.label_03.TabIndex = 5;
			this.label_03.Text = "Engine location:";
			// 
			// label_04
			// 
			this.label_04.AutoSize = true;
			this.label_04.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
			this.label_04.Location = new System.Drawing.Point(7, 190);
			this.label_04.Margin = new System.Windows.Forms.Padding(6, 6, 0, 0);
			this.label_04.Name = "label_04";
			this.label_04.Size = new System.Drawing.Size(66, 13);
			this.label_04.TabIndex = 8;
			this.label_04.Text = "Script folder:";
			// 
			// label_05
			// 
			this.label_05.AutoSize = true;
			this.label_05.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
			this.label_05.Location = new System.Drawing.Point(7, 241);
			this.label_05.Margin = new System.Windows.Forms.Padding(6, 6, 0, 0);
			this.label_05.Name = "label_05";
			this.label_05.Size = new System.Drawing.Size(70, 13);
			this.label_05.TabIndex = 12;
			this.label_05.Text = "Levels folder:";
			// 
			// sectionPanel
			// 
			this.sectionPanel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.sectionPanel.Controls.Add(this.button_OpenEngineFolder);
			this.sectionPanel.Controls.Add(this.textBox_EnginePath);
			this.sectionPanel.Controls.Add(this.label_03);
			this.sectionPanel.Controls.Add(this.button_ChangeLevelsPath);
			this.sectionPanel.Controls.Add(this.textBox_ProjectName);
			this.sectionPanel.Controls.Add(this.label_05);
			this.sectionPanel.Controls.Add(this.label_01);
			this.sectionPanel.Controls.Add(this.textBox_LevelsPath);
			this.sectionPanel.Controls.Add(this.button_OpenProjectFolder);
			this.sectionPanel.Controls.Add(this.button_OpenLevelsFolder);
			this.sectionPanel.Controls.Add(this.label_04);
			this.sectionPanel.Controls.Add(this.textBox_ProjectPath);
			this.sectionPanel.Controls.Add(this.checkBox_FullPaths);
			this.sectionPanel.Controls.Add(this.textBox_ScriptPath);
			this.sectionPanel.Controls.Add(this.button_OpenScriptFolder);
			this.sectionPanel.Controls.Add(this.button_ChangeScriptPath);
			this.sectionPanel.Controls.Add(this.label_02);
			this.sectionPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.sectionPanel.Location = new System.Drawing.Point(0, 0);
			this.sectionPanel.Name = "sectionPanel";
			this.sectionPanel.SectionHeader = "Project Information (Read Only)";
			this.sectionPanel.Size = new System.Drawing.Size(640, 320);
			this.sectionPanel.TabIndex = 0;
			// 
			// textBox_EnginePath
			// 
			this.textBox_EnginePath.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(48)))), ((int)(((byte)(48)))), ((int)(((byte)(48)))));
			this.textBox_EnginePath.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.textBox_EnginePath.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
			this.textBox_EnginePath.ForeColor = System.Drawing.Color.Gainsboro;
			this.textBox_EnginePath.Location = new System.Drawing.Point(7, 155);
			this.textBox_EnginePath.Margin = new System.Windows.Forms.Padding(6, 3, 3, 3);
			this.textBox_EnginePath.Name = "textBox_EnginePath";
			this.textBox_EnginePath.ReadOnly = true;
			this.textBox_EnginePath.Size = new System.Drawing.Size(592, 26);
			this.textBox_EnginePath.TabIndex = 6;
			// 
			// textBox_ProjectName
			// 
			this.textBox_ProjectName.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(48)))), ((int)(((byte)(48)))), ((int)(((byte)(48)))));
			this.textBox_ProjectName.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.textBox_ProjectName.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
			this.textBox_ProjectName.ForeColor = System.Drawing.Color.Gainsboro;
			this.textBox_ProjectName.Location = new System.Drawing.Point(7, 53);
			this.textBox_ProjectName.Margin = new System.Windows.Forms.Padding(6, 3, 6, 3);
			this.textBox_ProjectName.Name = "textBox_ProjectName";
			this.textBox_ProjectName.ReadOnly = true;
			this.textBox_ProjectName.Size = new System.Drawing.Size(624, 26);
			this.textBox_ProjectName.TabIndex = 1;
			// 
			// textBox_LevelsPath
			// 
			this.textBox_LevelsPath.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(48)))), ((int)(((byte)(48)))), ((int)(((byte)(48)))));
			this.textBox_LevelsPath.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.textBox_LevelsPath.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
			this.textBox_LevelsPath.ForeColor = System.Drawing.Color.Gainsboro;
			this.textBox_LevelsPath.Location = new System.Drawing.Point(7, 257);
			this.textBox_LevelsPath.Margin = new System.Windows.Forms.Padding(6, 3, 3, 3);
			this.textBox_LevelsPath.Name = "textBox_LevelsPath";
			this.textBox_LevelsPath.ReadOnly = true;
			this.textBox_LevelsPath.Size = new System.Drawing.Size(560, 26);
			this.textBox_LevelsPath.TabIndex = 13;
			// 
			// textBox_ProjectPath
			// 
			this.textBox_ProjectPath.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(48)))), ((int)(((byte)(48)))), ((int)(((byte)(48)))));
			this.textBox_ProjectPath.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.textBox_ProjectPath.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
			this.textBox_ProjectPath.ForeColor = System.Drawing.Color.Gainsboro;
			this.textBox_ProjectPath.Location = new System.Drawing.Point(7, 104);
			this.textBox_ProjectPath.Margin = new System.Windows.Forms.Padding(6, 3, 3, 3);
			this.textBox_ProjectPath.Name = "textBox_ProjectPath";
			this.textBox_ProjectPath.ReadOnly = true;
			this.textBox_ProjectPath.Size = new System.Drawing.Size(592, 26);
			this.textBox_ProjectPath.TabIndex = 3;
			// 
			// textBox_ScriptPath
			// 
			this.textBox_ScriptPath.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(48)))), ((int)(((byte)(48)))), ((int)(((byte)(48)))));
			this.textBox_ScriptPath.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.textBox_ScriptPath.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
			this.textBox_ScriptPath.ForeColor = System.Drawing.Color.Gainsboro;
			this.textBox_ScriptPath.Location = new System.Drawing.Point(7, 206);
			this.textBox_ScriptPath.Margin = new System.Windows.Forms.Padding(6, 3, 3, 3);
			this.textBox_ScriptPath.Name = "textBox_ScriptPath";
			this.textBox_ScriptPath.ReadOnly = true;
			this.textBox_ScriptPath.Size = new System.Drawing.Size(560, 26);
			this.textBox_ScriptPath.TabIndex = 9;
			// 
			// SettingsProjectInfo
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
			this.Controls.Add(this.sectionPanel);
			this.MaximumSize = new System.Drawing.Size(640, 320);
			this.MinimumSize = new System.Drawing.Size(640, 320);
			this.Name = "SettingsProjectInfo";
			this.Size = new System.Drawing.Size(640, 320);
			this.sectionPanel.ResumeLayout(false);
			this.sectionPanel.PerformLayout();
			this.ResumeLayout(false);

		}

		#endregion

		private DarkUI.Controls.DarkButton button_ChangeLevelsPath;
		private DarkUI.Controls.DarkButton button_ChangeScriptPath;
		private DarkUI.Controls.DarkButton button_OpenEngineFolder;
		private DarkUI.Controls.DarkButton button_OpenLevelsFolder;
		private DarkUI.Controls.DarkButton button_OpenProjectFolder;
		private DarkUI.Controls.DarkButton button_OpenScriptFolder;
		private DarkUI.Controls.DarkCheckBox checkBox_FullPaths;
		private DarkUI.Controls.DarkLabel label_01;
		private DarkUI.Controls.DarkLabel label_02;
		private DarkUI.Controls.DarkLabel label_03;
		private DarkUI.Controls.DarkLabel label_04;
		private DarkUI.Controls.DarkLabel label_05;
		private DarkUI.Controls.DarkSectionPanel sectionPanel;
		private System.Windows.Forms.TextBox textBox_EnginePath;
		private System.Windows.Forms.TextBox textBox_LevelsPath;
		private System.Windows.Forms.TextBox textBox_ProjectName;
		private System.Windows.Forms.TextBox textBox_ProjectPath;
		private System.Windows.Forms.TextBox textBox_ScriptPath;
		private System.Windows.Forms.ToolTip toolTip;
	}
}
