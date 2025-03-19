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
            components = new System.ComponentModel.Container();
            dgvStateChanges = new DarkUI.Controls.DarkDataGridView();
            columnStateName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            columnStateId = new System.Windows.Forms.DataGridViewTextBoxColumn();
            columnLowFrame = new System.Windows.Forms.DataGridViewTextBoxColumn();
            columnHighFrame = new System.Windows.Forms.DataGridViewTextBoxColumn();
            columnNextAnimation = new System.Windows.Forms.DataGridViewTextBoxColumn();
            columnNextFrame = new System.Windows.Forms.DataGridViewTextBoxColumn();
            btCancel = new DarkUI.Controls.DarkButton();
            btOk = new DarkUI.Controls.DarkButton();
            butPlayStateChange = new DarkUI.Controls.DarkButton();
            dgvControls = new TombLib.Controls.DarkDataGridViewControls();
            lblStateChangeAnnouncement = new DarkUI.Controls.DarkLabel();
            toolTip1 = new System.Windows.Forms.ToolTip(components);
            nudStateChangeEndFrame = new DarkUI.Controls.DarkNumericUpDown();
            nudStateChangeTransDuration = new DarkUI.Controls.DarkNumericUpDown();
            cbLooped = new DarkUI.Controls.DarkCheckBox();
            nudAnimTransDuration = new DarkUI.Controls.DarkNumericUpDown();
            butApply = new DarkUI.Controls.DarkButton();
            darkGroupBox1 = new DarkUI.Controls.DarkGroupBox();
            darkGroupBox2 = new DarkUI.Controls.DarkGroupBox();
            darkSectionPanel1 = new DarkUI.Controls.DarkSectionPanel();
            darkLabel3 = new DarkUI.Controls.DarkLabel();
            bcStateChange = new Controls.BezierCurveEditor();
            darkLabel2 = new DarkUI.Controls.DarkLabel();
            darkSectionPanel2 = new DarkUI.Controls.DarkSectionPanel();
            darkGroupBox3 = new DarkUI.Controls.DarkGroupBox();
            darkLabel1 = new DarkUI.Controls.DarkLabel();
            darkLabel29 = new DarkUI.Controls.DarkLabel();
            darkLabel28 = new DarkUI.Controls.DarkLabel();
            cbRotR = new DarkUI.Controls.DarkCheckBox();
            cbPosZ = new DarkUI.Controls.DarkCheckBox();
            cbPosX = new DarkUI.Controls.DarkCheckBox();
            cbRotY = new DarkUI.Controls.DarkCheckBox();
            cbRotP = new DarkUI.Controls.DarkCheckBox();
            cbPosY = new DarkUI.Controls.DarkCheckBox();
            darkLabel4 = new DarkUI.Controls.DarkLabel();
            bcAnimation = new Controls.BezierCurveEditor();
            ((System.ComponentModel.ISupportInitialize)dgvStateChanges).BeginInit();
            ((System.ComponentModel.ISupportInitialize)nudStateChangeEndFrame).BeginInit();
            ((System.ComponentModel.ISupportInitialize)nudStateChangeTransDuration).BeginInit();
            ((System.ComponentModel.ISupportInitialize)nudAnimTransDuration).BeginInit();
            darkGroupBox1.SuspendLayout();
            darkGroupBox2.SuspendLayout();
            darkSectionPanel1.SuspendLayout();
            darkSectionPanel2.SuspendLayout();
            darkGroupBox3.SuspendLayout();
            SuspendLayout();
            // 
            // dgvStateChanges
            // 
            dgvStateChanges.AllowUserToAddRows = false;
            dgvStateChanges.AllowUserToDragDropRows = false;
            dgvStateChanges.AllowUserToPasteCells = false;
            dgvStateChanges.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            dgvStateChanges.AutoGenerateColumns = false;
            dgvStateChanges.ColumnHeadersHeight = 17;
            dgvStateChanges.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] { columnStateName, columnStateId, columnLowFrame, columnHighFrame, columnNextAnimation, columnNextFrame });
            dgvStateChanges.ForegroundColor = System.Drawing.Color.FromArgb(220, 220, 220);
            dgvStateChanges.Location = new System.Drawing.Point(7, 22);
            dgvStateChanges.Name = "dgvStateChanges";
            dgvStateChanges.RowHeadersWidth = 40;
            dgvStateChanges.RowTemplate.Height = 16;
            dgvStateChanges.ShowCellErrors = false;
            dgvStateChanges.Size = new System.Drawing.Size(638, 216);
            dgvStateChanges.TabIndex = 48;
            dgvStateChanges.CellFormattingSafe += dgvStateChanges_CellFormattingSafe;
            dgvStateChanges.CellEndEdit += dgvStateChanges_CellEndEdit;
            dgvStateChanges.CellMouseDoubleClick += dgvStateChanges_CellMouseDoubleClick;
            dgvStateChanges.CellValidating += dgvStateChanges_CellValidating;
            dgvStateChanges.UserDeletedRow += dgvStateChanges_UserDeletedRow;
            // 
            // columnStateName
            // 
            columnStateName.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            columnStateName.DataPropertyName = "StateName";
            columnStateName.HeaderText = "State name";
            columnStateName.Name = "columnStateName";
            columnStateName.ReadOnly = true;
            // 
            // columnStateId
            // 
            columnStateId.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            columnStateId.DataPropertyName = "StateId";
            columnStateId.FillWeight = 50F;
            columnStateId.HeaderText = "State ID";
            columnStateId.Name = "columnStateId";
            // 
            // columnLowFrame
            // 
            columnLowFrame.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            columnLowFrame.DataPropertyName = "LowFrame";
            columnLowFrame.FillWeight = 50F;
            columnLowFrame.HeaderText = "Low frame";
            columnLowFrame.Name = "columnLowFrame";
            // 
            // columnHighFrame
            // 
            columnHighFrame.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            columnHighFrame.DataPropertyName = "HighFrame";
            columnHighFrame.FillWeight = 50F;
            columnHighFrame.HeaderText = "High frame";
            columnHighFrame.Name = "columnHighFrame";
            // 
            // columnNextAnimation
            // 
            columnNextAnimation.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            columnNextAnimation.DataPropertyName = "NextAnimation";
            columnNextAnimation.FillWeight = 50F;
            columnNextAnimation.HeaderText = "Next anim";
            columnNextAnimation.Name = "columnNextAnimation";
            // 
            // columnNextFrame
            // 
            columnNextFrame.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            columnNextFrame.DataPropertyName = "NextFrame";
            columnNextFrame.FillWeight = 50F;
            columnNextFrame.HeaderText = "Next frame";
            columnNextFrame.Name = "columnNextFrame";
            // 
            // btCancel
            // 
            btCancel.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
            btCancel.Checked = false;
            btCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            btCancel.Location = new System.Drawing.Point(610, 445);
            btCancel.Name = "btCancel";
            btCancel.Size = new System.Drawing.Size(81, 23);
            btCancel.TabIndex = 50;
            btCancel.Text = "Cancel";
            btCancel.Click += btCancel_Click;
            // 
            // btOk
            // 
            btOk.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
            btOk.Checked = false;
            btOk.Location = new System.Drawing.Point(523, 445);
            btOk.Name = "btOk";
            btOk.Size = new System.Drawing.Size(81, 23);
            btOk.TabIndex = 51;
            btOk.Text = "OK";
            btOk.Click += btOk_Click;
            // 
            // butPlayStateChange
            // 
            butPlayStateChange.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
            butPlayStateChange.Checked = false;
            butPlayStateChange.Image = Properties.Resources.actions_play_16;
            butPlayStateChange.Location = new System.Drawing.Point(651, 214);
            butPlayStateChange.Name = "butPlayStateChange";
            butPlayStateChange.Size = new System.Drawing.Size(28, 24);
            butPlayStateChange.TabIndex = 50;
            toolTip1.SetToolTip(butPlayStateChange, "Play state change in chain mode");
            butPlayStateChange.Click += butPlayStateChange_Click;
            // 
            // dgvControls
            // 
            dgvControls.AlwaysInsertAtZero = false;
            dgvControls.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
            dgvControls.Enabled = false;
            dgvControls.Location = new System.Drawing.Point(651, 22);
            dgvControls.Margin = new System.Windows.Forms.Padding(5, 3, 5, 3);
            dgvControls.MinimumSize = new System.Drawing.Size(28, 28);
            dgvControls.Name = "dgvControls";
            dgvControls.Size = new System.Drawing.Size(28, 186);
            dgvControls.TabIndex = 49;
            // 
            // lblStateChangeAnnouncement
            // 
            lblStateChangeAnnouncement.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            lblStateChangeAnnouncement.ForeColor = System.Drawing.Color.Gray;
            lblStateChangeAnnouncement.Location = new System.Drawing.Point(8, 450);
            lblStateChangeAnnouncement.Name = "lblStateChangeAnnouncement";
            lblStateChangeAnnouncement.Size = new System.Drawing.Size(423, 13);
            lblStateChangeAnnouncement.TabIndex = 53;
            lblStateChangeAnnouncement.Text = "Pending state change...";
            lblStateChangeAnnouncement.Visible = false;
            // 
            // nudStateChangeEndFrame
            // 
            nudStateChangeEndFrame.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            nudStateChangeEndFrame.IncrementAlternate = new decimal(new int[] { 10, 0, 0, 65536 });
            nudStateChangeEndFrame.Location = new System.Drawing.Point(185, 57);
            nudStateChangeEndFrame.LoopValues = false;
            nudStateChangeEndFrame.Maximum = new decimal(new int[] { 255, 0, 0, 0 });
            nudStateChangeEndFrame.Name = "nudStateChangeEndFrame";
            nudStateChangeEndFrame.Size = new System.Drawing.Size(57, 22);
            nudStateChangeEndFrame.TabIndex = 103;
            toolTip1.SetToolTip(nudStateChangeEndFrame, "Ending frame in the transition to the next animation");
            // 
            // nudStateChangeTransDuration
            // 
            nudStateChangeTransDuration.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            nudStateChangeTransDuration.IncrementAlternate = new decimal(new int[] { 10, 0, 0, 65536 });
            nudStateChangeTransDuration.Location = new System.Drawing.Point(185, 30);
            nudStateChangeTransDuration.LoopValues = false;
            nudStateChangeTransDuration.Maximum = new decimal(new int[] { 255, 0, 0, 0 });
            nudStateChangeTransDuration.Name = "nudStateChangeTransDuration";
            nudStateChangeTransDuration.Size = new System.Drawing.Size(57, 22);
            nudStateChangeTransDuration.TabIndex = 100;
            toolTip1.SetToolTip(nudStateChangeTransDuration, "Blending duration to the next animation in frames");
            // 
            // cbLooped
            // 
            cbLooped.AutoSize = true;
            cbLooped.Location = new System.Drawing.Point(71, 27);
            cbLooped.Name = "cbLooped";
            cbLooped.Size = new System.Drawing.Size(65, 17);
            cbLooped.TabIndex = 105;
            cbLooped.Text = "Looped";
            toolTip1.SetToolTip(cbLooped, "To be used when animation is looped on itself");
            // 
            // nudAnimTransDuration
            // 
            nudAnimTransDuration.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            nudAnimTransDuration.IncrementAlternate = new decimal(new int[] { 10, 0, 0, 65536 });
            nudAnimTransDuration.Location = new System.Drawing.Point(170, 30);
            nudAnimTransDuration.LoopValues = false;
            nudAnimTransDuration.Maximum = new decimal(new int[] { 255, 0, 0, 0 });
            nudAnimTransDuration.Name = "nudAnimTransDuration";
            nudAnimTransDuration.Size = new System.Drawing.Size(57, 22);
            nudAnimTransDuration.TabIndex = 97;
            toolTip1.SetToolTip(nudAnimTransDuration, "Blending duration to the next animation in frames");
            // 
            // butApply
            // 
            butApply.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
            butApply.Checked = false;
            butApply.Location = new System.Drawing.Point(437, 445);
            butApply.Name = "butApply";
            butApply.Size = new System.Drawing.Size(80, 23);
            butApply.TabIndex = 102;
            butApply.Text = "Apply";
            butApply.Click += butApply_Click;
            // 
            // darkGroupBox1
            // 
            darkGroupBox1.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            darkGroupBox1.Controls.Add(butPlayStateChange);
            darkGroupBox1.Controls.Add(dgvStateChanges);
            darkGroupBox1.Controls.Add(dgvControls);
            darkGroupBox1.Location = new System.Drawing.Point(5, 9);
            darkGroupBox1.Name = "darkGroupBox1";
            darkGroupBox1.Size = new System.Drawing.Size(686, 245);
            darkGroupBox1.TabIndex = 104;
            darkGroupBox1.TabStop = false;
            darkGroupBox1.Text = "State change editor";
            // 
            // darkGroupBox2
            // 
            darkGroupBox2.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            darkGroupBox2.Controls.Add(darkSectionPanel1);
            darkGroupBox2.Controls.Add(darkSectionPanel2);
            darkGroupBox2.Location = new System.Drawing.Point(6, 261);
            darkGroupBox2.Name = "darkGroupBox2";
            darkGroupBox2.Size = new System.Drawing.Size(685, 178);
            darkGroupBox2.TabIndex = 105;
            darkGroupBox2.TabStop = false;
            darkGroupBox2.Text = "Animation blending";
            // 
            // darkSectionPanel1
            // 
            darkSectionPanel1.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            darkSectionPanel1.Controls.Add(nudStateChangeEndFrame);
            darkSectionPanel1.Controls.Add(darkLabel3);
            darkSectionPanel1.Controls.Add(bcStateChange);
            darkSectionPanel1.Controls.Add(nudStateChangeTransDuration);
            darkSectionPanel1.Controls.Add(darkLabel2);
            darkSectionPanel1.Location = new System.Drawing.Point(428, 21);
            darkSectionPanel1.Name = "darkSectionPanel1";
            darkSectionPanel1.SectionHeader = "Selected state change";
            darkSectionPanel1.Size = new System.Drawing.Size(250, 149);
            darkSectionPanel1.TabIndex = 104;
            // 
            // darkLabel3
            // 
            darkLabel3.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
            darkLabel3.Location = new System.Drawing.Point(5, 59);
            darkLabel3.Name = "darkLabel3";
            darkLabel3.Size = new System.Drawing.Size(201, 13);
            darkLabel3.TabIndex = 104;
            darkLabel3.Text = "Next anim transition end frame:";
            // 
            // bcStateChange
            // 
            bcStateChange.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            bcStateChange.Location = new System.Drawing.Point(6, 85);
            bcStateChange.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            bcStateChange.Name = "bcStateChange";
            bcStateChange.Size = new System.Drawing.Size(236, 57);
            bcStateChange.TabIndex = 102;
            // 
            // darkLabel2
            // 
            darkLabel2.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
            darkLabel2.Location = new System.Drawing.Point(5, 32);
            darkLabel2.Name = "darkLabel2";
            darkLabel2.Size = new System.Drawing.Size(210, 13);
            darkLabel2.TabIndex = 101;
            darkLabel2.Text = "Next anim transition duration:";
            // 
            // darkSectionPanel2
            // 
            darkSectionPanel2.Controls.Add(darkGroupBox3);
            darkSectionPanel2.Controls.Add(nudAnimTransDuration);
            darkSectionPanel2.Controls.Add(darkLabel4);
            darkSectionPanel2.Controls.Add(bcAnimation);
            darkSectionPanel2.Location = new System.Drawing.Point(7, 21);
            darkSectionPanel2.Name = "darkSectionPanel2";
            darkSectionPanel2.SectionHeader = "Current animation";
            darkSectionPanel2.Size = new System.Drawing.Size(415, 149);
            darkSectionPanel2.TabIndex = 103;
            // 
            // darkGroupBox3
            // 
            darkGroupBox3.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            darkGroupBox3.Controls.Add(darkLabel1);
            darkGroupBox3.Controls.Add(cbLooped);
            darkGroupBox3.Controls.Add(darkLabel29);
            darkGroupBox3.Controls.Add(darkLabel28);
            darkGroupBox3.Controls.Add(cbRotR);
            darkGroupBox3.Controls.Add(cbPosZ);
            darkGroupBox3.Controls.Add(cbPosX);
            darkGroupBox3.Controls.Add(cbRotY);
            darkGroupBox3.Controls.Add(cbRotP);
            darkGroupBox3.Controls.Add(cbPosY);
            darkGroupBox3.Location = new System.Drawing.Point(234, 34);
            darkGroupBox3.Name = "darkGroupBox3";
            darkGroupBox3.Size = new System.Drawing.Size(175, 108);
            darkGroupBox3.TabIndex = 99;
            darkGroupBox3.TabStop = false;
            darkGroupBox3.Text = "Root motion";
            // 
            // darkLabel1
            // 
            darkLabel1.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
            darkLabel1.Location = new System.Drawing.Point(8, 28);
            darkLabel1.Name = "darkLabel1";
            darkLabel1.Size = new System.Drawing.Size(62, 13);
            darkLabel1.TabIndex = 108;
            darkLabel1.Text = "Mode:";
            // 
            // darkLabel29
            // 
            darkLabel29.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
            darkLabel29.Location = new System.Drawing.Point(8, 83);
            darkLabel29.Name = "darkLabel29";
            darkLabel29.Size = new System.Drawing.Size(57, 13);
            darkLabel29.TabIndex = 107;
            darkLabel29.Text = "Position:";
            // 
            // darkLabel28
            // 
            darkLabel28.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
            darkLabel28.Location = new System.Drawing.Point(8, 55);
            darkLabel28.Name = "darkLabel28";
            darkLabel28.Size = new System.Drawing.Size(62, 13);
            darkLabel28.TabIndex = 106;
            darkLabel28.Text = "Rotation:";
            // 
            // cbRotR
            // 
            cbRotR.AutoSize = true;
            cbRotR.Location = new System.Drawing.Point(137, 54);
            cbRotR.Name = "cbRotR";
            cbRotR.Size = new System.Drawing.Size(33, 17);
            cbRotR.TabIndex = 104;
            cbRotR.Text = "R";
            // 
            // cbPosZ
            // 
            cbPosZ.AutoSize = true;
            cbPosZ.Location = new System.Drawing.Point(137, 82);
            cbPosZ.Name = "cbPosZ";
            cbPosZ.Size = new System.Drawing.Size(32, 17);
            cbPosZ.TabIndex = 101;
            cbPosZ.Text = "Z";
            // 
            // cbPosX
            // 
            cbPosX.AutoSize = true;
            cbPosX.Location = new System.Drawing.Point(70, 82);
            cbPosX.Name = "cbPosX";
            cbPosX.Size = new System.Drawing.Size(32, 17);
            cbPosX.TabIndex = 99;
            cbPosX.Text = "X";
            // 
            // cbRotY
            // 
            cbRotY.AutoSize = true;
            cbRotY.Location = new System.Drawing.Point(71, 54);
            cbRotY.Name = "cbRotY";
            cbRotY.Size = new System.Drawing.Size(31, 17);
            cbRotY.TabIndex = 102;
            cbRotY.Text = "Y";
            // 
            // cbRotP
            // 
            cbRotP.AutoSize = true;
            cbRotP.Location = new System.Drawing.Point(104, 54);
            cbRotP.Name = "cbRotP";
            cbRotP.Size = new System.Drawing.Size(32, 17);
            cbRotP.TabIndex = 103;
            cbRotP.Text = "P";
            // 
            // cbPosY
            // 
            cbPosY.AutoSize = true;
            cbPosY.Location = new System.Drawing.Point(104, 82);
            cbPosY.Name = "cbPosY";
            cbPosY.Size = new System.Drawing.Size(31, 17);
            cbPosY.TabIndex = 100;
            cbPosY.Text = "Y";
            // 
            // darkLabel4
            // 
            darkLabel4.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
            darkLabel4.Location = new System.Drawing.Point(6, 32);
            darkLabel4.Name = "darkLabel4";
            darkLabel4.Size = new System.Drawing.Size(177, 13);
            darkLabel4.TabIndex = 98;
            darkLabel4.Text = "Next anim transition duration:";
            // 
            // bcAnimation
            // 
            bcAnimation.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            bcAnimation.Location = new System.Drawing.Point(6, 57);
            bcAnimation.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            bcAnimation.Name = "bcAnimation";
            bcAnimation.Size = new System.Drawing.Size(221, 85);
            bcAnimation.TabIndex = 0;
            // 
            // FormStateChangesEditor
            // 
            AcceptButton = btOk;
            AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            CancelButton = btCancel;
            ClientSize = new System.Drawing.Size(696, 473);
            Controls.Add(darkGroupBox2);
            Controls.Add(darkGroupBox1);
            Controls.Add(butApply);
            Controls.Add(lblStateChangeAnnouncement);
            Controls.Add(btCancel);
            Controls.Add(btOk);
            MinimizeBox = false;
            MinimumSize = new System.Drawing.Size(596, 242);
            Name = "FormStateChangesEditor";
            ShowIcon = false;
            ShowInTaskbar = false;
            SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            Text = "State changes";
            ((System.ComponentModel.ISupportInitialize)dgvStateChanges).EndInit();
            ((System.ComponentModel.ISupportInitialize)nudStateChangeEndFrame).EndInit();
            ((System.ComponentModel.ISupportInitialize)nudStateChangeTransDuration).EndInit();
            ((System.ComponentModel.ISupportInitialize)nudAnimTransDuration).EndInit();
            darkGroupBox1.ResumeLayout(false);
            darkGroupBox2.ResumeLayout(false);
            darkSectionPanel1.ResumeLayout(false);
            darkSectionPanel2.ResumeLayout(false);
            darkGroupBox3.ResumeLayout(false);
            darkGroupBox3.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        private TombLib.Controls.DarkDataGridViewControls dgvControls;
        private DarkUI.Controls.DarkDataGridView dgvStateChanges;
        private DarkUI.Controls.DarkButton btCancel;
        private DarkUI.Controls.DarkButton btOk;
        private DarkUI.Controls.DarkButton butPlayStateChange;
        private DarkUI.Controls.DarkLabel lblStateChangeAnnouncement;
        private System.Windows.Forms.ToolTip toolTip1;
        private System.Windows.Forms.DataGridViewTextBoxColumn columnStateName;
        private System.Windows.Forms.DataGridViewTextBoxColumn columnStateId;
        private System.Windows.Forms.DataGridViewTextBoxColumn columnLowFrame;
        private System.Windows.Forms.DataGridViewTextBoxColumn columnHighFrame;
        private System.Windows.Forms.DataGridViewTextBoxColumn columnNextAnimation;
        private System.Windows.Forms.DataGridViewTextBoxColumn columnNextFrame;
        private DarkUI.Controls.DarkButton butApply;
        private DarkUI.Controls.DarkGroupBox darkGroupBox1;
        private DarkUI.Controls.DarkGroupBox darkGroupBox2;
        private DarkUI.Controls.DarkSectionPanel darkSectionPanel2;
        private DarkUI.Controls.DarkSectionPanel darkSectionPanel1;
        private Controls.BezierCurveEditor bcAnimation;
        private DarkUI.Controls.DarkNumericUpDown nudAnimTransDuration;
        private DarkUI.Controls.DarkLabel darkLabel4;
        private DarkUI.Controls.DarkCheckBox cbLooped;
        private DarkUI.Controls.DarkCheckBox cbRotR;
        private DarkUI.Controls.DarkCheckBox cbPosX;
        private DarkUI.Controls.DarkCheckBox cbRotP;
        private DarkUI.Controls.DarkCheckBox cbPosY;
        private DarkUI.Controls.DarkCheckBox cbRotY;
        private DarkUI.Controls.DarkCheckBox cbPosZ;
        private DarkUI.Controls.DarkGroupBox darkGroupBox3;
        private DarkUI.Controls.DarkLabel darkLabel1;
        private DarkUI.Controls.DarkLabel darkLabel29;
        private DarkUI.Controls.DarkLabel darkLabel28;
        private Controls.BezierCurveEditor bcStateChange;
        private DarkUI.Controls.DarkNumericUpDown nudStateChangeTransDuration;
        private DarkUI.Controls.DarkLabel darkLabel2;
        private DarkUI.Controls.DarkNumericUpDown nudStateChangeEndFrame;
        private DarkUI.Controls.DarkLabel darkLabel3;
    }
}