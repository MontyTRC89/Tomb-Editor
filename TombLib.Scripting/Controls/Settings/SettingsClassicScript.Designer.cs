namespace TombLib.Scripting.Controls.Settings
{
	partial class SettingsClassicScript
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
			this.button_CommentsColor = new DarkUI.Controls.DarkButton();
			this.button_NewCommandsColor = new DarkUI.Controls.DarkButton();
			this.button_ReferencesColor = new DarkUI.Controls.DarkButton();
			this.button_SectionsColor = new DarkUI.Controls.DarkButton();
			this.button_StandardCommandsColor = new DarkUI.Controls.DarkButton();
			this.button_ValuesColor = new DarkUI.Controls.DarkButton();
			this.checkBox_Autocomplete = new DarkUI.Controls.DarkCheckBox();
			this.checkBox_CloseBrackets = new DarkUI.Controls.DarkCheckBox();
			this.checkBox_CloseQuotes = new DarkUI.Controls.DarkCheckBox();
			this.checkBox_LineNumbers = new DarkUI.Controls.DarkCheckBox();
			this.checkBox_LiveErrors = new DarkUI.Controls.DarkCheckBox();
			this.checkBox_PostCommaSpace = new DarkUI.Controls.DarkCheckBox();
			this.checkBox_PostEqualSpace = new DarkUI.Controls.DarkCheckBox();
			this.checkBox_PreCommaSpace = new DarkUI.Controls.DarkCheckBox();
			this.checkBox_PreEqualSpace = new DarkUI.Controls.DarkCheckBox();
			this.checkBox_ReduceSpaces = new DarkUI.Controls.DarkCheckBox();
			this.checkBox_SectionSeparators = new DarkUI.Controls.DarkCheckBox();
			this.checkBox_ToolTips = new DarkUI.Controls.DarkCheckBox();
			this.checkBox_VisibleSpaces = new DarkUI.Controls.DarkCheckBox();
			this.checkBox_VisibleTabs = new DarkUI.Controls.DarkCheckBox();
			this.checkBox_WordWrapping = new DarkUI.Controls.DarkCheckBox();
			this.colorDialog = new System.Windows.Forms.ColorDialog();
			this.comboBox_FontFamily = new DarkUI.Controls.DarkComboBox();
			this.darkLabel1 = new DarkUI.Controls.DarkLabel();
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
			this.groupBox_AddSpaces.SuspendLayout();
			this.groupBox_Colors.SuspendLayout();
			this.groupBox_Identation.SuspendLayout();
			this.groupBox_Preview.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.numeric_FontSize)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.numeric_UndoStackSize)).BeginInit();
			this.sectionPanel.SuspendLayout();
			this.SuspendLayout();
			// 
			// button_CommentsColor
			// 
			this.button_CommentsColor.BackColor = System.Drawing.Color.Green;
			this.button_CommentsColor.BackColorUseGeneric = false;
			this.button_CommentsColor.Checked = false;
			this.button_CommentsColor.Location = new System.Drawing.Point(283, 130);
			this.button_CommentsColor.Name = "button_CommentsColor";
			this.button_CommentsColor.Size = new System.Drawing.Size(256, 25);
			this.button_CommentsColor.TabIndex = 11;
			this.button_CommentsColor.Click += new System.EventHandler(this.button_Color_Click);
			// 
			// button_NewCommandsColor
			// 
			this.button_NewCommandsColor.BackColor = System.Drawing.Color.SpringGreen;
			this.button_NewCommandsColor.BackColorUseGeneric = false;
			this.button_NewCommandsColor.Checked = false;
			this.button_NewCommandsColor.Location = new System.Drawing.Point(283, 86);
			this.button_NewCommandsColor.Name = "button_NewCommandsColor";
			this.button_NewCommandsColor.Size = new System.Drawing.Size(256, 25);
			this.button_NewCommandsColor.TabIndex = 9;
			this.button_NewCommandsColor.Click += new System.EventHandler(this.button_Color_Click);
			// 
			// button_ReferencesColor
			// 
			this.button_ReferencesColor.BackColor = System.Drawing.Color.Orchid;
			this.button_ReferencesColor.BackColorUseGeneric = false;
			this.button_ReferencesColor.Checked = false;
			this.button_ReferencesColor.Location = new System.Drawing.Point(12, 130);
			this.button_ReferencesColor.Name = "button_ReferencesColor";
			this.button_ReferencesColor.Size = new System.Drawing.Size(256, 25);
			this.button_ReferencesColor.TabIndex = 5;
			this.button_ReferencesColor.Click += new System.EventHandler(this.button_Color_Click);
			// 
			// button_SectionsColor
			// 
			this.button_SectionsColor.BackColor = System.Drawing.Color.SteelBlue;
			this.button_SectionsColor.BackColorUseGeneric = false;
			this.button_SectionsColor.Checked = false;
			this.button_SectionsColor.Location = new System.Drawing.Point(12, 42);
			this.button_SectionsColor.Margin = new System.Windows.Forms.Padding(9, 3, 3, 3);
			this.button_SectionsColor.Name = "button_SectionsColor";
			this.button_SectionsColor.Size = new System.Drawing.Size(256, 25);
			this.button_SectionsColor.TabIndex = 1;
			this.button_SectionsColor.Click += new System.EventHandler(this.button_Color_Click);
			// 
			// button_StandardCommandsColor
			// 
			this.button_StandardCommandsColor.BackColor = System.Drawing.Color.MediumAquamarine;
			this.button_StandardCommandsColor.BackColorUseGeneric = false;
			this.button_StandardCommandsColor.Checked = false;
			this.button_StandardCommandsColor.Location = new System.Drawing.Point(283, 42);
			this.button_StandardCommandsColor.Margin = new System.Windows.Forms.Padding(3, 3, 9, 3);
			this.button_StandardCommandsColor.Name = "button_StandardCommandsColor";
			this.button_StandardCommandsColor.Size = new System.Drawing.Size(256, 25);
			this.button_StandardCommandsColor.TabIndex = 7;
			this.button_StandardCommandsColor.Click += new System.EventHandler(this.button_Color_Click);
			// 
			// button_ValuesColor
			// 
			this.button_ValuesColor.BackColor = System.Drawing.Color.LightSalmon;
			this.button_ValuesColor.BackColorUseGeneric = false;
			this.button_ValuesColor.Checked = false;
			this.button_ValuesColor.Location = new System.Drawing.Point(12, 85);
			this.button_ValuesColor.Name = "button_ValuesColor";
			this.button_ValuesColor.Size = new System.Drawing.Size(256, 25);
			this.button_ValuesColor.TabIndex = 3;
			this.button_ValuesColor.Click += new System.EventHandler(this.button_Color_Click);
			// 
			// checkBox_Autocomplete
			// 
			this.checkBox_Autocomplete.AutoSize = true;
			this.checkBox_Autocomplete.Location = new System.Drawing.Point(6, 164);
			this.checkBox_Autocomplete.Margin = new System.Windows.Forms.Padding(3, 6, 3, 0);
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
			// checkBox_LineNumbers
			// 
			this.checkBox_LineNumbers.AutoSize = true;
			this.checkBox_LineNumbers.Location = new System.Drawing.Point(6, 276);
			this.checkBox_LineNumbers.Margin = new System.Windows.Forms.Padding(3, 9, 3, 0);
			this.checkBox_LineNumbers.Name = "checkBox_LineNumbers";
			this.checkBox_LineNumbers.Size = new System.Drawing.Size(115, 17);
			this.checkBox_LineNumbers.TabIndex = 11;
			this.checkBox_LineNumbers.Text = "Show line numbers";
			this.checkBox_LineNumbers.CheckedChanged += new System.EventHandler(this.checkBox_LineNumbers_CheckedChanged);
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
			this.checkBox_LiveErrors.CheckedChanged += new System.EventHandler(this.checkBox_LiveErrors_CheckedChanged);
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
			this.checkBox_SectionSeparators.Location = new System.Drawing.Point(6, 293);
			this.checkBox_SectionSeparators.Margin = new System.Windows.Forms.Padding(3, 0, 3, 0);
			this.checkBox_SectionSeparators.Name = "checkBox_SectionSeparators";
			this.checkBox_SectionSeparators.Size = new System.Drawing.Size(142, 17);
			this.checkBox_SectionSeparators.TabIndex = 12;
			this.checkBox_SectionSeparators.Text = "Show section separators";
			this.checkBox_SectionSeparators.CheckedChanged += new System.EventHandler(this.checkBox_SectionSeparators_CheckedChanged);
			// 
			// checkBox_ToolTips
			// 
			this.checkBox_ToolTips.AutoSize = true;
			this.checkBox_ToolTips.Location = new System.Drawing.Point(6, 362);
			this.checkBox_ToolTips.Margin = new System.Windows.Forms.Padding(3, 9, 3, 0);
			this.checkBox_ToolTips.Name = "checkBox_ToolTips";
			this.checkBox_ToolTips.Size = new System.Drawing.Size(137, 17);
			this.checkBox_ToolTips.TabIndex = 15;
			this.checkBox_ToolTips.Text = "Show definition tool tips";
			this.checkBox_ToolTips.CheckedChanged += new System.EventHandler(this.checkBox_ToolTips_CheckedChanged);
			// 
			// checkBox_VisibleSpaces
			// 
			this.checkBox_VisibleSpaces.AutoSize = true;
			this.checkBox_VisibleSpaces.Location = new System.Drawing.Point(6, 319);
			this.checkBox_VisibleSpaces.Margin = new System.Windows.Forms.Padding(3, 9, 3, 0);
			this.checkBox_VisibleSpaces.Name = "checkBox_VisibleSpaces";
			this.checkBox_VisibleSpaces.Size = new System.Drawing.Size(122, 17);
			this.checkBox_VisibleSpaces.TabIndex = 13;
			this.checkBox_VisibleSpaces.Text = "Show visible spaces";
			this.checkBox_VisibleSpaces.CheckedChanged += new System.EventHandler(this.checkBox_VisibleSpaces_CheckedChanged);
			// 
			// checkBox_VisibleTabs
			// 
			this.checkBox_VisibleTabs.AutoSize = true;
			this.checkBox_VisibleTabs.Location = new System.Drawing.Point(6, 336);
			this.checkBox_VisibleTabs.Margin = new System.Windows.Forms.Padding(3, 0, 3, 0);
			this.checkBox_VisibleTabs.Name = "checkBox_VisibleTabs";
			this.checkBox_VisibleTabs.Size = new System.Drawing.Size(108, 17);
			this.checkBox_VisibleTabs.TabIndex = 14;
			this.checkBox_VisibleTabs.Text = "Show visible tabs";
			this.checkBox_VisibleTabs.CheckedChanged += new System.EventHandler(this.checkBox_VisibleTabs_CheckedChanged);
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
			this.checkBox_WordWrapping.CheckedChanged += new System.EventHandler(this.checkBox_WordWrapping_CheckedChanged);
			// 
			// colorDialog
			// 
			this.colorDialog.AnyColor = true;
			this.colorDialog.FullOpen = true;
			// 
			// comboBox_FontFamily
			// 
			this.comboBox_FontFamily.FormattingEnabled = true;
			this.comboBox_FontFamily.Location = new System.Drawing.Point(6, 92);
			this.comboBox_FontFamily.Margin = new System.Windows.Forms.Padding(6, 3, 3, 3);
			this.comboBox_FontFamily.Name = "comboBox_FontFamily";
			this.comboBox_FontFamily.Size = new System.Drawing.Size(140, 21);
			this.comboBox_FontFamily.TabIndex = 3;
			this.comboBox_FontFamily.SelectedIndexChanged += new System.EventHandler(this.comboBox_FontFamily_SelectedIndexChanged);
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
			// darkLabel2
			// 
			this.darkLabel2.AutoSize = true;
			this.darkLabel2.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
			this.darkLabel2.Location = new System.Drawing.Point(7, 76);
			this.darkLabel2.Margin = new System.Windows.Forms.Padding(6, 3, 3, 0);
			this.darkLabel2.Name = "darkLabel2";
			this.darkLabel2.Size = new System.Drawing.Size(118, 13);
			this.darkLabel2.TabIndex = 2;
			this.darkLabel2.Text = "Font family: (Mono only)";
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
			this.darkLabel4.Location = new System.Drawing.Point(12, 26);
			this.darkLabel4.Margin = new System.Windows.Forms.Padding(6, 9, 3, 0);
			this.darkLabel4.Name = "darkLabel4";
			this.darkLabel4.Size = new System.Drawing.Size(51, 13);
			this.darkLabel4.TabIndex = 0;
			this.darkLabel4.Text = "Sections:";
			// 
			// darkLabel5
			// 
			this.darkLabel5.AutoSize = true;
			this.darkLabel5.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
			this.darkLabel5.Location = new System.Drawing.Point(12, 70);
			this.darkLabel5.Name = "darkLabel5";
			this.darkLabel5.Size = new System.Drawing.Size(42, 13);
			this.darkLabel5.TabIndex = 2;
			this.darkLabel5.Text = "Values:";
			// 
			// darkLabel6
			// 
			this.darkLabel6.AutoSize = true;
			this.darkLabel6.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
			this.darkLabel6.Location = new System.Drawing.Point(12, 114);
			this.darkLabel6.Name = "darkLabel6";
			this.darkLabel6.Size = new System.Drawing.Size(65, 13);
			this.darkLabel6.TabIndex = 4;
			this.darkLabel6.Text = "References:";
			// 
			// darkLabel7
			// 
			this.darkLabel7.AutoSize = true;
			this.darkLabel7.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
			this.darkLabel7.Location = new System.Drawing.Point(282, 26);
			this.darkLabel7.Name = "darkLabel7";
			this.darkLabel7.Size = new System.Drawing.Size(107, 13);
			this.darkLabel7.TabIndex = 6;
			this.darkLabel7.Text = "Standard commands:";
			// 
			// darkLabel8
			// 
			this.darkLabel8.AutoSize = true;
			this.darkLabel8.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
			this.darkLabel8.Location = new System.Drawing.Point(282, 70);
			this.darkLabel8.Name = "darkLabel8";
			this.darkLabel8.Size = new System.Drawing.Size(86, 13);
			this.darkLabel8.TabIndex = 8;
			this.darkLabel8.Text = "New commands:";
			// 
			// darkLabel9
			// 
			this.darkLabel9.AutoSize = true;
			this.darkLabel9.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
			this.darkLabel9.Location = new System.Drawing.Point(282, 114);
			this.darkLabel9.Name = "darkLabel9";
			this.darkLabel9.Size = new System.Drawing.Size(59, 13);
			this.darkLabel9.TabIndex = 10;
			this.darkLabel9.Text = "Comments:";
			// 
			// elementHost
			// 
			this.elementHost.Location = new System.Drawing.Point(6, 19);
			this.elementHost.Name = "elementHost";
			this.elementHost.Size = new System.Drawing.Size(339, 127);
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
			this.groupBox_Colors.Controls.Add(this.darkLabel9);
			this.groupBox_Colors.Controls.Add(this.darkLabel8);
			this.groupBox_Colors.Controls.Add(this.darkLabel7);
			this.groupBox_Colors.Controls.Add(this.darkLabel6);
			this.groupBox_Colors.Controls.Add(this.darkLabel5);
			this.groupBox_Colors.Controls.Add(this.darkLabel4);
			this.groupBox_Colors.Controls.Add(this.button_StandardCommandsColor);
			this.groupBox_Colors.Controls.Add(this.button_NewCommandsColor);
			this.groupBox_Colors.Controls.Add(this.button_SectionsColor);
			this.groupBox_Colors.Controls.Add(this.button_ValuesColor);
			this.groupBox_Colors.Controls.Add(this.button_ReferencesColor);
			this.groupBox_Colors.Controls.Add(this.button_CommentsColor);
			this.groupBox_Colors.Location = new System.Drawing.Point(152, 28);
			this.groupBox_Colors.Margin = new System.Windows.Forms.Padding(3, 3, 6, 3);
			this.groupBox_Colors.Name = "groupBox_Colors";
			this.groupBox_Colors.Size = new System.Drawing.Size(551, 177);
			this.groupBox_Colors.TabIndex = 16;
			this.groupBox_Colors.TabStop = false;
			this.groupBox_Colors.Text = "Syntax colors";
			// 
			// groupBox_Identation
			// 
			this.groupBox_Identation.Controls.Add(this.groupBox_Preview);
			this.groupBox_Identation.Controls.Add(this.groupBox_AddSpaces);
			this.groupBox_Identation.Controls.Add(this.checkBox_ReduceSpaces);
			this.groupBox_Identation.Location = new System.Drawing.Point(152, 211);
			this.groupBox_Identation.Margin = new System.Windows.Forms.Padding(3, 3, 6, 6);
			this.groupBox_Identation.Name = "groupBox_Identation";
			this.groupBox_Identation.Size = new System.Drawing.Size(551, 177);
			this.groupBox_Identation.TabIndex = 17;
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
			this.numeric_FontSize.Size = new System.Drawing.Size(140, 20);
			this.numeric_FontSize.TabIndex = 1;
			this.numeric_FontSize.Value = new decimal(new int[] {
            12,
            0,
            0,
            0});
			this.numeric_FontSize.ValueChanged += new System.EventHandler(this.numeric_FontSize_ValueChanged);
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
			this.numeric_UndoStackSize.Size = new System.Drawing.Size(140, 20);
			this.numeric_UndoStackSize.TabIndex = 5;
			this.numeric_UndoStackSize.Value = new decimal(new int[] {
            256,
            0,
            0,
            0});
			// 
			// sectionPanel
			// 
			this.sectionPanel.Controls.Add(this.checkBox_ToolTips);
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
			this.sectionPanel.MaximumSize = new System.Drawing.Size(710, 395);
			this.sectionPanel.MinimumSize = new System.Drawing.Size(710, 395);
			this.sectionPanel.Name = "sectionPanel";
			this.sectionPanel.SectionHeader = "Classic Script";
			this.sectionPanel.Size = new System.Drawing.Size(710, 395);
			this.sectionPanel.TabIndex = 0;
			// 
			// SettingsClassicScript
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(63)))), ((int)(((byte)(65)))), ((int)(((byte)(69)))));
			this.Controls.Add(this.sectionPanel);
			this.MaximumSize = new System.Drawing.Size(710, 395);
			this.MinimumSize = new System.Drawing.Size(710, 395);
			this.Name = "SettingsClassicScript";
			this.Size = new System.Drawing.Size(710, 395);
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

		private DarkUI.Controls.DarkButton button_CommentsColor;
		private DarkUI.Controls.DarkButton button_NewCommandsColor;
		private DarkUI.Controls.DarkButton button_ReferencesColor;
		private DarkUI.Controls.DarkButton button_SectionsColor;
		private DarkUI.Controls.DarkButton button_StandardCommandsColor;
		private DarkUI.Controls.DarkButton button_ValuesColor;
		private DarkUI.Controls.DarkCheckBox checkBox_Autocomplete;
		private DarkUI.Controls.DarkCheckBox checkBox_CloseBrackets;
		private DarkUI.Controls.DarkCheckBox checkBox_CloseQuotes;
		private DarkUI.Controls.DarkCheckBox checkBox_LineNumbers;
		private DarkUI.Controls.DarkCheckBox checkBox_LiveErrors;
		private DarkUI.Controls.DarkCheckBox checkBox_PostCommaSpace;
		private DarkUI.Controls.DarkCheckBox checkBox_PostEqualSpace;
		private DarkUI.Controls.DarkCheckBox checkBox_PreCommaSpace;
		private DarkUI.Controls.DarkCheckBox checkBox_PreEqualSpace;
		private DarkUI.Controls.DarkCheckBox checkBox_ReduceSpaces;
		private DarkUI.Controls.DarkCheckBox checkBox_SectionSeparators;
		private DarkUI.Controls.DarkCheckBox checkBox_ToolTips;
		private DarkUI.Controls.DarkCheckBox checkBox_VisibleSpaces;
		private DarkUI.Controls.DarkCheckBox checkBox_VisibleTabs;
		private DarkUI.Controls.DarkCheckBox checkBox_WordWrapping;
		private DarkUI.Controls.DarkComboBox comboBox_FontFamily;
		private DarkUI.Controls.DarkGroupBox groupBox_AddSpaces;
		private DarkUI.Controls.DarkGroupBox groupBox_Colors;
		private DarkUI.Controls.DarkGroupBox groupBox_Identation;
		private DarkUI.Controls.DarkGroupBox groupBox_Preview;
		private DarkUI.Controls.DarkLabel darkLabel1;
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
	}
}
