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
            this.picSlider = new System.Windows.Forms.PictureBox();
            ((System.ComponentModel.ISupportInitialize)(this.picSlider)).BeginInit();
            this.SuspendLayout();
            // 
            // picSlider
            // 
            this.picSlider.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.picSlider.Dock = System.Windows.Forms.DockStyle.Fill;
            this.picSlider.Location = new System.Drawing.Point(0, 0);
            this.picSlider.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.picSlider.Name = "picSlider";
            this.picSlider.Padding = new System.Windows.Forms.Padding(4);
            this.picSlider.Size = new System.Drawing.Size(326, 37);
            this.picSlider.TabIndex = 1;
            this.picSlider.TabStop = false;
            this.picSlider.SizeChanged += new System.EventHandler(this.picSlider_SizeChanged);
            this.picSlider.Paint += new System.Windows.Forms.PaintEventHandler(this.picSlider_Paint);
            this.picSlider.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.picSlider_MouseDoubleClick);
            this.picSlider.MouseDown += new System.Windows.Forms.MouseEventHandler(this.picSlider_MouseDown);
            this.picSlider.MouseMove += new System.Windows.Forms.MouseEventHandler(this.picSlider_MouseMove);
            this.picSlider.MouseUp += new System.Windows.Forms.MouseEventHandler(this.picSlider_MouseUp);
            // 
            // AnimationTrackBar
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(5F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.picSlider);
            this.Font = new System.Drawing.Font("Segoe UI", 6.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.Name = "AnimationTrackBar";
            this.Size = new System.Drawing.Size(326, 37);
            ((System.ComponentModel.ISupportInitialize)(this.picSlider)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.PictureBox picSlider;
    }
}
