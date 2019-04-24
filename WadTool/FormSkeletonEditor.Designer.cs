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
            this.cbDrawGizmo = new DarkUI.Controls.DarkCheckBox();
            this.cbDrawGrid = new DarkUI.Controls.DarkCheckBox();
            this.butSaveChanges = new DarkUI.Controls.DarkButton();
            this.butRenameBone = new DarkUI.Controls.DarkButton();
            this.butDeleteBone = new DarkUI.Controls.DarkButton();
            this.butLoadModel = new DarkUI.Controls.DarkButton();
            this.darkStatusStrip1 = new DarkUI.Controls.DarkStatusStrip();
            this.butAddFromWad2 = new DarkUI.Controls.DarkButton();
            this.butAddFromFile = new DarkUI.Controls.DarkButton();
            this.butReplaceFromWad2 = new DarkUI.Controls.DarkButton();
            this.butReplaceFromFile = new DarkUI.Controls.DarkButton();
            this.cmBone = new DarkUI.Controls.DarkContextMenu();
            this.popToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.pushToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem1 = new System.Windows.Forms.ToolStripSeparator();
            this.moveUpToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.moveDownToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.panelRendering = new WadTool.Controls.PanelRenderingSkeleton();
            this.cmBone.SuspendLayout();
            this.SuspendLayout();
            // 
            // treeSkeleton
            // 
            this.treeSkeleton.AllowMoveNodes = true;
            this.treeSkeleton.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.treeSkeleton.Location = new System.Drawing.Point(725, 12);
            this.treeSkeleton.MaxDragChange = 20;
            this.treeSkeleton.Name = "treeSkeleton";
            this.treeSkeleton.Size = new System.Drawing.Size(280, 533);
            this.treeSkeleton.TabIndex = 0;
            this.treeSkeleton.Text = "darkTreeView1";
            this.treeSkeleton.Click += new System.EventHandler(this.treeSkeleton_Click);
            this.treeSkeleton.MouseDown += new System.Windows.Forms.MouseEventHandler(this.TreeSkeleton_MouseDown);
            // 
            // cbDrawGizmo
            // 
            this.cbDrawGizmo.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.cbDrawGizmo.AutoSize = true;
            this.cbDrawGizmo.Checked = true;
            this.cbDrawGizmo.CheckState = System.Windows.Forms.CheckState.Checked;
            this.cbDrawGizmo.Location = new System.Drawing.Point(724, 662);
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
            this.cbDrawGrid.Location = new System.Drawing.Point(724, 685);
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
            this.butSaveChanges.Location = new System.Drawing.Point(868, 679);
            this.butSaveChanges.Name = "butSaveChanges";
            this.butSaveChanges.Size = new System.Drawing.Size(137, 23);
            this.butSaveChanges.TabIndex = 80;
            this.butSaveChanges.Text = "Save changes";
            this.butSaveChanges.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.butSaveChanges.Click += new System.EventHandler(this.butSaveChanges_Click);
            // 
            // butRenameBone
            // 
            this.butRenameBone.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.butRenameBone.Image = global::WadTool.Properties.Resources.edit_16;
            this.butRenameBone.Location = new System.Drawing.Point(725, 551);
            this.butRenameBone.Name = "butRenameBone";
            this.butRenameBone.Size = new System.Drawing.Size(137, 23);
            this.butRenameBone.TabIndex = 84;
            this.butRenameBone.Text = "Rename bone";
            this.butRenameBone.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.butRenameBone.Click += new System.EventHandler(this.butRenameBone_Click);
            // 
            // butDeleteBone
            // 
            this.butDeleteBone.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.butDeleteBone.Image = global::WadTool.Properties.Resources.trash_161;
            this.butDeleteBone.Location = new System.Drawing.Point(868, 551);
            this.butDeleteBone.Name = "butDeleteBone";
            this.butDeleteBone.Size = new System.Drawing.Size(137, 23);
            this.butDeleteBone.TabIndex = 83;
            this.butDeleteBone.Text = "Delete bone";
            this.butDeleteBone.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.butDeleteBone.Click += new System.EventHandler(this.butDeleteBone_Click);
            // 
            // butLoadModel
            // 
            this.butLoadModel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.butLoadModel.Image = global::WadTool.Properties.Resources.opened_folder_16;
            this.butLoadModel.Location = new System.Drawing.Point(868, 638);
            this.butLoadModel.Name = "butLoadModel";
            this.butLoadModel.Size = new System.Drawing.Size(137, 23);
            this.butLoadModel.TabIndex = 80;
            this.butLoadModel.Text = "Load model";
            this.butLoadModel.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.butLoadModel.Click += new System.EventHandler(this.butLoadModel_Click);
            // 
            // darkStatusStrip1
            // 
            this.darkStatusStrip1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.darkStatusStrip1.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.darkStatusStrip1.Location = new System.Drawing.Point(0, 705);
            this.darkStatusStrip1.Name = "darkStatusStrip1";
            this.darkStatusStrip1.Padding = new System.Windows.Forms.Padding(0, 5, 0, 3);
            this.darkStatusStrip1.Size = new System.Drawing.Size(1016, 24);
            this.darkStatusStrip1.TabIndex = 85;
            this.darkStatusStrip1.Text = "darkStatusStrip1";
            // 
            // butAddFromWad2
            // 
            this.butAddFromWad2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.butAddFromWad2.Image = global::WadTool.Properties.Resources.search_16;
            this.butAddFromWad2.Location = new System.Drawing.Point(868, 609);
            this.butAddFromWad2.Name = "butAddFromWad2";
            this.butAddFromWad2.Size = new System.Drawing.Size(137, 23);
            this.butAddFromWad2.TabIndex = 87;
            this.butAddFromWad2.Text = "Add from Wad2";
            this.butAddFromWad2.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.butAddFromWad2.Click += new System.EventHandler(this.butSelectMesh_Click);
            // 
            // butAddFromFile
            // 
            this.butAddFromFile.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.butAddFromFile.Image = global::WadTool.Properties.Resources.opened_folder_16;
            this.butAddFromFile.Location = new System.Drawing.Point(868, 580);
            this.butAddFromFile.Name = "butAddFromFile";
            this.butAddFromFile.Size = new System.Drawing.Size(137, 23);
            this.butAddFromFile.TabIndex = 86;
            this.butAddFromFile.Text = "Add from file";
            this.butAddFromFile.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.butAddFromFile.Click += new System.EventHandler(this.butAddFromFile_Click);
            // 
            // butReplaceFromWad2
            // 
            this.butReplaceFromWad2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.butReplaceFromWad2.Image = global::WadTool.Properties.Resources.search_16;
            this.butReplaceFromWad2.Location = new System.Drawing.Point(725, 609);
            this.butReplaceFromWad2.Name = "butReplaceFromWad2";
            this.butReplaceFromWad2.Size = new System.Drawing.Size(137, 23);
            this.butReplaceFromWad2.TabIndex = 90;
            this.butReplaceFromWad2.Text = "Replace from Wad2";
            this.butReplaceFromWad2.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.butReplaceFromWad2.Click += new System.EventHandler(this.butReplaceFromWad2_Click);
            // 
            // butReplaceFromFile
            // 
            this.butReplaceFromFile.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.butReplaceFromFile.Image = global::WadTool.Properties.Resources.opened_folder_16;
            this.butReplaceFromFile.Location = new System.Drawing.Point(725, 580);
            this.butReplaceFromFile.Name = "butReplaceFromFile";
            this.butReplaceFromFile.Size = new System.Drawing.Size(137, 23);
            this.butReplaceFromFile.TabIndex = 89;
            this.butReplaceFromFile.Text = "Replace from file";
            this.butReplaceFromFile.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.butReplaceFromFile.Click += new System.EventHandler(this.butReplaceFromFile_Click);
            // 
            // cmBone
            // 
            this.cmBone.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.cmBone.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.cmBone.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.popToolStripMenuItem,
            this.pushToolStripMenuItem,
            this.toolStripMenuItem1,
            this.moveUpToolStripMenuItem,
            this.moveDownToolStripMenuItem});
            this.cmBone.Name = "cmBone";
            this.cmBone.Size = new System.Drawing.Size(138, 99);
            // 
            // popToolStripMenuItem
            // 
            this.popToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.popToolStripMenuItem.CheckOnClick = true;
            this.popToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.popToolStripMenuItem.Name = "popToolStripMenuItem";
            this.popToolStripMenuItem.Size = new System.Drawing.Size(137, 22);
            this.popToolStripMenuItem.Text = "Pop";
            this.popToolStripMenuItem.Click += new System.EventHandler(this.PopToolStripMenuItem_Click);
            // 
            // pushToolStripMenuItem
            // 
            this.pushToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.pushToolStripMenuItem.CheckOnClick = true;
            this.pushToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.pushToolStripMenuItem.Name = "pushToolStripMenuItem";
            this.pushToolStripMenuItem.Size = new System.Drawing.Size(137, 22);
            this.pushToolStripMenuItem.Text = "Push";
            this.pushToolStripMenuItem.Click += new System.EventHandler(this.PushToolStripMenuItem_Click);
            // 
            // toolStripMenuItem1
            // 
            this.toolStripMenuItem1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.toolStripMenuItem1.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.toolStripMenuItem1.Margin = new System.Windows.Forms.Padding(0, 0, 0, 1);
            this.toolStripMenuItem1.Name = "toolStripMenuItem1";
            this.toolStripMenuItem1.Size = new System.Drawing.Size(134, 6);
            // 
            // moveUpToolStripMenuItem
            // 
            this.moveUpToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.moveUpToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.moveUpToolStripMenuItem.Name = "moveUpToolStripMenuItem";
            this.moveUpToolStripMenuItem.Size = new System.Drawing.Size(137, 22);
            this.moveUpToolStripMenuItem.Text = "Move up";
            this.moveUpToolStripMenuItem.Click += new System.EventHandler(this.MoveUpToolStripMenuItem_Click);
            // 
            // moveDownToolStripMenuItem
            // 
            this.moveDownToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.moveDownToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.moveDownToolStripMenuItem.Name = "moveDownToolStripMenuItem";
            this.moveDownToolStripMenuItem.Size = new System.Drawing.Size(137, 22);
            this.moveDownToolStripMenuItem.Text = "Move down";
            this.moveDownToolStripMenuItem.Click += new System.EventHandler(this.MoveDownToolStripMenuItem_Click);
            // 
            // panelRendering
            // 
            this.panelRendering.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panelRendering.Location = new System.Drawing.Point(0, 12);
            this.panelRendering.Name = "panelRendering";
            this.panelRendering.Size = new System.Drawing.Size(719, 690);
            this.panelRendering.TabIndex = 91;
            // 
            // FormSkeletonEditor
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1016, 729);
            this.Controls.Add(this.panelRendering);
            this.Controls.Add(this.butReplaceFromWad2);
            this.Controls.Add(this.butReplaceFromFile);
            this.Controls.Add(this.butAddFromWad2);
            this.Controls.Add(this.butAddFromFile);
            this.Controls.Add(this.darkStatusStrip1);
            this.Controls.Add(this.butRenameBone);
            this.Controls.Add(this.butDeleteBone);
            this.Controls.Add(this.cbDrawGizmo);
            this.Controls.Add(this.cbDrawGrid);
            this.Controls.Add(this.butLoadModel);
            this.Controls.Add(this.butSaveChanges);
            this.Controls.Add(this.treeSkeleton);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.KeyPreview = true;
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(1024, 768);
            this.Name = "FormSkeletonEditor";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Skeleton editor";
            this.Load += new System.EventHandler(this.FormSkeletonEditor_Load);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.FormSkeletonEditor_KeyDown);
            this.cmBone.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private DarkUI.Controls.DarkTreeView treeSkeleton;
        private DarkUI.Controls.DarkCheckBox cbDrawGizmo;
        private DarkUI.Controls.DarkCheckBox cbDrawGrid;
        private DarkUI.Controls.DarkButton butSaveChanges;
        private DarkUI.Controls.DarkButton butRenameBone;
        private DarkUI.Controls.DarkButton butDeleteBone;
        private DarkUI.Controls.DarkButton butLoadModel;
        private DarkUI.Controls.DarkStatusStrip darkStatusStrip1;
        private DarkUI.Controls.DarkButton butAddFromWad2;
        private DarkUI.Controls.DarkButton butAddFromFile;
        private DarkUI.Controls.DarkButton butReplaceFromWad2;
        private DarkUI.Controls.DarkButton butReplaceFromFile;
        private DarkUI.Controls.DarkContextMenu cmBone;
        private System.Windows.Forms.ToolStripMenuItem popToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem pushToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem moveUpToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem moveDownToolStripMenuItem;
        private Controls.PanelRenderingSkeleton panelRendering;
    }
}