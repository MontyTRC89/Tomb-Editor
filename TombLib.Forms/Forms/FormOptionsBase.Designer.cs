namespace TombLib.Forms
{
    partial class FormOptionsBase
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
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.darkSectionPanel2 = new DarkUI.Controls.DarkSectionPanel();
            this.tabbedContainer = new TombLib.Controls.DarkTabbedContainer();
            this.darkSectionPanel1 = new DarkUI.Controls.DarkSectionPanel();
            this.optionsList = new DarkUI.Controls.DarkListView();
            this.panel5 = new DarkUI.Controls.DarkPanel();
            this.butApply = new DarkUI.Controls.DarkButton();
            this.butOk = new DarkUI.Controls.DarkButton();
            this.butCancel = new DarkUI.Controls.DarkButton();
            this.panel4 = new DarkUI.Controls.DarkPanel();
            this.butPageDefaults = new DarkUI.Controls.DarkButton();
            this.tableLayoutPanel1.SuspendLayout();
            this.darkSectionPanel2.SuspendLayout();
            this.darkSectionPanel1.SuspendLayout();
            this.panel5.SuspendLayout();
            this.panel4.SuspendLayout();
            this.SuspendLayout();
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 2;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Controls.Add(this.darkSectionPanel2, 1, 0);
            this.tableLayoutPanel1.Controls.Add(this.darkSectionPanel1, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.panel5, 1, 1);
            this.tableLayoutPanel1.Controls.Add(this.panel4, 0, 1);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(3, 0);
            this.tableLayoutPanel1.Margin = new System.Windows.Forms.Padding(5);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 2;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 34F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(608, 552);
            this.tableLayoutPanel1.TabIndex = 2;
            // 
            // darkSectionPanel2
            // 
            this.darkSectionPanel2.Controls.Add(this.tabbedContainer);
            this.darkSectionPanel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.darkSectionPanel2.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.darkSectionPanel2.Location = new System.Drawing.Point(209, 3);
            this.darkSectionPanel2.Name = "darkSectionPanel2";
            this.darkSectionPanel2.SectionHeader = null;
            this.darkSectionPanel2.Size = new System.Drawing.Size(396, 512);
            this.darkSectionPanel2.TabIndex = 2;
            // 
            // tabbedContainer
            // 
            this.tabbedContainer.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabbedContainer.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.tabbedContainer.Location = new System.Drawing.Point(1, 1);
            this.tabbedContainer.Name = "tabbedContainer";
            this.tabbedContainer.SelectedIndex = 0;
            this.tabbedContainer.Size = new System.Drawing.Size(394, 510);
            this.tabbedContainer.TabIndex = 2;
            // 
            // darkSectionPanel1
            // 
            this.darkSectionPanel1.Controls.Add(this.optionsList);
            this.darkSectionPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.darkSectionPanel1.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.darkSectionPanel1.Location = new System.Drawing.Point(3, 3);
            this.darkSectionPanel1.Name = "darkSectionPanel1";
            this.darkSectionPanel1.SectionHeader = null;
            this.darkSectionPanel1.Size = new System.Drawing.Size(200, 512);
            this.darkSectionPanel1.TabIndex = 7;
            // 
            // optionsList
            // 
            this.optionsList.Dock = System.Windows.Forms.DockStyle.Fill;
            this.optionsList.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.optionsList.Location = new System.Drawing.Point(1, 1);
            this.optionsList.Name = "optionsList";
            this.optionsList.Size = new System.Drawing.Size(198, 510);
            this.optionsList.TabIndex = 6;
            // 
            // panel5
            // 
            this.panel5.Controls.Add(this.butApply);
            this.panel5.Controls.Add(this.butOk);
            this.panel5.Controls.Add(this.butCancel);
            this.panel5.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel5.Location = new System.Drawing.Point(209, 521);
            this.panel5.Name = "panel5";
            this.panel5.Padding = new System.Windows.Forms.Padding(0, 0, 4, 0);
            this.panel5.Size = new System.Drawing.Size(396, 28);
            this.panel5.TabIndex = 5;
            // 
            // butApply
            // 
            this.butApply.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.butApply.Checked = false;
            this.butApply.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.butApply.Location = new System.Drawing.Point(139, 1);
            this.butApply.Name = "butApply";
            this.butApply.Size = new System.Drawing.Size(80, 24);
            this.butApply.TabIndex = 3;
            this.butApply.Text = "Apply";
            this.butApply.Click += new System.EventHandler(this.butApply_Click);
            // 
            // butOk
            // 
            this.butOk.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.butOk.Checked = false;
            this.butOk.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.butOk.Location = new System.Drawing.Point(225, 1);
            this.butOk.Name = "butOk";
            this.butOk.Size = new System.Drawing.Size(80, 24);
            this.butOk.TabIndex = 3;
            this.butOk.Text = "OK";
            this.butOk.Click += new System.EventHandler(this.butOk_Click);
            // 
            // butCancel
            // 
            this.butCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.butCancel.Checked = false;
            this.butCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.butCancel.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.butCancel.Location = new System.Drawing.Point(311, 1);
            this.butCancel.Name = "butCancel";
            this.butCancel.Size = new System.Drawing.Size(80, 24);
            this.butCancel.TabIndex = 3;
            this.butCancel.Text = "Cancel";
            // 
            // panel4
            // 
            this.panel4.Controls.Add(this.butPageDefaults);
            this.panel4.Location = new System.Drawing.Point(3, 521);
            this.panel4.Name = "panel4";
            this.panel4.Size = new System.Drawing.Size(200, 28);
            this.panel4.TabIndex = 8;
            // 
            // butPageDefaults
            // 
            this.butPageDefaults.Checked = false;
            this.butPageDefaults.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.butPageDefaults.Location = new System.Drawing.Point(1, 1);
            this.butPageDefaults.Name = "butPageDefaults";
            this.butPageDefaults.Size = new System.Drawing.Size(199, 24);
            this.butPageDefaults.TabIndex = 4;
            this.butPageDefaults.Text = "Set page to default";
            this.butPageDefaults.Click += new System.EventHandler(this.butPageDefaults_Click);
            // 
            // FormOptionsBase
            // 
            this.AcceptButton = this.butOk;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.butCancel;
            this.ClientSize = new System.Drawing.Size(614, 555);
            this.Controls.Add(this.tableLayoutPanel1);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FormOptionsBase";
            this.Padding = new System.Windows.Forms.Padding(3, 0, 3, 3);
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Options";
            this.tableLayoutPanel1.ResumeLayout(false);
            this.darkSectionPanel2.ResumeLayout(false);
            this.darkSectionPanel1.ResumeLayout(false);
            this.panel5.ResumeLayout(false);
            this.panel4.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private DarkUI.Controls.DarkSectionPanel darkSectionPanel2;
        private DarkUI.Controls.DarkSectionPanel darkSectionPanel1;
        private DarkUI.Controls.DarkListView optionsList;
        private DarkUI.Controls.DarkButton butApply;
        private DarkUI.Controls.DarkButton butOk;
        private DarkUI.Controls.DarkButton butCancel;
        private DarkUI.Controls.DarkPanel panel5;
        private DarkUI.Controls.DarkPanel panel4;
        private DarkUI.Controls.DarkButton butPageDefaults;
        protected TombLib.Controls.DarkTabbedContainer tabbedContainer;
    }
}