namespace TombIDE
{
	partial class FormProjectSetup
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
			this.button_BrowseScript = new DarkUI.Controls.DarkButton();
			this.button_Create = new DarkUI.Controls.DarkButton();
			this.panel_LevelsRadioChoice = new System.Windows.Forms.Panel();
			this.radio_Levels_01 = new DarkUI.Controls.DarkRadioButton();
			this.radio_Levels_02 = new DarkUI.Controls.DarkRadioButton();
			this.panel_ScriptRadioChoice = new System.Windows.Forms.Panel();
			this.radio_Script_01 = new DarkUI.Controls.DarkRadioButton();
			this.radio_Script_02 = new DarkUI.Controls.DarkRadioButton();
			this.textBox_LevelsPath = new DarkUI.Controls.DarkTextBox();
			this.textBox_ScriptPath = new DarkUI.Controls.DarkTextBox();
			this.progressBar = new DarkUI.Controls.DarkProgressBar();
			this.tabPage_Extra = new System.Windows.Forms.TabPage();
			this.tableLayoutPanel_Main02 = new System.Windows.Forms.TableLayoutPanel();
			this.tableLayoutPanel_Buttons02 = new System.Windows.Forms.TableLayoutPanel();
			this.button_Back = new DarkUI.Controls.DarkButton();
			this.tableLayoutPanel_Content02 = new System.Windows.Forms.TableLayoutPanel();
			this.tableLayoutPanel3 = new System.Windows.Forms.TableLayoutPanel();
			this.label_SubTitle02 = new DarkUI.Controls.DarkLabel();
			this.label_Title02 = new DarkUI.Controls.DarkLabel();
			this.tabPage_Basic = new System.Windows.Forms.TabPage();
			this.tableLayoutPanel_Main01 = new System.Windows.Forms.TableLayoutPanel();
			this.tableLayoutPanel_Content01 = new System.Windows.Forms.TableLayoutPanel();
			this.button_BrowseProject = new DarkUI.Controls.DarkButton();
			this.button_Help = new DarkUI.Controls.DarkButton();
			this.textBox_ProjectPath = new DarkUI.Controls.DarkTextBox();
			this.comboBox_EngineType = new DarkUI.Controls.DarkComboBox();
			this.label_01 = new DarkUI.Controls.DarkLabel();
			this.label_02 = new DarkUI.Controls.DarkLabel();
			this.textBox_ProjectName = new DarkUI.Controls.DarkTextBox();
			this.label_03 = new DarkUI.Controls.DarkLabel();
			this.tableLayoutPanel_Title = new System.Windows.Forms.TableLayoutPanel();
			this.label_SubTitle01 = new DarkUI.Controls.DarkLabel();
			this.label_Title01 = new DarkUI.Controls.DarkLabel();
			this.tableLayoutPanel_Buttons01 = new System.Windows.Forms.TableLayoutPanel();
			this.button_Next = new DarkUI.Controls.DarkButton();
			this.button_Cancel = new DarkUI.Controls.DarkButton();
			this.tablessTabControl = new TombLib.Controls.DarkTabbedContainer();
			this.panel_LevelsRadioChoice.SuspendLayout();
			this.panel_ScriptRadioChoice.SuspendLayout();
			this.tabPage_Extra.SuspendLayout();
			this.tableLayoutPanel_Main02.SuspendLayout();
			this.tableLayoutPanel_Buttons02.SuspendLayout();
			this.tableLayoutPanel_Content02.SuspendLayout();
			this.tableLayoutPanel3.SuspendLayout();
			this.tabPage_Basic.SuspendLayout();
			this.tableLayoutPanel_Main01.SuspendLayout();
			this.tableLayoutPanel_Content01.SuspendLayout();
			this.tableLayoutPanel_Title.SuspendLayout();
			this.tableLayoutPanel_Buttons01.SuspendLayout();
			this.tablessTabControl.SuspendLayout();
			this.SuspendLayout();
			// 
			// button_BrowseLevels
			// 
			this.button_BrowseLevels.Checked = false;
			this.button_BrowseLevels.Dock = System.Windows.Forms.DockStyle.Fill;
			this.button_BrowseLevels.Enabled = false;
			this.button_BrowseLevels.Location = new System.Drawing.Point(425, 190);
			this.button_BrowseLevels.Margin = new System.Windows.Forms.Padding(5, 0, 0, 5);
			this.button_BrowseLevels.Name = "button_BrowseLevels";
			this.button_BrowseLevels.Size = new System.Drawing.Size(95, 25);
			this.button_BrowseLevels.TabIndex = 14;
			this.button_BrowseLevels.Text = "Browse...";
			this.button_BrowseLevels.Click += new System.EventHandler(this.button_BrowseLevels_Click);
			// 
			// button_BrowseScript
			// 
			this.button_BrowseScript.Checked = false;
			this.button_BrowseScript.Dock = System.Windows.Forms.DockStyle.Fill;
			this.button_BrowseScript.Enabled = false;
			this.button_BrowseScript.Location = new System.Drawing.Point(425, 80);
			this.button_BrowseScript.Margin = new System.Windows.Forms.Padding(5, 0, 0, 5);
			this.button_BrowseScript.Name = "button_BrowseScript";
			this.button_BrowseScript.Size = new System.Drawing.Size(95, 25);
			this.button_BrowseScript.TabIndex = 10;
			this.button_BrowseScript.Text = "Browse...";
			this.button_BrowseScript.Click += new System.EventHandler(this.button_BrowseScript_Click);
			// 
			// button_Create
			// 
			this.button_Create.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.button_Create.Checked = false;
			this.button_Create.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.button_Create.Location = new System.Drawing.Point(121, 350);
			this.button_Create.Margin = new System.Windows.Forms.Padding(0);
			this.button_Create.Name = "button_Create";
			this.button_Create.Size = new System.Drawing.Size(75, 25);
			this.button_Create.TabIndex = 1;
			this.button_Create.Text = "Create";
			this.button_Create.Click += new System.EventHandler(this.button_Create_Click);
			// 
			// panel_LevelsRadioChoice
			// 
			this.panel_LevelsRadioChoice.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.panel_LevelsRadioChoice.Controls.Add(this.radio_Levels_01);
			this.panel_LevelsRadioChoice.Controls.Add(this.radio_Levels_02);
			this.panel_LevelsRadioChoice.Location = new System.Drawing.Point(0, 140);
			this.panel_LevelsRadioChoice.Margin = new System.Windows.Forms.Padding(0);
			this.panel_LevelsRadioChoice.Name = "panel_LevelsRadioChoice";
			this.panel_LevelsRadioChoice.Size = new System.Drawing.Size(420, 50);
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
			// panel_ScriptRadioChoice
			// 
			this.panel_ScriptRadioChoice.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.panel_ScriptRadioChoice.Controls.Add(this.radio_Script_01);
			this.panel_ScriptRadioChoice.Controls.Add(this.radio_Script_02);
			this.panel_ScriptRadioChoice.Location = new System.Drawing.Point(0, 30);
			this.panel_ScriptRadioChoice.Margin = new System.Windows.Forms.Padding(0);
			this.panel_ScriptRadioChoice.Name = "panel_ScriptRadioChoice";
			this.panel_ScriptRadioChoice.Size = new System.Drawing.Size(420, 50);
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
			this.textBox_LevelsPath.Dock = System.Windows.Forms.DockStyle.Fill;
			this.textBox_LevelsPath.Enabled = false;
			this.textBox_LevelsPath.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
			this.textBox_LevelsPath.Location = new System.Drawing.Point(0, 190);
			this.textBox_LevelsPath.Margin = new System.Windows.Forms.Padding(0);
			this.textBox_LevelsPath.Name = "textBox_LevelsPath";
			this.textBox_LevelsPath.Size = new System.Drawing.Size(420, 25);
			this.textBox_LevelsPath.TabIndex = 13;
			// 
			// textBox_ScriptPath
			// 
			this.textBox_ScriptPath.Dock = System.Windows.Forms.DockStyle.Fill;
			this.textBox_ScriptPath.Enabled = false;
			this.textBox_ScriptPath.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
			this.textBox_ScriptPath.Location = new System.Drawing.Point(0, 80);
			this.textBox_ScriptPath.Margin = new System.Windows.Forms.Padding(0);
			this.textBox_ScriptPath.Name = "textBox_ScriptPath";
			this.textBox_ScriptPath.Size = new System.Drawing.Size(420, 25);
			this.textBox_ScriptPath.TabIndex = 9;
			// 
			// progressBar
			// 
			this.progressBar.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.tableLayoutPanel_Content02.SetColumnSpan(this.progressBar, 2);
			this.progressBar.Location = new System.Drawing.Point(0, 350);
			this.progressBar.Margin = new System.Windows.Forms.Padding(0);
			this.progressBar.Name = "progressBar";
			this.progressBar.Size = new System.Drawing.Size(520, 25);
			this.progressBar.TabIndex = 2;
			// 
			// tabPage_Extra
			// 
			this.tabPage_Extra.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
			this.tabPage_Extra.Controls.Add(this.tableLayoutPanel_Main02);
			this.tabPage_Extra.Location = new System.Drawing.Point(4, 22);
			this.tabPage_Extra.Name = "tabPage_Extra";
			this.tabPage_Extra.Size = new System.Drawing.Size(776, 485);
			this.tabPage_Extra.TabIndex = 1;
			this.tabPage_Extra.Text = "Extra Options";
			// 
			// tableLayoutPanel_Main02
			// 
			this.tableLayoutPanel_Main02.ColumnCount = 2;
			this.tableLayoutPanel_Main02.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 520F));
			this.tableLayoutPanel_Main02.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanel_Main02.Controls.Add(this.tableLayoutPanel_Buttons02, 1, 1);
			this.tableLayoutPanel_Main02.Controls.Add(this.tableLayoutPanel_Content02, 0, 1);
			this.tableLayoutPanel_Main02.Controls.Add(this.tableLayoutPanel3, 0, 0);
			this.tableLayoutPanel_Main02.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tableLayoutPanel_Main02.Location = new System.Drawing.Point(0, 0);
			this.tableLayoutPanel_Main02.Margin = new System.Windows.Forms.Padding(0);
			this.tableLayoutPanel_Main02.Name = "tableLayoutPanel_Main02";
			this.tableLayoutPanel_Main02.Padding = new System.Windows.Forms.Padding(30);
			this.tableLayoutPanel_Main02.RowCount = 2;
			this.tableLayoutPanel_Main02.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 50F));
			this.tableLayoutPanel_Main02.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanel_Main02.Size = new System.Drawing.Size(776, 485);
			this.tableLayoutPanel_Main02.TabIndex = 19;
			// 
			// tableLayoutPanel_Buttons02
			// 
			this.tableLayoutPanel_Buttons02.ColumnCount = 2;
			this.tableLayoutPanel_Buttons02.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanel_Buttons02.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 80F));
			this.tableLayoutPanel_Buttons02.Controls.Add(this.button_Back, 0, 0);
			this.tableLayoutPanel_Buttons02.Controls.Add(this.button_Create, 1, 0);
			this.tableLayoutPanel_Buttons02.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tableLayoutPanel_Buttons02.Location = new System.Drawing.Point(550, 80);
			this.tableLayoutPanel_Buttons02.Margin = new System.Windows.Forms.Padding(0);
			this.tableLayoutPanel_Buttons02.Name = "tableLayoutPanel_Buttons02";
			this.tableLayoutPanel_Buttons02.RowCount = 1;
			this.tableLayoutPanel_Buttons02.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanel_Buttons02.Size = new System.Drawing.Size(196, 375);
			this.tableLayoutPanel_Buttons02.TabIndex = 4;
			// 
			// button_Back
			// 
			this.button_Back.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.button_Back.Checked = false;
			this.button_Back.Location = new System.Drawing.Point(41, 350);
			this.button_Back.Margin = new System.Windows.Forms.Padding(0);
			this.button_Back.Name = "button_Back";
			this.button_Back.Size = new System.Drawing.Size(75, 25);
			this.button_Back.TabIndex = 11;
			this.button_Back.Text = "Back";
			this.button_Back.Click += new System.EventHandler(this.button_Back_Click);
			// 
			// tableLayoutPanel_Content02
			// 
			this.tableLayoutPanel_Content02.ColumnCount = 2;
			this.tableLayoutPanel_Content02.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanel_Content02.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 100F));
			this.tableLayoutPanel_Content02.Controls.Add(this.panel_LevelsRadioChoice, 0, 2);
			this.tableLayoutPanel_Content02.Controls.Add(this.progressBar, 0, 4);
			this.tableLayoutPanel_Content02.Controls.Add(this.panel_ScriptRadioChoice, 0, 0);
			this.tableLayoutPanel_Content02.Controls.Add(this.button_BrowseLevels, 1, 3);
			this.tableLayoutPanel_Content02.Controls.Add(this.textBox_LevelsPath, 0, 3);
			this.tableLayoutPanel_Content02.Controls.Add(this.textBox_ScriptPath, 0, 1);
			this.tableLayoutPanel_Content02.Controls.Add(this.button_BrowseScript, 1, 1);
			this.tableLayoutPanel_Content02.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tableLayoutPanel_Content02.Location = new System.Drawing.Point(30, 80);
			this.tableLayoutPanel_Content02.Margin = new System.Windows.Forms.Padding(0);
			this.tableLayoutPanel_Content02.Name = "tableLayoutPanel_Content02";
			this.tableLayoutPanel_Content02.RowCount = 5;
			this.tableLayoutPanel_Content02.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 80F));
			this.tableLayoutPanel_Content02.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
			this.tableLayoutPanel_Content02.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 80F));
			this.tableLayoutPanel_Content02.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
			this.tableLayoutPanel_Content02.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanel_Content02.Size = new System.Drawing.Size(520, 375);
			this.tableLayoutPanel_Content02.TabIndex = 0;
			// 
			// tableLayoutPanel3
			// 
			this.tableLayoutPanel3.ColumnCount = 1;
			this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanel3.Controls.Add(this.label_SubTitle02, 0, 1);
			this.tableLayoutPanel3.Controls.Add(this.label_Title02, 0, 0);
			this.tableLayoutPanel3.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tableLayoutPanel3.Location = new System.Drawing.Point(30, 30);
			this.tableLayoutPanel3.Margin = new System.Windows.Forms.Padding(0);
			this.tableLayoutPanel3.Name = "tableLayoutPanel3";
			this.tableLayoutPanel3.RowCount = 2;
			this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
			this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanel3.Size = new System.Drawing.Size(520, 50);
			this.tableLayoutPanel3.TabIndex = 2;
			// 
			// label_SubTitle02
			// 
			this.label_SubTitle02.Dock = System.Windows.Forms.DockStyle.Fill;
			this.label_SubTitle02.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
			this.label_SubTitle02.Location = new System.Drawing.Point(0, 30);
			this.label_SubTitle02.Margin = new System.Windows.Forms.Padding(0);
			this.label_SubTitle02.Name = "label_SubTitle02";
			this.label_SubTitle02.Size = new System.Drawing.Size(520, 20);
			this.label_SubTitle02.TabIndex = 2;
			this.label_SubTitle02.Text = "Extra Options";
			this.label_SubTitle02.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// label_Title02
			// 
			this.label_Title02.Font = new System.Drawing.Font("Segoe UI", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
			this.label_Title02.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
			this.label_Title02.Location = new System.Drawing.Point(0, 0);
			this.label_Title02.Margin = new System.Windows.Forms.Padding(0);
			this.label_Title02.Name = "label_Title02";
			this.label_Title02.Size = new System.Drawing.Size(392, 30);
			this.label_Title02.TabIndex = 1;
			this.label_Title02.Text = "Create a new project";
			// 
			// tabPage_Basic
			// 
			this.tabPage_Basic.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
			this.tabPage_Basic.Controls.Add(this.tableLayoutPanel_Main01);
			this.tabPage_Basic.Location = new System.Drawing.Point(4, 22);
			this.tabPage_Basic.Name = "tabPage_Basic";
			this.tabPage_Basic.Size = new System.Drawing.Size(776, 485);
			this.tabPage_Basic.TabIndex = 0;
			this.tabPage_Basic.Text = "Basic Info";
			// 
			// tableLayoutPanel_Main01
			// 
			this.tableLayoutPanel_Main01.ColumnCount = 2;
			this.tableLayoutPanel_Main01.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 520F));
			this.tableLayoutPanel_Main01.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanel_Main01.Controls.Add(this.tableLayoutPanel_Content01, 0, 1);
			this.tableLayoutPanel_Main01.Controls.Add(this.tableLayoutPanel_Title, 0, 0);
			this.tableLayoutPanel_Main01.Controls.Add(this.tableLayoutPanel_Buttons01, 1, 1);
			this.tableLayoutPanel_Main01.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tableLayoutPanel_Main01.Location = new System.Drawing.Point(0, 0);
			this.tableLayoutPanel_Main01.Margin = new System.Windows.Forms.Padding(0);
			this.tableLayoutPanel_Main01.Name = "tableLayoutPanel_Main01";
			this.tableLayoutPanel_Main01.Padding = new System.Windows.Forms.Padding(30);
			this.tableLayoutPanel_Main01.RowCount = 2;
			this.tableLayoutPanel_Main01.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 50F));
			this.tableLayoutPanel_Main01.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanel_Main01.Size = new System.Drawing.Size(776, 485);
			this.tableLayoutPanel_Main01.TabIndex = 18;
			// 
			// tableLayoutPanel_Content01
			// 
			this.tableLayoutPanel_Content01.ColumnCount = 2;
			this.tableLayoutPanel_Content01.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanel_Content01.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 100F));
			this.tableLayoutPanel_Content01.Controls.Add(this.button_BrowseProject, 1, 3);
			this.tableLayoutPanel_Content01.Controls.Add(this.button_Help, 1, 5);
			this.tableLayoutPanel_Content01.Controls.Add(this.textBox_ProjectPath, 0, 3);
			this.tableLayoutPanel_Content01.Controls.Add(this.comboBox_EngineType, 0, 5);
			this.tableLayoutPanel_Content01.Controls.Add(this.label_01, 0, 0);
			this.tableLayoutPanel_Content01.Controls.Add(this.label_02, 0, 4);
			this.tableLayoutPanel_Content01.Controls.Add(this.textBox_ProjectName, 0, 1);
			this.tableLayoutPanel_Content01.Controls.Add(this.label_03, 0, 2);
			this.tableLayoutPanel_Content01.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tableLayoutPanel_Content01.Location = new System.Drawing.Point(30, 80);
			this.tableLayoutPanel_Content01.Margin = new System.Windows.Forms.Padding(0);
			this.tableLayoutPanel_Content01.Name = "tableLayoutPanel_Content01";
			this.tableLayoutPanel_Content01.RowCount = 7;
			this.tableLayoutPanel_Content01.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 50F));
			this.tableLayoutPanel_Content01.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
			this.tableLayoutPanel_Content01.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 50F));
			this.tableLayoutPanel_Content01.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
			this.tableLayoutPanel_Content01.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 50F));
			this.tableLayoutPanel_Content01.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
			this.tableLayoutPanel_Content01.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanel_Content01.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
			this.tableLayoutPanel_Content01.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
			this.tableLayoutPanel_Content01.Size = new System.Drawing.Size(520, 375);
			this.tableLayoutPanel_Content01.TabIndex = 0;
			// 
			// button_BrowseProject
			// 
			this.button_BrowseProject.Checked = false;
			this.button_BrowseProject.Dock = System.Windows.Forms.DockStyle.Fill;
			this.button_BrowseProject.Location = new System.Drawing.Point(425, 130);
			this.button_BrowseProject.Margin = new System.Windows.Forms.Padding(5, 0, 0, 5);
			this.button_BrowseProject.Name = "button_BrowseProject";
			this.button_BrowseProject.Size = new System.Drawing.Size(95, 25);
			this.button_BrowseProject.TabIndex = 6;
			this.button_BrowseProject.Text = "Browse...";
			this.button_BrowseProject.Click += new System.EventHandler(this.button_BrowseProject_Click);
			// 
			// button_Help
			// 
			this.button_Help.Checked = false;
			this.button_Help.Dock = System.Windows.Forms.DockStyle.Fill;
			this.button_Help.Location = new System.Drawing.Point(425, 210);
			this.button_Help.Margin = new System.Windows.Forms.Padding(5, 0, 0, 4);
			this.button_Help.Name = "button_Help";
			this.button_Help.Size = new System.Drawing.Size(95, 26);
			this.button_Help.TabIndex = 17;
			this.button_Help.Text = "Need Help?";
			this.button_Help.Click += new System.EventHandler(this.button_Help_Click);
			// 
			// textBox_ProjectPath
			// 
			this.textBox_ProjectPath.Dock = System.Windows.Forms.DockStyle.Fill;
			this.textBox_ProjectPath.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
			this.textBox_ProjectPath.Location = new System.Drawing.Point(0, 130);
			this.textBox_ProjectPath.Margin = new System.Windows.Forms.Padding(0);
			this.textBox_ProjectPath.Name = "textBox_ProjectPath";
			this.textBox_ProjectPath.Size = new System.Drawing.Size(420, 25);
			this.textBox_ProjectPath.TabIndex = 5;
			// 
			// comboBox_EngineType
			// 
			this.comboBox_EngineType.Dock = System.Windows.Forms.DockStyle.Fill;
			this.comboBox_EngineType.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
			this.comboBox_EngineType.FormattingEnabled = true;
			this.comboBox_EngineType.Items.AddRange(new object[] {
            "- Select -",
            "TRNG",
            "TRNG + FLEP"});
			this.comboBox_EngineType.Location = new System.Drawing.Point(0, 210);
			this.comboBox_EngineType.Margin = new System.Windows.Forms.Padding(0);
			this.comboBox_EngineType.Name = "comboBox_EngineType";
			this.comboBox_EngineType.Size = new System.Drawing.Size(420, 26);
			this.comboBox_EngineType.TabIndex = 3;
			// 
			// label_01
			// 
			this.label_01.Dock = System.Windows.Forms.DockStyle.Fill;
			this.label_01.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
			this.label_01.Location = new System.Drawing.Point(0, 0);
			this.label_01.Margin = new System.Windows.Forms.Padding(0, 0, 0, 6);
			this.label_01.Name = "label_01";
			this.label_01.Size = new System.Drawing.Size(420, 44);
			this.label_01.TabIndex = 0;
			this.label_01.Text = "Project name:";
			this.label_01.TextAlign = System.Drawing.ContentAlignment.BottomLeft;
			// 
			// label_02
			// 
			this.label_02.Dock = System.Windows.Forms.DockStyle.Fill;
			this.label_02.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
			this.label_02.Location = new System.Drawing.Point(0, 160);
			this.label_02.Margin = new System.Windows.Forms.Padding(0, 0, 0, 6);
			this.label_02.Name = "label_02";
			this.label_02.Size = new System.Drawing.Size(420, 44);
			this.label_02.TabIndex = 2;
			this.label_02.Text = "Engine type:";
			this.label_02.TextAlign = System.Drawing.ContentAlignment.BottomLeft;
			// 
			// textBox_ProjectName
			// 
			this.tableLayoutPanel_Content01.SetColumnSpan(this.textBox_ProjectName, 2);
			this.textBox_ProjectName.Dock = System.Windows.Forms.DockStyle.Fill;
			this.textBox_ProjectName.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
			this.textBox_ProjectName.Location = new System.Drawing.Point(0, 50);
			this.textBox_ProjectName.Margin = new System.Windows.Forms.Padding(0);
			this.textBox_ProjectName.Name = "textBox_ProjectName";
			this.textBox_ProjectName.Size = new System.Drawing.Size(520, 29);
			this.textBox_ProjectName.TabIndex = 1;
			// 
			// label_03
			// 
			this.label_03.Dock = System.Windows.Forms.DockStyle.Fill;
			this.label_03.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
			this.label_03.Location = new System.Drawing.Point(0, 80);
			this.label_03.Margin = new System.Windows.Forms.Padding(0, 0, 0, 6);
			this.label_03.Name = "label_03";
			this.label_03.Size = new System.Drawing.Size(420, 44);
			this.label_03.TabIndex = 4;
			this.label_03.Text = "Project location: (Where should the project be installed?)";
			this.label_03.TextAlign = System.Drawing.ContentAlignment.BottomLeft;
			// 
			// tableLayoutPanel_Title
			// 
			this.tableLayoutPanel_Title.ColumnCount = 1;
			this.tableLayoutPanel_Title.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanel_Title.Controls.Add(this.label_SubTitle01, 0, 1);
			this.tableLayoutPanel_Title.Controls.Add(this.label_Title01, 0, 0);
			this.tableLayoutPanel_Title.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tableLayoutPanel_Title.Location = new System.Drawing.Point(30, 30);
			this.tableLayoutPanel_Title.Margin = new System.Windows.Forms.Padding(0);
			this.tableLayoutPanel_Title.Name = "tableLayoutPanel_Title";
			this.tableLayoutPanel_Title.RowCount = 2;
			this.tableLayoutPanel_Title.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
			this.tableLayoutPanel_Title.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanel_Title.Size = new System.Drawing.Size(520, 50);
			this.tableLayoutPanel_Title.TabIndex = 2;
			// 
			// label_SubTitle01
			// 
			this.label_SubTitle01.Dock = System.Windows.Forms.DockStyle.Fill;
			this.label_SubTitle01.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
			this.label_SubTitle01.Location = new System.Drawing.Point(0, 30);
			this.label_SubTitle01.Margin = new System.Windows.Forms.Padding(0);
			this.label_SubTitle01.Name = "label_SubTitle01";
			this.label_SubTitle01.Size = new System.Drawing.Size(520, 20);
			this.label_SubTitle01.TabIndex = 2;
			this.label_SubTitle01.Text = "Basic Information";
			this.label_SubTitle01.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// label_Title01
			// 
			this.label_Title01.Font = new System.Drawing.Font("Segoe UI", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
			this.label_Title01.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
			this.label_Title01.Location = new System.Drawing.Point(0, 0);
			this.label_Title01.Margin = new System.Windows.Forms.Padding(0);
			this.label_Title01.Name = "label_Title01";
			this.label_Title01.Size = new System.Drawing.Size(392, 30);
			this.label_Title01.TabIndex = 1;
			this.label_Title01.Text = "Create a new project";
			// 
			// tableLayoutPanel_Buttons01
			// 
			this.tableLayoutPanel_Buttons01.ColumnCount = 2;
			this.tableLayoutPanel_Buttons01.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanel_Buttons01.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 80F));
			this.tableLayoutPanel_Buttons01.Controls.Add(this.button_Next, 0, 0);
			this.tableLayoutPanel_Buttons01.Controls.Add(this.button_Cancel, 0, 0);
			this.tableLayoutPanel_Buttons01.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tableLayoutPanel_Buttons01.Location = new System.Drawing.Point(550, 80);
			this.tableLayoutPanel_Buttons01.Margin = new System.Windows.Forms.Padding(0);
			this.tableLayoutPanel_Buttons01.Name = "tableLayoutPanel_Buttons01";
			this.tableLayoutPanel_Buttons01.RowCount = 1;
			this.tableLayoutPanel_Buttons01.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanel_Buttons01.Size = new System.Drawing.Size(196, 375);
			this.tableLayoutPanel_Buttons01.TabIndex = 3;
			// 
			// button_Next
			// 
			this.button_Next.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.button_Next.Checked = false;
			this.button_Next.Location = new System.Drawing.Point(121, 350);
			this.button_Next.Margin = new System.Windows.Forms.Padding(0);
			this.button_Next.Name = "button_Next";
			this.button_Next.Size = new System.Drawing.Size(75, 25);
			this.button_Next.TabIndex = 13;
			this.button_Next.Text = "Next";
			this.button_Next.Click += new System.EventHandler(this.button_Next_Click);
			// 
			// button_Cancel
			// 
			this.button_Cancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.button_Cancel.Checked = false;
			this.button_Cancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.button_Cancel.Location = new System.Drawing.Point(41, 350);
			this.button_Cancel.Margin = new System.Windows.Forms.Padding(0);
			this.button_Cancel.Name = "button_Cancel";
			this.button_Cancel.Size = new System.Drawing.Size(75, 25);
			this.button_Cancel.TabIndex = 12;
			this.button_Cancel.Text = "Cancel";
			// 
			// tablessTabControl
			// 
			this.tablessTabControl.Controls.Add(this.tabPage_Basic);
			this.tablessTabControl.Controls.Add(this.tabPage_Extra);
			this.tablessTabControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tablessTabControl.Location = new System.Drawing.Point(0, 0);
			this.tablessTabControl.Name = "tablessTabControl";
			this.tablessTabControl.SelectedIndex = 0;
			this.tablessTabControl.Size = new System.Drawing.Size(784, 511);
			this.tablessTabControl.TabIndex = 3;
			// 
			// FormProjectSetup
			// 
			this.AcceptButton = this.button_Create;
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
			this.ClientSize = new System.Drawing.Size(784, 511);
			this.Controls.Add(this.tablessTabControl);
			this.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
			this.MaximizeBox = false;
			this.Name = "FormProjectSetup";
			this.ShowIcon = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "Create a new project...";
			this.panel_LevelsRadioChoice.ResumeLayout(false);
			this.panel_ScriptRadioChoice.ResumeLayout(false);
			this.tabPage_Extra.ResumeLayout(false);
			this.tableLayoutPanel_Main02.ResumeLayout(false);
			this.tableLayoutPanel_Buttons02.ResumeLayout(false);
			this.tableLayoutPanel_Content02.ResumeLayout(false);
			this.tableLayoutPanel_Content02.PerformLayout();
			this.tableLayoutPanel3.ResumeLayout(false);
			this.tabPage_Basic.ResumeLayout(false);
			this.tableLayoutPanel_Main01.ResumeLayout(false);
			this.tableLayoutPanel_Content01.ResumeLayout(false);
			this.tableLayoutPanel_Content01.PerformLayout();
			this.tableLayoutPanel_Title.ResumeLayout(false);
			this.tableLayoutPanel_Buttons01.ResumeLayout(false);
			this.tablessTabControl.ResumeLayout(false);
			this.ResumeLayout(false);

		}

		#endregion

		private DarkUI.Controls.DarkButton button_BrowseLevels;
		private DarkUI.Controls.DarkButton button_BrowseScript;
		private DarkUI.Controls.DarkButton button_Create;
		private DarkUI.Controls.DarkProgressBar progressBar;
		private DarkUI.Controls.DarkRadioButton radio_Levels_01;
		private DarkUI.Controls.DarkRadioButton radio_Levels_02;
		private DarkUI.Controls.DarkRadioButton radio_Script_01;
		private DarkUI.Controls.DarkRadioButton radio_Script_02;
		private DarkUI.Controls.DarkTextBox textBox_LevelsPath;
		private DarkUI.Controls.DarkTextBox textBox_ScriptPath;
		private System.Windows.Forms.Panel panel_LevelsRadioChoice;
		private System.Windows.Forms.Panel panel_ScriptRadioChoice;
		private System.Windows.Forms.TabPage tabPage_Extra;
		private System.Windows.Forms.TableLayoutPanel tableLayoutPanel_Main02;
		private System.Windows.Forms.TableLayoutPanel tableLayoutPanel_Buttons02;
		private DarkUI.Controls.DarkButton button_Back;
		private System.Windows.Forms.TableLayoutPanel tableLayoutPanel_Content02;
		private System.Windows.Forms.TableLayoutPanel tableLayoutPanel3;
		private DarkUI.Controls.DarkLabel label_SubTitle02;
		private DarkUI.Controls.DarkLabel label_Title02;
		private System.Windows.Forms.TabPage tabPage_Basic;
		private System.Windows.Forms.TableLayoutPanel tableLayoutPanel_Main01;
		private System.Windows.Forms.TableLayoutPanel tableLayoutPanel_Content01;
		private DarkUI.Controls.DarkButton button_BrowseProject;
		private DarkUI.Controls.DarkButton button_Help;
		private DarkUI.Controls.DarkTextBox textBox_ProjectPath;
		private DarkUI.Controls.DarkComboBox comboBox_EngineType;
		private DarkUI.Controls.DarkLabel label_01;
		private DarkUI.Controls.DarkLabel label_02;
		private DarkUI.Controls.DarkTextBox textBox_ProjectName;
		private DarkUI.Controls.DarkLabel label_03;
		private System.Windows.Forms.TableLayoutPanel tableLayoutPanel_Title;
		private DarkUI.Controls.DarkLabel label_SubTitle01;
		private DarkUI.Controls.DarkLabel label_Title01;
		private TombLib.Controls.DarkTabbedContainer tablessTabControl;
		private System.Windows.Forms.TableLayoutPanel tableLayoutPanel_Buttons01;
		private DarkUI.Controls.DarkButton button_Next;
		private DarkUI.Controls.DarkButton button_Cancel;
	}
}