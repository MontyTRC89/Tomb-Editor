namespace TombLib.Controls.VisualScripting
{
    partial class VisibleNodeCondition
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.cbAction = new DarkUI.Controls.DarkComboBox();
            this.cbArgument = new DarkUI.Controls.DarkComboBox();
            this.nudArgument = new DarkUI.Controls.DarkNumericUpDown();
            this.panelArgument = new DarkUI.Controls.DarkPanel();
            ((System.ComponentModel.ISupportInitialize)(this.nudArgument)).BeginInit();
            this.SuspendLayout();
            // 
            // cbAction
            // 
            this.cbAction.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.cbAction.FormattingEnabled = true;
            this.cbAction.Location = new System.Drawing.Point(9, 9);
            this.cbAction.Name = "cbAction";
            this.cbAction.Size = new System.Drawing.Size(292, 24);
            this.cbAction.TabIndex = 0;
            // 
            // cbArgument
            // 
            this.cbArgument.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.cbArgument.FormattingEnabled = true;
            this.cbArgument.Location = new System.Drawing.Point(307, 9);
            this.cbArgument.Name = "cbArgument";
            this.cbArgument.Size = new System.Drawing.Size(141, 24);
            this.cbArgument.TabIndex = 1;
            // 
            // nudArgument
            // 
            this.nudArgument.IncrementAlternate = new decimal(new int[] {
            10,
            0,
            0,
            65536});
            this.nudArgument.Location = new System.Drawing.Point(307, 9);
            this.nudArgument.LoopValues = false;
            this.nudArgument.Name = "nudArgument";
            this.nudArgument.Size = new System.Drawing.Size(141, 23);
            this.nudArgument.TabIndex = 2;
            // 
            // panelArgument
            // 
            this.panelArgument.Location = new System.Drawing.Point(307, 9);
            this.panelArgument.Name = "panelArgument";
            this.panelArgument.Size = new System.Drawing.Size(141, 24);
            this.panelArgument.TabIndex = 3;
            // 
            // VisibleNodeCondition
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.WindowFrame;
            this.Controls.Add(this.nudArgument);
            this.Controls.Add(this.cbArgument);
            this.Controls.Add(this.panelArgument);
            this.Controls.Add(this.cbAction);
            this.GripSize = 0;
            this.Name = "VisibleNodeCondition";
            this.Size = new System.Drawing.Size(455, 43);
            ((System.ComponentModel.ISupportInitialize)(this.nudArgument)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private DarkUI.Controls.DarkComboBox cbAction;
        private DarkUI.Controls.DarkComboBox cbArgument;
        private DarkUI.Controls.DarkNumericUpDown nudArgument;
        private DarkUI.Controls.DarkPanel panelArgument;
    }
}
