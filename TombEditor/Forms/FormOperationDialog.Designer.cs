﻿using DarkUI.Controls;

namespace TombEditor.Forms
{
    partial class FormOperationDialog
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
            this.butOk = new DarkUI.Controls.DarkButton();
            this.pbStato = new DarkUI.Controls.DarkProgressBar();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.butCancel = new DarkUI.Controls.DarkButton();
            this.lstLog = new System.Windows.Forms.RichTextBox();
            this.panelLogAndProgressBar = new System.Windows.Forms.Panel();
            this.panelLog = new System.Windows.Forms.Panel();
            this.panelProgressBar = new System.Windows.Forms.Panel();
            this.tableLayoutPanel1.SuspendLayout();
            this.panelLogAndProgressBar.SuspendLayout();
            this.panelLog.SuspendLayout();
            this.panelProgressBar.SuspendLayout();
            this.SuspendLayout();
            // 
            // butOk
            // 
            this.butOk.Dock = System.Windows.Forms.DockStyle.Fill;
            this.butOk.Enabled = false;
            this.butOk.Location = new System.Drawing.Point(3, 3);
            this.butOk.Name = "butOk";
            this.butOk.Size = new System.Drawing.Size(317, 26);
            this.butOk.TabIndex = 0;
            this.butOk.Text = "OK";
            this.butOk.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.butOk.Click += new System.EventHandler(this.butOk_Click);
            // 
            // pbStato
            // 
            this.pbStato.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.pbStato.Location = new System.Drawing.Point(3, 2);
            this.pbStato.Name = "pbStato";
            this.pbStato.Size = new System.Drawing.Size(640, 23);
            this.pbStato.TabIndex = 3;
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tableLayoutPanel1.ColumnCount = 2;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.Controls.Add(this.butCancel, 1, 0);
            this.tableLayoutPanel1.Controls.Add(this.butOk, 0, 0);
            this.tableLayoutPanel1.Location = new System.Drawing.Point(4, 366);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 1;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 32F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(646, 32);
            this.tableLayoutPanel1.TabIndex = 4;
            // 
            // butCancel
            // 
            this.butCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.butCancel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.butCancel.Location = new System.Drawing.Point(326, 3);
            this.butCancel.Name = "butCancel";
            this.butCancel.Size = new System.Drawing.Size(317, 26);
            this.butCancel.TabIndex = 1;
            this.butCancel.Text = "Cancel";
            this.butCancel.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.butCancel.Click += new System.EventHandler(this.butCancel_Click);
            // 
            // lstLog
            // 
            this.lstLog.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.lstLog.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lstLog.Location = new System.Drawing.Point(3, 3);
            this.lstLog.Name = "lstLog";
            this.lstLog.ReadOnly = true;
            this.lstLog.Size = new System.Drawing.Size(640, 325);
            this.lstLog.TabIndex = 0;
            this.lstLog.Text = "";
            // 
            // panelLogAndProgressBar
            // 
            this.panelLogAndProgressBar.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panelLogAndProgressBar.Controls.Add(this.panelLog);
            this.panelLogAndProgressBar.Controls.Add(this.panelProgressBar);
            this.panelLogAndProgressBar.Location = new System.Drawing.Point(4, 4);
            this.panelLogAndProgressBar.Name = "panelLogAndProgressBar";
            this.panelLogAndProgressBar.Size = new System.Drawing.Size(646, 359);
            this.panelLogAndProgressBar.TabIndex = 5;
            // 
            // panelLog
            // 
            this.panelLog.AllowDrop = true;
            this.panelLog.Controls.Add(this.lstLog);
            this.panelLog.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelLog.Location = new System.Drawing.Point(0, 0);
            this.panelLog.Name = "panelLog";
            this.panelLog.Padding = new System.Windows.Forms.Padding(3);
            this.panelLog.Size = new System.Drawing.Size(646, 331);
            this.panelLog.TabIndex = 5;
            // 
            // panelProgressBar
            // 
            this.panelProgressBar.Controls.Add(this.pbStato);
            this.panelProgressBar.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panelProgressBar.Location = new System.Drawing.Point(0, 331);
            this.panelProgressBar.Name = "panelProgressBar";
            this.panelProgressBar.Size = new System.Drawing.Size(646, 28);
            this.panelProgressBar.TabIndex = 4;
            // 
            // FormOperationDialog
            // 
            this.AcceptButton = this.butOk;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.butCancel;
            this.ClientSize = new System.Drawing.Size(654, 402);
            this.Controls.Add(this.panelLogAndProgressBar);
            this.Controls.Add(this.tableLayoutPanel1);
            this.Font = new System.Drawing.Font("Segoe UI", 8.25F);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FormOperationDialog";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "<Unknown>";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.FormOperationDialog_FormClosing);
            this.Shown += new System.EventHandler(this.FormOperationDialog_Shown);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.panelLogAndProgressBar.ResumeLayout(false);
            this.panelLog.ResumeLayout(false);
            this.panelProgressBar.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private DarkButton butOk;
        private DarkProgressBar pbStato;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private DarkButton butCancel;
        private System.Windows.Forms.RichTextBox lstLog;
        private System.Windows.Forms.Panel panelLogAndProgressBar;
        private System.Windows.Forms.Panel panelLog;
        private System.Windows.Forms.Panel panelProgressBar;
    }
}