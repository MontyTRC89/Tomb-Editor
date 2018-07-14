namespace ScriptEditor
{
	partial class FormSettings : DarkUI.Forms.DarkForm
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.applyButton = new DarkUI.Controls.DarkButton();
			this.autocompleteCheck = new DarkUI.Controls.DarkCheckBox();
			this.autosaveCombo = new DarkUI.Controls.DarkComboBox();
			this.autoSaveLabel = new DarkUI.Controls.DarkLabel();
			this.cancelButton = new DarkUI.Controls.DarkButton();
			this.closeBracketsCheck = new DarkUI.Controls.DarkCheckBox();
			this.commentColorButton = new DarkUI.Controls.DarkButton();
			this.commentColorDialog = new System.Windows.Forms.ColorDialog();
			this.commentColorLabel = new DarkUI.Controls.DarkLabel();
			this.darkGroupBox1 = new DarkUI.Controls.DarkGroupBox();
			this.reindentRulesButton = new DarkUI.Controls.DarkButton();
			this.toolTipCheck = new DarkUI.Controls.DarkCheckBox();
			this.wordWrapCheck = new DarkUI.Controls.DarkCheckBox();
			this.showNumbersCheck = new DarkUI.Controls.DarkCheckBox();
			this.reindentCheck = new DarkUI.Controls.DarkCheckBox();
			this.showToolbarCheck = new DarkUI.Controls.DarkCheckBox();
			this.darkGroupBox2 = new DarkUI.Controls.DarkGroupBox();
			this.oldColorButton = new DarkUI.Controls.DarkButton();
			this.oldColorLabel = new DarkUI.Controls.DarkLabel();
			this.unknownColorButton = new DarkUI.Controls.DarkButton();
			this.newColorButton = new DarkUI.Controls.DarkButton();
			this.headerColorButton = new DarkUI.Controls.DarkButton();
			this.valueColorButton = new DarkUI.Controls.DarkButton();
			this.refColorButton = new DarkUI.Controls.DarkButton();
			this.unknownColorLabel = new DarkUI.Controls.DarkLabel();
			this.newColorLabel = new DarkUI.Controls.DarkLabel();
			this.headerColorLabel = new DarkUI.Controls.DarkLabel();
			this.valuesColorLabel = new DarkUI.Controls.DarkLabel();
			this.referencesColorLabel = new DarkUI.Controls.DarkLabel();
			this.showSpacesCheck = new DarkUI.Controls.DarkCheckBox();
			this.fontFaceCombo = new DarkUI.Controls.DarkComboBox();
			this.faceLabel = new DarkUI.Controls.DarkLabel();
			this.fontSizeNumeric = new DarkUI.Controls.DarkNumericUpDown();
			this.sizeLabel = new DarkUI.Controls.DarkLabel();
			this.darkGroupBox3 = new DarkUI.Controls.DarkGroupBox();
			this.restartLabel = new DarkUI.Controls.DarkLabel();
			this.resetDefaultButton = new DarkUI.Controls.DarkButton();
			this.headerColorDialog = new System.Windows.Forms.ColorDialog();
			this.newColorDialog = new System.Windows.Forms.ColorDialog();
			this.oldColorDialog = new System.Windows.Forms.ColorDialog();
			this.refColorDialog = new System.Windows.Forms.ColorDialog();
			this.unknownColorDialog = new System.Windows.Forms.ColorDialog();
			this.valueColorDialog = new System.Windows.Forms.ColorDialog();
			this.darkGroupBox1.SuspendLayout();
			this.darkGroupBox2.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.fontSizeNumeric)).BeginInit();
			this.darkGroupBox3.SuspendLayout();
			this.SuspendLayout();
			// 
			// applyButton
			// 
			this.applyButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.applyButton.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.applyButton.Location = new System.Drawing.Point(318, 9);
			this.applyButton.Name = "applyButton";
			this.applyButton.Size = new System.Drawing.Size(75, 25);
			this.applyButton.TabIndex = 1;
			this.applyButton.Text = "Apply";
			this.applyButton.Click += new System.EventHandler(this.applyButton_Click);
			// 
			// autocompleteCheck
			// 
			this.autocompleteCheck.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.autocompleteCheck.AutoSize = true;
			this.autocompleteCheck.Location = new System.Drawing.Point(6, 165);
			this.autocompleteCheck.Name = "autocompleteCheck";
			this.autocompleteCheck.Size = new System.Drawing.Size(126, 17);
			this.autocompleteCheck.TabIndex = 13;
			this.autocompleteCheck.Text = "Enable autocomplete";
			this.autocompleteCheck.CheckedChanged += new System.EventHandler(this.autocompleteCheck_CheckedChanged);
			// 
			// autosaveCombo
			// 
			this.autosaveCombo.FormattingEnabled = true;
			this.autosaveCombo.Items.AddRange(new object[] {
            "None",
            "1",
            "3",
            "5",
            "10",
            "15",
            "30"});
			this.autosaveCombo.Location = new System.Drawing.Point(6, 131);
			this.autosaveCombo.Name = "autosaveCombo";
			this.autosaveCombo.Size = new System.Drawing.Size(150, 21);
			this.autosaveCombo.TabIndex = 5;
			this.autosaveCombo.SelectedIndexChanged += new System.EventHandler(this.autosaveCombo_SelectedIndexChanged);
			// 
			// autoSaveLabel
			// 
			this.autoSaveLabel.AutoSize = true;
			this.autoSaveLabel.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
			this.autoSaveLabel.Location = new System.Drawing.Point(6, 111);
			this.autoSaveLabel.Name = "autoSaveLabel";
			this.autoSaveLabel.Size = new System.Drawing.Size(80, 13);
			this.autoSaveLabel.TabIndex = 4;
			this.autoSaveLabel.Text = "Autosave (min):";
			// 
			// cancelButton
			// 
			this.cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.cancelButton.Location = new System.Drawing.Point(399, 9);
			this.cancelButton.Name = "cancelButton";
			this.cancelButton.Size = new System.Drawing.Size(75, 25);
			this.cancelButton.TabIndex = 0;
			this.cancelButton.Text = "Cancel";
			// 
			// closeBracketsCheck
			// 
			this.closeBracketsCheck.AutoSize = true;
			this.closeBracketsCheck.Location = new System.Drawing.Point(6, 347);
			this.closeBracketsCheck.Name = "closeBracketsCheck";
			this.closeBracketsCheck.Size = new System.Drawing.Size(132, 17);
			this.closeBracketsCheck.TabIndex = 9;
			this.closeBracketsCheck.Text = "Auto close [ ] brackets";
			// 
			// commentColorButton
			// 
			this.commentColorButton.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.commentColorButton.BackColor = System.Drawing.Color.Green;
			this.commentColorButton.BackColorUseGeneric = false;
			this.commentColorButton.Location = new System.Drawing.Point(6, 47);
			this.commentColorButton.Name = "commentColorButton";
			this.commentColorButton.Size = new System.Drawing.Size(276, 25);
			this.commentColorButton.TabIndex = 18;
			this.commentColorButton.Click += new System.EventHandler(this.commentColorButton_Click);
			// 
			// commentColorDialog
			// 
			this.commentColorDialog.AnyColor = true;
			this.commentColorDialog.FullOpen = true;
			// 
			// commentColorLabel
			// 
			this.commentColorLabel.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.commentColorLabel.AutoSize = true;
			this.commentColorLabel.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
			this.commentColorLabel.Location = new System.Drawing.Point(6, 26);
			this.commentColorLabel.Name = "commentColorLabel";
			this.commentColorLabel.Size = new System.Drawing.Size(59, 13);
			this.commentColorLabel.TabIndex = 12;
			this.commentColorLabel.Text = "Comments:";
			// 
			// darkGroupBox1
			// 
			this.darkGroupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.darkGroupBox1.Controls.Add(this.reindentRulesButton);
			this.darkGroupBox1.Controls.Add(this.toolTipCheck);
			this.darkGroupBox1.Controls.Add(this.autocompleteCheck);
			this.darkGroupBox1.Controls.Add(this.wordWrapCheck);
			this.darkGroupBox1.Controls.Add(this.showNumbersCheck);
			this.darkGroupBox1.Controls.Add(this.reindentCheck);
			this.darkGroupBox1.Controls.Add(this.showToolbarCheck);
			this.darkGroupBox1.Controls.Add(this.darkGroupBox2);
			this.darkGroupBox1.Controls.Add(this.closeBracketsCheck);
			this.darkGroupBox1.Controls.Add(this.autosaveCombo);
			this.darkGroupBox1.Controls.Add(this.autoSaveLabel);
			this.darkGroupBox1.Controls.Add(this.showSpacesCheck);
			this.darkGroupBox1.Controls.Add(this.fontFaceCombo);
			this.darkGroupBox1.Controls.Add(this.faceLabel);
			this.darkGroupBox1.Controls.Add(this.fontSizeNumeric);
			this.darkGroupBox1.Controls.Add(this.sizeLabel);
			this.darkGroupBox1.Location = new System.Drawing.Point(12, 12);
			this.darkGroupBox1.Name = "darkGroupBox1";
			this.darkGroupBox1.Size = new System.Drawing.Size(456, 422);
			this.darkGroupBox1.TabIndex = 2;
			this.darkGroupBox1.TabStop = false;
			// 
			// reindentRulesButton
			// 
			this.reindentRulesButton.Location = new System.Drawing.Point(6, 393);
			this.reindentRulesButton.Name = "reindentRulesButton";
			this.reindentRulesButton.Size = new System.Drawing.Size(150, 23);
			this.reindentRulesButton.TabIndex = 16;
			this.reindentRulesButton.Text = "Reindent rules...";
			this.reindentRulesButton.Click += new System.EventHandler(this.reindentRulesButton_Click);
			// 
			// toolTipCheck
			// 
			this.toolTipCheck.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.toolTipCheck.AutoSize = true;
			this.toolTipCheck.Location = new System.Drawing.Point(6, 188);
			this.toolTipCheck.Name = "toolTipCheck";
			this.toolTipCheck.Size = new System.Drawing.Size(95, 17);
			this.toolTipCheck.TabIndex = 15;
			this.toolTipCheck.Text = "Enable tooltips";
			this.toolTipCheck.CheckedChanged += new System.EventHandler(this.toolTipCheck_CheckedChanged);
			// 
			// wordWrapCheck
			// 
			this.wordWrapCheck.AutoSize = true;
			this.wordWrapCheck.Location = new System.Drawing.Point(6, 324);
			this.wordWrapCheck.Name = "wordWrapCheck";
			this.wordWrapCheck.Size = new System.Drawing.Size(78, 17);
			this.wordWrapCheck.TabIndex = 12;
			this.wordWrapCheck.Text = "Word wrap";
			// 
			// showNumbersCheck
			// 
			this.showNumbersCheck.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.showNumbersCheck.AutoSize = true;
			this.showNumbersCheck.Location = new System.Drawing.Point(6, 258);
			this.showNumbersCheck.Name = "showNumbersCheck";
			this.showNumbersCheck.Size = new System.Drawing.Size(115, 17);
			this.showNumbersCheck.TabIndex = 11;
			this.showNumbersCheck.Text = "Show line numbers";
			// 
			// reindentCheck
			// 
			this.reindentCheck.AutoSize = true;
			this.reindentCheck.Location = new System.Drawing.Point(6, 370);
			this.reindentCheck.Name = "reindentCheck";
			this.reindentCheck.Size = new System.Drawing.Size(110, 17);
			this.reindentCheck.TabIndex = 10;
			this.reindentCheck.Text = "Reindent on save";
			// 
			// showToolbarCheck
			// 
			this.showToolbarCheck.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.showToolbarCheck.AutoSize = true;
			this.showToolbarCheck.Location = new System.Drawing.Point(6, 235);
			this.showToolbarCheck.Name = "showToolbarCheck";
			this.showToolbarCheck.Size = new System.Drawing.Size(88, 17);
			this.showToolbarCheck.TabIndex = 7;
			this.showToolbarCheck.Text = "Show toolbar";
			// 
			// darkGroupBox2
			// 
			this.darkGroupBox2.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.darkGroupBox2.Controls.Add(this.oldColorButton);
			this.darkGroupBox2.Controls.Add(this.oldColorLabel);
			this.darkGroupBox2.Controls.Add(this.unknownColorButton);
			this.darkGroupBox2.Controls.Add(this.newColorButton);
			this.darkGroupBox2.Controls.Add(this.headerColorButton);
			this.darkGroupBox2.Controls.Add(this.valueColorButton);
			this.darkGroupBox2.Controls.Add(this.refColorButton);
			this.darkGroupBox2.Controls.Add(this.commentColorButton);
			this.darkGroupBox2.Controls.Add(this.unknownColorLabel);
			this.darkGroupBox2.Controls.Add(this.newColorLabel);
			this.darkGroupBox2.Controls.Add(this.headerColorLabel);
			this.darkGroupBox2.Controls.Add(this.valuesColorLabel);
			this.darkGroupBox2.Controls.Add(this.referencesColorLabel);
			this.darkGroupBox2.Controls.Add(this.commentColorLabel);
			this.darkGroupBox2.Location = new System.Drawing.Point(162, 10);
			this.darkGroupBox2.Name = "darkGroupBox2";
			this.darkGroupBox2.Size = new System.Drawing.Size(288, 406);
			this.darkGroupBox2.TabIndex = 6;
			this.darkGroupBox2.TabStop = false;
			this.darkGroupBox2.Text = "Syntax Colors";
			// 
			// oldColorButton
			// 
			this.oldColorButton.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.oldColorButton.BackColor = System.Drawing.Color.MediumAquamarine;
			this.oldColorButton.BackColorUseGeneric = false;
			this.oldColorButton.Location = new System.Drawing.Point(6, 317);
			this.oldColorButton.Name = "oldColorButton";
			this.oldColorButton.Size = new System.Drawing.Size(276, 25);
			this.oldColorButton.TabIndex = 25;
			this.oldColorButton.Click += new System.EventHandler(this.oldColorButton_Click);
			// 
			// oldColorLabel
			// 
			this.oldColorLabel.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.oldColorLabel.AutoSize = true;
			this.oldColorLabel.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
			this.oldColorLabel.Location = new System.Drawing.Point(6, 296);
			this.oldColorLabel.Name = "oldColorLabel";
			this.oldColorLabel.Size = new System.Drawing.Size(107, 13);
			this.oldColorLabel.TabIndex = 24;
			this.oldColorLabel.Text = "Standard commands:";
			// 
			// unknownColorButton
			// 
			this.unknownColorButton.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.unknownColorButton.BackColor = System.Drawing.Color.Red;
			this.unknownColorButton.BackColorUseGeneric = false;
			this.unknownColorButton.Location = new System.Drawing.Point(6, 371);
			this.unknownColorButton.Name = "unknownColorButton";
			this.unknownColorButton.Size = new System.Drawing.Size(276, 25);
			this.unknownColorButton.TabIndex = 23;
			this.unknownColorButton.Click += new System.EventHandler(this.unknownColorButton_Click);
			// 
			// newColorButton
			// 
			this.newColorButton.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.newColorButton.BackColor = System.Drawing.Color.SpringGreen;
			this.newColorButton.BackColorUseGeneric = false;
			this.newColorButton.Location = new System.Drawing.Point(6, 263);
			this.newColorButton.Name = "newColorButton";
			this.newColorButton.Size = new System.Drawing.Size(276, 25);
			this.newColorButton.TabIndex = 22;
			this.newColorButton.Click += new System.EventHandler(this.newColorButton_Click);
			// 
			// headerColorButton
			// 
			this.headerColorButton.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.headerColorButton.BackColor = System.Drawing.Color.SteelBlue;
			this.headerColorButton.BackColorUseGeneric = false;
			this.headerColorButton.Location = new System.Drawing.Point(6, 209);
			this.headerColorButton.Name = "headerColorButton";
			this.headerColorButton.Size = new System.Drawing.Size(276, 25);
			this.headerColorButton.TabIndex = 21;
			this.headerColorButton.Click += new System.EventHandler(this.headerColorButton_Click);
			// 
			// valueColorButton
			// 
			this.valueColorButton.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.valueColorButton.BackColor = System.Drawing.Color.LightSalmon;
			this.valueColorButton.BackColorUseGeneric = false;
			this.valueColorButton.Location = new System.Drawing.Point(6, 155);
			this.valueColorButton.Name = "valueColorButton";
			this.valueColorButton.Size = new System.Drawing.Size(276, 25);
			this.valueColorButton.TabIndex = 20;
			this.valueColorButton.Click += new System.EventHandler(this.valueColorButton_Click);
			// 
			// refColorButton
			// 
			this.refColorButton.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.refColorButton.BackColor = System.Drawing.Color.Orchid;
			this.refColorButton.BackColorUseGeneric = false;
			this.refColorButton.Location = new System.Drawing.Point(6, 101);
			this.refColorButton.Name = "refColorButton";
			this.refColorButton.Size = new System.Drawing.Size(276, 25);
			this.refColorButton.TabIndex = 19;
			this.refColorButton.Click += new System.EventHandler(this.refColorButton_Click);
			// 
			// unknownColorLabel
			// 
			this.unknownColorLabel.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.unknownColorLabel.AutoSize = true;
			this.unknownColorLabel.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
			this.unknownColorLabel.Location = new System.Drawing.Point(6, 350);
			this.unknownColorLabel.Name = "unknownColorLabel";
			this.unknownColorLabel.Size = new System.Drawing.Size(56, 13);
			this.unknownColorLabel.TabIndex = 17;
			this.unknownColorLabel.Text = "Unknown:";
			// 
			// newColorLabel
			// 
			this.newColorLabel.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.newColorLabel.AutoSize = true;
			this.newColorLabel.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
			this.newColorLabel.Location = new System.Drawing.Point(6, 242);
			this.newColorLabel.Name = "newColorLabel";
			this.newColorLabel.Size = new System.Drawing.Size(95, 13);
			this.newColorLabel.TabIndex = 16;
			this.newColorLabel.Text = "TRNG commands:";
			// 
			// headerColorLabel
			// 
			this.headerColorLabel.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.headerColorLabel.AutoSize = true;
			this.headerColorLabel.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
			this.headerColorLabel.Location = new System.Drawing.Point(6, 188);
			this.headerColorLabel.Name = "headerColorLabel";
			this.headerColorLabel.Size = new System.Drawing.Size(50, 13);
			this.headerColorLabel.TabIndex = 15;
			this.headerColorLabel.Text = "Headers:";
			// 
			// valuesColorLabel
			// 
			this.valuesColorLabel.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.valuesColorLabel.AutoSize = true;
			this.valuesColorLabel.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
			this.valuesColorLabel.Location = new System.Drawing.Point(6, 134);
			this.valuesColorLabel.Name = "valuesColorLabel";
			this.valuesColorLabel.Size = new System.Drawing.Size(42, 13);
			this.valuesColorLabel.TabIndex = 14;
			this.valuesColorLabel.Text = "Values:";
			// 
			// referencesColorLabel
			// 
			this.referencesColorLabel.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.referencesColorLabel.AutoSize = true;
			this.referencesColorLabel.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
			this.referencesColorLabel.Location = new System.Drawing.Point(6, 80);
			this.referencesColorLabel.Name = "referencesColorLabel";
			this.referencesColorLabel.Size = new System.Drawing.Size(65, 13);
			this.referencesColorLabel.TabIndex = 13;
			this.referencesColorLabel.Text = "References:";
			// 
			// showSpacesCheck
			// 
			this.showSpacesCheck.AutoSize = true;
			this.showSpacesCheck.Location = new System.Drawing.Point(6, 281);
			this.showSpacesCheck.Name = "showSpacesCheck";
			this.showSpacesCheck.Size = new System.Drawing.Size(90, 17);
			this.showSpacesCheck.TabIndex = 8;
			this.showSpacesCheck.Text = "Show spaces";
			this.showSpacesCheck.CheckedChanged += new System.EventHandler(this.showSpacesCheck_CheckedChanged);
			// 
			// fontFaceCombo
			// 
			this.fontFaceCombo.FormattingEnabled = true;
			this.fontFaceCombo.Items.AddRange(new object[] {
            "Consolas",
            "Courier New",
            "Lucida Console",
            "Lucida Sans Typewriter"});
			this.fontFaceCombo.Location = new System.Drawing.Point(6, 83);
			this.fontFaceCombo.Name = "fontFaceCombo";
			this.fontFaceCombo.Size = new System.Drawing.Size(150, 21);
			this.fontFaceCombo.TabIndex = 3;
			// 
			// faceLabel
			// 
			this.faceLabel.AutoSize = true;
			this.faceLabel.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
			this.faceLabel.Location = new System.Drawing.Point(6, 63);
			this.faceLabel.Name = "faceLabel";
			this.faceLabel.Size = new System.Drawing.Size(55, 13);
			this.faceLabel.TabIndex = 2;
			this.faceLabel.Text = "Font face:";
			// 
			// fontSizeNumeric
			// 
			this.fontSizeNumeric.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(69)))), ((int)(((byte)(73)))), ((int)(((byte)(74)))));
			this.fontSizeNumeric.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
			this.fontSizeNumeric.IncrementAlternate = new decimal(new int[] {
            10,
            0,
            0,
            65536});
			this.fontSizeNumeric.Location = new System.Drawing.Point(6, 36);
			this.fontSizeNumeric.Maximum = new decimal(new int[] {
            30,
            0,
            0,
            0});
			this.fontSizeNumeric.Minimum = new decimal(new int[] {
            5,
            0,
            0,
            0});
			this.fontSizeNumeric.MousewheelSingleIncrement = true;
			this.fontSizeNumeric.Name = "fontSizeNumeric";
			this.fontSizeNumeric.Size = new System.Drawing.Size(150, 20);
			this.fontSizeNumeric.TabIndex = 1;
			this.fontSizeNumeric.Value = new decimal(new int[] {
            12,
            0,
            0,
            0});
			// 
			// sizeLabel
			// 
			this.sizeLabel.AutoSize = true;
			this.sizeLabel.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
			this.sizeLabel.Location = new System.Drawing.Point(6, 16);
			this.sizeLabel.Name = "sizeLabel";
			this.sizeLabel.Size = new System.Drawing.Size(52, 13);
			this.sizeLabel.TabIndex = 0;
			this.sizeLabel.Text = "Font size:";
			// 
			// darkGroupBox3
			// 
			this.darkGroupBox3.Controls.Add(this.restartLabel);
			this.darkGroupBox3.Controls.Add(this.resetDefaultButton);
			this.darkGroupBox3.Controls.Add(this.applyButton);
			this.darkGroupBox3.Controls.Add(this.cancelButton);
			this.darkGroupBox3.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.darkGroupBox3.Location = new System.Drawing.Point(0, 440);
			this.darkGroupBox3.Name = "darkGroupBox3";
			this.darkGroupBox3.Size = new System.Drawing.Size(480, 40);
			this.darkGroupBox3.TabIndex = 3;
			this.darkGroupBox3.TabStop = false;
			// 
			// restartLabel
			// 
			this.restartLabel.AutoSize = true;
			this.restartLabel.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
			this.restartLabel.Location = new System.Drawing.Point(224, 15);
			this.restartLabel.Name = "restartLabel";
			this.restartLabel.Size = new System.Drawing.Size(85, 13);
			this.restartLabel.TabIndex = 3;
			this.restartLabel.Text = "Restart required.";
			this.restartLabel.Visible = false;
			// 
			// resetDefaultButton
			// 
			this.resetDefaultButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.resetDefaultButton.Location = new System.Drawing.Point(6, 9);
			this.resetDefaultButton.Name = "resetDefaultButton";
			this.resetDefaultButton.Size = new System.Drawing.Size(150, 25);
			this.resetDefaultButton.TabIndex = 2;
			this.resetDefaultButton.Text = "Reset settings to default";
			this.resetDefaultButton.Click += new System.EventHandler(this.resetDefaultButton_Click);
			// 
			// headerColorDialog
			// 
			this.headerColorDialog.AnyColor = true;
			this.headerColorDialog.FullOpen = true;
			// 
			// newColorDialog
			// 
			this.newColorDialog.AnyColor = true;
			this.newColorDialog.FullOpen = true;
			// 
			// oldColorDialog
			// 
			this.oldColorDialog.AnyColor = true;
			this.oldColorDialog.FullOpen = true;
			// 
			// refColorDialog
			// 
			this.refColorDialog.AnyColor = true;
			this.refColorDialog.FullOpen = true;
			// 
			// unknownColorDialog
			// 
			this.unknownColorDialog.AnyColor = true;
			this.unknownColorDialog.FullOpen = true;
			// 
			// valueColorDialog
			// 
			this.valueColorDialog.AnyColor = true;
			this.valueColorDialog.FullOpen = true;
			// 
			// FormSettings
			// 
			this.AcceptButton = this.applyButton;
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
			this.CancelButton = this.cancelButton;
			this.ClientSize = new System.Drawing.Size(480, 480);
			this.Controls.Add(this.darkGroupBox3);
			this.Controls.Add(this.darkGroupBox1);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
			this.MaximizeBox = false;
			this.Name = "FormSettings";
			this.ShowIcon = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "Settings";
			this.darkGroupBox1.ResumeLayout(false);
			this.darkGroupBox1.PerformLayout();
			this.darkGroupBox2.ResumeLayout(false);
			this.darkGroupBox2.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.fontSizeNumeric)).EndInit();
			this.darkGroupBox3.ResumeLayout(false);
			this.darkGroupBox3.PerformLayout();
			this.ResumeLayout(false);

		}

		#endregion

		private DarkUI.Controls.DarkButton applyButton;
		private DarkUI.Controls.DarkButton cancelButton;
		private DarkUI.Controls.DarkButton commentColorButton;
		private DarkUI.Controls.DarkButton headerColorButton;
		private DarkUI.Controls.DarkButton newColorButton;
		private DarkUI.Controls.DarkButton oldColorButton;
		private DarkUI.Controls.DarkButton refColorButton;
		private DarkUI.Controls.DarkButton reindentRulesButton;
		private DarkUI.Controls.DarkButton resetDefaultButton;
		private DarkUI.Controls.DarkButton unknownColorButton;
		private DarkUI.Controls.DarkButton valueColorButton;
		private DarkUI.Controls.DarkCheckBox autocompleteCheck;
		private DarkUI.Controls.DarkCheckBox closeBracketsCheck;
		private DarkUI.Controls.DarkCheckBox reindentCheck;
		private DarkUI.Controls.DarkCheckBox showNumbersCheck;
		private DarkUI.Controls.DarkCheckBox showSpacesCheck;
		private DarkUI.Controls.DarkCheckBox showToolbarCheck;
		private DarkUI.Controls.DarkCheckBox toolTipCheck;
		private DarkUI.Controls.DarkCheckBox wordWrapCheck;
		private DarkUI.Controls.DarkComboBox autosaveCombo;
		private DarkUI.Controls.DarkComboBox fontFaceCombo;
		private DarkUI.Controls.DarkGroupBox darkGroupBox1;
		private DarkUI.Controls.DarkGroupBox darkGroupBox2;
		private DarkUI.Controls.DarkGroupBox darkGroupBox3;
		private DarkUI.Controls.DarkLabel autoSaveLabel;
		private DarkUI.Controls.DarkLabel commentColorLabel;
		private DarkUI.Controls.DarkLabel faceLabel;
		private DarkUI.Controls.DarkLabel headerColorLabel;
		private DarkUI.Controls.DarkLabel newColorLabel;
		private DarkUI.Controls.DarkLabel oldColorLabel;
		private DarkUI.Controls.DarkLabel referencesColorLabel;
		private DarkUI.Controls.DarkLabel restartLabel;
		private DarkUI.Controls.DarkLabel sizeLabel;
		private DarkUI.Controls.DarkLabel unknownColorLabel;
		private DarkUI.Controls.DarkLabel valuesColorLabel;
		private DarkUI.Controls.DarkNumericUpDown fontSizeNumeric;
		private System.Windows.Forms.ColorDialog commentColorDialog;
		private System.Windows.Forms.ColorDialog headerColorDialog;
		private System.Windows.Forms.ColorDialog newColorDialog;
		private System.Windows.Forms.ColorDialog oldColorDialog;
		private System.Windows.Forms.ColorDialog refColorDialog;
		private System.Windows.Forms.ColorDialog unknownColorDialog;
		private System.Windows.Forms.ColorDialog valueColorDialog;
	}
}