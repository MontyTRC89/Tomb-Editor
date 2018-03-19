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
            this.label1 = new DarkUI.Controls.DarkLabel();
            this.tbSound = new DarkUI.Controls.DarkTextBox();
            this.lstSounds = new DarkUI.Controls.DarkListView();
            this.butPlaySound = new DarkUI.Controls.DarkButton();
            this.butOK = new DarkUI.Controls.DarkButton();
            this.butCancel = new DarkUI.Controls.DarkButton();
            this.optionPlaySoundFromWad = new DarkUI.Controls.DarkRadioButton();
            this.optionPlayCustomSound = new DarkUI.Controls.DarkRadioButton();
            this.optionPlaySoundFromWadGroupBox = new DarkUI.Controls.DarkGroupBox();
            this.darkLabel1 = new DarkUI.Controls.DarkLabel();
            this.optionPlayCustomSoundGroupBox = new DarkUI.Controls.DarkGroupBox();
            this.soundInfoEditor = new TombLib.Controls.SoundInfoEditor();
            this.optionPlaySoundFromWadGroupBox.SuspendLayout();
            this.optionPlayCustomSoundGroupBox.SuspendLayout();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label1.AutoSize = true;
            this.label1.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.label1.Location = new System.Drawing.Point(5, 353);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(75, 13);
            this.label1.TabIndex = 54;
            this.label1.Text = "Sound name:";
            // 
            // tbSound
            // 
            this.tbSound.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tbSound.Location = new System.Drawing.Point(86, 350);
            this.tbSound.Name = "tbSound";
            this.tbSound.Size = new System.Drawing.Size(404, 22);
            this.tbSound.TabIndex = 55;
            this.tbSound.TextChanged += new System.EventHandler(this.tbSound_TextChanged);
            // 
            // lstSounds
            // 
            this.lstSounds.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lstSounds.Location = new System.Drawing.Point(8, 28);
            this.lstSounds.Name = "lstSounds";
            this.lstSounds.Size = new System.Drawing.Size(580, 311);
            this.lstSounds.TabIndex = 61;
            this.lstSounds.Text = "darkListView1";
            this.lstSounds.SelectedIndicesChanged += new System.EventHandler(this.lstSounds_SelectedIndicesChanged);
            // 
            // butPlaySound
            // 
            this.butPlaySound.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.butPlaySound.Image = global::TombEditor.Properties.Resources.actions_play_16;
            this.butPlaySound.Location = new System.Drawing.Point(496, 350);
            this.butPlaySound.Name = "butPlaySound";
            this.butPlaySound.Size = new System.Drawing.Size(92, 22);
            this.butPlaySound.TabIndex = 62;
            this.butPlaySound.Text = "Play sound";
            this.butPlaySound.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.butPlaySound.Click += new System.EventHandler(this.butPlay_Click);
            // 
            // butOK
            // 
            this.butOK.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.butOK.Location = new System.Drawing.Point(174, 495);
            this.butOK.Name = "butOK";
            this.butOK.Size = new System.Drawing.Size(130, 24);
            this.butOK.TabIndex = 0;
            this.butOK.Text = "Ok";
            this.butOK.Click += new System.EventHandler(this.butOK_Click);
            // 
            // butCancel
            // 
            this.butCancel.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.butCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.butCancel.Location = new System.Drawing.Point(310, 495);
            this.butCancel.Name = "butCancel";
            this.butCancel.Size = new System.Drawing.Size(130, 24);
            this.butCancel.TabIndex = 1;
            this.butCancel.Text = "Cancel";
            this.butCancel.Click += new System.EventHandler(this.butCancel_Click);
            // 
            // optionPlaySoundFromWad
            // 
            this.optionPlaySoundFromWad.AutoSize = true;
            this.optionPlaySoundFromWad.Location = new System.Drawing.Point(15, 40);
            this.optionPlaySoundFromWad.Name = "optionPlaySoundFromWad";
            this.optionPlaySoundFromWad.Size = new System.Drawing.Size(133, 17);
            this.optionPlaySoundFromWad.TabIndex = 1;
            this.optionPlaySoundFromWad.Text = "Play sound from wad";
            this.optionPlaySoundFromWad.CheckedChanged += new System.EventHandler(this.optionPlaySoundFromWad_CheckedChanged);
            // 
            // optionPlayCustomSound
            // 
            this.optionPlayCustomSound.AutoSize = true;
            this.optionPlayCustomSound.Checked = true;
            this.optionPlayCustomSound.Location = new System.Drawing.Point(15, 12);
            this.optionPlayCustomSound.Name = "optionPlayCustomSound";
            this.optionPlayCustomSound.Size = new System.Drawing.Size(121, 17);
            this.optionPlayCustomSound.TabIndex = 0;
            this.optionPlayCustomSound.TabStop = true;
            this.optionPlayCustomSound.Text = "Play custom sound";
            this.optionPlayCustomSound.CheckedChanged += new System.EventHandler(this.optionPlayCustomSound_CheckedChanged);
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
            this.optionPlaySoundFromWadGroupBox.Size = new System.Drawing.Size(594, 379);
            this.optionPlaySoundFromWadGroupBox.TabIndex = 66;
            this.optionPlaySoundFromWadGroupBox.TabStop = false;
            this.optionPlaySoundFromWadGroupBox.Text = "Play sound from wad settings";
            this.optionPlaySoundFromWadGroupBox.Visible = false;
            // 
            // darkLabel1
            // 
            this.darkLabel1.AutoSize = true;
            this.darkLabel1.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.darkLabel1.Location = new System.Drawing.Point(6, 12);
            this.darkLabel1.Name = "darkLabel1";
            this.darkLabel1.Size = new System.Drawing.Size(232, 13);
            this.darkLabel1.TabIndex = 54;
            this.darkLabel1.Text = "Sounds to choose from in the loaded wads:";
            // 
            // optionPlayCustomSoundGroupBox
            // 
            this.optionPlayCustomSoundGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.optionPlayCustomSoundGroupBox.Controls.Add(this.soundInfoEditor);
            this.optionPlayCustomSoundGroupBox.Location = new System.Drawing.Point(12, 63);
            this.optionPlayCustomSoundGroupBox.Name = "optionPlayCustomSoundGroupBox";
            this.optionPlayCustomSoundGroupBox.Size = new System.Drawing.Size(594, 379);
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
            this.soundInfoEditor.HasName = false;
            this.soundInfoEditor.Location = new System.Drawing.Point(6, 12);
            this.soundInfoEditor.MinimumSize = new System.Drawing.Size(442, 346);
            this.soundInfoEditor.Name = "soundInfoEditor";
            this.soundInfoEditor.Size = new System.Drawing.Size(577, 354);
            this.soundInfoEditor.TabIndex = 1;
            // 
            // FormSoundSource
            // 
            this.AcceptButton = this.butOK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.butCancel;
            this.ClientSize = new System.Drawing.Size(618, 531);
            this.Controls.Add(this.optionPlayCustomSoundGroupBox);
            this.Controls.Add(this.optionPlayCustomSound);
            this.Controls.Add(this.optionPlaySoundFromWad);
            this.Controls.Add(this.butOK);
            this.Controls.Add(this.butCancel);
            this.Controls.Add(this.optionPlaySoundFromWadGroupBox);
            this.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(400, 200);
            this.Name = "FormSoundSource";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Sound source";
            this.optionPlaySoundFromWadGroupBox.ResumeLayout(false);
            this.optionPlaySoundFromWadGroupBox.PerformLayout();
            this.optionPlayCustomSoundGroupBox.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private DarkUI.Controls.DarkLabel label1;
        private DarkUI.Controls.DarkTextBox tbSound;
        private DarkUI.Controls.DarkListView lstSounds;
        private DarkUI.Controls.DarkButton butPlaySound;
        private DarkUI.Controls.DarkButton butOK;
        private DarkUI.Controls.DarkButton butCancel;
        private DarkUI.Controls.DarkRadioButton optionPlaySoundFromWad;
        private DarkUI.Controls.DarkRadioButton optionPlayCustomSound;
        private DarkUI.Controls.DarkGroupBox optionPlaySoundFromWadGroupBox;
        private DarkUI.Controls.DarkGroupBox optionPlayCustomSoundGroupBox;
        private DarkUI.Controls.DarkLabel darkLabel1;
        private TombLib.Controls.SoundInfoEditor soundInfoEditor;
    }
}