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
            this.lightPalette = new TombEditor.Controls.PanelPalette();
            this.cbLightIsDynamicallyUsed = new DarkUI.Controls.DarkCheckBox();
            this.cbLightIsStaticallyUsed = new DarkUI.Controls.DarkCheckBox();
            this.cbLightCastsShadows = new DarkUI.Controls.DarkCheckBox();
            this.cbLightEnabled = new DarkUI.Controls.DarkCheckBox();
            this.numLightDirectionY = new TombEditor.Controls.LightParameterController();
            this.numLightDirectionX = new TombEditor.Controls.LightParameterController();
            this.numLightOut = new TombEditor.Controls.LightParameterController();
            this.numLightIn = new TombEditor.Controls.LightParameterController();
            this.numLightCutoff = new TombEditor.Controls.LightParameterController();
            this.numLightLen = new TombEditor.Controls.LightParameterController();
            this.numLightIntensity = new TombEditor.Controls.LightParameterController();
            this.panelLightColor = new System.Windows.Forms.Panel();
            this.darkLabel12 = new DarkUI.Controls.DarkLabel();
            this.darkLabel13 = new DarkUI.Controls.DarkLabel();
            this.darkLabel11 = new DarkUI.Controls.DarkLabel();
            this.darkLabel9 = new DarkUI.Controls.DarkLabel();
            this.darkLabel23 = new DarkUI.Controls.DarkLabel();
            this.darkLabel24 = new DarkUI.Controls.DarkLabel();
            this.darkLabel21 = new DarkUI.Controls.DarkLabel();
            this.darkLabel2 = new DarkUI.Controls.DarkLabel();
            this.darkLabel10 = new DarkUI.Controls.DarkLabel();
            this.darkLabel8 = new DarkUI.Controls.DarkLabel();
            this.darkLabel7 = new DarkUI.Controls.DarkLabel();
            this.darkLabel6 = new DarkUI.Controls.DarkLabel();
            this.butAddFogBulb = new DarkUI.Controls.DarkButton();
            this.butAddEffectLight = new DarkUI.Controls.DarkButton();
            this.butAddSpotLight = new DarkUI.Controls.DarkButton();
            this.butAddSun = new DarkUI.Controls.DarkButton();
            this.butAddShadow = new DarkUI.Controls.DarkButton();
            this.butAddPointLight = new DarkUI.Controls.DarkButton();
            this.darkLabel5 = new DarkUI.Controls.DarkLabel();
            this.colorDialog = new System.Windows.Forms.ColorDialog();
            ((System.ComponentModel.ISupportInitialize)(this.lightPalette)).BeginInit();
            this.SuspendLayout();
            // 
            // lightPalette
            // 
            this.lightPalette.Location = new System.Drawing.Point(444, 28);
            this.lightPalette.Name = "lightPalette";
            this.lightPalette.Size = new System.Drawing.Size(642, 99);
            this.lightPalette.TabIndex = 81;
            this.lightPalette.TabStop = false;
            // 
            // cbLightIsDynamicallyUsed
            // 
            this.cbLightIsDynamicallyUsed.CheckAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.cbLightIsDynamicallyUsed.Enabled = false;
            this.cbLightIsDynamicallyUsed.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cbLightIsDynamicallyUsed.Location = new System.Drawing.Point(423, 102);
            this.cbLightIsDynamicallyUsed.Name = "cbLightIsDynamicallyUsed";
            this.cbLightIsDynamicallyUsed.Size = new System.Drawing.Size(15, 22);
            this.cbLightIsDynamicallyUsed.TabIndex = 92;
            this.cbLightIsDynamicallyUsed.CheckedChanged += new System.EventHandler(this.cbLightIsDynamicallyUsed_CheckedChanged);
            // 
            // cbLightIsStaticallyUsed
            // 
            this.cbLightIsStaticallyUsed.CheckAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.cbLightIsStaticallyUsed.Enabled = false;
            this.cbLightIsStaticallyUsed.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cbLightIsStaticallyUsed.Location = new System.Drawing.Point(423, 77);
            this.cbLightIsStaticallyUsed.Name = "cbLightIsStaticallyUsed";
            this.cbLightIsStaticallyUsed.Size = new System.Drawing.Size(15, 22);
            this.cbLightIsStaticallyUsed.TabIndex = 91;
            this.cbLightIsStaticallyUsed.CheckedChanged += new System.EventHandler(this.cbLightIsStaticallyUsed_CheckedChanged);
            // 
            // cbLightCastsShadows
            // 
            this.cbLightCastsShadows.CheckAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.cbLightCastsShadows.Enabled = false;
            this.cbLightCastsShadows.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cbLightCastsShadows.Location = new System.Drawing.Point(423, 52);
            this.cbLightCastsShadows.Name = "cbLightCastsShadows";
            this.cbLightCastsShadows.Size = new System.Drawing.Size(15, 22);
            this.cbLightCastsShadows.TabIndex = 90;
            this.cbLightCastsShadows.CheckedChanged += new System.EventHandler(this.cbLightCastsShadows_CheckedChanged);
            // 
            // cbLightEnabled
            // 
            this.cbLightEnabled.CheckAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.cbLightEnabled.Enabled = false;
            this.cbLightEnabled.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cbLightEnabled.Location = new System.Drawing.Point(423, 27);
            this.cbLightEnabled.Name = "cbLightEnabled";
            this.cbLightEnabled.Size = new System.Drawing.Size(15, 22);
            this.cbLightEnabled.TabIndex = 89;
            this.cbLightEnabled.CheckedChanged += new System.EventHandler(this.cbLightEnabled_CheckedChanged);
            // 
            // numLightDirectionY
            // 
            this.numLightDirectionY.BackColor = System.Drawing.Color.DimGray;
            this.numLightDirectionY.Enabled = false;
            this.numLightDirectionY.LightParameter = TombEditor.Controls.LightParameter.Intensity;
            this.numLightDirectionY.Location = new System.Drawing.Point(302, 102);
            this.numLightDirectionY.Name = "numLightDirectionY";
            this.numLightDirectionY.Size = new System.Drawing.Size(60, 22);
            this.numLightDirectionY.TabIndex = 88;
            this.numLightDirectionY.Value = 0F;
            // 
            // numLightDirectionX
            // 
            this.numLightDirectionX.BackColor = System.Drawing.Color.DimGray;
            this.numLightDirectionX.Enabled = false;
            this.numLightDirectionX.LightParameter = TombEditor.Controls.LightParameter.Intensity;
            this.numLightDirectionX.Location = new System.Drawing.Point(302, 77);
            this.numLightDirectionX.Name = "numLightDirectionX";
            this.numLightDirectionX.Size = new System.Drawing.Size(60, 22);
            this.numLightDirectionX.TabIndex = 87;
            this.numLightDirectionX.Value = 0F;
            // 
            // numLightOut
            // 
            this.numLightOut.BackColor = System.Drawing.Color.DimGray;
            this.numLightOut.Enabled = false;
            this.numLightOut.LightParameter = TombEditor.Controls.LightParameter.Intensity;
            this.numLightOut.Location = new System.Drawing.Point(198, 102);
            this.numLightOut.Name = "numLightOut";
            this.numLightOut.Size = new System.Drawing.Size(60, 22);
            this.numLightOut.TabIndex = 86;
            this.numLightOut.Value = 0F;
            // 
            // numLightIn
            // 
            this.numLightIn.BackColor = System.Drawing.Color.DimGray;
            this.numLightIn.Enabled = false;
            this.numLightIn.LightParameter = TombEditor.Controls.LightParameter.Intensity;
            this.numLightIn.Location = new System.Drawing.Point(198, 77);
            this.numLightIn.Name = "numLightIn";
            this.numLightIn.Size = new System.Drawing.Size(60, 22);
            this.numLightIn.TabIndex = 85;
            this.numLightIn.Value = 0F;
            // 
            // numLightCutoff
            // 
            this.numLightCutoff.BackColor = System.Drawing.Color.DimGray;
            this.numLightCutoff.Enabled = false;
            this.numLightCutoff.LightParameter = TombEditor.Controls.LightParameter.Intensity;
            this.numLightCutoff.Location = new System.Drawing.Point(302, 52);
            this.numLightCutoff.Name = "numLightCutoff";
            this.numLightCutoff.Size = new System.Drawing.Size(60, 22);
            this.numLightCutoff.TabIndex = 84;
            this.numLightCutoff.Value = 0F;
            // 
            // numLightLen
            // 
            this.numLightLen.BackColor = System.Drawing.Color.DimGray;
            this.numLightLen.Enabled = false;
            this.numLightLen.LightParameter = TombEditor.Controls.LightParameter.Intensity;
            this.numLightLen.Location = new System.Drawing.Point(302, 27);
            this.numLightLen.Name = "numLightLen";
            this.numLightLen.Size = new System.Drawing.Size(60, 22);
            this.numLightLen.TabIndex = 83;
            this.numLightLen.Value = 0F;
            // 
            // numLightIntensity
            // 
            this.numLightIntensity.BackColor = System.Drawing.Color.DimGray;
            this.numLightIntensity.Enabled = false;
            this.numLightIntensity.LightParameter = TombEditor.Controls.LightParameter.Intensity;
            this.numLightIntensity.Location = new System.Drawing.Point(198, 52);
            this.numLightIntensity.Name = "numLightIntensity";
            this.numLightIntensity.Size = new System.Drawing.Size(60, 22);
            this.numLightIntensity.TabIndex = 82;
            this.numLightIntensity.Value = 0F;
            // 
            // panelLightColor
            // 
            this.panelLightColor.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panelLightColor.Enabled = false;
            this.panelLightColor.Location = new System.Drawing.Point(198, 27);
            this.panelLightColor.Name = "panelLightColor";
            this.panelLightColor.Size = new System.Drawing.Size(60, 22);
            this.panelLightColor.TabIndex = 69;
            this.panelLightColor.Click += new System.EventHandler(this.panelLightColor_Click);
            // 
            // darkLabel12
            // 
            this.darkLabel12.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.darkLabel12.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.darkLabel12.Location = new System.Drawing.Point(251, 102);
            this.darkLabel12.Name = "darkLabel12";
            this.darkLabel12.Size = new System.Drawing.Size(51, 22);
            this.darkLabel12.TabIndex = 80;
            this.darkLabel12.Text = "Dir Y";
            this.darkLabel12.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // darkLabel13
            // 
            this.darkLabel13.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.darkLabel13.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.darkLabel13.Location = new System.Drawing.Point(251, 77);
            this.darkLabel13.Name = "darkLabel13";
            this.darkLabel13.Size = new System.Drawing.Size(51, 22);
            this.darkLabel13.TabIndex = 79;
            this.darkLabel13.Text = "Dir X";
            this.darkLabel13.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // darkLabel11
            // 
            this.darkLabel11.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.darkLabel11.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.darkLabel11.Location = new System.Drawing.Point(147, 52);
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
            this.darkLabel9.Location = new System.Drawing.Point(251, 52);
            this.darkLabel9.Name = "darkLabel9";
            this.darkLabel9.Size = new System.Drawing.Size(51, 22);
            this.darkLabel9.TabIndex = 77;
            this.darkLabel9.Text = "Cutoff";
            this.darkLabel9.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // darkLabel23
            // 
            this.darkLabel23.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.darkLabel23.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.darkLabel23.Location = new System.Drawing.Point(362, 102);
            this.darkLabel23.Name = "darkLabel23";
            this.darkLabel23.Size = new System.Drawing.Size(55, 22);
            this.darkLabel23.TabIndex = 75;
            this.darkLabel23.Text = "Dynamic";
            this.darkLabel23.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // darkLabel24
            // 
            this.darkLabel24.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.darkLabel24.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.darkLabel24.Location = new System.Drawing.Point(359, 78);
            this.darkLabel24.Name = "darkLabel24";
            this.darkLabel24.Size = new System.Drawing.Size(58, 22);
            this.darkLabel24.TabIndex = 74;
            this.darkLabel24.Text = "Statically";
            this.darkLabel24.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // darkLabel21
            // 
            this.darkLabel21.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.darkLabel21.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.darkLabel21.Location = new System.Drawing.Point(362, 52);
            this.darkLabel21.Name = "darkLabel21";
            this.darkLabel21.Size = new System.Drawing.Size(55, 22);
            this.darkLabel21.TabIndex = 72;
            this.darkLabel21.Text = "Shadows";
            this.darkLabel21.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // darkLabel2
            // 
            this.darkLabel2.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.darkLabel2.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.darkLabel2.Location = new System.Drawing.Point(366, 27);
            this.darkLabel2.Name = "darkLabel2";
            this.darkLabel2.Size = new System.Drawing.Size(51, 22);
            this.darkLabel2.TabIndex = 76;
            this.darkLabel2.Text = "Enabled";
            this.darkLabel2.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // darkLabel10
            // 
            this.darkLabel10.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.darkLabel10.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.darkLabel10.Location = new System.Drawing.Point(251, 27);
            this.darkLabel10.Name = "darkLabel10";
            this.darkLabel10.Size = new System.Drawing.Size(51, 22);
            this.darkLabel10.TabIndex = 73;
            this.darkLabel10.Text = "Len";
            this.darkLabel10.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // darkLabel8
            // 
            this.darkLabel8.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.darkLabel8.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.darkLabel8.Location = new System.Drawing.Point(147, 102);
            this.darkLabel8.Name = "darkLabel8";
            this.darkLabel8.Size = new System.Drawing.Size(51, 22);
            this.darkLabel8.TabIndex = 71;
            this.darkLabel8.Text = "Out";
            this.darkLabel8.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // darkLabel7
            // 
            this.darkLabel7.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.darkLabel7.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.darkLabel7.Location = new System.Drawing.Point(147, 77);
            this.darkLabel7.Name = "darkLabel7";
            this.darkLabel7.Size = new System.Drawing.Size(51, 22);
            this.darkLabel7.TabIndex = 70;
            this.darkLabel7.Text = "In";
            this.darkLabel7.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // darkLabel6
            // 
            this.darkLabel6.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.darkLabel6.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.darkLabel6.Location = new System.Drawing.Point(147, 27);
            this.darkLabel6.Name = "darkLabel6";
            this.darkLabel6.Size = new System.Drawing.Size(51, 22);
            this.darkLabel6.TabIndex = 68;
            this.darkLabel6.Text = "Color";
            this.darkLabel6.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // butAddFogBulb
            // 
            this.butAddFogBulb.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.butAddFogBulb.Image = global::TombEditor.Properties.Resources.Fog_16;
            this.butAddFogBulb.Location = new System.Drawing.Point(77, 101);
            this.butAddFogBulb.Name = "butAddFogBulb";
            this.butAddFogBulb.Padding = new System.Windows.Forms.Padding(5);
            this.butAddFogBulb.Size = new System.Drawing.Size(68, 23);
            this.butAddFogBulb.TabIndex = 67;
            this.butAddFogBulb.Text = "Fog";
            this.butAddFogBulb.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.butAddFogBulb.Click += new System.EventHandler(this.butAddFogBulb_Click);
            // 
            // butAddEffectLight
            // 
            this.butAddEffectLight.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.butAddEffectLight.Image = global::TombEditor.Properties.Resources.Effect_16;
            this.butAddEffectLight.Location = new System.Drawing.Point(77, 72);
            this.butAddEffectLight.Name = "butAddEffectLight";
            this.butAddEffectLight.Padding = new System.Windows.Forms.Padding(5);
            this.butAddEffectLight.Size = new System.Drawing.Size(68, 23);
            this.butAddEffectLight.TabIndex = 66;
            this.butAddEffectLight.Text = "Effect";
            this.butAddEffectLight.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.butAddEffectLight.Click += new System.EventHandler(this.butAddEffectLight_Click);
            // 
            // butAddSpotLight
            // 
            this.butAddSpotLight.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.butAddSpotLight.Image = global::TombEditor.Properties.Resources.Spotlight_16;
            this.butAddSpotLight.Location = new System.Drawing.Point(77, 43);
            this.butAddSpotLight.Name = "butAddSpotLight";
            this.butAddSpotLight.Padding = new System.Windows.Forms.Padding(5);
            this.butAddSpotLight.Size = new System.Drawing.Size(68, 23);
            this.butAddSpotLight.TabIndex = 65;
            this.butAddSpotLight.Text = "Spot";
            this.butAddSpotLight.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.butAddSpotLight.Click += new System.EventHandler(this.butAddSpotLight_Click);
            // 
            // butAddSun
            // 
            this.butAddSun.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.butAddSun.Image = global::TombEditor.Properties.Resources.sun_16;
            this.butAddSun.Location = new System.Drawing.Point(3, 101);
            this.butAddSun.Name = "butAddSun";
            this.butAddSun.Padding = new System.Windows.Forms.Padding(5);
            this.butAddSun.Size = new System.Drawing.Size(68, 23);
            this.butAddSun.TabIndex = 64;
            this.butAddSun.Text = "Sun";
            this.butAddSun.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.butAddSun.Click += new System.EventHandler(this.butAddSun_Click);
            // 
            // butAddShadow
            // 
            this.butAddShadow.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.butAddShadow.Image = global::TombEditor.Properties.Resources.Shadow_16;
            this.butAddShadow.Location = new System.Drawing.Point(3, 72);
            this.butAddShadow.Name = "butAddShadow";
            this.butAddShadow.Padding = new System.Windows.Forms.Padding(5);
            this.butAddShadow.Size = new System.Drawing.Size(68, 23);
            this.butAddShadow.TabIndex = 63;
            this.butAddShadow.Text = "Shadow";
            this.butAddShadow.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.butAddShadow.Click += new System.EventHandler(this.butAddShadow_Click);
            // 
            // butAddPointLight
            // 
            this.butAddPointLight.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.butAddPointLight.Image = global::TombEditor.Properties.Resources.LightPoint_16;
            this.butAddPointLight.Location = new System.Drawing.Point(3, 43);
            this.butAddPointLight.Name = "butAddPointLight";
            this.butAddPointLight.Padding = new System.Windows.Forms.Padding(5);
            this.butAddPointLight.Size = new System.Drawing.Size(68, 23);
            this.butAddPointLight.TabIndex = 62;
            this.butAddPointLight.Text = "Point";
            this.butAddPointLight.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.butAddPointLight.Click += new System.EventHandler(this.butAddPointLight_Click);
            // 
            // darkLabel5
            // 
            this.darkLabel5.AutoSize = true;
            this.darkLabel5.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.darkLabel5.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.darkLabel5.Location = new System.Drawing.Point(0, 27);
            this.darkLabel5.Name = "darkLabel5";
            this.darkLabel5.Size = new System.Drawing.Size(55, 13);
            this.darkLabel5.TabIndex = 61;
            this.darkLabel5.Text = "Add light";
            // 
            // colorDialog
            // 
            this.colorDialog.AnyColor = true;
            this.colorDialog.FullOpen = true;
            // 
            // Lighting
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.lightPalette);
            this.Controls.Add(this.cbLightIsDynamicallyUsed);
            this.Controls.Add(this.cbLightIsStaticallyUsed);
            this.Controls.Add(this.cbLightCastsShadows);
            this.Controls.Add(this.cbLightEnabled);
            this.Controls.Add(this.numLightDirectionY);
            this.Controls.Add(this.numLightDirectionX);
            this.Controls.Add(this.numLightOut);
            this.Controls.Add(this.numLightIn);
            this.Controls.Add(this.numLightCutoff);
            this.Controls.Add(this.numLightLen);
            this.Controls.Add(this.numLightIntensity);
            this.Controls.Add(this.panelLightColor);
            this.Controls.Add(this.darkLabel12);
            this.Controls.Add(this.darkLabel13);
            this.Controls.Add(this.darkLabel11);
            this.Controls.Add(this.darkLabel9);
            this.Controls.Add(this.darkLabel23);
            this.Controls.Add(this.darkLabel24);
            this.Controls.Add(this.darkLabel21);
            this.Controls.Add(this.darkLabel2);
            this.Controls.Add(this.darkLabel10);
            this.Controls.Add(this.darkLabel8);
            this.Controls.Add(this.darkLabel7);
            this.Controls.Add(this.darkLabel6);
            this.Controls.Add(this.butAddFogBulb);
            this.Controls.Add(this.butAddEffectLight);
            this.Controls.Add(this.butAddSpotLight);
            this.Controls.Add(this.butAddSun);
            this.Controls.Add(this.butAddShadow);
            this.Controls.Add(this.butAddPointLight);
            this.Controls.Add(this.darkLabel5);
            this.DefaultDockArea = DarkUI.Docking.DarkDockArea.Bottom;
            this.DockText = "Lighting";
            this.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.Name = "Lighting";
            this.Size = new System.Drawing.Size(1088, 131);
            ((System.ComponentModel.ISupportInitialize)(this.lightPalette)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private Controls.PanelPalette lightPalette;
        private DarkUI.Controls.DarkCheckBox cbLightIsDynamicallyUsed;
        private DarkUI.Controls.DarkCheckBox cbLightIsStaticallyUsed;
        private DarkUI.Controls.DarkCheckBox cbLightCastsShadows;
        private DarkUI.Controls.DarkCheckBox cbLightEnabled;
        private Controls.LightParameterController numLightDirectionY;
        private Controls.LightParameterController numLightDirectionX;
        private Controls.LightParameterController numLightOut;
        private Controls.LightParameterController numLightIn;
        private Controls.LightParameterController numLightCutoff;
        private Controls.LightParameterController numLightLen;
        private Controls.LightParameterController numLightIntensity;
        private System.Windows.Forms.Panel panelLightColor;
        private DarkUI.Controls.DarkLabel darkLabel12;
        private DarkUI.Controls.DarkLabel darkLabel13;
        private DarkUI.Controls.DarkLabel darkLabel11;
        private DarkUI.Controls.DarkLabel darkLabel9;
        private DarkUI.Controls.DarkLabel darkLabel23;
        private DarkUI.Controls.DarkLabel darkLabel24;
        private DarkUI.Controls.DarkLabel darkLabel21;
        private DarkUI.Controls.DarkLabel darkLabel2;
        private DarkUI.Controls.DarkLabel darkLabel10;
        private DarkUI.Controls.DarkLabel darkLabel8;
        private DarkUI.Controls.DarkLabel darkLabel7;
        private DarkUI.Controls.DarkLabel darkLabel6;
        private DarkUI.Controls.DarkButton butAddFogBulb;
        private DarkUI.Controls.DarkButton butAddEffectLight;
        private DarkUI.Controls.DarkButton butAddSpotLight;
        private DarkUI.Controls.DarkButton butAddSun;
        private DarkUI.Controls.DarkButton butAddShadow;
        private DarkUI.Controls.DarkButton butAddPointLight;
        private DarkUI.Controls.DarkLabel darkLabel5;
        private System.Windows.Forms.ColorDialog colorDialog;
    }
}
