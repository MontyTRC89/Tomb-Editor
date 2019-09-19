namespace WadTool
{
    partial class FormReplaceAnimCommands
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
            this.darkGroupBox1 = new DarkUI.Controls.DarkGroupBox();
            this.aceFind = new WadTool.AnimCommandEditor();
            this.darkGroupBox2 = new DarkUI.Controls.DarkGroupBox();
            this.aceReplace = new WadTool.AnimCommandEditor();
            this.dgvResults = new DarkUI.Controls.DarkDataGridView();
            this.colReplaceFlag = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this.colAnimNumber = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colDescription = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.butFind = new DarkUI.Controls.DarkButton();
            this.butReplace = new DarkUI.Controls.DarkButton();
            this.butOK = new DarkUI.Controls.DarkButton();
            this.butSelectAll = new DarkUI.Controls.DarkButton();
            this.butDeselectAll = new DarkUI.Controls.DarkButton();
            this.butDelete = new DarkUI.Controls.DarkButton();
            this.statusLabel = new DarkUI.Controls.DarkLabel();
            this.darkGroupBox1.SuspendLayout();
            this.darkGroupBox2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvResults)).BeginInit();
            this.SuspendLayout();
            // 
            // darkGroupBox1
            // 
            this.darkGroupBox1.Controls.Add(this.aceFind);
            this.darkGroupBox1.Location = new System.Drawing.Point(12, 12);
            this.darkGroupBox1.Name = "darkGroupBox1";
            this.darkGroupBox1.Size = new System.Drawing.Size(383, 161);
            this.darkGroupBox1.TabIndex = 5;
            this.darkGroupBox1.TabStop = false;
            this.darkGroupBox1.Text = "Search for:";
            // 
            // aceFind
            // 
            this.aceFind.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.aceFind.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.aceFind.Location = new System.Drawing.Point(6, 21);
            this.aceFind.Name = "aceFind";
            this.aceFind.Size = new System.Drawing.Size(370, 134);
            this.aceFind.TabIndex = 2;
            // 
            // darkGroupBox2
            // 
            this.darkGroupBox2.Controls.Add(this.aceReplace);
            this.darkGroupBox2.Location = new System.Drawing.Point(401, 12);
            this.darkGroupBox2.Name = "darkGroupBox2";
            this.darkGroupBox2.Size = new System.Drawing.Size(383, 161);
            this.darkGroupBox2.TabIndex = 6;
            this.darkGroupBox2.TabStop = false;
            this.darkGroupBox2.Text = "Replace with:";
            // 
            // aceReplace
            // 
            this.aceReplace.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.aceReplace.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.aceReplace.Location = new System.Drawing.Point(6, 21);
            this.aceReplace.Name = "aceReplace";
            this.aceReplace.Size = new System.Drawing.Size(370, 134);
            this.aceReplace.TabIndex = 2;
            // 
            // dgvResults
            // 
            this.dgvResults.AllowUserToAddRows = false;
            this.dgvResults.AllowUserToDeleteRows = false;
            this.dgvResults.AllowUserToDragDropRows = false;
            this.dgvResults.AllowUserToPasteCells = false;
            this.dgvResults.AllowUserToResizeColumns = false;
            this.dgvResults.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.dgvResults.ColumnHeadersHeight = 17;
            this.dgvResults.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.colReplaceFlag,
            this.colAnimNumber,
            this.colDescription});
            this.dgvResults.Location = new System.Drawing.Point(12, 213);
            this.dgvResults.MultiSelect = false;
            this.dgvResults.Name = "dgvResults";
            this.dgvResults.RowHeadersWidth = 41;
            this.dgvResults.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.dgvResults.Size = new System.Drawing.Size(772, 168);
            this.dgvResults.TabIndex = 9;
            this.dgvResults.CellContentClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dgvResults_CellContentClick);
            this.dgvResults.CellContentDoubleClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dgvResults_CellContentClick);
            this.dgvResults.CellValueChanged += new System.Windows.Forms.DataGridViewCellEventHandler(this.dgvResults_CellValueChanged);
            // 
            // colReplaceFlag
            // 
            this.colReplaceFlag.HeaderText = "Replace";
            this.colReplaceFlag.Name = "colReplaceFlag";
            this.colReplaceFlag.Resizable = System.Windows.Forms.DataGridViewTriState.False;
            this.colReplaceFlag.Width = 60;
            // 
            // colAnimNumber
            // 
            this.colAnimNumber.HeaderText = "Anim #";
            this.colAnimNumber.Name = "colAnimNumber";
            this.colAnimNumber.ReadOnly = true;
            this.colAnimNumber.Width = 50;
            // 
            // colDescription
            // 
            this.colDescription.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.colDescription.HeaderText = "Description";
            this.colDescription.Name = "colDescription";
            this.colDescription.ReadOnly = true;
            this.colDescription.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            // 
            // butFind
            // 
            this.butFind.Location = new System.Drawing.Point(12, 184);
            this.butFind.Name = "butFind";
            this.butFind.Size = new System.Drawing.Size(80, 23);
            this.butFind.TabIndex = 10;
            this.butFind.Text = "Find";
            this.butFind.Click += new System.EventHandler(this.butFind_Click);
            // 
            // butReplace
            // 
            this.butReplace.Location = new System.Drawing.Point(98, 184);
            this.butReplace.Name = "butReplace";
            this.butReplace.Size = new System.Drawing.Size(108, 23);
            this.butReplace.TabIndex = 11;
            this.butReplace.Text = "Replace selected";
            this.butReplace.Click += new System.EventHandler(this.butReplace_Click);
            // 
            // butOK
            // 
            this.butOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.butOK.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.butOK.Location = new System.Drawing.Point(704, 387);
            this.butOK.Name = "butOK";
            this.butOK.Size = new System.Drawing.Size(80, 23);
            this.butOK.TabIndex = 12;
            this.butOK.Text = "OK";
            this.butOK.Click += new System.EventHandler(this.butOK_Click);
            // 
            // butSelectAll
            // 
            this.butSelectAll.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.butSelectAll.Location = new System.Drawing.Point(618, 184);
            this.butSelectAll.Name = "butSelectAll";
            this.butSelectAll.Size = new System.Drawing.Size(80, 23);
            this.butSelectAll.TabIndex = 13;
            this.butSelectAll.Text = "Select all";
            this.butSelectAll.Click += new System.EventHandler(this.butSelectAll_Click);
            // 
            // butDeselectAll
            // 
            this.butDeselectAll.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.butDeselectAll.Location = new System.Drawing.Point(704, 184);
            this.butDeselectAll.Name = "butDeselectAll";
            this.butDeselectAll.Size = new System.Drawing.Size(80, 23);
            this.butDeselectAll.TabIndex = 14;
            this.butDeselectAll.Text = "Deselect all";
            this.butDeselectAll.Click += new System.EventHandler(this.butDeselectAll_Click);
            // 
            // butDelete
            // 
            this.butDelete.Location = new System.Drawing.Point(212, 184);
            this.butDelete.Name = "butDelete";
            this.butDelete.Size = new System.Drawing.Size(108, 23);
            this.butDelete.TabIndex = 15;
            this.butDelete.Text = "Delete selected";
            this.butDelete.Click += new System.EventHandler(this.butDelete_Click);
            // 
            // statusLabel
            // 
            this.statusLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.statusLabel.AutoSize = true;
            this.statusLabel.ForeColor = System.Drawing.Color.Silver;
            this.statusLabel.Location = new System.Drawing.Point(9, 392);
            this.statusLabel.Name = "statusLabel";
            this.statusLabel.Size = new System.Drawing.Size(13, 13);
            this.statusLabel.TabIndex = 16;
            this.statusLabel.Text = "  ";
            // 
            // FormReplaceAnimCommands
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.butOK;
            this.ClientSize = new System.Drawing.Size(795, 421);
            this.Controls.Add(this.statusLabel);
            this.Controls.Add(this.butDelete);
            this.Controls.Add(this.butReplace);
            this.Controls.Add(this.butDeselectAll);
            this.Controls.Add(this.butSelectAll);
            this.Controls.Add(this.butOK);
            this.Controls.Add(this.butFind);
            this.Controls.Add(this.dgvResults);
            this.Controls.Add(this.darkGroupBox2);
            this.Controls.Add(this.darkGroupBox1);
            this.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.MaximumSize = new System.Drawing.Size(811, 10000);
            this.MinimumSize = new System.Drawing.Size(811, 400);
            this.Name = "FormReplaceAnimCommands";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Find & replace anim commands";
            this.darkGroupBox1.ResumeLayout(false);
            this.darkGroupBox2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgvResults)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private AnimCommandEditor aceFind;
        private DarkUI.Controls.DarkGroupBox darkGroupBox1;
        private DarkUI.Controls.DarkGroupBox darkGroupBox2;
        private AnimCommandEditor aceReplace;
        private DarkUI.Controls.DarkDataGridView dgvResults;
        private DarkUI.Controls.DarkButton butFind;
        private DarkUI.Controls.DarkButton butReplace;
        private DarkUI.Controls.DarkButton butOK;
        private DarkUI.Controls.DarkButton butSelectAll;
        private DarkUI.Controls.DarkButton butDeselectAll;
        private DarkUI.Controls.DarkButton butDelete;
        private DarkUI.Controls.DarkLabel statusLabel;
        private System.Windows.Forms.DataGridViewCheckBoxColumn colReplaceFlag;
        private System.Windows.Forms.DataGridViewTextBoxColumn colAnimNumber;
        private System.Windows.Forms.DataGridViewTextBoxColumn colDescription;
    }
}