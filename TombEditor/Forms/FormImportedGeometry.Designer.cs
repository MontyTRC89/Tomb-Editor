namespace TombEditor.Forms
{
    partial class FormImportedGeometry
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
            this.importedGeometryManager = new TombEditor.Controls.ImportedGeometryManager();
            this.butCancel = new DarkUI.Controls.DarkButton();
            this.butOk = new DarkUI.Controls.DarkButton();
            this.darkLabel1 = new DarkUI.Controls.DarkLabel();
            this.darkLabel2 = new DarkUI.Controls.DarkLabel();
            this.butAssign = new DarkUI.Controls.DarkButton();
            this.importedGeometryLabel = new DarkUI.Controls.DarkLabel();
            this.darkLabel3 = new DarkUI.Controls.DarkLabel();
            this.tbMeshFilter = new DarkUI.Controls.DarkTextBox();
            this.SuspendLayout();
            // 
            // importedGeometryManager
            // 
            this.importedGeometryManager.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.importedGeometryManager.LevelSettings = null;
            this.importedGeometryManager.Location = new System.Drawing.Point(6, 42);
            this.importedGeometryManager.Name = "importedGeometryManager";
            this.importedGeometryManager.SelectedImportedGeometry = null;
            this.importedGeometryManager.Size = new System.Drawing.Size(730, 306);
            this.importedGeometryManager.TabIndex = 0;
            // 
            // butCancel
            // 
            this.butCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.butCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.butCancel.Location = new System.Drawing.Point(653, 420);
            this.butCancel.Name = "butCancel";
            this.butCancel.Size = new System.Drawing.Size(80, 23);
            this.butCancel.TabIndex = 1;
            this.butCancel.Text = "Cancel";
            this.butCancel.Click += new System.EventHandler(this.butCancel_Click);
            // 
            // butOk
            // 
            this.butOk.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.butOk.Location = new System.Drawing.Point(567, 420);
            this.butOk.Name = "butOk";
            this.butOk.Size = new System.Drawing.Size(80, 23);
            this.butOk.TabIndex = 1;
            this.butOk.Text = "OK";
            this.butOk.Click += new System.EventHandler(this.butOk_Click);
            // 
            // darkLabel1
            // 
            this.darkLabel1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.darkLabel1.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.darkLabel1.Location = new System.Drawing.Point(4, 9);
            this.darkLabel1.Name = "darkLabel1";
            this.darkLabel1.Size = new System.Drawing.Size(732, 35);
            this.darkLabel1.TabIndex = 2;
            this.darkLabel1.Text = "Select a model from the following list or add a new one by pressing \'Create from " +
    "file\'.\r\nPress \'Assign\' afterwards to assign the loaded file to this imported geo" +
    "metry object.";
            // 
            // darkLabel2
            // 
            this.darkLabel2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.darkLabel2.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.darkLabel2.Location = new System.Drawing.Point(4, 360);
            this.darkLabel2.Name = "darkLabel2";
            this.darkLabel2.Size = new System.Drawing.Size(157, 17);
            this.darkLabel2.TabIndex = 2;
            this.darkLabel2.Text = "Chosen imported geometry:";
            this.darkLabel2.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // butAssign
            // 
            this.butAssign.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.butAssign.Location = new System.Drawing.Point(653, 357);
            this.butAssign.Name = "butAssign";
            this.butAssign.Size = new System.Drawing.Size(80, 23);
            this.butAssign.TabIndex = 1;
            this.butAssign.Text = "Assign";
            this.butAssign.Click += new System.EventHandler(this.butAssign_Click);
            // 
            // importedGeometryLabel
            // 
            this.importedGeometryLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.importedGeometryLabel.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.importedGeometryLabel.Location = new System.Drawing.Point(156, 360);
            this.importedGeometryLabel.Name = "importedGeometryLabel";
            this.importedGeometryLabel.Size = new System.Drawing.Size(491, 17);
            this.importedGeometryLabel.TabIndex = 2;
            this.importedGeometryLabel.Text = "None ☹";
            this.importedGeometryLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // darkLabel3
            // 
            this.darkLabel3.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.darkLabel3.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.darkLabel3.Location = new System.Drawing.Point(4, 386);
            this.darkLabel3.Name = "darkLabel3";
            this.darkLabel3.Size = new System.Drawing.Size(319, 17);
            this.darkLabel3.TabIndex = 2;
            this.darkLabel3.Text = "Mesh filter (only meshes that contain this will be shown):";
            this.darkLabel3.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // tbMeshFilter
            // 
            this.tbMeshFilter.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tbMeshFilter.Location = new System.Drawing.Point(309, 386);
            this.tbMeshFilter.Name = "tbMeshFilter";
            this.tbMeshFilter.Size = new System.Drawing.Size(424, 22);
            this.tbMeshFilter.TabIndex = 4;
            // 
            // FormImportedGeometry
            // 
            this.AcceptButton = this.butOk;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.butCancel;
            this.ClientSize = new System.Drawing.Size(740, 450);
            this.Controls.Add(this.butCancel);
            this.Controls.Add(this.butOk);
            this.Controls.Add(this.tbMeshFilter);
            this.Controls.Add(this.darkLabel3);
            this.Controls.Add(this.importedGeometryLabel);
            this.Controls.Add(this.darkLabel2);
            this.Controls.Add(this.darkLabel1);
            this.Controls.Add(this.butAssign);
            this.Controls.Add(this.importedGeometryManager);
            this.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.MinimizeBox = false;
            this.Name = "FormImportedGeometry";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Imported Geometry Settings";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private Controls.ImportedGeometryManager importedGeometryManager;
        private DarkUI.Controls.DarkButton butCancel;
        private DarkUI.Controls.DarkButton butOk;
        private DarkUI.Controls.DarkLabel darkLabel1;
        private DarkUI.Controls.DarkLabel darkLabel2;
        private DarkUI.Controls.DarkButton butAssign;
        private DarkUI.Controls.DarkLabel importedGeometryLabel;
        private DarkUI.Controls.DarkLabel darkLabel3;
        private DarkUI.Controls.DarkTextBox tbMeshFilter;
    }
}