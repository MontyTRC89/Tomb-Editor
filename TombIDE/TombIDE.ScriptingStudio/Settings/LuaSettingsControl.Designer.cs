namespace TombIDE.ScriptingStudio.Settings
{
	partial class LuaSettingsControl
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
			button_ImportScheme = new DarkUI.Controls.DarkButton();
			buttonContextMenu = new DarkUI.Controls.DarkContextMenu();
			menuItem_Bold = new System.Windows.Forms.ToolStripMenuItem();
			menuItem_Italic = new System.Windows.Forms.ToolStripMenuItem();
			checkBox_Autocomplete = new DarkUI.Controls.DarkCheckBox();
			checkBox_HighlightCurrentLine = new DarkUI.Controls.DarkCheckBox();
			checkBox_LineNumbers = new DarkUI.Controls.DarkCheckBox();
			checkBox_VisibleSpaces = new DarkUI.Controls.DarkCheckBox();
			checkBox_VisibleTabs = new DarkUI.Controls.DarkCheckBox();
			checkBox_WordWrapping = new DarkUI.Controls.DarkCheckBox();
			colorButton_Background = new DarkUI.Controls.DarkButton();
			colorButton_Foreground = new DarkUI.Controls.DarkButton();
			colorButton_Comments = new DarkUI.Controls.DarkButton();
			colorButton_SpecialOperators = new DarkUI.Controls.DarkButton();
			colorButton_Values = new DarkUI.Controls.DarkButton();
			colorButton_Statements = new DarkUI.Controls.DarkButton();
			colorButton_Operators = new DarkUI.Controls.DarkButton();
			colorDialog = new System.Windows.Forms.ColorDialog();
			darkLabel1 = new DarkUI.Controls.DarkLabel();
			darkLabel10 = new DarkUI.Controls.DarkLabel();
			darkLabel11 = new DarkUI.Controls.DarkLabel();
			darkLabel12 = new DarkUI.Controls.DarkLabel();
			darkLabel2 = new DarkUI.Controls.DarkLabel();
			darkLabel3 = new DarkUI.Controls.DarkLabel();
			darkLabel4 = new DarkUI.Controls.DarkLabel();
			darkLabel5 = new DarkUI.Controls.DarkLabel();
			darkLabel6 = new DarkUI.Controls.DarkLabel();
			darkLabel7 = new DarkUI.Controls.DarkLabel();
			darkLabel8 = new DarkUI.Controls.DarkLabel();
			elementHost = new System.Windows.Forms.Integration.ElementHost();
			groupBox_Colors = new DarkUI.Controls.DarkGroupBox();
			button_SaveScheme = new DarkUI.Controls.DarkButton();
			button_DeleteScheme = new DarkUI.Controls.DarkButton();
			button_OpenSchemesFolder = new DarkUI.Controls.DarkButton();
			comboBox_ColorSchemes = new DarkUI.Controls.DarkComboBox();
			groupBox_Preview = new DarkUI.Controls.DarkGroupBox();
			numeric_FontSize = new DarkUI.Controls.DarkNumericUpDown();
			numeric_UndoStackSize = new DarkUI.Controls.DarkNumericUpDown();
			sectionPanel = new DarkUI.Controls.DarkSectionPanel();
			checkBox_CloseParentheses = new DarkUI.Controls.DarkCheckBox();
			checkBox_CloseBraces = new DarkUI.Controls.DarkCheckBox();
			checkBox_CloseQuotes = new DarkUI.Controls.DarkCheckBox();
			checkBox_CloseBrackets = new DarkUI.Controls.DarkCheckBox();
			comboBox_FontFamily = new DarkUI.Controls.DarkComboBox();
			toolTip = new System.Windows.Forms.ToolTip(components);
			buttonContextMenu.SuspendLayout();
			groupBox_Colors.SuspendLayout();
			groupBox_Preview.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)numeric_FontSize).BeginInit();
			((System.ComponentModel.ISupportInitialize)numeric_UndoStackSize).BeginInit();
			sectionPanel.SuspendLayout();
			SuspendLayout();
			// 
			// button_ImportScheme
			// 
			button_ImportScheme.Checked = false;
			button_ImportScheme.Image = Properties.Resources.Import_16;
			button_ImportScheme.Location = new System.Drawing.Point(483, 16);
			button_ImportScheme.Margin = new System.Windows.Forms.Padding(3, 0, 3, 3);
			button_ImportScheme.Name = "button_ImportScheme";
			button_ImportScheme.Size = new System.Drawing.Size(25, 25);
			button_ImportScheme.TabIndex = 21;
			toolTip.SetToolTip(button_ImportScheme, "Import Scheme...");
			button_ImportScheme.Click += button_ImportScheme_Click;
			// 
			// buttonContextMenu
			// 
			buttonContextMenu.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
			buttonContextMenu.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
			buttonContextMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { menuItem_Bold, menuItem_Italic });
			buttonContextMenu.Name = "buttonContextMenu";
			buttonContextMenu.Size = new System.Drawing.Size(100, 48);
			buttonContextMenu.Opening += buttonContextMenu_Opening;
			// 
			// menuItem_Bold
			// 
			menuItem_Bold.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
			menuItem_Bold.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
			menuItem_Bold.Name = "menuItem_Bold";
			menuItem_Bold.Size = new System.Drawing.Size(99, 22);
			menuItem_Bold.Text = "Bold";
			menuItem_Bold.Click += menuItem_Bold_Click;
			// 
			// menuItem_Italic
			// 
			menuItem_Italic.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
			menuItem_Italic.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
			menuItem_Italic.Name = "menuItem_Italic";
			menuItem_Italic.Size = new System.Drawing.Size(99, 22);
			menuItem_Italic.Text = "Italic";
			menuItem_Italic.Click += menuItem_Italic_Click;
			// 
			// checkBox_Autocomplete
			// 
			checkBox_Autocomplete.AutoSize = true;
			checkBox_Autocomplete.Enabled = false;
			checkBox_Autocomplete.Location = new System.Drawing.Point(6, 166);
			checkBox_Autocomplete.Margin = new System.Windows.Forms.Padding(6, 6, 3, 0);
			checkBox_Autocomplete.Name = "checkBox_Autocomplete";
			checkBox_Autocomplete.Size = new System.Drawing.Size(30, 17);
			checkBox_Autocomplete.TabIndex = 6;
			checkBox_Autocomplete.Text = "-";
			// 
			// checkBox_HighlightCurrentLine
			// 
			checkBox_HighlightCurrentLine.AutoSize = true;
			checkBox_HighlightCurrentLine.Location = new System.Drawing.Point(6, 298);
			checkBox_HighlightCurrentLine.Margin = new System.Windows.Forms.Padding(3, 9, 3, 0);
			checkBox_HighlightCurrentLine.Name = "checkBox_HighlightCurrentLine";
			checkBox_HighlightCurrentLine.Size = new System.Drawing.Size(137, 17);
			checkBox_HighlightCurrentLine.TabIndex = 11;
			checkBox_HighlightCurrentLine.Text = "Highlight current line";
			checkBox_HighlightCurrentLine.CheckedChanged += VisiblePreviewSetting_Changed;
			// 
			// checkBox_LineNumbers
			// 
			checkBox_LineNumbers.AutoSize = true;
			checkBox_LineNumbers.Location = new System.Drawing.Point(6, 320);
			checkBox_LineNumbers.Margin = new System.Windows.Forms.Padding(3, 9, 3, 0);
			checkBox_LineNumbers.Name = "checkBox_LineNumbers";
			checkBox_LineNumbers.Size = new System.Drawing.Size(125, 17);
			checkBox_LineNumbers.TabIndex = 12;
			checkBox_LineNumbers.Text = "Show line numbers";
			checkBox_LineNumbers.CheckedChanged += VisiblePreviewSetting_Changed;
			// 
			// checkBox_VisibleSpaces
			// 
			checkBox_VisibleSpaces.AutoSize = true;
			checkBox_VisibleSpaces.Location = new System.Drawing.Point(6, 342);
			checkBox_VisibleSpaces.Margin = new System.Windows.Forms.Padding(3, 9, 3, 0);
			checkBox_VisibleSpaces.Name = "checkBox_VisibleSpaces";
			checkBox_VisibleSpaces.Size = new System.Drawing.Size(127, 17);
			checkBox_VisibleSpaces.TabIndex = 14;
			checkBox_VisibleSpaces.Text = "Show visible spaces";
			checkBox_VisibleSpaces.CheckedChanged += VisiblePreviewSetting_Changed;
			// 
			// checkBox_VisibleTabs
			// 
			checkBox_VisibleTabs.AutoSize = true;
			checkBox_VisibleTabs.Location = new System.Drawing.Point(6, 364);
			checkBox_VisibleTabs.Margin = new System.Windows.Forms.Padding(3, 0, 3, 0);
			checkBox_VisibleTabs.Name = "checkBox_VisibleTabs";
			checkBox_VisibleTabs.Size = new System.Drawing.Size(115, 17);
			checkBox_VisibleTabs.TabIndex = 15;
			checkBox_VisibleTabs.Text = "Show visible tabs";
			checkBox_VisibleTabs.CheckedChanged += VisiblePreviewSetting_Changed;
			// 
			// checkBox_WordWrapping
			// 
			checkBox_WordWrapping.AutoSize = true;
			checkBox_WordWrapping.Location = new System.Drawing.Point(6, 188);
			checkBox_WordWrapping.Margin = new System.Windows.Forms.Padding(3, 9, 3, 0);
			checkBox_WordWrapping.Name = "checkBox_WordWrapping";
			checkBox_WordWrapping.Size = new System.Drawing.Size(108, 17);
			checkBox_WordWrapping.TabIndex = 10;
			checkBox_WordWrapping.Text = "Word wrapping";
			checkBox_WordWrapping.CheckedChanged += VisiblePreviewSetting_Changed;
			// 
			// colorButton_Background
			// 
			colorButton_Background.BackColor = System.Drawing.Color.FromArgb(32, 32, 32);
			colorButton_Background.BackColorUseGeneric = false;
			colorButton_Background.Checked = false;
			colorButton_Background.Location = new System.Drawing.Point(370, 59);
			colorButton_Background.Margin = new System.Windows.Forms.Padding(6, 0, 9, 3);
			colorButton_Background.Name = "colorButton_Background";
			colorButton_Background.Size = new System.Drawing.Size(169, 25);
			colorButton_Background.TabIndex = 14;
			colorButton_Background.Click += button_Color_Click;
			// 
			// colorButton_Foreground
			// 
			colorButton_Foreground.BackColor = System.Drawing.Color.Gainsboro;
			colorButton_Foreground.BackColorUseGeneric = false;
			colorButton_Foreground.Checked = false;
			colorButton_Foreground.Location = new System.Drawing.Point(370, 100);
			colorButton_Foreground.Margin = new System.Windows.Forms.Padding(6, 0, 9, 3);
			colorButton_Foreground.Name = "colorButton_Foreground";
			colorButton_Foreground.Size = new System.Drawing.Size(169, 25);
			colorButton_Foreground.TabIndex = 16;
			colorButton_Foreground.Click += button_Color_Click;
			// 
			// colorButton_Comments
			// 
			colorButton_Comments.BackColor = System.Drawing.Color.Green;
			colorButton_Comments.BackColorUseGeneric = false;
			colorButton_Comments.Checked = false;
			colorButton_Comments.ContextMenuStrip = buttonContextMenu;
			colorButton_Comments.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
			colorButton_Comments.Location = new System.Drawing.Point(191, 100);
			colorButton_Comments.Margin = new System.Windows.Forms.Padding(3, 0, 3, 3);
			colorButton_Comments.Name = "colorButton_Comments";
			colorButton_Comments.Size = new System.Drawing.Size(170, 25);
			colorButton_Comments.TabIndex = 9;
			colorButton_Comments.UseForeColor = true;
			colorButton_Comments.Click += button_Color_Click;
			// 
			// colorButton_SpecialOperators
			// 
			colorButton_SpecialOperators.BackColor = System.Drawing.Color.Orchid;
			colorButton_SpecialOperators.BackColorUseGeneric = false;
			colorButton_SpecialOperators.Checked = false;
			colorButton_SpecialOperators.ContextMenuStrip = buttonContextMenu;
			colorButton_SpecialOperators.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
			colorButton_SpecialOperators.Location = new System.Drawing.Point(12, 141);
			colorButton_SpecialOperators.Margin = new System.Windows.Forms.Padding(9, 0, 3, 8);
			colorButton_SpecialOperators.Name = "colorButton_SpecialOperators";
			colorButton_SpecialOperators.Size = new System.Drawing.Size(170, 25);
			colorButton_SpecialOperators.TabIndex = 5;
			colorButton_SpecialOperators.UseForeColor = true;
			colorButton_SpecialOperators.Click += button_Color_Click;
			// 
			// colorButton_Values
			// 
			colorButton_Values.BackColor = System.Drawing.Color.SteelBlue;
			colorButton_Values.BackColorUseGeneric = false;
			colorButton_Values.Checked = false;
			colorButton_Values.ContextMenuStrip = buttonContextMenu;
			colorButton_Values.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
			colorButton_Values.Location = new System.Drawing.Point(12, 59);
			colorButton_Values.Margin = new System.Windows.Forms.Padding(9, 0, 3, 3);
			colorButton_Values.Name = "colorButton_Values";
			colorButton_Values.Size = new System.Drawing.Size(170, 25);
			colorButton_Values.TabIndex = 1;
			colorButton_Values.UseForeColor = true;
			colorButton_Values.Click += button_Color_Click;
			// 
			// colorButton_Statements
			// 
			colorButton_Statements.BackColor = System.Drawing.Color.MediumAquamarine;
			colorButton_Statements.BackColorUseGeneric = false;
			colorButton_Statements.Checked = false;
			colorButton_Statements.ContextMenuStrip = buttonContextMenu;
			colorButton_Statements.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
			colorButton_Statements.Location = new System.Drawing.Point(191, 59);
			colorButton_Statements.Margin = new System.Windows.Forms.Padding(6, 0, 3, 3);
			colorButton_Statements.Name = "colorButton_Statements";
			colorButton_Statements.Size = new System.Drawing.Size(170, 25);
			colorButton_Statements.TabIndex = 7;
			colorButton_Statements.UseForeColor = true;
			colorButton_Statements.Click += button_Color_Click;
			// 
			// colorButton_Operators
			// 
			colorButton_Operators.BackColor = System.Drawing.Color.LightSalmon;
			colorButton_Operators.BackColorUseGeneric = false;
			colorButton_Operators.Checked = false;
			colorButton_Operators.ContextMenuStrip = buttonContextMenu;
			colorButton_Operators.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
			colorButton_Operators.Location = new System.Drawing.Point(12, 100);
			colorButton_Operators.Margin = new System.Windows.Forms.Padding(3, 0, 3, 3);
			colorButton_Operators.Name = "colorButton_Operators";
			colorButton_Operators.Size = new System.Drawing.Size(170, 25);
			colorButton_Operators.TabIndex = 3;
			colorButton_Operators.UseForeColor = true;
			colorButton_Operators.Click += button_Color_Click;
			// 
			// colorDialog
			// 
			colorDialog.AnyColor = true;
			colorDialog.FullOpen = true;
			// 
			// darkLabel1
			// 
			darkLabel1.AutoSize = true;
			darkLabel1.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
			darkLabel1.Location = new System.Drawing.Point(7, 34);
			darkLabel1.Margin = new System.Windows.Forms.Padding(6, 9, 3, 0);
			darkLabel1.Name = "darkLabel1";
			darkLabel1.Size = new System.Drawing.Size(56, 13);
			darkLabel1.TabIndex = 0;
			darkLabel1.Text = "Font size:";
			// 
			// darkLabel10
			// 
			darkLabel10.AutoSize = true;
			darkLabel10.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
			darkLabel10.Location = new System.Drawing.Point(370, 46);
			darkLabel10.Margin = new System.Windows.Forms.Padding(3, 3, 3, 0);
			darkLabel10.Name = "darkLabel10";
			darkLabel10.Size = new System.Drawing.Size(72, 13);
			darkLabel10.TabIndex = 13;
			darkLabel10.Text = "Background:";
			// 
			// darkLabel11
			// 
			darkLabel11.AutoSize = true;
			darkLabel11.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
			darkLabel11.Location = new System.Drawing.Point(370, 87);
			darkLabel11.Name = "darkLabel11";
			darkLabel11.Size = new System.Drawing.Size(98, 13);
			darkLabel11.TabIndex = 15;
			darkLabel11.Text = "Normal text color:";
			// 
			// darkLabel12
			// 
			darkLabel12.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			darkLabel12.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
			darkLabel12.Location = new System.Drawing.Point(475, 17);
			darkLabel12.Name = "darkLabel12";
			darkLabel12.Size = new System.Drawing.Size(2, 23);
			darkLabel12.TabIndex = 20;
			// 
			// darkLabel2
			// 
			darkLabel2.AutoSize = true;
			darkLabel2.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
			darkLabel2.Location = new System.Drawing.Point(7, 76);
			darkLabel2.Margin = new System.Windows.Forms.Padding(6, 3, 3, 0);
			darkLabel2.Name = "darkLabel2";
			darkLabel2.Size = new System.Drawing.Size(67, 13);
			darkLabel2.TabIndex = 2;
			darkLabel2.Text = "Font family:";
			// 
			// darkLabel3
			// 
			darkLabel3.AutoSize = true;
			darkLabel3.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
			darkLabel3.Location = new System.Drawing.Point(7, 119);
			darkLabel3.Margin = new System.Windows.Forms.Padding(6, 3, 3, 0);
			darkLabel3.Name = "darkLabel3";
			darkLabel3.Size = new System.Drawing.Size(90, 13);
			darkLabel3.TabIndex = 4;
			darkLabel3.Text = "Undo stack size:";
			// 
			// darkLabel4
			// 
			darkLabel4.AutoSize = true;
			darkLabel4.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
			darkLabel4.Location = new System.Drawing.Point(12, 46);
			darkLabel4.Margin = new System.Windows.Forms.Padding(9, 3, 3, 0);
			darkLabel4.Name = "darkLabel4";
			darkLabel4.Size = new System.Drawing.Size(40, 13);
			darkLabel4.TabIndex = 0;
			darkLabel4.Text = "Values";
			// 
			// darkLabel5
			// 
			darkLabel5.AutoSize = true;
			darkLabel5.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
			darkLabel5.Location = new System.Drawing.Point(12, 87);
			darkLabel5.Name = "darkLabel5";
			darkLabel5.Size = new System.Drawing.Size(62, 13);
			darkLabel5.TabIndex = 2;
			darkLabel5.Text = "Operators:";
			// 
			// darkLabel6
			// 
			darkLabel6.AutoSize = true;
			darkLabel6.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
			darkLabel6.Location = new System.Drawing.Point(12, 128);
			darkLabel6.Name = "darkLabel6";
			darkLabel6.Size = new System.Drawing.Size(98, 13);
			darkLabel6.TabIndex = 4;
			darkLabel6.Text = "Special Operators";
			// 
			// darkLabel7
			// 
			darkLabel7.AutoSize = true;
			darkLabel7.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
			darkLabel7.Location = new System.Drawing.Point(190, 46);
			darkLabel7.Margin = new System.Windows.Forms.Padding(3, 3, 3, 0);
			darkLabel7.Name = "darkLabel7";
			darkLabel7.Size = new System.Drawing.Size(67, 13);
			darkLabel7.TabIndex = 6;
			darkLabel7.Text = "Statements:";
			// 
			// darkLabel8
			// 
			darkLabel8.AutoSize = true;
			darkLabel8.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
			darkLabel8.Location = new System.Drawing.Point(190, 87);
			darkLabel8.Name = "darkLabel8";
			darkLabel8.Size = new System.Drawing.Size(61, 13);
			darkLabel8.TabIndex = 8;
			darkLabel8.Text = "Comments";
			// 
			// elementHost
			// 
			elementHost.Dock = System.Windows.Forms.DockStyle.Fill;
			elementHost.Location = new System.Drawing.Point(3, 18);
			elementHost.Name = "elementHost";
			elementHost.Size = new System.Drawing.Size(545, 170);
			elementHost.TabIndex = 0;
			// 
			// groupBox_Colors
			// 
			groupBox_Colors.Controls.Add(button_ImportScheme);
			groupBox_Colors.Controls.Add(darkLabel12);
			groupBox_Colors.Controls.Add(button_SaveScheme);
			groupBox_Colors.Controls.Add(button_DeleteScheme);
			groupBox_Colors.Controls.Add(button_OpenSchemesFolder);
			groupBox_Colors.Controls.Add(darkLabel11);
			groupBox_Colors.Controls.Add(colorButton_Foreground);
			groupBox_Colors.Controls.Add(darkLabel10);
			groupBox_Colors.Controls.Add(colorButton_Background);
			groupBox_Colors.Controls.Add(comboBox_ColorSchemes);
			groupBox_Colors.Controls.Add(darkLabel8);
			groupBox_Colors.Controls.Add(darkLabel7);
			groupBox_Colors.Controls.Add(darkLabel6);
			groupBox_Colors.Controls.Add(darkLabel5);
			groupBox_Colors.Controls.Add(darkLabel4);
			groupBox_Colors.Controls.Add(colorButton_Statements);
			groupBox_Colors.Controls.Add(colorButton_Comments);
			groupBox_Colors.Controls.Add(colorButton_Values);
			groupBox_Colors.Controls.Add(colorButton_Operators);
			groupBox_Colors.Controls.Add(colorButton_SpecialOperators);
			groupBox_Colors.Enabled = false;
			groupBox_Colors.Location = new System.Drawing.Point(162, 28);
			groupBox_Colors.Margin = new System.Windows.Forms.Padding(3, 3, 6, 3);
			groupBox_Colors.Name = "groupBox_Colors";
			groupBox_Colors.Size = new System.Drawing.Size(551, 177);
			groupBox_Colors.TabIndex = 17;
			groupBox_Colors.TabStop = false;
			groupBox_Colors.Text = "Color schemes (Not implemented yet)";
			// 
			// button_SaveScheme
			// 
			button_SaveScheme.Checked = false;
			button_SaveScheme.Image = Properties.Resources.Save_16;
			button_SaveScheme.Location = new System.Drawing.Point(413, 16);
			button_SaveScheme.Name = "button_SaveScheme";
			button_SaveScheme.Size = new System.Drawing.Size(25, 25);
			button_SaveScheme.TabIndex = 19;
			toolTip.SetToolTip(button_SaveScheme, "Save Scheme As...");
			button_SaveScheme.Click += button_SaveScheme_Click;
			// 
			// button_DeleteScheme
			// 
			button_DeleteScheme.Checked = false;
			button_DeleteScheme.Image = Properties.Resources.Trash_16;
			button_DeleteScheme.Location = new System.Drawing.Point(444, 16);
			button_DeleteScheme.Name = "button_DeleteScheme";
			button_DeleteScheme.Size = new System.Drawing.Size(25, 25);
			button_DeleteScheme.TabIndex = 18;
			toolTip.SetToolTip(button_DeleteScheme, "Delete Scheme");
			button_DeleteScheme.Click += button_DeleteScheme_Click;
			// 
			// button_OpenSchemesFolder
			// 
			button_OpenSchemesFolder.Checked = false;
			button_OpenSchemesFolder.Image = Properties.Resources.ForwardArrow_16;
			button_OpenSchemesFolder.Location = new System.Drawing.Point(514, 16);
			button_OpenSchemesFolder.Margin = new System.Windows.Forms.Padding(3, 0, 3, 3);
			button_OpenSchemesFolder.Name = "button_OpenSchemesFolder";
			button_OpenSchemesFolder.Size = new System.Drawing.Size(25, 25);
			button_OpenSchemesFolder.TabIndex = 17;
			toolTip.SetToolTip(button_OpenSchemesFolder, "Open Schemes Folder");
			button_OpenSchemesFolder.Click += button_OpenSchemesFolder_Click;
			// 
			// comboBox_ColorSchemes
			// 
			comboBox_ColorSchemes.FormattingEnabled = true;
			comboBox_ColorSchemes.Location = new System.Drawing.Point(12, 19);
			comboBox_ColorSchemes.Margin = new System.Windows.Forms.Padding(9, 3, 3, 3);
			comboBox_ColorSchemes.Name = "comboBox_ColorSchemes";
			comboBox_ColorSchemes.Size = new System.Drawing.Size(395, 23);
			comboBox_ColorSchemes.TabIndex = 12;
			comboBox_ColorSchemes.SelectedIndexChanged += comboBox_ColorSchemes_SelectedIndexChanged;
			// 
			// groupBox_Preview
			// 
			groupBox_Preview.Controls.Add(elementHost);
			groupBox_Preview.Location = new System.Drawing.Point(162, 214);
			groupBox_Preview.Margin = new System.Windows.Forms.Padding(3, 6, 6, 6);
			groupBox_Preview.Name = "groupBox_Preview";
			groupBox_Preview.Size = new System.Drawing.Size(551, 191);
			groupBox_Preview.TabIndex = 2;
			groupBox_Preview.TabStop = false;
			groupBox_Preview.Text = "Preview";
			// 
			// numeric_FontSize
			// 
			numeric_FontSize.IncrementAlternate = new decimal(new int[] { 10, 0, 0, 65536 });
			numeric_FontSize.Location = new System.Drawing.Point(6, 50);
			numeric_FontSize.LoopValues = false;
			numeric_FontSize.Margin = new System.Windows.Forms.Padding(6, 3, 3, 3);
			numeric_FontSize.Maximum = new decimal(new int[] { 32, 0, 0, 0 });
			numeric_FontSize.Minimum = new decimal(new int[] { 4, 0, 0, 0 });
			numeric_FontSize.Name = "numeric_FontSize";
			numeric_FontSize.Size = new System.Drawing.Size(150, 22);
			numeric_FontSize.TabIndex = 1;
			numeric_FontSize.Value = new decimal(new int[] { 12, 0, 0, 0 });
			numeric_FontSize.ValueChanged += VisiblePreviewSetting_Changed;
			// 
			// numeric_UndoStackSize
			// 
			numeric_UndoStackSize.IncrementAlternate = new decimal(new int[] { 10, 0, 0, 65536 });
			numeric_UndoStackSize.Location = new System.Drawing.Point(6, 135);
			numeric_UndoStackSize.LoopValues = false;
			numeric_UndoStackSize.Margin = new System.Windows.Forms.Padding(6, 3, 3, 3);
			numeric_UndoStackSize.Maximum = new decimal(new int[] { 1024, 0, 0, 0 });
			numeric_UndoStackSize.Minimum = new decimal(new int[] { 16, 0, 0, 0 });
			numeric_UndoStackSize.Name = "numeric_UndoStackSize";
			numeric_UndoStackSize.Size = new System.Drawing.Size(150, 22);
			numeric_UndoStackSize.TabIndex = 5;
			numeric_UndoStackSize.Value = new decimal(new int[] { 256, 0, 0, 0 });
			// 
			// sectionPanel
			// 
			sectionPanel.Controls.Add(checkBox_CloseParentheses);
			sectionPanel.Controls.Add(checkBox_CloseBraces);
			sectionPanel.Controls.Add(checkBox_CloseQuotes);
			sectionPanel.Controls.Add(checkBox_CloseBrackets);
			sectionPanel.Controls.Add(groupBox_Preview);
			sectionPanel.Controls.Add(checkBox_HighlightCurrentLine);
			sectionPanel.Controls.Add(checkBox_VisibleTabs);
			sectionPanel.Controls.Add(checkBox_VisibleSpaces);
			sectionPanel.Controls.Add(checkBox_LineNumbers);
			sectionPanel.Controls.Add(darkLabel3);
			sectionPanel.Controls.Add(darkLabel2);
			sectionPanel.Controls.Add(darkLabel1);
			sectionPanel.Controls.Add(numeric_UndoStackSize);
			sectionPanel.Controls.Add(checkBox_Autocomplete);
			sectionPanel.Controls.Add(checkBox_WordWrapping);
			sectionPanel.Controls.Add(groupBox_Colors);
			sectionPanel.Controls.Add(comboBox_FontFamily);
			sectionPanel.Controls.Add(numeric_FontSize);
			sectionPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			sectionPanel.Location = new System.Drawing.Point(0, 0);
			sectionPanel.Name = "sectionPanel";
			sectionPanel.SectionHeader = "Lua";
			sectionPanel.Size = new System.Drawing.Size(720, 412);
			sectionPanel.TabIndex = 0;
			// 
			// checkBox_CloseParentheses
			// 
			checkBox_CloseParentheses.Location = new System.Drawing.Point(6, 210);
			checkBox_CloseParentheses.Margin = new System.Windows.Forms.Padding(3, 9, 3, 0);
			checkBox_CloseParentheses.Name = "checkBox_CloseParentheses";
			checkBox_CloseParentheses.Size = new System.Drawing.Size(150, 17);
			checkBox_CloseParentheses.TabIndex = 21;
			checkBox_CloseParentheses.Text = "Auto close parentheses ( )";
			// 
			// checkBox_CloseBraces
			// 
			checkBox_CloseBraces.AutoSize = true;
			checkBox_CloseBraces.Location = new System.Drawing.Point(6, 254);
			checkBox_CloseBraces.Margin = new System.Windows.Forms.Padding(3, 0, 3, 0);
			checkBox_CloseBraces.Name = "checkBox_CloseBraces";
			checkBox_CloseBraces.Size = new System.Drawing.Size(128, 17);
			checkBox_CloseBraces.TabIndex = 20;
			checkBox_CloseBraces.Text = "Auto close braces { }";
			// 
			// checkBox_CloseQuotes
			// 
			checkBox_CloseQuotes.AutoSize = true;
			checkBox_CloseQuotes.Location = new System.Drawing.Point(6, 276);
			checkBox_CloseQuotes.Margin = new System.Windows.Forms.Padding(3, 0, 3, 0);
			checkBox_CloseQuotes.Name = "checkBox_CloseQuotes";
			checkBox_CloseQuotes.Size = new System.Drawing.Size(133, 17);
			checkBox_CloseQuotes.TabIndex = 19;
			checkBox_CloseQuotes.Text = "Auto close quotes \" \"";
			// 
			// checkBox_CloseBrackets
			// 
			checkBox_CloseBrackets.AutoSize = true;
			checkBox_CloseBrackets.Location = new System.Drawing.Point(6, 232);
			checkBox_CloseBrackets.Margin = new System.Windows.Forms.Padding(3, 9, 3, 0);
			checkBox_CloseBrackets.Name = "checkBox_CloseBrackets";
			checkBox_CloseBrackets.Size = new System.Drawing.Size(138, 17);
			checkBox_CloseBrackets.TabIndex = 18;
			checkBox_CloseBrackets.Text = "Auto close brackets [ ]";
			// 
			// comboBox_FontFamily
			// 
			comboBox_FontFamily.FormattingEnabled = true;
			comboBox_FontFamily.Location = new System.Drawing.Point(6, 92);
			comboBox_FontFamily.Margin = new System.Windows.Forms.Padding(6, 3, 3, 3);
			comboBox_FontFamily.Name = "comboBox_FontFamily";
			comboBox_FontFamily.Size = new System.Drawing.Size(150, 23);
			comboBox_FontFamily.TabIndex = 3;
			comboBox_FontFamily.SelectedIndexChanged += comboBox_FontFamily_SelectedIndexChanged;
			// 
			// LuaSettingsControl
			// 
			AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			BackColor = System.Drawing.Color.FromArgb(63, 65, 69);
			Controls.Add(sectionPanel);
			Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
			MaximumSize = new System.Drawing.Size(720, 412);
			MinimumSize = new System.Drawing.Size(720, 412);
			Name = "LuaSettingsControl";
			Size = new System.Drawing.Size(720, 412);
			buttonContextMenu.ResumeLayout(false);
			groupBox_Colors.ResumeLayout(false);
			groupBox_Colors.PerformLayout();
			groupBox_Preview.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)numeric_FontSize).EndInit();
			((System.ComponentModel.ISupportInitialize)numeric_UndoStackSize).EndInit();
			sectionPanel.ResumeLayout(false);
			sectionPanel.PerformLayout();
			ResumeLayout(false);
		}

		#endregion

		private DarkUI.Controls.DarkButton button_DeleteScheme;
		private DarkUI.Controls.DarkButton button_ImportScheme;
		private DarkUI.Controls.DarkButton button_OpenSchemesFolder;
		private DarkUI.Controls.DarkButton button_SaveScheme;
		private DarkUI.Controls.DarkButton colorButton_Background;
		private DarkUI.Controls.DarkButton colorButton_Foreground;
		private DarkUI.Controls.DarkButton colorButton_Comments;
		private DarkUI.Controls.DarkButton colorButton_SpecialOperators;
		private DarkUI.Controls.DarkButton colorButton_Values;
		private DarkUI.Controls.DarkButton colorButton_Statements;
		private DarkUI.Controls.DarkButton colorButton_Operators;
		private DarkUI.Controls.DarkCheckBox checkBox_Autocomplete;
		private DarkUI.Controls.DarkCheckBox checkBox_HighlightCurrentLine;
		private DarkUI.Controls.DarkCheckBox checkBox_LineNumbers;
		private DarkUI.Controls.DarkCheckBox checkBox_VisibleSpaces;
		private DarkUI.Controls.DarkCheckBox checkBox_VisibleTabs;
		private DarkUI.Controls.DarkCheckBox checkBox_WordWrapping;
		private DarkUI.Controls.DarkComboBox comboBox_ColorSchemes;
		private DarkUI.Controls.DarkComboBox comboBox_FontFamily;
		private DarkUI.Controls.DarkContextMenu buttonContextMenu;
		private DarkUI.Controls.DarkGroupBox groupBox_Colors;
		private DarkUI.Controls.DarkGroupBox groupBox_Preview;
		private DarkUI.Controls.DarkLabel darkLabel1;
		private DarkUI.Controls.DarkLabel darkLabel10;
		private DarkUI.Controls.DarkLabel darkLabel11;
		private DarkUI.Controls.DarkLabel darkLabel12;
		private DarkUI.Controls.DarkLabel darkLabel2;
		private DarkUI.Controls.DarkLabel darkLabel3;
		private DarkUI.Controls.DarkLabel darkLabel4;
		private DarkUI.Controls.DarkLabel darkLabel5;
		private DarkUI.Controls.DarkLabel darkLabel6;
		private DarkUI.Controls.DarkLabel darkLabel7;
		private DarkUI.Controls.DarkLabel darkLabel8;
		private DarkUI.Controls.DarkNumericUpDown numeric_FontSize;
		private DarkUI.Controls.DarkNumericUpDown numeric_UndoStackSize;
		private DarkUI.Controls.DarkSectionPanel sectionPanel;
		private System.Windows.Forms.ColorDialog colorDialog;
		private System.Windows.Forms.Integration.ElementHost elementHost;
		private System.Windows.Forms.ToolStripMenuItem menuItem_Bold;
		private System.Windows.Forms.ToolStripMenuItem menuItem_Italic;
		private System.Windows.Forms.ToolTip toolTip;
		private DarkUI.Controls.DarkCheckBox checkBox_CloseBraces;
		private DarkUI.Controls.DarkCheckBox checkBox_CloseQuotes;
		private DarkUI.Controls.DarkCheckBox checkBox_CloseBrackets;
		private DarkUI.Controls.DarkCheckBox checkBox_CloseParentheses;
	}
}
