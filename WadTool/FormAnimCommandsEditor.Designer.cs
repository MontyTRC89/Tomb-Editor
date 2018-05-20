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
            this.tbPosZ = new DarkUI.Controls.DarkTextBox();
            this.darkLabel2 = new DarkUI.Controls.DarkLabel();
            this.tbPosY = new DarkUI.Controls.DarkTextBox();
            this.darkLabel1 = new DarkUI.Controls.DarkLabel();
            this.tbPosX = new DarkUI.Controls.DarkTextBox();
            this.panelJumpDistance = new System.Windows.Forms.Panel();
            this.darkLabel5 = new DarkUI.Controls.DarkLabel();
            this.tbVertical = new DarkUI.Controls.DarkTextBox();
            this.darkLabel6 = new DarkUI.Controls.DarkLabel();
            this.tbHorizontal = new DarkUI.Controls.DarkTextBox();
            this.panelEffect = new System.Windows.Forms.Panel();
            this.darkLabel4 = new DarkUI.Controls.DarkLabel();
            this.tbFlipEffect = new DarkUI.Controls.DarkTextBox();
            this.darkLabel7 = new DarkUI.Controls.DarkLabel();
            this.tbFlipEffectFrame = new DarkUI.Controls.DarkTextBox();
            this.panelSound = new System.Windows.Forms.Panel();
            this.comboSound = new DarkUI.Controls.DarkComboBox();
            this.darkLabel8 = new DarkUI.Controls.DarkLabel();
            this.darkLabel9 = new DarkUI.Controls.DarkLabel();
            this.tbPlaySoundFrame = new DarkUI.Controls.DarkTextBox();
            this.butAddEffect = new DarkUI.Controls.DarkButton();
            this.butDeleteEffect = new DarkUI.Controls.DarkButton();
            this.butSaveChanges = new DarkUI.Controls.DarkButton();
            this.panelPosition.SuspendLayout();
            this.panelJumpDistance.SuspendLayout();
            this.panelEffect.SuspendLayout();
            this.panelSound.SuspendLayout();
            this.SuspendLayout();
            // 
            // btCancel
            // 
            this.btCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btCancel.Location = new System.Drawing.Point(147, 312);
            this.btCancel.Name = "btCancel";
            this.btCancel.Size = new System.Drawing.Size(113, 26);
            this.btCancel.TabIndex = 50;
            this.btCancel.Text = "Cancel";
            this.btCancel.Click += new System.EventHandler(this.btCancel_Click);
            // 
            // btOk
            // 
            this.btOk.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btOk.Location = new System.Drawing.Point(266, 312);
            this.btOk.Name = "btOk";
            this.btOk.Size = new System.Drawing.Size(113, 26);
            this.btOk.TabIndex = 51;
            this.btOk.Text = "Ok";
            this.btOk.Click += new System.EventHandler(this.btOk_Click);
            // 
            // treeCommands
            // 
            this.treeCommands.Location = new System.Drawing.Point(12, 13);
            this.treeCommands.MaxDragChange = 20;
            this.treeCommands.Name = "treeCommands";
            this.treeCommands.Size = new System.Drawing.Size(499, 227);
            this.treeCommands.TabIndex = 52;
            this.treeCommands.Text = "darkTreeView1";
            this.treeCommands.SelectedNodesChanged += new System.EventHandler(this.treeCommands_SelectedNodesChanged);
            // 
            // comboCommandType
            // 
            this.comboCommandType.FormattingEnabled = true;
            this.comboCommandType.Items.AddRange(new object[] {
            "Set position",
            "Set jump distance",
            "Empty hands",
            "Kill entity",
            "Play sound",
            "Play sound (On land)",
            "Play sound (On water)",
            "Flipeffect"});
            this.comboCommandType.Location = new System.Drawing.Point(12, 246);
            this.comboCommandType.Name = "comboCommandType";
            this.comboCommandType.Size = new System.Drawing.Size(324, 23);
            this.comboCommandType.TabIndex = 53;
            this.comboCommandType.SelectedIndexChanged += new System.EventHandler(this.comboCommandType_SelectedIndexChanged);
            // 
            // panelPosition
            // 
            this.panelPosition.Controls.Add(this.darkLabel3);
            this.panelPosition.Controls.Add(this.tbPosZ);
            this.panelPosition.Controls.Add(this.darkLabel2);
            this.panelPosition.Controls.Add(this.tbPosY);
            this.panelPosition.Controls.Add(this.darkLabel1);
            this.panelPosition.Controls.Add(this.tbPosX);
            this.panelPosition.Location = new System.Drawing.Point(12, 275);
            this.panelPosition.Name = "panelPosition";
            this.panelPosition.Size = new System.Drawing.Size(324, 31);
            this.panelPosition.TabIndex = 54;
            // 
            // darkLabel3
            // 
            this.darkLabel3.AutoSize = true;
            this.darkLabel3.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.darkLabel3.Location = new System.Drawing.Point(219, 7);
            this.darkLabel3.Name = "darkLabel3";
            this.darkLabel3.Size = new System.Drawing.Size(16, 13);
            this.darkLabel3.TabIndex = 5;
            this.darkLabel3.Text = "Z:";
            // 
            // tbPosZ
            // 
            this.tbPosZ.Location = new System.Drawing.Point(241, 4);
            this.tbPosZ.Name = "tbPosZ";
            this.tbPosZ.Size = new System.Drawing.Size(73, 22);
            this.tbPosZ.TabIndex = 4;
            this.tbPosZ.Text = "0";
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
            this.tbPosY.Location = new System.Drawing.Point(131, 4);
            this.tbPosY.Name = "tbPosY";
            this.tbPosY.Size = new System.Drawing.Size(73, 22);
            this.tbPosY.TabIndex = 2;
            this.tbPosY.Text = "0";
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
            this.tbPosX.Location = new System.Drawing.Point(25, 3);
            this.tbPosX.Name = "tbPosX";
            this.tbPosX.Size = new System.Drawing.Size(73, 22);
            this.tbPosX.TabIndex = 0;
            this.tbPosX.Text = "0";
            // 
            // panelJumpDistance
            // 
            this.panelJumpDistance.Controls.Add(this.darkLabel5);
            this.panelJumpDistance.Controls.Add(this.tbVertical);
            this.panelJumpDistance.Controls.Add(this.darkLabel6);
            this.panelJumpDistance.Controls.Add(this.tbHorizontal);
            this.panelJumpDistance.Location = new System.Drawing.Point(12, 312);
            this.panelJumpDistance.Name = "panelJumpDistance";
            this.panelJumpDistance.Size = new System.Drawing.Size(324, 29);
            this.panelJumpDistance.TabIndex = 55;
            // 
            // darkLabel5
            // 
            this.darkLabel5.AutoSize = true;
            this.darkLabel5.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.darkLabel5.Location = new System.Drawing.Point(188, 6);
            this.darkLabel5.Name = "darkLabel5";
            this.darkLabel5.Size = new System.Drawing.Size(47, 13);
            this.darkLabel5.TabIndex = 3;
            this.darkLabel5.Text = "Vertical:";
            // 
            // tbVertical
            // 
            this.tbVertical.Location = new System.Drawing.Point(241, 3);
            this.tbVertical.Name = "tbVertical";
            this.tbVertical.Size = new System.Drawing.Size(73, 22);
            this.tbVertical.TabIndex = 2;
            this.tbVertical.Text = "0";
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
            this.tbHorizontal.Location = new System.Drawing.Point(73, 3);
            this.tbHorizontal.Name = "tbHorizontal";
            this.tbHorizontal.Size = new System.Drawing.Size(73, 22);
            this.tbHorizontal.TabIndex = 0;
            this.tbHorizontal.Text = "0";
            // 
            // panelEffect
            // 
            this.panelEffect.Controls.Add(this.darkLabel4);
            this.panelEffect.Controls.Add(this.tbFlipEffect);
            this.panelEffect.Controls.Add(this.darkLabel7);
            this.panelEffect.Controls.Add(this.tbFlipEffectFrame);
            this.panelEffect.Location = new System.Drawing.Point(12, 347);
            this.panelEffect.Name = "panelEffect";
            this.panelEffect.Size = new System.Drawing.Size(324, 29);
            this.panelEffect.TabIndex = 56;
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
            this.tbFlipEffect.Location = new System.Drawing.Point(241, 3);
            this.tbFlipEffect.Name = "tbFlipEffect";
            this.tbFlipEffect.Size = new System.Drawing.Size(73, 22);
            this.tbFlipEffect.TabIndex = 2;
            this.tbFlipEffect.Text = "0";
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
            this.tbFlipEffectFrame.Location = new System.Drawing.Point(73, 3);
            this.tbFlipEffectFrame.Name = "tbFlipEffectFrame";
            this.tbFlipEffectFrame.Size = new System.Drawing.Size(42, 22);
            this.tbFlipEffectFrame.TabIndex = 0;
            this.tbFlipEffectFrame.Text = "0";
            // 
            // panelSound
            // 
            this.panelSound.Controls.Add(this.comboSound);
            this.panelSound.Controls.Add(this.darkLabel8);
            this.panelSound.Controls.Add(this.darkLabel9);
            this.panelSound.Controls.Add(this.tbPlaySoundFrame);
            this.panelSound.Location = new System.Drawing.Point(12, 382);
            this.panelSound.Name = "panelSound";
            this.panelSound.Size = new System.Drawing.Size(324, 29);
            this.panelSound.TabIndex = 57;
            // 
            // comboSound
            // 
            this.comboSound.FormattingEnabled = true;
            this.comboSound.Location = new System.Drawing.Point(173, 2);
            this.comboSound.Name = "comboSound";
            this.comboSound.Size = new System.Drawing.Size(141, 23);
            this.comboSound.TabIndex = 4;
            // 
            // darkLabel8
            // 
            this.darkLabel8.AutoSize = true;
            this.darkLabel8.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.darkLabel8.Location = new System.Drawing.Point(128, 6);
            this.darkLabel8.Name = "darkLabel8";
            this.darkLabel8.Size = new System.Drawing.Size(44, 13);
            this.darkLabel8.TabIndex = 3;
            this.darkLabel8.Text = "Sound:";
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
            // tbPlaySoundFrame
            // 
            this.tbPlaySoundFrame.Location = new System.Drawing.Point(73, 3);
            this.tbPlaySoundFrame.Name = "tbPlaySoundFrame";
            this.tbPlaySoundFrame.Size = new System.Drawing.Size(42, 22);
            this.tbPlaySoundFrame.TabIndex = 0;
            this.tbPlaySoundFrame.Text = "0";
            // 
            // butAddEffect
            // 
            this.butAddEffect.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.butAddEffect.Image = global::WadTool.Properties.Resources.plus_math_16;
            this.butAddEffect.Location = new System.Drawing.Point(342, 246);
            this.butAddEffect.Name = "butAddEffect";
            this.butAddEffect.Size = new System.Drawing.Size(83, 23);
            this.butAddEffect.TabIndex = 95;
            this.butAddEffect.Text = "Add new";
            this.butAddEffect.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.butAddEffect.Click += new System.EventHandler(this.butAddEffect_Click);
            // 
            // butDeleteEffect
            // 
            this.butDeleteEffect.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.butDeleteEffect.Image = global::WadTool.Properties.Resources.trash_161;
            this.butDeleteEffect.Location = new System.Drawing.Point(431, 246);
            this.butDeleteEffect.Name = "butDeleteEffect";
            this.butDeleteEffect.Size = new System.Drawing.Size(80, 23);
            this.butDeleteEffect.TabIndex = 94;
            this.butDeleteEffect.Text = "Delete";
            this.butDeleteEffect.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.butDeleteEffect.Click += new System.EventHandler(this.butDeleteEffect_Click);
            // 
            // butSaveChanges
            // 
            this.butSaveChanges.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.butSaveChanges.Image = global::WadTool.Properties.Resources.save_16;
            this.butSaveChanges.Location = new System.Drawing.Point(342, 275);
            this.butSaveChanges.Name = "butSaveChanges";
            this.butSaveChanges.Size = new System.Drawing.Size(83, 23);
            this.butSaveChanges.TabIndex = 96;
            this.butSaveChanges.Text = "Save";
            this.butSaveChanges.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.butSaveChanges.Click += new System.EventHandler(this.butSaveChanges_Click);
            // 
            // FormAnimCommandsEditor
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(521, 350);
            this.Controls.Add(this.butSaveChanges);
            this.Controls.Add(this.butAddEffect);
            this.Controls.Add(this.butDeleteEffect);
            this.Controls.Add(this.panelSound);
            this.Controls.Add(this.panelEffect);
            this.Controls.Add(this.panelJumpDistance);
            this.Controls.Add(this.panelPosition);
            this.Controls.Add(this.comboCommandType);
            this.Controls.Add(this.treeCommands);
            this.Controls.Add(this.btCancel);
            this.Controls.Add(this.btOk);
            this.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FormAnimCommandsEditor";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Anim commands editor";
            this.panelPosition.ResumeLayout(false);
            this.panelPosition.PerformLayout();
            this.panelJumpDistance.ResumeLayout(false);
            this.panelJumpDistance.PerformLayout();
            this.panelEffect.ResumeLayout(false);
            this.panelEffect.PerformLayout();
            this.panelSound.ResumeLayout(false);
            this.panelSound.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion
        private DarkUI.Controls.DarkButton btCancel;
        private DarkUI.Controls.DarkButton btOk;
        private DarkUI.Controls.DarkTreeView treeCommands;
        private DarkUI.Controls.DarkComboBox comboCommandType;
        private System.Windows.Forms.Panel panelPosition;
        private DarkUI.Controls.DarkLabel darkLabel3;
        private DarkUI.Controls.DarkTextBox tbPosZ;
        private DarkUI.Controls.DarkLabel darkLabel2;
        private DarkUI.Controls.DarkTextBox tbPosY;
        private DarkUI.Controls.DarkLabel darkLabel1;
        private DarkUI.Controls.DarkTextBox tbPosX;
        private System.Windows.Forms.Panel panelJumpDistance;
        private DarkUI.Controls.DarkLabel darkLabel5;
        private DarkUI.Controls.DarkTextBox tbVertical;
        private DarkUI.Controls.DarkLabel darkLabel6;
        private DarkUI.Controls.DarkTextBox tbHorizontal;
        private System.Windows.Forms.Panel panelEffect;
        private DarkUI.Controls.DarkLabel darkLabel4;
        private DarkUI.Controls.DarkTextBox tbFlipEffect;
        private DarkUI.Controls.DarkLabel darkLabel7;
        private DarkUI.Controls.DarkTextBox tbFlipEffectFrame;
        private System.Windows.Forms.Panel panelSound;
        private DarkUI.Controls.DarkLabel darkLabel8;
        private DarkUI.Controls.DarkLabel darkLabel9;
        private DarkUI.Controls.DarkTextBox tbPlaySoundFrame;
        private DarkUI.Controls.DarkComboBox comboSound;
        private DarkUI.Controls.DarkButton butAddEffect;
        private DarkUI.Controls.DarkButton butDeleteEffect;
        private DarkUI.Controls.DarkButton butSaveChanges;
    }
}