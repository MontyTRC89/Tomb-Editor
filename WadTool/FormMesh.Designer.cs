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
            this.btCancel = new DarkUI.Controls.DarkButton();
            this.btOk = new DarkUI.Controls.DarkButton();
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
            this.lstMeshes.Size = new System.Drawing.Size(320, 504);
            this.lstMeshes.TabIndex = 1;
            this.lstMeshes.Text = "darkTreeView1";
            this.lstMeshes.Click += new System.EventHandler(this.lstMeshes_Click);
            // 
            // panelMesh
            // 
            this.panelMesh.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panelMesh.Location = new System.Drawing.Point(339, 13);
            this.panelMesh.Name = "panelMesh";
            this.panelMesh.Size = new System.Drawing.Size(473, 504);
            this.panelMesh.TabIndex = 0;
            // 
            // btCancel
            // 
            this.btCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btCancel.Location = new System.Drawing.Point(580, 537);
            this.btCancel.Name = "btCancel";
            this.btCancel.Size = new System.Drawing.Size(113, 26);
            this.btCancel.TabIndex = 52;
            this.btCancel.Text = "Cancel";
            this.btCancel.Click += new System.EventHandler(this.btCancel_Click);
            // 
            // btOk
            // 
            this.btOk.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btOk.Location = new System.Drawing.Point(699, 537);
            this.btOk.Name = "btOk";
            this.btOk.Size = new System.Drawing.Size(113, 26);
            this.btOk.TabIndex = 53;
            this.btOk.Text = "Ok";
            this.btOk.Click += new System.EventHandler(this.btOk_Click);
            // 
            // FormMesh
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(824, 575);
            this.Controls.Add(this.btCancel);
            this.Controls.Add(this.lstMeshes);
            this.Controls.Add(this.btOk);
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
        private DarkUI.Controls.DarkButton btCancel;
        private DarkUI.Controls.DarkButton btOk;
    }
}