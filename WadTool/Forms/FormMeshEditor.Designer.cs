namespace WadTool
{
    partial class FormMesh
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
            this.darkSectionPanel1 = new DarkUI.Controls.DarkSectionPanel();
            this.cbWireframe = new DarkUI.Controls.DarkCheckBox();
            this.butFindVertex = new DarkUI.Controls.DarkButton();
            this.cbVertexNumbers = new DarkUI.Controls.DarkCheckBox();
            this.butRemapVertex = new DarkUI.Controls.DarkButton();
            this.nudVertexNum = new DarkUI.Controls.DarkNumericUpDown();
            this.darkLabel1 = new DarkUI.Controls.DarkLabel();
            this.cbShowVertices = new DarkUI.Controls.DarkCheckBox();
            this.darkSectionPanel2 = new DarkUI.Controls.DarkSectionPanel();
            this.darkPanel1 = new DarkUI.Controls.DarkPanel();
            this.panelTree = new DarkUI.Controls.DarkPanel();
            this.darkSectionPanel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudVertexNum)).BeginInit();
            this.darkSectionPanel2.SuspendLayout();
            this.darkPanel1.SuspendLayout();
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
            this.panelMesh.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelMesh.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.panelMesh.Location = new System.Drawing.Point(319, 0);
            this.panelMesh.Name = "panelMesh";
            this.panelMesh.Size = new System.Drawing.Size(415, 393);
            this.panelMesh.TabIndex = 0;
            // 
            // btCancel
            // 
            this.btCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btCancel.Checked = false;
            this.btCancel.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.btCancel.Location = new System.Drawing.Point(571, 493);
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
            this.btOk.Location = new System.Drawing.Point(658, 493);
            this.btOk.Name = "btOk";
            this.btOk.Size = new System.Drawing.Size(81, 23);
            this.btOk.TabIndex = 53;
            this.btOk.Text = "OK";
            this.btOk.Click += new System.EventHandler(this.btOk_Click);
            // 
            // darkSectionPanel1
            // 
            this.darkSectionPanel1.Controls.Add(this.cbWireframe);
            this.darkSectionPanel1.Controls.Add(this.butFindVertex);
            this.darkSectionPanel1.Controls.Add(this.cbVertexNumbers);
            this.darkSectionPanel1.Controls.Add(this.butRemapVertex);
            this.darkSectionPanel1.Controls.Add(this.nudVertexNum);
            this.darkSectionPanel1.Controls.Add(this.darkLabel1);
            this.darkSectionPanel1.Controls.Add(this.cbShowVertices);
            this.darkSectionPanel1.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.darkSectionPanel1.Location = new System.Drawing.Point(319, 399);
            this.darkSectionPanel1.Name = "darkSectionPanel1";
            this.darkSectionPanel1.SectionHeader = "Vertex remap tools";
            this.darkSectionPanel1.Size = new System.Drawing.Size(415, 83);
            this.darkSectionPanel1.TabIndex = 54;
            // 
            // cbWireframe
            // 
            this.cbWireframe.AutoSize = true;
            this.cbWireframe.Location = new System.Drawing.Point(214, 32);
            this.cbWireframe.Name = "cbWireframe";
            this.cbWireframe.Size = new System.Drawing.Size(79, 17);
            this.cbWireframe.TabIndex = 6;
            this.cbWireframe.Text = "Wireframe";
            this.cbWireframe.CheckedChanged += new System.EventHandler(this.cbWireframe_CheckedChanged);
            // 
            // butFindVertex
            // 
            this.butFindVertex.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.butFindVertex.Checked = false;
            this.butFindVertex.Image = global::WadTool.Properties.Resources.general_search_16;
            this.butFindVertex.Location = new System.Drawing.Point(247, 55);
            this.butFindVertex.Name = "butFindVertex";
            this.butFindVertex.Size = new System.Drawing.Size(82, 22);
            this.butFindVertex.TabIndex = 5;
            this.butFindVertex.Text = "Jump to";
            this.butFindVertex.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.butFindVertex.Click += new System.EventHandler(this.butFindVertex_Click);
            // 
            // cbVertexNumbers
            // 
            this.cbVertexNumbers.AutoSize = true;
            this.cbVertexNumbers.Location = new System.Drawing.Point(101, 32);
            this.cbVertexNumbers.Name = "cbVertexNumbers";
            this.cbVertexNumbers.Size = new System.Drawing.Size(118, 17);
            this.cbVertexNumbers.TabIndex = 4;
            this.cbVertexNumbers.Text = "Show all numbers";
            this.cbVertexNumbers.CheckedChanged += new System.EventHandler(this.cbVertexNumbers_CheckedChanged);
            // 
            // butRemapVertex
            // 
            this.butRemapVertex.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.butRemapVertex.Checked = false;
            this.butRemapVertex.Image = global::WadTool.Properties.Resources.replace_16;
            this.butRemapVertex.Location = new System.Drawing.Point(335, 55);
            this.butRemapVertex.Name = "butRemapVertex";
            this.butRemapVertex.Size = new System.Drawing.Size(74, 22);
            this.butRemapVertex.TabIndex = 3;
            this.butRemapVertex.Text = "Remap";
            this.butRemapVertex.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.butRemapVertex.Click += new System.EventHandler(this.butRemapVertex_Click);
            // 
            // nudVertexNum
            // 
            this.nudVertexNum.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.nudVertexNum.IncrementAlternate = new decimal(new int[] {
            10,
            0,
            0,
            65536});
            this.nudVertexNum.Location = new System.Drawing.Point(96, 55);
            this.nudVertexNum.LoopValues = false;
            this.nudVertexNum.Maximum = new decimal(new int[] {
            65535,
            0,
            0,
            0});
            this.nudVertexNum.Name = "nudVertexNum";
            this.nudVertexNum.Size = new System.Drawing.Size(145, 22);
            this.nudVertexNum.TabIndex = 2;
            this.nudVertexNum.KeyDown += new System.Windows.Forms.KeyEventHandler(this.nudVertexNum_KeyDown);
            // 
            // darkLabel1
            // 
            this.darkLabel1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.darkLabel1.AutoSize = true;
            this.darkLabel1.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.darkLabel1.Location = new System.Drawing.Point(5, 57);
            this.darkLabel1.Name = "darkLabel1";
            this.darkLabel1.Size = new System.Drawing.Size(85, 13);
            this.darkLabel1.TabIndex = 1;
            this.darkLabel1.Text = "Vertex number:";
            // 
            // cbShowVertices
            // 
            this.cbShowVertices.AutoSize = true;
            this.cbShowVertices.Location = new System.Drawing.Point(8, 32);
            this.cbShowVertices.Name = "cbShowVertices";
            this.cbShowVertices.Size = new System.Drawing.Size(96, 17);
            this.cbShowVertices.TabIndex = 0;
            this.cbShowVertices.Text = "Show vertices";
            this.cbShowVertices.CheckedChanged += new System.EventHandler(this.cbShowVertices_CheckedChanged);
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
            // darkPanel1
            // 
            this.darkPanel1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.darkPanel1.Controls.Add(this.panelMesh);
            this.darkPanel1.Controls.Add(this.darkSectionPanel1);
            this.darkPanel1.Controls.Add(this.panelTree);
            this.darkPanel1.Location = new System.Drawing.Point(5, 5);
            this.darkPanel1.Name = "darkPanel1";
            this.darkPanel1.Size = new System.Drawing.Size(734, 482);
            this.darkPanel1.TabIndex = 56;
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
            // FormMesh
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(744, 522);
            this.Controls.Add(this.darkPanel1);
            this.Controls.Add(this.btCancel);
            this.Controls.Add(this.btOk);
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(760, 560);
            this.Name = "FormMesh";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Mesh editor";
            this.darkSectionPanel1.ResumeLayout(false);
            this.darkSectionPanel1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudVertexNum)).EndInit();
            this.darkSectionPanel2.ResumeLayout(false);
            this.darkPanel1.ResumeLayout(false);
            this.panelTree.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private Controls.PanelRenderingMesh panelMesh;
        private DarkUI.Controls.DarkTreeView lstMeshes;
        private DarkUI.Controls.DarkButton btCancel;
        private DarkUI.Controls.DarkButton btOk;
        private DarkUI.Controls.DarkSectionPanel darkSectionPanel1;
        private DarkUI.Controls.DarkCheckBox cbShowVertices;
        private DarkUI.Controls.DarkButton butRemapVertex;
        private DarkUI.Controls.DarkNumericUpDown nudVertexNum;
        private DarkUI.Controls.DarkLabel darkLabel1;
        private DarkUI.Controls.DarkSectionPanel darkSectionPanel2;
        private DarkUI.Controls.DarkCheckBox cbVertexNumbers;
        private DarkUI.Controls.DarkButton butFindVertex;
        private DarkUI.Controls.DarkCheckBox cbWireframe;
        private DarkUI.Controls.DarkPanel darkPanel1;
        private DarkUI.Controls.DarkPanel panelTree;
    }
}