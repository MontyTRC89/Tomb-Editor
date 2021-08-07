namespace WadTool
{
    partial class FormMeshEditor
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
            this.lstMeshes = new DarkUI.Controls.DarkTreeView();
            this.panelMesh = new WadTool.Controls.PanelRenderingMesh();
            this.btCancel = new DarkUI.Controls.DarkButton();
            this.btOk = new DarkUI.Controls.DarkButton();
            this.panelEditing = new DarkUI.Controls.DarkSectionPanel();
            this.cbWireframe = new DarkUI.Controls.DarkCheckBox();
            this.cbExtra = new DarkUI.Controls.DarkCheckBox();
            this.cbEditingMode = new DarkUI.Controls.DarkComboBox();
            this.darkLabel2 = new DarkUI.Controls.DarkLabel();
            this.tabsModes = new TombLib.Controls.DarkTabbedContainer();
            this.tabVertexRemap = new System.Windows.Forms.TabPage();
            this.butAutoFit = new DarkUI.Controls.DarkButton();
            this.nudVertexNum = new DarkUI.Controls.DarkNumericUpDown();
            this.butFindVertex = new DarkUI.Controls.DarkButton();
            this.darkLabel1 = new DarkUI.Controls.DarkLabel();
            this.butRemapVertex = new DarkUI.Controls.DarkButton();
            this.tabVertexShadesAndNormals = new System.Windows.Forms.TabPage();
            this.butApplyShadesToAllVertices = new DarkUI.Controls.DarkButton();
            this.panelColor = new DarkUI.Controls.DarkPanel();
            this.butRecalcNormalsAvg = new DarkUI.Controls.DarkButton();
            this.darkLabel7 = new DarkUI.Controls.DarkLabel();
            this.darkLabel10 = new DarkUI.Controls.DarkLabel();
            this.butRecalcNormals = new DarkUI.Controls.DarkButton();
            this.tabVertexEffects = new System.Windows.Forms.TabPage();
            this.butPreview = new DarkUI.Controls.DarkButton();
            this.butConvertFromShades = new DarkUI.Controls.DarkButton();
            this.nudMove = new DarkUI.Controls.DarkNumericUpDown();
            this.darkLabel5 = new DarkUI.Controls.DarkLabel();
            this.nudGlow = new DarkUI.Controls.DarkNumericUpDown();
            this.darkLabel4 = new DarkUI.Controls.DarkLabel();
            this.butApplyToAllVertices = new DarkUI.Controls.DarkButton();
            this.tabSphere = new System.Windows.Forms.TabPage();
            this.darkLabel6 = new DarkUI.Controls.DarkLabel();
            this.darkLabel3 = new DarkUI.Controls.DarkLabel();
            this.butResetSphere = new DarkUI.Controls.DarkButton();
            this.nudSphereRadius = new DarkUI.Controls.DarkNumericUpDown();
            this.darkLabel9 = new DarkUI.Controls.DarkLabel();
            this.nudSphereZ = new DarkUI.Controls.DarkNumericUpDown();
            this.nudSphereY = new DarkUI.Controls.DarkNumericUpDown();
            this.nudSphereX = new DarkUI.Controls.DarkNumericUpDown();
            this.darkLabel8 = new DarkUI.Controls.DarkLabel();
            this.tabFaceAttributes = new System.Windows.Forms.TabPage();
            this.butApplyToAllFaces = new DarkUI.Controls.DarkButton();
            this.cbTexture = new DarkUI.Controls.DarkCheckBox();
            this.cbBlendMode = new DarkUI.Controls.DarkComboBox();
            this.cbBlend = new DarkUI.Controls.DarkCheckBox();
            this.nudShineStrength = new DarkUI.Controls.DarkNumericUpDown();
            this.cbSheen = new DarkUI.Controls.DarkCheckBox();
            this.butDoubleSide = new DarkUI.Controls.DarkButton();
            this.darkSectionPanel2 = new DarkUI.Controls.DarkSectionPanel();
            this.panelMain = new DarkUI.Controls.DarkPanel();
            this.panelEditingTools = new DarkUI.Controls.DarkPanel();
            this.panelTree = new DarkUI.Controls.DarkPanel();
            this.panelTexturing = new DarkUI.Controls.DarkPanel();
            this.darkSectionPanel1 = new DarkUI.Controls.DarkSectionPanel();
            this.panelTextureMap = new WadTool.Controls.PanelTextureMap();
            this.panelTexturingTools = new DarkUI.Controls.DarkPanel();
            this.butExportTexture = new DarkUI.Controls.DarkButton();
            this.butAddTexture = new DarkUI.Controls.DarkButton();
            this.butDeleteTexture = new DarkUI.Controls.DarkButton();
            this.comboCurrentTexture = new DarkUI.Controls.DarkComboBox();
            this.toolTip = new System.Windows.Forms.ToolTip(this.components);
            this.panelEditing.SuspendLayout();
            this.tabsModes.SuspendLayout();
            this.tabVertexRemap.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudVertexNum)).BeginInit();
            this.tabVertexShadesAndNormals.SuspendLayout();
            this.tabVertexEffects.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudMove)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudGlow)).BeginInit();
            this.tabSphere.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudSphereRadius)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudSphereZ)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudSphereY)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudSphereX)).BeginInit();
            this.tabFaceAttributes.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudShineStrength)).BeginInit();
            this.darkSectionPanel2.SuspendLayout();
            this.panelMain.SuspendLayout();
            this.panelEditingTools.SuspendLayout();
            this.panelTree.SuspendLayout();
            this.panelTexturing.SuspendLayout();
            this.darkSectionPanel1.SuspendLayout();
            this.panelTexturingTools.SuspendLayout();
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
            this.lstMeshes.Size = new System.Drawing.Size(240, 534);
            this.lstMeshes.TabIndex = 1;
            this.lstMeshes.Text = "darkTreeView1";
            this.lstMeshes.Click += new System.EventHandler(this.lstMeshes_Click);
            this.lstMeshes.KeyDown += new System.Windows.Forms.KeyEventHandler(this.lstMeshes_KeyDown);
            // 
            // panelMesh
            // 
            this.panelMesh.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelMesh.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.panelMesh.Location = new System.Drawing.Point(247, 0);
            this.panelMesh.Name = "panelMesh";
            this.panelMesh.Size = new System.Drawing.Size(612, 463);
            this.panelMesh.TabIndex = 0;
            // 
            // btCancel
            // 
            this.btCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btCancel.Checked = false;
            this.btCancel.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.btCancel.Location = new System.Drawing.Point(1084, 571);
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
            this.btOk.Location = new System.Drawing.Point(997, 571);
            this.btOk.Name = "btOk";
            this.btOk.Size = new System.Drawing.Size(81, 23);
            this.btOk.TabIndex = 53;
            this.btOk.Text = "OK";
            this.btOk.Click += new System.EventHandler(this.btOk_Click);
            // 
            // panelEditing
            // 
            this.panelEditing.Controls.Add(this.cbWireframe);
            this.panelEditing.Controls.Add(this.cbExtra);
            this.panelEditing.Controls.Add(this.cbEditingMode);
            this.panelEditing.Controls.Add(this.darkLabel2);
            this.panelEditing.Controls.Add(this.tabsModes);
            this.panelEditing.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panelEditing.Location = new System.Drawing.Point(0, 5);
            this.panelEditing.Name = "panelEditing";
            this.panelEditing.SectionHeader = "Tools";
            this.panelEditing.Size = new System.Drawing.Size(612, 92);
            this.panelEditing.TabIndex = 54;
            // 
            // cbWireframe
            // 
            this.cbWireframe.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.cbWireframe.AutoSize = true;
            this.cbWireframe.Location = new System.Drawing.Point(406, 35);
            this.cbWireframe.Name = "cbWireframe";
            this.cbWireframe.Size = new System.Drawing.Size(79, 17);
            this.cbWireframe.TabIndex = 6;
            this.cbWireframe.Text = "Wireframe";
            this.toolTip.SetToolTip(this.cbWireframe, "Draw mesh as wireframe");
            this.cbWireframe.CheckedChanged += new System.EventHandler(this.cbWireframe_CheckedChanged);
            // 
            // cbExtra
            // 
            this.cbExtra.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.cbExtra.Location = new System.Drawing.Point(491, 35);
            this.cbExtra.Name = "cbExtra";
            this.cbExtra.Size = new System.Drawing.Size(109, 17);
            this.cbExtra.TabIndex = 4;
            this.cbExtra.Text = "Show all numbers";
            this.cbExtra.CheckedChanged += new System.EventHandler(this.cbVertexNumbers_CheckedChanged);
            // 
            // cbEditingMode
            // 
            this.cbEditingMode.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.cbEditingMode.FormattingEnabled = true;
            this.cbEditingMode.Location = new System.Drawing.Point(88, 33);
            this.cbEditingMode.Name = "cbEditingMode";
            this.cbEditingMode.Size = new System.Drawing.Size(309, 23);
            this.cbEditingMode.TabIndex = 9;
            this.cbEditingMode.SelectedIndexChanged += new System.EventHandler(this.cbEditingMode_SelectedIndexChanged);
            // 
            // darkLabel2
            // 
            this.darkLabel2.AutoSize = true;
            this.darkLabel2.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.darkLabel2.Location = new System.Drawing.Point(6, 36);
            this.darkLabel2.Name = "darkLabel2";
            this.darkLabel2.Size = new System.Drawing.Size(79, 13);
            this.darkLabel2.TabIndex = 8;
            this.darkLabel2.Text = "Editing mode:";
            // 
            // tabsModes
            // 
            this.tabsModes.Alignment = System.Windows.Forms.TabAlignment.Right;
            this.tabsModes.Controls.Add(this.tabVertexRemap);
            this.tabsModes.Controls.Add(this.tabVertexShadesAndNormals);
            this.tabsModes.Controls.Add(this.tabVertexEffects);
            this.tabsModes.Controls.Add(this.tabSphere);
            this.tabsModes.Controls.Add(this.tabFaceAttributes);
            this.tabsModes.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.tabsModes.Location = new System.Drawing.Point(1, 52);
            this.tabsModes.Multiline = true;
            this.tabsModes.Name = "tabsModes";
            this.tabsModes.SelectedIndex = 0;
            this.tabsModes.Size = new System.Drawing.Size(610, 39);
            this.tabsModes.TabIndex = 7;
            // 
            // tabVertexRemap
            // 
            this.tabVertexRemap.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.tabVertexRemap.Controls.Add(this.butAutoFit);
            this.tabVertexRemap.Controls.Add(this.nudVertexNum);
            this.tabVertexRemap.Controls.Add(this.butFindVertex);
            this.tabVertexRemap.Controls.Add(this.darkLabel1);
            this.tabVertexRemap.Controls.Add(this.butRemapVertex);
            this.tabVertexRemap.Location = new System.Drawing.Point(4, 4);
            this.tabVertexRemap.Name = "tabVertexRemap";
            this.tabVertexRemap.Size = new System.Drawing.Size(497, 31);
            this.tabVertexRemap.TabIndex = 0;
            this.tabVertexRemap.Text = "Vertex remapping";
            // 
            // butAutoFit
            // 
            this.butAutoFit.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.butAutoFit.Checked = false;
            this.butAutoFit.Image = global::WadTool.Properties.Resources.import_16;
            this.butAutoFit.Location = new System.Drawing.Point(389, 5);
            this.butAutoFit.Name = "butAutoFit";
            this.butAutoFit.Size = new System.Drawing.Size(105, 23);
            this.butAutoFit.TabIndex = 6;
            this.butAutoFit.Text = "Automatic fit";
            this.butAutoFit.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.toolTip.SetToolTip(this.butAutoFit, "Try to automatically remap all vertices sitting on mesh holes.\r\nFor legacy engine" +
        "s, you still have to manually remap hair hole.");
            this.butAutoFit.Click += new System.EventHandler(this.butAutoFit_Click);
            // 
            // nudVertexNum
            // 
            this.nudVertexNum.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.nudVertexNum.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.nudVertexNum.IncrementAlternate = new decimal(new int[] {
            10,
            0,
            0,
            65536});
            this.nudVertexNum.Location = new System.Drawing.Point(88, 5);
            this.nudVertexNum.LoopValues = false;
            this.nudVertexNum.Maximum = new decimal(new int[] {
            65535,
            0,
            0,
            0});
            this.nudVertexNum.Name = "nudVertexNum";
            this.nudVertexNum.Size = new System.Drawing.Size(89, 23);
            this.nudVertexNum.TabIndex = 2;
            this.toolTip.SetToolTip(this.nudVertexNum, "Vertex number to operate with");
            this.nudVertexNum.KeyDown += new System.Windows.Forms.KeyEventHandler(this.nudVertexNum_KeyDown);
            // 
            // butFindVertex
            // 
            this.butFindVertex.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.butFindVertex.Checked = false;
            this.butFindVertex.Image = global::WadTool.Properties.Resources.general_search_16;
            this.butFindVertex.Location = new System.Drawing.Point(183, 5);
            this.butFindVertex.Name = "butFindVertex";
            this.butFindVertex.Size = new System.Drawing.Size(120, 23);
            this.butFindVertex.TabIndex = 5;
            this.butFindVertex.Text = "Jump to selected";
            this.butFindVertex.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.toolTip.SetToolTip(this.butFindVertex, "Jump to specified vertex number");
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
            this.butRemapVertex.Location = new System.Drawing.Point(309, 5);
            this.butRemapVertex.Name = "butRemapVertex";
            this.butRemapVertex.Size = new System.Drawing.Size(74, 23);
            this.butRemapVertex.TabIndex = 3;
            this.butRemapVertex.Text = "Remap";
            this.butRemapVertex.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.toolTip.SetToolTip(this.butRemapVertex, "Remap selected vertex to specified vertex number");
            this.butRemapVertex.Click += new System.EventHandler(this.butRemapVertex_Click);
            // 
            // tabVertexShadesAndNormals
            // 
            this.tabVertexShadesAndNormals.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.tabVertexShadesAndNormals.Controls.Add(this.butApplyShadesToAllVertices);
            this.tabVertexShadesAndNormals.Controls.Add(this.panelColor);
            this.tabVertexShadesAndNormals.Controls.Add(this.butRecalcNormalsAvg);
            this.tabVertexShadesAndNormals.Controls.Add(this.darkLabel7);
            this.tabVertexShadesAndNormals.Controls.Add(this.darkLabel10);
            this.tabVertexShadesAndNormals.Controls.Add(this.butRecalcNormals);
            this.tabVertexShadesAndNormals.Location = new System.Drawing.Point(4, 4);
            this.tabVertexShadesAndNormals.Name = "tabVertexShadesAndNormals";
            this.tabVertexShadesAndNormals.Padding = new System.Windows.Forms.Padding(3);
            this.tabVertexShadesAndNormals.Size = new System.Drawing.Size(497, 31);
            this.tabVertexShadesAndNormals.TabIndex = 3;
            this.tabVertexShadesAndNormals.Text = "Vertex shades and normals";
            // 
            // butApplyShadesToAllVertices
            // 
            this.butApplyShadesToAllVertices.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.butApplyShadesToAllVertices.Checked = false;
            this.butApplyShadesToAllVertices.Location = new System.Drawing.Point(412, 5);
            this.butApplyShadesToAllVertices.Name = "butApplyShadesToAllVertices";
            this.butApplyShadesToAllVertices.Size = new System.Drawing.Size(82, 23);
            this.butApplyShadesToAllVertices.TabIndex = 18;
            this.butApplyShadesToAllVertices.Text = "Apply to all";
            this.toolTip.SetToolTip(this.butApplyShadesToAllVertices, "Apply specified vertex shade to all vertices");
            this.butApplyShadesToAllVertices.Click += new System.EventHandler(this.butApplyShadesToAllVertices_Click);
            // 
            // panelColor
            // 
            this.panelColor.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panelColor.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panelColor.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.panelColor.Location = new System.Drawing.Point(349, 5);
            this.panelColor.Name = "panelColor";
            this.panelColor.Size = new System.Drawing.Size(57, 23);
            this.panelColor.TabIndex = 16;
            this.panelColor.Tag = "";
            this.toolTip.SetToolTip(this.panelColor, "Vertex shade (color) to apply");
            this.panelColor.MouseDown += new System.Windows.Forms.MouseEventHandler(this.panelColor_MouseDown);
            // 
            // butRecalcNormalsAvg
            // 
            this.butRecalcNormalsAvg.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.butRecalcNormalsAvg.Checked = false;
            this.butRecalcNormalsAvg.Location = new System.Drawing.Point(193, 5);
            this.butRecalcNormalsAvg.Name = "butRecalcNormalsAvg";
            this.butRecalcNormalsAvg.Size = new System.Drawing.Size(72, 23);
            this.butRecalcNormalsAvg.TabIndex = 23;
            this.butRecalcNormalsAvg.Text = "Averaged";
            this.toolTip.SetToolTip(this.butRecalcNormalsAvg, "Averaged normals recalculation is more rough and straighforward way,\r\nbut still m" +
        "ay be useful in some cases.");
            this.butRecalcNormalsAvg.Click += new System.EventHandler(this.butRecalcNormalsAvg_Click);
            // 
            // darkLabel7
            // 
            this.darkLabel7.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.darkLabel7.AutoSize = true;
            this.darkLabel7.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.darkLabel7.Location = new System.Drawing.Point(272, 8);
            this.darkLabel7.Name = "darkLabel7";
            this.darkLabel7.Size = new System.Drawing.Size(75, 13);
            this.darkLabel7.TabIndex = 21;
            this.darkLabel7.Text = "Vertex shade:";
            // 
            // darkLabel10
            // 
            this.darkLabel10.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.darkLabel10.AutoSize = true;
            this.darkLabel10.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.darkLabel10.Location = new System.Drawing.Point(3, 8);
            this.darkLabel10.Name = "darkLabel10";
            this.darkLabel10.Size = new System.Drawing.Size(112, 13);
            this.darkLabel10.TabIndex = 22;
            this.darkLabel10.Text = "Recalculate normals:";
            // 
            // butRecalcNormals
            // 
            this.butRecalcNormals.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.butRecalcNormals.Checked = false;
            this.butRecalcNormals.Location = new System.Drawing.Point(115, 5);
            this.butRecalcNormals.Name = "butRecalcNormals";
            this.butRecalcNormals.Size = new System.Drawing.Size(72, 23);
            this.butRecalcNormals.TabIndex = 20;
            this.butRecalcNormals.Text = "Weighted";
            this.toolTip.SetToolTip(this.butRecalcNormals, "Weighted normals recalculation gives better results\r\nbecause it takes angles betw" +
        "een different faces in consideration.");
            this.butRecalcNormals.Click += new System.EventHandler(this.butRecalcNormals_Click);
            // 
            // tabVertexEffects
            // 
            this.tabVertexEffects.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.tabVertexEffects.Controls.Add(this.butPreview);
            this.tabVertexEffects.Controls.Add(this.butConvertFromShades);
            this.tabVertexEffects.Controls.Add(this.nudMove);
            this.tabVertexEffects.Controls.Add(this.darkLabel5);
            this.tabVertexEffects.Controls.Add(this.nudGlow);
            this.tabVertexEffects.Controls.Add(this.darkLabel4);
            this.tabVertexEffects.Controls.Add(this.butApplyToAllVertices);
            this.tabVertexEffects.Location = new System.Drawing.Point(4, 4);
            this.tabVertexEffects.Name = "tabVertexEffects";
            this.tabVertexEffects.Padding = new System.Windows.Forms.Padding(3);
            this.tabVertexEffects.Size = new System.Drawing.Size(497, 31);
            this.tabVertexEffects.TabIndex = 2;
            this.tabVertexEffects.Text = "Vertex effects";
            // 
            // butPreview
            // 
            this.butPreview.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.butPreview.Checked = false;
            this.butPreview.Image = global::WadTool.Properties.Resources.play_16;
            this.butPreview.Location = new System.Drawing.Point(216, 5);
            this.butPreview.Name = "butPreview";
            this.butPreview.Size = new System.Drawing.Size(24, 23);
            this.butPreview.TabIndex = 21;
            this.butPreview.Tag = "";
            this.toolTip.SetToolTip(this.butPreview, "Preview effects");
            this.butPreview.Click += new System.EventHandler(this.butPreview_Click);
            // 
            // butConvertFromShades
            // 
            this.butConvertFromShades.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.butConvertFromShades.Checked = false;
            this.butConvertFromShades.Location = new System.Drawing.Point(246, 5);
            this.butConvertFromShades.Name = "butConvertFromShades";
            this.butConvertFromShades.Size = new System.Drawing.Size(160, 23);
            this.butConvertFromShades.TabIndex = 20;
            this.butConvertFromShades.Text = "Convert from shades";
            this.toolTip.SetToolTip(this.butConvertFromShades, "Convert vertex effects from legacy workflow involving vertex shades.");
            this.butConvertFromShades.Click += new System.EventHandler(this.butConvertFromShades_Click);
            // 
            // nudMove
            // 
            this.nudMove.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.nudMove.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.nudMove.IncrementAlternate = new decimal(new int[] {
            10,
            0,
            0,
            65536});
            this.nudMove.Location = new System.Drawing.Point(145, 5);
            this.nudMove.LoopValues = false;
            this.nudMove.Maximum = new decimal(new int[] {
            63,
            0,
            0,
            0});
            this.nudMove.Name = "nudMove";
            this.nudMove.Size = new System.Drawing.Size(65, 23);
            this.nudMove.TabIndex = 14;
            this.toolTip.SetToolTip(this.nudMove, "For legacy engines, any value above 0 results in constant movement only for merge" +
        "d statics.\r\nIn such case, movement strength is derived from room effect strength" +
        ".\r\n");
            // 
            // darkLabel5
            // 
            this.darkLabel5.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.darkLabel5.AutoSize = true;
            this.darkLabel5.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.darkLabel5.Location = new System.Drawing.Point(108, 8);
            this.darkLabel5.Name = "darkLabel5";
            this.darkLabel5.Size = new System.Drawing.Size(38, 13);
            this.darkLabel5.TabIndex = 13;
            this.darkLabel5.Text = "Move:";
            // 
            // nudGlow
            // 
            this.nudGlow.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.nudGlow.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.nudGlow.IncrementAlternate = new decimal(new int[] {
            10,
            0,
            0,
            65536});
            this.nudGlow.Location = new System.Drawing.Point(38, 5);
            this.nudGlow.LoopValues = false;
            this.nudGlow.Maximum = new decimal(new int[] {
            63,
            0,
            0,
            0});
            this.nudGlow.Name = "nudGlow";
            this.nudGlow.Size = new System.Drawing.Size(65, 23);
            this.nudGlow.TabIndex = 12;
            this.toolTip.SetToolTip(this.nudGlow, "For legacy engines, any value above 0 results in constant glow only for merged st" +
        "atics.\r\nIn such case, glow strength is derived from room effect strength.");
            // 
            // darkLabel4
            // 
            this.darkLabel4.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.darkLabel4.AutoSize = true;
            this.darkLabel4.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.darkLabel4.Location = new System.Drawing.Point(3, 8);
            this.darkLabel4.Name = "darkLabel4";
            this.darkLabel4.Size = new System.Drawing.Size(37, 13);
            this.darkLabel4.TabIndex = 11;
            this.darkLabel4.Text = "Glow:";
            // 
            // butApplyToAllVertices
            // 
            this.butApplyToAllVertices.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.butApplyToAllVertices.Checked = false;
            this.butApplyToAllVertices.Location = new System.Drawing.Point(412, 5);
            this.butApplyToAllVertices.Name = "butApplyToAllVertices";
            this.butApplyToAllVertices.Size = new System.Drawing.Size(82, 23);
            this.butApplyToAllVertices.TabIndex = 10;
            this.butApplyToAllVertices.Text = "Apply to all";
            this.toolTip.SetToolTip(this.butApplyToAllVertices, "Apply specified vertex attributes to all faces");
            this.butApplyToAllVertices.Click += new System.EventHandler(this.butApplyToAllVertices_Click);
            // 
            // tabSphere
            // 
            this.tabSphere.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.tabSphere.Controls.Add(this.darkLabel6);
            this.tabSphere.Controls.Add(this.darkLabel3);
            this.tabSphere.Controls.Add(this.butResetSphere);
            this.tabSphere.Controls.Add(this.nudSphereRadius);
            this.tabSphere.Controls.Add(this.darkLabel9);
            this.tabSphere.Controls.Add(this.nudSphereZ);
            this.tabSphere.Controls.Add(this.nudSphereY);
            this.tabSphere.Controls.Add(this.nudSphereX);
            this.tabSphere.Controls.Add(this.darkLabel8);
            this.tabSphere.Location = new System.Drawing.Point(4, 4);
            this.tabSphere.Name = "tabSphere";
            this.tabSphere.Padding = new System.Windows.Forms.Padding(3);
            this.tabSphere.Size = new System.Drawing.Size(497, 31);
            this.tabSphere.TabIndex = 4;
            this.tabSphere.Text = "Sphere";
            // 
            // darkLabel6
            // 
            this.darkLabel6.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.darkLabel6.AutoSize = true;
            this.darkLabel6.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.darkLabel6.Location = new System.Drawing.Point(177, 8);
            this.darkLabel6.Name = "darkLabel6";
            this.darkLabel6.Size = new System.Drawing.Size(16, 13);
            this.darkLabel6.TabIndex = 23;
            this.darkLabel6.Text = "Z:";
            // 
            // darkLabel3
            // 
            this.darkLabel3.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.darkLabel3.AutoSize = true;
            this.darkLabel3.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.darkLabel3.Location = new System.Drawing.Point(90, 8);
            this.darkLabel3.Name = "darkLabel3";
            this.darkLabel3.Size = new System.Drawing.Size(15, 13);
            this.darkLabel3.TabIndex = 22;
            this.darkLabel3.Text = "Y:";
            // 
            // butResetSphere
            // 
            this.butResetSphere.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.butResetSphere.Checked = false;
            this.butResetSphere.Location = new System.Drawing.Point(409, 5);
            this.butResetSphere.Name = "butResetSphere";
            this.butResetSphere.Size = new System.Drawing.Size(85, 23);
            this.butResetSphere.TabIndex = 21;
            this.butResetSphere.Text = "Recalculate";
            this.toolTip.SetToolTip(this.butResetSphere, "Calculates average sphere which encloses the mesh");
            this.butResetSphere.Click += new System.EventHandler(this.butResetSphere_Click);
            // 
            // nudSphereRadius
            // 
            this.nudSphereRadius.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.nudSphereRadius.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.nudSphereRadius.IncrementAlternate = new decimal(new int[] {
            10,
            0,
            0,
            65536});
            this.nudSphereRadius.Location = new System.Drawing.Point(312, 5);
            this.nudSphereRadius.LoopValues = false;
            this.nudSphereRadius.Maximum = new decimal(new int[] {
            1000000,
            0,
            0,
            0});
            this.nudSphereRadius.Name = "nudSphereRadius";
            this.nudSphereRadius.Size = new System.Drawing.Size(91, 23);
            this.nudSphereRadius.TabIndex = 15;
            this.toolTip.SetToolTip(this.nudSphereRadius, "Shininess value on the range from 0 to 63");
            this.nudSphereRadius.ValueChanged += new System.EventHandler(this.nudSphereData_ValueChanged);
            this.nudSphereRadius.Validated += new System.EventHandler(this.nudSphereData_ValueChanged);
            // 
            // darkLabel9
            // 
            this.darkLabel9.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.darkLabel9.AutoSize = true;
            this.darkLabel9.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.darkLabel9.Location = new System.Drawing.Point(265, 8);
            this.darkLabel9.Name = "darkLabel9";
            this.darkLabel9.Size = new System.Drawing.Size(45, 13);
            this.darkLabel9.TabIndex = 14;
            this.darkLabel9.Text = "Radius:";
            // 
            // nudSphereZ
            // 
            this.nudSphereZ.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.nudSphereZ.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.nudSphereZ.IncrementAlternate = new decimal(new int[] {
            10,
            0,
            0,
            65536});
            this.nudSphereZ.Location = new System.Drawing.Point(194, 5);
            this.nudSphereZ.LoopValues = false;
            this.nudSphereZ.Maximum = new decimal(new int[] {
            1000000,
            0,
            0,
            0});
            this.nudSphereZ.Minimum = new decimal(new int[] {
            1000000,
            0,
            0,
            -2147483648});
            this.nudSphereZ.Name = "nudSphereZ";
            this.nudSphereZ.Size = new System.Drawing.Size(65, 23);
            this.nudSphereZ.TabIndex = 13;
            this.toolTip.SetToolTip(this.nudSphereZ, "Shininess value on the range from 0 to 63");
            this.nudSphereZ.ValueChanged += new System.EventHandler(this.nudSphereData_ValueChanged);
            this.nudSphereZ.Validated += new System.EventHandler(this.nudSphereData_ValueChanged);
            // 
            // nudSphereY
            // 
            this.nudSphereY.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.nudSphereY.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.nudSphereY.IncrementAlternate = new decimal(new int[] {
            10,
            0,
            0,
            65536});
            this.nudSphereY.Location = new System.Drawing.Point(107, 5);
            this.nudSphereY.LoopValues = false;
            this.nudSphereY.Maximum = new decimal(new int[] {
            1000000,
            0,
            0,
            0});
            this.nudSphereY.Minimum = new decimal(new int[] {
            1000000,
            0,
            0,
            -2147483648});
            this.nudSphereY.Name = "nudSphereY";
            this.nudSphereY.Size = new System.Drawing.Size(65, 23);
            this.nudSphereY.TabIndex = 11;
            this.toolTip.SetToolTip(this.nudSphereY, "Shininess value on the range from 0 to 63");
            this.nudSphereY.ValueChanged += new System.EventHandler(this.nudSphereData_ValueChanged);
            this.nudSphereY.Validated += new System.EventHandler(this.nudSphereData_ValueChanged);
            // 
            // nudSphereX
            // 
            this.nudSphereX.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.nudSphereX.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.nudSphereX.IncrementAlternate = new decimal(new int[] {
            10,
            0,
            0,
            65536});
            this.nudSphereX.Location = new System.Drawing.Point(19, 5);
            this.nudSphereX.LoopValues = false;
            this.nudSphereX.Maximum = new decimal(new int[] {
            1000000,
            0,
            0,
            0});
            this.nudSphereX.Minimum = new decimal(new int[] {
            1000000,
            0,
            0,
            -2147483648});
            this.nudSphereX.Name = "nudSphereX";
            this.nudSphereX.Size = new System.Drawing.Size(65, 23);
            this.nudSphereX.TabIndex = 9;
            this.toolTip.SetToolTip(this.nudSphereX, "Shininess value on the range from 0 to 63");
            this.nudSphereX.ValueChanged += new System.EventHandler(this.nudSphereData_ValueChanged);
            this.nudSphereX.Validated += new System.EventHandler(this.nudSphereData_ValueChanged);
            // 
            // darkLabel8
            // 
            this.darkLabel8.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.darkLabel8.AutoSize = true;
            this.darkLabel8.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.darkLabel8.Location = new System.Drawing.Point(3, 8);
            this.darkLabel8.Name = "darkLabel8";
            this.darkLabel8.Size = new System.Drawing.Size(16, 13);
            this.darkLabel8.TabIndex = 8;
            this.darkLabel8.Text = "X:";
            // 
            // tabFaceAttributes
            // 
            this.tabFaceAttributes.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.tabFaceAttributes.Controls.Add(this.butApplyToAllFaces);
            this.tabFaceAttributes.Controls.Add(this.cbTexture);
            this.tabFaceAttributes.Controls.Add(this.cbBlendMode);
            this.tabFaceAttributes.Controls.Add(this.cbBlend);
            this.tabFaceAttributes.Controls.Add(this.nudShineStrength);
            this.tabFaceAttributes.Controls.Add(this.cbSheen);
            this.tabFaceAttributes.Controls.Add(this.butDoubleSide);
            this.tabFaceAttributes.Location = new System.Drawing.Point(4, 4);
            this.tabFaceAttributes.Name = "tabFaceAttributes";
            this.tabFaceAttributes.Padding = new System.Windows.Forms.Padding(3);
            this.tabFaceAttributes.Size = new System.Drawing.Size(497, 31);
            this.tabFaceAttributes.TabIndex = 1;
            this.tabFaceAttributes.Text = "Face attributes and texturing";
            // 
            // butApplyToAllFaces
            // 
            this.butApplyToAllFaces.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.butApplyToAllFaces.Checked = false;
            this.butApplyToAllFaces.Location = new System.Drawing.Point(412, 5);
            this.butApplyToAllFaces.Name = "butApplyToAllFaces";
            this.butApplyToAllFaces.Size = new System.Drawing.Size(82, 23);
            this.butApplyToAllFaces.TabIndex = 9;
            this.butApplyToAllFaces.Text = "Apply to all";
            this.toolTip.SetToolTip(this.butApplyToAllFaces, "Apply specified face attributes to all faces");
            this.butApplyToAllFaces.Click += new System.EventHandler(this.butApplyToAllFaces_Click);
            // 
            // cbTexture
            // 
            this.cbTexture.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.cbTexture.AutoSize = true;
            this.cbTexture.Checked = true;
            this.cbTexture.CheckState = System.Windows.Forms.CheckState.Checked;
            this.cbTexture.Location = new System.Drawing.Point(342, 7);
            this.cbTexture.Name = "cbTexture";
            this.cbTexture.Size = new System.Drawing.Size(62, 17);
            this.cbTexture.TabIndex = 15;
            this.cbTexture.Text = "Texture";
            this.toolTip.SetToolTip(this.cbTexture, "Apply textures to faces");
            // 
            // cbBlendMode
            // 
            this.cbBlendMode.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.cbBlendMode.FormattingEnabled = true;
            this.cbBlendMode.Location = new System.Drawing.Point(214, 5);
            this.cbBlendMode.Name = "cbBlendMode";
            this.cbBlendMode.Size = new System.Drawing.Size(89, 23);
            this.cbBlendMode.TabIndex = 11;
            // 
            // cbBlend
            // 
            this.cbBlend.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.cbBlend.AutoSize = true;
            this.cbBlend.Checked = true;
            this.cbBlend.CheckState = System.Windows.Forms.CheckState.Checked;
            this.cbBlend.Location = new System.Drawing.Point(127, 7);
            this.cbBlend.Name = "cbBlend";
            this.cbBlend.Size = new System.Drawing.Size(91, 17);
            this.cbBlend.TabIndex = 14;
            this.cbBlend.Text = "Blend mode:";
            this.toolTip.SetToolTip(this.cbBlend, "Apply blend mode and double-sided attribute to faces");
            // 
            // nudShineStrength
            // 
            this.nudShineStrength.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.nudShineStrength.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.nudShineStrength.IncrementAlternate = new decimal(new int[] {
            10,
            0,
            0,
            65536});
            this.nudShineStrength.Location = new System.Drawing.Point(62, 5);
            this.nudShineStrength.LoopValues = false;
            this.nudShineStrength.Maximum = new decimal(new int[] {
            63,
            0,
            0,
            0});
            this.nudShineStrength.Name = "nudShineStrength";
            this.nudShineStrength.Size = new System.Drawing.Size(55, 23);
            this.nudShineStrength.TabIndex = 7;
            this.toolTip.SetToolTip(this.nudShineStrength, "Shininess value on the range from 0 to 63");
            // 
            // cbSheen
            // 
            this.cbSheen.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.cbSheen.AutoSize = true;
            this.cbSheen.Checked = true;
            this.cbSheen.CheckState = System.Windows.Forms.CheckState.Checked;
            this.cbSheen.Location = new System.Drawing.Point(6, 7);
            this.cbSheen.Name = "cbSheen";
            this.cbSheen.Size = new System.Drawing.Size(61, 17);
            this.cbSheen.TabIndex = 13;
            this.cbSheen.Text = "Sheen:";
            this.toolTip.SetToolTip(this.cbSheen, "Apply sheen attribute to faces");
            // 
            // butDoubleSide
            // 
            this.butDoubleSide.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.butDoubleSide.Checked = false;
            this.butDoubleSide.Image = global::WadTool.Properties.Resources.texture_DoubleSided_1_16;
            this.butDoubleSide.Location = new System.Drawing.Point(309, 5);
            this.butDoubleSide.Name = "butDoubleSide";
            this.butDoubleSide.Size = new System.Drawing.Size(24, 23);
            this.butDoubleSide.TabIndex = 12;
            this.butDoubleSide.Tag = "";
            this.toolTip.SetToolTip(this.butDoubleSide, "Double-sided");
            this.butDoubleSide.Click += new System.EventHandler(this.butDoubleSide_Click);
            // 
            // darkSectionPanel2
            // 
            this.darkSectionPanel2.Controls.Add(this.lstMeshes);
            this.darkSectionPanel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.darkSectionPanel2.Location = new System.Drawing.Point(0, 0);
            this.darkSectionPanel2.Name = "darkSectionPanel2";
            this.darkSectionPanel2.SectionHeader = "Mesh list";
            this.darkSectionPanel2.Size = new System.Drawing.Size(242, 560);
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
            this.panelMain.Controls.Add(this.panelTexturing);
            this.panelMain.Location = new System.Drawing.Point(5, 5);
            this.panelMain.Name = "panelMain";
            this.panelMain.Size = new System.Drawing.Size(1160, 560);
            this.panelMain.TabIndex = 56;
            // 
            // panelEditingTools
            // 
            this.panelEditingTools.Controls.Add(this.panelEditing);
            this.panelEditingTools.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panelEditingTools.Location = new System.Drawing.Point(247, 463);
            this.panelEditingTools.Name = "panelEditingTools";
            this.panelEditingTools.Size = new System.Drawing.Size(612, 97);
            this.panelEditingTools.TabIndex = 55;
            // 
            // panelTree
            // 
            this.panelTree.Controls.Add(this.darkSectionPanel2);
            this.panelTree.Dock = System.Windows.Forms.DockStyle.Left;
            this.panelTree.Location = new System.Drawing.Point(0, 0);
            this.panelTree.Name = "panelTree";
            this.panelTree.Padding = new System.Windows.Forms.Padding(0, 0, 5, 0);
            this.panelTree.Size = new System.Drawing.Size(247, 560);
            this.panelTree.TabIndex = 0;
            // 
            // panelTexturing
            // 
            this.panelTexturing.Controls.Add(this.darkSectionPanel1);
            this.panelTexturing.Dock = System.Windows.Forms.DockStyle.Right;
            this.panelTexturing.Location = new System.Drawing.Point(859, 0);
            this.panelTexturing.Name = "panelTexturing";
            this.panelTexturing.Padding = new System.Windows.Forms.Padding(3, 0, 0, 0);
            this.panelTexturing.Size = new System.Drawing.Size(301, 560);
            this.panelTexturing.TabIndex = 56;
            // 
            // darkSectionPanel1
            // 
            this.darkSectionPanel1.Controls.Add(this.panelTextureMap);
            this.darkSectionPanel1.Controls.Add(this.panelTexturingTools);
            this.darkSectionPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.darkSectionPanel1.Location = new System.Drawing.Point(3, 0);
            this.darkSectionPanel1.Name = "darkSectionPanel1";
            this.darkSectionPanel1.SectionHeader = "Texturing";
            this.darkSectionPanel1.Size = new System.Drawing.Size(298, 560);
            this.darkSectionPanel1.TabIndex = 0;
            // 
            // panelTextureMap
            // 
            this.panelTextureMap.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelTextureMap.Location = new System.Drawing.Point(1, 53);
            this.panelTextureMap.Name = "panelTextureMap";
            this.panelTextureMap.Size = new System.Drawing.Size(296, 506);
            this.panelTextureMap.TabIndex = 0;
            // 
            // panelTexturingTools
            // 
            this.panelTexturingTools.Controls.Add(this.butExportTexture);
            this.panelTexturingTools.Controls.Add(this.butAddTexture);
            this.panelTexturingTools.Controls.Add(this.butDeleteTexture);
            this.panelTexturingTools.Controls.Add(this.comboCurrentTexture);
            this.panelTexturingTools.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelTexturingTools.Location = new System.Drawing.Point(1, 25);
            this.panelTexturingTools.Name = "panelTexturingTools";
            this.panelTexturingTools.Size = new System.Drawing.Size(296, 28);
            this.panelTexturingTools.TabIndex = 1;
            // 
            // butExportTexture
            // 
            this.butExportTexture.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.butExportTexture.Checked = false;
            this.butExportTexture.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.butExportTexture.Image = global::WadTool.Properties.Resources.general_Export_16;
            this.butExportTexture.Location = new System.Drawing.Point(242, 3);
            this.butExportTexture.Name = "butExportTexture";
            this.butExportTexture.Size = new System.Drawing.Size(24, 23);
            this.butExportTexture.TabIndex = 7;
            this.butExportTexture.Tag = "";
            this.toolTip.SetToolTip(this.butExportTexture, "Export to file");
            this.butExportTexture.Click += new System.EventHandler(this.butExportTexture_Click);
            // 
            // butAddTexture
            // 
            this.butAddTexture.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.butAddTexture.Checked = false;
            this.butAddTexture.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.butAddTexture.Image = global::WadTool.Properties.Resources.general_plus_math_16;
            this.butAddTexture.Location = new System.Drawing.Point(212, 3);
            this.butAddTexture.Name = "butAddTexture";
            this.butAddTexture.Size = new System.Drawing.Size(24, 23);
            this.butAddTexture.TabIndex = 5;
            this.butAddTexture.Tag = "";
            this.butAddTexture.Click += new System.EventHandler(this.butAddTexture_Click);
            // 
            // butDeleteTexture
            // 
            this.butDeleteTexture.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.butDeleteTexture.Checked = false;
            this.butDeleteTexture.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.butDeleteTexture.Image = global::WadTool.Properties.Resources.trash_16;
            this.butDeleteTexture.Location = new System.Drawing.Point(272, 3);
            this.butDeleteTexture.Name = "butDeleteTexture";
            this.butDeleteTexture.Size = new System.Drawing.Size(24, 23);
            this.butDeleteTexture.TabIndex = 6;
            this.toolTip.SetToolTip(this.butDeleteTexture, "Delete texture");
            this.butDeleteTexture.Click += new System.EventHandler(this.butDeleteTexture_Click);
            // 
            // comboCurrentTexture
            // 
            this.comboCurrentTexture.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.comboCurrentTexture.FormattingEnabled = true;
            this.comboCurrentTexture.Location = new System.Drawing.Point(0, 3);
            this.comboCurrentTexture.Name = "comboCurrentTexture";
            this.comboCurrentTexture.Size = new System.Drawing.Size(236, 23);
            this.comboCurrentTexture.TabIndex = 4;
            this.comboCurrentTexture.SelectedValueChanged += new System.EventHandler(this.comboCurrentTexture_SelectedValueChanged);
            // 
            // FormMeshEditor
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1170, 601);
            this.Controls.Add(this.panelMain);
            this.Controls.Add(this.btCancel);
            this.Controls.Add(this.btOk);
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(1000, 640);
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
            this.tabVertexShadesAndNormals.ResumeLayout(false);
            this.tabVertexShadesAndNormals.PerformLayout();
            this.tabVertexEffects.ResumeLayout(false);
            this.tabVertexEffects.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudMove)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudGlow)).EndInit();
            this.tabSphere.ResumeLayout(false);
            this.tabSphere.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudSphereRadius)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudSphereZ)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudSphereY)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudSphereX)).EndInit();
            this.tabFaceAttributes.ResumeLayout(false);
            this.tabFaceAttributes.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudShineStrength)).EndInit();
            this.darkSectionPanel2.ResumeLayout(false);
            this.panelMain.ResumeLayout(false);
            this.panelEditingTools.ResumeLayout(false);
            this.panelTree.ResumeLayout(false);
            this.panelTexturing.ResumeLayout(false);
            this.darkSectionPanel1.ResumeLayout(false);
            this.panelTexturingTools.ResumeLayout(false);
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
        private DarkUI.Controls.DarkCheckBox cbExtra;
        private DarkUI.Controls.DarkButton butFindVertex;
        private DarkUI.Controls.DarkCheckBox cbWireframe;
        private DarkUI.Controls.DarkPanel panelMain;
        private DarkUI.Controls.DarkPanel panelTree;
        private DarkUI.Controls.DarkPanel panelEditingTools;
        private TombLib.Controls.DarkTabbedContainer tabsModes;
        private System.Windows.Forms.TabPage tabVertexRemap;
        private System.Windows.Forms.TabPage tabFaceAttributes;
        private DarkUI.Controls.DarkLabel darkLabel2;
        private DarkUI.Controls.DarkNumericUpDown nudShineStrength;
        private DarkUI.Controls.DarkButton butApplyToAllFaces;
        private System.Windows.Forms.TabPage tabVertexEffects;
        private DarkUI.Controls.DarkButton butApplyToAllVertices;
        private DarkUI.Controls.DarkNumericUpDown nudGlow;
        private DarkUI.Controls.DarkLabel darkLabel4;
        private DarkUI.Controls.DarkNumericUpDown nudMove;
        private DarkUI.Controls.DarkLabel darkLabel5;
        private DarkUI.Controls.DarkComboBox cbBlendMode;
        private DarkUI.Controls.DarkPanel panelColor;
        private System.Windows.Forms.TabPage tabVertexShadesAndNormals;
        private DarkUI.Controls.DarkButton butApplyShadesToAllVertices;
        private DarkUI.Controls.DarkButton butRecalcNormals;
        private DarkUI.Controls.DarkButton butConvertFromShades;
        private DarkUI.Controls.DarkLabel darkLabel7;
        private DarkUI.Controls.DarkComboBox cbEditingMode;
        private DarkUI.Controls.DarkButton butDoubleSide;
        private System.Windows.Forms.ToolTip toolTip;
        private DarkUI.Controls.DarkButton butAutoFit;
        private System.Windows.Forms.TabPage tabSphere;
        private DarkUI.Controls.DarkNumericUpDown nudSphereZ;
        private DarkUI.Controls.DarkNumericUpDown nudSphereY;
        private DarkUI.Controls.DarkNumericUpDown nudSphereX;
        private DarkUI.Controls.DarkLabel darkLabel8;
        private DarkUI.Controls.DarkLabel darkLabel9;
        private DarkUI.Controls.DarkButton butResetSphere;
        private DarkUI.Controls.DarkButton butPreview;
        private DarkUI.Controls.DarkPanel panelTexturing;
        private DarkUI.Controls.DarkSectionPanel darkSectionPanel1;
        private Controls.PanelTextureMap panelTextureMap;
        private DarkUI.Controls.DarkPanel panelTexturingTools;
        private DarkUI.Controls.DarkButton butAddTexture;
        private DarkUI.Controls.DarkButton butDeleteTexture;
        private DarkUI.Controls.DarkComboBox comboCurrentTexture;
        private DarkUI.Controls.DarkCheckBox cbTexture;
        private DarkUI.Controls.DarkCheckBox cbBlend;
        private DarkUI.Controls.DarkCheckBox cbSheen;
        private DarkUI.Controls.DarkLabel darkLabel6;
        private DarkUI.Controls.DarkLabel darkLabel3;
        private DarkUI.Controls.DarkLabel darkLabel10;
        private DarkUI.Controls.DarkButton butRecalcNormalsAvg;
        private DarkUI.Controls.DarkButton butExportTexture;
        private DarkUI.Controls.DarkNumericUpDown nudSphereRadius;
    }
}