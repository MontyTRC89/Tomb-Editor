namespace TombLib.Controls.VisualScripting
{
    partial class VisibleNodeAction
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
            this.cbAction = new DarkUI.Controls.DarkComboBox();
            this.argumentEditor = new TombLib.Controls.VisualScripting.ArgumentEditor();
            this.SuspendLayout();
            // 
            // cbAction
            // 
            this.cbAction.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.cbAction.FormattingEnabled = true;
            this.cbAction.Location = new System.Drawing.Point(9, 9);
            this.cbAction.Name = "cbAction";
            this.cbAction.Size = new System.Drawing.Size(400, 24);
            this.cbAction.TabIndex = 0;
            this.cbAction.SelectedIndexChanged += new System.EventHandler(this.cbAction_SelectedIndexChanged);
            // 
            // argumentEditor
            // 
            this.argumentEditor.BackColor = System.Drawing.SystemColors.WindowFrame;
            this.argumentEditor.Location = new System.Drawing.Point(9, 39);
            this.argumentEditor.Margin = new System.Windows.Forms.Padding(5, 3, 5, 3);
            this.argumentEditor.Name = "argumentEditor";
            this.argumentEditor.Size = new System.Drawing.Size(400, 24);
            this.argumentEditor.TabIndex = 1;
            // 
            // VisibleNodeAction
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.WindowFrame;
            this.Controls.Add(this.argumentEditor);
            this.Controls.Add(this.cbAction);
            this.GripSize = 0;
            this.Name = "VisibleNodeAction";
            this.Size = new System.Drawing.Size(417, 72);
            this.ResumeLayout(false);

        }

        #endregion

        private DarkUI.Controls.DarkComboBox cbAction;
        private ArgumentEditor argumentEditor;
    }
}
