namespace TombLib.Forms
{
    partial class FormSelectSample
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
            this.lstWaves = new DarkUI.Controls.DarkListView();
            this.butCancel = new DarkUI.Controls.DarkButton();
            this.butOK = new DarkUI.Controls.DarkButton();
            this.openFileDialogWave = new System.Windows.Forms.OpenFileDialog();
            this.butPlaySound = new DarkUI.Controls.DarkButton();
            this.butAddNewWave = new DarkUI.Controls.DarkButton();
            this.tbSearch = new DarkUI.Controls.DarkTextBox();
            this.SuspendLayout();
            //
            // darkStatusStrip1
            //
            this.darkStatusStrip1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.darkStatusStrip1.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.darkStatusStrip1.Location = new System.Drawing.Point(0, 405);
            this.darkStatusStrip1.Name = "darkStatusStrip1";
            this.darkStatusStrip1.Padding = new System.Windows.Forms.Padding(0, 5, 0, 3);
            this.darkStatusStrip1.Size = new System.Drawing.Size(338, 24);
            this.darkStatusStrip1.TabIndex = 0;
            this.darkStatusStrip1.Text = "darkStatusStrip1";
            //
            // lstWaves
            //
            this.lstWaves.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lstWaves.Location = new System.Drawing.Point(13, 39);
            this.lstWaves.Name = "lstWaves";
            this.lstWaves.Size = new System.Drawing.Size(314, 317);
            this.lstWaves.TabIndex = 1;
            this.lstWaves.Text = "darkListView1";
            //
            // butCancel
            //
            this.butCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.butCancel.Location = new System.Drawing.Point(82, 379);
            this.butCancel.Name = "butCancel";
            this.butCancel.Size = new System.Drawing.Size(65, 23);
            this.butCancel.TabIndex = 12;
            this.butCancel.Text = "Cancel";
            this.butCancel.Click += new System.EventHandler(this.butCancel_Click);
            //
            // butOK
            //
            this.butOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.butOK.Location = new System.Drawing.Point(13, 379);
            this.butOK.Name = "butOK";
            this.butOK.Size = new System.Drawing.Size(63, 23);
            this.butOK.TabIndex = 11;
            this.butOK.Text = "OK";
            this.butOK.Click += new System.EventHandler(this.butOK_Click);
            //
            // openFileDialogWave
            //
            this.openFileDialogWave.Filter = "WAVE audio (*.wav)|*.wav";
            this.openFileDialogWave.Title = "Add new WAVE";
            //
            // butPlaySound
            //
            this.butPlaySound.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.butPlaySound.Image = global::TombLib.Properties.Resources.actions_play_16;
            this.butPlaySound.Location = new System.Drawing.Point(270, 379);
            this.butPlaySound.Name = "butPlaySound";
            this.butPlaySound.Size = new System.Drawing.Size(57, 23);
            this.butPlaySound.TabIndex = 16;
            this.butPlaySound.Text = "Play";
            this.butPlaySound.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.butPlaySound.Click += new System.EventHandler(this.butPlaySound_Click);
            //
            // butAddNewWave
            //
            this.butAddNewWave.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.butAddNewWave.Image = global::TombLib.Properties.Resources.general_plus_math_16;
            this.butAddNewWave.Location = new System.Drawing.Point(153, 379);
            this.butAddNewWave.Name = "butAddNewWave";
            this.butAddNewWave.Size = new System.Drawing.Size(111, 23);
            this.butAddNewWave.TabIndex = 13;
            this.butAddNewWave.Text = "Add new sample";
            this.butAddNewWave.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.butAddNewWave.Click += new System.EventHandler(this.butAddNewWave_Click);
            //
            // tbSearch
            //
            this.tbSearch.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tbSearch.Location = new System.Drawing.Point(13, 12);
            this.tbSearch.Name = "tbSearch";
            this.tbSearch.Size = new System.Drawing.Size(313, 20);
            this.tbSearch.TabIndex = 17;
            this.tbSearch.KeyUp += new System.Windows.Forms.KeyEventHandler(this.tbSearch_KeyUp);
            //
            // FormSelectWave
            //
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(338, 429);
            this.Controls.Add(this.tbSearch);
            this.Controls.Add(this.butPlaySound);
            this.Controls.Add(this.butAddNewWave);
            this.Controls.Add(this.butCancel);
            this.Controls.Add(this.butOK);
            this.Controls.Add(this.lstWaves);
            this.Controls.Add(this.darkStatusStrip1);
            this.MinimizeBox = false;
            this.Name = "FormSelectWave";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Select WAV sample";
            this.Load += new System.EventHandler(this.FormSelectWave_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private DarkUI.Controls.DarkStatusStrip darkStatusStrip1;
        private DarkUI.Controls.DarkListView lstWaves;
        private DarkUI.Controls.DarkButton butCancel;
        private DarkUI.Controls.DarkButton butOK;
        private DarkUI.Controls.DarkButton butAddNewWave;
        private System.Windows.Forms.OpenFileDialog openFileDialogWave;
        private DarkUI.Controls.DarkButton butPlaySound;
        private DarkUI.Controls.DarkTextBox tbSearch;
    }
}