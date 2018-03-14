namespace WadTool
{
    partial class FormMesh
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
            this.lstMeshes = new DarkUI.Controls.DarkTreeView();
            this.panelMesh = new WadTool.Controls.PanelRenderingMesh();
            this.darkToolStrip1 = new DarkUI.Controls.DarkToolStrip();
            this.SuspendLayout();
            // 
            // lstMeshes
            // 
            this.lstMeshes.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.lstMeshes.Cursor = System.Windows.Forms.Cursors.Arrow;
            this.lstMeshes.Location = new System.Drawing.Point(13, 13);
            this.lstMeshes.MaxDragChange = 20;
            this.lstMeshes.Name = "lstMeshes";
            this.lstMeshes.Size = new System.Drawing.Size(261, 504);
            this.lstMeshes.TabIndex = 1;
            this.lstMeshes.Text = "darkTreeView1";
            this.lstMeshes.Click += new System.EventHandler(this.lstMeshes_Click);
            // 
            // panelMesh
            // 
            this.panelMesh.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panelMesh.Location = new System.Drawing.Point(280, 44);
            this.panelMesh.Name = "panelMesh";
            this.panelMesh.Size = new System.Drawing.Size(532, 473);
            this.panelMesh.TabIndex = 0;
            // 
            // darkToolStrip1
            // 
            this.darkToolStrip1.AutoSize = false;
            this.darkToolStrip1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.darkToolStrip1.Dock = System.Windows.Forms.DockStyle.None;
            this.darkToolStrip1.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.darkToolStrip1.Location = new System.Drawing.Point(280, 13);
            this.darkToolStrip1.Name = "darkToolStrip1";
            this.darkToolStrip1.Padding = new System.Windows.Forms.Padding(5, 0, 1, 0);
            this.darkToolStrip1.Size = new System.Drawing.Size(532, 28);
            this.darkToolStrip1.TabIndex = 2;
            this.darkToolStrip1.Text = "darkToolStrip1";
            // 
            // FormMesh
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(824, 575);
            this.Controls.Add(this.darkToolStrip1);
            this.Controls.Add(this.lstMeshes);
            this.Controls.Add(this.panelMesh);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
            this.Name = "FormMesh";
            this.StartPosition = System.Windows.Forms.FormStartPosition.WindowsDefaultBounds;
            this.Text = "Meshes";
            this.ResumeLayout(false);

        }

        #endregion

        private Controls.PanelRenderingMesh panelMesh;
        private DarkUI.Controls.DarkTreeView lstMeshes;
        private DarkUI.Controls.DarkToolStrip darkToolStrip1;
    }
}