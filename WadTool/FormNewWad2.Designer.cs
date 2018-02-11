namespace WadTool
{
    partial class FormNewWad2 
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
            this.darkLabel1 = new DarkUI.Controls.DarkLabel();
            this.comboGameVersion = new DarkUI.Controls.DarkComboBox();
            this.cbNG = new DarkUI.Controls.DarkCheckBox();
            this.comboSoundSystem = new DarkUI.Controls.DarkComboBox();
            this.darkLabel2 = new DarkUI.Controls.DarkLabel();
            this.butCreate = new DarkUI.Controls.DarkButton();
            this.butCancel = new DarkUI.Controls.DarkButton();
            this.labelSoundSystem = new DarkUI.Controls.DarkLabel();
            this.panelSoundManagement = new System.Windows.Forms.Panel();
            this.panelSoundManagement.SuspendLayout();
            this.SuspendLayout();
            // 
            // darkLabel1
            // 
            this.darkLabel1.AutoSize = true;
            this.darkLabel1.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.darkLabel1.Location = new System.Drawing.Point(13, 13);
            this.darkLabel1.Name = "darkLabel1";
            this.darkLabel1.Size = new System.Drawing.Size(79, 13);
            this.darkLabel1.TabIndex = 0;
            this.darkLabel1.Text = "Game version:";
            // 
            // comboGameVersion
            // 
            this.comboGameVersion.FormattingEnabled = true;
            this.comboGameVersion.Location = new System.Drawing.Point(98, 10);
            this.comboGameVersion.Name = "comboGameVersion";
            this.comboGameVersion.Size = new System.Drawing.Size(205, 23);
            this.comboGameVersion.TabIndex = 1;
            this.comboGameVersion.SelectedIndexChanged += new System.EventHandler(this.comboGameVersion_SelectedIndexChanged);
            // 
            // cbNG
            // 
            this.cbNG.AutoSize = true;
            this.cbNG.Location = new System.Drawing.Point(310, 13);
            this.cbNG.Name = "cbNG";
            this.cbNG.Size = new System.Drawing.Size(54, 17);
            this.cbNG.TabIndex = 2;
            this.cbNG.Text = "TRNG";
            // 
            // comboSoundSystem
            // 
            this.comboSoundSystem.FormattingEnabled = true;
            this.comboSoundSystem.Location = new System.Drawing.Point(98, 5);
            this.comboSoundSystem.Name = "comboSoundSystem";
            this.comboSoundSystem.Size = new System.Drawing.Size(205, 23);
            this.comboSoundSystem.TabIndex = 4;
            this.comboSoundSystem.SelectedValueChanged += new System.EventHandler(this.comboSoundSystem_SelectedValueChanged);
            // 
            // darkLabel2
            // 
            this.darkLabel2.AutoSize = true;
            this.darkLabel2.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.darkLabel2.Location = new System.Drawing.Point(13, 8);
            this.darkLabel2.Name = "darkLabel2";
            this.darkLabel2.Size = new System.Drawing.Size(81, 13);
            this.darkLabel2.TabIndex = 3;
            this.darkLabel2.Text = "Sound system:";
            // 
            // butCreate
            // 
            this.butCreate.Location = new System.Drawing.Point(90, 146);
            this.butCreate.Name = "butCreate";
            this.butCreate.Size = new System.Drawing.Size(96, 23);
            this.butCreate.TabIndex = 46;
            this.butCreate.Text = "Create Wad2";
            this.butCreate.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.butCreate.Click += new System.EventHandler(this.butCreate_Click);
            // 
            // butCancel
            // 
            this.butCancel.Location = new System.Drawing.Point(192, 146);
            this.butCancel.Name = "butCancel";
            this.butCancel.Size = new System.Drawing.Size(96, 23);
            this.butCancel.TabIndex = 47;
            this.butCancel.Text = "Cancel";
            this.butCancel.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.butCancel.Click += new System.EventHandler(this.butCancel_Click);
            // 
            // labelSoundSystem
            // 
            this.labelSoundSystem.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelSoundSystem.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.labelSoundSystem.Location = new System.Drawing.Point(98, 31);
            this.labelSoundSystem.Name = "labelSoundSystem";
            this.labelSoundSystem.Size = new System.Drawing.Size(273, 54);
            this.labelSoundSystem.TabIndex = 48;
            this.labelSoundSystem.Text = "Warning: the new dynamic soundmap manager should be used only for Wad2 files crea" +
    "ted for levels. If you are creating a Wad2 for releasing objects, use the old cl" +
    "assic TRLE system.";
            this.labelSoundSystem.Visible = false;
            // 
            // panelSoundManagement
            // 
            this.panelSoundManagement.Controls.Add(this.labelSoundSystem);
            this.panelSoundManagement.Controls.Add(this.darkLabel2);
            this.panelSoundManagement.Controls.Add(this.comboSoundSystem);
            this.panelSoundManagement.Location = new System.Drawing.Point(0, 40);
            this.panelSoundManagement.Name = "panelSoundManagement";
            this.panelSoundManagement.Size = new System.Drawing.Size(386, 100);
            this.panelSoundManagement.TabIndex = 49;
            // 
            // FormNewWad2
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(383, 181);
            this.Controls.Add(this.panelSoundManagement);
            this.Controls.Add(this.butCancel);
            this.Controls.Add(this.butCreate);
            this.Controls.Add(this.cbNG);
            this.Controls.Add(this.comboGameVersion);
            this.Controls.Add(this.darkLabel1);
            this.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Name = "FormNewWad2";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "New Wad2";
            this.panelSoundManagement.ResumeLayout(false);
            this.panelSoundManagement.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private DarkUI.Controls.DarkLabel darkLabel1;
        private DarkUI.Controls.DarkComboBox comboGameVersion;
        private DarkUI.Controls.DarkCheckBox cbNG;
        private DarkUI.Controls.DarkComboBox comboSoundSystem;
        private DarkUI.Controls.DarkLabel darkLabel2;
        private DarkUI.Controls.DarkButton butCreate;
        private DarkUI.Controls.DarkButton butCancel;
        private DarkUI.Controls.DarkLabel labelSoundSystem;
        private System.Windows.Forms.Panel panelSoundManagement;
    }
}