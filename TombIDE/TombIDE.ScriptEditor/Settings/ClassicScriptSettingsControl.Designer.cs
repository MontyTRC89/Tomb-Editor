namespace TombIDE.ScriptEditor.Settings
{
	partial class ClassicScriptSettingsControl
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
			this.button_ImportScheme = new DarkUI.Controls.DarkButton();
			this.buttonContextMenu = new DarkUI.Controls.DarkContextMenu();
			this.menuItem_Bold = new System.Windows.Forms.ToolStripMenuItem();
			this.menuItem_Italic = new System.Windows.Forms.ToolStripMenuItem();
			this.checkBox_Autocomplete = new DarkUI.Controls.DarkCheckBox();
			this.checkBox_CloseBrackets = new DarkUI.Controls.DarkCheckBox();
			this.checkBox_CloseQuotes = new DarkUI.Controls.DarkCheckBox();
			this.checkBox_HighlightCurrentLine = new DarkUI.Controls.DarkCheckBox();
			this.checkBox_LineNumbers = new DarkUI.Controls.DarkCheckBox();
			this.checkBox_LiveErrors = new DarkUI.Controls.DarkCheckBox();
			this.checkBox_PostCommaSpace = new DarkUI.Controls.DarkCheckBox();
			this.checkBox_PostEqualSpace = new DarkUI.Controls.DarkCheckBox();
			this.checkBox_PreCommaSpace = new DarkUI.Controls.DarkCheckBox();
			this.checkBox_PreEqualSpace = new DarkUI.Controls.DarkCheckBox();
			this.checkBox_ReduceSpaces = new DarkUI.Controls.DarkCheckBox();
			this.checkBox_SectionSeparators = new DarkUI.Controls.DarkCheckBox();
			this.checkBox_VisibleSpaces = new DarkUI.Controls.DarkCheckBox();
			this.checkBox_VisibleTabs = new DarkUI.Controls.DarkCheckBox();
			this.checkBox_WordWrapping = new DarkUI.Controls.DarkCheckBox();
			this.colorButton_Background = new DarkUI.Controls.DarkButton();
			this.colorButton_Comments = new DarkUI.Controls.DarkButton();
			this.colorButton_Foreground = new DarkUI.Controls.DarkButton();
			this.colorButton_NewCommands = new DarkUI.Controls.DarkButton();
			this.colorButton_References = new DarkUI.Controls.DarkButton();
			this.colorButton_Sections = new DarkUI.Controls.DarkButton();
			this.colorButton_StandardCommands = new DarkUI.Controls.DarkButton();
			this.colorButton_Values = new DarkUI.Controls.DarkButton();
			this.colorDialog = new System.Windows.Forms.ColorDialog();
			this.darkLabel1 = new DarkUI.Controls.DarkLabel();
			this.darkLabel10 = new DarkUI.Controls.DarkLabel();
			this.darkLabel11 = new DarkUI.Controls.DarkLabel();
			this.darkLabel12 = new DarkUI.Controls.DarkLabel();
			this.darkLabel2 = new DarkUI.Controls.DarkLabel();
			this.darkLabel3 = new DarkUI.Controls.DarkLabel();
			this.darkLabel4 = new DarkUI.Controls.DarkLabel();
			this.darkLabel5 = new DarkUI.Controls.DarkLabel();
			this.darkLabel6 = new DarkUI.Controls.DarkLabel();
			this.darkLabel7 = new DarkUI.Controls.DarkLabel();
			this.darkLabel8 = new DarkUI.Controls.DarkLabel();
			this.darkLabel9 = new DarkUI.Controls.DarkLabel();
			this.elementHost = new System.Windows.Forms.Integration.ElementHost();
			this.groupBox_AddSpaces = new DarkUI.Controls.DarkGroupBox();
			this.label_Comma = new DarkUI.Controls.DarkLabel();
			this.label_Equal = new DarkUI.Controls.DarkLabel();
			this.groupBox_Colors = new DarkUI.Controls.DarkGroupBox();
			this.groupBox_Identation = new DarkUI.Controls.DarkGroupBox();
			this.groupBox_Preview = new DarkUI.Controls.DarkGroupBox();
			this.numeric_FontSize = new DarkUI.Controls.DarkNumericUpDown();
			this.numeric_UndoStackSize = new DarkUI.Controls.DarkNumericUpDown();
			this.sectionPanel = new DarkUI.Controls.DarkSectionPanel();
			this.toolTip = new System.Windows.Forms.ToolTip(this.components);
			this.button_SaveScheme = new DarkUI.Controls.DarkButton();
			this.button_DeleteScheme = new DarkUI.Controls.DarkButton();
			this.button_OpenSchemesFolder = new DarkUI.Controls.DarkButton();
			this.comboBox_ColorSchemes = new DarkUI.Controls.DarkComboBox();
			this.comboBox_FontFamily = new DarkUI.Controls.DarkComboBox();
			this.buttonContextMenu.SuspendLayout();
			this.groupBox_AddSpaces.SuspendLayout();
			this.groupBox_Colors.SuspendLayout();
			this.groupBox_Identation.SuspendLayout();
			this.groupBox_Preview.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.numeric_FontSize)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.numeric_UndoStackSize)).BeginInit();
			this.sectionPanel.SuspendLayout();
			this.SuspendLayout();
			// 
			// button_ImportScheme
			// 
			this.button_ImportScheme.Checked = false;
			this.button_ImportScheme.Image = global::TombIDE.ScriptEditor.Properties.Resources.Import_16;
			this.button_ImportScheme.Location = new System.Drawing.Point(483, 16);
			this.button_ImportScheme.Margin = new System.Windows.Forms.Padding(3, 0, 3, 3);
			this.button_ImportScheme.Name = "button_ImportScheme";
			this.button_ImportScheme.Size = new System.Drawing.Size(25, 25);
			this.button_ImportScheme.TabIndex = 21;
			this.toolTip.SetToolTip(this.button_ImportScheme, "Import Scheme...");
			this.button_ImportScheme.Click += new System.EventHandler(this.button_ImportScheme_Click);
			// 
			// buttonContextMenu
			// 
			this.buttonContextMenu.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
			this.buttonContextMenu.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
			this.buttonContextMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.menuItem_Bold,
            this.menuItem_Italic});
			this.buttonContextMenu.Name = "buttonContextMenu";
			this.buttonContextMenu.Size = new System.Drawing.Size(100, 48);
			this.buttonContextMenu.Opening += new System.ComponentModel.CancelEventHandler(this.buttonContextMenu_Opening);
			// 
			// menuItem_Bold
			// 
			this.menuItem_Bold.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
			this.menuItem_Bold.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
			this.menuItem_Bold.Name = "menuItem_Bold";
			this.menuItem_Bold.Size = new System.Drawing.Size(99, 22);
			this.menuItem_Bold.Text = "Bold";
			this.menuItem_Bold.Click += new System.EventHandler(this.menuItem_Bold_Click);
			// 
			// menuItem_Italic
			// 
			this.menuItem_Italic.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
			this.menuItem_Italic.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
			this.menuItem_Italic.Name = "menuItem_Italic";
			this.menuItem_Italic.Size = new System.Drawing.Size(99, 22);
			this.menuItem_Italic.Text = "Italic";
			this.menuItem_Italic.Click += new System.EventHandler(this.menuItem_Italic_Click);
			// 
			// checkBox_Autocomplete
			// 
			this.checkBox_Autocomplete.AutoSize = true;
			this.checkBox_Autocomplete.Location = new System.Drawing.Point(6, 164);
			this.checkBox_Autocomplete.Margin = new System.Windows.Forms.Padding(6, 6, 3, 0);
			this.checkBox_Autocomplete.Name = "checkBox_Autocomplete";
			this.checkBox_Autocomplete.Size = new System.Drawing.Size(126, 17);
			this.checkBox_Autocomplete.TabIndex = 6;
			this.checkBox_Autocomplete.Text = "Enable autocomplete";
			// 
			// checkBox_CloseBrackets
			// 
			this.checkBox_CloseBrackets.AutoSize = true;
			this.checkBox_CloseBrackets.Location = new System.Drawing.Point(6, 207);
			this.checkBox_CloseBrackets.Margin = new System.Windows.Forms.Padding(3, 9, 3, 0);
			this.checkBox_CloseBrackets.Name = "checkBox_CloseBrackets";
			this.checkBox_CloseBrackets.Size = new System.Drawing.Size(132, 17);
			this.checkBox_CloseBrackets.TabIndex = 8;
			this.checkBox_CloseBrackets.Text = "Auto close brackets [ ]";
			// 
			// checkBox_CloseQuotes
			// 
			this.checkBox_CloseQuotes.AutoSize = true;
			this.checkBox_CloseQuotes.Location = new System.Drawing.Point(6, 224);
			this.checkBox_CloseQuotes.Margin = new System.Windows.Forms.Padding(3, 0, 3, 0);
			this.checkBox_CloseQuotes.Name = "checkBox_CloseQuotes";
			this.checkBox_CloseQuotes.Size = new System.Drawing.Size(127, 17);
			this.checkBox_CloseQuotes.TabIndex = 9;
			this.checkBox_CloseQuotes.Text = "Auto close quotes \" \"";
			// 
			// checkBox_HighlightCurrentLine
			// 
			this.checkBox_HighlightCurrentLine.AutoSize = true;
			this.checkBox_HighlightCurrentLine.Location = new System.Drawing.Point(6, 276);
			this.checkBox_HighlightCurrentLine.Margin = new System.Windows.Forms.Padding(3, 9, 3, 0);
			this.checkBox_HighlightCurrentLine.Name = "checkBox_HighlightCurrentLine";
			this.checkBox_HighlightCurrentLine.Size = new System.Drawing.Size(122, 17);
			this.checkBox_HighlightCurrentLine.TabIndex = 11;
			this.checkBox_HighlightCurrentLine.Text = "Highlight current line";
			this.checkBox_HighlightCurrentLine.CheckedChanged += new System.EventHandler(this.VisiblePreviewSetting_Changed);
			// 
			// checkBox_LineNumbers
			// 
			this.checkBox_LineNumbers.AutoSize = true;
			this.checkBox_LineNumbers.Location = new System.Drawing.Point(6, 302);
			this.checkBox_LineNumbers.Margin = new System.Windows.Forms.Padding(3, 9, 3, 0);
			this.checkBox_LineNumbers.Name = "checkBox_LineNumbers";
			this.checkBox_LineNumbers.Size = new System.Drawing.Size(115, 17);
			this.checkBox_LineNumbers.TabIndex = 12;
			this.checkBox_LineNumbers.Text = "Show line numbers";
			this.checkBox_LineNumbers.CheckedChanged += new System.EventHandler(this.VisiblePreviewSetting_Changed);
			// 
			// checkBox_LiveErrors
			// 
			this.checkBox_LiveErrors.AutoSize = true;
			this.checkBox_LiveErrors.Location = new System.Drawing.Point(6, 181);
			this.checkBox_LiveErrors.Margin = new System.Windows.Forms.Padding(3, 0, 3, 0);
			this.checkBox_LiveErrors.Name = "checkBox_LiveErrors";
			this.checkBox_LiveErrors.Size = new System.Drawing.Size(124, 17);
			this.checkBox_LiveErrors.TabIndex = 7;
			this.checkBox_LiveErrors.Text = "Live error underlining";
			this.checkBox_LiveErrors.CheckedChanged += new System.EventHandler(this.VisiblePreviewSetting_Changed);
			// 
			// checkBox_PostCommaSpace
			// 
			this.checkBox_PostCommaSpace.AutoSize = true;
			this.checkBox_PostCommaSpace.Location = new System.Drawing.Point(118, 77);
			this.checkBox_PostCommaSpace.Margin = new System.Windows.Forms.Padding(0, 0, 40, 26);
			this.checkBox_PostCommaSpace.Name = "checkBox_PostCommaSpace";
			this.checkBox_PostCommaSpace.Size = new System.Drawing.Size(15, 14);
			this.checkBox_PostCommaSpace.TabIndex = 3;
			this.checkBox_PostCommaSpace.CheckedChanged += new System.EventHandler(this.checkBox_PostCommaSpace_CheckedChanged);
			// 
			// checkBox_PostEqualSpace
			// 
			this.checkBox_PostEqualSpace.AutoSize = true;
			this.checkBox_PostEqualSpace.Location = new System.Drawing.Point(118, 36);
			this.checkBox_PostEqualSpace.Margin = new System.Windows.Forms.Padding(0, 20, 40, 0);
			this.checkBox_PostEqualSpace.Name = "checkBox_PostEqualSpace";
			this.checkBox_PostEqualSpace.Size = new System.Drawing.Size(15, 14);
			this.checkBox_PostEqualSpace.TabIndex = 1;
			this.checkBox_PostEqualSpace.CheckedChanged += new System.EventHandler(this.checkBox_PostEqualSpace_CheckedChanged);
			// 
			// checkBox_PreCommaSpace
			// 
			this.checkBox_PreCommaSpace.AutoSize = true;
			this.checkBox_PreCommaSpace.Location = new System.Drawing.Point(43, 77);
			this.checkBox_PreCommaSpace.Margin = new System.Windows.Forms.Padding(40, 0, 0, 26);
			this.checkBox_PreCommaSpace.Name = "checkBox_PreCommaSpace";
			this.checkBox_PreCommaSpace.Size = new System.Drawing.Size(15, 14);
			this.checkBox_PreCommaSpace.TabIndex = 2;
			this.checkBox_PreCommaSpace.CheckedChanged += new System.EventHandler(this.checkBox_PreCommaSpace_CheckedChanged);
			// 
			// checkBox_PreEqualSpace
			// 
			this.checkBox_PreEqualSpace.AutoSize = true;
			this.checkBox_PreEqualSpace.Location = new System.Drawing.Point(43, 36);
			this.checkBox_PreEqualSpace.Margin = new System.Windows.Forms.Padding(40, 20, 0, 0);
			this.checkBox_PreEqualSpace.Name = "checkBox_PreEqualSpace";
			this.checkBox_PreEqualSpace.Size = new System.Drawing.Size(15, 14);
			this.checkBox_PreEqualSpace.TabIndex = 0;
			this.checkBox_PreEqualSpace.CheckedChanged += new System.EventHandler(this.checkBox_PreEqualSpace_CheckedChanged);
			// 
			// checkBox_ReduceSpaces
			// 
			this.checkBox_ReduceSpaces.AutoSize = true;
			this.checkBox_ReduceSpaces.Location = new System.Drawing.Point(12, 151);
			this.checkBox_ReduceSpaces.Margin = new System.Windows.Forms.Padding(9, 6, 3, 6);
			this.checkBox_ReduceSpaces.Name = "checkBox_ReduceSpaces";
			this.checkBox_ReduceSpaces.Size = new System.Drawing.Size(169, 17);
			this.checkBox_ReduceSpaces.TabIndex = 1;
			this.checkBox_ReduceSpaces.Text = "Reduce the amount of spaces";
			this.checkBox_ReduceSpaces.CheckedChanged += new System.EventHandler(this.checkBox_ReduceSpaces_CheckedChanged);
			// 
			// checkBox_SectionSeparators
			// 
			this.checkBox_SectionSeparators.AutoSize = true;
			this.checkBox_SectionSeparators.Location = new System.Drawing.Point(6, 319);
			this.checkBox_SectionSeparators.Margin = new System.Windows.Forms.Padding(3, 0, 3, 0);
			this.checkBox_SectionSeparators.Name = "checkBox_SectionSeparators";
			this.checkBox_SectionSeparators.Size = new System.Drawing.Size(142, 17);
			this.checkBox_SectionSeparators.TabIndex = 13;
			this.checkBox_SectionSeparators.Text = "Show section separators";
			this.checkBox_SectionSeparators.CheckedChanged += new System.EventHandler(this.VisiblePreviewSetting_Changed);
			// 
			// checkBox_VisibleSpaces
			// 
			this.checkBox_VisibleSpaces.AutoSize = true;
			this.checkBox_VisibleSpaces.Location = new System.Drawing.Point(6, 345);
			this.checkBox_VisibleSpaces.Margin = new System.Windows.Forms.Padding(3, 9, 3, 0);
			this.checkBox_VisibleSpaces.Name = "checkBox_VisibleSpaces";
			this.checkBox_VisibleSpaces.Size = new System.Drawing.Size(122, 17);
			this.checkBox_VisibleSpaces.TabIndex = 14;
			this.checkBox_VisibleSpaces.Text = "Show visible spaces";
			this.checkBox_VisibleSpaces.CheckedChanged += new System.EventHandler(this.VisiblePreviewSetting_Changed);
			// 
			// checkBox_VisibleTabs
			// 
			this.checkBox_VisibleTabs.AutoSize = true;
			this.checkBox_VisibleTabs.Location = new System.Drawing.Point(6, 362);
			this.checkBox_VisibleTabs.Margin = new System.Windows.Forms.Padding(3, 0, 3, 0);
			this.checkBox_VisibleTabs.Name = "checkBox_VisibleTabs";
			this.checkBox_VisibleTabs.Size = new System.Drawing.Size(108, 17);
			this.checkBox_VisibleTabs.TabIndex = 15;
			this.checkBox_VisibleTabs.Text = "Show visible tabs";
			this.checkBox_VisibleTabs.CheckedChanged += new System.EventHandler(this.VisiblePreviewSetting_Changed);
			// 
			// checkBox_WordWrapping
			// 
			this.checkBox_WordWrapping.AutoSize = true;
			this.checkBox_WordWrapping.Location = new System.Drawing.Point(6, 250);
			this.checkBox_WordWrapping.Margin = new System.Windows.Forms.Padding(3, 9, 3, 0);
			this.checkBox_WordWrapping.Name = "checkBox_WordWrapping";
			this.checkBox_WordWrapping.Size = new System.Drawing.Size(98, 17);
			this.checkBox_WordWrapping.TabIndex = 10;
			this.checkBox_WordWrapping.Text = "Word wrapping";
			this.checkBox_WordWrapping.CheckedChanged += new System.EventHandler(this.VisiblePreviewSetting_Changed);
			// 
			// colorButton_Background
			// 
			this.colorButton_Background.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(32)))), ((int)(((byte)(32)))), ((int)(((byte)(32)))));
			this.colorButton_Background.BackColorUseGeneric = false;
			this.colorButton_Background.Checked = false;
			this.colorButton_Background.Location = new System.Drawing.Point(370, 59);
			this.colorButton_Background.Margin = new System.Windows.Forms.Padding(6, 0, 9, 3);
			this.colorButton_Background.Name = "colorButton_Background";
			this.colorButton_Background.Size = new System.Drawing.Size(169, 25);
			this.colorButton_Background.TabIndex = 14;
			this.colorButton_Background.Click += new System.EventHandler(this.button_Color_Click);
			// 
			// colorButton_Comments
			// 
			this.colorButton_Comments.BackColor = System.Drawing.Color.Green;
			this.colorButton_Comments.BackColorUseGeneric = false;
			this.colorButton_Comments.Checked = false;
			this.colorButton_Comments.ContextMenuStrip = this.buttonContextMenu;
			this.colorButton_Comments.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
			this.colorButton_Comments.Location = new System.Drawing.Point(191, 141);
			this.colorButton_Comments.Margin = new System.Windows.Forms.Padding(3, 0, 3, 3);
			this.colorButton_Comments.Name = "colorButton_Comments";
			this.colorButton_Comments.Size = new System.Drawing.Size(170, 25);
			this.colorButton_Comments.TabIndex = 11;
			this.colorButton_Comments.UseForeColor = true;
			this.colorButton_Comments.Click += new System.EventHandler(this.button_Color_Click);
			// 
			// colorButton_Foreground
			// 
			this.colorButton_Foreground.BackColor = System.Drawing.Color.Gainsboro;
			this.colorButton_Foreground.BackColorUseGeneric = false;
			this.colorButton_Foreground.Checked = false;
			this.colorButton_Foreground.Location = new System.Drawing.Point(370, 100);
			this.colorButton_Foreground.Margin = new System.Windows.Forms.Padding(6, 0, 9, 3);
			this.colorButton_Foreground.Name = "colorButton_Foreground";
			this.colorButton_Foreground.Size = new System.Drawing.Size(169, 25);
			this.colorButton_Foreground.TabIndex = 16;
			this.colorButton_Foreground.Click += new System.EventHandler(this.button_Color_Click);
			// 
			// colorButton_NewCommands
			// 
			this.colorButton_NewCommands.BackColor = System.Drawing.Color.SpringGreen;
			this.colorButton_NewCommands.BackColorUseGeneric = false;
			this.colorButton_NewCommands.Checked = false;
			this.colorButton_NewCommands.ContextMenuStrip = this.buttonContextMenu;
			this.colorButton_NewCommands.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
			this.colorButton_NewCommands.Location = new System.Drawing.Point(191, 100);
			this.colorButton_NewCommands.Margin = new System.Windows.Forms.Padding(3, 0, 3, 3);
			this.colorButton_NewCommands.Name = "colorButton_NewCommands";
			this.colorButton_NewCommands.Size = new System.Drawing.Size(170, 25);
			this.colorButton_NewCommands.TabIndex = 9;
			this.colorButton_NewCommands.UseForeColor = true;
			this.colorButton_NewCommands.Click += new System.EventHandler(this.button_Color_Click);
			// 
			// colorButton_References
			// 
			this.colorButton_References.BackColor = System.Drawing.Color.Orchid;
			this.colorButton_References.BackColorUseGeneric = false;
			this.colorButton_References.Checked = false;
			this.colorButton_References.ContextMenuStrip = this.buttonContextMenu;
			this.colorButton_References.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
			this.colorButton_References.Location = new System.Drawing.Point(12, 141);
			this.colorButton_References.Margin = new System.Windows.Forms.Padding(9, 0, 3, 8);
			this.colorButton_References.Name = "colorButton_References";
			this.colorButton_References.Size = new System.Drawing.Size(170, 25);
			this.colorButton_References.TabIndex = 5;
			this.colorButton_References.UseForeColor = true;
			this.colorButton_References.Click += new System.EventHandler(this.button_Color_Click);
			// 
			// colorButton_Sections
			// 
			this.colorButton_Sections.BackColor = System.Drawing.Color.SteelBlue;
			this.colorButton_Sections.BackColorUseGeneric = false;
			this.colorButton_Sections.Checked = false;
			this.colorButton_Sections.ContextMenuStrip = this.buttonContextMenu;
			this.colorButton_Sections.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
			this.colorButton_Sections.Location = new System.Drawing.Point(12, 59);
			this.colorButton_Sections.Margin = new System.Windows.Forms.Padding(9, 0, 3, 3);
			this.colorButton_Sections.Name = "colorButton_Sections";
			this.colorButton_Sections.Size = new System.Drawing.Size(170, 25);
			this.colorButton_Sections.TabIndex = 1;
			this.colorButton_Sections.UseForeColor = true;
			this.colorButton_Sections.Click += new System.EventHandler(this.button_Color_Click);
			// 
			// colorButton_StandardCommands
			// 
			this.colorButton_StandardCommands.BackColor = System.Drawing.Color.MediumAquamarine;
			this.colorButton_StandardCommands.BackColorUseGeneric = false;
			this.colorButton_StandardCommands.Checked = false;
			this.colorButton_StandardCommands.ContextMenuStrip = this.buttonContextMenu;
			this.colorButton_StandardCommands.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
			this.colorButton_StandardCommands.Location = new System.Drawing.Point(191, 59);
			this.colorButton_StandardCommands.Margin = new System.Windows.Forms.Padding(6, 0, 3, 3);
			this.colorButton_StandardCommands.Name = "colorButton_StandardCommands";
			this.colorButton_StandardCommands.Size = new System.Drawing.Size(170, 25);
			this.colorButton_StandardCommands.TabIndex = 7;
			this.colorButton_StandardCommands.UseForeColor = true;
			this.colorButton_StandardCommands.Click += new System.EventHandler(this.button_Color_Click);
			// 
			// colorButton_Values
			// 
			this.colorButton_Values.BackColor = System.Drawing.Color.LightSalmon;
			this.colorButton_Values.BackColorUseGeneric = false;
			this.colorButton_Values.Checked = false;
			this.colorButton_Values.ContextMenuStrip = this.buttonContextMenu;
			this.colorButton_Values.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
			this.colorButton_Values.Location = new System.Drawing.Point(12, 100);
			this.colorButton_Values.Margin = new System.Windows.Forms.Padding(3, 0, 3, 3);
			this.colorButton_Values.Name = "colorButton_Values";
			this.colorButton_Values.Size = new System.Drawing.Size(170, 25);
			this.colorButton_Values.TabIndex = 3;
			this.colorButton_Values.UseForeColor = true;
			this.colorButton_Values.Click += new System.EventHandler(this.button_Color_Click);
			// 
			// colorDialog
			// 
			this.colorDialog.AnyColor = true;
			this.colorDialog.FullOpen = true;
			// 
			// darkLabel1
			// 
			this.darkLabel1.AutoSize = true;
			this.darkLabel1.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
			this.darkLabel1.Location = new System.Drawing.Point(7, 34);
			this.darkLabel1.Margin = new System.Windows.Forms.Padding(6, 9, 3, 0);
			this.darkLabel1.Name = "darkLabel1";
			this.darkLabel1.Size = new System.Drawing.Size(52, 13);
			this.darkLabel1.TabIndex = 0;
			this.darkLabel1.Text = "Font size:";
			// 
			// darkLabel10
			// 
			this.darkLabel10.AutoSize = true;
			this.darkLabel10.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
			this.darkLabel10.Location = new System.Drawing.Point(370, 46);
			this.darkLabel10.Margin = new System.Windows.Forms.Padding(3, 3, 3, 0);
			this.darkLabel10.Name = "darkLabel10";
			this.darkLabel10.Size = new System.Drawing.Size(68, 13);
			this.darkLabel10.TabIndex = 13;
			this.darkLabel10.Text = "Background:";
			// 
			// darkLabel11
			// 
			this.darkLabel11.AutoSize = true;
			this.darkLabel11.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
			this.darkLabel11.Location = new System.Drawing.Point(370, 87);
			this.darkLabel11.Name = "darkLabel11";
			this.darkLabel11.Size = new System.Drawing.Size(89, 13);
			this.darkLabel11.TabIndex = 15;
			this.darkLabel11.Text = "Normal text color:";
			// 
			// darkLabel12
			// 
			this.darkLabel12.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.darkLabel12.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
			this.darkLabel12.Location = new System.Drawing.Point(475, 17);
			this.darkLabel12.Name = "darkLabel12";
			this.darkLabel12.Size = new System.Drawing.Size(2, 23);
			this.darkLabel12.TabIndex = 20;
			// 
			// darkLabel2
			// 
			this.darkLabel2.AutoSize = true;
			this.darkLabel2.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
			this.darkLabel2.Location = new System.Drawing.Point(7, 76);
			this.darkLabel2.Margin = new System.Windows.Forms.Padding(6, 3, 3, 0);
			this.darkLabel2.Name = "darkLabel2";
			this.darkLabel2.Size = new System.Drawing.Size(60, 13);
			this.darkLabel2.TabIndex = 2;
			this.darkLabel2.Text = "Font family:";
			// 
			// darkLabel3
			// 
			this.darkLabel3.AutoSize = true;
			this.darkLabel3.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
			this.darkLabel3.Location = new System.Drawing.Point(7, 119);
			this.darkLabel3.Margin = new System.Windows.Forms.Padding(6, 3, 3, 0);
			this.darkLabel3.Name = "darkLabel3";
			this.darkLabel3.Size = new System.Drawing.Size(86, 13);
			this.darkLabel3.TabIndex = 4;
			this.darkLabel3.Text = "Undo stack size:";
			// 
			// darkLabel4
			// 
			this.darkLabel4.AutoSize = true;
			this.darkLabel4.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
			this.darkLabel4.Location = new System.Drawing.Point(12, 46);
			this.darkLabel4.Margin = new System.Windows.Forms.Padding(9, 3, 3, 0);
			this.darkLabel4.Name = "darkLabel4";
			this.darkLabel4.Size = new System.Drawing.Size(51, 13);
			this.darkLabel4.TabIndex = 0;
			this.darkLabel4.Text = "Sections:";
			// 
			// darkLabel5
			// 
			this.darkLabel5.AutoSize = true;
			this.darkLabel5.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
			this.darkLabel5.Location = new System.Drawing.Point(12, 87);
			this.darkLabel5.Name = "darkLabel5";
			this.darkLabel5.Size = new System.Drawing.Size(42, 13);
			this.darkLabel5.TabIndex = 2;
			this.darkLabel5.Text = "Values:";
			// 
			// darkLabel6
			// 
			this.darkLabel6.AutoSize = true;
			this.darkLabel6.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
			this.darkLabel6.Location = new System.Drawing.Point(12, 128);
			this.darkLabel6.Name = "darkLabel6";
			this.darkLabel6.Size = new System.Drawing.Size(65, 13);
			this.darkLabel6.TabIndex = 4;
			this.darkLabel6.Text = "References:";
			// 
			// darkLabel7
			// 
			this.darkLabel7.AutoSize = true;
			this.darkLabel7.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
			this.darkLabel7.Location = new System.Drawing.Point(190, 46);
			this.darkLabel7.Margin = new System.Windows.Forms.Padding(3, 3, 3, 0);
			this.darkLabel7.Name = "darkLabel7";
			this.darkLabel7.Size = new System.Drawing.Size(107, 13);
			this.darkLabel7.TabIndex = 6;
			this.darkLabel7.Text = "Standard commands:";
			// 
			// darkLabel8
			// 
			this.darkLabel8.AutoSize = true;
			this.darkLabel8.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
			this.darkLabel8.Location = new System.Drawing.Point(190, 87);
			this.darkLabel8.Name = "darkLabel8";
			this.darkLabel8.Size = new System.Drawing.Size(86, 13);
			this.darkLabel8.TabIndex = 8;
			this.darkLabel8.Text = "New commands:";
			// 
			// darkLabel9
			// 
			this.darkLabel9.AutoSize = true;
			this.darkLabel9.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
			this.darkLabel9.Location = new System.Drawing.Point(190, 128);
			this.darkLabel9.Name = "darkLabel9";
			this.darkLabel9.Size = new System.Drawing.Size(59, 13);
			this.darkLabel9.TabIndex = 10;
			this.darkLabel9.Text = "Comments:";
			// 
			// elementHost
			// 
			this.elementHost.Dock = System.Windows.Forms.DockStyle.Fill;
			this.elementHost.Location = new System.Drawing.Point(3, 16);
			this.elementHost.Name = "elementHost";
			this.elementHost.Size = new System.Drawing.Size(345, 133);
			this.elementHost.TabIndex = 0;
			this.elementHost.Child = null;
			// 
			// groupBox_AddSpaces
			// 
			this.groupBox_AddSpaces.Controls.Add(this.label_Comma);
			this.groupBox_AddSpaces.Controls.Add(this.label_Equal);
			this.groupBox_AddSpaces.Controls.Add(this.checkBox_PostCommaSpace);
			this.groupBox_AddSpaces.Controls.Add(this.checkBox_PreCommaSpace);
			this.groupBox_AddSpaces.Controls.Add(this.checkBox_PostEqualSpace);
			this.groupBox_AddSpaces.Controls.Add(this.checkBox_PreEqualSpace);
			this.groupBox_AddSpaces.Location = new System.Drawing.Point(9, 22);
			this.groupBox_AddSpaces.Margin = new System.Windows.Forms.Padding(6, 6, 3, 3);
			this.groupBox_AddSpaces.Name = "groupBox_AddSpaces";
			this.groupBox_AddSpaces.Size = new System.Drawing.Size(176, 120);
			this.groupBox_AddSpaces.TabIndex = 0;
			this.groupBox_AddSpaces.TabStop = false;
			this.groupBox_AddSpaces.Text = "Insert spaces";
			// 
			// label_Comma
			// 
			this.label_Comma.Font = new System.Drawing.Font("Consolas", 15.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
			this.label_Comma.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
			this.label_Comma.Location = new System.Drawing.Point(76, 70);
			this.label_Comma.Margin = new System.Windows.Forms.Padding(18, 0, 18, 0);
			this.label_Comma.Name = "label_Comma";
			this.label_Comma.Size = new System.Drawing.Size(24, 24);
			this.label_Comma.TabIndex = 5;
			this.label_Comma.Text = ",";
			this.label_Comma.TextAlign = System.Drawing.ContentAlignment.BottomRight;
			// 
			// label_Equal
			// 
			this.label_Equal.Font = new System.Drawing.Font("Consolas", 15.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
			this.label_Equal.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
			this.label_Equal.Location = new System.Drawing.Point(76, 29);
			this.label_Equal.Margin = new System.Windows.Forms.Padding(18, 0, 18, 0);
			this.label_Equal.Name = "label_Equal";
			this.label_Equal.Size = new System.Drawing.Size(24, 24);
			this.label_Equal.TabIndex = 4;
			this.label_Equal.Text = "=";
			this.label_Equal.TextAlign = System.Drawing.ContentAlignment.BottomRight;
			// 
			// groupBox_Colors
			// 
			this.groupBox_Colors.Controls.Add(this.button_ImportScheme);
			this.groupBox_Colors.Controls.Add(this.darkLabel12);
			this.groupBox_Colors.Controls.Add(this.button_SaveScheme);
			this.groupBox_Colors.Controls.Add(this.button_DeleteScheme);
			this.groupBox_Colors.Controls.Add(this.button_OpenSchemesFolder);
			this.groupBox_Colors.Controls.Add(this.darkLabel11);
			this.groupBox_Colors.Controls.Add(this.colorButton_Foreground);
			this.groupBox_Colors.Controls.Add(this.darkLabel10);
			this.groupBox_Colors.Controls.Add(this.colorButton_Background);
			this.groupBox_Colors.Controls.Add(this.comboBox_ColorSchemes);
			this.groupBox_Colors.Controls.Add(this.darkLabel9);
			this.groupBox_Colors.Controls.Add(this.darkLabel8);
			this.groupBox_Colors.Controls.Add(this.darkLabel7);
			this.groupBox_Colors.Controls.Add(this.darkLabel6);
			this.groupBox_Colors.Controls.Add(this.darkLabel5);
			this.groupBox_Colors.Controls.Add(this.darkLabel4);
			this.groupBox_Colors.Controls.Add(this.colorButton_StandardCommands);
			this.groupBox_Colors.Controls.Add(this.colorButton_NewCommands);
			this.groupBox_Colors.Controls.Add(this.colorButton_Sections);
			this.groupBox_Colors.Controls.Add(this.colorButton_Values);
			this.groupBox_Colors.Controls.Add(this.colorButton_References);
			this.groupBox_Colors.Controls.Add(this.colorButton_Comments);
			this.groupBox_Colors.Location = new System.Drawing.Point(162, 28);
			this.groupBox_Colors.Margin = new System.Windows.Forms.Padding(3, 3, 6, 3);
			this.groupBox_Colors.Name = "groupBox_Colors";
			this.groupBox_Colors.Size = new System.Drawing.Size(551, 177);
			this.groupBox_Colors.TabIndex = 17;
			this.groupBox_Colors.TabStop = false;
			this.groupBox_Colors.Text = "Color schemes";
			// 
			// groupBox_Identation
			// 
			this.groupBox_Identation.Controls.Add(this.groupBox_Preview);
			this.groupBox_Identation.Controls.Add(this.groupBox_AddSpaces);
			this.groupBox_Identation.Controls.Add(this.checkBox_ReduceSpaces);
			this.groupBox_Identation.Location = new System.Drawing.Point(162, 228);
			this.groupBox_Identation.Margin = new System.Windows.Forms.Padding(3, 3, 6, 6);
			this.groupBox_Identation.Name = "groupBox_Identation";
			this.groupBox_Identation.Size = new System.Drawing.Size(551, 177);
			this.groupBox_Identation.TabIndex = 18;
			this.groupBox_Identation.TabStop = false;
			this.groupBox_Identation.Text = "Indentation rules";
			// 
			// groupBox_Preview
			// 
			this.groupBox_Preview.Controls.Add(this.elementHost);
			this.groupBox_Preview.Location = new System.Drawing.Point(191, 16);
			this.groupBox_Preview.Margin = new System.Windows.Forms.Padding(3, 0, 6, 6);
			this.groupBox_Preview.Name = "groupBox_Preview";
			this.groupBox_Preview.Size = new System.Drawing.Size(351, 152);
			this.groupBox_Preview.TabIndex = 2;
			this.groupBox_Preview.TabStop = false;
			this.groupBox_Preview.Text = "Preview";
			// 
			// numeric_FontSize
			// 
			this.numeric_FontSize.IncrementAlternate = new decimal(new int[] {
            10,
            0,
            0,
            65536});
			this.numeric_FontSize.Location = new System.Drawing.Point(6, 50);
			this.numeric_FontSize.LoopValues = false;
			this.numeric_FontSize.Margin = new System.Windows.Forms.Padding(6, 3, 3, 3);
			this.numeric_FontSize.Maximum = new decimal(new int[] {
            32,
            0,
            0,
            0});
			this.numeric_FontSize.Minimum = new decimal(new int[] {
            4,
            0,
            0,
            0});
			this.numeric_FontSize.Name = "numeric_FontSize";
			this.numeric_FontSize.Size = new System.Drawing.Size(150, 20);
			this.numeric_FontSize.TabIndex = 1;
			this.numeric_FontSize.Value = new decimal(new int[] {
            12,
            0,
            0,
            0});
			this.numeric_FontSize.ValueChanged += new System.EventHandler(this.VisiblePreviewSetting_Changed);
			// 
			// numeric_UndoStackSize
			// 
			this.numeric_UndoStackSize.IncrementAlternate = new decimal(new int[] {
            10,
            0,
            0,
            65536});
			this.numeric_UndoStackSize.Location = new System.Drawing.Point(6, 135);
			this.numeric_UndoStackSize.LoopValues = false;
			this.numeric_UndoStackSize.Margin = new System.Windows.Forms.Padding(6, 3, 3, 3);
			this.numeric_UndoStackSize.Maximum = new decimal(new int[] {
            1024,
            0,
            0,
            0});
			this.numeric_UndoStackSize.Minimum = new decimal(new int[] {
            16,
            0,
            0,
            0});
			this.numeric_UndoStackSize.Name = "numeric_UndoStackSize";
			this.numeric_UndoStackSize.Size = new System.Drawing.Size(150, 20);
			this.numeric_UndoStackSize.TabIndex = 5;
			this.numeric_UndoStackSize.Value = new decimal(new int[] {
            256,
            0,
            0,
            0});
			// 
			// sectionPanel
			// 
			this.sectionPanel.Controls.Add(this.checkBox_HighlightCurrentLine);
			this.sectionPanel.Controls.Add(this.checkBox_SectionSeparators);
			this.sectionPanel.Controls.Add(this.checkBox_VisibleTabs);
			this.sectionPanel.Controls.Add(this.checkBox_VisibleSpaces);
			this.sectionPanel.Controls.Add(this.checkBox_LineNumbers);
			this.sectionPanel.Controls.Add(this.groupBox_Identation);
			this.sectionPanel.Controls.Add(this.darkLabel3);
			this.sectionPanel.Controls.Add(this.darkLabel2);
			this.sectionPanel.Controls.Add(this.darkLabel1);
			this.sectionPanel.Controls.Add(this.checkBox_CloseQuotes);
			this.sectionPanel.Controls.Add(this.checkBox_LiveErrors);
			this.sectionPanel.Controls.Add(this.numeric_UndoStackSize);
			this.sectionPanel.Controls.Add(this.checkBox_Autocomplete);
			this.sectionPanel.Controls.Add(this.checkBox_WordWrapping);
			this.sectionPanel.Controls.Add(this.groupBox_Colors);
			this.sectionPanel.Controls.Add(this.checkBox_CloseBrackets);
			this.sectionPanel.Controls.Add(this.comboBox_FontFamily);
			this.sectionPanel.Controls.Add(this.numeric_FontSize);
			this.sectionPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.sectionPanel.Location = new System.Drawing.Point(0, 0);
			this.sectionPanel.Name = "sectionPanel";
			this.sectionPanel.SectionHeader = "Classic Script";
			this.sectionPanel.Size = new System.Drawing.Size(720, 412);
			this.sectionPanel.TabIndex = 0;
			// 
			// button_SaveScheme
			// 
			this.button_SaveScheme.Checked = false;
			this.button_SaveScheme.Image = global::TombIDE.ScriptEditor.Properties.Resources.Save_16;
			this.button_SaveScheme.Location = new System.Drawing.Point(413, 16);
			this.button_SaveScheme.Name = "button_SaveScheme";
			this.button_SaveScheme.Size = new System.Drawing.Size(25, 25);
			this.button_SaveScheme.TabIndex = 19;
			this.toolTip.SetToolTip(this.button_SaveScheme, "Save Scheme As...");
			this.button_SaveScheme.Click += new System.EventHandler(this.button_SaveScheme_Click);
			// 
			// button_DeleteScheme
			// 
			this.button_DeleteScheme.Checked = false;
			this.button_DeleteScheme.Image = global::TombIDE.ScriptEditor.Properties.Resources.Trash_16;
			this.button_DeleteScheme.Location = new System.Drawing.Point(444, 16);
			this.button_DeleteScheme.Name = "button_DeleteScheme";
			this.button_DeleteScheme.Size = new System.Drawing.Size(25, 25);
			this.button_DeleteScheme.TabIndex = 18;
			this.toolTip.SetToolTip(this.button_DeleteScheme, "Delete Scheme");
			this.button_DeleteScheme.Click += new System.EventHandler(this.button_DeleteScheme_Click);
			// 
			// button_OpenSchemesFolder
			// 
			this.button_OpenSchemesFolder.Checked = false;
			this.button_OpenSchemesFolder.Image = global::TombIDE.ScriptEditor.Properties.Resources.ForwardArrow_16;
			this.button_OpenSchemesFolder.Location = new System.Drawing.Point(514, 16);
			this.button_OpenSchemesFolder.Margin = new System.Windows.Forms.Padding(3, 0, 3, 3);
			this.button_OpenSchemesFolder.Name = "button_OpenSchemesFolder";
			this.button_OpenSchemesFolder.Size = new System.Drawing.Size(25, 25);
			this.button_OpenSchemesFolder.TabIndex = 17;
			this.toolTip.SetToolTip(this.button_OpenSchemesFolder, "Open Schemes Folder");
			this.button_OpenSchemesFolder.Click += new System.EventHandler(this.button_OpenSchemesFolder_Click);
			// 
			// comboBox_ColorSchemes
			// 
			this.comboBox_ColorSchemes.FormattingEnabled = true;
			this.comboBox_ColorSchemes.Location = new System.Drawing.Point(12, 19);
			this.comboBox_ColorSchemes.Margin = new System.Windows.Forms.Padding(9, 3, 3, 3);
			this.comboBox_ColorSchemes.Name = "comboBox_ColorSchemes";
			this.comboBox_ColorSchemes.Size = new System.Drawing.Size(395, 21);
			this.comboBox_ColorSchemes.TabIndex = 12;
			this.comboBox_ColorSchemes.SelectedIndexChanged += new System.EventHandler(this.comboBox_ColorSchemes_SelectedIndexChanged);
			// 
			// comboBox_FontFamily
			// 
			this.comboBox_FontFamily.FormattingEnabled = true;
			this.comboBox_FontFamily.Location = new System.Drawing.Point(6, 92);
			this.comboBox_FontFamily.Margin = new System.Windows.Forms.Padding(6, 3, 3, 3);
			this.comboBox_FontFamily.Name = "comboBox_FontFamily";
			this.comboBox_FontFamily.Size = new System.Drawing.Size(150, 21);
			this.comboBox_FontFamily.TabIndex = 3;
			this.comboBox_FontFamily.SelectedIndexChanged += new System.EventHandler(this.comboBox_FontFamily_SelectedIndexChanged);
			// 
			// ClassicScriptSettingsControl
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(63)))), ((int)(((byte)(65)))), ((int)(((byte)(69)))));
			this.Controls.Add(this.sectionPanel);
			this.MaximumSize = new System.Drawing.Size(720, 412);
			this.MinimumSize = new System.Drawing.Size(720, 412);
			this.Name = "ClassicScriptSettingsControl";
			this.Size = new System.Drawing.Size(720, 412);
			this.buttonContextMenu.ResumeLayout(false);
			this.groupBox_AddSpaces.ResumeLayout(false);
			this.groupBox_AddSpaces.PerformLayout();
			this.groupBox_Colors.ResumeLayout(false);
			this.groupBox_Colors.PerformLayout();
			this.groupBox_Identation.ResumeLayout(false);
			this.groupBox_Identation.PerformLayout();
			this.groupBox_Preview.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.numeric_FontSize)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.numeric_UndoStackSize)).EndInit();
			this.sectionPanel.ResumeLayout(false);
			this.sectionPanel.PerformLayout();
			this.ResumeLayout(false);

		}

		#endregion

		private DarkUI.Controls.DarkButton button_DeleteScheme;
		private DarkUI.Controls.DarkButton button_ImportScheme;
		private DarkUI.Controls.DarkButton button_OpenSchemesFolder;
		private DarkUI.Controls.DarkButton button_SaveScheme;
		private DarkUI.Controls.DarkButton colorButton_Background;
		private DarkUI.Controls.DarkButton colorButton_Comments;
		private DarkUI.Controls.DarkButton colorButton_Foreground;
		private DarkUI.Controls.DarkButton colorButton_NewCommands;
		private DarkUI.Controls.DarkButton colorButton_References;
		private DarkUI.Controls.DarkButton colorButton_Sections;
		private DarkUI.Controls.DarkButton colorButton_StandardCommands;
		private DarkUI.Controls.DarkButton colorButton_Values;
		private DarkUI.Controls.DarkCheckBox checkBox_Autocomplete;
		private DarkUI.Controls.DarkCheckBox checkBox_CloseBrackets;
		private DarkUI.Controls.DarkCheckBox checkBox_CloseQuotes;
		private DarkUI.Controls.DarkCheckBox checkBox_HighlightCurrentLine;
		private DarkUI.Controls.DarkCheckBox checkBox_LineNumbers;
		private DarkUI.Controls.DarkCheckBox checkBox_LiveErrors;
		private DarkUI.Controls.DarkCheckBox checkBox_PostCommaSpace;
		private DarkUI.Controls.DarkCheckBox checkBox_PostEqualSpace;
		private DarkUI.Controls.DarkCheckBox checkBox_PreCommaSpace;
		private DarkUI.Controls.DarkCheckBox checkBox_PreEqualSpace;
		private DarkUI.Controls.DarkCheckBox checkBox_ReduceSpaces;
		private DarkUI.Controls.DarkCheckBox checkBox_SectionSeparators;
		private DarkUI.Controls.DarkCheckBox checkBox_VisibleSpaces;
		private DarkUI.Controls.DarkCheckBox checkBox_VisibleTabs;
		private DarkUI.Controls.DarkCheckBox checkBox_WordWrapping;
		private DarkUI.Controls.DarkComboBox comboBox_ColorSchemes;
		private DarkUI.Controls.DarkComboBox comboBox_FontFamily;
		private DarkUI.Controls.DarkContextMenu buttonContextMenu;
		private DarkUI.Controls.DarkGroupBox groupBox_AddSpaces;
		private DarkUI.Controls.DarkGroupBox groupBox_Colors;
		private DarkUI.Controls.DarkGroupBox groupBox_Identation;
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
		private DarkUI.Controls.DarkLabel darkLabel9;
		private DarkUI.Controls.DarkLabel label_Comma;
		private DarkUI.Controls.DarkLabel label_Equal;
		private DarkUI.Controls.DarkNumericUpDown numeric_FontSize;
		private DarkUI.Controls.DarkNumericUpDown numeric_UndoStackSize;
		private DarkUI.Controls.DarkSectionPanel sectionPanel;
		private System.Windows.Forms.ColorDialog colorDialog;
		private System.Windows.Forms.Integration.ElementHost elementHost;
		private System.Windows.Forms.ToolStripMenuItem menuItem_Bold;
		private System.Windows.Forms.ToolStripMenuItem menuItem_Italic;
		private System.Windows.Forms.ToolTip toolTip;
	}
}
