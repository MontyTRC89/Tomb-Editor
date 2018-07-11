namespace TombEditor.Forms
{
    partial class FormSearch
    {
        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.comboScope = new DarkUI.Controls.DarkComboBox();
            this.darkLabel1 = new DarkUI.Controls.DarkLabel();
            this.darkLabel2 = new DarkUI.Controls.DarkLabel();
            this.txtKeywords = new DarkUI.Controls.DarkTextBox();
            this.objectList = new DarkUI.Controls.DarkDataGridView();
            this.objectListColumnType = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.objectListColumnRoom = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.objectListColumnName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.butOk = new DarkUI.Controls.DarkButton();
            ((System.ComponentModel.ISupportInitialize)(this.objectList)).BeginInit();
            this.SuspendLayout();
            // 
            // comboScope
            // 
            this.comboScope.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.comboScope.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.comboScope.FormattingEnabled = true;
            this.comboScope.Location = new System.Drawing.Point(94, 36);
            this.comboScope.Name = "comboScope";
            this.comboScope.Size = new System.Drawing.Size(538, 23);
            this.comboScope.TabIndex = 1;
            this.comboScope.SelectedIndexChanged += new System.EventHandler(this.comboScope_SelectedIndexChanged);
            // 
            // darkLabel1
            // 
            this.darkLabel1.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.darkLabel1.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.darkLabel1.Location = new System.Drawing.Point(4, 7);
            this.darkLabel1.Name = "darkLabel1";
            this.darkLabel1.Size = new System.Drawing.Size(84, 20);
            this.darkLabel1.TabIndex = 79;
            this.darkLabel1.Text = "Keywords:";
            this.darkLabel1.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // darkLabel2
            // 
            this.darkLabel2.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.darkLabel2.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.darkLabel2.Location = new System.Drawing.Point(4, 35);
            this.darkLabel2.Name = "darkLabel2";
            this.darkLabel2.Size = new System.Drawing.Size(84, 20);
            this.darkLabel2.TabIndex = 79;
            this.darkLabel2.Text = "Scope:";
            this.darkLabel2.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // txtKeywords
            // 
            this.txtKeywords.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtKeywords.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.txtKeywords.Location = new System.Drawing.Point(94, 9);
            this.txtKeywords.Name = "txtKeywords";
            this.txtKeywords.Size = new System.Drawing.Size(538, 22);
            this.txtKeywords.TabIndex = 0;
            this.txtKeywords.TextChanged += new System.EventHandler(this.txtKeywords_TextChanged);
            // 
            // objectList
            // 
            this.objectList.AllowUserToAddRows = false;
            this.objectList.AllowUserToDeleteRows = false;
            this.objectList.AllowUserToDragDropRows = false;
            this.objectList.AllowUserToPasteCells = false;
            this.objectList.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.objectList.ClipboardCopyMode = System.Windows.Forms.DataGridViewClipboardCopyMode.Disable;
            this.objectList.ColumnHeadersHeight = 17;
            this.objectList.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.objectListColumnType,
            this.objectListColumnRoom,
            this.objectListColumnName});
            this.objectList.Location = new System.Drawing.Point(7, 65);
            this.objectList.MultiSelect = false;
            this.objectList.Name = "objectList";
            this.objectList.ReadOnly = true;
            this.objectList.RowHeadersWidth = 41;
            this.objectList.Size = new System.Drawing.Size(625, 160);
            this.objectList.TabIndex = 2;
            this.objectList.VirtualMode = true;
            this.objectList.CellFormatting += new System.Windows.Forms.DataGridViewCellFormattingEventHandler(this.objectList_CellFormatting);
            this.objectList.CellValueNeeded += new System.Windows.Forms.DataGridViewCellValueEventHandler(this.objectList_CellValueNeeded);
            this.objectList.SelectionChanged += new System.EventHandler(this.objectList_SelectionChanged);
            this.objectList.Paint += new System.Windows.Forms.PaintEventHandler(this.objectList_Paint);
            // 
            // objectListColumnType
            // 
            this.objectListColumnType.HeaderText = "Type";
            this.objectListColumnType.Name = "objectListColumnType";
            this.objectListColumnType.ReadOnly = true;
            this.objectListColumnType.Width = 70;
            // 
            // objectListColumnRoom
            // 
            this.objectListColumnRoom.HeaderText = "Room";
            this.objectListColumnRoom.Name = "objectListColumnRoom";
            this.objectListColumnRoom.ReadOnly = true;
            // 
            // objectListColumnName
            // 
            this.objectListColumnName.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.objectListColumnName.HeaderText = "Name";
            this.objectListColumnName.Name = "objectListColumnName";
            this.objectListColumnName.ReadOnly = true;
            // 
            // butOk
            // 
            this.butOk.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.butOk.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.butOk.Location = new System.Drawing.Point(552, 231);
            this.butOk.Name = "butOk";
            this.butOk.Size = new System.Drawing.Size(80, 23);
            this.butOk.TabIndex = 3;
            this.butOk.Text = "OK";
            this.butOk.Click += new System.EventHandler(this.butOk_Click);
            // 
            // FormSearch
            // 
            this.AcceptButton = this.butOk;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.butOk;
            this.ClientSize = new System.Drawing.Size(640, 261);
            this.Controls.Add(this.butOk);
            this.Controls.Add(this.objectList);
            this.Controls.Add(this.txtKeywords);
            this.Controls.Add(this.darkLabel2);
            this.Controls.Add(this.darkLabel1);
            this.Controls.Add(this.comboScope);
            this.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(302, 238);
            this.Name = "FormSearch";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Search";
            ((System.ComponentModel.ISupportInitialize)(this.objectList)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private DarkUI.Controls.DarkComboBox comboScope;
        private DarkUI.Controls.DarkLabel darkLabel1;
        private DarkUI.Controls.DarkLabel darkLabel2;
        private DarkUI.Controls.DarkTextBox txtKeywords;
        private DarkUI.Controls.DarkDataGridView objectList;
        private DarkUI.Controls.DarkButton butOk;
        private System.Windows.Forms.DataGridViewTextBoxColumn objectListColumnType;
        private System.Windows.Forms.DataGridViewTextBoxColumn objectListColumnRoom;
        private System.Windows.Forms.DataGridViewTextBoxColumn objectListColumnName;
    }
}