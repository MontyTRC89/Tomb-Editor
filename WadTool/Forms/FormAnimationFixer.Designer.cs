namespace WadTool
{
    partial class FormAnimationFixer
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.btCancel = new DarkUI.Controls.DarkButton();
            this.btOk = new DarkUI.Controls.DarkButton();
            this.cbNextAnim = new DarkUI.Controls.DarkCheckBox();
            this.darkLabel1 = new DarkUI.Controls.DarkLabel();
            this.cbEndFrame = new DarkUI.Controls.DarkCheckBox();
            this.cbNextFrame = new DarkUI.Controls.DarkCheckBox();
            this.cbSchRanges = new DarkUI.Controls.DarkCheckBox();
            this.cbSchNextAnim = new DarkUI.Controls.DarkCheckBox();
            this.cbSchNextFrame = new DarkUI.Controls.DarkCheckBox();
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.SuspendLayout();
            // 
            // btCancel
            // 
            this.btCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btCancel.Checked = false;
            this.btCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btCancel.Location = new System.Drawing.Point(159, 194);
            this.btCancel.Name = "btCancel";
            this.btCancel.Size = new System.Drawing.Size(80, 23);
            this.btCancel.TabIndex = 50;
            this.btCancel.Text = "Cancel";
            this.btCancel.Click += new System.EventHandler(this.btCancel_Click);
            // 
            // btOk
            // 
            this.btOk.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btOk.Checked = false;
            this.btOk.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btOk.Location = new System.Drawing.Point(73, 194);
            this.btOk.Name = "btOk";
            this.btOk.Size = new System.Drawing.Size(80, 23);
            this.btOk.TabIndex = 51;
            this.btOk.Text = "OK";
            this.btOk.Click += new System.EventHandler(this.btOk_Click);
            // 
            // cbNextAnim
            // 
            this.cbNextAnim.AutoSize = true;
            this.cbNextAnim.Location = new System.Drawing.Point(10, 66);
            this.cbNextAnim.Name = "cbNextAnim";
            this.cbNextAnim.Size = new System.Drawing.Size(104, 17);
            this.cbNextAnim.TabIndex = 52;
            this.cbNextAnim.Text = "Next animation";
            this.toolTip1.SetToolTip(this.cbNextAnim, "Clamp next animation number to actual animation count.\r\nThis may break transition" +
        "s to animations in other slots, like vehicle extra.");
            // 
            // darkLabel1
            // 
            this.darkLabel1.AutoSize = true;
            this.darkLabel1.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.darkLabel1.Location = new System.Drawing.Point(7, 7);
            this.darkLabel1.Name = "darkLabel1";
            this.darkLabel1.Size = new System.Drawing.Size(228, 26);
            this.darkLabel1.TabIndex = 53;
            this.darkLabel1.Text = "Select parameters that you want to fix.\r\nHover on a checkbox for more information" +
    ".";
            // 
            // cbEndFrame
            // 
            this.cbEndFrame.AutoSize = true;
            this.cbEndFrame.Location = new System.Drawing.Point(10, 43);
            this.cbEndFrame.Name = "cbEndFrame";
            this.cbEndFrame.Size = new System.Drawing.Size(78, 17);
            this.cbEndFrame.TabIndex = 54;
            this.cbEndFrame.Text = "End frame";
            this.toolTip1.SetToolTip(this.cbEndFrame, "Clamp end frame value to animation\'s frame count.");
            // 
            // cbNextFrame
            // 
            this.cbNextFrame.AutoSize = true;
            this.cbNextFrame.Location = new System.Drawing.Point(10, 89);
            this.cbNextFrame.Name = "cbNextFrame";
            this.cbNextFrame.Size = new System.Drawing.Size(81, 17);
            this.cbNextFrame.TabIndex = 55;
            this.cbNextFrame.Text = "Next frame";
            this.toolTip1.SetToolTip(this.cbNextFrame, "Clamp next frame to next animation\'s end frame.\r\nThis may help in cases when end " +
        "frame was manually edited for next animation.");
            // 
            // cbSchRanges
            // 
            this.cbSchRanges.AutoSize = true;
            this.cbSchRanges.Location = new System.Drawing.Point(10, 112);
            this.cbSchRanges.Name = "cbSchRanges";
            this.cbSchRanges.Size = new System.Drawing.Size(131, 17);
            this.cbSchRanges.TabIndex = 56;
            this.cbSchRanges.Text = "State change ranges";
            this.toolTip1.SetToolTip(this.cbSchRanges, "Clamps low and high frame bounds for all state changes to animation\'s end frame\r\n" +
        "and solves the cases when low frame is greater than high frame.");
            // 
            // cbSchNextAnim
            // 
            this.cbSchNextAnim.AutoSize = true;
            this.cbSchNextAnim.Location = new System.Drawing.Point(10, 135);
            this.cbSchNextAnim.Name = "cbSchNextAnim";
            this.cbSchNextAnim.Size = new System.Drawing.Size(173, 17);
            this.cbSchNextAnim.TabIndex = 57;
            this.cbSchNextAnim.Text = "State change next animation";
            this.toolTip1.SetToolTip(this.cbSchNextAnim, "Clamp next animation number to actual animation count for all state changes.\r\nThi" +
        "s may break transitions to animations in other slots, like vehicle extra.");
            // 
            // cbSchNextFrame
            // 
            this.cbSchNextFrame.AutoSize = true;
            this.cbSchNextFrame.Location = new System.Drawing.Point(10, 158);
            this.cbSchNextFrame.Name = "cbSchNextFrame";
            this.cbSchNextFrame.Size = new System.Drawing.Size(150, 17);
            this.cbSchNextFrame.TabIndex = 58;
            this.cbSchNextFrame.Text = "State change next frame";
            this.toolTip1.SetToolTip(this.cbSchNextFrame, "Clamp next frame to next animation\'s end frame for all state changes.\r\nThis may h" +
        "elp in cases when end frame was manually edited for next animation.");
            // 
            // FormAnimationFixer
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(244, 222);
            this.Controls.Add(this.cbSchNextFrame);
            this.Controls.Add(this.cbSchNextAnim);
            this.Controls.Add(this.cbSchRanges);
            this.Controls.Add(this.cbNextFrame);
            this.Controls.Add(this.cbEndFrame);
            this.Controls.Add(this.darkLabel1);
            this.Controls.Add(this.cbNextAnim);
            this.Controls.Add(this.btCancel);
            this.Controls.Add(this.btOk);
            this.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FormAnimationFixer";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
            this.Text = "Animation fixer";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private DarkUI.Controls.DarkButton btCancel;
        private DarkUI.Controls.DarkButton btOk;
        private DarkUI.Controls.DarkCheckBox cbNextAnim;
        private DarkUI.Controls.DarkLabel darkLabel1;
        private DarkUI.Controls.DarkCheckBox cbEndFrame;
        private DarkUI.Controls.DarkCheckBox cbNextFrame;
        private DarkUI.Controls.DarkCheckBox cbSchRanges;
        private DarkUI.Controls.DarkCheckBox cbSchNextAnim;
        private DarkUI.Controls.DarkCheckBox cbSchNextFrame;
        private System.Windows.Forms.ToolTip toolTip1;
    }
}