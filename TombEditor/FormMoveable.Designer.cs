using DarkUI.Controls;

namespace TombEditor
{
    partial class FormMoveable
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
            this.cbInvisible = new DarkUI.Controls.DarkCheckBox();
            this.cbBit2 = new DarkUI.Controls.DarkCheckBox();
            this.cbClearBody = new DarkUI.Controls.DarkCheckBox();
            this.cbBit1 = new DarkUI.Controls.DarkCheckBox();
            this.cbBit3 = new DarkUI.Controls.DarkCheckBox();
            this.cbBit4 = new DarkUI.Controls.DarkCheckBox();
            this.cbBit5 = new DarkUI.Controls.DarkCheckBox();
            this.tbOCB = new DarkUI.Controls.DarkTextBox();
            this.label1 = new DarkUI.Controls.DarkLabel();
            this.backgroundWorker1 = new System.ComponentModel.BackgroundWorker();
            this.SuspendLayout();
            // 
            // butOK
            // 
            this.butOK.Location = new System.Drawing.Point(27, 211);
            this.butOK.Name = "butOK";
            this.butOK.Padding = new System.Windows.Forms.Padding(5);
            this.butOK.Size = new System.Drawing.Size(86, 23);
            this.butOK.TabIndex = 0;
            this.butOK.Text = "OK";
            this.butOK.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.butOK.Click += new System.EventHandler(this.butOK_Click);
            // 
            // butCancel
            // 
            this.butCancel.Location = new System.Drawing.Point(119, 211);
            this.butCancel.Name = "butCancel";
            this.butCancel.Padding = new System.Windows.Forms.Padding(5);
            this.butCancel.Size = new System.Drawing.Size(86, 23);
            this.butCancel.TabIndex = 1;
            this.butCancel.Text = "Cancel";
            this.butCancel.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.butCancel.Click += new System.EventHandler(this.butCancel_Click);
            // 
            // cbInvisible
            // 
            this.cbInvisible.AutoSize = true;
            this.cbInvisible.Location = new System.Drawing.Point(129, 12);
            this.cbInvisible.Name = "cbInvisible";
            this.cbInvisible.Size = new System.Drawing.Size(68, 17);
            this.cbInvisible.TabIndex = 5;
            this.cbInvisible.Text = "Invisible";
            // 
            // cbBit2
            // 
            this.cbBit2.AutoSize = true;
            this.cbBit2.Location = new System.Drawing.Point(18, 35);
            this.cbBit2.Name = "cbBit2";
            this.cbBit2.Size = new System.Drawing.Size(49, 17);
            this.cbBit2.TabIndex = 6;
            this.cbBit2.Text = "Bit 2";
            // 
            // cbClearBody
            // 
            this.cbClearBody.AutoSize = true;
            this.cbClearBody.Location = new System.Drawing.Point(129, 35);
            this.cbClearBody.Name = "cbClearBody";
            this.cbClearBody.Size = new System.Drawing.Size(81, 17);
            this.cbClearBody.TabIndex = 9;
            this.cbClearBody.Text = "Clear body";
            // 
            // cbBit1
            // 
            this.cbBit1.AutoSize = true;
            this.cbBit1.Location = new System.Drawing.Point(18, 12);
            this.cbBit1.Name = "cbBit1";
            this.cbBit1.Size = new System.Drawing.Size(49, 17);
            this.cbBit1.TabIndex = 10;
            this.cbBit1.Text = "Bit 1";
            // 
            // cbBit3
            // 
            this.cbBit3.AutoSize = true;
            this.cbBit3.Location = new System.Drawing.Point(18, 58);
            this.cbBit3.Name = "cbBit3";
            this.cbBit3.Size = new System.Drawing.Size(49, 17);
            this.cbBit3.TabIndex = 11;
            this.cbBit3.Text = "Bit 3";
            // 
            // cbBit4
            // 
            this.cbBit4.AutoSize = true;
            this.cbBit4.Location = new System.Drawing.Point(18, 82);
            this.cbBit4.Name = "cbBit4";
            this.cbBit4.Size = new System.Drawing.Size(49, 17);
            this.cbBit4.TabIndex = 12;
            this.cbBit4.Text = "Bit 4";
            // 
            // cbBit5
            // 
            this.cbBit5.AutoSize = true;
            this.cbBit5.Location = new System.Drawing.Point(18, 105);
            this.cbBit5.Name = "cbBit5";
            this.cbBit5.Size = new System.Drawing.Size(49, 17);
            this.cbBit5.TabIndex = 13;
            this.cbBit5.Text = "Bit 5";
            // 
            // tbOCB
            // 
            this.tbOCB.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(69)))), ((int)(((byte)(73)))), ((int)(((byte)(74)))));
            this.tbOCB.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.tbOCB.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.tbOCB.Location = new System.Drawing.Point(59, 160);
            this.tbOCB.Name = "tbOCB";
            this.tbOCB.Size = new System.Drawing.Size(146, 22);
            this.tbOCB.TabIndex = 15;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.label1.Location = new System.Drawing.Point(15, 163);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(33, 13);
            this.label1.TabIndex = 14;
            this.label1.Text = "OCB:";
            // 
            // FormMoveable
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(231, 246);
            this.Controls.Add(this.cbBit5);
            this.Controls.Add(this.tbOCB);
            this.Controls.Add(this.cbInvisible);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.cbBit4);
            this.Controls.Add(this.cbBit2);
            this.Controls.Add(this.butCancel);
            this.Controls.Add(this.cbBit3);
            this.Controls.Add(this.butOK);
            this.Controls.Add(this.cbBit1);
            this.Controls.Add(this.cbClearBody);
            this.Font = new System.Drawing.Font("Segoe UI", 8.25F);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Fixed3D;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FormMoveable";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Object";
            this.Load += new System.EventHandler(this.FormObject_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private DarkButton butOK;
        private DarkButton butCancel;
        private DarkCheckBox cbInvisible;
        private DarkCheckBox cbBit2;
        private DarkCheckBox cbClearBody;
        private DarkCheckBox cbBit1;
        private DarkCheckBox cbBit3;
        private DarkCheckBox cbBit4;
        private DarkCheckBox cbBit5;
        private DarkTextBox tbOCB;
        private DarkLabel label1;
        private System.ComponentModel.BackgroundWorker backgroundWorker1;
    }
}