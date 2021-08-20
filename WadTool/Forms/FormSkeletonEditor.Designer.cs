namespace WadTool
{
    partial class FormSkeletonEditor
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
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
            this.toolStripMenuItem2 = new System.Windows.Forms.ToolStripSeparator();
            this.addChildBoneFromFileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.replaceToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem4 = new System.Windows.Forms.ToolStripSeparator();
            this.addChildBoneToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.replaceFromWad2ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem3 = new System.Windows.Forms.ToolStripSeparator();
            this.deleteToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.renameToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.sectionCurrentBone = new DarkUI.Controls.DarkSectionPanel();
            this.comboLightType = new DarkUI.Controls.DarkComboBox();
            this.butSetToAll = new DarkUI.Controls.DarkButton();
            this.darkLabel1 = new DarkUI.Controls.DarkLabel();
            this.nudTransZ = new DarkUI.Controls.DarkNumericUpDown();
            this.nudTransY = new DarkUI.Controls.DarkNumericUpDown();
            this.darkLabel29 = new DarkUI.Controls.DarkLabel();
            this.darkLabel27 = new DarkUI.Controls.DarkLabel();
            this.nudTransX = new DarkUI.Controls.DarkNumericUpDown();
            this.darkLabel26 = new DarkUI.Controls.DarkLabel();
            this.darkLabel21 = new DarkUI.Controls.DarkLabel();
            this.panelAll = new DarkUI.Controls.DarkPanel();
            this.panelMain = new DarkUI.Controls.DarkPanel();
            this.panelRendering = new WadTool.Controls.PanelRenderingSkeleton();
            this.panelRight = new DarkUI.Controls.DarkPanel();
            this.section3D = new DarkUI.Controls.DarkSectionPanel();
            this.butEditMesh = new DarkUI.Controls.DarkButton();
            this.butExportSelectedMesh = new DarkUI.Controls.DarkButton();
            this.butCancel = new DarkUI.Controls.DarkButton();
            this.toolTip = new System.Windows.Forms.ToolTip(this.components);
            this.cmBone.SuspendLayout();
            this.sectionCurrentBone.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudTransZ)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudTransY)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudTransX)).BeginInit();
            this.panelAll.SuspendLayout();
            this.panelMain.SuspendLayout();
            this.panelRight.SuspendLayout();
            this.section3D.SuspendLayout();
            this.SuspendLayout();
            // 
            // treeSkeleton
            // 
            this.treeSkeleton.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.treeSkeleton.EvenNodeColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.treeSkeleton.ExpandOnDoubleClick = false;
            this.treeSkeleton.FocusedNodeColor = System.Drawing.Color.FromArgb(((int)(((byte)(75)))), ((int)(((byte)(110)))), ((int)(((byte)(175)))));
            this.treeSkeleton.Location = new System.Drawing.Point(3, 28);
            this.treeSkeleton.MaxDragChange = 20;
            this.treeSkeleton.Name = "treeSkeleton";
            this.treeSkeleton.NonFocusedNodeColor = System.Drawing.Color.FromArgb(((int)(((byte)(92)))), ((int)(((byte)(92)))), ((int)(((byte)(92)))));
            this.treeSkeleton.OddNodeColor = System.Drawing.Color.FromArgb(((int)(((byte)(57)))), ((int)(((byte)(60)))), ((int)(((byte)(62)))));
            this.treeSkeleton.Size = new System.Drawing.Size(310, 354);
            this.treeSkeleton.TabIndex = 0;
            this.treeSkeleton.Click += new System.EventHandler(this.treeSkeleton_Click);
            this.treeSkeleton.MouseDown += new System.Windows.Forms.MouseEventHandler(this.treeSkeleton_MouseDown);
            // 
            // cbDrawGizmo
            // 
            this.cbDrawGizmo.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.cbDrawGizmo.AutoSize = true;
            this.cbDrawGizmo.Checked = true;
            this.cbDrawGizmo.CheckState = System.Windows.Forms.CheckState.Checked;
            this.cbDrawGizmo.Location = new System.Drawing.Point(86, 570);
            this.cbDrawGizmo.Name = "cbDrawGizmo";
            this.cbDrawGizmo.Size = new System.Drawing.Size(87, 17);
            this.cbDrawGizmo.TabIndex = 82;
            this.cbDrawGizmo.Text = "Draw gizmo";
            this.cbDrawGizmo.CheckedChanged += new System.EventHandler(this.cbDrawGizmo_CheckedChanged);
            // 
            // cbDrawGrid
            // 
            this.cbDrawGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.cbDrawGrid.AutoSize = true;
            this.cbDrawGrid.Checked = true;
            this.cbDrawGrid.CheckState = System.Windows.Forms.CheckState.Checked;
            this.cbDrawGrid.Location = new System.Drawing.Point(7, 570);
            this.cbDrawGrid.Name = "cbDrawGrid";
            this.cbDrawGrid.Size = new System.Drawing.Size(77, 17);
            this.cbDrawGrid.TabIndex = 81;
            this.cbDrawGrid.Text = "Draw grid";
            this.cbDrawGrid.CheckedChanged += new System.EventHandler(this.cbDrawGrid_CheckedChanged);
            // 
            // butSaveChanges
            // 
            this.butSaveChanges.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.butSaveChanges.Checked = false;
            this.butSaveChanges.Location = new System.Drawing.Point(609, 566);
            this.butSaveChanges.Name = "butSaveChanges";
            this.butSaveChanges.Size = new System.Drawing.Size(81, 23);
            this.butSaveChanges.TabIndex = 80;
            this.butSaveChanges.Text = "OK";
            this.butSaveChanges.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.butSaveChanges.Click += new System.EventHandler(this.butSaveChanges_Click);
            // 
            // butRenameBone
            // 
            this.butRenameBone.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.butRenameBone.Checked = false;
            this.butRenameBone.Location = new System.Drawing.Point(3, 388);
            this.butRenameBone.Name = "butRenameBone";
            this.butRenameBone.Size = new System.Drawing.Size(100, 23);
            this.butRenameBone.TabIndex = 84;
            this.butRenameBone.Text = "Rename";
            this.butRenameBone.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.toolTip.SetToolTip(this.butRenameBone, "Rename currently selected bone");
            this.butRenameBone.Click += new System.EventHandler(this.butRenameBone_Click);
            // 
            // butDeleteBone
            // 
            this.butDeleteBone.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.butDeleteBone.Checked = false;
            this.butDeleteBone.Location = new System.Drawing.Point(214, 388);
            this.butDeleteBone.Name = "butDeleteBone";
            this.butDeleteBone.Size = new System.Drawing.Size(99, 23);
            this.butDeleteBone.TabIndex = 83;
            this.butDeleteBone.Text = "Delete";
            this.butDeleteBone.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.toolTip.SetToolTip(this.butDeleteBone, "Delete currently selected bone");
            this.butDeleteBone.Click += new System.EventHandler(this.butDeleteBone_Click);
            // 
            // butLoadModel
            // 
            this.butLoadModel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.butLoadModel.Checked = false;
            this.butLoadModel.Location = new System.Drawing.Point(214, 446);
            this.butLoadModel.Name = "butLoadModel";
            this.butLoadModel.Size = new System.Drawing.Size(99, 23);
            this.butLoadModel.TabIndex = 80;
            this.butLoadModel.Text = "Replace model";
            this.butLoadModel.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.toolTip.SetToolTip(this.butLoadModel, "Replace all meshes in skeleton from single 3D model");
            this.butLoadModel.Click += new System.EventHandler(this.butLoadModel_Click);
            // 
            // darkStatusStrip1
            // 
            this.darkStatusStrip1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.darkStatusStrip1.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.darkStatusStrip1.Location = new System.Drawing.Point(0, 595);
            this.darkStatusStrip1.Name = "darkStatusStrip1";
            this.darkStatusStrip1.Padding = new System.Windows.Forms.Padding(0, 5, 0, 3);
            this.darkStatusStrip1.Size = new System.Drawing.Size(784, 24);
            this.darkStatusStrip1.TabIndex = 85;
            this.darkStatusStrip1.Text = "darkStatusStrip1";
            // 
            // butAddFromWad2
            // 
            this.butAddFromWad2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.butAddFromWad2.Checked = false;
            this.butAddFromWad2.Location = new System.Drawing.Point(109, 417);
            this.butAddFromWad2.Name = "butAddFromWad2";
            this.butAddFromWad2.Size = new System.Drawing.Size(99, 23);
            this.butAddFromWad2.TabIndex = 87;
            this.butAddFromWad2.Text = "Add existing";
            this.butAddFromWad2.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.toolTip.SetToolTip(this.butAddFromWad2, "Make new bone by using existing mesh from current wad");
            this.butAddFromWad2.Click += new System.EventHandler(this.butSelectMesh_Click);
            // 
            // butAddFromFile
            // 
            this.butAddFromFile.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.butAddFromFile.Checked = false;
            this.butAddFromFile.Location = new System.Drawing.Point(3, 417);
            this.butAddFromFile.Name = "butAddFromFile";
            this.butAddFromFile.Size = new System.Drawing.Size(100, 23);
            this.butAddFromFile.TabIndex = 86;
            this.butAddFromFile.Text = "Add new";
            this.butAddFromFile.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.toolTip.SetToolTip(this.butAddFromFile, "Make new bone by importing 3D model");
            this.butAddFromFile.Click += new System.EventHandler(this.butAddFromFile_Click);
            // 
            // butReplaceFromWad2
            // 
            this.butReplaceFromWad2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.butReplaceFromWad2.Checked = false;
            this.butReplaceFromWad2.Location = new System.Drawing.Point(214, 417);
            this.butReplaceFromWad2.Name = "butReplaceFromWad2";
            this.butReplaceFromWad2.Size = new System.Drawing.Size(99, 23);
            this.butReplaceFromWad2.TabIndex = 90;
            this.butReplaceFromWad2.Text = "Replace existing";
            this.butReplaceFromWad2.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.toolTip.SetToolTip(this.butReplaceFromWad2, "Replace mesh for currently selected bone with existing one in wad");
            this.butReplaceFromWad2.Click += new System.EventHandler(this.butReplaceFromWad2_Click);
            // 
            // butReplaceFromFile
            // 
            this.butReplaceFromFile.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.butReplaceFromFile.Checked = false;
            this.butReplaceFromFile.Location = new System.Drawing.Point(3, 446);
            this.butReplaceFromFile.Name = "butReplaceFromFile";
            this.butReplaceFromFile.Size = new System.Drawing.Size(100, 23);
            this.butReplaceFromFile.TabIndex = 89;
            this.butReplaceFromFile.Text = "Import mesh";
            this.butReplaceFromFile.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.toolTip.SetToolTip(this.butReplaceFromFile, "Import new mesh for currently selected bone from 3D model");
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
            this.moveDownToolStripMenuItem,
            this.toolStripMenuItem2,
            this.addChildBoneFromFileToolStripMenuItem,
            this.replaceToolStripMenuItem,
            this.toolStripMenuItem4,
            this.addChildBoneToolStripMenuItem,
            this.replaceFromWad2ToolStripMenuItem,
            this.toolStripMenuItem3,
            this.deleteToolStripMenuItem,
            this.renameToolStripMenuItem});
            this.cmBone.Name = "cmBone";
            this.cmBone.Size = new System.Drawing.Size(191, 252);
            // 
            // popToolStripMenuItem
            // 
            this.popToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.popToolStripMenuItem.CheckOnClick = true;
            this.popToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.popToolStripMenuItem.Name = "popToolStripMenuItem";
            this.popToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.O)));
            this.popToolStripMenuItem.Size = new System.Drawing.Size(190, 22);
            this.popToolStripMenuItem.Text = "Pop";
            this.popToolStripMenuItem.Click += new System.EventHandler(this.PopToolStripMenuItem_Click);
            // 
            // pushToolStripMenuItem
            // 
            this.pushToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.pushToolStripMenuItem.CheckOnClick = true;
            this.pushToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.pushToolStripMenuItem.Name = "pushToolStripMenuItem";
            this.pushToolStripMenuItem.ShortcutKeyDisplayString = "Ctrl+P";
            this.pushToolStripMenuItem.Size = new System.Drawing.Size(190, 22);
            this.pushToolStripMenuItem.Text = "Push";
            this.pushToolStripMenuItem.Click += new System.EventHandler(this.PushToolStripMenuItem_Click);
            // 
            // toolStripMenuItem1
            // 
            this.toolStripMenuItem1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.toolStripMenuItem1.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.toolStripMenuItem1.Margin = new System.Windows.Forms.Padding(0, 0, 0, 1);
            this.toolStripMenuItem1.Name = "toolStripMenuItem1";
            this.toolStripMenuItem1.Size = new System.Drawing.Size(187, 6);
            // 
            // moveUpToolStripMenuItem
            // 
            this.moveUpToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.moveUpToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.moveUpToolStripMenuItem.Name = "moveUpToolStripMenuItem";
            this.moveUpToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Up)));
            this.moveUpToolStripMenuItem.Size = new System.Drawing.Size(190, 22);
            this.moveUpToolStripMenuItem.Text = "Move up";
            this.moveUpToolStripMenuItem.Click += new System.EventHandler(this.MoveUpToolStripMenuItem_Click);
            // 
            // moveDownToolStripMenuItem
            // 
            this.moveDownToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.moveDownToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.moveDownToolStripMenuItem.Name = "moveDownToolStripMenuItem";
            this.moveDownToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Down)));
            this.moveDownToolStripMenuItem.Size = new System.Drawing.Size(190, 22);
            this.moveDownToolStripMenuItem.Text = "Move down";
            this.moveDownToolStripMenuItem.Click += new System.EventHandler(this.MoveDownToolStripMenuItem_Click);
            // 
            // toolStripMenuItem2
            // 
            this.toolStripMenuItem2.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.toolStripMenuItem2.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.toolStripMenuItem2.Margin = new System.Windows.Forms.Padding(0, 0, 0, 1);
            this.toolStripMenuItem2.Name = "toolStripMenuItem2";
            this.toolStripMenuItem2.Size = new System.Drawing.Size(187, 6);
            // 
            // addChildBoneFromFileToolStripMenuItem
            // 
            this.addChildBoneFromFileToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.addChildBoneFromFileToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.addChildBoneFromFileToolStripMenuItem.Image = global::WadTool.Properties.Resources.general_plus_math_16;
            this.addChildBoneFromFileToolStripMenuItem.Name = "addChildBoneFromFileToolStripMenuItem";
            this.addChildBoneFromFileToolStripMenuItem.Size = new System.Drawing.Size(190, 22);
            this.addChildBoneFromFileToolStripMenuItem.Text = "Add child bone from file";
            this.addChildBoneFromFileToolStripMenuItem.Click += new System.EventHandler(this.AddChildBoneFromFileToolStripMenuItem_Click);
            // 
            // replaceToolStripMenuItem
            // 
            this.replaceToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.replaceToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.replaceToolStripMenuItem.Image = global::WadTool.Properties.Resources.actions_refresh_16;
            this.replaceToolStripMenuItem.Name = "replaceToolStripMenuItem";
            this.replaceToolStripMenuItem.Size = new System.Drawing.Size(190, 22);
            this.replaceToolStripMenuItem.Text = "Replace from file";
            this.replaceToolStripMenuItem.Click += new System.EventHandler(this.ReplaceToolStripMenuItem_Click);
            // 
            // toolStripMenuItem4
            // 
            this.toolStripMenuItem4.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.toolStripMenuItem4.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.toolStripMenuItem4.Margin = new System.Windows.Forms.Padding(0, 0, 0, 1);
            this.toolStripMenuItem4.Name = "toolStripMenuItem4";
            this.toolStripMenuItem4.Size = new System.Drawing.Size(187, 6);
            // 
            // addChildBoneToolStripMenuItem
            // 
            this.addChildBoneToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.addChildBoneToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.addChildBoneToolStripMenuItem.Image = global::WadTool.Properties.Resources.general_search_16;
            this.addChildBoneToolStripMenuItem.Name = "addChildBoneToolStripMenuItem";
            this.addChildBoneToolStripMenuItem.Size = new System.Drawing.Size(190, 22);
            this.addChildBoneToolStripMenuItem.Text = "Add child bone from Wad2";
            this.addChildBoneToolStripMenuItem.Click += new System.EventHandler(this.AddChildBoneToolStripMenuItem_Click);
            // 
            // replaceFromWad2ToolStripMenuItem
            // 
            this.replaceFromWad2ToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.replaceFromWad2ToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.replaceFromWad2ToolStripMenuItem.Image = global::WadTool.Properties.Resources.general_search_16;
            this.replaceFromWad2ToolStripMenuItem.Name = "replaceFromWad2ToolStripMenuItem";
            this.replaceFromWad2ToolStripMenuItem.Size = new System.Drawing.Size(190, 22);
            this.replaceFromWad2ToolStripMenuItem.Text = "Replace from Wad2";
            this.replaceFromWad2ToolStripMenuItem.Click += new System.EventHandler(this.ReplaceFromWad2ToolStripMenuItem_Click);
            // 
            // toolStripMenuItem3
            // 
            this.toolStripMenuItem3.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.toolStripMenuItem3.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.toolStripMenuItem3.Margin = new System.Windows.Forms.Padding(0, 0, 0, 1);
            this.toolStripMenuItem3.Name = "toolStripMenuItem3";
            this.toolStripMenuItem3.Size = new System.Drawing.Size(187, 6);
            // 
            // deleteToolStripMenuItem
            // 
            this.deleteToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.deleteToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.deleteToolStripMenuItem.Image = global::WadTool.Properties.Resources.trash_16;
            this.deleteToolStripMenuItem.Name = "deleteToolStripMenuItem";
            this.deleteToolStripMenuItem.Size = new System.Drawing.Size(190, 22);
            this.deleteToolStripMenuItem.Text = "Delete";
            this.deleteToolStripMenuItem.Click += new System.EventHandler(this.DeleteToolStripMenuItem_Click);
            // 
            // renameToolStripMenuItem
            // 
            this.renameToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.renameToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.renameToolStripMenuItem.Image = global::WadTool.Properties.Resources.edit_16;
            this.renameToolStripMenuItem.Name = "renameToolStripMenuItem";
            this.renameToolStripMenuItem.Size = new System.Drawing.Size(190, 22);
            this.renameToolStripMenuItem.Text = "Rename";
            this.renameToolStripMenuItem.Click += new System.EventHandler(this.RenameToolStripMenuItem_Click);
            // 
            // sectionCurrentBone
            // 
            this.sectionCurrentBone.Controls.Add(this.comboLightType);
            this.sectionCurrentBone.Controls.Add(this.butSetToAll);
            this.sectionCurrentBone.Controls.Add(this.darkLabel1);
            this.sectionCurrentBone.Controls.Add(this.nudTransZ);
            this.sectionCurrentBone.Controls.Add(this.nudTransY);
            this.sectionCurrentBone.Controls.Add(this.darkLabel29);
            this.sectionCurrentBone.Controls.Add(this.darkLabel27);
            this.sectionCurrentBone.Controls.Add(this.nudTransX);
            this.sectionCurrentBone.Controls.Add(this.darkLabel26);
            this.sectionCurrentBone.Controls.Add(this.darkLabel21);
            this.sectionCurrentBone.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.sectionCurrentBone.Location = new System.Drawing.Point(0, 472);
            this.sectionCurrentBone.Name = "sectionCurrentBone";
            this.sectionCurrentBone.SectionHeader = "Current mesh";
            this.sectionCurrentBone.Size = new System.Drawing.Size(316, 82);
            this.sectionCurrentBone.TabIndex = 107;
            // 
            // comboLightType
            // 
            this.comboLightType.FormattingEnabled = true;
            this.comboLightType.Items.AddRange(new object[] {
            "Dynamic Lighting",
            "Static Lighting"});
            this.comboLightType.Location = new System.Drawing.Point(82, 56);
            this.comboLightType.Name = "comboLightType";
            this.comboLightType.Size = new System.Drawing.Size(147, 23);
            this.comboLightType.TabIndex = 106;
            this.comboLightType.SelectedIndexChanged += new System.EventHandler(this.comboLightType_SelectedIndexChanged);
            // 
            // butSetToAll
            // 
            this.butSetToAll.Checked = false;
            this.butSetToAll.Location = new System.Drawing.Point(235, 56);
            this.butSetToAll.Name = "butSetToAll";
            this.butSetToAll.Size = new System.Drawing.Size(78, 23);
            this.butSetToAll.TabIndex = 108;
            this.butSetToAll.Text = "Set to all";
            this.butSetToAll.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.butSetToAll.Click += new System.EventHandler(this.butSetToAll_Click);
            // 
            // darkLabel1
            // 
            this.darkLabel1.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.darkLabel1.Location = new System.Drawing.Point(4, 59);
            this.darkLabel1.Name = "darkLabel1";
            this.darkLabel1.Size = new System.Drawing.Size(76, 13);
            this.darkLabel1.TabIndex = 107;
            this.darkLabel1.Text = "Light mode:";
            // 
            // nudTransZ
            // 
            this.nudTransZ.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.nudTransZ.DecimalPlaces = 4;
            this.nudTransZ.IncrementAlternate = new decimal(new int[] {
            160,
            0,
            0,
            65536});
            this.nudTransZ.Location = new System.Drawing.Point(248, 28);
            this.nudTransZ.LoopValues = false;
            this.nudTransZ.Maximum = new decimal(new int[] {
            32767,
            0,
            0,
            0});
            this.nudTransZ.Minimum = new decimal(new int[] {
            32768,
            0,
            0,
            -2147483648});
            this.nudTransZ.Name = "nudTransZ";
            this.nudTransZ.Size = new System.Drawing.Size(65, 22);
            this.nudTransZ.TabIndex = 104;
            this.nudTransZ.ValueChanged += new System.EventHandler(this.nudTransZ_ValueChanged);
            // 
            // nudTransY
            // 
            this.nudTransY.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.nudTransY.DecimalPlaces = 4;
            this.nudTransY.IncrementAlternate = new decimal(new int[] {
            160,
            0,
            0,
            65536});
            this.nudTransY.Location = new System.Drawing.Point(165, 28);
            this.nudTransY.LoopValues = false;
            this.nudTransY.Maximum = new decimal(new int[] {
            32767,
            0,
            0,
            0});
            this.nudTransY.Minimum = new decimal(new int[] {
            32768,
            0,
            0,
            -2147483648});
            this.nudTransY.Name = "nudTransY";
            this.nudTransY.Size = new System.Drawing.Size(64, 22);
            this.nudTransY.TabIndex = 103;
            this.nudTransY.ValueChanged += new System.EventHandler(this.nudTransY_ValueChanged);
            // 
            // darkLabel29
            // 
            this.darkLabel29.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.darkLabel29.Location = new System.Drawing.Point(4, 30);
            this.darkLabel29.Name = "darkLabel29";
            this.darkLabel29.Size = new System.Drawing.Size(60, 13);
            this.darkLabel29.TabIndex = 105;
            this.darkLabel29.Text = "Pivot:";
            // 
            // darkLabel27
            // 
            this.darkLabel27.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.darkLabel27.AutoSize = true;
            this.darkLabel27.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.darkLabel27.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.darkLabel27.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(211)))), ((int)(((byte)(211)))), ((int)(((byte)(211)))));
            this.darkLabel27.Location = new System.Drawing.Point(66, 30);
            this.darkLabel27.Name = "darkLabel27";
            this.darkLabel27.Size = new System.Drawing.Size(14, 13);
            this.darkLabel27.TabIndex = 99;
            this.darkLabel27.Text = "X";
            // 
            // nudTransX
            // 
            this.nudTransX.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.nudTransX.DecimalPlaces = 4;
            this.nudTransX.IncrementAlternate = new decimal(new int[] {
            160,
            0,
            0,
            65536});
            this.nudTransX.Location = new System.Drawing.Point(82, 28);
            this.nudTransX.LoopValues = false;
            this.nudTransX.Maximum = new decimal(new int[] {
            32767,
            0,
            0,
            0});
            this.nudTransX.Minimum = new decimal(new int[] {
            32768,
            0,
            0,
            -2147483648});
            this.nudTransX.Name = "nudTransX";
            this.nudTransX.Size = new System.Drawing.Size(64, 22);
            this.nudTransX.TabIndex = 102;
            this.nudTransX.ValueChanged += new System.EventHandler(this.nudTransX_ValueChanged);
            // 
            // darkLabel26
            // 
            this.darkLabel26.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.darkLabel26.AutoSize = true;
            this.darkLabel26.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.darkLabel26.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.darkLabel26.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(211)))), ((int)(((byte)(211)))), ((int)(((byte)(211)))));
            this.darkLabel26.Location = new System.Drawing.Point(150, 30);
            this.darkLabel26.Name = "darkLabel26";
            this.darkLabel26.Size = new System.Drawing.Size(14, 13);
            this.darkLabel26.TabIndex = 100;
            this.darkLabel26.Text = "Y";
            // 
            // darkLabel21
            // 
            this.darkLabel21.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.darkLabel21.AutoSize = true;
            this.darkLabel21.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.darkLabel21.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.darkLabel21.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(211)))), ((int)(((byte)(211)))), ((int)(((byte)(211)))));
            this.darkLabel21.Location = new System.Drawing.Point(233, 30);
            this.darkLabel21.Name = "darkLabel21";
            this.darkLabel21.Size = new System.Drawing.Size(14, 13);
            this.darkLabel21.TabIndex = 101;
            this.darkLabel21.Text = "Z";
            // 
            // panelAll
            // 
            this.panelAll.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panelAll.Controls.Add(this.panelMain);
            this.panelAll.Controls.Add(this.panelRight);
            this.panelAll.Location = new System.Drawing.Point(7, 6);
            this.panelAll.Name = "panelAll";
            this.panelAll.Size = new System.Drawing.Size(770, 554);
            this.panelAll.TabIndex = 108;
            // 
            // panelMain
            // 
            this.panelMain.Controls.Add(this.panelRendering);
            this.panelMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelMain.Location = new System.Drawing.Point(0, 0);
            this.panelMain.Name = "panelMain";
            this.panelMain.Padding = new System.Windows.Forms.Padding(0, 0, 2, 0);
            this.panelMain.Size = new System.Drawing.Size(454, 554);
            this.panelMain.TabIndex = 0;
            // 
            // panelRendering
            // 
            this.panelRendering.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelRendering.Location = new System.Drawing.Point(0, 0);
            this.panelRendering.Name = "panelRendering";
            this.panelRendering.Size = new System.Drawing.Size(452, 554);
            this.panelRendering.TabIndex = 0;
            this.panelRendering.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.panelRendering_MouseDoubleClick);
            // 
            // panelRight
            // 
            this.panelRight.Controls.Add(this.section3D);
            this.panelRight.Controls.Add(this.sectionCurrentBone);
            this.panelRight.Dock = System.Windows.Forms.DockStyle.Right;
            this.panelRight.Location = new System.Drawing.Point(454, 0);
            this.panelRight.Name = "panelRight";
            this.panelRight.Size = new System.Drawing.Size(316, 554);
            this.panelRight.TabIndex = 1;
            // 
            // section3D
            // 
            this.section3D.Controls.Add(this.butAddFromWad2);
            this.section3D.Controls.Add(this.butRenameBone);
            this.section3D.Controls.Add(this.butAddFromFile);
            this.section3D.Controls.Add(this.butEditMesh);
            this.section3D.Controls.Add(this.butExportSelectedMesh);
            this.section3D.Controls.Add(this.treeSkeleton);
            this.section3D.Controls.Add(this.butLoadModel);
            this.section3D.Controls.Add(this.butDeleteBone);
            this.section3D.Controls.Add(this.butReplaceFromWad2);
            this.section3D.Controls.Add(this.butReplaceFromFile);
            this.section3D.Dock = System.Windows.Forms.DockStyle.Fill;
            this.section3D.Location = new System.Drawing.Point(0, 0);
            this.section3D.Name = "section3D";
            this.section3D.SectionHeader = "Mesh tree";
            this.section3D.Size = new System.Drawing.Size(316, 472);
            this.section3D.TabIndex = 108;
            // 
            // butEditMesh
            // 
            this.butEditMesh.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.butEditMesh.Checked = false;
            this.butEditMesh.Location = new System.Drawing.Point(109, 388);
            this.butEditMesh.Name = "butEditMesh";
            this.butEditMesh.Size = new System.Drawing.Size(99, 23);
            this.butEditMesh.TabIndex = 92;
            this.butEditMesh.Text = "Edit mesh";
            this.butEditMesh.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.toolTip.SetToolTip(this.butEditMesh, "Edit mesh for currently selected bone");
            this.butEditMesh.Click += new System.EventHandler(this.butEditMesh_Click);
            // 
            // butExportSelectedMesh
            // 
            this.butExportSelectedMesh.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.butExportSelectedMesh.Checked = false;
            this.butExportSelectedMesh.Location = new System.Drawing.Point(109, 446);
            this.butExportSelectedMesh.Name = "butExportSelectedMesh";
            this.butExportSelectedMesh.Size = new System.Drawing.Size(99, 23);
            this.butExportSelectedMesh.TabIndex = 91;
            this.butExportSelectedMesh.Text = "Export mesh";
            this.butExportSelectedMesh.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.toolTip.SetToolTip(this.butExportSelectedMesh, "Export currently selected bone\'s mesh to 3D model");
            this.butExportSelectedMesh.Click += new System.EventHandler(this.butExportSelectedMesh_Click);
            // 
            // butCancel
            // 
            this.butCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.butCancel.Checked = false;
            this.butCancel.Location = new System.Drawing.Point(696, 566);
            this.butCancel.Name = "butCancel";
            this.butCancel.Size = new System.Drawing.Size(81, 23);
            this.butCancel.TabIndex = 112;
            this.butCancel.Text = "Cancel";
            this.butCancel.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.butCancel.Click += new System.EventHandler(this.butCancel_Click);
            // 
            // FormSkeletonEditor
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(784, 619);
            this.Controls.Add(this.butCancel);
            this.Controls.Add(this.panelAll);
            this.Controls.Add(this.cbDrawGrid);
            this.Controls.Add(this.cbDrawGizmo);
            this.Controls.Add(this.darkStatusStrip1);
            this.Controls.Add(this.butSaveChanges);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.KeyPreview = true;
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(800, 500);
            this.Name = "FormSkeletonEditor";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Skeleton editor";
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.formSkeletonEditor_KeyDown);
            this.cmBone.ResumeLayout(false);
            this.sectionCurrentBone.ResumeLayout(false);
            this.sectionCurrentBone.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudTransZ)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudTransY)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudTransX)).EndInit();
            this.panelAll.ResumeLayout(false);
            this.panelMain.ResumeLayout(false);
            this.panelRight.ResumeLayout(false);
            this.section3D.ResumeLayout(false);
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
        private System.Windows.Forms.ToolStripSeparator toolStripMenuItem2;
        private System.Windows.Forms.ToolStripMenuItem renameToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem replaceToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem deleteToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem replaceFromWad2ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem addChildBoneFromFileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem addChildBoneToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuItem4;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuItem3;
        private DarkUI.Controls.DarkSectionPanel sectionCurrentBone;
        private DarkUI.Controls.DarkNumericUpDown nudTransZ;
        private DarkUI.Controls.DarkNumericUpDown nudTransY;
        private DarkUI.Controls.DarkLabel darkLabel27;
        private DarkUI.Controls.DarkNumericUpDown nudTransX;
        private DarkUI.Controls.DarkLabel darkLabel29;
        private DarkUI.Controls.DarkLabel darkLabel26;
        private DarkUI.Controls.DarkLabel darkLabel21;
        private DarkUI.Controls.DarkPanel panelAll;
        private DarkUI.Controls.DarkPanel panelRight;
        private DarkUI.Controls.DarkSectionPanel section3D;
        private DarkUI.Controls.DarkPanel panelMain;
        private DarkUI.Controls.DarkButton butCancel;
        private DarkUI.Controls.DarkButton butExportSelectedMesh;
        private Controls.PanelRenderingSkeleton panelRendering;
        private DarkUI.Controls.DarkButton butSetToAll;
        private DarkUI.Controls.DarkLabel darkLabel1;
        private DarkUI.Controls.DarkComboBox comboLightType;
        private DarkUI.Controls.DarkButton butEditMesh;
        private System.Windows.Forms.ToolTip toolTip;
    }
}