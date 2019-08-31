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
            this.numericChance = new DarkUI.Controls.DarkNumericUpDown();
            this.numericPitch = new DarkUI.Controls.DarkNumericUpDown();
            this.numericRange = new DarkUI.Controls.DarkNumericUpDown();
            this.numericVolume = new DarkUI.Controls.DarkNumericUpDown();
            this.butClipboardPaste = new DarkUI.Controls.DarkButton();
            this.butClipboardCopy = new DarkUI.Controls.DarkButton();
            this.butExport = new DarkUI.Controls.DarkButton();
            this.cbRandomizePitch = new DarkUI.Controls.DarkCheckBox();
            this.cbRandomizeVolume = new DarkUI.Controls.DarkCheckBox();
            this.cbDisablePanning = new DarkUI.Controls.DarkCheckBox();
            this.trackBar = new System.Windows.Forms.TrackBar();
            this.comboSampleRateLabel = new DarkUI.Controls.DarkLabel();
            this.trackBar_100MillisecondMark = new DarkUI.Controls.DarkLabel();
            this.butPlayPreview = new DarkUI.Controls.DarkButton();
            this.dataGridView = new DarkUI.Controls.DarkDataGridView();
            this.WaveformColumn = new System.Windows.Forms.DataGridViewImageColumn();
            this.PlayButtonColumn = new DarkUI.Controls.DarkDataGridViewButtonColumn();
            this.DurationColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.SizeColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.numericChanceLabel = new DarkUI.Controls.DarkLabel();
            this.numericRangeLabel = new DarkUI.Controls.DarkLabel();
            this.numericPitchLabel = new DarkUI.Controls.DarkLabel();
            this.numericVolumeLabel = new DarkUI.Controls.DarkLabel();
            this.comboLoop = new DarkUI.Controls.DarkComboBox();
            this.darkLabel6 = new DarkUI.Controls.DarkLabel();
            this.darkLabel5 = new DarkUI.Controls.DarkLabel();
            this.darkLabel4 = new DarkUI.Controls.DarkLabel();
            this.darkLabel3 = new DarkUI.Controls.DarkLabel();
            this.darkLabel2 = new DarkUI.Controls.DarkLabel();
            this.tbName = new DarkUI.Controls.DarkTextBox();
            this.tbNameLabel = new DarkUI.Controls.DarkLabel();
            this.comboSampleRateTextBox = new DarkUI.Controls.DarkTextBox();
            this.comboSampleRate = new DarkUI.Controls.DarkComboBox();
            this.darkLabel8 = new DarkUI.Controls.DarkLabel();
            this.darkLabel1 = new DarkUI.Controls.DarkLabel();
            this.darkGroupBox1 = new DarkUI.Controls.DarkGroupBox();
            this.lblModeTooltip = new DarkUI.Controls.DarkLabel();
            this.darkLabel7 = new DarkUI.Controls.DarkLabel();
            this.dataGridViewControls = new TombLib.Controls.DarkDataGridViewControls();
            ((System.ComponentModel.ISupportInitialize)(this.numericChance)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericPitch)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericRange)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericVolume)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.trackBar)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView)).BeginInit();
            this.darkGroupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // toolTip
            // 
            this.toolTip.AutoPopDelay = 30000;
            this.toolTip.InitialDelay = 500;
            this.toolTip.ReshowDelay = 100;
            // 
            // numericChance
            // 
            this.numericChance.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(69)))), ((int)(((byte)(73)))), ((int)(((byte)(74)))));
            this.numericChance.DecimalPlaces = 1;
            this.numericChance.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.numericChance.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.numericChance.Increment = new decimal(new int[] {
            5,
            0,
            0,
            0});
            this.numericChance.IncrementAlternate = new decimal(new int[] {
            10,
            0,
            0,
            65536});
            this.numericChance.Location = new System.Drawing.Point(78, 84);
            this.numericChance.MousewheelSingleIncrement = true;
            this.numericChance.Name = "numericChance";
            this.numericChance.Size = new System.Drawing.Size(68, 22);
            this.numericChance.TabIndex = 15;
            this.toolTip.SetToolTip(this.numericChance, "Probability that any sample will play when triggered.");
            this.numericChance.ValueChanged += new System.EventHandler(this.OnSoundInfoChanged);
            // 
            // numericPitch
            // 
            this.numericPitch.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(69)))), ((int)(((byte)(73)))), ((int)(((byte)(74)))));
            this.numericPitch.DecimalPlaces = 1;
            this.numericPitch.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.numericPitch.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.numericPitch.Increment = new decimal(new int[] {
            5,
            0,
            0,
            0});
            this.numericPitch.IncrementAlternate = new decimal(new int[] {
            10,
            0,
            0,
            65536});
            this.numericPitch.Location = new System.Drawing.Point(78, 32);
            this.numericPitch.Maximum = new decimal(new int[] {
            1983,
            0,
            0,
            65536});
            this.numericPitch.MousewheelSingleIncrement = true;
            this.numericPitch.Name = "numericPitch";
            this.numericPitch.Size = new System.Drawing.Size(68, 22);
            this.numericPitch.TabIndex = 8;
            this.toolTip.SetToolTip(this.numericPitch, "Pitch of the sound. Value is relative.");
            this.numericPitch.ValueChanged += new System.EventHandler(this.OnSoundInfoChanged);
            // 
            // numericRange
            // 
            this.numericRange.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(69)))), ((int)(((byte)(73)))), ((int)(((byte)(74)))));
            this.numericRange.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.numericRange.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.numericRange.Increment = new decimal(new int[] {
            5,
            0,
            0,
            0});
            this.numericRange.IncrementAlternate = new decimal(new int[] {
            10,
            0,
            0,
            65536});
            this.numericRange.Location = new System.Drawing.Point(78, 58);
            this.numericRange.Maximum = new decimal(new int[] {
            255,
            0,
            0,
            0});
            this.numericRange.MousewheelSingleIncrement = true;
            this.numericRange.Name = "numericRange";
            this.numericRange.Size = new System.Drawing.Size(68, 22);
            this.numericRange.TabIndex = 12;
            this.toolTip.SetToolTip(this.numericRange, "Range in blocks from where the sample can be heard relative to where it was trigg" +
        "ered.");
            this.numericRange.ValueChanged += new System.EventHandler(this.OnSoundInfoChanged);
            // 
            // numericVolume
            // 
            this.numericVolume.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(69)))), ((int)(((byte)(73)))), ((int)(((byte)(74)))));
            this.numericVolume.DecimalPlaces = 1;
            this.numericVolume.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.numericVolume.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.numericVolume.IncrementAlternate = new decimal(new int[] {
            5,
            0,
            0,
            0});
            this.numericVolume.Location = new System.Drawing.Point(78, 6);
            this.numericVolume.MousewheelSingleIncrement = true;
            this.numericVolume.Name = "numericVolume";
            this.numericVolume.Size = new System.Drawing.Size(68, 22);
            this.numericVolume.TabIndex = 4;
            this.toolTip.SetToolTip(this.numericVolume, "Volume in percent of the volume used in the sample. The volume can\'t be increased" +
        ".");
            this.numericVolume.ValueChanged += new System.EventHandler(this.OnSoundInfoChanged);
            // 
            // butClipboardPaste
            // 
            this.butClipboardPaste.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.butClipboardPaste.Image = global::TombLib.Properties.Resources.general_clipboard_161;
            this.butClipboardPaste.Location = new System.Drawing.Point(378, 0);
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
            this.butClipboardCopy.Image = global::TombLib.Properties.Resources.general_copy_16;
            this.butClipboardCopy.Location = new System.Drawing.Point(350, 0);
            this.butClipboardCopy.Name = "butClipboardCopy";
            this.butClipboardCopy.Size = new System.Drawing.Size(22, 22);
            this.butClipboardCopy.TabIndex = 22;
            this.toolTip.SetToolTip(this.butClipboardCopy, "Copy all settings and samples into the clipboard.");
            this.butClipboardCopy.Click += new System.EventHandler(this.butClipboardCopy_Click);
            // 
            // butExport
            // 
            this.butExport.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.butExport.Image = global::TombLib.Properties.Resources.general_Export_16;
            this.butExport.Location = new System.Drawing.Point(374, 255);
            this.butExport.Name = "butExport";
            this.butExport.Size = new System.Drawing.Size(26, 23);
            this.butExport.TabIndex = 22;
            this.butExport.Text = "Play";
            this.butExport.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.toolTip.SetToolTip(this.butExport, "Export to audio file.");
            this.butExport.Click += new System.EventHandler(this.butExport_Click);
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
            // cbDisablePanning
            // 
            this.cbDisablePanning.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.cbDisablePanning.CheckAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.cbDisablePanning.Location = new System.Drawing.Point(288, 33);
            this.cbDisablePanning.Name = "cbDisablePanning";
            this.cbDisablePanning.Size = new System.Drawing.Size(107, 17);
            this.cbDisablePanning.TabIndex = 13;
            this.cbDisablePanning.Text = "Disable panning";
            this.toolTip.SetToolTip(this.cbDisablePanning, "Disable directional audio for this sound.");
            this.cbDisablePanning.CheckedChanged += new System.EventHandler(this.OnSoundInfoChanged);
            // 
            // trackBar
            // 
            this.trackBar.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.trackBar.AutoSize = false;
            this.trackBar.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(51)))), ((int)(((byte)(51)))), ((int)(((byte)(51)))));
            this.trackBar.Cursor = System.Windows.Forms.Cursors.SizeWE;
            this.trackBar.Location = new System.Drawing.Point(0, 196);
            this.trackBar.Maximum = 227;
            this.trackBar.Minimum = 5;
            this.trackBar.Name = "trackBar";
            this.trackBar.Size = new System.Drawing.Size(195, 15);
            this.trackBar.TabIndex = 23;
            this.trackBar.TickStyle = System.Windows.Forms.TickStyle.None;
            this.trackBar.Value = 116;
            this.trackBar.Scroll += new System.EventHandler(this.trackBar_Scroll);
            this.trackBar.Resize += new System.EventHandler(this.trackBar_Resize);
            // 
            // comboSampleRateLabel
            // 
            this.comboSampleRateLabel.AutoSize = true;
            this.comboSampleRateLabel.BackColor = System.Drawing.Color.Transparent;
            this.comboSampleRateLabel.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.comboSampleRateLabel.Location = new System.Drawing.Point(152, 114);
            this.comboSampleRateLabel.Name = "comboSampleRateLabel";
            this.comboSampleRateLabel.Size = new System.Drawing.Size(20, 13);
            this.comboSampleRateLabel.TabIndex = 27;
            this.comboSampleRateLabel.Text = "Hz";
            // 
            // trackBar_100MillisecondMark
            // 
            this.trackBar_100MillisecondMark.AutoSize = true;
            this.trackBar_100MillisecondMark.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(51)))), ((int)(((byte)(51)))), ((int)(((byte)(51)))));
            this.trackBar_100MillisecondMark.Font = new System.Drawing.Font("Microsoft Sans Serif", 6F);
            this.trackBar_100MillisecondMark.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.trackBar_100MillisecondMark.Location = new System.Drawing.Point(120, 204);
            this.trackBar_100MillisecondMark.Name = "trackBar_100MillisecondMark";
            this.trackBar_100MillisecondMark.Size = new System.Drawing.Size(21, 9);
            this.trackBar_100MillisecondMark.TabIndex = 24;
            this.trackBar_100MillisecondMark.Text = "0.1 s";
            // 
            // butPlayPreview
            // 
            this.butPlayPreview.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.butPlayPreview.Image = global::TombLib.Properties.Resources.actions_play_16;
            this.butPlayPreview.ImagePadding = 3;
            this.butPlayPreview.Location = new System.Drawing.Point(220, 0);
            this.butPlayPreview.Name = "butPlayPreview";
            this.butPlayPreview.Size = new System.Drawing.Size(124, 22);
            this.butPlayPreview.TabIndex = 2;
            this.butPlayPreview.Text = "In-game preview";
            this.butPlayPreview.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.butPlayPreview.Click += new System.EventHandler(this.butPlayPreview_Click);
            // 
            // dataGridView
            // 
            this.dataGridView.AllowUserToResizeRows = true;
            this.dataGridView.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.dataGridView.AutoGenerateColumns = false;
            this.dataGridView.ColumnHeadersHeight = 17;
            this.dataGridView.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.WaveformColumn,
            this.PlayButtonColumn,
            this.DurationColumn,
            this.SizeColumn});
            this.dataGridView.Location = new System.Drawing.Point(0, 195);
            this.dataGridView.Name = "dataGridView";
            this.dataGridView.RowHeadersWidth = 41;
            this.dataGridView.RowTemplate.Height = 64;
            this.dataGridView.Size = new System.Drawing.Size(368, 151);
            this.dataGridView.TabIndex = 20;
            this.dataGridView.Tag = "s";
            this.dataGridView.CellFormattingSafe += new DarkUI.Controls.DarkDataGridViewSafeCellFormattingEventHandler(this.dataGridView_CellFormattingSafe);
            this.dataGridView.CellContentClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dataGridView_CellContentClick);
            this.dataGridView.CellPainting += new System.Windows.Forms.DataGridViewCellPaintingEventHandler(this.dataGridView_CellPainting);
            // 
            // WaveformColumn
            // 
            this.WaveformColumn.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.WaveformColumn.HeaderText = "Waveform";
            this.WaveformColumn.Name = "WaveformColumn";
            // 
            // PlayButtonColumn
            // 
            this.PlayButtonColumn.HeaderText = "Play";
            this.PlayButtonColumn.Name = "PlayButtonColumn";
            this.PlayButtonColumn.ReadOnly = true;
            this.PlayButtonColumn.Width = 33;
            // 
            // DurationColumn
            // 
            this.DurationColumn.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
            this.DurationColumn.HeaderText = "Duration";
            this.DurationColumn.Name = "DurationColumn";
            this.DurationColumn.ReadOnly = true;
            this.DurationColumn.Width = 77;
            // 
            // SizeColumn
            // 
            this.SizeColumn.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
            this.SizeColumn.HeaderText = "Size";
            this.SizeColumn.Name = "SizeColumn";
            this.SizeColumn.ReadOnly = true;
            this.SizeColumn.Width = 51;
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
            // comboLoop
            // 
            this.comboLoop.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.comboLoop.FormattingEnabled = true;
            this.comboLoop.Items.AddRange(new object[] {
            "None",
            "One shot wait",
            "One shot rewound",
            "Loop"});
            this.comboLoop.Location = new System.Drawing.Point(263, 73);
            this.comboLoop.Name = "comboLoop";
            this.comboLoop.Size = new System.Drawing.Size(132, 23);
            this.comboLoop.TabIndex = 18;
            this.comboLoop.SelectedIndexChanged += new System.EventHandler(this.OnSoundInfoChanged);
            // 
            // darkLabel6
            // 
            this.darkLabel6.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.darkLabel6.AutoSize = true;
            this.darkLabel6.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.darkLabel6.Location = new System.Drawing.Point(217, 76);
            this.darkLabel6.Name = "darkLabel6";
            this.darkLabel6.Size = new System.Drawing.Size(40, 13);
            this.darkLabel6.TabIndex = 17;
            this.darkLabel6.Text = "Mode:";
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
            // tbName
            // 
            this.tbName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tbName.Location = new System.Drawing.Point(46, 0);
            this.tbName.Name = "tbName";
            this.tbName.Size = new System.Drawing.Size(168, 22);
            this.tbName.TabIndex = 1;
            this.tbName.TextChanged += new System.EventHandler(this.OnSoundInfoChanged);
            // 
            // tbNameLabel
            // 
            this.tbNameLabel.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.tbNameLabel.Location = new System.Drawing.Point(-3, 2);
            this.tbNameLabel.Name = "tbNameLabel";
            this.tbNameLabel.Size = new System.Drawing.Size(42, 13);
            this.tbNameLabel.TabIndex = 0;
            this.tbNameLabel.Text = "Name:";
            this.tbNameLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // comboSampleRateTextBox
            // 
            this.comboSampleRateTextBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.comboSampleRateTextBox.Location = new System.Drawing.Point(79, 114);
            this.comboSampleRateTextBox.MaxLength = 6;
            this.comboSampleRateTextBox.Name = "comboSampleRateTextBox";
            this.comboSampleRateTextBox.Size = new System.Drawing.Size(46, 15);
            this.comboSampleRateTextBox.TabIndex = 28;
            this.comboSampleRateTextBox.TextChanged += new System.EventHandler(this.comboSampleRateTextBox_TextChanged);
            this.comboSampleRateTextBox.KeyDown += new System.Windows.Forms.KeyEventHandler(this.comboSampleRateTextBox_KeyDown);
            this.comboSampleRateTextBox.Validating += new System.ComponentModel.CancelEventHandler(this.comboSampleRateTextBox_Validating);
            this.comboSampleRateTextBox.Validated += new System.EventHandler(this.comboSampleRateTextBox_Validated);
            // 
            // comboSampleRate
            // 
            this.comboSampleRate.DropDownHeight = 200;
            this.comboSampleRate.FormattingEnabled = true;
            this.comboSampleRate.IntegralHeight = false;
            this.comboSampleRate.Items.AddRange(new object[] {
            "8000",
            "11025",
            "16000",
            "22050",
            "32000",
            "44100"});
            this.comboSampleRate.Location = new System.Drawing.Point(78, 110);
            this.comboSampleRate.Name = "comboSampleRate";
            this.comboSampleRate.Size = new System.Drawing.Size(68, 23);
            this.comboSampleRate.TabIndex = 26;
            this.comboSampleRate.SelectedValueChanged += new System.EventHandler(this.comboSampleRate_SelectedValueChanged);
            // 
            // darkLabel8
            // 
            this.darkLabel8.AutoSize = true;
            this.darkLabel8.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.darkLabel8.Location = new System.Drawing.Point(0, 180);
            this.darkLabel8.Name = "darkLabel8";
            this.darkLabel8.Size = new System.Drawing.Size(168, 13);
            this.darkLabel8.TabIndex = 19;
            this.darkLabel8.Text = "List of samples (plays randomly)";
            // 
            // darkLabel1
            // 
            this.darkLabel1.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.darkLabel1.Location = new System.Drawing.Point(2, 113);
            this.darkLabel1.Name = "darkLabel1";
            this.darkLabel1.Size = new System.Drawing.Size(70, 13);
            this.darkLabel1.TabIndex = 29;
            this.darkLabel1.Text = "Sample rate:";
            this.darkLabel1.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // darkGroupBox1
            // 
            this.darkGroupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.darkGroupBox1.Controls.Add(this.lblModeTooltip);
            this.darkGroupBox1.Controls.Add(this.darkLabel7);
            this.darkGroupBox1.Controls.Add(this.numericVolume);
            this.darkGroupBox1.Controls.Add(this.darkLabel1);
            this.darkGroupBox1.Controls.Add(this.comboSampleRate);
            this.darkGroupBox1.Controls.Add(this.comboSampleRateLabel);
            this.darkGroupBox1.Controls.Add(this.comboSampleRateTextBox);
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
            this.darkGroupBox1.Location = new System.Drawing.Point(0, 28);
            this.darkGroupBox1.Name = "darkGroupBox1";
            this.darkGroupBox1.Size = new System.Drawing.Size(400, 138);
            this.darkGroupBox1.TabIndex = 30;
            this.darkGroupBox1.TabStop = false;
            // 
            // lblModeTooltip
            // 
            this.lblModeTooltip.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblModeTooltip.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.lblModeTooltip.ForeColor = System.Drawing.Color.Gray;
            this.lblModeTooltip.Location = new System.Drawing.Point(206, 99);
            this.lblModeTooltip.Name = "lblModeTooltip";
            this.lblModeTooltip.Size = new System.Drawing.Size(191, 36);
            this.lblModeTooltip.TabIndex = 31;
            this.lblModeTooltip.Text = "(hint)";
            this.lblModeTooltip.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // darkLabel7
            // 
            this.darkLabel7.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.darkLabel7.AutoSize = true;
            this.darkLabel7.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.darkLabel7.Location = new System.Drawing.Point(217, 9);
            this.darkLabel7.Name = "darkLabel7";
            this.darkLabel7.Size = new System.Drawing.Size(67, 13);
            this.darkLabel7.TabIndex = 30;
            this.darkLabel7.Text = "Randomize:";
            // 
            // dataGridViewControls
            // 
            this.dataGridViewControls.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.dataGridViewControls.Enabled = false;
            this.dataGridViewControls.Location = new System.Drawing.Point(374, 195);
            this.dataGridViewControls.MinimumSize = new System.Drawing.Size(24, 24);
            this.dataGridViewControls.Name = "dataGridViewControls";
            this.dataGridViewControls.Size = new System.Drawing.Size(26, 151);
            this.dataGridViewControls.TabIndex = 21;
            // 
            // SoundInfoEditor
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.Controls.Add(this.darkGroupBox1);
            this.Controls.Add(this.trackBar_100MillisecondMark);
            this.Controls.Add(this.trackBar);
            this.Controls.Add(this.butPlayPreview);
            this.Controls.Add(this.dataGridView);
            this.Controls.Add(this.butExport);
            this.Controls.Add(this.tbName);
            this.Controls.Add(this.butClipboardPaste);
            this.Controls.Add(this.tbNameLabel);
            this.Controls.Add(this.butClipboardCopy);
            this.Controls.Add(this.dataGridViewControls);
            this.Controls.Add(this.darkLabel8);
            this.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.MinimumSize = new System.Drawing.Size(400, 346);
            this.Name = "SoundInfoEditor";
            this.Size = new System.Drawing.Size(400, 346);
            ((System.ComponentModel.ISupportInitialize)(this.numericChance)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericPitch)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericRange)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericVolume)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.trackBar)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView)).EndInit();
            this.darkGroupBox1.ResumeLayout(false);
            this.darkGroupBox1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private DarkUI.Controls.DarkButton butExport;
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
        private DarkUI.Controls.DarkDataGridView dataGridView;
        private DarkDataGridViewControls dataGridViewControls;
        private DarkUI.Controls.DarkButton butPlayPreview;
        private DarkUI.Controls.DarkLabel darkLabel8;
        private System.Windows.Forms.DataGridViewImageColumn WaveformColumn;
        private DarkUI.Controls.DarkDataGridViewButtonColumn PlayButtonColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn DurationColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn SizeColumn;
        private System.Windows.Forms.TrackBar trackBar;
        private DarkUI.Controls.DarkLabel trackBar_100MillisecondMark;
        private DarkUI.Controls.DarkComboBox comboSampleRate;
        private DarkUI.Controls.DarkLabel comboSampleRateLabel;
        private DarkUI.Controls.DarkTextBox comboSampleRateTextBox;
        private DarkUI.Controls.DarkLabel numericRangeLabel;
        private DarkUI.Controls.DarkButton butClipboardCopy;
        private DarkUI.Controls.DarkButton butClipboardPaste;
        private DarkUI.Controls.DarkLabel darkLabel1;
        private DarkUI.Controls.DarkGroupBox darkGroupBox1;
        private DarkUI.Controls.DarkLabel lblModeTooltip;
        private DarkUI.Controls.DarkLabel darkLabel7;
    }
}
