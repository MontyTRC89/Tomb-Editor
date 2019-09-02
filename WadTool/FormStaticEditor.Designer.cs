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
            this.butImportMeshFromFile = new DarkUI.Controls.DarkButton();
            this.cbDrawNormals = new DarkUI.Controls.DarkCheckBox();
            this.butRecalcNormals = new DarkUI.Controls.DarkButton();
            this.comboLightType = new DarkUI.Controls.DarkComboBox();
            this.tbPositionZ = new DarkUI.Controls.DarkTextBox();
            this.darkLabel18 = new DarkUI.Controls.DarkLabel();
            this.tbPositionY = new DarkUI.Controls.DarkTextBox();
            this.darkLabel19 = new DarkUI.Controls.DarkLabel();
            this.tbPositionX = new DarkUI.Controls.DarkTextBox();
            this.darkLabel20 = new DarkUI.Controls.DarkLabel();
            this.panelRendering = new WadTool.Controls.PanelRenderingStaticEditor();
            this.darkGroupBox4 = new DarkUI.Controls.DarkGroupBox();
            this.darkGroupBox3 = new DarkUI.Controls.DarkGroupBox();
            this.butClearCollisionBox = new DarkUI.Controls.DarkButton();
            this.darkGroupBox1 = new DarkUI.Controls.DarkGroupBox();
            this.butClearVisibilityBox = new DarkUI.Controls.DarkButton();
            this.darkGroupBox2 = new DarkUI.Controls.DarkGroupBox();
            this.darkLabel21 = new DarkUI.Controls.DarkLabel();
            this.darkSectionPanel1 = new DarkUI.Controls.DarkSectionPanel();
            this.darkGroupBox5 = new DarkUI.Controls.DarkGroupBox();
            this.darkLabel1 = new DarkUI.Controls.DarkLabel();
            this.butCancel = new DarkUI.Controls.DarkButton();
            this.darkSectionPanel2 = new DarkUI.Controls.DarkSectionPanel();
            ((System.ComponentModel.ISupportInitialize)(this.numRadius)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numIntensity)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numAmbient)).BeginInit();
            this.darkGroupBox4.SuspendLayout();
            this.darkGroupBox3.SuspendLayout();
            this.darkGroupBox1.SuspendLayout();
            this.darkGroupBox2.SuspendLayout();
            this.darkSectionPanel1.SuspendLayout();
            this.darkGroupBox5.SuspendLayout();
            this.darkSectionPanel2.SuspendLayout();
            this.SuspendLayout();
            // 
            // darkStatusStrip1
            // 
            this.darkStatusStrip1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.darkStatusStrip1.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.darkStatusStrip1.Location = new System.Drawing.Point(0, 610);
            this.darkStatusStrip1.Name = "darkStatusStrip1";
            this.darkStatusStrip1.Padding = new System.Windows.Forms.Padding(0, 5, 0, 3);
            this.darkStatusStrip1.Size = new System.Drawing.Size(1068, 24);
            this.darkStatusStrip1.TabIndex = 0;
            this.darkStatusStrip1.Text = "darkStatusStrip1";
            // 
            // cbVisibilityBox
            // 
            this.cbVisibilityBox.AutoSize = true;
            this.cbVisibilityBox.Location = new System.Drawing.Point(6, 11);
            this.cbVisibilityBox.Name = "cbVisibilityBox";
            this.cbVisibilityBox.Size = new System.Drawing.Size(91, 17);
            this.cbVisibilityBox.TabIndex = 50;
            this.cbVisibilityBox.Text = "Visibility Box";
            this.cbVisibilityBox.CheckedChanged += new System.EventHandler(this.cbVisibilityBox_CheckedChanged);
            // 
            // darkLabel2
            // 
            this.darkLabel2.AutoSize = true;
            this.darkLabel2.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.darkLabel2.Location = new System.Drawing.Point(3, 39);
            this.darkLabel2.Name = "darkLabel2";
            this.darkLabel2.Size = new System.Drawing.Size(38, 13);
            this.darkLabel2.TabIndex = 51;
            this.darkLabel2.Text = "X min:";
            // 
            // tbVisibilityBoxMinX
            // 
            this.tbVisibilityBoxMinX.Location = new System.Drawing.Point(6, 56);
            this.tbVisibilityBoxMinX.Name = "tbVisibilityBoxMinX";
            this.tbVisibilityBoxMinX.Size = new System.Drawing.Size(73, 22);
            this.tbVisibilityBoxMinX.TabIndex = 52;
            this.tbVisibilityBoxMinX.Validated += new System.EventHandler(this.tbVisibilityBoxMinX_Validated);
            // 
            // tbVisibilityBoxMinY
            // 
            this.tbVisibilityBoxMinY.Location = new System.Drawing.Point(85, 56);
            this.tbVisibilityBoxMinY.Name = "tbVisibilityBoxMinY";
            this.tbVisibilityBoxMinY.Size = new System.Drawing.Size(73, 22);
            this.tbVisibilityBoxMinY.TabIndex = 54;
            this.tbVisibilityBoxMinY.Validated += new System.EventHandler(this.tbVisibilityBoxMinY_Validated);
            // 
            // darkLabel3
            // 
            this.darkLabel3.AutoSize = true;
            this.darkLabel3.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.darkLabel3.Location = new System.Drawing.Point(82, 39);
            this.darkLabel3.Name = "darkLabel3";
            this.darkLabel3.Size = new System.Drawing.Size(37, 13);
            this.darkLabel3.TabIndex = 53;
            this.darkLabel3.Text = "Y min:";
            // 
            // tbVisibilityBoxMinZ
            // 
            this.tbVisibilityBoxMinZ.Location = new System.Drawing.Point(164, 56);
            this.tbVisibilityBoxMinZ.Name = "tbVisibilityBoxMinZ";
            this.tbVisibilityBoxMinZ.Size = new System.Drawing.Size(72, 22);
            this.tbVisibilityBoxMinZ.TabIndex = 56;
            this.tbVisibilityBoxMinZ.Validated += new System.EventHandler(this.tbVisibilityBoxMinZ_Validated);
            // 
            // darkLabel4
            // 
            this.darkLabel4.AutoSize = true;
            this.darkLabel4.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.darkLabel4.Location = new System.Drawing.Point(161, 39);
            this.darkLabel4.Name = "darkLabel4";
            this.darkLabel4.Size = new System.Drawing.Size(38, 13);
            this.darkLabel4.TabIndex = 55;
            this.darkLabel4.Text = "Z min:";
            // 
            // tbVisibilityBoxMaxZ
            // 
            this.tbVisibilityBoxMaxZ.Location = new System.Drawing.Point(164, 98);
            this.tbVisibilityBoxMaxZ.Name = "tbVisibilityBoxMaxZ";
            this.tbVisibilityBoxMaxZ.Size = new System.Drawing.Size(72, 22);
            this.tbVisibilityBoxMaxZ.TabIndex = 62;
            this.tbVisibilityBoxMaxZ.Validated += new System.EventHandler(this.tbVisibilityBoxMaxZ_Validated);
            // 
            // darkLabel5
            // 
            this.darkLabel5.AutoSize = true;
            this.darkLabel5.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.darkLabel5.Location = new System.Drawing.Point(161, 81);
            this.darkLabel5.Name = "darkLabel5";
            this.darkLabel5.Size = new System.Drawing.Size(39, 13);
            this.darkLabel5.TabIndex = 61;
            this.darkLabel5.Text = "Z max:";
            // 
            // tbVisibilityBoxMaxY
            // 
            this.tbVisibilityBoxMaxY.Location = new System.Drawing.Point(85, 98);
            this.tbVisibilityBoxMaxY.Name = "tbVisibilityBoxMaxY";
            this.tbVisibilityBoxMaxY.Size = new System.Drawing.Size(73, 22);
            this.tbVisibilityBoxMaxY.TabIndex = 60;
            this.tbVisibilityBoxMaxY.Validated += new System.EventHandler(this.tbVisibilityBoxMaxY_Validated);
            // 
            // darkLabel6
            // 
            this.darkLabel6.AutoSize = true;
            this.darkLabel6.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.darkLabel6.Location = new System.Drawing.Point(82, 81);
            this.darkLabel6.Name = "darkLabel6";
            this.darkLabel6.Size = new System.Drawing.Size(38, 13);
            this.darkLabel6.TabIndex = 59;
            this.darkLabel6.Text = "Y max:";
            // 
            // tbVisibilityBoxMaxX
            // 
            this.tbVisibilityBoxMaxX.Location = new System.Drawing.Point(6, 98);
            this.tbVisibilityBoxMaxX.Name = "tbVisibilityBoxMaxX";
            this.tbVisibilityBoxMaxX.Size = new System.Drawing.Size(73, 22);
            this.tbVisibilityBoxMaxX.TabIndex = 58;
            this.tbVisibilityBoxMaxX.Validated += new System.EventHandler(this.tbVisibilityBoxMaxX_Validated);
            // 
            // darkLabel7
            // 
            this.darkLabel7.AutoSize = true;
            this.darkLabel7.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.darkLabel7.Location = new System.Drawing.Point(3, 81);
            this.darkLabel7.Name = "darkLabel7";
            this.darkLabel7.Size = new System.Drawing.Size(39, 13);
            this.darkLabel7.TabIndex = 57;
            this.darkLabel7.Text = "X max:";
            // 
            // tbCollisionBoxMaxZ
            // 
            this.tbCollisionBoxMaxZ.Location = new System.Drawing.Point(164, 98);
            this.tbCollisionBoxMaxZ.Name = "tbCollisionBoxMaxZ";
            this.tbCollisionBoxMaxZ.Size = new System.Drawing.Size(72, 22);
            this.tbCollisionBoxMaxZ.TabIndex = 76;
            this.tbCollisionBoxMaxZ.Validated += new System.EventHandler(this.tbCollisionBoxMaxZ_Validated);
            // 
            // darkLabel8
            // 
            this.darkLabel8.AutoSize = true;
            this.darkLabel8.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.darkLabel8.Location = new System.Drawing.Point(161, 81);
            this.darkLabel8.Name = "darkLabel8";
            this.darkLabel8.Size = new System.Drawing.Size(39, 13);
            this.darkLabel8.TabIndex = 75;
            this.darkLabel8.Text = "Z max:";
            // 
            // tbCollisionBoxMaxY
            // 
            this.tbCollisionBoxMaxY.Location = new System.Drawing.Point(85, 98);
            this.tbCollisionBoxMaxY.Name = "tbCollisionBoxMaxY";
            this.tbCollisionBoxMaxY.Size = new System.Drawing.Size(73, 22);
            this.tbCollisionBoxMaxY.TabIndex = 74;
            this.tbCollisionBoxMaxY.TextChanged += new System.EventHandler(this.tbCollisionBoxMaxY_TextChanged);
            // 
            // darkLabel9
            // 
            this.darkLabel9.AutoSize = true;
            this.darkLabel9.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.darkLabel9.Location = new System.Drawing.Point(82, 81);
            this.darkLabel9.Name = "darkLabel9";
            this.darkLabel9.Size = new System.Drawing.Size(38, 13);
            this.darkLabel9.TabIndex = 73;
            this.darkLabel9.Text = "Y max:";
            // 
            // tbCollisionBoxMaxX
            // 
            this.tbCollisionBoxMaxX.Location = new System.Drawing.Point(6, 98);
            this.tbCollisionBoxMaxX.Name = "tbCollisionBoxMaxX";
            this.tbCollisionBoxMaxX.Size = new System.Drawing.Size(73, 22);
            this.tbCollisionBoxMaxX.TabIndex = 72;
            this.tbCollisionBoxMaxX.Validated += new System.EventHandler(this.tbCollisionBoxMaxX_Validated);
            // 
            // darkLabel10
            // 
            this.darkLabel10.AutoSize = true;
            this.darkLabel10.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.darkLabel10.Location = new System.Drawing.Point(3, 81);
            this.darkLabel10.Name = "darkLabel10";
            this.darkLabel10.Size = new System.Drawing.Size(39, 13);
            this.darkLabel10.TabIndex = 71;
            this.darkLabel10.Text = "X max:";
            // 
            // tbCollisionBoxMinZ
            // 
            this.tbCollisionBoxMinZ.Location = new System.Drawing.Point(164, 56);
            this.tbCollisionBoxMinZ.Name = "tbCollisionBoxMinZ";
            this.tbCollisionBoxMinZ.Size = new System.Drawing.Size(72, 22);
            this.tbCollisionBoxMinZ.TabIndex = 70;
            this.tbCollisionBoxMinZ.Validated += new System.EventHandler(this.tbCollisionBoxMinZ_Validated);
            // 
            // darkLabel11
            // 
            this.darkLabel11.AutoSize = true;
            this.darkLabel11.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.darkLabel11.Location = new System.Drawing.Point(161, 39);
            this.darkLabel11.Name = "darkLabel11";
            this.darkLabel11.Size = new System.Drawing.Size(38, 13);
            this.darkLabel11.TabIndex = 69;
            this.darkLabel11.Text = "Z min:";
            // 
            // tbCollisionBoxMinY
            // 
            this.tbCollisionBoxMinY.Location = new System.Drawing.Point(85, 56);
            this.tbCollisionBoxMinY.Name = "tbCollisionBoxMinY";
            this.tbCollisionBoxMinY.Size = new System.Drawing.Size(73, 22);
            this.tbCollisionBoxMinY.TabIndex = 68;
            this.tbCollisionBoxMinY.Validated += new System.EventHandler(this.tbCollisionBoxMinY_Validated);
            // 
            // darkLabel12
            // 
            this.darkLabel12.AutoSize = true;
            this.darkLabel12.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.darkLabel12.Location = new System.Drawing.Point(82, 39);
            this.darkLabel12.Name = "darkLabel12";
            this.darkLabel12.Size = new System.Drawing.Size(37, 13);
            this.darkLabel12.TabIndex = 67;
            this.darkLabel12.Text = "Y min:";
            // 
            // tbCollisionBoxMinX
            // 
            this.tbCollisionBoxMinX.Location = new System.Drawing.Point(6, 56);
            this.tbCollisionBoxMinX.Name = "tbCollisionBoxMinX";
            this.tbCollisionBoxMinX.Size = new System.Drawing.Size(73, 22);
            this.tbCollisionBoxMinX.TabIndex = 66;
            this.tbCollisionBoxMinX.Validated += new System.EventHandler(this.tbCollisionBoxMinX_Validated);
            // 
            // darkLabel13
            // 
            this.darkLabel13.AutoSize = true;
            this.darkLabel13.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.darkLabel13.Location = new System.Drawing.Point(3, 39);
            this.darkLabel13.Name = "darkLabel13";
            this.darkLabel13.Size = new System.Drawing.Size(38, 13);
            this.darkLabel13.TabIndex = 65;
            this.darkLabel13.Text = "X min:";
            // 
            // cbCollisionBox
            // 
            this.cbCollisionBox.AutoSize = true;
            this.cbCollisionBox.Location = new System.Drawing.Point(6, 11);
            this.cbCollisionBox.Name = "cbCollisionBox";
            this.cbCollisionBox.Size = new System.Drawing.Size(93, 17);
            this.cbCollisionBox.TabIndex = 64;
            this.cbCollisionBox.Text = "Collision Box";
            this.cbCollisionBox.CheckedChanged += new System.EventHandler(this.cbCollisionBox_CheckedChanged);
            // 
            // cbDrawGrid
            // 
            this.cbDrawGrid.AutoSize = true;
            this.cbDrawGrid.Checked = true;
            this.cbDrawGrid.CheckState = System.Windows.Forms.CheckState.Checked;
            this.cbDrawGrid.Location = new System.Drawing.Point(121, 27);
            this.cbDrawGrid.Name = "cbDrawGrid";
            this.cbDrawGrid.Size = new System.Drawing.Size(48, 17);
            this.cbDrawGrid.TabIndex = 78;
            this.cbDrawGrid.Text = "Grid";
            this.cbDrawGrid.CheckedChanged += new System.EventHandler(this.cbDrawGrid_CheckedChanged);
            // 
            // cbDrawGizmo
            // 
            this.cbDrawGizmo.AutoSize = true;
            this.cbDrawGizmo.Checked = true;
            this.cbDrawGizmo.CheckState = System.Windows.Forms.CheckState.Checked;
            this.cbDrawGizmo.Location = new System.Drawing.Point(64, 27);
            this.cbDrawGizmo.Name = "cbDrawGizmo";
            this.cbDrawGizmo.Size = new System.Drawing.Size(58, 17);
            this.cbDrawGizmo.TabIndex = 79;
            this.cbDrawGizmo.Text = "Gizmo";
            this.cbDrawGizmo.CheckedChanged += new System.EventHandler(this.cbDrawGizmo_CheckedChanged);
            // 
            // butCalculateCollisionBox
            // 
            this.butCalculateCollisionBox.Location = new System.Drawing.Point(163, 7);
            this.butCalculateCollisionBox.Name = "butCalculateCollisionBox";
            this.butCalculateCollisionBox.Size = new System.Drawing.Size(73, 23);
            this.butCalculateCollisionBox.TabIndex = 77;
            this.butCalculateCollisionBox.Text = "Calculate";
            this.butCalculateCollisionBox.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.butCalculateCollisionBox.Click += new System.EventHandler(this.butCalculateCollisionBox_Click);
            // 
            // butCalculateVisibilityBox
            // 
            this.butCalculateVisibilityBox.Location = new System.Drawing.Point(163, 7);
            this.butCalculateVisibilityBox.Name = "butCalculateVisibilityBox";
            this.butCalculateVisibilityBox.Size = new System.Drawing.Size(73, 23);
            this.butCalculateVisibilityBox.TabIndex = 63;
            this.butCalculateVisibilityBox.Text = "Calculate";
            this.butCalculateVisibilityBox.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.butCalculateVisibilityBox.Click += new System.EventHandler(this.butCalculateVisibilityBox_Click);
            // 
            // butSaveChanges
            // 
            this.butSaveChanges.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.butSaveChanges.Location = new System.Drawing.Point(896, 583);
            this.butSaveChanges.Name = "butSaveChanges";
            this.butSaveChanges.Size = new System.Drawing.Size(80, 23);
            this.butSaveChanges.TabIndex = 46;
            this.butSaveChanges.Text = "OK";
            this.butSaveChanges.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.butSaveChanges.Click += new System.EventHandler(this.butSaveChanges_Click);
            // 
            // butResetTranslation
            // 
            this.butResetTranslation.Location = new System.Drawing.Point(6, 64);
            this.butResetTranslation.Name = "butResetTranslation";
            this.butResetTranslation.Size = new System.Drawing.Size(73, 23);
            this.butResetTranslation.TabIndex = 80;
            this.butResetTranslation.Text = "Translation";
            this.butResetTranslation.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.butResetTranslation.Click += new System.EventHandler(this.butResetTranslation_Click);
            // 
            // butResetRotation
            // 
            this.butResetRotation.Location = new System.Drawing.Point(85, 64);
            this.butResetRotation.Name = "butResetRotation";
            this.butResetRotation.Size = new System.Drawing.Size(73, 23);
            this.butResetRotation.TabIndex = 81;
            this.butResetRotation.Text = "Rotation";
            this.butResetRotation.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.butResetRotation.Click += new System.EventHandler(this.butResetRotation_Click);
            // 
            // butResetScale
            // 
            this.butResetScale.Location = new System.Drawing.Point(164, 64);
            this.butResetScale.Name = "butResetScale";
            this.butResetScale.Size = new System.Drawing.Size(72, 23);
            this.butResetScale.TabIndex = 82;
            this.butResetScale.Text = "Scale";
            this.butResetScale.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.butResetScale.Click += new System.EventHandler(this.butResetScale_Click);
            // 
            // butAddLight
            // 
            this.butAddLight.Image = global::WadTool.Properties.Resources.plus_math_16;
            this.butAddLight.Location = new System.Drawing.Point(182, 7);
            this.butAddLight.Name = "butAddLight";
            this.butAddLight.Size = new System.Drawing.Size(24, 23);
            this.butAddLight.TabIndex = 85;
            this.butAddLight.Click += new System.EventHandler(this.butAddLight_Click);
            // 
            // butDeleteLight
            // 
            this.butDeleteLight.Image = global::WadTool.Properties.Resources.trash_16;
            this.butDeleteLight.Location = new System.Drawing.Point(212, 7);
            this.butDeleteLight.Name = "butDeleteLight";
            this.butDeleteLight.Size = new System.Drawing.Size(24, 23);
            this.butDeleteLight.TabIndex = 86;
            this.butDeleteLight.Click += new System.EventHandler(this.butDeleteLight_Click);
            // 
            // numRadius
            // 
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
            this.numRadius.Location = new System.Drawing.Point(163, 64);
            this.numRadius.Maximum = new decimal(new int[] {
            256,
            0,
            0,
            0});
            this.numRadius.MousewheelSingleIncrement = true;
            this.numRadius.Name = "numRadius";
            this.numRadius.Size = new System.Drawing.Size(73, 22);
            this.numRadius.TabIndex = 90;
            this.numRadius.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.numRadius.ValueChanged += new System.EventHandler(this.numInnerRange_ValueChanged);
            // 
            // numIntensity
            // 
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
            this.numIntensity.Location = new System.Drawing.Point(163, 36);
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
            this.numIntensity.Size = new System.Drawing.Size(73, 22);
            this.numIntensity.TabIndex = 89;
            this.numIntensity.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.numIntensity.ValueChanged += new System.EventHandler(this.numIntensity_ValueChanged);
            // 
            // darkLabel15
            // 
            this.darkLabel15.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.darkLabel15.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.darkLabel15.Location = new System.Drawing.Point(99, 33);
            this.darkLabel15.Name = "darkLabel15";
            this.darkLabel15.Size = new System.Drawing.Size(59, 22);
            this.darkLabel15.TabIndex = 93;
            this.darkLabel15.Text = "Intensity:";
            this.darkLabel15.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // darkLabel16
            // 
            this.darkLabel16.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.darkLabel16.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.darkLabel16.Location = new System.Drawing.Point(99, 61);
            this.darkLabel16.Name = "darkLabel16";
            this.darkLabel16.Size = new System.Drawing.Size(51, 22);
            this.darkLabel16.TabIndex = 92;
            this.darkLabel16.Text = "Radius:";
            this.darkLabel16.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // numAmbient
            // 
            this.numAmbient.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(69)))), ((int)(((byte)(73)))), ((int)(((byte)(74)))));
            this.numAmbient.ForeColor = System.Drawing.Color.Gainsboro;
            this.numAmbient.IncrementAlternate = new decimal(new int[] {
            8,
            0,
            0,
            0});
            this.numAmbient.Location = new System.Drawing.Point(163, 92);
            this.numAmbient.Maximum = new decimal(new int[] {
            255,
            0,
            0,
            0});
            this.numAmbient.MousewheelSingleIncrement = true;
            this.numAmbient.Name = "numAmbient";
            this.numAmbient.Size = new System.Drawing.Size(73, 22);
            this.numAmbient.TabIndex = 94;
            this.numAmbient.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.numAmbient.ValueChanged += new System.EventHandler(this.numAmbient_ValueChanged);
            // 
            // darkLabel17
            // 
            this.darkLabel17.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.darkLabel17.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.darkLabel17.Location = new System.Drawing.Point(99, 89);
            this.darkLabel17.Name = "darkLabel17";
            this.darkLabel17.Size = new System.Drawing.Size(63, 22);
            this.darkLabel17.TabIndex = 95;
            this.darkLabel17.Text = "Ambient:";
            this.darkLabel17.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // cbDrawLights
            // 
            this.cbDrawLights.AutoSize = true;
            this.cbDrawLights.Checked = true;
            this.cbDrawLights.CheckState = System.Windows.Forms.CheckState.Checked;
            this.cbDrawLights.Location = new System.Drawing.Point(6, 27);
            this.cbDrawLights.Name = "cbDrawLights";
            this.cbDrawLights.Size = new System.Drawing.Size(57, 17);
            this.cbDrawLights.TabIndex = 96;
            this.cbDrawLights.Text = "Lights";
            this.cbDrawLights.CheckedChanged += new System.EventHandler(this.cbDrawLights_CheckedChanged);
            // 
            // lstLights
            // 
            this.lstLights.Location = new System.Drawing.Point(6, 36);
            this.lstLights.MaxDragChange = 20;
            this.lstLights.Name = "lstLights";
            this.lstLights.Size = new System.Drawing.Size(87, 107);
            this.lstLights.TabIndex = 97;
            this.lstLights.Text = "darkTreeView1";
            this.lstLights.Click += new System.EventHandler(this.lstLights_Click);
            // 
            // butImportMeshFromFile
            // 
            this.butImportMeshFromFile.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.butImportMeshFromFile.Location = new System.Drawing.Point(810, 583);
            this.butImportMeshFromFile.Name = "butImportMeshFromFile";
            this.butImportMeshFromFile.Size = new System.Drawing.Size(81, 23);
            this.butImportMeshFromFile.TabIndex = 98;
            this.butImportMeshFromFile.Text = "Import mesh";
            this.butImportMeshFromFile.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.butImportMeshFromFile.Click += new System.EventHandler(this.butImportMeshFromFile_Click);
            // 
            // cbDrawNormals
            // 
            this.cbDrawNormals.AutoSize = true;
            this.cbDrawNormals.Location = new System.Drawing.Point(167, 27);
            this.cbDrawNormals.Name = "cbDrawNormals";
            this.cbDrawNormals.Size = new System.Drawing.Size(68, 17);
            this.cbDrawNormals.TabIndex = 99;
            this.cbDrawNormals.Text = "Normals";
            this.cbDrawNormals.CheckedChanged += new System.EventHandler(this.cbDrawNormals_CheckedChanged);
            // 
            // butRecalcNormals
            // 
            this.butRecalcNormals.Location = new System.Drawing.Point(99, 120);
            this.butRecalcNormals.Name = "butRecalcNormals";
            this.butRecalcNormals.Size = new System.Drawing.Size(137, 23);
            this.butRecalcNormals.TabIndex = 101;
            this.butRecalcNormals.Text = "Recalculate normals";
            this.butRecalcNormals.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.butRecalcNormals.Click += new System.EventHandler(this.butRecalcNormals_Click);
            // 
            // comboLightType
            // 
            this.comboLightType.FormattingEnabled = true;
            this.comboLightType.Items.AddRange(new object[] {
            "Dynamic Lighting",
            "Static Lighting"});
            this.comboLightType.Location = new System.Drawing.Point(6, 7);
            this.comboLightType.Name = "comboLightType";
            this.comboLightType.Size = new System.Drawing.Size(170, 23);
            this.comboLightType.TabIndex = 102;
            this.comboLightType.SelectedIndexChanged += new System.EventHandler(this.comboLightType_SelectedIndexChanged);
            // 
            // tbPositionZ
            // 
            this.tbPositionZ.Location = new System.Drawing.Point(164, 21);
            this.tbPositionZ.Name = "tbPositionZ";
            this.tbPositionZ.Size = new System.Drawing.Size(72, 22);
            this.tbPositionZ.TabIndex = 108;
            this.tbPositionZ.Validated += new System.EventHandler(this.tbPositionZ_Validated);
            // 
            // darkLabel18
            // 
            this.darkLabel18.AutoSize = true;
            this.darkLabel18.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.darkLabel18.Location = new System.Drawing.Point(161, 4);
            this.darkLabel18.Name = "darkLabel18";
            this.darkLabel18.Size = new System.Drawing.Size(37, 13);
            this.darkLabel18.TabIndex = 107;
            this.darkLabel18.Text = "Pos Z:";
            // 
            // tbPositionY
            // 
            this.tbPositionY.Location = new System.Drawing.Point(85, 21);
            this.tbPositionY.Name = "tbPositionY";
            this.tbPositionY.Size = new System.Drawing.Size(73, 22);
            this.tbPositionY.TabIndex = 106;
            this.tbPositionY.Validated += new System.EventHandler(this.tbPositionY_Validated);
            // 
            // darkLabel19
            // 
            this.darkLabel19.AutoSize = true;
            this.darkLabel19.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.darkLabel19.Location = new System.Drawing.Point(82, 4);
            this.darkLabel19.Name = "darkLabel19";
            this.darkLabel19.Size = new System.Drawing.Size(36, 13);
            this.darkLabel19.TabIndex = 105;
            this.darkLabel19.Text = "Pos Y:";
            // 
            // tbPositionX
            // 
            this.tbPositionX.Location = new System.Drawing.Point(6, 21);
            this.tbPositionX.Name = "tbPositionX";
            this.tbPositionX.Size = new System.Drawing.Size(73, 22);
            this.tbPositionX.TabIndex = 104;
            this.tbPositionX.Validated += new System.EventHandler(this.tbPositionX_Validated);
            // 
            // darkLabel20
            // 
            this.darkLabel20.AutoSize = true;
            this.darkLabel20.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.darkLabel20.Location = new System.Drawing.Point(3, 4);
            this.darkLabel20.Name = "darkLabel20";
            this.darkLabel20.Size = new System.Drawing.Size(37, 13);
            this.darkLabel20.TabIndex = 103;
            this.darkLabel20.Text = "Pos X:";
            // 
            // panelRendering
            // 
            this.panelRendering.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelRendering.Location = new System.Drawing.Point(1, 1);
            this.panelRendering.Name = "panelRendering";
            this.panelRendering.Size = new System.Drawing.Size(797, 599);
            this.panelRendering.TabIndex = 109;
            // 
            // darkGroupBox4
            // 
            this.darkGroupBox4.Controls.Add(this.butRecalcNormals);
            this.darkGroupBox4.Controls.Add(this.lstLights);
            this.darkGroupBox4.Controls.Add(this.comboLightType);
            this.darkGroupBox4.Controls.Add(this.butAddLight);
            this.darkGroupBox4.Controls.Add(this.butDeleteLight);
            this.darkGroupBox4.Controls.Add(this.numIntensity);
            this.darkGroupBox4.Controls.Add(this.numAmbient);
            this.darkGroupBox4.Controls.Add(this.darkLabel15);
            this.darkGroupBox4.Controls.Add(this.darkLabel17);
            this.darkGroupBox4.Controls.Add(this.darkLabel16);
            this.darkGroupBox4.Controls.Add(this.numRadius);
            this.darkGroupBox4.Location = new System.Drawing.Point(5, 361);
            this.darkGroupBox4.Name = "darkGroupBox4";
            this.darkGroupBox4.Size = new System.Drawing.Size(242, 150);
            this.darkGroupBox4.TabIndex = 111;
            this.darkGroupBox4.TabStop = false;
            // 
            // darkGroupBox3
            // 
            this.darkGroupBox3.Controls.Add(this.butClearCollisionBox);
            this.darkGroupBox3.Controls.Add(this.cbCollisionBox);
            this.darkGroupBox3.Controls.Add(this.darkLabel13);
            this.darkGroupBox3.Controls.Add(this.darkLabel10);
            this.darkGroupBox3.Controls.Add(this.tbCollisionBoxMaxX);
            this.darkGroupBox3.Controls.Add(this.tbCollisionBoxMinZ);
            this.darkGroupBox3.Controls.Add(this.darkLabel9);
            this.darkGroupBox3.Controls.Add(this.darkLabel11);
            this.darkGroupBox3.Controls.Add(this.tbCollisionBoxMaxY);
            this.darkGroupBox3.Controls.Add(this.tbCollisionBoxMinY);
            this.darkGroupBox3.Controls.Add(this.darkLabel8);
            this.darkGroupBox3.Controls.Add(this.darkLabel12);
            this.darkGroupBox3.Controls.Add(this.tbCollisionBoxMaxZ);
            this.darkGroupBox3.Controls.Add(this.tbCollisionBoxMinX);
            this.darkGroupBox3.Controls.Add(this.butCalculateCollisionBox);
            this.darkGroupBox3.Location = new System.Drawing.Point(5, 231);
            this.darkGroupBox3.Name = "darkGroupBox3";
            this.darkGroupBox3.Size = new System.Drawing.Size(242, 126);
            this.darkGroupBox3.TabIndex = 110;
            this.darkGroupBox3.TabStop = false;
            // 
            // butClearCollisionBox
            // 
            this.butClearCollisionBox.Location = new System.Drawing.Point(102, 7);
            this.butClearCollisionBox.Name = "butClearCollisionBox";
            this.butClearCollisionBox.Size = new System.Drawing.Size(56, 23);
            this.butClearCollisionBox.TabIndex = 78;
            this.butClearCollisionBox.Text = "Clear";
            this.butClearCollisionBox.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.butClearCollisionBox.Click += new System.EventHandler(this.butClearCollisionBox_Click);
            // 
            // darkGroupBox1
            // 
            this.darkGroupBox1.Controls.Add(this.butClearVisibilityBox);
            this.darkGroupBox1.Controls.Add(this.tbVisibilityBoxMaxX);
            this.darkGroupBox1.Controls.Add(this.butCalculateVisibilityBox);
            this.darkGroupBox1.Controls.Add(this.tbVisibilityBoxMaxZ);
            this.darkGroupBox1.Controls.Add(this.darkLabel5);
            this.darkGroupBox1.Controls.Add(this.cbVisibilityBox);
            this.darkGroupBox1.Controls.Add(this.tbVisibilityBoxMaxY);
            this.darkGroupBox1.Controls.Add(this.darkLabel6);
            this.darkGroupBox1.Controls.Add(this.darkLabel7);
            this.darkGroupBox1.Controls.Add(this.tbVisibilityBoxMinZ);
            this.darkGroupBox1.Controls.Add(this.darkLabel4);
            this.darkGroupBox1.Controls.Add(this.darkLabel2);
            this.darkGroupBox1.Controls.Add(this.tbVisibilityBoxMinY);
            this.darkGroupBox1.Controls.Add(this.darkLabel3);
            this.darkGroupBox1.Controls.Add(this.tbVisibilityBoxMinX);
            this.darkGroupBox1.Location = new System.Drawing.Point(5, 101);
            this.darkGroupBox1.Name = "darkGroupBox1";
            this.darkGroupBox1.Size = new System.Drawing.Size(242, 126);
            this.darkGroupBox1.TabIndex = 109;
            this.darkGroupBox1.TabStop = false;
            // 
            // butClearVisibilityBox
            // 
            this.butClearVisibilityBox.Location = new System.Drawing.Point(101, 7);
            this.butClearVisibilityBox.Name = "butClearVisibilityBox";
            this.butClearVisibilityBox.Size = new System.Drawing.Size(56, 23);
            this.butClearVisibilityBox.TabIndex = 79;
            this.butClearVisibilityBox.Text = "Clear";
            this.butClearVisibilityBox.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.butClearVisibilityBox.Click += new System.EventHandler(this.butClearVisibilityBox_Click);
            // 
            // darkGroupBox2
            // 
            this.darkGroupBox2.Controls.Add(this.darkLabel21);
            this.darkGroupBox2.Controls.Add(this.tbPositionX);
            this.darkGroupBox2.Controls.Add(this.darkLabel20);
            this.darkGroupBox2.Controls.Add(this.darkLabel19);
            this.darkGroupBox2.Controls.Add(this.tbPositionY);
            this.darkGroupBox2.Controls.Add(this.darkLabel18);
            this.darkGroupBox2.Controls.Add(this.tbPositionZ);
            this.darkGroupBox2.Controls.Add(this.butResetTranslation);
            this.darkGroupBox2.Controls.Add(this.butResetRotation);
            this.darkGroupBox2.Controls.Add(this.butResetScale);
            this.darkGroupBox2.Location = new System.Drawing.Point(5, 4);
            this.darkGroupBox2.Name = "darkGroupBox2";
            this.darkGroupBox2.Size = new System.Drawing.Size(242, 93);
            this.darkGroupBox2.TabIndex = 0;
            this.darkGroupBox2.TabStop = false;
            // 
            // darkLabel21
            // 
            this.darkLabel21.AutoSize = true;
            this.darkLabel21.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.darkLabel21.Location = new System.Drawing.Point(3, 46);
            this.darkLabel21.Name = "darkLabel21";
            this.darkLabel21.Size = new System.Drawing.Size(38, 13);
            this.darkLabel21.TabIndex = 109;
            this.darkLabel21.Text = "Reset:";
            // 
            // darkSectionPanel1
            // 
            this.darkSectionPanel1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.darkSectionPanel1.Controls.Add(this.darkGroupBox5);
            this.darkSectionPanel1.Controls.Add(this.darkGroupBox2);
            this.darkSectionPanel1.Controls.Add(this.darkGroupBox4);
            this.darkSectionPanel1.Controls.Add(this.darkGroupBox1);
            this.darkSectionPanel1.Controls.Add(this.darkGroupBox3);
            this.darkSectionPanel1.Location = new System.Drawing.Point(810, 5);
            this.darkSectionPanel1.MinimumSize = new System.Drawing.Size(252, 569);
            this.darkSectionPanel1.Name = "darkSectionPanel1";
            this.darkSectionPanel1.SectionHeader = null;
            this.darkSectionPanel1.Size = new System.Drawing.Size(252, 569);
            this.darkSectionPanel1.TabIndex = 110;
            // 
            // darkGroupBox5
            // 
            this.darkGroupBox5.Controls.Add(this.cbDrawNormals);
            this.darkGroupBox5.Controls.Add(this.darkLabel1);
            this.darkGroupBox5.Controls.Add(this.cbDrawGrid);
            this.darkGroupBox5.Controls.Add(this.cbDrawGizmo);
            this.darkGroupBox5.Controls.Add(this.cbDrawLights);
            this.darkGroupBox5.Location = new System.Drawing.Point(5, 515);
            this.darkGroupBox5.Name = "darkGroupBox5";
            this.darkGroupBox5.Size = new System.Drawing.Size(242, 48);
            this.darkGroupBox5.TabIndex = 112;
            this.darkGroupBox5.TabStop = false;
            // 
            // darkLabel1
            // 
            this.darkLabel1.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.darkLabel1.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.darkLabel1.Location = new System.Drawing.Point(3, 4);
            this.darkLabel1.Name = "darkLabel1";
            this.darkLabel1.Size = new System.Drawing.Size(38, 22);
            this.darkLabel1.TabIndex = 94;
            this.darkLabel1.Text = "Draw:";
            this.darkLabel1.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // butCancel
            // 
            this.butCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.butCancel.Location = new System.Drawing.Point(981, 583);
            this.butCancel.Name = "butCancel";
            this.butCancel.Size = new System.Drawing.Size(81, 23);
            this.butCancel.TabIndex = 111;
            this.butCancel.Text = "Cancel";
            this.butCancel.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.butCancel.Click += new System.EventHandler(this.butCancel_Click);
            // 
            // darkSectionPanel2
            // 
            this.darkSectionPanel2.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.darkSectionPanel2.Controls.Add(this.panelRendering);
            this.darkSectionPanel2.Location = new System.Drawing.Point(5, 5);
            this.darkSectionPanel2.Name = "darkSectionPanel2";
            this.darkSectionPanel2.SectionHeader = null;
            this.darkSectionPanel2.Size = new System.Drawing.Size(799, 601);
            this.darkSectionPanel2.TabIndex = 112;
            // 
            // FormStaticEditor
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1068, 634);
            this.Controls.Add(this.darkSectionPanel2);
            this.Controls.Add(this.butImportMeshFromFile);
            this.Controls.Add(this.butCancel);
            this.Controls.Add(this.darkSectionPanel1);
            this.Controls.Add(this.darkStatusStrip1);
            this.Controls.Add(this.butSaveChanges);
            this.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(1084, 672);
            this.Name = "FormStaticEditor";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Static editor";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.FormStaticEditor_FormClosing);
            this.Load += new System.EventHandler(this.FormStaticEditor_Load);
            ((System.ComponentModel.ISupportInitialize)(this.numRadius)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numIntensity)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numAmbient)).EndInit();
            this.darkGroupBox4.ResumeLayout(false);
            this.darkGroupBox3.ResumeLayout(false);
            this.darkGroupBox3.PerformLayout();
            this.darkGroupBox1.ResumeLayout(false);
            this.darkGroupBox1.PerformLayout();
            this.darkGroupBox2.ResumeLayout(false);
            this.darkGroupBox2.PerformLayout();
            this.darkSectionPanel1.ResumeLayout(false);
            this.darkGroupBox5.ResumeLayout(false);
            this.darkGroupBox5.PerformLayout();
            this.darkSectionPanel2.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private DarkUI.Controls.DarkStatusStrip darkStatusStrip1;
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
        private DarkUI.Controls.DarkCheckBox cbDrawNormals;
        private DarkUI.Controls.DarkButton butRecalcNormals;
        private DarkUI.Controls.DarkComboBox comboLightType;
        private DarkUI.Controls.DarkTextBox tbPositionZ;
        private DarkUI.Controls.DarkLabel darkLabel18;
        private DarkUI.Controls.DarkTextBox tbPositionY;
        private DarkUI.Controls.DarkLabel darkLabel19;
        private DarkUI.Controls.DarkTextBox tbPositionX;
        private DarkUI.Controls.DarkLabel darkLabel20;
        private Controls.PanelRenderingStaticEditor panelRendering;
        private DarkUI.Controls.DarkGroupBox darkGroupBox3;
        private DarkUI.Controls.DarkGroupBox darkGroupBox1;
        private DarkUI.Controls.DarkGroupBox darkGroupBox2;
        private DarkUI.Controls.DarkLabel darkLabel21;
        private DarkUI.Controls.DarkSectionPanel darkSectionPanel1;
        private DarkUI.Controls.DarkGroupBox darkGroupBox4;
        private DarkUI.Controls.DarkButton butCancel;
        private DarkUI.Controls.DarkGroupBox darkGroupBox5;
        private DarkUI.Controls.DarkLabel darkLabel1;
        private DarkUI.Controls.DarkButton butClearCollisionBox;
        private DarkUI.Controls.DarkButton butClearVisibilityBox;
        private DarkUI.Controls.DarkSectionPanel darkSectionPanel2;
    }
}