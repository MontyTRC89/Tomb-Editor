namespace SoundTool
{
    partial class FormOptions
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
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.darkLabel73 = new DarkUI.Controls.DarkLabel();
            this.darkNumericUpDown42 = new DarkUI.Controls.DarkNumericUpDown();
            this.darkLabel72 = new DarkUI.Controls.DarkLabel();
            this.darkCheckBox1 = new DarkUI.Controls.DarkCheckBox();
            this.tabbedContainer.SuspendLayout();
            this.tabPage1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.darkNumericUpDown42)).BeginInit();
            this.SuspendLayout();
            // 
            // tabbedContainer
            // 
            this.tabbedContainer.Controls.Add(this.tabPage1);
            this.tabbedContainer.Size = new System.Drawing.Size(292, 108);
            // 
            // tabPage1
            // 
            this.tabPage1.BackColor = System.Drawing.Color.Transparent;
            this.tabPage1.Controls.Add(this.darkLabel73);
            this.tabPage1.Controls.Add(this.darkNumericUpDown42);
            this.tabPage1.Controls.Add(this.darkLabel72);
            this.tabPage1.Controls.Add(this.darkCheckBox1);
            this.tabPage1.Location = new System.Drawing.Point(4, 22);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage1.Size = new System.Drawing.Size(284, 82);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "General";
            // 
            // darkLabel73
            // 
            this.darkLabel73.AutoSize = true;
            this.darkLabel73.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.darkLabel73.Location = new System.Drawing.Point(263, 37);
            this.darkLabel73.Name = "darkLabel73";
            this.darkLabel73.Size = new System.Drawing.Size(16, 13);
            this.darkLabel73.TabIndex = 77;
            this.darkLabel73.Text = "%";
            // 
            // darkNumericUpDown42
            // 
            this.darkNumericUpDown42.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.darkNumericUpDown42.IncrementAlternate = new decimal(new int[] {
            5,
            0,
            0,
            0});
            this.darkNumericUpDown42.Location = new System.Drawing.Point(190, 34);
            this.darkNumericUpDown42.LoopValues = false;
            this.darkNumericUpDown42.Minimum = new decimal(new int[] {
            50,
            0,
            0,
            0});
            this.darkNumericUpDown42.Name = "darkNumericUpDown42";
            this.darkNumericUpDown42.Size = new System.Drawing.Size(67, 23);
            this.darkNumericUpDown42.TabIndex = 76;
            this.darkNumericUpDown42.Tag = "UI_FormColor_Brightness";
            this.darkNumericUpDown42.Value = new decimal(new int[] {
            50,
            0,
            0,
            0});
            // 
            // darkLabel72
            // 
            this.darkLabel72.AutoSize = true;
            this.darkLabel72.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.darkLabel72.Location = new System.Drawing.Point(6, 36);
            this.darkLabel72.Name = "darkLabel72";
            this.darkLabel72.Size = new System.Drawing.Size(166, 13);
            this.darkLabel72.TabIndex = 75;
            this.darkLabel72.Text = "UI brightness (requires restart):";
            // 
            // darkCheckBox1
            // 
            this.darkCheckBox1.AutoSize = true;
            this.darkCheckBox1.Location = new System.Drawing.Point(7, 7);
            this.darkCheckBox1.Name = "darkCheckBox1";
            this.darkCheckBox1.Size = new System.Drawing.Size(151, 17);
            this.darkCheckBox1.TabIndex = 0;
            this.darkCheckBox1.Tag = "SoundTool_AllowMultipleInstances";
            this.darkCheckBox1.Text = "Allow multiple instances";
            // 
            // FormOptions
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(512, 153);
            this.Name = "FormOptions";
            this.tabbedContainer.ResumeLayout(false);
            this.tabPage1.ResumeLayout(false);
            this.tabPage1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.darkNumericUpDown42)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TabPage tabPage1;
        private DarkUI.Controls.DarkCheckBox darkCheckBox1;
        private DarkUI.Controls.DarkLabel darkLabel73;
        private DarkUI.Controls.DarkNumericUpDown darkNumericUpDown42;
        private DarkUI.Controls.DarkLabel darkLabel72;
    }
}