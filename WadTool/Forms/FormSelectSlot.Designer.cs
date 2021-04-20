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
            this.butOK = new DarkUI.Controls.DarkButton();
            this.butCancel = new DarkUI.Controls.DarkButton();
            this.tbSearch = new DarkUI.Controls.DarkTextBox();
            this.tbSearchLabel = new DarkUI.Controls.DarkLabel();
            this.darkLabel2 = new DarkUI.Controls.DarkLabel();
            this.chosenId = new DarkUI.Controls.DarkNumericUpDown();
            this.chosenIdText = new DarkUI.Controls.DarkTextBox();
            this.lstSlots = new DarkUI.Controls.DarkListView();
            ((System.ComponentModel.ISupportInitialize)(this.chosenId)).BeginInit();
            this.SuspendLayout();
            // 
            // butOK
            // 
            this.butOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.butOK.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.butOK.Location = new System.Drawing.Point(236, 406);
            this.butOK.Margin = new System.Windows.Forms.Padding(0, 0, 2, 0);
            this.butOK.Name = "butOK";
            this.butOK.Size = new System.Drawing.Size(81, 23);
            this.butOK.TabIndex = 3;
            this.butOK.Text = "OK";
            this.butOK.Click += new System.EventHandler(this.butOK_Click);
            // 
            // butCancel
            // 
            this.butCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.butCancel.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.butCancel.Location = new System.Drawing.Point(321, 406);
            this.butCancel.Margin = new System.Windows.Forms.Padding(2, 0, 0, 0);
            this.butCancel.Name = "butCancel";
            this.butCancel.Size = new System.Drawing.Size(81, 23);
            this.butCancel.TabIndex = 4;
            this.butCancel.Text = "Cancel";
            this.butCancel.Click += new System.EventHandler(this.butCancel_Click);
            // 
            // tbSearch
            // 
            this.tbSearch.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tbSearch.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.tbSearch.Location = new System.Drawing.Point(104, 39);
            this.tbSearch.Name = "tbSearch";
            this.tbSearch.Size = new System.Drawing.Size(298, 22);
            this.tbSearch.TabIndex = 0;
            this.tbSearch.TextChanged += new System.EventHandler(this.tbSearch_TextChanged);
            // 
            // tbSearchLabel
            // 
            this.tbSearchLabel.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.tbSearchLabel.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.tbSearchLabel.Location = new System.Drawing.Point(12, 37);
            this.tbSearchLabel.Name = "tbSearchLabel";
            this.tbSearchLabel.Size = new System.Drawing.Size(91, 20);
            this.tbSearchLabel.TabIndex = 20;
            this.tbSearchLabel.Text = "Search keyword:";
            this.tbSearchLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // darkLabel2
            // 
            this.darkLabel2.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.darkLabel2.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.darkLabel2.Location = new System.Drawing.Point(15, 10);
            this.darkLabel2.Name = "darkLabel2";
            this.darkLabel2.Size = new System.Drawing.Size(88, 20);
            this.darkLabel2.TabIndex = 20;
            this.darkLabel2.Text = "Chosen ID:";
            this.darkLabel2.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // chosenId
            // 
            this.chosenId.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(69)))), ((int)(((byte)(73)))), ((int)(((byte)(74)))));
            this.chosenId.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.chosenId.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.chosenId.IncrementAlternate = new decimal(new int[] {
            32,
            0,
            0,
            0});
            this.chosenId.Location = new System.Drawing.Point(104, 12);
            this.chosenId.LoopValues = false;
            this.chosenId.Maximum = new decimal(new int[] {
            -1,
            0,
            0,
            0});
            this.chosenId.Name = "chosenId";
            this.chosenId.Size = new System.Drawing.Size(81, 22);
            this.chosenId.TabIndex = 1;
            this.chosenId.ValueChanged += new System.EventHandler(this.chosenId_ValueChanged);
            // 
            // chosenIdText
            // 
            this.chosenIdText.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.chosenIdText.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.chosenIdText.Location = new System.Drawing.Point(104, 12);
            this.chosenIdText.Name = "chosenIdText";
            this.chosenIdText.Size = new System.Drawing.Size(298, 22);
            this.chosenIdText.TabIndex = 21;
            this.chosenIdText.Visible = false;
            // 
            // lstSlots
            // 
            this.lstSlots.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lstSlots.Location = new System.Drawing.Point(4, 67);
            this.lstSlots.Name = "lstSlots";
            this.lstSlots.Size = new System.Drawing.Size(398, 335);
            this.lstSlots.TabIndex = 22;
            this.lstSlots.SelectedIndicesChanged += new System.EventHandler(this.lstSlots_SelectedIndicesChanged);
            this.lstSlots.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.lstSlots_MouseDoubleClick);
            // 
            // FormSelectSlot
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(406, 434);
            this.Controls.Add(this.lstSlots);
            this.Controls.Add(this.butCancel);
            this.Controls.Add(this.butOK);
            this.Controls.Add(this.chosenId);
            this.Controls.Add(this.darkLabel2);
            this.Controls.Add(this.tbSearchLabel);
            this.Controls.Add(this.tbSearch);
            this.Controls.Add(this.chosenIdText);
            this.MinimizeBox = false;
            this.Name = "FormSelectSlot";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Select slot";
            ((System.ComponentModel.ISupportInitialize)(this.chosenId)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private DarkUI.Controls.DarkButton butOK;
        private DarkUI.Controls.DarkButton butCancel;
        private DarkUI.Controls.DarkTextBox tbSearch;
        private DarkUI.Controls.DarkLabel tbSearchLabel;
        private DarkUI.Controls.DarkLabel darkLabel2;
        private DarkUI.Controls.DarkNumericUpDown chosenId;
        private DarkUI.Controls.DarkTextBox chosenIdText;
        private DarkUI.Controls.DarkListView lstSlots;
    }
}