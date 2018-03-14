namespace WadTool
{
    partial class FormMesh
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
            this.lstMeshes = new DarkUI.Controls.DarkDataGridView();
            this.columnName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.panelMesh = new WadTool.Controls.PanelRenderingMesh();
            ((System.ComponentModel.ISupportInitialize)(this.lstMeshes)).BeginInit();
            this.SuspendLayout();
            // 
            // lstMeshes
            // 
            this.lstMeshes.ColumnHeadersHeight = 17;
            this.lstMeshes.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.columnName});
            this.lstMeshes.Location = new System.Drawing.Point(12, 12);
            this.lstMeshes.Name = "lstMeshes";
            this.lstMeshes.RowHeadersWidth = 41;
            this.lstMeshes.Size = new System.Drawing.Size(262, 551);
            this.lstMeshes.TabIndex = 1;
            this.lstMeshes.Click += new System.EventHandler(this.lstMeshes_Click);
            // 
            // columnName
            // 
            this.columnName.HeaderText = "Name";
            this.columnName.Name = "columnName";
            this.columnName.Width = 200;
            // 
            // panelMesh
            // 
            this.panelMesh.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panelMesh.Location = new System.Drawing.Point(280, 12);
            this.panelMesh.Name = "panelMesh";
            this.panelMesh.Size = new System.Drawing.Size(532, 551);
            this.panelMesh.TabIndex = 0;
            // 
            // FormMesh
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(824, 575);
            this.Controls.Add(this.lstMeshes);
            this.Controls.Add(this.panelMesh);
            this.Name = "FormMesh";
            this.StartPosition = System.Windows.Forms.FormStartPosition.WindowsDefaultBounds;
            this.Text = "FormMesh";
            ((System.ComponentModel.ISupportInitialize)(this.lstMeshes)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private Controls.PanelRenderingMesh panelMesh;
        private DarkUI.Controls.DarkDataGridView lstMeshes;
        private System.Windows.Forms.DataGridViewTextBoxColumn columnName;
    }
}