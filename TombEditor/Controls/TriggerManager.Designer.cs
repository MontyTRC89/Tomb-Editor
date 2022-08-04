
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
            this.rbConstructor = new DarkUI.Controls.DarkRadioButton();
            this.rbLevelScript = new DarkUI.Controls.DarkRadioButton();
            this.toolTip = new System.Windows.Forms.ToolTip(this.components);
            this.nudCallCount = new DarkUI.Controls.DarkNumericUpDown();
            this.tbArgument = new DarkUI.Controls.DarkTextBox();
            this.tabbedContainer = new TombLib.Controls.DarkTabbedContainer();
            this.tabLevelScript = new System.Windows.Forms.TabPage();
            this.lstFunctions = new DarkUI.Controls.DarkListView();
            this.panelFunctionControls = new DarkUI.Controls.DarkPanel();
            this.darkLabel3 = new DarkUI.Controls.DarkLabel();
            this.darkLabel2 = new DarkUI.Controls.DarkLabel();
            this.lblListNotify = new DarkUI.Controls.DarkLabel();
            this.tabConstructor = new System.Windows.Forms.TabPage();
            this.darkPanel2 = new DarkUI.Controls.DarkPanel();
            this.darkLabel1 = new DarkUI.Controls.DarkLabel();
            this.darkPanel3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudCallCount)).BeginInit();
            this.tabbedContainer.SuspendLayout();
            this.tabLevelScript.SuspendLayout();
            this.panelFunctionControls.SuspendLayout();
            this.tabConstructor.SuspendLayout();
            this.darkPanel2.SuspendLayout();
            this.SuspendLayout();
            // 
            // darkPanel3
            // 
            this.darkPanel3.Controls.Add(this.lblNotify);
            this.darkPanel3.Controls.Add(this.butUnassign);
            this.darkPanel3.Controls.Add(this.butSearch);
            this.darkPanel3.Controls.Add(this.rbConstructor);
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
            this.toolTip.SetToolTip(this.butSearch, "Search for function");
            this.butSearch.Click += new System.EventHandler(this.butSearch_Click);
            // 
            // rbConstructor
            // 
            this.rbConstructor.AutoSize = true;
            this.rbConstructor.Enabled = false;
            this.rbConstructor.Location = new System.Drawing.Point(144, 5);
            this.rbConstructor.Name = "rbConstructor";
            this.rbConstructor.Size = new System.Drawing.Size(86, 17);
            this.rbConstructor.TabIndex = 1;
            this.rbConstructor.Text = "Constructor";
            this.toolTip.SetToolTip(this.rbConstructor, "Visually construct a trigger");
            this.rbConstructor.CheckedChanged += new System.EventHandler(this.rbConstructor_CheckedChanged);
            // 
            // rbLevelScript
            // 
            this.rbLevelScript.AutoSize = true;
            this.rbLevelScript.Checked = true;
            this.rbLevelScript.Location = new System.Drawing.Point(5, 5);
            this.rbLevelScript.Name = "rbLevelScript";
            this.rbLevelScript.Size = new System.Drawing.Size(133, 17);
            this.rbLevelScript.TabIndex = 0;
            this.rbLevelScript.TabStop = true;
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
            // tabbedContainer
            // 
            this.tabbedContainer.Controls.Add(this.tabLevelScript);
            this.tabbedContainer.Controls.Add(this.tabConstructor);
            this.tabbedContainer.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabbedContainer.Location = new System.Drawing.Point(0, 25);
            this.tabbedContainer.Name = "tabbedContainer";
            this.tabbedContainer.SelectedIndex = 0;
            this.tabbedContainer.Size = new System.Drawing.Size(758, 375);
            this.tabbedContainer.TabIndex = 1;
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
            this.darkLabel3.Size = new System.Drawing.Size(62, 13);
            this.darkLabel3.TabIndex = 2;
            this.darkLabel3.Text = "Call count:";
            // 
            // darkLabel2
            // 
            this.darkLabel2.AutoSize = true;
            this.darkLabel2.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.darkLabel2.Location = new System.Drawing.Point(0, 4);
            this.darkLabel2.Name = "darkLabel2";
            this.darkLabel2.Size = new System.Drawing.Size(61, 13);
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
            // tabConstructor
            // 
            this.tabConstructor.Controls.Add(this.darkPanel2);
            this.tabConstructor.Location = new System.Drawing.Point(4, 22);
            this.tabConstructor.Name = "tabConstructor";
            this.tabConstructor.Padding = new System.Windows.Forms.Padding(3);
            this.tabConstructor.Size = new System.Drawing.Size(750, 349);
            this.tabConstructor.TabIndex = 13;
            this.tabConstructor.Text = "Constructor";
            this.tabConstructor.UseVisualStyleBackColor = true;
            // 
            // darkPanel2
            // 
            this.darkPanel2.Controls.Add(this.darkLabel1);
            this.darkPanel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.darkPanel2.Location = new System.Drawing.Point(3, 3);
            this.darkPanel2.Name = "darkPanel2";
            this.darkPanel2.Size = new System.Drawing.Size(744, 343);
            this.darkPanel2.TabIndex = 0;
            // 
            // darkLabel1
            // 
            this.darkLabel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.darkLabel1.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.darkLabel1.Location = new System.Drawing.Point(0, 0);
            this.darkLabel1.Name = "darkLabel1";
            this.darkLabel1.Size = new System.Drawing.Size(744, 343);
            this.darkLabel1.TabIndex = 0;
            this.darkLabel1.Text = "SORY!\r\nNOT IMPLEMATEL!";
            this.darkLabel1.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // TriggerManager
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.tabbedContainer);
            this.Controls.Add(this.darkPanel3);
            this.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.Name = "TriggerManager";
            this.Size = new System.Drawing.Size(758, 400);
            this.darkPanel3.ResumeLayout(false);
            this.darkPanel3.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudCallCount)).EndInit();
            this.tabbedContainer.ResumeLayout(false);
            this.tabLevelScript.ResumeLayout(false);
            this.panelFunctionControls.ResumeLayout(false);
            this.panelFunctionControls.PerformLayout();
            this.tabConstructor.ResumeLayout(false);
            this.darkPanel2.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion
        private TombLib.Controls.DarkTabbedContainer tabbedContainer;
        private DarkUI.Controls.DarkPanel darkPanel3;
        private DarkUI.Controls.DarkRadioButton rbConstructor;
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
        private DarkUI.Controls.DarkPanel darkPanel2;
        private DarkUI.Controls.DarkLabel darkLabel1;
        private DarkUI.Controls.DarkLabel lblListNotify;
    }
}
