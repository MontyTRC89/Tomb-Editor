namespace TombEditor
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
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.tableLayoutPanel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // importedGeometryManager
            // 
            this.importedGeometryManager.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.importedGeometryManager.LevelSettings = null;
            this.importedGeometryManager.Location = new System.Drawing.Point(23, 47);
            this.importedGeometryManager.Name = "importedGeometryManager";
            this.importedGeometryManager.SelectedImportedGeometry = null;
            this.importedGeometryManager.Size = new System.Drawing.Size(686, 378);
            this.importedGeometryManager.TabIndex = 0;
            // 
            // butCancel
            // 
            this.butCancel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.butCancel.Location = new System.Drawing.Point(359, 3);
            this.butCancel.Name = "butCancel";
            this.butCancel.Padding = new System.Windows.Forms.Padding(5);
            this.butCancel.Size = new System.Drawing.Size(351, 24);
            this.butCancel.TabIndex = 1;
            this.butCancel.Text = "Cancel";
            this.butCancel.Click += new System.EventHandler(this.butCancel_Click);
            // 
            // butOk
            // 
            this.butOk.Dock = System.Windows.Forms.DockStyle.Fill;
            this.butOk.Location = new System.Drawing.Point(3, 3);
            this.butOk.Name = "butOk";
            this.butOk.Padding = new System.Windows.Forms.Padding(5);
            this.butOk.Size = new System.Drawing.Size(350, 24);
            this.butOk.TabIndex = 1;
            this.butOk.Text = "Ok";
            this.butOk.Click += new System.EventHandler(this.butOk_Click);
            // 
            // darkLabel1
            // 
            this.darkLabel1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.darkLabel1.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.darkLabel1.Location = new System.Drawing.Point(4, 9);
            this.darkLabel1.Name = "darkLabel1";
            this.darkLabel1.Size = new System.Drawing.Size(705, 35);
            this.darkLabel1.TabIndex = 2;
            this.darkLabel1.Text = "Select a model from the following list or add a new one by pressing \'Create from " +
    "file\'.\r\nPress \'Assign\' afterwards to assign the loaded file to this imported geo" +
    "metry object.";
            // 
            // darkLabel2
            // 
            this.darkLabel2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.darkLabel2.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.darkLabel2.Location = new System.Drawing.Point(221, 434);
            this.darkLabel2.Name = "darkLabel2";
            this.darkLabel2.Size = new System.Drawing.Size(144, 17);
            this.darkLabel2.TabIndex = 2;
            this.darkLabel2.Text = "Chosen imported geometry:";
            // 
            // butAssign
            // 
            this.butAssign.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.butAssign.Location = new System.Drawing.Point(23, 431);
            this.butAssign.Name = "butAssign";
            this.butAssign.Padding = new System.Windows.Forms.Padding(5);
            this.butAssign.Size = new System.Drawing.Size(185, 20);
            this.butAssign.TabIndex = 1;
            this.butAssign.Text = "Assign";
            this.butAssign.Click += new System.EventHandler(this.butAssign_Click);
            // 
            // importedGeometryLabel
            // 
            this.importedGeometryLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.importedGeometryLabel.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.importedGeometryLabel.Location = new System.Drawing.Point(371, 434);
            this.importedGeometryLabel.Name = "importedGeometryLabel";
            this.importedGeometryLabel.Size = new System.Drawing.Size(338, 17);
            this.importedGeometryLabel.TabIndex = 2;
            this.importedGeometryLabel.Text = "None ☹";
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 2;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.Controls.Add(this.butOk, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.butCancel, 1, 0);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 458);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 1;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(713, 30);
            this.tableLayoutPanel1.TabIndex = 3;
            // 
            // FormImportedGeometry
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(713, 488);
            this.Controls.Add(this.tableLayoutPanel1);
            this.Controls.Add(this.importedGeometryLabel);
            this.Controls.Add(this.darkLabel2);
            this.Controls.Add(this.darkLabel1);
            this.Controls.Add(this.butAssign);
            this.Controls.Add(this.importedGeometryManager);
            this.Name = "FormImportedGeometry";
            this.Text = "Imported Geometry Settings";
            this.tableLayoutPanel1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private Controls.ImportedGeometryManager importedGeometryManager;
        private DarkUI.Controls.DarkButton butCancel;
        private DarkUI.Controls.DarkButton butOk;
        private DarkUI.Controls.DarkLabel darkLabel1;
        private DarkUI.Controls.DarkLabel darkLabel2;
        private DarkUI.Controls.DarkButton butAssign;
        private DarkUI.Controls.DarkLabel importedGeometryLabel;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
    }
}