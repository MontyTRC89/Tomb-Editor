namespace WadTool
{
    partial class FormAnimCommandsEditor
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
            this.btCancel = new DarkUI.Controls.DarkButton();
            this.btOk = new DarkUI.Controls.DarkButton();
            this.treeCommands = new DarkUI.Controls.DarkTreeView();
            this.comboCommandType = new DarkUI.Controls.DarkComboBox();
            this.panelPosition = new System.Windows.Forms.Panel();
            this.darkLabel3 = new DarkUI.Controls.DarkLabel();
            this.tbPosZ = new DarkUI.Controls.DarkNumericUpDown();
            this.darkLabel2 = new DarkUI.Controls.DarkLabel();
            this.tbPosY = new DarkUI.Controls.DarkNumericUpDown();
            this.darkLabel1 = new DarkUI.Controls.DarkLabel();
            this.tbPosX = new DarkUI.Controls.DarkNumericUpDown();
            this.panelJumpDistance = new System.Windows.Forms.Panel();
            this.darkLabel5 = new DarkUI.Controls.DarkLabel();
            this.tbVertical = new DarkUI.Controls.DarkNumericUpDown();
            this.darkLabel6 = new DarkUI.Controls.DarkLabel();
            this.tbHorizontal = new DarkUI.Controls.DarkNumericUpDown();
            this.panelEffect = new System.Windows.Forms.Panel();
            this.darkLabel4 = new DarkUI.Controls.DarkLabel();
            this.tbFlipEffect = new DarkUI.Controls.DarkNumericUpDown();
            this.darkLabel7 = new DarkUI.Controls.DarkLabel();
            this.tbFlipEffectFrame = new DarkUI.Controls.DarkNumericUpDown();
            this.panelSound = new System.Windows.Forms.Panel();
            this.groupBox = new DarkUI.Controls.DarkGroupBox();
            this.soundInfoEditor = new TombLib.Controls.SoundInfoEditor();
            this.darkLabel9 = new DarkUI.Controls.DarkLabel();
            this.comboPlaySoundConditions = new DarkUI.Controls.DarkComboBox();
            this.tbPlaySoundFrame = new DarkUI.Controls.DarkNumericUpDown();
            this.butAddEffect = new DarkUI.Controls.DarkButton();
            this.butDeleteEffect = new DarkUI.Controls.DarkButton();
            this.splitContainer = new System.Windows.Forms.SplitContainer();
            this.darkLabel8 = new DarkUI.Controls.DarkLabel();
            this.darkTreeView1 = new DarkUI.Controls.DarkTreeView();
            this.panelPosition.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.tbPosZ)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.tbPosY)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.tbPosX)).BeginInit();
            this.panelJumpDistance.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.tbVertical)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.tbHorizontal)).BeginInit();
            this.panelEffect.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.tbFlipEffect)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.tbFlipEffectFrame)).BeginInit();
            this.panelSound.SuspendLayout();
            this.groupBox.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.tbPlaySoundFrame)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer)).BeginInit();
            this.splitContainer.Panel1.SuspendLayout();
            this.splitContainer.Panel2.SuspendLayout();
            this.splitContainer.SuspendLayout();
            this.SuspendLayout();
            // 
            // btCancel
            // 
            this.btCancel.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.btCancel.Location = new System.Drawing.Point(348, 549);
            this.btCancel.Name = "btCancel";
            this.btCancel.Size = new System.Drawing.Size(113, 26);
            this.btCancel.TabIndex = 50;
            this.btCancel.Text = "Cancel";
            this.btCancel.Click += new System.EventHandler(this.btCancel_Click);
            // 
            // btOk
            // 
            this.btOk.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.btOk.Location = new System.Drawing.Point(467, 549);
            this.btOk.Name = "btOk";
            this.btOk.Size = new System.Drawing.Size(113, 26);
            this.btOk.TabIndex = 51;
            this.btOk.Text = "Ok";
            this.btOk.Click += new System.EventHandler(this.btOk_Click);
            // 
            // treeCommands
            // 
            this.treeCommands.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.treeCommands.Location = new System.Drawing.Point(6, 3);
            this.treeCommands.MaxDragChange = 20;
            this.treeCommands.Name = "treeCommands";
            this.treeCommands.Size = new System.Drawing.Size(455, 505);
            this.treeCommands.TabIndex = 52;
            this.treeCommands.SelectedNodesChanged += new System.EventHandler(this.treeCommands_SelectedNodesChanged);
            // 
            // comboCommandType
            // 
            this.comboCommandType.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.comboCommandType.Enabled = false;
            this.comboCommandType.FormattingEnabled = true;
            this.comboCommandType.Items.AddRange(new object[] {
            "Set position",
            "Set jump distance",
            "Empty hands",
            "Kill entity",
            "Play sound",
            "Flipeffect"});
            this.comboCommandType.Location = new System.Drawing.Point(45, 4);
            this.comboCommandType.Name = "comboCommandType";
            this.comboCommandType.Size = new System.Drawing.Size(413, 23);
            this.comboCommandType.TabIndex = 53;
            this.comboCommandType.SelectedIndexChanged += new System.EventHandler(this.comboCommandType_SelectedIndexChanged);
            // 
            // panelPosition
            // 
            this.panelPosition.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panelPosition.Controls.Add(this.darkLabel3);
            this.panelPosition.Controls.Add(this.tbPosZ);
            this.panelPosition.Controls.Add(this.darkLabel2);
            this.panelPosition.Controls.Add(this.tbPosY);
            this.panelPosition.Controls.Add(this.darkLabel1);
            this.panelPosition.Controls.Add(this.tbPosX);
            this.panelPosition.Location = new System.Drawing.Point(3, 33);
            this.panelPosition.Name = "panelPosition";
            this.panelPosition.Size = new System.Drawing.Size(455, 31);
            this.panelPosition.TabIndex = 54;
            this.panelPosition.Visible = false;
            // 
            // darkLabel3
            // 
            this.darkLabel3.AutoSize = true;
            this.darkLabel3.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.darkLabel3.Location = new System.Drawing.Point(219, 6);
            this.darkLabel3.Name = "darkLabel3";
            this.darkLabel3.Size = new System.Drawing.Size(16, 13);
            this.darkLabel3.TabIndex = 5;
            this.darkLabel3.Text = "Z:";
            // 
            // tbPosZ
            // 
            this.tbPosZ.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(69)))), ((int)(((byte)(73)))), ((int)(((byte)(74)))));
            this.tbPosZ.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.tbPosZ.IncrementAlternate = new decimal(new int[] {
            10,
            0,
            0,
            65536});
            this.tbPosZ.Location = new System.Drawing.Point(241, 4);
            this.tbPosZ.Maximum = new decimal(new int[] {
            32767,
            0,
            0,
            0});
            this.tbPosZ.Minimum = new decimal(new int[] {
            32768,
            0,
            0,
            -2147483648});
            this.tbPosZ.MousewheelSingleIncrement = true;
            this.tbPosZ.Name = "tbPosZ";
            this.tbPosZ.Size = new System.Drawing.Size(73, 22);
            this.tbPosZ.TabIndex = 4;
            this.tbPosZ.ValueChanged += new System.EventHandler(this.tbPosZ_ValueChanged);
            // 
            // darkLabel2
            // 
            this.darkLabel2.AutoSize = true;
            this.darkLabel2.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.darkLabel2.Location = new System.Drawing.Point(110, 6);
            this.darkLabel2.Name = "darkLabel2";
            this.darkLabel2.Size = new System.Drawing.Size(15, 13);
            this.darkLabel2.TabIndex = 3;
            this.darkLabel2.Text = "Y:";
            // 
            // tbPosY
            // 
            this.tbPosY.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(69)))), ((int)(((byte)(73)))), ((int)(((byte)(74)))));
            this.tbPosY.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.tbPosY.IncrementAlternate = new decimal(new int[] {
            10,
            0,
            0,
            65536});
            this.tbPosY.Location = new System.Drawing.Point(131, 4);
            this.tbPosY.Maximum = new decimal(new int[] {
            32767,
            0,
            0,
            0});
            this.tbPosY.Minimum = new decimal(new int[] {
            32768,
            0,
            0,
            -2147483648});
            this.tbPosY.MousewheelSingleIncrement = true;
            this.tbPosY.Name = "tbPosY";
            this.tbPosY.Size = new System.Drawing.Size(73, 22);
            this.tbPosY.TabIndex = 2;
            this.tbPosY.ValueChanged += new System.EventHandler(this.tbPosY_ValueChanged);
            // 
            // darkLabel1
            // 
            this.darkLabel1.AutoSize = true;
            this.darkLabel1.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.darkLabel1.Location = new System.Drawing.Point(3, 6);
            this.darkLabel1.Name = "darkLabel1";
            this.darkLabel1.Size = new System.Drawing.Size(16, 13);
            this.darkLabel1.TabIndex = 1;
            this.darkLabel1.Text = "X:";
            // 
            // tbPosX
            // 
            this.tbPosX.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(69)))), ((int)(((byte)(73)))), ((int)(((byte)(74)))));
            this.tbPosX.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.tbPosX.IncrementAlternate = new decimal(new int[] {
            10,
            0,
            0,
            65536});
            this.tbPosX.Location = new System.Drawing.Point(25, 3);
            this.tbPosX.Maximum = new decimal(new int[] {
            32767,
            0,
            0,
            0});
            this.tbPosX.Minimum = new decimal(new int[] {
            32768,
            0,
            0,
            -2147483648});
            this.tbPosX.MousewheelSingleIncrement = true;
            this.tbPosX.Name = "tbPosX";
            this.tbPosX.Size = new System.Drawing.Size(73, 22);
            this.tbPosX.TabIndex = 0;
            this.tbPosX.ValueChanged += new System.EventHandler(this.tbPosX_ValueChanged);
            // 
            // panelJumpDistance
            // 
            this.panelJumpDistance.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panelJumpDistance.Controls.Add(this.darkLabel5);
            this.panelJumpDistance.Controls.Add(this.tbVertical);
            this.panelJumpDistance.Controls.Add(this.darkLabel6);
            this.panelJumpDistance.Controls.Add(this.tbHorizontal);
            this.panelJumpDistance.Location = new System.Drawing.Point(3, 70);
            this.panelJumpDistance.Name = "panelJumpDistance";
            this.panelJumpDistance.Size = new System.Drawing.Size(455, 29);
            this.panelJumpDistance.TabIndex = 55;
            this.panelJumpDistance.Visible = false;
            // 
            // darkLabel5
            // 
            this.darkLabel5.AutoSize = true;
            this.darkLabel5.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.darkLabel5.Location = new System.Drawing.Point(188, 6);
            this.darkLabel5.Name = "darkLabel5";
            this.darkLabel5.Size = new System.Drawing.Size(48, 13);
            this.darkLabel5.TabIndex = 3;
            this.darkLabel5.Text = "Vertical:";
            // 
            // tbVertical
            // 
            this.tbVertical.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(69)))), ((int)(((byte)(73)))), ((int)(((byte)(74)))));
            this.tbVertical.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.tbVertical.IncrementAlternate = new decimal(new int[] {
            10,
            0,
            0,
            65536});
            this.tbVertical.Location = new System.Drawing.Point(241, 3);
            this.tbVertical.Maximum = new decimal(new int[] {
            32767,
            0,
            0,
            0});
            this.tbVertical.Minimum = new decimal(new int[] {
            32768,
            0,
            0,
            -2147483648});
            this.tbVertical.MousewheelSingleIncrement = true;
            this.tbVertical.Name = "tbVertical";
            this.tbVertical.Size = new System.Drawing.Size(73, 22);
            this.tbVertical.TabIndex = 2;
            this.tbVertical.ValueChanged += new System.EventHandler(this.tbVertical_ValueChanged);
            // 
            // darkLabel6
            // 
            this.darkLabel6.AutoSize = true;
            this.darkLabel6.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.darkLabel6.Location = new System.Drawing.Point(3, 6);
            this.darkLabel6.Name = "darkLabel6";
            this.darkLabel6.Size = new System.Drawing.Size(64, 13);
            this.darkLabel6.TabIndex = 1;
            this.darkLabel6.Text = "Horizontal:";
            // 
            // tbHorizontal
            // 
            this.tbHorizontal.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(69)))), ((int)(((byte)(73)))), ((int)(((byte)(74)))));
            this.tbHorizontal.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.tbHorizontal.IncrementAlternate = new decimal(new int[] {
            10,
            0,
            0,
            65536});
            this.tbHorizontal.Location = new System.Drawing.Point(73, 3);
            this.tbHorizontal.Maximum = new decimal(new int[] {
            32767,
            0,
            0,
            0});
            this.tbHorizontal.Minimum = new decimal(new int[] {
            32768,
            0,
            0,
            -2147483648});
            this.tbHorizontal.MousewheelSingleIncrement = true;
            this.tbHorizontal.Name = "tbHorizontal";
            this.tbHorizontal.Size = new System.Drawing.Size(73, 22);
            this.tbHorizontal.TabIndex = 0;
            this.tbHorizontal.ValueChanged += new System.EventHandler(this.tbHorizontal_ValueChanged);
            // 
            // panelEffect
            // 
            this.panelEffect.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panelEffect.Controls.Add(this.darkLabel4);
            this.panelEffect.Controls.Add(this.tbFlipEffect);
            this.panelEffect.Controls.Add(this.darkLabel7);
            this.panelEffect.Controls.Add(this.tbFlipEffectFrame);
            this.panelEffect.Location = new System.Drawing.Point(3, 105);
            this.panelEffect.Name = "panelEffect";
            this.panelEffect.Size = new System.Drawing.Size(455, 29);
            this.panelEffect.TabIndex = 56;
            this.panelEffect.Visible = false;
            // 
            // darkLabel4
            // 
            this.darkLabel4.AutoSize = true;
            this.darkLabel4.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.darkLabel4.Location = new System.Drawing.Point(188, 6);
            this.darkLabel4.Name = "darkLabel4";
            this.darkLabel4.Size = new System.Drawing.Size(39, 13);
            this.darkLabel4.TabIndex = 3;
            this.darkLabel4.Text = "Effect:";
            // 
            // tbFlipEffect
            // 
            this.tbFlipEffect.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(69)))), ((int)(((byte)(73)))), ((int)(((byte)(74)))));
            this.tbFlipEffect.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.tbFlipEffect.IncrementAlternate = new decimal(new int[] {
            10,
            0,
            0,
            65536});
            this.tbFlipEffect.Location = new System.Drawing.Point(241, 3);
            this.tbFlipEffect.Maximum = new decimal(new int[] {
            32767,
            0,
            0,
            0});
            this.tbFlipEffect.Minimum = new decimal(new int[] {
            32768,
            0,
            0,
            -2147483648});
            this.tbFlipEffect.MousewheelSingleIncrement = true;
            this.tbFlipEffect.Name = "tbFlipEffect";
            this.tbFlipEffect.Size = new System.Drawing.Size(73, 22);
            this.tbFlipEffect.TabIndex = 2;
            this.tbFlipEffect.ValueChanged += new System.EventHandler(this.tbFlipEffect_ValueChanged);
            // 
            // darkLabel7
            // 
            this.darkLabel7.AutoSize = true;
            this.darkLabel7.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.darkLabel7.Location = new System.Drawing.Point(3, 6);
            this.darkLabel7.Name = "darkLabel7";
            this.darkLabel7.Size = new System.Drawing.Size(41, 13);
            this.darkLabel7.TabIndex = 1;
            this.darkLabel7.Text = "Frame:";
            // 
            // tbFlipEffectFrame
            // 
            this.tbFlipEffectFrame.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(69)))), ((int)(((byte)(73)))), ((int)(((byte)(74)))));
            this.tbFlipEffectFrame.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.tbFlipEffectFrame.IncrementAlternate = new decimal(new int[] {
            10,
            0,
            0,
            65536});
            this.tbFlipEffectFrame.Location = new System.Drawing.Point(73, 3);
            this.tbFlipEffectFrame.Maximum = new decimal(new int[] {
            32767,
            0,
            0,
            0});
            this.tbFlipEffectFrame.Minimum = new decimal(new int[] {
            32768,
            0,
            0,
            -2147483648});
            this.tbFlipEffectFrame.MousewheelSingleIncrement = true;
            this.tbFlipEffectFrame.Name = "tbFlipEffectFrame";
            this.tbFlipEffectFrame.Size = new System.Drawing.Size(73, 22);
            this.tbFlipEffectFrame.TabIndex = 0;
            this.tbFlipEffectFrame.ValueChanged += new System.EventHandler(this.tbFlipEffectFrame_ValueChanged);
            // 
            // panelSound
            // 
            this.panelSound.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panelSound.Controls.Add(this.groupBox);
            this.panelSound.Controls.Add(this.darkLabel9);
            this.panelSound.Controls.Add(this.comboPlaySoundConditions);
            this.panelSound.Controls.Add(this.tbPlaySoundFrame);
            this.panelSound.Location = new System.Drawing.Point(3, 140);
            this.panelSound.Name = "panelSound";
            this.panelSound.Size = new System.Drawing.Size(455, 400);
            this.panelSound.TabIndex = 57;
            this.panelSound.Visible = false;
            // 
            // groupBox
            // 
            this.groupBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox.Controls.Add(this.soundInfoEditor);
            this.groupBox.Location = new System.Drawing.Point(2, 29);
            this.groupBox.Name = "groupBox";
            this.groupBox.Size = new System.Drawing.Size(453, 371);
            this.groupBox.TabIndex = 98;
            this.groupBox.TabStop = false;
            this.groupBox.Text = "Sound settings";
            // 
            // soundInfoEditor
            // 
            this.soundInfoEditor.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.soundInfoEditor.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.soundInfoEditor.Location = new System.Drawing.Point(6, 22);
            this.soundInfoEditor.MinimumSize = new System.Drawing.Size(440, 346);
            this.soundInfoEditor.Name = "soundInfoEditor";
            this.soundInfoEditor.Size = new System.Drawing.Size(440, 346);
            this.soundInfoEditor.TabIndex = 98;
            // 
            // darkLabel9
            // 
            this.darkLabel9.AutoSize = true;
            this.darkLabel9.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.darkLabel9.Location = new System.Drawing.Point(3, 6);
            this.darkLabel9.Name = "darkLabel9";
            this.darkLabel9.Size = new System.Drawing.Size(41, 13);
            this.darkLabel9.TabIndex = 1;
            this.darkLabel9.Text = "Frame:";
            // 
            // comboPlaySoundConditions
            // 
            this.comboPlaySoundConditions.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.comboPlaySoundConditions.FormattingEnabled = true;
            this.comboPlaySoundConditions.Items.AddRange(new object[] {
            "Always",
            "Only when dry",
            "Only in the water"});
            this.comboPlaySoundConditions.Location = new System.Drawing.Point(191, 2);
            this.comboPlaySoundConditions.Name = "comboPlaySoundConditions";
            this.comboPlaySoundConditions.Size = new System.Drawing.Size(123, 23);
            this.comboPlaySoundConditions.TabIndex = 53;
            this.comboPlaySoundConditions.SelectedIndexChanged += new System.EventHandler(this.comboPlaySoundConditions_SelectedIndexChanged);
            // 
            // tbPlaySoundFrame
            // 
            this.tbPlaySoundFrame.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(69)))), ((int)(((byte)(73)))), ((int)(((byte)(74)))));
            this.tbPlaySoundFrame.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.tbPlaySoundFrame.IncrementAlternate = new decimal(new int[] {
            10,
            0,
            0,
            65536});
            this.tbPlaySoundFrame.Location = new System.Drawing.Point(73, 3);
            this.tbPlaySoundFrame.Maximum = new decimal(new int[] {
            32767,
            0,
            0,
            0});
            this.tbPlaySoundFrame.Minimum = new decimal(new int[] {
            32768,
            0,
            0,
            -2147483648});
            this.tbPlaySoundFrame.MousewheelSingleIncrement = true;
            this.tbPlaySoundFrame.Name = "tbPlaySoundFrame";
            this.tbPlaySoundFrame.Size = new System.Drawing.Size(73, 22);
            this.tbPlaySoundFrame.TabIndex = 0;
            this.tbPlaySoundFrame.ValueChanged += new System.EventHandler(this.tbPlaySoundFrame_ValueChanged);
            // 
            // butAddEffect
            // 
            this.butAddEffect.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.butAddEffect.Image = global::WadTool.Properties.Resources.plus_math_16;
            this.butAddEffect.Location = new System.Drawing.Point(292, 514);
            this.butAddEffect.Name = "butAddEffect";
            this.butAddEffect.Size = new System.Drawing.Size(83, 23);
            this.butAddEffect.TabIndex = 95;
            this.butAddEffect.Text = "Add new";
            this.butAddEffect.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.butAddEffect.Click += new System.EventHandler(this.butAddEffect_Click);
            // 
            // butDeleteEffect
            // 
            this.butDeleteEffect.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.butDeleteEffect.Image = global::WadTool.Properties.Resources.trash_161;
            this.butDeleteEffect.Location = new System.Drawing.Point(381, 514);
            this.butDeleteEffect.Name = "butDeleteEffect";
            this.butDeleteEffect.Size = new System.Drawing.Size(80, 23);
            this.butDeleteEffect.TabIndex = 94;
            this.butDeleteEffect.Text = "Delete";
            this.butDeleteEffect.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.butDeleteEffect.Click += new System.EventHandler(this.butDeleteEffect_Click);
            // 
            // splitContainer
            // 
            this.splitContainer.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.splitContainer.Location = new System.Drawing.Point(-2, 0);
            this.splitContainer.MinimumSize = new System.Drawing.Size(866, 543);
            this.splitContainer.Name = "splitContainer";
            // 
            // splitContainer.Panel1
            // 
            this.splitContainer.Panel1.Controls.Add(this.treeCommands);
            this.splitContainer.Panel1.Controls.Add(this.butAddEffect);
            this.splitContainer.Panel1.Controls.Add(this.butDeleteEffect);
            // 
            // splitContainer.Panel2
            // 
            this.splitContainer.Panel2.Controls.Add(this.darkLabel8);
            this.splitContainer.Panel2.Controls.Add(this.comboCommandType);
            this.splitContainer.Panel2.Controls.Add(this.panelSound);
            this.splitContainer.Panel2.Controls.Add(this.panelEffect);
            this.splitContainer.Panel2.Controls.Add(this.panelJumpDistance);
            this.splitContainer.Panel2.Controls.Add(this.panelPosition);
            this.splitContainer.Size = new System.Drawing.Size(929, 543);
            this.splitContainer.SplitterDistance = 464;
            this.splitContainer.TabIndex = 98;
            // 
            // darkLabel8
            // 
            this.darkLabel8.AutoSize = true;
            this.darkLabel8.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.darkLabel8.Location = new System.Drawing.Point(6, 7);
            this.darkLabel8.Name = "darkLabel8";
            this.darkLabel8.Size = new System.Drawing.Size(33, 13);
            this.darkLabel8.TabIndex = 1;
            this.darkLabel8.Text = "Type:";
            // 
            // darkTreeView1
            // 
            this.darkTreeView1.Location = new System.Drawing.Point(446, 548);
            this.darkTreeView1.MaxDragChange = 20;
            this.darkTreeView1.Name = "darkTreeView1";
            this.darkTreeView1.Size = new System.Drawing.Size(8, 8);
            this.darkTreeView1.TabIndex = 99;
            this.darkTreeView1.Text = "darkTreeView1";
            // 
            // FormAnimCommandsEditor
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(929, 585);
            this.Controls.Add(this.darkTreeView1);
            this.Controls.Add(this.splitContainer);
            this.Controls.Add(this.btCancel);
            this.Controls.Add(this.btOk);
            this.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(937, 612);
            this.Name = "FormAnimCommandsEditor";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Anim commands editor";
            this.panelPosition.ResumeLayout(false);
            this.panelPosition.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.tbPosZ)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.tbPosY)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.tbPosX)).EndInit();
            this.panelJumpDistance.ResumeLayout(false);
            this.panelJumpDistance.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.tbVertical)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.tbHorizontal)).EndInit();
            this.panelEffect.ResumeLayout(false);
            this.panelEffect.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.tbFlipEffect)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.tbFlipEffectFrame)).EndInit();
            this.panelSound.ResumeLayout(false);
            this.panelSound.PerformLayout();
            this.groupBox.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.tbPlaySoundFrame)).EndInit();
            this.splitContainer.Panel1.ResumeLayout(false);
            this.splitContainer.Panel2.ResumeLayout(false);
            this.splitContainer.Panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer)).EndInit();
            this.splitContainer.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion
        private DarkUI.Controls.DarkButton btCancel;
        private DarkUI.Controls.DarkButton btOk;
        private DarkUI.Controls.DarkTreeView treeCommands;
        private DarkUI.Controls.DarkComboBox comboCommandType;
        private System.Windows.Forms.Panel panelPosition;
        private DarkUI.Controls.DarkLabel darkLabel3;
        private DarkUI.Controls.DarkNumericUpDown tbPosZ;
        private DarkUI.Controls.DarkLabel darkLabel2;
        private DarkUI.Controls.DarkNumericUpDown tbPosY;
        private DarkUI.Controls.DarkLabel darkLabel1;
        private DarkUI.Controls.DarkNumericUpDown tbPosX;
        private System.Windows.Forms.Panel panelJumpDistance;
        private DarkUI.Controls.DarkLabel darkLabel5;
        private DarkUI.Controls.DarkNumericUpDown tbVertical;
        private DarkUI.Controls.DarkLabel darkLabel6;
        private DarkUI.Controls.DarkNumericUpDown tbHorizontal;
        private System.Windows.Forms.Panel panelEffect;
        private DarkUI.Controls.DarkLabel darkLabel4;
        private DarkUI.Controls.DarkNumericUpDown tbFlipEffect;
        private DarkUI.Controls.DarkLabel darkLabel7;
        private DarkUI.Controls.DarkNumericUpDown tbFlipEffectFrame;
        private System.Windows.Forms.Panel panelSound;
        private DarkUI.Controls.DarkLabel darkLabel9;
        private DarkUI.Controls.DarkNumericUpDown tbPlaySoundFrame;
        private DarkUI.Controls.DarkButton butAddEffect;
        private DarkUI.Controls.DarkButton butDeleteEffect;
        private System.Windows.Forms.SplitContainer splitContainer;
        private DarkUI.Controls.DarkLabel darkLabel8;
        private DarkUI.Controls.DarkComboBox comboPlaySoundConditions;
        private DarkUI.Controls.DarkGroupBox groupBox;
        private DarkUI.Controls.DarkTreeView darkTreeView1;
        private TombLib.Controls.SoundInfoEditor soundInfoEditor;
    }
}