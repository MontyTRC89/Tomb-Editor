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
            this.picSprite = new System.Windows.Forms.PictureBox();
            this.btOk = new DarkUI.Controls.DarkButton();
            this.btCancel = new DarkUI.Controls.DarkButton();
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.butRecalc = new DarkUI.Controls.DarkButton();
            this.darkGroupBox1 = new DarkUI.Controls.DarkGroupBox();
            this.cmbHorAdj = new DarkUI.Controls.DarkComboBox();
            this.darkLabel4 = new DarkUI.Controls.DarkLabel();
            this.darkLabel7 = new DarkUI.Controls.DarkLabel();
            this.nudB = new DarkUI.Controls.DarkNumericUpDown();
            this.cmbVerAdj = new DarkUI.Controls.DarkComboBox();
            this.darkLabel3 = new DarkUI.Controls.DarkLabel();
            this.darkLabel6 = new DarkUI.Controls.DarkLabel();
            this.nudR = new DarkUI.Controls.DarkNumericUpDown();
            this.nudScale = new DarkUI.Controls.DarkNumericUpDown();
            this.darkLabel2 = new DarkUI.Controls.DarkLabel();
            this.darkLabel5 = new DarkUI.Controls.DarkLabel();
            this.nudT = new DarkUI.Controls.DarkNumericUpDown();
            this.darkLabel1 = new DarkUI.Controls.DarkLabel();
            this.nudL = new DarkUI.Controls.DarkNumericUpDown();
            this.dataGridViewControls = new TombLib.Controls.DarkDataGridViewControls();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.picSprite)).BeginInit();
            this.darkGroupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudB)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudR)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudScale)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudT)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudL)).BeginInit();
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
            this.dataGridView.Location = new System.Drawing.Point(605, 5);
            this.dataGridView.Name = "dataGridView";
            this.dataGridView.RowHeadersWidth = 41;
            this.dataGridView.RowTemplate.Height = 50;
            this.dataGridView.Size = new System.Drawing.Size(281, 455);
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
            this.butReplaceSprite.Checked = false;
            this.butReplaceSprite.Image = global::WadTool.Properties.Resources.replace_16;
            this.butReplaceSprite.Location = new System.Drawing.Point(892, 96);
            this.butReplaceSprite.Name = "butReplaceSprite";
            this.butReplaceSprite.Size = new System.Drawing.Size(27, 25);
            this.butReplaceSprite.TabIndex = 50;
            this.toolTip1.SetToolTip(this.butReplaceSprite, "Replace sprite");
            this.butReplaceSprite.Click += new System.EventHandler(this.ButReplaceSprite_Click);
            // 
            // btExport
            // 
            this.btExport.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btExport.Checked = false;
            this.btExport.Enabled = false;
            this.btExport.Image = global::WadTool.Properties.Resources.general_Export_16;
            this.btExport.Location = new System.Drawing.Point(892, 65);
            this.btExport.Name = "btExport";
            this.btExport.Size = new System.Drawing.Size(27, 25);
            this.btExport.TabIndex = 49;
            this.toolTip1.SetToolTip(this.btExport, "Export to file");
            this.btExport.Click += new System.EventHandler(this.btExport_Click);
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
            this.picSprite.Size = new System.Drawing.Size(596, 558);
            this.picSprite.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.picSprite.TabIndex = 44;
            this.picSprite.TabStop = false;
            // 
            // btOk
            // 
            this.btOk.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btOk.Checked = false;
            this.btOk.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.btOk.Location = new System.Drawing.Point(751, 569);
            this.btOk.Name = "btOk";
            this.btOk.Size = new System.Drawing.Size(81, 23);
            this.btOk.TabIndex = 49;
            this.btOk.Text = "OK";
            this.btOk.Click += new System.EventHandler(this.btOk_Click);
            // 
            // btCancel
            // 
            this.btCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btCancel.Checked = false;
            this.btCancel.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.btCancel.Location = new System.Drawing.Point(838, 569);
            this.btCancel.Name = "btCancel";
            this.btCancel.Size = new System.Drawing.Size(81, 23);
            this.btCancel.TabIndex = 49;
            this.btCancel.Text = "Cancel";
            this.btCancel.Click += new System.EventHandler(this.btCancel_Click);
            // 
            // butRecalc
            // 
            this.butRecalc.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.butRecalc.Checked = false;
            this.butRecalc.Image = global::WadTool.Properties.Resources.replace_16;
            this.butRecalc.Location = new System.Drawing.Point(282, 66);
            this.butRecalc.Name = "butRecalc";
            this.butRecalc.Size = new System.Drawing.Size(23, 23);
            this.butRecalc.TabIndex = 51;
            this.toolTip1.SetToolTip(this.butRecalc, "Replace sprite");
            this.butRecalc.Click += new System.EventHandler(this.butRecalc_Click);
            // 
            // darkGroupBox1
            // 
            this.darkGroupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.darkGroupBox1.Controls.Add(this.butRecalc);
            this.darkGroupBox1.Controls.Add(this.cmbHorAdj);
            this.darkGroupBox1.Controls.Add(this.darkLabel4);
            this.darkGroupBox1.Controls.Add(this.darkLabel7);
            this.darkGroupBox1.Controls.Add(this.nudB);
            this.darkGroupBox1.Controls.Add(this.cmbVerAdj);
            this.darkGroupBox1.Controls.Add(this.darkLabel3);
            this.darkGroupBox1.Controls.Add(this.darkLabel6);
            this.darkGroupBox1.Controls.Add(this.nudR);
            this.darkGroupBox1.Controls.Add(this.nudScale);
            this.darkGroupBox1.Controls.Add(this.darkLabel2);
            this.darkGroupBox1.Controls.Add(this.darkLabel5);
            this.darkGroupBox1.Controls.Add(this.nudT);
            this.darkGroupBox1.Controls.Add(this.darkLabel1);
            this.darkGroupBox1.Controls.Add(this.nudL);
            this.darkGroupBox1.Location = new System.Drawing.Point(607, 466);
            this.darkGroupBox1.Name = "darkGroupBox1";
            this.darkGroupBox1.Size = new System.Drawing.Size(312, 97);
            this.darkGroupBox1.TabIndex = 51;
            this.darkGroupBox1.TabStop = false;
            this.darkGroupBox1.Text = "Alignment (TR1-3 only)";
            // 
            // cmbHorAdj
            // 
            this.cmbHorAdj.FormattingEnabled = true;
            this.cmbHorAdj.Items.AddRange(new object[] {
            "Left",
            "Center",
            "Right"});
            this.cmbHorAdj.Location = new System.Drawing.Point(184, 66);
            this.cmbHorAdj.Name = "cmbHorAdj";
            this.cmbHorAdj.Size = new System.Drawing.Size(92, 23);
            this.cmbHorAdj.TabIndex = 13;
            // 
            // darkLabel4
            // 
            this.darkLabel4.AutoSize = true;
            this.darkLabel4.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.darkLabel4.Location = new System.Drawing.Point(237, 24);
            this.darkLabel4.Name = "darkLabel4";
            this.darkLabel4.Size = new System.Drawing.Size(14, 13);
            this.darkLabel4.TabIndex = 10;
            this.darkLabel4.Text = "B";
            // 
            // darkLabel7
            // 
            this.darkLabel7.AutoSize = true;
            this.darkLabel7.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.darkLabel7.Location = new System.Drawing.Point(181, 50);
            this.darkLabel7.Name = "darkLabel7";
            this.darkLabel7.Size = new System.Drawing.Size(80, 13);
            this.darkLabel7.TabIndex = 16;
            this.darkLabel7.Text = "Horizontal adj";
            // 
            // nudB
            // 
            this.nudB.IncrementAlternate = new decimal(new int[] {
            10,
            0,
            0,
            65536});
            this.nudB.Location = new System.Drawing.Point(250, 22);
            this.nudB.LoopValues = false;
            this.nudB.Maximum = new decimal(new int[] {
            32767,
            0,
            0,
            0});
            this.nudB.Minimum = new decimal(new int[] {
            32767,
            0,
            0,
            -2147483648});
            this.nudB.Name = "nudB";
            this.nudB.Size = new System.Drawing.Size(55, 22);
            this.nudB.TabIndex = 9;
            this.nudB.ValueChanged += new System.EventHandler(this.nudAlignment_ValueChanged);
            // 
            // cmbVerAdj
            // 
            this.cmbVerAdj.FormattingEnabled = true;
            this.cmbVerAdj.Items.AddRange(new object[] {
            "Top",
            "Center",
            "Bottom"});
            this.cmbVerAdj.Location = new System.Drawing.Point(86, 66);
            this.cmbVerAdj.Name = "cmbVerAdj";
            this.cmbVerAdj.Size = new System.Drawing.Size(92, 23);
            this.cmbVerAdj.TabIndex = 11;
            // 
            // darkLabel3
            // 
            this.darkLabel3.AutoSize = true;
            this.darkLabel3.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.darkLabel3.Location = new System.Drawing.Point(160, 24);
            this.darkLabel3.Name = "darkLabel3";
            this.darkLabel3.Size = new System.Drawing.Size(14, 13);
            this.darkLabel3.TabIndex = 8;
            this.darkLabel3.Text = "R";
            // 
            // darkLabel6
            // 
            this.darkLabel6.AutoSize = true;
            this.darkLabel6.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.darkLabel6.Location = new System.Drawing.Point(83, 51);
            this.darkLabel6.Name = "darkLabel6";
            this.darkLabel6.Size = new System.Drawing.Size(63, 13);
            this.darkLabel6.TabIndex = 15;
            this.darkLabel6.Text = "Vertical adj";
            // 
            // nudR
            // 
            this.nudR.IncrementAlternate = new decimal(new int[] {
            10,
            0,
            0,
            65536});
            this.nudR.Location = new System.Drawing.Point(173, 22);
            this.nudR.LoopValues = false;
            this.nudR.Maximum = new decimal(new int[] {
            32767,
            0,
            0,
            0});
            this.nudR.Minimum = new decimal(new int[] {
            32767,
            0,
            0,
            -2147483648});
            this.nudR.Name = "nudR";
            this.nudR.Size = new System.Drawing.Size(55, 22);
            this.nudR.TabIndex = 7;
            this.nudR.ValueChanged += new System.EventHandler(this.nudAlignment_ValueChanged);
            // 
            // nudScale
            // 
            this.nudScale.DecimalPlaces = 1;
            this.nudScale.IncrementAlternate = new decimal(new int[] {
            10,
            0,
            0,
            65536});
            this.nudScale.Location = new System.Drawing.Point(10, 67);
            this.nudScale.LoopValues = false;
            this.nudScale.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            65536});
            this.nudScale.Name = "nudScale";
            this.nudScale.Size = new System.Drawing.Size(70, 22);
            this.nudScale.TabIndex = 12;
            this.nudScale.Value = new decimal(new int[] {
            1,
            0,
            0,
            65536});
            // 
            // darkLabel2
            // 
            this.darkLabel2.AutoSize = true;
            this.darkLabel2.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.darkLabel2.Location = new System.Drawing.Point(83, 24);
            this.darkLabel2.Name = "darkLabel2";
            this.darkLabel2.Size = new System.Drawing.Size(12, 13);
            this.darkLabel2.TabIndex = 6;
            this.darkLabel2.Text = "T";
            // 
            // darkLabel5
            // 
            this.darkLabel5.AutoSize = true;
            this.darkLabel5.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.darkLabel5.Location = new System.Drawing.Point(7, 51);
            this.darkLabel5.Name = "darkLabel5";
            this.darkLabel5.Size = new System.Drawing.Size(33, 13);
            this.darkLabel5.TabIndex = 14;
            this.darkLabel5.Text = "Scale";
            // 
            // nudT
            // 
            this.nudT.IncrementAlternate = new decimal(new int[] {
            10,
            0,
            0,
            65536});
            this.nudT.Location = new System.Drawing.Point(96, 22);
            this.nudT.LoopValues = false;
            this.nudT.Maximum = new decimal(new int[] {
            32767,
            0,
            0,
            0});
            this.nudT.Minimum = new decimal(new int[] {
            32767,
            0,
            0,
            -2147483648});
            this.nudT.Name = "nudT";
            this.nudT.Size = new System.Drawing.Size(55, 22);
            this.nudT.TabIndex = 5;
            this.nudT.ValueChanged += new System.EventHandler(this.nudAlignment_ValueChanged);
            // 
            // darkLabel1
            // 
            this.darkLabel1.AutoSize = true;
            this.darkLabel1.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.darkLabel1.Location = new System.Drawing.Point(7, 24);
            this.darkLabel1.Name = "darkLabel1";
            this.darkLabel1.Size = new System.Drawing.Size(12, 13);
            this.darkLabel1.TabIndex = 4;
            this.darkLabel1.Text = "L";
            // 
            // nudL
            // 
            this.nudL.IncrementAlternate = new decimal(new int[] {
            10,
            0,
            0,
            65536});
            this.nudL.Location = new System.Drawing.Point(20, 22);
            this.nudL.LoopValues = false;
            this.nudL.Maximum = new decimal(new int[] {
            32767,
            0,
            0,
            0});
            this.nudL.Minimum = new decimal(new int[] {
            32767,
            0,
            0,
            -2147483648});
            this.nudL.Name = "nudL";
            this.nudL.Size = new System.Drawing.Size(55, 22);
            this.nudL.TabIndex = 3;
            this.nudL.ValueChanged += new System.EventHandler(this.nudAlignment_ValueChanged);
            // 
            // dataGridViewControls
            // 
            this.dataGridViewControls.AlwaysInsertAtZero = false;
            this.dataGridViewControls.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.dataGridViewControls.Enabled = false;
            this.dataGridViewControls.Location = new System.Drawing.Point(892, 5);
            this.dataGridViewControls.MinimumSize = new System.Drawing.Size(24, 24);
            this.dataGridViewControls.Name = "dataGridViewControls";
            this.dataGridViewControls.Size = new System.Drawing.Size(27, 455);
            this.dataGridViewControls.TabIndex = 47;
            // 
            // FormSpriteSequenceEditor
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(924, 597);
            this.Controls.Add(this.darkGroupBox1);
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
            this.darkGroupBox1.ResumeLayout(false);
            this.darkGroupBox1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudB)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudR)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudScale)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudT)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudL)).EndInit();
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
        private DarkUI.Controls.DarkGroupBox darkGroupBox1;
        private DarkUI.Controls.DarkLabel darkLabel1;
        private DarkUI.Controls.DarkNumericUpDown nudL;
        private DarkUI.Controls.DarkLabel darkLabel4;
        private DarkUI.Controls.DarkNumericUpDown nudB;
        private DarkUI.Controls.DarkLabel darkLabel3;
        private DarkUI.Controls.DarkNumericUpDown nudR;
        private DarkUI.Controls.DarkLabel darkLabel2;
        private DarkUI.Controls.DarkNumericUpDown nudT;
        private DarkUI.Controls.DarkButton butRecalc;
        private DarkUI.Controls.DarkComboBox cmbHorAdj;
        private DarkUI.Controls.DarkLabel darkLabel7;
        private DarkUI.Controls.DarkComboBox cmbVerAdj;
        private DarkUI.Controls.DarkLabel darkLabel6;
        private DarkUI.Controls.DarkNumericUpDown nudScale;
        private DarkUI.Controls.DarkLabel darkLabel5;
    }
}