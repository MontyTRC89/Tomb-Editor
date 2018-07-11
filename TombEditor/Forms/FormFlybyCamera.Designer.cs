using DarkUI.Controls;

namespace TombEditor.Forms
{
    partial class FormFlybyCamera
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
            this.butOK = new DarkUI.Controls.DarkButton();
            this.cbBit0 = new DarkUI.Controls.DarkCheckBox();
            this.cbBit1 = new DarkUI.Controls.DarkCheckBox();
            this.cbBit2 = new DarkUI.Controls.DarkCheckBox();
            this.cbBit3 = new DarkUI.Controls.DarkCheckBox();
            this.cbBit4 = new DarkUI.Controls.DarkCheckBox();
            this.cbBit5 = new DarkUI.Controls.DarkCheckBox();
            this.cbBit6 = new DarkUI.Controls.DarkCheckBox();
            this.cbBit13 = new DarkUI.Controls.DarkCheckBox();
            this.cbBit12 = new DarkUI.Controls.DarkCheckBox();
            this.cbBit11 = new DarkUI.Controls.DarkCheckBox();
            this.cbBit10 = new DarkUI.Controls.DarkCheckBox();
            this.cbBit9 = new DarkUI.Controls.DarkCheckBox();
            this.cbBit8 = new DarkUI.Controls.DarkCheckBox();
            this.cbBit7 = new DarkUI.Controls.DarkCheckBox();
            this.cbBit15 = new DarkUI.Controls.DarkCheckBox();
            this.cbBit14 = new DarkUI.Controls.DarkCheckBox();
            this.label1 = new DarkUI.Controls.DarkLabel();
            this.tbSequence = new DarkUI.Controls.DarkTextBox();
            this.tbNumber = new DarkUI.Controls.DarkTextBox();
            this.label2 = new DarkUI.Controls.DarkLabel();
            this.tbFOV = new DarkUI.Controls.DarkTextBox();
            this.label3 = new DarkUI.Controls.DarkLabel();
            this.tbRoll = new DarkUI.Controls.DarkTextBox();
            this.label4 = new DarkUI.Controls.DarkLabel();
            this.tbTimer = new DarkUI.Controls.DarkTextBox();
            this.label5 = new DarkUI.Controls.DarkLabel();
            this.tbSpeed = new DarkUI.Controls.DarkTextBox();
            this.label6 = new DarkUI.Controls.DarkLabel();
            this.darkLabel1 = new DarkUI.Controls.DarkLabel();
            this.tbRotationX = new DarkUI.Controls.DarkTextBox();
            this.darkLabel2 = new DarkUI.Controls.DarkLabel();
            this.tbRotationY = new DarkUI.Controls.DarkTextBox();
            this.SuspendLayout();
            // 
            // butCancel
            // 
            this.butCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.butCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.butCancel.Location = new System.Drawing.Point(428, 389);
            this.butCancel.Name = "butCancel";
            this.butCancel.Size = new System.Drawing.Size(80, 23);
            this.butCancel.TabIndex = 1;
            this.butCancel.Text = "Cancel";
            this.butCancel.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.butCancel.Click += new System.EventHandler(this.butCancel_Click);
            // 
            // butOK
            // 
            this.butOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.butOK.Location = new System.Drawing.Point(342, 389);
            this.butOK.Name = "butOK";
            this.butOK.Size = new System.Drawing.Size(80, 23);
            this.butOK.TabIndex = 0;
            this.butOK.Text = "OK";
            this.butOK.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.butOK.Click += new System.EventHandler(this.butOK_Click);
            // 
            // cbBit0
            // 
            this.cbBit0.Location = new System.Drawing.Point(208, 12);
            this.cbBit0.Name = "cbBit0";
            this.cbBit0.Size = new System.Drawing.Size(300, 32);
            this.cbBit0.TabIndex = 2;
            this.cbBit0.Text = "Make a cut to flyby from Lara camera position";
            // 
            // cbBit1
            // 
            this.cbBit1.Location = new System.Drawing.Point(208, 35);
            this.cbBit1.Name = "cbBit1";
            this.cbBit1.Size = new System.Drawing.Size(300, 32);
            this.cbBit1.TabIndex = 3;
            this.cbBit1.Text = "Track entity position";
            // 
            // cbBit2
            // 
            this.cbBit2.Location = new System.Drawing.Point(208, 58);
            this.cbBit2.Name = "cbBit2";
            this.cbBit2.Size = new System.Drawing.Size(300, 32);
            this.cbBit2.TabIndex = 4;
            this.cbBit2.Text = "Infinite loop";
            // 
            // cbBit3
            // 
            this.cbBit3.Location = new System.Drawing.Point(208, 81);
            this.cbBit3.Name = "cbBit3";
            this.cbBit3.Size = new System.Drawing.Size(300, 32);
            this.cbBit3.TabIndex = 5;
            this.cbBit3.Text = "Create tracking camera";
            // 
            // cbBit4
            // 
            this.cbBit4.Location = new System.Drawing.Point(208, 104);
            this.cbBit4.Name = "cbBit4";
            this.cbBit4.Size = new System.Drawing.Size(300, 32);
            this.cbBit4.TabIndex = 6;
            this.cbBit4.Text = "Focus on Lara\'s last head position";
            // 
            // cbBit5
            // 
            this.cbBit5.Location = new System.Drawing.Point(208, 127);
            this.cbBit5.Name = "cbBit5";
            this.cbBit5.Size = new System.Drawing.Size(300, 32);
            this.cbBit5.TabIndex = 7;
            this.cbBit5.Text = "Focus on Lara\'s head";
            // 
            // cbBit6
            // 
            this.cbBit6.Location = new System.Drawing.Point(208, 150);
            this.cbBit6.Name = "cbBit6";
            this.cbBit6.Size = new System.Drawing.Size(300, 32);
            this.cbBit6.TabIndex = 8;
            this.cbBit6.Text = "Pan back to Lara\'s camera position";
            // 
            // cbBit13
            // 
            this.cbBit13.Location = new System.Drawing.Point(208, 311);
            this.cbBit13.Name = "cbBit13";
            this.cbBit13.Size = new System.Drawing.Size(300, 32);
            this.cbBit13.TabIndex = 15;
            this.cbBit13.Text = "Unused";
            // 
            // cbBit12
            // 
            this.cbBit12.Location = new System.Drawing.Point(208, 288);
            this.cbBit12.Name = "cbBit12";
            this.cbBit12.Size = new System.Drawing.Size(300, 32);
            this.cbBit12.TabIndex = 14;
            this.cbBit12.Text = "Unused";
            // 
            // cbBit11
            // 
            this.cbBit11.Location = new System.Drawing.Point(208, 265);
            this.cbBit11.Name = "cbBit11";
            this.cbBit11.Size = new System.Drawing.Size(300, 32);
            this.cbBit11.TabIndex = 13;
            this.cbBit11.Text = "Override cinematic mode and let Lara move";
            // 
            // cbBit10
            // 
            this.cbBit10.Location = new System.Drawing.Point(208, 242);
            this.cbBit10.Name = "cbBit10";
            this.cbBit10.Size = new System.Drawing.Size(300, 32);
            this.cbBit10.TabIndex = 12;
            this.cbBit10.Text = "Cinematic mode";
            // 
            // cbBit9
            // 
            this.cbBit9.Location = new System.Drawing.Point(208, 219);
            this.cbBit9.Name = "cbBit9";
            this.cbBit9.Size = new System.Drawing.Size(300, 32);
            this.cbBit9.TabIndex = 11;
            this.cbBit9.Text = "Disable exit from sequence with \"Look At\" key";
            // 
            // cbBit8
            // 
            this.cbBit8.Location = new System.Drawing.Point(208, 196);
            this.cbBit8.Name = "cbBit8";
            this.cbBit8.Size = new System.Drawing.Size(300, 32);
            this.cbBit8.TabIndex = 10;
            this.cbBit8.Text = "Freeze camera";
            // 
            // cbBit7
            // 
            this.cbBit7.Location = new System.Drawing.Point(208, 173);
            this.cbBit7.Name = "cbBit7";
            this.cbBit7.Size = new System.Drawing.Size(300, 32);
            this.cbBit7.TabIndex = 9;
            this.cbBit7.Text = "Jump to another flyby";
            // 
            // cbBit15
            // 
            this.cbBit15.Location = new System.Drawing.Point(208, 357);
            this.cbBit15.Name = "cbBit15";
            this.cbBit15.Size = new System.Drawing.Size(300, 32);
            this.cbBit15.TabIndex = 17;
            this.cbBit15.Text = "Unused";
            // 
            // cbBit14
            // 
            this.cbBit14.Location = new System.Drawing.Point(208, 334);
            this.cbBit14.Name = "cbBit14";
            this.cbBit14.Size = new System.Drawing.Size(300, 32);
            this.cbBit14.TabIndex = 16;
            this.cbBit14.Text = "Activate heavy trigger";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.label1.Location = new System.Drawing.Point(16, 22);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(60, 13);
            this.label1.TabIndex = 18;
            this.label1.Text = "Sequence:";
            // 
            // tbSequence
            // 
            this.tbSequence.Location = new System.Drawing.Point(81, 19);
            this.tbSequence.Name = "tbSequence";
            this.tbSequence.Size = new System.Drawing.Size(71, 22);
            this.tbSequence.TabIndex = 19;
            // 
            // tbNumber
            // 
            this.tbNumber.Location = new System.Drawing.Point(81, 45);
            this.tbNumber.Name = "tbNumber";
            this.tbNumber.Size = new System.Drawing.Size(71, 22);
            this.tbNumber.TabIndex = 21;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.label2.Location = new System.Drawing.Point(16, 48);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(51, 13);
            this.label2.TabIndex = 20;
            this.label2.Text = "Number:";
            // 
            // tbFOV
            // 
            this.tbFOV.Location = new System.Drawing.Point(81, 124);
            this.tbFOV.Name = "tbFOV";
            this.tbFOV.Size = new System.Drawing.Size(71, 22);
            this.tbFOV.TabIndex = 23;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.label3.Location = new System.Drawing.Point(16, 127);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(32, 13);
            this.label3.TabIndex = 22;
            this.label3.Text = "FOV:";
            // 
            // tbRoll
            // 
            this.tbRoll.Location = new System.Drawing.Point(81, 151);
            this.tbRoll.Name = "tbRoll";
            this.tbRoll.Size = new System.Drawing.Size(71, 22);
            this.tbRoll.TabIndex = 25;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.label4.Location = new System.Drawing.Point(16, 154);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(30, 13);
            this.label4.TabIndex = 24;
            this.label4.Text = "Roll:";
            // 
            // tbTimer
            // 
            this.tbTimer.Location = new System.Drawing.Point(81, 71);
            this.tbTimer.Name = "tbTimer";
            this.tbTimer.Size = new System.Drawing.Size(71, 22);
            this.tbTimer.TabIndex = 27;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.label5.Location = new System.Drawing.Point(16, 74);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(37, 13);
            this.label5.TabIndex = 26;
            this.label5.Text = "Timer:";
            // 
            // tbSpeed
            // 
            this.tbSpeed.Location = new System.Drawing.Point(81, 97);
            this.tbSpeed.Name = "tbSpeed";
            this.tbSpeed.Size = new System.Drawing.Size(71, 22);
            this.tbSpeed.TabIndex = 29;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.label6.Location = new System.Drawing.Point(16, 100);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(42, 13);
            this.label6.TabIndex = 28;
            this.label6.Text = "Speed:";
            // 
            // darkLabel1
            // 
            this.darkLabel1.AutoSize = true;
            this.darkLabel1.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.darkLabel1.Location = new System.Drawing.Point(16, 181);
            this.darkLabel1.Name = "darkLabel1";
            this.darkLabel1.Size = new System.Drawing.Size(64, 13);
            this.darkLabel1.TabIndex = 22;
            this.darkLabel1.Text = "Rotation X:";
            // 
            // tbRotationX
            // 
            this.tbRotationX.Location = new System.Drawing.Point(81, 178);
            this.tbRotationX.Name = "tbRotationX";
            this.tbRotationX.Size = new System.Drawing.Size(71, 22);
            this.tbRotationX.TabIndex = 23;
            // 
            // darkLabel2
            // 
            this.darkLabel2.AutoSize = true;
            this.darkLabel2.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.darkLabel2.Location = new System.Drawing.Point(16, 208);
            this.darkLabel2.Name = "darkLabel2";
            this.darkLabel2.Size = new System.Drawing.Size(63, 13);
            this.darkLabel2.TabIndex = 24;
            this.darkLabel2.Text = "Rotation Y:";
            // 
            // tbRotationY
            // 
            this.tbRotationY.Location = new System.Drawing.Point(81, 205);
            this.tbRotationY.Name = "tbRotationY";
            this.tbRotationY.Size = new System.Drawing.Size(71, 22);
            this.tbRotationY.TabIndex = 25;
            // 
            // FormFlybyCamera
            // 
            this.AcceptButton = this.butOK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.butCancel;
            this.ClientSize = new System.Drawing.Size(513, 418);
            this.Controls.Add(this.tbSpeed);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.tbTimer);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.tbRotationY);
            this.Controls.Add(this.tbRoll);
            this.Controls.Add(this.darkLabel2);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.tbRotationX);
            this.Controls.Add(this.darkLabel1);
            this.Controls.Add(this.tbFOV);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.tbNumber);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.tbSequence);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.cbBit15);
            this.Controls.Add(this.cbBit14);
            this.Controls.Add(this.cbBit13);
            this.Controls.Add(this.cbBit12);
            this.Controls.Add(this.cbBit11);
            this.Controls.Add(this.cbBit10);
            this.Controls.Add(this.cbBit9);
            this.Controls.Add(this.cbBit8);
            this.Controls.Add(this.cbBit7);
            this.Controls.Add(this.cbBit6);
            this.Controls.Add(this.cbBit5);
            this.Controls.Add(this.cbBit4);
            this.Controls.Add(this.cbBit3);
            this.Controls.Add(this.cbBit2);
            this.Controls.Add(this.cbBit1);
            this.Controls.Add(this.cbBit0);
            this.Controls.Add(this.butCancel);
            this.Controls.Add(this.butOK);
            this.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Fixed3D;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FormFlybyCamera";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Flyby camera";
            this.Load += new System.EventHandler(this.FormFlybyCamera_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private DarkButton butOK;
        private DarkButton butCancel;
        private DarkCheckBox cbBit0;
        private DarkCheckBox cbBit1;
        private DarkCheckBox cbBit2;
        private DarkCheckBox cbBit3;
        private DarkCheckBox cbBit4;
        private DarkCheckBox cbBit5;
        private DarkCheckBox cbBit6;
        private DarkCheckBox cbBit13;
        private DarkCheckBox cbBit12;
        private DarkCheckBox cbBit11;
        private DarkCheckBox cbBit10;
        private DarkCheckBox cbBit9;
        private DarkCheckBox cbBit8;
        private DarkCheckBox cbBit7;
        private DarkCheckBox cbBit15;
        private DarkCheckBox cbBit14;
        private DarkLabel label1;
        private DarkTextBox tbSequence;
        private DarkTextBox tbNumber;
        private DarkLabel label2;
        private DarkTextBox tbFOV;
        private DarkLabel label3;
        private DarkTextBox tbRoll;
        private DarkLabel label4;
        private DarkTextBox tbTimer;
        private DarkLabel label5;
        private DarkTextBox tbSpeed;
        private DarkLabel label6;
        private DarkLabel darkLabel1;
        private DarkTextBox tbRotationX;
        private DarkLabel darkLabel2;
        private DarkTextBox tbRotationY;
    }
}