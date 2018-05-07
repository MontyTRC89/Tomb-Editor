namespace WadTool
{
    partial class FormSoundOverview
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
            this.soundInfosDataGridView = new DarkUI.Controls.DarkDataGridView();
            this.ColumnSoundInfoName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.darkLabel1 = new DarkUI.Controls.DarkLabel();
            this.soundInfosDataGridViewTxtSearch = new DarkUI.Controls.DarkTextBox();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.panel1 = new System.Windows.Forms.Panel();
            this.splitContainer2 = new System.Windows.Forms.SplitContainer();
            this.soundSettingsGroupBox = new DarkUI.Controls.DarkGroupBox();
            this.soundInfoEditor = new TombLib.Controls.SoundInfoEditor();
            this.panel2 = new System.Windows.Forms.Panel();
            this.usedForDataGridView = new DarkUI.Controls.DarkDataGridView();
            this.soundUseNameColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.darkLabel2 = new DarkUI.Controls.DarkLabel();
            this.btCancel = new DarkUI.Controls.DarkButton();
            this.btOk = new DarkUI.Controls.DarkButton();
            ((System.ComponentModel.ISupportInitialize)(this.soundInfosDataGridView)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).BeginInit();
            this.splitContainer2.Panel1.SuspendLayout();
            this.splitContainer2.Panel2.SuspendLayout();
            this.splitContainer2.SuspendLayout();
            this.soundSettingsGroupBox.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.usedForDataGridView)).BeginInit();
            this.SuspendLayout();
            // 
            // soundInfosDataGridView
            // 
            this.soundInfosDataGridView.AllowUserToAddRows = false;
            this.soundInfosDataGridView.AllowUserToDeleteRows = false;
            this.soundInfosDataGridView.AllowUserToDragDropRows = false;
            this.soundInfosDataGridView.AllowUserToPasteCells = false;
            this.soundInfosDataGridView.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.soundInfosDataGridView.AutoGenerateColumns = false;
            this.soundInfosDataGridView.ColumnHeadersHeight = 23;
            this.soundInfosDataGridView.ColumnHeadersVisible = false;
            this.soundInfosDataGridView.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.ColumnSoundInfoName});
            this.soundInfosDataGridView.Location = new System.Drawing.Point(12, 38);
            this.soundInfosDataGridView.MultiSelect = false;
            this.soundInfosDataGridView.Name = "soundInfosDataGridView";
            this.soundInfosDataGridView.RowHeadersWidth = 41;
            this.soundInfosDataGridView.Size = new System.Drawing.Size(320, 649);
            this.soundInfosDataGridView.TabIndex = 0;
            this.soundInfosDataGridView.CellFormattingSafe += new DarkUI.Controls.DarkDataGridViewSafeCellFormattingEventHandler(this.soundInfosDataGridView_CellFormattingSafe);
            this.soundInfosDataGridView.SelectionChanged += new System.EventHandler(this.soundInfosDataGridView_SelectionChanged);
            // 
            // ColumnSoundInfoName
            // 
            this.ColumnSoundInfoName.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.ColumnSoundInfoName.DataPropertyName = "Name";
            this.ColumnSoundInfoName.HeaderText = "Name";
            this.ColumnSoundInfoName.Name = "ColumnSoundInfoName";
            // 
            // darkLabel1
            // 
            this.darkLabel1.AutoSize = true;
            this.darkLabel1.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.darkLabel1.Location = new System.Drawing.Point(22, 15);
            this.darkLabel1.Name = "darkLabel1";
            this.darkLabel1.Size = new System.Drawing.Size(41, 13);
            this.darkLabel1.TabIndex = 1;
            this.darkLabel1.Text = "Search";
            // 
            // soundInfosDataGridViewTxtSearch
            // 
            this.soundInfosDataGridViewTxtSearch.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.soundInfosDataGridViewTxtSearch.Location = new System.Drawing.Point(67, 12);
            this.soundInfosDataGridViewTxtSearch.Name = "soundInfosDataGridViewTxtSearch";
            this.soundInfosDataGridViewTxtSearch.Size = new System.Drawing.Size(265, 20);
            this.soundInfosDataGridViewTxtSearch.TabIndex = 2;
            this.soundInfosDataGridViewTxtSearch.TextChanged += new System.EventHandler(this.soundInfosDataGridViewTxtSearch_TextChanged);
            // 
            // splitContainer1
            // 
            this.splitContainer1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.splitContainer1.Location = new System.Drawing.Point(0, 0);
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.soundInfosDataGridView);
            this.splitContainer1.Panel1.Controls.Add(this.soundInfosDataGridViewTxtSearch);
            this.splitContainer1.Panel1.Controls.Add(this.darkLabel1);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.panel1);
            this.splitContainer1.Panel2.Controls.Add(this.splitContainer2);
            this.splitContainer1.Size = new System.Drawing.Size(1016, 702);
            this.splitContainer1.SplitterDistance = 335;
            this.splitContainer1.TabIndex = 3;
            // 
            // panel1
            // 
            this.panel1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.panel1.BackColor = System.Drawing.Color.Gray;
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(2, 701);
            this.panel1.TabIndex = 0;
            // 
            // splitContainer2
            // 
            this.splitContainer2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer2.Location = new System.Drawing.Point(0, 0);
            this.splitContainer2.Name = "splitContainer2";
            this.splitContainer2.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer2.Panel1
            // 
            this.splitContainer2.Panel1.Controls.Add(this.soundSettingsGroupBox);
            // 
            // splitContainer2.Panel2
            // 
            this.splitContainer2.Panel2.Controls.Add(this.panel2);
            this.splitContainer2.Panel2.Controls.Add(this.usedForDataGridView);
            this.splitContainer2.Panel2.Controls.Add(this.darkLabel2);
            this.splitContainer2.Size = new System.Drawing.Size(677, 702);
            this.splitContainer2.SplitterDistance = 410;
            this.splitContainer2.TabIndex = 23;
            // 
            // soundSettingsGroupBox
            // 
            this.soundSettingsGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.soundSettingsGroupBox.Controls.Add(this.soundInfoEditor);
            this.soundSettingsGroupBox.Location = new System.Drawing.Point(8, 5);
            this.soundSettingsGroupBox.Name = "soundSettingsGroupBox";
            this.soundSettingsGroupBox.Size = new System.Drawing.Size(654, 402);
            this.soundSettingsGroupBox.TabIndex = 23;
            this.soundSettingsGroupBox.TabStop = false;
            this.soundSettingsGroupBox.Text = "Sound settings";
            // 
            // soundInfoEditor
            // 
            this.soundInfoEditor.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.soundInfoEditor.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.soundInfoEditor.Location = new System.Drawing.Point(15, 19);
            this.soundInfoEditor.MinimumSize = new System.Drawing.Size(442, 346);
            this.soundInfoEditor.Name = "soundInfoEditor";
            this.soundInfoEditor.ReadOnly = true;
            this.soundInfoEditor.Size = new System.Drawing.Size(633, 378);
            this.soundInfoEditor.TabIndex = 22;
            this.soundInfoEditor.SoundInfoChanged += new System.EventHandler(this.soundInfoEditor_SoundInfoChanged);
            // 
            // panel2
            // 
            this.panel2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panel2.BackColor = System.Drawing.Color.Gray;
            this.panel2.Location = new System.Drawing.Point(4, 0);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(673, 2);
            this.panel2.TabIndex = 2;
            // 
            // usedForDataGridView
            // 
            this.usedForDataGridView.AllowUserToAddRows = false;
            this.usedForDataGridView.AllowUserToDeleteRows = false;
            this.usedForDataGridView.AllowUserToDragDropRows = false;
            this.usedForDataGridView.AllowUserToPasteCells = false;
            this.usedForDataGridView.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.usedForDataGridView.AutoGenerateColumns = false;
            this.usedForDataGridView.ColumnHeadersHeight = 23;
            this.usedForDataGridView.ColumnHeadersVisible = false;
            this.usedForDataGridView.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.soundUseNameColumn});
            this.usedForDataGridView.Location = new System.Drawing.Point(8, 28);
            this.usedForDataGridView.Name = "usedForDataGridView";
            this.usedForDataGridView.RowHeadersWidth = 41;
            this.usedForDataGridView.Size = new System.Drawing.Size(616, 235);
            this.usedForDataGridView.TabIndex = 1;
            // 
            // soundUseNameColumn
            // 
            this.soundUseNameColumn.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.soundUseNameColumn.DataPropertyName = "Name";
            this.soundUseNameColumn.HeaderText = "Name";
            this.soundUseNameColumn.Name = "soundUseNameColumn";
            this.soundUseNameColumn.ReadOnly = true;
            // 
            // darkLabel2
            // 
            this.darkLabel2.AutoSize = true;
            this.darkLabel2.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.darkLabel2.Location = new System.Drawing.Point(8, 12);
            this.darkLabel2.Name = "darkLabel2";
            this.darkLabel2.Size = new System.Drawing.Size(50, 13);
            this.darkLabel2.TabIndex = 1;
            this.darkLabel2.Text = "Used for:";
            // 
            // btCancel
            // 
            this.btCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btCancel.Location = new System.Drawing.Point(772, 708);
            this.btCancel.Name = "btCancel";
            this.btCancel.Size = new System.Drawing.Size(113, 26);
            this.btCancel.TabIndex = 50;
            this.btCancel.Text = "Cancel";
            this.btCancel.Click += new System.EventHandler(this.btCancel_Click);
            // 
            // btOk
            // 
            this.btOk.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btOk.Location = new System.Drawing.Point(891, 708);
            this.btOk.Name = "btOk";
            this.btOk.Size = new System.Drawing.Size(113, 26);
            this.btOk.TabIndex = 51;
            this.btOk.Text = "Ok";
            this.btOk.Click += new System.EventHandler(this.btOk_Click);
            // 
            // FormSoundOverview
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1016, 741);
            this.Controls.Add(this.btCancel);
            this.Controls.Add(this.btOk);
            this.Controls.Add(this.splitContainer1);
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(740, 687);
            this.Name = "FormSoundOverview";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Sound Overview";
            ((System.ComponentModel.ISupportInitialize)(this.soundInfosDataGridView)).EndInit();
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel1.PerformLayout();
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.splitContainer2.Panel1.ResumeLayout(false);
            this.splitContainer2.Panel2.ResumeLayout(false);
            this.splitContainer2.Panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).EndInit();
            this.splitContainer2.ResumeLayout(false);
            this.soundSettingsGroupBox.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.usedForDataGridView)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private DarkUI.Controls.DarkDataGridView soundInfosDataGridView;
        private DarkUI.Controls.DarkLabel darkLabel1;
        private DarkUI.Controls.DarkTextBox soundInfosDataGridViewTxtSearch;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private DarkUI.Controls.DarkDataGridView usedForDataGridView;
        private System.Windows.Forms.Panel panel1;
        private DarkUI.Controls.DarkButton btCancel;
        private DarkUI.Controls.DarkButton btOk;
        private DarkUI.Controls.DarkLabel darkLabel2;
        private DarkUI.Controls.DarkGroupBox soundSettingsGroupBox;
        private TombLib.Controls.SoundInfoEditor soundInfoEditor;
        private System.Windows.Forms.DataGridViewTextBoxColumn ColumnSoundInfoName;
        private System.Windows.Forms.DataGridViewTextBoxColumn soundUseNameColumn;
        private System.Windows.Forms.SplitContainer splitContainer2;
        private System.Windows.Forms.Panel panel2;
    }
}