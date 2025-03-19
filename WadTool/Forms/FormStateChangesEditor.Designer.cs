﻿namespace WadTool
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
            nudBlendEndFrame = new DarkUI.Controls.DarkNumericUpDown();
            nudBlendFrameCount = new DarkUI.Controls.DarkNumericUpDown();
            butApply = new DarkUI.Controls.DarkButton();
            darkGroupBox1 = new DarkUI.Controls.DarkGroupBox();
            darkGroupBox2 = new DarkUI.Controls.DarkGroupBox();
            darkLabel1 = new DarkUI.Controls.DarkLabel();
            cbBlendPreset = new DarkUI.Controls.DarkComboBox();
            darkLabel3 = new DarkUI.Controls.DarkLabel();
            darkLabel2 = new DarkUI.Controls.DarkLabel();
            bezierCurveEditor = new Controls.BezierCurveEditor();
            ((System.ComponentModel.ISupportInitialize)dgvStateChanges).BeginInit();
            ((System.ComponentModel.ISupportInitialize)nudBlendEndFrame).BeginInit();
            ((System.ComponentModel.ISupportInitialize)nudBlendFrameCount).BeginInit();
            darkGroupBox1.SuspendLayout();
            darkGroupBox2.SuspendLayout();
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
            dgvStateChanges.Size = new System.Drawing.Size(489, 292);
            dgvStateChanges.TabIndex = 48;
            dgvStateChanges.CellFormattingSafe += dgvStateChanges_CellFormattingSafe;
            dgvStateChanges.CellEndEdit += dgvStateChanges_CellEndEdit;
            dgvStateChanges.CellMouseDoubleClick += dgvStateChanges_CellMouseDoubleClick;
            dgvStateChanges.CellValidating += dgvStateChanges_CellValidating;
            dgvStateChanges.SelectionChanged += dgvStateChanges_SelectionChanged;
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
            btCancel.Location = new System.Drawing.Point(721, 336);
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
            btOk.Location = new System.Drawing.Point(634, 336);
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
            butPlayStateChange.Location = new System.Drawing.Point(502, 290);
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
            dgvControls.Location = new System.Drawing.Point(502, 22);
            dgvControls.Margin = new System.Windows.Forms.Padding(6, 3, 6, 3);
            dgvControls.MinimumSize = new System.Drawing.Size(33, 32);
            dgvControls.Name = "dgvControls";
            dgvControls.Size = new System.Drawing.Size(33, 262);
            dgvControls.TabIndex = 49;
            // 
            // lblStateChangeAnnouncement
            // 
            lblStateChangeAnnouncement.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            lblStateChangeAnnouncement.ForeColor = System.Drawing.Color.Gray;
            lblStateChangeAnnouncement.Location = new System.Drawing.Point(8, 341);
            lblStateChangeAnnouncement.Name = "lblStateChangeAnnouncement";
            lblStateChangeAnnouncement.Size = new System.Drawing.Size(534, 13);
            lblStateChangeAnnouncement.TabIndex = 53;
            lblStateChangeAnnouncement.Text = "Pending state change...";
            lblStateChangeAnnouncement.Visible = false;
            // 
            // nudBlendEndFrame
            // 
            nudBlendEndFrame.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            nudBlendEndFrame.IncrementAlternate = new decimal(new int[] { 10, 0, 0, 65536 });
            nudBlendEndFrame.Location = new System.Drawing.Point(190, 46);
            nudBlendEndFrame.LoopValues = false;
            nudBlendEndFrame.Maximum = new decimal(new int[] { 255, 0, 0, 0 });
            nudBlendEndFrame.Name = "nudBlendEndFrame";
            nudBlendEndFrame.Size = new System.Drawing.Size(57, 22);
            nudBlendEndFrame.TabIndex = 103;
            toolTip1.SetToolTip(nudBlendEndFrame, "Ending frame in the transition to the next animation");
            nudBlendEndFrame.ValueChanged += nudBlendEndFrame_ValueChanged;
            // 
            // nudBlendFrameCount
            // 
            nudBlendFrameCount.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            nudBlendFrameCount.IncrementAlternate = new decimal(new int[] { 10, 0, 0, 65536 });
            nudBlendFrameCount.Location = new System.Drawing.Point(190, 19);
            nudBlendFrameCount.LoopValues = false;
            nudBlendFrameCount.Maximum = new decimal(new int[] { 255, 0, 0, 0 });
            nudBlendFrameCount.Name = "nudBlendFrameCount";
            nudBlendFrameCount.Size = new System.Drawing.Size(57, 22);
            nudBlendFrameCount.TabIndex = 100;
            toolTip1.SetToolTip(nudBlendFrameCount, "Blending duration to the next animation in frames");
            nudBlendFrameCount.ValueChanged += nudBlendFrameCount_ValueChanged;
            // 
            // butApply
            // 
            butApply.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
            butApply.Checked = false;
            butApply.Location = new System.Drawing.Point(548, 336);
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
            darkGroupBox1.Size = new System.Drawing.Size(537, 321);
            darkGroupBox1.TabIndex = 104;
            darkGroupBox1.TabStop = false;
            darkGroupBox1.Text = "State change editor";
            // 
            // darkGroupBox2
            // 
            darkGroupBox2.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
            darkGroupBox2.Controls.Add(darkLabel1);
            darkGroupBox2.Controls.Add(cbBlendPreset);
            darkGroupBox2.Controls.Add(nudBlendEndFrame);
            darkGroupBox2.Controls.Add(nudBlendFrameCount);
            darkGroupBox2.Controls.Add(darkLabel3);
            darkGroupBox2.Controls.Add(darkLabel2);
            darkGroupBox2.Controls.Add(bezierCurveEditor);
            darkGroupBox2.Location = new System.Drawing.Point(548, 9);
            darkGroupBox2.Name = "darkGroupBox2";
            darkGroupBox2.Size = new System.Drawing.Size(254, 321);
            darkGroupBox2.TabIndex = 105;
            darkGroupBox2.TabStop = false;
            darkGroupBox2.Text = "Animation blending";
            // 
            // darkLabel1
            // 
            darkLabel1.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
            darkLabel1.AutoSize = true;
            darkLabel1.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
            darkLabel1.Location = new System.Drawing.Point(6, 295);
            darkLabel1.Name = "darkLabel1";
            darkLabel1.Size = new System.Drawing.Size(41, 13);
            darkLabel1.TabIndex = 106;
            darkLabel1.Text = "Preset:";
            // 
            // cbBlendPreset
            // 
            cbBlendPreset.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            cbBlendPreset.FormattingEnabled = true;
            cbBlendPreset.Items.AddRange(new object[] { "Linear", "Ease In", "Ease Out", "Ease In and Out" });
            cbBlendPreset.Location = new System.Drawing.Point(53, 291);
            cbBlendPreset.Name = "cbBlendPreset";
            cbBlendPreset.Size = new System.Drawing.Size(194, 23);
            cbBlendPreset.TabIndex = 105;
            cbBlendPreset.SelectedIndexChanged += cbBlendPreset_SelectedIndexChanged;
            // 
            // darkLabel3
            // 
            darkLabel3.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
            darkLabel3.Location = new System.Drawing.Point(6, 48);
            darkLabel3.Name = "darkLabel3";
            darkLabel3.Size = new System.Drawing.Size(201, 13);
            darkLabel3.TabIndex = 104;
            darkLabel3.Text = "Next anim transition end frame:";
            // 
            // darkLabel2
            // 
            darkLabel2.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
            darkLabel2.Location = new System.Drawing.Point(6, 21);
            darkLabel2.Name = "darkLabel2";
            darkLabel2.Size = new System.Drawing.Size(210, 13);
            darkLabel2.TabIndex = 101;
            darkLabel2.Text = "Next anim transition duration:";
            // 
            // bezierCurveEditor
            // 
            bezierCurveEditor.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            bezierCurveEditor.Location = new System.Drawing.Point(6, 74);
            bezierCurveEditor.Margin = new System.Windows.Forms.Padding(5, 3, 5, 3);
            bezierCurveEditor.Name = "bezierCurveEditor";
            bezierCurveEditor.Size = new System.Drawing.Size(241, 211);
            bezierCurveEditor.TabIndex = 102;
            bezierCurveEditor.ValueChanged += bezierCurveEditor_ValueChanged;
            // 
            // FormStateChangesEditor
            // 
            AcceptButton = btOk;
            AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            CancelButton = btCancel;
            ClientSize = new System.Drawing.Size(807, 364);
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
            ((System.ComponentModel.ISupportInitialize)nudBlendEndFrame).EndInit();
            ((System.ComponentModel.ISupportInitialize)nudBlendFrameCount).EndInit();
            darkGroupBox1.ResumeLayout(false);
            darkGroupBox2.ResumeLayout(false);
            darkGroupBox2.PerformLayout();
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
        private DarkUI.Controls.DarkCheckBox cbRotR;
        private DarkUI.Controls.DarkCheckBox cbPosX;
        private DarkUI.Controls.DarkCheckBox cbRotP;
        private DarkUI.Controls.DarkCheckBox cbPosY;
        private DarkUI.Controls.DarkCheckBox cbRotY;
        private Controls.BezierCurveEditor bezierCurveEditor;
        private DarkUI.Controls.DarkNumericUpDown nudBlendFrameCount;
        private DarkUI.Controls.DarkLabel darkLabel2;
        private DarkUI.Controls.DarkNumericUpDown nudBlendEndFrame;
        private DarkUI.Controls.DarkLabel darkLabel3;
        private DarkUI.Controls.DarkLabel darkLabel1;
        private DarkUI.Controls.DarkComboBox cbBlendPreset;
    }
}