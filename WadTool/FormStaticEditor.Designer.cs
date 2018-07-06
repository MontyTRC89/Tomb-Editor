namespace WadTool
{
    partial class FormStaticEditor
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
            this.darkStatusStrip1 = new DarkUI.Controls.DarkStatusStrip();
            this.cbVisibilityBox = new DarkUI.Controls.DarkCheckBox();
            this.darkLabel2 = new DarkUI.Controls.DarkLabel();
            this.tbVisibilityBoxMinX = new DarkUI.Controls.DarkTextBox();
            this.tbVisibilityBoxMinY = new DarkUI.Controls.DarkTextBox();
            this.darkLabel3 = new DarkUI.Controls.DarkLabel();
            this.tbVisibilityBoxMinZ = new DarkUI.Controls.DarkTextBox();
            this.darkLabel4 = new DarkUI.Controls.DarkLabel();
            this.tbVisibilityBoxMaxZ = new DarkUI.Controls.DarkTextBox();
            this.darkLabel5 = new DarkUI.Controls.DarkLabel();
            this.tbVisibilityBoxMaxY = new DarkUI.Controls.DarkTextBox();
            this.darkLabel6 = new DarkUI.Controls.DarkLabel();
            this.tbVisibilityBoxMaxX = new DarkUI.Controls.DarkTextBox();
            this.darkLabel7 = new DarkUI.Controls.DarkLabel();
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
            this.cbCollisionBox = new DarkUI.Controls.DarkCheckBox();
            this.cbDrawGrid = new DarkUI.Controls.DarkCheckBox();
            this.cbDrawGizmo = new DarkUI.Controls.DarkCheckBox();
            this.butCalculateCollisionBox = new DarkUI.Controls.DarkButton();
            this.butCalculateVisibilityBox = new DarkUI.Controls.DarkButton();
            this.butSaveChanges = new DarkUI.Controls.DarkButton();
            this.butResetTranslation = new DarkUI.Controls.DarkButton();
            this.butResetRotation = new DarkUI.Controls.DarkButton();
            this.butResetScale = new DarkUI.Controls.DarkButton();
            this.darkLabel1 = new DarkUI.Controls.DarkLabel();
            this.darkLabel14 = new DarkUI.Controls.DarkLabel();
            this.butAddLight = new DarkUI.Controls.DarkButton();
            this.butDeleteLight = new DarkUI.Controls.DarkButton();
            this.numRadius = new DarkUI.Controls.DarkNumericUpDown();
            this.numIntensity = new DarkUI.Controls.DarkNumericUpDown();
            this.darkLabel15 = new DarkUI.Controls.DarkLabel();
            this.darkLabel16 = new DarkUI.Controls.DarkLabel();
            this.numAmbient = new DarkUI.Controls.DarkNumericUpDown();
            this.darkLabel17 = new DarkUI.Controls.DarkLabel();
            this.cbDrawLights = new DarkUI.Controls.DarkCheckBox();
            this.lstLights = new DarkUI.Controls.DarkTreeView();
            this.panelRendering = new WadTool.Controls.PanelRenderingStaticEditor();
            this.butImportMeshFromFile = new DarkUI.Controls.DarkButton();
            ((System.ComponentModel.ISupportInitialize)(this.numRadius)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numIntensity)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numAmbient)).BeginInit();
            this.SuspendLayout();
            // 
            // darkStatusStrip1
            // 
            this.darkStatusStrip1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.darkStatusStrip1.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.darkStatusStrip1.Location = new System.Drawing.Point(0, 705);
            this.darkStatusStrip1.Name = "darkStatusStrip1";
            this.darkStatusStrip1.Padding = new System.Windows.Forms.Padding(0, 5, 0, 3);
            this.darkStatusStrip1.Size = new System.Drawing.Size(1008, 24);
            this.darkStatusStrip1.TabIndex = 0;
            this.darkStatusStrip1.Text = "darkStatusStrip1";
            // 
            // cbVisibilityBox
            // 
            this.cbVisibilityBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.cbVisibilityBox.AutoSize = true;
            this.cbVisibilityBox.Location = new System.Drawing.Point(766, 14);
            this.cbVisibilityBox.Name = "cbVisibilityBox";
            this.cbVisibilityBox.Size = new System.Drawing.Size(91, 17);
            this.cbVisibilityBox.TabIndex = 50;
            this.cbVisibilityBox.Text = "Visibility Box";
            this.cbVisibilityBox.CheckedChanged += new System.EventHandler(this.cbVisibilityBox_CheckedChanged);
            // 
            // darkLabel2
            // 
            this.darkLabel2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.darkLabel2.AutoSize = true;
            this.darkLabel2.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.darkLabel2.Location = new System.Drawing.Point(763, 40);
            this.darkLabel2.Name = "darkLabel2";
            this.darkLabel2.Size = new System.Drawing.Size(38, 13);
            this.darkLabel2.TabIndex = 51;
            this.darkLabel2.Text = "X min:";
            // 
            // tbVisibilityBoxMinX
            // 
            this.tbVisibilityBoxMinX.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.tbVisibilityBoxMinX.Location = new System.Drawing.Point(766, 57);
            this.tbVisibilityBoxMinX.Name = "tbVisibilityBoxMinX";
            this.tbVisibilityBoxMinX.Size = new System.Drawing.Size(73, 22);
            this.tbVisibilityBoxMinX.TabIndex = 52;
            this.tbVisibilityBoxMinX.Validated += new System.EventHandler(this.tbVisibilityBoxMinX_Validated);
            // 
            // tbVisibilityBoxMinY
            // 
            this.tbVisibilityBoxMinY.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.tbVisibilityBoxMinY.Location = new System.Drawing.Point(845, 57);
            this.tbVisibilityBoxMinY.Name = "tbVisibilityBoxMinY";
            this.tbVisibilityBoxMinY.Size = new System.Drawing.Size(73, 22);
            this.tbVisibilityBoxMinY.TabIndex = 54;
            this.tbVisibilityBoxMinY.Validated += new System.EventHandler(this.tbVisibilityBoxMinY_Validated);
            // 
            // darkLabel3
            // 
            this.darkLabel3.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.darkLabel3.AutoSize = true;
            this.darkLabel3.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.darkLabel3.Location = new System.Drawing.Point(842, 40);
            this.darkLabel3.Name = "darkLabel3";
            this.darkLabel3.Size = new System.Drawing.Size(37, 13);
            this.darkLabel3.TabIndex = 53;
            this.darkLabel3.Text = "Y min:";
            // 
            // tbVisibilityBoxMinZ
            // 
            this.tbVisibilityBoxMinZ.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.tbVisibilityBoxMinZ.Location = new System.Drawing.Point(924, 57);
            this.tbVisibilityBoxMinZ.Name = "tbVisibilityBoxMinZ";
            this.tbVisibilityBoxMinZ.Size = new System.Drawing.Size(72, 22);
            this.tbVisibilityBoxMinZ.TabIndex = 56;
            this.tbVisibilityBoxMinZ.Validated += new System.EventHandler(this.tbVisibilityBoxMinZ_Validated);
            // 
            // darkLabel4
            // 
            this.darkLabel4.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.darkLabel4.AutoSize = true;
            this.darkLabel4.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.darkLabel4.Location = new System.Drawing.Point(921, 40);
            this.darkLabel4.Name = "darkLabel4";
            this.darkLabel4.Size = new System.Drawing.Size(38, 13);
            this.darkLabel4.TabIndex = 55;
            this.darkLabel4.Text = "Z min:";
            // 
            // tbVisibilityBoxMaxZ
            // 
            this.tbVisibilityBoxMaxZ.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.tbVisibilityBoxMaxZ.Location = new System.Drawing.Point(924, 105);
            this.tbVisibilityBoxMaxZ.Name = "tbVisibilityBoxMaxZ";
            this.tbVisibilityBoxMaxZ.Size = new System.Drawing.Size(72, 22);
            this.tbVisibilityBoxMaxZ.TabIndex = 62;
            this.tbVisibilityBoxMaxZ.Validated += new System.EventHandler(this.tbVisibilityBoxMaxZ_Validated);
            // 
            // darkLabel5
            // 
            this.darkLabel5.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.darkLabel5.AutoSize = true;
            this.darkLabel5.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.darkLabel5.Location = new System.Drawing.Point(921, 88);
            this.darkLabel5.Name = "darkLabel5";
            this.darkLabel5.Size = new System.Drawing.Size(39, 13);
            this.darkLabel5.TabIndex = 61;
            this.darkLabel5.Text = "Z max:";
            // 
            // tbVisibilityBoxMaxY
            // 
            this.tbVisibilityBoxMaxY.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.tbVisibilityBoxMaxY.Location = new System.Drawing.Point(845, 105);
            this.tbVisibilityBoxMaxY.Name = "tbVisibilityBoxMaxY";
            this.tbVisibilityBoxMaxY.Size = new System.Drawing.Size(73, 22);
            this.tbVisibilityBoxMaxY.TabIndex = 60;
            this.tbVisibilityBoxMaxY.Validated += new System.EventHandler(this.tbVisibilityBoxMaxY_Validated);
            // 
            // darkLabel6
            // 
            this.darkLabel6.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.darkLabel6.AutoSize = true;
            this.darkLabel6.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.darkLabel6.Location = new System.Drawing.Point(842, 88);
            this.darkLabel6.Name = "darkLabel6";
            this.darkLabel6.Size = new System.Drawing.Size(38, 13);
            this.darkLabel6.TabIndex = 59;
            this.darkLabel6.Text = "Y max:";
            // 
            // tbVisibilityBoxMaxX
            // 
            this.tbVisibilityBoxMaxX.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.tbVisibilityBoxMaxX.Location = new System.Drawing.Point(766, 105);
            this.tbVisibilityBoxMaxX.Name = "tbVisibilityBoxMaxX";
            this.tbVisibilityBoxMaxX.Size = new System.Drawing.Size(73, 22);
            this.tbVisibilityBoxMaxX.TabIndex = 58;
            this.tbVisibilityBoxMaxX.Validated += new System.EventHandler(this.tbVisibilityBoxMaxX_Validated);
            // 
            // darkLabel7
            // 
            this.darkLabel7.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.darkLabel7.AutoSize = true;
            this.darkLabel7.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.darkLabel7.Location = new System.Drawing.Point(763, 88);
            this.darkLabel7.Name = "darkLabel7";
            this.darkLabel7.Size = new System.Drawing.Size(39, 13);
            this.darkLabel7.TabIndex = 57;
            this.darkLabel7.Text = "X max:";
            // 
            // tbCollisionBoxMaxZ
            // 
            this.tbCollisionBoxMaxZ.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.tbCollisionBoxMaxZ.Location = new System.Drawing.Point(924, 279);
            this.tbCollisionBoxMaxZ.Name = "tbCollisionBoxMaxZ";
            this.tbCollisionBoxMaxZ.Size = new System.Drawing.Size(72, 22);
            this.tbCollisionBoxMaxZ.TabIndex = 76;
            this.tbCollisionBoxMaxZ.Validated += new System.EventHandler(this.tbCollisionBoxMaxZ_Validated);
            // 
            // darkLabel8
            // 
            this.darkLabel8.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.darkLabel8.AutoSize = true;
            this.darkLabel8.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.darkLabel8.Location = new System.Drawing.Point(921, 262);
            this.darkLabel8.Name = "darkLabel8";
            this.darkLabel8.Size = new System.Drawing.Size(39, 13);
            this.darkLabel8.TabIndex = 75;
            this.darkLabel8.Text = "Z max:";
            // 
            // tbCollisionBoxMaxY
            // 
            this.tbCollisionBoxMaxY.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.tbCollisionBoxMaxY.Location = new System.Drawing.Point(845, 279);
            this.tbCollisionBoxMaxY.Name = "tbCollisionBoxMaxY";
            this.tbCollisionBoxMaxY.Size = new System.Drawing.Size(73, 22);
            this.tbCollisionBoxMaxY.TabIndex = 74;
            this.tbCollisionBoxMaxY.TextChanged += new System.EventHandler(this.tbCollisionBoxMaxY_TextChanged);
            // 
            // darkLabel9
            // 
            this.darkLabel9.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.darkLabel9.AutoSize = true;
            this.darkLabel9.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.darkLabel9.Location = new System.Drawing.Point(842, 262);
            this.darkLabel9.Name = "darkLabel9";
            this.darkLabel9.Size = new System.Drawing.Size(38, 13);
            this.darkLabel9.TabIndex = 73;
            this.darkLabel9.Text = "Y max:";
            // 
            // tbCollisionBoxMaxX
            // 
            this.tbCollisionBoxMaxX.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.tbCollisionBoxMaxX.Location = new System.Drawing.Point(766, 279);
            this.tbCollisionBoxMaxX.Name = "tbCollisionBoxMaxX";
            this.tbCollisionBoxMaxX.Size = new System.Drawing.Size(73, 22);
            this.tbCollisionBoxMaxX.TabIndex = 72;
            this.tbCollisionBoxMaxX.Validated += new System.EventHandler(this.tbCollisionBoxMaxX_Validated);
            // 
            // darkLabel10
            // 
            this.darkLabel10.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.darkLabel10.AutoSize = true;
            this.darkLabel10.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.darkLabel10.Location = new System.Drawing.Point(763, 262);
            this.darkLabel10.Name = "darkLabel10";
            this.darkLabel10.Size = new System.Drawing.Size(39, 13);
            this.darkLabel10.TabIndex = 71;
            this.darkLabel10.Text = "X max:";
            // 
            // tbCollisionBoxMinZ
            // 
            this.tbCollisionBoxMinZ.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.tbCollisionBoxMinZ.Location = new System.Drawing.Point(924, 231);
            this.tbCollisionBoxMinZ.Name = "tbCollisionBoxMinZ";
            this.tbCollisionBoxMinZ.Size = new System.Drawing.Size(72, 22);
            this.tbCollisionBoxMinZ.TabIndex = 70;
            this.tbCollisionBoxMinZ.Validated += new System.EventHandler(this.tbCollisionBoxMinZ_Validated);
            // 
            // darkLabel11
            // 
            this.darkLabel11.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.darkLabel11.AutoSize = true;
            this.darkLabel11.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.darkLabel11.Location = new System.Drawing.Point(921, 214);
            this.darkLabel11.Name = "darkLabel11";
            this.darkLabel11.Size = new System.Drawing.Size(38, 13);
            this.darkLabel11.TabIndex = 69;
            this.darkLabel11.Text = "Z min:";
            // 
            // tbCollisionBoxMinY
            // 
            this.tbCollisionBoxMinY.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.tbCollisionBoxMinY.Location = new System.Drawing.Point(845, 231);
            this.tbCollisionBoxMinY.Name = "tbCollisionBoxMinY";
            this.tbCollisionBoxMinY.Size = new System.Drawing.Size(73, 22);
            this.tbCollisionBoxMinY.TabIndex = 68;
            this.tbCollisionBoxMinY.Validated += new System.EventHandler(this.tbCollisionBoxMinY_Validated);
            // 
            // darkLabel12
            // 
            this.darkLabel12.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.darkLabel12.AutoSize = true;
            this.darkLabel12.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.darkLabel12.Location = new System.Drawing.Point(842, 214);
            this.darkLabel12.Name = "darkLabel12";
            this.darkLabel12.Size = new System.Drawing.Size(37, 13);
            this.darkLabel12.TabIndex = 67;
            this.darkLabel12.Text = "Y min:";
            // 
            // tbCollisionBoxMinX
            // 
            this.tbCollisionBoxMinX.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.tbCollisionBoxMinX.Location = new System.Drawing.Point(766, 231);
            this.tbCollisionBoxMinX.Name = "tbCollisionBoxMinX";
            this.tbCollisionBoxMinX.Size = new System.Drawing.Size(73, 22);
            this.tbCollisionBoxMinX.TabIndex = 66;
            this.tbCollisionBoxMinX.Validated += new System.EventHandler(this.tbCollisionBoxMinX_Validated);
            // 
            // darkLabel13
            // 
            this.darkLabel13.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.darkLabel13.AutoSize = true;
            this.darkLabel13.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.darkLabel13.Location = new System.Drawing.Point(763, 214);
            this.darkLabel13.Name = "darkLabel13";
            this.darkLabel13.Size = new System.Drawing.Size(38, 13);
            this.darkLabel13.TabIndex = 65;
            this.darkLabel13.Text = "X min:";
            // 
            // cbCollisionBox
            // 
            this.cbCollisionBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.cbCollisionBox.AutoSize = true;
            this.cbCollisionBox.Location = new System.Drawing.Point(766, 188);
            this.cbCollisionBox.Name = "cbCollisionBox";
            this.cbCollisionBox.Size = new System.Drawing.Size(93, 17);
            this.cbCollisionBox.TabIndex = 64;
            this.cbCollisionBox.Text = "Collision Box";
            this.cbCollisionBox.CheckedChanged += new System.EventHandler(this.cbCollisionBox_CheckedChanged);
            // 
            // cbDrawGrid
            // 
            this.cbDrawGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.cbDrawGrid.AutoSize = true;
            this.cbDrawGrid.Checked = true;
            this.cbDrawGrid.CheckState = System.Windows.Forms.CheckState.Checked;
            this.cbDrawGrid.Location = new System.Drawing.Point(766, 672);
            this.cbDrawGrid.Name = "cbDrawGrid";
            this.cbDrawGrid.Size = new System.Drawing.Size(77, 17);
            this.cbDrawGrid.TabIndex = 78;
            this.cbDrawGrid.Text = "Draw grid";
            this.cbDrawGrid.CheckedChanged += new System.EventHandler(this.cbDrawGrid_CheckedChanged);
            // 
            // cbDrawGizmo
            // 
            this.cbDrawGizmo.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.cbDrawGizmo.AutoSize = true;
            this.cbDrawGizmo.Checked = true;
            this.cbDrawGizmo.CheckState = System.Windows.Forms.CheckState.Checked;
            this.cbDrawGizmo.Location = new System.Drawing.Point(766, 649);
            this.cbDrawGizmo.Name = "cbDrawGizmo";
            this.cbDrawGizmo.Size = new System.Drawing.Size(87, 17);
            this.cbDrawGizmo.TabIndex = 79;
            this.cbDrawGizmo.Text = "Draw gizmo";
            this.cbDrawGizmo.CheckedChanged += new System.EventHandler(this.cbDrawGizmo_CheckedChanged);
            // 
            // butCalculateCollisionBox
            // 
            this.butCalculateCollisionBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.butCalculateCollisionBox.Image = global::WadTool.Properties.Resources.resize_16;
            this.butCalculateCollisionBox.Location = new System.Drawing.Point(766, 317);
            this.butCalculateCollisionBox.Name = "butCalculateCollisionBox";
            this.butCalculateCollisionBox.Size = new System.Drawing.Size(230, 23);
            this.butCalculateCollisionBox.TabIndex = 77;
            this.butCalculateCollisionBox.Text = "Calculate collision box";
            this.butCalculateCollisionBox.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.butCalculateCollisionBox.Click += new System.EventHandler(this.butCalculateCollisionBox_Click);
            // 
            // butCalculateVisibilityBox
            // 
            this.butCalculateVisibilityBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.butCalculateVisibilityBox.Image = global::WadTool.Properties.Resources.resize_16;
            this.butCalculateVisibilityBox.Location = new System.Drawing.Point(766, 143);
            this.butCalculateVisibilityBox.Name = "butCalculateVisibilityBox";
            this.butCalculateVisibilityBox.Size = new System.Drawing.Size(230, 23);
            this.butCalculateVisibilityBox.TabIndex = 63;
            this.butCalculateVisibilityBox.Text = "Calculate visibility box";
            this.butCalculateVisibilityBox.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.butCalculateVisibilityBox.Click += new System.EventHandler(this.butCalculateVisibilityBox_Click);
            // 
            // butSaveChanges
            // 
            this.butSaveChanges.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.butSaveChanges.Image = global::WadTool.Properties.Resources.save_16;
            this.butSaveChanges.Location = new System.Drawing.Point(884, 668);
            this.butSaveChanges.Name = "butSaveChanges";
            this.butSaveChanges.Size = new System.Drawing.Size(112, 23);
            this.butSaveChanges.TabIndex = 46;
            this.butSaveChanges.Text = "Save changes";
            this.butSaveChanges.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.butSaveChanges.Click += new System.EventHandler(this.butSaveChanges_Click);
            // 
            // butResetTranslation
            // 
            this.butResetTranslation.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.butResetTranslation.Location = new System.Drawing.Point(804, 362);
            this.butResetTranslation.Name = "butResetTranslation";
            this.butResetTranslation.Size = new System.Drawing.Size(75, 23);
            this.butResetTranslation.TabIndex = 80;
            this.butResetTranslation.Text = "Translation";
            this.butResetTranslation.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.butResetTranslation.Click += new System.EventHandler(this.butResetTranslation_Click);
            // 
            // butResetRotation
            // 
            this.butResetRotation.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.butResetRotation.Location = new System.Drawing.Point(885, 362);
            this.butResetRotation.Name = "butResetRotation";
            this.butResetRotation.Size = new System.Drawing.Size(61, 23);
            this.butResetRotation.TabIndex = 81;
            this.butResetRotation.Text = "Rotation";
            this.butResetRotation.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.butResetRotation.Click += new System.EventHandler(this.butResetRotation_Click);
            // 
            // butResetScale
            // 
            this.butResetScale.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.butResetScale.Location = new System.Drawing.Point(952, 362);
            this.butResetScale.Name = "butResetScale";
            this.butResetScale.Size = new System.Drawing.Size(44, 23);
            this.butResetScale.TabIndex = 82;
            this.butResetScale.Text = "Scale";
            this.butResetScale.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.butResetScale.Click += new System.EventHandler(this.butResetScale_Click);
            // 
            // darkLabel1
            // 
            this.darkLabel1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.darkLabel1.AutoSize = true;
            this.darkLabel1.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.darkLabel1.Location = new System.Drawing.Point(763, 367);
            this.darkLabel1.Name = "darkLabel1";
            this.darkLabel1.Size = new System.Drawing.Size(35, 13);
            this.darkLabel1.TabIndex = 83;
            this.darkLabel1.Text = "Reset";
            // 
            // darkLabel14
            // 
            this.darkLabel14.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.darkLabel14.AutoSize = true;
            this.darkLabel14.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.darkLabel14.Location = new System.Drawing.Point(766, 408);
            this.darkLabel14.Name = "darkLabel14";
            this.darkLabel14.Size = new System.Drawing.Size(38, 13);
            this.darkLabel14.TabIndex = 84;
            this.darkLabel14.Text = "Lights";
            // 
            // butAddLight
            // 
            this.butAddLight.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.butAddLight.Image = global::WadTool.Properties.Resources.plus_math_16;
            this.butAddLight.Location = new System.Drawing.Point(769, 435);
            this.butAddLight.Name = "butAddLight";
            this.butAddLight.Size = new System.Drawing.Size(59, 23);
            this.butAddLight.TabIndex = 85;
            this.butAddLight.Text = "Add";
            this.butAddLight.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.butAddLight.Click += new System.EventHandler(this.butAddLight_Click);
            // 
            // butDeleteLight
            // 
            this.butDeleteLight.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.butDeleteLight.Image = global::WadTool.Properties.Resources.trash_16;
            this.butDeleteLight.Location = new System.Drawing.Point(834, 435);
            this.butDeleteLight.Name = "butDeleteLight";
            this.butDeleteLight.Size = new System.Drawing.Size(63, 23);
            this.butDeleteLight.TabIndex = 86;
            this.butDeleteLight.Text = "Delete";
            this.butDeleteLight.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            // 
            // numRadius
            // 
            this.numRadius.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.numRadius.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(69)))), ((int)(((byte)(73)))), ((int)(((byte)(74)))));
            this.numRadius.DecimalPlaces = 2;
            this.numRadius.Enabled = false;
            this.numRadius.ForeColor = System.Drawing.Color.Gainsboro;
            this.numRadius.Increment = new decimal(new int[] {
            3,
            0,
            0,
            131072});
            this.numRadius.IncrementAlternate = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.numRadius.Location = new System.Drawing.Point(834, 501);
            this.numRadius.Maximum = new decimal(new int[] {
            256,
            0,
            0,
            0});
            this.numRadius.MousewheelSingleIncrement = true;
            this.numRadius.Name = "numRadius";
            this.numRadius.Size = new System.Drawing.Size(63, 22);
            this.numRadius.TabIndex = 90;
            this.numRadius.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.numRadius.ValueChanged += new System.EventHandler(this.numInnerRange_ValueChanged);
            // 
            // numIntensity
            // 
            this.numIntensity.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.numIntensity.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(69)))), ((int)(((byte)(73)))), ((int)(((byte)(74)))));
            this.numIntensity.DecimalPlaces = 2;
            this.numIntensity.Enabled = false;
            this.numIntensity.ForeColor = System.Drawing.Color.Gainsboro;
            this.numIntensity.Increment = new decimal(new int[] {
            3,
            0,
            0,
            131072});
            this.numIntensity.IncrementAlternate = new decimal(new int[] {
            12,
            0,
            0,
            131072});
            this.numIntensity.Location = new System.Drawing.Point(834, 473);
            this.numIntensity.Maximum = new decimal(new int[] {
            128,
            0,
            0,
            0});
            this.numIntensity.Minimum = new decimal(new int[] {
            128,
            0,
            0,
            -2147483648});
            this.numIntensity.MousewheelSingleIncrement = true;
            this.numIntensity.Name = "numIntensity";
            this.numIntensity.Size = new System.Drawing.Size(63, 22);
            this.numIntensity.TabIndex = 89;
            this.numIntensity.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.numIntensity.ValueChanged += new System.EventHandler(this.numIntensity_ValueChanged);
            // 
            // darkLabel15
            // 
            this.darkLabel15.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.darkLabel15.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.darkLabel15.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.darkLabel15.Location = new System.Drawing.Point(769, 473);
            this.darkLabel15.Name = "darkLabel15";
            this.darkLabel15.Size = new System.Drawing.Size(51, 22);
            this.darkLabel15.TabIndex = 93;
            this.darkLabel15.Text = "Intensity";
            this.darkLabel15.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // darkLabel16
            // 
            this.darkLabel16.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.darkLabel16.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.darkLabel16.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.darkLabel16.Location = new System.Drawing.Point(769, 501);
            this.darkLabel16.Name = "darkLabel16";
            this.darkLabel16.Size = new System.Drawing.Size(51, 22);
            this.darkLabel16.TabIndex = 92;
            this.darkLabel16.Text = "Radius";
            this.darkLabel16.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // numAmbient
            // 
            this.numAmbient.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.numAmbient.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(69)))), ((int)(((byte)(73)))), ((int)(((byte)(74)))));
            this.numAmbient.ForeColor = System.Drawing.Color.Gainsboro;
            this.numAmbient.IncrementAlternate = new decimal(new int[] {
            8,
            0,
            0,
            0});
            this.numAmbient.Location = new System.Drawing.Point(834, 538);
            this.numAmbient.Maximum = new decimal(new int[] {
            255,
            0,
            0,
            0});
            this.numAmbient.MousewheelSingleIncrement = true;
            this.numAmbient.Name = "numAmbient";
            this.numAmbient.Size = new System.Drawing.Size(63, 22);
            this.numAmbient.TabIndex = 94;
            this.numAmbient.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.numAmbient.ValueChanged += new System.EventHandler(this.numAmbient_ValueChanged);
            // 
            // darkLabel17
            // 
            this.darkLabel17.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.darkLabel17.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.darkLabel17.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.darkLabel17.Location = new System.Drawing.Point(769, 538);
            this.darkLabel17.Name = "darkLabel17";
            this.darkLabel17.Size = new System.Drawing.Size(51, 22);
            this.darkLabel17.TabIndex = 95;
            this.darkLabel17.Text = "Ambient";
            this.darkLabel17.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // cbDrawLights
            // 
            this.cbDrawLights.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.cbDrawLights.AutoSize = true;
            this.cbDrawLights.Checked = true;
            this.cbDrawLights.CheckState = System.Windows.Forms.CheckState.Checked;
            this.cbDrawLights.Location = new System.Drawing.Point(766, 626);
            this.cbDrawLights.Name = "cbDrawLights";
            this.cbDrawLights.Size = new System.Drawing.Size(85, 17);
            this.cbDrawLights.TabIndex = 96;
            this.cbDrawLights.Text = "Draw lights";
            this.cbDrawLights.CheckedChanged += new System.EventHandler(this.cbDrawLights_CheckedChanged);
            // 
            // lstLights
            // 
            this.lstLights.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lstLights.Location = new System.Drawing.Point(903, 435);
            this.lstLights.MaxDragChange = 20;
            this.lstLights.Name = "lstLights";
            this.lstLights.Size = new System.Drawing.Size(93, 125);
            this.lstLights.TabIndex = 97;
            this.lstLights.Text = "darkTreeView1";
            this.lstLights.Click += new System.EventHandler(this.lstLights_Click);
            // 
            // panelRendering
            // 
            this.panelRendering.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panelRendering.Location = new System.Drawing.Point(13, 13);
            this.panelRendering.Name = "panelRendering";
            this.panelRendering.Size = new System.Drawing.Size(743, 678);
            this.panelRendering.TabIndex = 1;
            // 
            // butImportMeshFromFile
            // 
            this.butImportMeshFromFile.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.butImportMeshFromFile.Image = global::WadTool.Properties.Resources.opened_folder_16;
            this.butImportMeshFromFile.Location = new System.Drawing.Point(766, 585);
            this.butImportMeshFromFile.Name = "butImportMeshFromFile";
            this.butImportMeshFromFile.Size = new System.Drawing.Size(230, 23);
            this.butImportMeshFromFile.TabIndex = 98;
            this.butImportMeshFromFile.Text = "Import mesh from file";
            this.butImportMeshFromFile.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.butImportMeshFromFile.Click += new System.EventHandler(this.butImportMeshFromFile_Click);
            // 
            // FormStaticEditor
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1008, 729);
            this.Controls.Add(this.butImportMeshFromFile);
            this.Controls.Add(this.lstLights);
            this.Controls.Add(this.cbDrawLights);
            this.Controls.Add(this.numAmbient);
            this.Controls.Add(this.darkLabel17);
            this.Controls.Add(this.numRadius);
            this.Controls.Add(this.numIntensity);
            this.Controls.Add(this.butDeleteLight);
            this.Controls.Add(this.butAddLight);
            this.Controls.Add(this.darkLabel16);
            this.Controls.Add(this.darkLabel14);
            this.Controls.Add(this.darkLabel15);
            this.Controls.Add(this.darkLabel1);
            this.Controls.Add(this.butResetScale);
            this.Controls.Add(this.butResetRotation);
            this.Controls.Add(this.butResetTranslation);
            this.Controls.Add(this.cbDrawGizmo);
            this.Controls.Add(this.cbDrawGrid);
            this.Controls.Add(this.butCalculateCollisionBox);
            this.Controls.Add(this.tbCollisionBoxMaxZ);
            this.Controls.Add(this.darkLabel8);
            this.Controls.Add(this.tbCollisionBoxMaxY);
            this.Controls.Add(this.darkLabel9);
            this.Controls.Add(this.tbCollisionBoxMaxX);
            this.Controls.Add(this.darkLabel10);
            this.Controls.Add(this.tbCollisionBoxMinZ);
            this.Controls.Add(this.darkLabel11);
            this.Controls.Add(this.tbCollisionBoxMinY);
            this.Controls.Add(this.darkLabel12);
            this.Controls.Add(this.tbCollisionBoxMinX);
            this.Controls.Add(this.darkLabel13);
            this.Controls.Add(this.cbCollisionBox);
            this.Controls.Add(this.butCalculateVisibilityBox);
            this.Controls.Add(this.tbVisibilityBoxMaxZ);
            this.Controls.Add(this.darkLabel5);
            this.Controls.Add(this.tbVisibilityBoxMaxY);
            this.Controls.Add(this.darkLabel6);
            this.Controls.Add(this.tbVisibilityBoxMaxX);
            this.Controls.Add(this.darkLabel7);
            this.Controls.Add(this.tbVisibilityBoxMinZ);
            this.Controls.Add(this.darkLabel4);
            this.Controls.Add(this.tbVisibilityBoxMinY);
            this.Controls.Add(this.darkLabel3);
            this.Controls.Add(this.tbVisibilityBoxMinX);
            this.Controls.Add(this.darkLabel2);
            this.Controls.Add(this.cbVisibilityBox);
            this.Controls.Add(this.butSaveChanges);
            this.Controls.Add(this.panelRendering);
            this.Controls.Add(this.darkStatusStrip1);
            this.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(1024, 768);
            this.Name = "FormStaticEditor";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Static editor";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.FormStaticEditor_FormClosing);
            ((System.ComponentModel.ISupportInitialize)(this.numRadius)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numIntensity)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numAmbient)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private DarkUI.Controls.DarkStatusStrip darkStatusStrip1;
        private Controls.PanelRenderingStaticEditor panelRendering;
        private DarkUI.Controls.DarkButton butSaveChanges;
        private DarkUI.Controls.DarkCheckBox cbVisibilityBox;
        private DarkUI.Controls.DarkLabel darkLabel2;
        private DarkUI.Controls.DarkTextBox tbVisibilityBoxMinX;
        private DarkUI.Controls.DarkTextBox tbVisibilityBoxMinY;
        private DarkUI.Controls.DarkLabel darkLabel3;
        private DarkUI.Controls.DarkTextBox tbVisibilityBoxMinZ;
        private DarkUI.Controls.DarkLabel darkLabel4;
        private DarkUI.Controls.DarkTextBox tbVisibilityBoxMaxZ;
        private DarkUI.Controls.DarkLabel darkLabel5;
        private DarkUI.Controls.DarkTextBox tbVisibilityBoxMaxY;
        private DarkUI.Controls.DarkLabel darkLabel6;
        private DarkUI.Controls.DarkTextBox tbVisibilityBoxMaxX;
        private DarkUI.Controls.DarkLabel darkLabel7;
        private DarkUI.Controls.DarkButton butCalculateVisibilityBox;
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
        private DarkUI.Controls.DarkCheckBox cbCollisionBox;
        private DarkUI.Controls.DarkCheckBox cbDrawGrid;
        private DarkUI.Controls.DarkCheckBox cbDrawGizmo;
        private DarkUI.Controls.DarkButton butResetTranslation;
        private DarkUI.Controls.DarkButton butResetRotation;
        private DarkUI.Controls.DarkButton butResetScale;
        private DarkUI.Controls.DarkLabel darkLabel1;
        private DarkUI.Controls.DarkLabel darkLabel14;
        private DarkUI.Controls.DarkButton butAddLight;
        private DarkUI.Controls.DarkButton butDeleteLight;
        private DarkUI.Controls.DarkNumericUpDown numRadius;
        private DarkUI.Controls.DarkNumericUpDown numIntensity;
        private DarkUI.Controls.DarkLabel darkLabel15;
        private DarkUI.Controls.DarkLabel darkLabel16;
        private DarkUI.Controls.DarkNumericUpDown numAmbient;
        private DarkUI.Controls.DarkLabel darkLabel17;
        private DarkUI.Controls.DarkCheckBox cbDrawLights;
        private DarkUI.Controls.DarkTreeView lstLights;
        private DarkUI.Controls.DarkButton butImportMeshFromFile;
    }
}