using DarkUI.Controls;

namespace TombEditor.Forms
{
    partial class FormMemo
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
            this.butOK = new DarkUI.Controls.DarkButton();
            this.butCancel = new DarkUI.Controls.DarkButton();
            this.tbText = new DarkUI.Controls.DarkTextBox();
            this.cbAlwaysDisplayText = new DarkUI.Controls.DarkCheckBox();
            this.SuspendLayout();
            // 
            // butOK
            // 
            this.butOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.butOK.Checked = false;
            this.butOK.Location = new System.Drawing.Point(371, 242);
            this.butOK.Name = "butOK";
            this.butOK.Size = new System.Drawing.Size(80, 23);
            this.butOK.TabIndex = 8;
            this.butOK.Text = "OK";
            this.butOK.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.butOK.Click += new System.EventHandler(this.butOK_Click);
            // 
            // butCancel
            // 
            this.butCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.butCancel.Checked = false;
            this.butCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.butCancel.Location = new System.Drawing.Point(457, 242);
            this.butCancel.Name = "butCancel";
            this.butCancel.Size = new System.Drawing.Size(80, 23);
            this.butCancel.TabIndex = 9;
            this.butCancel.Text = "Cancel";
            this.butCancel.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.butCancel.Click += new System.EventHandler(this.butCancel_Click);
            // 
            // tbText
            // 
            this.tbText.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tbText.Location = new System.Drawing.Point(8, 8);
            this.tbText.Multiline = true;
            this.tbText.Name = "tbText";
            this.tbText.Size = new System.Drawing.Size(529, 228);
            this.tbText.TabIndex = 11;
            // 
            // cbAlwaysDisplayText
            // 
            this.cbAlwaysDisplayText.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.cbAlwaysDisplayText.AutoSize = true;
            this.cbAlwaysDisplayText.Location = new System.Drawing.Point(8, 246);
            this.cbAlwaysDisplayText.Name = "cbAlwaysDisplayText";
            this.cbAlwaysDisplayText.Size = new System.Drawing.Size(122, 17);
            this.cbAlwaysDisplayText.TabIndex = 12;
            this.cbAlwaysDisplayText.Text = "Always display text";
            // 
            // FormMemo
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.butCancel;
            this.ClientSize = new System.Drawing.Size(544, 272);
            this.Controls.Add(this.cbAlwaysDisplayText);
            this.Controls.Add(this.tbText);
            this.Controls.Add(this.butCancel);
            this.Controls.Add(this.butOK);
            this.KeyPreview = true;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(300, 250);
            this.Name = "FormMemo";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
            this.Text = "Edit memo";
            this.Shown += new System.EventHandler(this.FormMemo_Shown);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.FormMemo_KeyDown);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private DarkButton butOK;
        private DarkButton butCancel;
        private DarkTextBox tbText;
        private DarkCheckBox cbAlwaysDisplayText;
    }
}