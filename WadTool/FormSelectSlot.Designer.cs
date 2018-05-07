namespace WadTool
{
    partial class FormSelectSlot
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
            this.treeSlots = new DarkUI.Controls.DarkTreeView();
            this.butOK = new DarkUI.Controls.DarkButton();
            this.butCancel = new DarkUI.Controls.DarkButton();
            this.tbSearch = new DarkUI.Controls.DarkTextBox();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.tbSearchLabel = new DarkUI.Controls.DarkLabel();
            this.darkLabel2 = new DarkUI.Controls.DarkLabel();
            this.chosenId = new DarkUI.Controls.DarkNumericUpDown();
            this.backgroundWorker1 = new System.ComponentModel.BackgroundWorker();
            this.chosenIdText = new DarkUI.Controls.DarkTextBox();
            this.tableLayoutPanel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.chosenId)).BeginInit();
            this.SuspendLayout();
            // 
            // treeSlots
            // 
            this.treeSlots.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.treeSlots.Location = new System.Drawing.Point(12, 67);
            this.treeSlots.MaxDragChange = 20;
            this.treeSlots.Name = "treeSlots";
            this.treeSlots.Size = new System.Drawing.Size(382, 329);
            this.treeSlots.TabIndex = 2;
            this.treeSlots.Text = "darkTreeView1";
            this.treeSlots.SelectedNodesChanged += new System.EventHandler(this.treeSlots_SelectedNodesChanged);
            // 
            // butOK
            // 
            this.butOK.Dock = System.Windows.Forms.DockStyle.Fill;
            this.butOK.Location = new System.Drawing.Point(0, 0);
            this.butOK.Margin = new System.Windows.Forms.Padding(0, 0, 2, 0);
            this.butOK.Name = "butOK";
            this.butOK.Size = new System.Drawing.Size(189, 25);
            this.butOK.TabIndex = 3;
            this.butOK.Text = "OK";
            this.butOK.Click += new System.EventHandler(this.butOK_Click);
            // 
            // butCancel
            // 
            this.butCancel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.butCancel.Location = new System.Drawing.Point(193, 0);
            this.butCancel.Margin = new System.Windows.Forms.Padding(2, 0, 0, 0);
            this.butCancel.Name = "butCancel";
            this.butCancel.Size = new System.Drawing.Size(189, 25);
            this.butCancel.TabIndex = 4;
            this.butCancel.Text = "Cancel";
            this.butCancel.Click += new System.EventHandler(this.butCancel_Click);
            // 
            // tbSearch
            // 
            this.tbSearch.Location = new System.Drawing.Point(128, 39);
            this.tbSearch.Name = "tbSearch";
            this.tbSearch.Size = new System.Drawing.Size(266, 20);
            this.tbSearch.TabIndex = 0;
            this.tbSearch.TextChanged += new System.EventHandler(this.tbSearch_TextChanged);
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tableLayoutPanel1.ColumnCount = 2;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.Controls.Add(this.butOK, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.butCancel, 1, 0);
            this.tableLayoutPanel1.Location = new System.Drawing.Point(12, 399);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 1;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(382, 25);
            this.tableLayoutPanel1.TabIndex = 19;
            // 
            // tbSearchLabel
            // 
            this.tbSearchLabel.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.tbSearchLabel.Location = new System.Drawing.Point(12, 39);
            this.tbSearchLabel.Name = "tbSearchLabel";
            this.tbSearchLabel.Size = new System.Drawing.Size(110, 20);
            this.tbSearchLabel.TabIndex = 20;
            this.tbSearchLabel.Text = "Search keyword:";
            this.tbSearchLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // darkLabel2
            // 
            this.darkLabel2.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.darkLabel2.Location = new System.Drawing.Point(12, 12);
            this.darkLabel2.Name = "darkLabel2";
            this.darkLabel2.Size = new System.Drawing.Size(110, 20);
            this.darkLabel2.TabIndex = 20;
            this.darkLabel2.Text = "Chosen Id:";
            this.darkLabel2.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // chosenId
            // 
            this.chosenId.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(69)))), ((int)(((byte)(73)))), ((int)(((byte)(74)))));
            this.chosenId.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.chosenId.IncrementAlternate = new decimal(new int[] {
            32,
            0,
            0,
            0});
            this.chosenId.Location = new System.Drawing.Point(128, 12);
            this.chosenId.Maximum = new decimal(new int[] {
            -1,
            0,
            0,
            0});
            this.chosenId.MousewheelSingleIncrement = true;
            this.chosenId.Name = "chosenId";
            this.chosenId.Size = new System.Drawing.Size(105, 20);
            this.chosenId.TabIndex = 1;
            this.chosenId.ValueChanged += new System.EventHandler(this.chosenId_ValueChanged);
            // 
            // chosenIdText
            // 
            this.chosenIdText.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.chosenIdText.Location = new System.Drawing.Point(128, 12);
            this.chosenIdText.Name = "chosenIdText";
            this.chosenIdText.Size = new System.Drawing.Size(266, 20);
            this.chosenIdText.TabIndex = 21;
            this.chosenIdText.Visible = false;
            // 
            // FormSelectSlot
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(406, 434);
            this.Controls.Add(this.chosenId);
            this.Controls.Add(this.darkLabel2);
            this.Controls.Add(this.tbSearchLabel);
            this.Controls.Add(this.tableLayoutPanel1);
            this.Controls.Add(this.tbSearch);
            this.Controls.Add(this.treeSlots);
            this.Controls.Add(this.chosenIdText);
            this.MinimizeBox = false;
            this.Name = "FormSelectSlot";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Select slot";
            this.tableLayoutPanel1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.chosenId)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private DarkUI.Controls.DarkTreeView treeSlots;
        private DarkUI.Controls.DarkButton butOK;
        private DarkUI.Controls.DarkButton butCancel;
        private DarkUI.Controls.DarkTextBox tbSearch;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private DarkUI.Controls.DarkLabel tbSearchLabel;
        private DarkUI.Controls.DarkLabel darkLabel2;
        private DarkUI.Controls.DarkNumericUpDown chosenId;
        private System.ComponentModel.BackgroundWorker backgroundWorker1;
        private DarkUI.Controls.DarkTextBox chosenIdText;
    }
}