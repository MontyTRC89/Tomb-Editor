namespace WadTool
{
    partial class FormMain
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

        #region Codice generato da Progettazione Windows Form

        /// <summary>
        /// Metodo necessario per il supporto della finestra di progettazione. Non modificare
        /// il contenuto del metodo con l'editor di codice.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormMain));
            this.butTest = new DarkUI.Controls.DarkButton();
            this.SuspendLayout();
            // 
            // butTest
            // 
            this.butTest.Location = new System.Drawing.Point(13, 13);
            this.butTest.Name = "butTest";
            this.butTest.Padding = new System.Windows.Forms.Padding(5);
            this.butTest.Size = new System.Drawing.Size(75, 23);
            this.butTest.TabIndex = 0;
            this.butTest.Text = "Test";
            this.butTest.Click += new System.EventHandler(this.butTest_Click);
            // 
            // FormMain
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(448, 210);
            this.Controls.Add(this.butTest);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "FormMain";
            this.Text = "WadTool test form";
            this.ResumeLayout(false);

        }

        #endregion

        private DarkUI.Controls.DarkButton butTest;
    }
}

