using DarkUI.Forms;

namespace TombLib.Forms
{
    partial class Prj2SoundsConversionDialog : DarkForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Prj2SoundsConversionDialog));
            this.butCancel = new DarkUI.Controls.DarkButton();
            this.butOK = new DarkUI.Controls.DarkButton();
            this.darkLabel1 = new DarkUI.Controls.DarkLabel();
            this.dgvSoundInfos = new DarkUI.Controls.DarkDataGridView();
            this.nameColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.newIdColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.newNameColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.folderBrowser = new System.Windows.Forms.FolderBrowserDialog();
            this.darkStatusStrip1 = new DarkUI.Controls.DarkStatusStrip();
            this.statusSamples = new System.Windows.Forms.ToolStripStatusLabel();
            this.panel2 = new System.Windows.Forms.Panel();
            this.darkLabel3 = new DarkUI.Controls.DarkLabel();
            this.butSearchSoundsCatalogPath = new DarkUI.Controls.DarkButton();
            this.darkLabel7 = new DarkUI.Controls.DarkLabel();
            this.tbSoundsCatalogPath = new DarkUI.Controls.DarkTextBox();
            ((System.ComponentModel.ISupportInitialize)(this.dgvSoundInfos)).BeginInit();
            this.darkStatusStrip1.SuspendLayout();
            this.panel2.SuspendLayout();
            this.SuspendLayout();
            // 
            // butCancel
            // 
            this.butCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.butCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.butCancel.Location = new System.Drawing.Point(562, 606);
            this.butCancel.Name = "butCancel";
            this.butCancel.Size = new System.Drawing.Size(80, 23);
            this.butCancel.TabIndex = 3;
            this.butCancel.Text = "Cancel";
            this.butCancel.Click += new System.EventHandler(this.butCancel_Click);
            // 
            // butOK
            // 
            this.butOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.butOK.Location = new System.Drawing.Point(476, 606);
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
            this.darkLabel1.Size = new System.Drawing.Size(630, 55);
            this.darkLabel1.TabIndex = 4;
            this.darkLabel1.Text = resources.GetString("darkLabel1.Text");
            this.darkLabel1.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // dgvSoundInfos
            // 
            this.dgvSoundInfos.AllowUserToAddRows = false;
            this.dgvSoundInfos.AllowUserToDeleteRows = false;
            this.dgvSoundInfos.AllowUserToDragDropRows = false;
            this.dgvSoundInfos.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.dgvSoundInfos.ColumnHeadersHeight = 17;
            this.dgvSoundInfos.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.nameColumn,
            this.newIdColumn,
            this.newNameColumn});
            this.dgvSoundInfos.Location = new System.Drawing.Point(12, 70);
            this.dgvSoundInfos.Name = "dgvSoundInfos";
            this.dgvSoundInfos.RowHeadersWidth = 41;
            this.dgvSoundInfos.Size = new System.Drawing.Size(630, 448);
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
            // darkStatusStrip1
            // 
            this.darkStatusStrip1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.darkStatusStrip1.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.darkStatusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.statusSamples});
            this.darkStatusStrip1.Location = new System.Drawing.Point(0, 636);
            this.darkStatusStrip1.Name = "darkStatusStrip1";
            this.darkStatusStrip1.Padding = new System.Windows.Forms.Padding(0, 5, 0, 3);
            this.darkStatusStrip1.Size = new System.Drawing.Size(654, 28);
            this.darkStatusStrip1.TabIndex = 12;
            this.darkStatusStrip1.Text = "darkStatusStrip1";
            // 
            // statusSamples
            // 
            this.statusSamples.Name = "statusSamples";
            this.statusSamples.Size = new System.Drawing.Size(0, 0);
            // 
            // panel2
            // 
            this.panel2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panel2.Controls.Add(this.darkLabel3);
            this.panel2.Controls.Add(this.butSearchSoundsCatalogPath);
            this.panel2.Controls.Add(this.darkLabel7);
            this.panel2.Controls.Add(this.tbSoundsCatalogPath);
            this.panel2.Location = new System.Drawing.Point(12, 524);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(630, 76);
            this.panel2.TabIndex = 13;
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
            this.butSearchSoundsCatalogPath.Location = new System.Drawing.Point(535, 23);
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
            this.tbSoundsCatalogPath.Size = new System.Drawing.Size(529, 22);
            this.tbSoundsCatalogPath.TabIndex = 2;
            // 
            // Prj2SoundsConversionDialog
            // 
            this.AcceptButton = this.butOK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.butCancel;
            this.ClientSize = new System.Drawing.Size(654, 664);
            this.Controls.Add(this.panel2);
            this.Controls.Add(this.darkStatusStrip1);
            this.Controls.Add(this.dgvSoundInfos);
            this.Controls.Add(this.darkLabel1);
            this.Controls.Add(this.butCancel);
            this.Controls.Add(this.butOK);
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(670, 560);
            this.Name = "Prj2SoundsConversionDialog";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Convert Prj2 to Xml sound system";
            this.Load += new System.EventHandler(this.Prj2SoundsConversionDialog_Load);
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
        private System.Windows.Forms.DataGridViewTextBoxColumn nameColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn newIdColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn newNameColumn;
        private System.Windows.Forms.Panel panel2;
        private DarkUI.Controls.DarkButton butSearchSoundsCatalogPath;
        private DarkUI.Controls.DarkLabel darkLabel7;
        private DarkUI.Controls.DarkTextBox tbSoundsCatalogPath;
        private DarkUI.Controls.DarkLabel darkLabel3;
    }
}