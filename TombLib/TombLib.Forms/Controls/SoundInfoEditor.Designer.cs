namespace TombLib.Controls
{
    partial class SoundInfoEditor
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

        #region Component Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.toolTip = new System.Windows.Forms.ToolTip(this.components);
            this.cbGlobal = new DarkUI.Controls.DarkCheckBox();
            this.numericVolume = new DarkUI.Controls.DarkNumericUpDown();
            this.cbDisablePanning = new DarkUI.Controls.DarkCheckBox();
            this.cbRandomizeVolume = new DarkUI.Controls.DarkCheckBox();
            this.cbRandomizePitch = new DarkUI.Controls.DarkCheckBox();
            this.numericChance = new DarkUI.Controls.DarkNumericUpDown();
            this.numericPitch = new DarkUI.Controls.DarkNumericUpDown();
            this.numericRange = new DarkUI.Controls.DarkNumericUpDown();
            this.butClipboardPaste = new DarkUI.Controls.DarkButton();
            this.butClipboardCopy = new DarkUI.Controls.DarkButton();
            this.butResetToDefaults = new DarkUI.Controls.DarkButton();
            this.cbIndexed = new DarkUI.Controls.DarkCheckBox();
            this.colSampleName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.picDisabledOverlay = new System.Windows.Forms.PictureBox();
            this.butMoveDown = new DarkUI.Controls.DarkButton();
            this.butMoveUp = new DarkUI.Controls.DarkButton();
            this.butDeleteSample = new DarkUI.Controls.DarkButton();
            this.butAddSample = new DarkUI.Controls.DarkButton();
            this.dgvSamples = new DarkUI.Controls.DarkDataGridView();
            this.SamplePathColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.SampleFoundPathColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.tbID = new DarkUI.Controls.DarkTextBox();
            this.darkLabel9 = new DarkUI.Controls.DarkLabel();
            this.darkGroupBox1 = new DarkUI.Controls.DarkGroupBox();
            this.lblModeTooltip = new DarkUI.Controls.DarkLabel();
            this.darkLabel7 = new DarkUI.Controls.DarkLabel();
            this.darkLabel2 = new DarkUI.Controls.DarkLabel();
            this.numericChanceLabel = new DarkUI.Controls.DarkLabel();
            this.darkLabel3 = new DarkUI.Controls.DarkLabel();
            this.numericRangeLabel = new DarkUI.Controls.DarkLabel();
            this.darkLabel4 = new DarkUI.Controls.DarkLabel();
            this.numericPitchLabel = new DarkUI.Controls.DarkLabel();
            this.darkLabel5 = new DarkUI.Controls.DarkLabel();
            this.numericVolumeLabel = new DarkUI.Controls.DarkLabel();
            this.darkLabel6 = new DarkUI.Controls.DarkLabel();
            this.comboLoop = new DarkUI.Controls.DarkComboBox();
            this.butPlayPreview = new DarkUI.Controls.DarkButton();
            this.tbName = new DarkUI.Controls.DarkTextBox();
            this.tbNameLabel = new DarkUI.Controls.DarkLabel();
            this.butBrowse = new DarkUI.Controls.DarkButton();
            ((System.ComponentModel.ISupportInitialize)(this.numericVolume)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericChance)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericPitch)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericRange)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.picDisabledOverlay)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgvSamples)).BeginInit();
            this.darkGroupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // toolTip
            // 
            this.toolTip.AutoPopDelay = 30000;
            this.toolTip.InitialDelay = 500;
            this.toolTip.ReshowDelay = 100;
            // 
            // cbGlobal
            // 
            this.cbGlobal.Location = new System.Drawing.Point(108, 4);
            this.cbGlobal.Name = "cbGlobal";
            this.cbGlobal.Size = new System.Drawing.Size(64, 17);
            this.cbGlobal.TabIndex = 33;
            this.cbGlobal.Text = "Global";
            this.toolTip.SetToolTip(this.cbGlobal, "Always include sound when Tomb Editor autodetect option is used");
            this.cbGlobal.CheckedChanged += new System.EventHandler(this.OnSoundInfoChanged);
            // 
            // numericVolume
            // 
            this.numericVolume.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.numericVolume.IncrementAlternate = new decimal(new int[] {
            5,
            0,
            0,
            0});
            this.numericVolume.Location = new System.Drawing.Point(78, 6);
            this.numericVolume.LoopValues = false;
            this.numericVolume.Name = "numericVolume";
            this.numericVolume.Size = new System.Drawing.Size(68, 22);
            this.numericVolume.TabIndex = 4;
            this.toolTip.SetToolTip(this.numericVolume, "Volume in percent of the volume used in the sample.");
            this.numericVolume.ValueChanged += new System.EventHandler(this.OnSoundInfoChanged);
            // 
            // cbDisablePanning
            // 
            this.cbDisablePanning.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.cbDisablePanning.CheckAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.cbDisablePanning.Location = new System.Drawing.Point(255, 33);
            this.cbDisablePanning.Name = "cbDisablePanning";
            this.cbDisablePanning.Size = new System.Drawing.Size(140, 17);
            this.cbDisablePanning.TabIndex = 13;
            this.cbDisablePanning.Text = "Disable 3D positioning";
            this.toolTip.SetToolTip(this.cbDisablePanning, "Disable 3D environment for this sound.");
            this.cbDisablePanning.CheckedChanged += new System.EventHandler(this.OnSoundInfoChanged);
            // 
            // cbRandomizeVolume
            // 
            this.cbRandomizeVolume.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.cbRandomizeVolume.CheckAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.cbRandomizeVolume.Location = new System.Drawing.Point(333, 7);
            this.cbRandomizeVolume.Name = "cbRandomizeVolume";
            this.cbRandomizeVolume.Size = new System.Drawing.Size(62, 17);
            this.cbRandomizeVolume.TabIndex = 6;
            this.cbRandomizeVolume.Text = "volume";
            this.toolTip.SetToolTip(this.cbRandomizeVolume, "Slightly vary the volume for each playback of the sound info (around 12%).");
            this.cbRandomizeVolume.CheckedChanged += new System.EventHandler(this.OnSoundInfoChanged);
            // 
            // cbRandomizePitch
            // 
            this.cbRandomizePitch.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.cbRandomizePitch.CheckAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.cbRandomizePitch.Location = new System.Drawing.Point(272, 7);
            this.cbRandomizePitch.Name = "cbRandomizePitch";
            this.cbRandomizePitch.Size = new System.Drawing.Size(59, 17);
            this.cbRandomizePitch.TabIndex = 10;
            this.cbRandomizePitch.Text = "pitch";
            this.toolTip.SetToolTip(this.cbRandomizePitch, "Slightly vary the pitch for each playback of the sound info (around 10%).");
            this.cbRandomizePitch.CheckedChanged += new System.EventHandler(this.OnSoundInfoChanged);
            // 
            // numericChance
            // 
            this.numericChance.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.numericChance.IncrementAlternate = new decimal(new int[] {
            10,
            0,
            0,
            65536});
            this.numericChance.Location = new System.Drawing.Point(78, 84);
            this.numericChance.LoopValues = false;
            this.numericChance.Name = "numericChance";
            this.numericChance.Size = new System.Drawing.Size(68, 22);
            this.numericChance.TabIndex = 15;
            this.toolTip.SetToolTip(this.numericChance, "Probability that any sample will play when triggered.");
            this.numericChance.ValueChanged += new System.EventHandler(this.OnSoundInfoChanged);
            // 
            // numericPitch
            // 
            this.numericPitch.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.numericPitch.IncrementAlternate = new decimal(new int[] {
            10,
            0,
            0,
            65536});
            this.numericPitch.Location = new System.Drawing.Point(78, 32);
            this.numericPitch.LoopValues = false;
            this.numericPitch.Minimum = new decimal(new int[] {
            100,
            0,
            0,
            -2147483648});
            this.numericPitch.Name = "numericPitch";
            this.numericPitch.Size = new System.Drawing.Size(68, 22);
            this.numericPitch.TabIndex = 8;
            this.toolTip.SetToolTip(this.numericPitch, "Pitch of the sound. Value is relative.");
            this.numericPitch.ValueChanged += new System.EventHandler(this.OnSoundInfoChanged);
            // 
            // numericRange
            // 
            this.numericRange.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.numericRange.IncrementAlternate = new decimal(new int[] {
            10,
            0,
            0,
            65536});
            this.numericRange.Location = new System.Drawing.Point(78, 58);
            this.numericRange.LoopValues = false;
            this.numericRange.Maximum = new decimal(new int[] {
            255,
            0,
            0,
            0});
            this.numericRange.Name = "numericRange";
            this.numericRange.Size = new System.Drawing.Size(68, 22);
            this.numericRange.TabIndex = 12;
            this.toolTip.SetToolTip(this.numericRange, "Range in blocks from where the sample can be heard relative to where it was trigg" +
        "ered.");
            this.numericRange.ValueChanged += new System.EventHandler(this.OnSoundInfoChanged);
            // 
            // butClipboardPaste
            // 
            this.butClipboardPaste.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.butClipboardPaste.Checked = false;
            this.butClipboardPaste.Image = global::TombLib.Properties.Resources.general_clipboard_161;
            this.butClipboardPaste.Location = new System.Drawing.Point(350, 31);
            this.butClipboardPaste.Name = "butClipboardPaste";
            this.butClipboardPaste.Size = new System.Drawing.Size(22, 22);
            this.butClipboardPaste.TabIndex = 22;
            this.toolTip.SetToolTip(this.butClipboardPaste, "Paste all settings and samples from the clipboard. (Must have been copied before." +
        ")");
            this.butClipboardPaste.Click += new System.EventHandler(this.butClipboardPaste_Click);
            // 
            // butClipboardCopy
            // 
            this.butClipboardCopy.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.butClipboardCopy.Checked = false;
            this.butClipboardCopy.Image = global::TombLib.Properties.Resources.general_copy_16;
            this.butClipboardCopy.Location = new System.Drawing.Point(322, 31);
            this.butClipboardCopy.Name = "butClipboardCopy";
            this.butClipboardCopy.Size = new System.Drawing.Size(22, 22);
            this.butClipboardCopy.TabIndex = 22;
            this.toolTip.SetToolTip(this.butClipboardCopy, "Copy all settings and samples into the clipboard.");
            this.butClipboardCopy.Click += new System.EventHandler(this.butClipboardCopy_Click);
            // 
            // butResetToDefaults
            // 
            this.butResetToDefaults.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.butResetToDefaults.Checked = false;
            this.butResetToDefaults.Image = global::TombLib.Properties.Resources.general_undo_16;
            this.butResetToDefaults.Location = new System.Drawing.Point(378, 31);
            this.butResetToDefaults.Name = "butResetToDefaults";
            this.butResetToDefaults.Size = new System.Drawing.Size(22, 22);
            this.butResetToDefaults.TabIndex = 104;
            this.toolTip.SetToolTip(this.butResetToDefaults, "Reset to defaults");
            this.butResetToDefaults.Click += new System.EventHandler(this.butResetToDefaults_Click);
            // 
            // cbIndexed
            // 
            this.cbIndexed.Location = new System.Drawing.Point(173, 4);
            this.cbIndexed.Name = "cbIndexed";
            this.cbIndexed.Size = new System.Drawing.Size(143, 17);
            this.cbIndexed.TabIndex = 105;
            this.cbIndexed.Text = "Include for MAIN.SFX";
            this.toolTip.SetToolTip(this.cbIndexed, "Include this sound for TR2-3 MAIN.SFX file");
            // 
            // colSampleName
            // 
            this.colSampleName.HeaderText = "Name";
            this.colSampleName.Name = "colSampleName";
            this.colSampleName.Width = 150;
            // 
            // picDisabledOverlay
            // 
            this.picDisabledOverlay.ErrorImage = null;
            this.picDisabledOverlay.Image = global::TombLib.Properties.Resources.misc_SoundToolOverlay;
            this.picDisabledOverlay.InitialImage = null;
            this.picDisabledOverlay.Location = new System.Drawing.Point(280, 4);
            this.picDisabledOverlay.Name = "picDisabledOverlay";
            this.picDisabledOverlay.Size = new System.Drawing.Size(115, 22);
            this.picDisabledOverlay.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
            this.picDisabledOverlay.TabIndex = 102;
            this.picDisabledOverlay.TabStop = false;
            this.picDisabledOverlay.Visible = false;
            // 
            // butMoveDown
            // 
            this.butMoveDown.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.butMoveDown.Checked = false;
            this.butMoveDown.Image = global::TombLib.Properties.Resources.general_ArrowDown_16;
            this.butMoveDown.ImagePadding = 3;
            this.butMoveDown.Location = new System.Drawing.Point(378, 178);
            this.butMoveDown.Name = "butMoveDown";
            this.butMoveDown.Size = new System.Drawing.Size(22, 22);
            this.butMoveDown.TabIndex = 101;
            this.butMoveDown.Visible = false;
            // 
            // butMoveUp
            // 
            this.butMoveUp.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.butMoveUp.Checked = false;
            this.butMoveUp.Image = global::TombLib.Properties.Resources.general_ArrowUp_16;
            this.butMoveUp.ImagePadding = 3;
            this.butMoveUp.Location = new System.Drawing.Point(350, 178);
            this.butMoveUp.Name = "butMoveUp";
            this.butMoveUp.Size = new System.Drawing.Size(22, 22);
            this.butMoveUp.TabIndex = 100;
            this.butMoveUp.Visible = false;
            // 
            // butDeleteSample
            // 
            this.butDeleteSample.Checked = false;
            this.butDeleteSample.Image = global::TombLib.Properties.Resources.general_trash_16;
            this.butDeleteSample.ImagePadding = 3;
            this.butDeleteSample.Location = new System.Drawing.Point(178, 178);
            this.butDeleteSample.Name = "butDeleteSample";
            this.butDeleteSample.Size = new System.Drawing.Size(83, 22);
            this.butDeleteSample.TabIndex = 99;
            this.butDeleteSample.Text = "Delete";
            this.butDeleteSample.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.butDeleteSample.Click += new System.EventHandler(this.butDeleteSample_Click);
            // 
            // butAddSample
            // 
            this.butAddSample.Checked = false;
            this.butAddSample.Image = global::TombLib.Properties.Resources.general_plus_math_16;
            this.butAddSample.ImagePadding = 3;
            this.butAddSample.Location = new System.Drawing.Point(0, 178);
            this.butAddSample.Name = "butAddSample";
            this.butAddSample.Size = new System.Drawing.Size(83, 22);
            this.butAddSample.TabIndex = 98;
            this.butAddSample.Text = "Add new";
            this.butAddSample.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.butAddSample.Click += new System.EventHandler(this.butAddSample_Click);
            // 
            // dgvSamples
            // 
            this.dgvSamples.AllowUserToAddRows = false;
            this.dgvSamples.AllowUserToDeleteRows = false;
            this.dgvSamples.AllowUserToDragDropRows = false;
            this.dgvSamples.AllowUserToOrderColumns = true;
            this.dgvSamples.AllowUserToPasteCells = false;
            this.dgvSamples.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.dgvSamples.ColumnHeadersHeight = 17;
            this.dgvSamples.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.SamplePathColumn,
            this.SampleFoundPathColumn});
            this.dgvSamples.Location = new System.Drawing.Point(0, 206);
            this.dgvSamples.Name = "dgvSamples";
            this.dgvSamples.RowHeadersWidth = 41;
            this.dgvSamples.Size = new System.Drawing.Size(400, 220);
            this.dgvSamples.TabIndex = 97;
            this.dgvSamples.RowsAdded += new System.Windows.Forms.DataGridViewRowsAddedEventHandler(this.dgvSamples_RowsAdded);
            this.dgvSamples.RowsRemoved += new System.Windows.Forms.DataGridViewRowsRemovedEventHandler(this.dgvSamples_RowsRemoved);
            // 
            // SamplePathColumn
            // 
            this.SamplePathColumn.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.SamplePathColumn.FillWeight = 30F;
            this.SamplePathColumn.HeaderText = "Sample name";
            this.SamplePathColumn.Name = "SamplePathColumn";
            this.SamplePathColumn.ReadOnly = true;
            // 
            // SampleFoundPathColumn
            // 
            this.SampleFoundPathColumn.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.SampleFoundPathColumn.FillWeight = 70F;
            this.SampleFoundPathColumn.HeaderText = "Found in directory";
            this.SampleFoundPathColumn.Name = "SampleFoundPathColumn";
            this.SampleFoundPathColumn.ReadOnly = true;
            // 
            // tbID
            // 
            this.tbID.Location = new System.Drawing.Point(46, 3);
            this.tbID.Name = "tbID";
            this.tbID.ReadOnly = true;
            this.tbID.Size = new System.Drawing.Size(56, 22);
            this.tbID.TabIndex = 32;
            // 
            // darkLabel9
            // 
            this.darkLabel9.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.darkLabel9.Location = new System.Drawing.Point(-3, 5);
            this.darkLabel9.Name = "darkLabel9";
            this.darkLabel9.Size = new System.Drawing.Size(42, 13);
            this.darkLabel9.TabIndex = 31;
            this.darkLabel9.Text = "ID:";
            this.darkLabel9.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // darkGroupBox1
            // 
            this.darkGroupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.darkGroupBox1.Controls.Add(this.lblModeTooltip);
            this.darkGroupBox1.Controls.Add(this.darkLabel7);
            this.darkGroupBox1.Controls.Add(this.numericVolume);
            this.darkGroupBox1.Controls.Add(this.cbDisablePanning);
            this.darkGroupBox1.Controls.Add(this.cbRandomizeVolume);
            this.darkGroupBox1.Controls.Add(this.cbRandomizePitch);
            this.darkGroupBox1.Controls.Add(this.darkLabel2);
            this.darkGroupBox1.Controls.Add(this.numericChanceLabel);
            this.darkGroupBox1.Controls.Add(this.darkLabel3);
            this.darkGroupBox1.Controls.Add(this.numericRangeLabel);
            this.darkGroupBox1.Controls.Add(this.darkLabel4);
            this.darkGroupBox1.Controls.Add(this.numericPitchLabel);
            this.darkGroupBox1.Controls.Add(this.darkLabel5);
            this.darkGroupBox1.Controls.Add(this.numericVolumeLabel);
            this.darkGroupBox1.Controls.Add(this.darkLabel6);
            this.darkGroupBox1.Controls.Add(this.numericChance);
            this.darkGroupBox1.Controls.Add(this.comboLoop);
            this.darkGroupBox1.Controls.Add(this.numericPitch);
            this.darkGroupBox1.Controls.Add(this.numericRange);
            this.darkGroupBox1.Location = new System.Drawing.Point(0, 59);
            this.darkGroupBox1.Name = "darkGroupBox1";
            this.darkGroupBox1.Size = new System.Drawing.Size(400, 113);
            this.darkGroupBox1.TabIndex = 30;
            this.darkGroupBox1.TabStop = false;
            // 
            // lblModeTooltip
            // 
            this.lblModeTooltip.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblModeTooltip.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.lblModeTooltip.ForeColor = System.Drawing.Color.Gray;
            this.lblModeTooltip.Location = new System.Drawing.Point(174, 87);
            this.lblModeTooltip.Name = "lblModeTooltip";
            this.lblModeTooltip.Size = new System.Drawing.Size(221, 19);
            this.lblModeTooltip.TabIndex = 31;
            this.lblModeTooltip.Text = "hint";
            this.lblModeTooltip.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // darkLabel7
            // 
            this.darkLabel7.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.darkLabel7.AutoSize = true;
            this.darkLabel7.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.darkLabel7.Location = new System.Drawing.Point(217, 8);
            this.darkLabel7.Name = "darkLabel7";
            this.darkLabel7.Size = new System.Drawing.Size(67, 13);
            this.darkLabel7.TabIndex = 30;
            this.darkLabel7.Text = "Randomize:";
            // 
            // darkLabel2
            // 
            this.darkLabel2.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.darkLabel2.Location = new System.Drawing.Point(19, 8);
            this.darkLabel2.Name = "darkLabel2";
            this.darkLabel2.Size = new System.Drawing.Size(55, 13);
            this.darkLabel2.TabIndex = 3;
            this.darkLabel2.Text = "Volume:";
            this.darkLabel2.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // numericChanceLabel
            // 
            this.numericChanceLabel.AutoSize = true;
            this.numericChanceLabel.BackColor = System.Drawing.Color.Transparent;
            this.numericChanceLabel.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.numericChanceLabel.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.numericChanceLabel.Location = new System.Drawing.Point(152, 87);
            this.numericChanceLabel.Name = "numericChanceLabel";
            this.numericChanceLabel.Size = new System.Drawing.Size(16, 13);
            this.numericChanceLabel.TabIndex = 16;
            this.numericChanceLabel.Text = "%";
            // 
            // darkLabel3
            // 
            this.darkLabel3.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.darkLabel3.Location = new System.Drawing.Point(17, 60);
            this.darkLabel3.Name = "darkLabel3";
            this.darkLabel3.Size = new System.Drawing.Size(55, 13);
            this.darkLabel3.TabIndex = 11;
            this.darkLabel3.Text = "Range:";
            this.darkLabel3.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // numericRangeLabel
            // 
            this.numericRangeLabel.AutoSize = true;
            this.numericRangeLabel.BackColor = System.Drawing.Color.Transparent;
            this.numericRangeLabel.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.numericRangeLabel.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.numericRangeLabel.Location = new System.Drawing.Point(152, 61);
            this.numericRangeLabel.Name = "numericRangeLabel";
            this.numericRangeLabel.Size = new System.Drawing.Size(40, 13);
            this.numericRangeLabel.TabIndex = 9;
            this.numericRangeLabel.Text = "blocks";
            // 
            // darkLabel4
            // 
            this.darkLabel4.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.darkLabel4.Location = new System.Drawing.Point(17, 34);
            this.darkLabel4.Name = "darkLabel4";
            this.darkLabel4.Size = new System.Drawing.Size(55, 13);
            this.darkLabel4.TabIndex = 7;
            this.darkLabel4.Text = "Pitch:";
            this.darkLabel4.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // numericPitchLabel
            // 
            this.numericPitchLabel.AutoSize = true;
            this.numericPitchLabel.BackColor = System.Drawing.Color.Transparent;
            this.numericPitchLabel.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.numericPitchLabel.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.numericPitchLabel.Location = new System.Drawing.Point(152, 35);
            this.numericPitchLabel.Name = "numericPitchLabel";
            this.numericPitchLabel.Size = new System.Drawing.Size(16, 13);
            this.numericPitchLabel.TabIndex = 9;
            this.numericPitchLabel.Text = "%";
            // 
            // darkLabel5
            // 
            this.darkLabel5.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.darkLabel5.Location = new System.Drawing.Point(17, 86);
            this.darkLabel5.Name = "darkLabel5";
            this.darkLabel5.Size = new System.Drawing.Size(55, 13);
            this.darkLabel5.TabIndex = 14;
            this.darkLabel5.Text = "Chance:";
            this.darkLabel5.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // numericVolumeLabel
            // 
            this.numericVolumeLabel.AutoSize = true;
            this.numericVolumeLabel.BackColor = System.Drawing.Color.Transparent;
            this.numericVolumeLabel.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.numericVolumeLabel.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.numericVolumeLabel.Location = new System.Drawing.Point(152, 9);
            this.numericVolumeLabel.Name = "numericVolumeLabel";
            this.numericVolumeLabel.Size = new System.Drawing.Size(16, 13);
            this.numericVolumeLabel.TabIndex = 5;
            this.numericVolumeLabel.Text = "%";
            // 
            // darkLabel6
            // 
            this.darkLabel6.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.darkLabel6.AutoSize = true;
            this.darkLabel6.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.darkLabel6.Location = new System.Drawing.Point(217, 61);
            this.darkLabel6.Name = "darkLabel6";
            this.darkLabel6.Size = new System.Drawing.Size(40, 13);
            this.darkLabel6.TabIndex = 17;
            this.darkLabel6.Text = "Mode:";
            // 
            // comboLoop
            // 
            this.comboLoop.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.comboLoop.FormattingEnabled = true;
            this.comboLoop.Items.AddRange(new object[] {
            "None",
            "One shot wait",
            "One shot rewound",
            "Loop"});
            this.comboLoop.Location = new System.Drawing.Point(263, 58);
            this.comboLoop.Name = "comboLoop";
            this.comboLoop.Size = new System.Drawing.Size(132, 23);
            this.comboLoop.TabIndex = 18;
            this.comboLoop.SelectedIndexChanged += new System.EventHandler(this.OnSoundInfoChanged);
            // 
            // butPlayPreview
            // 
            this.butPlayPreview.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.butPlayPreview.Checked = false;
            this.butPlayPreview.Image = global::TombLib.Properties.Resources.actions_play_16;
            this.butPlayPreview.ImagePadding = 3;
            this.butPlayPreview.Location = new System.Drawing.Point(202, 31);
            this.butPlayPreview.Name = "butPlayPreview";
            this.butPlayPreview.Size = new System.Drawing.Size(114, 22);
            this.butPlayPreview.TabIndex = 2;
            this.butPlayPreview.Text = "In-game preview";
            this.butPlayPreview.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.butPlayPreview.Visible = false;
            this.butPlayPreview.Click += new System.EventHandler(this.butPlayPreview_Click);
            // 
            // tbName
            // 
            this.tbName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tbName.Location = new System.Drawing.Point(46, 31);
            this.tbName.Name = "tbName";
            this.tbName.Size = new System.Drawing.Size(150, 22);
            this.tbName.TabIndex = 1;
            this.tbName.TextChanged += new System.EventHandler(this.OnSoundInfoChanged);
            // 
            // tbNameLabel
            // 
            this.tbNameLabel.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.tbNameLabel.Location = new System.Drawing.Point(-3, 33);
            this.tbNameLabel.Name = "tbNameLabel";
            this.tbNameLabel.Size = new System.Drawing.Size(42, 13);
            this.tbNameLabel.TabIndex = 0;
            this.tbNameLabel.Text = "Name:";
            this.tbNameLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // butBrowse
            // 
            this.butBrowse.Checked = false;
            this.butBrowse.Image = global::TombLib.Properties.Resources.general_Open_16;
            this.butBrowse.ImagePadding = 3;
            this.butBrowse.Location = new System.Drawing.Point(89, 178);
            this.butBrowse.Name = "butBrowse";
            this.butBrowse.Size = new System.Drawing.Size(83, 22);
            this.butBrowse.TabIndex = 103;
            this.butBrowse.Text = "Browse...";
            this.butBrowse.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.butBrowse.Click += new System.EventHandler(this.butBrowse_Click);
            // 
            // SoundInfoEditor
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.Controls.Add(this.picDisabledOverlay);
            this.Controls.Add(this.cbIndexed);
            this.Controls.Add(this.butMoveDown);
            this.Controls.Add(this.butMoveUp);
            this.Controls.Add(this.butDeleteSample);
            this.Controls.Add(this.butAddSample);
            this.Controls.Add(this.dgvSamples);
            this.Controls.Add(this.cbGlobal);
            this.Controls.Add(this.tbID);
            this.Controls.Add(this.darkLabel9);
            this.Controls.Add(this.darkGroupBox1);
            this.Controls.Add(this.butPlayPreview);
            this.Controls.Add(this.tbName);
            this.Controls.Add(this.butClipboardPaste);
            this.Controls.Add(this.tbNameLabel);
            this.Controls.Add(this.butClipboardCopy);
            this.Controls.Add(this.butBrowse);
            this.Controls.Add(this.butResetToDefaults);
            this.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.MinimumSize = new System.Drawing.Size(400, 246);
            this.Name = "SoundInfoEditor";
            this.Size = new System.Drawing.Size(400, 426);
            ((System.ComponentModel.ISupportInitialize)(this.numericVolume)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericChance)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericPitch)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericRange)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.picDisabledOverlay)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgvSamples)).EndInit();
            this.darkGroupBox1.ResumeLayout(false);
            this.darkGroupBox1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private DarkUI.Controls.DarkComboBox comboLoop;
        private DarkUI.Controls.DarkLabel darkLabel6;
        private DarkUI.Controls.DarkLabel darkLabel5;
        private DarkUI.Controls.DarkLabel darkLabel4;
        private DarkUI.Controls.DarkLabel darkLabel3;
        private DarkUI.Controls.DarkLabel darkLabel2;
        private DarkUI.Controls.DarkCheckBox cbRandomizePitch;
        private DarkUI.Controls.DarkCheckBox cbRandomizeVolume;
        private DarkUI.Controls.DarkCheckBox cbDisablePanning;
        private DarkUI.Controls.DarkTextBox tbName;
        private DarkUI.Controls.DarkLabel tbNameLabel;
        private System.Windows.Forms.ToolTip toolTip;
        private DarkUI.Controls.DarkNumericUpDown numericVolume;
        private DarkUI.Controls.DarkNumericUpDown numericRange;
        private DarkUI.Controls.DarkNumericUpDown numericPitch;
        private DarkUI.Controls.DarkNumericUpDown numericChance;
        private DarkUI.Controls.DarkLabel numericVolumeLabel;
        private DarkUI.Controls.DarkLabel numericPitchLabel;
        private DarkUI.Controls.DarkLabel numericChanceLabel;
        private DarkUI.Controls.DarkButton butPlayPreview;
        private DarkUI.Controls.DarkLabel numericRangeLabel;
        private DarkUI.Controls.DarkButton butClipboardCopy;
        private DarkUI.Controls.DarkButton butClipboardPaste;
        private DarkUI.Controls.DarkGroupBox darkGroupBox1;
        private DarkUI.Controls.DarkLabel darkLabel7;
        private DarkUI.Controls.DarkTextBox tbID;
        private DarkUI.Controls.DarkLabel darkLabel9;
        private DarkUI.Controls.DarkLabel lblModeTooltip;
        private System.Windows.Forms.DataGridViewTextBoxColumn colSampleName;
        private DarkUI.Controls.DarkCheckBox cbGlobal;
        private DarkUI.Controls.DarkDataGridView dgvSamples;
        private DarkUI.Controls.DarkButton butAddSample;
        private DarkUI.Controls.DarkButton butDeleteSample;
        private DarkUI.Controls.DarkButton butMoveUp;
        private DarkUI.Controls.DarkButton butMoveDown;
        private System.Windows.Forms.PictureBox picDisabledOverlay;
        private DarkUI.Controls.DarkButton butBrowse;
        private DarkUI.Controls.DarkButton butResetToDefaults;
        private System.Windows.Forms.DataGridViewTextBoxColumn SamplePathColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn SampleFoundPathColumn;
        private DarkUI.Controls.DarkCheckBox cbIndexed;
    }
}
