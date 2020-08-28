namespace TombEditor.Forms
{
    partial class FormRoomProperties
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
            this.butCancel = new DarkUI.Controls.DarkButton();
            this.butOk = new DarkUI.Controls.DarkButton();
            this.cbReverb = new DarkUI.Controls.DarkCheckBox();
            this.cbRoomType = new DarkUI.Controls.DarkCheckBox();
            this.cbSky = new DarkUI.Controls.DarkCheckBox();
            this.cbWind = new DarkUI.Controls.DarkCheckBox();
            this.cbDamage = new DarkUI.Controls.DarkCheckBox();
            this.cbCold = new DarkUI.Controls.DarkCheckBox();
            this.cbPathfinding = new DarkUI.Controls.DarkCheckBox();
            this.cbLensflare = new DarkUI.Controls.DarkCheckBox();
            this.cbPortalShade = new DarkUI.Controls.DarkCheckBox();
            this.cbLightEffect = new DarkUI.Controls.DarkCheckBox();
            this.cbLightStrength = new DarkUI.Controls.DarkCheckBox();
            this.cbVisible = new DarkUI.Controls.DarkCheckBox();
            this.darkLabel1 = new DarkUI.Controls.DarkLabel();
            this.cbAmbient = new DarkUI.Controls.DarkCheckBox();
            this.cbTags = new DarkUI.Controls.DarkCheckBox();
            this.SuspendLayout();
            // 
            // butCancel
            // 
            this.butCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.butCancel.Checked = false;
            this.butCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.butCancel.Location = new System.Drawing.Point(163, 220);
            this.butCancel.Name = "butCancel";
            this.butCancel.Size = new System.Drawing.Size(80, 23);
            this.butCancel.TabIndex = 14;
            this.butCancel.Text = "Cancel";
            this.butCancel.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.butCancel.Click += new System.EventHandler(this.butCancel_Click);
            // 
            // butOk
            // 
            this.butOk.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.butOk.Checked = false;
            this.butOk.Location = new System.Drawing.Point(77, 220);
            this.butOk.Name = "butOk";
            this.butOk.Size = new System.Drawing.Size(80, 23);
            this.butOk.TabIndex = 13;
            this.butOk.Text = "OK";
            this.butOk.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.butOk.Click += new System.EventHandler(this.butOk_Click);
            // 
            // cbReverb
            // 
            this.cbReverb.AutoSize = true;
            this.cbReverb.Location = new System.Drawing.Point(110, 75);
            this.cbReverb.Name = "cbReverb";
            this.cbReverb.Size = new System.Drawing.Size(86, 17);
            this.cbReverb.TabIndex = 3;
            this.cbReverb.Text = "Reverb type";
            // 
            // cbRoomType
            // 
            this.cbRoomType.AutoSize = true;
            this.cbRoomType.Location = new System.Drawing.Point(12, 75);
            this.cbRoomType.Name = "cbRoomType";
            this.cbRoomType.Size = new System.Drawing.Size(81, 17);
            this.cbRoomType.TabIndex = 2;
            this.cbRoomType.Text = "Room type";
            // 
            // cbSky
            // 
            this.cbSky.AutoSize = true;
            this.cbSky.Location = new System.Drawing.Point(12, 98);
            this.cbSky.Name = "cbSky";
            this.cbSky.Size = new System.Drawing.Size(62, 17);
            this.cbSky.TabIndex = 3;
            this.cbSky.Text = "Skybox";
            // 
            // cbWind
            // 
            this.cbWind.AutoSize = true;
            this.cbWind.Location = new System.Drawing.Point(12, 121);
            this.cbWind.Name = "cbWind";
            this.cbWind.Size = new System.Drawing.Size(54, 17);
            this.cbWind.TabIndex = 5;
            this.cbWind.Text = "Wind";
            // 
            // cbDamage
            // 
            this.cbDamage.AutoSize = true;
            this.cbDamage.Location = new System.Drawing.Point(110, 98);
            this.cbDamage.Name = "cbDamage";
            this.cbDamage.Size = new System.Drawing.Size(68, 17);
            this.cbDamage.TabIndex = 4;
            this.cbDamage.Text = "Damage";
            // 
            // cbCold
            // 
            this.cbCold.AutoSize = true;
            this.cbCold.Location = new System.Drawing.Point(110, 121);
            this.cbCold.Name = "cbCold";
            this.cbCold.Size = new System.Drawing.Size(50, 17);
            this.cbCold.TabIndex = 6;
            this.cbCold.Text = "Cold";
            // 
            // cbPathfinding
            // 
            this.cbPathfinding.AutoSize = true;
            this.cbPathfinding.Location = new System.Drawing.Point(110, 144);
            this.cbPathfinding.Name = "cbPathfinding";
            this.cbPathfinding.Size = new System.Drawing.Size(106, 17);
            this.cbPathfinding.TabIndex = 8;
            this.cbPathfinding.Text = "No pathfinding";
            // 
            // cbLensflare
            // 
            this.cbLensflare.AutoSize = true;
            this.cbLensflare.Location = new System.Drawing.Point(12, 144);
            this.cbLensflare.Name = "cbLensflare";
            this.cbLensflare.Size = new System.Drawing.Size(88, 17);
            this.cbLensflare.TabIndex = 7;
            this.cbLensflare.Text = "No lensflare";
            // 
            // cbPortalShade
            // 
            this.cbPortalShade.AutoSize = true;
            this.cbPortalShade.Location = new System.Drawing.Point(12, 190);
            this.cbPortalShade.Name = "cbPortalShade";
            this.cbPortalShade.Size = new System.Drawing.Size(90, 17);
            this.cbPortalShade.TabIndex = 11;
            this.cbPortalShade.Text = "Portal shade";
            // 
            // cbLightEffect
            // 
            this.cbLightEffect.AutoSize = true;
            this.cbLightEffect.Location = new System.Drawing.Point(12, 167);
            this.cbLightEffect.Name = "cbLightEffect";
            this.cbLightEffect.Size = new System.Drawing.Size(55, 17);
            this.cbLightEffect.TabIndex = 9;
            this.cbLightEffect.Text = "Effect";
            // 
            // cbLightStrength
            // 
            this.cbLightStrength.AutoSize = true;
            this.cbLightStrength.Location = new System.Drawing.Point(110, 167);
            this.cbLightStrength.Name = "cbLightStrength";
            this.cbLightStrength.Size = new System.Drawing.Size(102, 17);
            this.cbLightStrength.TabIndex = 10;
            this.cbLightStrength.Text = "Effect strength";
            // 
            // cbVisible
            // 
            this.cbVisible.AutoSize = true;
            this.cbVisible.Location = new System.Drawing.Point(110, 190);
            this.cbVisible.Name = "cbVisible";
            this.cbVisible.Size = new System.Drawing.Size(69, 17);
            this.cbVisible.TabIndex = 12;
            this.cbVisible.Text = "Visibility";
            // 
            // darkLabel1
            // 
            this.darkLabel1.AutoSize = true;
            this.darkLabel1.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.darkLabel1.Location = new System.Drawing.Point(9, 9);
            this.darkLabel1.Name = "darkLabel1";
            this.darkLabel1.Size = new System.Drawing.Size(229, 26);
            this.darkLabel1.TabIndex = 15;
            this.darkLabel1.Text = "Apply chosen properties from current room\r\nto all selected rooms:";
            // 
            // cbAmbient
            // 
            this.cbAmbient.AutoSize = true;
            this.cbAmbient.Location = new System.Drawing.Point(12, 52);
            this.cbAmbient.Name = "cbAmbient";
            this.cbAmbient.Size = new System.Drawing.Size(96, 17);
            this.cbAmbient.TabIndex = 0;
            this.cbAmbient.Text = "Ambient light";
            // 
            // cbTags
            // 
            this.cbTags.AutoSize = true;
            this.cbTags.Location = new System.Drawing.Point(110, 52);
            this.cbTags.Name = "cbTags";
            this.cbTags.Size = new System.Drawing.Size(49, 17);
            this.cbTags.TabIndex = 1;
            this.cbTags.Text = "Tags";
            // 
            // FormRoomProperties
            // 
            this.AcceptButton = this.butOk;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.butCancel;
            this.ClientSize = new System.Drawing.Size(251, 251);
            this.Controls.Add(this.cbTags);
            this.Controls.Add(this.cbAmbient);
            this.Controls.Add(this.darkLabel1);
            this.Controls.Add(this.cbVisible);
            this.Controls.Add(this.cbLightStrength);
            this.Controls.Add(this.cbLightEffect);
            this.Controls.Add(this.cbPortalShade);
            this.Controls.Add(this.cbLensflare);
            this.Controls.Add(this.cbPathfinding);
            this.Controls.Add(this.cbCold);
            this.Controls.Add(this.cbDamage);
            this.Controls.Add(this.cbWind);
            this.Controls.Add(this.cbSky);
            this.Controls.Add(this.cbRoomType);
            this.Controls.Add(this.cbReverb);
            this.Controls.Add(this.butCancel);
            this.Controls.Add(this.butOk);
            this.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FormRoomProperties";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Change room properties";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private DarkUI.Controls.DarkButton butCancel;
        private DarkUI.Controls.DarkButton butOk;
        private DarkUI.Controls.DarkCheckBox cbReverb;
        private DarkUI.Controls.DarkCheckBox cbRoomType;
        private DarkUI.Controls.DarkCheckBox cbSky;
        private DarkUI.Controls.DarkCheckBox cbWind;
        private DarkUI.Controls.DarkCheckBox cbDamage;
        private DarkUI.Controls.DarkCheckBox cbCold;
        private DarkUI.Controls.DarkCheckBox cbPathfinding;
        private DarkUI.Controls.DarkCheckBox cbLensflare;
        private DarkUI.Controls.DarkCheckBox cbPortalShade;
        private DarkUI.Controls.DarkCheckBox cbLightEffect;
        private DarkUI.Controls.DarkCheckBox cbLightStrength;
        private DarkUI.Controls.DarkCheckBox cbVisible;
        private DarkUI.Controls.DarkLabel darkLabel1;
        private DarkUI.Controls.DarkCheckBox cbAmbient;
        private DarkUI.Controls.DarkCheckBox cbTags;
    }
}