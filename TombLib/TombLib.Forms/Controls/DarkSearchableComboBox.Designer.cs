namespace TombLib.Controls
{
    partial class DarkSearchableComboBox
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
            this.button = new DarkUI.Controls.DarkButton();
            this.combo = new DarkUI.Controls.DarkComboBox();
            this.SuspendLayout();
            // 
            // button
            // 
            this.button.Checked = false;
            this.button.Dock = System.Windows.Forms.DockStyle.Right;
            this.button.Image = global::TombLib.Properties.Resources.general_search_16;
            this.button.Location = new System.Drawing.Point(129, 0);
            this.button.Name = "button";
            this.button.Size = new System.Drawing.Size(21, 21);
            this.button.TabIndex = 1;
            this.button.TabStop = false;
            this.button.Click += new System.EventHandler(this.button_Click);
            // 
            // combo
            // 
            this.combo.Dock = System.Windows.Forms.DockStyle.Fill;
            this.combo.DropDownHeight = 400;
            this.combo.FormattingEnabled = true;
            this.combo.IntegralHeight = false;
            this.combo.Location = new System.Drawing.Point(0, 0);
            this.combo.Name = "combo";
            this.combo.Size = new System.Drawing.Size(129, 21);
            this.combo.TabIndex = 0;
            this.combo.Resize += new System.EventHandler(this.combo_Resize);
            // 
            // DarkSearchableComboBox
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.combo);
            this.Controls.Add(this.button);
            this.Name = "DarkSearchableComboBox";
            this.Size = new System.Drawing.Size(150, 21);
            this.Resize += new System.EventHandler(this.DarkSearchableComboBox_Resize);
            this.ResumeLayout(false);

        }

        #endregion

        private DarkUI.Controls.DarkComboBox combo;
        private DarkUI.Controls.DarkButton button;
    }
}
