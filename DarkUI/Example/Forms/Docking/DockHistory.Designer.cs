﻿using DarkUI.Controls;

namespace Example.Forms.Docking
{
    partial class DockHistory
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
            this.lstHistory = new DarkUI.Controls.DarkListView();
            this.SuspendLayout();
            // 
            // lstHistory
            // 
            this.lstHistory.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lstHistory.Location = new System.Drawing.Point(0, 25);
            this.lstHistory.Name = "lstHistory";
            this.lstHistory.Size = new System.Drawing.Size(200, 175);
            this.lstHistory.TabIndex = 0;
            this.lstHistory.Text = "darkListView1";
            // 
            // DockHistory
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.lstHistory);
            this.DefaultDockArea = DarkUI.Docking.DarkDockArea.Right;
            this.DockText = "History";
            this.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Icon = global::Example.Icons.RefactoringLog_12810;
            this.MinimumSize = new System.Drawing.Size(100, 100);
            this.Name = "DockHistory";
            this.SerializationKey = "DockHistory";
            this.Size = new System.Drawing.Size(200, 200);
            this.ResumeLayout(false);

        }

        #endregion

        private DarkListView lstHistory;
    }
}
