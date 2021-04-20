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
            this.components = new System.ComponentModel.Container();
            this.dgvStateChanges = new DarkUI.Controls.DarkDataGridView();
            this.columnStateName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.columnStateId = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.columnLowFrame = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.columnHighFrame = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.columnNextAnimation = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.columnNextFrame = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.btCancel = new DarkUI.Controls.DarkButton();
            this.btOk = new DarkUI.Controls.DarkButton();
            this.darkSectionPanel1 = new DarkUI.Controls.DarkSectionPanel();
            this.butPlayStateChange = new DarkUI.Controls.DarkButton();
            this.lblStateChangeAnnouncement = new DarkUI.Controls.DarkLabel();
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.dgvControls = new TombLib.Controls.DarkDataGridViewControls();
            ((System.ComponentModel.ISupportInitialize)(this.dgvStateChanges)).BeginInit();
            this.darkSectionPanel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // dgvStateChanges
            // 
            this.dgvStateChanges.AllowUserToAddRows = false;
            this.dgvStateChanges.AllowUserToDragDropRows = false;
            this.dgvStateChanges.AllowUserToPasteCells = false;
            this.dgvStateChanges.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.dgvStateChanges.AutoGenerateColumns = false;
            this.dgvStateChanges.ColumnHeadersHeight = 17;
            this.dgvStateChanges.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.columnStateName,
            this.columnStateId,
            this.columnLowFrame,
            this.columnHighFrame,
            this.columnNextAnimation,
            this.columnNextFrame});
            this.dgvStateChanges.Location = new System.Drawing.Point(6, 6);
            this.dgvStateChanges.Name = "dgvStateChanges";
            this.dgvStateChanges.RowHeadersWidth = 40;
            this.dgvStateChanges.RowTemplate.Height = 16;
            this.dgvStateChanges.ShowCellErrors = false;
            this.dgvStateChanges.Size = new System.Drawing.Size(525, 209);
            this.dgvStateChanges.TabIndex = 48;
            this.dgvStateChanges.CellFormattingSafe += new DarkUI.Controls.DarkDataGridViewSafeCellFormattingEventHandler(this.dgvStateChanges_CellFormattingSafe);
            this.dgvStateChanges.CellEndEdit += new System.Windows.Forms.DataGridViewCellEventHandler(this.dgvStateChanges_CellEndEdit);
            this.dgvStateChanges.CellMouseDoubleClick += new System.Windows.Forms.DataGridViewCellMouseEventHandler(this.dgvStateChanges_CellMouseDoubleClick);
            this.dgvStateChanges.CellValidating += new System.Windows.Forms.DataGridViewCellValidatingEventHandler(this.dgvStateChanges_CellValidating);
            this.dgvStateChanges.UserDeletedRow += new System.Windows.Forms.DataGridViewRowEventHandler(this.dgvStateChanges_UserDeletedRow);
            // 
            // columnStateName
            // 
            this.columnStateName.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.columnStateName.DataPropertyName = "StateName";
            this.columnStateName.HeaderText = "State name";
            this.columnStateName.Name = "columnStateName";
            this.columnStateName.ReadOnly = true;
            // 
            // columnStateId
            // 
            this.columnStateId.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.columnStateId.DataPropertyName = "StateId";
            this.columnStateId.FillWeight = 50F;
            this.columnStateId.HeaderText = "State ID";
            this.columnStateId.Name = "columnStateId";
            // 
            // columnLowFrame
            // 
            this.columnLowFrame.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.columnLowFrame.DataPropertyName = "LowFrame";
            this.columnLowFrame.FillWeight = 50F;
            this.columnLowFrame.HeaderText = "Low frame";
            this.columnLowFrame.Name = "columnLowFrame";
            // 
            // columnHighFrame
            // 
            this.columnHighFrame.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.columnHighFrame.DataPropertyName = "HighFrame";
            this.columnHighFrame.FillWeight = 50F;
            this.columnHighFrame.HeaderText = "High frame";
            this.columnHighFrame.Name = "columnHighFrame";
            // 
            // columnNextAnimation
            // 
            this.columnNextAnimation.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.columnNextAnimation.DataPropertyName = "NextAnimation";
            this.columnNextAnimation.FillWeight = 50F;
            this.columnNextAnimation.HeaderText = "Next anim";
            this.columnNextAnimation.Name = "columnNextAnimation";
            // 
            // columnNextFrame
            // 
            this.columnNextFrame.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.columnNextFrame.DataPropertyName = "NextFrame";
            this.columnNextFrame.FillWeight = 50F;
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
            this.darkSectionPanel1.Controls.Add(this.butPlayStateChange);
            this.darkSectionPanel1.Controls.Add(this.dgvStateChanges);
            this.darkSectionPanel1.Controls.Add(this.dgvControls);
            this.darkSectionPanel1.Location = new System.Drawing.Point(5, 5);
            this.darkSectionPanel1.Name = "darkSectionPanel1";
            this.darkSectionPanel1.SectionHeader = null;
            this.darkSectionPanel1.Size = new System.Drawing.Size(570, 221);
            this.darkSectionPanel1.TabIndex = 52;
            // 
            // butPlayStateChange
            // 
            this.butPlayStateChange.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.butPlayStateChange.Image = global::WadTool.Properties.Resources.actions_play_16;
            this.butPlayStateChange.Location = new System.Drawing.Point(537, 191);
            this.butPlayStateChange.Name = "butPlayStateChange";
            this.butPlayStateChange.Size = new System.Drawing.Size(27, 24);
            this.butPlayStateChange.TabIndex = 50;
            this.toolTip1.SetToolTip(this.butPlayStateChange, "Play state change in chain mode");
            this.butPlayStateChange.Click += new System.EventHandler(this.butPlayStateChange_Click);
            // 
            // lblStateChangeAnnouncement
            // 
            this.lblStateChangeAnnouncement.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lblStateChangeAnnouncement.ForeColor = System.Drawing.Color.Gray;
            this.lblStateChangeAnnouncement.Location = new System.Drawing.Point(8, 237);
            this.lblStateChangeAnnouncement.Name = "lblStateChangeAnnouncement";
            this.lblStateChangeAnnouncement.Size = new System.Drawing.Size(393, 13);
            this.lblStateChangeAnnouncement.TabIndex = 53;
            this.lblStateChangeAnnouncement.Text = "Pending state change...";
            this.lblStateChangeAnnouncement.Visible = false;
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
            this.dgvControls.Size = new System.Drawing.Size(27, 178);
            this.dgvControls.TabIndex = 49;
            // 
            // FormStateChangesEditor
            // 
            this.AcceptButton = this.btOk;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.btCancel;
            this.ClientSize = new System.Drawing.Size(580, 260);
            this.Controls.Add(this.lblStateChangeAnnouncement);
            this.Controls.Add(this.darkSectionPanel1);
            this.Controls.Add(this.btCancel);
            this.Controls.Add(this.btOk);
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
        private DarkUI.Controls.DarkButton butPlayStateChange;
        private DarkUI.Controls.DarkLabel lblStateChangeAnnouncement;
        private System.Windows.Forms.ToolTip toolTip1;
        private System.Windows.Forms.DataGridViewTextBoxColumn columnStateName;
        private System.Windows.Forms.DataGridViewTextBoxColumn columnStateId;
        private System.Windows.Forms.DataGridViewTextBoxColumn columnLowFrame;
        private System.Windows.Forms.DataGridViewTextBoxColumn columnHighFrame;
        private System.Windows.Forms.DataGridViewTextBoxColumn columnNextAnimation;
        private System.Windows.Forms.DataGridViewTextBoxColumn columnNextFrame;
    }
}