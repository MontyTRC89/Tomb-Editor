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
            this.cbShowVertices = new DarkUI.Controls.DarkCheckBox();
            this.darkLabel1 = new DarkUI.Controls.DarkLabel();
            this.nudVertexNum = new DarkUI.Controls.DarkNumericUpDown();
            this.butRemapVertex = new DarkUI.Controls.DarkButton();
            this.darkSectionPanel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudVertexNum)).BeginInit();
            this.SuspendLayout();
            // 
            // lstMeshes
            // 
            this.lstMeshes.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.lstMeshes.Cursor = System.Windows.Forms.Cursors.Arrow;
            this.lstMeshes.EvenNodeColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.lstMeshes.FocusedNodeColor = System.Drawing.Color.FromArgb(((int)(((byte)(75)))), ((int)(((byte)(110)))), ((int)(((byte)(175)))));
            this.lstMeshes.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.lstMeshes.Location = new System.Drawing.Point(5, 5);
            this.lstMeshes.MaxDragChange = 20;
            this.lstMeshes.Name = "lstMeshes";
            this.lstMeshes.NonFocusedNodeColor = System.Drawing.Color.FromArgb(((int)(((byte)(92)))), ((int)(((byte)(92)))), ((int)(((byte)(92)))));
            this.lstMeshes.OddNodeColor = System.Drawing.Color.FromArgb(((int)(((byte)(57)))), ((int)(((byte)(60)))), ((int)(((byte)(62)))));
            this.lstMeshes.Size = new System.Drawing.Size(320, 440);
            this.lstMeshes.TabIndex = 1;
            this.lstMeshes.Text = "darkTreeView1";
            this.lstMeshes.Click += new System.EventHandler(this.lstMeshes_Click);
            // 
            // panelMesh
            // 
            this.panelMesh.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panelMesh.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.panelMesh.Location = new System.Drawing.Point(330, 5);
            this.panelMesh.Name = "panelMesh";
            this.panelMesh.Size = new System.Drawing.Size(457, 533);
            this.panelMesh.TabIndex = 0;
            // 
            // btCancel
            // 
            this.btCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btCancel.Checked = false;
            this.btCancel.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.btCancel.Location = new System.Drawing.Point(619, 542);
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
            this.btOk.Location = new System.Drawing.Point(706, 542);
            this.btOk.Name = "btOk";
            this.btOk.Size = new System.Drawing.Size(81, 23);
            this.btOk.TabIndex = 53;
            this.btOk.Text = "OK";
            this.btOk.Click += new System.EventHandler(this.btOk_Click);
            // 
            // darkSectionPanel1
            // 
            this.darkSectionPanel1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.darkSectionPanel1.Controls.Add(this.butRemapVertex);
            this.darkSectionPanel1.Controls.Add(this.nudVertexNum);
            this.darkSectionPanel1.Controls.Add(this.darkLabel1);
            this.darkSectionPanel1.Controls.Add(this.cbShowVertices);
            this.darkSectionPanel1.Location = new System.Drawing.Point(5, 451);
            this.darkSectionPanel1.Name = "darkSectionPanel1";
            this.darkSectionPanel1.SectionHeader = "Vertex remap";
            this.darkSectionPanel1.Size = new System.Drawing.Size(319, 85);
            this.darkSectionPanel1.TabIndex = 54;
            // 
            // cbShowVertices
            // 
            this.cbShowVertices.AutoSize = true;
            this.cbShowVertices.Location = new System.Drawing.Point(7, 28);
            this.cbShowVertices.Name = "cbShowVertices";
            this.cbShowVertices.Size = new System.Drawing.Size(126, 17);
            this.cbShowVertices.TabIndex = 0;
            this.cbShowVertices.Text = "Show mesh vertices";
            this.cbShowVertices.CheckedChanged += new System.EventHandler(this.cbShowVertices_CheckedChanged);
            // 
            // darkLabel1
            // 
            this.darkLabel1.AutoSize = true;
            this.darkLabel1.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.darkLabel1.Location = new System.Drawing.Point(4, 57);
            this.darkLabel1.Name = "darkLabel1";
            this.darkLabel1.Size = new System.Drawing.Size(125, 13);
            this.darkLabel1.TabIndex = 1;
            this.darkLabel1.Text = "Current vertex number:";
            // 
            // nudVertexNum
            // 
            this.nudVertexNum.IncrementAlternate = new decimal(new int[] {
            10,
            0,
            0,
            65536});
            this.nudVertexNum.Location = new System.Drawing.Point(135, 55);
            this.nudVertexNum.LoopValues = false;
            this.nudVertexNum.Maximum = new decimal(new int[] {
            65535,
            0,
            0,
            0});
            this.nudVertexNum.Name = "nudVertexNum";
            this.nudVertexNum.Size = new System.Drawing.Size(94, 22);
            this.nudVertexNum.TabIndex = 2;
            // 
            // butRemapVertex
            // 
            this.butRemapVertex.Checked = false;
            this.butRemapVertex.Location = new System.Drawing.Point(235, 55);
            this.butRemapVertex.Name = "butRemapVertex";
            this.butRemapVertex.Size = new System.Drawing.Size(77, 22);
            this.butRemapVertex.TabIndex = 3;
            this.butRemapVertex.Text = "Remap";
            this.butRemapVertex.Click += new System.EventHandler(this.butRemapVertex_Click);
            // 
            // FormMesh
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(792, 571);
            this.Controls.Add(this.darkSectionPanel1);
            this.Controls.Add(this.btCancel);
            this.Controls.Add(this.lstMeshes);
            this.Controls.Add(this.btOk);
            this.Controls.Add(this.panelMesh);
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(800, 600);
            this.Name = "FormMesh";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Meshes";
            this.darkSectionPanel1.ResumeLayout(false);
            this.darkSectionPanel1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudVertexNum)).EndInit();
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
    }
}