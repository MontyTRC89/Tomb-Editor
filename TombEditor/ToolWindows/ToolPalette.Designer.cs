namespace TombEditor.ToolWindows
{
    partial class ToolPalette
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
            this.toolBox = new TombEditor.Controls.ToolBox();
            this.SuspendLayout();
            // 
            // toolBox
            // 
            this.toolBox.AutoSize = true;
            this.toolBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.toolBox.LayoutStyle = System.Windows.Forms.ToolStripLayoutStyle.Flow;
            this.toolBox.Location = new System.Drawing.Point(0, 25);
            this.toolBox.Name = "toolBox";
            this.toolBox.Size = new System.Drawing.Size(436, 204);
            this.toolBox.TabIndex = 0;
            this.toolBox.SizeChanged += new System.EventHandler(this.toolBox_SizeChanged);
            // 
            // ToolPalette
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.toolBox);
            this.DefaultDockArea = DarkUI.Docking.DarkDockArea.Right;
            this.DockText = "Tools";
            this.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.MinimumSize = new System.Drawing.Size(28, 52);
            this.Name = "ToolPalette";
            this.SerializationKey = "ToolPalette";
            this.Size = new System.Drawing.Size(436, 229);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private Controls.ToolBox toolBox;
    }
}
