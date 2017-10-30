namespace TombEditor.Controls
{
    partial class ImportedGeometryManager
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

        #region Component Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.dataGridViewControls = new TombEditor.Controls.DarkDataGridViewControls();
            this.dataGridView = new DarkUI.Controls.DarkDataGridView();
            this.nameColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.pathColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.searchButtonColumn = new DarkUI.Controls.DarkDataGridViewButtonColumn();
            this.scaleColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.errorMessageColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView)).BeginInit();
            this.SuspendLayout();
            // 
            // dataGridViewControls
            // 
            this.dataGridViewControls.AllowUserMove = false;
            this.dataGridViewControls.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.dataGridViewControls.Enabled = false;
            this.dataGridViewControls.Location = new System.Drawing.Point(850, 0);
            this.dataGridViewControls.MinimumSize = new System.Drawing.Size(24, 100);
            this.dataGridViewControls.Name = "dataGridViewControls";
            this.dataGridViewControls.Size = new System.Drawing.Size(24, 330);
            this.dataGridViewControls.TabIndex = 1;
            // 
            // dataGridView
            // 
            this.dataGridView.AllowUserToDragDropRows = false;
            this.dataGridView.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.dataGridView.AutoGenerateColumns = false;
            this.dataGridView.ColumnHeadersHeight = 17;
            this.dataGridView.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.nameColumn,
            this.pathColumn,
            this.searchButtonColumn,
            this.scaleColumn,
            this.errorMessageColumn});
            this.dataGridView.Location = new System.Drawing.Point(0, 0);
            this.dataGridView.Name = "dataGridView";
            this.dataGridView.RowHeadersWidth = 41;
            this.dataGridView.Size = new System.Drawing.Size(844, 330);
            this.dataGridView.TabIndex = 0;
            this.dataGridView.CellContentClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dataGridView_CellContentClick);
            this.dataGridView.CellFormatting += new System.Windows.Forms.DataGridViewCellFormattingEventHandler(this.dataGridView_CellFormatting);
            this.dataGridView.SelectionChanged += new System.EventHandler(this.dataGridView_SelectionChanged);
            this.dataGridView.UserDeletedRow += new System.Windows.Forms.DataGridViewRowEventHandler(this.dataGridView_UserDeletedRow);
            this.dataGridView.UserDeletingRow += new System.Windows.Forms.DataGridViewRowCancelEventHandler(this.dataGridView_UserDeletingRow);
            // 
            // nameColumn
            // 
            this.nameColumn.DataPropertyName = "Name";
            this.nameColumn.HeaderText = "Name";
            this.nameColumn.Name = "nameColumn";
            // 
            // pathColumn
            // 
            this.pathColumn.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.DisplayedCells;
            this.pathColumn.DataPropertyName = "Path";
            this.pathColumn.HeaderText = "Path";
            this.pathColumn.Name = "pathColumn";
            this.pathColumn.Width = 54;
            // 
            // searchButtonColumn
            // 
            this.searchButtonColumn.HeaderText = "Search";
            this.searchButtonColumn.Name = "searchButtonColumn";
            this.searchButtonColumn.Text = "Search";
            this.searchButtonColumn.Width = 50;
            // 
            // scaleColumn
            // 
            this.scaleColumn.DataPropertyName = "Scale";
            this.scaleColumn.HeaderText = "Scale";
            this.scaleColumn.Name = "scaleColumn";
            this.scaleColumn.Width = 50;
            // 
            // errorMessageColumn
            // 
            this.errorMessageColumn.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.DisplayedCells;
            this.errorMessageColumn.DataPropertyName = "ErrorMessage";
            this.errorMessageColumn.HeaderText = "Message";
            this.errorMessageColumn.Name = "errorMessageColumn";
            this.errorMessageColumn.Width = 76;
            // 
            // ImportedGeometryManager
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.dataGridViewControls);
            this.Controls.Add(this.dataGridView);
            this.Name = "ImportedGeometryManager";
            this.Size = new System.Drawing.Size(877, 330);
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion
        private DarkDataGridViewControls dataGridViewControls;
        private DarkUI.Controls.DarkDataGridView dataGridView;
        private System.Windows.Forms.DataGridViewTextBoxColumn nameColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn pathColumn;
        private DarkUI.Controls.DarkDataGridViewButtonColumn searchButtonColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn scaleColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn errorMessageColumn;
    }
}
