﻿namespace TombEditor.Controls
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
            this.dataGridView = new DarkUI.Controls.DarkDataGridView();
            this.dataGridViewControls = new TombLib.Controls.DarkDataGridViewControls();
            this.nameColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.pathColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.searchButtonColumn = new DarkUI.Controls.DarkDataGridViewButtonColumn();
            this.scaleColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.errorMessageColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.swapXYColumn = new DarkUI.Controls.DarkDataGridViewCheckBoxColumn();
            this.swapXZColumn = new DarkUI.Controls.DarkDataGridViewCheckBoxColumn();
            this.swapYZColumn = new DarkUI.Controls.DarkDataGridViewCheckBoxColumn();
            this.flipXColumn = new DarkUI.Controls.DarkDataGridViewCheckBoxColumn();
            this.flipYColumn = new DarkUI.Controls.DarkDataGridViewCheckBoxColumn();
            this.flipZColumn = new DarkUI.Controls.DarkDataGridViewCheckBoxColumn();
            this.flipUV_Vcolumn = new DarkUI.Controls.DarkDataGridViewCheckBoxColumn();
            this.invertFacesColumn = new DarkUI.Controls.DarkDataGridViewCheckBoxColumn();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView)).BeginInit();
            this.SuspendLayout();
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
            this.errorMessageColumn,
            this.swapXYColumn,
            this.swapXZColumn,
            this.swapYZColumn,
            this.flipXColumn,
            this.flipYColumn,
            this.flipZColumn,
            this.flipUV_Vcolumn,
            this.invertFacesColumn});
            this.dataGridView.Location = new System.Drawing.Point(0, 0);
            this.dataGridView.Name = "dataGridView";
            this.dataGridView.RowHeadersWidth = 41;
            this.dataGridView.Size = new System.Drawing.Size(844, 330);
            this.dataGridView.TabIndex = 0;
            this.dataGridView.CellContentClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dataGridView_CellContentClick);
            this.dataGridView.CellFormatting += new System.Windows.Forms.DataGridViewCellFormattingEventHandler(this.dataGridView_CellFormatting);
            this.dataGridView.CellMouseDoubleClick += new System.Windows.Forms.DataGridViewCellMouseEventHandler(this.dataGridView_CellMouseDoubleClick);
            this.dataGridView.DataError += new System.Windows.Forms.DataGridViewDataErrorEventHandler(this.dataGridView_DataError);
            this.dataGridView.SelectionChanged += new System.EventHandler(this.dataGridView_SelectionChanged);
            this.dataGridView.UserDeletedRow += new System.Windows.Forms.DataGridViewRowEventHandler(this.dataGridView_UserDeletedRow);
            this.dataGridView.UserDeletingRow += new System.Windows.Forms.DataGridViewRowCancelEventHandler(this.dataGridView_UserDeletingRow);
            // 
            // dataGridViewControls
            // 
            this.dataGridViewControls.AllowUserMove = false;
            this.dataGridViewControls.AlwaysInsertAtZero = false;
            this.dataGridViewControls.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.dataGridViewControls.Enabled = false;
            this.dataGridViewControls.Location = new System.Drawing.Point(850, 0);
            this.dataGridViewControls.MinimumSize = new System.Drawing.Size(24, 100);
            this.dataGridViewControls.Name = "dataGridViewControls";
            this.dataGridViewControls.Size = new System.Drawing.Size(24, 330);
            this.dataGridViewControls.TabIndex = 1;
            // 
            // nameColumn
            // 
            this.nameColumn.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.nameColumn.DataPropertyName = "Name";
            this.nameColumn.FillWeight = 50F;
            this.nameColumn.HeaderText = "Name";
            this.nameColumn.Name = "nameColumn";
            // 
            // pathColumn
            // 
            this.pathColumn.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.pathColumn.DataPropertyName = "Path";
            this.pathColumn.FillWeight = 50F;
            this.pathColumn.HeaderText = "Path";
            this.pathColumn.Name = "pathColumn";
            this.pathColumn.ReadOnly = true;
            // 
            // searchButtonColumn
            // 
            this.searchButtonColumn.HeaderText = "Browse";
            this.searchButtonColumn.Name = "searchButtonColumn";
            this.searchButtonColumn.Text = "Browse";
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
            // swapXYColumn
            // 
            this.swapXYColumn.DataPropertyName = "SwapXY";
            this.swapXYColumn.HeaderText = "X↔Y";
            this.swapXYColumn.Name = "swapXYColumn";
            this.swapXYColumn.Resizable = System.Windows.Forms.DataGridViewTriState.False;
            this.swapXYColumn.ToolTipText = "Swap X-Y axis";
            this.swapXYColumn.Width = 34;
            // 
            // swapXZColumn
            // 
            this.swapXZColumn.DataPropertyName = "SwapXZ";
            this.swapXZColumn.HeaderText = "X↔Z";
            this.swapXZColumn.Name = "swapXZColumn";
            this.swapXZColumn.Resizable = System.Windows.Forms.DataGridViewTriState.False;
            this.swapXZColumn.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Automatic;
            this.swapXZColumn.ToolTipText = "Swap X-Z axis";
            this.swapXZColumn.Width = 34;
            // 
            // swapYZColumn
            // 
            this.swapYZColumn.DataPropertyName = "SwapYZ";
            this.swapYZColumn.HeaderText = "Y↔Z";
            this.swapYZColumn.Name = "swapYZColumn";
            this.swapYZColumn.Resizable = System.Windows.Forms.DataGridViewTriState.False;
            this.swapYZColumn.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Automatic;
            this.swapYZColumn.ToolTipText = "Swap Y-Z axis";
            this.swapYZColumn.Width = 34;
            // 
            // flipXColumn
            // 
            this.flipXColumn.DataPropertyName = "FlipX";
            this.flipXColumn.HeaderText = "-X";
            this.flipXColumn.Name = "flipXColumn";
            this.flipXColumn.Resizable = System.Windows.Forms.DataGridViewTriState.False;
            this.flipXColumn.ToolTipText = "Invert X axis";
            this.flipXColumn.Width = 22;
            // 
            // flipYColumn
            // 
            this.flipYColumn.DataPropertyName = "FlipY";
            this.flipYColumn.HeaderText = "-Y";
            this.flipYColumn.Name = "flipYColumn";
            this.flipYColumn.Resizable = System.Windows.Forms.DataGridViewTriState.False;
            this.flipYColumn.ToolTipText = "Invert Y axis";
            this.flipYColumn.Width = 22;
            // 
            // flipZColumn
            // 
            this.flipZColumn.DataPropertyName = "FlipZ";
            this.flipZColumn.HeaderText = "-Z";
            this.flipZColumn.Name = "flipZColumn";
            this.flipZColumn.Resizable = System.Windows.Forms.DataGridViewTriState.False;
            this.flipZColumn.ToolTipText = "Invert Z axis";
            this.flipZColumn.Width = 22;
            // 
            // flipUV_Vcolumn
            // 
            this.flipUV_Vcolumn.DataPropertyName = "FlipUV_V";
            this.flipUV_Vcolumn.HeaderText = "UV: -V";
            this.flipUV_Vcolumn.Name = "flipUV_Vcolumn";
            this.flipUV_Vcolumn.Resizable = System.Windows.Forms.DataGridViewTriState.False;
            this.flipUV_Vcolumn.ToolTipText = "Invert V texture coordinate";
            this.flipUV_Vcolumn.Width = 44;
            // 
            // invertFacesColumn
            // 
            this.invertFacesColumn.DataPropertyName = "InvertFaces";
            this.invertFacesColumn.HeaderText = "Inv. F.";
            this.invertFacesColumn.Name = "invertFacesColumn";
            this.invertFacesColumn.Resizable = System.Windows.Forms.DataGridViewTriState.True;
            this.invertFacesColumn.ToolTipText = "Invert faces";
            this.invertFacesColumn.Width = 44;
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
        private TombLib.Controls.DarkDataGridViewControls dataGridViewControls;
        private DarkUI.Controls.DarkDataGridView dataGridView;
        private System.Windows.Forms.DataGridViewTextBoxColumn nameColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn pathColumn;
        private DarkUI.Controls.DarkDataGridViewButtonColumn searchButtonColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn scaleColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn errorMessageColumn;
        private DarkUI.Controls.DarkDataGridViewCheckBoxColumn swapXYColumn;
        private DarkUI.Controls.DarkDataGridViewCheckBoxColumn swapXZColumn;
        private DarkUI.Controls.DarkDataGridViewCheckBoxColumn swapYZColumn;
        private DarkUI.Controls.DarkDataGridViewCheckBoxColumn flipXColumn;
        private DarkUI.Controls.DarkDataGridViewCheckBoxColumn flipYColumn;
        private DarkUI.Controls.DarkDataGridViewCheckBoxColumn flipZColumn;
        private DarkUI.Controls.DarkDataGridViewCheckBoxColumn flipUV_Vcolumn;
        private DarkUI.Controls.DarkDataGridViewCheckBoxColumn invertFacesColumn;
    }
}
