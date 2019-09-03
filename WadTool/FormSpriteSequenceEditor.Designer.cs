namespace WadTool
{
    partial class FormSpriteSequenceEditor
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.dataGridView = new DarkUI.Controls.DarkDataGridView();
            this.PreviewColumn = new System.Windows.Forms.DataGridViewImageColumn();
            this.SizeColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.IdColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.butReplaceSprite = new DarkUI.Controls.DarkButton();
            this.btExport = new DarkUI.Controls.DarkButton();
            this.dataGridViewControls = new TombLib.Controls.DarkDataGridViewControls();
            this.picSprite = new System.Windows.Forms.PictureBox();
            this.btOk = new DarkUI.Controls.DarkButton();
            this.btCancel = new DarkUI.Controls.DarkButton();
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.picSprite)).BeginInit();
            this.SuspendLayout();
            // 
            // dataGridView
            // 
            this.dataGridView.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.dataGridView.AutoGenerateColumns = false;
            this.dataGridView.ColumnHeadersHeight = 17;
            this.dataGridView.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.PreviewColumn,
            this.SizeColumn,
            this.IdColumn});
            this.dataGridView.Location = new System.Drawing.Point(393, 5);
            this.dataGridView.Name = "dataGridView";
            this.dataGridView.RowHeadersWidth = 41;
            this.dataGridView.RowTemplate.Height = 50;
            this.dataGridView.Size = new System.Drawing.Size(281, 408);
            this.dataGridView.TabIndex = 46;
            this.dataGridView.CellFormattingSafe += new DarkUI.Controls.DarkDataGridViewSafeCellFormattingEventHandler(this.dataGridView_CellFormattingSafe);
            this.dataGridView.SelectionChanged += new System.EventHandler(this.dataGridView_SelectionChanged);
            this.dataGridView.Click += new System.EventHandler(this.dataGridView_Click);
            // 
            // PreviewColumn
            // 
            this.PreviewColumn.HeaderText = "Preview";
            this.PreviewColumn.ImageLayout = System.Windows.Forms.DataGridViewImageCellLayout.Zoom;
            this.PreviewColumn.Name = "PreviewColumn";
            this.PreviewColumn.ReadOnly = true;
            this.PreviewColumn.Width = 80;
            // 
            // SizeColumn
            // 
            this.SizeColumn.HeaderText = "Size";
            this.SizeColumn.Name = "SizeColumn";
            this.SizeColumn.ReadOnly = true;
            this.SizeColumn.Width = 80;
            // 
            // IdColumn
            // 
            this.IdColumn.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.IdColumn.HeaderText = "ID";
            this.IdColumn.Name = "IdColumn";
            this.IdColumn.ReadOnly = true;
            // 
            // butReplaceSprite
            // 
            this.butReplaceSprite.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.butReplaceSprite.Image = global::WadTool.Properties.Resources.replace_16;
            this.butReplaceSprite.Location = new System.Drawing.Point(680, 96);
            this.butReplaceSprite.Name = "butReplaceSprite";
            this.butReplaceSprite.Size = new System.Drawing.Size(27, 25);
            this.butReplaceSprite.TabIndex = 50;
            this.toolTip1.SetToolTip(this.butReplaceSprite, "Replace sprite");
            this.butReplaceSprite.Click += new System.EventHandler(this.ButReplaceSprite_Click);
            // 
            // btExport
            // 
            this.btExport.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btExport.Enabled = false;
            this.btExport.Image = global::WadTool.Properties.Resources.general_Export_16;
            this.btExport.Location = new System.Drawing.Point(680, 65);
            this.btExport.Name = "btExport";
            this.btExport.Size = new System.Drawing.Size(27, 25);
            this.btExport.TabIndex = 49;
            this.toolTip1.SetToolTip(this.btExport, "Export to file");
            this.btExport.Click += new System.EventHandler(this.btExport_Click);
            // 
            // dataGridViewControls
            // 
            this.dataGridViewControls.AlwaysInsertAtZero = false;
            this.dataGridViewControls.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.dataGridViewControls.Enabled = false;
            this.dataGridViewControls.Location = new System.Drawing.Point(680, 5);
            this.dataGridViewControls.MinimumSize = new System.Drawing.Size(24, 24);
            this.dataGridViewControls.Name = "dataGridViewControls";
            this.dataGridViewControls.Size = new System.Drawing.Size(27, 408);
            this.dataGridViewControls.TabIndex = 47;
            // 
            // picSprite
            // 
            this.picSprite.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.picSprite.BackColor = System.Drawing.Color.Fuchsia;
            this.picSprite.BackgroundImage = global::WadTool.Properties.Resources.misc_TransparentBackground;
            this.picSprite.Location = new System.Drawing.Point(5, 5);
            this.picSprite.Name = "picSprite";
            this.picSprite.Padding = new System.Windows.Forms.Padding(3);
            this.picSprite.Size = new System.Drawing.Size(384, 408);
            this.picSprite.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.picSprite.TabIndex = 44;
            this.picSprite.TabStop = false;
            // 
            // btOk
            // 
            this.btOk.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btOk.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.btOk.Location = new System.Drawing.Point(539, 419);
            this.btOk.Name = "btOk";
            this.btOk.Size = new System.Drawing.Size(81, 23);
            this.btOk.TabIndex = 49;
            this.btOk.Text = "OK";
            this.btOk.Click += new System.EventHandler(this.btOk_Click);
            // 
            // btCancel
            // 
            this.btCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btCancel.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.btCancel.Location = new System.Drawing.Point(626, 419);
            this.btCancel.Name = "btCancel";
            this.btCancel.Size = new System.Drawing.Size(81, 23);
            this.btCancel.TabIndex = 49;
            this.btCancel.Text = "Cancel";
            this.btCancel.Click += new System.EventHandler(this.btCancel_Click);
            // 
            // FormSpriteSequenceEditor
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(712, 447);
            this.Controls.Add(this.dataGridView);
            this.Controls.Add(this.butReplaceSprite);
            this.Controls.Add(this.picSprite);
            this.Controls.Add(this.btExport);
            this.Controls.Add(this.btCancel);
            this.Controls.Add(this.dataGridViewControls);
            this.Controls.Add(this.btOk);
            this.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(554, 390);
            this.Name = "FormSpriteSequenceEditor";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Sprite editor";
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.picSprite)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.PictureBox picSprite;
        private TombLib.Controls.DarkDataGridViewControls dataGridViewControls;
        private DarkUI.Controls.DarkButton btOk;
        private DarkUI.Controls.DarkButton btCancel;
        private DarkUI.Controls.DarkButton btExport;
        private System.Windows.Forms.ToolTip toolTip1;
        private DarkUI.Controls.DarkDataGridView dataGridView;
        private DarkUI.Controls.DarkButton butReplaceSprite;
        private System.Windows.Forms.DataGridViewImageColumn PreviewColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn SizeColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn IdColumn;
    }
}