using DarkUI.Controls;

namespace TombEditor
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
            this.pbStato = new System.Windows.Forms.ProgressBar();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.butCancel = new DarkUI.Controls.DarkButton();
            this.lstLog = new System.Windows.Forms.RichTextBox();
            this.tableLayoutPanel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // butOk
            // 
            this.butOk.Dock = System.Windows.Forms.DockStyle.Fill;
            this.butOk.Enabled = false;
            this.butOk.Location = new System.Drawing.Point(3, 3);
            this.butOk.Name = "butOk";
            this.butOk.Padding = new System.Windows.Forms.Padding(5);
            this.butOk.Size = new System.Drawing.Size(317, 26);
            this.butOk.TabIndex = 1;
            this.butOk.Text = "Ok";
            this.butOk.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.butOk.Click += new System.EventHandler(this.butOk_Click);
            // 
            // pbStato
            // 
            this.pbStato.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.pbStato.Location = new System.Drawing.Point(7, 339);
            this.pbStato.Name = "pbStato";
            this.pbStato.Size = new System.Drawing.Size(641, 23);
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
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(646, 32);
            this.tableLayoutPanel1.TabIndex = 4;
            // 
            // butCancel
            // 
            this.butCancel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.butCancel.Location = new System.Drawing.Point(326, 3);
            this.butCancel.Name = "butCancel";
            this.butCancel.Padding = new System.Windows.Forms.Padding(5);
            this.butCancel.Size = new System.Drawing.Size(317, 26);
            this.butCancel.TabIndex = 2;
            this.butCancel.Text = "Close";
            this.butCancel.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.butCancel.Click += new System.EventHandler(this.butCancel_Click);
            // 
            // lstLog
            // 
            this.lstLog.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lstLog.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.lstLog.Location = new System.Drawing.Point(7, 7);
            this.lstLog.Name = "lstLog";
            this.lstLog.Size = new System.Drawing.Size(641, 325);
            this.lstLog.TabIndex = 6;
            this.lstLog.Text = "";
            // 
            // FormOperationDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(654, 402);
            this.Controls.Add(this.tableLayoutPanel1);
            this.Controls.Add(this.pbStato);
            this.Controls.Add(this.lstLog);
            this.Font = new System.Drawing.Font("Segoe UI", 8.25F);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(160, 120);
            this.Name = "FormOperationDialog";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "<Unknown>";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.FormImportPRJ_FormClosing);
            this.Shown += new System.EventHandler(this.FormBuildLevel_Shown);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private DarkButton butOk;
        private System.Windows.Forms.ProgressBar pbStato;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private DarkButton butCancel;
        private System.Windows.Forms.RichTextBox lstLog;
    }
}