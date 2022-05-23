namespace TombEditor.ToolWindows
{
    partial class Lighting
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
            this.cbLightIsDynamicallyUsed = new DarkUI.Controls.DarkCheckBox();
            this.cbLightIsStaticallyUsed = new DarkUI.Controls.DarkCheckBox();
            this.cbLightIsObstructedByRoomGeometry = new DarkUI.Controls.DarkCheckBox();
            this.cbLightEnabled = new DarkUI.Controls.DarkCheckBox();
            this.panelLightColor = new DarkUI.Controls.DarkPanel();
            this.darkLabel12 = new DarkUI.Controls.DarkLabel();
            this.darkLabel13 = new DarkUI.Controls.DarkLabel();
            this.darkLabel11 = new DarkUI.Controls.DarkLabel();
            this.darkLabel9 = new DarkUI.Controls.DarkLabel();
            this.darkLabel10 = new DarkUI.Controls.DarkLabel();
            this.darkLabel8 = new DarkUI.Controls.DarkLabel();
            this.darkLabel7 = new DarkUI.Controls.DarkLabel();
            this.butAddLight = new DarkUI.Controls.DarkButton();
            this.numIntensity = new DarkUI.Controls.DarkNumericUpDown();
            this.numInnerRange = new DarkUI.Controls.DarkNumericUpDown();
            this.numOuterRange = new DarkUI.Controls.DarkNumericUpDown();
            this.numInnerAngle = new DarkUI.Controls.DarkNumericUpDown();
            this.numOuterAngle = new DarkUI.Controls.DarkNumericUpDown();
            this.numDirectionX = new DarkUI.Controls.DarkNumericUpDown();
            this.numDirectionY = new DarkUI.Controls.DarkNumericUpDown();
            this.toolTip = new System.Windows.Forms.ToolTip(this.components);
            this.cbLightIsUsedForImportedGeometry = new DarkUI.Controls.DarkCheckBox();
            this.cmbLightQuality = new DarkUI.Controls.DarkComboBox();
            this.cmbLightTypes = new DarkUI.Controls.DarkComboBox();
            this.cbCastShadow = new DarkUI.Controls.DarkCheckBox();
            this.darkLabel6 = new DarkUI.Controls.DarkLabel();
            ((System.ComponentModel.ISupportInitialize)(this.numIntensity)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numInnerRange)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numOuterRange)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numInnerAngle)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numOuterAngle)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numDirectionX)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numDirectionY)).BeginInit();
            this.SuspendLayout();
            // 
            // cbLightIsDynamicallyUsed
            // 
            this.cbLightIsDynamicallyUsed.Enabled = false;
            this.cbLightIsDynamicallyUsed.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cbLightIsDynamicallyUsed.Location = new System.Drawing.Point(79, 100);
            this.cbLightIsDynamicallyUsed.Name = "cbLightIsDynamicallyUsed";
            this.cbLightIsDynamicallyUsed.Size = new System.Drawing.Size(70, 22);
            this.cbLightIsDynamicallyUsed.TabIndex = 18;
            this.cbLightIsDynamicallyUsed.Text = "Dynamic";
            this.toolTip.SetToolTip(this.cbLightIsDynamicallyUsed, "Use light for moveables ingame");
            this.cbLightIsDynamicallyUsed.CheckedChanged += new System.EventHandler(this.cbLightIsDynamicallyUsed_CheckedChanged);
            // 
            // cbLightIsStaticallyUsed
            // 
            this.cbLightIsStaticallyUsed.Enabled = false;
            this.cbLightIsStaticallyUsed.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cbLightIsStaticallyUsed.Location = new System.Drawing.Point(79, 81);
            this.cbLightIsStaticallyUsed.Name = "cbLightIsStaticallyUsed";
            this.cbLightIsStaticallyUsed.Size = new System.Drawing.Size(70, 22);
            this.cbLightIsStaticallyUsed.TabIndex = 17;
            this.cbLightIsStaticallyUsed.Text = "Static";
            this.toolTip.SetToolTip(this.cbLightIsStaticallyUsed, "Use light for room geometry lighting");
            this.cbLightIsStaticallyUsed.CheckedChanged += new System.EventHandler(this.cbLightIsStaticallyUsed_CheckedChanged);
            // 
            // cbLightIsObstructedByRoomGeometry
            // 
            this.cbLightIsObstructedByRoomGeometry.Enabled = false;
            this.cbLightIsObstructedByRoomGeometry.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cbLightIsObstructedByRoomGeometry.Location = new System.Drawing.Point(3, 100);
            this.cbLightIsObstructedByRoomGeometry.Name = "cbLightIsObstructedByRoomGeometry";
            this.cbLightIsObstructedByRoomGeometry.Size = new System.Drawing.Size(70, 22);
            this.cbLightIsObstructedByRoomGeometry.TabIndex = 16;
            this.cbLightIsObstructedByRoomGeometry.Text = "Obstruct";
            this.toolTip.SetToolTip(this.cbLightIsObstructedByRoomGeometry, "Determines whether the effect of this light is obstructed by room geometry");
            this.cbLightIsObstructedByRoomGeometry.CheckedChanged += new System.EventHandler(this.cbLightIsObstructedByRoomGeometry_CheckedChanged);
            // 
            // cbLightEnabled
            // 
            this.cbLightEnabled.Enabled = false;
            this.cbLightEnabled.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cbLightEnabled.Location = new System.Drawing.Point(3, 81);
            this.cbLightEnabled.Name = "cbLightEnabled";
            this.cbLightEnabled.Size = new System.Drawing.Size(65, 22);
            this.cbLightEnabled.TabIndex = 15;
            this.cbLightEnabled.Text = "Enabled";
            this.toolTip.SetToolTip(this.cbLightEnabled, "Light is enabled");
            this.cbLightEnabled.CheckedChanged += new System.EventHandler(this.cbLightEnabled_CheckedChanged);
            // 
            // panelLightColor
            // 
            this.panelLightColor.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panelLightColor.Enabled = false;
            this.panelLightColor.Location = new System.Drawing.Point(206, 28);
            this.panelLightColor.Name = "panelLightColor";
            this.panelLightColor.Size = new System.Drawing.Size(60, 22);
            this.panelLightColor.TabIndex = 7;
            this.toolTip.SetToolTip(this.panelLightColor, "Light color");
            this.panelLightColor.Click += new System.EventHandler(this.panelLightColor_Click);
            // 
            // darkLabel12
            // 
            this.darkLabel12.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.darkLabel12.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.darkLabel12.Location = new System.Drawing.Point(268, 84);
            this.darkLabel12.Name = "darkLabel12";
            this.darkLabel12.Size = new System.Drawing.Size(38, 22);
            this.darkLabel12.TabIndex = 80;
            this.darkLabel12.Text = "Dir Y";
            this.darkLabel12.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // darkLabel13
            // 
            this.darkLabel13.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.darkLabel13.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.darkLabel13.Location = new System.Drawing.Point(268, 113);
            this.darkLabel13.Name = "darkLabel13";
            this.darkLabel13.Size = new System.Drawing.Size(38, 22);
            this.darkLabel13.TabIndex = 79;
            this.darkLabel13.Text = "Dir X";
            this.darkLabel13.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // darkLabel11
            // 
            this.darkLabel11.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.darkLabel11.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.darkLabel11.Location = new System.Drawing.Point(155, 55);
            this.darkLabel11.Name = "darkLabel11";
            this.darkLabel11.Size = new System.Drawing.Size(51, 22);
            this.darkLabel11.TabIndex = 78;
            this.darkLabel11.Text = "Intensity";
            this.darkLabel11.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // darkLabel9
            // 
            this.darkLabel9.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.darkLabel9.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.darkLabel9.Location = new System.Drawing.Point(268, 55);
            this.darkLabel9.Name = "darkLabel9";
            this.darkLabel9.Size = new System.Drawing.Size(38, 22);
            this.darkLabel9.TabIndex = 77;
            this.darkLabel9.Text = "Out α";
            this.darkLabel9.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // darkLabel10
            // 
            this.darkLabel10.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.darkLabel10.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.darkLabel10.Location = new System.Drawing.Point(268, 26);
            this.darkLabel10.Name = "darkLabel10";
            this.darkLabel10.Size = new System.Drawing.Size(38, 22);
            this.darkLabel10.TabIndex = 73;
            this.darkLabel10.Text = "In α";
            this.darkLabel10.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // darkLabel8
            // 
            this.darkLabel8.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.darkLabel8.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.darkLabel8.Location = new System.Drawing.Point(155, 113);
            this.darkLabel8.Name = "darkLabel8";
            this.darkLabel8.Size = new System.Drawing.Size(51, 22);
            this.darkLabel8.TabIndex = 71;
            this.darkLabel8.Text = "Out d";
            this.darkLabel8.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // darkLabel7
            // 
            this.darkLabel7.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.darkLabel7.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.darkLabel7.Location = new System.Drawing.Point(155, 84);
            this.darkLabel7.Name = "darkLabel7";
            this.darkLabel7.Size = new System.Drawing.Size(51, 22);
            this.darkLabel7.TabIndex = 70;
            this.darkLabel7.Text = "In d";
            this.darkLabel7.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // butAddLight
            // 
            this.butAddLight.Checked = false;
            this.butAddLight.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.butAddLight.Image = global::TombEditor.Properties.Resources.general_plus_math_16;
            this.butAddLight.Location = new System.Drawing.Point(123, 28);
            this.butAddLight.Name = "butAddLight";
            this.butAddLight.Size = new System.Drawing.Size(23, 23);
            this.butAddLight.TabIndex = 0;
            this.butAddLight.Tag = "";
            this.toolTip.SetToolTip(this.butAddLight, "Place light");
            this.butAddLight.Click += new System.EventHandler(this.butAddLight_Click);
            // 
            // numIntensity
            // 
            this.numIntensity.DecimalPlaces = 2;
            this.numIntensity.Enabled = false;
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
            this.numIntensity.Location = new System.Drawing.Point(206, 57);
            this.numIntensity.LoopValues = false;
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
            this.numIntensity.Name = "numIntensity";
            this.numIntensity.Size = new System.Drawing.Size(60, 22);
            this.numIntensity.TabIndex = 8;
            this.numIntensity.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.toolTip.SetToolTip(this.numIntensity, "Light intensity");
            this.numIntensity.ValueChanged += new System.EventHandler(this.numIntensity_ValueChanged);
            // 
            // numInnerRange
            // 
            this.numInnerRange.DecimalPlaces = 2;
            this.numInnerRange.Enabled = false;
            this.numInnerRange.Increment = new decimal(new int[] {
            3,
            0,
            0,
            131072});
            this.numInnerRange.IncrementAlternate = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.numInnerRange.Location = new System.Drawing.Point(206, 86);
            this.numInnerRange.LoopValues = false;
            this.numInnerRange.Maximum = new decimal(new int[] {
            256,
            0,
            0,
            0});
            this.numInnerRange.Name = "numInnerRange";
            this.numInnerRange.Size = new System.Drawing.Size(60, 22);
            this.numInnerRange.TabIndex = 9;
            this.numInnerRange.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.toolTip.SetToolTip(this.numInnerRange, "Inner radius or distance");
            this.numInnerRange.ValueChanged += new System.EventHandler(this.numInnerRange_ValueChanged);
            // 
            // numOuterRange
            // 
            this.numOuterRange.DecimalPlaces = 2;
            this.numOuterRange.Enabled = false;
            this.numOuterRange.Increment = new decimal(new int[] {
            3,
            0,
            0,
            131072});
            this.numOuterRange.IncrementAlternate = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.numOuterRange.Location = new System.Drawing.Point(206, 115);
            this.numOuterRange.LoopValues = false;
            this.numOuterRange.Maximum = new decimal(new int[] {
            256,
            0,
            0,
            0});
            this.numOuterRange.Name = "numOuterRange";
            this.numOuterRange.Size = new System.Drawing.Size(60, 22);
            this.numOuterRange.TabIndex = 10;
            this.numOuterRange.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.toolTip.SetToolTip(this.numOuterRange, "Outer radius or distance");
            this.numOuterRange.ValueChanged += new System.EventHandler(this.numOuterRange_ValueChanged);
            // 
            // numInnerAngle
            // 
            this.numInnerAngle.DecimalPlaces = 2;
            this.numInnerAngle.Enabled = false;
            this.numInnerAngle.IncrementAlternate = new decimal(new int[] {
            10,
            0,
            0,
            0});
            this.numInnerAngle.Location = new System.Drawing.Point(306, 28);
            this.numInnerAngle.LoopValues = false;
            this.numInnerAngle.Maximum = new decimal(new int[] {
            180,
            0,
            0,
            0});
            this.numInnerAngle.Name = "numInnerAngle";
            this.numInnerAngle.Size = new System.Drawing.Size(60, 22);
            this.numInnerAngle.TabIndex = 11;
            this.numInnerAngle.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.toolTip.SetToolTip(this.numInnerAngle, "Inner cone angle");
            this.numInnerAngle.ValueChanged += new System.EventHandler(this.numInnerAngle_ValueChanged);
            // 
            // numOuterAngle
            // 
            this.numOuterAngle.DecimalPlaces = 2;
            this.numOuterAngle.Enabled = false;
            this.numOuterAngle.IncrementAlternate = new decimal(new int[] {
            10,
            0,
            0,
            0});
            this.numOuterAngle.Location = new System.Drawing.Point(306, 57);
            this.numOuterAngle.LoopValues = false;
            this.numOuterAngle.Maximum = new decimal(new int[] {
            180,
            0,
            0,
            0});
            this.numOuterAngle.Name = "numOuterAngle";
            this.numOuterAngle.Size = new System.Drawing.Size(60, 22);
            this.numOuterAngle.TabIndex = 12;
            this.numOuterAngle.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.toolTip.SetToolTip(this.numOuterAngle, "Outer cone angle");
            this.numOuterAngle.ValueChanged += new System.EventHandler(this.numOuterAngle_ValueChanged);
            // 
            // numDirectionX
            // 
            this.numDirectionX.DecimalPlaces = 2;
            this.numDirectionX.Enabled = false;
            this.numDirectionX.IncrementAlternate = new decimal(new int[] {
            10,
            0,
            0,
            0});
            this.numDirectionX.Location = new System.Drawing.Point(306, 115);
            this.numDirectionX.LoopValues = false;
            this.numDirectionX.Maximum = new decimal(new int[] {
            90,
            0,
            0,
            0});
            this.numDirectionX.Minimum = new decimal(new int[] {
            90,
            0,
            0,
            -2147483648});
            this.numDirectionX.Name = "numDirectionX";
            this.numDirectionX.Size = new System.Drawing.Size(60, 22);
            this.numDirectionX.TabIndex = 14;
            this.numDirectionX.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.toolTip.SetToolTip(this.numDirectionX, "Angle around the X axis (vertical rotation)");
            this.numDirectionX.ValueChanged += new System.EventHandler(this.numDirectionX_ValueChanged);
            // 
            // numDirectionY
            // 
            this.numDirectionY.DecimalPlaces = 2;
            this.numDirectionY.Enabled = false;
            this.numDirectionY.IncrementAlternate = new decimal(new int[] {
            10,
            0,
            0,
            0});
            this.numDirectionY.Location = new System.Drawing.Point(306, 86);
            this.numDirectionY.LoopValues = false;
            this.numDirectionY.Maximum = new decimal(new int[] {
            720,
            0,
            0,
            0});
            this.numDirectionY.Minimum = new decimal(new int[] {
            360,
            0,
            0,
            -2147483648});
            this.numDirectionY.Name = "numDirectionY";
            this.numDirectionY.Size = new System.Drawing.Size(60, 22);
            this.numDirectionY.TabIndex = 13;
            this.numDirectionY.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.toolTip.SetToolTip(this.numDirectionY, "Angle around the Y axis (horizontal rotation)");
            this.numDirectionY.ValueChanged += new System.EventHandler(this.numDirectionY_ValueChanged);
            // 
            // toolTip
            // 
            this.toolTip.AutoPopDelay = 5000;
            this.toolTip.InitialDelay = 500;
            this.toolTip.ReshowDelay = 100;
            // 
            // cbLightIsUsedForImportedGeometry
            // 
            this.cbLightIsUsedForImportedGeometry.Enabled = false;
            this.cbLightIsUsedForImportedGeometry.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cbLightIsUsedForImportedGeometry.Location = new System.Drawing.Point(79, 119);
            this.cbLightIsUsedForImportedGeometry.Name = "cbLightIsUsedForImportedGeometry";
            this.cbLightIsUsedForImportedGeometry.Size = new System.Drawing.Size(70, 22);
            this.cbLightIsUsedForImportedGeometry.TabIndex = 19;
            this.cbLightIsUsedForImportedGeometry.Text = "Imported";
            this.toolTip.SetToolTip(this.cbLightIsUsedForImportedGeometry, "Use light for imported geometry");
            this.cbLightIsUsedForImportedGeometry.CheckedChanged += new System.EventHandler(this.cbLightIsUsedForImportedGeometry_CheckedChanged);
            // 
            // cmbLightQuality
            // 
            this.cmbLightQuality.Enabled = false;
            this.cmbLightQuality.Items.AddRange(new object[] {
            "Default quality",
            "Low quality",
            "Medium quality",
            "High quality"});
            this.cmbLightQuality.Location = new System.Drawing.Point(3, 57);
            this.cmbLightQuality.Name = "cmbLightQuality";
            this.cmbLightQuality.Size = new System.Drawing.Size(143, 23);
            this.cmbLightQuality.TabIndex = 6;
            this.toolTip.SetToolTip(this.cmbLightQuality, "Raytracing sample count.\r\nHigher value gives more smooth shadows on obstructed ar" +
        "eas.");
            this.cmbLightQuality.SelectionChangeCommitted += new System.EventHandler(this.cmbLightQualityChanged);
            // 
            // cmbLightTypes
            // 
            this.cmbLightTypes.Location = new System.Drawing.Point(3, 28);
            this.cmbLightTypes.Name = "cmbLightTypes";
            this.cmbLightTypes.Size = new System.Drawing.Size(114, 23);
            this.cmbLightTypes.TabIndex = 81;
            this.toolTip.SetToolTip(this.cmbLightTypes, "Light type to place");
            // 
            // cbCastShadow
            // 
            this.cbCastShadow.Enabled = false;
            this.cbCastShadow.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cbCastShadow.Location = new System.Drawing.Point(3, 119);
            this.cbCastShadow.Name = "cbCastShadow";
            this.cbCastShadow.Size = new System.Drawing.Size(70, 22);
            this.cbCastShadow.TabIndex = 87;
            this.cbCastShadow.Text = "Shadow";
            this.toolTip.SetToolTip(this.cbCastShadow, "Determines whether this light can cast shadows");
            // 
            // darkLabel6
            // 
            this.darkLabel6.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.darkLabel6.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.darkLabel6.Location = new System.Drawing.Point(155, 26);
            this.darkLabel6.Name = "darkLabel6";
            this.darkLabel6.Size = new System.Drawing.Size(51, 22);
            this.darkLabel6.TabIndex = 68;
            this.darkLabel6.Text = "Color";
            this.darkLabel6.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // Lighting
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.cbCastShadow);
            this.Controls.Add(this.cmbLightTypes);
            this.Controls.Add(this.cmbLightQuality);
            this.Controls.Add(this.cbLightIsUsedForImportedGeometry);
            this.Controls.Add(this.numDirectionY);
            this.Controls.Add(this.numDirectionX);
            this.Controls.Add(this.numOuterAngle);
            this.Controls.Add(this.numInnerAngle);
            this.Controls.Add(this.numOuterRange);
            this.Controls.Add(this.numInnerRange);
            this.Controls.Add(this.numIntensity);
            this.Controls.Add(this.cbLightIsDynamicallyUsed);
            this.Controls.Add(this.cbLightIsStaticallyUsed);
            this.Controls.Add(this.cbLightIsObstructedByRoomGeometry);
            this.Controls.Add(this.cbLightEnabled);
            this.Controls.Add(this.panelLightColor);
            this.Controls.Add(this.darkLabel12);
            this.Controls.Add(this.darkLabel13);
            this.Controls.Add(this.darkLabel11);
            this.Controls.Add(this.darkLabel9);
            this.Controls.Add(this.darkLabel10);
            this.Controls.Add(this.darkLabel8);
            this.Controls.Add(this.darkLabel7);
            this.Controls.Add(this.darkLabel6);
            this.Controls.Add(this.butAddLight);
            this.DefaultDockArea = DarkUI.Docking.DarkDockArea.Bottom;
            this.DockText = "Lighting";
            this.MinimumSize = new System.Drawing.Size(371, 141);
            this.Name = "Lighting";
            this.SerializationKey = "Lighting";
            this.Size = new System.Drawing.Size(371, 141);
            ((System.ComponentModel.ISupportInitialize)(this.numIntensity)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numInnerRange)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numOuterRange)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numInnerAngle)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numOuterAngle)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numDirectionX)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numDirectionY)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion
        private DarkUI.Controls.DarkCheckBox cbLightIsDynamicallyUsed;
        private DarkUI.Controls.DarkCheckBox cbLightIsStaticallyUsed;
        private DarkUI.Controls.DarkCheckBox cbLightIsObstructedByRoomGeometry;
        private DarkUI.Controls.DarkCheckBox cbLightEnabled;
        private DarkUI.Controls.DarkPanel panelLightColor;
        private DarkUI.Controls.DarkLabel darkLabel12;
        private DarkUI.Controls.DarkLabel darkLabel13;
        private DarkUI.Controls.DarkLabel darkLabel11;
        private DarkUI.Controls.DarkLabel darkLabel9;
        private DarkUI.Controls.DarkLabel darkLabel10;
        private DarkUI.Controls.DarkLabel darkLabel8;
        private DarkUI.Controls.DarkLabel darkLabel7;
        private DarkUI.Controls.DarkButton butAddLight;
        private DarkUI.Controls.DarkNumericUpDown numIntensity;
        private DarkUI.Controls.DarkNumericUpDown numInnerRange;
        private DarkUI.Controls.DarkNumericUpDown numOuterRange;
        private DarkUI.Controls.DarkNumericUpDown numInnerAngle;
        private DarkUI.Controls.DarkNumericUpDown numOuterAngle;
        private DarkUI.Controls.DarkNumericUpDown numDirectionX;
        private DarkUI.Controls.DarkNumericUpDown numDirectionY;
        private System.Windows.Forms.ToolTip toolTip;
        private DarkUI.Controls.DarkCheckBox cbLightIsUsedForImportedGeometry;
        private DarkUI.Controls.DarkComboBox cmbLightQuality;
        private DarkUI.Controls.DarkComboBox cmbLightTypes;
        private DarkUI.Controls.DarkCheckBox cbCastShadow;
        private DarkUI.Controls.DarkLabel darkLabel6;
    }
}
