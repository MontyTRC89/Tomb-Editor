namespace TombLib.Controls.VisualScripting
{
    partial class VisibleNodeBase
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
            this.components = new System.ComponentModel.Container();
            this.cbFunction = new TombLib.Controls.DarkSearchableComboBox();
            this.toolTip = new System.Windows.Forms.ToolTip(this.components);
            this.SuspendLayout();
            // 
            // cbFunction
            // 
            this.cbFunction.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.cbFunction.Location = new System.Drawing.Point(9, 9);
            this.cbFunction.Name = "cbFunction";
            this.cbFunction.Size = new System.Drawing.Size(133, 24);
            this.cbFunction.TabIndex = 2;
            this.cbFunction.SelectedIndexChanged += new System.EventHandler(this.cbFunction_SelectedIndexChanged);
            this.cbFunction.MouseDown += new System.Windows.Forms.MouseEventHandler(this.cbFunction_MouseDown);
            // 
            // VisibleNodeBase
            // 
            this.AllowDrop = true;
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.cbFunction);
            this.Font = new System.Drawing.Font("Segoe UI Semilight", 9F);
            this.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.Name = "VisibleNodeBase";
            this.Size = new System.Drawing.Size(150, 42);
            this.VerticalGrip = false;
            this.ResumeLayout(false);

        }

        #endregion

        private TombLib.Controls.DarkSearchableComboBox cbFunction;
        private System.Windows.Forms.ToolTip toolTip;
    }
}
