namespace TombIDE
{
	partial class FormImportLevel
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
			this.button_DeselectAll = new DarkUI.Controls.DarkButton();
			this.button_Import = new DarkUI.Controls.DarkButton();
			this.button_OpenAudioFolder = new DarkUI.Controls.DarkButton();
			this.button_SelectAll = new DarkUI.Controls.DarkButton();
			this.checkBox_EnableHorizon = new DarkUI.Controls.DarkCheckBox();
			this.checkBox_GenerateSection = new DarkUI.Controls.DarkCheckBox();
			this.label_01 = new DarkUI.Controls.DarkLabel();
			this.label_02 = new DarkUI.Controls.DarkLabel();
			this.label_03 = new DarkUI.Controls.DarkLabel();
			this.panel_01 = new System.Windows.Forms.Panel();
			this.textBox_Prj2Path = new DarkUI.Controls.DarkTextBox();
			this.panel_02 = new System.Windows.Forms.Panel();
			this.treeView = new DarkUI.Controls.DarkTreeView();
			this.radioButton_FolderKeep = new DarkUI.Controls.DarkRadioButton();
			this.radioButton_SelectedCopy = new DarkUI.Controls.DarkRadioButton();
			this.radioButton_SpecifiedCopy = new DarkUI.Controls.DarkRadioButton();
			this.textBox_LevelName = new DarkUI.Controls.DarkTextBox();
			this.panel_04 = new System.Windows.Forms.Panel();
			this.panel_ScriptSettings = new System.Windows.Forms.Panel();
			this.textBox_SoundID = new DarkUI.Controls.DarkTextBox();
			this.progressBar = new DarkUI.Controls.DarkProgressBar();
			this.panel_01.SuspendLayout();
			this.panel_02.SuspendLayout();
			this.panel_04.SuspendLayout();
			this.panel_ScriptSettings.SuspendLayout();
			this.SuspendLayout();
			// 
			// button_DeselectAll
			// 
			this.button_DeselectAll.Enabled = false;
			this.button_DeselectAll.Location = new System.Drawing.Point(225, 118);
			this.button_DeselectAll.Margin = new System.Windows.Forms.Padding(3, 3, 6, 3);
			this.button_DeselectAll.Name = "button_DeselectAll";
			this.button_DeselectAll.Size = new System.Drawing.Size(213, 23);
			this.button_DeselectAll.TabIndex = 5;
			this.button_DeselectAll.Text = "Deselect all";
			this.button_DeselectAll.Click += new System.EventHandler(this.button_DeselectAll_Click);
			// 
			// button_Import
			// 
			this.button_Import.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.button_Import.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.button_Import.Location = new System.Drawing.Point(9, 520);
			this.button_Import.Margin = new System.Windows.Forms.Padding(0, 3, 0, 3);
			this.button_Import.Name = "button_Import";
			this.button_Import.Size = new System.Drawing.Size(446, 23);
			this.button_Import.TabIndex = 4;
			this.button_Import.Text = "Import Level";
			this.button_Import.Click += new System.EventHandler(this.button_Import_Click);
			// 
			// button_OpenAudioFolder
			// 
			this.button_OpenAudioFolder.Image = global::TombIDE.ProjectMaster.Properties.Resources.forward_arrow_16;
			this.button_OpenAudioFolder.Location = new System.Drawing.Point(330, 4);
			this.button_OpenAudioFolder.Margin = new System.Windows.Forms.Padding(3, 4, 4, 3);
			this.button_OpenAudioFolder.Name = "button_OpenAudioFolder";
			this.button_OpenAudioFolder.Size = new System.Drawing.Size(96, 24);
			this.button_OpenAudioFolder.TabIndex = 2;
			this.button_OpenAudioFolder.Text = "Open /audio/";
			this.button_OpenAudioFolder.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
			this.button_OpenAudioFolder.Click += new System.EventHandler(this.button_OpenAudioFolder_Click);
			// 
			// button_SelectAll
			// 
			this.button_SelectAll.Enabled = false;
			this.button_SelectAll.Location = new System.Drawing.Point(6, 118);
			this.button_SelectAll.Margin = new System.Windows.Forms.Padding(6, 3, 3, 3);
			this.button_SelectAll.Name = "button_SelectAll";
			this.button_SelectAll.Size = new System.Drawing.Size(213, 23);
			this.button_SelectAll.TabIndex = 4;
			this.button_SelectAll.Text = "Select all";
			this.button_SelectAll.Click += new System.EventHandler(this.button_SelectAll_Click);
			// 
			// checkBox_EnableHorizon
			// 
			this.checkBox_EnableHorizon.Checked = true;
			this.checkBox_EnableHorizon.CheckState = System.Windows.Forms.CheckState.Checked;
			this.checkBox_EnableHorizon.Location = new System.Drawing.Point(6, 35);
			this.checkBox_EnableHorizon.Margin = new System.Windows.Forms.Padding(6);
			this.checkBox_EnableHorizon.Name = "checkBox_EnableHorizon";
			this.checkBox_EnableHorizon.Size = new System.Drawing.Size(418, 25);
			this.checkBox_EnableHorizon.TabIndex = 3;
			this.checkBox_EnableHorizon.Text = "Enable HORIZON (Make the skybox visible in the level)";
			// 
			// checkBox_GenerateSection
			// 
			this.checkBox_GenerateSection.Checked = true;
			this.checkBox_GenerateSection.CheckState = System.Windows.Forms.CheckState.Checked;
			this.checkBox_GenerateSection.Location = new System.Drawing.Point(6, 6);
			this.checkBox_GenerateSection.Margin = new System.Windows.Forms.Padding(6, 6, 6, 3);
			this.checkBox_GenerateSection.Name = "checkBox_GenerateSection";
			this.checkBox_GenerateSection.Size = new System.Drawing.Size(432, 20);
			this.checkBox_GenerateSection.TabIndex = 0;
			this.checkBox_GenerateSection.Text = "Generate a new [Level] section for this level in the project\'s Script.txt file";
			this.checkBox_GenerateSection.CheckedChanged += new System.EventHandler(this.checkBox_GenerateSection_CheckedChanged);
			// 
			// label_01
			// 
			this.label_01.AutoSize = true;
			this.label_01.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
			this.label_01.Location = new System.Drawing.Point(6, 6);
			this.label_01.Margin = new System.Windows.Forms.Padding(6, 6, 3, 0);
			this.label_01.Name = "label_01";
			this.label_01.Size = new System.Drawing.Size(136, 13);
			this.label_01.TabIndex = 0;
			this.label_01.Text = "Specified source .prj2 path:";
			// 
			// label_02
			// 
			this.label_02.AutoSize = true;
			this.label_02.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
			this.label_02.Location = new System.Drawing.Point(6, 6);
			this.label_02.Margin = new System.Windows.Forms.Padding(6, 6, 3, 0);
			this.label_02.Name = "label_02";
			this.label_02.Size = new System.Drawing.Size(65, 13);
			this.label_02.TabIndex = 0;
			this.label_02.Text = "Level name:";
			// 
			// label_03
			// 
			this.label_03.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
			this.label_03.Location = new System.Drawing.Point(6, 6);
			this.label_03.Margin = new System.Windows.Forms.Padding(6, 6, 3, 3);
			this.label_03.Name = "label_03";
			this.label_03.Size = new System.Drawing.Size(94, 20);
			this.label_03.TabIndex = 0;
			this.label_03.Text = "Ambient sound ID:";
			this.label_03.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// panel_01
			// 
			this.panel_01.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.panel_01.Controls.Add(this.label_01);
			this.panel_01.Controls.Add(this.textBox_Prj2Path);
			this.panel_01.Location = new System.Drawing.Point(9, 12);
			this.panel_01.Margin = new System.Windows.Forms.Padding(0, 3, 0, 3);
			this.panel_01.Name = "panel_01";
			this.panel_01.Size = new System.Drawing.Size(446, 50);
			this.panel_01.TabIndex = 0;
			// 
			// textBox_Prj2Path
			// 
			this.textBox_Prj2Path.Location = new System.Drawing.Point(6, 22);
			this.textBox_Prj2Path.Margin = new System.Windows.Forms.Padding(6, 3, 6, 6);
			this.textBox_Prj2Path.Name = "textBox_Prj2Path";
			this.textBox_Prj2Path.ReadOnly = true;
			this.textBox_Prj2Path.Size = new System.Drawing.Size(432, 20);
			this.textBox_Prj2Path.TabIndex = 1;
			// 
			// panel_02
			// 
			this.panel_02.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.panel_02.Controls.Add(this.treeView);
			this.panel_02.Controls.Add(this.button_DeselectAll);
			this.panel_02.Controls.Add(this.button_SelectAll);
			this.panel_02.Controls.Add(this.radioButton_FolderKeep);
			this.panel_02.Controls.Add(this.radioButton_SelectedCopy);
			this.panel_02.Controls.Add(this.radioButton_SpecifiedCopy);
			this.panel_02.Controls.Add(this.label_02);
			this.panel_02.Controls.Add(this.textBox_LevelName);
			this.panel_02.Location = new System.Drawing.Point(9, 68);
			this.panel_02.Margin = new System.Windows.Forms.Padding(0, 3, 0, 3);
			this.panel_02.Name = "panel_02";
			this.panel_02.Size = new System.Drawing.Size(446, 332);
			this.panel_02.TabIndex = 1;
			// 
			// treeView
			// 
			this.treeView.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(48)))), ((int)(((byte)(48)))), ((int)(((byte)(48)))));
			this.treeView.Enabled = false;
			this.treeView.Location = new System.Drawing.Point(6, 147);
			this.treeView.Margin = new System.Windows.Forms.Padding(6, 3, 6, 3);
			this.treeView.MaxDragChange = 20;
			this.treeView.MultiSelect = true;
			this.treeView.Name = "treeView";
			this.treeView.Size = new System.Drawing.Size(432, 148);
			this.treeView.TabIndex = 6;
			// 
			// radioButton_FolderKeep
			// 
			this.radioButton_FolderKeep.Location = new System.Drawing.Point(6, 301);
			this.radioButton_FolderKeep.Margin = new System.Windows.Forms.Padding(6, 3, 6, 9);
			this.radioButton_FolderKeep.Name = "radioButton_FolderKeep";
			this.radioButton_FolderKeep.Size = new System.Drawing.Size(432, 20);
			this.radioButton_FolderKeep.TabIndex = 7;
			this.radioButton_FolderKeep.Text = "Import the whole specified folder but keep it\'s original location (Not recommende" +
    "d)";
			this.radioButton_FolderKeep.CheckedChanged += new System.EventHandler(this.radioButton_SpecificKeep_CheckedChanged);
			// 
			// radioButton_SelectedCopy
			// 
			this.radioButton_SelectedCopy.Location = new System.Drawing.Point(6, 86);
			this.radioButton_SelectedCopy.Margin = new System.Windows.Forms.Padding(6, 3, 6, 3);
			this.radioButton_SelectedCopy.Name = "radioButton_SelectedCopy";
			this.radioButton_SelectedCopy.Size = new System.Drawing.Size(432, 26);
			this.radioButton_SelectedCopy.TabIndex = 3;
			this.radioButton_SelectedCopy.Text = "Import selected .prj2 files from the specified folder and copy them into the proj" +
    "ect\'s /Levels/ folder";
			this.radioButton_SelectedCopy.CheckedChanged += new System.EventHandler(this.radioButton_SelectedCopy_CheckedChanged);
			// 
			// radioButton_SpecifiedCopy
			// 
			this.radioButton_SpecifiedCopy.Checked = true;
			this.radioButton_SpecifiedCopy.Location = new System.Drawing.Point(6, 60);
			this.radioButton_SpecifiedCopy.Margin = new System.Windows.Forms.Padding(6, 9, 6, 3);
			this.radioButton_SpecifiedCopy.Name = "radioButton_SpecifiedCopy";
			this.radioButton_SpecifiedCopy.Size = new System.Drawing.Size(432, 20);
			this.radioButton_SpecifiedCopy.TabIndex = 2;
			this.radioButton_SpecifiedCopy.TabStop = true;
			this.radioButton_SpecifiedCopy.Text = "Only import the specified .prj2 file and copy it into the project\'s /Levels/ fold" +
    "er";
			this.radioButton_SpecifiedCopy.CheckedChanged += new System.EventHandler(this.radioButton_SpecifiedCopy_CheckedChanged);
			// 
			// textBox_LevelName
			// 
			this.textBox_LevelName.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
			this.textBox_LevelName.Location = new System.Drawing.Point(6, 22);
			this.textBox_LevelName.Margin = new System.Windows.Forms.Padding(6, 3, 6, 3);
			this.textBox_LevelName.Name = "textBox_LevelName";
			this.textBox_LevelName.Size = new System.Drawing.Size(432, 26);
			this.textBox_LevelName.TabIndex = 1;
			// 
			// panel_04
			// 
			this.panel_04.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.panel_04.Controls.Add(this.panel_ScriptSettings);
			this.panel_04.Controls.Add(this.checkBox_GenerateSection);
			this.panel_04.Location = new System.Drawing.Point(9, 406);
			this.panel_04.Margin = new System.Windows.Forms.Padding(0, 3, 0, 3);
			this.panel_04.Name = "panel_04";
			this.panel_04.Size = new System.Drawing.Size(446, 108);
			this.panel_04.TabIndex = 3;
			// 
			// panel_ScriptSettings
			// 
			this.panel_ScriptSettings.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.panel_ScriptSettings.Controls.Add(this.button_OpenAudioFolder);
			this.panel_ScriptSettings.Controls.Add(this.checkBox_EnableHorizon);
			this.panel_ScriptSettings.Controls.Add(this.textBox_SoundID);
			this.panel_ScriptSettings.Controls.Add(this.label_03);
			this.panel_ScriptSettings.Location = new System.Drawing.Point(6, 32);
			this.panel_ScriptSettings.Margin = new System.Windows.Forms.Padding(6, 3, 6, 6);
			this.panel_ScriptSettings.Name = "panel_ScriptSettings";
			this.panel_ScriptSettings.Size = new System.Drawing.Size(432, 68);
			this.panel_ScriptSettings.TabIndex = 1;
			// 
			// textBox_SoundID
			// 
			this.textBox_SoundID.Location = new System.Drawing.Point(104, 6);
			this.textBox_SoundID.Margin = new System.Windows.Forms.Padding(1, 6, 3, 3);
			this.textBox_SoundID.Name = "textBox_SoundID";
			this.textBox_SoundID.Size = new System.Drawing.Size(220, 20);
			this.textBox_SoundID.TabIndex = 1;
			this.textBox_SoundID.Text = "110";
			// 
			// progressBar
			// 
			this.progressBar.Location = new System.Drawing.Point(9, 520);
			this.progressBar.Margin = new System.Windows.Forms.Padding(0, 3, 0, 3);
			this.progressBar.Name = "progressBar";
			this.progressBar.Size = new System.Drawing.Size(446, 23);
			this.progressBar.TabIndex = 5;
			this.progressBar.Visible = false;
			// 
			// FormImportLevel
			// 
			this.AcceptButton = this.button_Import;
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(464, 555);
			this.Controls.Add(this.panel_04);
			this.Controls.Add(this.button_Import);
			this.Controls.Add(this.panel_01);
			this.Controls.Add(this.panel_02);
			this.Controls.Add(this.progressBar);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.MaximizeBox = false;
			this.Name = "FormImportLevel";
			this.ShowIcon = false;
			this.ShowInTaskbar = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "Import Level";
			this.panel_01.ResumeLayout(false);
			this.panel_01.PerformLayout();
			this.panel_02.ResumeLayout(false);
			this.panel_02.PerformLayout();
			this.panel_04.ResumeLayout(false);
			this.panel_ScriptSettings.ResumeLayout(false);
			this.panel_ScriptSettings.PerformLayout();
			this.ResumeLayout(false);

		}

		#endregion

		private DarkUI.Controls.DarkButton button_DeselectAll;
		private DarkUI.Controls.DarkButton button_Import;
		private DarkUI.Controls.DarkButton button_OpenAudioFolder;
		private DarkUI.Controls.DarkButton button_SelectAll;
		private DarkUI.Controls.DarkCheckBox checkBox_EnableHorizon;
		private DarkUI.Controls.DarkCheckBox checkBox_GenerateSection;
		private DarkUI.Controls.DarkLabel label_01;
		private DarkUI.Controls.DarkLabel label_02;
		private DarkUI.Controls.DarkLabel label_03;
		private DarkUI.Controls.DarkProgressBar progressBar;
		private DarkUI.Controls.DarkRadioButton radioButton_FolderKeep;
		private DarkUI.Controls.DarkRadioButton radioButton_SelectedCopy;
		private DarkUI.Controls.DarkRadioButton radioButton_SpecifiedCopy;
		private DarkUI.Controls.DarkTextBox textBox_LevelName;
		private DarkUI.Controls.DarkTextBox textBox_Prj2Path;
		private DarkUI.Controls.DarkTextBox textBox_SoundID;
		private DarkUI.Controls.DarkTreeView treeView;
		private System.Windows.Forms.Panel panel_01;
		private System.Windows.Forms.Panel panel_02;
		private System.Windows.Forms.Panel panel_04;
		private System.Windows.Forms.Panel panel_ScriptSettings;
	}
}