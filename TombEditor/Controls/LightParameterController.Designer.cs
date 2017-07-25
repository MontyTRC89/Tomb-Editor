namespace TombEditor.Controls
{
    partial class LightParameterController
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
            this.butUp = new DarkUI.Controls.DarkButton();
            this.butDown = new DarkUI.Controls.DarkButton();
            this.labelContent = new DarkUI.Controls.DarkLabel();
            this.SuspendLayout();
            // 
            // butUp
            // 
            this.butUp.Image = global::TombEditor.Properties.Resources.controller_up;
            this.butUp.Location = new System.Drawing.Point(44, 0);
            this.butUp.Name = "butUp";
            this.butUp.Padding = new System.Windows.Forms.Padding(5);
            this.butUp.Size = new System.Drawing.Size(16, 11);
            this.butUp.TabIndex = 2;
            this.butUp.Text = "Cancel";
            this.butUp.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.butUp.MouseDown += new System.Windows.Forms.MouseEventHandler(this.butUp_MouseDown);
            // 
            // butDown
            // 
            this.butDown.Image = global::TombEditor.Properties.Resources.controller_down;
            this.butDown.Location = new System.Drawing.Point(44, 11);
            this.butDown.Name = "butDown";
            this.butDown.Padding = new System.Windows.Forms.Padding(5);
            this.butDown.Size = new System.Drawing.Size(16, 11);
            this.butDown.TabIndex = 3;
            this.butDown.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.butDown.MouseDown += new System.Windows.Forms.MouseEventHandler(this.butDown_MouseDown);
            // 
            // labelContent
            // 
            this.labelContent.AutoSize = true;
            this.labelContent.BackColor = System.Drawing.Color.Transparent;
            this.labelContent.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelContent.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.labelContent.Location = new System.Drawing.Point(3, 5);
            this.labelContent.Name = "labelContent";
            this.labelContent.Size = new System.Drawing.Size(28, 13);
            this.labelContent.TabIndex = 49;
            this.labelContent.Text = "0,00";
            // 
            // LightParameterController
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.WindowText;
            this.Controls.Add(this.labelContent);
            this.Controls.Add(this.butDown);
            this.Controls.Add(this.butUp);
            this.Name = "LightParameterController";
            this.Size = new System.Drawing.Size(60, 22);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private DarkUI.Controls.DarkButton butUp;
        private DarkUI.Controls.DarkButton butDown;
        private DarkUI.Controls.DarkLabel labelContent;
    }
}
