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
			this.button_OpenAudioFolder = new DarkUI.Controls.DarkButton();
			this.checkBox_CustomFileName = new DarkUI.Controls.DarkCheckBox();
			this.checkBox_EnableHorizon = new DarkUI.Controls.DarkCheckBox();
			this.label_02 = new DarkUI.Controls.DarkLabel();
			this.numeric_SoundID = new DarkUI.Controls.DarkNumericUpDown();
			this.panel_ScriptSettings = new System.Windows.Forms.Panel();
			this.tableLayoutPanel_Script = new System.Windows.Forms.TableLayoutPanel();
			this.tableLayoutPanel_Main = new System.Windows.Forms.TableLayoutPanel();
			this.label_Title = new DarkUI.Controls.DarkLabel();
			this.tableLayoutPanel_Content = new System.Windows.Forms.TableLayoutPanel();
			this.checkBox_GenerateSection = new DarkUI.Controls.DarkCheckBox();
			this.textBox_CustomFileName = new DarkUI.Controls.DarkTextBox();
			this.label_01 = new DarkUI.Controls.DarkLabel();
			this.textBox_LevelName = new DarkUI.Controls.DarkTextBox();
			this.tableLayoutPanel_Buttons01 = new System.Windows.Forms.TableLayoutPanel();
			this.button_Create = new DarkUI.Controls.DarkButton();
			this.button_Cancel = new DarkUI.Controls.DarkButton();
			((System.ComponentModel.ISupportInitialize)(this.numeric_SoundID)).BeginInit();
			this.panel_ScriptSettings.SuspendLayout();
			this.tableLayoutPanel_Script.SuspendLayout();
			this.tableLayoutPanel_Main.SuspendLayout();
			this.tableLayoutPanel_Content.SuspendLayout();
			this.tableLayoutPanel_Buttons01.SuspendLayout();
			this.SuspendLayout();
			// 
			// button_OpenAudioFolder
			// 
			this.button_OpenAudioFolder.Checked = false;
			this.button_OpenAudioFolder.Dock = System.Windows.Forms.DockStyle.Fill;
			this.button_OpenAudioFolder.Image = global::TombIDE.ProjectMaster.Properties.Resources.forward_arrow_16;
			this.button_OpenAudioFolder.Location = new System.Drawing.Point(373, 20);
			this.button_OpenAudioFolder.Margin = new System.Windows.Forms.Padding(5, 0, 0, 5);
			this.button_OpenAudioFolder.Name = "button_OpenAudioFolder";
			this.button_OpenAudioFolder.Size = new System.Drawing.Size(105, 25);
			this.button_OpenAudioFolder.TabIndex = 2;
			this.button_OpenAudioFolder.Text = "Open /audio/";
			this.button_OpenAudioFolder.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
			this.button_OpenAudioFolder.Click += new System.EventHandler(this.button_OpenAudioFolder_Click);
			// 
			// checkBox_CustomFileName
			// 
			this.checkBox_CustomFileName.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.checkBox_CustomFileName.Location = new System.Drawing.Point(0, 104);
			this.checkBox_CustomFileName.Margin = new System.Windows.Forms.Padding(0, 0, 0, 6);
			this.checkBox_CustomFileName.Name = "checkBox_CustomFileName";
			this.checkBox_CustomFileName.Size = new System.Drawing.Size(520, 20);
			this.checkBox_CustomFileName.TabIndex = 2;
			this.checkBox_CustomFileName.Text = "Use custom PRJ2 / DATA name";
			this.checkBox_CustomFileName.CheckedChanged += new System.EventHandler(this.checkBox_CustomFileName_CheckedChanged);
			// 
			// checkBox_EnableHorizon
			// 
			this.checkBox_EnableHorizon.Checked = true;
			this.checkBox_EnableHorizon.CheckState = System.Windows.Forms.CheckState.Checked;
			this.tableLayoutPanel_Script.SetColumnSpan(this.checkBox_EnableHorizon, 2);
			this.checkBox_EnableHorizon.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.checkBox_EnableHorizon.Location = new System.Drawing.Point(0, 75);
			this.checkBox_EnableHorizon.Margin = new System.Windows.Forms.Padding(0);
			this.checkBox_EnableHorizon.Name = "checkBox_EnableHorizon";
			this.checkBox_EnableHorizon.Size = new System.Drawing.Size(478, 25);
			this.checkBox_EnableHorizon.TabIndex = 3;
			this.checkBox_EnableHorizon.Text = "Enable HORIZON (Make the skybox visible in the level)";
			// 
			// label_02
			// 
			this.tableLayoutPanel_Script.SetColumnSpan(this.label_02, 2);
			this.label_02.Dock = System.Windows.Forms.DockStyle.Fill;
			this.label_02.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
			this.label_02.Location = new System.Drawing.Point(0, 0);
			this.label_02.Margin = new System.Windows.Forms.Padding(0, 0, 0, 6);
			this.label_02.Name = "label_02";
			this.label_02.Size = new System.Drawing.Size(478, 14);
			this.label_02.TabIndex = 0;
			this.label_02.Text = "Ambient sound ID:";
			this.label_02.TextAlign = System.Drawing.ContentAlignment.BottomLeft;
			// 
			// numeric_SoundID
			// 
			this.numeric_SoundID.Dock = System.Windows.Forms.DockStyle.Fill;
			this.numeric_SoundID.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
			this.numeric_SoundID.IncrementAlternate = new decimal(new int[] {
            10,
            0,
            0,
            65536});
			this.numeric_SoundID.Location = new System.Drawing.Point(0, 20);
			this.numeric_SoundID.LoopValues = false;
			this.numeric_SoundID.Margin = new System.Windows.Forms.Padding(0);
			this.numeric_SoundID.Maximum = new decimal(new int[] {
            999,
            0,
            0,
            0});
			this.numeric_SoundID.Name = "numeric_SoundID";
			this.numeric_SoundID.Size = new System.Drawing.Size(368, 25);
			this.numeric_SoundID.TabIndex = 1;
			this.numeric_SoundID.Value = new decimal(new int[] {
            110,
            0,
            0,
            0});
			// 
			// panel_ScriptSettings
			// 
			this.panel_ScriptSettings.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.panel_ScriptSettings.Controls.Add(this.tableLayoutPanel_Script);
			this.panel_ScriptSettings.Dock = System.Windows.Forms.DockStyle.Top;
			this.panel_ScriptSettings.Location = new System.Drawing.Point(0, 210);
			this.panel_ScriptSettings.Margin = new System.Windows.Forms.Padding(0);
			this.panel_ScriptSettings.Name = "panel_ScriptSettings";
			this.panel_ScriptSettings.Padding = new System.Windows.Forms.Padding(20);
			this.panel_ScriptSettings.Size = new System.Drawing.Size(520, 140);
			this.panel_ScriptSettings.TabIndex = 1;
			// 
			// tableLayoutPanel_Script
			// 
			this.tableLayoutPanel_Script.ColumnCount = 2;
			this.tableLayoutPanel_Script.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanel_Script.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 110F));
			this.tableLayoutPanel_Script.Controls.Add(this.label_02, 0, 0);
			this.tableLayoutPanel_Script.Controls.Add(this.checkBox_EnableHorizon, 0, 2);
			this.tableLayoutPanel_Script.Controls.Add(this.button_OpenAudioFolder, 1, 1);
			this.tableLayoutPanel_Script.Controls.Add(this.numeric_SoundID, 0, 1);
			this.tableLayoutPanel_Script.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tableLayoutPanel_Script.Location = new System.Drawing.Point(20, 20);
			this.tableLayoutPanel_Script.Margin = new System.Windows.Forms.Padding(0);
			this.tableLayoutPanel_Script.Name = "tableLayoutPanel_Script";
			this.tableLayoutPanel_Script.RowCount = 3;
			this.tableLayoutPanel_Script.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
			this.tableLayoutPanel_Script.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
			this.tableLayoutPanel_Script.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 50F));
			this.tableLayoutPanel_Script.Size = new System.Drawing.Size(478, 98);
			this.tableLayoutPanel_Script.TabIndex = 4;
			// 
			// tableLayoutPanel_Main
			// 
			this.tableLayoutPanel_Main.ColumnCount = 2;
			this.tableLayoutPanel_Main.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 520F));
			this.tableLayoutPanel_Main.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanel_Main.Controls.Add(this.label_Title, 0, 0);
			this.tableLayoutPanel_Main.Controls.Add(this.tableLayoutPanel_Content, 0, 1);
			this.tableLayoutPanel_Main.Controls.Add(this.tableLayoutPanel_Buttons01, 1, 1);
			this.tableLayoutPanel_Main.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tableLayoutPanel_Main.Location = new System.Drawing.Point(0, 0);
			this.tableLayoutPanel_Main.Margin = new System.Windows.Forms.Padding(0);
			this.tableLayoutPanel_Main.Name = "tableLayoutPanel_Main";
			this.tableLayoutPanel_Main.Padding = new System.Windows.Forms.Padding(30);
			this.tableLayoutPanel_Main.RowCount = 2;
			this.tableLayoutPanel_Main.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 35F));
			this.tableLayoutPanel_Main.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanel_Main.Size = new System.Drawing.Size(784, 511);
			this.tableLayoutPanel_Main.TabIndex = 19;
			// 
			// label_Title
			// 
			this.label_Title.Dock = System.Windows.Forms.DockStyle.Fill;
			this.label_Title.Font = new System.Drawing.Font("Segoe UI", 15.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
			this.label_Title.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
			this.label_Title.Location = new System.Drawing.Point(30, 30);
			this.label_Title.Margin = new System.Windows.Forms.Padding(0);
			this.label_Title.Name = "label_Title";
			this.label_Title.Size = new System.Drawing.Size(520, 35);
			this.label_Title.TabIndex = 4;
			this.label_Title.Text = "Create a new level";
			// 
			// tableLayoutPanel_Content
			// 
			this.tableLayoutPanel_Content.ColumnCount = 1;
			this.tableLayoutPanel_Content.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanel_Content.Controls.Add(this.checkBox_CustomFileName, 0, 2);
			this.tableLayoutPanel_Content.Controls.Add(this.checkBox_GenerateSection, 0, 4);
			this.tableLayoutPanel_Content.Controls.Add(this.textBox_CustomFileName, 0, 3);
			this.tableLayoutPanel_Content.Controls.Add(this.panel_ScriptSettings, 0, 5);
			this.tableLayoutPanel_Content.Controls.Add(this.label_01, 0, 0);
			this.tableLayoutPanel_Content.Controls.Add(this.textBox_LevelName, 0, 1);
			this.tableLayoutPanel_Content.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tableLayoutPanel_Content.Location = new System.Drawing.Point(30, 65);
			this.tableLayoutPanel_Content.Margin = new System.Windows.Forms.Padding(0);
			this.tableLayoutPanel_Content.Name = "tableLayoutPanel_Content";
			this.tableLayoutPanel_Content.RowCount = 6;
			this.tableLayoutPanel_Content.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 50F));
			this.tableLayoutPanel_Content.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
			this.tableLayoutPanel_Content.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 50F));
			this.tableLayoutPanel_Content.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
			this.tableLayoutPanel_Content.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 50F));
			this.tableLayoutPanel_Content.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanel_Content.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
			this.tableLayoutPanel_Content.Size = new System.Drawing.Size(520, 416);
			this.tableLayoutPanel_Content.TabIndex = 0;
			// 
			// checkBox_GenerateSection
			// 
			this.checkBox_GenerateSection.Checked = true;
			this.checkBox_GenerateSection.CheckState = System.Windows.Forms.CheckState.Checked;
			this.checkBox_GenerateSection.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.checkBox_GenerateSection.Location = new System.Drawing.Point(0, 184);
			this.checkBox_GenerateSection.Margin = new System.Windows.Forms.Padding(0, 0, 0, 6);
			this.checkBox_GenerateSection.Name = "checkBox_GenerateSection";
			this.checkBox_GenerateSection.Size = new System.Drawing.Size(520, 20);
			this.checkBox_GenerateSection.TabIndex = 0;
			this.checkBox_GenerateSection.Text = "Generate a new [Level] section for this level in the project\'s Script.txt file";
			this.checkBox_GenerateSection.CheckedChanged += new System.EventHandler(this.checkBox_GenerateSection_CheckedChanged);
			// 
			// textBox_CustomFileName
			// 
			this.textBox_CustomFileName.Dock = System.Windows.Forms.DockStyle.Fill;
			this.textBox_CustomFileName.Enabled = false;
			this.textBox_CustomFileName.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
			this.textBox_CustomFileName.Location = new System.Drawing.Point(0, 130);
			this.textBox_CustomFileName.Margin = new System.Windows.Forms.Padding(0);
			this.textBox_CustomFileName.Name = "textBox_CustomFileName";
			this.textBox_CustomFileName.Size = new System.Drawing.Size(520, 25);
			this.textBox_CustomFileName.TabIndex = 5;
			// 
			// label_01
			// 
			this.label_01.Dock = System.Windows.Forms.DockStyle.Fill;
			this.label_01.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
			this.label_01.Location = new System.Drawing.Point(0, 0);
			this.label_01.Margin = new System.Windows.Forms.Padding(0, 0, 0, 6);
			this.label_01.Name = "label_01";
			this.label_01.Size = new System.Drawing.Size(520, 44);
			this.label_01.TabIndex = 0;
			this.label_01.Text = "Level name:";
			this.label_01.TextAlign = System.Drawing.ContentAlignment.BottomLeft;
			// 
			// textBox_LevelName
			// 
			this.textBox_LevelName.Dock = System.Windows.Forms.DockStyle.Fill;
			this.textBox_LevelName.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
			this.textBox_LevelName.Location = new System.Drawing.Point(0, 50);
			this.textBox_LevelName.Margin = new System.Windows.Forms.Padding(0);
			this.textBox_LevelName.Name = "textBox_LevelName";
			this.textBox_LevelName.Size = new System.Drawing.Size(520, 29);
			this.textBox_LevelName.TabIndex = 1;
			this.textBox_LevelName.TextChanged += new System.EventHandler(this.textBox_LevelName_TextChanged);
			// 
			// tableLayoutPanel_Buttons01
			// 
			this.tableLayoutPanel_Buttons01.ColumnCount = 2;
			this.tableLayoutPanel_Buttons01.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanel_Buttons01.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 80F));
			this.tableLayoutPanel_Buttons01.Controls.Add(this.button_Create, 0, 0);
			this.tableLayoutPanel_Buttons01.Controls.Add(this.button_Cancel, 0, 0);
			this.tableLayoutPanel_Buttons01.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tableLayoutPanel_Buttons01.Location = new System.Drawing.Point(550, 65);
			this.tableLayoutPanel_Buttons01.Margin = new System.Windows.Forms.Padding(0);
			this.tableLayoutPanel_Buttons01.Name = "tableLayoutPanel_Buttons01";
			this.tableLayoutPanel_Buttons01.RowCount = 1;
			this.tableLayoutPanel_Buttons01.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanel_Buttons01.Size = new System.Drawing.Size(204, 416);
			this.tableLayoutPanel_Buttons01.TabIndex = 3;
			// 
			// button_Create
			// 
			this.button_Create.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.button_Create.Checked = false;
			this.button_Create.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.button_Create.Location = new System.Drawing.Point(129, 391);
			this.button_Create.Margin = new System.Windows.Forms.Padding(0);
			this.button_Create.Name = "button_Create";
			this.button_Create.Size = new System.Drawing.Size(75, 25);
			this.button_Create.TabIndex = 13;
			this.button_Create.Text = "Create";
			this.button_Create.Click += new System.EventHandler(this.button_Create_Click);
			// 
			// button_Cancel
			// 
			this.button_Cancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.button_Cancel.Checked = false;
			this.button_Cancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.button_Cancel.Location = new System.Drawing.Point(49, 391);
			this.button_Cancel.Margin = new System.Windows.Forms.Padding(0);
			this.button_Cancel.Name = "button_Cancel";
			this.button_Cancel.Size = new System.Drawing.Size(75, 25);
			this.button_Cancel.TabIndex = 12;
			this.button_Cancel.Text = "Cancel";
			// 
			// FormLevelSetup
			// 
			this.AcceptButton = this.button_Create;
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.CancelButton = this.button_Cancel;
			this.ClientSize = new System.Drawing.Size(784, 511);
			this.Controls.Add(this.tableLayoutPanel_Main);
			this.FlatBorder = true;
			this.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.MaximizeBox = false;
			this.Name = "FormLevelSetup";
			this.ShowIcon = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "Create a new level...";
			((System.ComponentModel.ISupportInitialize)(this.numeric_SoundID)).EndInit();
			this.panel_ScriptSettings.ResumeLayout(false);
			this.tableLayoutPanel_Script.ResumeLayout(false);
			this.tableLayoutPanel_Main.ResumeLayout(false);
			this.tableLayoutPanel_Content.ResumeLayout(false);
			this.tableLayoutPanel_Content.PerformLayout();
			this.tableLayoutPanel_Buttons01.ResumeLayout(false);
			this.ResumeLayout(false);

		}

		#endregion

		private DarkUI.Controls.DarkButton button_OpenAudioFolder;
		private DarkUI.Controls.DarkCheckBox checkBox_CustomFileName;
		private DarkUI.Controls.DarkCheckBox checkBox_EnableHorizon;
		private DarkUI.Controls.DarkLabel label_02;
		private DarkUI.Controls.DarkNumericUpDown numeric_SoundID;
		private System.Windows.Forms.Panel panel_ScriptSettings;
		private System.Windows.Forms.TableLayoutPanel tableLayoutPanel_Main;
		private System.Windows.Forms.TableLayoutPanel tableLayoutPanel_Content;
		private DarkUI.Controls.DarkTextBox textBox_CustomFileName;
		private DarkUI.Controls.DarkLabel label_01;
		private DarkUI.Controls.DarkTextBox textBox_LevelName;
		private System.Windows.Forms.TableLayoutPanel tableLayoutPanel_Buttons01;
		private DarkUI.Controls.DarkButton button_Create;
		private DarkUI.Controls.DarkButton button_Cancel;
		private System.Windows.Forms.TableLayoutPanel tableLayoutPanel_Script;
		private DarkUI.Controls.DarkLabel label_Title;
		private DarkUI.Controls.DarkCheckBox checkBox_GenerateSection;
	}
}