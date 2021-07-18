namespace TombEditor.Forms.TombEngine
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
            this.ckFixed = new DarkUI.Controls.DarkCheckBox();
            this.darkLabel1 = new DarkUI.Controls.DarkLabel();
            this.nudMoveTimer = new DarkUI.Controls.DarkNumericUpDown();
            this.tbLuaName = new DarkUI.Controls.DarkTextBox();
            this.labelLuaName = new DarkUI.Controls.DarkLabel();
            ((System.ComponentModel.ISupportInitialize)(this.nudMoveTimer)).BeginInit();
            this.SuspendLayout();
            // 
            // butCancel
            // 
            this.butCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.butCancel.Checked = false;
            this.butCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.butCancel.Location = new System.Drawing.Point(238, 105);
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
            this.butOk.Location = new System.Drawing.Point(152, 105);
            this.butOk.Name = "butOk";
            this.butOk.Size = new System.Drawing.Size(80, 23);
            this.butOk.TabIndex = 1;
            this.butOk.Text = "OK";
            this.butOk.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.butOk.Click += new System.EventHandler(this.butOk_Click);
            // 
            // ckFixed
            // 
            this.ckFixed.AutoSize = true;
            this.ckFixed.Location = new System.Drawing.Point(10, 39);
            this.ckFixed.Name = "ckFixed";
            this.ckFixed.Size = new System.Drawing.Size(172, 17);
            this.ckFixed.TabIndex = 6;
            this.ckFixed.Text = "Lock from look key breakout";
            // 
            // darkLabel1
            // 
            this.darkLabel1.AutoSize = true;
            this.darkLabel1.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.darkLabel1.Location = new System.Drawing.Point(7, 14);
            this.darkLabel1.Name = "darkLabel1";
            this.darkLabel1.Size = new System.Drawing.Size(93, 13);
            this.darkLabel1.TabIndex = 7;
            this.darkLabel1.Text = "Movement timer:";
            // 
            // nudMoveTimer
            // 
            this.nudMoveTimer.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.nudMoveTimer.IncrementAlternate = new decimal(new int[] {
            10,
            0,
            0,
            65536});
            this.nudMoveTimer.Location = new System.Drawing.Point(238, 12);
            this.nudMoveTimer.LoopValues = false;
            this.nudMoveTimer.Maximum = new decimal(new int[] {
            31,
            0,
            0,
            0});
            this.nudMoveTimer.Name = "nudMoveTimer";
            this.nudMoveTimer.Size = new System.Drawing.Size(80, 22);
            this.nudMoveTimer.TabIndex = 8;
            // 
            // tbLuaName
            // 
            this.tbLuaName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tbLuaName.Location = new System.Drawing.Point(88, 66);
            this.tbLuaName.Name = "tbLuaName";
            this.tbLuaName.Size = new System.Drawing.Size(230, 22);
            this.tbLuaName.TabIndex = 19;
            // 
            // labelLuaName
            // 
            this.labelLuaName.AutoSize = true;
            this.labelLuaName.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.labelLuaName.Location = new System.Drawing.Point(7, 68);
            this.labelLuaName.Name = "labelLuaName";
            this.labelLuaName.Size = new System.Drawing.Size(75, 13);
            this.labelLuaName.TabIndex = 20;
            this.labelLuaName.Text = "Lua Name:";
            // 
            // FormCamera
            // 
            this.AcceptButton = this.butOk;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.butCancel;
            this.ClientSize = new System.Drawing.Size(326, 136);
            this.Controls.Add(this.tbLuaName);
            this.Controls.Add(this.labelLuaName);
            this.Controls.Add(this.nudMoveTimer);
            this.Controls.Add(this.darkLabel1);
            this.Controls.Add(this.butCancel);
            this.Controls.Add(this.ckFixed);
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
        private DarkUI.Controls.DarkCheckBox ckFixed;
        private DarkUI.Controls.DarkLabel darkLabel1;
        private DarkUI.Controls.DarkNumericUpDown nudMoveTimer;
        private DarkUI.Controls.DarkTextBox tbLuaName;
        private DarkUI.Controls.DarkLabel labelLuaName;
    }
}