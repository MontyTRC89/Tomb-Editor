namespace TombLib.Controls
{
    partial class DarkDataGridViewControls
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
            this.butDown = new DarkUI.Controls.DarkButton();
            this.butUp = new DarkUI.Controls.DarkButton();
            this.butNew = new DarkUI.Controls.DarkButton();
            this.butDelete = new DarkUI.Controls.DarkButton();
            this.SuspendLayout();
            // 
            // butDown
            // 
            this.butDown.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.butDown.Image = global::TombLib.Properties.Resources.general_ArrowDown_16;
            this.butDown.Location = new System.Drawing.Point(0, 90);
            this.butDown.Name = "butDown";
            this.butDown.Size = new System.Drawing.Size(24, 24);
            this.butDown.TabIndex = 1;
            this.butDown.Click += new System.EventHandler(this.butDown_Click);
            // 
            // butUp
            // 
            this.butUp.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.butUp.Image = global::TombLib.Properties.Resources.general_ArrowUp_16;
            this.butUp.Location = new System.Drawing.Point(0, 60);
            this.butUp.Name = "butUp";
            this.butUp.Size = new System.Drawing.Size(24, 24);
            this.butUp.TabIndex = 1;
            this.butUp.Click += new System.EventHandler(this.butUp_Click);
            // 
            // butNew
            // 
            this.butNew.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.butNew.Image = global::TombLib.Properties.Resources.general_plus_math_16;
            this.butNew.ImagePadding = 4;
            this.butNew.Location = new System.Drawing.Point(0, 0);
            this.butNew.Name = "butNew";
            this.butNew.Size = new System.Drawing.Size(24, 24);
            this.butNew.TabIndex = 1;
            this.butNew.Click += new System.EventHandler(this.butNew_Click);
            // 
            // butDelete
            // 
            this.butDelete.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.butDelete.Image = global::TombLib.Properties.Resources.general_trash_16;
            this.butDelete.ImagePadding = 3;
            this.butDelete.Location = new System.Drawing.Point(0, 30);
            this.butDelete.Name = "butDelete";
            this.butDelete.Size = new System.Drawing.Size(24, 24);
            this.butDelete.TabIndex = 1;
            this.butDelete.Click += new System.EventHandler(this.butDelete_Click);
            // 
            // DarkDataGridViewControls
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.butDelete);
            this.Controls.Add(this.butNew);
            this.Controls.Add(this.butUp);
            this.Controls.Add(this.butDown);
            this.Enabled = false;
            this.MinimumSize = new System.Drawing.Size(24, 24);
            this.Name = "DarkDataGridViewControls";
            this.Size = new System.Drawing.Size(24, 114);
            this.ResumeLayout(false);

        }

        #endregion
        private DarkUI.Controls.DarkButton butDown;
        private DarkUI.Controls.DarkButton butUp;
        private DarkUI.Controls.DarkButton butNew;
        private DarkUI.Controls.DarkButton butDelete;
    }
}
