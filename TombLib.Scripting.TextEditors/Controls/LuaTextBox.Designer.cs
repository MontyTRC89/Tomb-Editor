namespace TombLib.Scripting.Forms.Controls
{
    partial class LuaTextBox
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
            this.ehTextEditor = new System.Windows.Forms.Integration.ElementHost();
            this.SuspendLayout();
            // 
            // ehTextEditor
            // 
            this.ehTextEditor.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ehTextEditor.Location = new System.Drawing.Point(0, 0);
            this.ehTextEditor.Name = "ehTextEditor";
            this.ehTextEditor.Size = new System.Drawing.Size(150, 150);
            this.ehTextEditor.TabIndex = 0;
            this.ehTextEditor.Text = "elementHost1";
            this.ehTextEditor.Child = null;
            // 
            // LuaTextEditor2
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.ehTextEditor);
            this.Name = "LuaTextEditor2";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Integration.ElementHost ehTextEditor;
    }
}
