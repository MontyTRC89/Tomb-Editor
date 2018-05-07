namespace WadTool
{
    partial class FormStateChangesEditor
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
            this.dgvStateChanges = new DarkUI.Controls.DarkDataGridView();
            this.btCancel = new DarkUI.Controls.DarkButton();
            this.btOk = new DarkUI.Controls.DarkButton();
            this.dgvControls = new TombLib.Controls.DarkDataGridViewControls();
            this.columnStateId = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.columnLowFrame = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.columnHighFrame = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.columnNextAnimation = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.columnNextFrame = new System.Windows.Forms.DataGridViewTextBoxColumn();
            ((System.ComponentModel.ISupportInitialize)(this.dgvStateChanges)).BeginInit();
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
            this.dgvStateChanges.Location = new System.Drawing.Point(12, 12);
            this.dgvStateChanges.Name = "dgvStateChanges";
            this.dgvStateChanges.RowHeadersWidth = 40;
            this.dgvStateChanges.RowTemplate.Height = 16;
            this.dgvStateChanges.Size = new System.Drawing.Size(524, 365);
            this.dgvStateChanges.TabIndex = 48;
            // 
            // btCancel
            // 
            this.btCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btCancel.Location = new System.Drawing.Point(175, 393);
            this.btCancel.Name = "btCancel";
            this.btCancel.Size = new System.Drawing.Size(113, 26);
            this.btCancel.TabIndex = 50;
            this.btCancel.Text = "Cancel";
            this.btCancel.Click += new System.EventHandler(this.btCancel_Click);
            // 
            // btOk
            // 
            this.btOk.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btOk.Location = new System.Drawing.Point(294, 393);
            this.btOk.Name = "btOk";
            this.btOk.Size = new System.Drawing.Size(113, 26);
            this.btOk.TabIndex = 51;
            this.btOk.Text = "Ok";
            this.btOk.Click += new System.EventHandler(this.btOk_Click);
            // 
            // dgvControls
            // 
            this.dgvControls.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.dgvControls.Enabled = false;
            this.dgvControls.Location = new System.Drawing.Point(545, 12);
            this.dgvControls.MinimumSize = new System.Drawing.Size(24, 24);
            this.dgvControls.Name = "dgvControls";
            this.dgvControls.Size = new System.Drawing.Size(27, 365);
            this.dgvControls.TabIndex = 49;
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
            this.columnNextFrame.DataPropertyName = "NextFrame";
            this.columnNextFrame.HeaderText = "NextFrame";
            this.columnNextFrame.Name = "columnNextFrame";
            // 
            // FormStateChangesEditor
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(584, 431);
            this.Controls.Add(this.btCancel);
            this.Controls.Add(this.btOk);
            this.Controls.Add(this.dgvControls);
            this.Controls.Add(this.dgvStateChanges);
            this.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FormStateChangesEditor";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "State changes";
            ((System.ComponentModel.ISupportInitialize)(this.dgvStateChanges)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private TombLib.Controls.DarkDataGridViewControls dgvControls;
        private DarkUI.Controls.DarkDataGridView dgvStateChanges;
        private DarkUI.Controls.DarkButton btCancel;
        private DarkUI.Controls.DarkButton btOk;
        private System.Windows.Forms.DataGridViewTextBoxColumn columnStateId;
        private System.Windows.Forms.DataGridViewTextBoxColumn columnLowFrame;
        private System.Windows.Forms.DataGridViewTextBoxColumn columnHighFrame;
        private System.Windows.Forms.DataGridViewTextBoxColumn columnNextAnimation;
        private System.Windows.Forms.DataGridViewTextBoxColumn columnNextFrame;
    }
}