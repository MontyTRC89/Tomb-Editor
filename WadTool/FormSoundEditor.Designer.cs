namespace WadTool
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormSoundEditor));
            this.darkStatusStrip1 = new DarkUI.Controls.DarkStatusStrip();
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
            this.labelStatus = new System.Windows.Forms.ToolStripStatusLabel();
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
            this.tbName.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(69)))), ((int)(((byte)(73)))), ((int)(((byte)(74)))));
            this.tbName.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.tbName.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
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
            this.tbVolume.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(69)))), ((int)(((byte)(73)))), ((int)(((byte)(74)))));
            this.tbVolume.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.tbVolume.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
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
            this.tbRange.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(69)))), ((int)(((byte)(73)))), ((int)(((byte)(74)))));
            this.tbRange.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.tbRange.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
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
            this.tbPitch.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(69)))), ((int)(((byte)(73)))), ((int)(((byte)(74)))));
            this.tbPitch.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.tbPitch.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
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
            this.tbChance.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(69)))), ((int)(((byte)(73)))), ((int)(((byte)(74)))));
            this.tbChance.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.tbChance.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
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
            this.butSaveChanges.Image = global::WadTool.Properties.Resources.save_16;
            this.butSaveChanges.Location = new System.Drawing.Point(453, 12);
            this.butSaveChanges.Name = "butSaveChanges";
            this.butSaveChanges.Padding = new System.Windows.Forms.Padding(5);
            this.butSaveChanges.Size = new System.Drawing.Size(112, 23);
            this.butSaveChanges.TabIndex = 36;
            this.butSaveChanges.Text = "Save changes";
            this.butSaveChanges.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.butSaveChanges.Visible = false;
            this.butSaveChanges.Click += new System.EventHandler(this.butSaveChanges_Click);
            // 
            // comboId
            // 
            this.comboId.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(69)))), ((int)(((byte)(73)))), ((int)(((byte)(74)))));
            this.comboId.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(81)))), ((int)(((byte)(81)))), ((int)(((byte)(81)))));
            this.comboId.BorderStyle = System.Windows.Forms.ButtonBorderStyle.Solid;
            this.comboId.ButtonColor = System.Drawing.Color.FromArgb(((int)(((byte)(43)))), ((int)(((byte)(43)))), ((int)(((byte)(43)))));
            this.comboId.ButtonIcon = ((System.Drawing.Bitmap)(resources.GetObject("comboId.ButtonIcon")));
            this.comboId.DrawDropdownHoverOutline = false;
            this.comboId.DrawFocusRectangle = false;
            this.comboId.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawVariable;
            this.comboId.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboId.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.comboId.ForeColor = System.Drawing.Color.Gainsboro;
            this.comboId.FormattingEnabled = true;
            this.comboId.Location = new System.Drawing.Point(326, 12);
            this.comboId.Name = "comboId";
            this.comboId.Size = new System.Drawing.Size(79, 21);
            this.comboId.TabIndex = 35;
            this.comboId.Text = "None";
            this.comboId.TextPadding = new System.Windows.Forms.Padding(2);
            // 
            // comboLoop
            // 
            this.comboLoop.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(69)))), ((int)(((byte)(73)))), ((int)(((byte)(74)))));
            this.comboLoop.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(81)))), ((int)(((byte)(81)))), ((int)(((byte)(81)))));
            this.comboLoop.BorderStyle = System.Windows.Forms.ButtonBorderStyle.Solid;
            this.comboLoop.ButtonColor = System.Drawing.Color.FromArgb(((int)(((byte)(43)))), ((int)(((byte)(43)))), ((int)(((byte)(43)))));
            this.comboLoop.ButtonIcon = ((System.Drawing.Bitmap)(resources.GetObject("comboLoop.ButtonIcon")));
            this.comboLoop.DrawDropdownHoverOutline = false;
            this.comboLoop.DrawFocusRectangle = false;
            this.comboLoop.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawVariable;
            this.comboLoop.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboLoop.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.comboLoop.ForeColor = System.Drawing.Color.Gainsboro;
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
            this.comboLoop.TextPadding = new System.Windows.Forms.Padding(2);
            // 
            // butAddNewSound
            // 
            this.butAddNewSound.Image = global::WadTool.Properties.Resources.plus_math_16;
            this.butAddNewSound.Location = new System.Drawing.Point(12, 470);
            this.butAddNewSound.Name = "butAddNewSound";
            this.butAddNewSound.Padding = new System.Windows.Forms.Padding(5);
            this.butAddNewSound.Size = new System.Drawing.Size(123, 23);
            this.butAddNewSound.TabIndex = 14;
            this.butAddNewSound.Text = "Add new sound";
            this.butAddNewSound.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.butAddNewSound.Click += new System.EventHandler(this.butAddNewSound_Click);
            // 
            // butDeleteSound
            // 
            this.butDeleteSound.Image = global::WadTool.Properties.Resources.trash_16;
            this.butDeleteSound.Location = new System.Drawing.Point(141, 470);
            this.butDeleteSound.Name = "butDeleteSound";
            this.butDeleteSound.Padding = new System.Windows.Forms.Padding(5);
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
            this.butAddNewWave.Image = global::WadTool.Properties.Resources.plus_math_16;
            this.butAddNewWave.Location = new System.Drawing.Point(486, 354);
            this.butAddNewWave.Name = "butAddNewWave";
            this.butAddNewWave.Padding = new System.Windows.Forms.Padding(5);
            this.butAddNewWave.Size = new System.Drawing.Size(79, 23);
            this.butAddNewWave.TabIndex = 39;
            this.butAddNewWave.Text = "Add new";
            this.butAddNewWave.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.butAddNewWave.Click += new System.EventHandler(this.butAddNewWave_Click);
            // 
            // butDeleteWave
            // 
            this.butDeleteWave.Image = global::WadTool.Properties.Resources.trash_16;
            this.butDeleteWave.Location = new System.Drawing.Point(486, 383);
            this.butDeleteWave.Name = "butDeleteWave";
            this.butDeleteWave.Padding = new System.Windows.Forms.Padding(5);
            this.butDeleteWave.Size = new System.Drawing.Size(79, 23);
            this.butDeleteWave.TabIndex = 38;
            this.butDeleteWave.Text = "Delete";
            this.butDeleteWave.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.butDeleteWave.Click += new System.EventHandler(this.butDeleteWave_Click);
            // 
            // butPlaySound
            // 
            this.butPlaySound.Image = global::WadTool.Properties.Resources.play_16;
            this.butPlaySound.Location = new System.Drawing.Point(486, 412);
            this.butPlaySound.Name = "butPlaySound";
            this.butPlaySound.Padding = new System.Windows.Forms.Padding(5);
            this.butPlaySound.Size = new System.Drawing.Size(79, 23);
            this.butPlaySound.TabIndex = 40;
            this.butPlaySound.Text = "Play";
            this.butPlaySound.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.butPlaySound.Click += new System.EventHandler(this.butPlaySound_Click);
            // 
            // labelStatus
            // 
            this.labelStatus.Name = "labelStatus";
            this.labelStatus.Size = new System.Drawing.Size(0, 19);
            // 
            // FormSoundEditor
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(576, 535);
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
            this.Load += new System.EventHandler(this.FormSoundEditor_Load);
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
    }
}