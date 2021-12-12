namespace TombEditor.Forms
{
    partial class FormCamera
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
            this.butCancel = new DarkUI.Controls.DarkButton();
            this.butOk = new DarkUI.Controls.DarkButton();
            this.darkLabel1 = new DarkUI.Controls.DarkLabel();
            this.nudMoveTimer = new DarkUI.Controls.DarkNumericUpDown();
            this.ckGlideOut = new DarkUI.Controls.DarkCheckBox();
            this.comboCameraMode = new DarkUI.Controls.DarkComboBox();
            this.darkLabel2 = new DarkUI.Controls.DarkLabel();
            ((System.ComponentModel.ISupportInitialize)(this.nudMoveTimer)).BeginInit();
            this.SuspendLayout();
            // 
            // butCancel
            // 
            this.butCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.butCancel.Checked = false;
            this.butCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.butCancel.Location = new System.Drawing.Point(139, 71);
            this.butCancel.Name = "butCancel";
            this.butCancel.Size = new System.Drawing.Size(80, 23);
            this.butCancel.TabIndex = 2;
            this.butCancel.Text = "Cancel";
            this.butCancel.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.butCancel.Click += new System.EventHandler(this.butCancel_Click);
            // 
            // butOk
            // 
            this.butOk.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.butOk.Checked = false;
            this.butOk.Location = new System.Drawing.Point(53, 71);
            this.butOk.Name = "butOk";
            this.butOk.Size = new System.Drawing.Size(80, 23);
            this.butOk.TabIndex = 1;
            this.butOk.Text = "OK";
            this.butOk.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.butOk.Click += new System.EventHandler(this.butOk_Click);
            // 
            // darkLabel1
            // 
            this.darkLabel1.AutoSize = true;
            this.darkLabel1.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.darkLabel1.Location = new System.Drawing.Point(7, 14);
            this.darkLabel1.Name = "darkLabel1";
            this.darkLabel1.Size = new System.Drawing.Size(66, 13);
            this.darkLabel1.TabIndex = 7;
            this.darkLabel1.Text = "Glide timer:";
            // 
            // nudMoveTimer
            // 
            this.nudMoveTimer.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.nudMoveTimer.IncrementAlternate = new decimal(new int[] {
            10,
            0,
            0,
            65536});
            this.nudMoveTimer.Location = new System.Drawing.Point(89, 12);
            this.nudMoveTimer.LoopValues = false;
            this.nudMoveTimer.Maximum = new decimal(new int[] {
            31,
            0,
            0,
            0});
            this.nudMoveTimer.Name = "nudMoveTimer";
            this.nudMoveTimer.Size = new System.Drawing.Size(54, 22);
            this.nudMoveTimer.TabIndex = 8;
            // 
            // ckGlideOut
            // 
            this.ckGlideOut.AutoSize = true;
            this.ckGlideOut.Location = new System.Drawing.Point(151, 13);
            this.ckGlideOut.Name = "ckGlideOut";
            this.ckGlideOut.Size = new System.Drawing.Size(74, 17);
            this.ckGlideOut.TabIndex = 9;
            this.ckGlideOut.Text = "Glide out";
            // 
            // comboCameraMode
            // 
            this.comboCameraMode.FormattingEnabled = true;
            this.comboCameraMode.Items.AddRange(new object[] {
            "Default",
            "Locked",
            "Sniper"});
            this.comboCameraMode.Location = new System.Drawing.Point(89, 40);
            this.comboCameraMode.Name = "comboCameraMode";
            this.comboCameraMode.Size = new System.Drawing.Size(130, 23);
            this.comboCameraMode.TabIndex = 10;
            this.comboCameraMode.SelectedIndexChanged += new System.EventHandler(this.comboCameraMode_SelectedIndexChanged);
            // 
            // darkLabel2
            // 
            this.darkLabel2.AutoSize = true;
            this.darkLabel2.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.darkLabel2.Location = new System.Drawing.Point(7, 43);
            this.darkLabel2.Name = "darkLabel2";
            this.darkLabel2.Size = new System.Drawing.Size(80, 13);
            this.darkLabel2.TabIndex = 11;
            this.darkLabel2.Text = "Camera mode:";
            // 
            // FormCamera
            // 
            this.AcceptButton = this.butOk;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.butCancel;
            this.ClientSize = new System.Drawing.Size(227, 102);
            this.Controls.Add(this.darkLabel2);
            this.Controls.Add(this.comboCameraMode);
            this.Controls.Add(this.ckGlideOut);
            this.Controls.Add(this.nudMoveTimer);
            this.Controls.Add(this.darkLabel1);
            this.Controls.Add(this.butCancel);
            this.Controls.Add(this.butOk);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FormCamera";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Edit camera";
            ((System.ComponentModel.ISupportInitialize)(this.nudMoveTimer)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private DarkUI.Controls.DarkButton butCancel;
        private DarkUI.Controls.DarkButton butOk;
        private DarkUI.Controls.DarkLabel darkLabel1;
        private DarkUI.Controls.DarkNumericUpDown nudMoveTimer;
        private DarkUI.Controls.DarkCheckBox ckGlideOut;
        private DarkUI.Controls.DarkComboBox comboCameraMode;
        private DarkUI.Controls.DarkLabel darkLabel2;
    }
}