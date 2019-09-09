namespace TombIDE.ProjectMaster
{
	partial class FormLevelSetup
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
			this.button_Create = new DarkUI.Controls.DarkButton();
			this.button_OpenAudioFolder = new DarkUI.Controls.DarkButton();
			this.checkBox_CustomFileName = new DarkUI.Controls.DarkCheckBox();
			this.checkBox_EnableHorizon = new DarkUI.Controls.DarkCheckBox();
			this.checkBox_GenerateSection = new DarkUI.Controls.DarkCheckBox();
			this.label_01 = new DarkUI.Controls.DarkLabel();
			this.label_02 = new DarkUI.Controls.DarkLabel();
			this.numeric_SoundID = new DarkUI.Controls.DarkNumericUpDown();
			this.panel_01 = new System.Windows.Forms.Panel();
			this.panel_ScriptSettings = new System.Windows.Forms.Panel();
			this.textBox_CustomFileName = new DarkUI.Controls.DarkTextBox();
			this.textBox_LevelName = new DarkUI.Controls.DarkTextBox();
			((System.ComponentModel.ISupportInitialize)(this.numeric_SoundID)).BeginInit();
			this.panel_01.SuspendLayout();
			this.panel_ScriptSettings.SuspendLayout();
			this.SuspendLayout();
			// 
			// button_Create
			// 
			this.button_Create.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.button_Create.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.button_Create.Location = new System.Drawing.Point(9, 203);
			this.button_Create.Margin = new System.Windows.Forms.Padding(0, 3, 0, 3);
			this.button_Create.Name = "button_Create";
			this.button_Create.Size = new System.Drawing.Size(446, 23);
			this.button_Create.TabIndex = 5;
			this.button_Create.Text = "Create New Level";
			this.button_Create.Click += new System.EventHandler(this.button_Create_Click);
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
			// checkBox_CustomFileName
			// 
			this.checkBox_CustomFileName.Location = new System.Drawing.Point(9, 60);
			this.checkBox_CustomFileName.Margin = new System.Windows.Forms.Padding(0, 6, 3, 6);
			this.checkBox_CustomFileName.Name = "checkBox_CustomFileName";
			this.checkBox_CustomFileName.Size = new System.Drawing.Size(150, 20);
			this.checkBox_CustomFileName.TabIndex = 2;
			this.checkBox_CustomFileName.Text = "Custom PRJ2 / DAT name";
			this.checkBox_CustomFileName.CheckedChanged += new System.EventHandler(this.checkBox_CustomFileName_CheckedChanged);
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
			this.label_01.Location = new System.Drawing.Point(9, 9);
			this.label_01.Margin = new System.Windows.Forms.Padding(0, 0, 3, 0);
			this.label_01.Name = "label_01";
			this.label_01.Size = new System.Drawing.Size(65, 13);
			this.label_01.TabIndex = 0;
			this.label_01.Text = "Level name:";
			// 
			// label_02
			// 
			this.label_02.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
			this.label_02.Location = new System.Drawing.Point(6, 6);
			this.label_02.Margin = new System.Windows.Forms.Padding(6, 6, 3, 3);
			this.label_02.Name = "label_02";
			this.label_02.Size = new System.Drawing.Size(94, 20);
			this.label_02.TabIndex = 0;
			this.label_02.Text = "Ambient sound ID:";
			this.label_02.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// numeric_SoundID
			// 
			this.numeric_SoundID.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(69)))), ((int)(((byte)(73)))), ((int)(((byte)(74)))));
			this.numeric_SoundID.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
			this.numeric_SoundID.IncrementAlternate = new decimal(new int[] {
            10,
            0,
            0,
            65536});
			this.numeric_SoundID.Location = new System.Drawing.Point(104, 6);
			this.numeric_SoundID.Margin = new System.Windows.Forms.Padding(1, 6, 3, 3);
			this.numeric_SoundID.Maximum = new decimal(new int[] {
            999,
            0,
            0,
            0});
			this.numeric_SoundID.MousewheelSingleIncrement = true;
			this.numeric_SoundID.Name = "numeric_SoundID";
			this.numeric_SoundID.Size = new System.Drawing.Size(220, 20);
			this.numeric_SoundID.TabIndex = 1;
			this.numeric_SoundID.Value = new decimal(new int[] {
            110,
            0,
            0,
            0});
			// 
			// panel_01
			// 
			this.panel_01.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.panel_01.Controls.Add(this.panel_ScriptSettings);
			this.panel_01.Controls.Add(this.checkBox_GenerateSection);
			this.panel_01.Location = new System.Drawing.Point(9, 89);
			this.panel_01.Margin = new System.Windows.Forms.Padding(0, 3, 0, 3);
			this.panel_01.Name = "panel_01";
			this.panel_01.Size = new System.Drawing.Size(446, 108);
			this.panel_01.TabIndex = 4;
			// 
			// panel_ScriptSettings
			// 
			this.panel_ScriptSettings.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.panel_ScriptSettings.Controls.Add(this.numeric_SoundID);
			this.panel_ScriptSettings.Controls.Add(this.button_OpenAudioFolder);
			this.panel_ScriptSettings.Controls.Add(this.checkBox_EnableHorizon);
			this.panel_ScriptSettings.Controls.Add(this.label_02);
			this.panel_ScriptSettings.Location = new System.Drawing.Point(6, 32);
			this.panel_ScriptSettings.Margin = new System.Windows.Forms.Padding(6, 3, 6, 6);
			this.panel_ScriptSettings.Name = "panel_ScriptSettings";
			this.panel_ScriptSettings.Size = new System.Drawing.Size(432, 68);
			this.panel_ScriptSettings.TabIndex = 1;
			// 
			// textBox_CustomFileName
			// 
			this.textBox_CustomFileName.Enabled = false;
			this.textBox_CustomFileName.Location = new System.Drawing.Point(165, 60);
			this.textBox_CustomFileName.Margin = new System.Windows.Forms.Padding(3, 6, 0, 6);
			this.textBox_CustomFileName.Name = "textBox_CustomFileName";
			this.textBox_CustomFileName.Size = new System.Drawing.Size(290, 20);
			this.textBox_CustomFileName.TabIndex = 3;
			this.textBox_CustomFileName.TextChanged += new System.EventHandler(this.textBox_CustomFileName_TextChanged);
			// 
			// textBox_LevelName
			// 
			this.textBox_LevelName.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
			this.textBox_LevelName.Location = new System.Drawing.Point(9, 25);
			this.textBox_LevelName.Margin = new System.Windows.Forms.Padding(0, 3, 0, 3);
			this.textBox_LevelName.Name = "textBox_LevelName";
			this.textBox_LevelName.Size = new System.Drawing.Size(446, 26);
			this.textBox_LevelName.TabIndex = 1;
			this.textBox_LevelName.TextChanged += new System.EventHandler(this.textBox_LevelName_TextChanged);
			// 
			// FormLevelSetup
			// 
			this.AcceptButton = this.button_Create;
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(464, 238);
			this.Controls.Add(this.textBox_CustomFileName);
			this.Controls.Add(this.checkBox_CustomFileName);
			this.Controls.Add(this.panel_01);
			this.Controls.Add(this.button_Create);
			this.Controls.Add(this.label_01);
			this.Controls.Add(this.textBox_LevelName);
			this.FlatBorder = true;
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.MaximizeBox = false;
			this.Name = "FormLevelSetup";
			this.ShowIcon = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "Create New Level";
			((System.ComponentModel.ISupportInitialize)(this.numeric_SoundID)).EndInit();
			this.panel_01.ResumeLayout(false);
			this.panel_ScriptSettings.ResumeLayout(false);
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private DarkUI.Controls.DarkButton button_Create;
		private DarkUI.Controls.DarkButton button_OpenAudioFolder;
		private DarkUI.Controls.DarkCheckBox checkBox_CustomFileName;
		private DarkUI.Controls.DarkCheckBox checkBox_EnableHorizon;
		private DarkUI.Controls.DarkCheckBox checkBox_GenerateSection;
		private DarkUI.Controls.DarkLabel label_01;
		private DarkUI.Controls.DarkLabel label_02;
		private DarkUI.Controls.DarkNumericUpDown numeric_SoundID;
		private DarkUI.Controls.DarkTextBox textBox_CustomFileName;
		private DarkUI.Controls.DarkTextBox textBox_LevelName;
		private System.Windows.Forms.Panel panel_01;
		private System.Windows.Forms.Panel panel_ScriptSettings;
	}
}