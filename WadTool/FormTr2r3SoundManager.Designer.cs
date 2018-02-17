namespace WadTool
{
    partial class FormTr2r3SoundManager
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
            this.butSaveChanges = new DarkUI.Controls.DarkButton();
            this.darkStatusStrip1 = new DarkUI.Controls.DarkStatusStrip();
            this.labelStatistics = new System.Windows.Forms.ToolStripStatusLabel();
            this.dgvSounds = new DarkUI.Controls.DarkDataGridView();
            this.columnSelected = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this.columnSoundId = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.columnSoundName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.butPlaySound = new DarkUI.Controls.DarkButton();
            this.darkStatusStrip1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvSounds)).BeginInit();
            this.SuspendLayout();
            // 
            // butSaveChanges
            // 
            this.butSaveChanges.Image = global::WadTool.Properties.Resources.save_16;
            this.butSaveChanges.Location = new System.Drawing.Point(81, 471);
            this.butSaveChanges.Name = "butSaveChanges";
            this.butSaveChanges.Size = new System.Drawing.Size(112, 23);
            this.butSaveChanges.TabIndex = 47;
            this.butSaveChanges.Text = "Save changes";
            this.butSaveChanges.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.butSaveChanges.Click += new System.EventHandler(this.butSaveChanges_Click);
            // 
            // darkStatusStrip1
            // 
            this.darkStatusStrip1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.darkStatusStrip1.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.darkStatusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.labelStatistics});
            this.darkStatusStrip1.Location = new System.Drawing.Point(0, 500);
            this.darkStatusStrip1.Name = "darkStatusStrip1";
            this.darkStatusStrip1.Padding = new System.Windows.Forms.Padding(0, 5, 0, 3);
            this.darkStatusStrip1.Size = new System.Drawing.Size(397, 32);
            this.darkStatusStrip1.TabIndex = 48;
            this.darkStatusStrip1.Text = "darkStatusStrip1";
            // 
            // labelStatistics
            // 
            this.labelStatistics.Name = "labelStatistics";
            this.labelStatistics.Size = new System.Drawing.Size(14, 19);
            this.labelStatistics.Text = "#";
            // 
            // dgvSounds
            // 
            this.dgvSounds.AllowUserToAddRows = false;
            this.dgvSounds.AllowUserToDeleteRows = false;
            this.dgvSounds.AllowUserToDragDropRows = false;
            this.dgvSounds.ColumnHeadersHeight = 17;
            this.dgvSounds.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.columnSelected,
            this.columnSoundId,
            this.columnSoundName});
            this.dgvSounds.Dock = System.Windows.Forms.DockStyle.Top;
            this.dgvSounds.Location = new System.Drawing.Point(0, 0);
            this.dgvSounds.Name = "dgvSounds";
            this.dgvSounds.RowHeadersWidth = 41;
            this.dgvSounds.Size = new System.Drawing.Size(397, 465);
            this.dgvSounds.TabIndex = 49;
            // 
            // columnSelected
            // 
            this.columnSelected.HeaderText = "";
            this.columnSelected.Name = "columnSelected";
            this.columnSelected.Width = 40;
            // 
            // columnSoundId
            // 
            this.columnSoundId.HeaderText = "Id";
            this.columnSoundId.Name = "columnSoundId";
            this.columnSoundId.ReadOnly = true;
            this.columnSoundId.Width = 80;
            // 
            // columnSoundName
            // 
            this.columnSoundName.HeaderText = "Name";
            this.columnSoundName.Name = "columnSoundName";
            this.columnSoundName.ReadOnly = true;
            this.columnSoundName.Width = 250;
            // 
            // butPlaySound
            // 
            this.butPlaySound.Image = global::WadTool.Properties.Resources.play_16;
            this.butPlaySound.Location = new System.Drawing.Point(199, 471);
            this.butPlaySound.Name = "butPlaySound";
            this.butPlaySound.Size = new System.Drawing.Size(112, 23);
            this.butPlaySound.TabIndex = 50;
            this.butPlaySound.Text = "Play sound";
            this.butPlaySound.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.butPlaySound.Click += new System.EventHandler(this.butPlaySound_Click);
            // 
            // FormTr2r3SoundManager
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(397, 532);
            this.Controls.Add(this.butPlaySound);
            this.Controls.Add(this.dgvSounds);
            this.Controls.Add(this.darkStatusStrip1);
            this.Controls.Add(this.butSaveChanges);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Name = "FormTr2r3SoundManager";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Sound manager";
            this.darkStatusStrip1.ResumeLayout(false);
            this.darkStatusStrip1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvSounds)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion
        private DarkUI.Controls.DarkButton butSaveChanges;
        private DarkUI.Controls.DarkStatusStrip darkStatusStrip1;
        private System.Windows.Forms.ToolStripStatusLabel labelStatistics;
        private DarkUI.Controls.DarkDataGridView dgvSounds;
        private System.Windows.Forms.DataGridViewCheckBoxColumn columnSelected;
        private System.Windows.Forms.DataGridViewTextBoxColumn columnSoundId;
        private System.Windows.Forms.DataGridViewTextBoxColumn columnSoundName;
        private DarkUI.Controls.DarkButton butPlaySound;
    }
}