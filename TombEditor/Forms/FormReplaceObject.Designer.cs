namespace TombEditor.Forms
{
    partial class FormReplaceObject
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
            this.darkLabel1 = new DarkUI.Controls.DarkLabel();
            this.lblDest = new DarkUI.Controls.DarkLabel();
            this.butSelectSourceObject = new DarkUI.Controls.DarkButton();
            this.butSelectDestObject = new DarkUI.Controls.DarkButton();
            this.butCancel = new DarkUI.Controls.DarkButton();
            this.butReplace = new DarkUI.Controls.DarkButton();
            this.butNewSearch = new DarkUI.Controls.DarkButton();
            this.cbSelectedRooms = new DarkUI.Controls.DarkCheckBox();
            this.darkLabel3 = new DarkUI.Controls.DarkLabel();
            this.darkLabel4 = new DarkUI.Controls.DarkLabel();
            this.cmbSearchType = new DarkUI.Controls.DarkComboBox();
            this.cmbReplaceType = new DarkUI.Controls.DarkComboBox();
            this.tbSourceObject = new DarkUI.Controls.DarkLabel();
            this.tbDestObject = new DarkUI.Controls.DarkLabel();
            this.colSource = new DarkUI.Controls.DarkPanel();
            this.colDest = new DarkUI.Controls.DarkPanel();
            this.butSwapSrcDest = new DarkUI.Controls.DarkButton();
            this.darkStatusStrip1 = new DarkUI.Controls.DarkStatusStrip();
            this.lblResult = new System.Windows.Forms.ToolStripStatusLabel();
            this.darkStatusStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // darkLabel1
            // 
            this.darkLabel1.AutoSize = true;
            this.darkLabel1.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.darkLabel1.Location = new System.Drawing.Point(7, 9);
            this.darkLabel1.Name = "darkLabel1";
            this.darkLabel1.Size = new System.Drawing.Size(130, 13);
            this.darkLabel1.TabIndex = 0;
            this.darkLabel1.Text = "Source object to search:";
            // 
            // lblDest
            // 
            this.lblDest.AutoSize = true;
            this.lblDest.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.lblDest.Location = new System.Drawing.Point(7, 89);
            this.lblDest.Name = "lblDest";
            this.lblDest.Size = new System.Drawing.Size(185, 13);
            this.lblDest.TabIndex = 1;
            this.lblDest.Text = "Destination object to replace with:";
            // 
            // butSelectSourceObject
            // 
            this.butSelectSourceObject.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.butSelectSourceObject.Checked = false;
            this.butSelectSourceObject.Location = new System.Drawing.Point(474, 25);
            this.butSelectSourceObject.Name = "butSelectSourceObject";
            this.butSelectSourceObject.Size = new System.Drawing.Size(99, 22);
            this.butSelectSourceObject.TabIndex = 0;
            this.butSelectSourceObject.Text = "Select in level";
            this.butSelectSourceObject.Click += new System.EventHandler(this.butSelectSourceObject_Click);
            // 
            // butSelectDestObject
            // 
            this.butSelectDestObject.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.butSelectDestObject.Checked = false;
            this.butSelectDestObject.Location = new System.Drawing.Point(474, 105);
            this.butSelectDestObject.Name = "butSelectDestObject";
            this.butSelectDestObject.Size = new System.Drawing.Size(99, 22);
            this.butSelectDestObject.TabIndex = 2;
            this.butSelectDestObject.Text = "Select in level";
            this.butSelectDestObject.Click += new System.EventHandler(this.butSelectDestObject_Click);
            // 
            // butCancel
            // 
            this.butCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.butCancel.Checked = false;
            this.butCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.butCancel.Location = new System.Drawing.Point(493, 167);
            this.butCancel.Name = "butCancel";
            this.butCancel.Size = new System.Drawing.Size(80, 23);
            this.butCancel.TabIndex = 8;
            this.butCancel.Text = "Close";
            this.butCancel.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.butCancel.Click += new System.EventHandler(this.butCancel_Click);
            // 
            // butReplace
            // 
            this.butReplace.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.butReplace.Checked = false;
            this.butReplace.Location = new System.Drawing.Point(407, 167);
            this.butReplace.Name = "butReplace";
            this.butReplace.Size = new System.Drawing.Size(80, 23);
            this.butReplace.TabIndex = 7;
            this.butReplace.Text = "Replace";
            this.butReplace.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.butReplace.Click += new System.EventHandler(this.butReplace_Click);
            // 
            // butNewSearch
            // 
            this.butNewSearch.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.butNewSearch.Checked = false;
            this.butNewSearch.Location = new System.Drawing.Point(157, 167);
            this.butNewSearch.Name = "butNewSearch";
            this.butNewSearch.Size = new System.Drawing.Size(80, 23);
            this.butNewSearch.TabIndex = 5;
            this.butNewSearch.Text = "New search";
            this.butNewSearch.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.butNewSearch.Click += new System.EventHandler(this.butNewSearch_Click);
            // 
            // cbSelectedRooms
            // 
            this.cbSelectedRooms.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.cbSelectedRooms.AutoSize = true;
            this.cbSelectedRooms.Checked = true;
            this.cbSelectedRooms.CheckState = System.Windows.Forms.CheckState.Checked;
            this.cbSelectedRooms.Location = new System.Drawing.Point(10, 171);
            this.cbSelectedRooms.Name = "cbSelectedRooms";
            this.cbSelectedRooms.Size = new System.Drawing.Size(129, 17);
            this.cbSelectedRooms.TabIndex = 4;
            this.cbSelectedRooms.Text = "Selected rooms only";
            // 
            // darkLabel3
            // 
            this.darkLabel3.AutoSize = true;
            this.darkLabel3.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.darkLabel3.Location = new System.Drawing.Point(7, 136);
            this.darkLabel3.Name = "darkLabel3";
            this.darkLabel3.Size = new System.Drawing.Size(75, 13);
            this.darkLabel3.TabIndex = 10;
            this.darkLabel3.Text = "Replace type:";
            // 
            // darkLabel4
            // 
            this.darkLabel4.AutoSize = true;
            this.darkLabel4.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.darkLabel4.Location = new System.Drawing.Point(7, 56);
            this.darkLabel4.Name = "darkLabel4";
            this.darkLabel4.Size = new System.Drawing.Size(69, 13);
            this.darkLabel4.TabIndex = 12;
            this.darkLabel4.Text = "Search type:";
            // 
            // cmbSearchType
            // 
            this.cmbSearchType.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.cmbSearchType.FormattingEnabled = true;
            this.cmbSearchType.Location = new System.Drawing.Point(88, 53);
            this.cmbSearchType.Name = "cmbSearchType";
            this.cmbSearchType.Size = new System.Drawing.Size(485, 23);
            this.cmbSearchType.TabIndex = 1;
            // 
            // cmbReplaceType
            // 
            this.cmbReplaceType.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.cmbReplaceType.FormattingEnabled = true;
            this.cmbReplaceType.Location = new System.Drawing.Point(88, 133);
            this.cmbReplaceType.Name = "cmbReplaceType";
            this.cmbReplaceType.Size = new System.Drawing.Size(485, 23);
            this.cmbReplaceType.TabIndex = 3;
            // 
            // tbSourceObject
            // 
            this.tbSourceObject.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tbSourceObject.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.tbSourceObject.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.tbSourceObject.Location = new System.Drawing.Point(10, 25);
            this.tbSourceObject.Name = "tbSourceObject";
            this.tbSourceObject.Size = new System.Drawing.Size(458, 22);
            this.tbSourceObject.TabIndex = 14;
            this.tbSourceObject.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // tbDestObject
            // 
            this.tbDestObject.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tbDestObject.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.tbDestObject.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.tbDestObject.Location = new System.Drawing.Point(10, 105);
            this.tbDestObject.Name = "tbDestObject";
            this.tbDestObject.Size = new System.Drawing.Size(458, 22);
            this.tbDestObject.TabIndex = 15;
            this.tbDestObject.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // colSource
            // 
            this.colSource.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.colSource.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.colSource.Location = new System.Drawing.Point(407, 25);
            this.colSource.Name = "colSource";
            this.colSource.Size = new System.Drawing.Size(61, 22);
            this.colSource.TabIndex = 16;
            this.colSource.Visible = false;
            this.colSource.Click += new System.EventHandler(this.colorPicker_Click);
            // 
            // colDest
            // 
            this.colDest.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.colDest.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.colDest.Location = new System.Drawing.Point(407, 105);
            this.colDest.Name = "colDest";
            this.colDest.Size = new System.Drawing.Size(61, 22);
            this.colDest.TabIndex = 17;
            this.colDest.Visible = false;
            this.colDest.Click += new System.EventHandler(this.colorPicker_Click);
            // 
            // butSwapSrcDest
            // 
            this.butSwapSrcDest.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.butSwapSrcDest.Checked = false;
            this.butSwapSrcDest.Location = new System.Drawing.Point(243, 167);
            this.butSwapSrcDest.Name = "butSwapSrcDest";
            this.butSwapSrcDest.Size = new System.Drawing.Size(158, 23);
            this.butSwapSrcDest.TabIndex = 6;
            this.butSwapSrcDest.Text = "Swap source / destination";
            this.butSwapSrcDest.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.butSwapSrcDest.Click += new System.EventHandler(this.butSwapSrcDest_Click);
            // 
            // darkStatusStrip1
            // 
            this.darkStatusStrip1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.darkStatusStrip1.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.darkStatusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.lblResult});
            this.darkStatusStrip1.Location = new System.Drawing.Point(0, 200);
            this.darkStatusStrip1.Name = "darkStatusStrip1";
            this.darkStatusStrip1.Padding = new System.Windows.Forms.Padding(2, 5, 0, 3);
            this.darkStatusStrip1.Size = new System.Drawing.Size(584, 26);
            this.darkStatusStrip1.TabIndex = 19;
            // 
            // lblResult
            // 
            this.lblResult.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.lblResult.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.lblResult.ForeColor = System.Drawing.Color.Silver;
            this.lblResult.Name = "lblResult";
            this.lblResult.Size = new System.Drawing.Size(0, 13);
            this.lblResult.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // FormReplaceObject
            // 
            this.AcceptButton = this.butNewSearch;
            this.AllowDrop = true;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.butCancel;
            this.ClientSize = new System.Drawing.Size(584, 226);
            this.Controls.Add(this.darkStatusStrip1);
            this.Controls.Add(this.butSwapSrcDest);
            this.Controls.Add(this.colDest);
            this.Controls.Add(this.colSource);
            this.Controls.Add(this.tbDestObject);
            this.Controls.Add(this.tbSourceObject);
            this.Controls.Add(this.cmbSearchType);
            this.Controls.Add(this.darkLabel4);
            this.Controls.Add(this.cmbReplaceType);
            this.Controls.Add(this.darkLabel3);
            this.Controls.Add(this.cbSelectedRooms);
            this.Controls.Add(this.butNewSearch);
            this.Controls.Add(this.butCancel);
            this.Controls.Add(this.butReplace);
            this.Controls.Add(this.butSelectDestObject);
            this.Controls.Add(this.butSelectSourceObject);
            this.Controls.Add(this.lblDest);
            this.Controls.Add(this.darkLabel1);
            this.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.MaximizeBox = false;
            this.MaximumSize = new System.Drawing.Size(10000, 264);
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(600, 264);
            this.Name = "FormReplaceObject";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Search & replace objects";
            this.DragDrop += new System.Windows.Forms.DragEventHandler(this.FormReplaceObject_DragDrop);
            this.DragEnter += new System.Windows.Forms.DragEventHandler(this.FormReplaceObject_DragEnter);
            this.darkStatusStrip1.ResumeLayout(false);
            this.darkStatusStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private DarkUI.Controls.DarkLabel darkLabel1;
        private DarkUI.Controls.DarkLabel lblDest;
        private DarkUI.Controls.DarkButton butSelectSourceObject;
        private DarkUI.Controls.DarkButton butSelectDestObject;
        private DarkUI.Controls.DarkButton butCancel;
        private DarkUI.Controls.DarkButton butReplace;
        private DarkUI.Controls.DarkButton butNewSearch;
        private DarkUI.Controls.DarkCheckBox cbSelectedRooms;
        private DarkUI.Controls.DarkLabel darkLabel3;
        private DarkUI.Controls.DarkComboBox cmbReplaceType;
        private DarkUI.Controls.DarkComboBox cmbSearchType;
        private DarkUI.Controls.DarkLabel darkLabel4;
        private DarkUI.Controls.DarkLabel tbSourceObject;
        private DarkUI.Controls.DarkLabel tbDestObject;
        private DarkUI.Controls.DarkPanel colSource;
        private DarkUI.Controls.DarkPanel colDest;
        private DarkUI.Controls.DarkButton butSwapSrcDest;
        private DarkUI.Controls.DarkStatusStrip darkStatusStrip1;
        private System.Windows.Forms.ToolStripStatusLabel lblResult;
    }
}