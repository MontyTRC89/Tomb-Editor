namespace TombEditor.Forms
{
    partial class FormImportPrj
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
            this.butCancel = new DarkUI.Controls.DarkButton();
            this.butOk = new DarkUI.Controls.DarkButton();
            this.butBrowseTxt = new DarkUI.Controls.DarkButton();
            this.tbTxtPath = new DarkUI.Controls.DarkTextBox();
            this.darkGroupBox1 = new DarkUI.Controls.DarkGroupBox();
            this.darkLabel3 = new DarkUI.Controls.DarkLabel();
            this.darkGroupBox2 = new DarkUI.Controls.DarkGroupBox();
            this.darkLabel1 = new DarkUI.Controls.DarkLabel();
            this.cbUseHalfPixelCorrection = new DarkUI.Controls.DarkCheckBox();
            this.darkLabel2 = new DarkUI.Controls.DarkLabel();
            this.cbRespectMousepatch = new DarkUI.Controls.DarkCheckBox();
            this.darkGroupBox1.SuspendLayout();
            this.darkGroupBox2.SuspendLayout();
            this.SuspendLayout();
            // 
            // butCancel
            // 
            this.butCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.butCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.butCancel.Location = new System.Drawing.Point(378, 237);
            this.butCancel.Name = "butCancel";
            this.butCancel.Size = new System.Drawing.Size(80, 23);
            this.butCancel.TabIndex = 2;
            this.butCancel.Text = "Cancel";
            this.butCancel.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.butCancel.Click += new System.EventHandler(this.butCancel_Click);
            // 
            // butOk
            // 
            this.butOk.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.butOk.Location = new System.Drawing.Point(292, 237);
            this.butOk.Name = "butOk";
            this.butOk.Size = new System.Drawing.Size(80, 23);
            this.butOk.TabIndex = 1;
            this.butOk.Text = "OK";
            this.butOk.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.butOk.Click += new System.EventHandler(this.butOk_Click);
            // 
            // butBrowseTxt
            // 
            this.butBrowseTxt.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.butBrowseTxt.Image = global::TombEditor.Properties.Resources.general_Open_16;
            this.butBrowseTxt.Location = new System.Drawing.Point(367, 21);
            this.butBrowseTxt.Name = "butBrowseTxt";
            this.butBrowseTxt.Size = new System.Drawing.Size(79, 22);
            this.butBrowseTxt.TabIndex = 8;
            this.butBrowseTxt.Text = "Browse...";
            this.butBrowseTxt.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.butBrowseTxt.Click += new System.EventHandler(this.ButBrowseTxt_Click);
            // 
            // tbTxtPath
            // 
            this.tbTxtPath.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tbTxtPath.Location = new System.Drawing.Point(6, 21);
            this.tbTxtPath.Name = "tbTxtPath";
            this.tbTxtPath.Size = new System.Drawing.Size(355, 22);
            this.tbTxtPath.TabIndex = 7;
            // 
            // darkGroupBox1
            // 
            this.darkGroupBox1.Controls.Add(this.darkLabel3);
            this.darkGroupBox1.Controls.Add(this.tbTxtPath);
            this.darkGroupBox1.Controls.Add(this.butBrowseTxt);
            this.darkGroupBox1.Location = new System.Drawing.Point(6, 148);
            this.darkGroupBox1.Name = "darkGroupBox1";
            this.darkGroupBox1.Size = new System.Drawing.Size(452, 81);
            this.darkGroupBox1.TabIndex = 11;
            this.darkGroupBox1.TabStop = false;
            this.darkGroupBox1.Text = "Base sound catalog (optional)";
            // 
            // darkLabel3
            // 
            this.darkLabel3.ForeColor = System.Drawing.Color.Silver;
            this.darkLabel3.Location = new System.Drawing.Point(6, 47);
            this.darkLabel3.Name = "darkLabel3";
            this.darkLabel3.Size = new System.Drawing.Size(440, 31);
            this.darkLabel3.TabIndex = 10;
            this.darkLabel3.Text = "Specifiy sound catalog (sounds.txt or xml file). If not specified, legacy SFX/SAM" +
    " files provided with specified WAD will be used.";
            // 
            // darkGroupBox2
            // 
            this.darkGroupBox2.Controls.Add(this.darkLabel1);
            this.darkGroupBox2.Controls.Add(this.cbUseHalfPixelCorrection);
            this.darkGroupBox2.Controls.Add(this.darkLabel2);
            this.darkGroupBox2.Controls.Add(this.cbRespectMousepatch);
            this.darkGroupBox2.Location = new System.Drawing.Point(6, 8);
            this.darkGroupBox2.Name = "darkGroupBox2";
            this.darkGroupBox2.Size = new System.Drawing.Size(452, 134);
            this.darkGroupBox2.TabIndex = 12;
            this.darkGroupBox2.TabStop = false;
            this.darkGroupBox2.Text = "Import settings";
            // 
            // darkLabel1
            // 
            this.darkLabel1.ForeColor = System.Drawing.Color.Silver;
            this.darkLabel1.Location = new System.Drawing.Point(6, 98);
            this.darkLabel1.Name = "darkLabel1";
            this.darkLabel1.Size = new System.Drawing.Size(440, 33);
            this.darkLabel1.TabIndex = 13;
            this.darkLabel1.Text = "Legacy texture cropping to prevent border bleeding. Use only if you are about to " +
    "turn off advanced texture padding in Level settings.";
            this.darkLabel1.Click += new System.EventHandler(this.darkLabel1_Click);
            this.darkLabel1.MouseEnter += new System.EventHandler(this.darkLabel1_MouseEnter);
            // 
            // cbUseHalfPixelCorrection
            // 
            this.cbUseHalfPixelCorrection.AutoSize = true;
            this.cbUseHalfPixelCorrection.Location = new System.Drawing.Point(6, 79);
            this.cbUseHalfPixelCorrection.Name = "cbUseHalfPixelCorrection";
            this.cbUseHalfPixelCorrection.Size = new System.Drawing.Size(169, 17);
            this.cbUseHalfPixelCorrection.TabIndex = 12;
            this.cbUseHalfPixelCorrection.Tag = "";
            this.cbUseHalfPixelCorrection.Text = "Use half-pixel UV correction";
            // 
            // darkLabel2
            // 
            this.darkLabel2.ForeColor = System.Drawing.Color.Silver;
            this.darkLabel2.Location = new System.Drawing.Point(6, 41);
            this.darkLabel2.Name = "darkLabel2";
            this.darkLabel2.Size = new System.Drawing.Size(440, 33);
            this.darkLabel2.TabIndex = 11;
            this.darkLabel2.Text = "If you\'ve used a patch which increased maximum amount of flyby sequences in winro" +
    "omedit, use this option, otherwise flyby indices will be corrupted.";
            this.darkLabel2.Click += new System.EventHandler(this.darkLabel2_Click);
            this.darkLabel2.MouseEnter += new System.EventHandler(this.darkLabel2_MouseEnter);
            // 
            // cbRespectMousepatch
            // 
            this.cbRespectMousepatch.AutoSize = true;
            this.cbRespectMousepatch.Location = new System.Drawing.Point(6, 21);
            this.cbRespectMousepatch.Name = "cbRespectMousepatch";
            this.cbRespectMousepatch.Size = new System.Drawing.Size(265, 17);
            this.cbRespectMousepatch.TabIndex = 5;
            this.cbRespectMousepatch.Tag = "";
            this.cbRespectMousepatch.Text = "Respect T4Larson\'s mousepatch flyby handling";
            // 
            // FormImportPrj
            // 
            this.AcceptButton = this.butOk;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.butCancel;
            this.ClientSize = new System.Drawing.Size(464, 267);
            this.Controls.Add(this.darkGroupBox2);
            this.Controls.Add(this.darkGroupBox1);
            this.Controls.Add(this.butCancel);
            this.Controls.Add(this.butOk);
            this.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FormImportPrj";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Import PRJ";
            this.darkGroupBox1.ResumeLayout(false);
            this.darkGroupBox1.PerformLayout();
            this.darkGroupBox2.ResumeLayout(false);
            this.darkGroupBox2.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion
        private DarkUI.Controls.DarkButton butCancel;
        private DarkUI.Controls.DarkButton butOk;
        private DarkUI.Controls.DarkButton butBrowseTxt;
        private DarkUI.Controls.DarkTextBox tbTxtPath;
        private DarkUI.Controls.DarkGroupBox darkGroupBox1;
        private DarkUI.Controls.DarkLabel darkLabel3;
        private DarkUI.Controls.DarkGroupBox darkGroupBox2;
        private DarkUI.Controls.DarkLabel darkLabel2;
        private DarkUI.Controls.DarkCheckBox cbRespectMousepatch;
        private DarkUI.Controls.DarkCheckBox cbUseHalfPixelCorrection;
        private DarkUI.Controls.DarkLabel darkLabel1;
    }
}