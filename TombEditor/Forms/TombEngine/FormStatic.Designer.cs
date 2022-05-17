using DarkUI.Controls;

namespace TombEditor.Forms.TombEngine
{
    partial class FormStatic
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
            this.tbOCB = new DarkUI.Controls.DarkTextBox();
            this.label1 = new DarkUI.Controls.DarkLabel();
            this.lblColor = new DarkUI.Controls.DarkLabel();
            this.panelColor = new DarkUI.Controls.DarkPanel();
            this.tbLuaName = new DarkUI.Controls.DarkTextBox();
            this.labelLuaName = new DarkUI.Controls.DarkLabel();
            this.SuspendLayout();
            // 
            // butOK
            // 
            this.butOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.butOK.Checked = false;
            this.butOK.Location = new System.Drawing.Point(45, 96);
            this.butOK.Name = "butOK";
            this.butOK.Size = new System.Drawing.Size(80, 23);
            this.butOK.TabIndex = 14;
            this.butOK.Text = "OK";
            this.butOK.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.butOK.Click += new System.EventHandler(this.butOK_Click);
            // 
            // butCancel
            // 
            this.butCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.butCancel.Checked = false;
            this.butCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.butCancel.Location = new System.Drawing.Point(131, 96);
            this.butCancel.Name = "butCancel";
            this.butCancel.Size = new System.Drawing.Size(80, 23);
            this.butCancel.TabIndex = 15;
            this.butCancel.Text = "Cancel";
            this.butCancel.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.butCancel.Click += new System.EventHandler(this.butCancel_Click);
            // 
            // tbOCB
            // 
            this.tbOCB.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.tbOCB.Location = new System.Drawing.Point(73, 10);
            this.tbOCB.Name = "tbOCB";
            this.tbOCB.Size = new System.Drawing.Size(138, 22);
            this.tbOCB.TabIndex = 13;
            this.tbOCB.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.tbOCB_KeyPress);
            // 
            // label1
            // 
            this.label1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.label1.AutoSize = true;
            this.label1.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.label1.Location = new System.Drawing.Point(7, 12);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(33, 13);
            this.label1.TabIndex = 27;
            this.label1.Text = "OCB:";
            // 
            // lblColor
            // 
            this.lblColor.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.lblColor.AutoSize = true;
            this.lblColor.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.lblColor.Location = new System.Drawing.Point(7, 69);
            this.lblColor.Name = "lblColor";
            this.lblColor.Size = new System.Drawing.Size(32, 13);
            this.lblColor.TabIndex = 28;
            this.lblColor.Text = "Tint: ";
            // 
            // panelColor
            // 
            this.panelColor.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.panelColor.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panelColor.Location = new System.Drawing.Point(73, 66);
            this.panelColor.Name = "panelColor";
            this.panelColor.Size = new System.Drawing.Size(138, 22);
            this.panelColor.TabIndex = 29;
            this.panelColor.Click += new System.EventHandler(this.panelColor_Click);
            // 
            // tbLuaName
            // 
            this.tbLuaName.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.tbLuaName.Location = new System.Drawing.Point(73, 38);
            this.tbLuaName.Name = "tbLuaName";
            this.tbLuaName.Size = new System.Drawing.Size(138, 22);
            this.tbLuaName.TabIndex = 30;
            // 
            // labelLuaName
            // 
            this.labelLuaName.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.labelLuaName.AutoSize = true;
            this.labelLuaName.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.labelLuaName.Location = new System.Drawing.Point(7, 40);
            this.labelLuaName.Name = "labelLuaName";
            this.labelLuaName.Size = new System.Drawing.Size(60, 13);
            this.labelLuaName.TabIndex = 31;
            this.labelLuaName.Text = "Lua Name:";
            // 
            // FormStatic
            // 
            this.AcceptButton = this.butOK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.butCancel;
            this.ClientSize = new System.Drawing.Size(217, 125);
            this.Controls.Add(this.tbLuaName);
            this.Controls.Add(this.labelLuaName);
            this.Controls.Add(this.panelColor);
            this.Controls.Add(this.lblColor);
            this.Controls.Add(this.tbOCB);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.butCancel);
            this.Controls.Add(this.butOK);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Fixed3D;
            this.MaximizeBox = false;
            this.MaximumSize = new System.Drawing.Size(237, 168);
            this.MinimizeBox = false;
            this.Name = "FormStatic";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Static mesh";
            this.Load += new System.EventHandler(this.FormObject_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private DarkButton butOK;
        private DarkButton butCancel;
        private DarkTextBox tbOCB;
        private DarkLabel label1;
        private DarkLabel lblColor;
        private DarkUI.Controls.DarkPanel panelColor;
        private DarkTextBox tbLuaName;
        private DarkLabel labelLuaName;
    }
}