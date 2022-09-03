
namespace TombEditor.Controls
{
    partial class TriggerManager
    {
        /// <summary> 
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.darkPanel3 = new DarkUI.Controls.DarkPanel();
            this.lblNotify = new DarkUI.Controls.DarkLabel();
            this.butUnassign = new DarkUI.Controls.DarkButton();
            this.butSearch = new DarkUI.Controls.DarkButton();
            this.rbNodeEditor = new DarkUI.Controls.DarkRadioButton();
            this.rbLevelScript = new DarkUI.Controls.DarkRadioButton();
            this.toolTip = new System.Windows.Forms.ToolTip(this.components);
            this.nudCallCount = new DarkUI.Controls.DarkNumericUpDown();
            this.tbArgument = new DarkUI.Controls.DarkTextBox();
            this.butAddActionNode = new DarkUI.Controls.DarkButton();
            this.butAddConditionNode = new DarkUI.Controls.DarkButton();
            this.butDeleteNode = new DarkUI.Controls.DarkButton();
            this.nudCallCount2 = new DarkUI.Controls.DarkNumericUpDown();
            this.butClearNodes = new DarkUI.Controls.DarkButton();
            this.butRenameNode = new DarkUI.Controls.DarkButton();
            this.butChangeNodeColor = new DarkUI.Controls.DarkButton();
            this.butLinkSelectedNodes = new DarkUI.Controls.DarkButton();
            this.tabbedContainer = new TombLib.Controls.DarkTabbedContainer();
            this.tabConstructor = new System.Windows.Forms.TabPage();
            this.panelNodeControls = new DarkUI.Controls.DarkPanel();
            this.darkLabel1 = new DarkUI.Controls.DarkLabel();
            this.nodeEditor = new TombLib.Controls.VisualScripting.NodeEditor();
            this.lblWait = new DarkUI.Controls.DarkLabel();
            this.tabLevelScript = new System.Windows.Forms.TabPage();
            this.lstFunctions = new DarkUI.Controls.DarkListView();
            this.panelFunctionControls = new DarkUI.Controls.DarkPanel();
            this.darkLabel3 = new DarkUI.Controls.DarkLabel();
            this.darkLabel2 = new DarkUI.Controls.DarkLabel();
            this.lblListNotify = new DarkUI.Controls.DarkLabel();
            this.butExport = new DarkUI.Controls.DarkButton();
            this.darkPanel3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudCallCount)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudCallCount2)).BeginInit();
            this.tabbedContainer.SuspendLayout();
            this.tabConstructor.SuspendLayout();
            this.panelNodeControls.SuspendLayout();
            this.tabLevelScript.SuspendLayout();
            this.panelFunctionControls.SuspendLayout();
            this.SuspendLayout();
            // 
            // darkPanel3
            // 
            this.darkPanel3.Controls.Add(this.lblNotify);
            this.darkPanel3.Controls.Add(this.butUnassign);
            this.darkPanel3.Controls.Add(this.butSearch);
            this.darkPanel3.Controls.Add(this.rbNodeEditor);
            this.darkPanel3.Controls.Add(this.rbLevelScript);
            this.darkPanel3.Dock = System.Windows.Forms.DockStyle.Top;
            this.darkPanel3.Location = new System.Drawing.Point(0, 0);
            this.darkPanel3.Name = "darkPanel3";
            this.darkPanel3.Size = new System.Drawing.Size(758, 25);
            this.darkPanel3.TabIndex = 21;
            // 
            // lblNotify
            // 
            this.lblNotify.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lblNotify.AutoEllipsis = true;
            this.lblNotify.ForeColor = System.Drawing.Color.DarkGray;
            this.lblNotify.Location = new System.Drawing.Point(236, 6);
            this.lblNotify.Name = "lblNotify";
            this.lblNotify.Size = new System.Drawing.Size(461, 13);
            this.lblNotify.TabIndex = 4;
            this.lblNotify.Text = "Event notify";
            this.lblNotify.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.toolTip.SetToolTip(this.lblNotify, "A function which was linked to current event was not found in script file.\r\nPleas" +
        "e select another one or restore linked function in script.");
            this.lblNotify.Visible = false;
            // 
            // butUnassign
            // 
            this.butUnassign.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.butUnassign.Checked = false;
            this.butUnassign.Image = global::TombEditor.Properties.Resources.actions_delete_16;
            this.butUnassign.Location = new System.Drawing.Point(703, 2);
            this.butUnassign.Name = "butUnassign";
            this.butUnassign.Size = new System.Drawing.Size(23, 23);
            this.butUnassign.TabIndex = 3;
            this.toolTip.SetToolTip(this.butUnassign, "Unassign function from event");
            this.butUnassign.Click += new System.EventHandler(this.butUnassign_Click);
            // 
            // butSearch
            // 
            this.butSearch.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.butSearch.Checked = false;
            this.butSearch.Image = global::TombEditor.Properties.Resources.general_search_16;
            this.butSearch.Location = new System.Drawing.Point(732, 2);
            this.butSearch.Name = "butSearch";
            this.butSearch.Size = new System.Drawing.Size(23, 23);
            this.butSearch.TabIndex = 2;
            this.toolTip.SetToolTip(this.butSearch, "Search for item");
            this.butSearch.Click += new System.EventHandler(this.butSearch_Click);
            // 
            // rbNodeEditor
            // 
            this.rbNodeEditor.AutoSize = true;
            this.rbNodeEditor.Checked = true;
            this.rbNodeEditor.Location = new System.Drawing.Point(5, 5);
            this.rbNodeEditor.Name = "rbNodeEditor";
            this.rbNodeEditor.Size = new System.Drawing.Size(87, 17);
            this.rbNodeEditor.TabIndex = 1;
            this.rbNodeEditor.TabStop = true;
            this.rbNodeEditor.Text = "Node editor";
            this.toolTip.SetToolTip(this.rbNodeEditor, "Visually construct a trigger");
            this.rbNodeEditor.CheckedChanged += new System.EventHandler(this.rbNodeEditor_CheckedChanged);
            // 
            // rbLevelScript
            // 
            this.rbLevelScript.AutoSize = true;
            this.rbLevelScript.Location = new System.Drawing.Point(98, 5);
            this.rbLevelScript.Name = "rbLevelScript";
            this.rbLevelScript.Size = new System.Drawing.Size(131, 17);
            this.rbLevelScript.TabIndex = 0;
            this.rbLevelScript.Text = "Level script functions";
            this.toolTip.SetToolTip(this.rbLevelScript, "Select from functions in level script file");
            this.rbLevelScript.CheckedChanged += new System.EventHandler(this.rbLevelScript_CheckedChanged);
            // 
            // nudCallCount
            // 
            this.nudCallCount.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.nudCallCount.IncrementAlternate = new decimal(new int[] {
            10,
            0,
            0,
            65536});
            this.nudCallCount.Location = new System.Drawing.Point(681, 2);
            this.nudCallCount.LoopValues = false;
            this.nudCallCount.Name = "nudCallCount";
            this.nudCallCount.Size = new System.Drawing.Size(63, 22);
            this.nudCallCount.TabIndex = 3;
            this.toolTip.SetToolTip(this.nudCallCount, "Determines how many times trigger will be called.\r\nIf 0, it means trigger will be" +
        " called infinitely.");
            this.nudCallCount.ValueChanged += new System.EventHandler(this.nudCallCount_ValueChanged);
            // 
            // tbArgument
            // 
            this.tbArgument.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tbArgument.Location = new System.Drawing.Point(63, 2);
            this.tbArgument.Name = "tbArgument";
            this.tbArgument.Size = new System.Drawing.Size(542, 22);
            this.tbArgument.TabIndex = 0;
            this.toolTip.SetToolTip(this.tbArgument, "Determine function argument, if function supports it.\r\nMust be formatted accordin" +
        "g to lua syntax.");
            this.tbArgument.Validated += new System.EventHandler(this.tbArgument_Validated);
            // 
            // butAddActionNode
            // 
            this.butAddActionNode.Checked = false;
            this.butAddActionNode.Image = global::TombEditor.Properties.Resources.general_plus_math_16;
            this.butAddActionNode.Location = new System.Drawing.Point(0, 1);
            this.butAddActionNode.Name = "butAddActionNode";
            this.butAddActionNode.Size = new System.Drawing.Size(87, 23);
            this.butAddActionNode.TabIndex = 5;
            this.butAddActionNode.Text = "Action";
            this.butAddActionNode.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.toolTip.SetToolTip(this.butAddActionNode, "Add new action node");
            this.butAddActionNode.Click += new System.EventHandler(this.butAddActionNode_Click);
            // 
            // butAddConditionNode
            // 
            this.butAddConditionNode.Checked = false;
            this.butAddConditionNode.Image = global::TombEditor.Properties.Resources.general_plus_math_16;
            this.butAddConditionNode.Location = new System.Drawing.Point(93, 1);
            this.butAddConditionNode.Name = "butAddConditionNode";
            this.butAddConditionNode.Size = new System.Drawing.Size(87, 23);
            this.butAddConditionNode.TabIndex = 6;
            this.butAddConditionNode.Text = "Condition";
            this.butAddConditionNode.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.toolTip.SetToolTip(this.butAddConditionNode, "Add new condition node");
            this.butAddConditionNode.Click += new System.EventHandler(this.butAddConditionNode_Click);
            // 
            // butDeleteNode
            // 
            this.butDeleteNode.Checked = false;
            this.butDeleteNode.Image = global::TombEditor.Properties.Resources.general_trash_16;
            this.butDeleteNode.Location = new System.Drawing.Point(273, 1);
            this.butDeleteNode.Name = "butDeleteNode";
            this.butDeleteNode.Size = new System.Drawing.Size(23, 23);
            this.butDeleteNode.TabIndex = 7;
            this.toolTip.SetToolTip(this.butDeleteNode, "Delete selected nodes");
            this.butDeleteNode.Click += new System.EventHandler(this.butDeleteNode_Click);
            // 
            // nudCallCount2
            // 
            this.nudCallCount2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.nudCallCount2.IncrementAlternate = new decimal(new int[] {
            10,
            0,
            0,
            65536});
            this.nudCallCount2.Location = new System.Drawing.Point(681, 2);
            this.nudCallCount2.LoopValues = false;
            this.nudCallCount2.Name = "nudCallCount2";
            this.nudCallCount2.Size = new System.Drawing.Size(63, 22);
            this.nudCallCount2.TabIndex = 3;
            this.toolTip.SetToolTip(this.nudCallCount2, "Determines how many times trigger will be called.\r\nIf 0, it means trigger will be" +
        " called infinitely.");
            this.nudCallCount2.ValueChanged += new System.EventHandler(this.nudCallCount_ValueChanged);
            // 
            // butClearNodes
            // 
            this.butClearNodes.Checked = false;
            this.butClearNodes.Image = global::TombEditor.Properties.Resources.actions_delete_16;
            this.butClearNodes.Location = new System.Drawing.Point(302, 1);
            this.butClearNodes.Name = "butClearNodes";
            this.butClearNodes.Size = new System.Drawing.Size(23, 23);
            this.butClearNodes.TabIndex = 8;
            this.toolTip.SetToolTip(this.butClearNodes, "Clear all nodes");
            this.butClearNodes.Click += new System.EventHandler(this.butClearNodes_Click);
            // 
            // butRenameNode
            // 
            this.butRenameNode.Checked = false;
            this.butRenameNode.Image = global::TombEditor.Properties.Resources.general_edit_16;
            this.butRenameNode.Location = new System.Drawing.Point(215, 1);
            this.butRenameNode.Name = "butRenameNode";
            this.butRenameNode.Size = new System.Drawing.Size(23, 23);
            this.butRenameNode.TabIndex = 9;
            this.toolTip.SetToolTip(this.butRenameNode, "Rename last selected node");
            this.butRenameNode.Click += new System.EventHandler(this.butRenameNode_Click);
            // 
            // butChangeNodeColor
            // 
            this.butChangeNodeColor.Checked = false;
            this.butChangeNodeColor.Image = global::TombEditor.Properties.Resources.actions_TextureMode_16;
            this.butChangeNodeColor.Location = new System.Drawing.Point(244, 1);
            this.butChangeNodeColor.Name = "butChangeNodeColor";
            this.butChangeNodeColor.Size = new System.Drawing.Size(23, 23);
            this.butChangeNodeColor.TabIndex = 10;
            this.toolTip.SetToolTip(this.butChangeNodeColor, "Change color for last selected node");
            this.butChangeNodeColor.Click += new System.EventHandler(this.butChangeNodeColor_Click);
            // 
            // butLinkSelectedNodes
            // 
            this.butLinkSelectedNodes.Checked = false;
            this.butLinkSelectedNodes.Image = global::TombEditor.Properties.Resources.actions_Merge_16;
            this.butLinkSelectedNodes.Location = new System.Drawing.Point(186, 1);
            this.butLinkSelectedNodes.Name = "butLinkSelectedNodes";
            this.butLinkSelectedNodes.Size = new System.Drawing.Size(23, 23);
            this.butLinkSelectedNodes.TabIndex = 11;
            this.toolTip.SetToolTip(this.butLinkSelectedNodes, "Link selected nodes, if possible");
            this.butLinkSelectedNodes.Click += new System.EventHandler(this.butLinkSelectedNodes_Click);
            // 
            // tabbedContainer
            // 
            this.tabbedContainer.Controls.Add(this.tabConstructor);
            this.tabbedContainer.Controls.Add(this.tabLevelScript);
            this.tabbedContainer.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabbedContainer.Location = new System.Drawing.Point(0, 25);
            this.tabbedContainer.Name = "tabbedContainer";
            this.tabbedContainer.SelectedIndex = 0;
            this.tabbedContainer.Size = new System.Drawing.Size(758, 375);
            this.tabbedContainer.TabIndex = 1;
            // 
            // tabConstructor
            // 
            this.tabConstructor.Controls.Add(this.panelNodeControls);
            this.tabConstructor.Controls.Add(this.nodeEditor);
            this.tabConstructor.Controls.Add(this.lblWait);
            this.tabConstructor.Location = new System.Drawing.Point(4, 22);
            this.tabConstructor.Name = "tabConstructor";
            this.tabConstructor.Padding = new System.Windows.Forms.Padding(3);
            this.tabConstructor.Size = new System.Drawing.Size(750, 349);
            this.tabConstructor.TabIndex = 13;
            this.tabConstructor.Text = "Node editor";
            this.tabConstructor.UseVisualStyleBackColor = true;
            // 
            // panelNodeControls
            // 
            this.panelNodeControls.Controls.Add(this.butExport);
            this.panelNodeControls.Controls.Add(this.butLinkSelectedNodes);
            this.panelNodeControls.Controls.Add(this.butChangeNodeColor);
            this.panelNodeControls.Controls.Add(this.butRenameNode);
            this.panelNodeControls.Controls.Add(this.butClearNodes);
            this.panelNodeControls.Controls.Add(this.nudCallCount2);
            this.panelNodeControls.Controls.Add(this.butDeleteNode);
            this.panelNodeControls.Controls.Add(this.darkLabel1);
            this.panelNodeControls.Controls.Add(this.butAddActionNode);
            this.panelNodeControls.Controls.Add(this.butAddConditionNode);
            this.panelNodeControls.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panelNodeControls.Location = new System.Drawing.Point(3, 321);
            this.panelNodeControls.Name = "panelNodeControls";
            this.panelNodeControls.Size = new System.Drawing.Size(744, 25);
            this.panelNodeControls.TabIndex = 8;
            // 
            // darkLabel1
            // 
            this.darkLabel1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.darkLabel1.AutoSize = true;
            this.darkLabel1.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.darkLabel1.Location = new System.Drawing.Point(613, 4);
            this.darkLabel1.Name = "darkLabel1";
            this.darkLabel1.Size = new System.Drawing.Size(60, 13);
            this.darkLabel1.TabIndex = 2;
            this.darkLabel1.Text = "Call count:";
            // 
            // nodeEditor
            // 
            this.nodeEditor.AllowDrop = true;
            this.nodeEditor.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.nodeEditor.DefaultNodeWidth = 400;
            this.nodeEditor.GridSize = 256;
            this.nodeEditor.GridStep = 8F;
            this.nodeEditor.LinksAsRopes = false;
            this.nodeEditor.Location = new System.Drawing.Point(3, 5);
            this.nodeEditor.Name = "nodeEditor";
            this.nodeEditor.SelectionColor = System.Drawing.Color.FromArgb(((int)(((byte)(75)))), ((int)(((byte)(110)))), ((int)(((byte)(175)))));
            this.nodeEditor.Size = new System.Drawing.Size(744, 310);
            this.nodeEditor.TabIndex = 0;
            // 
            // lblWait
            // 
            this.lblWait.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lblWait.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.lblWait.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.lblWait.Location = new System.Drawing.Point(3, 5);
            this.lblWait.Name = "lblWait";
            this.lblWait.Size = new System.Drawing.Size(744, 310);
            this.lblWait.TabIndex = 9;
            this.lblWait.Text = "Please wait...";
            this.lblWait.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // tabLevelScript
            // 
            this.tabLevelScript.Controls.Add(this.lstFunctions);
            this.tabLevelScript.Controls.Add(this.panelFunctionControls);
            this.tabLevelScript.Controls.Add(this.lblListNotify);
            this.tabLevelScript.Location = new System.Drawing.Point(4, 22);
            this.tabLevelScript.Name = "tabLevelScript";
            this.tabLevelScript.Padding = new System.Windows.Forms.Padding(3);
            this.tabLevelScript.Size = new System.Drawing.Size(750, 349);
            this.tabLevelScript.TabIndex = 12;
            this.tabLevelScript.Text = "Level script functions";
            this.tabLevelScript.UseVisualStyleBackColor = true;
            // 
            // lstFunctions
            // 
            this.lstFunctions.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lstFunctions.Location = new System.Drawing.Point(3, 5);
            this.lstFunctions.Name = "lstFunctions";
            this.lstFunctions.Size = new System.Drawing.Size(744, 310);
            this.lstFunctions.TabIndex = 0;
            this.lstFunctions.Text = "darkListView1";
            this.lstFunctions.SelectedIndicesChanged += new System.EventHandler(this.lstFunctions_SelectedIndicesChanged);
            // 
            // panelFunctionControls
            // 
            this.panelFunctionControls.Controls.Add(this.nudCallCount);
            this.panelFunctionControls.Controls.Add(this.darkLabel3);
            this.panelFunctionControls.Controls.Add(this.darkLabel2);
            this.panelFunctionControls.Controls.Add(this.tbArgument);
            this.panelFunctionControls.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panelFunctionControls.Location = new System.Drawing.Point(3, 321);
            this.panelFunctionControls.Name = "panelFunctionControls";
            this.panelFunctionControls.Size = new System.Drawing.Size(744, 25);
            this.panelFunctionControls.TabIndex = 1;
            // 
            // darkLabel3
            // 
            this.darkLabel3.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.darkLabel3.AutoSize = true;
            this.darkLabel3.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.darkLabel3.Location = new System.Drawing.Point(613, 4);
            this.darkLabel3.Name = "darkLabel3";
            this.darkLabel3.Size = new System.Drawing.Size(60, 13);
            this.darkLabel3.TabIndex = 2;
            this.darkLabel3.Text = "Call count:";
            // 
            // darkLabel2
            // 
            this.darkLabel2.AutoSize = true;
            this.darkLabel2.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.darkLabel2.Location = new System.Drawing.Point(0, 4);
            this.darkLabel2.Name = "darkLabel2";
            this.darkLabel2.Size = new System.Drawing.Size(60, 13);
            this.darkLabel2.TabIndex = 1;
            this.darkLabel2.Text = "Argument:";
            // 
            // lblListNotify
            // 
            this.lblListNotify.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblListNotify.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.lblListNotify.Location = new System.Drawing.Point(3, 3);
            this.lblListNotify.Name = "lblListNotify";
            this.lblListNotify.Size = new System.Drawing.Size(744, 343);
            this.lblListNotify.TabIndex = 2;
            this.lblListNotify.Text = "Function list notify";
            this.lblListNotify.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.lblListNotify.EnabledChanged += new System.EventHandler(this.lblListNotify_EnabledChanged);
            this.lblListNotify.Click += new System.EventHandler(this.lblListNotify_Click);
            // 
            // butExport
            // 
            this.butExport.Checked = false;
            this.butExport.Image = global::TombEditor.Properties.Resources.general_copy_16;
            this.butExport.Location = new System.Drawing.Point(331, 1);
            this.butExport.Name = "butExport";
            this.butExport.Size = new System.Drawing.Size(23, 23);
            this.butExport.TabIndex = 12;
            this.toolTip.SetToolTip(this.butExport, "Export lua script to clipboard");
            this.butExport.Click += new System.EventHandler(this.butExport_Click);
            // 
            // TriggerManager
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.tabbedContainer);
            this.Controls.Add(this.darkPanel3);
            this.Font = new System.Drawing.Font("Segoe UI", 8.25F);
            this.Name = "TriggerManager";
            this.Size = new System.Drawing.Size(758, 400);
            this.darkPanel3.ResumeLayout(false);
            this.darkPanel3.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudCallCount)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudCallCount2)).EndInit();
            this.tabbedContainer.ResumeLayout(false);
            this.tabConstructor.ResumeLayout(false);
            this.panelNodeControls.ResumeLayout(false);
            this.panelNodeControls.PerformLayout();
            this.tabLevelScript.ResumeLayout(false);
            this.panelFunctionControls.ResumeLayout(false);
            this.panelFunctionControls.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion
        private TombLib.Controls.DarkTabbedContainer tabbedContainer;
        private DarkUI.Controls.DarkPanel darkPanel3;
        private DarkUI.Controls.DarkRadioButton rbNodeEditor;
        private DarkUI.Controls.DarkRadioButton rbLevelScript;
        private DarkUI.Controls.DarkButton butSearch;
        private DarkUI.Controls.DarkButton butUnassign;
        private System.Windows.Forms.ToolTip toolTip;
        private DarkUI.Controls.DarkLabel lblNotify;
        private System.Windows.Forms.TabPage tabLevelScript;
        private DarkUI.Controls.DarkListView lstFunctions;
        private DarkUI.Controls.DarkPanel panelFunctionControls;
        private DarkUI.Controls.DarkNumericUpDown nudCallCount;
        private DarkUI.Controls.DarkLabel darkLabel3;
        private DarkUI.Controls.DarkLabel darkLabel2;
        private DarkUI.Controls.DarkTextBox tbArgument;
        private System.Windows.Forms.TabPage tabConstructor;
        private DarkUI.Controls.DarkLabel lblListNotify;
        private TombLib.Controls.VisualScripting.NodeEditor nodeEditor;
        private DarkUI.Controls.DarkButton butDeleteNode;
        private DarkUI.Controls.DarkButton butAddConditionNode;
        private DarkUI.Controls.DarkButton butAddActionNode;
        private DarkUI.Controls.DarkPanel panelNodeControls;
        private DarkUI.Controls.DarkButton butClearNodes;
        private DarkUI.Controls.DarkNumericUpDown nudCallCount2;
        private DarkUI.Controls.DarkLabel darkLabel1;
        private DarkUI.Controls.DarkButton butRenameNode;
        private DarkUI.Controls.DarkButton butChangeNodeColor;
        private DarkUI.Controls.DarkButton butLinkSelectedNodes;
        private DarkUI.Controls.DarkLabel lblWait;
        private DarkUI.Controls.DarkButton butExport;
    }
}
