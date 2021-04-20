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
            this.butCancel = new DarkUI.Controls.DarkButton();
            this.butOk = new DarkUI.Controls.DarkButton();
            this.darkLabel1 = new DarkUI.Controls.DarkLabel();
            this.darkLabel2 = new DarkUI.Controls.DarkLabel();
            this.butAssign = new DarkUI.Controls.DarkButton();
            this.importedGeometryLabel = new DarkUI.Controls.DarkLabel();
            this.comboLightingModel = new DarkUI.Controls.DarkComboBox();
            this.darkLabel4 = new DarkUI.Controls.DarkLabel();
            this.darkGroupBox1 = new DarkUI.Controls.DarkGroupBox();
            this.panelColor = new DarkUI.Controls.DarkPanel();
            this.cbHide = new DarkUI.Controls.DarkCheckBox();
            this.cbSharpEdges = new DarkUI.Controls.DarkCheckBox();
            this.importedGeometryManager = new TombEditor.Controls.ImportedGeometryManager();
            this.darkGroupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // butCancel
            // 
            this.butCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.butCancel.Checked = false;
            this.butCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.butCancel.Location = new System.Drawing.Point(658, 444);
            this.butCancel.Name = "butCancel";
            this.butCancel.Size = new System.Drawing.Size(80, 23);
            this.butCancel.TabIndex = 5;
            this.butCancel.Text = "Cancel";
            this.butCancel.Click += new System.EventHandler(this.butCancel_Click);
            // 
            // butOk
            // 
            this.butOk.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.butOk.Checked = false;
            this.butOk.Location = new System.Drawing.Point(572, 444);
            this.butOk.Name = "butOk";
            this.butOk.Size = new System.Drawing.Size(80, 23);
            this.butOk.TabIndex = 4;
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
            this.darkLabel1.Size = new System.Drawing.Size(737, 30);
            this.darkLabel1.TabIndex = 2;
            this.darkLabel1.Text = "Select a model from the following list or add a new one by pressing plus button.\r" +
    "\nDouble-click on entry or press \'Assign\' to assign the loaded file to this impor" +
    "ted geometry object.";
            // 
            // darkLabel2
            // 
            this.darkLabel2.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.darkLabel2.Location = new System.Drawing.Point(6, 10);
            this.darkLabel2.Name = "darkLabel2";
            this.darkLabel2.Size = new System.Drawing.Size(95, 17);
            this.darkLabel2.TabIndex = 2;
            this.darkLabel2.Text = "Chosen entry:";
            this.darkLabel2.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // butAssign
            // 
            this.butAssign.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.butAssign.Checked = false;
            this.butAssign.Location = new System.Drawing.Point(646, 7);
            this.butAssign.Name = "butAssign";
            this.butAssign.Size = new System.Drawing.Size(80, 23);
            this.butAssign.TabIndex = 1;
            this.butAssign.Text = "Assign";
            this.butAssign.Click += new System.EventHandler(this.butAssign_Click);
            // 
            // importedGeometryLabel
            // 
            this.importedGeometryLabel.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.importedGeometryLabel.Location = new System.Drawing.Point(125, 10);
            this.importedGeometryLabel.Name = "importedGeometryLabel";
            this.importedGeometryLabel.Size = new System.Drawing.Size(511, 17);
            this.importedGeometryLabel.TabIndex = 2;
            this.importedGeometryLabel.Text = "None";
            this.importedGeometryLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // comboLightingModel
            // 
            this.comboLightingModel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.comboLightingModel.FormattingEnabled = true;
            this.comboLightingModel.Items.AddRange(new object[] {
            "No lights",
            "Vertex colors",
            "Calculate from lights in room",
            "Specified tint only"});
            this.comboLightingModel.Location = new System.Drawing.Point(128, 36);
            this.comboLightingModel.Name = "comboLightingModel";
            this.comboLightingModel.Size = new System.Drawing.Size(309, 23);
            this.comboLightingModel.TabIndex = 3;
            // 
            // darkLabel4
            // 
            this.darkLabel4.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.darkLabel4.Location = new System.Drawing.Point(6, 37);
            this.darkLabel4.Name = "darkLabel4";
            this.darkLabel4.Size = new System.Drawing.Size(113, 17);
            this.darkLabel4.TabIndex = 6;
            this.darkLabel4.Text = "Lighting model:";
            this.darkLabel4.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // darkGroupBox1
            // 
            this.darkGroupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.darkGroupBox1.Controls.Add(this.panelColor);
            this.darkGroupBox1.Controls.Add(this.cbHide);
            this.darkGroupBox1.Controls.Add(this.cbSharpEdges);
            this.darkGroupBox1.Controls.Add(this.butAssign);
            this.darkGroupBox1.Controls.Add(this.comboLightingModel);
            this.darkGroupBox1.Controls.Add(this.darkLabel4);
            this.darkGroupBox1.Controls.Add(this.darkLabel2);
            this.darkGroupBox1.Controls.Add(this.importedGeometryLabel);
            this.darkGroupBox1.Location = new System.Drawing.Point(6, 373);
            this.darkGroupBox1.Name = "darkGroupBox1";
            this.darkGroupBox1.Size = new System.Drawing.Size(732, 65);
            this.darkGroupBox1.TabIndex = 7;
            this.darkGroupBox1.TabStop = false;
            // 
            // panelColor
            // 
            this.panelColor.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.panelColor.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panelColor.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.panelColor.Location = new System.Drawing.Point(443, 36);
            this.panelColor.Name = "panelColor";
            this.panelColor.Size = new System.Drawing.Size(59, 23);
            this.panelColor.TabIndex = 13;
            this.panelColor.Tag = "EditAmbientLight";
            this.panelColor.Click += new System.EventHandler(this.panelColor_Click);
            // 
            // cbHide
            // 
            this.cbHide.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.cbHide.AutoSize = true;
            this.cbHide.Location = new System.Drawing.Point(629, 38);
            this.cbHide.Name = "cbHide";
            this.cbHide.Size = new System.Drawing.Size(97, 17);
            this.cbHide.TabIndex = 8;
            this.cbHide.Text = "Hide in editor";
            // 
            // cbSharpEdges
            // 
            this.cbSharpEdges.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.cbSharpEdges.AutoSize = true;
            this.cbSharpEdges.Location = new System.Drawing.Point(508, 38);
            this.cbSharpEdges.Name = "cbSharpEdges";
            this.cbSharpEdges.Size = new System.Drawing.Size(120, 17);
            this.cbSharpEdges.TabIndex = 7;
            this.cbSharpEdges.Text = "Force sharp edges";
            // 
            // importedGeometryManager
            // 
            this.importedGeometryManager.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.importedGeometryManager.LevelSettings = null;
            this.importedGeometryManager.Location = new System.Drawing.Point(6, 40);
            this.importedGeometryManager.Name = "importedGeometryManager";
            this.importedGeometryManager.SelectedImportedGeometry = null;
            this.importedGeometryManager.Size = new System.Drawing.Size(735, 328);
            this.importedGeometryManager.TabIndex = 0;
            this.importedGeometryManager.MouseDoubleClick += new System.EventHandler<System.Windows.Forms.MouseEventArgs>(this.importedGeometryManager_MouseDoubleClick);
            // 
            // FormImportedGeometry
            // 
            this.AcceptButton = this.butOk;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.butCancel;
            this.ClientSize = new System.Drawing.Size(745, 474);
            this.Controls.Add(this.darkGroupBox1);
            this.Controls.Add(this.butCancel);
            this.Controls.Add(this.butOk);
            this.Controls.Add(this.darkLabel1);
            this.Controls.Add(this.importedGeometryManager);
            this.MinimizeBox = false;
            this.Name = "FormImportedGeometry";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Imported Geometry Settings";
            this.darkGroupBox1.ResumeLayout(false);
            this.darkGroupBox1.PerformLayout();
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
        private DarkUI.Controls.DarkComboBox comboLightingModel;
        private DarkUI.Controls.DarkLabel darkLabel4;
        private DarkUI.Controls.DarkGroupBox darkGroupBox1;
        private DarkUI.Controls.DarkCheckBox cbSharpEdges;
        private DarkUI.Controls.DarkCheckBox cbHide;
        private DarkUI.Controls.DarkPanel panelColor;
    }
}