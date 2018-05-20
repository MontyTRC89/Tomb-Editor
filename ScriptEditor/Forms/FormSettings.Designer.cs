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
			this.sizeLabel = new DarkUI.Controls.DarkLabel();
			this.fontSizeNumeric = new DarkUI.Controls.DarkNumericUpDown();
			this.darkGroupBox1 = new DarkUI.Controls.DarkGroupBox();
			this.toolTipCheck = new DarkUI.Controls.DarkCheckBox();
			this.autocompleteCheck = new DarkUI.Controls.DarkCheckBox();
			this.wordWrapCheck = new DarkUI.Controls.DarkCheckBox();
			this.showStatusCheck = new DarkUI.Controls.DarkCheckBox();
			this.reindentCheck = new DarkUI.Controls.DarkCheckBox();
			this.closeBracketsCheck = new DarkUI.Controls.DarkCheckBox();
			this.showSpacesCheck = new DarkUI.Controls.DarkCheckBox();
			this.showToolbarCheck = new DarkUI.Controls.DarkCheckBox();
			this.darkGroupBox2 = new DarkUI.Controls.DarkGroupBox();
			this.unknownColorButton = new DarkUI.Controls.DarkButton();
			this.keyColorButton = new DarkUI.Controls.DarkButton();
			this.headerColorButton = new DarkUI.Controls.DarkButton();
			this.valueColorButton = new DarkUI.Controls.DarkButton();
			this.refColorButton = new DarkUI.Controls.DarkButton();
			this.commentColorButton = new DarkUI.Controls.DarkButton();
			this.unknownColorLabel = new DarkUI.Controls.DarkLabel();
			this.keyColorLabel = new DarkUI.Controls.DarkLabel();
			this.headerColorLabel = new DarkUI.Controls.DarkLabel();
			this.valuesColorLabel = new DarkUI.Controls.DarkLabel();
			this.referencesColorLabel = new DarkUI.Controls.DarkLabel();
			this.commentColorLabel = new DarkUI.Controls.DarkLabel();
			this.autoSaveCombo = new DarkUI.Controls.DarkComboBox();
			this.autoSaveLabel = new DarkUI.Controls.DarkLabel();
			this.fontFaceCombo = new DarkUI.Controls.DarkComboBox();
			this.faceLabel = new DarkUI.Controls.DarkLabel();
			this.darkGroupBox3 = new DarkUI.Controls.DarkGroupBox();
			this.restartLabel = new DarkUI.Controls.DarkLabel();
			this.resetDefaultButton = new DarkUI.Controls.DarkButton();
			this.applyButton = new DarkUI.Controls.DarkButton();
			this.cancelButton = new DarkUI.Controls.DarkButton();
			this.commentColorDialog = new System.Windows.Forms.ColorDialog();
			this.refColorDialog = new System.Windows.Forms.ColorDialog();
			this.valueColorDialog = new System.Windows.Forms.ColorDialog();
			this.headerColorDialog = new System.Windows.Forms.ColorDialog();
			this.keyColorDialog = new System.Windows.Forms.ColorDialog();
			this.unknownColorDialog = new System.Windows.Forms.ColorDialog();
			((System.ComponentModel.ISupportInitialize)(this.fontSizeNumeric)).BeginInit();
			this.darkGroupBox1.SuspendLayout();
			this.darkGroupBox2.SuspendLayout();
			this.darkGroupBox3.SuspendLayout();
			this.SuspendLayout();
			// 
			// sizeLabel
			// 
			this.sizeLabel.AutoSize = true;
			this.sizeLabel.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
			this.sizeLabel.Location = new System.Drawing.Point(6, 16);
			this.sizeLabel.Name = "sizeLabel";
			this.sizeLabel.Size = new System.Drawing.Size(54, 13);
			this.sizeLabel.TabIndex = 0;
			this.sizeLabel.Text = "Font Size:";
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
			// darkGroupBox1
			// 
			this.darkGroupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.darkGroupBox1.Controls.Add(this.toolTipCheck);
			this.darkGroupBox1.Controls.Add(this.autocompleteCheck);
			this.darkGroupBox1.Controls.Add(this.wordWrapCheck);
			this.darkGroupBox1.Controls.Add(this.showStatusCheck);
			this.darkGroupBox1.Controls.Add(this.reindentCheck);
			this.darkGroupBox1.Controls.Add(this.closeBracketsCheck);
			this.darkGroupBox1.Controls.Add(this.showSpacesCheck);
			this.darkGroupBox1.Controls.Add(this.showToolbarCheck);
			this.darkGroupBox1.Controls.Add(this.darkGroupBox2);
			this.darkGroupBox1.Controls.Add(this.autoSaveCombo);
			this.darkGroupBox1.Controls.Add(this.autoSaveLabel);
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
			// toolTipCheck
			// 
			this.toolTipCheck.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.toolTipCheck.AutoSize = true;
			this.toolTipCheck.Location = new System.Drawing.Point(6, 307);
			this.toolTipCheck.Name = "toolTipCheck";
			this.toolTipCheck.Size = new System.Drawing.Size(97, 17);
			this.toolTipCheck.TabIndex = 15;
			this.toolTipCheck.Text = "Show ToolTips";
			this.toolTipCheck.CheckedChanged += new System.EventHandler(this.toolTipCheck_CheckedChanged);
			// 
			// autocompleteCheck
			// 
			this.autocompleteCheck.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.autocompleteCheck.AutoSize = true;
			this.autocompleteCheck.Location = new System.Drawing.Point(6, 283);
			this.autocompleteCheck.Name = "autocompleteCheck";
			this.autocompleteCheck.Size = new System.Drawing.Size(127, 17);
			this.autocompleteCheck.TabIndex = 13;
			this.autocompleteCheck.Text = "Enable Autocomplete";
			this.autocompleteCheck.CheckedChanged += new System.EventHandler(this.autocompleteCheck_CheckedChanged);
			// 
			// wordWrapCheck
			// 
			this.wordWrapCheck.AutoSize = true;
			this.wordWrapCheck.Location = new System.Drawing.Point(6, 231);
			this.wordWrapCheck.Name = "wordWrapCheck";
			this.wordWrapCheck.Size = new System.Drawing.Size(81, 17);
			this.wordWrapCheck.TabIndex = 12;
			this.wordWrapCheck.Text = "Word Wrap";
			// 
			// showStatusCheck
			// 
			this.showStatusCheck.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.showStatusCheck.AutoSize = true;
			this.showStatusCheck.Location = new System.Drawing.Point(6, 389);
			this.showStatusCheck.Name = "showStatusCheck";
			this.showStatusCheck.Size = new System.Drawing.Size(101, 17);
			this.showStatusCheck.TabIndex = 11;
			this.showStatusCheck.Text = "Show Statusbar";
			// 
			// reindentCheck
			// 
			this.reindentCheck.AutoSize = true;
			this.reindentCheck.Location = new System.Drawing.Point(6, 159);
			this.reindentCheck.Name = "reindentCheck";
			this.reindentCheck.Size = new System.Drawing.Size(112, 17);
			this.reindentCheck.TabIndex = 10;
			this.reindentCheck.Text = "Reindent on Save";
			// 
			// closeBracketsCheck
			// 
			this.closeBracketsCheck.AutoSize = true;
			this.closeBracketsCheck.Location = new System.Drawing.Point(6, 183);
			this.closeBracketsCheck.Name = "closeBracketsCheck";
			this.closeBracketsCheck.Size = new System.Drawing.Size(133, 17);
			this.closeBracketsCheck.TabIndex = 9;
			this.closeBracketsCheck.Text = "Auto close [ ] Brackets";
			// 
			// showSpacesCheck
			// 
			this.showSpacesCheck.AutoSize = true;
			this.showSpacesCheck.Location = new System.Drawing.Point(6, 207);
			this.showSpacesCheck.Name = "showSpacesCheck";
			this.showSpacesCheck.Size = new System.Drawing.Size(92, 17);
			this.showSpacesCheck.TabIndex = 8;
			this.showSpacesCheck.Text = "Show Spaces";
			// 
			// showToolbarCheck
			// 
			this.showToolbarCheck.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.showToolbarCheck.AutoSize = true;
			this.showToolbarCheck.Location = new System.Drawing.Point(6, 365);
			this.showToolbarCheck.Name = "showToolbarCheck";
			this.showToolbarCheck.Size = new System.Drawing.Size(92, 17);
			this.showToolbarCheck.TabIndex = 7;
			this.showToolbarCheck.Text = "Show Toolbar";
			// 
			// darkGroupBox2
			// 
			this.darkGroupBox2.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.darkGroupBox2.Controls.Add(this.unknownColorButton);
			this.darkGroupBox2.Controls.Add(this.keyColorButton);
			this.darkGroupBox2.Controls.Add(this.headerColorButton);
			this.darkGroupBox2.Controls.Add(this.valueColorButton);
			this.darkGroupBox2.Controls.Add(this.refColorButton);
			this.darkGroupBox2.Controls.Add(this.commentColorButton);
			this.darkGroupBox2.Controls.Add(this.unknownColorLabel);
			this.darkGroupBox2.Controls.Add(this.keyColorLabel);
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
			// unknownColorButton
			// 
			this.unknownColorButton.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.unknownColorButton.BackColor = System.Drawing.Color.Red;
			this.unknownColorButton.BackColorUseGeneric = false;
			this.unknownColorButton.Location = new System.Drawing.Point(6, 361);
			this.unknownColorButton.Name = "unknownColorButton";
			this.unknownColorButton.Size = new System.Drawing.Size(276, 25);
			this.unknownColorButton.TabIndex = 23;
			this.unknownColorButton.Click += new System.EventHandler(this.unknownColorButton_Click);
			// 
			// keyColorButton
			// 
			this.keyColorButton.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.keyColorButton.BackColor = System.Drawing.Color.MediumAquamarine;
			this.keyColorButton.BackColorUseGeneric = false;
			this.keyColorButton.Location = new System.Drawing.Point(6, 299);
			this.keyColorButton.Name = "keyColorButton";
			this.keyColorButton.Size = new System.Drawing.Size(276, 25);
			this.keyColorButton.TabIndex = 22;
			this.keyColorButton.Click += new System.EventHandler(this.keyColorButton_Click);
			// 
			// headerColorButton
			// 
			this.headerColorButton.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.headerColorButton.BackColor = System.Drawing.Color.SteelBlue;
			this.headerColorButton.BackColorUseGeneric = false;
			this.headerColorButton.Location = new System.Drawing.Point(6, 237);
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
			this.valueColorButton.Location = new System.Drawing.Point(6, 175);
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
			this.refColorButton.Location = new System.Drawing.Point(6, 113);
			this.refColorButton.Name = "refColorButton";
			this.refColorButton.Size = new System.Drawing.Size(276, 25);
			this.refColorButton.TabIndex = 19;
			this.refColorButton.Click += new System.EventHandler(this.refColorButton_Click);
			// 
			// commentColorButton
			// 
			this.commentColorButton.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.commentColorButton.BackColor = System.Drawing.Color.Green;
			this.commentColorButton.BackColorUseGeneric = false;
			this.commentColorButton.Location = new System.Drawing.Point(6, 51);
			this.commentColorButton.Name = "commentColorButton";
			this.commentColorButton.Size = new System.Drawing.Size(276, 25);
			this.commentColorButton.TabIndex = 18;
			this.commentColorButton.Click += new System.EventHandler(this.commentColorButton_Click);
			// 
			// unknownColorLabel
			// 
			this.unknownColorLabel.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.unknownColorLabel.AutoSize = true;
			this.unknownColorLabel.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
			this.unknownColorLabel.Location = new System.Drawing.Point(6, 336);
			this.unknownColorLabel.Name = "unknownColorLabel";
			this.unknownColorLabel.Size = new System.Drawing.Size(56, 13);
			this.unknownColorLabel.TabIndex = 17;
			this.unknownColorLabel.Text = "Unknown:";
			// 
			// keyColorLabel
			// 
			this.keyColorLabel.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.keyColorLabel.AutoSize = true;
			this.keyColorLabel.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
			this.keyColorLabel.Location = new System.Drawing.Point(6, 274);
			this.keyColorLabel.Name = "keyColorLabel";
			this.keyColorLabel.Size = new System.Drawing.Size(62, 13);
			this.keyColorLabel.TabIndex = 16;
			this.keyColorLabel.Text = "Key values:";
			// 
			// headerColorLabel
			// 
			this.headerColorLabel.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.headerColorLabel.AutoSize = true;
			this.headerColorLabel.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
			this.headerColorLabel.Location = new System.Drawing.Point(6, 212);
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
			this.valuesColorLabel.Location = new System.Drawing.Point(6, 150);
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
			this.referencesColorLabel.Location = new System.Drawing.Point(6, 88);
			this.referencesColorLabel.Name = "referencesColorLabel";
			this.referencesColorLabel.Size = new System.Drawing.Size(65, 13);
			this.referencesColorLabel.TabIndex = 13;
			this.referencesColorLabel.Text = "References:";
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
			// autoSaveCombo
			// 
			this.autoSaveCombo.FormattingEnabled = true;
			this.autoSaveCombo.Items.AddRange(new object[] {
            "None."});
			this.autoSaveCombo.Location = new System.Drawing.Point(6, 131);
			this.autoSaveCombo.Name = "autoSaveCombo";
			this.autoSaveCombo.Size = new System.Drawing.Size(150, 21);
			this.autoSaveCombo.TabIndex = 5;
			// 
			// autoSaveLabel
			// 
			this.autoSaveLabel.AutoSize = true;
			this.autoSaveLabel.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
			this.autoSaveLabel.Location = new System.Drawing.Point(6, 111);
			this.autoSaveLabel.Name = "autoSaveLabel";
			this.autoSaveLabel.Size = new System.Drawing.Size(85, 13);
			this.autoSaveLabel.TabIndex = 4;
			this.autoSaveLabel.Text = "Auto Save (min):";
			// 
			// fontFaceCombo
			// 
			this.fontFaceCombo.FormattingEnabled = true;
			this.fontFaceCombo.Items.AddRange(new object[] {
            "Consolas",
            "Courier New",
            "Lucida Console",
            "Lucida Sans Typewriter",
            "MingLiU-ExtB",
            "MS Gothic",
            "MS Mincho",
            "OCR A Extended",
            "SimSun",
            "SimSun-ExtB"});
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
			this.faceLabel.Size = new System.Drawing.Size(58, 13);
			this.faceLabel.TabIndex = 2;
			this.faceLabel.Text = "Font Face:";
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
			this.restartLabel.Location = new System.Drawing.Point(227, 15);
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
			// commentColorDialog
			// 
			this.commentColorDialog.AnyColor = true;
			this.commentColorDialog.FullOpen = true;
			// 
			// refColorDialog
			// 
			this.refColorDialog.AnyColor = true;
			this.refColorDialog.FullOpen = true;
			// 
			// valueColorDialog
			// 
			this.valueColorDialog.AnyColor = true;
			this.valueColorDialog.FullOpen = true;
			// 
			// headerColorDialog
			// 
			this.headerColorDialog.AnyColor = true;
			this.headerColorDialog.FullOpen = true;
			// 
			// keyColorDialog
			// 
			this.keyColorDialog.AnyColor = true;
			this.keyColorDialog.FullOpen = true;
			// 
			// unknownColorDialog
			// 
			this.unknownColorDialog.AnyColor = true;
			this.unknownColorDialog.FullOpen = true;
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
			this.Text = "FormSettings";
			((System.ComponentModel.ISupportInitialize)(this.fontSizeNumeric)).EndInit();
			this.darkGroupBox1.ResumeLayout(false);
			this.darkGroupBox1.PerformLayout();
			this.darkGroupBox2.ResumeLayout(false);
			this.darkGroupBox2.PerformLayout();
			this.darkGroupBox3.ResumeLayout(false);
			this.darkGroupBox3.PerformLayout();
			this.ResumeLayout(false);

		}

		#endregion

		private DarkUI.Controls.DarkLabel sizeLabel;
		private DarkUI.Controls.DarkNumericUpDown fontSizeNumeric;
		private DarkUI.Controls.DarkGroupBox darkGroupBox1;
		private DarkUI.Controls.DarkComboBox fontFaceCombo;
		private DarkUI.Controls.DarkLabel faceLabel;
		private DarkUI.Controls.DarkComboBox autoSaveCombo;
		private DarkUI.Controls.DarkLabel autoSaveLabel;
		private DarkUI.Controls.DarkGroupBox darkGroupBox2;
		private DarkUI.Controls.DarkCheckBox reindentCheck;
		private DarkUI.Controls.DarkCheckBox closeBracketsCheck;
		private DarkUI.Controls.DarkCheckBox showSpacesCheck;
		private DarkUI.Controls.DarkCheckBox showToolbarCheck;
		private DarkUI.Controls.DarkCheckBox showStatusCheck;
		private DarkUI.Controls.DarkGroupBox darkGroupBox3;
		private DarkUI.Controls.DarkButton resetDefaultButton;
		private DarkUI.Controls.DarkButton applyButton;
		private DarkUI.Controls.DarkButton cancelButton;
		private DarkUI.Controls.DarkLabel referencesColorLabel;
		private DarkUI.Controls.DarkLabel commentColorLabel;
		private DarkUI.Controls.DarkLabel valuesColorLabel;
		private DarkUI.Controls.DarkLabel headerColorLabel;
		private DarkUI.Controls.DarkLabel unknownColorLabel;
		private DarkUI.Controls.DarkLabel keyColorLabel;
		private DarkUI.Controls.DarkButton unknownColorButton;
		private DarkUI.Controls.DarkButton keyColorButton;
		private DarkUI.Controls.DarkButton headerColorButton;
		private DarkUI.Controls.DarkButton valueColorButton;
		private DarkUI.Controls.DarkButton refColorButton;
		private DarkUI.Controls.DarkButton commentColorButton;
		private DarkUI.Controls.DarkCheckBox wordWrapCheck;
		private DarkUI.Controls.DarkCheckBox toolTipCheck;
		private DarkUI.Controls.DarkCheckBox autocompleteCheck;
		private System.Windows.Forms.ColorDialog commentColorDialog;
		private System.Windows.Forms.ColorDialog refColorDialog;
		private System.Windows.Forms.ColorDialog valueColorDialog;
		private System.Windows.Forms.ColorDialog headerColorDialog;
		private System.Windows.Forms.ColorDialog keyColorDialog;
		private System.Windows.Forms.ColorDialog unknownColorDialog;
		private DarkUI.Controls.DarkLabel restartLabel;
	}
}