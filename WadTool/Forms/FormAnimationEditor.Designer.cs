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
            this.undoToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.redoToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
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
            this.toolStripMenuItem2 = new System.Windows.Forms.ToolStripSeparator();
            this.importToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.exportToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem7 = new System.Windows.Forms.ToolStripSeparator();
            this.findReplaceAnimcommandsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.calculateBoundingBoxForAllFramesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.deleteBoundingBoxForAllFramesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.resampleAnimationToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.resampleAnimationToKeyframesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.frameToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.insertFrameAfterCurrentOneToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.insertnFramesAfterCurrentOneToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.deleteFrameToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.deleteEveryNthFrameToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem4 = new System.Windows.Forms.ToolStripSeparator();
            this.cutToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.copyToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.pasteToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem1 = new System.Windows.Forms.ToolStripSeparator();
            this.interpolateFramesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem6 = new System.Windows.Forms.ToolStripSeparator();
            this.calculateCollisionBoxForCurrentFrameToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.deleteCollisionBoxForCurrentFrameToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.renderingToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.drawGizmoToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.drawGridToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.drawCollisionBoxToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.smoothAnimationsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.darkStatusStrip1 = new DarkUI.Controls.DarkStatusStrip();
            this.statusFrame = new System.Windows.Forms.ToolStripStatusLabel();
            this.darkLabel22 = new DarkUI.Controls.DarkLabel();
            this.darkLabel23 = new DarkUI.Controls.DarkLabel();
            this.darkLabel24 = new DarkUI.Controls.DarkLabel();
            this.darkLabel25 = new DarkUI.Controls.DarkLabel();
            this.tbStateId = new TombLib.Controls.DarkAutocompleteTextBox();
            this.darkLabel7 = new DarkUI.Controls.DarkLabel();
            this.darkLabel6 = new DarkUI.Controls.DarkLabel();
            this.darkLabel5 = new DarkUI.Controls.DarkLabel();
            this.darkLabel4 = new DarkUI.Controls.DarkLabel();
            this.tbName = new DarkUI.Controls.DarkTextBox();
            this.darkLabel3 = new DarkUI.Controls.DarkLabel();
            this.tbSearchAnimation = new DarkUI.Controls.DarkTextBox();
            this.topBar = new DarkUI.Controls.DarkToolStrip();
            this.butTbSaveAllChanges = new System.Windows.Forms.ToolStripButton();
            this.butTbUndo = new System.Windows.Forms.ToolStripButton();
            this.butTbRedo = new System.Windows.Forms.ToolStripButton();
            this.butTbResetCamera = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.labelAnims = new System.Windows.Forms.ToolStripLabel();
            this.butTbAddAnimation = new System.Windows.Forms.ToolStripButton();
            this.butTbImport = new System.Windows.Forms.ToolStripButton();
            this.butTbDeleteAnimation = new System.Windows.Forms.ToolStripButton();
            this.butTbCutAnimation = new System.Windows.Forms.ToolStripButton();
            this.butTbCopyAnimation = new System.Windows.Forms.ToolStripButton();
            this.butTbPasteAnimation = new System.Windows.Forms.ToolStripButton();
            this.butTbReplaceAnimation = new System.Windows.Forms.ToolStripButton();
            this.butTbSplitAnimation = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.labelFrames = new System.Windows.Forms.ToolStripLabel();
            this.butTbAddFrame = new System.Windows.Forms.ToolStripButton();
            this.butTbDeleteFrame = new System.Windows.Forms.ToolStripButton();
            this.butTbCutFrame = new System.Windows.Forms.ToolStripButton();
            this.butTbCopyFrame = new System.Windows.Forms.ToolStripButton();
            this.butTbPasteFrame = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator5 = new System.Windows.Forms.ToolStripSeparator();
            this.butTbInterpolateFrames = new System.Windows.Forms.ToolStripButton();
            this.tbInterpolateFrameCount = new System.Windows.Forms.ToolStripTextBox();
            this.toolStripSeparator4 = new System.Windows.Forms.ToolStripSeparator();
            this.labelBone = new System.Windows.Forms.ToolStripLabel();
            this.comboBoneList = new DarkUI.Controls.ToolStripDarkComboBox();
            this.toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
            this.labelRoom = new System.Windows.Forms.ToolStripLabel();
            this.comboRoomList = new DarkUI.Controls.ToolStripDarkComboBox();
            this.lstAnimations = new DarkUI.Controls.DarkListView();
            this.darkSectionPanel1 = new DarkUI.Controls.DarkSectionPanel();
            this.butShowAll = new DarkUI.Controls.DarkButton();
            this.butDeleteAnimation = new DarkUI.Controls.DarkButton();
            this.darkButton1 = new DarkUI.Controls.DarkButton();
            this.butAddNewAnimation = new DarkUI.Controls.DarkButton();
            this.panelView = new DarkUI.Controls.DarkSectionPanel();
            this.panelRendering = new WadTool.Controls.PanelRenderingAnimationEditor();
            this.panelTimeline = new System.Windows.Forms.Panel();
            this.timeline = new TombLib.Controls.AnimationTrackBar();
            this.panelTransport = new System.Windows.Forms.Panel();
            this.darkToolStrip1 = new DarkUI.Controls.DarkToolStrip();
            this.toolStripSeparator6 = new System.Windows.Forms.ToolStripSeparator();
            this.butTransportStart = new System.Windows.Forms.ToolStripButton();
            this.butTransportFrameBack = new System.Windows.Forms.ToolStripButton();
            this.butTransportPlay = new System.Windows.Forms.ToolStripButton();
            this.butTransportFrameForward = new System.Windows.Forms.ToolStripButton();
            this.butTransportEnd = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator7 = new System.Windows.Forms.ToolStripSeparator();
            this.butTransportChained = new System.Windows.Forms.ToolStripButton();
            this.butTransportSound = new System.Windows.Forms.ToolStripButton();
            this.butTransportLandWater = new System.Windows.Forms.ToolStripButton();
            this.darkSectionPanel4 = new DarkUI.Controls.DarkSectionPanel();
            this.darkLabel2 = new DarkUI.Controls.DarkLabel();
            this.tbEndHorVel = new DarkUI.Controls.DarkTextBox();
            this.tbStartHorVel = new DarkUI.Controls.DarkTextBox();
            this.tbEndVertVel = new DarkUI.Controls.DarkTextBox();
            this.butClearAnimCollision = new DarkUI.Controls.DarkButton();
            this.tbStartVertVel = new DarkUI.Controls.DarkTextBox();
            this.butCalculateAnimCollision = new DarkUI.Controls.DarkButton();
            this.nudNextFrame = new DarkUI.Controls.DarkNumericUpDown();
            this.nudNextAnim = new DarkUI.Controls.DarkNumericUpDown();
            this.nudFramerate = new DarkUI.Controls.DarkNumericUpDown();
            this.butSearchStateID = new DarkUI.Controls.DarkButton();
            this.cmbStateID = new DarkUI.Controls.DarkComboBox();
            this.darkButton3 = new DarkUI.Controls.DarkButton();
            this.butEditStateChanges = new DarkUI.Controls.DarkButton();
            this.panelMain = new System.Windows.Forms.Panel();
            this.panelTools = new System.Windows.Forms.Panel();
            this.panelTransform = new DarkUI.Controls.DarkSectionPanel();
            this.darkLabel8 = new DarkUI.Controls.DarkLabel();
            this.picTransformPreview = new System.Windows.Forms.PictureBox();
            this.cmbTransformMode = new DarkUI.Controls.DarkComboBox();
            this.darkLabel29 = new DarkUI.Controls.DarkLabel();
            this.darkLabel28 = new DarkUI.Controls.DarkLabel();
            this.darkLabel21 = new DarkUI.Controls.DarkLabel();
            this.darkLabel26 = new DarkUI.Controls.DarkLabel();
            this.nudTransX = new DarkUI.Controls.DarkNumericUpDown();
            this.darkLabel27 = new DarkUI.Controls.DarkLabel();
            this.nudTransY = new DarkUI.Controls.DarkNumericUpDown();
            this.nudTransZ = new DarkUI.Controls.DarkNumericUpDown();
            this.darkLabel1 = new DarkUI.Controls.DarkLabel();
            this.darkLabel18 = new DarkUI.Controls.DarkLabel();
            this.nudRotX = new DarkUI.Controls.DarkNumericUpDown();
            this.darkLabel19 = new DarkUI.Controls.DarkLabel();
            this.nudRotY = new DarkUI.Controls.DarkNumericUpDown();
            this.nudRotZ = new DarkUI.Controls.DarkNumericUpDown();
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.toolStripButton6 = new System.Windows.Forms.ToolStripButton();
            this.darkContextMenu1 = new DarkUI.Controls.DarkContextMenu();
            this.cmTimelineContextMenu = new DarkUI.Controls.DarkContextMenu();
            this.cmMarkInMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.cmMarkOutMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.cmSelectAllMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.cnClearSelectionMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator8 = new System.Windows.Forms.ToolStripSeparator();
            this.cmCreateAnimCommandMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.cmCreateStateChangeMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.topMenu.SuspendLayout();
            this.darkStatusStrip1.SuspendLayout();
            this.topBar.SuspendLayout();
            this.darkSectionPanel1.SuspendLayout();
            this.panelView.SuspendLayout();
            this.panelTimeline.SuspendLayout();
            this.panelTransport.SuspendLayout();
            this.darkToolStrip1.SuspendLayout();
            this.darkSectionPanel4.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudNextFrame)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudNextAnim)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudFramerate)).BeginInit();
            this.panelMain.SuspendLayout();
            this.panelTools.SuspendLayout();
            this.panelTransform.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.picTransformPreview)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudTransX)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudTransY)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudTransZ)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudRotX)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudRotY)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudRotZ)).BeginInit();
            this.cmTimelineContextMenu.SuspendLayout();
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
            this.renderingToolStripMenuItem});
            this.topMenu.Location = new System.Drawing.Point(0, 0);
            this.topMenu.Name = "topMenu";
            this.topMenu.Padding = new System.Windows.Forms.Padding(3, 2, 0, 2);
            this.topMenu.Size = new System.Drawing.Size(975, 24);
            this.topMenu.TabIndex = 0;
            this.topMenu.Text = "darkMenuStrip1";
            // 
            // fileeToolStripMenuItem
            // 
            this.fileeToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.fileeToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.undoToolStripMenuItem,
            this.redoToolStripMenuItem,
            this.saveChangesToolStripMenuItem,
            this.toolStripMenuItem3,
            this.closeToolStripMenuItem});
            this.fileeToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.fileeToolStripMenuItem.Name = "fileeToolStripMenuItem";
            this.fileeToolStripMenuItem.Size = new System.Drawing.Size(39, 20);
            this.fileeToolStripMenuItem.Text = "Edit";
            // 
            // undoToolStripMenuItem
            // 
            this.undoToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.undoToolStripMenuItem.Enabled = false;
            this.undoToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(153)))), ((int)(((byte)(153)))), ((int)(((byte)(153)))));
            this.undoToolStripMenuItem.Image = global::WadTool.Properties.Resources.general_undo_16;
            this.undoToolStripMenuItem.Name = "undoToolStripMenuItem";
            this.undoToolStripMenuItem.Size = new System.Drawing.Size(145, 22);
            this.undoToolStripMenuItem.Text = "Undo";
            this.undoToolStripMenuItem.Click += new System.EventHandler(this.undoToolStripMenuItem_Click);
            // 
            // redoToolStripMenuItem
            // 
            this.redoToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.redoToolStripMenuItem.Enabled = false;
            this.redoToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(153)))), ((int)(((byte)(153)))), ((int)(((byte)(153)))));
            this.redoToolStripMenuItem.Image = global::WadTool.Properties.Resources.general_redo_16;
            this.redoToolStripMenuItem.Name = "redoToolStripMenuItem";
            this.redoToolStripMenuItem.Size = new System.Drawing.Size(145, 22);
            this.redoToolStripMenuItem.Text = "Redo";
            this.redoToolStripMenuItem.Click += new System.EventHandler(this.redoToolStripMenuItem_Click);
            // 
            // saveChangesToolStripMenuItem
            // 
            this.saveChangesToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.saveChangesToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.saveChangesToolStripMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("saveChangesToolStripMenuItem.Image")));
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
            this.toolStripMenuItem2,
            this.importToolStripMenuItem,
            this.exportToolStripMenuItem,
            this.toolStripMenuItem7,
            this.findReplaceAnimcommandsToolStripMenuItem,
            this.calculateBoundingBoxForAllFramesToolStripMenuItem,
            this.deleteBoundingBoxForAllFramesToolStripMenuItem,
            this.resampleAnimationToolStripMenuItem,
            this.resampleAnimationToKeyframesToolStripMenuItem});
            this.animationToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.animationToolStripMenuItem.Name = "animationToolStripMenuItem";
            this.animationToolStripMenuItem.Size = new System.Drawing.Size(75, 20);
            this.animationToolStripMenuItem.Text = "Animation";
            // 
            // addNewToolStripMenuItem
            // 
            this.addNewToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.addNewToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.addNewToolStripMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("addNewToolStripMenuItem.Image")));
            this.addNewToolStripMenuItem.Name = "addNewToolStripMenuItem";
            this.addNewToolStripMenuItem.Size = new System.Drawing.Size(264, 22);
            this.addNewToolStripMenuItem.Text = "New animation";
            this.addNewToolStripMenuItem.Click += new System.EventHandler(this.addNewAnimationToolStripMenuItem_Click);
            // 
            // deleteToolStripMenuItem
            // 
            this.deleteToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.deleteToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.deleteToolStripMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("deleteToolStripMenuItem.Image")));
            this.deleteToolStripMenuItem.Name = "deleteToolStripMenuItem";
            this.deleteToolStripMenuItem.Size = new System.Drawing.Size(264, 22);
            this.deleteToolStripMenuItem.Text = "Delete animation";
            this.deleteToolStripMenuItem.Click += new System.EventHandler(this.deleteAnimationToolStripMenuItem_Click);
            // 
            // splitAnimationToolStripMenuItem
            // 
            this.splitAnimationToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.splitAnimationToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.splitAnimationToolStripMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("splitAnimationToolStripMenuItem.Image")));
            this.splitAnimationToolStripMenuItem.Name = "splitAnimationToolStripMenuItem";
            this.splitAnimationToolStripMenuItem.Size = new System.Drawing.Size(264, 22);
            this.splitAnimationToolStripMenuItem.Text = "Split animation";
            this.splitAnimationToolStripMenuItem.Click += new System.EventHandler(this.splitAnimationToolStripMenuItem_Click);
            // 
            // toolStripMenuItem5
            // 
            this.toolStripMenuItem5.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.toolStripMenuItem5.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.toolStripMenuItem5.Margin = new System.Windows.Forms.Padding(0, 0, 0, 1);
            this.toolStripMenuItem5.Name = "toolStripMenuItem5";
            this.toolStripMenuItem5.Size = new System.Drawing.Size(261, 6);
            // 
            // curToolStripMenuItem
            // 
            this.curToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.curToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.curToolStripMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("curToolStripMenuItem.Image")));
            this.curToolStripMenuItem.Name = "curToolStripMenuItem";
            this.curToolStripMenuItem.Size = new System.Drawing.Size(264, 22);
            this.curToolStripMenuItem.Text = "Cut";
            this.curToolStripMenuItem.Click += new System.EventHandler(this.cutAnimationToolStripMenuItem_Click);
            // 
            // copyToolStripMenuItem1
            // 
            this.copyToolStripMenuItem1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.copyToolStripMenuItem1.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.copyToolStripMenuItem1.Image = ((System.Drawing.Image)(resources.GetObject("copyToolStripMenuItem1.Image")));
            this.copyToolStripMenuItem1.Name = "copyToolStripMenuItem1";
            this.copyToolStripMenuItem1.Size = new System.Drawing.Size(264, 22);
            this.copyToolStripMenuItem1.Text = "Copy";
            this.copyToolStripMenuItem1.Click += new System.EventHandler(this.copyAnimationToolStripMenuItem_Click);
            // 
            // pasteToolStripMenuItem1
            // 
            this.pasteToolStripMenuItem1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.pasteToolStripMenuItem1.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.pasteToolStripMenuItem1.Image = ((System.Drawing.Image)(resources.GetObject("pasteToolStripMenuItem1.Image")));
            this.pasteToolStripMenuItem1.Name = "pasteToolStripMenuItem1";
            this.pasteToolStripMenuItem1.Size = new System.Drawing.Size(264, 22);
            this.pasteToolStripMenuItem1.Text = "Paste";
            this.pasteToolStripMenuItem1.Click += new System.EventHandler(this.pasteAnimationToolStripMenuItem_Click);
            // 
            // toolStripMenuItem2
            // 
            this.toolStripMenuItem2.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.toolStripMenuItem2.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.toolStripMenuItem2.Margin = new System.Windows.Forms.Padding(0, 0, 0, 1);
            this.toolStripMenuItem2.Name = "toolStripMenuItem2";
            this.toolStripMenuItem2.Size = new System.Drawing.Size(261, 6);
            // 
            // importToolStripMenuItem
            // 
            this.importToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.importToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.importToolStripMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("importToolStripMenuItem.Image")));
            this.importToolStripMenuItem.Name = "importToolStripMenuItem";
            this.importToolStripMenuItem.Size = new System.Drawing.Size(264, 22);
            this.importToolStripMenuItem.Text = "Import...";
            this.importToolStripMenuItem.Click += new System.EventHandler(this.importToolStripMenuItem_Click);
            // 
            // exportToolStripMenuItem
            // 
            this.exportToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.exportToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.exportToolStripMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("exportToolStripMenuItem.Image")));
            this.exportToolStripMenuItem.Name = "exportToolStripMenuItem";
            this.exportToolStripMenuItem.Size = new System.Drawing.Size(264, 22);
            this.exportToolStripMenuItem.Text = "Export...";
            this.exportToolStripMenuItem.Click += new System.EventHandler(this.exportToolStripMenuItem_Click);
            // 
            // toolStripMenuItem7
            // 
            this.toolStripMenuItem7.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.toolStripMenuItem7.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.toolStripMenuItem7.Margin = new System.Windows.Forms.Padding(0, 0, 0, 1);
            this.toolStripMenuItem7.Name = "toolStripMenuItem7";
            this.toolStripMenuItem7.Size = new System.Drawing.Size(261, 6);
            // 
            // findReplaceAnimcommandsToolStripMenuItem
            // 
            this.findReplaceAnimcommandsToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.findReplaceAnimcommandsToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.findReplaceAnimcommandsToolStripMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("findReplaceAnimcommandsToolStripMenuItem.Image")));
            this.findReplaceAnimcommandsToolStripMenuItem.Name = "findReplaceAnimcommandsToolStripMenuItem";
            this.findReplaceAnimcommandsToolStripMenuItem.Size = new System.Drawing.Size(264, 22);
            this.findReplaceAnimcommandsToolStripMenuItem.Text = "Find && replace animcommands...";
            this.findReplaceAnimcommandsToolStripMenuItem.Click += new System.EventHandler(this.findReplaceAnimcommandsToolStripMenuItem_Click);
            // 
            // calculateBoundingBoxForAllFramesToolStripMenuItem
            // 
            this.calculateBoundingBoxForAllFramesToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.calculateBoundingBoxForAllFramesToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.calculateBoundingBoxForAllFramesToolStripMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("calculateBoundingBoxForAllFramesToolStripMenuItem.Image")));
            this.calculateBoundingBoxForAllFramesToolStripMenuItem.Name = "calculateBoundingBoxForAllFramesToolStripMenuItem";
            this.calculateBoundingBoxForAllFramesToolStripMenuItem.Size = new System.Drawing.Size(264, 22);
            this.calculateBoundingBoxForAllFramesToolStripMenuItem.Text = "Calculate collision box for all frames";
            this.calculateBoundingBoxForAllFramesToolStripMenuItem.Click += new System.EventHandler(this.calculateBoundingBoxForAllFramesToolStripMenuItem_Click);
            // 
            // deleteBoundingBoxForAllFramesToolStripMenuItem
            // 
            this.deleteBoundingBoxForAllFramesToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.deleteBoundingBoxForAllFramesToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.deleteBoundingBoxForAllFramesToolStripMenuItem.Name = "deleteBoundingBoxForAllFramesToolStripMenuItem";
            this.deleteBoundingBoxForAllFramesToolStripMenuItem.Size = new System.Drawing.Size(264, 22);
            this.deleteBoundingBoxForAllFramesToolStripMenuItem.Text = "Delete collision box for all frames";
            this.deleteBoundingBoxForAllFramesToolStripMenuItem.Click += new System.EventHandler(this.deleteBoundingBoxForAllFramesToolStripMenuItem_Click);
            // 
            // resampleAnimationToolStripMenuItem
            // 
            this.resampleAnimationToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.resampleAnimationToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.resampleAnimationToolStripMenuItem.Name = "resampleAnimationToolStripMenuItem";
            this.resampleAnimationToolStripMenuItem.Size = new System.Drawing.Size(264, 22);
            this.resampleAnimationToolStripMenuItem.Text = "Resample animation";
            this.resampleAnimationToolStripMenuItem.Click += new System.EventHandler(this.resampleAnimationToolStripMenuItem_Click);
            // 
            // resampleAnimationToKeyframesToolStripMenuItem
            // 
            this.resampleAnimationToKeyframesToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.resampleAnimationToKeyframesToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.resampleAnimationToKeyframesToolStripMenuItem.Name = "resampleAnimationToKeyframesToolStripMenuItem";
            this.resampleAnimationToKeyframesToolStripMenuItem.Size = new System.Drawing.Size(264, 22);
            this.resampleAnimationToKeyframesToolStripMenuItem.Text = "Resample animation to framerate";
            this.resampleAnimationToKeyframesToolStripMenuItem.Click += new System.EventHandler(this.resampleAnimationToKeyframesToolStripMenuItem_Click);
            // 
            // frameToolStripMenuItem
            // 
            this.frameToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.frameToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.insertFrameAfterCurrentOneToolStripMenuItem,
            this.insertnFramesAfterCurrentOneToolStripMenuItem,
            this.deleteFrameToolStripMenuItem,
            this.deleteEveryNthFrameToolStripMenuItem,
            this.toolStripMenuItem4,
            this.cutToolStripMenuItem,
            this.copyToolStripMenuItem,
            this.pasteToolStripMenuItem,
            this.toolStripMenuItem1,
            this.interpolateFramesToolStripMenuItem,
            this.toolStripMenuItem6,
            this.calculateCollisionBoxForCurrentFrameToolStripMenuItem,
            this.deleteCollisionBoxForCurrentFrameToolStripMenuItem});
            this.frameToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.frameToolStripMenuItem.Name = "frameToolStripMenuItem";
            this.frameToolStripMenuItem.Size = new System.Drawing.Size(57, 20);
            this.frameToolStripMenuItem.Text = "Frames";
            // 
            // insertFrameAfterCurrentOneToolStripMenuItem
            // 
            this.insertFrameAfterCurrentOneToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.insertFrameAfterCurrentOneToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.insertFrameAfterCurrentOneToolStripMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("insertFrameAfterCurrentOneToolStripMenuItem.Image")));
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
            this.insertnFramesAfterCurrentOneToolStripMenuItem.Click += new System.EventHandler(this.insertFramesAfterCurrentToolStripMenuItem_Click);
            // 
            // deleteFrameToolStripMenuItem
            // 
            this.deleteFrameToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.deleteFrameToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.deleteFrameToolStripMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("deleteFrameToolStripMenuItem.Image")));
            this.deleteFrameToolStripMenuItem.Name = "deleteFrameToolStripMenuItem";
            this.deleteFrameToolStripMenuItem.Size = new System.Drawing.Size(285, 22);
            this.deleteFrameToolStripMenuItem.Text = "Delete frame";
            this.deleteFrameToolStripMenuItem.Click += new System.EventHandler(this.deleteFramesToolStripMenuItem_Click);
            // 
            // deleteEveryNthFrameToolStripMenuItem
            // 
            this.deleteEveryNthFrameToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.deleteEveryNthFrameToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.deleteEveryNthFrameToolStripMenuItem.Name = "deleteEveryNthFrameToolStripMenuItem";
            this.deleteEveryNthFrameToolStripMenuItem.Size = new System.Drawing.Size(285, 22);
            this.deleteEveryNthFrameToolStripMenuItem.Text = "Delete every (n)th frame";
            this.deleteEveryNthFrameToolStripMenuItem.Click += new System.EventHandler(this.DeleteEveryNthFrameToolStripMenuItem_Click);
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
            this.cutToolStripMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("cutToolStripMenuItem.Image")));
            this.cutToolStripMenuItem.Name = "cutToolStripMenuItem";
            this.cutToolStripMenuItem.Size = new System.Drawing.Size(285, 22);
            this.cutToolStripMenuItem.Text = "Cut";
            this.cutToolStripMenuItem.Click += new System.EventHandler(this.cutFramesToolStripMenuItem_Click);
            // 
            // copyToolStripMenuItem
            // 
            this.copyToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.copyToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.copyToolStripMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("copyToolStripMenuItem.Image")));
            this.copyToolStripMenuItem.Name = "copyToolStripMenuItem";
            this.copyToolStripMenuItem.Size = new System.Drawing.Size(285, 22);
            this.copyToolStripMenuItem.Text = "Copy";
            this.copyToolStripMenuItem.Click += new System.EventHandler(this.copyFramesToolStripMenuItem_Click);
            // 
            // pasteToolStripMenuItem
            // 
            this.pasteToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.pasteToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.pasteToolStripMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("pasteToolStripMenuItem.Image")));
            this.pasteToolStripMenuItem.Name = "pasteToolStripMenuItem";
            this.pasteToolStripMenuItem.Size = new System.Drawing.Size(285, 22);
            this.pasteToolStripMenuItem.Text = "Paste";
            this.pasteToolStripMenuItem.Click += new System.EventHandler(this.pasteFramesToolStripMenuItem_Click);
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
            this.calculateCollisionBoxForCurrentFrameToolStripMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("calculateCollisionBoxForCurrentFrameToolStripMenuItem.Image")));
            this.calculateCollisionBoxForCurrentFrameToolStripMenuItem.Name = "calculateCollisionBoxForCurrentFrameToolStripMenuItem";
            this.calculateCollisionBoxForCurrentFrameToolStripMenuItem.Size = new System.Drawing.Size(285, 22);
            this.calculateCollisionBoxForCurrentFrameToolStripMenuItem.Text = "Calculate collision box for current frame";
            this.calculateCollisionBoxForCurrentFrameToolStripMenuItem.Click += new System.EventHandler(this.calculateBoundingBoxForCurrentFrameToolStripMenuItem_Click);
            // 
            // deleteCollisionBoxForCurrentFrameToolStripMenuItem
            // 
            this.deleteCollisionBoxForCurrentFrameToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.deleteCollisionBoxForCurrentFrameToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.deleteCollisionBoxForCurrentFrameToolStripMenuItem.Name = "deleteCollisionBoxForCurrentFrameToolStripMenuItem";
            this.deleteCollisionBoxForCurrentFrameToolStripMenuItem.Size = new System.Drawing.Size(285, 22);
            this.deleteCollisionBoxForCurrentFrameToolStripMenuItem.Text = "Delete collision box for current frame";
            this.deleteCollisionBoxForCurrentFrameToolStripMenuItem.Click += new System.EventHandler(this.deleteCollisionBoxForCurrentFrameToolStripMenuItem_Click);
            // 
            // renderingToolStripMenuItem
            // 
            this.renderingToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.renderingToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.drawGizmoToolStripMenuItem,
            this.drawGridToolStripMenuItem,
            this.drawCollisionBoxToolStripMenuItem,
            this.smoothAnimationsToolStripMenuItem});
            this.renderingToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.renderingToolStripMenuItem.Name = "renderingToolStripMenuItem";
            this.renderingToolStripMenuItem.Size = new System.Drawing.Size(73, 20);
            this.renderingToolStripMenuItem.Text = "Rendering";
            // 
            // drawGizmoToolStripMenuItem
            // 
            this.drawGizmoToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.drawGizmoToolStripMenuItem.Checked = true;
            this.drawGizmoToolStripMenuItem.CheckOnClick = true;
            this.drawGizmoToolStripMenuItem.CheckState = System.Windows.Forms.CheckState.Checked;
            this.drawGizmoToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.drawGizmoToolStripMenuItem.Name = "drawGizmoToolStripMenuItem";
            this.drawGizmoToolStripMenuItem.Size = new System.Drawing.Size(173, 22);
            this.drawGizmoToolStripMenuItem.Text = "Draw gizmo";
            this.drawGizmoToolStripMenuItem.Click += new System.EventHandler(this.drawGizmoToolStripMenuItem_Click);
            // 
            // drawGridToolStripMenuItem
            // 
            this.drawGridToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.drawGridToolStripMenuItem.Checked = true;
            this.drawGridToolStripMenuItem.CheckOnClick = true;
            this.drawGridToolStripMenuItem.CheckState = System.Windows.Forms.CheckState.Checked;
            this.drawGridToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.drawGridToolStripMenuItem.Name = "drawGridToolStripMenuItem";
            this.drawGridToolStripMenuItem.Size = new System.Drawing.Size(173, 22);
            this.drawGridToolStripMenuItem.Text = "Draw grid";
            this.drawGridToolStripMenuItem.Click += new System.EventHandler(this.drawGridToolStripMenuItem_Click);
            // 
            // drawCollisionBoxToolStripMenuItem
            // 
            this.drawCollisionBoxToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.drawCollisionBoxToolStripMenuItem.Checked = true;
            this.drawCollisionBoxToolStripMenuItem.CheckOnClick = true;
            this.drawCollisionBoxToolStripMenuItem.CheckState = System.Windows.Forms.CheckState.Checked;
            this.drawCollisionBoxToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.drawCollisionBoxToolStripMenuItem.Name = "drawCollisionBoxToolStripMenuItem";
            this.drawCollisionBoxToolStripMenuItem.Size = new System.Drawing.Size(173, 22);
            this.drawCollisionBoxToolStripMenuItem.Text = "Draw collision box";
            this.drawCollisionBoxToolStripMenuItem.Click += new System.EventHandler(this.drawCollisionBoxToolStripMenuItem_Click);
            // 
            // smoothAnimationsToolStripMenuItem
            // 
            this.smoothAnimationsToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.smoothAnimationsToolStripMenuItem.Checked = true;
            this.smoothAnimationsToolStripMenuItem.CheckOnClick = true;
            this.smoothAnimationsToolStripMenuItem.CheckState = System.Windows.Forms.CheckState.Checked;
            this.smoothAnimationsToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.smoothAnimationsToolStripMenuItem.Name = "smoothAnimationsToolStripMenuItem";
            this.smoothAnimationsToolStripMenuItem.Size = new System.Drawing.Size(173, 22);
            this.smoothAnimationsToolStripMenuItem.Text = "Smooth animation";
            this.smoothAnimationsToolStripMenuItem.Click += new System.EventHandler(this.smoothAnimationsToolStripMenuItem_Click);
            // 
            // darkStatusStrip1
            // 
            this.darkStatusStrip1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.darkStatusStrip1.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.darkStatusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.statusFrame});
            this.darkStatusStrip1.Location = new System.Drawing.Point(0, 650);
            this.darkStatusStrip1.Name = "darkStatusStrip1";
            this.darkStatusStrip1.Padding = new System.Windows.Forms.Padding(0, 5, 0, 3);
            this.darkStatusStrip1.Size = new System.Drawing.Size(975, 25);
            this.darkStatusStrip1.TabIndex = 1;
            this.darkStatusStrip1.Text = "darkStatusStrip1";
            // 
            // statusFrame
            // 
            this.statusFrame.Margin = new System.Windows.Forms.Padding(0, 1, 0, 2);
            this.statusFrame.Name = "statusFrame";
            this.statusFrame.Size = new System.Drawing.Size(43, 14);
            this.statusFrame.Text = "Frame:";
            this.statusFrame.TextAlign = System.Drawing.ContentAlignment.TopLeft;
            // 
            // darkLabel22
            // 
            this.darkLabel22.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.darkLabel22.Location = new System.Drawing.Point(207, 125);
            this.darkLabel22.Name = "darkLabel22";
            this.darkLabel22.Size = new System.Drawing.Size(60, 13);
            this.darkLabel22.TabIndex = 120;
            this.darkLabel22.Text = "End H vel";
            // 
            // darkLabel23
            // 
            this.darkLabel23.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.darkLabel23.Location = new System.Drawing.Point(2, 125);
            this.darkLabel23.Name = "darkLabel23";
            this.darkLabel23.Size = new System.Drawing.Size(64, 13);
            this.darkLabel23.TabIndex = 114;
            this.darkLabel23.Text = "Start V vel";
            // 
            // darkLabel24
            // 
            this.darkLabel24.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.darkLabel24.Location = new System.Drawing.Point(139, 125);
            this.darkLabel24.Name = "darkLabel24";
            this.darkLabel24.Size = new System.Drawing.Size(65, 13);
            this.darkLabel24.TabIndex = 118;
            this.darkLabel24.Text = "Start H vel";
            // 
            // darkLabel25
            // 
            this.darkLabel25.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.darkLabel25.Location = new System.Drawing.Point(69, 125);
            this.darkLabel25.Name = "darkLabel25";
            this.darkLabel25.Size = new System.Drawing.Size(61, 13);
            this.darkLabel25.TabIndex = 116;
            this.darkLabel25.Text = "End V vel";
            // 
            // tbStateId
            // 
            this.tbStateId.AutocompleteWords = ((System.Collections.Generic.List<string>)(resources.GetObject("tbStateId.AutocompleteWords")));
            this.tbStateId.Location = new System.Drawing.Point(44, 56);
            this.tbStateId.Name = "tbStateId";
            this.tbStateId.Size = new System.Drawing.Size(193, 22);
            this.tbStateId.TabIndex = 103;
            this.tbStateId.KeyDown += new System.Windows.Forms.KeyEventHandler(this.tbStateId_KeyDown);
            this.tbStateId.Validated += new System.EventHandler(this.tbStateId_Validated);
            // 
            // darkLabel7
            // 
            this.darkLabel7.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.darkLabel7.Location = new System.Drawing.Point(2, 58);
            this.darkLabel7.Name = "darkLabel7";
            this.darkLabel7.Size = new System.Drawing.Size(38, 13);
            this.darkLabel7.TabIndex = 102;
            this.darkLabel7.Text = "State:";
            // 
            // darkLabel6
            // 
            this.darkLabel6.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.darkLabel6.Location = new System.Drawing.Point(139, 82);
            this.darkLabel6.Name = "darkLabel6";
            this.darkLabel6.Size = new System.Drawing.Size(65, 13);
            this.darkLabel6.TabIndex = 100;
            this.darkLabel6.Text = "Next frame";
            // 
            // darkLabel5
            // 
            this.darkLabel5.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.darkLabel5.Location = new System.Drawing.Point(69, 82);
            this.darkLabel5.Name = "darkLabel5";
            this.darkLabel5.Size = new System.Drawing.Size(64, 13);
            this.darkLabel5.TabIndex = 98;
            this.darkLabel5.Text = "Next anim";
            // 
            // darkLabel4
            // 
            this.darkLabel4.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.darkLabel4.Location = new System.Drawing.Point(2, 82);
            this.darkLabel4.Name = "darkLabel4";
            this.darkLabel4.Size = new System.Drawing.Size(64, 13);
            this.darkLabel4.TabIndex = 96;
            this.darkLabel4.Text = "Framerate";
            // 
            // tbName
            // 
            this.tbName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tbName.Location = new System.Drawing.Point(44, 28);
            this.tbName.Name = "tbName";
            this.tbName.Size = new System.Drawing.Size(231, 22);
            this.tbName.TabIndex = 95;
            this.tbName.KeyDown += new System.Windows.Forms.KeyEventHandler(this.tbName_KeyDown);
            this.tbName.Validated += new System.EventHandler(this.tbName_Validated);
            // 
            // darkLabel3
            // 
            this.darkLabel3.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.darkLabel3.Location = new System.Drawing.Point(2, 30);
            this.darkLabel3.Name = "darkLabel3";
            this.darkLabel3.Size = new System.Drawing.Size(41, 20);
            this.darkLabel3.TabIndex = 94;
            this.darkLabel3.Text = "Name:";
            // 
            // tbSearchAnimation
            // 
            this.tbSearchAnimation.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tbSearchAnimation.Location = new System.Drawing.Point(4, 28);
            this.tbSearchAnimation.Name = "tbSearchAnimation";
            this.tbSearchAnimation.Size = new System.Drawing.Size(221, 22);
            this.tbSearchAnimation.TabIndex = 123;
            this.toolTip1.SetToolTip(this.tbSearchAnimation, "Numerical input - filter by state ID.\r\nString input - filter by animation name.\r\n" +
        "\r\nTokens:\r\ns:[name or ID] - state name or ID\r\na:[name or ID] - anim name or ID");
            this.tbSearchAnimation.KeyDown += new System.Windows.Forms.KeyEventHandler(this.tbSearchByStateID_KeyDown);
            // 
            // topBar
            // 
            this.topBar.AutoSize = false;
            this.topBar.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.topBar.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.topBar.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.topBar.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.butTbSaveAllChanges,
            this.butTbUndo,
            this.butTbRedo,
            this.butTbResetCamera,
            this.toolStripSeparator1,
            this.labelAnims,
            this.butTbAddAnimation,
            this.butTbImport,
            this.butTbDeleteAnimation,
            this.butTbCutAnimation,
            this.butTbCopyAnimation,
            this.butTbPasteAnimation,
            this.butTbReplaceAnimation,
            this.butTbSplitAnimation,
            this.toolStripSeparator2,
            this.labelFrames,
            this.butTbAddFrame,
            this.butTbDeleteFrame,
            this.butTbCutFrame,
            this.butTbCopyFrame,
            this.butTbPasteFrame,
            this.toolStripSeparator5,
            this.butTbInterpolateFrames,
            this.tbInterpolateFrameCount,
            this.toolStripSeparator4,
            this.labelBone,
            this.comboBoneList,
            this.toolStripSeparator3,
            this.labelRoom,
            this.comboRoomList});
            this.topBar.Location = new System.Drawing.Point(0, 24);
            this.topBar.Name = "topBar";
            this.topBar.Padding = new System.Windows.Forms.Padding(5, 0, 1, 0);
            this.topBar.Size = new System.Drawing.Size(975, 28);
            this.topBar.TabIndex = 6;
            this.topBar.Text = "darkToolStrip1";
            // 
            // butTbSaveAllChanges
            // 
            this.butTbSaveAllChanges.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.butTbSaveAllChanges.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.butTbSaveAllChanges.Enabled = false;
            this.butTbSaveAllChanges.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.butTbSaveAllChanges.Image = ((System.Drawing.Image)(resources.GetObject("butTbSaveAllChanges.Image")));
            this.butTbSaveAllChanges.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.butTbSaveAllChanges.Name = "butTbSaveAllChanges";
            this.butTbSaveAllChanges.Size = new System.Drawing.Size(23, 25);
            this.butTbSaveAllChanges.Click += new System.EventHandler(this.butTbSaveChanges_Click);
            // 
            // butTbUndo
            // 
            this.butTbUndo.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.butTbUndo.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.butTbUndo.Enabled = false;
            this.butTbUndo.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.butTbUndo.Image = global::WadTool.Properties.Resources.general_undo_16;
            this.butTbUndo.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.butTbUndo.Name = "butTbUndo";
            this.butTbUndo.Size = new System.Drawing.Size(23, 25);
            this.butTbUndo.ToolTipText = "Undo";
            this.butTbUndo.Click += new System.EventHandler(this.butTbUndo_Click);
            // 
            // butTbRedo
            // 
            this.butTbRedo.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.butTbRedo.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.butTbRedo.Enabled = false;
            this.butTbRedo.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.butTbRedo.Image = global::WadTool.Properties.Resources.general_redo_16;
            this.butTbRedo.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.butTbRedo.Name = "butTbRedo";
            this.butTbRedo.Size = new System.Drawing.Size(23, 25);
            this.butTbRedo.ToolTipText = "Redo";
            this.butTbRedo.Click += new System.EventHandler(this.butTbRedo_Click);
            // 
            // butTbResetCamera
            // 
            this.butTbResetCamera.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.butTbResetCamera.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.butTbResetCamera.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.butTbResetCamera.Image = global::WadTool.Properties.Resources.general_target_16;
            this.butTbResetCamera.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.butTbResetCamera.Name = "butTbResetCamera";
            this.butTbResetCamera.Size = new System.Drawing.Size(23, 25);
            this.butTbResetCamera.ToolTipText = "Reset camera";
            this.butTbResetCamera.Click += new System.EventHandler(this.butTbResetCamera_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.toolStripSeparator1.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.toolStripSeparator1.Margin = new System.Windows.Forms.Padding(0, 0, 2, 0);
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(6, 28);
            // 
            // labelAnims
            // 
            this.labelAnims.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.labelAnims.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.labelAnims.Name = "labelAnims";
            this.labelAnims.Size = new System.Drawing.Size(44, 25);
            this.labelAnims.Text = "Anims:";
            // 
            // butTbAddAnimation
            // 
            this.butTbAddAnimation.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.butTbAddAnimation.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.butTbAddAnimation.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.butTbAddAnimation.Image = ((System.Drawing.Image)(resources.GetObject("butTbAddAnimation.Image")));
            this.butTbAddAnimation.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.butTbAddAnimation.Name = "butTbAddAnimation";
            this.butTbAddAnimation.Size = new System.Drawing.Size(23, 25);
            this.butTbAddAnimation.Text = "toolStripButton2";
            this.butTbAddAnimation.ToolTipText = "Add animation";
            this.butTbAddAnimation.Click += new System.EventHandler(this.butTbAddAnimation_Click);
            // 
            // butTbImport
            // 
            this.butTbImport.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.butTbImport.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.butTbImport.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.butTbImport.Image = ((System.Drawing.Image)(resources.GetObject("butTbImport.Image")));
            this.butTbImport.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.butTbImport.Name = "butTbImport";
            this.butTbImport.Size = new System.Drawing.Size(23, 25);
            this.butTbImport.ToolTipText = "Import...";
            this.butTbImport.Click += new System.EventHandler(this.butTbImport_Click);
            // 
            // butTbDeleteAnimation
            // 
            this.butTbDeleteAnimation.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.butTbDeleteAnimation.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.butTbDeleteAnimation.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.butTbDeleteAnimation.Image = ((System.Drawing.Image)(resources.GetObject("butTbDeleteAnimation.Image")));
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
            this.butTbCutAnimation.Image = ((System.Drawing.Image)(resources.GetObject("butTbCutAnimation.Image")));
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
            this.butTbCopyAnimation.Image = ((System.Drawing.Image)(resources.GetObject("butTbCopyAnimation.Image")));
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
            this.butTbPasteAnimation.Image = ((System.Drawing.Image)(resources.GetObject("butTbPasteAnimation.Image")));
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
            this.butTbReplaceAnimation.Image = ((System.Drawing.Image)(resources.GetObject("butTbReplaceAnimation.Image")));
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
            this.butTbSplitAnimation.Image = ((System.Drawing.Image)(resources.GetObject("butTbSplitAnimation.Image")));
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
            // labelFrames
            // 
            this.labelFrames.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.labelFrames.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.labelFrames.Name = "labelFrames";
            this.labelFrames.Size = new System.Drawing.Size(48, 25);
            this.labelFrames.Text = "Frames:";
            // 
            // butTbAddFrame
            // 
            this.butTbAddFrame.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.butTbAddFrame.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.butTbAddFrame.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.butTbAddFrame.Image = ((System.Drawing.Image)(resources.GetObject("butTbAddFrame.Image")));
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
            this.butTbDeleteFrame.Image = ((System.Drawing.Image)(resources.GetObject("butTbDeleteFrame.Image")));
            this.butTbDeleteFrame.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.butTbDeleteFrame.Name = "butTbDeleteFrame";
            this.butTbDeleteFrame.Size = new System.Drawing.Size(23, 25);
            this.butTbDeleteFrame.Text = "toolStripButton3";
            this.butTbDeleteFrame.ToolTipText = "Delete frames";
            this.butTbDeleteFrame.Click += new System.EventHandler(this.butTbDeleteFrame_Click);
            // 
            // butTbCutFrame
            // 
            this.butTbCutFrame.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.butTbCutFrame.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.butTbCutFrame.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.butTbCutFrame.Image = ((System.Drawing.Image)(resources.GetObject("butTbCutFrame.Image")));
            this.butTbCutFrame.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.butTbCutFrame.Name = "butTbCutFrame";
            this.butTbCutFrame.Size = new System.Drawing.Size(23, 25);
            this.butTbCutFrame.Text = "toolStripButton8";
            this.butTbCutFrame.ToolTipText = "Cut frames";
            this.butTbCutFrame.Click += new System.EventHandler(this.butTbCutFrame_Click);
            // 
            // butTbCopyFrame
            // 
            this.butTbCopyFrame.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.butTbCopyFrame.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.butTbCopyFrame.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.butTbCopyFrame.Image = ((System.Drawing.Image)(resources.GetObject("butTbCopyFrame.Image")));
            this.butTbCopyFrame.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.butTbCopyFrame.Name = "butTbCopyFrame";
            this.butTbCopyFrame.Size = new System.Drawing.Size(23, 25);
            this.butTbCopyFrame.Text = "toolStripButton4";
            this.butTbCopyFrame.ToolTipText = "Copy frames";
            this.butTbCopyFrame.Click += new System.EventHandler(this.butTbCopyFrame_Click);
            // 
            // butTbPasteFrame
            // 
            this.butTbPasteFrame.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.butTbPasteFrame.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.butTbPasteFrame.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.butTbPasteFrame.Image = ((System.Drawing.Image)(resources.GetObject("butTbPasteFrame.Image")));
            this.butTbPasteFrame.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.butTbPasteFrame.Name = "butTbPasteFrame";
            this.butTbPasteFrame.Size = new System.Drawing.Size(23, 25);
            this.butTbPasteFrame.Text = "toolStripButton5";
            this.butTbPasteFrame.ToolTipText = "Paste frames";
            this.butTbPasteFrame.Click += new System.EventHandler(this.butTbPasteFrame_Click);
            // 
            // toolStripSeparator5
            // 
            this.toolStripSeparator5.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.toolStripSeparator5.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.toolStripSeparator5.Margin = new System.Windows.Forms.Padding(0, 0, 2, 0);
            this.toolStripSeparator5.Name = "toolStripSeparator5";
            this.toolStripSeparator5.Size = new System.Drawing.Size(6, 28);
            // 
            // butTbInterpolateFrames
            // 
            this.butTbInterpolateFrames.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.butTbInterpolateFrames.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.butTbInterpolateFrames.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.butTbInterpolateFrames.Image = ((System.Drawing.Image)(resources.GetObject("butTbInterpolateFrames.Image")));
            this.butTbInterpolateFrames.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.butTbInterpolateFrames.Name = "butTbInterpolateFrames";
            this.butTbInterpolateFrames.Size = new System.Drawing.Size(23, 25);
            this.butTbInterpolateFrames.ToolTipText = "Interpolate frames";
            this.butTbInterpolateFrames.Click += new System.EventHandler(this.butTbInterpolateFrames_Click);
            // 
            // tbInterpolateFrameCount
            // 
            this.tbInterpolateFrameCount.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.tbInterpolateFrameCount.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.tbInterpolateFrameCount.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.tbInterpolateFrameCount.Name = "tbInterpolateFrameCount";
            this.tbInterpolateFrameCount.Size = new System.Drawing.Size(28, 28);
            this.tbInterpolateFrameCount.Text = "3";
            // 
            // toolStripSeparator4
            // 
            this.toolStripSeparator4.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.toolStripSeparator4.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.toolStripSeparator4.Margin = new System.Windows.Forms.Padding(0, 0, 2, 0);
            this.toolStripSeparator4.Name = "toolStripSeparator4";
            this.toolStripSeparator4.Size = new System.Drawing.Size(6, 28);
            // 
            // labelBone
            // 
            this.labelBone.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.labelBone.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.labelBone.Name = "labelBone";
            this.labelBone.Size = new System.Drawing.Size(37, 25);
            this.labelBone.Text = "Bone:";
            // 
            // comboBoneList
            // 
            this.comboBoneList.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.comboBoneList.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.comboBoneList.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.comboBoneList.Name = "comboBoneList";
            this.comboBoneList.SelectedIndex = -1;
            this.comboBoneList.Size = new System.Drawing.Size(121, 25);
            this.comboBoneList.SelectedIndexChanged += new System.EventHandler(this.comboBoneList_SelectedIndexChanged);
            // 
            // toolStripSeparator3
            // 
            this.toolStripSeparator3.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.toolStripSeparator3.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.toolStripSeparator3.Margin = new System.Windows.Forms.Padding(0, 0, 2, 0);
            this.toolStripSeparator3.Name = "toolStripSeparator3";
            this.toolStripSeparator3.Size = new System.Drawing.Size(6, 28);
            // 
            // labelRoom
            // 
            this.labelRoom.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.labelRoom.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.labelRoom.Name = "labelRoom";
            this.labelRoom.Size = new System.Drawing.Size(42, 25);
            this.labelRoom.Text = "Room:";
            // 
            // comboRoomList
            // 
            this.comboRoomList.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.comboRoomList.Enabled = false;
            this.comboRoomList.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.comboRoomList.Name = "comboRoomList";
            this.comboRoomList.SelectedIndex = -1;
            this.comboRoomList.Size = new System.Drawing.Size(121, 25);
            this.comboRoomList.SelectedIndexChanged += new System.EventHandler(this.comboRoomList_SelectedIndexChanged);
            // 
            // lstAnimations
            // 
            this.lstAnimations.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lstAnimations.Location = new System.Drawing.Point(4, 53);
            this.lstAnimations.MouseWheelScrollSpeedV = 0.2F;
            this.lstAnimations.Name = "lstAnimations";
            this.lstAnimations.Size = new System.Drawing.Size(271, 196);
            this.lstAnimations.TabIndex = 8;
            this.lstAnimations.SelectedIndicesChanged += new System.EventHandler(this.lstAnimations_SelectedIndicesChanged);
            this.lstAnimations.Click += new System.EventHandler(this.lstAnimations_Click);
            // 
            // darkSectionPanel1
            // 
            this.darkSectionPanel1.Controls.Add(this.lstAnimations);
            this.darkSectionPanel1.Controls.Add(this.butShowAll);
            this.darkSectionPanel1.Controls.Add(this.tbSearchAnimation);
            this.darkSectionPanel1.Controls.Add(this.butDeleteAnimation);
            this.darkSectionPanel1.Controls.Add(this.darkButton1);
            this.darkSectionPanel1.Controls.Add(this.butAddNewAnimation);
            this.darkSectionPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.darkSectionPanel1.Location = new System.Drawing.Point(0, 0);
            this.darkSectionPanel1.MaximumSize = new System.Drawing.Size(280, 10000);
            this.darkSectionPanel1.MinimumSize = new System.Drawing.Size(280, 120);
            this.darkSectionPanel1.Name = "darkSectionPanel1";
            this.darkSectionPanel1.SectionHeader = "Animation List";
            this.darkSectionPanel1.Size = new System.Drawing.Size(280, 281);
            this.darkSectionPanel1.TabIndex = 9;
            // 
            // butShowAll
            // 
            this.butShowAll.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.butShowAll.Image = global::WadTool.Properties.Resources.actions_delete_16;
            this.butShowAll.Location = new System.Drawing.Point(253, 28);
            this.butShowAll.Name = "butShowAll";
            this.butShowAll.Size = new System.Drawing.Size(22, 22);
            this.butShowAll.TabIndex = 124;
            this.toolTip1.SetToolTip(this.butShowAll, "Reset filtering");
            this.butShowAll.Click += new System.EventHandler(this.butShowAll_Click);
            // 
            // butDeleteAnimation
            // 
            this.butDeleteAnimation.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.butDeleteAnimation.Image = ((System.Drawing.Image)(resources.GetObject("butDeleteAnimation.Image")));
            this.butDeleteAnimation.Location = new System.Drawing.Point(252, 253);
            this.butDeleteAnimation.Name = "butDeleteAnimation";
            this.butDeleteAnimation.Size = new System.Drawing.Size(23, 24);
            this.butDeleteAnimation.TabIndex = 23;
            this.toolTip1.SetToolTip(this.butDeleteAnimation, "Delete animation");
            this.butDeleteAnimation.Click += new System.EventHandler(this.butDeleteAnimation_Click);
            // 
            // darkButton1
            // 
            this.darkButton1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.darkButton1.Image = ((System.Drawing.Image)(resources.GetObject("darkButton1.Image")));
            this.darkButton1.Location = new System.Drawing.Point(228, 28);
            this.darkButton1.Name = "darkButton1";
            this.darkButton1.Size = new System.Drawing.Size(22, 22);
            this.darkButton1.TabIndex = 122;
            this.toolTip1.SetToolTip(this.darkButton1, "Filter list.\r\nNumerical input - filter by state ID\r\nString input - filter by name" +
        "");
            this.darkButton1.Click += new System.EventHandler(this.butSearchByStateID_Click);
            // 
            // butAddNewAnimation
            // 
            this.butAddNewAnimation.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.butAddNewAnimation.Image = ((System.Drawing.Image)(resources.GetObject("butAddNewAnimation.Image")));
            this.butAddNewAnimation.Location = new System.Drawing.Point(225, 253);
            this.butAddNewAnimation.Name = "butAddNewAnimation";
            this.butAddNewAnimation.Size = new System.Drawing.Size(23, 24);
            this.butAddNewAnimation.TabIndex = 93;
            this.toolTip1.SetToolTip(this.butAddNewAnimation, "Add new animation");
            this.butAddNewAnimation.Click += new System.EventHandler(this.butAddNewAnimation_Click);
            // 
            // panelView
            // 
            this.panelView.Controls.Add(this.panelRendering);
            this.panelView.Controls.Add(this.panelTimeline);
            this.panelView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelView.Location = new System.Drawing.Point(284, 4);
            this.panelView.Name = "panelView";
            this.panelView.SectionHeader = null;
            this.panelView.Size = new System.Drawing.Size(687, 590);
            this.panelView.TabIndex = 10;
            // 
            // panelRendering
            // 
            this.panelRendering.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelRendering.Location = new System.Drawing.Point(1, 1);
            this.panelRendering.Name = "panelRendering";
            this.panelRendering.Size = new System.Drawing.Size(685, 550);
            this.panelRendering.TabIndex = 9;
            this.panelRendering.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.panelRendering_MouseDoubleClick);
            this.panelRendering.MouseEnter += new System.EventHandler(this.panelRendering_MouseEnter);
            this.panelRendering.MouseMove += new System.Windows.Forms.MouseEventHandler(this.panelRendering_MouseMove);
            // 
            // panelTimeline
            // 
            this.panelTimeline.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.panelTimeline.Controls.Add(this.timeline);
            this.panelTimeline.Controls.Add(this.panelTransport);
            this.panelTimeline.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panelTimeline.Location = new System.Drawing.Point(1, 551);
            this.panelTimeline.Name = "panelTimeline";
            this.panelTimeline.Size = new System.Drawing.Size(685, 38);
            this.panelTimeline.TabIndex = 8;
            // 
            // timeline
            // 
            this.timeline.Dock = System.Windows.Forms.DockStyle.Fill;
            this.timeline.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.timeline.Location = new System.Drawing.Point(0, 0);
            this.timeline.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.timeline.Maximum = 0;
            this.timeline.Minimum = 0;
            this.timeline.Name = "timeline";
            this.timeline.SelectionEnd = 0;
            this.timeline.SelectionStart = 0;
            this.timeline.Size = new System.Drawing.Size(438, 38);
            this.timeline.TabIndex = 3;
            this.timeline.Value = 0;
            this.timeline.ValueChanged += new System.EventHandler(this.timeline_ValueChanged);
            this.timeline.SelectionChanged += new System.EventHandler(this.timeline_SelectionChanged);
            this.timeline.MouseDown += new System.Windows.Forms.MouseEventHandler(this.timeline_MouseDown);
            // 
            // panelTransport
            // 
            this.panelTransport.Controls.Add(this.darkToolStrip1);
            this.panelTransport.Dock = System.Windows.Forms.DockStyle.Right;
            this.panelTransport.Location = new System.Drawing.Point(438, 0);
            this.panelTransport.Name = "panelTransport";
            this.panelTransport.Size = new System.Drawing.Size(247, 38);
            this.panelTransport.TabIndex = 2;
            // 
            // darkToolStrip1
            // 
            this.darkToolStrip1.AutoSize = false;
            this.darkToolStrip1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.darkToolStrip1.CanOverflow = false;
            this.darkToolStrip1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.darkToolStrip1.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.darkToolStrip1.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.darkToolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripSeparator6,
            this.butTransportStart,
            this.butTransportFrameBack,
            this.butTransportPlay,
            this.butTransportFrameForward,
            this.butTransportEnd,
            this.toolStripSeparator7,
            this.butTransportChained,
            this.butTransportSound,
            this.butTransportLandWater});
            this.darkToolStrip1.Location = new System.Drawing.Point(0, 0);
            this.darkToolStrip1.Name = "darkToolStrip1";
            this.darkToolStrip1.Padding = new System.Windows.Forms.Padding(3, 0, 1, 0);
            this.darkToolStrip1.Size = new System.Drawing.Size(247, 38);
            this.darkToolStrip1.TabIndex = 0;
            this.darkToolStrip1.Text = "darkToolStrip1";
            // 
            // toolStripSeparator6
            // 
            this.toolStripSeparator6.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.toolStripSeparator6.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.toolStripSeparator6.Margin = new System.Windows.Forms.Padding(0, 0, 2, 0);
            this.toolStripSeparator6.Name = "toolStripSeparator6";
            this.toolStripSeparator6.Size = new System.Drawing.Size(6, 38);
            // 
            // butTransportStart
            // 
            this.butTransportStart.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.butTransportStart.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.butTransportStart.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.butTransportStart.Image = ((System.Drawing.Image)(resources.GetObject("butTransportStart.Image")));
            this.butTransportStart.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.butTransportStart.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.butTransportStart.Name = "butTransportStart";
            this.butTransportStart.Size = new System.Drawing.Size(28, 35);
            this.butTransportStart.ToolTipText = "Go to start";
            this.butTransportStart.Click += new System.EventHandler(this.butTransportStart_Click);
            // 
            // butTransportFrameBack
            // 
            this.butTransportFrameBack.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.butTransportFrameBack.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.butTransportFrameBack.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.butTransportFrameBack.Image = ((System.Drawing.Image)(resources.GetObject("butTransportFrameBack.Image")));
            this.butTransportFrameBack.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.butTransportFrameBack.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.butTransportFrameBack.Name = "butTransportFrameBack";
            this.butTransportFrameBack.Size = new System.Drawing.Size(28, 35);
            this.butTransportFrameBack.ToolTipText = "Back 1 frame";
            this.butTransportFrameBack.Click += new System.EventHandler(this.butTransportFrameBack_Click);
            // 
            // butTransportPlay
            // 
            this.butTransportPlay.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.butTransportPlay.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.butTransportPlay.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.butTransportPlay.Image = ((System.Drawing.Image)(resources.GetObject("butTransportPlay.Image")));
            this.butTransportPlay.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.butTransportPlay.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.butTransportPlay.Name = "butTransportPlay";
            this.butTransportPlay.Size = new System.Drawing.Size(28, 35);
            this.butTransportPlay.ToolTipText = "Playback";
            this.butTransportPlay.Click += new System.EventHandler(this.butTransportPlay_Click);
            // 
            // butTransportFrameForward
            // 
            this.butTransportFrameForward.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.butTransportFrameForward.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.butTransportFrameForward.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.butTransportFrameForward.Image = ((System.Drawing.Image)(resources.GetObject("butTransportFrameForward.Image")));
            this.butTransportFrameForward.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.butTransportFrameForward.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.butTransportFrameForward.Name = "butTransportFrameForward";
            this.butTransportFrameForward.Size = new System.Drawing.Size(28, 35);
            this.butTransportFrameForward.ToolTipText = "Forward 1 frame";
            this.butTransportFrameForward.Click += new System.EventHandler(this.butTransportFrameForward_Click);
            // 
            // butTransportEnd
            // 
            this.butTransportEnd.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.butTransportEnd.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.butTransportEnd.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.butTransportEnd.Image = ((System.Drawing.Image)(resources.GetObject("butTransportEnd.Image")));
            this.butTransportEnd.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.butTransportEnd.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.butTransportEnd.Name = "butTransportEnd";
            this.butTransportEnd.Size = new System.Drawing.Size(28, 35);
            this.butTransportEnd.ToolTipText = "Go to end";
            this.butTransportEnd.Click += new System.EventHandler(this.butTransportEnd_Click);
            // 
            // toolStripSeparator7
            // 
            this.toolStripSeparator7.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.toolStripSeparator7.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.toolStripSeparator7.Margin = new System.Windows.Forms.Padding(0, 0, 2, 0);
            this.toolStripSeparator7.Name = "toolStripSeparator7";
            this.toolStripSeparator7.Size = new System.Drawing.Size(6, 38);
            // 
            // butTransportChained
            // 
            this.butTransportChained.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.butTransportChained.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.butTransportChained.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.butTransportChained.Image = ((System.Drawing.Image)(resources.GetObject("butTransportChained.Image")));
            this.butTransportChained.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.butTransportChained.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.butTransportChained.Name = "butTransportChained";
            this.butTransportChained.Size = new System.Drawing.Size(28, 35);
            this.butTransportChained.ToolTipText = "Chain playback";
            this.butTransportChained.Click += new System.EventHandler(this.transportChained_Click);
            // 
            // butTransportSound
            // 
            this.butTransportSound.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.butTransportSound.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.butTransportSound.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.butTransportSound.Image = ((System.Drawing.Image)(resources.GetObject("butTransportSound.Image")));
            this.butTransportSound.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.butTransportSound.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.butTransportSound.Name = "butTransportSound";
            this.butTransportSound.Size = new System.Drawing.Size(28, 35);
            this.butTransportSound.ToolTipText = "Toggle sound preview";
            this.butTransportSound.Click += new System.EventHandler(this.butTransportSound_Click);
            // 
            // butTransportLandWater
            // 
            this.butTransportLandWater.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.butTransportLandWater.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.butTransportLandWater.DoubleClickEnabled = true;
            this.butTransportLandWater.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.butTransportLandWater.Image = ((System.Drawing.Image)(resources.GetObject("butTransportLandWater.Image")));
            this.butTransportLandWater.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.butTransportLandWater.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.butTransportLandWater.Name = "butTransportLandWater";
            this.butTransportLandWater.Size = new System.Drawing.Size(28, 35);
            this.butTransportLandWater.ToolTipText = "Toggle sound conditions";
            this.butTransportLandWater.Click += new System.EventHandler(this.butTransportLandWater_Click);
            // 
            // darkSectionPanel4
            // 
            this.darkSectionPanel4.Controls.Add(this.darkLabel2);
            this.darkSectionPanel4.Controls.Add(this.tbEndHorVel);
            this.darkSectionPanel4.Controls.Add(this.tbStartHorVel);
            this.darkSectionPanel4.Controls.Add(this.tbEndVertVel);
            this.darkSectionPanel4.Controls.Add(this.butClearAnimCollision);
            this.darkSectionPanel4.Controls.Add(this.tbStartVertVel);
            this.darkSectionPanel4.Controls.Add(this.butCalculateAnimCollision);
            this.darkSectionPanel4.Controls.Add(this.nudNextFrame);
            this.darkSectionPanel4.Controls.Add(this.nudNextAnim);
            this.darkSectionPanel4.Controls.Add(this.tbStateId);
            this.darkSectionPanel4.Controls.Add(this.nudFramerate);
            this.darkSectionPanel4.Controls.Add(this.butSearchStateID);
            this.darkSectionPanel4.Controls.Add(this.darkLabel25);
            this.darkSectionPanel4.Controls.Add(this.cmbStateID);
            this.darkSectionPanel4.Controls.Add(this.darkButton3);
            this.darkSectionPanel4.Controls.Add(this.darkLabel24);
            this.darkSectionPanel4.Controls.Add(this.darkLabel23);
            this.darkSectionPanel4.Controls.Add(this.darkLabel22);
            this.darkSectionPanel4.Controls.Add(this.butEditStateChanges);
            this.darkSectionPanel4.Controls.Add(this.tbName);
            this.darkSectionPanel4.Controls.Add(this.darkLabel3);
            this.darkSectionPanel4.Controls.Add(this.darkLabel4);
            this.darkSectionPanel4.Controls.Add(this.darkLabel5);
            this.darkSectionPanel4.Controls.Add(this.darkLabel6);
            this.darkSectionPanel4.Controls.Add(this.darkLabel7);
            this.darkSectionPanel4.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.darkSectionPanel4.Location = new System.Drawing.Point(0, 281);
            this.darkSectionPanel4.MaximumSize = new System.Drawing.Size(280, 238);
            this.darkSectionPanel4.Name = "darkSectionPanel4";
            this.darkSectionPanel4.SectionHeader = "Current Animation";
            this.darkSectionPanel4.Size = new System.Drawing.Size(280, 197);
            this.darkSectionPanel4.TabIndex = 127;
            // 
            // darkLabel2
            // 
            this.darkLabel2.AutoSize = true;
            this.darkLabel2.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.darkLabel2.Location = new System.Drawing.Point(207, 82);
            this.darkLabel2.Name = "darkLabel2";
            this.darkLabel2.Size = new System.Drawing.Size(50, 13);
            this.darkLabel2.TabIndex = 96;
            this.darkLabel2.Text = "Bounds:";
            // 
            // tbEndHorVel
            // 
            this.tbEndHorVel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.tbEndHorVel.Location = new System.Drawing.Point(211, 141);
            this.tbEndHorVel.Name = "tbEndHorVel";
            this.tbEndHorVel.Size = new System.Drawing.Size(64, 22);
            this.tbEndHorVel.TabIndex = 132;
            this.tbEndHorVel.Validated += new System.EventHandler(this.tbEndHorVel_Validated);
            // 
            // tbStartHorVel
            // 
            this.tbStartHorVel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.tbStartHorVel.Location = new System.Drawing.Point(142, 141);
            this.tbStartHorVel.Name = "tbStartHorVel";
            this.tbStartHorVel.Size = new System.Drawing.Size(64, 22);
            this.tbStartHorVel.TabIndex = 131;
            this.tbStartHorVel.Validated += new System.EventHandler(this.tbStartHorVel_Validated);
            // 
            // tbEndVertVel
            // 
            this.tbEndVertVel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.tbEndVertVel.Location = new System.Drawing.Point(73, 141);
            this.tbEndVertVel.Name = "tbEndVertVel";
            this.tbEndVertVel.Size = new System.Drawing.Size(64, 22);
            this.tbEndVertVel.TabIndex = 130;
            this.tbEndVertVel.Validated += new System.EventHandler(this.tbEndVertVel_Validated);
            // 
            // butClearAnimCollision
            // 
            this.butClearAnimCollision.Image = global::WadTool.Properties.Resources.actions_delete_16;
            this.butClearAnimCollision.Location = new System.Drawing.Point(246, 98);
            this.butClearAnimCollision.Name = "butClearAnimCollision";
            this.butClearAnimCollision.Size = new System.Drawing.Size(29, 22);
            this.butClearAnimCollision.TabIndex = 93;
            this.toolTip1.SetToolTip(this.butClearAnimCollision, "Clear bounding box for current animation");
            this.butClearAnimCollision.Click += new System.EventHandler(this.butClearAnimCollision_Click);
            // 
            // tbStartVertVel
            // 
            this.tbStartVertVel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.tbStartVertVel.Location = new System.Drawing.Point(4, 141);
            this.tbStartVertVel.Name = "tbStartVertVel";
            this.tbStartVertVel.Size = new System.Drawing.Size(64, 22);
            this.tbStartVertVel.TabIndex = 97;
            this.tbStartVertVel.Validated += new System.EventHandler(this.tbStartVertVel_Validated);
            // 
            // butCalculateAnimCollision
            // 
            this.butCalculateAnimCollision.Image = ((System.Drawing.Image)(resources.GetObject("butCalculateAnimCollision.Image")));
            this.butCalculateAnimCollision.Location = new System.Drawing.Point(211, 98);
            this.butCalculateAnimCollision.Name = "butCalculateAnimCollision";
            this.butCalculateAnimCollision.Size = new System.Drawing.Size(30, 22);
            this.butCalculateAnimCollision.TabIndex = 92;
            this.toolTip1.SetToolTip(this.butCalculateAnimCollision, "Calculate bounding box for current animation");
            this.butCalculateAnimCollision.Click += new System.EventHandler(this.butCalculateAnimCollision_Click);
            // 
            // nudNextFrame
            // 
            this.nudNextFrame.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(69)))), ((int)(((byte)(73)))), ((int)(((byte)(74)))));
            this.nudNextFrame.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.nudNextFrame.IncrementAlternate = new decimal(new int[] {
            10,
            0,
            0,
            65536});
            this.nudNextFrame.Location = new System.Drawing.Point(142, 98);
            this.nudNextFrame.Maximum = new decimal(new int[] {
            32767,
            0,
            0,
            0});
            this.nudNextFrame.MousewheelSingleIncrement = true;
            this.nudNextFrame.Name = "nudNextFrame";
            this.nudNextFrame.Size = new System.Drawing.Size(64, 22);
            this.nudNextFrame.TabIndex = 128;
            this.nudNextFrame.Validated += new System.EventHandler(this.nudNextFrame_Validated);
            // 
            // nudNextAnim
            // 
            this.nudNextAnim.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(69)))), ((int)(((byte)(73)))), ((int)(((byte)(74)))));
            this.nudNextAnim.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.nudNextAnim.IncrementAlternate = new decimal(new int[] {
            10,
            0,
            0,
            65536});
            this.nudNextAnim.Location = new System.Drawing.Point(73, 98);
            this.nudNextAnim.Maximum = new decimal(new int[] {
            32767,
            0,
            0,
            0});
            this.nudNextAnim.MousewheelSingleIncrement = true;
            this.nudNextAnim.Name = "nudNextAnim";
            this.nudNextAnim.Size = new System.Drawing.Size(64, 22);
            this.nudNextAnim.TabIndex = 127;
            this.nudNextAnim.Validated += new System.EventHandler(this.nudNextAnim_Validated);
            // 
            // nudFramerate
            // 
            this.nudFramerate.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(69)))), ((int)(((byte)(73)))), ((int)(((byte)(74)))));
            this.nudFramerate.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.nudFramerate.IncrementAlternate = new decimal(new int[] {
            10,
            0,
            0,
            65536});
            this.nudFramerate.Location = new System.Drawing.Point(4, 98);
            this.nudFramerate.Maximum = new decimal(new int[] {
            255,
            0,
            0,
            0});
            this.nudFramerate.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.nudFramerate.MousewheelSingleIncrement = true;
            this.nudFramerate.Name = "nudFramerate";
            this.nudFramerate.Size = new System.Drawing.Size(64, 22);
            this.nudFramerate.TabIndex = 126;
            this.nudFramerate.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.nudFramerate.Validated += new System.EventHandler(this.nudFramerate_Validated);
            // 
            // butSearchStateID
            // 
            this.butSearchStateID.Image = ((System.Drawing.Image)(resources.GetObject("butSearchStateID.Image")));
            this.butSearchStateID.Location = new System.Drawing.Point(252, 56);
            this.butSearchStateID.Name = "butSearchStateID";
            this.butSearchStateID.Size = new System.Drawing.Size(23, 23);
            this.butSearchStateID.TabIndex = 125;
            this.butSearchStateID.Click += new System.EventHandler(this.butSearchStateID_Click);
            // 
            // cmbStateID
            // 
            this.cmbStateID.FormattingEnabled = true;
            this.cmbStateID.Location = new System.Drawing.Point(44, 56);
            this.cmbStateID.Name = "cmbStateID";
            this.cmbStateID.Size = new System.Drawing.Size(209, 23);
            this.cmbStateID.TabIndex = 124;
            this.cmbStateID.SelectedIndexChanged += new System.EventHandler(this.cmbStateID_SelectedIndexChanged);
            // 
            // darkButton3
            // 
            this.darkButton3.Location = new System.Drawing.Point(142, 169);
            this.darkButton3.Name = "darkButton3";
            this.darkButton3.Size = new System.Drawing.Size(133, 23);
            this.darkButton3.TabIndex = 123;
            this.darkButton3.Text = "Anim commands...";
            this.darkButton3.Click += new System.EventHandler(this.butEditAnimCommands_Click);
            // 
            // butEditStateChanges
            // 
            this.butEditStateChanges.Location = new System.Drawing.Point(5, 169);
            this.butEditStateChanges.Name = "butEditStateChanges";
            this.butEditStateChanges.Size = new System.Drawing.Size(132, 23);
            this.butEditStateChanges.TabIndex = 122;
            this.butEditStateChanges.Text = "State changes...";
            this.butEditStateChanges.Click += new System.EventHandler(this.butEditStateChanges_Click);
            // 
            // panelMain
            // 
            this.panelMain.Controls.Add(this.panelView);
            this.panelMain.Controls.Add(this.panelTools);
            this.panelMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelMain.Location = new System.Drawing.Point(0, 52);
            this.panelMain.Name = "panelMain";
            this.panelMain.Padding = new System.Windows.Forms.Padding(4);
            this.panelMain.Size = new System.Drawing.Size(975, 598);
            this.panelMain.TabIndex = 129;
            // 
            // panelTools
            // 
            this.panelTools.Controls.Add(this.darkSectionPanel1);
            this.panelTools.Controls.Add(this.darkSectionPanel4);
            this.panelTools.Controls.Add(this.panelTransform);
            this.panelTools.Dock = System.Windows.Forms.DockStyle.Left;
            this.panelTools.Location = new System.Drawing.Point(4, 4);
            this.panelTools.MaximumSize = new System.Drawing.Size(280, 10000);
            this.panelTools.MinimumSize = new System.Drawing.Size(280, 289);
            this.panelTools.Name = "panelTools";
            this.panelTools.Size = new System.Drawing.Size(280, 590);
            this.panelTools.TabIndex = 0;
            // 
            // panelTransform
            // 
            this.panelTransform.Controls.Add(this.darkLabel8);
            this.panelTransform.Controls.Add(this.picTransformPreview);
            this.panelTransform.Controls.Add(this.cmbTransformMode);
            this.panelTransform.Controls.Add(this.darkLabel29);
            this.panelTransform.Controls.Add(this.darkLabel28);
            this.panelTransform.Controls.Add(this.darkLabel21);
            this.panelTransform.Controls.Add(this.darkLabel26);
            this.panelTransform.Controls.Add(this.nudTransX);
            this.panelTransform.Controls.Add(this.darkLabel27);
            this.panelTransform.Controls.Add(this.nudTransY);
            this.panelTransform.Controls.Add(this.nudTransZ);
            this.panelTransform.Controls.Add(this.darkLabel1);
            this.panelTransform.Controls.Add(this.darkLabel18);
            this.panelTransform.Controls.Add(this.nudRotX);
            this.panelTransform.Controls.Add(this.darkLabel19);
            this.panelTransform.Controls.Add(this.nudRotY);
            this.panelTransform.Controls.Add(this.nudRotZ);
            this.panelTransform.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panelTransform.Location = new System.Drawing.Point(0, 478);
            this.panelTransform.Name = "panelTransform";
            this.panelTransform.SectionHeader = "Transform";
            this.panelTransform.Size = new System.Drawing.Size(280, 112);
            this.panelTransform.TabIndex = 130;
            // 
            // darkLabel8
            // 
            this.darkLabel8.AutoSize = true;
            this.darkLabel8.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.darkLabel8.Location = new System.Drawing.Point(2, 87);
            this.darkLabel8.Name = "darkLabel8";
            this.darkLabel8.Size = new System.Drawing.Size(40, 13);
            this.darkLabel8.TabIndex = 101;
            this.darkLabel8.Text = "Mode:";
            // 
            // picTransformPreview
            // 
            this.picTransformPreview.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.picTransformPreview.Location = new System.Drawing.Point(198, 84);
            this.picTransformPreview.Name = "picTransformPreview";
            this.picTransformPreview.Size = new System.Drawing.Size(77, 23);
            this.picTransformPreview.TabIndex = 100;
            this.picTransformPreview.TabStop = false;
            this.toolTip1.SetToolTip(this.picTransformPreview, "Transform graph preview");
            // 
            // cmbTransformMode
            // 
            this.cmbTransformMode.FormattingEnabled = true;
            this.cmbTransformMode.Items.AddRange(new object[] {
            "None",
            "Linear",
            "Smooth",
            "Symmetric"});
            this.cmbTransformMode.Location = new System.Drawing.Point(44, 84);
            this.cmbTransformMode.Name = "cmbTransformMode";
            this.cmbTransformMode.Size = new System.Drawing.Size(147, 23);
            this.cmbTransformMode.TabIndex = 99;
            this.toolTip1.SetToolTip(this.cmbTransformMode, "Transform interpolation mode");
            this.cmbTransformMode.SelectedIndexChanged += new System.EventHandler(this.cmbTransformMode_SelectedIndexChanged);
            // 
            // darkLabel29
            // 
            this.darkLabel29.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.darkLabel29.Location = new System.Drawing.Point(2, 58);
            this.darkLabel29.Name = "darkLabel29";
            this.darkLabel29.Size = new System.Drawing.Size(28, 13);
            this.darkLabel29.TabIndex = 98;
            this.darkLabel29.Text = "Pos:";
            // 
            // darkLabel28
            // 
            this.darkLabel28.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.darkLabel28.Location = new System.Drawing.Point(2, 30);
            this.darkLabel28.Name = "darkLabel28";
            this.darkLabel28.Size = new System.Drawing.Size(29, 13);
            this.darkLabel28.TabIndex = 97;
            this.darkLabel28.Text = "Rot:";
            // 
            // darkLabel21
            // 
            this.darkLabel21.AutoSize = true;
            this.darkLabel21.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.darkLabel21.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.darkLabel21.ForeColor = System.Drawing.Color.LightGray;
            this.darkLabel21.Location = new System.Drawing.Point(195, 58);
            this.darkLabel21.Name = "darkLabel21";
            this.darkLabel21.Size = new System.Drawing.Size(14, 13);
            this.darkLabel21.TabIndex = 20;
            this.darkLabel21.Text = "Z";
            // 
            // darkLabel26
            // 
            this.darkLabel26.AutoSize = true;
            this.darkLabel26.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.darkLabel26.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.darkLabel26.ForeColor = System.Drawing.Color.LightGray;
            this.darkLabel26.Location = new System.Drawing.Point(112, 58);
            this.darkLabel26.Name = "darkLabel26";
            this.darkLabel26.Size = new System.Drawing.Size(14, 13);
            this.darkLabel26.TabIndex = 19;
            this.darkLabel26.Text = "Y";
            // 
            // nudTransX
            // 
            this.nudTransX.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(69)))), ((int)(((byte)(73)))), ((int)(((byte)(74)))));
            this.nudTransX.DecimalPlaces = 4;
            this.nudTransX.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.nudTransX.IncrementAlternate = new decimal(new int[] {
            160,
            0,
            0,
            65536});
            this.nudTransX.Location = new System.Drawing.Point(44, 56);
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
            this.nudTransX.MousewheelSingleIncrement = true;
            this.nudTransX.Name = "nudTransX";
            this.nudTransX.Size = new System.Drawing.Size(64, 22);
            this.nudTransX.TabIndex = 15;
            this.nudTransX.ValueChanged += new System.EventHandler(this.nudTransX_ValueChanged);
            // 
            // darkLabel27
            // 
            this.darkLabel27.AutoSize = true;
            this.darkLabel27.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.darkLabel27.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.darkLabel27.ForeColor = System.Drawing.Color.LightGray;
            this.darkLabel27.Location = new System.Drawing.Point(28, 58);
            this.darkLabel27.Name = "darkLabel27";
            this.darkLabel27.Size = new System.Drawing.Size(14, 13);
            this.darkLabel27.TabIndex = 18;
            this.darkLabel27.Text = "X";
            // 
            // nudTransY
            // 
            this.nudTransY.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(69)))), ((int)(((byte)(73)))), ((int)(((byte)(74)))));
            this.nudTransY.DecimalPlaces = 4;
            this.nudTransY.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.nudTransY.IncrementAlternate = new decimal(new int[] {
            160,
            0,
            0,
            65536});
            this.nudTransY.Location = new System.Drawing.Point(127, 56);
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
            this.nudTransY.MousewheelSingleIncrement = true;
            this.nudTransY.Name = "nudTransY";
            this.nudTransY.Size = new System.Drawing.Size(64, 22);
            this.nudTransY.TabIndex = 16;
            this.nudTransY.ValueChanged += new System.EventHandler(this.nudTransY_ValueChanged);
            // 
            // nudTransZ
            // 
            this.nudTransZ.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(69)))), ((int)(((byte)(73)))), ((int)(((byte)(74)))));
            this.nudTransZ.DecimalPlaces = 4;
            this.nudTransZ.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.nudTransZ.IncrementAlternate = new decimal(new int[] {
            160,
            0,
            0,
            65536});
            this.nudTransZ.Location = new System.Drawing.Point(210, 56);
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
            this.nudTransZ.MousewheelSingleIncrement = true;
            this.nudTransZ.Name = "nudTransZ";
            this.nudTransZ.Size = new System.Drawing.Size(65, 22);
            this.nudTransZ.TabIndex = 17;
            this.nudTransZ.ValueChanged += new System.EventHandler(this.nudTransZ_ValueChanged);
            // 
            // darkLabel1
            // 
            this.darkLabel1.AutoSize = true;
            this.darkLabel1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.darkLabel1.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.darkLabel1.ForeColor = System.Drawing.Color.LightGray;
            this.darkLabel1.Location = new System.Drawing.Point(195, 30);
            this.darkLabel1.Name = "darkLabel1";
            this.darkLabel1.Size = new System.Drawing.Size(14, 13);
            this.darkLabel1.TabIndex = 14;
            this.darkLabel1.Text = "R";
            // 
            // darkLabel18
            // 
            this.darkLabel18.AutoSize = true;
            this.darkLabel18.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.darkLabel18.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.darkLabel18.ForeColor = System.Drawing.Color.LightGray;
            this.darkLabel18.Location = new System.Drawing.Point(112, 30);
            this.darkLabel18.Name = "darkLabel18";
            this.darkLabel18.Size = new System.Drawing.Size(14, 13);
            this.darkLabel18.TabIndex = 13;
            this.darkLabel18.Text = "P";
            // 
            // nudRotX
            // 
            this.nudRotX.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(69)))), ((int)(((byte)(73)))), ((int)(((byte)(74)))));
            this.nudRotX.DecimalPlaces = 4;
            this.nudRotX.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.nudRotX.IncrementAlternate = new decimal(new int[] {
            50,
            0,
            0,
            65536});
            this.nudRotX.Location = new System.Drawing.Point(44, 28);
            this.nudRotX.Maximum = new decimal(new int[] {
            360,
            0,
            0,
            0});
            this.nudRotX.Minimum = new decimal(new int[] {
            360,
            0,
            0,
            -2147483648});
            this.nudRotX.MousewheelSingleIncrement = true;
            this.nudRotX.Name = "nudRotX";
            this.nudRotX.Size = new System.Drawing.Size(64, 22);
            this.nudRotX.TabIndex = 9;
            this.nudRotX.ValueChanged += new System.EventHandler(this.nudRotX_ValueChanged);
            // 
            // darkLabel19
            // 
            this.darkLabel19.AutoSize = true;
            this.darkLabel19.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.darkLabel19.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.darkLabel19.ForeColor = System.Drawing.Color.LightGray;
            this.darkLabel19.Location = new System.Drawing.Point(28, 30);
            this.darkLabel19.Name = "darkLabel19";
            this.darkLabel19.Size = new System.Drawing.Size(14, 13);
            this.darkLabel19.TabIndex = 12;
            this.darkLabel19.Text = "Y";
            // 
            // nudRotY
            // 
            this.nudRotY.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(69)))), ((int)(((byte)(73)))), ((int)(((byte)(74)))));
            this.nudRotY.DecimalPlaces = 4;
            this.nudRotY.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.nudRotY.IncrementAlternate = new decimal(new int[] {
            50,
            0,
            0,
            65536});
            this.nudRotY.Location = new System.Drawing.Point(127, 28);
            this.nudRotY.Maximum = new decimal(new int[] {
            360,
            0,
            0,
            0});
            this.nudRotY.Minimum = new decimal(new int[] {
            360,
            0,
            0,
            -2147483648});
            this.nudRotY.MousewheelSingleIncrement = true;
            this.nudRotY.Name = "nudRotY";
            this.nudRotY.Size = new System.Drawing.Size(64, 22);
            this.nudRotY.TabIndex = 10;
            this.nudRotY.ValueChanged += new System.EventHandler(this.nudRotY_ValueChanged);
            // 
            // nudRotZ
            // 
            this.nudRotZ.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(69)))), ((int)(((byte)(73)))), ((int)(((byte)(74)))));
            this.nudRotZ.DecimalPlaces = 4;
            this.nudRotZ.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.nudRotZ.IncrementAlternate = new decimal(new int[] {
            50,
            0,
            0,
            65536});
            this.nudRotZ.Location = new System.Drawing.Point(211, 28);
            this.nudRotZ.Maximum = new decimal(new int[] {
            360,
            0,
            0,
            0});
            this.nudRotZ.Minimum = new decimal(new int[] {
            360,
            0,
            0,
            -2147483648});
            this.nudRotZ.MousewheelSingleIncrement = true;
            this.nudRotZ.Name = "nudRotZ";
            this.nudRotZ.Size = new System.Drawing.Size(64, 22);
            this.nudRotZ.TabIndex = 11;
            this.nudRotZ.ValueChanged += new System.EventHandler(this.nudRotZ_ValueChanged);
            // 
            // toolStripButton6
            // 
            this.toolStripButton6.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.toolStripButton6.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripButton6.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.toolStripButton6.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButton6.Image")));
            this.toolStripButton6.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.toolStripButton6.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton6.Name = "toolStripButton6";
            this.toolStripButton6.Size = new System.Drawing.Size(28, 35);
            // 
            // darkContextMenu1
            // 
            this.darkContextMenu1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.darkContextMenu1.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.darkContextMenu1.Name = "darkContextMenu1";
            this.darkContextMenu1.Size = new System.Drawing.Size(61, 4);
            // 
            // cmTimelineContextMenu
            // 
            this.cmTimelineContextMenu.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.cmTimelineContextMenu.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.cmTimelineContextMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.cmMarkInMenuItem,
            this.cmMarkOutMenuItem,
            this.cmSelectAllMenuItem,
            this.cnClearSelectionMenuItem,
            this.toolStripSeparator8,
            this.cmCreateAnimCommandMenuItem,
            this.cmCreateStateChangeMenuItem});
            this.cmTimelineContextMenu.Name = "cmTimelineContextMenu";
            this.cmTimelineContextMenu.Size = new System.Drawing.Size(206, 143);
            // 
            // cmMarkInMenuItem
            // 
            this.cmMarkInMenuItem.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.cmMarkInMenuItem.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.cmMarkInMenuItem.Name = "cmMarkInMenuItem";
            this.cmMarkInMenuItem.Size = new System.Drawing.Size(205, 22);
            this.cmMarkInMenuItem.Text = "Mark in";
            this.cmMarkInMenuItem.Click += new System.EventHandler(this.cmMarkInMenuItem_Click);
            // 
            // cmMarkOutMenuItem
            // 
            this.cmMarkOutMenuItem.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.cmMarkOutMenuItem.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.cmMarkOutMenuItem.Name = "cmMarkOutMenuItem";
            this.cmMarkOutMenuItem.Size = new System.Drawing.Size(205, 22);
            this.cmMarkOutMenuItem.Text = "Mark out";
            this.cmMarkOutMenuItem.Click += new System.EventHandler(this.cmMarkOutMenuItem_Click);
            // 
            // cmSelectAllMenuItem
            // 
            this.cmSelectAllMenuItem.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.cmSelectAllMenuItem.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.cmSelectAllMenuItem.Name = "cmSelectAllMenuItem";
            this.cmSelectAllMenuItem.Size = new System.Drawing.Size(205, 22);
            this.cmSelectAllMenuItem.Text = "Select all";
            this.cmSelectAllMenuItem.Click += new System.EventHandler(this.cmSelectAllMenuItem_Click);
            // 
            // cnClearSelectionMenuItem
            // 
            this.cnClearSelectionMenuItem.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.cnClearSelectionMenuItem.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.cnClearSelectionMenuItem.Name = "cnClearSelectionMenuItem";
            this.cnClearSelectionMenuItem.Size = new System.Drawing.Size(205, 22);
            this.cnClearSelectionMenuItem.Text = "Clear selection";
            this.cnClearSelectionMenuItem.Click += new System.EventHandler(this.cnClearSelectionMenuItem_Click);
            // 
            // toolStripSeparator8
            // 
            this.toolStripSeparator8.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.toolStripSeparator8.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.toolStripSeparator8.Margin = new System.Windows.Forms.Padding(0, 0, 0, 1);
            this.toolStripSeparator8.Name = "toolStripSeparator8";
            this.toolStripSeparator8.Size = new System.Drawing.Size(202, 6);
            // 
            // cmCreateAnimCommandMenuItem
            // 
            this.cmCreateAnimCommandMenuItem.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.cmCreateAnimCommandMenuItem.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.cmCreateAnimCommandMenuItem.Name = "cmCreateAnimCommandMenuItem";
            this.cmCreateAnimCommandMenuItem.Size = new System.Drawing.Size(205, 22);
            this.cmCreateAnimCommandMenuItem.Text = "Create anim command...";
            this.cmCreateAnimCommandMenuItem.Click += new System.EventHandler(this.cmCreateAnimCommandMenuItem_Click);
            // 
            // cmCreateStateChangeMenuItem
            // 
            this.cmCreateStateChangeMenuItem.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.cmCreateStateChangeMenuItem.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.cmCreateStateChangeMenuItem.Name = "cmCreateStateChangeMenuItem";
            this.cmCreateStateChangeMenuItem.Size = new System.Drawing.Size(205, 22);
            this.cmCreateStateChangeMenuItem.Text = "Create state change...";
            this.cmCreateStateChangeMenuItem.Click += new System.EventHandler(this.cmCreateStateChangeMenuItem_Click);
            // 
            // FormAnimationEditor
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(975, 675);
            this.Controls.Add(this.panelMain);
            this.Controls.Add(this.topBar);
            this.Controls.Add(this.darkStatusStrip1);
            this.Controls.Add(this.topMenu);
            this.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MainMenuStrip = this.topMenu;
            this.MinimumSize = new System.Drawing.Size(767, 598);
            this.Name = "FormAnimationEditor";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Animation editor";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.formAnimationEditor_FormClosing);
            this.topMenu.ResumeLayout(false);
            this.topMenu.PerformLayout();
            this.darkStatusStrip1.ResumeLayout(false);
            this.darkStatusStrip1.PerformLayout();
            this.topBar.ResumeLayout(false);
            this.topBar.PerformLayout();
            this.darkSectionPanel1.ResumeLayout(false);
            this.darkSectionPanel1.PerformLayout();
            this.panelView.ResumeLayout(false);
            this.panelTimeline.ResumeLayout(false);
            this.panelTransport.ResumeLayout(false);
            this.darkToolStrip1.ResumeLayout(false);
            this.darkToolStrip1.PerformLayout();
            this.darkSectionPanel4.ResumeLayout(false);
            this.darkSectionPanel4.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudNextFrame)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudNextAnim)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudFramerate)).EndInit();
            this.panelMain.ResumeLayout(false);
            this.panelTools.ResumeLayout(false);
            this.panelTransform.ResumeLayout(false);
            this.panelTransform.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.picTransformPreview)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudTransX)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudTransY)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudTransZ)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudRotX)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudRotY)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudRotZ)).EndInit();
            this.cmTimelineContextMenu.ResumeLayout(false);
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
        private System.Windows.Forms.ToolStripSeparator toolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem interpolateFramesToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem animationToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem addNewToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem deleteToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuItem5;
        private System.Windows.Forms.ToolStripMenuItem copyToolStripMenuItem1;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuItem2;
        private System.Windows.Forms.ToolStripMenuItem importToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem exportToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem renderingToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem drawGridToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem drawGizmoToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem drawCollisionBoxToolStripMenuItem;
        private DarkUI.Controls.DarkStatusStrip darkStatusStrip1;
        private DarkUI.Controls.DarkButton butDeleteAnimation;
        private System.Windows.Forms.ToolStripStatusLabel statusFrame;
        private DarkUI.Controls.DarkButton butAddNewAnimation;
        private DarkUI.Controls.DarkLabel darkLabel4;
        private DarkUI.Controls.DarkTextBox tbName;
        private DarkUI.Controls.DarkLabel darkLabel3;
        private TombLib.Controls.DarkAutocompleteTextBox tbStateId;
        private DarkUI.Controls.DarkLabel darkLabel7;
        private DarkUI.Controls.DarkLabel darkLabel6;
        private DarkUI.Controls.DarkLabel darkLabel5;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuItem6;
        private System.Windows.Forms.ToolStripMenuItem calculateCollisionBoxForCurrentFrameToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuItem7;
        private System.Windows.Forms.ToolStripMenuItem calculateBoundingBoxForAllFramesToolStripMenuItem;
        private DarkUI.Controls.DarkToolStrip topBar;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripLabel labelAnims;
        private System.Windows.Forms.ToolStripButton butTbAddAnimation;
        private System.Windows.Forms.ToolStripButton butTbDeleteAnimation;
        private System.Windows.Forms.ToolStripButton butTbCopyAnimation;
        private System.Windows.Forms.ToolStripButton butTbPasteAnimation;
        private System.Windows.Forms.ToolStripButton butTbCutFrame;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolStripLabel labelFrames;
        private System.Windows.Forms.ToolStripButton butTbAddFrame;
        private System.Windows.Forms.ToolStripButton butTbDeleteFrame;
        private System.Windows.Forms.ToolStripButton butTbCopyFrame;
        private System.Windows.Forms.ToolStripButton butTbPasteFrame;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator3;
        private System.Windows.Forms.ToolStripLabel labelBone;
        private System.Windows.Forms.ToolStripMenuItem insertnFramesAfterCurrentOneToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem pasteToolStripMenuItem1;
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
        private DarkUI.Controls.DarkLabel darkLabel20;
        private DarkUI.Controls.DarkLabel darkLabel22;
        private DarkUI.Controls.DarkLabel darkLabel23;
        private DarkUI.Controls.DarkLabel darkLabel24;
        private DarkUI.Controls.DarkLabel darkLabel25;
        private System.Windows.Forms.ToolStripButton butTbReplaceAnimation;
        private System.Windows.Forms.ToolStripButton butTbSplitAnimation;
        private System.Windows.Forms.ToolStripMenuItem splitAnimationToolStripMenuItem;
        private System.Windows.Forms.ToolStripLabel labelRoom;
        private DarkUI.Controls.DarkTextBox tbSearchAnimation;
        private DarkUI.Controls.DarkButton darkButton1;
        private DarkUI.Controls.DarkButton butShowAll;
        private System.Windows.Forms.ToolStripMenuItem deleteBoundingBoxForAllFramesToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem deleteCollisionBoxForCurrentFrameToolStripMenuItem;
        private DarkUI.Controls.DarkListView lstAnimations;
        private DarkUI.Controls.DarkSectionPanel darkSectionPanel1;
        private DarkUI.Controls.DarkSectionPanel panelView;
        private DarkUI.Controls.DarkSectionPanel darkSectionPanel4;
        private System.Windows.Forms.Panel panelMain;
        private System.Windows.Forms.Panel panelTools;
        private System.Windows.Forms.Panel panelTimeline;
        private System.Windows.Forms.Panel panelTransport;
        private Controls.PanelRenderingAnimationEditor panelRendering;
        private AnimationTrackBar timeline;
        private DarkUI.Controls.DarkButton darkButton3;
        private DarkUI.Controls.DarkButton butEditStateChanges;
        private System.Windows.Forms.ToolStripButton toolStripButton6;
        private DarkUI.Controls.DarkToolStrip darkToolStrip1;
        private System.Windows.Forms.ToolStripButton butTransportSound;
        private System.Windows.Forms.ToolStripButton butTransportStart;
        private System.Windows.Forms.ToolStripButton butTransportFrameBack;
        private System.Windows.Forms.ToolStripButton butTransportPlay;
        private System.Windows.Forms.ToolStripButton butTransportFrameForward;
        private System.Windows.Forms.ToolStripButton butTransportEnd;
        private DarkUI.Controls.ToolStripDarkComboBox comboBoneList;
        private DarkUI.Controls.ToolStripDarkComboBox comboRoomList;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator4;
        private System.Windows.Forms.ToolTip toolTip1;
        private System.Windows.Forms.ToolStripButton butTbSaveAllChanges;
        private System.Windows.Forms.ToolStripButton butTbInterpolateFrames;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator5;
        private System.Windows.Forms.ToolStripTextBox tbInterpolateFrameCount;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator6;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator7;
        private System.Windows.Forms.ToolStripButton butTransportLandWater;
        private DarkUI.Controls.DarkLabel darkLabel2;
        private DarkUI.Controls.DarkButton butClearAnimCollision;
        private DarkUI.Controls.DarkButton butCalculateAnimCollision;
        private System.Windows.Forms.ToolStripButton butTbUndo;
        private System.Windows.Forms.ToolStripButton butTbRedo;
        private System.Windows.Forms.ToolStripMenuItem resampleAnimationToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem resampleAnimationToKeyframesToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem undoToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem redoToolStripMenuItem;
        private System.Windows.Forms.ToolStripButton butTransportChained;
        private DarkUI.Controls.DarkComboBox cmbStateID;
        private DarkUI.Controls.DarkNumericUpDown nudNextFrame;
        private DarkUI.Controls.DarkNumericUpDown nudNextAnim;
        private DarkUI.Controls.DarkNumericUpDown nudFramerate;
        private DarkUI.Controls.DarkButton butSearchStateID;
        private DarkUI.Controls.DarkTextBox tbEndHorVel;
        private DarkUI.Controls.DarkTextBox tbStartHorVel;
        private DarkUI.Controls.DarkTextBox tbEndVertVel;
        private DarkUI.Controls.DarkTextBox tbStartVertVel;
        private System.Windows.Forms.ToolStripMenuItem deleteEveryNthFrameToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem findReplaceAnimcommandsToolStripMenuItem;
        private System.Windows.Forms.ToolStripButton butTbImport;
        private System.Windows.Forms.ToolStripMenuItem smoothAnimationsToolStripMenuItem;
        private DarkUI.Controls.DarkContextMenu darkContextMenu1;
        private DarkUI.Controls.DarkContextMenu cmTimelineContextMenu;
        private System.Windows.Forms.ToolStripMenuItem cmMarkInMenuItem;
        private System.Windows.Forms.ToolStripMenuItem cmMarkOutMenuItem;
        private System.Windows.Forms.ToolStripMenuItem cnClearSelectionMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator8;
        private System.Windows.Forms.ToolStripMenuItem cmCreateAnimCommandMenuItem;
        private System.Windows.Forms.ToolStripMenuItem cmCreateStateChangeMenuItem;
        private DarkUI.Controls.DarkSectionPanel panelTransform;
        private DarkUI.Controls.DarkLabel darkLabel1;
        private DarkUI.Controls.DarkLabel darkLabel18;
        private DarkUI.Controls.DarkNumericUpDown nudRotX;
        private DarkUI.Controls.DarkLabel darkLabel19;
        private DarkUI.Controls.DarkNumericUpDown nudRotY;
        private DarkUI.Controls.DarkNumericUpDown nudRotZ;
        private DarkUI.Controls.DarkLabel darkLabel28;
        private DarkUI.Controls.DarkLabel darkLabel21;
        private DarkUI.Controls.DarkLabel darkLabel26;
        private DarkUI.Controls.DarkNumericUpDown nudTransX;
        private DarkUI.Controls.DarkLabel darkLabel27;
        private DarkUI.Controls.DarkNumericUpDown nudTransY;
        private DarkUI.Controls.DarkNumericUpDown nudTransZ;
        private DarkUI.Controls.DarkLabel darkLabel8;
        private System.Windows.Forms.PictureBox picTransformPreview;
        private DarkUI.Controls.DarkComboBox cmbTransformMode;
        private DarkUI.Controls.DarkLabel darkLabel29;
        private System.Windows.Forms.ToolStripMenuItem cmSelectAllMenuItem;
        private System.Windows.Forms.ToolStripButton butTbResetCamera;
    }
}