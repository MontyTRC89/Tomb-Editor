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
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.numericChance = new DarkUI.Controls.DarkNumericUpDown();
            this.numericPitch = new DarkUI.Controls.DarkNumericUpDown();
            this.numericRange = new DarkUI.Controls.DarkNumericUpDown();
            this.numericVolume = new DarkUI.Controls.DarkNumericUpDown();
            this.cbRandomizePitch = new DarkUI.Controls.DarkCheckBox();
            this.cbRandomizeVolume = new DarkUI.Controls.DarkCheckBox();
            this.cbDisablePanning = new DarkUI.Controls.DarkCheckBox();
            this.trackBar = new System.Windows.Forms.TrackBar();
            this.comboSampleRate = new DarkUI.Controls.DarkComboBox();
            this.comboLoop = new DarkUI.Controls.DarkComboBox();
            this.comboSampleRateLabel = new DarkUI.Controls.DarkLabel();
            this.trackBar_1SecondMark = new DarkUI.Controls.DarkLabel();
            this.darkLabel8 = new DarkUI.Controls.DarkLabel();
            this.butPlayPreview = new DarkUI.Controls.DarkButton();
            this.dataGridView = new DarkUI.Controls.DarkDataGridView();
            this.WaveformColumn = new System.Windows.Forms.DataGridViewImageColumn();
            this.PlayButtonColumn = new DarkUI.Controls.DarkDataGridViewButtonColumn();
            this.DurationColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.SizeColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.darkLabel11 = new DarkUI.Controls.DarkLabel();
            this.darkLabel10 = new DarkUI.Controls.DarkLabel();
            this.darkLabel7 = new DarkUI.Controls.DarkLabel();
            this.butExport = new DarkUI.Controls.DarkButton();
            this.darkLabel6 = new DarkUI.Controls.DarkLabel();
            this.darkLabel5 = new DarkUI.Controls.DarkLabel();
            this.darkLabel4 = new DarkUI.Controls.DarkLabel();
            this.darkLabel3 = new DarkUI.Controls.DarkLabel();
            this.darkLabel2 = new DarkUI.Controls.DarkLabel();
            this.tbName = new DarkUI.Controls.DarkTextBox();
            this.darkLabel1 = new DarkUI.Controls.DarkLabel();
            this.comboSampleRateTextBox = new DarkUI.Controls.DarkTextBox();
            this.dataGridViewControls = new TombLib.Controls.DarkDataGridViewControls();
            ((System.ComponentModel.ISupportInitialize)(this.numericChance)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericPitch)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericRange)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericVolume)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.trackBar)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView)).BeginInit();
            this.SuspendLayout();
            // 
            // toolTip1
            // 
            this.toolTip1.AutoPopDelay = 30000;
            this.toolTip1.InitialDelay = 500;
            this.toolTip1.ReshowDelay = 100;
            // 
            // numericChance
            // 
            this.numericChance.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(69)))), ((int)(((byte)(73)))), ((int)(((byte)(74)))));
            this.numericChance.DecimalPlaces = 1;
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
            this.numericChance.Location = new System.Drawing.Point(46, 105);
            this.numericChance.MousewheelSingleIncrement = true;
            this.numericChance.Name = "numericChance";
            this.numericChance.Size = new System.Drawing.Size(63, 20);
            this.numericChance.TabIndex = 15;
            this.toolTip1.SetToolTip(this.numericChance, "Probability that any sample will play when triggered.");
            this.numericChance.ValueChanged += new System.EventHandler(this.OnSoundInfoChanged);
            // 
            // numericPitch
            // 
            this.numericPitch.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(69)))), ((int)(((byte)(73)))), ((int)(((byte)(74)))));
            this.numericPitch.DecimalPlaces = 1;
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
            this.numericPitch.Location = new System.Drawing.Point(46, 53);
            this.numericPitch.Maximum = new decimal(new int[] {
            1983,
            0,
            0,
            65536});
            this.numericPitch.MousewheelSingleIncrement = true;
            this.numericPitch.Name = "numericPitch";
            this.numericPitch.Size = new System.Drawing.Size(63, 20);
            this.numericPitch.TabIndex = 8;
            this.toolTip1.SetToolTip(this.numericPitch, "Pitch of the sound on playback. The pitch is relative to the pitch of the sample." +
        " At most the pitch can be almost doubled");
            this.numericPitch.ValueChanged += new System.EventHandler(this.OnSoundInfoChanged);
            // 
            // numericRange
            // 
            this.numericRange.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(69)))), ((int)(((byte)(73)))), ((int)(((byte)(74)))));
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
            this.numericRange.Location = new System.Drawing.Point(46, 79);
            this.numericRange.Maximum = new decimal(new int[] {
            255,
            0,
            0,
            0});
            this.numericRange.MousewheelSingleIncrement = true;
            this.numericRange.Name = "numericRange";
            this.numericRange.Size = new System.Drawing.Size(63, 20);
            this.numericRange.TabIndex = 12;
            this.toolTip1.SetToolTip(this.numericRange, "Range in blocks from where the sample can be heard at all relative to where it wa" +
        "s triggered.");
            this.numericRange.ValueChanged += new System.EventHandler(this.OnSoundInfoChanged);
            // 
            // numericVolume
            // 
            this.numericVolume.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(69)))), ((int)(((byte)(73)))), ((int)(((byte)(74)))));
            this.numericVolume.DecimalPlaces = 1;
            this.numericVolume.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.numericVolume.IncrementAlternate = new decimal(new int[] {
            5,
            0,
            0,
            0});
            this.numericVolume.Location = new System.Drawing.Point(45, 26);
            this.numericVolume.MousewheelSingleIncrement = true;
            this.numericVolume.Name = "numericVolume";
            this.numericVolume.Size = new System.Drawing.Size(63, 20);
            this.numericVolume.TabIndex = 4;
            this.toolTip1.SetToolTip(this.numericVolume, "Volume in percent of the volume used in the sample. The volume can not be increas" +
        "ed.");
            this.numericVolume.ValueChanged += new System.EventHandler(this.OnSoundInfoChanged);
            // 
            // cbRandomizePitch
            // 
            this.cbRandomizePitch.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.cbRandomizePitch.CheckAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.cbRandomizePitch.Location = new System.Drawing.Point(334, 53);
            this.cbRandomizePitch.Name = "cbRandomizePitch";
            this.cbRandomizePitch.Size = new System.Drawing.Size(109, 17);
            this.cbRandomizePitch.TabIndex = 10;
            this.cbRandomizePitch.Text = "Randomize pitch";
            this.toolTip1.SetToolTip(this.cbRandomizePitch, "Vary the pitch for each playback of the sound info.\r\nThe pitch is sped up and slo" +
        "wed down by rougly 10%. (Not relative to the pitch on the right)");
            this.cbRandomizePitch.CheckedChanged += new System.EventHandler(this.OnSoundInfoChanged);
            // 
            // cbRandomizeVolume
            // 
            this.cbRandomizeVolume.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.cbRandomizeVolume.CheckAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.cbRandomizeVolume.Location = new System.Drawing.Point(321, 27);
            this.cbRandomizeVolume.Name = "cbRandomizeVolume";
            this.cbRandomizeVolume.Size = new System.Drawing.Size(122, 17);
            this.cbRandomizeVolume.TabIndex = 6;
            this.cbRandomizeVolume.Text = "Randomize volume";
            this.toolTip1.SetToolTip(this.cbRandomizeVolume, "Vary the volume for each playback of the sound info.\r\nThe volume is reduced by an" +
        " absolete value of 1/8 of the full volume. (Not relative to the volume on the ri" +
        "ght)");
            this.cbRandomizeVolume.CheckedChanged += new System.EventHandler(this.OnSoundInfoChanged);
            // 
            // cbDisablePanning
            // 
            this.cbDisablePanning.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.cbDisablePanning.CheckAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.cbDisablePanning.Location = new System.Drawing.Point(336, 79);
            this.cbDisablePanning.Name = "cbDisablePanning";
            this.cbDisablePanning.Size = new System.Drawing.Size(107, 17);
            this.cbDisablePanning.TabIndex = 13;
            this.cbDisablePanning.Text = "Disable panning";
            this.toolTip1.SetToolTip(this.cbDisablePanning, "Disable directional audio for this sound.");
            this.cbDisablePanning.CheckedChanged += new System.EventHandler(this.OnSoundInfoChanged);
            // 
            // trackBar
            // 
            this.trackBar.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.trackBar.AutoSize = false;
            this.trackBar.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(51)))), ((int)(((byte)(51)))), ((int)(((byte)(51)))));
            this.trackBar.Cursor = System.Windows.Forms.Cursors.SizeWE;
            this.trackBar.Location = new System.Drawing.Point(0, 168);
            this.trackBar.Maximum = 227;
            this.trackBar.Minimum = 5;
            this.trackBar.Name = "trackBar";
            this.trackBar.Size = new System.Drawing.Size(237, 15);
            this.trackBar.TabIndex = 23;
            this.trackBar.TickStyle = System.Windows.Forms.TickStyle.None;
            this.trackBar.Value = 116;
            this.trackBar.Scroll += new System.EventHandler(this.trackBar_Scroll);
            this.trackBar.Resize += new System.EventHandler(this.trackBar_Resize);
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
            this.comboSampleRate.Location = new System.Drawing.Point(196, 132);
            this.comboSampleRate.Name = "comboSampleRate";
            this.comboSampleRate.Size = new System.Drawing.Size(78, 21);
            this.comboSampleRate.TabIndex = 26;
            this.comboSampleRate.SelectedValueChanged += new System.EventHandler(this.comboSampleRate_SelectedValueChanged);
            // 
            // comboLoop
            // 
            this.comboLoop.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.comboLoop.DropDownWidth = 460;
            this.comboLoop.FormattingEnabled = true;
            this.comboLoop.Items.AddRange(new object[] {
            "None",
            "One shot rewound (The sound will be replayed from the beginning if triggered agai" +
                "n)",
            "One shot wait (The sound sound will be ignored until the current one is stopped)",
            "Loop (The sound will be looped until stopped by an engine event)"});
            this.comboLoop.Location = new System.Drawing.Point(363, 104);
            this.comboLoop.Name = "comboLoop";
            this.comboLoop.Size = new System.Drawing.Size(79, 21);
            this.comboLoop.TabIndex = 18;
            this.comboLoop.SelectedIndexChanged += new System.EventHandler(this.OnSoundInfoChanged);
            // 
            // comboSampleRateLabel
            // 
            this.comboSampleRateLabel.AutoSize = true;
            this.comboSampleRateLabel.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(69)))), ((int)(((byte)(73)))), ((int)(((byte)(74)))));
            this.comboSampleRateLabel.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.comboSampleRateLabel.Location = new System.Drawing.Point(234, 136);
            this.comboSampleRateLabel.Name = "comboSampleRateLabel";
            this.comboSampleRateLabel.Size = new System.Drawing.Size(20, 13);
            this.comboSampleRateLabel.TabIndex = 27;
            this.comboSampleRateLabel.Text = "Hz";
            // 
            // trackBar_1SecondMark
            // 
            this.trackBar_1SecondMark.AutoSize = true;
            this.trackBar_1SecondMark.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(51)))), ((int)(((byte)(51)))), ((int)(((byte)(51)))));
            this.trackBar_1SecondMark.Font = new System.Drawing.Font("Microsoft Sans Serif", 6F);
            this.trackBar_1SecondMark.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.trackBar_1SecondMark.Location = new System.Drawing.Point(120, 176);
            this.trackBar_1SecondMark.Name = "trackBar_1SecondMark";
            this.trackBar_1SecondMark.Size = new System.Drawing.Size(21, 9);
            this.trackBar_1SecondMark.TabIndex = 24;
            this.trackBar_1SecondMark.Text = "0.1 s";
            // 
            // darkLabel8
            // 
            this.darkLabel8.AutoSize = true;
            this.darkLabel8.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.darkLabel8.Location = new System.Drawing.Point(-1, 136);
            this.darkLabel8.Name = "darkLabel8";
            this.darkLabel8.Size = new System.Drawing.Size(201, 26);
            this.darkLabel8.TabIndex = 19;
            this.darkLabel8.Text = "List of samples, all converted to PCM\r\nIn game, one sample is randomly chosen.";
            // 
            // butPlayPreview
            // 
            this.butPlayPreview.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.butPlayPreview.Image = global::TombLib.Properties.Resources.actions_play_16;
            this.butPlayPreview.ImagePadding = 10;
            this.butPlayPreview.Location = new System.Drawing.Point(222, 0);
            this.butPlayPreview.Name = "butPlayPreview";
            this.butPlayPreview.Size = new System.Drawing.Size(220, 20);
            this.butPlayPreview.TabIndex = 2;
            this.butPlayPreview.Text = " Play preview (respects parameters)";
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
            this.dataGridView.Location = new System.Drawing.Point(0, 167);
            this.dataGridView.Name = "dataGridView";
            this.dataGridView.RowHeadersWidth = 41;
            this.dataGridView.RowTemplate.Height = 64;
            this.dataGridView.Size = new System.Drawing.Size(410, 179);
            this.dataGridView.TabIndex = 20;
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
            this.DurationColumn.Width = 71;
            // 
            // SizeColumn
            // 
            this.SizeColumn.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
            this.SizeColumn.HeaderText = "Size";
            this.SizeColumn.Name = "SizeColumn";
            this.SizeColumn.ReadOnly = true;
            this.SizeColumn.Width = 51;
            // 
            // darkLabel11
            // 
            this.darkLabel11.AutoSize = true;
            this.darkLabel11.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(69)))), ((int)(((byte)(73)))), ((int)(((byte)(74)))));
            this.darkLabel11.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.darkLabel11.Location = new System.Drawing.Point(79, 110);
            this.darkLabel11.Name = "darkLabel11";
            this.darkLabel11.Size = new System.Drawing.Size(15, 13);
            this.darkLabel11.TabIndex = 16;
            this.darkLabel11.Text = "%";
            // 
            // darkLabel10
            // 
            this.darkLabel10.AutoSize = true;
            this.darkLabel10.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(69)))), ((int)(((byte)(73)))), ((int)(((byte)(74)))));
            this.darkLabel10.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.darkLabel10.Location = new System.Drawing.Point(77, 55);
            this.darkLabel10.Name = "darkLabel10";
            this.darkLabel10.Size = new System.Drawing.Size(15, 13);
            this.darkLabel10.TabIndex = 9;
            this.darkLabel10.Text = "%";
            // 
            // darkLabel7
            // 
            this.darkLabel7.AutoSize = true;
            this.darkLabel7.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(69)))), ((int)(((byte)(73)))), ((int)(((byte)(74)))));
            this.darkLabel7.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.darkLabel7.Location = new System.Drawing.Point(76, 28);
            this.darkLabel7.Name = "darkLabel7";
            this.darkLabel7.Size = new System.Drawing.Size(15, 13);
            this.darkLabel7.TabIndex = 5;
            this.darkLabel7.Text = "%";
            // 
            // butExport
            // 
            this.butExport.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.butExport.Image = global::TombLib.Properties.Resources.general_Export_16;
            this.butExport.Location = new System.Drawing.Point(416, 227);
            this.butExport.Name = "butExport";
            this.butExport.Size = new System.Drawing.Size(26, 23);
            this.butExport.TabIndex = 22;
            this.butExport.Text = "Play";
            this.butExport.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.butExport.Click += new System.EventHandler(this.butExport_Click);
            // 
            // darkLabel6
            // 
            this.darkLabel6.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.darkLabel6.AutoSize = true;
            this.darkLabel6.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.darkLabel6.Location = new System.Drawing.Point(295, 107);
            this.darkLabel6.Name = "darkLabel6";
            this.darkLabel6.Size = new System.Drawing.Size(63, 13);
            this.darkLabel6.TabIndex = 17;
            this.darkLabel6.Text = "Loop mode:";
            // 
            // darkLabel5
            // 
            this.darkLabel5.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.darkLabel5.Location = new System.Drawing.Point(0, 108);
            this.darkLabel5.Name = "darkLabel5";
            this.darkLabel5.Size = new System.Drawing.Size(46, 13);
            this.darkLabel5.TabIndex = 14;
            this.darkLabel5.Text = "Chance:";
            this.darkLabel5.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // darkLabel4
            // 
            this.darkLabel4.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.darkLabel4.Location = new System.Drawing.Point(0, 56);
            this.darkLabel4.Name = "darkLabel4";
            this.darkLabel4.Size = new System.Drawing.Size(46, 13);
            this.darkLabel4.TabIndex = 7;
            this.darkLabel4.Text = "Pitch:";
            this.darkLabel4.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // darkLabel3
            // 
            this.darkLabel3.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.darkLabel3.Location = new System.Drawing.Point(0, 82);
            this.darkLabel3.Name = "darkLabel3";
            this.darkLabel3.Size = new System.Drawing.Size(46, 13);
            this.darkLabel3.TabIndex = 11;
            this.darkLabel3.Text = "Range:";
            this.darkLabel3.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // darkLabel2
            // 
            this.darkLabel2.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.darkLabel2.Location = new System.Drawing.Point(0, 29);
            this.darkLabel2.Name = "darkLabel2";
            this.darkLabel2.Size = new System.Drawing.Size(46, 13);
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
            this.tbName.Size = new System.Drawing.Size(170, 20);
            this.tbName.TabIndex = 1;
            this.tbName.TextChanged += new System.EventHandler(this.OnSoundInfoChanged);
            // 
            // darkLabel1
            // 
            this.darkLabel1.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.darkLabel1.Location = new System.Drawing.Point(0, 3);
            this.darkLabel1.Name = "darkLabel1";
            this.darkLabel1.Size = new System.Drawing.Size(46, 13);
            this.darkLabel1.TabIndex = 0;
            this.darkLabel1.Text = "Name:";
            this.darkLabel1.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // comboSampleRateTextBox
            // 
            this.comboSampleRateTextBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.comboSampleRateTextBox.Location = new System.Drawing.Point(201, 136);
            this.comboSampleRateTextBox.Name = "comboSampleRateTextBox";
            this.comboSampleRateTextBox.Size = new System.Drawing.Size(52, 13);
            this.comboSampleRateTextBox.TabIndex = 28;
            this.comboSampleRateTextBox.TextChanged += new System.EventHandler(this.comboSampleRateTextBox_TextChanged);
            this.comboSampleRateTextBox.KeyDown += new System.Windows.Forms.KeyEventHandler(this.comboSampleRateTextBox_KeyDown);
            this.comboSampleRateTextBox.Validating += new System.ComponentModel.CancelEventHandler(this.comboSampleRateTextBox_Validating);
            this.comboSampleRateTextBox.Validated += new System.EventHandler(this.comboSampleRateTextBox_Validated);
            // 
            // dataGridViewControls
            // 
            this.dataGridViewControls.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.dataGridViewControls.Enabled = false;
            this.dataGridViewControls.Location = new System.Drawing.Point(416, 167);
            this.dataGridViewControls.MinimumSize = new System.Drawing.Size(24, 24);
            this.dataGridViewControls.Name = "dataGridViewControls";
            this.dataGridViewControls.Size = new System.Drawing.Size(26, 179);
            this.dataGridViewControls.TabIndex = 21;
            // 
            // SoundInfoEditor
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.Controls.Add(this.comboSampleRateLabel);
            this.Controls.Add(this.trackBar_1SecondMark);
            this.Controls.Add(this.trackBar);
            this.Controls.Add(this.butPlayPreview);
            this.Controls.Add(this.dataGridView);
            this.Controls.Add(this.darkLabel11);
            this.Controls.Add(this.darkLabel10);
            this.Controls.Add(this.darkLabel7);
            this.Controls.Add(this.numericChance);
            this.Controls.Add(this.numericPitch);
            this.Controls.Add(this.numericRange);
            this.Controls.Add(this.numericVolume);
            this.Controls.Add(this.butExport);
            this.Controls.Add(this.comboLoop);
            this.Controls.Add(this.darkLabel6);
            this.Controls.Add(this.darkLabel5);
            this.Controls.Add(this.darkLabel4);
            this.Controls.Add(this.darkLabel3);
            this.Controls.Add(this.darkLabel2);
            this.Controls.Add(this.cbRandomizePitch);
            this.Controls.Add(this.cbRandomizeVolume);
            this.Controls.Add(this.cbDisablePanning);
            this.Controls.Add(this.tbName);
            this.Controls.Add(this.darkLabel1);
            this.Controls.Add(this.dataGridViewControls);
            this.Controls.Add(this.comboSampleRateTextBox);
            this.Controls.Add(this.comboSampleRate);
            this.Controls.Add(this.darkLabel8);
            this.MinimumSize = new System.Drawing.Size(442, 346);
            this.Name = "SoundInfoEditor";
            this.Size = new System.Drawing.Size(442, 346);
            ((System.ComponentModel.ISupportInitialize)(this.numericChance)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericPitch)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericRange)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericVolume)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.trackBar)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView)).EndInit();
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
        private DarkUI.Controls.DarkLabel darkLabel1;
        private System.Windows.Forms.ToolTip toolTip1;
        private DarkUI.Controls.DarkNumericUpDown numericVolume;
        private DarkUI.Controls.DarkNumericUpDown numericRange;
        private DarkUI.Controls.DarkNumericUpDown numericPitch;
        private DarkUI.Controls.DarkNumericUpDown numericChance;
        private DarkUI.Controls.DarkLabel darkLabel7;
        private DarkUI.Controls.DarkLabel darkLabel10;
        private DarkUI.Controls.DarkLabel darkLabel11;
        private DarkUI.Controls.DarkDataGridView dataGridView;
        private DarkDataGridViewControls dataGridViewControls;
        private DarkUI.Controls.DarkButton butPlayPreview;
        private DarkUI.Controls.DarkLabel darkLabel8;
        private System.Windows.Forms.DataGridViewImageColumn WaveformColumn;
        private DarkUI.Controls.DarkDataGridViewButtonColumn PlayButtonColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn DurationColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn SizeColumn;
        private System.Windows.Forms.TrackBar trackBar;
        private DarkUI.Controls.DarkLabel trackBar_1SecondMark;
        private DarkUI.Controls.DarkComboBox comboSampleRate;
        private DarkUI.Controls.DarkLabel comboSampleRateLabel;
        private DarkUI.Controls.DarkTextBox comboSampleRateTextBox;
    }
}
