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
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.panel2 = new System.Windows.Forms.Panel();
            this.btExport = new DarkUI.Controls.DarkButton();
            this.panel1 = new System.Windows.Forms.Panel();
            this.picSprite = new System.Windows.Forms.PictureBox();
            this.btOk = new DarkUI.Controls.DarkButton();
            this.btCancel = new DarkUI.Controls.DarkButton();
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.butReplaceSprite = new DarkUI.Controls.DarkButton();
            this.dataGridViewControls = new TombLib.Controls.DarkDataGridViewControls();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView)).BeginInit();
            this.tableLayoutPanel1.SuspendLayout();
            this.panel2.SuspendLayout();
            this.panel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.picSprite)).BeginInit();
            this.SuspendLayout();
            // 
            // dataGridView
            // 
            this.dataGridView.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.dataGridView.AutoGenerateColumns = false;
            this.dataGridView.ColumnHeadersHeight = 17;
            this.dataGridView.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.PreviewColumn,
            this.SizeColumn,
            this.IdColumn});
            this.dataGridView.Location = new System.Drawing.Point(5, 14);
            this.dataGridView.Name = "dataGridView";
            this.dataGridView.RowHeadersWidth = 41;
            this.dataGridView.RowTemplate.Height = 50;
            this.dataGridView.Size = new System.Drawing.Size(297, 486);
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
            this.IdColumn.HeaderText = "Id";
            this.IdColumn.Name = "IdColumn";
            this.IdColumn.ReadOnly = true;
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tableLayoutPanel1.ColumnCount = 2;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.Controls.Add(this.panel2, 1, 0);
            this.tableLayoutPanel1.Controls.Add(this.panel1, 0, 0);
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, -2);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 1;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(693, 506);
            this.tableLayoutPanel1.TabIndex = 48;
            // 
            // panel2
            // 
            this.panel2.Controls.Add(this.butReplaceSprite);
            this.panel2.Controls.Add(this.btExport);
            this.panel2.Controls.Add(this.dataGridViewControls);
            this.panel2.Controls.Add(this.dataGridView);
            this.panel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel2.Location = new System.Drawing.Point(346, 0);
            this.panel2.Margin = new System.Windows.Forms.Padding(0);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(347, 506);
            this.panel2.TabIndex = 1;
            // 
            // btExport
            // 
            this.btExport.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btExport.Enabled = false;
            this.btExport.Image = global::WadTool.Properties.Resources.general_Export_16;
            this.btExport.Location = new System.Drawing.Point(309, 74);
            this.btExport.Name = "btExport";
            this.btExport.Size = new System.Drawing.Size(27, 25);
            this.btExport.TabIndex = 49;
            this.toolTip1.SetToolTip(this.btExport, "Export to file.");
            this.btExport.Click += new System.EventHandler(this.btExport_Click);
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.picSprite);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Margin = new System.Windows.Forms.Padding(0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(346, 506);
            this.panel1.TabIndex = 0;
            // 
            // picSprite
            // 
            this.picSprite.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.picSprite.BackColor = System.Drawing.Color.Fuchsia;
            this.picSprite.BackgroundImage = global::WadTool.Properties.Resources.misc_TransparentBackground;
            this.picSprite.Location = new System.Drawing.Point(12, 14);
            this.picSprite.Name = "picSprite";
            this.picSprite.Size = new System.Drawing.Size(329, 486);
            this.picSprite.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.picSprite.TabIndex = 44;
            this.picSprite.TabStop = false;
            // 
            // btOk
            // 
            this.btOk.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btOk.Location = new System.Drawing.Point(568, 510);
            this.btOk.Name = "btOk";
            this.btOk.Size = new System.Drawing.Size(113, 26);
            this.btOk.TabIndex = 49;
            this.btOk.Text = "Ok";
            this.btOk.Click += new System.EventHandler(this.btOk_Click);
            // 
            // btCancel
            // 
            this.btCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btCancel.Location = new System.Drawing.Point(449, 510);
            this.btCancel.Name = "btCancel";
            this.btCancel.Size = new System.Drawing.Size(113, 26);
            this.btCancel.TabIndex = 49;
            this.btCancel.Text = "Cancel";
            this.btCancel.Click += new System.EventHandler(this.btCancel_Click);
            // 
            // butReplaceSprite
            // 
            this.butReplaceSprite.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.butReplaceSprite.Image = global::WadTool.Properties.Resources.replace_16;
            this.butReplaceSprite.Location = new System.Drawing.Point(309, 105);
            this.butReplaceSprite.Name = "butReplaceSprite";
            this.butReplaceSprite.Size = new System.Drawing.Size(27, 25);
            this.butReplaceSprite.TabIndex = 50;
            this.toolTip1.SetToolTip(this.butReplaceSprite, "Replace sprite");
            this.butReplaceSprite.Click += new System.EventHandler(this.ButReplaceSprite_Click);
            // 
            // dataGridViewControls
            // 
            this.dataGridViewControls.AlwaysInsertAtZero = false;
            this.dataGridViewControls.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.dataGridViewControls.Enabled = false;
            this.dataGridViewControls.Location = new System.Drawing.Point(309, 14);
            this.dataGridViewControls.MinimumSize = new System.Drawing.Size(24, 24);
            this.dataGridViewControls.Name = "dataGridViewControls";
            this.dataGridViewControls.Size = new System.Drawing.Size(27, 486);
            this.dataGridViewControls.TabIndex = 47;
            // 
            // FormSpriteSequenceEditor
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(693, 542);
            this.Controls.Add(this.btCancel);
            this.Controls.Add(this.btOk);
            this.Controls.Add(this.tableLayoutPanel1);
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(554, 390);
            this.Name = "FormSpriteSequenceEditor";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Sprite editor";
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView)).EndInit();
            this.tableLayoutPanel1.ResumeLayout(false);
            this.panel2.ResumeLayout(false);
            this.panel1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.picSprite)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.PictureBox picSprite;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.Panel panel1;
        private TombLib.Controls.DarkDataGridViewControls dataGridViewControls;
        private DarkUI.Controls.DarkButton btOk;
        private DarkUI.Controls.DarkButton btCancel;
        private DarkUI.Controls.DarkButton btExport;
        private System.Windows.Forms.ToolTip toolTip1;
        private DarkUI.Controls.DarkDataGridView dataGridView;
        private System.Windows.Forms.DataGridViewImageColumn PreviewColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn SizeColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn IdColumn;
        private DarkUI.Controls.DarkButton butReplaceSprite;
    }
}