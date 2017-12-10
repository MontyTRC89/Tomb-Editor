using DarkUI.Forms;

namespace TombLib.Forms
{
    partial class FormImportTr4Wad : DarkForm
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
            this.components = new System.ComponentModel.Container();
            this.butCancel = new DarkUI.Controls.DarkButton();
            this.butOK = new DarkUI.Controls.DarkButton();
            this.darkLabel1 = new DarkUI.Controls.DarkLabel();
            this.dgvSamples = new DarkUI.Controls.DarkDataGridView();
            this.columnSampleName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.columnSamplePath = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.columnFound = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this.butAddPath = new DarkUI.Controls.DarkButton();
            this.butDeletePath = new DarkUI.Controls.DarkButton();
            this.darkLabel2 = new DarkUI.Controls.DarkLabel();
            this.butReloadSamples = new DarkUI.Controls.DarkButton();
            this.lstPaths = new DarkUI.Controls.DarkListBox(this.components);
            this.folderBrowser = new System.Windows.Forms.FolderBrowserDialog();
            this.darkStatusStrip1 = new DarkUI.Controls.DarkStatusStrip();
            this.statusSamples = new System.Windows.Forms.ToolStripStatusLabel();
            ((System.ComponentModel.ISupportInitialize)(this.dgvSamples)).BeginInit();
            this.darkStatusStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // butCancel
            // 
            this.butCancel.Location = new System.Drawing.Point(336, 530);
            this.butCancel.Name = "butCancel";
            this.butCancel.Size = new System.Drawing.Size(84, 23);
            this.butCancel.TabIndex = 3;
            this.butCancel.Text = "Cancel";
            this.butCancel.Click += new System.EventHandler(this.butCancel_Click);
            // 
            // butOK
            // 
            this.butOK.Location = new System.Drawing.Point(246, 530);
            this.butOK.Name = "butOK";
            this.butOK.Size = new System.Drawing.Size(84, 23);
            this.butOK.TabIndex = 2;
            this.butOK.Text = "Ok";
            this.butOK.Click += new System.EventHandler(this.butOK_Click);
            // 
            // darkLabel1
            // 
            this.darkLabel1.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.darkLabel1.Location = new System.Drawing.Point(13, 13);
            this.darkLabel1.Name = "darkLabel1";
            this.darkLabel1.Size = new System.Drawing.Size(648, 20);
            this.darkLabel1.TabIndex = 4;
            this.darkLabel1.Text = "Some WAV samples are missing. You can locate them manually and also add new paths" +
    " where to search.";
            // 
            // dgvSamples
            // 
            this.dgvSamples.AllowUserToAddRows = false;
            this.dgvSamples.AllowUserToDeleteRows = false;
            this.dgvSamples.AllowUserToDragDropRows = false;
            this.dgvSamples.ColumnHeadersHeight = 17;
            this.dgvSamples.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.columnSampleName,
            this.columnSamplePath,
            this.columnFound});
            this.dgvSamples.Location = new System.Drawing.Point(16, 36);
            this.dgvSamples.Name = "dgvSamples";
            this.dgvSamples.RowHeadersWidth = 41;
            this.dgvSamples.Size = new System.Drawing.Size(645, 303);
            this.dgvSamples.TabIndex = 5;
            // 
            // columnSampleName
            // 
            this.columnSampleName.HeaderText = "Sample";
            this.columnSampleName.Name = "columnSampleName";
            this.columnSampleName.Width = 150;
            // 
            // columnSamplePath
            // 
            this.columnSamplePath.HeaderText = "Path";
            this.columnSamplePath.Name = "columnSamplePath";
            this.columnSamplePath.Width = 400;
            // 
            // columnFound
            // 
            this.columnFound.HeaderText = "Found";
            this.columnFound.Name = "columnFound";
            this.columnFound.ReadOnly = true;
            this.columnFound.Width = 50;
            // 
            // butAddPath
            // 
            this.butAddPath.Location = new System.Drawing.Point(487, 475);
            this.butAddPath.Name = "butAddPath";
            this.butAddPath.Size = new System.Drawing.Size(84, 23);
            this.butAddPath.TabIndex = 6;
            this.butAddPath.Text = "Add path";
            this.butAddPath.Click += new System.EventHandler(this.butAddPath_Click);
            // 
            // butDeletePath
            // 
            this.butDeletePath.Enabled = false;
            this.butDeletePath.Location = new System.Drawing.Point(577, 475);
            this.butDeletePath.Name = "butDeletePath";
            this.butDeletePath.Size = new System.Drawing.Size(84, 23);
            this.butDeletePath.TabIndex = 7;
            this.butDeletePath.Text = "Delete path";
            this.butDeletePath.Click += new System.EventHandler(this.butDeletePath_Click);
            // 
            // darkLabel2
            // 
            this.darkLabel2.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.darkLabel2.Location = new System.Drawing.Point(13, 354);
            this.darkLabel2.Name = "darkLabel2";
            this.darkLabel2.Size = new System.Drawing.Size(648, 20);
            this.darkLabel2.TabIndex = 8;
            this.darkLabel2.Text = "Search paths:";
            // 
            // butReloadSamples
            // 
            this.butReloadSamples.Location = new System.Drawing.Point(562, 7);
            this.butReloadSamples.Name = "butReloadSamples";
            this.butReloadSamples.Size = new System.Drawing.Size(99, 23);
            this.butReloadSamples.TabIndex = 10;
            this.butReloadSamples.Text = "Reload samples";
            this.butReloadSamples.Click += new System.EventHandler(this.butReloadSamples_Click);
            // 
            // lstPaths
            // 
            this.lstPaths.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(69)))), ((int)(((byte)(73)))), ((int)(((byte)(74)))));
            this.lstPaths.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.lstPaths.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed;
            this.lstPaths.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.lstPaths.FormattingEnabled = true;
            this.lstPaths.ItemHeight = 18;
            this.lstPaths.Location = new System.Drawing.Point(16, 377);
            this.lstPaths.Name = "lstPaths";
            this.lstPaths.Size = new System.Drawing.Size(645, 92);
            this.lstPaths.TabIndex = 11;
            this.lstPaths.Click += new System.EventHandler(this.lstPaths_Click);
            // 
            // darkStatusStrip1
            // 
            this.darkStatusStrip1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.darkStatusStrip1.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.darkStatusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.statusSamples});
            this.darkStatusStrip1.Location = new System.Drawing.Point(0, 567);
            this.darkStatusStrip1.Name = "darkStatusStrip1";
            this.darkStatusStrip1.Padding = new System.Windows.Forms.Padding(0, 5, 0, 3);
            this.darkStatusStrip1.Size = new System.Drawing.Size(673, 32);
            this.darkStatusStrip1.TabIndex = 12;
            this.darkStatusStrip1.Text = "darkStatusStrip1";
            // 
            // statusSamples
            // 
            this.statusSamples.Name = "statusSamples";
            this.statusSamples.Size = new System.Drawing.Size(0, 19);
            // 
            // FormImportTr4Wad
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(673, 599);
            this.ControlBox = false;
            this.Controls.Add(this.darkStatusStrip1);
            this.Controls.Add(this.lstPaths);
            this.Controls.Add(this.butReloadSamples);
            this.Controls.Add(this.darkLabel2);
            this.Controls.Add(this.butDeletePath);
            this.Controls.Add(this.butAddPath);
            this.Controls.Add(this.dgvSamples);
            this.Controls.Add(this.darkLabel1);
            this.Controls.Add(this.butCancel);
            this.Controls.Add(this.butOK);
            this.Font = new System.Drawing.Font("Segoe UI", 8.25F);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FormImportTr4Wad";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Import TR4 WAD";
            this.Load += new System.EventHandler(this.FormImportTr4Wad_Load);
            ((System.ComponentModel.ISupportInitialize)(this.dgvSamples)).EndInit();
            this.darkStatusStrip1.ResumeLayout(false);
            this.darkStatusStrip1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private DarkUI.Controls.DarkButton butCancel;
        private DarkUI.Controls.DarkButton butOK;
        private DarkUI.Controls.DarkLabel darkLabel1;
        private DarkUI.Controls.DarkDataGridView dgvSamples;
        private System.Windows.Forms.DataGridViewTextBoxColumn columnSampleName;
        private System.Windows.Forms.DataGridViewTextBoxColumn columnSamplePath;
        private System.Windows.Forms.DataGridViewCheckBoxColumn columnFound;
        private DarkUI.Controls.DarkButton butAddPath;
        private DarkUI.Controls.DarkButton butDeletePath;
        private DarkUI.Controls.DarkLabel darkLabel2;
        private DarkUI.Controls.DarkButton butReloadSamples;
        private DarkUI.Controls.DarkListBox lstPaths;
        private System.Windows.Forms.FolderBrowserDialog folderBrowser;
        private DarkUI.Controls.DarkStatusStrip darkStatusStrip1;
        private System.Windows.Forms.ToolStripStatusLabel statusSamples;
    }
}