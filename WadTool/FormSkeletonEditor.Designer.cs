namespace WadTool
{
    partial class FormSkeletonEditor
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

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.treeSkeleton = new DarkUI.Controls.DarkTreeView();
            this.panelRendering = new WadTool.Controls.PanelRenderingSkeleton();
            this.SuspendLayout();
            // 
            // treeSkeleton
            // 
            this.treeSkeleton.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.treeSkeleton.Location = new System.Drawing.Point(13, 13);
            this.treeSkeleton.MaxDragChange = 20;
            this.treeSkeleton.Name = "treeSkeleton";
            this.treeSkeleton.Size = new System.Drawing.Size(271, 678);
            this.treeSkeleton.TabIndex = 0;
            this.treeSkeleton.Text = "darkTreeView1";
            this.treeSkeleton.Click += new System.EventHandler(this.treeSkeleton_Click);
            // 
            // panelRendering
            // 
            this.panelRendering.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panelRendering.Location = new System.Drawing.Point(290, 13);
            this.panelRendering.Name = "panelRendering";
            this.panelRendering.SelectedNode = null;
            this.panelRendering.Size = new System.Drawing.Size(706, 678);
            this.panelRendering.Skeleton = null;
            this.panelRendering.StaticScale = 1F;
            this.panelRendering.TabIndex = 1;
            // 
            // FormSkeletonEditor
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1008, 729);
            this.Controls.Add(this.panelRendering);
            this.Controls.Add(this.treeSkeleton);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
            this.MinimumSize = new System.Drawing.Size(1024, 768);
            this.Name = "FormSkeletonEditor";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Skeleton editor";
            this.ResumeLayout(false);

        }

        #endregion

        private DarkUI.Controls.DarkTreeView treeSkeleton;
        private Controls.PanelRenderingSkeleton panelRendering;
    }
}