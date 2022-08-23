
namespace TombEditor.Controls.TriggerConstructor
{
    partial class TCCondition
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
            this.cbCondition = new DarkUI.Controls.DarkComboBox();
            this.butOperator = new DarkUI.Controls.DarkButton();
            this.cbOperand = new DarkUI.Controls.DarkComboBox();
            this.butDelete = new DarkUI.Controls.DarkButton();
            this.tbOperand = new DarkUI.Controls.DarkTextBox();
            this.SuspendLayout();
            // 
            // cbCondition
            // 
            this.cbCondition.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.cbCondition.FormattingEnabled = true;
            this.cbCondition.Location = new System.Drawing.Point(4, 5);
            this.cbCondition.Name = "cbCondition";
            this.cbCondition.Size = new System.Drawing.Size(183, 26);
            this.cbCondition.TabIndex = 0;
            // 
            // butOperator
            // 
            this.butOperator.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.butOperator.Checked = false;
            this.butOperator.Font = new System.Drawing.Font("Segoe UI Semibold", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.butOperator.Location = new System.Drawing.Point(193, 5);
            this.butOperator.Name = "butOperator";
            this.butOperator.Size = new System.Drawing.Size(42, 26);
            this.butOperator.TabIndex = 1;
            this.butOperator.Text = ">=";
            // 
            // cbOperand
            // 
            this.cbOperand.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.cbOperand.FormattingEnabled = true;
            this.cbOperand.Location = new System.Drawing.Point(241, 5);
            this.cbOperand.Name = "cbOperand";
            this.cbOperand.Size = new System.Drawing.Size(80, 26);
            this.cbOperand.TabIndex = 2;
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
            // tbOperand
            // 
            this.tbOperand.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.tbOperand.Location = new System.Drawing.Point(241, 5);
            this.tbOperand.Name = "tbOperand";
            this.tbOperand.Size = new System.Drawing.Size(80, 25);
            this.tbOperand.TabIndex = 4;
            // 
            // TCCondition
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 17F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.tbOperand);
            this.Controls.Add(this.butDelete);
            this.Controls.Add(this.cbOperand);
            this.Controls.Add(this.butOperator);
            this.Controls.Add(this.cbCondition);
            this.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.Name = "TCCondition";
            this.Size = new System.Drawing.Size(358, 36);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private DarkUI.Controls.DarkComboBox cbCondition;
        private DarkUI.Controls.DarkButton butOperator;
        private DarkUI.Controls.DarkComboBox cbOperand;
        private DarkUI.Controls.DarkButton butDelete;
        private DarkUI.Controls.DarkTextBox tbOperand;
    }
}
