
namespace TombEditor.Controls.TriggerConstructor
{
    partial class TCAction
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
            this.cbArgument = new DarkUI.Controls.DarkComboBox();
            this.butDelete = new DarkUI.Controls.DarkButton();
            this.tbArgument = new DarkUI.Controls.DarkTextBox();
            this.SuspendLayout();
            // 
            // cbAction
            // 
            this.cbAction.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.cbAction.FormattingEnabled = true;
            this.cbAction.Location = new System.Drawing.Point(4, 5);
            this.cbAction.Name = "cbAction";
            this.cbAction.Size = new System.Drawing.Size(231, 26);
            this.cbAction.TabIndex = 0;
            // 
            // cbArgument
            // 
            this.cbArgument.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.cbArgument.FormattingEnabled = true;
            this.cbArgument.Location = new System.Drawing.Point(241, 5);
            this.cbArgument.Name = "cbArgument";
            this.cbArgument.Size = new System.Drawing.Size(80, 26);
            this.cbArgument.TabIndex = 2;
            // 
            // butDelete
            // 
            this.butDelete.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.butDelete.Checked = false;
            this.butDelete.Font = new System.Drawing.Font("Segoe UI Semibold", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.butDelete.Image = global::TombEditor.Properties.Resources.general_trash_16;
            this.butDelete.Location = new System.Drawing.Point(327, 5);
            this.butDelete.Name = "butDelete";
            this.butDelete.Size = new System.Drawing.Size(26, 26);
            this.butDelete.TabIndex = 3;
            // 
            // tbArgument
            // 
            this.tbArgument.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.tbArgument.Location = new System.Drawing.Point(241, 5);
            this.tbArgument.Name = "tbArgument";
            this.tbArgument.Size = new System.Drawing.Size(80, 25);
            this.tbArgument.TabIndex = 4;
            // 
            // TCAction
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 17F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.tbArgument);
            this.Controls.Add(this.butDelete);
            this.Controls.Add(this.cbArgument);
            this.Controls.Add(this.cbAction);
            this.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.Name = "TCAction";
            this.Size = new System.Drawing.Size(358, 36);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private DarkUI.Controls.DarkComboBox cbAction;
        private DarkUI.Controls.DarkComboBox cbArgument;
        private DarkUI.Controls.DarkButton butDelete;
        private DarkUI.Controls.DarkTextBox tbArgument;
    }
}
