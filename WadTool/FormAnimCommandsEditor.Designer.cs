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
            this.butAddEffect = new DarkUI.Controls.DarkButton();
            this.butDeleteEffect = new DarkUI.Controls.DarkButton();
            this.darkLabel8 = new DarkUI.Controls.DarkLabel();
            this.darkGroupBox1 = new DarkUI.Controls.DarkGroupBox();
            this.butCommandDown = new DarkUI.Controls.DarkButton();
            this.butCommandUp = new DarkUI.Controls.DarkButton();
            this.darkGroupBox2 = new DarkUI.Controls.DarkGroupBox();
            this.darkSectionPanel1 = new DarkUI.Controls.DarkSectionPanel();
            this.commandControls = new TombLib.Controls.DarkTabbedContainer();
            this.tabSetPosition = new System.Windows.Forms.TabPage();
            this.darkLabel3 = new DarkUI.Controls.DarkLabel();
            this.tbPosX = new DarkUI.Controls.DarkNumericUpDown();
            this.tbPosZ = new DarkUI.Controls.DarkNumericUpDown();
            this.darkLabel1 = new DarkUI.Controls.DarkLabel();
            this.darkLabel2 = new DarkUI.Controls.DarkLabel();
            this.tbPosY = new DarkUI.Controls.DarkNumericUpDown();
            this.tabSetJumpDistance = new System.Windows.Forms.TabPage();
            this.darkLabel5 = new DarkUI.Controls.DarkLabel();
            this.darkLabel6 = new DarkUI.Controls.DarkLabel();
            this.tbVertical = new DarkUI.Controls.DarkNumericUpDown();
            this.tbHorizontal = new DarkUI.Controls.DarkNumericUpDown();
            this.tabFlipeffect = new System.Windows.Forms.TabPage();
            this.darkLabel4 = new DarkUI.Controls.DarkLabel();
            this.darkLabel7 = new DarkUI.Controls.DarkLabel();
            this.tbFlipEffectFrame = new DarkUI.Controls.DarkNumericUpDown();
            this.tbFlipEffect = new DarkUI.Controls.DarkNumericUpDown();
            this.tabPlaySound = new System.Windows.Forms.TabPage();
            this.darkLabel11 = new DarkUI.Controls.DarkLabel();
            this.darkLabel10 = new DarkUI.Controls.DarkLabel();
            this.comboSound = new DarkUI.Controls.DarkComboBox();
            this.tbPlaySoundFrame = new DarkUI.Controls.DarkNumericUpDown();
            this.darkLabel9 = new DarkUI.Controls.DarkLabel();
            this.comboPlaySoundConditions = new DarkUI.Controls.DarkComboBox();
            this.darkGroupBox1.SuspendLayout();
            this.darkGroupBox2.SuspendLayout();
            this.darkSectionPanel1.SuspendLayout();
            this.commandControls.SuspendLayout();
            this.tabSetPosition.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.tbPosX)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.tbPosZ)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.tbPosY)).BeginInit();
            this.tabSetJumpDistance.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.tbVertical)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.tbHorizontal)).BeginInit();
            this.tabFlipeffect.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.tbFlipEffectFrame)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.tbFlipEffect)).BeginInit();
            this.tabPlaySound.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.tbPlaySoundFrame)).BeginInit();
            this.SuspendLayout();
            // 
            // btCancel
            // 
            this.btCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btCancel.Location = new System.Drawing.Point(296, 292);
            this.btCancel.Name = "btCancel";
            this.btCancel.Size = new System.Drawing.Size(80, 23);
            this.btCancel.TabIndex = 50;
            this.btCancel.Text = "Cancel";
            this.btCancel.Click += new System.EventHandler(this.btCancel_Click);
            // 
            // btOk
            // 
            this.btOk.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btOk.Location = new System.Drawing.Point(210, 292);
            this.btOk.Name = "btOk";
            this.btOk.Size = new System.Drawing.Size(80, 23);
            this.btOk.TabIndex = 51;
            this.btOk.Text = "OK";
            this.btOk.Click += new System.EventHandler(this.btOk_Click);
            // 
            // treeCommands
            // 
            this.treeCommands.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.treeCommands.Location = new System.Drawing.Point(6, 6);
            this.treeCommands.MaxDragChange = 20;
            this.treeCommands.Name = "treeCommands";
            this.treeCommands.Size = new System.Drawing.Size(329, 122);
            this.treeCommands.TabIndex = 52;
            this.treeCommands.SelectedNodesChanged += new System.EventHandler(this.treeCommands_SelectedNodesChanged);
            // 
            // comboCommandType
            // 
            this.comboCommandType.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.comboCommandType.FormattingEnabled = true;
            this.comboCommandType.Items.AddRange(new object[] {
            "Set position",
            "Set jump distance",
            "Empty hands",
            "Kill entity",
            "Play sound",
            "Flipeffect"});
            this.comboCommandType.Location = new System.Drawing.Point(45, 7);
            this.comboCommandType.Name = "comboCommandType";
            this.comboCommandType.Size = new System.Drawing.Size(320, 23);
            this.comboCommandType.TabIndex = 53;
            this.comboCommandType.SelectedIndexChanged += new System.EventHandler(this.comboCommandType_SelectedIndexChanged);
            // 
            // butAddEffect
            // 
            this.butAddEffect.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.butAddEffect.Image = global::WadTool.Properties.Resources.plus_math_16;
            this.butAddEffect.Location = new System.Drawing.Point(341, 6);
            this.butAddEffect.Name = "butAddEffect";
            this.butAddEffect.Size = new System.Drawing.Size(24, 24);
            this.butAddEffect.TabIndex = 95;
            this.butAddEffect.Click += new System.EventHandler(this.butAddEffect_Click);
            // 
            // butDeleteEffect
            // 
            this.butDeleteEffect.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.butDeleteEffect.Image = global::WadTool.Properties.Resources.trash_161;
            this.butDeleteEffect.Location = new System.Drawing.Point(341, 36);
            this.butDeleteEffect.Name = "butDeleteEffect";
            this.butDeleteEffect.Size = new System.Drawing.Size(24, 24);
            this.butDeleteEffect.TabIndex = 94;
            this.butDeleteEffect.Click += new System.EventHandler(this.butDeleteEffect_Click);
            // 
            // darkLabel8
            // 
            this.darkLabel8.AutoSize = true;
            this.darkLabel8.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.darkLabel8.Location = new System.Drawing.Point(6, 10);
            this.darkLabel8.Name = "darkLabel8";
            this.darkLabel8.Size = new System.Drawing.Size(33, 13);
            this.darkLabel8.TabIndex = 1;
            this.darkLabel8.Text = "Type:";
            // 
            // darkGroupBox1
            // 
            this.darkGroupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.darkGroupBox1.Controls.Add(this.butCommandDown);
            this.darkGroupBox1.Controls.Add(this.butCommandUp);
            this.darkGroupBox1.Controls.Add(this.butAddEffect);
            this.darkGroupBox1.Controls.Add(this.treeCommands);
            this.darkGroupBox1.Controls.Add(this.butDeleteEffect);
            this.darkGroupBox1.Location = new System.Drawing.Point(5, 5);
            this.darkGroupBox1.Name = "darkGroupBox1";
            this.darkGroupBox1.Size = new System.Drawing.Size(371, 134);
            this.darkGroupBox1.TabIndex = 99;
            this.darkGroupBox1.TabStop = false;
            // 
            // butCommandDown
            // 
            this.butCommandDown.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.butCommandDown.Image = global::WadTool.Properties.Resources.general_ArrowDown_16;
            this.butCommandDown.Location = new System.Drawing.Point(341, 104);
            this.butCommandDown.Name = "butCommandDown";
            this.butCommandDown.Size = new System.Drawing.Size(24, 24);
            this.butCommandDown.TabIndex = 97;
            this.butCommandDown.Click += new System.EventHandler(this.butCommandDown_Click);
            // 
            // butCommandUp
            // 
            this.butCommandUp.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.butCommandUp.Image = global::WadTool.Properties.Resources.general_ArrowUp_16;
            this.butCommandUp.Location = new System.Drawing.Point(341, 76);
            this.butCommandUp.Name = "butCommandUp";
            this.butCommandUp.Size = new System.Drawing.Size(24, 24);
            this.butCommandUp.TabIndex = 96;
            this.butCommandUp.Click += new System.EventHandler(this.butCommandUp_Click);
            // 
            // darkGroupBox2
            // 
            this.darkGroupBox2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.darkGroupBox2.Controls.Add(this.darkSectionPanel1);
            this.darkGroupBox2.Controls.Add(this.darkLabel8);
            this.darkGroupBox2.Controls.Add(this.comboCommandType);
            this.darkGroupBox2.Location = new System.Drawing.Point(5, 143);
            this.darkGroupBox2.Name = "darkGroupBox2";
            this.darkGroupBox2.Size = new System.Drawing.Size(371, 143);
            this.darkGroupBox2.TabIndex = 100;
            this.darkGroupBox2.TabStop = false;
            // 
            // darkSectionPanel1
            // 
            this.darkSectionPanel1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.darkSectionPanel1.Controls.Add(this.commandControls);
            this.darkSectionPanel1.Location = new System.Drawing.Point(6, 36);
            this.darkSectionPanel1.Name = "darkSectionPanel1";
            this.darkSectionPanel1.SectionHeader = null;
            this.darkSectionPanel1.Size = new System.Drawing.Size(359, 101);
            this.darkSectionPanel1.TabIndex = 54;
            // 
            // commandControls
            // 
            this.commandControls.Controls.Add(this.tabSetPosition);
            this.commandControls.Controls.Add(this.tabSetJumpDistance);
            this.commandControls.Controls.Add(this.tabFlipeffect);
            this.commandControls.Controls.Add(this.tabPlaySound);
            this.commandControls.Dock = System.Windows.Forms.DockStyle.Fill;
            this.commandControls.Location = new System.Drawing.Point(1, 1);
            this.commandControls.Name = "commandControls";
            this.commandControls.SelectedIndex = 0;
            this.commandControls.Size = new System.Drawing.Size(357, 99);
            this.commandControls.TabIndex = 0;
            // 
            // tabSetPosition
            // 
            this.tabSetPosition.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.tabSetPosition.Controls.Add(this.darkLabel3);
            this.tabSetPosition.Controls.Add(this.tbPosX);
            this.tabSetPosition.Controls.Add(this.tbPosZ);
            this.tabSetPosition.Controls.Add(this.darkLabel1);
            this.tabSetPosition.Controls.Add(this.darkLabel2);
            this.tabSetPosition.Controls.Add(this.tbPosY);
            this.tabSetPosition.Location = new System.Drawing.Point(4, 22);
            this.tabSetPosition.Name = "tabSetPosition";
            this.tabSetPosition.Padding = new System.Windows.Forms.Padding(3);
            this.tabSetPosition.Size = new System.Drawing.Size(349, 73);
            this.tabSetPosition.TabIndex = 0;
            this.tabSetPosition.Text = "setPosition";
            // 
            // darkLabel3
            // 
            this.darkLabel3.AutoSize = true;
            this.darkLabel3.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.darkLabel3.Location = new System.Drawing.Point(164, 8);
            this.darkLabel3.Name = "darkLabel3";
            this.darkLabel3.Size = new System.Drawing.Size(16, 13);
            this.darkLabel3.TabIndex = 5;
            this.darkLabel3.Text = "Z:";
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
            this.tbPosX.Location = new System.Drawing.Point(9, 24);
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
            // tbPosZ
            // 
            this.tbPosZ.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(69)))), ((int)(((byte)(73)))), ((int)(((byte)(74)))));
            this.tbPosZ.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.tbPosZ.IncrementAlternate = new decimal(new int[] {
            10,
            0,
            0,
            65536});
            this.tbPosZ.Location = new System.Drawing.Point(167, 24);
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
            // darkLabel1
            // 
            this.darkLabel1.AutoSize = true;
            this.darkLabel1.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.darkLabel1.Location = new System.Drawing.Point(6, 8);
            this.darkLabel1.Name = "darkLabel1";
            this.darkLabel1.Size = new System.Drawing.Size(16, 13);
            this.darkLabel1.TabIndex = 1;
            this.darkLabel1.Text = "X:";
            // 
            // darkLabel2
            // 
            this.darkLabel2.AutoSize = true;
            this.darkLabel2.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.darkLabel2.Location = new System.Drawing.Point(85, 8);
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
            this.tbPosY.Location = new System.Drawing.Point(88, 24);
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
            // tabSetJumpDistance
            // 
            this.tabSetJumpDistance.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.tabSetJumpDistance.Controls.Add(this.darkLabel5);
            this.tabSetJumpDistance.Controls.Add(this.darkLabel6);
            this.tabSetJumpDistance.Controls.Add(this.tbVertical);
            this.tabSetJumpDistance.Controls.Add(this.tbHorizontal);
            this.tabSetJumpDistance.Location = new System.Drawing.Point(4, 22);
            this.tabSetJumpDistance.Name = "tabSetJumpDistance";
            this.tabSetJumpDistance.Padding = new System.Windows.Forms.Padding(3);
            this.tabSetJumpDistance.Size = new System.Drawing.Size(349, 73);
            this.tabSetJumpDistance.TabIndex = 1;
            this.tabSetJumpDistance.Text = "setJumpDistance";
            // 
            // darkLabel5
            // 
            this.darkLabel5.AutoSize = true;
            this.darkLabel5.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.darkLabel5.Location = new System.Drawing.Point(85, 8);
            this.darkLabel5.Name = "darkLabel5";
            this.darkLabel5.Size = new System.Drawing.Size(48, 13);
            this.darkLabel5.TabIndex = 3;
            this.darkLabel5.Text = "Vertical:";
            // 
            // darkLabel6
            // 
            this.darkLabel6.AutoSize = true;
            this.darkLabel6.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.darkLabel6.Location = new System.Drawing.Point(6, 8);
            this.darkLabel6.Name = "darkLabel6";
            this.darkLabel6.Size = new System.Drawing.Size(64, 13);
            this.darkLabel6.TabIndex = 1;
            this.darkLabel6.Text = "Horizontal:";
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
            this.tbVertical.Location = new System.Drawing.Point(88, 24);
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
            // tbHorizontal
            // 
            this.tbHorizontal.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(69)))), ((int)(((byte)(73)))), ((int)(((byte)(74)))));
            this.tbHorizontal.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.tbHorizontal.IncrementAlternate = new decimal(new int[] {
            10,
            0,
            0,
            65536});
            this.tbHorizontal.Location = new System.Drawing.Point(9, 24);
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
            // tabFlipeffect
            // 
            this.tabFlipeffect.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.tabFlipeffect.Controls.Add(this.darkLabel4);
            this.tabFlipeffect.Controls.Add(this.darkLabel7);
            this.tabFlipeffect.Controls.Add(this.tbFlipEffectFrame);
            this.tabFlipeffect.Controls.Add(this.tbFlipEffect);
            this.tabFlipeffect.Location = new System.Drawing.Point(4, 22);
            this.tabFlipeffect.Name = "tabFlipeffect";
            this.tabFlipeffect.Padding = new System.Windows.Forms.Padding(3);
            this.tabFlipeffect.Size = new System.Drawing.Size(349, 73);
            this.tabFlipeffect.TabIndex = 2;
            this.tabFlipeffect.Text = "flipeffect";
            // 
            // darkLabel4
            // 
            this.darkLabel4.AutoSize = true;
            this.darkLabel4.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.darkLabel4.Location = new System.Drawing.Point(88, 8);
            this.darkLabel4.Name = "darkLabel4";
            this.darkLabel4.Size = new System.Drawing.Size(39, 13);
            this.darkLabel4.TabIndex = 5;
            this.darkLabel4.Text = "Effect:";
            // 
            // darkLabel7
            // 
            this.darkLabel7.AutoSize = true;
            this.darkLabel7.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.darkLabel7.Location = new System.Drawing.Point(6, 8);
            this.darkLabel7.Name = "darkLabel7";
            this.darkLabel7.Size = new System.Drawing.Size(41, 13);
            this.darkLabel7.TabIndex = 4;
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
            this.tbFlipEffectFrame.Location = new System.Drawing.Point(9, 24);
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
            // tbFlipEffect
            // 
            this.tbFlipEffect.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(69)))), ((int)(((byte)(73)))), ((int)(((byte)(74)))));
            this.tbFlipEffect.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.tbFlipEffect.IncrementAlternate = new decimal(new int[] {
            10,
            0,
            0,
            65536});
            this.tbFlipEffect.Location = new System.Drawing.Point(88, 24);
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
            // tabPlaySound
            // 
            this.tabPlaySound.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.tabPlaySound.Controls.Add(this.darkLabel11);
            this.tabPlaySound.Controls.Add(this.darkLabel10);
            this.tabPlaySound.Controls.Add(this.comboSound);
            this.tabPlaySound.Controls.Add(this.tbPlaySoundFrame);
            this.tabPlaySound.Controls.Add(this.darkLabel9);
            this.tabPlaySound.Controls.Add(this.comboPlaySoundConditions);
            this.tabPlaySound.Location = new System.Drawing.Point(4, 22);
            this.tabPlaySound.Name = "tabPlaySound";
            this.tabPlaySound.Padding = new System.Windows.Forms.Padding(3);
            this.tabPlaySound.Size = new System.Drawing.Size(349, 73);
            this.tabPlaySound.TabIndex = 3;
            this.tabPlaySound.Text = "playSound";
            // 
            // darkLabel11
            // 
            this.darkLabel11.AutoSize = true;
            this.darkLabel11.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.darkLabel11.Location = new System.Drawing.Point(85, 8);
            this.darkLabel11.Name = "darkLabel11";
            this.darkLabel11.Size = new System.Drawing.Size(62, 13);
            this.darkLabel11.TabIndex = 102;
            this.darkLabel11.Text = "Condition:";
            // 
            // darkLabel10
            // 
            this.darkLabel10.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.darkLabel10.Location = new System.Drawing.Point(6, 56);
            this.darkLabel10.Name = "darkLabel10";
            this.darkLabel10.Size = new System.Drawing.Size(46, 20);
            this.darkLabel10.TabIndex = 101;
            this.darkLabel10.Text = "Sound:";
            // 
            // comboSound
            // 
            this.comboSound.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.comboSound.FormattingEnabled = true;
            this.comboSound.Location = new System.Drawing.Point(53, 53);
            this.comboSound.Name = "comboSound";
            this.comboSound.Size = new System.Drawing.Size(290, 23);
            this.comboSound.TabIndex = 100;
            this.comboSound.SelectedIndexChanged += new System.EventHandler(this.ComboSound_SelectedIndexChanged);
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
            this.tbPlaySoundFrame.Location = new System.Drawing.Point(9, 24);
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
            // darkLabel9
            // 
            this.darkLabel9.AutoSize = true;
            this.darkLabel9.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.darkLabel9.Location = new System.Drawing.Point(6, 8);
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
            this.comboPlaySoundConditions.Location = new System.Drawing.Point(88, 24);
            this.comboPlaySoundConditions.Name = "comboPlaySoundConditions";
            this.comboPlaySoundConditions.Size = new System.Drawing.Size(255, 23);
            this.comboPlaySoundConditions.TabIndex = 53;
            this.comboPlaySoundConditions.SelectedIndexChanged += new System.EventHandler(this.comboPlaySoundConditions_SelectedIndexChanged);
            // 
            // FormAnimCommandsEditor
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(381, 320);
            this.Controls.Add(this.darkGroupBox2);
            this.Controls.Add(this.darkGroupBox1);
            this.Controls.Add(this.btCancel);
            this.Controls.Add(this.btOk);
            this.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(397, 334);
            this.Name = "FormAnimCommandsEditor";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Anim commands editor";
            this.darkGroupBox1.ResumeLayout(false);
            this.darkGroupBox2.ResumeLayout(false);
            this.darkGroupBox2.PerformLayout();
            this.darkSectionPanel1.ResumeLayout(false);
            this.commandControls.ResumeLayout(false);
            this.tabSetPosition.ResumeLayout(false);
            this.tabSetPosition.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.tbPosX)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.tbPosZ)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.tbPosY)).EndInit();
            this.tabSetJumpDistance.ResumeLayout(false);
            this.tabSetJumpDistance.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.tbVertical)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.tbHorizontal)).EndInit();
            this.tabFlipeffect.ResumeLayout(false);
            this.tabFlipeffect.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.tbFlipEffectFrame)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.tbFlipEffect)).EndInit();
            this.tabPlaySound.ResumeLayout(false);
            this.tabPlaySound.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.tbPlaySoundFrame)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion
        private DarkUI.Controls.DarkButton btCancel;
        private DarkUI.Controls.DarkButton btOk;
        private DarkUI.Controls.DarkTreeView treeCommands;
        private DarkUI.Controls.DarkComboBox comboCommandType;
        private DarkUI.Controls.DarkLabel darkLabel3;
        private DarkUI.Controls.DarkNumericUpDown tbPosZ;
        private DarkUI.Controls.DarkLabel darkLabel2;
        private DarkUI.Controls.DarkNumericUpDown tbPosY;
        private DarkUI.Controls.DarkLabel darkLabel1;
        private DarkUI.Controls.DarkNumericUpDown tbPosX;
        private DarkUI.Controls.DarkLabel darkLabel5;
        private DarkUI.Controls.DarkNumericUpDown tbVertical;
        private DarkUI.Controls.DarkLabel darkLabel6;
        private DarkUI.Controls.DarkNumericUpDown tbHorizontal;
        private DarkUI.Controls.DarkNumericUpDown tbFlipEffect;
        private DarkUI.Controls.DarkNumericUpDown tbFlipEffectFrame;
        private DarkUI.Controls.DarkLabel darkLabel9;
        private DarkUI.Controls.DarkNumericUpDown tbPlaySoundFrame;
        private DarkUI.Controls.DarkButton butAddEffect;
        private DarkUI.Controls.DarkButton butDeleteEffect;
        private DarkUI.Controls.DarkLabel darkLabel8;
        private DarkUI.Controls.DarkComboBox comboPlaySoundConditions;
        private DarkUI.Controls.DarkComboBox comboSound;
        private DarkUI.Controls.DarkGroupBox darkGroupBox1;
        private DarkUI.Controls.DarkButton butCommandDown;
        private DarkUI.Controls.DarkButton butCommandUp;
        private DarkUI.Controls.DarkGroupBox darkGroupBox2;
        private DarkUI.Controls.DarkSectionPanel darkSectionPanel1;
        private TombLib.Controls.DarkTabbedContainer commandControls;
        private System.Windows.Forms.TabPage tabSetPosition;
        private System.Windows.Forms.TabPage tabSetJumpDistance;
        private System.Windows.Forms.TabPage tabFlipeffect;
        private System.Windows.Forms.TabPage tabPlaySound;
        private DarkUI.Controls.DarkLabel darkLabel10;
        private DarkUI.Controls.DarkLabel darkLabel4;
        private DarkUI.Controls.DarkLabel darkLabel7;
        private DarkUI.Controls.DarkLabel darkLabel11;
    }
}