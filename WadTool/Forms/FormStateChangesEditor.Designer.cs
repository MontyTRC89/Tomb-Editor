namespace WadTool
{
    partial class FormStateChangesEditor
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
            this.dgvStateChanges = new DarkUI.Controls.DarkDataGridView();
            this.columnStateId = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.columnLowFrame = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.columnHighFrame = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.columnNextAnimation = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.columnNextFrame = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.btCancel = new DarkUI.Controls.DarkButton();
            this.btOk = new DarkUI.Controls.DarkButton();
            this.darkSectionPanel1 = new DarkUI.Controls.DarkSectionPanel();
            this.dgvControls = new TombLib.Controls.DarkDataGridViewControls();
            ((System.ComponentModel.ISupportInitialize)(this.dgvStateChanges)).BeginInit();
            this.darkSectionPanel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // dgvStateChanges
            // 
            this.dgvStateChanges.AllowUserToDragDropRows = false;
            this.dgvStateChanges.AllowUserToOrderColumns = true;
            this.dgvStateChanges.AllowUserToPasteCells = false;
            this.dgvStateChanges.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.dgvStateChanges.AutoGenerateColumns = false;
            this.dgvStateChanges.ColumnHeadersHeight = 17;
            this.dgvStateChanges.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.columnStateId,
            this.columnLowFrame,
            this.columnHighFrame,
            this.columnNextAnimation,
            this.columnNextFrame});
            this.dgvStateChanges.Location = new System.Drawing.Point(6, 6);
            this.dgvStateChanges.Name = "dgvStateChanges";
            this.dgvStateChanges.RowHeadersWidth = 40;
            this.dgvStateChanges.RowTemplate.Height = 16;
            this.dgvStateChanges.Size = new System.Drawing.Size(525, 209);
            this.dgvStateChanges.TabIndex = 48;
            // 
            // columnStateId
            // 
            this.columnStateId.DataPropertyName = "StateId";
            this.columnStateId.HeaderText = "State ID";
            this.columnStateId.Name = "columnStateId";
            // 
            // columnLowFrame
            // 
            this.columnLowFrame.DataPropertyName = "LowFrame";
            this.columnLowFrame.HeaderText = "Low frame";
            this.columnLowFrame.Name = "columnLowFrame";
            // 
            // columnHighFrame
            // 
            this.columnHighFrame.DataPropertyName = "HighFrame";
            this.columnHighFrame.HeaderText = "High frame";
            this.columnHighFrame.Name = "columnHighFrame";
            // 
            // columnNextAnimation
            // 
            this.columnNextAnimation.DataPropertyName = "NextAnimation";
            this.columnNextAnimation.HeaderText = "Next animation";
            this.columnNextAnimation.Name = "columnNextAnimation";
            // 
            // columnNextFrame
            // 
            this.columnNextFrame.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.columnNextFrame.DataPropertyName = "NextFrame";
            this.columnNextFrame.HeaderText = "Next frame";
            this.columnNextFrame.Name = "columnNextFrame";
            // 
            // btCancel
            // 
            this.btCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btCancel.Location = new System.Drawing.Point(494, 232);
            this.btCancel.Name = "btCancel";
            this.btCancel.Size = new System.Drawing.Size(81, 23);
            this.btCancel.TabIndex = 50;
            this.btCancel.Text = "Cancel";
            this.btCancel.Click += new System.EventHandler(this.btCancel_Click);
            // 
            // btOk
            // 
            this.btOk.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btOk.Location = new System.Drawing.Point(407, 232);
            this.btOk.Name = "btOk";
            this.btOk.Size = new System.Drawing.Size(81, 23);
            this.btOk.TabIndex = 51;
            this.btOk.Text = "OK";
            this.btOk.Click += new System.EventHandler(this.btOk_Click);
            // 
            // darkSectionPanel1
            // 
            this.darkSectionPanel1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.darkSectionPanel1.Controls.Add(this.dgvStateChanges);
            this.darkSectionPanel1.Controls.Add(this.dgvControls);
            this.darkSectionPanel1.Location = new System.Drawing.Point(5, 5);
            this.darkSectionPanel1.Name = "darkSectionPanel1";
            this.darkSectionPanel1.SectionHeader = null;
            this.darkSectionPanel1.Size = new System.Drawing.Size(570, 221);
            this.darkSectionPanel1.TabIndex = 52;
            // 
            // dgvControls
            // 
            this.dgvControls.AlwaysInsertAtZero = false;
            this.dgvControls.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.dgvControls.Enabled = false;
            this.dgvControls.Location = new System.Drawing.Point(537, 6);
            this.dgvControls.MinimumSize = new System.Drawing.Size(24, 24);
            this.dgvControls.Name = "dgvControls";
            this.dgvControls.Size = new System.Drawing.Size(27, 209);
            this.dgvControls.TabIndex = 49;
            // 
            // FormStateChangesEditor
            // 
            this.AcceptButton = this.btOk;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.btCancel;
            this.ClientSize = new System.Drawing.Size(580, 260);
            this.Controls.Add(this.darkSectionPanel1);
            this.Controls.Add(this.btCancel);
            this.Controls.Add(this.btOk);
            this.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(596, 242);
            this.Name = "FormStateChangesEditor";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "State changes";
            ((System.ComponentModel.ISupportInitialize)(this.dgvStateChanges)).EndInit();
            this.darkSectionPanel1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private TombLib.Controls.DarkDataGridViewControls dgvControls;
        private DarkUI.Controls.DarkDataGridView dgvStateChanges;
        private DarkUI.Controls.DarkButton btCancel;
        private DarkUI.Controls.DarkButton btOk;
        private DarkUI.Controls.DarkSectionPanel darkSectionPanel1;
        private System.Windows.Forms.DataGridViewTextBoxColumn columnStateId;
        private System.Windows.Forms.DataGridViewTextBoxColumn columnLowFrame;
        private System.Windows.Forms.DataGridViewTextBoxColumn columnHighFrame;
        private System.Windows.Forms.DataGridViewTextBoxColumn columnNextAnimation;
        private System.Windows.Forms.DataGridViewTextBoxColumn columnNextFrame;
    }
}