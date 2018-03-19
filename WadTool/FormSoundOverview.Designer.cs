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
            this.darkLabel1 = new DarkUI.Controls.DarkLabel();
            this.soundInfosDataGridViewTxtSearch = new DarkUI.Controls.DarkTextBox();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.panel1 = new System.Windows.Forms.Panel();
            this.darkDataGridView2 = new DarkUI.Controls.DarkDataGridView();
            this.btCancel = new DarkUI.Controls.DarkButton();
            this.btOk = new DarkUI.Controls.DarkButton();
            ((System.ComponentModel.ISupportInitialize)(this.soundInfosDataGridView)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.darkDataGridView2)).BeginInit();
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
            this.soundInfosDataGridView.ColumnHeadersHeight = 4;
            this.soundInfosDataGridView.ColumnHeadersVisible = false;
            this.soundInfosDataGridView.Location = new System.Drawing.Point(12, 38);
            this.soundInfosDataGridView.Name = "soundInfosDataGridView";
            this.soundInfosDataGridView.RowHeadersWidth = 41;
            this.soundInfosDataGridView.Size = new System.Drawing.Size(221, 566);
            this.soundInfosDataGridView.TabIndex = 0;
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
            this.soundInfosDataGridViewTxtSearch.Size = new System.Drawing.Size(166, 20);
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
            this.splitContainer1.Panel2.Controls.Add(this.darkDataGridView2);
            this.splitContainer1.Panel2.Controls.Add(this.panel1);
            this.splitContainer1.Size = new System.Drawing.Size(712, 619);
            this.splitContainer1.SplitterDistance = 236;
            this.splitContainer1.SplitterWidth = 2;
            this.splitContainer1.TabIndex = 3;
            // 
            // panel1
            // 
            this.panel1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.panel1.BackColor = System.Drawing.Color.Gray;
            this.panel1.Location = new System.Drawing.Point(5, 5);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(2, 611);
            this.panel1.TabIndex = 0;
            // 
            // darkDataGridView2
            // 
            this.darkDataGridView2.AllowUserToAddRows = false;
            this.darkDataGridView2.AllowUserToDeleteRows = false;
            this.darkDataGridView2.AllowUserToDragDropRows = false;
            this.darkDataGridView2.AllowUserToPasteCells = false;
            this.darkDataGridView2.ColumnHeadersHeight = 4;
            this.darkDataGridView2.ColumnHeadersVisible = false;
            this.darkDataGridView2.Location = new System.Drawing.Point(13, 38);
            this.darkDataGridView2.Name = "darkDataGridView2";
            this.darkDataGridView2.RowHeadersWidth = 41;
            this.darkDataGridView2.Size = new System.Drawing.Size(449, 150);
            this.darkDataGridView2.TabIndex = 1;
            // 
            // btCancel
            // 
            this.btCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btCancel.Location = new System.Drawing.Point(468, 625);
            this.btCancel.Name = "btCancel";
            this.btCancel.Size = new System.Drawing.Size(113, 26);
            this.btCancel.TabIndex = 50;
            this.btCancel.Text = "Cancel";
            this.btCancel.Click += new System.EventHandler(this.btCancel_Click);
            // 
            // btOk
            // 
            this.btOk.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btOk.Location = new System.Drawing.Point(587, 625);
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
            this.ClientSize = new System.Drawing.Size(712, 658);
            this.Controls.Add(this.btCancel);
            this.Controls.Add(this.btOk);
            this.Controls.Add(this.splitContainer1);
            this.MinimizeBox = false;
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
            ((System.ComponentModel.ISupportInitialize)(this.darkDataGridView2)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private DarkUI.Controls.DarkDataGridView soundInfosDataGridView;
        private DarkUI.Controls.DarkLabel darkLabel1;
        private DarkUI.Controls.DarkTextBox soundInfosDataGridViewTxtSearch;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private DarkUI.Controls.DarkDataGridView darkDataGridView2;
        private System.Windows.Forms.Panel panel1;
        private DarkUI.Controls.DarkButton btCancel;
        private DarkUI.Controls.DarkButton btOk;
    }
}