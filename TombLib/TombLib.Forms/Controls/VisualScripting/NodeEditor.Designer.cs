namespace TombLib.Controls.VisualScripting
{
    partial class NodeEditor
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
            this.SuspendLayout();
            // 
            // NodeEditor
            // 
            this.AllowDrop = true;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.DoubleBuffered = true;
            this.Name = "NodeEditor";
            this.DragDrop += new System.Windows.Forms.DragEventHandler(this.NodeEditor_DragDrop);
            this.DragEnter += new System.Windows.Forms.DragEventHandler(this.NodeEditor_DragEnter);
            this.ResumeLayout(false);

        }

        #endregion
    }
}
