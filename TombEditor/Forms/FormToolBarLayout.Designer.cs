namespace TombEditor.Forms
{
    partial class FormToolBarLayout
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
            this.components = new System.ComponentModel.Container();
            this.dgvSource = new DarkUI.Controls.DarkDataGridView();
            this.ColumnSourceButton = new DarkUI.Controls.DarkDataGridViewButtonColumn();
            this.ColumnSourceName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.butToSrc = new DarkUI.Controls.DarkButton();
            this.darkSectionPanel1 = new DarkUI.Controls.DarkSectionPanel();
            this.darkSectionPanel2 = new DarkUI.Controls.DarkSectionPanel();
            this.dgvDest = new DarkUI.Controls.DarkDataGridView();
            this.ColumnDestButton = new DarkUI.Controls.DarkDataGridViewButtonColumn();
            this.ColumnDestText = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.butToDest = new DarkUI.Controls.DarkButton();
            this.butApply = new DarkUI.Controls.DarkButton();
            this.butOk = new DarkUI.Controls.DarkButton();
            this.butCancel = new DarkUI.Controls.DarkButton();
            this.butDefaults = new DarkUI.Controls.DarkButton();
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            ((System.ComponentModel.ISupportInitialize)(this.dgvSource)).BeginInit();
            this.darkSectionPanel1.SuspendLayout();
            this.darkSectionPanel2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvDest)).BeginInit();
            this.SuspendLayout();
            // 
            // dgvSource
            // 
            this.dgvSource.AllowUserToAddRows = false;
            this.dgvSource.AllowUserToDeleteRows = false;
            this.dgvSource.AllowUserToPasteCells = false;
            this.dgvSource.AllowUserToResizeColumns = false;
            this.dgvSource.ColumnHeadersHeight = 23;
            this.dgvSource.ColumnHeadersVisible = false;
            this.dgvSource.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.ColumnSourceButton,
            this.ColumnSourceName});
            this.dgvSource.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgvSource.Location = new System.Drawing.Point(1, 25);
            this.dgvSource.MultiSelect = false;
            this.dgvSource.Name = "dgvSource";
            this.dgvSource.RowHeadersWidth = 41;
            this.dgvSource.Size = new System.Drawing.Size(198, 312);
            this.dgvSource.TabIndex = 1;
            this.dgvSource.CellContentDoubleClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dgvSource_CellContentDoubleClick);
            this.dgvSource.CellPainting += new System.Windows.Forms.DataGridViewCellPaintingEventHandler(this.dgvSource_CellPainting);
            // 
            // ColumnSourceButton
            // 
            this.ColumnSourceButton.HeaderText = "";
            this.ColumnSourceButton.Name = "ColumnSourceButton";
            this.ColumnSourceButton.ReadOnly = true;
            this.ColumnSourceButton.Width = 30;
            // 
            // ColumnSourceName
            // 
            this.ColumnSourceName.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.ColumnSourceName.HeaderText = "";
            this.ColumnSourceName.Name = "ColumnSourceName";
            this.ColumnSourceName.ReadOnly = true;
            // 
            // butToSrc
            // 
            this.butToSrc.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.butToSrc.Checked = false;
            this.butToSrc.Image = global::TombEditor.Properties.Resources.general_angle_left_16;
            this.butToSrc.Location = new System.Drawing.Point(211, 6);
            this.butToSrc.Name = "butToSrc";
            this.butToSrc.Size = new System.Drawing.Size(30, 166);
            this.butToSrc.TabIndex = 2;
            this.toolTip1.SetToolTip(this.butToSrc, "Remove selected button from toolbar");
            this.butToSrc.Click += new System.EventHandler(this.butToSource_Click);
            // 
            // darkSectionPanel1
            // 
            this.darkSectionPanel1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.darkSectionPanel1.Controls.Add(this.dgvSource);
            this.darkSectionPanel1.Location = new System.Drawing.Point(5, 6);
            this.darkSectionPanel1.Name = "darkSectionPanel1";
            this.darkSectionPanel1.SectionHeader = "Available buttons";
            this.darkSectionPanel1.Size = new System.Drawing.Size(200, 338);
            this.darkSectionPanel1.TabIndex = 3;
            // 
            // darkSectionPanel2
            // 
            this.darkSectionPanel2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.darkSectionPanel2.Controls.Add(this.dgvDest);
            this.darkSectionPanel2.Location = new System.Drawing.Point(247, 6);
            this.darkSectionPanel2.Name = "darkSectionPanel2";
            this.darkSectionPanel2.SectionHeader = "Buttons in toolbar";
            this.darkSectionPanel2.Size = new System.Drawing.Size(200, 338);
            this.darkSectionPanel2.TabIndex = 4;
            // 
            // dgvDest
            // 
            this.dgvDest.AllowUserToAddRows = false;
            this.dgvDest.AllowUserToDeleteRows = false;
            this.dgvDest.AllowUserToPasteCells = false;
            this.dgvDest.AllowUserToResizeColumns = false;
            this.dgvDest.ColumnHeadersHeight = 23;
            this.dgvDest.ColumnHeadersVisible = false;
            this.dgvDest.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.ColumnDestButton,
            this.ColumnDestText});
            this.dgvDest.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgvDest.Location = new System.Drawing.Point(1, 25);
            this.dgvDest.MultiSelect = false;
            this.dgvDest.Name = "dgvDest";
            this.dgvDest.RowHeadersWidth = 41;
            this.dgvDest.Size = new System.Drawing.Size(198, 312);
            this.dgvDest.TabIndex = 2;
            this.dgvDest.CellContentDoubleClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dgvDest_CellContentDoubleClick);
            this.dgvDest.CellPainting += new System.Windows.Forms.DataGridViewCellPaintingEventHandler(this.dgvDest_CellPainting);
            // 
            // ColumnDestButton
            // 
            this.ColumnDestButton.HeaderText = "";
            this.ColumnDestButton.Name = "ColumnDestButton";
            this.ColumnDestButton.ReadOnly = true;
            this.ColumnDestButton.Width = 30;
            // 
            // ColumnDestText
            // 
            this.ColumnDestText.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.ColumnDestText.HeaderText = "";
            this.ColumnDestText.Name = "ColumnDestText";
            this.ColumnDestText.ReadOnly = true;
            // 
            // butToDest
            // 
            this.butToDest.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.butToDest.Checked = false;
            this.butToDest.Image = global::TombEditor.Properties.Resources.general_angle_right_16;
            this.butToDest.Location = new System.Drawing.Point(211, 178);
            this.butToDest.Name = "butToDest";
            this.butToDest.Size = new System.Drawing.Size(30, 166);
            this.butToDest.TabIndex = 5;
            this.toolTip1.SetToolTip(this.butToDest, "Add selected button to toolbar");
            this.butToDest.Click += new System.EventHandler(this.butToDest_Click);
            // 
            // butApply
            // 
            this.butApply.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.butApply.Checked = false;
            this.butApply.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.butApply.Location = new System.Drawing.Point(195, 350);
            this.butApply.Name = "butApply";
            this.butApply.Size = new System.Drawing.Size(80, 24);
            this.butApply.TabIndex = 6;
            this.butApply.Text = "Apply";
            this.butApply.Click += new System.EventHandler(this.butApply_Click);
            // 
            // butOk
            // 
            this.butOk.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.butOk.Checked = false;
            this.butOk.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.butOk.Location = new System.Drawing.Point(281, 350);
            this.butOk.Name = "butOk";
            this.butOk.Size = new System.Drawing.Size(80, 24);
            this.butOk.TabIndex = 7;
            this.butOk.Text = "OK";
            this.butOk.Click += new System.EventHandler(this.butOk_Click);
            // 
            // butCancel
            // 
            this.butCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.butCancel.Checked = false;
            this.butCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.butCancel.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.butCancel.Location = new System.Drawing.Point(367, 350);
            this.butCancel.Name = "butCancel";
            this.butCancel.Size = new System.Drawing.Size(80, 24);
            this.butCancel.TabIndex = 8;
            this.butCancel.Text = "Cancel";
            this.butCancel.Click += new System.EventHandler(this.butCancel_Click);
            // 
            // butDefaults
            // 
            this.butDefaults.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.butDefaults.Checked = false;
            this.butDefaults.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.butDefaults.Location = new System.Drawing.Point(6, 350);
            this.butDefaults.Name = "butDefaults";
            this.butDefaults.Size = new System.Drawing.Size(80, 24);
            this.butDefaults.TabIndex = 9;
            this.butDefaults.Text = "Defaults";
            this.butDefaults.Click += new System.EventHandler(this.butDefaults_Click);
            // 
            // FormToolBarLayout
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(453, 380);
            this.Controls.Add(this.butDefaults);
            this.Controls.Add(this.butApply);
            this.Controls.Add(this.butOk);
            this.Controls.Add(this.butCancel);
            this.Controls.Add(this.butToDest);
            this.Controls.Add(this.darkSectionPanel2);
            this.Controls.Add(this.darkSectionPanel1);
            this.Controls.Add(this.butToSrc);
            this.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.MaximumSize = new System.Drawing.Size(469, 2000);
            this.MinimumSize = new System.Drawing.Size(469, 418);
            this.Name = "FormToolBarLayout";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Toolbar Layout";
            ((System.ComponentModel.ISupportInitialize)(this.dgvSource)).EndInit();
            this.darkSectionPanel1.ResumeLayout(false);
            this.darkSectionPanel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgvDest)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion
        private DarkUI.Controls.DarkDataGridView dgvSource;
        private DarkUI.Controls.DarkButton butToSrc;
        private DarkUI.Controls.DarkSectionPanel darkSectionPanel1;
        private DarkUI.Controls.DarkSectionPanel darkSectionPanel2;
        private DarkUI.Controls.DarkButton butToDest;
        private DarkUI.Controls.DarkButton butApply;
        private DarkUI.Controls.DarkButton butOk;
        private DarkUI.Controls.DarkButton butCancel;
        private DarkUI.Controls.DarkButton butDefaults;
        private DarkUI.Controls.DarkDataGridViewButtonColumn ColumnSourceButton;
        private System.Windows.Forms.DataGridViewTextBoxColumn ColumnSourceName;
        private DarkUI.Controls.DarkDataGridView dgvDest;
        private DarkUI.Controls.DarkDataGridViewButtonColumn ColumnDestButton;
        private System.Windows.Forms.DataGridViewTextBoxColumn ColumnDestText;
        private System.Windows.Forms.ToolTip toolTip1;
    }
}