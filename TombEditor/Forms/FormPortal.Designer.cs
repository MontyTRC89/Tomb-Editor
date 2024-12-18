namespace TombEditor.Forms
{
    partial class FormPortal
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
            butCancel = new DarkUI.Controls.DarkButton();
            butOk = new DarkUI.Controls.DarkButton();
            comboPortalEffect = new DarkUI.Controls.DarkComboBox();
            darkLabel2 = new DarkUI.Controls.DarkLabel();
            darkGroupBox1 = new DarkUI.Controls.DarkGroupBox();
            cbReflectLights = new DarkUI.Controls.DarkCheckBox();
            cbReflectStatics = new DarkUI.Controls.DarkCheckBox();
            cbReflectMoveables = new DarkUI.Controls.DarkCheckBox();
            cbReflectSprites = new DarkUI.Controls.DarkCheckBox();
            darkGroupBox1.SuspendLayout();
            SuspendLayout();
            // 
            // butCancel
            // 
            butCancel.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
            butCancel.Checked = false;
            butCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            butCancel.Location = new System.Drawing.Point(161, 163);
            butCancel.Name = "butCancel";
            butCancel.Size = new System.Drawing.Size(80, 23);
            butCancel.TabIndex = 2;
            butCancel.Text = "Cancel";
            butCancel.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            butCancel.Click += butCancel_Click;
            // 
            // butOk
            // 
            butOk.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
            butOk.Checked = false;
            butOk.Location = new System.Drawing.Point(75, 163);
            butOk.Name = "butOk";
            butOk.Size = new System.Drawing.Size(80, 23);
            butOk.TabIndex = 1;
            butOk.Text = "OK";
            butOk.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            butOk.Click += butOk_Click;
            // 
            // comboPortalEffect
            // 
            comboPortalEffect.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            comboPortalEffect.FormattingEnabled = true;
            comboPortalEffect.Location = new System.Drawing.Point(87, 9);
            comboPortalEffect.Name = "comboPortalEffect";
            comboPortalEffect.Size = new System.Drawing.Size(154, 23);
            comboPortalEffect.TabIndex = 10;
            comboPortalEffect.SelectedIndexChanged += comboPortalEffect_SelectedIndexChanged;
            // 
            // darkLabel2
            // 
            darkLabel2.AutoSize = true;
            darkLabel2.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
            darkLabel2.Location = new System.Drawing.Point(7, 12);
            darkLabel2.Name = "darkLabel2";
            darkLabel2.Size = new System.Drawing.Size(72, 13);
            darkLabel2.TabIndex = 11;
            darkLabel2.Text = "Portal effect:";
            // 
            // darkGroupBox1
            // 
            darkGroupBox1.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            darkGroupBox1.Controls.Add(cbReflectSprites);
            darkGroupBox1.Controls.Add(cbReflectLights);
            darkGroupBox1.Controls.Add(cbReflectStatics);
            darkGroupBox1.Controls.Add(cbReflectMoveables);
            darkGroupBox1.Location = new System.Drawing.Point(7, 40);
            darkGroupBox1.Name = "darkGroupBox1";
            darkGroupBox1.Size = new System.Drawing.Size(234, 114);
            darkGroupBox1.TabIndex = 12;
            darkGroupBox1.TabStop = false;
            darkGroupBox1.Text = "Mirror options";
            // 
            // cbReflectLights
            // 
            cbReflectLights.AutoSize = true;
            cbReflectLights.Location = new System.Drawing.Point(7, 91);
            cbReflectLights.Name = "cbReflectLights";
            cbReflectLights.Size = new System.Drawing.Size(138, 17);
            cbReflectLights.TabIndex = 2;
            cbReflectLights.Text = "Reflect dynamic lights";
            // 
            // cbReflectStatics
            // 
            cbReflectStatics.AutoSize = true;
            cbReflectStatics.Location = new System.Drawing.Point(7, 45);
            cbReflectStatics.Name = "cbReflectStatics";
            cbReflectStatics.Size = new System.Drawing.Size(96, 17);
            cbReflectStatics.TabIndex = 1;
            cbReflectStatics.Text = "Reflect statics";
            // 
            // cbReflectMoveables
            // 
            cbReflectMoveables.AutoSize = true;
            cbReflectMoveables.Location = new System.Drawing.Point(7, 22);
            cbReflectMoveables.Name = "cbReflectMoveables";
            cbReflectMoveables.Size = new System.Drawing.Size(118, 17);
            cbReflectMoveables.TabIndex = 0;
            cbReflectMoveables.Text = "Reflect moveables";
            // 
            // cbReflectSprites
            // 
            cbReflectSprites.AutoSize = true;
            cbReflectSprites.Location = new System.Drawing.Point(7, 68);
            cbReflectSprites.Name = "cbReflectSprites";
            cbReflectSprites.Size = new System.Drawing.Size(107, 17);
            cbReflectSprites.TabIndex = 3;
            cbReflectSprites.Text = "Reflect sprites";
            // 
            // FormPortal
            // 
            AcceptButton = butOk;
            AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            CancelButton = butCancel;
            ClientSize = new System.Drawing.Size(249, 194);
            Controls.Add(darkGroupBox1);
            Controls.Add(darkLabel2);
            Controls.Add(comboPortalEffect);
            Controls.Add(butCancel);
            Controls.Add(butOk);
            FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "FormPortal";
            ShowIcon = false;
            ShowInTaskbar = false;
            StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            Text = "Edit portal";
            darkGroupBox1.ResumeLayout(false);
            darkGroupBox1.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion
        private DarkUI.Controls.DarkButton butCancel;
        private DarkUI.Controls.DarkButton butOk;
        private DarkUI.Controls.DarkComboBox comboPortalEffect;
        private DarkUI.Controls.DarkLabel darkLabel2;
        private DarkUI.Controls.DarkGroupBox darkGroupBox1;
        private DarkUI.Controls.DarkCheckBox cbReflectStatics;
        private DarkUI.Controls.DarkCheckBox cbReflectMoveables;
        private DarkUI.Controls.DarkCheckBox cbReflectLights;
        private DarkUI.Controls.DarkCheckBox cbReflectSprites;
    }
}