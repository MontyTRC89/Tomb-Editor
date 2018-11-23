namespace TombEditor.Forms
{
    partial class FormSoundSource
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
            this.butOK = new DarkUI.Controls.DarkButton();
            this.butCancel = new DarkUI.Controls.DarkButton();
            this.optionPlaySoundFromWad = new DarkUI.Controls.DarkRadioButton();
            this.optionPlayCustomSound = new DarkUI.Controls.DarkRadioButton();
            this.optionPlayCustomSoundGroupBox = new DarkUI.Controls.DarkGroupBox();
            this.soundInfoEditor = new TombLib.Controls.SoundInfoEditor();
            this.lstSounds = new DarkUI.Controls.DarkListView();
            this.tbSound = new DarkUI.Controls.DarkTextBox();
            this.label1 = new DarkUI.Controls.DarkLabel();
            this.darkLabel1 = new DarkUI.Controls.DarkLabel();
            this.butPlaySound = new DarkUI.Controls.DarkButton();
            this.optionPlaySoundFromWadGroupBox = new DarkUI.Controls.DarkGroupBox();
            this.optionPlayCustomSoundGroupBox.SuspendLayout();
            this.optionPlaySoundFromWadGroupBox.SuspendLayout();
            this.SuspendLayout();
            // 
            // butOK
            // 
            this.butOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.butOK.Location = new System.Drawing.Point(306, 507);
            this.butOK.Name = "butOK";
            this.butOK.Size = new System.Drawing.Size(80, 23);
            this.butOK.TabIndex = 0;
            this.butOK.Text = "OK";
            this.butOK.Click += new System.EventHandler(this.butOK_Click);
            // 
            // butCancel
            // 
            this.butCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.butCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.butCancel.Location = new System.Drawing.Point(392, 507);
            this.butCancel.Name = "butCancel";
            this.butCancel.Size = new System.Drawing.Size(80, 23);
            this.butCancel.TabIndex = 1;
            this.butCancel.Text = "Cancel";
            this.butCancel.Click += new System.EventHandler(this.butCancel_Click);
            // 
            // optionPlaySoundFromWad
            // 
            this.optionPlaySoundFromWad.AutoSize = true;
            this.optionPlaySoundFromWad.Location = new System.Drawing.Point(12, 12);
            this.optionPlaySoundFromWad.Name = "optionPlaySoundFromWad";
            this.optionPlaySoundFromWad.Size = new System.Drawing.Size(133, 17);
            this.optionPlaySoundFromWad.TabIndex = 1;
            this.optionPlaySoundFromWad.Text = "Play sound from wad";
            this.optionPlaySoundFromWad.CheckedChanged += new System.EventHandler(this.optionPlaySoundFromWad_CheckedChanged);
            // 
            // optionPlayCustomSound
            // 
            this.optionPlayCustomSound.AutoSize = true;
            this.optionPlayCustomSound.Location = new System.Drawing.Point(12, 35);
            this.optionPlayCustomSound.Name = "optionPlayCustomSound";
            this.optionPlayCustomSound.Size = new System.Drawing.Size(121, 17);
            this.optionPlayCustomSound.TabIndex = 0;
            this.optionPlayCustomSound.Text = "Play custom sound";
            this.optionPlayCustomSound.CheckedChanged += new System.EventHandler(this.optionPlayCustomSound_CheckedChanged);
            // 
            // optionPlayCustomSoundGroupBox
            // 
            this.optionPlayCustomSoundGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.optionPlayCustomSoundGroupBox.Controls.Add(this.soundInfoEditor);
            this.optionPlayCustomSoundGroupBox.Location = new System.Drawing.Point(12, 63);
            this.optionPlayCustomSoundGroupBox.Name = "optionPlayCustomSoundGroupBox";
            this.optionPlayCustomSoundGroupBox.Size = new System.Drawing.Size(460, 438);
            this.optionPlayCustomSoundGroupBox.TabIndex = 5;
            this.optionPlayCustomSoundGroupBox.TabStop = false;
            this.optionPlayCustomSoundGroupBox.Text = "Custom sound settings";
            // 
            // soundInfoEditor
            // 
            this.soundInfoEditor.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.soundInfoEditor.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.soundInfoEditor.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.soundInfoEditor.Location = new System.Drawing.Point(6, 20);
            this.soundInfoEditor.MinimumSize = new System.Drawing.Size(442, 346);
            this.soundInfoEditor.Name = "soundInfoEditor";
            this.soundInfoEditor.Size = new System.Drawing.Size(448, 411);
            this.soundInfoEditor.TabIndex = 1;
            // 
            // lstSounds
            // 
            this.lstSounds.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lstSounds.Location = new System.Drawing.Point(8, 41);
            this.lstSounds.Name = "lstSounds";
            this.lstSounds.Size = new System.Drawing.Size(446, 357);
            this.lstSounds.TabIndex = 61;
            this.lstSounds.Text = "darkListView1";
            this.lstSounds.SelectedIndicesChanged += new System.EventHandler(this.lstSounds_SelectedIndicesChanged);
            // 
            // tbSound
            // 
            this.tbSound.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tbSound.Location = new System.Drawing.Point(86, 409);
            this.tbSound.Name = "tbSound";
            this.tbSound.Size = new System.Drawing.Size(270, 22);
            this.tbSound.TabIndex = 55;
            this.tbSound.TextChanged += new System.EventHandler(this.tbSound_TextChanged);
            // 
            // label1
            // 
            this.label1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label1.AutoSize = true;
            this.label1.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.label1.Location = new System.Drawing.Point(5, 412);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(75, 13);
            this.label1.TabIndex = 54;
            this.label1.Text = "Sound name:";
            // 
            // darkLabel1
            // 
            this.darkLabel1.AutoSize = true;
            this.darkLabel1.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.darkLabel1.Location = new System.Drawing.Point(6, 25);
            this.darkLabel1.Name = "darkLabel1";
            this.darkLabel1.Size = new System.Drawing.Size(232, 13);
            this.darkLabel1.TabIndex = 54;
            this.darkLabel1.Text = "Sounds to choose from in the loaded wads:";
            // 
            // butPlaySound
            // 
            this.butPlaySound.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.butPlaySound.Image = global::TombEditor.Properties.Resources.actions_play_16;
            this.butPlaySound.Location = new System.Drawing.Point(362, 409);
            this.butPlaySound.Name = "butPlaySound";
            this.butPlaySound.Size = new System.Drawing.Size(92, 22);
            this.butPlaySound.TabIndex = 62;
            this.butPlaySound.Text = "Play sound";
            this.butPlaySound.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.butPlaySound.Click += new System.EventHandler(this.butPlay_Click);
            // 
            // optionPlaySoundFromWadGroupBox
            // 
            this.optionPlaySoundFromWadGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.optionPlaySoundFromWadGroupBox.Controls.Add(this.butPlaySound);
            this.optionPlaySoundFromWadGroupBox.Controls.Add(this.darkLabel1);
            this.optionPlaySoundFromWadGroupBox.Controls.Add(this.label1);
            this.optionPlaySoundFromWadGroupBox.Controls.Add(this.tbSound);
            this.optionPlaySoundFromWadGroupBox.Controls.Add(this.lstSounds);
            this.optionPlaySoundFromWadGroupBox.Location = new System.Drawing.Point(12, 63);
            this.optionPlaySoundFromWadGroupBox.Name = "optionPlaySoundFromWadGroupBox";
            this.optionPlaySoundFromWadGroupBox.Size = new System.Drawing.Size(460, 438);
            this.optionPlaySoundFromWadGroupBox.TabIndex = 66;
            this.optionPlaySoundFromWadGroupBox.TabStop = false;
            this.optionPlaySoundFromWadGroupBox.Text = "Play sound from wad settings";
            this.optionPlaySoundFromWadGroupBox.Visible = false;
            // 
            // FormSoundSource
            // 
            this.AcceptButton = this.butOK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.butCancel;
            this.ClientSize = new System.Drawing.Size(484, 542);
            this.Controls.Add(this.optionPlayCustomSound);
            this.Controls.Add(this.optionPlaySoundFromWad);
            this.Controls.Add(this.butOK);
            this.Controls.Add(this.butCancel);
            this.Controls.Add(this.optionPlaySoundFromWadGroupBox);
            this.Controls.Add(this.optionPlayCustomSoundGroupBox);
            this.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(500, 520);
            this.Name = "FormSoundSource";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Sound source";
            this.optionPlayCustomSoundGroupBox.ResumeLayout(false);
            this.optionPlaySoundFromWadGroupBox.ResumeLayout(false);
            this.optionPlaySoundFromWadGroupBox.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private DarkUI.Controls.DarkButton butOK;
        private DarkUI.Controls.DarkButton butCancel;
        private DarkUI.Controls.DarkRadioButton optionPlaySoundFromWad;
        private DarkUI.Controls.DarkRadioButton optionPlayCustomSound;
        private DarkUI.Controls.DarkGroupBox optionPlayCustomSoundGroupBox;
        private TombLib.Controls.SoundInfoEditor soundInfoEditor;
        private DarkUI.Controls.DarkListView lstSounds;
        private DarkUI.Controls.DarkTextBox tbSound;
        private DarkUI.Controls.DarkLabel label1;
        private DarkUI.Controls.DarkLabel darkLabel1;
        private DarkUI.Controls.DarkButton butPlaySound;
        private DarkUI.Controls.DarkGroupBox optionPlaySoundFromWadGroupBox;
    }
}