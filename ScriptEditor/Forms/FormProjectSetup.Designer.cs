namespace ScriptEditor
{
    partial class FormProjectSetup : DarkUI.Forms.DarkForm
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormProjectSetup));
			this.button_Apply = new DarkUI.Controls.DarkButton();
			this.button_BrowseGame = new DarkUI.Controls.DarkButton();
			this.button_BrowseNGCenter = new DarkUI.Controls.DarkButton();
			this.groupBox_ProjectName = new DarkUI.Controls.DarkGroupBox();
			this.label_ProjectName = new DarkUI.Controls.DarkLabel();
			this.textBox_ProjectName = new DarkUI.Controls.DarkTextBox();
			this.groupBox_ProjectSetup = new DarkUI.Controls.DarkGroupBox();
			this.textBox_GamePath = new DarkUI.Controls.DarkTextBox();
			this.label_EngineType = new DarkUI.Controls.DarkLabel();
			this.radioButton_TR5Main = new DarkUI.Controls.DarkRadioButton();
			this.label_GamePath = new DarkUI.Controls.DarkLabel();
			this.radioButton_TR5 = new DarkUI.Controls.DarkRadioButton();
			this.radioButton_TRNG = new DarkUI.Controls.DarkRadioButton();
			this.label_NGCenterPath = new DarkUI.Controls.DarkLabel();
			this.textBox_NGCenterPath = new DarkUI.Controls.DarkTextBox();
			this.radioButton_TR4 = new DarkUI.Controls.DarkRadioButton();
			this.groupBox_ProjectName.SuspendLayout();
			this.groupBox_ProjectSetup.SuspendLayout();
			this.SuspendLayout();
			// 
			// button_Apply
			// 
			this.button_Apply.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
			this.button_Apply.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.button_Apply.Location = new System.Drawing.Point(9, 165);
			this.button_Apply.Name = "button_Apply";
			this.button_Apply.Size = new System.Drawing.Size(460, 24);
			this.button_Apply.TabIndex = 4;
			this.button_Apply.Text = "Add Project";
			this.button_Apply.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
			this.button_Apply.Click += new System.EventHandler(this.button_Apply_Click);
			// 
			// button_BrowseGame
			// 
			this.button_BrowseGame.Image = ((System.Drawing.Image)(resources.GetObject("button_BrowseGame.Image")));
			this.button_BrowseGame.Location = new System.Drawing.Point(379, 11);
			this.button_BrowseGame.Name = "button_BrowseGame";
			this.button_BrowseGame.Size = new System.Drawing.Size(75, 22);
			this.button_BrowseGame.TabIndex = 3;
			this.button_BrowseGame.Text = "Browse...";
			this.button_BrowseGame.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
			this.button_BrowseGame.Click += new System.EventHandler(this.button_BrowseGame_Click);
			// 
			// button_BrowseNGCenter
			// 
			this.button_BrowseNGCenter.Image = ((System.Drawing.Image)(resources.GetObject("button_BrowseNGCenter.Image")));
			this.button_BrowseNGCenter.Location = new System.Drawing.Point(379, 68);
			this.button_BrowseNGCenter.Name = "button_BrowseNGCenter";
			this.button_BrowseNGCenter.Size = new System.Drawing.Size(75, 22);
			this.button_BrowseNGCenter.TabIndex = 8;
			this.button_BrowseNGCenter.Text = "Browse...";
			this.button_BrowseNGCenter.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
			this.button_BrowseNGCenter.Click += new System.EventHandler(this.button_BrowseNGCenter_Click);
			// 
			// groupBox_ProjectName
			// 
			this.groupBox_ProjectName.Controls.Add(this.label_ProjectName);
			this.groupBox_ProjectName.Controls.Add(this.textBox_ProjectName);
			this.groupBox_ProjectName.Location = new System.Drawing.Point(9, 9);
			this.groupBox_ProjectName.Margin = new System.Windows.Forms.Padding(0);
			this.groupBox_ProjectName.Name = "groupBox_ProjectName";
			this.groupBox_ProjectName.Padding = new System.Windows.Forms.Padding(3, 0, 3, 9);
			this.groupBox_ProjectName.Size = new System.Drawing.Size(460, 45);
			this.groupBox_ProjectName.TabIndex = 14;
			this.groupBox_ProjectName.TabStop = false;
			// 
			// label_ProjectName
			// 
			this.label_ProjectName.AutoSize = true;
			this.label_ProjectName.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
			this.label_ProjectName.Location = new System.Drawing.Point(6, 15);
			this.label_ProjectName.Margin = new System.Windows.Forms.Padding(3, 0, 3, 3);
			this.label_ProjectName.Name = "label_ProjectName";
			this.label_ProjectName.Size = new System.Drawing.Size(72, 13);
			this.label_ProjectName.TabIndex = 19;
			this.label_ProjectName.Text = "Project name:";
			// 
			// textBox_ProjectName
			// 
			this.textBox_ProjectName.Location = new System.Drawing.Point(84, 13);
			this.textBox_ProjectName.Margin = new System.Windows.Forms.Padding(3, 0, 3, 3);
			this.textBox_ProjectName.Name = "textBox_ProjectName";
			this.textBox_ProjectName.Size = new System.Drawing.Size(370, 20);
			this.textBox_ProjectName.TabIndex = 7;
			// 
			// groupBox_ProjectSetup
			// 
			this.groupBox_ProjectSetup.Controls.Add(this.textBox_GamePath);
			this.groupBox_ProjectSetup.Controls.Add(this.label_EngineType);
			this.groupBox_ProjectSetup.Controls.Add(this.button_BrowseGame);
			this.groupBox_ProjectSetup.Controls.Add(this.radioButton_TR5Main);
			this.groupBox_ProjectSetup.Controls.Add(this.label_GamePath);
			this.groupBox_ProjectSetup.Controls.Add(this.radioButton_TR5);
			this.groupBox_ProjectSetup.Controls.Add(this.radioButton_TRNG);
			this.groupBox_ProjectSetup.Controls.Add(this.label_NGCenterPath);
			this.groupBox_ProjectSetup.Controls.Add(this.textBox_NGCenterPath);
			this.groupBox_ProjectSetup.Controls.Add(this.radioButton_TR4);
			this.groupBox_ProjectSetup.Controls.Add(this.button_BrowseNGCenter);
			this.groupBox_ProjectSetup.Location = new System.Drawing.Point(9, 57);
			this.groupBox_ProjectSetup.Margin = new System.Windows.Forms.Padding(0, 3, 0, 3);
			this.groupBox_ProjectSetup.Name = "groupBox_ProjectSetup";
			this.groupBox_ProjectSetup.Padding = new System.Windows.Forms.Padding(3, 0, 3, 9);
			this.groupBox_ProjectSetup.Size = new System.Drawing.Size(460, 102);
			this.groupBox_ProjectSetup.TabIndex = 15;
			this.groupBox_ProjectSetup.TabStop = false;
			// 
			// textBox_GamePath
			// 
			this.textBox_GamePath.Location = new System.Drawing.Point(84, 13);
			this.textBox_GamePath.Margin = new System.Windows.Forms.Padding(3, 0, 3, 3);
			this.textBox_GamePath.Name = "textBox_GamePath";
			this.textBox_GamePath.Size = new System.Drawing.Size(289, 20);
			this.textBox_GamePath.TabIndex = 1;
			this.textBox_GamePath.TextChanged += new System.EventHandler(this.textBox_GamePath_TextChanged);
			// 
			// label_EngineType
			// 
			this.label_EngineType.AutoSize = true;
			this.label_EngineType.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
			this.label_EngineType.Location = new System.Drawing.Point(6, 44);
			this.label_EngineType.Margin = new System.Windows.Forms.Padding(3, 12, 3, 12);
			this.label_EngineType.Name = "label_EngineType";
			this.label_EngineType.Size = new System.Drawing.Size(66, 13);
			this.label_EngineType.TabIndex = 18;
			this.label_EngineType.Text = "Engine type:";
			// 
			// radioButton_TR5Main
			// 
			this.radioButton_TR5Main.AutoSize = true;
			this.radioButton_TR5Main.Location = new System.Drawing.Point(244, 42);
			this.radioButton_TR5Main.Name = "radioButton_TR5Main";
			this.radioButton_TR5Main.Size = new System.Drawing.Size(69, 17);
			this.radioButton_TR5Main.TabIndex = 17;
			this.radioButton_TR5Main.TabStop = true;
			this.radioButton_TR5Main.Text = "TR5Main";
			// 
			// label_GamePath
			// 
			this.label_GamePath.AutoSize = true;
			this.label_GamePath.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
			this.label_GamePath.Location = new System.Drawing.Point(6, 15);
			this.label_GamePath.Margin = new System.Windows.Forms.Padding(3, 0, 3, 3);
			this.label_GamePath.Name = "label_GamePath";
			this.label_GamePath.Size = new System.Drawing.Size(77, 13);
			this.label_GamePath.TabIndex = 6;
			this.label_GamePath.Text = "Game .exe file:";
			// 
			// radioButton_TR5
			// 
			this.radioButton_TR5.AutoSize = true;
			this.radioButton_TR5.Location = new System.Drawing.Point(192, 42);
			this.radioButton_TR5.Name = "radioButton_TR5";
			this.radioButton_TR5.Size = new System.Drawing.Size(46, 17);
			this.radioButton_TR5.TabIndex = 16;
			this.radioButton_TR5.TabStop = true;
			this.radioButton_TR5.Text = "TR5";
			// 
			// radioButton_TRNG
			// 
			this.radioButton_TRNG.AutoSize = true;
			this.radioButton_TRNG.Location = new System.Drawing.Point(130, 42);
			this.radioButton_TRNG.Name = "radioButton_TRNG";
			this.radioButton_TRNG.Size = new System.Drawing.Size(56, 17);
			this.radioButton_TRNG.TabIndex = 15;
			this.radioButton_TRNG.TabStop = true;
			this.radioButton_TRNG.Text = "TRNG";
			this.radioButton_TRNG.CheckedChanged += new System.EventHandler(this.radioButton_TRNG_CheckedChanged);
			// 
			// label_NGCenterPath
			// 
			this.label_NGCenterPath.AutoSize = true;
			this.label_NGCenterPath.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
			this.label_NGCenterPath.Location = new System.Drawing.Point(6, 73);
			this.label_NGCenterPath.Margin = new System.Windows.Forms.Padding(3);
			this.label_NGCenterPath.Name = "label_NGCenterPath";
			this.label_NGCenterPath.Size = new System.Drawing.Size(99, 13);
			this.label_NGCenterPath.TabIndex = 9;
			this.label_NGCenterPath.Text = "NG_Center.exe file:";
			// 
			// textBox_NGCenterPath
			// 
			this.textBox_NGCenterPath.Location = new System.Drawing.Point(111, 70);
			this.textBox_NGCenterPath.Name = "textBox_NGCenterPath";
			this.textBox_NGCenterPath.Size = new System.Drawing.Size(262, 20);
			this.textBox_NGCenterPath.TabIndex = 7;
			// 
			// radioButton_TR4
			// 
			this.radioButton_TR4.AutoSize = true;
			this.radioButton_TR4.Location = new System.Drawing.Point(78, 42);
			this.radioButton_TR4.Name = "radioButton_TR4";
			this.radioButton_TR4.Size = new System.Drawing.Size(46, 17);
			this.radioButton_TR4.TabIndex = 14;
			this.radioButton_TR4.TabStop = true;
			this.radioButton_TR4.Text = "TR4";
			// 
			// FormProjectSetup
			// 
			this.AcceptButton = this.button_Apply;
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
			this.ClientSize = new System.Drawing.Size(478, 201);
			this.Controls.Add(this.button_Apply);
			this.Controls.Add(this.groupBox_ProjectSetup);
			this.Controls.Add(this.groupBox_ProjectName);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.MaximizeBox = false;
			this.Name = "FormProjectSetup";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "Add New Project";
			this.groupBox_ProjectName.ResumeLayout(false);
			this.groupBox_ProjectName.PerformLayout();
			this.groupBox_ProjectSetup.ResumeLayout(false);
			this.groupBox_ProjectSetup.PerformLayout();
			this.ResumeLayout(false);

        }

        #endregion

        private DarkUI.Controls.DarkButton button_Apply;
        private DarkUI.Controls.DarkButton button_BrowseGame;
        private DarkUI.Controls.DarkButton button_BrowseNGCenter;
        private DarkUI.Controls.DarkGroupBox groupBox_ProjectName;
        private DarkUI.Controls.DarkGroupBox groupBox_ProjectSetup;
        private DarkUI.Controls.DarkLabel label_EngineType;
        private DarkUI.Controls.DarkLabel label_GamePath;
        private DarkUI.Controls.DarkLabel label_NGCenterPath;
        private DarkUI.Controls.DarkLabel label_ProjectName;
        private DarkUI.Controls.DarkRadioButton radioButton_TR4;
        private DarkUI.Controls.DarkRadioButton radioButton_TR5;
        private DarkUI.Controls.DarkRadioButton radioButton_TR5Main;
        private DarkUI.Controls.DarkRadioButton radioButton_TRNG;
        private DarkUI.Controls.DarkTextBox textBox_GamePath;
        private DarkUI.Controls.DarkTextBox textBox_NGCenterPath;
        private DarkUI.Controls.DarkTextBox textBox_ProjectName;
    }
}