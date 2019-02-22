namespace ScriptEditor
{
    partial class FormProjectSetup : DarkUI.Forms.DarkForm
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormProjectSetup));
			this.applyButton = new DarkUI.Controls.DarkButton();
			this.engineTypeLabel = new DarkUI.Controls.DarkLabel();
			this.gameBrowseButton = new DarkUI.Controls.DarkButton();
			this.gamePathLabel = new DarkUI.Controls.DarkLabel();
			this.gamePathTextBox = new DarkUI.Controls.DarkTextBox();
			this.nameTextBox = new DarkUI.Controls.DarkTextBox();
			this.ngCenterBrowseButton = new DarkUI.Controls.DarkButton();
			this.ngCenterPathTextBox = new DarkUI.Controls.DarkTextBox();
			this.ngFolderLabel = new DarkUI.Controls.DarkLabel();
			this.projectNameGroup = new DarkUI.Controls.DarkGroupBox();
			this.projectNameLabel = new DarkUI.Controls.DarkLabel();
			this.projectSetupGroup = new DarkUI.Controls.DarkGroupBox();
			this.tr5mainRadioButton = new DarkUI.Controls.DarkRadioButton();
			this.tr5RadioButton = new DarkUI.Controls.DarkRadioButton();
			this.trngRadioButton = new DarkUI.Controls.DarkRadioButton();
			this.tr4RadioButton = new DarkUI.Controls.DarkRadioButton();
			this.projectNameGroup.SuspendLayout();
			this.projectSetupGroup.SuspendLayout();
			this.SuspendLayout();
			// 
			// applyButton
			// 
			this.applyButton.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
			this.applyButton.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.applyButton.Location = new System.Drawing.Point(9, 165);
			this.applyButton.Name = "applyButton";
			this.applyButton.Size = new System.Drawing.Size(460, 24);
			this.applyButton.TabIndex = 4;
			this.applyButton.Text = "Add Project";
			this.applyButton.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
			this.applyButton.Click += new System.EventHandler(this.applyButton_Click);
			// 
			// engineTypeLabel
			// 
			this.engineTypeLabel.AutoSize = true;
			this.engineTypeLabel.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
			this.engineTypeLabel.Location = new System.Drawing.Point(6, 44);
			this.engineTypeLabel.Margin = new System.Windows.Forms.Padding(3, 12, 3, 12);
			this.engineTypeLabel.Name = "engineTypeLabel";
			this.engineTypeLabel.Size = new System.Drawing.Size(66, 13);
			this.engineTypeLabel.TabIndex = 18;
			this.engineTypeLabel.Text = "Engine type:";
			// 
			// gameBrowseButton
			// 
			this.gameBrowseButton.Image = ((System.Drawing.Image)(resources.GetObject("gameBrowseButton.Image")));
			this.gameBrowseButton.Location = new System.Drawing.Point(379, 11);
			this.gameBrowseButton.Name = "gameBrowseButton";
			this.gameBrowseButton.Size = new System.Drawing.Size(75, 22);
			this.gameBrowseButton.TabIndex = 3;
			this.gameBrowseButton.Text = "Browse...";
			this.gameBrowseButton.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
			this.gameBrowseButton.Click += new System.EventHandler(this.gameBrowseButton_Click);
			// 
			// gamePathLabel
			// 
			this.gamePathLabel.AutoSize = true;
			this.gamePathLabel.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
			this.gamePathLabel.Location = new System.Drawing.Point(6, 15);
			this.gamePathLabel.Margin = new System.Windows.Forms.Padding(3, 0, 3, 3);
			this.gamePathLabel.Name = "gamePathLabel";
			this.gamePathLabel.Size = new System.Drawing.Size(77, 13);
			this.gamePathLabel.TabIndex = 6;
			this.gamePathLabel.Text = "Game .exe file:";
			// 
			// gamePathTextBox
			// 
			this.gamePathTextBox.Location = new System.Drawing.Point(84, 13);
			this.gamePathTextBox.Margin = new System.Windows.Forms.Padding(3, 0, 3, 3);
			this.gamePathTextBox.Name = "gamePathTextBox";
			this.gamePathTextBox.Size = new System.Drawing.Size(289, 20);
			this.gamePathTextBox.TabIndex = 1;
			this.gamePathTextBox.TextChanged += new System.EventHandler(this.gamePathTextBox_TextChanged);
			// 
			// nameTextBox
			// 
			this.nameTextBox.Location = new System.Drawing.Point(84, 13);
			this.nameTextBox.Margin = new System.Windows.Forms.Padding(3, 0, 3, 3);
			this.nameTextBox.Name = "nameTextBox";
			this.nameTextBox.Size = new System.Drawing.Size(370, 20);
			this.nameTextBox.TabIndex = 7;
			// 
			// ngCenterBrowseButton
			// 
			this.ngCenterBrowseButton.Image = ((System.Drawing.Image)(resources.GetObject("ngCenterBrowseButton.Image")));
			this.ngCenterBrowseButton.Location = new System.Drawing.Point(379, 68);
			this.ngCenterBrowseButton.Name = "ngCenterBrowseButton";
			this.ngCenterBrowseButton.Size = new System.Drawing.Size(75, 22);
			this.ngCenterBrowseButton.TabIndex = 8;
			this.ngCenterBrowseButton.Text = "Browse...";
			this.ngCenterBrowseButton.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
			this.ngCenterBrowseButton.Click += new System.EventHandler(this.ngCenterBrowseButton_Click);
			// 
			// ngCenterPathTextBox
			// 
			this.ngCenterPathTextBox.Location = new System.Drawing.Point(104, 70);
			this.ngCenterPathTextBox.Name = "ngCenterPathTextBox";
			this.ngCenterPathTextBox.Size = new System.Drawing.Size(269, 20);
			this.ngCenterPathTextBox.TabIndex = 7;
			// 
			// ngFolderLabel
			// 
			this.ngFolderLabel.AutoSize = true;
			this.ngFolderLabel.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
			this.ngFolderLabel.Location = new System.Drawing.Point(6, 73);
			this.ngFolderLabel.Margin = new System.Windows.Forms.Padding(3);
			this.ngFolderLabel.Name = "ngFolderLabel";
			this.ngFolderLabel.Size = new System.Drawing.Size(92, 13);
			this.ngFolderLabel.TabIndex = 9;
			this.ngFolderLabel.Text = "NG_Center folder:";
			// 
			// projectNameGroup
			// 
			this.projectNameGroup.Controls.Add(this.projectNameLabel);
			this.projectNameGroup.Controls.Add(this.nameTextBox);
			this.projectNameGroup.Location = new System.Drawing.Point(9, 9);
			this.projectNameGroup.Margin = new System.Windows.Forms.Padding(0);
			this.projectNameGroup.Name = "projectNameGroup";
			this.projectNameGroup.Padding = new System.Windows.Forms.Padding(3, 0, 3, 9);
			this.projectNameGroup.Size = new System.Drawing.Size(460, 45);
			this.projectNameGroup.TabIndex = 14;
			this.projectNameGroup.TabStop = false;
			// 
			// projectNameLabel
			// 
			this.projectNameLabel.AutoSize = true;
			this.projectNameLabel.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
			this.projectNameLabel.Location = new System.Drawing.Point(6, 15);
			this.projectNameLabel.Margin = new System.Windows.Forms.Padding(3, 0, 3, 3);
			this.projectNameLabel.Name = "projectNameLabel";
			this.projectNameLabel.Size = new System.Drawing.Size(72, 13);
			this.projectNameLabel.TabIndex = 19;
			this.projectNameLabel.Text = "Project name:";
			// 
			// projectSetupGroup
			// 
			this.projectSetupGroup.Controls.Add(this.gamePathTextBox);
			this.projectSetupGroup.Controls.Add(this.engineTypeLabel);
			this.projectSetupGroup.Controls.Add(this.gameBrowseButton);
			this.projectSetupGroup.Controls.Add(this.tr5mainRadioButton);
			this.projectSetupGroup.Controls.Add(this.gamePathLabel);
			this.projectSetupGroup.Controls.Add(this.tr5RadioButton);
			this.projectSetupGroup.Controls.Add(this.trngRadioButton);
			this.projectSetupGroup.Controls.Add(this.ngFolderLabel);
			this.projectSetupGroup.Controls.Add(this.ngCenterPathTextBox);
			this.projectSetupGroup.Controls.Add(this.tr4RadioButton);
			this.projectSetupGroup.Controls.Add(this.ngCenterBrowseButton);
			this.projectSetupGroup.Location = new System.Drawing.Point(9, 57);
			this.projectSetupGroup.Margin = new System.Windows.Forms.Padding(0, 3, 0, 3);
			this.projectSetupGroup.Name = "projectSetupGroup";
			this.projectSetupGroup.Padding = new System.Windows.Forms.Padding(3, 0, 3, 9);
			this.projectSetupGroup.Size = new System.Drawing.Size(460, 102);
			this.projectSetupGroup.TabIndex = 15;
			this.projectSetupGroup.TabStop = false;
			// 
			// tr5mainRadioButton
			// 
			this.tr5mainRadioButton.AutoSize = true;
			this.tr5mainRadioButton.Location = new System.Drawing.Point(244, 42);
			this.tr5mainRadioButton.Name = "tr5mainRadioButton";
			this.tr5mainRadioButton.Size = new System.Drawing.Size(69, 17);
			this.tr5mainRadioButton.TabIndex = 17;
			this.tr5mainRadioButton.TabStop = true;
			this.tr5mainRadioButton.Text = "TR5Main";
			// 
			// tr5RadioButton
			// 
			this.tr5RadioButton.AutoSize = true;
			this.tr5RadioButton.Location = new System.Drawing.Point(192, 42);
			this.tr5RadioButton.Name = "tr5RadioButton";
			this.tr5RadioButton.Size = new System.Drawing.Size(46, 17);
			this.tr5RadioButton.TabIndex = 16;
			this.tr5RadioButton.TabStop = true;
			this.tr5RadioButton.Text = "TR5";
			// 
			// trngRadioButton
			// 
			this.trngRadioButton.AutoSize = true;
			this.trngRadioButton.Location = new System.Drawing.Point(130, 42);
			this.trngRadioButton.Name = "trngRadioButton";
			this.trngRadioButton.Size = new System.Drawing.Size(56, 17);
			this.trngRadioButton.TabIndex = 15;
			this.trngRadioButton.TabStop = true;
			this.trngRadioButton.Text = "TRNG";
			this.trngRadioButton.CheckedChanged += new System.EventHandler(this.trngRadioButton_CheckedChanged);
			// 
			// tr4RadioButton
			// 
			this.tr4RadioButton.AutoSize = true;
			this.tr4RadioButton.Location = new System.Drawing.Point(78, 42);
			this.tr4RadioButton.Name = "tr4RadioButton";
			this.tr4RadioButton.Size = new System.Drawing.Size(46, 17);
			this.tr4RadioButton.TabIndex = 14;
			this.tr4RadioButton.TabStop = true;
			this.tr4RadioButton.Text = "TR4";
			// 
			// FormProjectSetup
			// 
			this.AcceptButton = this.applyButton;
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
			this.ClientSize = new System.Drawing.Size(478, 201);
			this.Controls.Add(this.applyButton);
			this.Controls.Add(this.projectSetupGroup);
			this.Controls.Add(this.projectNameGroup);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.MaximizeBox = false;
			this.Name = "FormProjectSetup";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "Add New Project";
			this.projectNameGroup.ResumeLayout(false);
			this.projectNameGroup.PerformLayout();
			this.projectSetupGroup.ResumeLayout(false);
			this.projectSetupGroup.PerformLayout();
			this.ResumeLayout(false);

        }

        #endregion

        private DarkUI.Controls.DarkButton applyButton;
        private DarkUI.Controls.DarkButton gameBrowseButton;
        private DarkUI.Controls.DarkButton ngCenterBrowseButton;
        private DarkUI.Controls.DarkGroupBox projectNameGroup;
        private DarkUI.Controls.DarkGroupBox projectSetupGroup;
        private DarkUI.Controls.DarkLabel engineTypeLabel;
        private DarkUI.Controls.DarkLabel gamePathLabel;
        private DarkUI.Controls.DarkLabel ngFolderLabel;
        private DarkUI.Controls.DarkLabel projectNameLabel;
        private DarkUI.Controls.DarkRadioButton tr4RadioButton;
        private DarkUI.Controls.DarkRadioButton tr5mainRadioButton;
        private DarkUI.Controls.DarkRadioButton tr5RadioButton;
        private DarkUI.Controls.DarkRadioButton trngRadioButton;
        private DarkUI.Controls.DarkTextBox gamePathTextBox;
        private DarkUI.Controls.DarkTextBox nameTextBox;
        private DarkUI.Controls.DarkTextBox ngCenterPathTextBox;
    }
}