namespace WadTool
{
    partial class FormSkeletonEditor
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormSkeletonEditor));
            this.treeSkeleton = new DarkUI.Controls.DarkTreeView();
            this.panelRendering = new WadTool.Controls.PanelRenderingSkeleton();
            this.cbDrawGizmo = new DarkUI.Controls.DarkCheckBox();
            this.cbDrawGrid = new DarkUI.Controls.DarkCheckBox();
            this.butSaveChanges = new DarkUI.Controls.DarkButton();
            this.butRenameBone = new DarkUI.Controls.DarkButton();
            this.butDeleteBone = new DarkUI.Controls.DarkButton();
            this.SuspendLayout();
            // 
            // treeSkeleton
            // 
            this.treeSkeleton.AllowMoveNodes = true;
            this.treeSkeleton.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.treeSkeleton.Location = new System.Drawing.Point(725, 12);
            this.treeSkeleton.MaxDragChange = 20;
            this.treeSkeleton.Name = "treeSkeleton";
            this.treeSkeleton.Size = new System.Drawing.Size(271, 600);
            this.treeSkeleton.TabIndex = 0;
            this.treeSkeleton.Text = "darkTreeView1";
            this.treeSkeleton.Click += new System.EventHandler(this.treeSkeleton_Click);
            // 
            // panelRendering
            // 
            this.panelRendering.Location = new System.Drawing.Point(12, 12);
            this.panelRendering.Name = "panelRendering";
            this.panelRendering.Size = new System.Drawing.Size(706, 678);
            this.panelRendering.TabIndex = 1;
            // 
            // cbDrawGizmo
            // 
            this.cbDrawGizmo.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.cbDrawGizmo.AutoSize = true;
            this.cbDrawGizmo.Checked = true;
            this.cbDrawGizmo.CheckState = System.Windows.Forms.CheckState.Checked;
            this.cbDrawGizmo.Location = new System.Drawing.Point(725, 650);
            this.cbDrawGizmo.Name = "cbDrawGizmo";
            this.cbDrawGizmo.Size = new System.Drawing.Size(81, 17);
            this.cbDrawGizmo.TabIndex = 82;
            this.cbDrawGizmo.Text = "Draw gizmo";
            this.cbDrawGizmo.CheckedChanged += new System.EventHandler(this.cbDrawGizmo_CheckedChanged);
            // 
            // cbDrawGrid
            // 
            this.cbDrawGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.cbDrawGrid.AutoSize = true;
            this.cbDrawGrid.Checked = true;
            this.cbDrawGrid.CheckState = System.Windows.Forms.CheckState.Checked;
            this.cbDrawGrid.Location = new System.Drawing.Point(725, 673);
            this.cbDrawGrid.Name = "cbDrawGrid";
            this.cbDrawGrid.Size = new System.Drawing.Size(71, 17);
            this.cbDrawGrid.TabIndex = 81;
            this.cbDrawGrid.Text = "Draw grid";
            this.cbDrawGrid.CheckedChanged += new System.EventHandler(this.cbDrawGrid_CheckedChanged);
            // 
            // butSaveChanges
            // 
            this.butSaveChanges.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.butSaveChanges.Image = global::WadTool.Properties.Resources.save_16;
            this.butSaveChanges.Location = new System.Drawing.Point(884, 667);
            this.butSaveChanges.Name = "butSaveChanges";
            this.butSaveChanges.Size = new System.Drawing.Size(112, 23);
            this.butSaveChanges.TabIndex = 80;
            this.butSaveChanges.Text = "Save changes";
            this.butSaveChanges.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.butSaveChanges.Click += new System.EventHandler(this.butSaveChanges_Click);
            // 
            // butRenameBone
            // 
            this.butRenameBone.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.butRenameBone.Image = global::WadTool.Properties.Resources.edit_16;
            this.butRenameBone.Location = new System.Drawing.Point(725, 621);
            this.butRenameBone.Name = "butRenameBone";
            this.butRenameBone.Size = new System.Drawing.Size(81, 23);
            this.butRenameBone.TabIndex = 84;
            this.butRenameBone.Text = "Rename";
            this.butRenameBone.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.butRenameBone.Click += new System.EventHandler(this.butRenameBone_Click);
            // 
            // butDeleteBone
            // 
            this.butDeleteBone.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.butDeleteBone.Image = global::WadTool.Properties.Resources.trash_161;
            this.butDeleteBone.Location = new System.Drawing.Point(812, 621);
            this.butDeleteBone.Name = "butDeleteBone";
            this.butDeleteBone.Size = new System.Drawing.Size(72, 23);
            this.butDeleteBone.TabIndex = 83;
            this.butDeleteBone.Text = "Delete";
            this.butDeleteBone.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.butDeleteBone.Click += new System.EventHandler(this.butDeleteBone_Click);
            // 
            // FormSkeletonEditor
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1016, 741);
            this.Controls.Add(this.butRenameBone);
            this.Controls.Add(this.butDeleteBone);
            this.Controls.Add(this.cbDrawGizmo);
            this.Controls.Add(this.cbDrawGrid);
            this.Controls.Add(this.butSaveChanges);
            this.Controls.Add(this.treeSkeleton);
            this.Controls.Add(this.panelRendering);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MinimumSize = new System.Drawing.Size(1024, 768);
            this.Name = "FormSkeletonEditor";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Skeleton editor";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private DarkUI.Controls.DarkTreeView treeSkeleton;
        private Controls.PanelRenderingSkeleton panelRendering;
        private DarkUI.Controls.DarkCheckBox cbDrawGizmo;
        private DarkUI.Controls.DarkCheckBox cbDrawGrid;
        private DarkUI.Controls.DarkButton butSaveChanges;
        private DarkUI.Controls.DarkButton butRenameBone;
        private DarkUI.Controls.DarkButton butDeleteBone;
    }
}