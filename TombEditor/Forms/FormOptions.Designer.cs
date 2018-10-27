namespace TombEditor.Forms
{
    partial class FormOptions
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
            this.tabbedContainer = new TombEditor.Controls.DarkTabbedContainer();
            this.tabPage8 = new System.Windows.Forms.TabPage();
            this.darkSectionPanel1 = new DarkUI.Controls.DarkSectionPanel();
            this.optionsList = new DarkUI.Controls.DarkListView();
            this.panel11 = new System.Windows.Forms.Panel();
            this.butApply = new DarkUI.Controls.DarkButton();
            this.butOk = new DarkUI.Controls.DarkButton();
            this.butCancel = new DarkUI.Controls.DarkButton();
            this.tableLayoutPanel1.SuspendLayout();
            this.darkSectionPanel2.SuspendLayout();
            this.tabbedContainer.SuspendLayout();
            this.darkSectionPanel1.SuspendLayout();
            this.panel11.SuspendLayout();
            this.SuspendLayout();
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 2;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel1.Controls.Add(this.darkSectionPanel2, 1, 0);
            this.tableLayoutPanel1.Controls.Add(this.darkSectionPanel1, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.panel11, 1, 1);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Margin = new System.Windows.Forms.Padding(5);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 2;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 34F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(816, 356);
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
            this.darkSectionPanel2.Size = new System.Drawing.Size(604, 316);
            this.darkSectionPanel2.TabIndex = 2;
            // 
            // tabbedContainer
            // 
            this.tabbedContainer.Controls.Add(this.tabPage8);
            this.tabbedContainer.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabbedContainer.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.tabbedContainer.Location = new System.Drawing.Point(1, 1);
            this.tabbedContainer.Name = "tabbedContainer";
            this.tabbedContainer.SelectedIndex = 0;
            this.tabbedContainer.Size = new System.Drawing.Size(602, 314);
            this.tabbedContainer.TabIndex = 2;
            // 
            // tabPage8
            // 
            this.tabPage8.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.tabPage8.Location = new System.Drawing.Point(4, 22);
            this.tabPage8.Name = "tabPage8";
            this.tabPage8.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage8.Size = new System.Drawing.Size(594, 288);
            this.tabPage8.TabIndex = 7;
            this.tabPage8.Text = "General";
            // 
            // darkSectionPanel1
            // 
            this.darkSectionPanel1.Controls.Add(this.optionsList);
            this.darkSectionPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.darkSectionPanel1.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.darkSectionPanel1.Location = new System.Drawing.Point(3, 3);
            this.darkSectionPanel1.Name = "darkSectionPanel1";
            this.darkSectionPanel1.SectionHeader = null;
            this.darkSectionPanel1.Size = new System.Drawing.Size(200, 316);
            this.darkSectionPanel1.TabIndex = 7;
            // 
            // optionsList
            // 
            this.optionsList.Dock = System.Windows.Forms.DockStyle.Fill;
            this.optionsList.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.optionsList.Location = new System.Drawing.Point(1, 1);
            this.optionsList.Name = "optionsList";
            this.optionsList.Size = new System.Drawing.Size(198, 314);
            this.optionsList.TabIndex = 6;
            // 
            // panel11
            // 
            this.panel11.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.panel11.Controls.Add(this.butApply);
            this.panel11.Controls.Add(this.butOk);
            this.panel11.Controls.Add(this.butCancel);
            this.panel11.Location = new System.Drawing.Point(553, 325);
            this.panel11.Name = "panel11";
            this.panel11.Padding = new System.Windows.Forms.Padding(0, 0, 4, 0);
            this.panel11.Size = new System.Drawing.Size(260, 28);
            this.panel11.TabIndex = 5;
            // 
            // butApply
            // 
            this.butApply.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.butApply.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.butApply.Location = new System.Drawing.Point(3, 1);
            this.butApply.Name = "butApply";
            this.butApply.Size = new System.Drawing.Size(80, 24);
            this.butApply.TabIndex = 3;
            this.butApply.Text = "Apply";
            // 
            // butOk
            // 
            this.butOk.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.butOk.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.butOk.Location = new System.Drawing.Point(89, 1);
            this.butOk.Name = "butOk";
            this.butOk.Size = new System.Drawing.Size(80, 24);
            this.butOk.TabIndex = 3;
            this.butOk.Text = "OK";
            // 
            // butCancel
            // 
            this.butCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.butCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.butCancel.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.butCancel.Location = new System.Drawing.Point(175, 1);
            this.butCancel.Name = "butCancel";
            this.butCancel.Size = new System.Drawing.Size(80, 24);
            this.butCancel.TabIndex = 3;
            this.butCancel.Text = "Cancel";
            // 
            // FormOptions
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(816, 356);
            this.Controls.Add(this.tableLayoutPanel1);
            this.MinimizeBox = false;
            this.Name = "FormOptions";
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.Text = "Options";
            this.tableLayoutPanel1.ResumeLayout(false);
            this.darkSectionPanel2.ResumeLayout(false);
            this.tabbedContainer.ResumeLayout(false);
            this.darkSectionPanel1.ResumeLayout(false);
            this.panel11.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private DarkUI.Controls.DarkSectionPanel darkSectionPanel2;
        private Controls.DarkTabbedContainer tabbedContainer;
        private System.Windows.Forms.TabPage tabPage8;
        private DarkUI.Controls.DarkSectionPanel darkSectionPanel1;
        private DarkUI.Controls.DarkListView optionsList;
        private System.Windows.Forms.Panel panel11;
        private DarkUI.Controls.DarkButton butApply;
        private DarkUI.Controls.DarkButton butOk;
        private DarkUI.Controls.DarkButton butCancel;
    }
}