namespace TombLib.Forms
{
    partial class FormSoundEditor
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
            this.darkStatusStrip1 = new DarkUI.Controls.DarkStatusStrip();
            this.labelStatus = new System.Windows.Forms.ToolStripStatusLabel();
            this.lstSoundInfos = new DarkUI.Controls.DarkListView();
            this.lstWaves = new DarkUI.Controls.DarkListView();
            this.darkLabel1 = new DarkUI.Controls.DarkLabel();
            this.tbName = new DarkUI.Controls.DarkTextBox();
            this.cbFlagN = new DarkUI.Controls.DarkCheckBox();
            this.cbRandomizeGain = new DarkUI.Controls.DarkCheckBox();
            this.cbRandomizePitch = new DarkUI.Controls.DarkCheckBox();
            this.tbVolume = new DarkUI.Controls.DarkTextBox();
            this.darkLabel2 = new DarkUI.Controls.DarkLabel();
            this.tbRange = new DarkUI.Controls.DarkTextBox();
            this.darkLabel3 = new DarkUI.Controls.DarkLabel();
            this.tbPitch = new DarkUI.Controls.DarkTextBox();
            this.darkLabel4 = new DarkUI.Controls.DarkLabel();
            this.tbChance = new DarkUI.Controls.DarkTextBox();
            this.darkLabel5 = new DarkUI.Controls.DarkLabel();
            this.darkLabel6 = new DarkUI.Controls.DarkLabel();
            this.darkLabel7 = new DarkUI.Controls.DarkLabel();
            this.butSaveChanges = new DarkUI.Controls.DarkButton();
            this.comboId = new DarkUI.Controls.DarkComboBox();
            this.comboLoop = new DarkUI.Controls.DarkComboBox();
            this.butAddNewSound = new DarkUI.Controls.DarkButton();
            this.butDeleteSound = new DarkUI.Controls.DarkButton();
            this.darkLabel8 = new DarkUI.Controls.DarkLabel();
            this.butAddNewWave = new DarkUI.Controls.DarkButton();
            this.butDeleteWave = new DarkUI.Controls.DarkButton();
            this.butPlaySound = new DarkUI.Controls.DarkButton();
            this.butClose = new DarkUI.Controls.DarkButton();
            this.butSave = new DarkUI.Controls.DarkButton();
            this.darkStatusStrip1.SuspendLayout();
            this.SuspendLayout();
            //
            // darkStatusStrip1
            //
            this.darkStatusStrip1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.darkStatusStrip1.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.darkStatusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.labelStatus});
            this.darkStatusStrip1.Location = new System.Drawing.Point(0, 503);
            this.darkStatusStrip1.Name = "darkStatusStrip1";
            this.darkStatusStrip1.Padding = new System.Windows.Forms.Padding(0, 5, 0, 3);
            this.darkStatusStrip1.Size = new System.Drawing.Size(576, 32);
            this.darkStatusStrip1.TabIndex = 0;
            this.darkStatusStrip1.Text = "darkStatusStrip1";
            //
            // labelStatus
            //
            this.labelStatus.Name = "labelStatus";
            this.labelStatus.Size = new System.Drawing.Size(0, 0);
            //
            // lstSoundInfos
            //
            this.lstSoundInfos.Location = new System.Drawing.Point(12, 12);
            this.lstSoundInfos.Name = "lstSoundInfos";
            this.lstSoundInfos.Size = new System.Drawing.Size(240, 452);
            this.lstSoundInfos.TabIndex = 1;
            this.lstSoundInfos.Text = "darkListView1";
            this.lstSoundInfos.MouseClick += new System.Windows.Forms.MouseEventHandler(this.lstSoundInfos_MouseClick);
            //
            // lstWaves
            //
            this.lstWaves.Location = new System.Drawing.Point(276, 354);
            this.lstWaves.Name = "lstWaves";
            this.lstWaves.Size = new System.Drawing.Size(204, 110);
            this.lstWaves.TabIndex = 15;
            this.lstWaves.Text = "darkListView1";
            //
            // darkLabel1
            //
            this.darkLabel1.AutoSize = true;
            this.darkLabel1.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.darkLabel1.Location = new System.Drawing.Point(273, 51);
            this.darkLabel1.Name = "darkLabel1";
            this.darkLabel1.Size = new System.Drawing.Size(38, 13);
            this.darkLabel1.TabIndex = 18;
            this.darkLabel1.Text = "Name:";
            //
            // tbName
            //
            this.tbName.Location = new System.Drawing.Point(326, 49);
            this.tbName.Name = "tbName";
            this.tbName.Size = new System.Drawing.Size(239, 20);
            this.tbName.TabIndex = 19;
            //
            // cbFlagN
            //
            this.cbFlagN.AutoSize = true;
            this.cbFlagN.Location = new System.Drawing.Point(276, 242);
            this.cbFlagN.Name = "cbFlagN";
            this.cbFlagN.Size = new System.Drawing.Size(103, 17);
            this.cbFlagN.TabIndex = 20;
            this.cbFlagN.Text = "Unknown N flag";
            //
            // cbRandomizeGain
            //
            this.cbRandomizeGain.AutoSize = true;
            this.cbRandomizeGain.Location = new System.Drawing.Point(276, 265);
            this.cbRandomizeGain.Name = "cbRandomizeGain";
            this.cbRandomizeGain.Size = new System.Drawing.Size(102, 17);
            this.cbRandomizeGain.TabIndex = 22;
            this.cbRandomizeGain.Text = "Randomize gain";
            //
            // cbRandomizePitch
            //
            this.cbRandomizePitch.AutoSize = true;
            this.cbRandomizePitch.Location = new System.Drawing.Point(276, 288);
            this.cbRandomizePitch.Name = "cbRandomizePitch";
            this.cbRandomizePitch.Size = new System.Drawing.Size(105, 17);
            this.cbRandomizePitch.TabIndex = 23;
            this.cbRandomizePitch.Text = "Randomize pitch";
            //
            // tbVolume
            //
            this.tbVolume.Location = new System.Drawing.Point(326, 87);
            this.tbVolume.Name = "tbVolume";
            this.tbVolume.Size = new System.Drawing.Size(79, 20);
            this.tbVolume.TabIndex = 25;
            //
            // darkLabel2
            //
            this.darkLabel2.AutoSize = true;
            this.darkLabel2.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.darkLabel2.Location = new System.Drawing.Point(273, 89);
            this.darkLabel2.Name = "darkLabel2";
            this.darkLabel2.Size = new System.Drawing.Size(45, 13);
            this.darkLabel2.TabIndex = 24;
            this.darkLabel2.Text = "Volume:";
            //
            // tbRange
            //
            this.tbRange.Location = new System.Drawing.Point(326, 113);
            this.tbRange.Name = "tbRange";
            this.tbRange.Size = new System.Drawing.Size(79, 20);
            this.tbRange.TabIndex = 27;
            //
            // darkLabel3
            //
            this.darkLabel3.AutoSize = true;
            this.darkLabel3.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.darkLabel3.Location = new System.Drawing.Point(273, 115);
            this.darkLabel3.Name = "darkLabel3";
            this.darkLabel3.Size = new System.Drawing.Size(42, 13);
            this.darkLabel3.TabIndex = 26;
            this.darkLabel3.Text = "Range:";
            //
            // tbPitch
            //
            this.tbPitch.Location = new System.Drawing.Point(326, 139);
            this.tbPitch.Name = "tbPitch";
            this.tbPitch.Size = new System.Drawing.Size(79, 20);
            this.tbPitch.TabIndex = 29;
            //
            // darkLabel4
            //
            this.darkLabel4.AutoSize = true;
            this.darkLabel4.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.darkLabel4.Location = new System.Drawing.Point(273, 141);
            this.darkLabel4.Name = "darkLabel4";
            this.darkLabel4.Size = new System.Drawing.Size(34, 13);
            this.darkLabel4.TabIndex = 28;
            this.darkLabel4.Text = "Pitch:";
            //
            // tbChance
            //
            this.tbChance.Location = new System.Drawing.Point(326, 165);
            this.tbChance.Name = "tbChance";
            this.tbChance.Size = new System.Drawing.Size(79, 20);
            this.tbChance.TabIndex = 31;
            //
            // darkLabel5
            //
            this.darkLabel5.AutoSize = true;
            this.darkLabel5.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.darkLabel5.Location = new System.Drawing.Point(273, 167);
            this.darkLabel5.Name = "darkLabel5";
            this.darkLabel5.Size = new System.Drawing.Size(47, 13);
            this.darkLabel5.TabIndex = 30;
            this.darkLabel5.Text = "Chance:";
            //
            // darkLabel6
            //
            this.darkLabel6.AutoSize = true;
            this.darkLabel6.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.darkLabel6.Location = new System.Drawing.Point(273, 206);
            this.darkLabel6.Name = "darkLabel6";
            this.darkLabel6.Size = new System.Drawing.Size(34, 13);
            this.darkLabel6.TabIndex = 32;
            this.darkLabel6.Text = "Loop:";
            //
            // darkLabel7
            //
            this.darkLabel7.AutoSize = true;
            this.darkLabel7.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.darkLabel7.Location = new System.Drawing.Point(273, 15);
            this.darkLabel7.Name = "darkLabel7";
            this.darkLabel7.Size = new System.Drawing.Size(28, 13);
            this.darkLabel7.TabIndex = 34;
            this.darkLabel7.Text = "Slot:";
            //
            // butSaveChanges
            //
            this.butSaveChanges.Image = global::TombLib.Properties.Resources.general_Save_16;
            this.butSaveChanges.Location = new System.Drawing.Point(453, 12);
            this.butSaveChanges.Name = "butSaveChanges";
            this.butSaveChanges.Size = new System.Drawing.Size(112, 23);
            this.butSaveChanges.TabIndex = 36;
            this.butSaveChanges.Text = "Save changes";
            this.butSaveChanges.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.butSaveChanges.Visible = false;
            this.butSaveChanges.Click += new System.EventHandler(this.butSaveChanges_Click);
            //
            // comboId
            //
            this.comboId.FormattingEnabled = true;
            this.comboId.Location = new System.Drawing.Point(326, 12);
            this.comboId.Name = "comboId";
            this.comboId.Size = new System.Drawing.Size(79, 21);
            this.comboId.TabIndex = 35;
            this.comboId.Text = "None";
            //
            // comboLoop
            //
            this.comboLoop.FormattingEnabled = true;
            this.comboLoop.Items.AddRange(new object[] {
            "None",
            "W (0x01)",
            "R (0x02)",
            "L (0x03)"});
            this.comboLoop.Location = new System.Drawing.Point(326, 203);
            this.comboLoop.Name = "comboLoop";
            this.comboLoop.Size = new System.Drawing.Size(79, 21);
            this.comboLoop.TabIndex = 33;
            this.comboLoop.Text = "None";
            //
            // butAddNewSound
            //
            this.butAddNewSound.Image = global::TombLib.Properties.Resources.general_plus_math_16;
            this.butAddNewSound.Location = new System.Drawing.Point(12, 470);
            this.butAddNewSound.Name = "butAddNewSound";
            this.butAddNewSound.Size = new System.Drawing.Size(123, 23);
            this.butAddNewSound.TabIndex = 14;
            this.butAddNewSound.Text = "Add new sound";
            this.butAddNewSound.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.butAddNewSound.Click += new System.EventHandler(this.butAddNewSound_Click);
            //
            // butDeleteSound
            //
            this.butDeleteSound.Image = global::TombLib.Properties.Resources.general_trash_16;
            this.butDeleteSound.Location = new System.Drawing.Point(141, 470);
            this.butDeleteSound.Name = "butDeleteSound";
            this.butDeleteSound.Size = new System.Drawing.Size(112, 23);
            this.butDeleteSound.TabIndex = 13;
            this.butDeleteSound.Text = "Delete sound";
            this.butDeleteSound.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.butDeleteSound.Click += new System.EventHandler(this.butDeleteSound_Click);
            //
            // darkLabel8
            //
            this.darkLabel8.AutoSize = true;
            this.darkLabel8.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.darkLabel8.Location = new System.Drawing.Point(273, 338);
            this.darkLabel8.Name = "darkLabel8";
            this.darkLabel8.Size = new System.Drawing.Size(76, 13);
            this.darkLabel8.TabIndex = 37;
            this.darkLabel8.Text = "WAV samples:";
            //
            // butAddNewWave
            //
            this.butAddNewWave.Image = global::TombLib.Properties.Resources.general_plus_math_16;
            this.butAddNewWave.Location = new System.Drawing.Point(486, 354);
            this.butAddNewWave.Name = "butAddNewWave";
            this.butAddNewWave.Size = new System.Drawing.Size(79, 23);
            this.butAddNewWave.TabIndex = 39;
            this.butAddNewWave.Text = "Add new";
            this.butAddNewWave.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.butAddNewWave.Click += new System.EventHandler(this.butAddNewWave_Click);
            //
            // butDeleteWave
            //
            this.butDeleteWave.Image = global::TombLib.Properties.Resources.general_trash_16;
            this.butDeleteWave.Location = new System.Drawing.Point(486, 383);
            this.butDeleteWave.Name = "butDeleteWave";
            this.butDeleteWave.Size = new System.Drawing.Size(79, 23);
            this.butDeleteWave.TabIndex = 38;
            this.butDeleteWave.Text = "Delete";
            this.butDeleteWave.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.butDeleteWave.Click += new System.EventHandler(this.butDeleteWave_Click);
            //
            // butPlaySound
            //
            this.butPlaySound.Image = global::TombLib.Properties.Resources.actions_play_16;
            this.butPlaySound.Location = new System.Drawing.Point(486, 412);
            this.butPlaySound.Name = "butPlaySound";
            this.butPlaySound.Size = new System.Drawing.Size(79, 23);
            this.butPlaySound.TabIndex = 40;
            this.butPlaySound.Text = "Play";
            this.butPlaySound.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.butPlaySound.Click += new System.EventHandler(this.butPlaySound_Click);
            //
            // butClose
            //
            this.butClose.Location = new System.Drawing.Point(456, 470);
            this.butClose.Name = "butClose";
            this.butClose.Size = new System.Drawing.Size(109, 23);
            this.butClose.TabIndex = 41;
            this.butClose.Text = "Exit without saving";
            this.butClose.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.butClose.Click += new System.EventHandler(this.butClose_Click);
            //
            // butSave
            //
            this.butSave.Location = new System.Drawing.Point(357, 470);
            this.butSave.Name = "butSave";
            this.butSave.Size = new System.Drawing.Size(93, 23);
            this.butSave.TabIndex = 42;
            this.butSave.Text = "Save to Wad2";
            this.butSave.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.butSave.Click += new System.EventHandler(this.butSave_Click);
            //
            // FormSoundEditor
            //
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(576, 535);
            this.Controls.Add(this.butSave);
            this.Controls.Add(this.butClose);
            this.Controls.Add(this.butPlaySound);
            this.Controls.Add(this.butAddNewWave);
            this.Controls.Add(this.butDeleteWave);
            this.Controls.Add(this.darkLabel8);
            this.Controls.Add(this.butSaveChanges);
            this.Controls.Add(this.comboId);
            this.Controls.Add(this.darkLabel7);
            this.Controls.Add(this.comboLoop);
            this.Controls.Add(this.darkLabel6);
            this.Controls.Add(this.tbChance);
            this.Controls.Add(this.darkLabel5);
            this.Controls.Add(this.tbPitch);
            this.Controls.Add(this.darkLabel4);
            this.Controls.Add(this.tbRange);
            this.Controls.Add(this.darkLabel3);
            this.Controls.Add(this.tbVolume);
            this.Controls.Add(this.darkLabel2);
            this.Controls.Add(this.cbRandomizePitch);
            this.Controls.Add(this.cbRandomizeGain);
            this.Controls.Add(this.cbFlagN);
            this.Controls.Add(this.tbName);
            this.Controls.Add(this.darkLabel1);
            this.Controls.Add(this.lstWaves);
            this.Controls.Add(this.butAddNewSound);
            this.Controls.Add(this.butDeleteSound);
            this.Controls.Add(this.lstSoundInfos);
            this.Controls.Add(this.darkStatusStrip1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FormSoundEditor";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Sound editor";
            this.darkStatusStrip1.ResumeLayout(false);
            this.darkStatusStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private DarkUI.Controls.DarkStatusStrip darkStatusStrip1;
        private DarkUI.Controls.DarkListView lstSoundInfos;
        private DarkUI.Controls.DarkButton butDeleteSound;
        private DarkUI.Controls.DarkButton butAddNewSound;
        private DarkUI.Controls.DarkListView lstWaves;
        private DarkUI.Controls.DarkLabel darkLabel1;
        private DarkUI.Controls.DarkTextBox tbName;
        private DarkUI.Controls.DarkCheckBox cbFlagN;
        private DarkUI.Controls.DarkCheckBox cbRandomizeGain;
        private DarkUI.Controls.DarkCheckBox cbRandomizePitch;
        private DarkUI.Controls.DarkTextBox tbVolume;
        private DarkUI.Controls.DarkLabel darkLabel2;
        private DarkUI.Controls.DarkTextBox tbRange;
        private DarkUI.Controls.DarkLabel darkLabel3;
        private DarkUI.Controls.DarkTextBox tbPitch;
        private DarkUI.Controls.DarkLabel darkLabel4;
        private DarkUI.Controls.DarkTextBox tbChance;
        private DarkUI.Controls.DarkLabel darkLabel5;
        private DarkUI.Controls.DarkLabel darkLabel6;
        private DarkUI.Controls.DarkComboBox comboLoop;
        private DarkUI.Controls.DarkComboBox comboId;
        private DarkUI.Controls.DarkLabel darkLabel7;
        private DarkUI.Controls.DarkButton butSaveChanges;
        private DarkUI.Controls.DarkLabel darkLabel8;
        private DarkUI.Controls.DarkButton butAddNewWave;
        private DarkUI.Controls.DarkButton butDeleteWave;
        private DarkUI.Controls.DarkButton butPlaySound;
        private System.Windows.Forms.ToolStripStatusLabel labelStatus;
        private DarkUI.Controls.DarkButton butClose;
        private DarkUI.Controls.DarkButton butSave;
    }
}