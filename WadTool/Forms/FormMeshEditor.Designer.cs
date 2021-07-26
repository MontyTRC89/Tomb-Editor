namespace WadTool
{
    partial class FormMeshEditor
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
            this.lstMeshes = new DarkUI.Controls.DarkTreeView();
            this.panelMesh = new WadTool.Controls.PanelRenderingMesh();
            this.btCancel = new DarkUI.Controls.DarkButton();
            this.btOk = new DarkUI.Controls.DarkButton();
            this.panelEditing = new DarkUI.Controls.DarkSectionPanel();
            this.cbWireframe = new DarkUI.Controls.DarkCheckBox();
            this.cbVertexNumbers = new DarkUI.Controls.DarkCheckBox();
            this.cbEditingMode = new DarkUI.Controls.DarkComboBox();
            this.darkLabel2 = new DarkUI.Controls.DarkLabel();
            this.tabsModes = new TombLib.Controls.DarkTabbedContainer();
            this.tabVertexRemap = new System.Windows.Forms.TabPage();
            this.nudVertexNum = new DarkUI.Controls.DarkNumericUpDown();
            this.butFindVertex = new DarkUI.Controls.DarkButton();
            this.darkLabel1 = new DarkUI.Controls.DarkLabel();
            this.butRemapVertex = new DarkUI.Controls.DarkButton();
            this.tabShininess = new System.Windows.Forms.TabPage();
            this.nudShineStrength = new DarkUI.Controls.DarkNumericUpDown();
            this.butApplyShinyToAllFaces = new DarkUI.Controls.DarkButton();
            this.darkLabel3 = new DarkUI.Controls.DarkLabel();
            this.darkSectionPanel2 = new DarkUI.Controls.DarkSectionPanel();
            this.panelMain = new DarkUI.Controls.DarkPanel();
            this.panelEditingTools = new DarkUI.Controls.DarkPanel();
            this.panelTree = new DarkUI.Controls.DarkPanel();
            this.panelEditing.SuspendLayout();
            this.tabsModes.SuspendLayout();
            this.tabVertexRemap.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudVertexNum)).BeginInit();
            this.tabShininess.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudShineStrength)).BeginInit();
            this.darkSectionPanel2.SuspendLayout();
            this.panelMain.SuspendLayout();
            this.panelEditingTools.SuspendLayout();
            this.panelTree.SuspendLayout();
            this.SuspendLayout();
            // 
            // lstMeshes
            // 
            this.lstMeshes.Cursor = System.Windows.Forms.Cursors.Arrow;
            this.lstMeshes.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lstMeshes.EvenNodeColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.lstMeshes.ExpandOnDoubleClick = false;
            this.lstMeshes.FocusedNodeColor = System.Drawing.Color.FromArgb(((int)(((byte)(75)))), ((int)(((byte)(110)))), ((int)(((byte)(175)))));
            this.lstMeshes.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.lstMeshes.Location = new System.Drawing.Point(1, 25);
            this.lstMeshes.MaxDragChange = 20;
            this.lstMeshes.Name = "lstMeshes";
            this.lstMeshes.NonFocusedNodeColor = System.Drawing.Color.FromArgb(((int)(((byte)(92)))), ((int)(((byte)(92)))), ((int)(((byte)(92)))));
            this.lstMeshes.OddNodeColor = System.Drawing.Color.FromArgb(((int)(((byte)(57)))), ((int)(((byte)(60)))), ((int)(((byte)(62)))));
            this.lstMeshes.Size = new System.Drawing.Size(312, 456);
            this.lstMeshes.TabIndex = 1;
            this.lstMeshes.Text = "darkTreeView1";
            this.lstMeshes.Click += new System.EventHandler(this.lstMeshes_Click);
            this.lstMeshes.KeyDown += new System.Windows.Forms.KeyEventHandler(this.lstMeshes_KeyDown);
            // 
            // panelMesh
            // 
            this.panelMesh.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelMesh.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.panelMesh.Location = new System.Drawing.Point(319, 0);
            this.panelMesh.Name = "panelMesh";
            this.panelMesh.Size = new System.Drawing.Size(415, 385);
            this.panelMesh.TabIndex = 0;
            // 
            // btCancel
            // 
            this.btCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btCancel.Checked = false;
            this.btCancel.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.btCancel.Location = new System.Drawing.Point(658, 493);
            this.btCancel.Name = "btCancel";
            this.btCancel.Size = new System.Drawing.Size(81, 23);
            this.btCancel.TabIndex = 52;
            this.btCancel.Text = "Cancel";
            this.btCancel.Click += new System.EventHandler(this.btCancel_Click);
            // 
            // btOk
            // 
            this.btOk.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btOk.Checked = false;
            this.btOk.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.btOk.Location = new System.Drawing.Point(571, 493);
            this.btOk.Name = "btOk";
            this.btOk.Size = new System.Drawing.Size(81, 23);
            this.btOk.TabIndex = 53;
            this.btOk.Text = "OK";
            this.btOk.Click += new System.EventHandler(this.btOk_Click);
            // 
            // panelEditing
            // 
            this.panelEditing.Controls.Add(this.cbWireframe);
            this.panelEditing.Controls.Add(this.cbVertexNumbers);
            this.panelEditing.Controls.Add(this.cbEditingMode);
            this.panelEditing.Controls.Add(this.darkLabel2);
            this.panelEditing.Controls.Add(this.tabsModes);
            this.panelEditing.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panelEditing.Location = new System.Drawing.Point(0, 5);
            this.panelEditing.Name = "panelEditing";
            this.panelEditing.SectionHeader = "Editing tools";
            this.panelEditing.Size = new System.Drawing.Size(415, 92);
            this.panelEditing.TabIndex = 54;
            // 
            // cbWireframe
            // 
            this.cbWireframe.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.cbWireframe.AutoSize = true;
            this.cbWireframe.Location = new System.Drawing.Point(191, 35);
            this.cbWireframe.Name = "cbWireframe";
            this.cbWireframe.Size = new System.Drawing.Size(79, 17);
            this.cbWireframe.TabIndex = 6;
            this.cbWireframe.Text = "Wireframe";
            this.cbWireframe.CheckedChanged += new System.EventHandler(this.cbWireframe_CheckedChanged);
            // 
            // cbVertexNumbers
            // 
            this.cbVertexNumbers.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.cbVertexNumbers.AutoSize = true;
            this.cbVertexNumbers.Location = new System.Drawing.Point(272, 35);
            this.cbVertexNumbers.Name = "cbVertexNumbers";
            this.cbVertexNumbers.Size = new System.Drawing.Size(136, 17);
            this.cbVertexNumbers.TabIndex = 4;
            this.cbVertexNumbers.Text = "Show vertex numbers";
            this.cbVertexNumbers.CheckedChanged += new System.EventHandler(this.cbVertexNumbers_CheckedChanged);
            // 
            // cbEditingMode
            // 
            this.cbEditingMode.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.cbEditingMode.FormattingEnabled = true;
            this.cbEditingMode.Location = new System.Drawing.Point(47, 33);
            this.cbEditingMode.Name = "cbEditingMode";
            this.cbEditingMode.Size = new System.Drawing.Size(133, 23);
            this.cbEditingMode.TabIndex = 9;
            this.cbEditingMode.SelectedIndexChanged += new System.EventHandler(this.cbEditingMode_SelectedIndexChanged);
            // 
            // darkLabel2
            // 
            this.darkLabel2.AutoSize = true;
            this.darkLabel2.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.darkLabel2.Location = new System.Drawing.Point(6, 36);
            this.darkLabel2.Name = "darkLabel2";
            this.darkLabel2.Size = new System.Drawing.Size(40, 13);
            this.darkLabel2.TabIndex = 8;
            this.darkLabel2.Text = "Mode:";
            // 
            // tabsModes
            // 
            this.tabsModes.Alignment = System.Windows.Forms.TabAlignment.Right;
            this.tabsModes.Controls.Add(this.tabVertexRemap);
            this.tabsModes.Controls.Add(this.tabShininess);
            this.tabsModes.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.tabsModes.Location = new System.Drawing.Point(1, 52);
            this.tabsModes.Multiline = true;
            this.tabsModes.Name = "tabsModes";
            this.tabsModes.SelectedIndex = 0;
            this.tabsModes.Size = new System.Drawing.Size(413, 39);
            this.tabsModes.TabIndex = 7;
            // 
            // tabVertexRemap
            // 
            this.tabVertexRemap.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.tabVertexRemap.Controls.Add(this.nudVertexNum);
            this.tabVertexRemap.Controls.Add(this.butFindVertex);
            this.tabVertexRemap.Controls.Add(this.darkLabel1);
            this.tabVertexRemap.Controls.Add(this.butRemapVertex);
            this.tabVertexRemap.Location = new System.Drawing.Point(4, 4);
            this.tabVertexRemap.Name = "tabVertexRemap";
            this.tabVertexRemap.Size = new System.Drawing.Size(363, 31);
            this.tabVertexRemap.TabIndex = 0;
            this.tabVertexRemap.Text = "Vertex remap";
            // 
            // nudVertexNum
            // 
            this.nudVertexNum.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.nudVertexNum.IncrementAlternate = new decimal(new int[] {
            10,
            0,
            0,
            65536});
            this.nudVertexNum.Location = new System.Drawing.Point(88, 6);
            this.nudVertexNum.LoopValues = false;
            this.nudVertexNum.Maximum = new decimal(new int[] {
            65535,
            0,
            0,
            0});
            this.nudVertexNum.Name = "nudVertexNum";
            this.nudVertexNum.Size = new System.Drawing.Size(112, 22);
            this.nudVertexNum.TabIndex = 2;
            this.nudVertexNum.KeyDown += new System.Windows.Forms.KeyEventHandler(this.nudVertexNum_KeyDown);
            // 
            // butFindVertex
            // 
            this.butFindVertex.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.butFindVertex.Checked = false;
            this.butFindVertex.Image = global::WadTool.Properties.Resources.general_search_16;
            this.butFindVertex.Location = new System.Drawing.Point(206, 6);
            this.butFindVertex.Name = "butFindVertex";
            this.butFindVertex.Size = new System.Drawing.Size(74, 22);
            this.butFindVertex.TabIndex = 5;
            this.butFindVertex.Text = "Jump to";
            this.butFindVertex.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.butFindVertex.Click += new System.EventHandler(this.butFindVertex_Click);
            // 
            // darkLabel1
            // 
            this.darkLabel1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.darkLabel1.AutoSize = true;
            this.darkLabel1.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.darkLabel1.Location = new System.Drawing.Point(3, 8);
            this.darkLabel1.Name = "darkLabel1";
            this.darkLabel1.Size = new System.Drawing.Size(84, 13);
            this.darkLabel1.TabIndex = 1;
            this.darkLabel1.Text = "Vertex number:";
            // 
            // butRemapVertex
            // 
            this.butRemapVertex.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.butRemapVertex.Checked = false;
            this.butRemapVertex.Image = global::WadTool.Properties.Resources.replace_16;
            this.butRemapVertex.Location = new System.Drawing.Point(286, 6);
            this.butRemapVertex.Name = "butRemapVertex";
            this.butRemapVertex.Size = new System.Drawing.Size(74, 22);
            this.butRemapVertex.TabIndex = 3;
            this.butRemapVertex.Text = "Remap";
            this.butRemapVertex.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.butRemapVertex.Click += new System.EventHandler(this.butRemapVertex_Click);
            // 
            // tabShininess
            // 
            this.tabShininess.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.tabShininess.Controls.Add(this.nudShineStrength);
            this.tabShininess.Controls.Add(this.butApplyShinyToAllFaces);
            this.tabShininess.Controls.Add(this.darkLabel3);
            this.tabShininess.Location = new System.Drawing.Point(4, 4);
            this.tabShininess.Name = "tabShininess";
            this.tabShininess.Padding = new System.Windows.Forms.Padding(3);
            this.tabShininess.Size = new System.Drawing.Size(363, 31);
            this.tabShininess.TabIndex = 1;
            this.tabShininess.Text = "Shininess";
            // 
            // nudShineStrength
            // 
            this.nudShineStrength.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.nudShineStrength.IncrementAlternate = new decimal(new int[] {
            10,
            0,
            0,
            65536});
            this.nudShineStrength.Location = new System.Drawing.Point(60, 6);
            this.nudShineStrength.LoopValues = false;
            this.nudShineStrength.Maximum = new decimal(new int[] {
            63,
            0,
            0,
            0});
            this.nudShineStrength.Name = "nudShineStrength";
            this.nudShineStrength.Size = new System.Drawing.Size(171, 22);
            this.nudShineStrength.TabIndex = 7;
            this.nudShineStrength.ValueChanged += new System.EventHandler(this.nudShineStrength_ValueChanged);
            // 
            // butApplyShinyToAllFaces
            // 
            this.butApplyShinyToAllFaces.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.butApplyShinyToAllFaces.Checked = false;
            this.butApplyShinyToAllFaces.Location = new System.Drawing.Point(237, 6);
            this.butApplyShinyToAllFaces.Name = "butApplyShinyToAllFaces";
            this.butApplyShinyToAllFaces.Size = new System.Drawing.Size(123, 22);
            this.butApplyShinyToAllFaces.TabIndex = 9;
            this.butApplyShinyToAllFaces.Text = "Apply to all faces";
            this.butApplyShinyToAllFaces.Click += new System.EventHandler(this.butApplyShinyToAllFaces_Click);
            // 
            // darkLabel3
            // 
            this.darkLabel3.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.darkLabel3.AutoSize = true;
            this.darkLabel3.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.darkLabel3.Location = new System.Drawing.Point(3, 8);
            this.darkLabel3.Name = "darkLabel3";
            this.darkLabel3.Size = new System.Drawing.Size(55, 13);
            this.darkLabel3.TabIndex = 6;
            this.darkLabel3.Text = "Strength:";
            // 
            // darkSectionPanel2
            // 
            this.darkSectionPanel2.Controls.Add(this.lstMeshes);
            this.darkSectionPanel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.darkSectionPanel2.Location = new System.Drawing.Point(0, 0);
            this.darkSectionPanel2.Name = "darkSectionPanel2";
            this.darkSectionPanel2.SectionHeader = "Mesh list";
            this.darkSectionPanel2.Size = new System.Drawing.Size(314, 482);
            this.darkSectionPanel2.TabIndex = 55;
            // 
            // panelMain
            // 
            this.panelMain.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panelMain.Controls.Add(this.panelMesh);
            this.panelMain.Controls.Add(this.panelEditingTools);
            this.panelMain.Controls.Add(this.panelTree);
            this.panelMain.Location = new System.Drawing.Point(5, 5);
            this.panelMain.Name = "panelMain";
            this.panelMain.Size = new System.Drawing.Size(734, 482);
            this.panelMain.TabIndex = 56;
            // 
            // panelEditingTools
            // 
            this.panelEditingTools.Controls.Add(this.panelEditing);
            this.panelEditingTools.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panelEditingTools.Location = new System.Drawing.Point(319, 385);
            this.panelEditingTools.Name = "panelEditingTools";
            this.panelEditingTools.Size = new System.Drawing.Size(415, 97);
            this.panelEditingTools.TabIndex = 55;
            // 
            // panelTree
            // 
            this.panelTree.Controls.Add(this.darkSectionPanel2);
            this.panelTree.Dock = System.Windows.Forms.DockStyle.Left;
            this.panelTree.Location = new System.Drawing.Point(0, 0);
            this.panelTree.Name = "panelTree";
            this.panelTree.Padding = new System.Windows.Forms.Padding(0, 0, 5, 0);
            this.panelTree.Size = new System.Drawing.Size(319, 482);
            this.panelTree.TabIndex = 0;
            // 
            // FormMeshEditor
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(744, 522);
            this.Controls.Add(this.panelMain);
            this.Controls.Add(this.btCancel);
            this.Controls.Add(this.btOk);
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(760, 560);
            this.Name = "FormMeshEditor";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Mesh editor";
            this.panelEditing.ResumeLayout(false);
            this.panelEditing.PerformLayout();
            this.tabsModes.ResumeLayout(false);
            this.tabVertexRemap.ResumeLayout(false);
            this.tabVertexRemap.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudVertexNum)).EndInit();
            this.tabShininess.ResumeLayout(false);
            this.tabShininess.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudShineStrength)).EndInit();
            this.darkSectionPanel2.ResumeLayout(false);
            this.panelMain.ResumeLayout(false);
            this.panelEditingTools.ResumeLayout(false);
            this.panelTree.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private Controls.PanelRenderingMesh panelMesh;
        private DarkUI.Controls.DarkTreeView lstMeshes;
        private DarkUI.Controls.DarkButton btCancel;
        private DarkUI.Controls.DarkButton btOk;
        private DarkUI.Controls.DarkSectionPanel panelEditing;
        private DarkUI.Controls.DarkButton butRemapVertex;
        private DarkUI.Controls.DarkNumericUpDown nudVertexNum;
        private DarkUI.Controls.DarkLabel darkLabel1;
        private DarkUI.Controls.DarkSectionPanel darkSectionPanel2;
        private DarkUI.Controls.DarkCheckBox cbVertexNumbers;
        private DarkUI.Controls.DarkButton butFindVertex;
        private DarkUI.Controls.DarkCheckBox cbWireframe;
        private DarkUI.Controls.DarkPanel panelMain;
        private DarkUI.Controls.DarkPanel panelTree;
        private DarkUI.Controls.DarkPanel panelEditingTools;
        private TombLib.Controls.DarkTabbedContainer tabsModes;
        private System.Windows.Forms.TabPage tabVertexRemap;
        private System.Windows.Forms.TabPage tabShininess;
        private DarkUI.Controls.DarkComboBox cbEditingMode;
        private DarkUI.Controls.DarkLabel darkLabel2;
        private DarkUI.Controls.DarkNumericUpDown nudShineStrength;
        private DarkUI.Controls.DarkButton butApplyShinyToAllFaces;
        private DarkUI.Controls.DarkLabel darkLabel3;
    }
}