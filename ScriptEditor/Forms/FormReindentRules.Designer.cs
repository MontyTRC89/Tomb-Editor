namespace ScriptEditor
{
	partial class FormReindentRules : DarkUI.Forms.DarkForm
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
			this.components = new System.ComponentModel.Container();
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormReindentRules));
			this.addSpacesGroupBox = new DarkUI.Controls.DarkGroupBox();
			this.commaLabel = new DarkUI.Controls.DarkLabel();
			this.equalLabel = new DarkUI.Controls.DarkLabel();
			this.postCommaSpaceCheck = new DarkUI.Controls.DarkCheckBox();
			this.preCommaSpaceCheck = new DarkUI.Controls.DarkCheckBox();
			this.postEqualSpaceCheck = new DarkUI.Controls.DarkCheckBox();
			this.preEqualSpaceCheck = new DarkUI.Controls.DarkCheckBox();
			this.buttonsGroupBox = new DarkUI.Controls.DarkGroupBox();
			this.defaultButton = new DarkUI.Controls.DarkButton();
			this.cancelButton = new DarkUI.Controls.DarkButton();
			this.saveButton = new DarkUI.Controls.DarkButton();
			this.mainGroupBox = new DarkUI.Controls.DarkGroupBox();
			this.previewGroupBox = new DarkUI.Controls.DarkGroupBox();
			this.preview = new FastColoredTextBoxNS.FastColoredTextBox();
			this.reduceSpacesCheck = new DarkUI.Controls.DarkCheckBox();
			this.addSpacesGroupBox.SuspendLayout();
			this.buttonsGroupBox.SuspendLayout();
			this.mainGroupBox.SuspendLayout();
			this.previewGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.preview)).BeginInit();
			this.SuspendLayout();
			// 
			// addSpacesGroupBox
			// 
			this.addSpacesGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.addSpacesGroupBox.Controls.Add(this.commaLabel);
			this.addSpacesGroupBox.Controls.Add(this.equalLabel);
			this.addSpacesGroupBox.Controls.Add(this.postCommaSpaceCheck);
			this.addSpacesGroupBox.Controls.Add(this.preCommaSpaceCheck);
			this.addSpacesGroupBox.Controls.Add(this.postEqualSpaceCheck);
			this.addSpacesGroupBox.Controls.Add(this.preEqualSpaceCheck);
			this.addSpacesGroupBox.Location = new System.Drawing.Point(9, 9);
			this.addSpacesGroupBox.Margin = new System.Windows.Forms.Padding(0, 0, 0, 4);
			this.addSpacesGroupBox.Name = "addSpacesGroupBox";
			this.addSpacesGroupBox.Size = new System.Drawing.Size(182, 121);
			this.addSpacesGroupBox.TabIndex = 6;
			this.addSpacesGroupBox.TabStop = false;
			this.addSpacesGroupBox.Text = "Insert spaces";
			// 
			// commaLabel
			// 
			this.commaLabel.AutoSize = true;
			this.commaLabel.Font = new System.Drawing.Font("Consolas", 15.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
			this.commaLabel.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
			this.commaLabel.Location = new System.Drawing.Point(80, 76);
			this.commaLabel.Margin = new System.Windows.Forms.Padding(22, 0, 22, 0);
			this.commaLabel.Name = "commaLabel";
			this.commaLabel.Size = new System.Drawing.Size(22, 24);
			this.commaLabel.TabIndex = 5;
			this.commaLabel.Text = ",";
			// 
			// equalLabel
			// 
			this.equalLabel.AutoSize = true;
			this.equalLabel.Font = new System.Drawing.Font("Consolas", 15.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
			this.equalLabel.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
			this.equalLabel.Location = new System.Drawing.Point(80, 30);
			this.equalLabel.Margin = new System.Windows.Forms.Padding(22, 0, 22, 0);
			this.equalLabel.Name = "equalLabel";
			this.equalLabel.Size = new System.Drawing.Size(22, 24);
			this.equalLabel.TabIndex = 4;
			this.equalLabel.Text = "=";
			// 
			// postCommaSpaceCheck
			// 
			this.postCommaSpaceCheck.AutoSize = true;
			this.postCommaSpaceCheck.Location = new System.Drawing.Point(124, 78);
			this.postCommaSpaceCheck.Margin = new System.Windows.Forms.Padding(0, 32, 40, 26);
			this.postCommaSpaceCheck.Name = "postCommaSpaceCheck";
			this.postCommaSpaceCheck.Size = new System.Drawing.Size(15, 14);
			this.postCommaSpaceCheck.TabIndex = 3;
			this.postCommaSpaceCheck.CheckedChanged += new System.EventHandler(this.postCommaSpaceCheck_CheckedChanged);
			// 
			// preCommaSpaceCheck
			// 
			this.preCommaSpaceCheck.AutoSize = true;
			this.preCommaSpaceCheck.Location = new System.Drawing.Point(43, 78);
			this.preCommaSpaceCheck.Margin = new System.Windows.Forms.Padding(40, 32, 0, 26);
			this.preCommaSpaceCheck.Name = "preCommaSpaceCheck";
			this.preCommaSpaceCheck.Size = new System.Drawing.Size(15, 14);
			this.preCommaSpaceCheck.TabIndex = 2;
			this.preCommaSpaceCheck.CheckedChanged += new System.EventHandler(this.preCommaSpaceCheck_CheckedChanged);
			// 
			// postEqualSpaceCheck
			// 
			this.postEqualSpaceCheck.AutoSize = true;
			this.postEqualSpaceCheck.Location = new System.Drawing.Point(124, 32);
			this.postEqualSpaceCheck.Margin = new System.Windows.Forms.Padding(0, 16, 40, 0);
			this.postEqualSpaceCheck.Name = "postEqualSpaceCheck";
			this.postEqualSpaceCheck.Size = new System.Drawing.Size(15, 14);
			this.postEqualSpaceCheck.TabIndex = 1;
			this.postEqualSpaceCheck.CheckedChanged += new System.EventHandler(this.postEqualSpaceCheck_CheckedChanged);
			// 
			// preEqualSpaceCheck
			// 
			this.preEqualSpaceCheck.AutoSize = true;
			this.preEqualSpaceCheck.Location = new System.Drawing.Point(43, 32);
			this.preEqualSpaceCheck.Margin = new System.Windows.Forms.Padding(40, 16, 0, 0);
			this.preEqualSpaceCheck.Name = "preEqualSpaceCheck";
			this.preEqualSpaceCheck.Size = new System.Drawing.Size(15, 14);
			this.preEqualSpaceCheck.TabIndex = 0;
			this.preEqualSpaceCheck.CheckedChanged += new System.EventHandler(this.preEqualSpaceCheck_CheckedChanged);
			// 
			// buttonsGroupBox
			// 
			this.buttonsGroupBox.Controls.Add(this.defaultButton);
			this.buttonsGroupBox.Controls.Add(this.cancelButton);
			this.buttonsGroupBox.Controls.Add(this.saveButton);
			this.buttonsGroupBox.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.buttonsGroupBox.Location = new System.Drawing.Point(0, 160);
			this.buttonsGroupBox.Name = "buttonsGroupBox";
			this.buttonsGroupBox.Size = new System.Drawing.Size(570, 40);
			this.buttonsGroupBox.TabIndex = 7;
			this.buttonsGroupBox.TabStop = false;
			// 
			// defaultButton
			// 
			this.defaultButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.defaultButton.Location = new System.Drawing.Point(6, 9);
			this.defaultButton.Name = "defaultButton";
			this.defaultButton.Size = new System.Drawing.Size(128, 25);
			this.defaultButton.TabIndex = 6;
			this.defaultButton.Text = "Reset rules to default";
			this.defaultButton.Click += new System.EventHandler(this.defaultButton_Click);
			// 
			// cancelButton
			// 
			this.cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.cancelButton.Location = new System.Drawing.Point(489, 9);
			this.cancelButton.Name = "cancelButton";
			this.cancelButton.Size = new System.Drawing.Size(75, 25);
			this.cancelButton.TabIndex = 5;
			this.cancelButton.Text = "Cancel";
			// 
			// saveButton
			// 
			this.saveButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.saveButton.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.saveButton.Location = new System.Drawing.Point(408, 9);
			this.saveButton.Name = "saveButton";
			this.saveButton.Size = new System.Drawing.Size(75, 25);
			this.saveButton.TabIndex = 4;
			this.saveButton.Text = "Save";
			this.saveButton.Click += new System.EventHandler(this.saveButton_Click);
			// 
			// mainGroupBox
			// 
			this.mainGroupBox.Controls.Add(this.previewGroupBox);
			this.mainGroupBox.Controls.Add(this.addSpacesGroupBox);
			this.mainGroupBox.Controls.Add(this.reduceSpacesCheck);
			this.mainGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.mainGroupBox.Location = new System.Drawing.Point(0, 0);
			this.mainGroupBox.Name = "mainGroupBox";
			this.mainGroupBox.Size = new System.Drawing.Size(570, 160);
			this.mainGroupBox.TabIndex = 8;
			this.mainGroupBox.TabStop = false;
			// 
			// previewGroupBox
			// 
			this.previewGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.previewGroupBox.Controls.Add(this.preview);
			this.previewGroupBox.Location = new System.Drawing.Point(194, 9);
			this.previewGroupBox.Margin = new System.Windows.Forms.Padding(3, 0, 0, 3);
			this.previewGroupBox.Name = "previewGroupBox";
			this.previewGroupBox.Size = new System.Drawing.Size(367, 145);
			this.previewGroupBox.TabIndex = 7;
			this.previewGroupBox.TabStop = false;
			this.previewGroupBox.Text = "Preview";
			// 
			// preview
			// 
			this.preview.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.preview.AutoCompleteBracketsList = new char[0];
			this.preview.AutoIndentCharsPatterns = "\r\n";
			this.preview.AutoScrollMinSize = new System.Drawing.Size(305, 108);
			this.preview.BackBrush = null;
			this.preview.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(32)))), ((int)(((byte)(32)))), ((int)(((byte)(32)))));
			this.preview.BookmarkColor = System.Drawing.Color.Transparent;
			this.preview.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.preview.CaretColor = System.Drawing.Color.Transparent;
			this.preview.CaretVisible = false;
			this.preview.CharHeight = 18;
			this.preview.CharWidth = 9;
			this.preview.Cursor = System.Windows.Forms.Cursors.IBeam;
			this.preview.DisabledColor = System.Drawing.Color.Transparent;
			this.preview.Enabled = false;
			this.preview.FoldingIndicatorColor = System.Drawing.Color.Transparent;
			this.preview.Font = new System.Drawing.Font("Consolas", 12F);
			this.preview.IndentBackColor = System.Drawing.Color.Transparent;
			this.preview.IsReplaceMode = false;
			this.preview.LeftPadding = 6;
			this.preview.LineNumberColor = System.Drawing.Color.Transparent;
			this.preview.Location = new System.Drawing.Point(6, 19);
			this.preview.Name = "preview";
			this.preview.Paddings = new System.Windows.Forms.Padding(0);
			this.preview.ReadOnly = true;
			this.preview.SelectionColor = System.Drawing.Color.Transparent;
			this.preview.ServiceColors = ((FastColoredTextBoxNS.ServiceColors)(resources.GetObject("preview.ServiceColors")));
			this.preview.ServiceLinesColor = System.Drawing.Color.Transparent;
			this.preview.ShowLineNumbers = false;
			this.preview.ShowScrollBars = false;
			this.preview.Size = new System.Drawing.Size(355, 120);
			this.preview.TabIndex = 0;
			this.preview.Text = "[Level]\r\nName=Coastal Ruins\r\nRain=ENABLED\r\nLayer1=128,128,128,-8\r\nMirror=69,$7400" +
    "   ; Crossbow room\r\nLevel=DATA\\COASTAL,105";
			this.preview.TextAreaBorderColor = System.Drawing.Color.Transparent;
			this.preview.Zoom = 100;
			// 
			// reduceSpacesCheck
			// 
			this.reduceSpacesCheck.AutoSize = true;
			this.reduceSpacesCheck.Location = new System.Drawing.Point(16, 137);
			this.reduceSpacesCheck.Margin = new System.Windows.Forms.Padding(3, 3, 12, 3);
			this.reduceSpacesCheck.Name = "reduceSpacesCheck";
			this.reduceSpacesCheck.Size = new System.Drawing.Size(169, 17);
			this.reduceSpacesCheck.TabIndex = 3;
			this.reduceSpacesCheck.Text = "Reduce the amount of spaces";
			this.reduceSpacesCheck.CheckedChanged += new System.EventHandler(this.reduceSpacesCheck_CheckedChanged);
			// 
			// FormReindentRules
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(570, 200);
			this.Controls.Add(this.mainGroupBox);
			this.Controls.Add(this.buttonsGroupBox);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
			this.Name = "FormReindentRules";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "Reindent rules";
			this.addSpacesGroupBox.ResumeLayout(false);
			this.addSpacesGroupBox.PerformLayout();
			this.buttonsGroupBox.ResumeLayout(false);
			this.mainGroupBox.ResumeLayout(false);
			this.mainGroupBox.PerformLayout();
			this.previewGroupBox.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.preview)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

		private DarkUI.Controls.DarkButton cancelButton;
		private DarkUI.Controls.DarkButton defaultButton;
		private DarkUI.Controls.DarkButton saveButton;
		private DarkUI.Controls.DarkCheckBox postCommaSpaceCheck;
		private DarkUI.Controls.DarkCheckBox postEqualSpaceCheck;
		private DarkUI.Controls.DarkCheckBox preCommaSpaceCheck;
		private DarkUI.Controls.DarkCheckBox preEqualSpaceCheck;
		private DarkUI.Controls.DarkCheckBox reduceSpacesCheck;
		private DarkUI.Controls.DarkGroupBox addSpacesGroupBox;
		private DarkUI.Controls.DarkGroupBox buttonsGroupBox;
		private DarkUI.Controls.DarkGroupBox mainGroupBox;
		private DarkUI.Controls.DarkGroupBox previewGroupBox;
		private DarkUI.Controls.DarkLabel commaLabel;
		private DarkUI.Controls.DarkLabel equalLabel;
		private FastColoredTextBoxNS.FastColoredTextBox preview;
	}
}