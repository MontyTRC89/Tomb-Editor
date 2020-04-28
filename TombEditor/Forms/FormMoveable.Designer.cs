﻿using DarkUI.Controls;

namespace TombEditor.Forms
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
            this.lblColor = new DarkUI.Controls.DarkLabel();
            this.panelColor = new System.Windows.Forms.Panel();
            this.SuspendLayout();
            // 
            // butOK
            // 
            this.butOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.butOK.Checked = false;
            this.butOK.Location = new System.Drawing.Point(39, 182);
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
            this.butCancel.Location = new System.Drawing.Point(125, 182);
            this.butCancel.Name = "butCancel";
            this.butCancel.Size = new System.Drawing.Size(80, 23);
            this.butCancel.TabIndex = 9;
            this.butCancel.Text = "Cancel";
            this.butCancel.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.butCancel.Click += new System.EventHandler(this.butCancel_Click);
            // 
            // cbInvisible
            // 
            this.cbInvisible.AutoSize = true;
            this.cbInvisible.Location = new System.Drawing.Point(76, 9);
            this.cbInvisible.Name = "cbInvisible";
            this.cbInvisible.Size = new System.Drawing.Size(68, 17);
            this.cbInvisible.TabIndex = 5;
            this.cbInvisible.Text = "Invisible";
            // 
            // cbBit2
            // 
            this.cbBit2.AutoSize = true;
            this.cbBit2.Location = new System.Drawing.Point(8, 32);
            this.cbBit2.Name = "cbBit2";
            this.cbBit2.Size = new System.Drawing.Size(49, 17);
            this.cbBit2.TabIndex = 1;
            this.cbBit2.Text = "Bit 2";
            // 
            // cbClearBody
            // 
            this.cbClearBody.AutoSize = true;
            this.cbClearBody.Location = new System.Drawing.Point(76, 32);
            this.cbClearBody.Name = "cbClearBody";
            this.cbClearBody.Size = new System.Drawing.Size(81, 17);
            this.cbClearBody.TabIndex = 6;
            this.cbClearBody.Text = "Clear body";
            // 
            // cbBit1
            // 
            this.cbBit1.AutoSize = true;
            this.cbBit1.Location = new System.Drawing.Point(8, 9);
            this.cbBit1.Name = "cbBit1";
            this.cbBit1.Size = new System.Drawing.Size(49, 17);
            this.cbBit1.TabIndex = 0;
            this.cbBit1.Text = "Bit 1";
            // 
            // cbBit3
            // 
            this.cbBit3.AutoSize = true;
            this.cbBit3.Location = new System.Drawing.Point(8, 55);
            this.cbBit3.Name = "cbBit3";
            this.cbBit3.Size = new System.Drawing.Size(49, 17);
            this.cbBit3.TabIndex = 2;
            this.cbBit3.Text = "Bit 3";
            // 
            // cbBit4
            // 
            this.cbBit4.AutoSize = true;
            this.cbBit4.Location = new System.Drawing.Point(8, 79);
            this.cbBit4.Name = "cbBit4";
            this.cbBit4.Size = new System.Drawing.Size(49, 17);
            this.cbBit4.TabIndex = 3;
            this.cbBit4.Text = "Bit 4";
            // 
            // cbBit5
            // 
            this.cbBit5.AutoSize = true;
            this.cbBit5.Location = new System.Drawing.Point(8, 102);
            this.cbBit5.Name = "cbBit5";
            this.cbBit5.Size = new System.Drawing.Size(49, 17);
            this.cbBit5.TabIndex = 4;
            this.cbBit5.Text = "Bit 5";
            // 
            // tbOCB
            // 
            this.tbOCB.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tbOCB.Location = new System.Drawing.Point(39, 126);
            this.tbOCB.Name = "tbOCB";
            this.tbOCB.Size = new System.Drawing.Size(166, 22);
            this.tbOCB.TabIndex = 7;
            this.tbOCB.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.tbOCB_KeyPress);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.label1.Location = new System.Drawing.Point(5, 128);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(33, 13);
            this.label1.TabIndex = 14;
            this.label1.Text = "OCB:";
            // 
            // lblColor
            // 
            this.lblColor.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.lblColor.AutoSize = true;
            this.lblColor.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.lblColor.Location = new System.Drawing.Point(8, 158);
            this.lblColor.Name = "lblColor";
            this.lblColor.Size = new System.Drawing.Size(29, 13);
            this.lblColor.TabIndex = 15;
            this.lblColor.Text = "Tint:";
            // 
            // panelColor
            // 
            this.panelColor.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panelColor.BackColor = System.Drawing.Color.White;
            this.panelColor.Location = new System.Drawing.Point(39, 154);
            this.panelColor.Name = "panelColor";
            this.panelColor.Size = new System.Drawing.Size(166, 22);
            this.panelColor.TabIndex = 16;
            this.panelColor.Click += new System.EventHandler(this.panelColor_Click);
            // 
            // FormMoveable
            // 
            this.AcceptButton = this.butOK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.butCancel;
            this.ClientSize = new System.Drawing.Size(212, 212);
            this.Controls.Add(this.panelColor);
            this.Controls.Add(this.lblColor);
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
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
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
        private DarkLabel lblColor;
        private System.Windows.Forms.Panel panelColor;
    }
}