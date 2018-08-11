using System.Windows.Forms;
using DarkUI.Controls;

namespace TombEditor.Forms
{
    partial class FormAnimatedTextures
    {
        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.darkLabel1 = new DarkUI.Controls.DarkLabel();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.panel2 = new System.Windows.Forms.Panel();
            this.comboCurrentTexture = new DarkUI.Controls.DarkComboBox();
            this.textureMap = new TombEditor.Forms.FormAnimatedTextures.PanelTextureMapForAnimations();
            this.panel1 = new System.Windows.Forms.Panel();
            this.butEditSetName = new DarkUI.Controls.DarkButton();
            this.lblProcAnim = new DarkUI.Controls.DarkLabel();
            this.panel3 = new System.Windows.Forms.Panel();
            this.butAddProcAnim = new DarkUI.Controls.DarkButton();
            this.butReplaceProcAnim = new DarkUI.Controls.DarkButton();
            this.butMergeProcAnim = new DarkUI.Controls.DarkButton();
            this.butCloneProcAnim = new DarkUI.Controls.DarkButton();
            this.cbSmooth = new DarkUI.Controls.DarkCheckBox();
            this.cbLoop = new DarkUI.Controls.DarkCheckBox();
            this.numFrames = new DarkUI.Controls.DarkNumericUpDown();
            this.lblFrames = new DarkUI.Controls.DarkLabel();
            this.lblProgress = new DarkUI.Controls.DarkLabel();
            this.lblAnimatorName = new DarkUI.Controls.DarkLabel();
            this.numStrength = new DarkUI.Controls.DarkNumericUpDown();
            this.butGenerateProcAnim = new DarkUI.Controls.DarkButton();
            this.comboProcPresets = new DarkUI.Controls.DarkComboBox();
            this.butUpdate = new DarkUI.Controls.DarkButton();
            this.label1 = new DarkUI.Controls.DarkLabel();
            this.tooManyFramesWarning = new DarkUI.Controls.DarkLabel();
            this.previewProgressBar = new DarkUI.Controls.DarkProgressBar();
            this.texturesDataGridView = new DarkUI.Controls.DarkDataGridView();
            this.texturesDataGridViewColumnImage = new System.Windows.Forms.DataGridViewImageColumn();
            this.texturesDataGridViewColumnRepeat = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.texturesDataGridViewColumnTexture = new System.Windows.Forms.DataGridViewComboBoxColumn();
            this.texturesDataGridViewColumnArea = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.texturesDataGridViewColumnTexCoord0 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.texturesDataGridViewColumnTexCoord1 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.texturesDataGridViewColumnTexCoord2 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.texturesDataGridViewColumnTexCoord3 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.lblHeaderNgSettings = new DarkUI.Controls.DarkLabel();
            this.lblPreview = new DarkUI.Controls.DarkLabel();
            this.settingsPanelNG = new System.Windows.Forms.Panel();
            this.lblUvRotate = new DarkUI.Controls.DarkLabel();
            this.comboUvRotate = new DarkUI.Controls.DarkComboBox();
            this.lblFps = new DarkUI.Controls.DarkLabel();
            this.comboFps = new DarkUI.Controls.DarkComboBox();
            this.lblEffect = new DarkUI.Controls.DarkLabel();
            this.comboEffect = new DarkUI.Controls.DarkComboBox();
            this.previewImage = new System.Windows.Forms.PictureBox();
            this.texturesDataGridViewControls = new TombLib.Controls.DarkDataGridViewControls();
            this.butAnimatedTextureSetDelete = new DarkUI.Controls.DarkButton();
            this.butAnimatedTextureSetNew = new DarkUI.Controls.DarkButton();
            this.comboAnimatedTextureSets = new DarkUI.Controls.DarkComboBox();
            this.butOk = new DarkUI.Controls.DarkButton();
            this.toolTip = new System.Windows.Forms.ToolTip(this.components);
            this.tableLayoutPanel1.SuspendLayout();
            this.panel2.SuspendLayout();
            this.panel1.SuspendLayout();
            this.panel3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numFrames)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numStrength)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.texturesDataGridView)).BeginInit();
            this.settingsPanelNG.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.previewImage)).BeginInit();
            this.SuspendLayout();
            // 
            // darkLabel1
            // 
            this.darkLabel1.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.darkLabel1.Location = new System.Drawing.Point(1, 8);
            this.darkLabel1.Name = "darkLabel1";
            this.darkLabel1.Size = new System.Drawing.Size(82, 17);
            this.darkLabel1.TabIndex = 0;
            this.darkLabel1.Text = "Animation set:";
            this.darkLabel1.TextAlign = System.Drawing.ContentAlignment.BottomLeft;
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tableLayoutPanel1.ColumnCount = 2;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 56.49652F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 43.50348F));
            this.tableLayoutPanel1.Controls.Add(this.panel2, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.panel1, 0, 0);
            this.tableLayoutPanel1.Location = new System.Drawing.Point(4, 4);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 1;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 618F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(862, 618);
            this.tableLayoutPanel1.TabIndex = 43;
            // 
            // panel2
            // 
            this.panel2.Controls.Add(this.comboCurrentTexture);
            this.panel2.Controls.Add(this.textureMap);
            this.panel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel2.Location = new System.Drawing.Point(490, 3);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(369, 612);
            this.panel2.TabIndex = 21;
            // 
            // comboCurrentTexture
            // 
            this.comboCurrentTexture.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.comboCurrentTexture.FormattingEnabled = true;
            this.comboCurrentTexture.Location = new System.Drawing.Point(4, 5);
            this.comboCurrentTexture.Name = "comboCurrentTexture";
            this.comboCurrentTexture.Size = new System.Drawing.Size(359, 23);
            this.comboCurrentTexture.TabIndex = 4;
            this.comboCurrentTexture.DropDown += new System.EventHandler(this.comboCurrentTexture_DropDown);
            this.comboCurrentTexture.SelectedValueChanged += new System.EventHandler(this.comboCurrentTexture_SelectedValueChanged);
            // 
            // textureMap
            // 
            this.textureMap.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textureMap.Location = new System.Drawing.Point(4, 32);
            this.textureMap.Margin = new System.Windows.Forms.Padding(3, 30, 3, 3);
            this.textureMap.Name = "textureMap";
            this.textureMap.Size = new System.Drawing.Size(359, 578);
            this.textureMap.TabIndex = 0;
            this.textureMap.DoubleClick += new System.EventHandler(this.textureMap_DoubleClick);
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.butEditSetName);
            this.panel1.Controls.Add(this.lblProcAnim);
            this.panel1.Controls.Add(this.panel3);
            this.panel1.Controls.Add(this.butUpdate);
            this.panel1.Controls.Add(this.label1);
            this.panel1.Controls.Add(this.tooManyFramesWarning);
            this.panel1.Controls.Add(this.previewProgressBar);
            this.panel1.Controls.Add(this.texturesDataGridView);
            this.panel1.Controls.Add(this.lblHeaderNgSettings);
            this.panel1.Controls.Add(this.lblPreview);
            this.panel1.Controls.Add(this.settingsPanelNG);
            this.panel1.Controls.Add(this.previewImage);
            this.panel1.Controls.Add(this.texturesDataGridViewControls);
            this.panel1.Controls.Add(this.butAnimatedTextureSetDelete);
            this.panel1.Controls.Add(this.butAnimatedTextureSetNew);
            this.panel1.Controls.Add(this.darkLabel1);
            this.panel1.Controls.Add(this.comboAnimatedTextureSets);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel1.Location = new System.Drawing.Point(3, 3);
            this.panel1.Margin = new System.Windows.Forms.Padding(3, 3, 3, 2);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(481, 613);
            this.panel1.TabIndex = 0;
            // 
            // butEditSetName
            // 
            this.butEditSetName.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.butEditSetName.Image = global::TombEditor.Properties.Resources.general_edit_16;
            this.butEditSetName.Location = new System.Drawing.Point(399, 5);
            this.butEditSetName.Name = "butEditSetName";
            this.butEditSetName.Size = new System.Drawing.Size(24, 24);
            this.butEditSetName.TabIndex = 22;
            this.butEditSetName.Tag = "EditRoomName";
            this.butEditSetName.Click += new System.EventHandler(this.butEditSetName_Click);
            // 
            // lblProcAnim
            // 
            this.lblProcAnim.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.lblProcAnim.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.lblProcAnim.Location = new System.Drawing.Point(1, 431);
            this.lblProcAnim.Name = "lblProcAnim";
            this.lblProcAnim.Size = new System.Drawing.Size(120, 18);
            this.lblProcAnim.TabIndex = 21;
            this.lblProcAnim.Text = "Procedural animation";
            this.lblProcAnim.TextAlign = System.Drawing.ContentAlignment.BottomLeft;
            // 
            // panel3
            // 
            this.panel3.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panel3.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panel3.Controls.Add(this.butAddProcAnim);
            this.panel3.Controls.Add(this.butReplaceProcAnim);
            this.panel3.Controls.Add(this.butMergeProcAnim);
            this.panel3.Controls.Add(this.butCloneProcAnim);
            this.panel3.Controls.Add(this.cbSmooth);
            this.panel3.Controls.Add(this.cbLoop);
            this.panel3.Controls.Add(this.numFrames);
            this.panel3.Controls.Add(this.lblFrames);
            this.panel3.Controls.Add(this.lblProgress);
            this.panel3.Controls.Add(this.lblAnimatorName);
            this.panel3.Controls.Add(this.numStrength);
            this.panel3.Controls.Add(this.butGenerateProcAnim);
            this.panel3.Controls.Add(this.comboProcPresets);
            this.panel3.Location = new System.Drawing.Point(5, 452);
            this.panel3.Name = "panel3";
            this.panel3.Size = new System.Drawing.Size(335, 81);
            this.panel3.TabIndex = 20;
            // 
            // butAddProcAnim
            // 
            this.butAddProcAnim.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.butAddProcAnim.Image = global::TombEditor.Properties.Resources.general_plus_math_16;
            this.butAddProcAnim.Location = new System.Drawing.Point(218, 51);
            this.butAddProcAnim.Name = "butAddProcAnim";
            this.butAddProcAnim.Size = new System.Drawing.Size(23, 23);
            this.butAddProcAnim.TabIndex = 23;
            this.butAddProcAnim.Tag = "";
            this.toolTip.SetToolTip(this.butAddProcAnim, "Generate and add frames after current sequence frame");
            this.butAddProcAnim.Click += new System.EventHandler(this.butAddProcAnim_Click);
            // 
            // butReplaceProcAnim
            // 
            this.butReplaceProcAnim.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.butReplaceProcAnim.Image = global::TombEditor.Properties.Resources.actions_refresh_16;
            this.butReplaceProcAnim.Location = new System.Drawing.Point(305, 51);
            this.butReplaceProcAnim.Name = "butReplaceProcAnim";
            this.butReplaceProcAnim.Size = new System.Drawing.Size(23, 23);
            this.butReplaceProcAnim.TabIndex = 22;
            this.butReplaceProcAnim.Tag = "";
            this.toolTip.SetToolTip(this.butReplaceProcAnim, "Generate and replace current animation");
            this.butReplaceProcAnim.Click += new System.EventHandler(this.butReplaceProcAnim_Click);
            // 
            // butMergeProcAnim
            // 
            this.butMergeProcAnim.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.butMergeProcAnim.Image = global::TombEditor.Properties.Resources.actions_Merge_16;
            this.butMergeProcAnim.Location = new System.Drawing.Point(276, 51);
            this.butMergeProcAnim.Name = "butMergeProcAnim";
            this.butMergeProcAnim.Size = new System.Drawing.Size(23, 23);
            this.butMergeProcAnim.TabIndex = 21;
            this.butMergeProcAnim.Tag = "";
            this.toolTip.SetToolTip(this.butMergeProcAnim, "Generate and merge into current animation");
            this.butMergeProcAnim.Click += new System.EventHandler(this.butMergeProcAnim_Click);
            // 
            // butCloneProcAnim
            // 
            this.butCloneProcAnim.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.butCloneProcAnim.Image = global::TombEditor.Properties.Resources.general_copy_16;
            this.butCloneProcAnim.Location = new System.Drawing.Point(247, 51);
            this.butCloneProcAnim.Name = "butCloneProcAnim";
            this.butCloneProcAnim.Size = new System.Drawing.Size(23, 23);
            this.butCloneProcAnim.TabIndex = 20;
            this.butCloneProcAnim.Tag = "";
            this.toolTip.SetToolTip(this.butCloneProcAnim, "Generate and merge into copy of current animation");
            this.butCloneProcAnim.Click += new System.EventHandler(this.butCloneProcAnim_Click);
            // 
            // cbSmooth
            // 
            this.cbSmooth.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.cbSmooth.Location = new System.Drawing.Point(87, 55);
            this.cbSmooth.Name = "cbSmooth";
            this.cbSmooth.Size = new System.Drawing.Size(91, 17);
            this.cbSmooth.TabIndex = 19;
            this.cbSmooth.Text = "Smooth";
            this.toolTip.SetToolTip(this.cbSmooth, "Ease-in and ease-out animation");
            // 
            // cbLoop
            // 
            this.cbLoop.Location = new System.Drawing.Point(5, 55);
            this.cbLoop.Name = "cbLoop";
            this.cbLoop.Size = new System.Drawing.Size(72, 17);
            this.cbLoop.TabIndex = 18;
            this.cbLoop.Text = "Symmetric";
            this.toolTip.SetToolTip(this.cbLoop, "Make animation symmetric");
            // 
            // numFrames
            // 
            this.numFrames.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.numFrames.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(69)))), ((int)(((byte)(73)))), ((int)(((byte)(74)))));
            this.numFrames.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.numFrames.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.numFrames.IncrementAlternate = new decimal(new int[] {
            10,
            0,
            0,
            65536});
            this.numFrames.Location = new System.Drawing.Point(261, 22);
            this.numFrames.Maximum = new decimal(new int[] {
            32767,
            0,
            0,
            0});
            this.numFrames.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.numFrames.MousewheelSingleIncrement = true;
            this.numFrames.Name = "numFrames";
            this.numFrames.Size = new System.Drawing.Size(67, 23);
            this.numFrames.TabIndex = 15;
            this.toolTip.SetToolTip(this.numFrames, "Amount of resulting animation frames");
            this.numFrames.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            // 
            // lblFrames
            // 
            this.lblFrames.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblFrames.AutoSize = true;
            this.lblFrames.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.lblFrames.Location = new System.Drawing.Point(262, 6);
            this.lblFrames.Name = "lblFrames";
            this.lblFrames.Size = new System.Drawing.Size(46, 13);
            this.lblFrames.TabIndex = 14;
            this.lblFrames.Text = "Frames:";
            // 
            // lblProgress
            // 
            this.lblProgress.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblProgress.AutoSize = true;
            this.lblProgress.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.lblProgress.Location = new System.Drawing.Point(186, 6);
            this.lblProgress.Name = "lblProgress";
            this.lblProgress.Size = new System.Drawing.Size(66, 13);
            this.lblProgress.TabIndex = 12;
            this.lblProgress.Text = "Progress %:";
            // 
            // lblAnimatorName
            // 
            this.lblAnimatorName.AutoSize = true;
            this.lblAnimatorName.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.lblAnimatorName.Location = new System.Drawing.Point(3, 7);
            this.lblAnimatorName.Name = "lblAnimatorName";
            this.lblAnimatorName.Size = new System.Drawing.Size(57, 13);
            this.lblAnimatorName.TabIndex = 11;
            this.lblAnimatorName.Text = "Animator:";
            // 
            // numStrength
            // 
            this.numStrength.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.numStrength.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(69)))), ((int)(((byte)(73)))), ((int)(((byte)(74)))));
            this.numStrength.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.numStrength.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.numStrength.IncrementAlternate = new decimal(new int[] {
            10,
            0,
            0,
            65536});
            this.numStrength.Location = new System.Drawing.Point(189, 22);
            this.numStrength.Minimum = new decimal(new int[] {
            100,
            0,
            0,
            -2147483648});
            this.numStrength.MousewheelSingleIncrement = true;
            this.numStrength.Name = "numStrength";
            this.numStrength.Size = new System.Drawing.Size(66, 23);
            this.numStrength.TabIndex = 10;
            this.toolTip.SetToolTip(this.numStrength, "Effect progress or strength");
            this.numStrength.Value = new decimal(new int[] {
            100,
            0,
            0,
            0});
            // 
            // butGenerateProcAnim
            // 
            this.butGenerateProcAnim.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.butGenerateProcAnim.Image = global::TombEditor.Properties.Resources.general_create_new_16;
            this.butGenerateProcAnim.Location = new System.Drawing.Point(189, 51);
            this.butGenerateProcAnim.Name = "butGenerateProcAnim";
            this.butGenerateProcAnim.Size = new System.Drawing.Size(23, 23);
            this.butGenerateProcAnim.TabIndex = 8;
            this.butGenerateProcAnim.Tag = "";
            this.toolTip.SetToolTip(this.butGenerateProcAnim, "Generate new animation");
            this.butGenerateProcAnim.Click += new System.EventHandler(this.butGenerateProcAnim_Click);
            // 
            // comboProcPresets
            // 
            this.comboProcPresets.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.comboProcPresets.Items.AddRange(new object[] {
            "Stretch horizontal",
            "Stretch vertical",
            "Scale",
            "Skew horizontal ↖",
            "Skew horizontal ↗",
            "Skew vertical ↖",
            "Skew vertical ↗",
            "Spin",
            "Pan horizontal",
            "Pan vertical",
            "Shake"});
            this.comboProcPresets.Location = new System.Drawing.Point(5, 22);
            this.comboProcPresets.Name = "comboProcPresets";
            this.comboProcPresets.Size = new System.Drawing.Size(178, 23);
            this.comboProcPresets.TabIndex = 7;
            this.toolTip.SetToolTip(this.comboProcPresets, "Animation type");
            // 
            // butUpdate
            // 
            this.butUpdate.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.butUpdate.Enabled = false;
            this.butUpdate.Image = global::TombEditor.Properties.Resources.general_undo_16;
            this.butUpdate.ImagePadding = 4;
            this.butUpdate.Location = new System.Drawing.Point(457, 115);
            this.butUpdate.Margin = new System.Windows.Forms.Padding(0, 0, 2, 0);
            this.butUpdate.Name = "butUpdate";
            this.butUpdate.Size = new System.Drawing.Size(24, 24);
            this.butUpdate.TabIndex = 2;
            this.butUpdate.Click += new System.EventHandler(this.butUpdate_Click);
            // 
            // label1
            // 
            this.label1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.label1.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.label1.Location = new System.Drawing.Point(1, 37);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(473, 15);
            this.label1.TabIndex = 17;
            this.label1.Text = "Frames";
            this.label1.TextAlign = System.Drawing.ContentAlignment.BottomLeft;
            // 
            // tooManyFramesWarning
            // 
            this.tooManyFramesWarning.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tooManyFramesWarning.BackColor = System.Drawing.Color.Firebrick;
            this.tooManyFramesWarning.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.tooManyFramesWarning.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.tooManyFramesWarning.Image = global::TombEditor.Properties.Resources.general_Warning_16;
            this.tooManyFramesWarning.ImageAlign = System.Drawing.ContentAlignment.TopCenter;
            this.tooManyFramesWarning.Location = new System.Drawing.Point(457, 145);
            this.tooManyFramesWarning.Name = "tooManyFramesWarning";
            this.tooManyFramesWarning.Size = new System.Drawing.Size(24, 223);
            this.tooManyFramesWarning.TabIndex = 19;
            this.tooManyFramesWarning.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.tooManyFramesWarning.Visible = false;
            // 
            // previewProgressBar
            // 
            this.previewProgressBar.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.previewProgressBar.Location = new System.Drawing.Point(346, 587);
            this.previewProgressBar.Maximum = 0;
            this.previewProgressBar.Name = "previewProgressBar";
            this.previewProgressBar.Size = new System.Drawing.Size(135, 23);
            this.previewProgressBar.Style = System.Windows.Forms.ProgressBarStyle.Continuous;
            this.previewProgressBar.TabIndex = 18;
            this.previewProgressBar.TextMode = DarkUI.Controls.DarkProgressBarMode.XOfN;
            // 
            // texturesDataGridView
            // 
            this.texturesDataGridView.AllowUserToAddRows = false;
            this.texturesDataGridView.AllowUserToPasteCells = false;
            this.texturesDataGridView.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.texturesDataGridView.AutoGenerateColumns = false;
            this.texturesDataGridView.ColumnHeadersHeight = 17;
            this.texturesDataGridView.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.texturesDataGridViewColumnImage,
            this.texturesDataGridViewColumnRepeat,
            this.texturesDataGridViewColumnTexture,
            this.texturesDataGridViewColumnArea,
            this.texturesDataGridViewColumnTexCoord0,
            this.texturesDataGridViewColumnTexCoord1,
            this.texturesDataGridViewColumnTexCoord2,
            this.texturesDataGridViewColumnTexCoord3});
            this.texturesDataGridView.Enabled = false;
            this.texturesDataGridView.Location = new System.Drawing.Point(4, 55);
            this.texturesDataGridView.Name = "texturesDataGridView";
            this.texturesDataGridView.RowHeadersWidth = 41;
            this.texturesDataGridView.RowTemplate.Height = 48;
            this.texturesDataGridView.Size = new System.Drawing.Size(447, 373);
            this.texturesDataGridView.TabIndex = 15;
            this.texturesDataGridView.CellDoubleClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.texturesDataGridView_CellDoubleClick);
            this.texturesDataGridView.CellFormatting += new System.Windows.Forms.DataGridViewCellFormattingEventHandler(this.texturesDataGridView_CellFormatting);
            this.texturesDataGridView.CellParsing += new System.Windows.Forms.DataGridViewCellParsingEventHandler(this.texturesDataGridView_CellParsing);
            this.texturesDataGridView.SelectionChanged += new System.EventHandler(this.texturesDataGridView_SelectionChanged);
            // 
            // texturesDataGridViewColumnImage
            // 
            this.texturesDataGridViewColumnImage.HeaderText = "Image";
            this.texturesDataGridViewColumnImage.ImageLayout = System.Windows.Forms.DataGridViewImageCellLayout.Zoom;
            this.texturesDataGridViewColumnImage.Name = "texturesDataGridViewColumnImage";
            this.texturesDataGridViewColumnImage.ReadOnly = true;
            this.texturesDataGridViewColumnImage.Width = 48;
            // 
            // texturesDataGridViewColumnRepeat
            // 
            this.texturesDataGridViewColumnRepeat.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.DisplayedCells;
            this.texturesDataGridViewColumnRepeat.DataPropertyName = "Repeat";
            this.texturesDataGridViewColumnRepeat.HeaderText = "Repeat";
            this.texturesDataGridViewColumnRepeat.Name = "texturesDataGridViewColumnRepeat";
            this.texturesDataGridViewColumnRepeat.Width = 67;
            // 
            // texturesDataGridViewColumnTexture
            // 
            this.texturesDataGridViewColumnTexture.DataPropertyName = "Texture";
            this.texturesDataGridViewColumnTexture.DisplayStyle = System.Windows.Forms.DataGridViewComboBoxDisplayStyle.ComboBox;
            this.texturesDataGridViewColumnTexture.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.texturesDataGridViewColumnTexture.HeaderText = "Texture";
            this.texturesDataGridViewColumnTexture.Name = "texturesDataGridViewColumnTexture";
            this.texturesDataGridViewColumnTexture.Width = 80;
            // 
            // texturesDataGridViewColumnArea
            // 
            this.texturesDataGridViewColumnArea.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
            this.texturesDataGridViewColumnArea.HeaderText = "Area";
            this.texturesDataGridViewColumnArea.Name = "texturesDataGridViewColumnArea";
            this.texturesDataGridViewColumnArea.ReadOnly = true;
            this.texturesDataGridViewColumnArea.Width = 54;
            // 
            // texturesDataGridViewColumnTexCoord0
            // 
            this.texturesDataGridViewColumnTexCoord0.DataPropertyName = "TexCoord0";
            this.texturesDataGridViewColumnTexCoord0.HeaderText = "Edge 0";
            this.texturesDataGridViewColumnTexCoord0.Name = "texturesDataGridViewColumnTexCoord0";
            this.texturesDataGridViewColumnTexCoord0.Width = 70;
            // 
            // texturesDataGridViewColumnTexCoord1
            // 
            this.texturesDataGridViewColumnTexCoord1.DataPropertyName = "TexCoord1";
            this.texturesDataGridViewColumnTexCoord1.HeaderText = "Edge 1";
            this.texturesDataGridViewColumnTexCoord1.Name = "texturesDataGridViewColumnTexCoord1";
            this.texturesDataGridViewColumnTexCoord1.Width = 70;
            // 
            // texturesDataGridViewColumnTexCoord2
            // 
            this.texturesDataGridViewColumnTexCoord2.DataPropertyName = "TexCoord2";
            this.texturesDataGridViewColumnTexCoord2.HeaderText = "Edge 2";
            this.texturesDataGridViewColumnTexCoord2.Name = "texturesDataGridViewColumnTexCoord2";
            this.texturesDataGridViewColumnTexCoord2.Width = 70;
            // 
            // texturesDataGridViewColumnTexCoord3
            // 
            this.texturesDataGridViewColumnTexCoord3.DataPropertyName = "TexCoord3";
            this.texturesDataGridViewColumnTexCoord3.HeaderText = "Edge 3";
            this.texturesDataGridViewColumnTexCoord3.Name = "texturesDataGridViewColumnTexCoord3";
            this.texturesDataGridViewColumnTexCoord3.Width = 70;
            // 
            // lblHeaderNgSettings
            // 
            this.lblHeaderNgSettings.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.lblHeaderNgSettings.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.lblHeaderNgSettings.Location = new System.Drawing.Point(1, 535);
            this.lblHeaderNgSettings.Name = "lblHeaderNgSettings";
            this.lblHeaderNgSettings.Size = new System.Drawing.Size(82, 18);
            this.lblHeaderNgSettings.TabIndex = 10;
            this.lblHeaderNgSettings.Text = " NG Settings";
            this.lblHeaderNgSettings.TextAlign = System.Drawing.ContentAlignment.BottomLeft;
            // 
            // lblPreview
            // 
            this.lblPreview.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.lblPreview.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.lblPreview.Location = new System.Drawing.Point(343, 431);
            this.lblPreview.Name = "lblPreview";
            this.lblPreview.Size = new System.Drawing.Size(133, 18);
            this.lblPreview.TabIndex = 11;
            this.lblPreview.Text = "Preview";
            this.lblPreview.TextAlign = System.Drawing.ContentAlignment.BottomLeft;
            // 
            // settingsPanelNG
            // 
            this.settingsPanelNG.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.settingsPanelNG.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.settingsPanelNG.Controls.Add(this.lblUvRotate);
            this.settingsPanelNG.Controls.Add(this.comboUvRotate);
            this.settingsPanelNG.Controls.Add(this.lblFps);
            this.settingsPanelNG.Controls.Add(this.comboFps);
            this.settingsPanelNG.Controls.Add(this.lblEffect);
            this.settingsPanelNG.Controls.Add(this.comboEffect);
            this.settingsPanelNG.Location = new System.Drawing.Point(4, 556);
            this.settingsPanelNG.Name = "settingsPanelNG";
            this.settingsPanelNG.Size = new System.Drawing.Size(336, 54);
            this.settingsPanelNG.TabIndex = 12;
            // 
            // lblUvRotate
            // 
            this.lblUvRotate.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblUvRotate.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.lblUvRotate.Location = new System.Drawing.Point(169, 1);
            this.lblUvRotate.Name = "lblUvRotate";
            this.lblUvRotate.Size = new System.Drawing.Size(58, 23);
            this.lblUvRotate.TabIndex = 10;
            this.lblUvRotate.Text = "UvRotate:";
            this.lblUvRotate.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // comboUvRotate
            // 
            this.comboUvRotate.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.comboUvRotate.Location = new System.Drawing.Point(169, 24);
            this.comboUvRotate.Name = "comboUvRotate";
            this.comboUvRotate.Size = new System.Drawing.Size(160, 23);
            this.comboUvRotate.TabIndex = 11;
            this.comboUvRotate.SelectionChangeCommitted += new System.EventHandler(this.comboUvRotate_SelectionChangeCommitted);
            // 
            // lblFps
            // 
            this.lblFps.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblFps.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.lblFps.Location = new System.Drawing.Point(89, 1);
            this.lblFps.Name = "lblFps";
            this.lblFps.Size = new System.Drawing.Size(32, 23);
            this.lblFps.TabIndex = 8;
            this.lblFps.Text = "FPS:";
            this.lblFps.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // comboFps
            // 
            this.comboFps.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.comboFps.Location = new System.Drawing.Point(92, 24);
            this.comboFps.Name = "comboFps";
            this.comboFps.Size = new System.Drawing.Size(71, 23);
            this.comboFps.TabIndex = 9;
            this.comboFps.SelectionChangeCommitted += new System.EventHandler(this.comboFps_SelectionChangeCommitted);
            // 
            // lblEffect
            // 
            this.lblEffect.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.lblEffect.Location = new System.Drawing.Point(1, 1);
            this.lblEffect.Name = "lblEffect";
            this.lblEffect.Size = new System.Drawing.Size(41, 23);
            this.lblEffect.TabIndex = 6;
            this.lblEffect.Text = "Effect:";
            this.lblEffect.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // comboEffect
            // 
            this.comboEffect.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.comboEffect.Location = new System.Drawing.Point(6, 24);
            this.comboEffect.Name = "comboEffect";
            this.comboEffect.Size = new System.Drawing.Size(80, 23);
            this.comboEffect.TabIndex = 7;
            this.comboEffect.SelectedIndexChanged += new System.EventHandler(this.comboEffect_SelectedIndexChanged);
            this.comboEffect.SelectionChangeCommitted += new System.EventHandler(this.comboEffect_SelectionChangeCommitted);
            // 
            // previewImage
            // 
            this.previewImage.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.previewImage.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.previewImage.Location = new System.Drawing.Point(346, 452);
            this.previewImage.Name = "previewImage";
            this.previewImage.Size = new System.Drawing.Size(135, 135);
            this.previewImage.TabIndex = 13;
            this.previewImage.TabStop = false;
            // 
            // texturesDataGridViewControls
            // 
            this.texturesDataGridViewControls.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.texturesDataGridViewControls.Enabled = false;
            this.texturesDataGridViewControls.Location = new System.Drawing.Point(457, 55);
            this.texturesDataGridViewControls.MinimumSize = new System.Drawing.Size(24, 100);
            this.texturesDataGridViewControls.Name = "texturesDataGridViewControls";
            this.texturesDataGridViewControls.Size = new System.Drawing.Size(24, 373);
            this.texturesDataGridViewControls.TabIndex = 16;
            // 
            // butAnimatedTextureSetDelete
            // 
            this.butAnimatedTextureSetDelete.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.butAnimatedTextureSetDelete.Enabled = false;
            this.butAnimatedTextureSetDelete.Image = global::TombEditor.Properties.Resources.general_trash_16;
            this.butAnimatedTextureSetDelete.ImagePadding = 3;
            this.butAnimatedTextureSetDelete.Location = new System.Drawing.Point(457, 5);
            this.butAnimatedTextureSetDelete.Margin = new System.Windows.Forms.Padding(3, 0, 0, 0);
            this.butAnimatedTextureSetDelete.Name = "butAnimatedTextureSetDelete";
            this.butAnimatedTextureSetDelete.Size = new System.Drawing.Size(24, 24);
            this.butAnimatedTextureSetDelete.TabIndex = 3;
            this.butAnimatedTextureSetDelete.Click += new System.EventHandler(this.butAnimatedTextureSetDelete_Click);
            // 
            // butAnimatedTextureSetNew
            // 
            this.butAnimatedTextureSetNew.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.butAnimatedTextureSetNew.Image = global::TombEditor.Properties.Resources.general_plus_math_16;
            this.butAnimatedTextureSetNew.ImagePadding = 4;
            this.butAnimatedTextureSetNew.Location = new System.Drawing.Point(428, 5);
            this.butAnimatedTextureSetNew.Margin = new System.Windows.Forms.Padding(0, 0, 2, 0);
            this.butAnimatedTextureSetNew.Name = "butAnimatedTextureSetNew";
            this.butAnimatedTextureSetNew.Size = new System.Drawing.Size(24, 24);
            this.butAnimatedTextureSetNew.TabIndex = 2;
            this.butAnimatedTextureSetNew.Click += new System.EventHandler(this.butAnimatedTextureSetNew_Click);
            // 
            // comboAnimatedTextureSets
            // 
            this.comboAnimatedTextureSets.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.comboAnimatedTextureSets.ItemHeight = 18;
            this.comboAnimatedTextureSets.Location = new System.Drawing.Point(88, 5);
            this.comboAnimatedTextureSets.Name = "comboAnimatedTextureSets";
            this.comboAnimatedTextureSets.Size = new System.Drawing.Size(305, 24);
            this.comboAnimatedTextureSets.TabIndex = 1;
            this.comboAnimatedTextureSets.SelectedIndexChanged += new System.EventHandler(this.comboAnimatedTextureSets_SelectedIndexChanged);
            // 
            // butOk
            // 
            this.butOk.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.butOk.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.butOk.Location = new System.Drawing.Point(777, 625);
            this.butOk.Name = "butOk";
            this.butOk.Size = new System.Drawing.Size(80, 23);
            this.butOk.TabIndex = 8;
            this.butOk.Text = "OK";
            this.butOk.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.butOk.Click += new System.EventHandler(this.butOk_Click);
            // 
            // toolTip
            // 
            this.toolTip.AutomaticDelay = 100;
            this.toolTip.AutoPopDelay = 30000;
            this.toolTip.InitialDelay = 100;
            this.toolTip.ReshowDelay = 20;
            // 
            // FormAnimatedTextures
            // 
            this.AcceptButton = this.butOk;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.butOk;
            this.ClientSize = new System.Drawing.Size(869, 656);
            this.Controls.Add(this.tableLayoutPanel1);
            this.Controls.Add(this.butOk);
            this.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(691, 500);
            this.Name = "FormAnimatedTextures";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Animated textures";
            this.tableLayoutPanel1.ResumeLayout(false);
            this.panel2.ResumeLayout(false);
            this.panel1.ResumeLayout(false);
            this.panel3.ResumeLayout(false);
            this.panel3.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numFrames)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numStrength)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.texturesDataGridView)).EndInit();
            this.settingsPanelNG.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.previewImage)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion
        private DarkLabel darkLabel1;
        private DarkComboBox comboAnimatedTextureSets;
        private DarkButton butAnimatedTextureSetDelete;
        private DarkButton butAnimatedTextureSetNew;
        private TableLayoutPanel tableLayoutPanel1;
        private Panel panel1;
        private DarkButton butOk;
        private DarkLabel tooManyFramesWarning;
        private DarkProgressBar previewProgressBar;
        private DarkDataGridView texturesDataGridView;
        private DarkLabel lblHeaderNgSettings;
        private DarkLabel lblPreview;
        private Panel settingsPanelNG;
        private DarkLabel lblEffect;
        private DarkComboBox comboEffect;
        private PictureBox previewImage;
        private TombLib.Controls.DarkDataGridViewControls texturesDataGridViewControls;
        private DataGridViewImageColumn texturesDataGridViewColumnImage;
        private DataGridViewTextBoxColumn texturesDataGridViewColumnRepeat;
        private DataGridViewComboBoxColumn texturesDataGridViewColumnTexture;
        private DataGridViewTextBoxColumn texturesDataGridViewColumnArea;
        private DataGridViewTextBoxColumn texturesDataGridViewColumnTexCoord0;
        private DataGridViewTextBoxColumn texturesDataGridViewColumnTexCoord1;
        private DataGridViewTextBoxColumn texturesDataGridViewColumnTexCoord2;
        private DataGridViewTextBoxColumn texturesDataGridViewColumnTexCoord3;
        private DarkButton butUpdate;
        private ToolTip toolTip;
        private System.ComponentModel.IContainer components;
        private DarkLabel lblUvRotate;
        private DarkComboBox comboUvRotate;
        private DarkLabel lblFps;
        private DarkComboBox comboFps;
        private Panel panel2;
        private DarkComboBox comboCurrentTexture;
        private FormAnimatedTextures.PanelTextureMapForAnimations textureMap;
        private DarkLabel lblProcAnim;
        private Panel panel3;
        private DarkComboBox comboProcPresets;
        private DarkButton butGenerateProcAnim;
        private DarkNumericUpDown numStrength;
        private DarkLabel lblAnimatorName;
        private DarkLabel lblProgress;
        private DarkNumericUpDown numFrames;
        private DarkLabel lblFrames;
        private DarkCheckBox cbSmooth;
        private DarkCheckBox cbLoop;
        private DarkButton butEditSetName;
        private DarkButton butMergeProcAnim;
        private DarkButton butCloneProcAnim;
        private DarkLabel label1;
        private DarkButton butReplaceProcAnim;
        private DarkButton butAddProcAnim;
    }
}