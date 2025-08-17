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
            components = new System.ComponentModel.Container();
            treeFunctions = new DarkUI.Controls.DarkTreeView();
            sectionDesc = new DarkUI.Controls.DarkSectionPanel();
            lblDesc = new DarkUI.Controls.DarkLabel();
            txtSearch = new DarkUI.Controls.DarkTextBox();
            butSearch = new DarkUI.Controls.DarkButton();
            butExpand = new DarkUI.Controls.DarkButton();
            toolTip1 = new System.Windows.Forms.ToolTip(components);
            sectionDesc.SuspendLayout();
            SuspendLayout();
            // 
            // treeFunctions
            // 
            treeFunctions.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            treeFunctions.ExpandOnDoubleClick = false;
            treeFunctions.Location = new System.Drawing.Point(6, 34);
            treeFunctions.MaxDragChange = 20;
            treeFunctions.Name = "treeFunctions";
            treeFunctions.ShowIcons = true;
            treeFunctions.Size = new System.Drawing.Size(328, 317);
            treeFunctions.TabIndex = 0;
            treeFunctions.Text = "darkTreeView1";
            treeFunctions.SelectedNodesChanged += treeFunctions_SelectedNodesChanged;
            treeFunctions.KeyDown += treeFunctions_KeyDown;
            treeFunctions.MouseDoubleClick += treeFunctions_MouseDoubleClick;
            // 
            // sectionDesc
            // 
            sectionDesc.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            sectionDesc.Controls.Add(lblDesc);
            sectionDesc.Location = new System.Drawing.Point(6, 357);
            sectionDesc.Name = "sectionDesc";
            sectionDesc.SectionHeader = null;
            sectionDesc.Size = new System.Drawing.Size(328, 77);
            sectionDesc.TabIndex = 1;
            // 
            // lblDesc
            // 
            lblDesc.Dock = System.Windows.Forms.DockStyle.Fill;
            lblDesc.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
            lblDesc.Location = new System.Drawing.Point(1, 1);
            lblDesc.Name = "lblDesc";
            lblDesc.Padding = new System.Windows.Forms.Padding(2);
            lblDesc.Size = new System.Drawing.Size(326, 75);
            lblDesc.TabIndex = 0;
            // 
            // txtSearch
            // 
            txtSearch.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            txtSearch.Location = new System.Drawing.Point(6, 6);
            txtSearch.Name = "txtSearch";
            txtSearch.Size = new System.Drawing.Size(278, 22);
            txtSearch.TabIndex = 2;
            txtSearch.KeyDown += txtSearch_KeyDown;
            // 
            // butSearch
            // 
            butSearch.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            butSearch.Checked = false;
            butSearch.Image = Properties.Resources.general_search_16;
            butSearch.Location = new System.Drawing.Point(284, 6);
            butSearch.Name = "butSearch";
            butSearch.Size = new System.Drawing.Size(22, 22);
            butSearch.TabIndex = 3;
            toolTip1.SetToolTip(butSearch, "Search for nodes");
            butSearch.Click += butSearch_Click;
            // 
            // butExpand
            // 
            butExpand.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            butExpand.Checked = false;
            butExpand.Image = Properties.Resources.general_ArrowDown_16;
            butExpand.Location = new System.Drawing.Point(312, 6);
            butExpand.Name = "butExpand";
            butExpand.Size = new System.Drawing.Size(22, 22);
            butExpand.TabIndex = 4;
            toolTip1.SetToolTip(butExpand, "Expand all nodes");
            butExpand.Click += butExpand_Click;
            // 
            // FormFunctionList
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(340, 440);
            Controls.Add(butExpand);
            Controls.Add(butSearch);
            Controls.Add(txtSearch);
            Controls.Add(sectionDesc);
            Controls.Add(treeFunctions);
            DoubleBuffered = true;
            FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            KeyPreview = true;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "FormFunctionList";
            ShowIcon = false;
            ShowInTaskbar = false;
            SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            StartPosition = System.Windows.Forms.FormStartPosition.Manual;
            Text = "FunctionList";
            sectionDesc.ResumeLayout(false);
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private DarkUI.Controls.DarkTreeView treeFunctions;
        private DarkUI.Controls.DarkSectionPanel sectionDesc;
        private DarkUI.Controls.DarkLabel lblDesc;
        private DarkUI.Controls.DarkTextBox txtSearch;
        private DarkUI.Controls.DarkButton butSearch;
        private DarkUI.Controls.DarkButton butExpand;
        private System.Windows.Forms.ToolTip toolTip1;
    }
}