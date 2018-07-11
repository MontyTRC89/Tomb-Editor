using DarkUI.Controls;

namespace TombEditor.Forms
{
    partial class FormStaticMesh
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
            this.butOK = new DarkUI.Controls.DarkButton();
            this.butCancel = new DarkUI.Controls.DarkButton();
            this.cbDisableCollision = new DarkUI.Controls.DarkCheckBox();
            this.cbIceTrasparency = new DarkUI.Controls.DarkCheckBox();
            this.backgroundWorker1 = new System.ComponentModel.BackgroundWorker();
            this.cbHeavyTriggerOnCollision = new DarkUI.Controls.DarkCheckBox();
            this.cbHardShatter = new DarkUI.Controls.DarkCheckBox();
            this.cbHugeCollision = new DarkUI.Controls.DarkCheckBox();
            this.cbPoisonLaraOnCollision = new DarkUI.Controls.DarkCheckBox();
            this.cbExplodeKillingOnCollision = new DarkUI.Controls.DarkCheckBox();
            this.cbBurnLaraOnCollision = new DarkUI.Controls.DarkCheckBox();
            this.cbDamageLaraOnContact = new DarkUI.Controls.DarkCheckBox();
            this.cbGlassTrasparency = new DarkUI.Controls.DarkCheckBox();
            this.cbScalable = new DarkUI.Controls.DarkCheckBox();
            this.tbScalable = new DarkUI.Controls.DarkTextBox();
            this.SuspendLayout();
            // 
            // butOK
            // 
            this.butOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.butOK.Location = new System.Drawing.Point(56, 288);
            this.butOK.Name = "butOK";
            this.butOK.Size = new System.Drawing.Size(80, 23);
            this.butOK.TabIndex = 0;
            this.butOK.Text = "OK";
            this.butOK.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.butOK.Click += new System.EventHandler(this.butOK_Click);
            // 
            // butCancel
            // 
            this.butCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.butCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.butCancel.Location = new System.Drawing.Point(142, 288);
            this.butCancel.Name = "butCancel";
            this.butCancel.Size = new System.Drawing.Size(80, 23);
            this.butCancel.TabIndex = 1;
            this.butCancel.Text = "Cancel";
            this.butCancel.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.butCancel.Click += new System.EventHandler(this.butCancel_Click);
            // 
            // cbDisableCollision
            // 
            this.cbDisableCollision.AutoSize = true;
            this.cbDisableCollision.Location = new System.Drawing.Point(12, 12);
            this.cbDisableCollision.Name = "cbDisableCollision";
            this.cbDisableCollision.Size = new System.Drawing.Size(110, 17);
            this.cbDisableCollision.TabIndex = 5;
            this.cbDisableCollision.Text = "Disable collision";
            // 
            // cbIceTrasparency
            // 
            this.cbIceTrasparency.AutoSize = true;
            this.cbIceTrasparency.Location = new System.Drawing.Point(12, 35);
            this.cbIceTrasparency.Name = "cbIceTrasparency";
            this.cbIceTrasparency.Size = new System.Drawing.Size(102, 17);
            this.cbIceTrasparency.TabIndex = 9;
            this.cbIceTrasparency.Text = "Ice trasparency";
            // 
            // cbHeavyTriggerOnCollision
            // 
            this.cbHeavyTriggerOnCollision.AutoSize = true;
            this.cbHeavyTriggerOnCollision.Location = new System.Drawing.Point(12, 219);
            this.cbHeavyTriggerOnCollision.Name = "cbHeavyTriggerOnCollision";
            this.cbHeavyTriggerOnCollision.Size = new System.Drawing.Size(199, 17);
            this.cbHeavyTriggerOnCollision.TabIndex = 16;
            this.cbHeavyTriggerOnCollision.Text = "Activate heavy trigger on collision";
            // 
            // cbHardShatter
            // 
            this.cbHardShatter.AutoSize = true;
            this.cbHardShatter.Location = new System.Drawing.Point(12, 196);
            this.cbHardShatter.Name = "cbHardShatter";
            this.cbHardShatter.Size = new System.Drawing.Size(90, 17);
            this.cbHardShatter.TabIndex = 17;
            this.cbHardShatter.Text = "Hard shatter";
            // 
            // cbHugeCollision
            // 
            this.cbHugeCollision.AutoSize = true;
            this.cbHugeCollision.Location = new System.Drawing.Point(12, 173);
            this.cbHugeCollision.Name = "cbHugeCollision";
            this.cbHugeCollision.Size = new System.Drawing.Size(100, 17);
            this.cbHugeCollision.TabIndex = 18;
            this.cbHugeCollision.Text = "Huge collision";
            // 
            // cbPoisonLaraOnCollision
            // 
            this.cbPoisonLaraOnCollision.AutoSize = true;
            this.cbPoisonLaraOnCollision.Location = new System.Drawing.Point(12, 150);
            this.cbPoisonLaraOnCollision.Name = "cbPoisonLaraOnCollision";
            this.cbPoisonLaraOnCollision.Size = new System.Drawing.Size(148, 17);
            this.cbPoisonLaraOnCollision.TabIndex = 19;
            this.cbPoisonLaraOnCollision.Text = "Poison Lara on collision";
            // 
            // cbExplodeKillingOnCollision
            // 
            this.cbExplodeKillingOnCollision.AutoSize = true;
            this.cbExplodeKillingOnCollision.Location = new System.Drawing.Point(12, 127);
            this.cbExplodeKillingOnCollision.Name = "cbExplodeKillingOnCollision";
            this.cbExplodeKillingOnCollision.Size = new System.Drawing.Size(165, 17);
            this.cbExplodeKillingOnCollision.TabIndex = 20;
            this.cbExplodeKillingOnCollision.Text = "Explode killing on collision";
            // 
            // cbBurnLaraOnCollision
            // 
            this.cbBurnLaraOnCollision.AutoSize = true;
            this.cbBurnLaraOnCollision.Location = new System.Drawing.Point(12, 104);
            this.cbBurnLaraOnCollision.Name = "cbBurnLaraOnCollision";
            this.cbBurnLaraOnCollision.Size = new System.Drawing.Size(138, 17);
            this.cbBurnLaraOnCollision.TabIndex = 21;
            this.cbBurnLaraOnCollision.Text = "Burn Lara on collision";
            // 
            // cbDamageLaraOnContact
            // 
            this.cbDamageLaraOnContact.AutoSize = true;
            this.cbDamageLaraOnContact.Location = new System.Drawing.Point(12, 81);
            this.cbDamageLaraOnContact.Name = "cbDamageLaraOnContact";
            this.cbDamageLaraOnContact.Size = new System.Drawing.Size(155, 17);
            this.cbDamageLaraOnContact.TabIndex = 22;
            this.cbDamageLaraOnContact.Text = "Damage Lara on collision";
            // 
            // cbGlassTrasparency
            // 
            this.cbGlassTrasparency.AutoSize = true;
            this.cbGlassTrasparency.Location = new System.Drawing.Point(12, 58);
            this.cbGlassTrasparency.Name = "cbGlassTrasparency";
            this.cbGlassTrasparency.Size = new System.Drawing.Size(115, 17);
            this.cbGlassTrasparency.TabIndex = 23;
            this.cbGlassTrasparency.Text = "Glass trasparency";
            // 
            // cbScalable
            // 
            this.cbScalable.AutoSize = true;
            this.cbScalable.Location = new System.Drawing.Point(12, 252);
            this.cbScalable.Name = "cbScalable";
            this.cbScalable.Size = new System.Drawing.Size(68, 17);
            this.cbScalable.TabIndex = 24;
            this.cbScalable.Text = "Scalable";
            this.cbScalable.CheckedChanged += new System.EventHandler(this.cbScalable_CheckedChanged);
            // 
            // tbScalable
            // 
            this.tbScalable.Location = new System.Drawing.Point(86, 251);
            this.tbScalable.Name = "tbScalable";
            this.tbScalable.Size = new System.Drawing.Size(53, 22);
            this.tbScalable.TabIndex = 25;
            this.tbScalable.Text = "0";
            // 
            // FormStaticMesh
            // 
            this.AcceptButton = this.butOK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.butCancel;
            this.ClientSize = new System.Drawing.Size(228, 318);
            this.Controls.Add(this.tbScalable);
            this.Controls.Add(this.cbScalable);
            this.Controls.Add(this.cbGlassTrasparency);
            this.Controls.Add(this.cbDamageLaraOnContact);
            this.Controls.Add(this.cbBurnLaraOnCollision);
            this.Controls.Add(this.cbExplodeKillingOnCollision);
            this.Controls.Add(this.cbPoisonLaraOnCollision);
            this.Controls.Add(this.cbHugeCollision);
            this.Controls.Add(this.cbHardShatter);
            this.Controls.Add(this.cbHeavyTriggerOnCollision);
            this.Controls.Add(this.cbDisableCollision);
            this.Controls.Add(this.butCancel);
            this.Controls.Add(this.butOK);
            this.Controls.Add(this.cbIceTrasparency);
            this.Font = new System.Drawing.Font("Segoe UI", 8.25F);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Fixed3D;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FormStaticMesh";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Static mesh";
            this.Load += new System.EventHandler(this.FormObject_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private DarkButton butOK;
        private DarkButton butCancel;
        private DarkCheckBox cbDisableCollision;
        private DarkCheckBox cbIceTrasparency;
        private System.ComponentModel.BackgroundWorker backgroundWorker1;
        private DarkCheckBox cbHeavyTriggerOnCollision;
        private DarkCheckBox cbHardShatter;
        private DarkCheckBox cbHugeCollision;
        private DarkCheckBox cbPoisonLaraOnCollision;
        private DarkCheckBox cbExplodeKillingOnCollision;
        private DarkCheckBox cbBurnLaraOnCollision;
        private DarkCheckBox cbDamageLaraOnContact;
        private DarkCheckBox cbGlassTrasparency;
        private DarkCheckBox cbScalable;
        private DarkTextBox tbScalable;
    }
}