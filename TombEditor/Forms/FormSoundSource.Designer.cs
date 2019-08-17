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
            this.optionPlaySoundFromWadGroupBox = new DarkUI.Controls.DarkGroupBox();
            this.lstSounds = new DarkUI.Controls.DarkListView();
            this.tbSound = new DarkUI.Controls.DarkTextBox();
            this.label1 = new DarkUI.Controls.DarkLabel();
            this.darkLabel1 = new DarkUI.Controls.DarkLabel();
            this.butPlaySound = new DarkUI.Controls.DarkButton();
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
            this.optionPlaySoundFromWadGroupBox.Location = new System.Drawing.Point(12, 12);
            this.optionPlaySoundFromWadGroupBox.Name = "optionPlaySoundFromWadGroupBox";
            this.optionPlaySoundFromWadGroupBox.Size = new System.Drawing.Size(460, 489);
            this.optionPlaySoundFromWadGroupBox.TabIndex = 66;
            this.optionPlaySoundFromWadGroupBox.TabStop = false;
            this.optionPlaySoundFromWadGroupBox.Text = "Play sound from wad settings";
            // 
            // lstSounds
            // 
            this.lstSounds.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lstSounds.Location = new System.Drawing.Point(8, 41);
            this.lstSounds.Name = "lstSounds";
            this.lstSounds.Size = new System.Drawing.Size(446, 408);
            this.lstSounds.TabIndex = 61;
            this.lstSounds.Text = "darkListView1";
            this.lstSounds.SelectedIndicesChanged += new System.EventHandler(this.lstSounds_SelectedIndicesChanged);
            this.lstSounds.Click += new System.EventHandler(this.LstSounds_Click);
            // 
            // tbSound
            // 
            this.tbSound.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tbSound.Location = new System.Drawing.Point(86, 460);
            this.tbSound.Name = "tbSound";
            this.tbSound.Size = new System.Drawing.Size(270, 22);
            this.tbSound.TabIndex = 55;
            // 
            // label1
            // 
            this.label1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label1.AutoSize = true;
            this.label1.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.label1.Location = new System.Drawing.Point(5, 463);
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
            this.butPlaySound.Location = new System.Drawing.Point(362, 460);
            this.butPlaySound.Name = "butPlaySound";
            this.butPlaySound.Size = new System.Drawing.Size(92, 22);
            this.butPlaySound.TabIndex = 62;
            this.butPlaySound.Text = "Play sound";
            this.butPlaySound.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.butPlaySound.Visible = false;
            this.butPlaySound.Click += new System.EventHandler(this.butPlay_Click);
            // 
            // FormSoundSource
            // 
            this.AcceptButton = this.butOK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.butCancel;
            this.ClientSize = new System.Drawing.Size(484, 542);
            this.Controls.Add(this.butOK);
            this.Controls.Add(this.butCancel);
            this.Controls.Add(this.optionPlaySoundFromWadGroupBox);
            this.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(500, 520);
            this.Name = "FormSoundSource";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Sound source";
            this.optionPlaySoundFromWadGroupBox.ResumeLayout(false);
            this.optionPlaySoundFromWadGroupBox.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion
        private DarkUI.Controls.DarkButton butOK;
        private DarkUI.Controls.DarkButton butCancel;
        private DarkUI.Controls.DarkGroupBox optionPlaySoundFromWadGroupBox;
        private DarkUI.Controls.DarkButton butPlaySound;
        private DarkUI.Controls.DarkLabel darkLabel1;
        private DarkUI.Controls.DarkLabel label1;
        private DarkUI.Controls.DarkTextBox tbSound;
        private DarkUI.Controls.DarkListView lstSounds;
    }
}