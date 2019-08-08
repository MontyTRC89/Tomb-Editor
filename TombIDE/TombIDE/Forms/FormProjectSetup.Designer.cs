namespace TombIDE
{
	partial class FormProjectSetup : DarkUI.Forms.DarkForm
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
			this.button_BrowseLevels = new DarkUI.Controls.DarkButton();
			this.button_BrowseProject = new DarkUI.Controls.DarkButton();
			this.button_BrowseScript = new DarkUI.Controls.DarkButton();
			this.button_Create = new DarkUI.Controls.DarkButton();
			this.comboBox_EngineType = new DarkUI.Controls.DarkComboBox();
			this.label_01 = new DarkUI.Controls.DarkLabel();
			this.label_02 = new DarkUI.Controls.DarkLabel();
			this.label_03 = new DarkUI.Controls.DarkLabel();
			this.panel_LevelsRadioChoice = new System.Windows.Forms.Panel();
			this.radio_Levels_01 = new DarkUI.Controls.DarkRadioButton();
			this.radio_Levels_02 = new DarkUI.Controls.DarkRadioButton();
			this.panel_ProjectSettings = new System.Windows.Forms.Panel();
			this.panel_ScriptRadioChoice = new System.Windows.Forms.Panel();
			this.radio_Script_01 = new DarkUI.Controls.DarkRadioButton();
			this.radio_Script_02 = new DarkUI.Controls.DarkRadioButton();
			this.textBox_LevelsPath = new DarkUI.Controls.DarkTextBox();
			this.textBox_ScriptPath = new DarkUI.Controls.DarkTextBox();
			this.textBox_ProjectPath = new DarkUI.Controls.DarkTextBox();
			this.textBox_ProjectName = new DarkUI.Controls.DarkTextBox();
			this.progressBar = new DarkUI.Controls.DarkProgressBar();
			this.panel_LevelsRadioChoice.SuspendLayout();
			this.panel_ProjectSettings.SuspendLayout();
			this.panel_ScriptRadioChoice.SuspendLayout();
			this.SuspendLayout();
			// 
			// button_BrowseLevels
			// 
			this.button_BrowseLevels.Enabled = false;
			this.button_BrowseLevels.Location = new System.Drawing.Point(363, 266);
			this.button_BrowseLevels.Margin = new System.Windows.Forms.Padding(0, 2, 6, 8);
			this.button_BrowseLevels.Name = "button_BrowseLevels";
			this.button_BrowseLevels.Size = new System.Drawing.Size(75, 22);
			this.button_BrowseLevels.TabIndex = 14;
			this.button_BrowseLevels.Text = "Browse...";
			this.button_BrowseLevels.Click += new System.EventHandler(this.button_BrowseLevels_Click);
			// 
			// button_BrowseProject
			// 
			this.button_BrowseProject.Location = new System.Drawing.Point(363, 102);
			this.button_BrowseProject.Margin = new System.Windows.Forms.Padding(0, 3, 6, 8);
			this.button_BrowseProject.Name = "button_BrowseProject";
			this.button_BrowseProject.Size = new System.Drawing.Size(75, 22);
			this.button_BrowseProject.TabIndex = 6;
			this.button_BrowseProject.Text = "Browse...";
			this.button_BrowseProject.Click += new System.EventHandler(this.button_BrowseProject_Click);
			// 
			// button_BrowseScript
			// 
			this.button_BrowseScript.Enabled = false;
			this.button_BrowseScript.Location = new System.Drawing.Point(363, 184);
			this.button_BrowseScript.Margin = new System.Windows.Forms.Padding(0, 2, 6, 8);
			this.button_BrowseScript.Name = "button_BrowseScript";
			this.button_BrowseScript.Size = new System.Drawing.Size(75, 22);
			this.button_BrowseScript.TabIndex = 10;
			this.button_BrowseScript.Text = "Browse...";
			this.button_BrowseScript.Click += new System.EventHandler(this.button_BrowseScript_Click);
			// 
			// button_Create
			// 
			this.button_Create.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.button_Create.Location = new System.Drawing.Point(327, 316);
			this.button_Create.Margin = new System.Windows.Forms.Padding(3, 3, 0, 3);
			this.button_Create.Name = "button_Create";
			this.button_Create.Size = new System.Drawing.Size(128, 23);
			this.button_Create.TabIndex = 1;
			this.button_Create.Text = "Create Project";
			this.button_Create.Click += new System.EventHandler(this.button_Create_Click);
			// 
			// comboBox_EngineType
			// 
			this.comboBox_EngineType.FormattingEnabled = true;
			this.comboBox_EngineType.Items.AddRange(new object[] {
            "TR4",
            "TRNG",
            "TR5 (Unavailable)",
            "TR5Main (Unavailable)"});
			this.comboBox_EngineType.Location = new System.Drawing.Point(72, 54);
			this.comboBox_EngineType.Margin = new System.Windows.Forms.Padding(3, 6, 6, 6);
			this.comboBox_EngineType.Name = "comboBox_EngineType";
			this.comboBox_EngineType.Size = new System.Drawing.Size(366, 21);
			this.comboBox_EngineType.TabIndex = 3;
			// 
			// label_01
			// 
			this.label_01.AutoSize = true;
			this.label_01.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
			this.label_01.Location = new System.Drawing.Point(3, 3);
			this.label_01.Margin = new System.Windows.Forms.Padding(3, 3, 3, 0);
			this.label_01.Name = "label_01";
			this.label_01.Size = new System.Drawing.Size(72, 13);
			this.label_01.TabIndex = 0;
			this.label_01.Text = "Project name:";
			// 
			// label_02
			// 
			this.label_02.AutoSize = true;
			this.label_02.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
			this.label_02.Location = new System.Drawing.Point(3, 57);
			this.label_02.Margin = new System.Windows.Forms.Padding(3, 9, 0, 0);
			this.label_02.Name = "label_02";
			this.label_02.Size = new System.Drawing.Size(66, 13);
			this.label_02.TabIndex = 2;
			this.label_02.Text = "Engine type:";
			// 
			// label_03
			// 
			this.label_03.AutoSize = true;
			this.label_03.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
			this.label_03.Location = new System.Drawing.Point(3, 87);
			this.label_03.Margin = new System.Windows.Forms.Padding(3, 6, 3, 0);
			this.label_03.Name = "label_03";
			this.label_03.Size = new System.Drawing.Size(83, 13);
			this.label_03.TabIndex = 4;
			this.label_03.Text = "Project location:";
			// 
			// panel_LevelsRadioChoice
			// 
			this.panel_LevelsRadioChoice.Controls.Add(this.radio_Levels_01);
			this.panel_LevelsRadioChoice.Controls.Add(this.radio_Levels_02);
			this.panel_LevelsRadioChoice.Location = new System.Drawing.Point(6, 214);
			this.panel_LevelsRadioChoice.Margin = new System.Windows.Forms.Padding(3, 0, 3, 0);
			this.panel_LevelsRadioChoice.Name = "panel_LevelsRadioChoice";
			this.panel_LevelsRadioChoice.Size = new System.Drawing.Size(432, 50);
			this.panel_LevelsRadioChoice.TabIndex = 15;
			// 
			// radio_Levels_01
			// 
			this.radio_Levels_01.Checked = true;
			this.radio_Levels_01.Location = new System.Drawing.Point(3, 3);
			this.radio_Levels_01.Margin = new System.Windows.Forms.Padding(3, 3, 3, 0);
			this.radio_Levels_01.Name = "radio_Levels_01";
			this.radio_Levels_01.Size = new System.Drawing.Size(348, 20);
			this.radio_Levels_01.TabIndex = 11;
			this.radio_Levels_01.TabStop = true;
			this.radio_Levels_01.Text = "Store newly created Levels inside the project folder (Default)";
			this.radio_Levels_01.CheckedChanged += new System.EventHandler(this.radio_Levels_01_CheckedChanged);
			// 
			// radio_Levels_02
			// 
			this.radio_Levels_02.Location = new System.Drawing.Point(3, 27);
			this.radio_Levels_02.Name = "radio_Levels_02";
			this.radio_Levels_02.Size = new System.Drawing.Size(348, 20);
			this.radio_Levels_02.TabIndex = 12;
			this.radio_Levels_02.Text = "Use a different \"Levels\" location";
			this.radio_Levels_02.CheckedChanged += new System.EventHandler(this.radio_Level_02_CheckedChanged);
			// 
			// panel_ProjectSettings
			// 
			this.panel_ProjectSettings.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.panel_ProjectSettings.Controls.Add(this.panel_ScriptRadioChoice);
			this.panel_ProjectSettings.Controls.Add(this.panel_LevelsRadioChoice);
			this.panel_ProjectSettings.Controls.Add(this.button_BrowseLevels);
			this.panel_ProjectSettings.Controls.Add(this.textBox_LevelsPath);
			this.panel_ProjectSettings.Controls.Add(this.button_BrowseScript);
			this.panel_ProjectSettings.Controls.Add(this.textBox_ScriptPath);
			this.panel_ProjectSettings.Controls.Add(this.button_BrowseProject);
			this.panel_ProjectSettings.Controls.Add(this.comboBox_EngineType);
			this.panel_ProjectSettings.Controls.Add(this.textBox_ProjectPath);
			this.panel_ProjectSettings.Controls.Add(this.label_03);
			this.panel_ProjectSettings.Controls.Add(this.label_01);
			this.panel_ProjectSettings.Controls.Add(this.textBox_ProjectName);
			this.panel_ProjectSettings.Controls.Add(this.label_02);
			this.panel_ProjectSettings.Location = new System.Drawing.Point(9, 12);
			this.panel_ProjectSettings.Margin = new System.Windows.Forms.Padding(0, 3, 0, 3);
			this.panel_ProjectSettings.Name = "panel_ProjectSettings";
			this.panel_ProjectSettings.Size = new System.Drawing.Size(446, 298);
			this.panel_ProjectSettings.TabIndex = 0;
			// 
			// panel_ScriptRadioChoice
			// 
			this.panel_ScriptRadioChoice.Controls.Add(this.radio_Script_01);
			this.panel_ScriptRadioChoice.Controls.Add(this.radio_Script_02);
			this.panel_ScriptRadioChoice.Location = new System.Drawing.Point(6, 132);
			this.panel_ScriptRadioChoice.Margin = new System.Windows.Forms.Padding(0);
			this.panel_ScriptRadioChoice.Name = "panel_ScriptRadioChoice";
			this.panel_ScriptRadioChoice.Size = new System.Drawing.Size(432, 50);
			this.panel_ScriptRadioChoice.TabIndex = 16;
			// 
			// radio_Script_01
			// 
			this.radio_Script_01.Checked = true;
			this.radio_Script_01.Location = new System.Drawing.Point(3, 3);
			this.radio_Script_01.Margin = new System.Windows.Forms.Padding(3, 3, 3, 0);
			this.radio_Script_01.Name = "radio_Script_01";
			this.radio_Script_01.Size = new System.Drawing.Size(348, 20);
			this.radio_Script_01.TabIndex = 7;
			this.radio_Script_01.TabStop = true;
			this.radio_Script_01.Text = "Create a \"Script\" folder inside the project folder (Default)";
			this.radio_Script_01.CheckedChanged += new System.EventHandler(this.radio_Script_01_CheckedChanged);
			// 
			// radio_Script_02
			// 
			this.radio_Script_02.Location = new System.Drawing.Point(3, 27);
			this.radio_Script_02.Name = "radio_Script_02";
			this.radio_Script_02.Size = new System.Drawing.Size(348, 20);
			this.radio_Script_02.TabIndex = 8;
			this.radio_Script_02.Text = "Use a different \"Script\" location";
			this.radio_Script_02.CheckedChanged += new System.EventHandler(this.radio_Script_02_CheckedChanged);
			// 
			// textBox_LevelsPath
			// 
			this.textBox_LevelsPath.Enabled = false;
			this.textBox_LevelsPath.Location = new System.Drawing.Point(6, 267);
			this.textBox_LevelsPath.Margin = new System.Windows.Forms.Padding(6, 3, 6, 9);
			this.textBox_LevelsPath.Name = "textBox_LevelsPath";
			this.textBox_LevelsPath.Size = new System.Drawing.Size(351, 20);
			this.textBox_LevelsPath.TabIndex = 13;
			// 
			// textBox_ScriptPath
			// 
			this.textBox_ScriptPath.Enabled = false;
			this.textBox_ScriptPath.Location = new System.Drawing.Point(6, 185);
			this.textBox_ScriptPath.Margin = new System.Windows.Forms.Padding(6, 3, 6, 9);
			this.textBox_ScriptPath.Name = "textBox_ScriptPath";
			this.textBox_ScriptPath.Size = new System.Drawing.Size(351, 20);
			this.textBox_ScriptPath.TabIndex = 9;
			// 
			// textBox_ProjectPath
			// 
			this.textBox_ProjectPath.Location = new System.Drawing.Point(6, 103);
			this.textBox_ProjectPath.Margin = new System.Windows.Forms.Padding(6, 3, 6, 9);
			this.textBox_ProjectPath.Name = "textBox_ProjectPath";
			this.textBox_ProjectPath.Size = new System.Drawing.Size(351, 20);
			this.textBox_ProjectPath.TabIndex = 5;
			// 
			// textBox_ProjectName
			// 
			this.textBox_ProjectName.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
			this.textBox_ProjectName.Location = new System.Drawing.Point(6, 19);
			this.textBox_ProjectName.Margin = new System.Windows.Forms.Padding(6, 3, 6, 3);
			this.textBox_ProjectName.Name = "textBox_ProjectName";
			this.textBox_ProjectName.Size = new System.Drawing.Size(432, 26);
			this.textBox_ProjectName.TabIndex = 1;
			// 
			// progressBar
			// 
			this.progressBar.Location = new System.Drawing.Point(9, 316);
			this.progressBar.Margin = new System.Windows.Forms.Padding(0, 3, 3, 3);
			this.progressBar.Name = "progressBar";
			this.progressBar.Size = new System.Drawing.Size(312, 23);
			this.progressBar.TabIndex = 2;
			// 
			// FormProjectSetup
			// 
			this.AcceptButton = this.button_Create;
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
			this.ClientSize = new System.Drawing.Size(464, 351);
			this.Controls.Add(this.progressBar);
			this.Controls.Add(this.button_Create);
			this.Controls.Add(this.panel_ProjectSettings);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.MaximizeBox = false;
			this.Name = "FormProjectSetup";
			this.ShowIcon = false;
			this.ShowInTaskbar = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "Create New Project";
			this.panel_LevelsRadioChoice.ResumeLayout(false);
			this.panel_ProjectSettings.ResumeLayout(false);
			this.panel_ProjectSettings.PerformLayout();
			this.panel_ScriptRadioChoice.ResumeLayout(false);
			this.ResumeLayout(false);

		}

		#endregion

		private DarkUI.Controls.DarkButton button_BrowseLevels;
		private DarkUI.Controls.DarkButton button_BrowseProject;
		private DarkUI.Controls.DarkButton button_BrowseScript;
		private DarkUI.Controls.DarkButton button_Create;
		private DarkUI.Controls.DarkComboBox comboBox_EngineType;
		private DarkUI.Controls.DarkLabel label_01;
		private DarkUI.Controls.DarkLabel label_02;
		private DarkUI.Controls.DarkLabel label_03;
		private DarkUI.Controls.DarkProgressBar progressBar;
		private DarkUI.Controls.DarkRadioButton radio_Levels_01;
		private DarkUI.Controls.DarkRadioButton radio_Levels_02;
		private DarkUI.Controls.DarkRadioButton radio_Script_01;
		private DarkUI.Controls.DarkRadioButton radio_Script_02;
		private DarkUI.Controls.DarkTextBox textBox_LevelsPath;
		private DarkUI.Controls.DarkTextBox textBox_ProjectName;
		private DarkUI.Controls.DarkTextBox textBox_ProjectPath;
		private DarkUI.Controls.DarkTextBox textBox_ScriptPath;
		private System.Windows.Forms.Panel panel_LevelsRadioChoice;
		private System.Windows.Forms.Panel panel_ProjectSettings;
		private System.Windows.Forms.Panel panel_ScriptRadioChoice;
	}
}