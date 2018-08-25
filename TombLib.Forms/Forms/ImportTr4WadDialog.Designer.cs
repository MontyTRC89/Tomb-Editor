using DarkUI.Forms;

namespace TombLib.Forms
{
    partial class ImportTr4WadDialog : DarkForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ImportTr4WadDialog));
            this.butCancel = new DarkUI.Controls.DarkButton();
            this.butOK = new DarkUI.Controls.DarkButton();
            this.darkLabel1 = new DarkUI.Controls.DarkLabel();
            this.dgvSamples = new DarkUI.Controls.DarkDataGridView();
            this.columnSampleName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.columnSamplePath = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.columnSearch = new DarkUI.Controls.DarkDataGridViewButtonColumn();
            this.columnFound = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this.darkLabel2 = new DarkUI.Controls.DarkLabel();
            this.lstPaths = new DarkUI.Controls.DarkListBox(this.components);
            this.folderBrowser = new System.Windows.Forms.FolderBrowserDialog();
            this.darkStatusStrip1 = new DarkUI.Controls.DarkStatusStrip();
            this.statusSamples = new System.Windows.Forms.ToolStripStatusLabel();
            this.butReloadSamples = new DarkUI.Controls.DarkButton();
            this.butDeletePath = new DarkUI.Controls.DarkButton();
            this.butAddPath = new DarkUI.Controls.DarkButton();
            ((System.ComponentModel.ISupportInitialize)(this.dgvSamples)).BeginInit();
            this.darkStatusStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // butCancel
            // 
            this.butCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.butCancel.Location = new System.Drawing.Point(570, 475);
            this.butCancel.Name = "butCancel";
            this.butCancel.Size = new System.Drawing.Size(80, 23);
            this.butCancel.TabIndex = 3;
            this.butCancel.Text = "Cancel";
            this.butCancel.Click += new System.EventHandler(this.butCancel_Click);
            // 
            // butOK
            // 
            this.butOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.butOK.Location = new System.Drawing.Point(484, 475);
            this.butOK.Name = "butOK";
            this.butOK.Size = new System.Drawing.Size(80, 23);
            this.butOK.TabIndex = 2;
            this.butOK.Text = "OK";
            this.butOK.Click += new System.EventHandler(this.butOK_Click);
            // 
            // darkLabel1
            // 
            this.darkLabel1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.darkLabel1.BackColor = System.Drawing.Color.DarkGreen;
            this.darkLabel1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.darkLabel1.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.darkLabel1.Location = new System.Drawing.Point(12, 9);
            this.darkLabel1.Name = "darkLabel1";
            this.darkLabel1.Padding = new System.Windows.Forms.Padding(4);
            this.darkLabel1.Size = new System.Drawing.Size(532, 55);
            this.darkLabel1.TabIndex = 4;
            this.darkLabel1.Text = resources.GetString("darkLabel1.Text");
            this.darkLabel1.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // dgvSamples
            // 
            this.dgvSamples.AllowUserToAddRows = false;
            this.dgvSamples.AllowUserToDeleteRows = false;
            this.dgvSamples.AllowUserToDragDropRows = false;
            this.dgvSamples.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.dgvSamples.ColumnHeadersHeight = 17;
            this.dgvSamples.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.columnSampleName,
            this.columnSamplePath,
            this.columnSearch,
            this.columnFound});
            this.dgvSamples.Location = new System.Drawing.Point(12, 70);
            this.dgvSamples.Name = "dgvSamples";
            this.dgvSamples.RowHeadersWidth = 41;
            this.dgvSamples.Size = new System.Drawing.Size(638, 294);
            this.dgvSamples.TabIndex = 5;
            this.dgvSamples.CellContentClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dgvSamples_CellContentClick);
            // 
            // columnSampleName
            // 
            this.columnSampleName.HeaderText = "Sample";
            this.columnSampleName.Name = "columnSampleName";
            this.columnSampleName.Width = 150;
            // 
            // columnSamplePath
            // 
            this.columnSamplePath.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.columnSamplePath.HeaderText = "Path";
            this.columnSamplePath.Name = "columnSamplePath";
            // 
            // columnSearch
            // 
            this.columnSearch.HeaderText = "";
            this.columnSearch.Name = "columnSearch";
            this.columnSearch.Resizable = System.Windows.Forms.DataGridViewTriState.False;
            this.columnSearch.Text = "Search";
            this.columnSearch.Width = 60;
            // 
            // columnFound
            // 
            this.columnFound.HeaderText = "Found";
            this.columnFound.Name = "columnFound";
            this.columnFound.ReadOnly = true;
            this.columnFound.Width = 50;
            // 
            // darkLabel2
            // 
            this.darkLabel2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.darkLabel2.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.darkLabel2.Location = new System.Drawing.Point(9, 369);
            this.darkLabel2.Name = "darkLabel2";
            this.darkLabel2.Size = new System.Drawing.Size(656, 20);
            this.darkLabel2.TabIndex = 8;
            this.darkLabel2.Text = "Search paths:";
            // 
            // lstPaths
            // 
            this.lstPaths.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lstPaths.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(69)))), ((int)(((byte)(73)))), ((int)(((byte)(74)))));
            this.lstPaths.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.lstPaths.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed;
            this.lstPaths.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.lstPaths.FormattingEnabled = true;
            this.lstPaths.IntegralHeight = false;
            this.lstPaths.ItemHeight = 18;
            this.lstPaths.Location = new System.Drawing.Point(12, 386);
            this.lstPaths.Name = "lstPaths";
            this.lstPaths.Size = new System.Drawing.Size(638, 83);
            this.lstPaths.TabIndex = 11;
            this.lstPaths.Click += new System.EventHandler(this.lstPaths_Click);
            this.lstPaths.KeyDown += new System.Windows.Forms.KeyEventHandler(this.lstPaths_KeyDown);
            // 
            // darkStatusStrip1
            // 
            this.darkStatusStrip1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.darkStatusStrip1.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.darkStatusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.statusSamples});
            this.darkStatusStrip1.Location = new System.Drawing.Point(0, 505);
            this.darkStatusStrip1.Name = "darkStatusStrip1";
            this.darkStatusStrip1.Padding = new System.Windows.Forms.Padding(0, 5, 0, 3);
            this.darkStatusStrip1.Size = new System.Drawing.Size(662, 28);
            this.darkStatusStrip1.TabIndex = 12;
            this.darkStatusStrip1.Text = "darkStatusStrip1";
            // 
            // statusSamples
            // 
            this.statusSamples.Name = "statusSamples";
            this.statusSamples.Size = new System.Drawing.Size(0, 0);
            // 
            // butReloadSamples
            // 
            this.butReloadSamples.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.butReloadSamples.Image = global::TombLib.Properties.Resources.actions_refresh_16;
            this.butReloadSamples.Location = new System.Drawing.Point(550, 9);
            this.butReloadSamples.Name = "butReloadSamples";
            this.butReloadSamples.Size = new System.Drawing.Size(100, 55);
            this.butReloadSamples.TabIndex = 10;
            this.butReloadSamples.Text = "Reload samples";
            this.butReloadSamples.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText;
            this.butReloadSamples.Click += new System.EventHandler(this.butReloadSamples_Click);
            // 
            // butDeletePath
            // 
            this.butDeletePath.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.butDeletePath.Enabled = false;
            this.butDeletePath.Image = global::TombLib.Properties.Resources.general_trash_16;
            this.butDeletePath.Location = new System.Drawing.Point(118, 475);
            this.butDeletePath.Name = "butDeletePath";
            this.butDeletePath.Size = new System.Drawing.Size(100, 23);
            this.butDeletePath.TabIndex = 7;
            this.butDeletePath.Text = "Delete path";
            this.butDeletePath.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.butDeletePath.Click += new System.EventHandler(this.butDeletePath_Click);
            // 
            // butAddPath
            // 
            this.butAddPath.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.butAddPath.Image = global::TombLib.Properties.Resources.general_plus_math_16;
            this.butAddPath.Location = new System.Drawing.Point(12, 475);
            this.butAddPath.Name = "butAddPath";
            this.butAddPath.Size = new System.Drawing.Size(100, 23);
            this.butAddPath.TabIndex = 6;
            this.butAddPath.Text = " Add path";
            this.butAddPath.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.butAddPath.Click += new System.EventHandler(this.butAddPath_Click);
            // 
            // ImportTr4WadDialog
            // 
            this.AcceptButton = this.butOK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.butCancel;
            this.ClientSize = new System.Drawing.Size(662, 533);
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
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(670, 560);
            this.Name = "ImportTr4WadDialog";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
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
        private DarkUI.Controls.DarkButton butAddPath;
        private DarkUI.Controls.DarkButton butDeletePath;
        private DarkUI.Controls.DarkLabel darkLabel2;
        private DarkUI.Controls.DarkButton butReloadSamples;
        private DarkUI.Controls.DarkListBox lstPaths;
        private System.Windows.Forms.FolderBrowserDialog folderBrowser;
        private DarkUI.Controls.DarkStatusStrip darkStatusStrip1;
        private System.Windows.Forms.ToolStripStatusLabel statusSamples;
        private System.Windows.Forms.DataGridViewTextBoxColumn columnSampleName;
        private System.Windows.Forms.DataGridViewTextBoxColumn columnSamplePath;
        private DarkUI.Controls.DarkDataGridViewButtonColumn columnSearch;
        private System.Windows.Forms.DataGridViewCheckBoxColumn columnFound;
    }
}