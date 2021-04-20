using DarkUI.Forms;

namespace TombLib.Forms
{
    partial class Wad2SoundsConversionDialog : DarkForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Wad2SoundsConversionDialog));
            this.butCancel = new DarkUI.Controls.DarkButton();
            this.butOK = new DarkUI.Controls.DarkButton();
            this.darkLabel1 = new DarkUI.Controls.DarkLabel();
            this.dgvSoundInfos = new DarkUI.Controls.DarkDataGridView();
            this.nameColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.newIdColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.newNameColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.columnFound = new DarkUI.Controls.DarkDataGridViewCheckBoxColumn();
            this.ExportSamplesColumn = new DarkUI.Controls.DarkDataGridViewCheckBoxColumn();
            this.folderBrowser = new System.Windows.Forms.FolderBrowserDialog();
            this.darkStatusStrip1 = new DarkUI.Controls.DarkStatusStrip();
            this.statusSamples = new System.Windows.Forms.ToolStripStatusLabel();
            this.butSelectAll = new DarkUI.Controls.DarkButton();
            this.butUnselectAll = new DarkUI.Controls.DarkButton();
            this.panel2 = new System.Windows.Forms.Panel();
            this.darkLabel3 = new DarkUI.Controls.DarkLabel();
            this.butSearchSoundsCatalogPath = new DarkUI.Controls.DarkButton();
            this.darkLabel7 = new DarkUI.Controls.DarkLabel();
            this.tbSoundsCatalogPath = new DarkUI.Controls.DarkTextBox();
            this.butUnselectAllSamples = new DarkUI.Controls.DarkButton();
            this.butSelectAllSamples = new DarkUI.Controls.DarkButton();
            ((System.ComponentModel.ISupportInitialize)(this.dgvSoundInfos)).BeginInit();
            this.darkStatusStrip1.SuspendLayout();
            this.panel2.SuspendLayout();
            this.SuspendLayout();
            // 
            // butCancel
            // 
            this.butCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.butCancel.Checked = false;
            this.butCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.butCancel.Location = new System.Drawing.Point(645, 595);
            this.butCancel.Name = "butCancel";
            this.butCancel.Size = new System.Drawing.Size(80, 23);
            this.butCancel.TabIndex = 3;
            this.butCancel.Text = "Cancel";
            this.butCancel.Click += new System.EventHandler(this.butCancel_Click);
            // 
            // butOK
            // 
            this.butOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.butOK.Checked = false;
            this.butOK.Location = new System.Drawing.Point(559, 595);
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
            this.darkLabel1.Size = new System.Drawing.Size(713, 55);
            this.darkLabel1.TabIndex = 4;
            this.darkLabel1.Text = resources.GetString("darkLabel1.Text");
            this.darkLabel1.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // dgvSoundInfos
            // 
            this.dgvSoundInfos.AllowUserToAddRows = false;
            this.dgvSoundInfos.AllowUserToDeleteRows = false;
            this.dgvSoundInfos.AllowUserToDragDropRows = false;
            this.dgvSoundInfos.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.dgvSoundInfos.ColumnHeadersHeight = 17;
            this.dgvSoundInfos.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.nameColumn,
            this.newIdColumn,
            this.newNameColumn,
            this.columnFound,
            this.ExportSamplesColumn});
            this.dgvSoundInfos.Location = new System.Drawing.Point(12, 70);
            this.dgvSoundInfos.Name = "dgvSoundInfos";
            this.dgvSoundInfos.RowHeadersWidth = 41;
            this.dgvSoundInfos.Size = new System.Drawing.Size(713, 410);
            this.dgvSoundInfos.TabIndex = 5;
            this.dgvSoundInfos.CellContentClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dgvSamples_CellContentClick);
            this.dgvSoundInfos.CellValidated += new System.Windows.Forms.DataGridViewCellEventHandler(this.DgvSoundInfos_CellValidated);
            this.dgvSoundInfos.CellValidating += new System.Windows.Forms.DataGridViewCellValidatingEventHandler(this.DgvSoundInfos_CellValidating);
            // 
            // nameColumn
            // 
            this.nameColumn.HeaderText = "Name";
            this.nameColumn.Name = "nameColumn";
            this.nameColumn.Width = 300;
            // 
            // newIdColumn
            // 
            this.newIdColumn.HeaderText = "New Id";
            this.newIdColumn.Name = "newIdColumn";
            this.newIdColumn.Width = 60;
            // 
            // newNameColumn
            // 
            this.newNameColumn.HeaderText = "New name";
            this.newNameColumn.Name = "newNameColumn";
            this.newNameColumn.Width = 150;
            // 
            // columnFound
            // 
            this.columnFound.HeaderText = "Save to Xml";
            this.columnFound.Name = "columnFound";
            this.columnFound.ReadOnly = true;
            this.columnFound.Width = 80;
            // 
            // ExportSamplesColumn
            // 
            this.ExportSamplesColumn.HeaderText = "Export samples";
            this.ExportSamplesColumn.Name = "ExportSamplesColumn";
            // 
            // darkStatusStrip1
            // 
            this.darkStatusStrip1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.darkStatusStrip1.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.darkStatusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.statusSamples});
            this.darkStatusStrip1.Location = new System.Drawing.Point(0, 625);
            this.darkStatusStrip1.Name = "darkStatusStrip1";
            this.darkStatusStrip1.Padding = new System.Windows.Forms.Padding(0, 5, 0, 3);
            this.darkStatusStrip1.Size = new System.Drawing.Size(737, 28);
            this.darkStatusStrip1.TabIndex = 12;
            this.darkStatusStrip1.Text = "darkStatusStrip1";
            // 
            // statusSamples
            // 
            this.statusSamples.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.statusSamples.Name = "statusSamples";
            this.statusSamples.Size = new System.Drawing.Size(0, 0);
            // 
            // butSelectAll
            // 
            this.butSelectAll.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.butSelectAll.BackColor = System.Drawing.Color.Green;
            this.butSelectAll.Checked = false;
            this.butSelectAll.Location = new System.Drawing.Point(12, 486);
            this.butSelectAll.Name = "butSelectAll";
            this.butSelectAll.Size = new System.Drawing.Size(124, 23);
            this.butSelectAll.TabIndex = 13;
            this.butSelectAll.Text = "Save all to Xml";
            this.butSelectAll.Click += new System.EventHandler(this.ButSelectAll_Click);
            // 
            // butUnselectAll
            // 
            this.butUnselectAll.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.butUnselectAll.BackColor = System.Drawing.Color.Red;
            this.butUnselectAll.Checked = false;
            this.butUnselectAll.Location = new System.Drawing.Point(142, 486);
            this.butUnselectAll.Name = "butUnselectAll";
            this.butUnselectAll.Size = new System.Drawing.Size(124, 23);
            this.butUnselectAll.TabIndex = 14;
            this.butUnselectAll.Text = "Don\'t save to Xml";
            this.butUnselectAll.Click += new System.EventHandler(this.ButUnselectAll_Click);
            // 
            // panel2
            // 
            this.panel2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panel2.Controls.Add(this.darkLabel3);
            this.panel2.Controls.Add(this.butSearchSoundsCatalogPath);
            this.panel2.Controls.Add(this.darkLabel7);
            this.panel2.Controls.Add(this.tbSoundsCatalogPath);
            this.panel2.Location = new System.Drawing.Point(12, 513);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(713, 76);
            this.panel2.TabIndex = 15;
            // 
            // darkLabel3
            // 
            this.darkLabel3.ForeColor = System.Drawing.Color.Silver;
            this.darkLabel3.Location = new System.Drawing.Point(0, 48);
            this.darkLabel3.Name = "darkLabel3";
            this.darkLabel3.Size = new System.Drawing.Size(627, 28);
            this.darkLabel3.TabIndex = 11;
            this.darkLabel3.Text = "Specifiy sound catalog (sounds.txt or xml file). If not specified, TrCatalog.xml " +
    "will be used.";
            // 
            // butSearchSoundsCatalogPath
            // 
            this.butSearchSoundsCatalogPath.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.butSearchSoundsCatalogPath.Checked = false;
            this.butSearchSoundsCatalogPath.Location = new System.Drawing.Point(533, 23);
            this.butSearchSoundsCatalogPath.Name = "butSearchSoundsCatalogPath";
            this.butSearchSoundsCatalogPath.Size = new System.Drawing.Size(92, 22);
            this.butSearchSoundsCatalogPath.TabIndex = 3;
            this.butSearchSoundsCatalogPath.Text = "Search";
            this.butSearchSoundsCatalogPath.Click += new System.EventHandler(this.butSearchSoundsCatalogPath_Click);
            // 
            // darkLabel7
            // 
            this.darkLabel7.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.darkLabel7.Location = new System.Drawing.Point(0, 3);
            this.darkLabel7.Name = "darkLabel7";
            this.darkLabel7.Size = new System.Drawing.Size(439, 17);
            this.darkLabel7.TabIndex = 1;
            this.darkLabel7.Text = "Optional sound catalog file (TXT, XML or SFX):";
            // 
            // tbSoundsCatalogPath
            // 
            this.tbSoundsCatalogPath.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tbSoundsCatalogPath.Location = new System.Drawing.Point(0, 23);
            this.tbSoundsCatalogPath.Name = "tbSoundsCatalogPath";
            this.tbSoundsCatalogPath.Size = new System.Drawing.Size(527, 22);
            this.tbSoundsCatalogPath.TabIndex = 2;
            // 
            // butUnselectAllSamples
            // 
            this.butUnselectAllSamples.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.butUnselectAllSamples.BackColor = System.Drawing.Color.Red;
            this.butUnselectAllSamples.Checked = false;
            this.butUnselectAllSamples.Location = new System.Drawing.Point(601, 486);
            this.butUnselectAllSamples.Name = "butUnselectAllSamples";
            this.butUnselectAllSamples.Size = new System.Drawing.Size(124, 23);
            this.butUnselectAllSamples.TabIndex = 17;
            this.butUnselectAllSamples.Text = "Don\'t export samples";
            this.butUnselectAllSamples.Click += new System.EventHandler(this.butUnselectAllSamples_Click);
            // 
            // butSelectAllSamples
            // 
            this.butSelectAllSamples.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.butSelectAllSamples.BackColor = System.Drawing.Color.Green;
            this.butSelectAllSamples.Checked = false;
            this.butSelectAllSamples.Location = new System.Drawing.Point(471, 486);
            this.butSelectAllSamples.Name = "butSelectAllSamples";
            this.butSelectAllSamples.Size = new System.Drawing.Size(124, 23);
            this.butSelectAllSamples.TabIndex = 16;
            this.butSelectAllSamples.Text = "Export all samples";
            this.butSelectAllSamples.Click += new System.EventHandler(this.butSelectAllSamples_Click);
            // 
            // Wad2SoundsConversionDialog
            // 
            this.AcceptButton = this.butOK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.butCancel;
            this.ClientSize = new System.Drawing.Size(737, 653);
            this.Controls.Add(this.butUnselectAllSamples);
            this.Controls.Add(this.butSelectAllSamples);
            this.Controls.Add(this.panel2);
            this.Controls.Add(this.butUnselectAll);
            this.Controls.Add(this.butSelectAll);
            this.Controls.Add(this.darkStatusStrip1);
            this.Controls.Add(this.dgvSoundInfos);
            this.Controls.Add(this.darkLabel1);
            this.Controls.Add(this.butCancel);
            this.Controls.Add(this.butOK);
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(670, 560);
            this.Name = "Wad2SoundsConversionDialog";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Convert Wad2 to Xml sound system";
            this.Load += new System.EventHandler(this.Wad2SoundsConversionDialog_Load);
            ((System.ComponentModel.ISupportInitialize)(this.dgvSoundInfos)).EndInit();
            this.darkStatusStrip1.ResumeLayout(false);
            this.darkStatusStrip1.PerformLayout();
            this.panel2.ResumeLayout(false);
            this.panel2.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private DarkUI.Controls.DarkButton butCancel;
        private DarkUI.Controls.DarkButton butOK;
        private DarkUI.Controls.DarkLabel darkLabel1;
        private DarkUI.Controls.DarkDataGridView dgvSoundInfos;
        private System.Windows.Forms.FolderBrowserDialog folderBrowser;
        private DarkUI.Controls.DarkStatusStrip darkStatusStrip1;
        private System.Windows.Forms.ToolStripStatusLabel statusSamples;
        private DarkUI.Controls.DarkButton butSelectAll;
        private DarkUI.Controls.DarkButton butUnselectAll;
        private System.Windows.Forms.DataGridViewTextBoxColumn nameColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn newIdColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn newNameColumn;
        private DarkUI.Controls.DarkDataGridViewCheckBoxColumn columnFound;
        private DarkUI.Controls.DarkDataGridViewCheckBoxColumn ExportSamplesColumn;
        private System.Windows.Forms.Panel panel2;
        private DarkUI.Controls.DarkLabel darkLabel3;
        private DarkUI.Controls.DarkButton butSearchSoundsCatalogPath;
        private DarkUI.Controls.DarkLabel darkLabel7;
        private DarkUI.Controls.DarkTextBox tbSoundsCatalogPath;
        private DarkUI.Controls.DarkButton butUnselectAllSamples;
        private DarkUI.Controls.DarkButton butSelectAllSamples;
    }
}