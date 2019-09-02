namespace TombLib.Controls
{
    partial class AnimationTrackBar
    {
        /// <summary> 
        /// Variabile di progettazione necessaria.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Pulire le risorse in uso.
        /// </summary>
        /// <param name="disposing">ha valore true se le risorse gestite devono essere eliminate, false in caso contrario.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Codice generato da Progettazione componenti

        /// <summary> 
        /// Metodo necessario per il supporto della finestra di progettazione. Non modificare 
        /// il contenuto del metodo con l'editor di codice.
        /// </summary>
        private void InitializeComponent()
        {
            this.panelCommands = new System.Windows.Forms.Panel();
            this.picSlider = new System.Windows.Forms.PictureBox();
            ((System.ComponentModel.ISupportInitialize)(this.picSlider)).BeginInit();
            this.SuspendLayout();
            // 
            // panelCommands
            // 
            this.panelCommands.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.panelCommands.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelCommands.Location = new System.Drawing.Point(0, 25);
            this.panelCommands.Name = "panelCommands";
            this.panelCommands.Size = new System.Drawing.Size(391, 72);
            this.panelCommands.TabIndex = 1;
            // 
            // picSlider
            // 
            this.picSlider.BackColor = System.Drawing.Color.Black;
            this.picSlider.Dock = System.Windows.Forms.DockStyle.Top;
            this.picSlider.Location = new System.Drawing.Point(0, 0);
            this.picSlider.Name = "picSlider";
            this.picSlider.Size = new System.Drawing.Size(391, 25);
            this.picSlider.TabIndex = 1;
            this.picSlider.TabStop = false;
            // 
            // AnimationTrackBar
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.panelCommands);
            this.Controls.Add(this.picSlider);
            this.Name = "AnimationTrackBar";
            this.Size = new System.Drawing.Size(391, 97);
            ((System.ComponentModel.ISupportInitialize)(this.picSlider)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.Panel panelCommands;
        private System.Windows.Forms.PictureBox picSlider;
    }
}
