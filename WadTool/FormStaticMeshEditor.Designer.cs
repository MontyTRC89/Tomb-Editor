namespace WadTool
{
    partial class FormStaticMeshEditor
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
            this.darkStatusStrip1 = new DarkUI.Controls.DarkStatusStrip();
            this.panelRendering = new WadTool.Controls.PanelRenderingStaticMeshEditor();
            this.darkLabel1 = new DarkUI.Controls.DarkLabel();
            this.butSelectMesh = new DarkUI.Controls.DarkButton();
            this.butSaveChanges = new DarkUI.Controls.DarkButton();
            this.butImportMesh = new DarkUI.Controls.DarkButton();
            this.SuspendLayout();
            // 
            // darkStatusStrip1
            // 
            this.darkStatusStrip1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.darkStatusStrip1.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.darkStatusStrip1.Location = new System.Drawing.Point(0, 540);
            this.darkStatusStrip1.Name = "darkStatusStrip1";
            this.darkStatusStrip1.Padding = new System.Windows.Forms.Padding(0, 5, 0, 3);
            this.darkStatusStrip1.Size = new System.Drawing.Size(829, 24);
            this.darkStatusStrip1.TabIndex = 0;
            this.darkStatusStrip1.Text = "darkStatusStrip1";
            // 
            // panelRendering
            // 
            this.panelRendering.Camera = null;
            this.panelRendering.Location = new System.Drawing.Point(13, 13);
            this.panelRendering.Name = "panelRendering";
            this.panelRendering.Size = new System.Drawing.Size(564, 513);
            this.panelRendering.StaticMesh = null;
            this.panelRendering.StaticPosition = new SharpDX.Vector3(0F, 0F, 0F);
            this.panelRendering.StaticRotation = new SharpDX.Vector3(0F, 0F, 0F);
            this.panelRendering.StaticScale = 1F;
            this.panelRendering.TabIndex = 1;
            // 
            // darkLabel1
            // 
            this.darkLabel1.AutoSize = true;
            this.darkLabel1.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.darkLabel1.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.darkLabel1.Location = new System.Drawing.Point(584, 13);
            this.darkLabel1.Name = "darkLabel1";
            this.darkLabel1.Size = new System.Drawing.Size(36, 13);
            this.darkLabel1.TabIndex = 48;
            this.darkLabel1.Text = "Mesh";
            // 
            // butSelectMesh
            // 
            this.butSelectMesh.Image = global::WadTool.Properties.Resources._3DView_16;
            this.butSelectMesh.Location = new System.Drawing.Point(587, 29);
            this.butSelectMesh.Name = "butSelectMesh";
            this.butSelectMesh.Padding = new System.Windows.Forms.Padding(5);
            this.butSelectMesh.Size = new System.Drawing.Size(112, 23);
            this.butSelectMesh.TabIndex = 47;
            this.butSelectMesh.Text = "Select mesh";
            this.butSelectMesh.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            // 
            // butSaveChanges
            // 
            this.butSaveChanges.Image = global::WadTool.Properties.Resources.save_16;
            this.butSaveChanges.Location = new System.Drawing.Point(705, 503);
            this.butSaveChanges.Name = "butSaveChanges";
            this.butSaveChanges.Padding = new System.Windows.Forms.Padding(5);
            this.butSaveChanges.Size = new System.Drawing.Size(112, 23);
            this.butSaveChanges.TabIndex = 46;
            this.butSaveChanges.Text = "Save changes";
            this.butSaveChanges.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.butSaveChanges.Click += new System.EventHandler(this.butSaveChanges_Click);
            // 
            // butImportMesh
            // 
            this.butImportMesh.Image = global::WadTool.Properties.Resources.opened_folder_16;
            this.butImportMesh.Location = new System.Drawing.Point(705, 29);
            this.butImportMesh.Name = "butImportMesh";
            this.butImportMesh.Padding = new System.Windows.Forms.Padding(5);
            this.butImportMesh.Size = new System.Drawing.Size(112, 23);
            this.butImportMesh.TabIndex = 49;
            this.butImportMesh.Text = "Import mesh";
            this.butImportMesh.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            // 
            // FormStaticMeshEditor
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(829, 564);
            this.Controls.Add(this.butImportMesh);
            this.Controls.Add(this.darkLabel1);
            this.Controls.Add(this.butSelectMesh);
            this.Controls.Add(this.butSaveChanges);
            this.Controls.Add(this.panelRendering);
            this.Controls.Add(this.darkStatusStrip1);
            this.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FormStaticMeshEditor";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Static mesh editor";
            this.Load += new System.EventHandler(this.FormStaticMeshEditor_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private DarkUI.Controls.DarkStatusStrip darkStatusStrip1;
        private Controls.PanelRenderingStaticMeshEditor panelRendering;
        private DarkUI.Controls.DarkButton butSaveChanges;
        private DarkUI.Controls.DarkButton butSelectMesh;
        private DarkUI.Controls.DarkLabel darkLabel1;
        private DarkUI.Controls.DarkButton butImportMesh;
    }
}