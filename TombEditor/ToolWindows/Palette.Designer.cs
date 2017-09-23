namespace TombEditor.ToolWindows
{
    partial class Palette
    {
        /// <summary> 
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.lightPalette = new TombEditor.Controls.PanelPalette();
            ((System.ComponentModel.ISupportInitialize)(this.lightPalette)).BeginInit();
            this.SuspendLayout();
            // 
            // lightPalette
            // 
            this.lightPalette.Location = new System.Drawing.Point(2, 28);
            this.lightPalette.Name = "lightPalette";
            this.lightPalette.Size = new System.Drawing.Size(642, 99);
            this.lightPalette.TabIndex = 82;
            this.lightPalette.TabStop = false;
            // 
            // Palette
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.lightPalette);
            this.DefaultDockArea = DarkUI.Docking.DarkDockArea.Bottom;
            this.DockText = "Palette";
            this.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.MinimumSize = new System.Drawing.Size(645, 128);
            this.Name = "Palette";
            this.SerializationKey = "Palette";
            this.Size = new System.Drawing.Size(645, 128);
            ((System.ComponentModel.ISupportInitialize)(this.lightPalette)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private Controls.PanelPalette lightPalette;
    }
}
