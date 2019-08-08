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
			this.button_ChangeLevelsPath = new DarkUI.Controls.DarkButton();
			this.button_ChangeScriptPath = new DarkUI.Controls.DarkButton();
			this.button_OpenLevelsFolder = new DarkUI.Controls.DarkButton();
			this.button_OpenProjectFolder = new DarkUI.Controls.DarkButton();
			this.button_OpenScriptFolder = new DarkUI.Controls.DarkButton();
			this.checkBox_FullPaths = new DarkUI.Controls.DarkCheckBox();
			this.label_01 = new DarkUI.Controls.DarkLabel();
			this.label_02 = new DarkUI.Controls.DarkLabel();
			this.label_03 = new DarkUI.Controls.DarkLabel();
			this.label_04 = new DarkUI.Controls.DarkLabel();
			this.sectionPanel = new DarkUI.Controls.DarkSectionPanel();
			this.textBox_ProjectName = new System.Windows.Forms.TextBox();
			this.textBox_LevelsPath = new System.Windows.Forms.TextBox();
			this.textBox_ProjectPath = new System.Windows.Forms.TextBox();
			this.textBox_ScriptPath = new System.Windows.Forms.TextBox();
			this.sectionPanel.SuspendLayout();
			this.SuspendLayout();
			// 
			// button_ChangeLevelsPath
			// 
			this.button_ChangeLevelsPath.Location = new System.Drawing.Point(555, 206);
			this.button_ChangeLevelsPath.Margin = new System.Windows.Forms.Padding(3, 3, 6, 3);
			this.button_ChangeLevelsPath.Name = "button_ChangeLevelsPath";
			this.button_ChangeLevelsPath.Size = new System.Drawing.Size(76, 26);
			this.button_ChangeLevelsPath.TabIndex = 12;
			this.button_ChangeLevelsPath.Text = "Change...";
			this.button_ChangeLevelsPath.Click += new System.EventHandler(this.button_ChangeLevelsPath_Click);
			// 
			// button_ChangeScriptPath
			// 
			this.button_ChangeScriptPath.Location = new System.Drawing.Point(555, 155);
			this.button_ChangeScriptPath.Margin = new System.Windows.Forms.Padding(3, 3, 6, 3);
			this.button_ChangeScriptPath.Name = "button_ChangeScriptPath";
			this.button_ChangeScriptPath.Size = new System.Drawing.Size(76, 26);
			this.button_ChangeScriptPath.TabIndex = 8;
			this.button_ChangeScriptPath.Text = "Change...";
			this.button_ChangeScriptPath.Click += new System.EventHandler(this.button_ChangeScriptPath_Click);
			// 
			// button_OpenLevelsFolder
			// 
			this.button_OpenLevelsFolder.Image = global::TombIDE.ProjectMaster.Properties.Resources.forward_arrow_16;
			this.button_OpenLevelsFolder.Location = new System.Drawing.Point(523, 206);
			this.button_OpenLevelsFolder.Name = "button_OpenLevelsFolder";
			this.button_OpenLevelsFolder.Size = new System.Drawing.Size(26, 26);
			this.button_OpenLevelsFolder.TabIndex = 11;
			this.button_OpenLevelsFolder.Click += new System.EventHandler(this.button_OpenLevelsFolder_Click);
			// 
			// button_OpenProjectFolder
			// 
			this.button_OpenProjectFolder.Image = global::TombIDE.ProjectMaster.Properties.Resources.forward_arrow_16;
			this.button_OpenProjectFolder.Location = new System.Drawing.Point(523, 104);
			this.button_OpenProjectFolder.Margin = new System.Windows.Forms.Padding(3, 3, 6, 3);
			this.button_OpenProjectFolder.Name = "button_OpenProjectFolder";
			this.button_OpenProjectFolder.Size = new System.Drawing.Size(108, 26);
			this.button_OpenProjectFolder.TabIndex = 4;
			this.button_OpenProjectFolder.Text = "Open Explorer";
			this.button_OpenProjectFolder.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
			this.button_OpenProjectFolder.Click += new System.EventHandler(this.button_OpenProjectFolder_Click);
			// 
			// button_OpenScriptFolder
			// 
			this.button_OpenScriptFolder.Image = global::TombIDE.ProjectMaster.Properties.Resources.forward_arrow_16;
			this.button_OpenScriptFolder.Location = new System.Drawing.Point(523, 155);
			this.button_OpenScriptFolder.Name = "button_OpenScriptFolder";
			this.button_OpenScriptFolder.Size = new System.Drawing.Size(26, 26);
			this.button_OpenScriptFolder.TabIndex = 7;
			this.button_OpenScriptFolder.Click += new System.EventHandler(this.button_OpenScriptFolder_Click);
			// 
			// checkBox_FullPaths
			// 
			this.checkBox_FullPaths.Location = new System.Drawing.Point(7, 241);
			this.checkBox_FullPaths.Margin = new System.Windows.Forms.Padding(6, 6, 3, 6);
			this.checkBox_FullPaths.Name = "checkBox_FullPaths";
			this.checkBox_FullPaths.Size = new System.Drawing.Size(330, 18);
			this.checkBox_FullPaths.TabIndex = 13;
			this.checkBox_FullPaths.Text = "View full paths of folders which are inside the Project location";
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
			this.label_03.Size = new System.Drawing.Size(66, 13);
			this.label_03.TabIndex = 5;
			this.label_03.Text = "Script folder:";
			// 
			// label_04
			// 
			this.label_04.AutoSize = true;
			this.label_04.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
			this.label_04.Location = new System.Drawing.Point(7, 190);
			this.label_04.Margin = new System.Windows.Forms.Padding(6, 6, 0, 0);
			this.label_04.Name = "label_04";
			this.label_04.Size = new System.Drawing.Size(70, 13);
			this.label_04.TabIndex = 9;
			this.label_04.Text = "Levels folder:";
			// 
			// sectionPanel
			// 
			this.sectionPanel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.sectionPanel.Controls.Add(this.button_ChangeLevelsPath);
			this.sectionPanel.Controls.Add(this.textBox_ProjectName);
			this.sectionPanel.Controls.Add(this.label_04);
			this.sectionPanel.Controls.Add(this.label_01);
			this.sectionPanel.Controls.Add(this.textBox_LevelsPath);
			this.sectionPanel.Controls.Add(this.button_OpenProjectFolder);
			this.sectionPanel.Controls.Add(this.button_OpenLevelsFolder);
			this.sectionPanel.Controls.Add(this.label_03);
			this.sectionPanel.Controls.Add(this.textBox_ProjectPath);
			this.sectionPanel.Controls.Add(this.checkBox_FullPaths);
			this.sectionPanel.Controls.Add(this.textBox_ScriptPath);
			this.sectionPanel.Controls.Add(this.button_OpenScriptFolder);
			this.sectionPanel.Controls.Add(this.button_ChangeScriptPath);
			this.sectionPanel.Controls.Add(this.label_02);
			this.sectionPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.sectionPanel.Location = new System.Drawing.Point(0, 0);
			this.sectionPanel.Name = "sectionPanel";
			this.sectionPanel.SectionHeader = "Project Information";
			this.sectionPanel.Size = new System.Drawing.Size(640, 268);
			this.sectionPanel.TabIndex = 0;
			// 
			// textBox_ProjectName
			// 
			this.textBox_ProjectName.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(48)))), ((int)(((byte)(48)))), ((int)(((byte)(48)))));
			this.textBox_ProjectName.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.textBox_ProjectName.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
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
			this.textBox_LevelsPath.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
			this.textBox_LevelsPath.ForeColor = System.Drawing.Color.Gainsboro;
			this.textBox_LevelsPath.Location = new System.Drawing.Point(7, 206);
			this.textBox_LevelsPath.Margin = new System.Windows.Forms.Padding(6, 3, 3, 3);
			this.textBox_LevelsPath.Name = "textBox_LevelsPath";
			this.textBox_LevelsPath.ReadOnly = true;
			this.textBox_LevelsPath.Size = new System.Drawing.Size(510, 26);
			this.textBox_LevelsPath.TabIndex = 10;
			// 
			// textBox_ProjectPath
			// 
			this.textBox_ProjectPath.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(48)))), ((int)(((byte)(48)))), ((int)(((byte)(48)))));
			this.textBox_ProjectPath.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.textBox_ProjectPath.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
			this.textBox_ProjectPath.ForeColor = System.Drawing.Color.Gainsboro;
			this.textBox_ProjectPath.Location = new System.Drawing.Point(7, 104);
			this.textBox_ProjectPath.Margin = new System.Windows.Forms.Padding(6, 3, 3, 3);
			this.textBox_ProjectPath.Name = "textBox_ProjectPath";
			this.textBox_ProjectPath.ReadOnly = true;
			this.textBox_ProjectPath.Size = new System.Drawing.Size(510, 26);
			this.textBox_ProjectPath.TabIndex = 3;
			// 
			// textBox_ScriptPath
			// 
			this.textBox_ScriptPath.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(48)))), ((int)(((byte)(48)))), ((int)(((byte)(48)))));
			this.textBox_ScriptPath.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.textBox_ScriptPath.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
			this.textBox_ScriptPath.ForeColor = System.Drawing.Color.Gainsboro;
			this.textBox_ScriptPath.Location = new System.Drawing.Point(7, 155);
			this.textBox_ScriptPath.Margin = new System.Windows.Forms.Padding(6, 3, 3, 3);
			this.textBox_ScriptPath.Name = "textBox_ScriptPath";
			this.textBox_ScriptPath.ReadOnly = true;
			this.textBox_ScriptPath.Size = new System.Drawing.Size(510, 26);
			this.textBox_ScriptPath.TabIndex = 6;
			// 
			// SettingsProjectInfo
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
			this.Controls.Add(this.sectionPanel);
			this.MaximumSize = new System.Drawing.Size(640, 268);
			this.MinimumSize = new System.Drawing.Size(640, 268);
			this.Name = "SettingsProjectInfo";
			this.Size = new System.Drawing.Size(640, 268);
			this.sectionPanel.ResumeLayout(false);
			this.sectionPanel.PerformLayout();
			this.ResumeLayout(false);

		}

		#endregion

		private DarkUI.Controls.DarkButton button_ChangeLevelsPath;
		private DarkUI.Controls.DarkButton button_ChangeScriptPath;
		private DarkUI.Controls.DarkButton button_OpenLevelsFolder;
		private DarkUI.Controls.DarkButton button_OpenProjectFolder;
		private DarkUI.Controls.DarkButton button_OpenScriptFolder;
		private DarkUI.Controls.DarkCheckBox checkBox_FullPaths;
		private DarkUI.Controls.DarkLabel label_01;
		private DarkUI.Controls.DarkLabel label_02;
		private DarkUI.Controls.DarkLabel label_03;
		private DarkUI.Controls.DarkLabel label_04;
		private DarkUI.Controls.DarkSectionPanel sectionPanel;
		private System.Windows.Forms.TextBox textBox_LevelsPath;
		private System.Windows.Forms.TextBox textBox_ProjectName;
		private System.Windows.Forms.TextBox textBox_ProjectPath;
		private System.Windows.Forms.TextBox textBox_ScriptPath;
	}
}
