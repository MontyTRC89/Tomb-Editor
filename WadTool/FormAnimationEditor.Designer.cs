using TombLib.Controls;
using TombLib.Wad;

namespace WadTool
{
    partial class FormAnimationEditor
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormAnimationEditor));
            this.topMenu = new DarkUI.Controls.DarkMenuStrip();
            this.fileeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.saveChangesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem3 = new System.Windows.Forms.ToolStripSeparator();
            this.closeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.animationToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.addNewToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.deleteToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.splitAnimationToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem5 = new System.Windows.Forms.ToolStripSeparator();
            this.curToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.copyToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.pasteToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.pasteReplaceToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem2 = new System.Windows.Forms.ToolStripSeparator();
            this.importToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.exportToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem7 = new System.Windows.Forms.ToolStripSeparator();
            this.calculateBoundingBoxForAllFramesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.frameToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.insertFrameAfterCurrentOneToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.insertnFramesAfterCurrentOneToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.deleteFrameToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem4 = new System.Windows.Forms.ToolStripSeparator();
            this.cutToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.copyToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.pasteToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.pasteReplaceToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem1 = new System.Windows.Forms.ToolStripSeparator();
            this.interpolateFramesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem6 = new System.Windows.Forms.ToolStripSeparator();
            this.calculateCollisionBoxForCurrentFrameToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.renderingToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.drawGridToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.drawGizmoToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.drawCollisionBoxToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.advancedToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.loadPrj2ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.darkStatusStrip1 = new DarkUI.Controls.DarkStatusStrip();
            this.statusFrame = new System.Windows.Forms.ToolStripStatusLabel();
            this.panelRight = new System.Windows.Forms.Panel();
            this.tbSearchByStateID = new DarkUI.Controls.DarkTextBox();
            this.darkButton1 = new DarkUI.Controls.DarkButton();
            this.tbLateralEndVelocity = new DarkUI.Controls.DarkTextBox();
            this.tbLateralStartVelocity = new DarkUI.Controls.DarkTextBox();
            this.tbStartVelocity = new DarkUI.Controls.DarkTextBox();
            this.tbEndVelocity = new DarkUI.Controls.DarkTextBox();
            this.darkLabel22 = new DarkUI.Controls.DarkLabel();
            this.darkLabel23 = new DarkUI.Controls.DarkLabel();
            this.darkLabel24 = new DarkUI.Controls.DarkLabel();
            this.darkLabel25 = new DarkUI.Controls.DarkLabel();
            this.butPlayAnimation = new DarkUI.Controls.DarkButton();
            this.darkLabel2 = new DarkUI.Controls.DarkLabel();
            this.tbStateId = new DarkUI.Controls.DarkTextBox();
            this.butDeleteFrame = new DarkUI.Controls.DarkButton();
            this.darkLabel7 = new DarkUI.Controls.DarkLabel();
            this.tbNextFrame = new DarkUI.Controls.DarkTextBox();
            this.darkLabel6 = new DarkUI.Controls.DarkLabel();
            this.tbNextAnimation = new DarkUI.Controls.DarkTextBox();
            this.darkLabel5 = new DarkUI.Controls.DarkLabel();
            this.tbFramerate = new DarkUI.Controls.DarkTextBox();
            this.darkLabel4 = new DarkUI.Controls.DarkLabel();
            this.tbName = new DarkUI.Controls.DarkTextBox();
            this.darkLabel3 = new DarkUI.Controls.DarkLabel();
            this.butAddNewAnimation = new DarkUI.Controls.DarkButton();
            this.butEditAnimCommands = new DarkUI.Controls.DarkButton();
            this.butCalculateCollisionBox = new DarkUI.Controls.DarkButton();
            this.tbCollisionBoxMaxZ = new DarkUI.Controls.DarkTextBox();
            this.darkLabel8 = new DarkUI.Controls.DarkLabel();
            this.tbCollisionBoxMaxY = new DarkUI.Controls.DarkTextBox();
            this.darkLabel9 = new DarkUI.Controls.DarkLabel();
            this.tbCollisionBoxMaxX = new DarkUI.Controls.DarkTextBox();
            this.darkLabel10 = new DarkUI.Controls.DarkLabel();
            this.tbCollisionBoxMinZ = new DarkUI.Controls.DarkTextBox();
            this.darkLabel11 = new DarkUI.Controls.DarkLabel();
            this.tbCollisionBoxMinY = new DarkUI.Controls.DarkTextBox();
            this.darkLabel12 = new DarkUI.Controls.DarkLabel();
            this.tbCollisionBoxMinX = new DarkUI.Controls.DarkTextBox();
            this.darkLabel13 = new DarkUI.Controls.DarkLabel();
            this.butEditStateChanges = new DarkUI.Controls.DarkButton();
            this.butDeleteAnimation = new DarkUI.Controls.DarkButton();
            this.treeAnimations = new DarkUI.Controls.DarkTreeView();
            this.panel1 = new System.Windows.Forms.Panel();
            this.trackFrames = new System.Windows.Forms.TrackBar();
            this.topBar = new DarkUI.Controls.DarkToolStrip();
            this.toolStripButton1 = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripLabel1 = new System.Windows.Forms.ToolStripLabel();
            this.butTbAddAnimation = new System.Windows.Forms.ToolStripButton();
            this.butTbDeleteAnimation = new System.Windows.Forms.ToolStripButton();
            this.butTbCutAnimation = new System.Windows.Forms.ToolStripButton();
            this.butTbCopyAnimation = new System.Windows.Forms.ToolStripButton();
            this.butTbPasteAnimation = new System.Windows.Forms.ToolStripButton();
            this.butTbReplaceAnimation = new System.Windows.Forms.ToolStripButton();
            this.butTbSplitAnimation = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripLabel2 = new System.Windows.Forms.ToolStripLabel();
            this.butTbAddFrame = new System.Windows.Forms.ToolStripButton();
            this.butTbDeleteFrame = new System.Windows.Forms.ToolStripButton();
            this.butTbCutFrame = new System.Windows.Forms.ToolStripButton();
            this.butTbCopyFrame = new System.Windows.Forms.ToolStripButton();
            this.butTbPasteFrame = new System.Windows.Forms.ToolStripButton();
            this.butTbReplaceFrame = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripLabel3 = new System.Windows.Forms.ToolStripLabel();
            this.comboSkeleton = new System.Windows.Forms.ToolStripComboBox();
            this.toolStripLabel4 = new System.Windows.Forms.ToolStripLabel();
            this.comboRooms = new System.Windows.Forms.ToolStripComboBox();
            this.timerPlayAnimation = new System.Windows.Forms.Timer(this.components);
            this.openFileDialogImport = new System.Windows.Forms.OpenFileDialog();
            this.saveFileDialogExport = new System.Windows.Forms.SaveFileDialog();
            this.panelRendering = new WadTool.Controls.PanelRenderingAnimationEditor();
            this.panelInterpolate = new System.Windows.Forms.Panel();
            this.butInterpolateFrames = new DarkUI.Controls.DarkButton();
            this.butInterpolateSetCurrent2 = new DarkUI.Controls.DarkButton();
            this.butInterpolateSetCurrent1 = new DarkUI.Controls.DarkButton();
            this.darkLabel21 = new DarkUI.Controls.DarkLabel();
            this.tbInterpolateNumFrames = new DarkUI.Controls.DarkTextBox();
            this.darkLabel18 = new DarkUI.Controls.DarkLabel();
            this.tbInterpolateFrame2 = new DarkUI.Controls.DarkTextBox();
            this.darkLabel19 = new DarkUI.Controls.DarkLabel();
            this.tbInterpolateFrame1 = new DarkUI.Controls.DarkTextBox();
            this.darkLabel20 = new DarkUI.Controls.DarkLabel();
            this.tbLatAccel = new DarkUI.Controls.DarkTextBox();
            this.darkLabel14 = new DarkUI.Controls.DarkLabel();
            this.darkLabel17 = new DarkUI.Controls.DarkLabel();
            this.tbLatSpeed = new DarkUI.Controls.DarkTextBox();
            this.tbSpeed = new DarkUI.Controls.DarkTextBox();
            this.darkLabel15 = new DarkUI.Controls.DarkLabel();
            this.darkLabel16 = new DarkUI.Controls.DarkLabel();
            this.tbAccel = new DarkUI.Controls.DarkTextBox();
            this.openFileDialogPrj2 = new System.Windows.Forms.OpenFileDialog();
            this.butShowAll = new DarkUI.Controls.DarkButton();
            this.topMenu.SuspendLayout();
            this.darkStatusStrip1.SuspendLayout();
            this.panelRight.SuspendLayout();
            this.panel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.trackFrames)).BeginInit();
            this.topBar.SuspendLayout();
            this.panelRendering.SuspendLayout();
            this.panelInterpolate.SuspendLayout();
            this.SuspendLayout();
            // 
            // topMenu
            // 
            this.topMenu.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.topMenu.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.topMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileeToolStripMenuItem,
            this.animationToolStripMenuItem,
            this.frameToolStripMenuItem,
            this.renderingToolStripMenuItem,
            this.advancedToolStripMenuItem});
            this.topMenu.Location = new System.Drawing.Point(0, 0);
            this.topMenu.Name = "topMenu";
            this.topMenu.Padding = new System.Windows.Forms.Padding(3, 2, 0, 2);
            this.topMenu.Size = new System.Drawing.Size(1058, 24);
            this.topMenu.TabIndex = 0;
            this.topMenu.Text = "darkMenuStrip1";
            // 
            // fileeToolStripMenuItem
            // 
            this.fileeToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.fileeToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.saveChangesToolStripMenuItem,
            this.toolStripMenuItem3,
            this.closeToolStripMenuItem});
            this.fileeToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.fileeToolStripMenuItem.Name = "fileeToolStripMenuItem";
            this.fileeToolStripMenuItem.Size = new System.Drawing.Size(37, 20);
            this.fileeToolStripMenuItem.Text = "File";
            // 
            // saveChangesToolStripMenuItem
            // 
            this.saveChangesToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.saveChangesToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.saveChangesToolStripMenuItem.Image = global::WadTool.Properties.Resources.save_16;
            this.saveChangesToolStripMenuItem.Name = "saveChangesToolStripMenuItem";
            this.saveChangesToolStripMenuItem.Size = new System.Drawing.Size(145, 22);
            this.saveChangesToolStripMenuItem.Text = "Save changes";
            this.saveChangesToolStripMenuItem.Click += new System.EventHandler(this.saveChangesToolStripMenuItem_Click);
            // 
            // toolStripMenuItem3
            // 
            this.toolStripMenuItem3.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.toolStripMenuItem3.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.toolStripMenuItem3.Margin = new System.Windows.Forms.Padding(0, 0, 0, 1);
            this.toolStripMenuItem3.Name = "toolStripMenuItem3";
            this.toolStripMenuItem3.Size = new System.Drawing.Size(142, 6);
            // 
            // closeToolStripMenuItem
            // 
            this.closeToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.closeToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.closeToolStripMenuItem.Name = "closeToolStripMenuItem";
            this.closeToolStripMenuItem.Size = new System.Drawing.Size(145, 22);
            this.closeToolStripMenuItem.Text = "Close";
            this.closeToolStripMenuItem.Click += new System.EventHandler(this.closeToolStripMenuItem_Click);
            // 
            // animationToolStripMenuItem
            // 
            this.animationToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.animationToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.addNewToolStripMenuItem,
            this.deleteToolStripMenuItem,
            this.splitAnimationToolStripMenuItem,
            this.toolStripMenuItem5,
            this.curToolStripMenuItem,
            this.copyToolStripMenuItem1,
            this.pasteToolStripMenuItem1,
            this.pasteReplaceToolStripMenuItem1,
            this.toolStripMenuItem2,
            this.importToolStripMenuItem,
            this.exportToolStripMenuItem,
            this.toolStripMenuItem7,
            this.calculateBoundingBoxForAllFramesToolStripMenuItem});
            this.animationToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.animationToolStripMenuItem.Name = "animationToolStripMenuItem";
            this.animationToolStripMenuItem.Size = new System.Drawing.Size(75, 20);
            this.animationToolStripMenuItem.Text = "Animation";
            // 
            // addNewToolStripMenuItem
            // 
            this.addNewToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.addNewToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.addNewToolStripMenuItem.Image = global::WadTool.Properties.Resources.plus_math_16;
            this.addNewToolStripMenuItem.Name = "addNewToolStripMenuItem";
            this.addNewToolStripMenuItem.Size = new System.Drawing.Size(272, 22);
            this.addNewToolStripMenuItem.Text = "New animation";
            this.addNewToolStripMenuItem.Click += new System.EventHandler(this.addNewToolStripMenuItem_Click_1);
            // 
            // deleteToolStripMenuItem
            // 
            this.deleteToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.deleteToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.deleteToolStripMenuItem.Image = global::WadTool.Properties.Resources.trash_16;
            this.deleteToolStripMenuItem.Name = "deleteToolStripMenuItem";
            this.deleteToolStripMenuItem.Size = new System.Drawing.Size(272, 22);
            this.deleteToolStripMenuItem.Text = "Delete animation";
            this.deleteToolStripMenuItem.Click += new System.EventHandler(this.deleteToolStripMenuItem_Click);
            // 
            // splitAnimationToolStripMenuItem
            // 
            this.splitAnimationToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.splitAnimationToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.splitAnimationToolStripMenuItem.Image = global::WadTool.Properties.Resources.split_16;
            this.splitAnimationToolStripMenuItem.Name = "splitAnimationToolStripMenuItem";
            this.splitAnimationToolStripMenuItem.Size = new System.Drawing.Size(272, 22);
            this.splitAnimationToolStripMenuItem.Text = "Split animation";
            this.splitAnimationToolStripMenuItem.Click += new System.EventHandler(this.splitAnimationToolStripMenuItem_Click);
            // 
            // toolStripMenuItem5
            // 
            this.toolStripMenuItem5.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.toolStripMenuItem5.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.toolStripMenuItem5.Margin = new System.Windows.Forms.Padding(0, 0, 0, 1);
            this.toolStripMenuItem5.Name = "toolStripMenuItem5";
            this.toolStripMenuItem5.Size = new System.Drawing.Size(269, 6);
            // 
            // curToolStripMenuItem
            // 
            this.curToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.curToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.curToolStripMenuItem.Image = global::WadTool.Properties.Resources.cut_16;
            this.curToolStripMenuItem.Name = "curToolStripMenuItem";
            this.curToolStripMenuItem.Size = new System.Drawing.Size(272, 22);
            this.curToolStripMenuItem.Text = "Cut";
            this.curToolStripMenuItem.Click += new System.EventHandler(this.curToolStripMenuItem_Click);
            // 
            // copyToolStripMenuItem1
            // 
            this.copyToolStripMenuItem1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.copyToolStripMenuItem1.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.copyToolStripMenuItem1.Image = global::WadTool.Properties.Resources.copy_16;
            this.copyToolStripMenuItem1.Name = "copyToolStripMenuItem1";
            this.copyToolStripMenuItem1.Size = new System.Drawing.Size(272, 22);
            this.copyToolStripMenuItem1.Text = "Copy";
            this.copyToolStripMenuItem1.Click += new System.EventHandler(this.copyToolStripMenuItem1_Click);
            // 
            // pasteToolStripMenuItem1
            // 
            this.pasteToolStripMenuItem1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.pasteToolStripMenuItem1.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.pasteToolStripMenuItem1.Image = global::WadTool.Properties.Resources.paste_16;
            this.pasteToolStripMenuItem1.Name = "pasteToolStripMenuItem1";
            this.pasteToolStripMenuItem1.Size = new System.Drawing.Size(272, 22);
            this.pasteToolStripMenuItem1.Text = "Paste (Insert)";
            this.pasteToolStripMenuItem1.Click += new System.EventHandler(this.pasteToolStripMenuItem1_Click);
            // 
            // pasteReplaceToolStripMenuItem1
            // 
            this.pasteReplaceToolStripMenuItem1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.pasteReplaceToolStripMenuItem1.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.pasteReplaceToolStripMenuItem1.Image = global::WadTool.Properties.Resources.paste_special_16;
            this.pasteReplaceToolStripMenuItem1.Name = "pasteReplaceToolStripMenuItem1";
            this.pasteReplaceToolStripMenuItem1.Size = new System.Drawing.Size(272, 22);
            this.pasteReplaceToolStripMenuItem1.Text = "Paste (Replace)";
            this.pasteReplaceToolStripMenuItem1.Click += new System.EventHandler(this.pasteReplaceToolStripMenuItem1_Click);
            // 
            // toolStripMenuItem2
            // 
            this.toolStripMenuItem2.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.toolStripMenuItem2.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.toolStripMenuItem2.Margin = new System.Windows.Forms.Padding(0, 0, 0, 1);
            this.toolStripMenuItem2.Name = "toolStripMenuItem2";
            this.toolStripMenuItem2.Size = new System.Drawing.Size(269, 6);
            // 
            // importToolStripMenuItem
            // 
            this.importToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.importToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.importToolStripMenuItem.Image = global::WadTool.Properties.Resources.general_Import_16;
            this.importToolStripMenuItem.Name = "importToolStripMenuItem";
            this.importToolStripMenuItem.Size = new System.Drawing.Size(272, 22);
            this.importToolStripMenuItem.Text = "Import";
            this.importToolStripMenuItem.Click += new System.EventHandler(this.importToolStripMenuItem_Click);
            // 
            // exportToolStripMenuItem
            // 
            this.exportToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.exportToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.exportToolStripMenuItem.Image = global::WadTool.Properties.Resources.general_Export_16;
            this.exportToolStripMenuItem.Name = "exportToolStripMenuItem";
            this.exportToolStripMenuItem.Size = new System.Drawing.Size(272, 22);
            this.exportToolStripMenuItem.Text = "Export";
            this.exportToolStripMenuItem.Click += new System.EventHandler(this.exportToolStripMenuItem_Click);
            // 
            // toolStripMenuItem7
            // 
            this.toolStripMenuItem7.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.toolStripMenuItem7.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.toolStripMenuItem7.Margin = new System.Windows.Forms.Padding(0, 0, 0, 1);
            this.toolStripMenuItem7.Name = "toolStripMenuItem7";
            this.toolStripMenuItem7.Size = new System.Drawing.Size(269, 6);
            // 
            // calculateBoundingBoxForAllFramesToolStripMenuItem
            // 
            this.calculateBoundingBoxForAllFramesToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.calculateBoundingBoxForAllFramesToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.calculateBoundingBoxForAllFramesToolStripMenuItem.Image = global::WadTool.Properties.Resources.resize_16;
            this.calculateBoundingBoxForAllFramesToolStripMenuItem.Name = "calculateBoundingBoxForAllFramesToolStripMenuItem";
            this.calculateBoundingBoxForAllFramesToolStripMenuItem.Size = new System.Drawing.Size(272, 22);
            this.calculateBoundingBoxForAllFramesToolStripMenuItem.Text = "Calculate bounding box for all frames";
            this.calculateBoundingBoxForAllFramesToolStripMenuItem.Click += new System.EventHandler(this.calculateBoundingBoxForAllFramesToolStripMenuItem_Click);
            // 
            // frameToolStripMenuItem
            // 
            this.frameToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.frameToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.insertFrameAfterCurrentOneToolStripMenuItem,
            this.insertnFramesAfterCurrentOneToolStripMenuItem,
            this.deleteFrameToolStripMenuItem,
            this.toolStripMenuItem4,
            this.cutToolStripMenuItem,
            this.copyToolStripMenuItem,
            this.pasteToolStripMenuItem,
            this.pasteReplaceToolStripMenuItem,
            this.toolStripMenuItem1,
            this.interpolateFramesToolStripMenuItem,
            this.toolStripMenuItem6,
            this.calculateCollisionBoxForCurrentFrameToolStripMenuItem});
            this.frameToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.frameToolStripMenuItem.Name = "frameToolStripMenuItem";
            this.frameToolStripMenuItem.Size = new System.Drawing.Size(52, 20);
            this.frameToolStripMenuItem.Text = "Frame";
            // 
            // insertFrameAfterCurrentOneToolStripMenuItem
            // 
            this.insertFrameAfterCurrentOneToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.insertFrameAfterCurrentOneToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.insertFrameAfterCurrentOneToolStripMenuItem.Image = global::WadTool.Properties.Resources.plus_math_16;
            this.insertFrameAfterCurrentOneToolStripMenuItem.Name = "insertFrameAfterCurrentOneToolStripMenuItem";
            this.insertFrameAfterCurrentOneToolStripMenuItem.Size = new System.Drawing.Size(285, 22);
            this.insertFrameAfterCurrentOneToolStripMenuItem.Text = "Insert frame after current one";
            this.insertFrameAfterCurrentOneToolStripMenuItem.Click += new System.EventHandler(this.insertFrameAfterCurrentOneToolStripMenuItem_Click);
            // 
            // insertnFramesAfterCurrentOneToolStripMenuItem
            // 
            this.insertnFramesAfterCurrentOneToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.insertnFramesAfterCurrentOneToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.insertnFramesAfterCurrentOneToolStripMenuItem.Name = "insertnFramesAfterCurrentOneToolStripMenuItem";
            this.insertnFramesAfterCurrentOneToolStripMenuItem.Size = new System.Drawing.Size(285, 22);
            this.insertnFramesAfterCurrentOneToolStripMenuItem.Text = "Insert (n) frames after current one";
            this.insertnFramesAfterCurrentOneToolStripMenuItem.Click += new System.EventHandler(this.insertnFramesAfterCurrentOneToolStripMenuItem_Click);
            // 
            // deleteFrameToolStripMenuItem
            // 
            this.deleteFrameToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.deleteFrameToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.deleteFrameToolStripMenuItem.Image = global::WadTool.Properties.Resources.trash_16;
            this.deleteFrameToolStripMenuItem.Name = "deleteFrameToolStripMenuItem";
            this.deleteFrameToolStripMenuItem.Size = new System.Drawing.Size(285, 22);
            this.deleteFrameToolStripMenuItem.Text = "Delete frame";
            this.deleteFrameToolStripMenuItem.Click += new System.EventHandler(this.deleteFrameToolStripMenuItem_Click);
            // 
            // toolStripMenuItem4
            // 
            this.toolStripMenuItem4.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.toolStripMenuItem4.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.toolStripMenuItem4.Margin = new System.Windows.Forms.Padding(0, 0, 0, 1);
            this.toolStripMenuItem4.Name = "toolStripMenuItem4";
            this.toolStripMenuItem4.Size = new System.Drawing.Size(282, 6);
            // 
            // cutToolStripMenuItem
            // 
            this.cutToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.cutToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.cutToolStripMenuItem.Image = global::WadTool.Properties.Resources.cut_16;
            this.cutToolStripMenuItem.Name = "cutToolStripMenuItem";
            this.cutToolStripMenuItem.Size = new System.Drawing.Size(285, 22);
            this.cutToolStripMenuItem.Text = "Cut";
            this.cutToolStripMenuItem.Click += new System.EventHandler(this.cutToolStripMenuItem_Click);
            // 
            // copyToolStripMenuItem
            // 
            this.copyToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.copyToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.copyToolStripMenuItem.Image = global::WadTool.Properties.Resources.copy_16;
            this.copyToolStripMenuItem.Name = "copyToolStripMenuItem";
            this.copyToolStripMenuItem.Size = new System.Drawing.Size(285, 22);
            this.copyToolStripMenuItem.Text = "Copy";
            this.copyToolStripMenuItem.Click += new System.EventHandler(this.copyToolStripMenuItem_Click);
            // 
            // pasteToolStripMenuItem
            // 
            this.pasteToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.pasteToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.pasteToolStripMenuItem.Image = global::WadTool.Properties.Resources.paste_16;
            this.pasteToolStripMenuItem.Name = "pasteToolStripMenuItem";
            this.pasteToolStripMenuItem.Size = new System.Drawing.Size(285, 22);
            this.pasteToolStripMenuItem.Text = "Paste (Insert)";
            this.pasteToolStripMenuItem.Click += new System.EventHandler(this.pasteToolStripMenuItem_Click);
            // 
            // pasteReplaceToolStripMenuItem
            // 
            this.pasteReplaceToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.pasteReplaceToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.pasteReplaceToolStripMenuItem.Image = global::WadTool.Properties.Resources.paste_special_16;
            this.pasteReplaceToolStripMenuItem.Name = "pasteReplaceToolStripMenuItem";
            this.pasteReplaceToolStripMenuItem.Size = new System.Drawing.Size(285, 22);
            this.pasteReplaceToolStripMenuItem.Text = "Paste (Replace)";
            this.pasteReplaceToolStripMenuItem.Click += new System.EventHandler(this.pasteReplaceToolStripMenuItem_Click);
            // 
            // toolStripMenuItem1
            // 
            this.toolStripMenuItem1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.toolStripMenuItem1.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.toolStripMenuItem1.Margin = new System.Windows.Forms.Padding(0, 0, 0, 1);
            this.toolStripMenuItem1.Name = "toolStripMenuItem1";
            this.toolStripMenuItem1.Size = new System.Drawing.Size(282, 6);
            // 
            // interpolateFramesToolStripMenuItem
            // 
            this.interpolateFramesToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.interpolateFramesToolStripMenuItem.Checked = true;
            this.interpolateFramesToolStripMenuItem.CheckOnClick = true;
            this.interpolateFramesToolStripMenuItem.CheckState = System.Windows.Forms.CheckState.Checked;
            this.interpolateFramesToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.interpolateFramesToolStripMenuItem.Name = "interpolateFramesToolStripMenuItem";
            this.interpolateFramesToolStripMenuItem.Size = new System.Drawing.Size(285, 22);
            this.interpolateFramesToolStripMenuItem.Text = "Interpolate frames";
            this.interpolateFramesToolStripMenuItem.Click += new System.EventHandler(this.interpolateFramesToolStripMenuItem_Click);
            // 
            // toolStripMenuItem6
            // 
            this.toolStripMenuItem6.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.toolStripMenuItem6.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.toolStripMenuItem6.Margin = new System.Windows.Forms.Padding(0, 0, 0, 1);
            this.toolStripMenuItem6.Name = "toolStripMenuItem6";
            this.toolStripMenuItem6.Size = new System.Drawing.Size(282, 6);
            // 
            // calculateCollisionBoxForCurrentFrameToolStripMenuItem
            // 
            this.calculateCollisionBoxForCurrentFrameToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.calculateCollisionBoxForCurrentFrameToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.calculateCollisionBoxForCurrentFrameToolStripMenuItem.Image = global::WadTool.Properties.Resources.resize_16;
            this.calculateCollisionBoxForCurrentFrameToolStripMenuItem.Name = "calculateCollisionBoxForCurrentFrameToolStripMenuItem";
            this.calculateCollisionBoxForCurrentFrameToolStripMenuItem.Size = new System.Drawing.Size(285, 22);
            this.calculateCollisionBoxForCurrentFrameToolStripMenuItem.Text = "Calculate collision box for current frame";
            this.calculateCollisionBoxForCurrentFrameToolStripMenuItem.Click += new System.EventHandler(this.calculateCollisionBoxForCurrentFrameToolStripMenuItem_Click);
            // 
            // renderingToolStripMenuItem
            // 
            this.renderingToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.renderingToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.drawGridToolStripMenuItem,
            this.drawGizmoToolStripMenuItem,
            this.drawCollisionBoxToolStripMenuItem});
            this.renderingToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.renderingToolStripMenuItem.Name = "renderingToolStripMenuItem";
            this.renderingToolStripMenuItem.Size = new System.Drawing.Size(73, 20);
            this.renderingToolStripMenuItem.Text = "Rendering";
            // 
            // drawGridToolStripMenuItem
            // 
            this.drawGridToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.drawGridToolStripMenuItem.Checked = true;
            this.drawGridToolStripMenuItem.CheckOnClick = true;
            this.drawGridToolStripMenuItem.CheckState = System.Windows.Forms.CheckState.Checked;
            this.drawGridToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.drawGridToolStripMenuItem.Name = "drawGridToolStripMenuItem";
            this.drawGridToolStripMenuItem.Size = new System.Drawing.Size(170, 22);
            this.drawGridToolStripMenuItem.Text = "Draw grid";
            this.drawGridToolStripMenuItem.Click += new System.EventHandler(this.drawGridToolStripMenuItem_Click);
            // 
            // drawGizmoToolStripMenuItem
            // 
            this.drawGizmoToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.drawGizmoToolStripMenuItem.Checked = true;
            this.drawGizmoToolStripMenuItem.CheckOnClick = true;
            this.drawGizmoToolStripMenuItem.CheckState = System.Windows.Forms.CheckState.Checked;
            this.drawGizmoToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.drawGizmoToolStripMenuItem.Name = "drawGizmoToolStripMenuItem";
            this.drawGizmoToolStripMenuItem.Size = new System.Drawing.Size(170, 22);
            this.drawGizmoToolStripMenuItem.Text = "Draw gizmo";
            this.drawGizmoToolStripMenuItem.Click += new System.EventHandler(this.drawGizmoToolStripMenuItem_Click);
            // 
            // drawCollisionBoxToolStripMenuItem
            // 
            this.drawCollisionBoxToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.drawCollisionBoxToolStripMenuItem.Checked = true;
            this.drawCollisionBoxToolStripMenuItem.CheckOnClick = true;
            this.drawCollisionBoxToolStripMenuItem.CheckState = System.Windows.Forms.CheckState.Checked;
            this.drawCollisionBoxToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.drawCollisionBoxToolStripMenuItem.Name = "drawCollisionBoxToolStripMenuItem";
            this.drawCollisionBoxToolStripMenuItem.Size = new System.Drawing.Size(170, 22);
            this.drawCollisionBoxToolStripMenuItem.Text = "Draw collision box";
            this.drawCollisionBoxToolStripMenuItem.Click += new System.EventHandler(this.drawCollisionBoxToolStripMenuItem_Click);
            // 
            // advancedToolStripMenuItem
            // 
            this.advancedToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.advancedToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.loadPrj2ToolStripMenuItem});
            this.advancedToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.advancedToolStripMenuItem.Name = "advancedToolStripMenuItem";
            this.advancedToolStripMenuItem.Size = new System.Drawing.Size(72, 20);
            this.advancedToolStripMenuItem.Text = "Advanced";
            // 
            // loadPrj2ToolStripMenuItem
            // 
            this.loadPrj2ToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.loadPrj2ToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.loadPrj2ToolStripMenuItem.Name = "loadPrj2ToolStripMenuItem";
            this.loadPrj2ToolStripMenuItem.Size = new System.Drawing.Size(123, 22);
            this.loadPrj2ToolStripMenuItem.Text = "Load Prj2";
            this.loadPrj2ToolStripMenuItem.Click += new System.EventHandler(this.loadPrj2ToolStripMenuItem_Click);
            // 
            // darkStatusStrip1
            // 
            this.darkStatusStrip1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.darkStatusStrip1.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.darkStatusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.statusFrame});
            this.darkStatusStrip1.Location = new System.Drawing.Point(0, 705);
            this.darkStatusStrip1.Name = "darkStatusStrip1";
            this.darkStatusStrip1.Padding = new System.Windows.Forms.Padding(0, 5, 0, 3);
            this.darkStatusStrip1.Size = new System.Drawing.Size(1058, 36);
            this.darkStatusStrip1.TabIndex = 1;
            this.darkStatusStrip1.Text = "darkStatusStrip1";
            // 
            // statusFrame
            // 
            this.statusFrame.Name = "statusFrame";
            this.statusFrame.Size = new System.Drawing.Size(40, 23);
            this.statusFrame.Text = "Frame";
            // 
            // panelRight
            // 
            this.panelRight.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panelRight.Controls.Add(this.butShowAll);
            this.panelRight.Controls.Add(this.tbSearchByStateID);
            this.panelRight.Controls.Add(this.darkButton1);
            this.panelRight.Controls.Add(this.tbLateralEndVelocity);
            this.panelRight.Controls.Add(this.tbLateralStartVelocity);
            this.panelRight.Controls.Add(this.tbStartVelocity);
            this.panelRight.Controls.Add(this.tbEndVelocity);
            this.panelRight.Controls.Add(this.darkLabel22);
            this.panelRight.Controls.Add(this.darkLabel23);
            this.panelRight.Controls.Add(this.darkLabel24);
            this.panelRight.Controls.Add(this.darkLabel25);
            this.panelRight.Controls.Add(this.butPlayAnimation);
            this.panelRight.Controls.Add(this.darkLabel2);
            this.panelRight.Controls.Add(this.tbStateId);
            this.panelRight.Controls.Add(this.butDeleteFrame);
            this.panelRight.Controls.Add(this.darkLabel7);
            this.panelRight.Controls.Add(this.tbNextFrame);
            this.panelRight.Controls.Add(this.darkLabel6);
            this.panelRight.Controls.Add(this.tbNextAnimation);
            this.panelRight.Controls.Add(this.darkLabel5);
            this.panelRight.Controls.Add(this.tbFramerate);
            this.panelRight.Controls.Add(this.darkLabel4);
            this.panelRight.Controls.Add(this.tbName);
            this.panelRight.Controls.Add(this.darkLabel3);
            this.panelRight.Controls.Add(this.butAddNewAnimation);
            this.panelRight.Controls.Add(this.butEditAnimCommands);
            this.panelRight.Controls.Add(this.butCalculateCollisionBox);
            this.panelRight.Controls.Add(this.tbCollisionBoxMaxZ);
            this.panelRight.Controls.Add(this.darkLabel8);
            this.panelRight.Controls.Add(this.tbCollisionBoxMaxY);
            this.panelRight.Controls.Add(this.darkLabel9);
            this.panelRight.Controls.Add(this.tbCollisionBoxMaxX);
            this.panelRight.Controls.Add(this.darkLabel10);
            this.panelRight.Controls.Add(this.tbCollisionBoxMinZ);
            this.panelRight.Controls.Add(this.darkLabel11);
            this.panelRight.Controls.Add(this.tbCollisionBoxMinY);
            this.panelRight.Controls.Add(this.darkLabel12);
            this.panelRight.Controls.Add(this.tbCollisionBoxMinX);
            this.panelRight.Controls.Add(this.darkLabel13);
            this.panelRight.Controls.Add(this.butEditStateChanges);
            this.panelRight.Controls.Add(this.butDeleteAnimation);
            this.panelRight.Controls.Add(this.treeAnimations);
            this.panelRight.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.panelRight.Location = new System.Drawing.Point(815, 27);
            this.panelRight.Name = "panelRight";
            this.panelRight.Size = new System.Drawing.Size(243, 673);
            this.panelRight.TabIndex = 2;
            // 
            // tbSearchByStateID
            // 
            this.tbSearchByStateID.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tbSearchByStateID.Location = new System.Drawing.Point(6, 3);
            this.tbSearchByStateID.Name = "tbSearchByStateID";
            this.tbSearchByStateID.Size = new System.Drawing.Size(33, 22);
            this.tbSearchByStateID.TabIndex = 123;
            // 
            // darkButton1
            // 
            this.darkButton1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.darkButton1.Image = global::WadTool.Properties.Resources.search_16;
            this.darkButton1.Location = new System.Drawing.Point(45, 3);
            this.darkButton1.Name = "darkButton1";
            this.darkButton1.Size = new System.Drawing.Size(109, 22);
            this.darkButton1.TabIndex = 122;
            this.darkButton1.Text = "Search State ID";
            this.darkButton1.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.darkButton1.Click += new System.EventHandler(this.butSearchByStateID_Click);
            // 
            // tbLateralEndVelocity
            // 
            this.tbLateralEndVelocity.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tbLateralEndVelocity.Location = new System.Drawing.Point(177, 443);
            this.tbLateralEndVelocity.Name = "tbLateralEndVelocity";
            this.tbLateralEndVelocity.Size = new System.Drawing.Size(59, 22);
            this.tbLateralEndVelocity.TabIndex = 121;
            this.tbLateralEndVelocity.Validated += new System.EventHandler(this.tbLateralEndVelocity_Validated);
            // 
            // tbLateralStartVelocity
            // 
            this.tbLateralStartVelocity.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tbLateralStartVelocity.Location = new System.Drawing.Point(177, 415);
            this.tbLateralStartVelocity.Name = "tbLateralStartVelocity";
            this.tbLateralStartVelocity.Size = new System.Drawing.Size(59, 22);
            this.tbLateralStartVelocity.TabIndex = 119;
            this.tbLateralStartVelocity.Validated += new System.EventHandler(this.tbLateralStartVelocity_Validated);
            // 
            // tbStartVelocity
            // 
            this.tbStartVelocity.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tbStartVelocity.Location = new System.Drawing.Point(177, 359);
            this.tbStartVelocity.Name = "tbStartVelocity";
            this.tbStartVelocity.Size = new System.Drawing.Size(58, 22);
            this.tbStartVelocity.TabIndex = 115;
            this.tbStartVelocity.Validated += new System.EventHandler(this.tbStartVelocity_Validated);
            // 
            // tbEndVelocity
            // 
            this.tbEndVelocity.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tbEndVelocity.Location = new System.Drawing.Point(177, 387);
            this.tbEndVelocity.Name = "tbEndVelocity";
            this.tbEndVelocity.Size = new System.Drawing.Size(59, 22);
            this.tbEndVelocity.TabIndex = 117;
            this.tbEndVelocity.Validated += new System.EventHandler(this.tbEndVelocity_Validated);
            // 
            // darkLabel22
            // 
            this.darkLabel22.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.darkLabel22.AutoSize = true;
            this.darkLabel22.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.darkLabel22.Location = new System.Drawing.Point(105, 445);
            this.darkLabel22.Name = "darkLabel22";
            this.darkLabel22.Size = new System.Drawing.Size(66, 13);
            this.darkLabel22.TabIndex = 120;
            this.darkLabel22.Text = "End lat. vel:";
            // 
            // darkLabel23
            // 
            this.darkLabel23.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.darkLabel23.AutoSize = true;
            this.darkLabel23.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.darkLabel23.Location = new System.Drawing.Point(105, 361);
            this.darkLabel23.Name = "darkLabel23";
            this.darkLabel23.Size = new System.Drawing.Size(75, 13);
            this.darkLabel23.TabIndex = 114;
            this.darkLabel23.Text = "Start velocity:";
            // 
            // darkLabel24
            // 
            this.darkLabel24.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.darkLabel24.AutoSize = true;
            this.darkLabel24.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.darkLabel24.Location = new System.Drawing.Point(105, 417);
            this.darkLabel24.Name = "darkLabel24";
            this.darkLabel24.Size = new System.Drawing.Size(70, 13);
            this.darkLabel24.TabIndex = 118;
            this.darkLabel24.Text = "Start lat. vel:";
            // 
            // darkLabel25
            // 
            this.darkLabel25.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.darkLabel25.AutoSize = true;
            this.darkLabel25.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.darkLabel25.Location = new System.Drawing.Point(105, 389);
            this.darkLabel25.Name = "darkLabel25";
            this.darkLabel25.Size = new System.Drawing.Size(71, 13);
            this.darkLabel25.TabIndex = 116;
            this.darkLabel25.Text = "End velocity:";
            // 
            // butPlayAnimation
            // 
            this.butPlayAnimation.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.butPlayAnimation.Image = global::WadTool.Properties.Resources.play_16;
            this.butPlayAnimation.Location = new System.Drawing.Point(95, 298);
            this.butPlayAnimation.Name = "butPlayAnimation";
            this.butPlayAnimation.Size = new System.Drawing.Size(59, 23);
            this.butPlayAnimation.TabIndex = 113;
            this.butPlayAnimation.Text = "Play";
            this.butPlayAnimation.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            // 
            // darkLabel2
            // 
            this.darkLabel2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.darkLabel2.AutoSize = true;
            this.darkLabel2.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.darkLabel2.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.darkLabel2.Location = new System.Drawing.Point(3, 515);
            this.darkLabel2.Name = "darkLabel2";
            this.darkLabel2.Size = new System.Drawing.Size(79, 13);
            this.darkLabel2.TabIndex = 104;
            this.darkLabel2.Text = "Current frame";
            // 
            // tbStateId
            // 
            this.tbStateId.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tbStateId.Location = new System.Drawing.Point(69, 443);
            this.tbStateId.Name = "tbStateId";
            this.tbStateId.Size = new System.Drawing.Size(30, 22);
            this.tbStateId.TabIndex = 103;
            this.tbStateId.Validated += new System.EventHandler(this.tbStateId_Validated);
            // 
            // butDeleteFrame
            // 
            this.butDeleteFrame.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.butDeleteFrame.Image = global::WadTool.Properties.Resources.trash_161;
            this.butDeleteFrame.Location = new System.Drawing.Point(164, 527);
            this.butDeleteFrame.Name = "butDeleteFrame";
            this.butDeleteFrame.Size = new System.Drawing.Size(72, 23);
            this.butDeleteFrame.TabIndex = 27;
            this.butDeleteFrame.Text = "Delete";
            this.butDeleteFrame.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.butDeleteFrame.Click += new System.EventHandler(this.butDeleteFrame_Click);
            // 
            // darkLabel7
            // 
            this.darkLabel7.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.darkLabel7.AutoSize = true;
            this.darkLabel7.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.darkLabel7.Location = new System.Drawing.Point(3, 445);
            this.darkLabel7.Name = "darkLabel7";
            this.darkLabel7.Size = new System.Drawing.Size(50, 13);
            this.darkLabel7.TabIndex = 102;
            this.darkLabel7.Text = "State ID:";
            // 
            // tbNextFrame
            // 
            this.tbNextFrame.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tbNextFrame.Location = new System.Drawing.Point(69, 415);
            this.tbNextFrame.Name = "tbNextFrame";
            this.tbNextFrame.Size = new System.Drawing.Size(30, 22);
            this.tbNextFrame.TabIndex = 101;
            this.tbNextFrame.Validated += new System.EventHandler(this.tbNextFrame_Validated);
            // 
            // darkLabel6
            // 
            this.darkLabel6.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.darkLabel6.AutoSize = true;
            this.darkLabel6.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.darkLabel6.Location = new System.Drawing.Point(3, 417);
            this.darkLabel6.Name = "darkLabel6";
            this.darkLabel6.Size = new System.Drawing.Size(65, 13);
            this.darkLabel6.TabIndex = 100;
            this.darkLabel6.Text = "Next frame:";
            // 
            // tbNextAnimation
            // 
            this.tbNextAnimation.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tbNextAnimation.Location = new System.Drawing.Point(69, 387);
            this.tbNextAnimation.Name = "tbNextAnimation";
            this.tbNextAnimation.Size = new System.Drawing.Size(30, 22);
            this.tbNextAnimation.TabIndex = 99;
            this.tbNextAnimation.Validated += new System.EventHandler(this.tbNextAnimation_Validated);
            // 
            // darkLabel5
            // 
            this.darkLabel5.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.darkLabel5.AutoSize = true;
            this.darkLabel5.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.darkLabel5.Location = new System.Drawing.Point(3, 389);
            this.darkLabel5.Name = "darkLabel5";
            this.darkLabel5.Size = new System.Drawing.Size(61, 13);
            this.darkLabel5.TabIndex = 98;
            this.darkLabel5.Text = "Next anim:";
            // 
            // tbFramerate
            // 
            this.tbFramerate.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tbFramerate.Location = new System.Drawing.Point(69, 359);
            this.tbFramerate.Name = "tbFramerate";
            this.tbFramerate.Size = new System.Drawing.Size(29, 22);
            this.tbFramerate.TabIndex = 97;
            this.tbFramerate.Validated += new System.EventHandler(this.tbFramerate_Validated);
            // 
            // darkLabel4
            // 
            this.darkLabel4.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.darkLabel4.AutoSize = true;
            this.darkLabel4.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.darkLabel4.Location = new System.Drawing.Point(3, 361);
            this.darkLabel4.Name = "darkLabel4";
            this.darkLabel4.Size = new System.Drawing.Size(61, 13);
            this.darkLabel4.TabIndex = 96;
            this.darkLabel4.Text = "Framerate:";
            // 
            // tbName
            // 
            this.tbName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tbName.Location = new System.Drawing.Point(69, 331);
            this.tbName.Name = "tbName";
            this.tbName.Size = new System.Drawing.Size(167, 22);
            this.tbName.TabIndex = 95;
            this.tbName.Validated += new System.EventHandler(this.tbName_Validated);
            // 
            // darkLabel3
            // 
            this.darkLabel3.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.darkLabel3.AutoSize = true;
            this.darkLabel3.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.darkLabel3.Location = new System.Drawing.Point(3, 333);
            this.darkLabel3.Name = "darkLabel3";
            this.darkLabel3.Size = new System.Drawing.Size(39, 13);
            this.darkLabel3.TabIndex = 94;
            this.darkLabel3.Text = "Name:";
            // 
            // butAddNewAnimation
            // 
            this.butAddNewAnimation.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.butAddNewAnimation.Image = global::WadTool.Properties.Resources.plus_math_16;
            this.butAddNewAnimation.Location = new System.Drawing.Point(6, 298);
            this.butAddNewAnimation.Name = "butAddNewAnimation";
            this.butAddNewAnimation.Size = new System.Drawing.Size(83, 23);
            this.butAddNewAnimation.TabIndex = 93;
            this.butAddNewAnimation.Text = "Add new";
            this.butAddNewAnimation.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.butAddNewAnimation.Click += new System.EventHandler(this.butAddNewAnimation_Click);
            // 
            // butEditAnimCommands
            // 
            this.butEditAnimCommands.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.butEditAnimCommands.Image = global::WadTool.Properties.Resources.anim_commands_16;
            this.butEditAnimCommands.Location = new System.Drawing.Point(116, 471);
            this.butEditAnimCommands.Name = "butEditAnimCommands";
            this.butEditAnimCommands.Size = new System.Drawing.Size(120, 23);
            this.butEditAnimCommands.TabIndex = 92;
            this.butEditAnimCommands.Text = "Anim commands";
            this.butEditAnimCommands.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.butEditAnimCommands.Click += new System.EventHandler(this.butEditAnimCommands_Click);
            // 
            // butCalculateCollisionBox
            // 
            this.butCalculateCollisionBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.butCalculateCollisionBox.Image = global::WadTool.Properties.Resources.resize_16;
            this.butCalculateCollisionBox.Location = new System.Drawing.Point(6, 647);
            this.butCalculateCollisionBox.Name = "butCalculateCollisionBox";
            this.butCalculateCollisionBox.Size = new System.Drawing.Size(230, 23);
            this.butCalculateCollisionBox.TabIndex = 91;
            this.butCalculateCollisionBox.Text = "Calculate collision box for current frame";
            this.butCalculateCollisionBox.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.butCalculateCollisionBox.Click += new System.EventHandler(this.butCalculateCollisionBox_Click);
            // 
            // tbCollisionBoxMaxZ
            // 
            this.tbCollisionBoxMaxZ.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tbCollisionBoxMaxZ.Location = new System.Drawing.Point(164, 616);
            this.tbCollisionBoxMaxZ.Name = "tbCollisionBoxMaxZ";
            this.tbCollisionBoxMaxZ.Size = new System.Drawing.Size(72, 22);
            this.tbCollisionBoxMaxZ.TabIndex = 90;
            this.tbCollisionBoxMaxZ.Validated += new System.EventHandler(this.tbCollisionBoxMaxZ_Validated);
            // 
            // darkLabel8
            // 
            this.darkLabel8.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.darkLabel8.AutoSize = true;
            this.darkLabel8.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.darkLabel8.Location = new System.Drawing.Point(161, 599);
            this.darkLabel8.Name = "darkLabel8";
            this.darkLabel8.Size = new System.Drawing.Size(39, 13);
            this.darkLabel8.TabIndex = 89;
            this.darkLabel8.Text = "Z max:";
            // 
            // tbCollisionBoxMaxY
            // 
            this.tbCollisionBoxMaxY.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tbCollisionBoxMaxY.Location = new System.Drawing.Point(85, 616);
            this.tbCollisionBoxMaxY.Name = "tbCollisionBoxMaxY";
            this.tbCollisionBoxMaxY.Size = new System.Drawing.Size(73, 22);
            this.tbCollisionBoxMaxY.TabIndex = 88;
            this.tbCollisionBoxMaxY.Validated += new System.EventHandler(this.tbCollisionBoxMaxY_Validated);
            // 
            // darkLabel9
            // 
            this.darkLabel9.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.darkLabel9.AutoSize = true;
            this.darkLabel9.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.darkLabel9.Location = new System.Drawing.Point(82, 599);
            this.darkLabel9.Name = "darkLabel9";
            this.darkLabel9.Size = new System.Drawing.Size(38, 13);
            this.darkLabel9.TabIndex = 87;
            this.darkLabel9.Text = "Y max:";
            // 
            // tbCollisionBoxMaxX
            // 
            this.tbCollisionBoxMaxX.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tbCollisionBoxMaxX.Location = new System.Drawing.Point(6, 616);
            this.tbCollisionBoxMaxX.Name = "tbCollisionBoxMaxX";
            this.tbCollisionBoxMaxX.Size = new System.Drawing.Size(73, 22);
            this.tbCollisionBoxMaxX.TabIndex = 86;
            this.tbCollisionBoxMaxX.Validated += new System.EventHandler(this.tbCollisionBoxMaxX_Validated);
            // 
            // darkLabel10
            // 
            this.darkLabel10.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.darkLabel10.AutoSize = true;
            this.darkLabel10.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.darkLabel10.Location = new System.Drawing.Point(3, 599);
            this.darkLabel10.Name = "darkLabel10";
            this.darkLabel10.Size = new System.Drawing.Size(39, 13);
            this.darkLabel10.TabIndex = 85;
            this.darkLabel10.Text = "X max:";
            // 
            // tbCollisionBoxMinZ
            // 
            this.tbCollisionBoxMinZ.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tbCollisionBoxMinZ.Location = new System.Drawing.Point(164, 568);
            this.tbCollisionBoxMinZ.Name = "tbCollisionBoxMinZ";
            this.tbCollisionBoxMinZ.Size = new System.Drawing.Size(72, 22);
            this.tbCollisionBoxMinZ.TabIndex = 84;
            this.tbCollisionBoxMinZ.Validated += new System.EventHandler(this.tbCollisionBoxMinZ_Validated);
            // 
            // darkLabel11
            // 
            this.darkLabel11.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.darkLabel11.AutoSize = true;
            this.darkLabel11.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.darkLabel11.Location = new System.Drawing.Point(161, 551);
            this.darkLabel11.Name = "darkLabel11";
            this.darkLabel11.Size = new System.Drawing.Size(38, 13);
            this.darkLabel11.TabIndex = 83;
            this.darkLabel11.Text = "Z min:";
            // 
            // tbCollisionBoxMinY
            // 
            this.tbCollisionBoxMinY.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tbCollisionBoxMinY.Location = new System.Drawing.Point(85, 568);
            this.tbCollisionBoxMinY.Name = "tbCollisionBoxMinY";
            this.tbCollisionBoxMinY.Size = new System.Drawing.Size(73, 22);
            this.tbCollisionBoxMinY.TabIndex = 82;
            this.tbCollisionBoxMinY.Validated += new System.EventHandler(this.tbCollisionBoxMinY_Validated);
            // 
            // darkLabel12
            // 
            this.darkLabel12.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.darkLabel12.AutoSize = true;
            this.darkLabel12.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.darkLabel12.Location = new System.Drawing.Point(82, 551);
            this.darkLabel12.Name = "darkLabel12";
            this.darkLabel12.Size = new System.Drawing.Size(37, 13);
            this.darkLabel12.TabIndex = 81;
            this.darkLabel12.Text = "Y min:";
            // 
            // tbCollisionBoxMinX
            // 
            this.tbCollisionBoxMinX.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tbCollisionBoxMinX.Location = new System.Drawing.Point(6, 568);
            this.tbCollisionBoxMinX.Name = "tbCollisionBoxMinX";
            this.tbCollisionBoxMinX.Size = new System.Drawing.Size(73, 22);
            this.tbCollisionBoxMinX.TabIndex = 80;
            this.tbCollisionBoxMinX.Validated += new System.EventHandler(this.tbCollisionBoxMinX_Validated);
            // 
            // darkLabel13
            // 
            this.darkLabel13.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.darkLabel13.AutoSize = true;
            this.darkLabel13.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.darkLabel13.Location = new System.Drawing.Point(3, 551);
            this.darkLabel13.Name = "darkLabel13";
            this.darkLabel13.Size = new System.Drawing.Size(38, 13);
            this.darkLabel13.TabIndex = 79;
            this.darkLabel13.Text = "X min:";
            // 
            // butEditStateChanges
            // 
            this.butEditStateChanges.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.butEditStateChanges.Image = global::WadTool.Properties.Resources.state_changes_16;
            this.butEditStateChanges.Location = new System.Drawing.Point(6, 471);
            this.butEditStateChanges.Name = "butEditStateChanges";
            this.butEditStateChanges.Size = new System.Drawing.Size(104, 23);
            this.butEditStateChanges.TabIndex = 28;
            this.butEditStateChanges.Text = "State changes";
            this.butEditStateChanges.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.butEditStateChanges.Click += new System.EventHandler(this.butEditStateChanges_Click);
            // 
            // butDeleteAnimation
            // 
            this.butDeleteAnimation.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.butDeleteAnimation.Image = global::WadTool.Properties.Resources.trash_161;
            this.butDeleteAnimation.Location = new System.Drawing.Point(160, 298);
            this.butDeleteAnimation.Name = "butDeleteAnimation";
            this.butDeleteAnimation.Size = new System.Drawing.Size(76, 23);
            this.butDeleteAnimation.TabIndex = 23;
            this.butDeleteAnimation.Text = "Delete";
            this.butDeleteAnimation.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.butDeleteAnimation.Click += new System.EventHandler(this.butDeleteAnimation_Click);
            // 
            // treeAnimations
            // 
            this.treeAnimations.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.treeAnimations.Location = new System.Drawing.Point(3, 27);
            this.treeAnimations.MaxDragChange = 20;
            this.treeAnimations.Name = "treeAnimations";
            this.treeAnimations.Size = new System.Drawing.Size(233, 265);
            this.treeAnimations.TabIndex = 0;
            this.treeAnimations.Text = "darkTreeView1";
            this.treeAnimations.Click += new System.EventHandler(this.treeAnimations_Click);
            // 
            // panel1
            // 
            this.panel1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panel1.Controls.Add(this.trackFrames);
            this.panel1.Location = new System.Drawing.Point(0, 635);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(812, 65);
            this.panel1.TabIndex = 5;
            // 
            // trackFrames
            // 
            this.trackFrames.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.trackFrames.Location = new System.Drawing.Point(3, 8);
            this.trackFrames.Name = "trackFrames";
            this.trackFrames.Size = new System.Drawing.Size(806, 45);
            this.trackFrames.TabIndex = 0;
            this.trackFrames.ValueChanged += new System.EventHandler(this.trackFrames_ValueChanged);
            // 
            // topBar
            // 
            this.topBar.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.topBar.AutoSize = false;
            this.topBar.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.topBar.Dock = System.Windows.Forms.DockStyle.None;
            this.topBar.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.topBar.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripButton1,
            this.toolStripSeparator1,
            this.toolStripLabel1,
            this.butTbAddAnimation,
            this.butTbDeleteAnimation,
            this.butTbCutAnimation,
            this.butTbCopyAnimation,
            this.butTbPasteAnimation,
            this.butTbReplaceAnimation,
            this.butTbSplitAnimation,
            this.toolStripSeparator2,
            this.toolStripLabel2,
            this.butTbAddFrame,
            this.butTbDeleteFrame,
            this.butTbCutFrame,
            this.butTbCopyFrame,
            this.butTbPasteFrame,
            this.butTbReplaceFrame,
            this.toolStripSeparator3,
            this.toolStripLabel3,
            this.comboSkeleton,
            this.toolStripLabel4,
            this.comboRooms});
            this.topBar.Location = new System.Drawing.Point(0, 28);
            this.topBar.Name = "topBar";
            this.topBar.Padding = new System.Windows.Forms.Padding(5, 0, 1, 0);
            this.topBar.Size = new System.Drawing.Size(809, 28);
            this.topBar.TabIndex = 6;
            this.topBar.Text = "darkToolStrip1";
            // 
            // toolStripButton1
            // 
            this.toolStripButton1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.toolStripButton1.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripButton1.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.toolStripButton1.Image = global::WadTool.Properties.Resources.save_16;
            this.toolStripButton1.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton1.Name = "toolStripButton1";
            this.toolStripButton1.Size = new System.Drawing.Size(23, 25);
            this.toolStripButton1.Text = "toolStripButton1";
            this.toolStripButton1.ToolTipText = "Save changes";
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.toolStripSeparator1.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.toolStripSeparator1.Margin = new System.Windows.Forms.Padding(0, 0, 2, 0);
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(6, 28);
            // 
            // toolStripLabel1
            // 
            this.toolStripLabel1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.toolStripLabel1.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.toolStripLabel1.Name = "toolStripLabel1";
            this.toolStripLabel1.Size = new System.Drawing.Size(44, 25);
            this.toolStripLabel1.Text = "Anims:";
            // 
            // butTbAddAnimation
            // 
            this.butTbAddAnimation.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.butTbAddAnimation.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.butTbAddAnimation.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.butTbAddAnimation.Image = global::WadTool.Properties.Resources.plus_math_16;
            this.butTbAddAnimation.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.butTbAddAnimation.Name = "butTbAddAnimation";
            this.butTbAddAnimation.Size = new System.Drawing.Size(23, 25);
            this.butTbAddAnimation.Text = "toolStripButton2";
            this.butTbAddAnimation.ToolTipText = "Add animation";
            this.butTbAddAnimation.Click += new System.EventHandler(this.butTbAddAnimation_Click);
            // 
            // butTbDeleteAnimation
            // 
            this.butTbDeleteAnimation.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.butTbDeleteAnimation.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.butTbDeleteAnimation.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.butTbDeleteAnimation.Image = global::WadTool.Properties.Resources.trash_16;
            this.butTbDeleteAnimation.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.butTbDeleteAnimation.Name = "butTbDeleteAnimation";
            this.butTbDeleteAnimation.Size = new System.Drawing.Size(23, 25);
            this.butTbDeleteAnimation.Text = "toolStripButton3";
            this.butTbDeleteAnimation.ToolTipText = "Delete animation";
            this.butTbDeleteAnimation.Click += new System.EventHandler(this.butTbDeleteAnimation_Click);
            // 
            // butTbCutAnimation
            // 
            this.butTbCutAnimation.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.butTbCutAnimation.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.butTbCutAnimation.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.butTbCutAnimation.Image = global::WadTool.Properties.Resources.cut_16;
            this.butTbCutAnimation.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.butTbCutAnimation.Name = "butTbCutAnimation";
            this.butTbCutAnimation.Size = new System.Drawing.Size(23, 25);
            this.butTbCutAnimation.Text = "toolStripButton8";
            this.butTbCutAnimation.ToolTipText = "Cut animation";
            this.butTbCutAnimation.Click += new System.EventHandler(this.butTbCutAnimation_Click);
            // 
            // butTbCopyAnimation
            // 
            this.butTbCopyAnimation.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.butTbCopyAnimation.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.butTbCopyAnimation.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.butTbCopyAnimation.Image = global::WadTool.Properties.Resources.copy_16;
            this.butTbCopyAnimation.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.butTbCopyAnimation.Name = "butTbCopyAnimation";
            this.butTbCopyAnimation.Size = new System.Drawing.Size(23, 25);
            this.butTbCopyAnimation.Text = "toolStripButton4";
            this.butTbCopyAnimation.ToolTipText = "Copy animation";
            this.butTbCopyAnimation.Click += new System.EventHandler(this.butTbCopyAnimation_Click);
            // 
            // butTbPasteAnimation
            // 
            this.butTbPasteAnimation.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.butTbPasteAnimation.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.butTbPasteAnimation.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.butTbPasteAnimation.Image = global::WadTool.Properties.Resources.paste_16;
            this.butTbPasteAnimation.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.butTbPasteAnimation.Name = "butTbPasteAnimation";
            this.butTbPasteAnimation.Size = new System.Drawing.Size(23, 25);
            this.butTbPasteAnimation.Text = "toolStripButton5";
            this.butTbPasteAnimation.ToolTipText = "Paste animation";
            this.butTbPasteAnimation.Click += new System.EventHandler(this.butTbPasteAnimation_Click);
            // 
            // butTbReplaceAnimation
            // 
            this.butTbReplaceAnimation.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.butTbReplaceAnimation.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.butTbReplaceAnimation.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.butTbReplaceAnimation.Image = global::WadTool.Properties.Resources.paste_special_16;
            this.butTbReplaceAnimation.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.butTbReplaceAnimation.Name = "butTbReplaceAnimation";
            this.butTbReplaceAnimation.Size = new System.Drawing.Size(23, 25);
            this.butTbReplaceAnimation.Text = "toolStripButton5";
            this.butTbReplaceAnimation.ToolTipText = "Replace animation";
            this.butTbReplaceAnimation.Click += new System.EventHandler(this.butTbReplaceAnimation_Click);
            // 
            // butTbSplitAnimation
            // 
            this.butTbSplitAnimation.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.butTbSplitAnimation.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.butTbSplitAnimation.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.butTbSplitAnimation.Image = global::WadTool.Properties.Resources.split_16;
            this.butTbSplitAnimation.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.butTbSplitAnimation.Name = "butTbSplitAnimation";
            this.butTbSplitAnimation.Size = new System.Drawing.Size(23, 25);
            this.butTbSplitAnimation.Text = "toolStripButton5";
            this.butTbSplitAnimation.ToolTipText = "Split animation";
            this.butTbSplitAnimation.Click += new System.EventHandler(this.butTbSplitAnimation_Click);
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.toolStripSeparator2.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.toolStripSeparator2.Margin = new System.Windows.Forms.Padding(0, 0, 2, 0);
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(6, 28);
            // 
            // toolStripLabel2
            // 
            this.toolStripLabel2.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.toolStripLabel2.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.toolStripLabel2.Name = "toolStripLabel2";
            this.toolStripLabel2.Size = new System.Drawing.Size(48, 25);
            this.toolStripLabel2.Text = "Frames:";
            // 
            // butTbAddFrame
            // 
            this.butTbAddFrame.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.butTbAddFrame.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.butTbAddFrame.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.butTbAddFrame.Image = global::WadTool.Properties.Resources.plus_math_16;
            this.butTbAddFrame.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.butTbAddFrame.Name = "butTbAddFrame";
            this.butTbAddFrame.Size = new System.Drawing.Size(23, 25);
            this.butTbAddFrame.Text = "toolStripButton2";
            this.butTbAddFrame.ToolTipText = "Add frame";
            this.butTbAddFrame.Click += new System.EventHandler(this.butTbAddFrame_Click);
            // 
            // butTbDeleteFrame
            // 
            this.butTbDeleteFrame.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.butTbDeleteFrame.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.butTbDeleteFrame.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.butTbDeleteFrame.Image = global::WadTool.Properties.Resources.trash_16;
            this.butTbDeleteFrame.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.butTbDeleteFrame.Name = "butTbDeleteFrame";
            this.butTbDeleteFrame.Size = new System.Drawing.Size(23, 25);
            this.butTbDeleteFrame.Text = "toolStripButton3";
            this.butTbDeleteFrame.ToolTipText = "Delete frame";
            this.butTbDeleteFrame.Click += new System.EventHandler(this.butTbDeleteFrame_Click);
            // 
            // butTbCutFrame
            // 
            this.butTbCutFrame.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.butTbCutFrame.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.butTbCutFrame.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.butTbCutFrame.Image = global::WadTool.Properties.Resources.cut_16;
            this.butTbCutFrame.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.butTbCutFrame.Name = "butTbCutFrame";
            this.butTbCutFrame.Size = new System.Drawing.Size(23, 25);
            this.butTbCutFrame.Text = "toolStripButton8";
            this.butTbCutFrame.ToolTipText = "Cut frame";
            this.butTbCutFrame.Click += new System.EventHandler(this.butTbCutFrame_Click);
            // 
            // butTbCopyFrame
            // 
            this.butTbCopyFrame.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.butTbCopyFrame.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.butTbCopyFrame.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.butTbCopyFrame.Image = global::WadTool.Properties.Resources.copy_16;
            this.butTbCopyFrame.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.butTbCopyFrame.Name = "butTbCopyFrame";
            this.butTbCopyFrame.Size = new System.Drawing.Size(23, 25);
            this.butTbCopyFrame.Text = "toolStripButton4";
            this.butTbCopyFrame.ToolTipText = "Copy frame";
            this.butTbCopyFrame.Click += new System.EventHandler(this.butTbCopyFrame_Click);
            // 
            // butTbPasteFrame
            // 
            this.butTbPasteFrame.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.butTbPasteFrame.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.butTbPasteFrame.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.butTbPasteFrame.Image = global::WadTool.Properties.Resources.paste_16;
            this.butTbPasteFrame.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.butTbPasteFrame.Name = "butTbPasteFrame";
            this.butTbPasteFrame.Size = new System.Drawing.Size(23, 25);
            this.butTbPasteFrame.Text = "toolStripButton5";
            this.butTbPasteFrame.ToolTipText = "Paste frame";
            this.butTbPasteFrame.Click += new System.EventHandler(this.butTbPasteFrame_Click);
            // 
            // butTbReplaceFrame
            // 
            this.butTbReplaceFrame.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.butTbReplaceFrame.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.butTbReplaceFrame.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.butTbReplaceFrame.Image = global::WadTool.Properties.Resources.paste_special_16;
            this.butTbReplaceFrame.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.butTbReplaceFrame.Name = "butTbReplaceFrame";
            this.butTbReplaceFrame.Size = new System.Drawing.Size(23, 25);
            this.butTbReplaceFrame.Text = "toolStripButton5";
            this.butTbReplaceFrame.ToolTipText = "Replace frame";
            this.butTbReplaceFrame.Click += new System.EventHandler(this.butTbReplaceFrame_Click);
            // 
            // toolStripSeparator3
            // 
            this.toolStripSeparator3.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.toolStripSeparator3.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.toolStripSeparator3.Margin = new System.Windows.Forms.Padding(0, 0, 2, 0);
            this.toolStripSeparator3.Name = "toolStripSeparator3";
            this.toolStripSeparator3.Size = new System.Drawing.Size(6, 28);
            // 
            // toolStripLabel3
            // 
            this.toolStripLabel3.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.toolStripLabel3.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.toolStripLabel3.Name = "toolStripLabel3";
            this.toolStripLabel3.Size = new System.Drawing.Size(37, 25);
            this.toolStripLabel3.Text = "Bone:";
            // 
            // comboSkeleton
            // 
            this.comboSkeleton.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.comboSkeleton.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboSkeleton.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.comboSkeleton.Name = "comboSkeleton";
            this.comboSkeleton.Size = new System.Drawing.Size(120, 28);
            this.comboSkeleton.SelectedIndexChanged += new System.EventHandler(this.comboSkeleton_SelectedIndexChanged);
            // 
            // toolStripLabel4
            // 
            this.toolStripLabel4.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.toolStripLabel4.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.toolStripLabel4.Name = "toolStripLabel4";
            this.toolStripLabel4.Size = new System.Drawing.Size(42, 25);
            this.toolStripLabel4.Text = "Room:";
            // 
            // comboRooms
            // 
            this.comboRooms.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.comboRooms.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboRooms.Enabled = false;
            this.comboRooms.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.comboRooms.Name = "comboRooms";
            this.comboRooms.Size = new System.Drawing.Size(120, 28);
            this.comboRooms.SelectedIndexChanged += new System.EventHandler(this.comboRooms_SelectedIndexChanged);
            // 
            // timerPlayAnimation
            // 
            this.timerPlayAnimation.Interval = 33;
            this.timerPlayAnimation.Tick += new System.EventHandler(this.timerPlayAnimation_Tick);
            // 
            // openFileDialogImport
            // 
            this.openFileDialogImport.Filter = "Wad Tool animation (*.anim)|*.anim";
            this.openFileDialogImport.Title = "Import animation";
            // 
            // saveFileDialogExport
            // 
            this.saveFileDialogExport.Filter = "Wad Tool animation (*.anim)|*.anim";
            this.saveFileDialogExport.Title = "Export animation";
            // 
            // panelRendering
            // 
            this.panelRendering.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panelRendering.Controls.Add(this.panelInterpolate);
            this.panelRendering.Controls.Add(this.tbLatAccel);
            this.panelRendering.Controls.Add(this.darkLabel14);
            this.panelRendering.Controls.Add(this.darkLabel17);
            this.panelRendering.Controls.Add(this.tbLatSpeed);
            this.panelRendering.Controls.Add(this.tbSpeed);
            this.panelRendering.Controls.Add(this.darkLabel15);
            this.panelRendering.Controls.Add(this.darkLabel16);
            this.panelRendering.Controls.Add(this.tbAccel);
            this.panelRendering.Location = new System.Drawing.Point(3, 54);
            this.panelRendering.Name = "panelRendering";
            this.panelRendering.Size = new System.Drawing.Size(809, 575);
            this.panelRendering.TabIndex = 4;
            // 
            // panelInterpolate
            // 
            this.panelInterpolate.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.panelInterpolate.Controls.Add(this.butInterpolateFrames);
            this.panelInterpolate.Controls.Add(this.butInterpolateSetCurrent2);
            this.panelInterpolate.Controls.Add(this.butInterpolateSetCurrent1);
            this.panelInterpolate.Controls.Add(this.darkLabel21);
            this.panelInterpolate.Controls.Add(this.tbInterpolateNumFrames);
            this.panelInterpolate.Controls.Add(this.darkLabel18);
            this.panelInterpolate.Controls.Add(this.tbInterpolateFrame2);
            this.panelInterpolate.Controls.Add(this.darkLabel19);
            this.panelInterpolate.Controls.Add(this.tbInterpolateFrame1);
            this.panelInterpolate.Controls.Add(this.darkLabel20);
            this.panelInterpolate.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.panelInterpolate.Location = new System.Drawing.Point(570, 5);
            this.panelInterpolate.Name = "panelInterpolate";
            this.panelInterpolate.Size = new System.Drawing.Size(236, 148);
            this.panelInterpolate.TabIndex = 0;
            // 
            // butInterpolateFrames
            // 
            this.butInterpolateFrames.Image = global::WadTool.Properties.Resources.interpolate_16;
            this.butInterpolateFrames.Location = new System.Drawing.Point(11, 112);
            this.butInterpolateFrames.Name = "butInterpolateFrames";
            this.butInterpolateFrames.Size = new System.Drawing.Size(213, 23);
            this.butInterpolateFrames.TabIndex = 96;
            this.butInterpolateFrames.Text = "Interpolate selected frames";
            this.butInterpolateFrames.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.butInterpolateFrames.Click += new System.EventHandler(this.butInterpolateFrames_Click);
            // 
            // butInterpolateSetCurrent2
            // 
            this.butInterpolateSetCurrent2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.butInterpolateSetCurrent2.Location = new System.Drawing.Point(139, 58);
            this.butInterpolateSetCurrent2.Name = "butInterpolateSetCurrent2";
            this.butInterpolateSetCurrent2.Size = new System.Drawing.Size(85, 22);
            this.butInterpolateSetCurrent2.TabIndex = 95;
            this.butInterpolateSetCurrent2.Text = "Set current";
            this.butInterpolateSetCurrent2.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.butInterpolateSetCurrent2.Click += new System.EventHandler(this.butInterpolateSetCurrent2_Click);
            // 
            // butInterpolateSetCurrent1
            // 
            this.butInterpolateSetCurrent1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.butInterpolateSetCurrent1.Location = new System.Drawing.Point(139, 30);
            this.butInterpolateSetCurrent1.Name = "butInterpolateSetCurrent1";
            this.butInterpolateSetCurrent1.Size = new System.Drawing.Size(85, 22);
            this.butInterpolateSetCurrent1.TabIndex = 94;
            this.butInterpolateSetCurrent1.Text = "Set current";
            this.butInterpolateSetCurrent1.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.butInterpolateSetCurrent1.Click += new System.EventHandler(this.butInterpolateSetCurrent1_Click);
            // 
            // darkLabel21
            // 
            this.darkLabel21.AutoSize = true;
            this.darkLabel21.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.darkLabel21.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.darkLabel21.Location = new System.Drawing.Point(8, 9);
            this.darkLabel21.Name = "darkLabel21";
            this.darkLabel21.Size = new System.Drawing.Size(102, 13);
            this.darkLabel21.TabIndex = 60;
            this.darkLabel21.Text = "Interpolate frames";
            // 
            // tbInterpolateNumFrames
            // 
            this.tbInterpolateNumFrames.Location = new System.Drawing.Point(73, 86);
            this.tbInterpolateNumFrames.Name = "tbInterpolateNumFrames";
            this.tbInterpolateNumFrames.Size = new System.Drawing.Size(60, 22);
            this.tbInterpolateNumFrames.TabIndex = 59;
            this.tbInterpolateNumFrames.Text = "0";
            // 
            // darkLabel18
            // 
            this.darkLabel18.AutoSize = true;
            this.darkLabel18.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.darkLabel18.Location = new System.Drawing.Point(8, 88);
            this.darkLabel18.Name = "darkLabel18";
            this.darkLabel18.Size = new System.Drawing.Size(58, 13);
            this.darkLabel18.TabIndex = 58;
            this.darkLabel18.Text = "N. frames:";
            // 
            // tbInterpolateFrame2
            // 
            this.tbInterpolateFrame2.Location = new System.Drawing.Point(73, 58);
            this.tbInterpolateFrame2.Name = "tbInterpolateFrame2";
            this.tbInterpolateFrame2.ReadOnly = true;
            this.tbInterpolateFrame2.Size = new System.Drawing.Size(60, 22);
            this.tbInterpolateFrame2.TabIndex = 57;
            this.tbInterpolateFrame2.Text = "0";
            // 
            // darkLabel19
            // 
            this.darkLabel19.AutoSize = true;
            this.darkLabel19.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.darkLabel19.Location = new System.Drawing.Point(8, 60);
            this.darkLabel19.Name = "darkLabel19";
            this.darkLabel19.Size = new System.Drawing.Size(50, 13);
            this.darkLabel19.TabIndex = 56;
            this.darkLabel19.Text = "Frame 2:";
            // 
            // tbInterpolateFrame1
            // 
            this.tbInterpolateFrame1.Location = new System.Drawing.Point(73, 30);
            this.tbInterpolateFrame1.Name = "tbInterpolateFrame1";
            this.tbInterpolateFrame1.ReadOnly = true;
            this.tbInterpolateFrame1.Size = new System.Drawing.Size(60, 22);
            this.tbInterpolateFrame1.TabIndex = 55;
            this.tbInterpolateFrame1.Text = "0";
            // 
            // darkLabel20
            // 
            this.darkLabel20.AutoSize = true;
            this.darkLabel20.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.darkLabel20.Location = new System.Drawing.Point(8, 32);
            this.darkLabel20.Name = "darkLabel20";
            this.darkLabel20.Size = new System.Drawing.Size(50, 13);
            this.darkLabel20.TabIndex = 54;
            this.darkLabel20.Text = "Frame 1:";
            // 
            // tbLatAccel
            // 
            this.tbLatAccel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tbLatAccel.Location = new System.Drawing.Point(77, 93);
            this.tbLatAccel.Name = "tbLatAccel";
            this.tbLatAccel.Size = new System.Drawing.Size(81, 20);
            this.tbLatAccel.TabIndex = 112;
            this.tbLatAccel.Visible = false;
            this.tbLatAccel.TextChanged += new System.EventHandler(this.tbLatAccel_TextChanged);
            // 
            // darkLabel14
            // 
            this.darkLabel14.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.darkLabel14.AutoSize = true;
            this.darkLabel14.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.darkLabel14.Location = new System.Drawing.Point(17, 95);
            this.darkLabel14.Name = "darkLabel14";
            this.darkLabel14.Size = new System.Drawing.Size(57, 13);
            this.darkLabel14.TabIndex = 111;
            this.darkLabel14.Text = "Lat. accel:";
            this.darkLabel14.Visible = false;
            // 
            // darkLabel17
            // 
            this.darkLabel17.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.darkLabel17.AutoSize = true;
            this.darkLabel17.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.darkLabel17.Location = new System.Drawing.Point(17, 11);
            this.darkLabel17.Name = "darkLabel17";
            this.darkLabel17.Size = new System.Drawing.Size(41, 13);
            this.darkLabel17.TabIndex = 105;
            this.darkLabel17.Text = "Speed:";
            this.darkLabel17.Visible = false;
            // 
            // tbLatSpeed
            // 
            this.tbLatSpeed.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tbLatSpeed.Location = new System.Drawing.Point(77, 65);
            this.tbLatSpeed.Name = "tbLatSpeed";
            this.tbLatSpeed.Size = new System.Drawing.Size(81, 20);
            this.tbLatSpeed.TabIndex = 110;
            this.tbLatSpeed.Visible = false;
            this.tbLatSpeed.Validated += new System.EventHandler(this.tbLatSpeed_Validated);
            // 
            // tbSpeed
            // 
            this.tbSpeed.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tbSpeed.Location = new System.Drawing.Point(77, 9);
            this.tbSpeed.Name = "tbSpeed";
            this.tbSpeed.Size = new System.Drawing.Size(80, 20);
            this.tbSpeed.TabIndex = 106;
            this.tbSpeed.Visible = false;
            this.tbSpeed.Validated += new System.EventHandler(this.tbSpeed_Validated);
            // 
            // darkLabel15
            // 
            this.darkLabel15.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.darkLabel15.AutoSize = true;
            this.darkLabel15.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.darkLabel15.Location = new System.Drawing.Point(17, 67);
            this.darkLabel15.Name = "darkLabel15";
            this.darkLabel15.Size = new System.Drawing.Size(60, 13);
            this.darkLabel15.TabIndex = 109;
            this.darkLabel15.Text = "Lat. speed:";
            this.darkLabel15.Visible = false;
            // 
            // darkLabel16
            // 
            this.darkLabel16.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.darkLabel16.AutoSize = true;
            this.darkLabel16.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.darkLabel16.Location = new System.Drawing.Point(17, 39);
            this.darkLabel16.Name = "darkLabel16";
            this.darkLabel16.Size = new System.Drawing.Size(37, 13);
            this.darkLabel16.TabIndex = 107;
            this.darkLabel16.Text = "Accel:";
            this.darkLabel16.Visible = false;
            // 
            // tbAccel
            // 
            this.tbAccel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tbAccel.Location = new System.Drawing.Point(77, 37);
            this.tbAccel.Name = "tbAccel";
            this.tbAccel.Size = new System.Drawing.Size(81, 20);
            this.tbAccel.TabIndex = 108;
            this.tbAccel.Visible = false;
            this.tbAccel.Validated += new System.EventHandler(this.tbAccel_Validated);
            // 
            // openFileDialogPrj2
            // 
            this.openFileDialogPrj2.Filter = "Tomb Editor Project (*.prj2)|*.prj2";
            this.openFileDialogPrj2.Title = "Open Prj2";
            // 
            // butShowAll
            // 
            this.butShowAll.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.butShowAll.Image = global::WadTool.Properties.Resources.angle_right_16;
            this.butShowAll.Location = new System.Drawing.Point(160, 3);
            this.butShowAll.Name = "butShowAll";
            this.butShowAll.Size = new System.Drawing.Size(75, 22);
            this.butShowAll.TabIndex = 124;
            this.butShowAll.Text = "Show all";
            this.butShowAll.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.butShowAll.Click += new System.EventHandler(this.butShowAll_Click);
            // 
            // FormAnimationEditor
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1058, 741);
            this.Controls.Add(this.topBar);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.panelRendering);
            this.Controls.Add(this.panelRight);
            this.Controls.Add(this.darkStatusStrip1);
            this.Controls.Add(this.topMenu);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MainMenuStrip = this.topMenu;
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(1024, 768);
            this.Name = "FormAnimationEditor";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Animation editor";
            this.topMenu.ResumeLayout(false);
            this.topMenu.PerformLayout();
            this.darkStatusStrip1.ResumeLayout(false);
            this.darkStatusStrip1.PerformLayout();
            this.panelRight.ResumeLayout(false);
            this.panelRight.PerformLayout();
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.trackFrames)).EndInit();
            this.topBar.ResumeLayout(false);
            this.topBar.PerformLayout();
            this.panelRendering.ResumeLayout(false);
            this.panelRendering.PerformLayout();
            this.panelInterpolate.ResumeLayout(false);
            this.panelInterpolate.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private DarkUI.Controls.DarkMenuStrip topMenu;
        private System.Windows.Forms.ToolStripMenuItem fileeToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem saveChangesToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuItem3;
        private System.Windows.Forms.ToolStripMenuItem closeToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem frameToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem insertFrameAfterCurrentOneToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem deleteFrameToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuItem4;
        private System.Windows.Forms.ToolStripMenuItem cutToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem copyToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem pasteToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem pasteReplaceToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem interpolateFramesToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem animationToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem addNewToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem deleteToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuItem5;
        private System.Windows.Forms.ToolStripMenuItem copyToolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem pasteToolStripMenuItem1;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuItem2;
        private System.Windows.Forms.ToolStripMenuItem importToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem exportToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem renderingToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem drawGridToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem drawGizmoToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem drawCollisionBoxToolStripMenuItem;
        private DarkUI.Controls.DarkStatusStrip darkStatusStrip1;
        private System.Windows.Forms.Panel panelRight;
        private DarkUI.Controls.DarkTreeView treeAnimations;
        private Controls.PanelRenderingAnimationEditor panelRendering;
        private DarkUI.Controls.DarkButton butDeleteAnimation;
        private DarkUI.Controls.DarkButton butEditStateChanges;
        private DarkUI.Controls.DarkButton butDeleteFrame;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.TrackBar trackFrames;
        private DarkUI.Controls.DarkButton butCalculateCollisionBox;
        private DarkUI.Controls.DarkTextBox tbCollisionBoxMaxZ;
        private DarkUI.Controls.DarkLabel darkLabel8;
        private DarkUI.Controls.DarkTextBox tbCollisionBoxMaxY;
        private DarkUI.Controls.DarkLabel darkLabel9;
        private DarkUI.Controls.DarkTextBox tbCollisionBoxMaxX;
        private DarkUI.Controls.DarkLabel darkLabel10;
        private DarkUI.Controls.DarkTextBox tbCollisionBoxMinZ;
        private DarkUI.Controls.DarkLabel darkLabel11;
        private DarkUI.Controls.DarkTextBox tbCollisionBoxMinY;
        private DarkUI.Controls.DarkLabel darkLabel12;
        private DarkUI.Controls.DarkTextBox tbCollisionBoxMinX;
        private DarkUI.Controls.DarkLabel darkLabel13;
        private System.Windows.Forms.ToolStripStatusLabel statusFrame;
        private DarkUI.Controls.DarkButton butEditAnimCommands;
        private DarkUI.Controls.DarkButton butAddNewAnimation;
        private DarkUI.Controls.DarkTextBox tbFramerate;
        private DarkUI.Controls.DarkLabel darkLabel4;
        private DarkUI.Controls.DarkTextBox tbName;
        private DarkUI.Controls.DarkLabel darkLabel3;
        private DarkUI.Controls.DarkTextBox tbStateId;
        private DarkUI.Controls.DarkLabel darkLabel7;
        private DarkUI.Controls.DarkTextBox tbNextFrame;
        private DarkUI.Controls.DarkLabel darkLabel6;
        private DarkUI.Controls.DarkTextBox tbNextAnimation;
        private DarkUI.Controls.DarkLabel darkLabel5;
        private DarkUI.Controls.DarkLabel darkLabel2;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuItem6;
        private System.Windows.Forms.ToolStripMenuItem calculateCollisionBoxForCurrentFrameToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuItem7;
        private System.Windows.Forms.ToolStripMenuItem calculateBoundingBoxForAllFramesToolStripMenuItem;
        private DarkUI.Controls.DarkToolStrip topBar;
        private System.Windows.Forms.ToolStripButton toolStripButton1;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripLabel toolStripLabel1;
        private System.Windows.Forms.ToolStripButton butTbAddAnimation;
        private System.Windows.Forms.ToolStripButton butTbDeleteAnimation;
        private System.Windows.Forms.ToolStripButton butTbCopyAnimation;
        private System.Windows.Forms.ToolStripButton butTbPasteAnimation;
        private System.Windows.Forms.ToolStripButton butTbCutFrame;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolStripLabel toolStripLabel2;
        private System.Windows.Forms.ToolStripButton butTbAddFrame;
        private System.Windows.Forms.ToolStripButton butTbDeleteFrame;
        private System.Windows.Forms.ToolStripButton butTbCopyFrame;
        private System.Windows.Forms.ToolStripButton butTbPasteFrame;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator3;
        private System.Windows.Forms.ToolStripLabel toolStripLabel3;
        private System.Windows.Forms.ToolStripComboBox comboSkeleton;
        private System.Windows.Forms.ToolStripMenuItem insertnFramesAfterCurrentOneToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem pasteReplaceToolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem curToolStripMenuItem;
        private System.Windows.Forms.ToolStripButton butTbCutAnimation;
        private DarkUI.Controls.DarkTextBox tbLatAccel;
        private DarkUI.Controls.DarkLabel darkLabel14;
        private DarkUI.Controls.DarkTextBox tbLatSpeed;
        private DarkUI.Controls.DarkLabel darkLabel15;
        private DarkUI.Controls.DarkTextBox tbAccel;
        private DarkUI.Controls.DarkLabel darkLabel16;
        private DarkUI.Controls.DarkTextBox tbSpeed;
        private DarkUI.Controls.DarkLabel darkLabel17;
        private System.Windows.Forms.Panel panelInterpolate;
        private DarkUI.Controls.DarkButton butInterpolateFrames;
        private DarkUI.Controls.DarkButton butInterpolateSetCurrent2;
        private DarkUI.Controls.DarkButton butInterpolateSetCurrent1;
        private DarkUI.Controls.DarkLabel darkLabel21;
        private DarkUI.Controls.DarkTextBox tbInterpolateNumFrames;
        private DarkUI.Controls.DarkLabel darkLabel18;
        private DarkUI.Controls.DarkTextBox tbInterpolateFrame2;
        private DarkUI.Controls.DarkLabel darkLabel19;
        private DarkUI.Controls.DarkTextBox tbInterpolateFrame1;
        private DarkUI.Controls.DarkLabel darkLabel20;
        private DarkUI.Controls.DarkButton butPlayAnimation;
        private System.Windows.Forms.Timer timerPlayAnimation;
        private DarkUI.Controls.DarkTextBox tbLateralEndVelocity;
        private DarkUI.Controls.DarkTextBox tbLateralStartVelocity;
        private DarkUI.Controls.DarkTextBox tbStartVelocity;
        private DarkUI.Controls.DarkTextBox tbEndVelocity;
        private DarkUI.Controls.DarkLabel darkLabel22;
        private DarkUI.Controls.DarkLabel darkLabel23;
        private DarkUI.Controls.DarkLabel darkLabel24;
        private DarkUI.Controls.DarkLabel darkLabel25;
        private System.Windows.Forms.ToolStripButton butTbReplaceAnimation;
        private System.Windows.Forms.ToolStripButton butTbSplitAnimation;
        private System.Windows.Forms.ToolStripButton butTbReplaceFrame;
        private System.Windows.Forms.ToolStripMenuItem splitAnimationToolStripMenuItem;
        private System.Windows.Forms.OpenFileDialog openFileDialogImport;
        private System.Windows.Forms.SaveFileDialog saveFileDialogExport;
        private System.Windows.Forms.ToolStripMenuItem advancedToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem loadPrj2ToolStripMenuItem;
        private System.Windows.Forms.ToolStripLabel toolStripLabel4;
        private System.Windows.Forms.ToolStripComboBox comboRooms;
        private System.Windows.Forms.OpenFileDialog openFileDialogPrj2;
        private DarkUI.Controls.DarkTextBox tbSearchByStateID;
        private DarkUI.Controls.DarkButton darkButton1;
        private DarkUI.Controls.DarkButton butShowAll;
    }
}