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
            this.butCreate = new DarkUI.Controls.DarkButton();
            this.butCancel = new DarkUI.Controls.DarkButton();
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
            // 
            // butCreate
            // 
            this.butCreate.Location = new System.Drawing.Point(98, 39);
            this.butCreate.Name = "butCreate";
            this.butCreate.Size = new System.Drawing.Size(96, 23);
            this.butCreate.TabIndex = 2;
            this.butCreate.Text = "Create Wad2";
            this.butCreate.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.butCreate.Click += new System.EventHandler(this.butCreate_Click);
            // 
            // butCancel
            // 
            this.butCancel.Location = new System.Drawing.Point(207, 39);
            this.butCancel.Name = "butCancel";
            this.butCancel.Size = new System.Drawing.Size(96, 23);
            this.butCancel.TabIndex = 3;
            this.butCancel.Text = "Cancel";
            this.butCancel.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.butCancel.Click += new System.EventHandler(this.butCancel_Click);
            // 
            // FormNewWad2
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(309, 76);
            this.Controls.Add(this.butCancel);
            this.Controls.Add(this.butCreate);
            this.Controls.Add(this.comboGameVersion);
            this.Controls.Add(this.darkLabel1);
            this.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Name = "FormNewWad2";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "New Wad2";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private DarkUI.Controls.DarkLabel darkLabel1;
        private DarkUI.Controls.DarkComboBox comboGameVersion;
        private DarkUI.Controls.DarkButton butCreate;
        private DarkUI.Controls.DarkButton butCancel;
    }
}