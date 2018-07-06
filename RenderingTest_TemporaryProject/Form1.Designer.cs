namespace RenderingTest_TemporaryProject
{
    partial class Form1
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.renderingPanel1 = new TombLib.Controls.RenderingPanel();
            this.SuspendLayout();
            // 
            // renderingPanel1
            // 
            this.renderingPanel1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.renderingPanel1.Location = new System.Drawing.Point(12, 12);
            this.renderingPanel1.Name = "renderingPanel1";
            this.renderingPanel1.Size = new System.Drawing.Size(364, 297);
            this.renderingPanel1.TabIndex = 0;
            this.renderingPanel1.Draw += new System.EventHandler(this.renderingPanel1_Draw);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(517, 389);
            this.Controls.Add(this.renderingPanel1);
            this.Name = "Form1";
            this.Text = "Form1";
            this.ResumeLayout(false);

        }

        #endregion

        private TombLib.Controls.RenderingPanel renderingPanel1;
    }
}

