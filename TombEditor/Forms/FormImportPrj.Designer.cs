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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormImportPrj));
            this.butCancel = new DarkUI.Controls.DarkButton();
            this.butOk = new DarkUI.Controls.DarkButton();
            this.darkLabel1 = new DarkUI.Controls.DarkLabel();
            this.tbPrjPath = new DarkUI.Controls.DarkTextBox();
            this.butBrowsePrj = new DarkUI.Controls.DarkButton();
            this.butBrowseTxt = new DarkUI.Controls.DarkButton();
            this.tbTxtPath = new DarkUI.Controls.DarkTextBox();
            this.darkLabel2 = new DarkUI.Controls.DarkLabel();
            this.darkLabel3 = new DarkUI.Controls.DarkLabel();
            this.darkLabel4 = new DarkUI.Controls.DarkLabel();
            this.SuspendLayout();
            // 
            // butCancel
            // 
            this.butCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.butCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.butCancel.Location = new System.Drawing.Point(420, 203);
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
            this.butOk.Location = new System.Drawing.Point(334, 203);
            this.butOk.Name = "butOk";
            this.butOk.Size = new System.Drawing.Size(80, 23);
            this.butOk.TabIndex = 1;
            this.butOk.Text = "OK";
            this.butOk.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.butOk.Click += new System.EventHandler(this.butOk_Click);
            // 
            // darkLabel1
            // 
            this.darkLabel1.AutoSize = true;
            this.darkLabel1.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.darkLabel1.Location = new System.Drawing.Point(13, 13);
            this.darkLabel1.Name = "darkLabel1";
            this.darkLabel1.Size = new System.Drawing.Size(78, 13);
            this.darkLabel1.TabIndex = 3;
            this.darkLabel1.Text = "PRJ to import:";
            // 
            // tbPrjPath
            // 
            this.tbPrjPath.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tbPrjPath.Location = new System.Drawing.Point(111, 11);
            this.tbPrjPath.Name = "tbPrjPath";
            this.tbPrjPath.Size = new System.Drawing.Size(303, 22);
            this.tbPrjPath.TabIndex = 4;
            // 
            // butBrowsePrj
            // 
            this.butBrowsePrj.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.butBrowsePrj.Image = global::TombEditor.Properties.Resources.general_Open_16;
            this.butBrowsePrj.Location = new System.Drawing.Point(421, 11);
            this.butBrowsePrj.Name = "butBrowsePrj";
            this.butBrowsePrj.Size = new System.Drawing.Size(79, 23);
            this.butBrowsePrj.TabIndex = 5;
            this.butBrowsePrj.Text = "Browse";
            this.butBrowsePrj.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.butBrowsePrj.Click += new System.EventHandler(this.ButBrowsePrj_Click);
            // 
            // butBrowseTxt
            // 
            this.butBrowseTxt.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.butBrowseTxt.Image = global::TombEditor.Properties.Resources.general_Open_16;
            this.butBrowseTxt.Location = new System.Drawing.Point(421, 40);
            this.butBrowseTxt.Name = "butBrowseTxt";
            this.butBrowseTxt.Size = new System.Drawing.Size(79, 23);
            this.butBrowseTxt.TabIndex = 8;
            this.butBrowseTxt.Text = "Browse";
            this.butBrowseTxt.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.butBrowseTxt.Click += new System.EventHandler(this.ButBrowseTxt_Click);
            // 
            // tbTxtPath
            // 
            this.tbTxtPath.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tbTxtPath.Location = new System.Drawing.Point(111, 40);
            this.tbTxtPath.Name = "tbTxtPath";
            this.tbTxtPath.Size = new System.Drawing.Size(303, 22);
            this.tbTxtPath.TabIndex = 7;
            // 
            // darkLabel2
            // 
            this.darkLabel2.AutoSize = true;
            this.darkLabel2.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.darkLabel2.Location = new System.Drawing.Point(13, 42);
            this.darkLabel2.Name = "darkLabel2";
            this.darkLabel2.Size = new System.Drawing.Size(90, 13);
            this.darkLabel2.TabIndex = 6;
            this.darkLabel2.Text = "Sounds catalog:";
            // 
            // darkLabel3
            // 
            this.darkLabel3.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.darkLabel3.Location = new System.Drawing.Point(111, 69);
            this.darkLabel3.Name = "darkLabel3";
            this.darkLabel3.Size = new System.Drawing.Size(389, 58);
            this.darkLabel3.TabIndex = 9;
            this.darkLabel3.Text = resources.GetString("darkLabel3.Text");
            // 
            // darkLabel4
            // 
            this.darkLabel4.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.darkLabel4.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.darkLabel4.Location = new System.Drawing.Point(111, 143);
            this.darkLabel4.Name = "darkLabel4";
            this.darkLabel4.Size = new System.Drawing.Size(389, 49);
            this.darkLabel4.TabIndex = 10;
            this.darkLabel4.Text = "Warning: if you have selected to load a custom Sounds.txt file, it will be conver" +
    "ted to our new XML file format and the file will be placed in the same path of T" +
    "XT file.";
            // 
            // FormImportPrj
            // 
            this.AcceptButton = this.butOk;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.butCancel;
            this.ClientSize = new System.Drawing.Size(508, 234);
            this.Controls.Add(this.darkLabel4);
            this.Controls.Add(this.darkLabel3);
            this.Controls.Add(this.butBrowseTxt);
            this.Controls.Add(this.tbTxtPath);
            this.Controls.Add(this.darkLabel2);
            this.Controls.Add(this.butBrowsePrj);
            this.Controls.Add(this.tbPrjPath);
            this.Controls.Add(this.darkLabel1);
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
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private DarkUI.Controls.DarkButton butCancel;
        private DarkUI.Controls.DarkButton butOk;
        private DarkUI.Controls.DarkLabel darkLabel1;
        private DarkUI.Controls.DarkTextBox tbPrjPath;
        private DarkUI.Controls.DarkButton butBrowsePrj;
        private DarkUI.Controls.DarkButton butBrowseTxt;
        private DarkUI.Controls.DarkTextBox tbTxtPath;
        private DarkUI.Controls.DarkLabel darkLabel2;
        private DarkUI.Controls.DarkLabel darkLabel3;
        private DarkUI.Controls.DarkLabel darkLabel4;
    }
}