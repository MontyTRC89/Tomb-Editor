namespace TombEditor.Forms
{
    partial class FormFindTextures
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
            this.butNewSearch = new DarkUI.Controls.DarkButton();
            this.butCancel = new DarkUI.Controls.DarkButton();
            this.dgvUntextured = new DarkUI.Controls.DarkDataGridView();
            this.colRoom = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colCoordinates = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.cbSelectedRooms = new DarkUI.Controls.DarkCheckBox();
            this.darkStatusStrip1 = new DarkUI.Controls.DarkStatusStrip();
            this.lblStatus = new System.Windows.Forms.ToolStripStatusLabel();
            this.darkLabel1 = new DarkUI.Controls.DarkLabel();
            this.cbSearchType = new DarkUI.Controls.DarkComboBox();
            ((System.ComponentModel.ISupportInitialize)(this.dgvUntextured)).BeginInit();
            this.darkStatusStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // butNewSearch
            // 
            this.butNewSearch.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.butNewSearch.Checked = false;
            this.butNewSearch.Location = new System.Drawing.Point(142, 308);
            this.butNewSearch.Name = "butNewSearch";
            this.butNewSearch.Size = new System.Drawing.Size(80, 23);
            this.butNewSearch.TabIndex = 9;
            this.butNewSearch.Text = "New search";
            this.butNewSearch.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.butNewSearch.Click += new System.EventHandler(this.butNewSearch_Click);
            // 
            // butCancel
            // 
            this.butCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.butCancel.Checked = false;
            this.butCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.butCancel.Location = new System.Drawing.Point(228, 308);
            this.butCancel.Name = "butCancel";
            this.butCancel.Size = new System.Drawing.Size(80, 23);
            this.butCancel.TabIndex = 10;
            this.butCancel.Text = "Close";
            this.butCancel.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.butCancel.Click += new System.EventHandler(this.butCancel_Click);
            // 
            // dgvUntextured
            // 
            this.dgvUntextured.AllowUserToAddRows = false;
            this.dgvUntextured.AllowUserToDeleteRows = false;
            this.dgvUntextured.AllowUserToDragDropRows = false;
            this.dgvUntextured.AllowUserToPasteCells = false;
            this.dgvUntextured.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.dgvUntextured.ColumnHeadersHeight = 17;
            this.dgvUntextured.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.colRoom,
            this.colCoordinates});
            this.dgvUntextured.ForegroundColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.dgvUntextured.Location = new System.Drawing.Point(7, 6);
            this.dgvUntextured.MultiSelect = false;
            this.dgvUntextured.Name = "dgvUntextured";
            this.dgvUntextured.RowHeadersWidth = 41;
            this.dgvUntextured.Size = new System.Drawing.Size(301, 267);
            this.dgvUntextured.TabIndex = 11;
            this.dgvUntextured.SelectionChanged += new System.EventHandler(this.dgvUntextured_SelectionChanged);
            // 
            // colRoom
            // 
            this.colRoom.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.colRoom.FillWeight = 75F;
            this.colRoom.HeaderText = "Room";
            this.colRoom.Name = "colRoom";
            this.colRoom.ReadOnly = true;
            this.colRoom.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            // 
            // colCoordinates
            // 
            this.colCoordinates.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.colCoordinates.FillWeight = 25F;
            this.colCoordinates.HeaderText = "Block";
            this.colCoordinates.Name = "colCoordinates";
            this.colCoordinates.ReadOnly = true;
            this.colCoordinates.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            // 
            // cbSelectedRooms
            // 
            this.cbSelectedRooms.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.cbSelectedRooms.AutoSize = true;
            this.cbSelectedRooms.Checked = true;
            this.cbSelectedRooms.CheckState = System.Windows.Forms.CheckState.Checked;
            this.cbSelectedRooms.Location = new System.Drawing.Point(7, 312);
            this.cbSelectedRooms.Name = "cbSelectedRooms";
            this.cbSelectedRooms.Size = new System.Drawing.Size(129, 17);
            this.cbSelectedRooms.TabIndex = 12;
            this.cbSelectedRooms.Text = "Selected rooms only";
            // 
            // darkStatusStrip1
            // 
            this.darkStatusStrip1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.darkStatusStrip1.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.darkStatusStrip1.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.darkStatusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.lblStatus});
            this.darkStatusStrip1.Location = new System.Drawing.Point(0, 339);
            this.darkStatusStrip1.Name = "darkStatusStrip1";
            this.darkStatusStrip1.Padding = new System.Windows.Forms.Padding(2, 5, 0, 3);
            this.darkStatusStrip1.Size = new System.Drawing.Size(314, 26);
            this.darkStatusStrip1.TabIndex = 13;
            // 
            // lblStatus
            // 
            this.lblStatus.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.lblStatus.Name = "lblStatus";
            this.lblStatus.Size = new System.Drawing.Size(0, 13);
            // 
            // darkLabel1
            // 
            this.darkLabel1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.darkLabel1.AutoSize = true;
            this.darkLabel1.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.darkLabel1.Location = new System.Drawing.Point(4, 282);
            this.darkLabel1.Name = "darkLabel1";
            this.darkLabel1.Size = new System.Drawing.Size(33, 13);
            this.darkLabel1.TabIndex = 14;
            this.darkLabel1.Text = "Find:";
            // 
            // cbSearchType
            // 
            this.cbSearchType.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.cbSearchType.FormattingEnabled = true;
            this.cbSearchType.Items.AddRange(new object[] {
            "Untextured faces",
            "Broken faces",
            "Exact match with selected texture",
            "Partial match with selected texture",
            "Texture set only"});
            this.cbSearchType.Location = new System.Drawing.Point(43, 279);
            this.cbSearchType.Name = "cbSearchType";
            this.cbSearchType.Size = new System.Drawing.Size(265, 23);
            this.cbSearchType.TabIndex = 15;
            // 
            // FormFindTextures
            // 
            this.AcceptButton = this.butNewSearch;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.butCancel;
            this.ClientSize = new System.Drawing.Size(314, 365);
            this.Controls.Add(this.cbSearchType);
            this.Controls.Add(this.darkLabel1);
            this.Controls.Add(this.darkStatusStrip1);
            this.Controls.Add(this.cbSelectedRooms);
            this.Controls.Add(this.dgvUntextured);
            this.Controls.Add(this.butNewSearch);
            this.Controls.Add(this.butCancel);
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(330, 400);
            this.Name = "FormFindTextures";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
            this.Text = "Search textures";
            ((System.ComponentModel.ISupportInitialize)(this.dgvUntextured)).EndInit();
            this.darkStatusStrip1.ResumeLayout(false);
            this.darkStatusStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private DarkUI.Controls.DarkButton butNewSearch;
        private DarkUI.Controls.DarkButton butCancel;
        private DarkUI.Controls.DarkDataGridView dgvUntextured;
        private DarkUI.Controls.DarkCheckBox cbSelectedRooms;
        private System.Windows.Forms.DataGridViewTextBoxColumn colRoom;
        private System.Windows.Forms.DataGridViewTextBoxColumn colCoordinates;
        private DarkUI.Controls.DarkStatusStrip darkStatusStrip1;
        private System.Windows.Forms.ToolStripStatusLabel lblStatus;
        private DarkUI.Controls.DarkLabel darkLabel1;
        private DarkUI.Controls.DarkComboBox cbSearchType;
    }
}