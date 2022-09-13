namespace TombLib.Controls.VisualScripting
{
    partial class FormFunctionList
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
            this.treeFunctions = new DarkUI.Controls.DarkTreeView();
            this.sectionDesc = new DarkUI.Controls.DarkSectionPanel();
            this.lblDesc = new DarkUI.Controls.DarkLabel();
            this.txtSearch = new DarkUI.Controls.DarkTextBox();
            this.butSearch = new DarkUI.Controls.DarkButton();
            this.sectionDesc.SuspendLayout();
            this.SuspendLayout();
            // 
            // treeFunctions
            // 
            this.treeFunctions.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.treeFunctions.ExpandOnDoubleClick = false;
            this.treeFunctions.Location = new System.Drawing.Point(6, 34);
            this.treeFunctions.MaxDragChange = 20;
            this.treeFunctions.Name = "treeFunctions";
            this.treeFunctions.ShowIcons = true;
            this.treeFunctions.Size = new System.Drawing.Size(277, 269);
            this.treeFunctions.TabIndex = 0;
            this.treeFunctions.Text = "darkTreeView1";
            this.treeFunctions.SelectedNodesChanged += new System.EventHandler(this.treeFunctions_SelectedNodesChanged);
            this.treeFunctions.KeyDown += new System.Windows.Forms.KeyEventHandler(this.treeFunctions_KeyDown);
            this.treeFunctions.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.treeFunctions_MouseDoubleClick);
            // 
            // sectionDesc
            // 
            this.sectionDesc.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.sectionDesc.Controls.Add(this.lblDesc);
            this.sectionDesc.Location = new System.Drawing.Point(6, 309);
            this.sectionDesc.Name = "sectionDesc";
            this.sectionDesc.SectionHeader = null;
            this.sectionDesc.Size = new System.Drawing.Size(277, 77);
            this.sectionDesc.TabIndex = 1;
            // 
            // lblDesc
            // 
            this.lblDesc.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblDesc.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.lblDesc.Location = new System.Drawing.Point(1, 1);
            this.lblDesc.Name = "lblDesc";
            this.lblDesc.Padding = new System.Windows.Forms.Padding(2);
            this.lblDesc.Size = new System.Drawing.Size(275, 75);
            this.lblDesc.TabIndex = 0;
            // 
            // txtSearch
            // 
            this.txtSearch.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtSearch.Location = new System.Drawing.Point(6, 6);
            this.txtSearch.Name = "txtSearch";
            this.txtSearch.Size = new System.Drawing.Size(255, 22);
            this.txtSearch.TabIndex = 2;
            this.txtSearch.KeyDown += new System.Windows.Forms.KeyEventHandler(this.txtSearch_KeyDown);
            // 
            // butSearch
            // 
            this.butSearch.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.butSearch.Checked = false;
            this.butSearch.Image = global::TombLib.Properties.Resources.general_search_16;
            this.butSearch.Location = new System.Drawing.Point(261, 6);
            this.butSearch.Name = "butSearch";
            this.butSearch.Size = new System.Drawing.Size(22, 22);
            this.butSearch.TabIndex = 3;
            this.butSearch.Click += new System.EventHandler(this.butSearch_Click);
            // 
            // FormFunctionList
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(289, 392);
            this.Controls.Add(this.butSearch);
            this.Controls.Add(this.txtSearch);
            this.Controls.Add(this.sectionDesc);
            this.Controls.Add(this.treeFunctions);
            this.DoubleBuffered = true;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.KeyPreview = true;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FormFunctionList";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
            this.Text = "FunctionList";
            this.sectionDesc.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private DarkUI.Controls.DarkTreeView treeFunctions;
        private DarkUI.Controls.DarkSectionPanel sectionDesc;
        private DarkUI.Controls.DarkLabel lblDesc;
        private DarkUI.Controls.DarkTextBox txtSearch;
        private DarkUI.Controls.DarkButton butSearch;
    }
}